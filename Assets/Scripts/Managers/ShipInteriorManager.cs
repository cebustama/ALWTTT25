using ALWTTT.Characters.Band;
using ALWTTT.Music;
using ALWTTT.Utils;
using MidiGenPlay;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALWTTT.Managers
{
    public class ShipInteriorManager : MonoBehaviour
    {
        private const string DebugTag = "<color=green>[Rehearsal]</color>";

        [Header("Spawn Points")]
        [SerializeField] private List<Transform> musicianPosList;

        [Header("Refs")]
        [SerializeField] private ShipInteriorCanvas shipCanvas;
        [SerializeField] private SceneChanger sceneChanger;

        [Header("Compose (Dev)")]
        [SerializeField] private bool composeUseLayeredEntrances = true;
        [SerializeField] private bool composeEnablePostProcessing = true;
        [SerializeField] private bool composeUsePersonalityBias = true;

        [SerializeField] private MidiMusicManager.HighlightMode defaultHighlightMode =
            MidiMusicManager.HighlightMode.DuckOthers;
        private string _nextHighlightMusicianId;

        private bool isPlaying;

        private readonly List<MusicianBase> _spawned = new();

        #region Cache
        private GameManager GameManager => GameManager.Instance;
        private MidiMusicManager MidiMusicManager => MidiMusicManager.Instance;
        #endregion

        private void Start()
        {
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
                shipCanvas.PopulateHighlightDropdown(items, id => _nextHighlightMusicianId = id, true);

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
            }
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
                ReturnToMap();
            });

            isPlaying = false;
            Debug.Log($"{DebugTag} ComposeAndPreview: end");
        }

        private void OnRelax()
        {
            // TODO: restore stress to all band members
            ReturnToMap();
        }

        private void OnBandTalk()
        {
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
    }
}