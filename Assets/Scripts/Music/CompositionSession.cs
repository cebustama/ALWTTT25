using ALWTTT.Cards;
using ALWTTT.Cards.Effects;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.UI;
using Melanchall.DryWetMidi.MusicTheory;
using MidiGenPlay;
using MidiGenPlay.Composition;
using MidiGenPlay.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Music
{
    public class CompositionSession
    {
        public enum CompositionState
        {
            Idle,                   // Not in a jam session
            BuildingCurrentPart,    // Player is building the first (or current) part before any playback
            PlayingCurrentPart,     // A confirmed part is currently playing in loop
            BuildingNextPart,       // While the current part is looping, player is drafting the next part
            Ended                   // Jam is over
        }
        private CompositionState _state = CompositionState.Idle;
        public CompositionState State => _state;

        private ICompositionContext _ctx;
        private JamRules _rules;
        private MidiGenPlayConfig _settings;
        private System.Random _rng;

        // [S5g / MGP-ALWTTT-SEED-1 / D-S5gb-2=B] Per-song render seed. Run-entropy:
        // each song (Begin→End) gets one seed; every render within the song reuses
        // it (same seed ⇒ same palette picks ⇒ mid-song re-renders stay harmonically
        // stable, replacing the accidental stability the constant defaultSeed gave).
        private int? _songSeed;

#if ALWTTT_DEV
        // Dev pin for reproducible songs / ST-S5gb-1 fixed-seed runs. Set before
        // Begin(); null = normal run entropy. Dev-tab wiring deferred (SSoT_Dev_Mode).
        public static int? DevPinnedSongSeed = null;
#endif

        private bool _isPlaying;
        private int _currentPartIndex = -1;
        private int _loopsTotalForPart;
        private int _loopsRemainingForPart;

        private int _currentInspiration;
        private int _perLoopInspirationCurrentPart;
        private int _buildingPartInspirationPerLoop;

        private float _loopStartTime;
        private float _loopDurationSeconds;

        public event Action<LoopFeedbackContext> LoopFinished;
        // [SINGER-1] Raised once per loop, immediately before PlayRaw, carrying the
        // loop's stems + musical context so the singer can arm from the melody stem.
        public event Action<ALWTTT.Music.Voice.SingerLoopContext> LoopPlaybackStarting;
        public event Action<PartFeedbackContext> PartFinished;
        public event Action<SongFeedbackContext> SongFinished;

        public class PartCache
        {
            public byte[] mergedBytes;
            public float seconds;
            public int resolvedBpm;
            // [DBG-C1] Re-keyed on (musicianId, TrackRole) end-to-end. A
            // musician holding two role-tracks holds two independent entries.
            public Dictionary<MusicianTrackKey, byte[]> stemsByTrack = new();
            public Dictionary<MusicianTrackKey, MIDIInstrumentSO> resolvedMelInstByTrack = new();
            public Dictionary<MusicianTrackKey, MIDIPercussionInstrumentSO>
                resolvedPercInstByTrack = new();
#if ALWTTT_DEV
            // [DBG-C2] DevOverrideStamp captured when this entry was rendered.
            // Compared at loop start; mismatch ⇒ invalidate + re-render.
            public int devOverrideStamp;
#endif
        }

        private readonly Dictionary<int, PartCache> _partCache = new();

        // [JAM-1 / MGP-MEL-1b P7 / D-R3C-2=A] Per-part shared harmony carried
        // forward so a card joining an ongoing jam ACCOMPANIES it instead of
        // rewriting it. Values are runtime SO clones from MidiMusicManager —
        // never assets, never serialized, never written to disk. Per-song
        // lifetime: cleared in Begin()/End() alongside _partCache.
        private readonly Dictionary<int, ChordProgressionData> _jamProgressionByPart = new();

        // [JAM-1] Tonality/root at the moment each progression was captured.
        // Comparing this against the CURRENT model is how we detect "the
        // incoming card moved the tonality" without enumerating effect types.
        private readonly Dictionary<int, (Tonality ton, NoteName root)> _jamTonalitySnapByPart = new();

        // [JAM-2 / F-JAM-SCALE-SPLIT] Tonality/root the captured harmony ACTUALLY
        // RENDERED IN. Deliberately a SECOND field, not a replacement for the
        // snapshot above: that one tracks the MODEL and answers "did the player
        // move the key since we captured?"; this one tracks the RENDER and answers
        // "what mode did these chords sound in?". The two diverge whenever a
        // Backing card adopts — adoption mutates the per-render PartConfig and
        // never the model (verified O1), so the model reads Ionian while the
        // chords are Lydian. Collapsing them into one field would break the
        // move-detection guard.
        private readonly Dictionary<int, (Tonality ton, NoteName root)> _jamRenderedTonalityByPart = new();

        // [B1 / #7.1 / D-F=γ] Session-level instrument pin maps. Keyed on
        // "musicianId|role". Populated after each successful render with the
        // resolved Instrument / PercussionInstrument. Consumed before each
        // render to keep the same musician's voice consistent across style
        // changes (Major → Minor, Waltz → some other Rhythm style, etc.)
        // within the same song. Per-song lifetime: reset in Begin() / End().
        //
        // Skipped when the UI TrackEntry has an explicit instrument override
        // (overrideMelodicInstrument / overridePercussionInstrument != null);
        // those cards honor the explicit SO. Type-override cards
        // (hasOverrideInstrumentType=true) do pin the random pick within the
        // type, so re-renders stay consistent.
        private readonly Dictionary<string, MIDIInstrumentSO> _sessionMelodicPin = new();
        private readonly Dictionary<string, MIDIPercussionInstrumentSO> _sessionPercussionPin = new();

#if ALWTTT_DEV
        // [DBG-C1 / D2=A, D3] Infinite composition-loop dev toggle. Owned by
        // DevCompositionDebugTab; consumed by HandleLoopFinished. When ON, the
        // loop countdown resets instead of advancing part / ending the song;
        // per-loop host hooks (LoopFinished subscribers: draw, inspiration)
        // keep firing. Reset at song boundary in Begin()/End() — never leaks
        // across songs; the field itself does not exist in production builds.
        public static bool DevInfiniteCompositionLoop;

        // [DBG-C2 / D-C2-1..4=A] Per-render pattern-override map, owned by
        // DevCompositionDebugTab. Keyed (musicianId, TrackRole). Passed to
        // RenderSinglePart only when non-empty (null when idle — BC gate).
        // Overrides are deliberately NOT part of any cache key: MMM bypasses
        // its stem/bundle caches when the map is supplied (D-C2-4=A), and the
        // stamp below invalidates this session's PartCache on change. Reset
        // at song boundary in Begin()/End(); does not exist in production.
        public static readonly System.Collections.Generic.Dictionary<
            MusicianTrackKey, PatternDataSO> DevPatternOverrides = new();

#if ALWTTT_DEV
        // [D-CSV-24=B] Tracks added via DevInjectCompositionCard are audition-
        // only: excluded from per-loop inspiration so R2a is economy-neutral
        // (parity with "no cost, no effects"). Keyed (musicianId, role).
        // Cleared at song boundary; a genuine card play on the same track
        // reclaims it into the economy (see TryPlayCompositionCard).
        private static readonly HashSet<MusicianTrackKey> _devInjectedTrackKeys = new();
#endif

        // [DBG-C2] Monotonic stamp, bumped on every override mutation and by
        // the R2a "Re-render part now" button (D-C2-3=A). PlaySinglePartLoop
        // compares it with the value stamped on the PartCache entry at render
        // time; a mismatch invalidates the entry (keepTempo+keepInstruments)
        // so the next loop renders fresh through the normal seeded path.
        public static int DevOverrideStamp;
        public static void DevBumpOverrideStamp() => DevOverrideStamp++;

        // [DBG-C2] The tab needs the same MidiGenPlayConfig this session uses
        // so its PatternRepositoryResources scans the same Resources roots.
        public MidiGenPlayConfig DevMidiConfig => _settings;

        // [DBG-C1] Read-only dev accessors for the composition-debug tab.
        public int DevCurrentPartIndex => _currentPartIndex;
        public int DevLoopsRemainingForPart => _loopsRemainingForPart;
        public int DevLoopsTotalForPart => _loopsTotalForPart;
        public int? DevSongSeed => _songSeed;
        public ALWTTT.UI.SongCompositionUI DevCompositionUI => _ctx?.CompositionUI;

        // [CSV-2] Musician lookup for the dev tab's permitted-set annotation
        // (InstrumentRules.GetPermittedMelodic needs the MusicianBase).
        public ALWTTT.Characters.Band.MusicianBase DevResolveMusicianById(string id)
            => _ctx?.ResolveMusicianById(id);

        // [CSV-2 / D-CSV-5=A refined] Dev instrument overrides write
        // TrackEntry.override*Instrument directly (hash-participating,
        // card-identical). This helper mirrors the INSTRUMENT-CARD
        // invalidation path, NOT the pattern-stamp path: the stamp path
        // invalidates with keepInstruments=TRUE, which preserves
        // cache.resolvedMelInstByTrack and re-feeds it into RenderSinglePart
        // as instrumentOverrides — that stale map must be dropped when the
        // instrument itself changes. keepTempo stays true (instrument changes
        // don't retune). Other tracks keep their voices via the session pin
        // maps (_sessionMelodicPin / _sessionPercussionPin), which this does
        // not touch; the overridden track's pin is skipped while the explicit
        // override is set (BuildMelodicPinKey/PercussionPinKey return null)
        // and re-applies on Clear — which is what makes clear/restore
        // byte-identical under a pinned seed.
        public void DevInvalidateForInstrumentOverride(int partIndex)
        {
            InvalidatePartCache(partIndex, keepTempo: true,
                keepInstrumentsOverride: false);
            DevBumpOverrideStamp();
        }

        /// <summary>[CSV-3] Band exposure for the dev tab's target picker.</summary>
        public System.Collections.Generic.IReadOnlyList<MusicianBase> DevBand => _ctx?.Band;

        /// <summary>
        /// [CSV-3 / R2a / D-CSV-8=A] Debug-play a catalogue card's MUSICAL side on the
        /// LIVE session model. Applies: ApplyCardDefinitionToPart (primary action +
        /// CompositionCardPayload.modifierEffects), the production invalidation +
        /// pending path. Skips: inspiration check/spend, InspirationGenerated one-shot,
        /// CardPayload.effects.
        /// [D-CSV-24=B] Economy-neutral: an injected TRACK is marked audition-only and
        /// excluded from EvalPerLoopInsp, so its per-loop inspiration bonus does NOT
        /// enter the run economy. The track still renders. A genuine play on the same
        /// (musicianId, role) reclaims it.
        /// NOTE (SINGER-1): renders through the normal loop path, so the singer sings
        /// the auditioned melody at the next loop start — intended.
        /// Not present in production builds.
        /// </summary>
        public bool DevInjectCompositionCard(
            CardDefinition def, string targetMusicianId, out string reason)
        {
            reason = null;
            var ui = _ctx?.CompositionUI;
            if (ui == null) { reason = "CompositionUI is null"; return false; }
            if (def == null || !def.IsComposition || def.CompositionPayload == null)
            { reason = "Not a composition card"; return false; }
            var comp = def.CompositionPayload;

            if (IsFinalLoopRunning)
            { reason = "Final loop running — enable Infinite loop to audition"; return false; }

            MusicianBase target = !string.IsNullOrEmpty(targetMusicianId)
                ? _ctx.ResolveMusicianById(targetMusicianId) : null;
            if (comp.RequiresMusicianTarget && target == null && def.RequiresFixedPerformer)
                target = _ctx.ResolveMusicianByType(def.FixedPerformerType);
            if (comp.RequiresMusicianTarget && target == null)
            { reason = "Card requires a musician target"; return false; }

            if (!ui.CanApplyDefinition(def, target, out var canReason))
            { reason = $"CanApply refused: {canReason}"; return false; }

            bool loopIsRunning = _isPlaying &&
                (_state == CompositionState.BuildingNextPart
                 || _state == CompositionState.PlayingCurrentPart);
            int partIdx = loopIsRunning ? _currentPartIndex : ui.Model.CurrentPartIndex;

            if (!ui.ApplyCardDefinitionToPart(def, target, partIdx))
            { reason = "ApplyCardDefinitionToPart returned false"; return false; }

            bool isTrack = comp.PrimaryKind == CardPrimaryKind.Track;

            // [D-CSV-24=B] Mark the injected track as audition-only BEFORE the resync
            // below, so EvalPerLoopInsp already excludes it and the badge stays put.
            if (isTrack && comp.TrackAction != null
                && target?.MusicianCharacterData != null
                && !string.IsNullOrEmpty(target.MusicianCharacterData.CharacterId))
            {
                _devInjectedTrackKeys.Add(new MusicianTrackKey(
                    target.MusicianCharacterData.CharacterId, comp.TrackAction.role));
            }

            bool affectsSound = CompositionCardClassifier.AffectsSound(comp);
            bool affectsPartMeter =
                CompositionCardClassifier.IsTempoCard(comp)
                || CompositionCardClassifier.IsTimeSignatureCard(comp)
                || CompositionCardClassifier.IsTonalityCard(comp)
                || CompositionCardClassifier.IsModulationCard(comp);

            if (loopIsRunning && affectsSound)
            {
                InvalidatePartCache(_currentPartIndex, ShouldKeepTempo(def), ShouldKeepInstruments(def));
                if (affectsPartMeter)
                    ui.MarkAllTracksPending(_currentPartIndex);
                else if (isTrack && target?.MusicianCharacterData != null
                         && !string.IsNullOrEmpty(target.MusicianCharacterData.CharacterId))
                    ui.MarkTrackPending(_currentPartIndex, target.MusicianCharacterData.CharacterId);
                else
                    ui.MarkAllTracksPending(_currentPartIndex);
            }

            // Derived per-loop-inspiration resync. With the key marked above, the
            // injected track contributes 0 — so the badge does NOT jump (D-CSV-24=B).
            if (isTrack && _state != CompositionState.BuildingCurrentPart
                && _currentPartIndex >= 0 && _currentPartIndex < ui.Model.parts.Count)
            {
                _perLoopInspirationCurrentPart = EvalPerLoopInsp(ui.Model.parts[_currentPartIndex]);
                ui.SetPlusInspiration(GlobalPerLoopBadgeValue);
            }

            Debug.Log($"<color=lime>[CSV-3][R2a]</color> Injected musical side of " +
                $"'{def.DisplayName}' → part {partIdx} " +
                $"(no cost, no effects, economy-neutral; renders next loop).");
            return true;
        }
#endif

        public bool TryGetPartCache(int partIndex, out PartCache cache) =>
            _partCache.TryGetValue(partIndex, out cache);

        public PartCache GetOrCreatePartCache(int partIndex)
            => _partCache.TryGetValue(partIndex, out var c)
            ? c : (_partCache[partIndex] = new PartCache());

        private readonly Dictionary<int, List<LoopFeedbackContext>> _loopHistoryByPart
            = new Dictionary<int, List<LoopFeedbackContext>>();
        private readonly List<PartFeedbackContext> _finishedParts
            = new List<PartFeedbackContext>();

        private readonly Dictionary<int, bool> _keepInstrumentByPart = new();

        /// <summary>
        /// Sets whether instrument choices for a given part should be reused
        /// when regenerating that part.
        /// </summary>
        public void SetKeepInstrumentForPart(int partIndex, bool keep)
        {
            _keepInstrumentByPart[partIndex] = keep;
        }

        /// <summary>
        /// Returns whether the part should preserve its cached instrument choices.
        /// Defaults to true when not explicitly set.
        /// </summary>
        private bool GetKeepInstrumentForPart(int partIndex)
        {
            return !_keepInstrumentByPart.TryGetValue(partIndex, out var keep) || keep;
        }

        /// <summary>
        /// True while any part/loop is currently playing through MidiMusicManager.
        /// </summary>
        public bool IsLoopPlaying => _isPlaying;

        /// <summary>
        /// [CARD-UX-1 / D2=A] True while a performance loop is running AND it is the
        /// part's final loop: a composition change applied now lands on the CURRENTLY
        /// LOOPING part (D-D=β: every drop routes to _currentPartIndex) and renders on
        /// the NEXT loop of that part — which does not exist. Pure waste ⇒ deny.
        ///
        /// EXEMPT while TutorialLoopHoldGate is armed: the held loop REPLAYS, so the
        /// pending change would in fact render. (TutorialModalGate is NOT exempt —
        /// modals suspend audience turns and dragging, they do NOT replay the loop.)
        ///
        /// Demo-cut note: parts-per-song=1, so "final loop" == the beat-8 held loop.
        /// Inside the tutorial the composition gate at that moment is
        /// TutorialInputGate.SingleCardOnly (finisher-only), not this lock.
        /// </summary>
        public bool IsFinalLoopRunning =>
            _isPlaying &&
            (_state == CompositionState.BuildingNextPart ||
             _state == CompositionState.PlayingCurrentPart) &&
            _loopsRemainingForPart == 1 &&
#if ALWTTT_DEV
            // [DBG-C1 / D2=A] Under infinite loop, "final loop" never
            // finalizes — a next render always exists, so the CARD-UX-1
            // waste-deny does not apply. Dev-only branch; production
            // predicate is byte-identical.
            !DevInfiniteCompositionLoop &&
#endif
            !ALWTTT.Tutorial.TutorialLoopHoldGate.IsArmed;

        /// <summary>
        /// True while the session is active (after Begin and before End).
        /// </summary>
        public bool IsActive =>
            _state != CompositionState.Idle &&
            _state != CompositionState.Ended;

        /// <summary>
        /// [S5b / D-S5b-QUERY-HOME=A] Musician IDs (== MusicianCharacterData.CharacterId)
        /// that own a track in <paramref name="partIndex"/> of the live UI model — i.e.
        /// the musicians whose stems play in that part's current loop. Returns an empty
        /// set when the part is out of range or the UI model is unavailable.
        ///
        /// Read-through accessor: the authority for "which musicians have a track" is the
        /// SongCompositionUI model (PartEntry.tracks); this method only enumerates it.
        /// Consumed by GigManager.ApplyBpmToStage to gate band beat-animation to
        /// performing musicians. The set is recomputed every loop, so a track added
        /// mid-loop animates on the same loop its stem is re-rendered.
        /// </summary>
        public HashSet<string> GetMusicianIdsWithTrackInPart(int partIndex)
        {
            var set = new HashSet<string>();
            var parts = _ctx?.CompositionUI?.Model?.parts;
            if (parts == null || partIndex < 0 || partIndex >= parts.Count) return set;

            var tracks = parts[partIndex]?.tracks;
            if (tracks == null) return set;

            foreach (var t in tracks)
                if (t != null && !string.IsNullOrEmpty(t.musicianId))
                    set.Add(t.musicianId);

            return set;
        }

        // ----- Public API -----
        public void Begin(
            ICompositionContext ctx, JamRules rules, MidiGenPlayConfig settings, System.Random rng)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _rules = rules ?? new JamRules();
            _settings = settings ?? new MidiGenPlayConfig();
            _rng = rng ?? new System.Random();

            _state = CompositionState.BuildingCurrentPart;
            _currentPartIndex = -1;
            _isPlaying = false;
            _partCache.Clear();
            _jamProgressionByPart.Clear();      // [JAM-1] runtime clones die with the song
            _jamTonalitySnapByPart.Clear();     // [JAM-1]
            _jamRenderedTonalityByPart.Clear(); // [JAM-2]
            _ctx.Music?.ResetStemCache(); // [B1 / D7=B] Per-song stem cache reset.
            _sessionMelodicPin.Clear();   // [B1 / #7.1 / D-F=γ]
#if ALWTTT_DEV
            DevInfiniteCompositionLoop = false; // [DBG-C1] song-boundary reset
            DevPatternOverrides.Clear();        // [DBG-C2] never leaks across songs
            _devInjectedTrackKeys.Clear();
#endif
            _sessionPercussionPin.Clear(); // [B1 / #7.1 / D-F=γ]
            _loopHistoryByPart.Clear();
            _finishedParts.Clear();
            _buildingPartInspirationPerLoop = 0;
            _perLoopInspirationCurrentPart = 0; // [DF-INSPLOOP-badge/ST-2] limpia valor stale entre canciones

            // [S5g / D-S5gb-2=B] One seed per song, stable until End().
#if ALWTTT_DEV
            _songSeed = DevPinnedSongSeed
                        ?? unchecked((int)System.DateTime.UtcNow.Ticks);
#else
            _songSeed = unchecked((int)System.DateTime.UtcNow.Ticks);
#endif
            _ctx.Log($"[Session] SongSeed={_songSeed}", true); // quote-able for repro

            _keepInstrumentByPart.Clear();

            _ctx.ShowCompositionUI(true);
            _ctx.ShowHand(true);

            var ui = _ctx.CompositionUI;
            ui?.ResetSession();
            ui?.PopulateMusicianIcons(_ctx.Band);
            ui?.SetIconReferencePartIndex(0);

            _currentInspiration = ResolveSessionStartInspiration();
            ui?.SetInspirationVisible(true);
            ui?.SetInspiration(_currentInspiration);
            ui?.SetPlusInspiration(GlobalPerLoopBadgeValue);

            _ctx.LoopsTimerUI?.ClearProgress();
            _ctx.LoopsTimerUI?.SetBarsVisible(false);

            _ctx.OnSessionStarted();
            _ctx.Log("[Session] Begin → BuildingCurrentPart", true);
        }

        public void End()
        {
            ALWTTT.Tutorial.TutorialLoopHoldGate.Release(); // [TUT-R2] defensive

            var songCtx = new SongFeedbackContext(_finishedParts);
            SongFinished?.Invoke(songCtx);

            _state = CompositionState.Ended;
            _isPlaying = false;
            _partCache.Clear();
            _jamProgressionByPart.Clear();      // [JAM-1] runtime clones die with the song
            _jamTonalitySnapByPart.Clear();     // [JAM-1]
            _jamRenderedTonalityByPart.Clear(); // [JAM-2]
            _ctx.Music?.ResetStemCache(); // [B1 / D7=B] Per-song stem cache reset.
#if ALWTTT_DEV
            DevInfiniteCompositionLoop = false; // [DBG-C1] song-boundary reset
            DevPatternOverrides.Clear();        // [DBG-C2] never leaks across songs
            _devInjectedTrackKeys.Clear();
#endif
            _sessionMelodicPin.Clear();   // [B1 / #7.1 / D-F=γ]
            _sessionPercussionPin.Clear(); // [B1 / #7.1 / D-F=γ]
            _loopHistoryByPart.Clear();
            _finishedParts.Clear();
            _keepInstrumentByPart.Clear();
            _songSeed = null; // [S5g] seed dies with the song

            _ctx.LoopsTimerUI?.ClearProgress();
            _ctx.ShowHand(false);
            _ctx.ShowCompositionUI(false);
            _ctx.OnSessionEnded();

            _ctx.Log("[Session] End", true);
        }

#if ALWTTT_DEV
        /// <summary>Dev Mode: current live inspiration budget for this session.</summary>
        public int CurrentInspiration => _currentInspiration;

        /// <summary>
        /// Dev Mode: set live session inspiration and refresh the composition UI.
        /// Does NOT write back to PersistentGameplayData — caller is responsible
        /// (GigManager.DevSetInspiration). See SSoT_Dev_Mode §13.2.
        /// </summary>
        public void DevSetCurrentInspiration(int value)
        {
            int before = _currentInspiration;
            _currentInspiration = Math.Max(0, value);
            _ctx?.CompositionUI?.SetInspiration(_currentInspiration);
            Debug.Log($"<color=lime>[DevMode] CompositionSession.DevSetCurrentInspiration " +
                $"before={before} → after={_currentInspiration}</color>");
        }
#endif

        // -----------------------------------------------------------
        // [B2 / #4] Inspiration cost API (shared by comp + action cards)
        // -----------------------------------------------------------

        /// <summary>True if the current session has at least <paramref name="cost"/> inspiration.</summary>
        public bool CanAffordInspiration(int cost)
        {
            return Math.Max(0, cost) <= _currentInspiration;
        }

        /// <summary>
        /// Deduct <paramref name="cost"/> from session inspiration and refresh the UI.
        /// Caller must check <see cref="CanAffordInspiration"/> first; this method
        /// clamps to zero on underflow but won't refuse.
        /// </summary>
        public void SpendInspiration(int cost)
        {
            cost = Math.Max(0, cost);
            if (cost == 0) return;
            _currentInspiration = Math.Max(0, _currentInspiration - cost);
            _ctx?.CompositionUI?.SetInspiration(_currentInspiration);
        }

        /// <summary>
        /// [B2 / #4] Flash the inspiration value text in the loss color without
        /// changing the underlying value. Used as a "denied — not enough" signal.
        /// Safe to call when no UI is wired.
        /// </summary>
        public void FlashInspirationDenied()
        {
            _ctx?.CompositionUI?.FlashInspirationDenied();
        }

        public void ConfirmCurrentPartAndStart()
        {
            if (_state != CompositionState.BuildingCurrentPart) return;

            _currentPartIndex = 0;
            _loopsTotalForPart = _rules.loopsPerPart;
            _loopsRemainingForPart = _rules.loopsPerPart;

            float secs = PlaySinglePartLoop(_currentPartIndex);
            if (secs <= 0f) { _ctx.Log("[Session] Failed to start first loop"); return; }

            _loopDurationSeconds = secs;
            _loopStartTime = Time.time;
            _ctx.CompositionUI?.SetIconReferencePartIndex(_currentPartIndex);

            var l = _ctx.LoopsTimerUI;
            if (l != null)
            {
                l.BuildBars(_loopsTotalForPart);
                l.SetProgress(0, 0f);
                l.SetBarsVisible(true);
            }

            _perLoopInspirationCurrentPart =
                EvalPerLoopInsp(_ctx.CompositionUI.Model.parts[_currentPartIndex]);
            _ctx.CompositionUI?.SetPlusInspiration(GlobalPerLoopBadgeValue);

            _state = CompositionState.BuildingNextPart;
            _currentInspiration = ResolveSessionStartInspiration();
            _ctx.CompositionUI?.SetInspiration(_currentInspiration);

            _ctx.CompositionUI?.BeginDraftNextPart("Part B");
            _ctx.Log("[Session] Now looping Part A and drafting Part B");
        }

        // Llamar en Update del host (opcional) para barra de progreso + fin de loop
        public void Tick(float dt)
        {
            if (_state != CompositionState.BuildingNextPart && _state != CompositionState.PlayingCurrentPart) return;

            var mm = _ctx.Music;
            if (mm == null) return;

            bool midiIsPlaying = mm.IsAnySongPlaying();

            if (!midiIsPlaying && _isPlaying)
                HandleLoopFinished();

            if (midiIsPlaying && _isPlaying && _loopDurationSeconds > 0f)
            {
                float elapsed = Time.time - _loopStartTime;
                float pct = Mathf.Clamp01(elapsed / _loopDurationSeconds);

                int loopsCompleted = _loopsTotalForPart - _loopsRemainingForPart;
                int loopIdx0 = Math.Max(0, loopsCompleted);
                _ctx.LoopsTimerUI?.SetProgress(loopIdx0, pct);
            }
        }
        public bool TryPlayCompositionCard(
            CardBase card, MusicianBase target, CardDropZone zone)
        {
            // ----- helpers -----
            void Info(string msg) => _ctx?.Log($"[TryPlay] {msg}");
            bool Fail(string msg) { _ctx?.Log($"[TryPlay][FAIL] {msg}", true); return false; }

            // [LOG-1 / D-LOG-3=B] Verbose tier for the INTERIOR of a card play.
            // `enter`, `SUCCESS` and every Fail stay on Info: those three
            // delimit the play and are what you read when a card misbehaves.
            // The intermediate bookkeeping below is derivable from them.
            void InfoV(string msg)
            {
                var dev = ALWTTT.Managers.GigManager.Instance != null
                    ? ALWTTT.Managers.GigManager.Instance.DevSettings : null;
                if (dev != null && dev.UseLogs && dev.LogVerbose) Info(msg);
            }

            var ui = _ctx?.CompositionUI;
            if (ui == null) return Fail("UI is null");
            if (card == null || card.CardDefinition == null) return Fail("Card or CardData is null");

            var def = card.CardDefinition;
            var comp = def?.CompositionPayload;

            bool isComp = comp != null;
            bool isTrack = comp != null && comp.PrimaryKind == CardPrimaryKind.Track;
            bool isTempo = comp != null &&
                CompositionCardClassifier.IsTempoCard(comp);
            bool isTimeSig = comp != null &&
                CompositionCardClassifier.IsTimeSignatureCard(comp);
            bool isTonality = comp != null &&
                CompositionCardClassifier.IsTonalityCard(comp);
            bool isModulation = comp != null &&
                CompositionCardClassifier.IsModulationCard(comp);
            bool requiresTarget = comp != null && comp.RequiresMusicianTarget;
            bool affectsSound = comp != null && CompositionCardClassifier.AffectsSound(comp);

            // [JAM-1 / MGP-MEL-1b §2.6 / D4=A] Runtime belt for hand-edited
            // assets that bypassed the editor check. The composer cannot tell
            // "default tonality" from "effect-pinned tonality", so adoption
            // wins SILENTLY at compose time and the TonalityEffect is a lie.
            // Warn only — never block a play mid-gig.
            if (isTonality
                && comp.TrackAction?.styleBundle is BackingCardConfigSO bkAdopt
                && bkAdopt.adoptProgressionTonality)
            {
                Debug.LogWarning($"[CARD-VALIDATION] '{def.DisplayName}': TonalityEffect + " +
                    $"adoptProgressionTonality — adoption wins, the effect is inert.");
            }

            // [B1 / #1+#2 / D-H1=α] A card "affects part meter" when it
            // mutates any field that goes into partMeterHash (TS, tonality,
            // root note, tempo). Such cards force every stem in the part to
            // regenerate next loop — even if the card ALSO replaces a single
            // track (e.g. Pentameter is a Rhythm-track card that ALSO sets
            // TS=5/4). Pending visualization must reflect that all tracks
            // will change.
            bool affectsPartMeter = isTempo || isTimeSig || isTonality || isModulation;

            Info($"enter name='{def?.DisplayName}' zone={zone} state={_state} " +
                 $"isComp={isComp} isTrack={isTrack} isTempo={isTempo} " +
                 $"isTimeSig={isTimeSig} requiresTarget={requiresTarget}");


            // 0b) [CARD-UX-1 / D2=A] Final-loop lock. Deny BEFORE any spend so
            // neither inspiration (step 1/8) nor the ECON budget burns (budget
            // burns only on played==true, upstream in GigManager).
            if (def.IsComposition && IsFinalLoopRunning)
                return Fail("Final-loop lock: no next loop would render this change");

            // 1) Inspiration cost (only for composition cards)
            if (def.IsComposition)
            {
                int cost = Math.Max(0, def.InspirationCost);
                InfoV($"inspiration: have={_currentInspiration} " +
                    $"cost={cost} gen={def.InspirationGenerated}");
                if (cost > _currentInspiration)
                {
                    _ctx?.CompositionUI?.FlashInspirationDenied(); // [B2 / #4]
                    return Fail("Not enough inspiration");
                }
            }

            // 2) Resolve target (only for track cards)
            if (requiresTarget)
            {
                if (target == null && def.RequiresFixedPerformer)
                {
                    target = _ctx.ResolveMusicianByType(def.FixedPerformerType);
                    Info($"fixed target resolver → " +
                        $"{(target != null ? target.MusicianCharacterData.CharacterName : "null")}");
                }
                if (def.RequiresMusicianTarget && target == null)
                    return Fail("Card requires musician target but none resolved");
            }

            // 3) Business rules (centralized in UI)
            if (!ui.CanApply(card, target, out var reason))
                return Fail($"UI.CanApply refused: {reason}");

            // 4) [B1 / D-D=β] Zone normalization. NextPart gesture removed
            // in Phase B; every drop is treated as CurrentPart. The
            // CardDropZone.NextPart enum value + downstream branches are
            // preserved dormant for migration coexistence.
            if (zone == CardDropZone.NextPart)
            {
                Info("[B1/D-D=β] redirecting NextPart → CurrentPart (dormant gesture)");
                zone = CardDropZone.CurrentPart;
            }

            // 5) Compute part index based on zone + loop state
            bool loopIsRunning =
                _isPlaying &&
                (_state == CompositionState.BuildingNextPart
                || _state == CompositionState.PlayingCurrentPart);

            int partIdx;
            if (loopIsRunning)
                partIdx = (zone == CardDropZone.NextPart) ?
                    ui.Model.CurrentPartIndex : _currentPartIndex;
            else
                partIdx = ui.Model.CurrentPartIndex;

            InfoV($"routing: loopRunning={loopIsRunning} zone={zone} -> partIdx={partIdx} " +
                 $"(ui.CurrentPartIndex={ui.Model.CurrentPartIndex} " +
                 $"currentPartIndex={_currentPartIndex})");

            // 6) Apply to model
            if (!ui.ApplyCardToPart(card, target, partIdx))
                return Fail("ui.ApplyCardToPart returned false");

#if ALWTTT_DEV
            // [D-CSV-24=B] A genuine play on a previously dev-injected track
            // reclaims it into the per-loop economy.
            if (isTrack && comp?.TrackAction != null
                && target?.MusicianCharacterData != null
                && !string.IsNullOrEmpty(target.MusicianCharacterData.CharacterId))
                _devInjectedTrackKeys.Remove(new MusicianTrackKey(
                    target.MusicianCharacterData.CharacterId, comp.TrackAction.role));
#endif

            // Apply Effects (status effects) immediately
            ApplyStatusActionsFromCard(def, target);

            // 7) Invalidate cache + mark pending UI if the card affects sound
            if (loopIsRunning && affectsSound)
            {
                bool keepTempo = ShouldKeepTempo(def);
                bool keepInstruments = ShouldKeepInstruments(def);

                int invalidateIdx = (zone == CardDropZone.NextPart) ?
                    partIdx : _currentPartIndex;

                Info($"invalidating cache part={invalidateIdx} keepTempo={keepTempo} " +
                    $"keepInstruments={keepInstruments} affectsSound={affectsSound}");

                InvalidatePartCache(invalidateIdx, keepTempo, keepInstruments);

                // [B1 / #1+#2 / D-H1=α] Mark pending visualization on UI.
                //
                // Priority order:
                // 1. If the card affects part-level meter (TS, tonality, root,
                //    tempo) → mark ALL tracks pending. The partMeterHash will
                //    change, so every stem regenerates next loop, even if the
                //    card also has a track effect (e.g. Pentameter sets TS=5/4
                //    AND replaces the Rhythm track).
                // 2. Else if it's a pure track card with a resolved target →
                //    mark only the target's track.
                // 3. Else (catch-all for non-track sound-affecting cards
                //    without explicit meter change) → mark all tracks.
                if (affectsPartMeter)
                {
                    Info($"[Pending] part-meter card → MarkAllTracksPending " +
                        $"partIdx={invalidateIdx} " +
                        $"(isTempo={isTempo} isTimeSig={isTimeSig} " +
                        $"isTonality={isTonality} isModulation={isModulation})");
                    ui.MarkAllTracksPending(invalidateIdx);
                }
                else if (isTrack
                    && target?.MusicianCharacterData != null
                    && !string.IsNullOrEmpty(target.MusicianCharacterData.CharacterId))
                {
                    Info($"[Pending] track-card → MarkTrackPending " +
                        $"partIdx={invalidateIdx} mus='{target.MusicianCharacterData.CharacterId}' " +
                        $"name='{target.MusicianCharacterData.CharacterName}'");
                    ui.MarkTrackPending(invalidateIdx, target.MusicianCharacterData.CharacterId);
                }
                else
                {
                    Info($"[Pending] other → MarkAllTracksPending " +
                        $"partIdx={invalidateIdx} isTrack={isTrack} " +
                        $"target='{(target?.MusicianCharacterData?.CharacterName ?? "null")}'");
                    ui.MarkAllTracksPending(invalidateIdx);
                }
            }

            // 8) Spend / preview inspiration
            if (def.IsComposition)
            {
                int cost = Math.Max(0, def.InspirationCost);
                _currentInspiration = Math.Max(0, _currentInspiration - cost);
                ui.SetInspiration(_currentInspiration);

                int gen = Math.Max(0, def.InspirationGenerated);
                if (gen > 0)
                {
                    _buildingPartInspirationPerLoop += gen;
                    Info($"per-loop inspiration bonus updated: +={gen} " +
                        $"(now {_buildingPartInspirationPerLoop})");
                }
            }

            // 9) Refresh per-loop inspiration for the currently looping part if we changed tracks
            if (isTrack && _state != CompositionState.BuildingCurrentPart)
            {
                if (_currentPartIndex >= 0 && _currentPartIndex < ui.Model.parts.Count)
                {
                    _perLoopInspirationCurrentPart =
                        EvalPerLoopInsp(ui.Model.parts[_currentPartIndex]);

                    ui.SetPlusInspiration(GlobalPerLoopBadgeValue);
                    Info($"recalc per-loop inspiration for " +
                        $"currentPart={_currentPartIndex} → {_perLoopInspirationCurrentPart}");
                }
            }

            Info("SUCCESS");
            return true;
        }


        // ----- Private methods -----

        private void ApplyStatusActionsFromCard(CardDefinition def, MusicianBase target)
        {
            var payload = def != null ? def.Payload : null;
            var effects = payload != null ? payload.Effects : null;
            if (effects == null || effects.Count == 0) return;

            for (int i = 0; i < effects.Count; i++)
            {
                if (effects[i] is not ApplyStatusEffectSpec ase)
                    continue;

                if (ase.stacksDelta == 0) continue;

                if (ase.status == null)
                {
                    _ctx?.Log($"[Effects][WARN] Card '{def?.DisplayName}' has ApplyStatusEffectSpec with null status.", true);
                    continue;
                }

                // Composition session no es coroutine -> ignorar delay por ahora (igual que antes)
                // Si quieres: loggear cuando delay > 0.
                if (ase.delay > 0f)
                    _ctx?.Log($"[Effects][INFO] Ignoring delay={ase.delay} on '{def?.DisplayName}' (CompositionSession).", false);

                switch (ase.targetType)
                {
                    case ActionTargetType.Self:
                    case ActionTargetType.Musician:
                        if (target == null)
                        {
                            _ctx?.Log($"[Effects][WARN] '{def?.DisplayName}' targets musician/self but target is null.", true);
                            break;
                        }
                        if (target.Statuses == null)
                        {
                            _ctx?.Log($"[Effects][WARN] Target '{target.name}' has no Statuses container.", true);
                            break;
                        }
                        target.Statuses.Apply(ase.status, ase.stacksDelta);
                        break;

                    case ActionTargetType.AllMusicians:
                        if (_ctx?.Band == null) break;
                        for (int m = 0; m < _ctx.Band.Count; m++)
                        {
                            var mus = _ctx.Band[m];
                            if (mus == null || mus.Statuses == null) continue;
                            mus.Statuses.Apply(ase.status, ase.stacksDelta);
                        }
                        break;

                    // MVP: ignora audiencia en CompositionSession, como antes
                    case ActionTargetType.AudienceCharacter:
                    case ActionTargetType.AllAudienceCharacters:
                    case ActionTargetType.RandomAudienceCharacter:
                        _ctx?.Log($"[Effects][INFO] Ignoring audience-targeted effect '{ase.status.EffectId}' in CompositionSession (MVP).", false);
                        break;

                    case ActionTargetType.RandomMusician:
                        if (_ctx?.Band == null || _ctx.Band.Count == 0) break;
                        var idx = _rng.Next(0, _ctx.Band.Count);
                        var rand = _ctx.Band[idx];
                        if (rand != null && rand.Statuses != null)
                            rand.Statuses.Apply(ase.status, ase.stacksDelta);
                        break;

                    default:
                        _ctx?.Log($"[Effects][WARN] Unhandled ActionTargetType '{ase.targetType}' in CompositionSession.", true);
                        break;
                }
            }
        }

        [Obsolete("Deck/hand lifecycle is owned by GigManager/DeckManager. CompositionSession must not mutate piles.")]
        private void PrepareDeck()
        {
            // Intentionally empty. Any future deck refresh must be requested via the host (GigManager),
            // not performed here (otherwise we risk wiping the player's hand).
        }


        private float PlaySinglePartLoop(int partIndex)
        {
            var mm = _ctx.Music;
            if (mm == null) return 0f;

            var cfg = BuildSongConfigFromUI();
            if (cfg == null) return 0f;

            if (partIndex < 0 || partIndex >= cfg.Parts.Count) return 0f;

            var ownerIds = mm.GetChannelOwnerIdsFor(cfg);
            mm.SetChannelOwners(ownerIds?.ToList());

#if ALWTTT_DEV
            // [DBG-C2 / D-C2-3=A, D-C2-4=A] Override state changed since this
            // part was last rendered (assign/clear/Roman/R2a) → invalidate so
            // the miss branch below re-renders with the current overrides.
            // keepTempo+keepInstruments: overrides change patterns, not the
            // part's BPM or the musicians' voices.
            if (_partCache.TryGetValue(partIndex, out var devEntry)
                && devEntry != null
                && devEntry.devOverrideStamp != DevOverrideStamp)
            {
                Debug.Log($"<color=lime>[DBG-C2]</color> Override stamp " +
                    $"{devEntry.devOverrideStamp}→{DevOverrideStamp} — invalidating " +
                    $"part {partIndex} cache (keepTempo, keepInstruments).");
                InvalidatePartCache(partIndex, keepTempo: true,
                    keepInstrumentsOverride: true);
            }
#endif

            if (!_partCache.TryGetValue(partIndex, out var cache)
                || cache?.mergedBytes == null || cache.mergedBytes.Length == 0)
            {
                int? bpmOverride = (cache != null && cache.resolvedBpm > 0)
                    ? cache.resolvedBpm : (int?)null;

                // [LOG-1] The F-4 boundary-call shape dump was removed at F-4
                // closure. It fired on every render that missed the cache and
                // had no error-path counterpart, so nothing is lost by taking
                // it out. Its two locals went with it.

                // [B1 / D-E=α'] Compute UI-stable input hashes per musician
                // for this part. Passed to RenderSinglePart so the stem cache
                // keys on player-controlled inputs (not the random instrument
                // resolution that happens inside FromUI).
                var trackInputsHashes =
                    Music.SongConfigBuilder.ComputeTrackInputsHashesForPart(
                        _ctx, partIndex,
                        mm.GigMixGains); // [BAL-1] gain enters the hash (D-BAL-3=A)

                // [B1 / #7.1 / D-F=γ] Apply session-level instrument pins to
                // cfg before the render. Keeps the same musician's voice
                // consistent when only style/role-style changes. Respects
                // explicit instrument overrides from cards (skipped per-track
                // when UI TrackEntry has overrideMelodicInstrument /
                // overridePercussionInstrument set).
                ApplyInstrumentPins(cfg, partIndex);

                // [DBG-C1] instrumentOverrides is composite-keyed: one entry
                // per (musician, role). [JAM-1] The override map now carries a
                // second, production-live source: the stored shared harmony,
                // imposed when this render should ACCOMPANY rather than lead.
                // NOTE: any non-empty map makes MMM bypass its stem/bundle
                // caches (D-C2-4=A, MidiMusicManager.cs:961). Accepted cost.
                Dictionary<MusicianTrackKey, PatternDataSO> overrides = null;

                if (_jamProgressionByPart.TryGetValue(partIndex, out var jamProg)
                    && jamProg != null
                    && !JamTonalityMovedSinceCapture(partIndex)
                    && TryGetBackingJamTarget(partIndex, out var backingKey, out var adopts)
                    && !adopts)
                {
                    overrides = new Dictionary<MusicianTrackKey, PatternDataSO>
                    { [backingKey] = jamProg };
                    _ctx?.Log($"[JAM-1] part={partIndex} imposing shared progression " +
                              $"'{jamProg.name}' on Backing (accompany policy, D-R3C-2=A)", true);

                    // [JAM-2 / F-JAM-SCALE-SPLIT] Impose the MODE as well as the
                    // chords. Without this the part renders every OTHER track
                    // (melody, lead) against the model's scale while the Backing
                    // plays chords authored in a different mode — e.g. B major
                    // (with D#) over an Ionian A scale (with D natural). Chords
                    // survive because the package renders them AsAuthored; the
                    // scale does not, because nothing carried it across.
                    //
                    // Scope is the per-render cfg, which BuildSongConfigFromUI
                    // rebuilds every loop. The model is NOT touched, so D-R3C-3=A'
                    // holds: the mode lives exactly as long as the jam entry does.
                    if (_jamRenderedTonalityByPart.TryGetValue(partIndex, out var jamRen)
                        && cfg?.Parts != null
                        && partIndex >= 0 && partIndex < cfg.Parts.Count
                        && cfg.Parts[partIndex] != null)
                    {
                        var jamCfgPart = cfg.Parts[partIndex];
                        if (jamCfgPart.Tonality != jamRen.ton || jamCfgPart.RootNote != jamRen.root)
                        {
                            _ctx?.Log($"[JAM-2] part={partIndex} aligning render tonality " +
                                      $"{jamCfgPart.Tonality}/{jamCfgPart.RootNote} -> " +
                                      $"{jamRen.ton}/{jamRen.root} (mode of the imposed harmony)", true);
                            jamCfgPart.Tonality = jamRen.ton;
                            jamCfgPart.RootNote = jamRen.root;
                        }
                    }
                }
#if ALWTTT_DEV
                // [DBG-C2] Dev overrides deliberately win over JAM-1: the dev
                // tab exists precisely to force a pattern regardless of policy.
                if (DevPatternOverrides.Count > 0)
                {
                    overrides ??= new Dictionary<MusicianTrackKey, PatternDataSO>();
                    foreach (var kv in DevPatternOverrides)
                        overrides[kv.Key] = kv.Value;
                }
#endif
                var (merged, stems, seconds, bpmChosen, instByTrack) =
                    mm.RenderSinglePart(cfg, partIndex, bpmOverride,
                        cache?.resolvedMelInstByTrack,
                        trackInputsHashes,
                        seedOverride: _songSeed,      // [S5g / MGP-ALWTTT-SEED-1]
                        patternOverrides: overrides); // [DBG-C2 + JAM-1]

                if (merged == null || merged.Length == 0 || seconds <= 0f) return 0f;

                // [B1 / #7.1 / D-F=γ] Record resolved instruments into the
                // session-level pin map for future renders to reuse.
                UpdateInstrumentPins(cfg, partIndex);

                // [JAM-1 / D-R3C-4=B′] Capture AFTER the render — adoption mutates
                // the PartConfig in place during compose, so the post-render
                // tonality is the mode this progression actually rendered in.
                //
                // But capture ONLY when the harmony came from somewhere OTHER than
                // the Backing card's own fixed override. Rationale: JAM-1 exists to
                // carry forward harmony a LATER card would otherwise overwrite. A
                // card's own progressionOverride cannot drift between renders, so
                // re-imposing it on itself buys nothing and costs the stem/bundle
                // cache (D-C2-4=A bypasses on any override). Procedural and palette
                // sources DO drift, so those still get pinned.
                //
                // On the CardOverride path we CLEAR rather than leave stale: that
                // card's fixed harmony is now the part's truth, and a leftover entry
                // would keep the tonality snapshot frozen at an older render.
                var jamSrc = mm.LastSharedProgressionSource;
                bool jamCaptureable =
                    jamSrc != ResolvedSource.CardOverride
                    && mm.LastSharedProgressionData != null;

                if (jamCaptureable)
                {
                    _jamProgressionByPart[partIndex] = mm.LastSharedProgressionData;
                    var jamPart = _ctx?.CompositionUI?.Model?.parts?[partIndex];
                    if (jamPart != null)
                        _jamTonalitySnapByPart[partIndex] = (jamPart.tonality, jamPart.rootNote);

                    // [JAM-2] The package mutates cfg.Parts[partIndex] IN PLACE when
                    // the Backing card adopts, so after the render this reads the mode
                    // the chords actually sounded in — Lydian where the model still
                    // says Ionian. No new package readback is needed: cfg is ours.
                    // Captured post-render for the same reason B' captures late.
                    if (cfg?.Parts != null
                        && partIndex >= 0 && partIndex < cfg.Parts.Count
                        && cfg.Parts[partIndex] != null)
                    {
                        var jamRenPart = cfg.Parts[partIndex];
                        _jamRenderedTonalityByPart[partIndex] =
                            (jamRenPart.Tonality, jamRenPart.RootNote);
                    }
                }
                else
                {
                    _jamProgressionByPart.Remove(partIndex);
                    _jamTonalitySnapByPart.Remove(partIndex);
                    _jamRenderedTonalityByPart.Remove(partIndex);   // [JAM-2]
                }

                // [B1 / #1+#2 / D-H4=α] Fresh render succeeded for this part —
                // clear pending visualization on its tracks.
                _ctx?.CompositionUI?.OnRenderCompleted(partIndex);

                if (cache == null) cache = new PartCache();
                cache.mergedBytes = merged;
                cache.seconds = seconds;
                cache.stemsByTrack = stems ?? new Dictionary<MusicianTrackKey, byte[]>();
                cache.resolvedBpm = bpmChosen;
                // [DBG-C1] Readback is (musician, role)-keyed — unambiguous for
                // every musician; the BASS-1 single-track guard is retired.
                if (instByTrack != null)
                    foreach (var kv in instByTrack)
                        cache.resolvedMelInstByTrack[kv.Key] = kv.Value;

                _partCache[partIndex] = cache;
#if ALWTTT_DEV
                cache.devOverrideStamp = DevOverrideStamp; // [DBG-C2]
#endif
            }

            if (cache.resolvedBpm > 0)
            {
                _ctx?.OnPartBpmResolved(partIndex, cache.resolvedBpm);
            }

            var partName = _ctx.CompositionUI.Model.parts[partIndex].label;

            // [SINGER-1] Announce before PlayRaw so the singer is armed when
            // OnSongStarted fires. Subscriber failures must never kill the loop.
            try
            {
                var pcfg = cfg.Parts[partIndex];
                LoopPlaybackStarting?.Invoke(new ALWTTT.Music.Voice.SingerLoopContext
                {
                    partIndex = partIndex,
                    stemsByTrack = cache.stemsByTrack,
                    tonality = pcfg.Tonality,
                    rootNote = pcfg.RootNote,
                    timeSignature = pcfg.TimeSignature,
                    bpm = cache.resolvedBpm,
                    seconds = cache.seconds
                });
            }
            catch (Exception ex)
            {
                Debug.Log($"[SINGER-1] LoopPlaybackStarting part={partIndex} " +
                  $"subs={LoopPlaybackStarting?.GetInvocationList().Length ?? 0} " +
                  $"stems={cache.stemsByTrack?.Count ?? -1} bpm={cache.resolvedBpm}");
            }

            var duration =
                mm.PlayRaw(cache.mergedBytes, cache.seconds,
                $"Part {partIndex} (cached:{partName})");
            if (duration <= 0f) return 0f;

            _isPlaying = true;
            _loopStartTime = Time.time;
            _loopDurationSeconds = duration;
            return duration;
        }

        private SongConfig BuildSongConfigFromUI()
        {
            var ui = _ctx.CompositionUI; if (ui == null) return null;

            var instruments = new InstrumentRepositoryResources(_settings);
            var patterns = new PatternRepositoryResources(_settings);
            instruments.Refresh();
            patterns.Refresh();

            return SongConfigBuilder.FromUI(
                ctx: _ctx,
                instruments: instruments,
                patterns: patterns,
                getPermittedMelodic: (mus, role) =>
                    InstrumentRules.GetPermittedMelodic(mus, role, instruments),
                _rng
            );
        }

        private static int EvalPerLoopInsp(SongCompositionUI.PartEntry part)
        {
            if (part == null || part.tracks == null) return 0;
            int sum = 0;
            foreach (var t in part.tracks)
            {
#if ALWTTT_DEV
                // [D-CSV-24=B] Audition-only tracks contribute no per-loop
                // inspiration — R2a is economy-neutral.
                if (t != null && !string.IsNullOrEmpty(t.musicianId)
                    && _devInjectedTrackKeys.Contains(new MusicianTrackKey(t.musicianId, t.role)))
                    continue;
#endif
                sum += Math.Max(0, t.inspirationGenerated);
                // [DF-INSPLOOP / D-INSP-1=D] Card-gated per-loop bonus, derived
                // from the track's source card. Track replaced/removed → bonus
                // gone with it. Never touches inspirationGenerated, so
                // LoopTrackSnapshot / TotalComplexity stay inert (D-INSP-4).
                sum += ALWTTT.Cards.Effects.AddInspirationPerLoopSpec.SumFor(
                    t.sourceCardDefinition);
            }
            return sum;
        }

        /// <summary>
        /// Canonical session-budget inspiration mutator. Clamps to
        /// PersistentGameplayData.MaxInspiration, refreshes the composition UI,
        /// and mirrors the result to PersistentGameplayData.CurrentInspiration
        /// so in-session and persistent fields stay in sync.
        ///
        /// Returns the actual delta applied (post-clamp). Returns 0 when
        /// already at cap or when delta=0.
        ///
        /// Used by:
        /// - track-derived per-loop gain (HandleLoopFinished)
        /// - host hooks via _session.AddCurrentInspiration (M4.6F-3
        ///   OnCompositionLoopFinished)
        /// </summary>
        public int AddCurrentInspiration(int delta)
        {
            if (delta == 0) return 0;

            var pd = GameManager.Instance != null
                ? GameManager.Instance.PersistentGameplayData
                : null;
            int max = pd != null ? pd.MaxInspiration : int.MaxValue;

            int before = _currentInspiration;
            int after = Mathf.Clamp(before + delta, 0, max);
            if (after == before) return 0;

            _currentInspiration = after;
            _ctx?.CompositionUI?.SetInspiration(_currentInspiration);

            if (pd != null) pd.CurrentInspiration = after;

            return after - before;
        }

        // [DF-INSPLOOP-badge/ST-2] El badge global +INS muestra el TOTAL por loop =
        // grant plano (pd.InspirationPerLoop, lo aplica GigManager.OnCompositionLoopFinished)
        // + derivado de pistas/carta (EvalPerLoopInsp, se aplica aquí). Nominal/un-clamped (S5e).
        private int FlatPerLoopGrant
        {
            get
            {
                var pd = GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;
                return pd != null ? Math.Max(0, pd.InspirationPerLoop) : 0;
            }
        }
        private int GlobalPerLoopBadgeValue => FlatPerLoopGrant + _perLoopInspirationCurrentPart;

        private void HandleLoopFinished()
        {
            _isPlaying = false;

            // [TUT-R2b / D-TUT-R2b-1=B] El loop repite en CUALQUIER frontera
            // mientras haya un diálogo en pantalla (TutorialModalGate), no solo
            // en el hold dirigido del beat 8. La música sigue sonando (sin
            // freeze de timeScale, precedente S4 intacto); lo que se detiene es
            // la PROGRESIÓN: sin decremento, sin inspiración, sin
            // LoopResolvedEvent, sin snapshots. Al cerrar el diálogo, el
            // siguiente fin de loop resuelve con normalidad.
            if (ALWTTT.Tutorial.TutorialModalGate.IsActive ||
                (_loopsRemainingForPart == 1 &&
                 ALWTTT.Tutorial.TutorialLoopHoldGate.IsArmed))
            {
                float heldSecs = PlaySinglePartLoop(_currentPartIndex);
                if (heldSecs <= 0f)
                {
                    ALWTTT.Tutorial.TutorialLoopHoldGate.Release();
                    End();
                    return;
                }
                return;
            }

            _loopsRemainingForPart--;

            int inspirationGainedThisLoop = _perLoopInspirationCurrentPart;
            if (inspirationGainedThisLoop > 0)
            {
                AddCurrentInspiration(inspirationGainedThisLoop);
                // Badge shows un-clamped per-loop track contribution: the
                // player's signal of next-loop potential, independent of cap.
                _ctx.CompositionUI?.SetPlusInspiration(GlobalPerLoopBadgeValue);
            }

            var model = _ctx.CompositionUI?.Model;
            SongCompositionUI.PartEntry partEntry = null;

            if (model != null &&
                _currentPartIndex >= 0 &&
                _currentPartIndex < model.parts.Count)
            {
                partEntry = model.parts[_currentPartIndex];
            }

            var trackSnapshots = new List<LoopTrackSnapshot>();
            if (partEntry?.tracks != null)
            {
                foreach (var t in partEntry.tracks)
                {
                    var snap = new LoopTrackSnapshot(
                        musicianId: t.musicianId,
                        role: t.role,
                        synergyType: t.synergyType,
                        inspirationGenerated: t.inspirationGenerated,
                        info: t.info
                    );
                    trackSnapshots.Add(snap);
                }
            }

            int loopIndex0 = _loopsTotalForPart - _loopsRemainingForPart - 1;
            string partLabel = _ctx.CompositionUI?.GetPartLabel(_currentPartIndex) ??
                $"Part {_currentPartIndex}";

            // [B3] Read musical identity from the active PartEntry so audience
            // taste preferences (B3-code-F) can react to what actually played.
            // Defaults match "song-authored default" when no PartEntry data is
            // available — TempoScale=1.0, and the enum defaults (typically 4/4,
            // C, Ionian) — so the ctx is always sane.
            float tempoScale = partEntry?.tempoScale ?? 1f;
            TimeSignature timeSignature = partEntry?.timeSignature ?? default;
            NoteName rootNote = partEntry?.rootNote ?? default;
            Tonality tonality = partEntry?.tonality ?? default;

            var ctx = new LoopFeedbackContext(
                partIndex: _currentPartIndex,
                loopIndexWithinPart: loopIndex0,
                loopsInPart: _loopsTotalForPart,
                partLabel: partLabel,
                inspirationGainedThisLoop: inspirationGainedThisLoop,
                inspirationAfterLoop: _currentInspiration,
                tracks: trackSnapshots,
                tempoScale: tempoScale,
                timeSignature: timeSignature,
                rootNote: rootNote,
                tonality: tonality
            );

#if UNITY_EDITOR
            // [B3 / ST-B3b-CTX1] Diagnostic — verify musical identity surfaces
            // correctly. Strip at B3 closure if log noise becomes friction.
            // [LOG-1] B3 is closed and the noise became friction: demoted to
            // the verbose tier rather than stripped, so the identity readout is
            // still one toggle away.
            var devLoopCtx = ALWTTT.Managers.GigManager.Instance != null
                ? ALWTTT.Managers.GigManager.Instance.DevSettings : null;
            if (devLoopCtx != null && devLoopCtx.UseLogs && devLoopCtx.LogVerbose)
                Debug.Log(
                    $"<color=cyan>[LoopCtx] Part={_currentPartIndex} " +
                    $"Loop={loopIndex0 + 1}/{_loopsTotalForPart} " +
                    $"TempoScale={tempoScale:0.##} TS={timeSignature} " +
                    $"Root={rootNote} Tonality={tonality}</color>");
#endif

            // Store in per-part history
            if (!_loopHistoryByPart.TryGetValue(_currentPartIndex, out var list))
            {
                list = new List<LoopFeedbackContext>();
                _loopHistoryByPart[_currentPartIndex] = list;
            }
            list.Add(ctx);

            LoopFinished?.Invoke(ctx);

#if ALWTTT_DEV
            // [DBG-C1 / D2=A] Infinite composition loop: when the countdown
            // would exhaust, reset it to the full per-part value instead of
            // reaching the part-advance / song-end branch. Everything above —
            // decrement, per-loop inspiration, LoopFeedbackContext, history,
            // LoopFinished subscribers (host draw hooks) — already ran for
            // this loop, exactly as in normal flow. Toggling OFF simply lets
            // the restored countdown run out normally.
            if (DevInfiniteCompositionLoop && _loopsRemainingForPart <= 0)
            {
                _loopsRemainingForPart = _loopsTotalForPart;
                Debug.Log(
                    "<color=lime>[DevMode][InfLoop]</color> Countdown exhausted → " +
                    $"reset to {_loopsTotalForPart}. Part {_currentPartIndex} keeps looping.");
            }
#endif

            if (_loopsRemainingForPart > 0)
            {
                // F-4 D3-B (Stage A, production-quality): mirror AdvanceToNextPart's
                // graceful-end pattern when PlaySinglePartLoop returns 0f. Without
                // this guard, a render failure mid-part leaves _loopStartTime /
                // _loopDurationSeconds stale (PlaySinglePartLoop only updates them
                // on success at lines 532-533) and the Update tick spins re-firing
                // HandleLoopFinished. End the session cleanly instead. NOT [F-4]-
                // tagged for revert — the underlying invariant (capture-and-gate
                // PlaySinglePartLoop's return) is permanent.
                float secs = PlaySinglePartLoop(_currentPartIndex);
                if (secs <= 0f) { End(); return; }
                return;
            }

            EmitPartFinishedForCurrentPart();

            if (ComputeNextPartIsReady())
            {
                AdvanceToNextPart();
                return;
            }

            End();
        }

        private void EmitPartFinishedForCurrentPart()
        {
            int partIndex = _currentPartIndex;

            _loopHistoryByPart.TryGetValue(partIndex, out var loops);
            loops ??= new List<LoopFeedbackContext>();

            string partLabel = null;
            var ui = _ctx.CompositionUI;
            if (ui != null &&
                ui.Model != null &&
                ui.Model.parts != null &&
                partIndex >= 0 &&
                partIndex < ui.Model.parts.Count)
            {
                partLabel = ui.Model.parts[partIndex].label;
            }

            var partCtx = new PartFeedbackContext(
                partIndex: partIndex,
                partLabel: partLabel,
                loops: loops,
                audienceLoopImpressions: null // to be filled later by GigManager
            );

            PartFinished?.Invoke(partCtx);
            _finishedParts.Add(partCtx);

            // we don't need to keep per-loop history for this part anymore
            _loopHistoryByPart.Remove(partIndex);

            _ctx.Log(partCtx.ToString());
        }

        private bool ComputeNextPartIsReady() =>
            _ctx.CompositionUI != null
            && _ctx.CompositionUI.HasPlayableNextPart(_currentPartIndex);

        private void AdvanceToNextPart()
        {
            var ui = _ctx.CompositionUI;
            int nextIdx = _currentPartIndex + 1;
            if (ui == null || nextIdx >= ui.Model.parts.Count) { End(); return; }

            _currentPartIndex = nextIdx;
            ui.SetIconReferencePartIndex(_currentPartIndex);

            _loopsTotalForPart = _rules.loopsPerPart;
            _loopsRemainingForPart = _rules.loopsPerPart;

            float secs = PlaySinglePartLoop(_currentPartIndex);
            if (secs <= 0f) { End(); return; }

            _loopDurationSeconds = secs;
            _loopStartTime = Time.time;

            _ctx.LoopsTimerUI?.BuildBars(_loopsTotalForPart);
            _ctx.LoopsTimerUI?.SetProgress(0, 0f);
            _ctx.LoopsTimerUI?.SetBarsVisible(true);

            _perLoopInspirationCurrentPart = EvalPerLoopInsp(ui.Model.parts[_currentPartIndex]);
            ui.SetPlusInspiration(GlobalPerLoopBadgeValue);

            bool final = ui.IsPartFinal(_currentPartIndex);
            if (!final)
            {
                _state = CompositionState.BuildingNextPart;
                _currentInspiration = ResolveSessionStartInspiration();
                ui.SetInspiration(_currentInspiration);
                ui.BeginDraftNextPart($"Part {_currentPartIndex + 2}");
            }
            else
            {
                _state = CompositionState.PlayingCurrentPart; // solo reproduce y termina
            }
        }

        /// <summary>
        /// MB3 D3: session-start inspiration semantic.
        /// inspirationPerPart == 0 → carry-over from PersistentGameplayData.CurrentInspiration.
        /// inspirationPerPart != 0 → reset to rules value (existing behavior).
        /// Applied symmetrically by Begin, ConfirmCurrentPartAndStart, AdvanceToNextPart.
        /// </summary>
        private int ResolveSessionStartInspiration()
        {
            if (_rules.inspirationPerPart != 0)
                return _rules.inspirationPerPart;

            var pd = GameManager.Instance != null
                ? GameManager.Instance.PersistentGameplayData
                : null;
            return pd != null ? pd.CurrentInspiration : 0;
        }

        private bool ShouldKeepTempo(CardDefinition c)
        {
            // Keep current tempo unless the played card explicitly changes tempo/time signature.
            if (c == null) return true;

            var comp = c.CompositionPayload;
            if (comp == null) return true;

            // Tempo cards force a tempo re-resolve.
            if (CompositionCardClassifier.IsTempoCard(comp))
                return false;

            // Time signature cards: keep tempo (tempo stays, grid changes).
            if (CompositionCardClassifier.IsTimeSignatureCard(comp))
                return true;

            // Track cards / tonality cards: keep tempo.
            // (We don't have a Tonality classifier yet; default keep.)
            return true;
        }

        private bool ShouldKeepInstruments(CardDefinition c)
        {
            if (c == null) return true;

            var comp = c.CompositionPayload;
            if (comp == null) return true;

            // Only cards that are clearly "instrument changing"
            // should force a re-roll of instruments.
            // Everything else keeps pinned instruments.
            if (CompositionCardClassifier.IsInstrumentCard(comp))
                return false;

            return true;
        }

        private void InvalidatePartCache(
            int partIndex,
            bool keepTempo,
            bool? keepInstrumentsOverride = null)
        {
            if (!_partCache.TryGetValue(partIndex, out var cache) || cache == null) return;

            int preservedBpm = keepTempo ? cache.resolvedBpm : 0;

            bool keepInstruments = keepInstrumentsOverride ??
                GetKeepInstrumentForPart(partIndex);

            _partCache[partIndex] = new PartCache
            {
                mergedBytes = null,
                seconds = 0f,
                resolvedBpm = preservedBpm,
                stemsByTrack = new Dictionary<MusicianTrackKey, byte[]>(),
                resolvedMelInstByTrack = keepInstruments
                    ? cache.resolvedMelInstByTrack
                    : new Dictionary<MusicianTrackKey, MIDIInstrumentSO>(),
                resolvedPercInstByTrack = new()
            };
        }

        // [JAM-1] Did the part's tonality move since we captured the harmony?
        // Any effect that changes tonality (TonalityEffect, ModulationEffect)
        // mutates the UI model BEFORE the render, so one comparison catches
        // them all — no coupling to the effect taxonomy.
        private bool JamTonalityMovedSinceCapture(int partIndex)
        {
            if (!_jamTonalitySnapByPart.TryGetValue(partIndex, out var snap)) return true;
            var p = _ctx?.CompositionUI?.Model?.parts;
            if (p == null || partIndex < 0 || partIndex >= p.Count) return true;
            return p[partIndex].tonality != snap.ton || p[partIndex].rootNote != snap.root;
        }

        // [JAM-1] Does the Backing card on this part opt into compose-time
        // tonality adoption? That mutation happens DURING the render, so the
        // comparison above cannot see it — hence this explicit flag check.
        // Returns the Backing track key as a by-product (that is the key the
        // override must be filed under).
        private bool TryGetBackingJamTarget(int partIndex, out MusicianTrackKey key, out bool adopts)
        {
            key = default; adopts = false;
            var p = _ctx?.CompositionUI?.Model?.parts;
            if (p == null || partIndex < 0 || partIndex >= p.Count) return false;
            var tr = p[partIndex].tracks?.FirstOrDefault(t => t.role == TrackRole.Backing);
            if (tr == null || string.IsNullOrEmpty(tr.musicianId)) return false;
            key = new MusicianTrackKey(tr.musicianId, TrackRole.Backing);
            adopts = (tr.styleBundle as BackingCardConfigSO)?.adoptProgressionTonality == true;
            return true;
        }

        private void ApplyInstrumentPins(SongConfig cfg, int partIndex)
        {
            if (cfg == null) return;
            if (partIndex < 0 || partIndex >= cfg.Parts.Count) return;
            var cfgPart = cfg.Parts[partIndex];
            if (cfgPart?.Tracks == null) return;

            var ui = _ctx?.CompositionUI;
            if (ui?.Model?.parts == null) return;
            if (partIndex >= ui.Model.parts.Count) return;
            var uiPart = ui.Model.parts[partIndex];
            if (uiPart?.tracks == null) return;

            foreach (var tcfg in cfgPart.Tracks)
            {
                if (tcfg == null || string.IsNullOrEmpty(tcfg.MusicianId)) continue;
                // [BASS-1 / R11] Role-scoped: a musician may hold multiple
                // role-tracks; a musician-only Find would match the wrong
                // TrackEntry. The pin keys were ALREADY mus|role — only this
                // lookup was musician-only.
                var trModel = uiPart.tracks.Find(t =>
                    t.musicianId == tcfg.MusicianId && t.role == tcfg.Role);
                if (trModel == null) continue;

                var melKey = BuildMelodicPinKey(tcfg, trModel);
                if (melKey != null
                    && _sessionMelodicPin.TryGetValue(melKey, out var melPin)
                    && melPin != null)
                {
                    tcfg.Instrument = melPin;
                }

                var percKey = BuildPercussionPinKey(tcfg, trModel);
                if (percKey != null
                    && _sessionPercussionPin.TryGetValue(percKey, out var percPin)
                    && percPin != null)
                {
                    tcfg.PercussionInstrument = percPin;
                }
            }
        }

        private void UpdateInstrumentPins(SongConfig cfg, int partIndex)
        {
            if (cfg == null) return;
            if (partIndex < 0 || partIndex >= cfg.Parts.Count) return;
            var cfgPart = cfg.Parts[partIndex];
            if (cfgPart?.Tracks == null) return;

            var ui = _ctx?.CompositionUI;
            if (ui?.Model?.parts == null) return;
            if (partIndex >= ui.Model.parts.Count) return;
            var uiPart = ui.Model.parts[partIndex];
            if (uiPart?.tracks == null) return;

            foreach (var tcfg in cfgPart.Tracks)
            {
                if (tcfg == null || string.IsNullOrEmpty(tcfg.MusicianId)) continue;
                // [BASS-1 / R11] Role-scoped (see ApplyInstrumentPins).
                var trModel = uiPart.tracks.Find(t =>
                    t.musicianId == tcfg.MusicianId && t.role == tcfg.Role);
                if (trModel == null) continue;

                var melKey = BuildMelodicPinKey(tcfg, trModel);
                if (melKey != null && tcfg.Instrument != null)
                    _sessionMelodicPin[melKey] = tcfg.Instrument;

                var percKey = BuildPercussionPinKey(tcfg, trModel);
                if (percKey != null && tcfg.PercussionInstrument != null)
                    _sessionPercussionPin[percKey] = tcfg.PercussionInstrument;
            }
        }

        // SO-specific override → pin skipped (deterministic; FromUI's pick IS the override).
        // Type override → pin key includes TYPE:<value> so changing types refreshes the pick.
        // No override → pin key uses |NONE so it persists across style changes.
        private static string BuildMelodicPinKey(
            SongConfig.PartConfig.TrackConfig tcfg,
            SongCompositionUI.TrackEntry trModel)
        {
            if (trModel == null) return null;
            if (trModel.overrideMelodicInstrument != null) return null;
            if (trModel.hasOverrideInstrumentType)
                return $"{tcfg.MusicianId}|{tcfg.Role}|TYPE:{trModel.overrideInstrumentType}";
            return $"{tcfg.MusicianId}|{tcfg.Role}|NONE";
        }

        private static string BuildPercussionPinKey(
            SongConfig.PartConfig.TrackConfig tcfg,
            SongCompositionUI.TrackEntry trModel)
        {
            if (trModel == null) return null;
            if (trModel.overridePercussionInstrument != null) return null;
            return $"{tcfg.MusicianId}|{tcfg.Role}|NONE";
        }
    }
}