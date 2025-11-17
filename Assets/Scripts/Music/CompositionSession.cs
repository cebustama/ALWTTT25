using ALWTTT.Cards;
using ALWTTT.Characters.Band;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.UI;
using MidiGenPlay;
using MidiGenPlay.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Music
{
    public class CompositionSession
    {
        public enum State 
        {
            Idle,                   // Not in a jam session
            BuildingCurrentPart,    // Player is building the first (or current) part before any playback
            PlayingCurrentPart,     // A confirmed part is currently playing in loop
            BuildingNextPart,       // While the current part is looping, player is drafting the next part
            Ended                   // Jam is over
        }
        private State _state = State.Idle;

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

        // ----- Public API -----
        public void Begin(
            ICompositionContext ctx, JamRules rules, MidiGenPlayConfig settings, System.Random rng)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _rules = rules ?? new JamRules();
            _settings = settings ?? new MidiGenPlayConfig();
            _rng = rng ?? new System.Random();

            _state = State.BuildingCurrentPart;
            _currentPartIndex = -1;
            _isPlaying = false;
            _partCache.Clear();
            _buildingPartInspirationPerLoop = 0;

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
            _state = State.Ended;
            _isPlaying = false;
            _partCache.Clear();

            _ctx.LoopsTimerUI?.ClearProgress();
            _ctx.ShowHand(false);
            _ctx.ShowCompositionUI(false);
            _ctx.OnSessionEnded();

            _ctx.Log("[Session] End", true);
        }

        public void ConfirmCurrentPartAndStart()
        {
            if (_state != State.BuildingCurrentPart) return;

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

            _state = State.BuildingNextPart;
            _currentInspiration = _rules.inspirationPerPart;
            PrepareDeck();
            _ctx.CompositionUI?.SetInspiration(_currentInspiration);

            _ctx.CompositionUI?.BeginDraftNextPart("Part B");
            _ctx.Log("[Session] Now looping Part A and drafting Part B");
        }

        // Llamar en Update del host (opcional) para barra de progreso + fin de loop
        public void Tick(float dt)
        {
            if (_state != State.BuildingNextPart && _state != State.PlayingCurrentPart) return;
            var mm = _ctx.Music; if (mm == null) return;

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
            var ui = _ctx.CompositionUI; if (ui == null || card == null) return false;
            var c = card.CardData;

            // Coste inspiración (solo composición)
            if (c.IsComposition)
            {
                int cost = Math.Max(0, c.GrooveCost);
                if (cost > _currentInspiration) return false;
            }

            // Resolver target por tipo si hace falta
            bool isTrackCard = c.IsTrackCard;
            if (isTrackCard && target == null && c.HasFixedMusicianTarget)
                target = _ctx.ResolveMusicianByType(c.MusicianCharacterType);
            if (isTrackCard && target == null) return false;

            if (!ui.CanApply(card, target, out var reason))
            {
                _ctx.Log($"[Session] Cannot play card: {reason}");
                return false;
            }

            // Si la parte actual es FINAL, redirige NextPart → CurrentPart
            bool currentIsFinal = ui.IsPartFinal(_currentPartIndex);
            if (currentIsFinal && zone == CardDropZone.NextPart)
                zone = CardDropZone.CurrentPart;

            // Elegir índice de parte
            bool loopIsRunning = 
                _isPlaying 
                && (_state == State.BuildingNextPart || _state == State.PlayingCurrentPart);
            int partIdx;
            if (loopIsRunning)
                partIdx = (zone == CardDropZone.NextPart) ? ui.Model.CurrentPartIndex : _currentPartIndex;
            else
                partIdx = ui.Model.CurrentPartIndex;

            // Aplicar al modelo/UI
            if (!ui.ApplyCardToPart(card, target, partIdx)) return false;

            // Invalidate cache si afecta sonido
            if (loopIsRunning && c.AffectsSound)
            {
                bool keepTempo = ShouldKeepTempo(c);
                int invalidateIdx = 
                    (zone == CardDropZone.NextPart) ? partIdx : _currentPartIndex;
                InvalidatePartCache(invalidateIdx, keepTempo);
            }

            // Gastar/preview inspiración
            if (c.IsComposition)
            {
                int cost = Math.Max(0, c.GrooveCost);
                _currentInspiration = Math.Max(0, _currentInspiration - cost);
                ui.SetInspiration(_currentInspiration);

                int gen = Math.Max(0, c.GrooveGenerated);
                if (gen > 0) _buildingPartInspirationPerLoop += gen;
            }

            if (c.IsTrackCard && _state != State.BuildingCurrentPart)
            {
                if (_currentPartIndex >= 0 && _currentPartIndex < ui.Model.parts.Count)
                {
                    _perLoopInspirationCurrentPart = 
                        EvalPerLoopInsp(ui.Model.parts[_currentPartIndex]);
                    ui.SetPlusInspiration(_perLoopInspirationCurrentPart);
                }
            }

            return true;
        }

        // ----- Private methods -----
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
            var mm = _ctx.Music; if (mm == null) return 0f;
            var cfg = BuildSongConfigFromUI(); if (cfg == null) return 0f;
            if (partIndex < 0 || partIndex >= cfg.Parts.Count) return 0f;

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
                getPinnedMelOrNull: (partIdx, musId) =>
                    TryGetPinnedMelInst(partIdx, musId, out var inst) ? inst : null,
                getCachedBpm: GetCachedBpm,
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
            if (_perLoopInspirationCurrentPart > 0)
            {
                _currentInspiration += _perLoopInspirationCurrentPart;
                _ctx.CompositionUI?.SetInspiration(_currentInspiration);
                _ctx.CompositionUI?.SetPlusInspiration(_perLoopInspirationCurrentPart);
            }

            if (_loopsRemainingForPart > 0)
            {
                PlaySinglePartLoop(_currentPartIndex);
                return;
            }

            // ¿Hay siguiente parte jugable?
            if (ComputeNextPartIsReady())
            {
                AdvanceToNextPart();
                return;
            }

            End();
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
                _state = State.BuildingNextPart;
                _currentInspiration = _rules.inspirationPerPart;
                PrepareDeck();
                ui.SetInspiration(_currentInspiration);
                ui.BeginDraftNextPart($"Part {_currentPartIndex + 2}");
            }
            else
            {
                _state = State.PlayingCurrentPart; // solo reproduce y termina
            }
        }

        private bool ShouldKeepTempo(CardData c)
        {
            if (c == null) return true;
            if (c.IsTempoCard) return false;
            if (c.IsTimeSignatureCard) return true;
            if (c.IsTrackCard) return true;
            if (c.IsTonalityCard) return true;
            return true;
        }

        private void InvalidatePartCache(int partIndex, bool keepTempo)
        {
            if (!_partCache.TryGetValue(partIndex, out var cache) || cache == null) return;
            int preservedBpm = keepTempo ? cache.resolvedBpm : 0;

            _partCache[partIndex] = new PartCache
            {
                mergedBytes = null,
                seconds = 0f,
                resolvedBpm = preservedBpm,
                stemsByMusician = new Dictionary<string, byte[]>(),
                resolvedMelInstByMusician = keepTempo ? cache.resolvedMelInstByMusician : new(),
                resolvedPercInstByMusician = new()
            };
        }

        private bool TryGetPinnedMelInst(
            int partIndex, string musicianId, out MIDIInstrumentSO inst)
        {
            inst = null;
            return _partCache.TryGetValue(partIndex, out var pc)
                && pc != null
                && pc.resolvedMelInstByMusician.TryGetValue(musicianId, out inst)
                && inst != null;
        }

        private int? GetCachedBpm(int partIndex)
        {
            return _partCache
                .TryGetValue(partIndex, out var pc) && pc != null && pc.resolvedBpm > 0
                ? pc.resolvedBpm
                : (int?)null;
        }
    }
}