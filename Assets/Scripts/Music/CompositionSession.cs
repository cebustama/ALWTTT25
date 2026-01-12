using ALWTTT.Cards;
using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.Status;
using ALWTTT.UI;
using MidiGenPlay;
using MidiGenPlay.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public event Action<PartFeedbackContext> PartFinished;
        public event Action<SongFeedbackContext> SongFinished;

        public class PartCache
        {
            public byte[] mergedBytes;
            public float seconds;
            public int resolvedBpm;
            public Dictionary<string, byte[]> stemsByMusician = new();
            public Dictionary<string, MIDIInstrumentSO> resolvedMelInstByMusician = new();
            public Dictionary<string, MIDIPercussionInstrumentSO> 
                resolvedPercInstByMusician = new();
        }

        private readonly Dictionary<int, PartCache> _partCache = new();

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
        /// True while the session is active (after Begin and before End).
        /// </summary>
        public bool IsActive =>
            _state != CompositionState.Idle &&
            _state != CompositionState.Ended;

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
            _loopHistoryByPart.Clear();
            _finishedParts.Clear();
            _buildingPartInspirationPerLoop = 0;

            _keepInstrumentByPart.Clear();

            _ctx.ShowCompositionUI(true);
            _ctx.ShowHand(true);

            var ui = _ctx.CompositionUI;
            ui?.ResetSession();
            ui?.PopulateMusicianIcons(_ctx.Band);
            ui?.SetIconReferencePartIndex(0);

            PrepareDeck();
            _currentInspiration = _rules.inspirationPerPart;
            ui?.SetInspirationVisible(true);
            ui?.SetInspiration(_currentInspiration);
            ui?.SetPlusInspiration(0);

            _ctx.LoopsTimerUI?.ClearProgress();
            _ctx.LoopsTimerUI?.SetBarsVisible(false);

            _ctx.OnSessionStarted();
            _ctx.Log("[Session] Begin → BuildingCurrentPart", true);
        }

        public void End()
        {
            var songCtx = new SongFeedbackContext(_finishedParts);
            SongFinished?.Invoke(songCtx);

            _state = CompositionState.Ended;
            _isPlaying = false;
            _partCache.Clear();
            _loopHistoryByPart.Clear();
            _finishedParts.Clear();
            _keepInstrumentByPart.Clear();

            _ctx.LoopsTimerUI?.ClearProgress();
            _ctx.ShowHand(false);
            _ctx.ShowCompositionUI(false);
            _ctx.OnSessionEnded();

            _ctx.Log("[Session] End", true);
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
            _ctx.CompositionUI?.SetPlusInspiration(_perLoopInspirationCurrentPart);

            _state = CompositionState.BuildingNextPart;
            _currentInspiration = _rules.inspirationPerPart;
            PrepareDeck();
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
            bool requiresTarget = comp != null && comp.RequiresMusicianTarget;
            bool affectsSound = comp != null && CompositionCardClassifier.AffectsSound(comp);

            Info($"enter name='{def?.DisplayName}' zone={zone} state={_state} " +
                 $"isComp={isComp} isTrack={isTrack} isTempo={isTempo} " +
                 $"isTimeSig={isTimeSig} requiresTarget={requiresTarget}");

            // 1) Inspiration cost (only for composition cards)
            if (def.IsComposition)
            {
                int cost = Math.Max(0, def.InspirationCost);
                Info($"inspiration: have={_currentInspiration} " +
                    $"cost={cost} gen={def.InspirationGenerated}");
                if (cost > _currentInspiration)
                    return Fail("Not enough inspiration");
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

            // 4) Zone normalization (if current part is final, Next → Current)
            bool currentIsFinal = ui.IsPartFinal(_currentPartIndex);
            if (currentIsFinal && zone == CardDropZone.NextPart)
            {
                Info("current part is FINAL → redirecting zone NextPart → CurrentPart");
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

            Info($"routing: loopRunning={loopIsRunning} zone={zone} -> partIdx={partIdx} " +
                 $"(ui.CurrentPartIndex={ui.Model.CurrentPartIndex} " +
                 $"currentPartIndex={_currentPartIndex})");

            // 6) Apply to model
            if (!ui.ApplyCardToPart(card, target, partIdx))
                return Fail("ui.ApplyCardToPart returned false");

            // Apply CSO StatusActions immediately
            ApplyStatusActionsFromCard(def, target);

            // 7) Invalidate cache if the card affects sound
            if (loopIsRunning && affectsSound)
            {
                bool keepTempo = ShouldKeepTempo(def);

                bool keepInstruments = ShouldKeepInstruments(def);

                int invalidateIdx = (zone == CardDropZone.NextPart) ? 
                    partIdx : _currentPartIndex;

                Info($"invalidating cache part={invalidateIdx} keepTempo={keepTempo} " +
                    $"keepInstruments={keepInstruments} affectsSound={affectsSound}");

                InvalidatePartCache(invalidateIdx, keepTempo, keepInstruments);
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

                    ui.SetPlusInspiration(_perLoopInspirationCurrentPart);
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
            var statusActions = payload != null ? payload.StatusActions : null;
            if (statusActions == null || statusActions.Count == 0) return;

            // Resolve catalogue: prefer target, fallback to any band member.
            StatusEffectCatalogueSO catalogue =
                (target != null ? target.StatusCatalogue : null);

            if (catalogue == null && _ctx?.Band != null)
            {
                for (int i = 0; i < _ctx.Band.Count; i++)
                {
                    var m = _ctx.Band[i];
                    if (m != null && m.StatusCatalogue != null)
                    {
                        catalogue = m.StatusCatalogue;
                        break;
                    }
                }
            }

            if (catalogue == null)
            {
                _ctx?.Log($"[StatusActions][WARN] Card '{def?.DisplayName}' has StatusActions but no StatusEffectCatalogueSO found on target or band.", true);
                return;
            }

            for (int i = 0; i < statusActions.Count; i++)
            {
                var sa = statusActions[i];
                if (sa == null) continue;
                if (sa.StacksDelta == 0) continue;

                // Composition session is not coroutine-based here; ignore Delay for now (or log).
                // If you want delay later, we can queue them and execute in Tick().
                var effectId = sa.EffectId;

                StatusEffectSO statusDef;
                try { statusDef = catalogue.GetOrThrow(effectId); }
                catch (Exception e)
                {
                    _ctx?.Log($"[StatusActions][ERROR] Missing StatusEffectSO for id '{effectId}' in catalogue '{catalogue.name}'. Card='{def?.DisplayName}'. {e.Message}", true);
                    continue;
                }

                switch (sa.TargetType)
                {
                    case ActionTargetType.Musician:
                        if (target == null)
                        {
                            _ctx?.Log($"[StatusActions][WARN] '{def?.DisplayName}' targets Musician but target is null.", true);
                            break;
                        }
                        if (target.Statuses == null)
                        {
                            _ctx?.Log($"[StatusActions][WARN] Target '{target.name}' has no Statuses container.", true);
                            break;
                        }
                        target.Statuses.Apply(statusDef, sa.StacksDelta);
                        break;

                    case ActionTargetType.AllMusicians:
                        if (_ctx?.Band == null) break;
                        for (int m = 0; m < _ctx.Band.Count; m++)
                        {
                            var mus = _ctx.Band[m];
                            if (mus == null || mus.Statuses == null) continue;
                            mus.Statuses.Apply(statusDef, sa.StacksDelta);
                        }
                        break;

                    // Not supported in CompositionSession MVP:
                    case ActionTargetType.AudienceCharacter:
                    case ActionTargetType.AllAudienceCharacters:
                    case ActionTargetType.RandomAudienceCharacter:
                        _ctx?.Log($"[StatusActions][INFO] Ignoring audience-targeted StatusAction '{effectId}' in CompositionSession (MVP).", false);
                        break;

                    // RandomMusician etc could be supported if you want:
                    case ActionTargetType.RandomMusician:
                        if (_ctx?.Band == null || _ctx.Band.Count == 0) break;
                        var idx = _rng.Next(0, _ctx.Band.Count);
                        var rand = _ctx.Band[idx];
                        if (rand != null && rand.Statuses != null)
                            rand.Statuses.Apply(statusDef, sa.StacksDelta);
                        break;

                    default:
                        _ctx?.Log($"[StatusActions][WARN] Unhandled ActionTargetType '{sa.TargetType}' in CompositionSession.", true);
                        break;
                }
            }
        }


        private void PrepareDeck()
        {
            var gd = GameManager.Instance.GameplayData;
            var pool = gd.CompositionCardPool != null && gd.CompositionCardPool.Count > 0
                ? gd.CompositionCardPool
                : gd.AllCardsList.FindAll(c => c != null && c.IsComposition);

            var deck = _ctx.Deck;
            deck.ClearAll();
            deck.AddToDrawPile(pool);
            deck.DrawCards(gd.DrawCount);
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

            if (!_partCache.TryGetValue(partIndex, out var cache) 
                || cache?.mergedBytes == null || cache.mergedBytes.Length == 0)
            {
                int? bpmOverride = (cache != null && cache.resolvedBpm > 0) 
                    ? cache.resolvedBpm : (int?)null;

                var (merged, stems, seconds, bpmChosen, instByMus) =
                    mm.RenderSinglePart(cfg, partIndex, bpmOverride, 
                    cache?.resolvedMelInstByMusician);

                if (merged == null || merged.Length == 0 || seconds <= 0f) return 0f;

                if (cache == null) cache = new PartCache();
                cache.mergedBytes = merged;
                cache.seconds = seconds;
                cache.stemsByMusician = stems ?? new Dictionary<string, byte[]>();
                cache.resolvedBpm = bpmChosen;
                if (instByMus != null)
                    foreach (var kv in instByMus)
                        cache.resolvedMelInstByMusician[kv.Key] = kv.Value;

                _partCache[partIndex] = cache;
            }

            if (cache.resolvedBpm > 0)
            {
                _ctx?.OnPartBpmResolved(partIndex, cache.resolvedBpm);
            }

            var partName = _ctx.CompositionUI.Model.parts[partIndex].label;
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
                sum += Math.Max(0, t.inspirationGenerated);
            return sum;
        }

        private void HandleLoopFinished()
        {
            _isPlaying = false;

            _loopsRemainingForPart--;

            int inspirationGainedThisLoop = _perLoopInspirationCurrentPart;
            if (inspirationGainedThisLoop > 0)
            {
                _currentInspiration += inspirationGainedThisLoop;
                _ctx.CompositionUI?.SetInspiration(_currentInspiration);
                _ctx.CompositionUI?.SetPlusInspiration(inspirationGainedThisLoop);
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

            var ctx = new LoopFeedbackContext(
                partIndex: _currentPartIndex,
                loopIndexWithinPart: loopIndex0,
                loopsInPart: _loopsTotalForPart,
                partLabel: partLabel,
                inspirationGainedThisLoop: inspirationGainedThisLoop,
                inspirationAfterLoop: _currentInspiration,
                tracks: trackSnapshots
            );

            // Store in per-part history
            if (!_loopHistoryByPart.TryGetValue(_currentPartIndex, out var list))
            {
                list = new List<LoopFeedbackContext>();
                _loopHistoryByPart[_currentPartIndex] = list;
            }
            list.Add(ctx);

            LoopFinished?.Invoke(ctx);

            if (_loopsRemainingForPart > 0)
            {
                PlaySinglePartLoop(_currentPartIndex);
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
            ui.SetPlusInspiration(_perLoopInspirationCurrentPart);

            bool final = ui.IsPartFinal(_currentPartIndex);
            if (!final)
            {
                _state = CompositionState.BuildingNextPart;
                _currentInspiration = _rules.inspirationPerPart;
                PrepareDeck();
                ui.SetInspiration(_currentInspiration);
                ui.BeginDraftNextPart($"Part {_currentPartIndex + 2}");
            }
            else
            {
                _state = CompositionState.PlayingCurrentPart; // solo reproduce y termina
            }
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
                stemsByMusician = new Dictionary<string, byte[]>(),
                resolvedMelInstByMusician = keepInstruments
                    ? cache.resolvedMelInstByMusician
                    : new Dictionary<string, MIDIInstrumentSO>(),
                resolvedPercInstByMusician = new()
            };
        }
    }
}