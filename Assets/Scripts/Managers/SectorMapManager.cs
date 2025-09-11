using UnityEngine;
using TMPro;
using ALWTTT.Data;
using ALWTTT.Generation;
using ALWTTT.Map;

namespace ALWTTT.Managers
{
    public class SectorMapManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SectorMapData sectorMapData;

        [Header("Visual Controller")]
        [SerializeField] private SectorMapVisual mapVisual;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI fansText;
        [SerializeField] private TextMeshProUGUI cohesionText;

        private GameManager GM => GameManager.Instance;

        private SectorMapState State
        {
            get => GM.PersistentGameplayData.CurrentSectorMapState;
            set => GM.PersistentGameplayData.CurrentSectorMapState = value;
        }

        private void Start()
        {
            EnsureState();
            mapVisual.Render(State);
            RefreshHUD();
        }

        private void Update()
        {
            // Quick iteration: regenerate with R
            if (Input.GetKeyDown(KeyCode.R))
                Regenerate();

            // If something in state changes at runtime (e.g., moving current node),
            // this keeps visuals (visited/selected) in sync:
            mapVisual.SyncNodeStates();
        }

        private void EnsureState()
        {
            int sectorId = GM.PersistentGameplayData.CurrentSectorId;
            if (State == null || State.SectorId != sectorId)
            {
                var gen = new SectorGraphGenerator();
                State = gen.Generate(sectorId, sectorMapData, null);
            }
        }

        [ContextMenu("Regenerate")]
        public void Regenerate()
        {
            var gen = new SectorGraphGenerator();
            State = gen.Generate(GM.PersistentGameplayData.CurrentSectorId, sectorMapData, null);

            mapVisual.Render(State);
            RefreshHUD();
        }

        private void RefreshHUD()
        {
            var pd = GM.PersistentGameplayData;
            if (fansText) fansText.text = $"Fans: {pd.Fans}";
            if (cohesionText) cohesionText.text = $"Cohesion: {pd.BandCohesion}";
        }
    }
}
