using ALWTTT.Characters.Band;
using ALWTTT.UI;
using ALWTTT.Utils;
using System.Collections.Generic;
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

        private readonly List<MusicianBase> _spawned = new();
        private GameManager GM => GameManager.Instance;

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
            var pd = GM.PersistentGameplayData;
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

        // --- Button callbacks (stub actions for now) ---
        private void OnCompose()
        {
            var pd = GM.PersistentGameplayData;

            // Make a new song for this run (random from PossibleSongList)
            var newSong = pd.GenerateSong();

            // Pre-generate/cache the MIDI for the *current band roster*
            if (newSong != null && MidiMusicManager.Instance != null)
            {
                MidiMusicManager.Instance.GenerateSongs(new[] { newSong });
            }

            // Back to map
            ReturnToMap();
        }

        private void OnRelax()
        {
            // TODO: restore stress to all band members
            ReturnToMap();
        }

        private void OnBandTalk()
        {
            var pd = GM.PersistentGameplayData;
            var gd = GM.GameplayData;

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
            var pd = GM.PersistentGameplayData;
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