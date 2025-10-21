using ALWTTT.Characters;
using ALWTTT.Characters.Band;
using ALWTTT.Music;
using ALWTTT.Utils;
using MidiGenPlay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ALWTTT.CardData;
using static MidiGenPlay.IntroMutator;
using static MidiGenPlay.SoloMutator;

namespace ALWTTT.Managers
{
    public class ShipInteriorManager : MonoBehaviour
    {
        private const string DebugTag = "<color=green>[Rehearsal]</color>";

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> musicianPosList;

        [Header("Cards / Hand")]
        [SerializeField] private HandController shipHand;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Camera handCamera;

        [Header("Refs")]
        [SerializeField] private ShipInteriorCanvas shipCanvas;
        [SerializeField] private SceneChanger sceneChanger;

        [Header("Compose (Dev)")]
        [SerializeField] private bool composeUseLayeredEntrances = true;
        [SerializeField] private bool composeEnablePostProcessing = true;
        [SerializeField] private bool composeUsePersonalityBias = true;
        [SerializeField] private bool composeAddIntro = false;
        [SerializeField] private bool composeAddOutro = false;

        [SerializeField] private MidiMusicManager.HighlightMode defaultHighlightMode =
            MidiMusicManager.HighlightMode.DuckOthers;
        private string _nextHighlightMusicianId;

        // TODO: Intro/Outro type
        private string _nextIntroMusicianId;
        private string _nextOutroMusicianId;

        // TODO: Solo type
        private string _nextSoloMusicianId;

        private string _nextAltTrackMusicianId;
        private string _nextAltStrategyId;

        private bool isPlaying;

        private readonly List<MusicianBase> _spawned = new();

        #region Cache
        private GameManager GameManager => GameManager.Instance;
        private MidiMusicManager MidiMusicManager => MidiMusicManager.Instance;
        #endregion

        private void Start()
        {
            if (DeckManager.Instance != null && shipHand != null)
                DeckManager.Instance.SetHandController(shipHand);

            BuildBand();

            if (shipCanvas)
            {
                shipCanvas.Setup(onCompose: OnCompose, onRelax: OnRelax, onBandTalk: OnBandTalk);

                shipCanvas.HookMetronomeToggle(
                    OnMetronomeToggled, 
                    MidiMusicManager != null && MidiMusicManager.MetronomeEnabled);

                // populate dropdown with the spawned musicians
                var items = _spawned
                    .Select(m => (m.MusicianCharacterData.CharacterId,
                              m.MusicianCharacterData.CharacterName))
                    .ToList();
                
                // DEBUGGING
                shipCanvas.PopulateHighlightDropdown(items, id => _nextHighlightMusicianId = id, true);

                shipCanvas.PopulateIntroDropdown(
                    items,
                    id =>
                    {
                        _nextIntroMusicianId = id;
                        composeAddIntro = !string.IsNullOrEmpty(id); // NONE → false
                    },
                    includeNoneOption: true
                );

                shipCanvas.PopulateOutroDropdown(
                    items,
                    id =>
                    {
                        _nextOutroMusicianId = id;
                        composeAddOutro = !string.IsNullOrEmpty(id); // NONE → false
                    },
                    includeNoneOption: true
                );

                shipCanvas.PopulateSoloDropdown(
                    items,
                    id => _nextSoloMusicianId = id, // null => NONE
                    includeNoneOption: true
                );

                shipCanvas.HookLayeredEntranceToggle(
                    v => composeUseLayeredEntrances = v,
                    composeUseLayeredEntrances);

                shipCanvas.HookEnablePPToggle(
                    v => composeEnablePostProcessing = v,
                    composeEnablePostProcessing);

                shipCanvas.HookEnablePersonalityToggle(
                    v => composeUsePersonalityBias = v,
                    composeUsePersonalityBias);

                // Tempo scale options for NEXT song (Phase 1: just queued, not applied yet)
                var tempoOptions = new List<(string label, float factor)>
                {
                    ("0.75× (laid-back)", 0.75f),
                    ("1.00× (default)",   1.00f),
                    ("1.25×",             1.25f),
                    ("1.50×",             1.50f),
                    ("2.00×",             2.00f),
                };

                shipCanvas.PopulateTempoScaleDropdown(
                    tempoOptions,
                    factor => MidiMusicManager?.ScheduleNextSongTempoScale(factor),
                    defaultIndex: 1); // 1.00×

                // Alt Track musician dropdown (NONE → no alternate)
                shipCanvas.PopulateAlternateTrackDropdown(
                    items,
                    id => _nextAltTrackMusicianId = id,
                    includeNoneOption: true);

                // Alt Strategy dropdown (NONE → no alternate)
                var strategies = new List<(string id, string label)>
                {
                    ("busier",  "Busier"),
                    ("sparser", "Sparser"),
                    ("rotate1", "Rotate 1 beat"),
                    ("rotate2", "Rotate 2 beats"),
                    // add more later if you want (e.g., "rotate3")
                };

                shipCanvas.PopulateAlternateStrategyDropdown(
                    strategies,
                    id => _nextAltStrategyId = id,
                    includeNoneOption: true);
            }

            SetHandVisible(false);
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
            if (isPlaying) return;

            SetHandVisible(true);
            PrepareRehearsalDeck();
            StartCoroutine(ComposeAndPreviewRoutine());
        }

        private IEnumerator ComposeAndPreviewRoutine()
        {
            isPlaying = true;

            var mm = MidiMusicManager.Instance;
            var pd = GameManager.PersistentGameplayData;

            Debug.Log($"{DebugTag} ComposeAndPreview: begin");

            if (mm == null)
            {
                Debug.LogError($"{DebugTag} MidiMusicManager is null");
                isPlaying = false; yield break;
            }

            // 1) Generate song
            var newSong = pd.GenerateSong();
            if (newSong == null)
            {
                Debug.LogError($"{DebugTag} GenerateSong returned null");
                isPlaying = false; yield break;
            }
            Debug.Log($"{DebugTag} Obtained '{newSong.SongTitle}' song data from pool.");

            var personalityMap = new Dictionary<string, IMusicianPersonality>();
            foreach (var m in _spawned)
            {
                var id = m.MusicianCharacterData.CharacterId;
                // TODO: Different personality per musician
                personalityMap[id] = new NeutralPersonality(id);
            }
            MidiMusicManager.SetMusicianPersonalities(personalityMap);

            if (composeAddIntro && !string.IsNullOrEmpty(_nextIntroMusicianId))
            {
                MidiMusicManager.AddIntro(
                    musicianId: _nextIntroMusicianId,
                    measures: 1,
                    style: IntroStyle.CountIn);
            }

            if (composeAddOutro && !string.IsNullOrEmpty(_nextOutroMusicianId))
            {
                MidiMusicManager.AddOutro(
                    musicianId: _nextOutroMusicianId,
                    measures: 2,                 // tweak if desired
                    style: IntroStyle.Pad);      // reuse styles; Pad is a good default for outro
            }

            // Optional SOLO part: only if a musician was picked (dropdown != NONE)
            if (!string.IsNullOrEmpty(_nextSoloMusicianId))
            {
                MidiMusicManager.AppendSoloPart(
                    musicianId: _nextSoloMusicianId,
                    style: SoloStyle.Virtuoso, // default; adjust later if you add a style UI
                    measures: 8); // default; tweak as you prefer (8/12/16)
            }

            if (!string.IsNullOrEmpty(_nextAltTrackMusicianId) &&
                !string.IsNullOrEmpty(_nextAltStrategyId))
            {
                MidiMusicManager.ReplaceTrack(
                    partIndexOrAll: -1, // apply to all parts for MVP; add a scope control later if desired
                    musicianId: _nextAltTrackMusicianId,
                    new MidiMusicManager.StrategyOverride(_nextAltStrategyId));

                Debug.Log($"{DebugTag} AlternateTrack queued: musician={_nextAltTrackMusicianId}, strategy={_nextAltStrategyId}");
            }

            // 2) Channel owners for this arrangement (index=channel → musicianId)
            var owners = mm.GetChannelOwnerIdsFor(newSong);
            if (owners == null || owners.Count == 0)
            {
                Debug.LogError($"{DebugTag} GetChannelOwnerIdsFor returned empty.");
                isPlaying = false; yield break;
            }

            Debug.Log($"{DebugTag} Channel owners (ch->id): " +
                string.Join(", ", owners.Select((id, i) => $"{i}:{id}")));


            // 3) Wire live routing (so realtime events reach the right musician)
            mm.SetChannelOwners(owners?.ToList());
            Debug.Log($"{DebugTag} SetChannelOwners OK");

            // 4) Build visual entrance order => band objects by id
            var byId = _spawned.ToDictionary(m => m.MusicianCharacterData.CharacterId, m => m);
            var orderedMusicians = owners
                .Select(id => byId.TryGetValue(id, out var m) ? m : null)
                .Where(m => m != null)
                .ToList();

            if (orderedMusicians.Count == 0)
            {
                Debug.LogError($"{DebugTag} No spawned musician matched channel owners.");
                isPlaying = false; yield break;
            }

            var entranceIdsOrdered = orderedMusicians
                .Select(m => m.MusicianCharacterData.CharacterId)
                .ToList();

            var names = string.Join(", ",
                orderedMusicians.Select(m => m.MusicianCharacterData.CharacterName));
            Debug.Log($"{DebugTag} Ordered musicians: {names}");

            // 5) Layered preview
            float lastDuration = 0f;
            if (composeUseLayeredEntrances)
            {
                for (int k = 1; k <= orderedMusicians.Count; k++)
                {
                    var newcomer = orderedMusicians[k - 1];
                    Debug.Log($"{DebugTag} Loop {k}/{orderedMusicians.Count} " +
                        $"newcomer='{newcomer.MusicianCharacterData.CharacterName}'");

                    // Play subset by first k musicians in 'owners'
                    lastDuration =
                        mm.PlaySameArrangementSubsetByMusicians(
                            newSong, entranceIdsOrdered, k);

                    // Wait for actual end
                    yield return MidiMusicManager.WaitForEnd();
                    Debug.Log($"{DebugTag} Loop {k} ended.");
                }
            }

            // queue highlight if selected
            if (!string.IsNullOrEmpty(_nextHighlightMusicianId))
                mm.Highlight(_nextHighlightMusicianId, defaultHighlightMode);

            // Afinal full-band pass with seams flags ON
            mm.SetPostProcessingEnabled(composeEnablePostProcessing);

            // queue humanization for *this* song generation
            if (composeEnablePostProcessing)
            {
                mm.EnableHumanization(new MidiMusicManager.HumanizeOptions
                {
                    maxTickOffset = 6,   // ≈ six ticks timing jitter
                    velocityJitter = 12,  // ≈ ±12 velocity
                    lengthJitter = 6    // ≈ six ticks note-length jitter
                });
            }

            mm.SetPersonalityBiasEnabled(composeUsePersonalityBias);

            // Play the exact same SongData with NO subset (whole band)
            lastDuration = mm.Play(newSong);
            yield return MidiMusicManager.WaitForEnd();
            Debug.Log($"{DebugTag} Full-band pass ended.");

            // 6) New Song panel
            var newNames = orderedMusicians.Select(b => b.MusicianCharacterData.CharacterName)
                .ToArray();
            Debug.Log($"{DebugTag} Showing NewSongPanel for '{newSong.SongTitle}'");
            shipCanvas.NewSongPanel?.Show(newSong, newNames, lastDuration, onClose: () =>
            {
                Debug.Log($"{DebugTag} NewSongPanel closed, returning to map.");
                SetHandVisible(false);
                ReturnToMap();
            });

            isPlaying = false;
            Debug.Log($"{DebugTag} ComposeAndPreview: end");
        }

        private void OnRelax()
        {
            SetHandVisible(false);

            // TODO: restore stress to all band members
            ReturnToMap();
        }

        private void OnBandTalk()
        {
            SetHandVisible(false);

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

        public bool TryPlayCompositionCard(CardBase card, MusicianBase target)
        {
            var mm = MidiMusicManager.Instance;
            if (mm == null || card == null) return false;

            var c = card.CardData;
            var musicianId = target != null
                ? target.MusicianCharacterData.CharacterId
                : _nextHighlightMusicianId;

            switch (c.CompositionType)
            {
                // TEMPO
                case CompositionCardType.Tempo_Slow:
                    mm.ScheduleNextSongTempoScale(0.75f); 
                    return true;
                case CompositionCardType.Tempo_Fast:
                    mm.ScheduleNextSongTempoScale(1.25f); 
                    return true;
                case CompositionCardType.Tempo_VeryFast: 
                    mm.ScheduleNextSongTempoScale(1.50f); 
                    return true;

                // THEME
                case CompositionCardType.Theme_Love: 
                    GameManager.PersistentGameplayData.SetNextThemeTag("Love"); 
                    return true;
                case CompositionCardType.Theme_Injustice: 
                    GameManager.PersistentGameplayData.SetNextThemeTag("Injustice"); 
                    return true;
                case CompositionCardType.Theme_Party: 
                    GameManager.PersistentGameplayData.SetNextThemeTag("Party"); 
                    return true;

                // PARTS
                case CompositionCardType.Part_Intro:
                    if (!string.IsNullOrEmpty(musicianId)) 
                    { 
                        mm.AddIntro(musicianId, 1, IntroMutator.IntroStyle.CountIn); 
                        return true; 
                    }
                    return false;
                case CompositionCardType.Part_Solo:
                    if (!string.IsNullOrEmpty(musicianId)) 
                    { 
                        mm.AppendSoloPart(musicianId, SoloMutator.SoloStyle.Virtuoso, 8); 
                        return true; 
                    }
                    return false;
                case CompositionCardType.Part_Outro:
                    if (!string.IsNullOrEmpty(musicianId)) 
                    { 
                        mm.AddOutro(musicianId, 2, IntroMutator.IntroStyle.Pad); 
                        return true; 
                    }
                    return false;

                // TRACKS
                case CompositionCardType.Track_Rhythm:
                case CompositionCardType.Track_Backing:
                case CompositionCardType.Track_Bassline:
                case CompositionCardType.Track_Melody:
                case CompositionCardType.Track_Harmony:
                    if (!string.IsNullOrEmpty(musicianId))
                    {
                        mm.ReplaceTrack(-1, musicianId, new MidiMusicManager.StrategyOverride("busier")); // o “sparser/rotate1/rotate2”
                        return true;
                    }
                    return false;

                // Time Signature
                case CompositionCardType.TimeSignature_4_4:
                case CompositionCardType.TimeSignature_3_4:
                case CompositionCardType.TimeSignature_6_8:
                case CompositionCardType.TimeSignature_5_4:
                    // TODO next sprint: mm.SetPartTimeSignature(...)
                    Debug.LogWarning("[Rehearsal] TimeSignature cards pending implementation.");
                    return false;

                default:
                    return false;
            }
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
    }
}