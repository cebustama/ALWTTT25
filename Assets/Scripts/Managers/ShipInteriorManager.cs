using ALWTTT.Characters.Band;
using ALWTTT.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALWTTT.Managers
{
    public class ShipInteriorManager : MonoBehaviour
    {
        [Header("Spawn Points")]
        [SerializeField] private List<Transform> musicianPosList;

        [Header("Refs")]
        [SerializeField] private ShipInteriorCanvas shipCanvas;
        [SerializeField] private SceneChanger sceneChanger;

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
                shipCanvas.Setup(
                    onCompose: OnCompose,
                    onRelax: OnRelax,
                    onBandTalk: OnBandTalk
                );
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
                _spawned.Add(clone);
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

            var newSong = pd.GenerateSong();
            if (newSong == null || mm == null) { isPlaying = false; yield break; }

            // Compute entrance order by the channel -> musician map (so rhythm joins first, etc.)
            var fullKey = typeof(MidiMusicManager)
                            .GetMethod("CacheKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .Invoke(mm, new object[] { newSong, pd.MusicianList }) as string; // or expose a public getter if you prefer

            // Better: add a public helper on MidiMusicManager
            var entranceOrderIds = mm.GetChannelOwnerIdsFor(newSong); // see helper below

            // Build entrance order from channel owners (so rhythm/lead/backing join in the right order)
            var entranceIds = mm.GetChannelOwnerIdsFor(newSong);
            var byId = _spawned.ToDictionary(m => m.MusicianCharacterData.CharacterId, m => m);
            var orderedMusicians = entranceIds.Select(id => byId.TryGetValue(id, out var m) ? m : null)
                                              .Where(m => m != null)
                                              .ToList();

            float lastDuration = 0f;
            for (int k = 1; k <= orderedMusicians.Count; k++)
            {
                var newcomer = orderedMusicians[k - 1];
                //newcomer?.CharacterAnimator?.SetTrigger("Join");

                // Play subset by those musicians (not by channel index!)
                lastDuration = mm.PlaySameArrangementSubsetByMusicians(newSong, entranceIds, k);

                // Live overlay texts for this loop (optional, powerful)
                StartCoroutine(mm.DebugOverlayNotesForLoop(
                    newSong, entranceIds, k,
                    anchorById: _spawned.ToDictionary(m => m.MusicianCharacterData.CharacterId, m => m.transform)));

                yield return MidiMusicManager.WaitForEnd();
            }

            // 3) Show New Song panel; on Confirm => back to map
            var names = orderedMusicians.Select(b => b.MusicianCharacterData.CharacterName).ToArray();
            shipCanvas.NewSongPanel?.Show(newSong, names, lastDuration, onClose: () =>
            {
                ReturnToMap(); // <-- per “one activity per Rehearsal”
            });

            isPlaying = false;
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
    }
}