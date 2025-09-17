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
            // TODO: create & add song to pd.CurrentSongList
            ReturnToMap();
        }

        private void OnRelax()
        {
            // TODO: restore stress to all band members
            ReturnToMap();
        }

        private void OnBandTalk()
        {
            // TODO: resolve conflicts OR restore cohesion if no conflicts
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