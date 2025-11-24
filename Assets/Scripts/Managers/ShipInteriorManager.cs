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

        private readonly List<MusicianBase> _spawned = new();
        public List<MusicianBase> SpawnedBand => _spawned;
        public SongCompositionUI.SongModel GetCurrentComposition() => compositionUI?.Model;

        #region Cache
        private GameManager GameManager => GameManager.Instance;
        private MidiMusicManager MidiMusicManager => MidiMusicManager.Instance;
        #endregion

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

        #region Composition
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

            public void OnPartBpmResolved(int partIndex, int bpm)
            {
                // For now, nothing.
            }
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

            if (compositionUI)
                compositionUI.HookPlayButton(OnPlayPressed);

            SetHandVisible(false);
            SetCompositionVisible(false);
        }

        private void Update()
        {
            _session?.Tick(Time.deltaTime);
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

        private MusicianBase ResolveMusicianByType(MusicianCharacterType t)
        {
            if (t == MusicianCharacterType.None || _spawned == null) return null;

            return _spawned.FirstOrDefault(m =>
                m?.MusicianCharacterData != null &&
                m.MusicianCharacterData.CharacterType == t);
        }
    }
}