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

            var pd = GameManager.PersistentGameplayData;
            var mm = MidiMusicManager;

            // 1) Create the song for this run
            var newSong = pd.GenerateSong();
            if (newSong == null || mm == null) { isPlaying = false; yield break; }

            // Ensure full-band is generated once (cached)
            mm.GenerateSongs(new[] { newSong }); // full band only

            // 2) Layered preview: same arrangement, more channels each loop
            var bandOrder = _spawned.Where(m => m != null).ToList();
            if (bandOrder.Count == 0) { isPlaying = false; yield break; }

            float lastDuration = 0f;
            for (int k = 1; k <= bandOrder.Count; k++)
            {
                // Optional: visual cue when a new musician "joins"
                var justJoined = bandOrder[k - 1];
                if (justJoined?.CharacterAnimator != null)
                    justJoined.CharacterAnimator.JumpOnBeat = true;

                lastDuration = mm.PlaySameArrangementSubset(newSong, k);
                yield return new WaitForSeconds(lastDuration);
            }

            // 3) Show New Song panel; on Confirm => back to map
            var names = bandOrder.Select(b => b.MusicianCharacterData.CharacterName).ToArray();
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