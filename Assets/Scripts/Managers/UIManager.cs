using ALWTTT.Encounters;
using ALWTTT.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ALWTTT.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Canvases")]
        [SerializeField] private GigCanvas gigCanvas;
        [SerializeField] private RewardCanvas rewardCanvas;
        [SerializeField] private InventoryCanvas inventoryCanvas;

        [Header("Fader")]
        [SerializeField] private CanvasGroup fader;
        [SerializeField] private float fadeSpeed = 1f;

        #region Encapsulation

        public GigCanvas GigCanvas => gigCanvas;
        public RewardCanvas RewardCanvas => rewardCanvas;
        public InventoryCanvas InventoryCanvas => inventoryCanvas;
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

        public void SetCanvas(CanvasBase targetCanvas, bool open, bool reset = false)
        {
            if (reset)
                targetCanvas.ResetCanvas();

            if (open)
                targetCanvas.OpenCanvas();
            else
                targetCanvas.CloseCanvas();
        }

        public void ChangeScene(int index)
        {
            StartCoroutine(ChangeSceneRoutine(index));
        }

        public void OpenInventory(List<CardData> cardList, string title)
        {
            SetCanvas(InventoryCanvas, true, true);
            InventoryCanvas.ChangeTitle(title);
            InventoryCanvas.SetCards(cardList);
        }

        private IEnumerator ChangeSceneRoutine(int index)
        {
            SceneManager.LoadScene(index);
            yield return StartCoroutine(Fade(false));
        }

        public IEnumerator Fade(bool isIn)
        {
            var waitFrame = new WaitForEndOfFrame();
            var timer = isIn ? 0f : 1f;

            while (true)
            {
                timer += Time.deltaTime * (isIn ? fadeSpeed : -fadeSpeed);

                fader.alpha = timer;

                if (timer >= 1f) break;

                yield return waitFrame;
            }
        }
    }
}