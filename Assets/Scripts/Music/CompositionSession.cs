using ALWTTT.Cards;
using ALWTTT.Cards.Effects;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
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
            _ctx.Music?.ResetStemCache(); // [B1 / D7=B] Per-song stem cache reset.
            _sessionMelodicPin.Clear();   // [B1 / #7.1 / D-F=γ]
            _sessionPercussionPin.Clear(); // [B1 / #7.1 / D-F=γ]
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

            _currentInspiration = ResolveSessionStartInspiration();
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
            _ctx.Music?.ResetStemCache(); // [B1 / D7=B] Per-song stem cache reset.
            _sessionMelodicPin.Clear();   // [B1 / #7.1 / D-F=γ]
            _sessionPercussionPin.Clear(); // [B1 / #7.1 / D-F=γ]
            _loopHistoryByPart.Clear();
            _finishedParts.Clear();
            _keepInstrumentByPart.Clear();

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

            Info($"routing: loopRunning={loopIsRunning} zone={zone} -> partIdx={partIdx} " +
                 $"(ui.CurrentPartIndex={ui.Model.CurrentPartIndex} " +
                 $"currentPartIndex={_currentPartIndex})");

            // 6) Apply to model
            if (!ui.ApplyCardToPart(card, target, partIdx))
                return Fail("ui.ApplyCardToPart returned false");

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

            if (!_partCache.TryGetValue(partIndex, out var cache)
                || cache?.mergedBytes == null || cache.mergedBytes.Length == 0)
            {
                int? bpmOverride = (cache != null && cache.resolvedBpm > 0)
                    ? cache.resolvedBpm : (int?)null;

                // [F-4] diagnostic — boundary-call shape dump immediately before
                // MidiMusicManager.RenderSinglePart. Tracks the IndexOutOfRange
                // crash inside MidiGenPlay.SongOrchestrator at >=4 loops/part.
                // Removed at F-4 closure.
                int loopIndex0 = _loopsTotalForPart - _loopsRemainingForPart;
                int tracksAtPart = (partIndex >= 0 && partIndex < cfg.Parts.Count
                    && cfg.Parts[partIndex]?.Tracks != null)
                    ? cfg.Parts[partIndex].Tracks.Count : -1;
                Debug.Log(
                    $"<color=lime>[F-4][CompSession]</color> RenderSinglePart call: " +
                    $"partIndex={partIndex} loop={loopIndex0 + 1}/{_loopsTotalForPart} " +
                    $"parts={cfg.Parts?.Count ?? -1} " +
                    $"channelOwners={cfg.ChannelMusicianOrder?.Count ?? -1} " +
                    $"channelRoles={cfg.ChannelRoles?.Count ?? -1} " +
                    $"tracksAtPart={tracksAtPart} " +
                    $"bpmOverride={bpmOverride?.ToString() ?? "null"} " +
                    $"cacheState={(cache == null ? "null" : "stale-or-empty")} " +
                    $"melCacheCount={cache?.resolvedMelInstByMusician?.Count ?? 0}");

                // [B1 / D-E=α'] Compute UI-stable input hashes per musician
                // for this part. Passed to RenderSinglePart so the stem cache
                // keys on player-controlled inputs (not the random instrument
                // resolution that happens inside FromUI).
                var trackInputsHashes =
                    Music.SongConfigBuilder.ComputeTrackInputsHashesForPart(
                        _ctx, partIndex);

                // [B1 / #7.1 / D-F=γ] Apply session-level instrument pins to
                // cfg before the render. Keeps the same musician's voice
                // consistent when only style/role-style changes. Respects
                // explicit instrument overrides from cards (skipped per-track
                // when UI TrackEntry has overrideMelodicInstrument /
                // overridePercussionInstrument set).
                ApplyInstrumentPins(cfg, partIndex);

                var (merged, stems, seconds, bpmChosen, instByMus) =
                    mm.RenderSinglePart(cfg, partIndex, bpmOverride,
                        cache?.resolvedMelInstByMusician,
                        trackInputsHashes);

                if (merged == null || merged.Length == 0 || seconds <= 0f) return 0f;

                // [B1 / #7.1 / D-F=γ] Record resolved instruments into the
                // session-level pin map for future renders to reuse.
                UpdateInstrumentPins(cfg, partIndex);

                // [B1 / #1+#2 / D-H4=α] Fresh render succeeded for this part —
                // clear pending visualization on its tracks.
                _ctx?.CompositionUI?.OnRenderCompleted(partIndex);

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

        private void HandleLoopFinished()
        {
            _isPlaying = false;

            _loopsRemainingForPart--;

            int inspirationGainedThisLoop = _perLoopInspirationCurrentPart;
            if (inspirationGainedThisLoop > 0)
            {
                AddCurrentInspiration(inspirationGainedThisLoop);
                // Badge shows un-clamped per-loop track contribution: the
                // player's signal of next-loop potential, independent of cap.
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
            ui.SetPlusInspiration(_perLoopInspirationCurrentPart);

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
                stemsByMusician = new Dictionary<string, byte[]>(),
                resolvedMelInstByMusician = keepInstruments
                    ? cache.resolvedMelInstByMusician
                    : new Dictionary<string, MIDIInstrumentSO>(),
                resolvedPercInstByMusician = new()
            };
        }

        // ─────────────────────────────────────────────────────────────
        // [B1 / #7.1 / D-F=γ.1] Instrument pin helpers — refined to key
        // by override state so the pin doesn't override a type-override card.
        // ─────────────────────────────────────────────────────────────
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
                var trModel = uiPart.tracks.Find(t => t.musicianId == tcfg.MusicianId);
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
                var trModel = uiPart.tracks.Find(t => t.musicianId == tcfg.MusicianId);
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