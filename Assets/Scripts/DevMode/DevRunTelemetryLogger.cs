// Place at: Assets/Scripts/DevMode/DevRunTelemetryLogger.cs
#if ALWTTT_DEV
using System;
using System.Collections.Generic;
using System.IO;
using ALWTTT.Managers;
using ALWTTT.Sensory;
using UnityEngine;

namespace ALWTTT.DevMode
{
    /// <summary>
    /// [TLM-1 / BR-D2=A] ALWTTT_DEV-gated run telemetry logger. Subscribes to the
    /// sensory bus (read-only — publishes nothing, mutates no game state) and writes
    /// ONE JSON-Lines record per gig on <see cref="GigOutcomeEvent"/>.
    ///
    /// Purpose: give S5i (and every later playtest) recorded fact for the two
    /// primary balance metrics — pick rate and appearance-in-winning-runs — with
    /// the mandatory confound guard: song-index-at-play-time per played card
    /// (the "Madness"/SFX late-run confound, BALANCE-XREF).
    ///
    /// Lifecycle: Initialize()/Shutdown() called from DevModeController
    /// Awake/OnDestroy (sibling pattern to DevGigOutcomeTracker).
    ///
    /// Coverage (D-TLM-3=A — documented gap, mirrors the existing tally):
    ///   - Logged: normal-flow outcomes via ResolveGigOutcomeAndEnd (the only
    ///     GigOutcomeEvent publish site). lossCause is therefore always
    ///     "unconvinced_after_final_song" for logged losses.
    ///   - NOT logged: cohesion-collapse losses (MusicianBase.OnBreakdown →
    ///     LoseGig() directly — no event), and editor Debug context-menu
    ///     Win/Lose (bypass by design). Partial gigs (retry/quit mid-gig)
    ///     produce no record; accumulators reset on the next GigStartedEvent.
    ///
    /// Output: JSON Lines, append-per-gig.
    ///   Editor:       &lt;projectRoot&gt;/DevTelemetry/gig_runs_YYYY-MM-DD.jsonl
    ///   Dev player:   Application.persistentDataPath/DevTelemetry/…
    /// Never under Assets/ or Resources/ (no importer churn, never shipped).
    /// </summary>
    public static class DevRunTelemetryLogger
    {
        private const string Tag = "[DevRunTelemetryLogger]";
        private const int SchemaVersion = 1;

        // ---------------------------------------------------------------
        // Record DTOs (JsonUtility-serializable)
        // ---------------------------------------------------------------

        [Serializable]
        private class GigRecord
        {
            public int schemaVersion;
            public string timestampUtc;
            public string sessionId;        // groups one play-session sitting
            public string encounterLabel;   // GigEncounter.GetLabel()
            public int requiredSongCount;
            public bool won;
            public string lossCause;        // "unconvinced_after_final_song" | "" on win
            public int songsCompleted;      // PD.CurrentSongIndex at outcome
            public int loopsPlayed;         // LoopResolvedEvent count this gig
            public List<string> roster = new List<string>();
            public List<AudienceRecord> audience = new List<AudienceRecord>();
            public List<PlayRecord> plays = new List<PlayRecord>();       // ordered
            public List<PlayCountRecord> playCounts = new List<PlayCountRecord>();
        }

        [Serializable]
        private class AudienceRecord
        {
            public string name;     // authored CharacterName (stable across sessions)
            public int index;       // spawn index (disambiguates duplicates)
            public int endVibe;     // remaining persuasion resistance at outcome
            public int maxVibe;
            public bool convinced;
        }

        [Serializable]
        private class PlayRecord
        {
            public string cardId;        // CardDefinition.Id
            public int songIndex;        // PD.CurrentSongIndex AT PLAY TIME (confound guard)
            public bool isComposition;
            public int inspirationCost;  // authored cost
        }

        [Serializable]
        private class PlayCountRecord
        {
            public string cardId;
            public int count;
        }

        // ---------------------------------------------------------------
        // State
        // ---------------------------------------------------------------

        private static readonly string SessionId =
            Guid.NewGuid().ToString("N").Substring(0, 8);

        private static bool _initialized;
        private static int _requiredSongCount;
        private static int _loopsThisGig;
        private static readonly List<PlayRecord> _plays = new List<PlayRecord>();

        /// <summary>Full path of the most recent record write (empty until the
        /// first gig resolves). Surfaced by the Dev Stats tab.</summary>
        public static string LastWritePath { get; private set; } = string.Empty;

        // ---------------------------------------------------------------
        // Lifecycle (called by DevModeController)
        // ---------------------------------------------------------------

        public static void Initialize()
        {
            if (_initialized) return;
            var bus = SensoryEventBus.Instance;
            if (bus == null)
            {
                Debug.LogWarning($"{Tag} No SensoryEventBus at init — logger inactive.");
                return;
            }

            bus.Subscribe<GigStartedEvent>(OnGigStarted);
            bus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            bus.Subscribe<LoopResolvedEvent>(OnLoopResolved);
            bus.Subscribe<GigOutcomeEvent>(OnGigOutcome);
            _initialized = true;
            Debug.Log($"{Tag} Initialized (session {SessionId}). Records → {OutputDirectory()}");
        }

        public static void Shutdown()
        {
            if (!_initialized) return;
            var bus = SensoryEventBus.Instance;
            if (bus != null)
            {
                bus.Unsubscribe<GigStartedEvent>(OnGigStarted);
                bus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
                bus.Unsubscribe<LoopResolvedEvent>(OnLoopResolved);
                bus.Unsubscribe<GigOutcomeEvent>(OnGigOutcome);
            }
            _initialized = false;
        }

        // ---------------------------------------------------------------
        // Handlers (read-only; every handler is exception-safe by bus contract,
        // but WriteRecord additionally try/catches so IO can never leak upward)
        // ---------------------------------------------------------------

        private static void OnGigStarted(GigStartedEvent e)
        {
            _plays.Clear();
            _loopsThisGig = 0;
            _requiredSongCount = e.RequiredSongCount;
        }

        private static void OnCardPlayed(CardPlayedEvent e)
        {
            var pd = GameManager.Instance.PersistentGameplayData;
            _plays.Add(new PlayRecord
            {
                cardId = e.Definition != null ? e.Definition.Id : "(null-definition)",
                songIndex = pd != null ? pd.CurrentSongIndex : -1,
                isComposition = e.IsComposition,
                inspirationCost = e.InspirationCost
            });
        }

        private static void OnLoopResolved(LoopResolvedEvent e) => _loopsThisGig++;

        private static void OnGigOutcome(GigOutcomeEvent e)
        {
            try { WriteRecord(e.Won); }
            catch (Exception ex)
            {
                Debug.LogError($"{Tag} Failed to write gig record: {ex}");
            }
            finally
            {
                _plays.Clear();
                _loopsThisGig = 0;
            }
        }

        // ---------------------------------------------------------------
        // Record assembly + IO
        // ---------------------------------------------------------------

        private static void WriteRecord(bool won)
        {
            var pd = GameManager.Instance.PersistentGameplayData;
            var gm = ALWTTT.Managers.GigManager.Instance;

            var rec = new GigRecord
            {
                schemaVersion = SchemaVersion,
                timestampUtc = DateTime.UtcNow.ToString("o"),
                sessionId = SessionId,
                won = won,
                lossCause = won ? string.Empty : "unconvinced_after_final_song",
                songsCompleted = pd != null ? pd.CurrentSongIndex : -1,
                loopsPlayed = _loopsThisGig,
                requiredSongCount = _requiredSongCount,
                plays = new List<PlayRecord>(_plays)
            };

            // Encounter label — same fallback chain LoseGig uses.
            var encounter = pd != null && pd.CurrentEncounter != null
                ? pd.CurrentEncounter
                : gm != null ? gm.CurrentGigEncounter : null;
            rec.encounterLabel = encounter != null ? encounter.GetLabel() : "(none)";

            // Roster (stable authored ids).
            if (gm != null && gm.CurrentMusicianCharacterList != null)
                foreach (var m in gm.CurrentMusicianCharacterList)
                    if (m != null && m.MusicianCharacterData != null)
                        rec.roster.Add(m.MusicianCharacterData.CharacterId);

            // Per-audience end state. GigOutcomeEvent publishes BEFORE
            // WinGig()/LoseGig(), so this snapshot precedes any cleanup.
            // NOTE: uses authored CharacterName + spawn index, NOT
            // AudienceCharacterBase.CharacterId (which embeds GetInstanceID and
            // is not stable across sessions).
            if (gm != null && gm.CurrentAudienceCharacterList != null)
            {
                for (int i = 0; i < gm.CurrentAudienceCharacterList.Count; i++)
                {
                    var aud = gm.CurrentAudienceCharacterList[i];
                    if (aud == null || aud.Stats == null) continue;
                    rec.audience.Add(new AudienceRecord
                    {
                        name = aud.AudienceCharacterData != null
                            ? aud.AudienceCharacterData.CharacterName : "(unknown)",
                        index = i,
                        endVibe = aud.Stats.CurrentVibe,
                        maxVibe = aud.Stats.MaxVibe,
                        convinced = aud.Stats.IsConvinced
                    });
                }
            }

            // Per-card counts (aggregate of the ordered play list).
            var counts = new Dictionary<string, int>();
            foreach (var p in _plays)
                counts[p.cardId] = counts.TryGetValue(p.cardId, out var c) ? c + 1 : 1;
            foreach (var kvp in counts)
                rec.playCounts.Add(new PlayCountRecord { cardId = kvp.Key, count = kvp.Value });

            string dir = OutputDirectory();
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir,
                $"gig_runs_{DateTime.Now:yyyy-MM-dd}.jsonl");
            File.AppendAllText(path, JsonUtility.ToJson(rec) + Environment.NewLine);

            LastWritePath = path;
            Debug.Log($"{Tag} Gig record appended ({(won ? "WIN" : "LOSS")}, " +
                      $"{_plays.Count} plays, {_loopsThisGig} loops) → {path}");
        }

        private static string OutputDirectory()
        {
#if UNITY_EDITOR
            // <projectRoot>/DevTelemetry — outside Assets/, no importer churn.
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "DevTelemetry"));
#else
            return Path.Combine(Application.persistentDataPath, "DevTelemetry");
#endif
        }
    }
}
#endif