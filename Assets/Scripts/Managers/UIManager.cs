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
    }
}