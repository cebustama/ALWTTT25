using ALWTTT.Cards;
using ALWTTT.Characters;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Music;
using ALWTTT.UI;
using ALWTTT.Utils;

using MidiGenPlay;
using MidiGenPlay.Composition;
using MidiGenPlay.Interfaces;
using MidiGenPlay.Services;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Managers
{
    public class ShipInteriorManager : MonoBehaviour
    {
        private const string DebugTag = "<color=green>[Rehearsal]</color>";

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> musicianPosList;

        [Header("Jam Rules / Runtime")]
        [SerializeField] private JamRules jamRules = new JamRules();

        [Header("Cards / Hand")]
        [SerializeField] private HandController shipHand;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera handCamera;

        [Header("Composition")]
        [SerializeField] private SongCompositionUI compositionUI;
        // TODO: Access through SongCompositionUI? Or not?
        [SerializeField] private LoopsTimerUI loopsTimerUI;
        [SerializeField] private MidiGenPlayConfig midiGenPlayConfig;
        [SerializeField] private bool useProceduralRhythm = true;
        [SerializeField] private bool useProceduralBacking = true;

        [Header("Melody (Dev)")]
        [SerializeField] private MelodicLeadingConfig melodicConfig;
        [SerializeField] private MelodyStrategyId melodyStrategyId;
        [SerializeField] private HarmonicLeadingConfig harmonicConfig;
        [SerializeField] private HarmonyStrategyId harmonyStrategyId;

        [Header("Refs")]
        [SerializeField] private ShipInteriorCanvas shipCanvas;
        [SerializeField] private SceneChanger sceneChanger;

        [Header("Dev")]
        [SerializeField] private bool useLogs = false;
        [SerializeField] private bool composeUseLayeredEntrances = true;
        [SerializeField] private bool composeEnablePostProcessing = true;
        [SerializeField] private bool composeUsePersonalityBias = true;
        [SerializeField] private bool composeAddIntro = false;
        [SerializeField] private bool composeAddOutro = false;

        [SerializeField] private MidiMusicManager.HighlightMode defaultHighlightMode =
            MidiMusicManager.HighlightMode.DuckOthers;
        private string _nextHighlightMusicianId;


        private int buildingPartInspirationPerLoop = 0; // sum of GrooveGenerated while drafting part
        private int currentPartInspiration = 0;
        private int currentPartIndex = -1;
        private bool nextPartIsReady = false;


        private readonly List<MusicianBase> _spawned = new();
        public List<MusicianBase> SpawnedBand => _spawned;
        public SongCompositionUI.SongModel GetCurrentComposition() => compositionUI?.Model;

        #region Cache
        private GameManager GameManager => GameManager.Instance;
        private MidiMusicManager MidiMusicManager => MidiMusicManager.Instance;
        #endregion

        private IInstrumentRepository instrumentRepo;
        private IPatternRepository patternRepo;
        private System.Random rng = new System.Random();

        private void Log(string log, bool highlight = false, string customColor = "")
        {
            if (useLogs)
            {
                if (highlight) 
                    Debug.Log($"{DebugTag} <color=yellow>{log}</color>");
                else if (!string.IsNullOrWhiteSpace(customColor))
                    Debug.Log($"{DebugTag} <color={customColor}>{log}</color>");
                else 
                    Debug.Log($"{DebugTag} {log}");
            }
        }

        #region Composition Refactor
        private CompositionSession _session;

        private class ShipContext : ICompositionContext
        {
            private readonly ShipInteriorManager _host;
            public ShipContext(ShipInteriorManager host) { _host = host; }

            public SongCompositionUI CompositionUI => _host?.compositionUI;
            public LoopsTimerUI LoopsTimerUI => _host?.loopsTimerUI;
            public DeckManager Deck => DeckManager.Instance;
            public MidiMusicManager Music => MidiMusicManager.Instance;
            public IReadOnlyList<MusicianBase> Band => _host.SpawnedBand.AsReadOnly();

            public void ShowCompositionUI(bool v) => _host?.compositionUI?.gameObject.SetActive(v);
            public void ShowHand(bool v) => _host?.SetHandVisible(v);

            public MusicianBase ResolveMusicianByType(MusicianCharacterType t) => 
                _host.ResolveMusicianByType(t);

            public MusicianBase ResolveMusicianById(string id) =>
                _host.SpawnedBand.FirstOrDefault(m =>
                    m && m.MusicianCharacterData.CharacterId == id);

            public bool TryGetPartCache(int partIndex, out CompositionSession.PartCache cache)
            {
                cache = null;
                if (_host._session == null) return false;
                return _host._session.TryGetPartCache(partIndex, out cache);
            }

            public CompositionSession.PartCache GetOrCreatePartCache(int partIndex)
            {
                if (_host._session == null)
                {
                    return new CompositionSession.PartCache();
                }
                return _host._session.GetOrCreatePartCache(partIndex);
            }

            public void OnSessionStarted() { }
            public void OnSessionEnded() { }
            public void Log(string msg, bool hi = false) => _host.Log(msg, hi);
        }
        #endregion

        private void Start()
        {
            RebindDeckToShipHand();
            BuildBand();

            if (shipCanvas)
            {
                shipCanvas.Setup(onCompose: OnCompose, onRelax: OnRelax, onBandTalk: OnBandTalk);
                shipCanvas.HookPlayButton(OnPlayPressed);

                shipCanvas.HookMetronomeToggle(
                    OnMetronomeToggled,
                    MidiMusicManager != null && MidiMusicManager.MetronomeEnabled);

                // Populate dropdowns with spawned musicians
                var items = _spawned
                    .Select(m => (m.MusicianCharacterData.CharacterId,
                                  m.MusicianCharacterData.CharacterName))
                    .ToList();

                shipCanvas.PopulateHighlightDropdown(items, id => _nextHighlightMusicianId = id, true);
                shipCanvas.HookLayeredEntranceToggle(v => composeUseLayeredEntrances = v, composeUseLayeredEntrances);
                shipCanvas.HookEnablePPToggle(v => composeEnablePostProcessing = v, composeEnablePostProcessing);
                shipCanvas.HookEnablePersonalityToggle(v => composeUsePersonalityBias = v, composeUsePersonalityBias);

                // Tempo scale choices for the *next* song (they enqueue in MidiMusicManager)
                var tempoOptions = new List<(string label, float factor)>
                {
                    ("0.75× (laid-back)", 0.75f), ("1.00× (default)", 1.00f),
                    ("1.25×", 1.25f),             ("1.50×", 1.50f), ("2.00×", 2.00f)
                };
                shipCanvas.PopulateTempoScaleDropdown(tempoOptions,
                    factor => MidiMusicManager?.ScheduleNextSongTempoScale(factor), defaultIndex: 1);
            }

            SetHandVisible(false);
            SetCompositionVisible(false);

            // Services
            instrumentRepo = new InstrumentRepositoryResources(midiGenPlayConfig);
            patternRepo = new PatternRepositoryResources(midiGenPlayConfig);
            instrumentRepo.Refresh();
            patternRepo.Refresh();
        }

        private void Update()
        {
            _session?.Tick(Time.deltaTime);
        }

        private void PrepareRehearsalDeck()
        {
            var gd = GameManager.GameplayData;
            var pool = gd.CompositionCardPool != null && gd.CompositionCardPool.Count > 0
                ? gd.CompositionCardPool
                : gd.AllCardsList.FindAll(c => c != null && c.IsComposition);

            var deck = DeckManager.Instance;
            deck.ClearAll();
            deck.AddToDrawPile(pool);
            deck.DrawCards(gd.DrawCount);
        }

        private void BuildBand()
        {
            var pd = GameManager.PersistentGameplayData;
            _spawned.Clear();

            for (int i = 0; i < pd.MusicianList.Count; i++)
            {
                var prefab = pd.MusicianList[i];
                var parent = musicianPosList != null && musicianPosList.Count > i
                           ? musicianPosList[i]
                           : transform;

                var clone = Instantiate(prefab, parent);
                clone.BuildCharacter(); // same pattern as GigManager  :contentReference[oaicite:2]{index=2}

                var responder = clone.gameObject.GetComponent<MusicianMidiResponder>();
                if (responder == null) responder =
                        clone.gameObject.AddComponent<MusicianMidiResponder>();
                responder.Init(clone);

                _spawned.Add(clone);

                MidiMusicManager?.RegisterMusicianAnchor(
                    clone.MusicianCharacterData.CharacterId, clone.transform);
            }
        }

        private void OnCompose()
        {
            if (_session != null) _session.End();
            _session = new CompositionSession();
            var ctx = new ShipContext(this);
            _session.Begin(ctx, jamRules, midiGenPlayConfig, rng);
        }

        private void OnPlayPressed()
        {
            _session?.ConfirmCurrentPartAndStart();
        }

        private void BeginRehearsalSession()
        {
            SetHandVisible(true);
            SetCompositionVisible(true);

            // Clean any pending music intents from a previous session
            try
            {
                // TODO: implement in MidiMusicManager
                //MidiMusicManager?.ClearQueuedIntents(); 
            }
            catch { /* safe no-op if method doesn't exist yet */ }

            // Use composition pool from GameplayData and draw a fresh hand
            PrepareRehearsalDeck();

            if (compositionUI) compositionUI.ResetSession();
            compositionUI?.PopulateMusicianIcons(_spawned);
        }

        private void OnRelax()
        {
            _session?.End();
            _session = null;
            // TODO: restore stress to all band members
            ReturnToMap();
        }

        private void OnBandTalk()
        {
            _session?.End();
            _session = null;

            var pd = GameManager.PersistentGameplayData;
            var gd = GameManager.GameplayData;

            if (pd.BandConflicts != null && pd.BandConflicts.Count > 0)
            {
                // Remove all current conflicts
                pd.BandConflicts.Clear();
            }
            else
            {
                // Restore cohesion (clamped)
                pd.BandCohesion = Mathf.Clamp(
                    pd.BandCohesion + gd.CohesionRestoredByBandTalk,
                    0, gd.MaxCohesion);
            }

            ReturnToMap();
        }

        private void ReturnToMap()
        {
            // Mark node completed (consume the Rehearsal) – adjust if you want revisits
            var pd = GameManager.PersistentGameplayData;
            var state = pd.CurrentSectorMapState;
            if (state != null)
            {
                var node = state.GetNode(pd.LastMapNodeId);
                if (node != null) node.Completed = true;
            }

            sceneChanger.OpenMapScene();
        }

        private void OnMetronomeToggled(bool enabled)
        {
            MidiMusicManager?.SetMetronomeEnabled(enabled);
        }

        public CharacterBase GetSelectedMusicianOrDefault()
        {
            // TODO: Use raycast or other method
            if (!string.IsNullOrEmpty(_nextHighlightMusicianId))
            {
                var found = _spawned.FirstOrDefault(m =>
                    m.MusicianCharacterData.CharacterId == _nextHighlightMusicianId);
                if (found) return found;
            }
            return _spawned.Count > 0 ? _spawned[0] : null;
        }

        public bool TryPlayCompositionCard(CardBase card, MusicianBase target, CardDropZone zone)
        {
            return _session?.TryPlayCompositionCard(card, target, zone) ?? false;
        }

        private void SetHandVisible(bool visible)
        {
            if (shipHand != null)
            {
                shipHand.gameObject.SetActive(visible);
                if (visible) shipHand.EnableDragging();
                else shipHand.DisableDragging();
            }
        }

        private void RebindDeckToShipHand()
        {
            if (DeckManager.Instance != null && shipHand != null)
            {
                DeckManager.Instance.SetHandController(shipHand);
                shipHand.SetTargetResolver(ResolveMusicianByType);
            }
        }

        private void SetCompositionVisible(bool visible)
        {
            if (compositionUI != null)
                compositionUI.gameObject.SetActive(visible);
        }

        // TODO: Standarize, move to correct class, etc
        private TempoRange GetTempoRangeFromLabel(string label)
        {
            switch (label)
            {
                case "Slow": return TempoRange.Slow;
                case "Fast": return TempoRange.Fast;
                case "Very Fast": return TempoRange.VeryFast;
                default: return TempoRange.Moderate;
            }
        }

        private TimeSignature GetTimeSignatureFromLabel(string label)
        {
            switch (label)
            {
                case "4/4": return TimeSignature.FourFour;
                case "3/4": return TimeSignature.ThreeFour;
                case "6/8": return TimeSignature.SixEight;
                case "5/4": return TimeSignature.FiveFour;
                default: return TimeSignature.FourFour;
            }
        }

        private Tonality GetTonalityFromLabel(string label)
        {
            switch (label)
            {
                case "Ionian":      return Tonality.Ionian;
                case "Dorian":      return Tonality.Dorian;
                case "Phrygian":    return Tonality.Phrygian;
                case "Lydian":      return Tonality.Lydian;
                case "Mixolydian":  return Tonality.Mixolydian;
                case "Aeolian":     return Tonality.Aeolian;
                case "Locrian":     return Tonality.Locrian;
                default:            return Tonality.Ionian;
            }
        }

        private TrackRole GetRoleFromLabel(string label)
        {
            switch (label)
            {
                case "Rhythm": return TrackRole.Rhythm;
                case "Backing": return TrackRole.Backing;
                case "Bassline": return TrackRole.Bassline;
                case "Melody": return TrackRole.Melody;
                case "Harmony": return TrackRole.Harmony;
                default: return TrackRole.Melody;
            }
        }

        private void ResetAfterPlayback()
        {
            BeginRehearsalSession();
        }

        // Role-aware
        private IEnumerable<MIDIInstrumentSO> GetPermittedInstruments(
            MusicianBase musician, TrackRole role)
        {
            var allMelodic = instrumentRepo.GetMelodicInstruments();
            if (musician == null || musician.MusicianCharacterData == null) return allMelodic;

            var prof = musician.MusicianCharacterData.Profile;
            if (prof == null) return allMelodic;

            // Choose the primary list based on role, with a secondary fallback.
            List<InstrumentType> primary = null;
            List<InstrumentType> secondary = null;

            switch (role)
            {
                case TrackRole.Backing:
                case TrackRole.Bassline:   // many bands treat bass as part of the backline
                    primary = prof.backingInstruments;
                    secondary = prof.leadInstruments;
                    break;

                case TrackRole.Melody:
                case TrackRole.Harmony:
                    primary = prof.leadInstruments;
                    secondary = prof.backingInstruments;
                    break;

                default:
                    primary = prof.backingInstruments;
                    secondary = prof.leadInstruments;
                    break;
            }

            IEnumerable<MIDIInstrumentSO> FilterBy(List<InstrumentType> list) =>
                (list == null || list.Count == 0)
                    ? Enumerable.Empty<MIDIInstrumentSO>()
                    : allMelodic.Where(i => list.Contains(i.InstrumentType));

            var filtered = FilterBy(primary).ToList();
            if (filtered.Count == 0) filtered = FilterBy(secondary).ToList();

            return filtered.Count > 0 ? filtered : allMelodic;
        }

        private int EvaluatePerLoopInspirationGain(SongCompositionUI.PartEntry part)
        {
            if (part == null || part.tracks == null) return 0;
            // MVP rule: sum grooveGenerated for each active track in this part
            int sum = 0;
            foreach (var t in part.tracks)
                sum += Mathf.Max(0, t.inspirationGenerated);
            return sum;
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

        private MusicianBase ResolveMusicianByType(MusicianCharacterType t)
        {
            if (t == MusicianCharacterType.None || _spawned == null) return null;

            return _spawned.FirstOrDefault(m =>
                m?.MusicianCharacterData != null &&
                m.MusicianCharacterData.CharacterType == t);
        }

        private bool ComputeNextPartIsReady()
        {
            return compositionUI != null
                && compositionUI.HasPlayableNextPart(currentPartIndex);
        }
    }
}