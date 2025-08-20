using ALWTTT.Encounters;
using ALWTTT.UI;
using UnityEngine;

namespace ALWTTT.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Canvases")]
        [SerializeField] private GigCanvas gigCanvas;
        [SerializeField] private RewardCanvas rewardCanvas;

        #region Encapsulation

        public GigCanvas GigCanvas => gigCanvas;
        public RewardCanvas RewardCanvas => rewardCanvas;
        public GameManager GameManager => GameManager.Instance;
        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                transform.parent = null; // why?
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            GigCanvas.FillSongDropdown();
        }

        public void SetupEncounterUI(GigEncounter encounter)
        {
            Debug.Log($"<color=green>HERE {encounter.NumberOfSongs}</color>");
            GigCanvas.SetupSongIcons(encounter.NumberOfSongs);
            GigCanvas.SetCurrentSongIndex(0);
        }
    }
}