using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Managers;
using ALWTTT.Utils;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class GigCanvas : CanvasBase
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI drawPileTextField;
        [SerializeField] private TextMeshProUGUI discardPileTextField;
        [SerializeField] private TextMeshProUGUI exhaustPileTextField;

        [Header("Buttons")]
        [SerializeField] private Button lossConfirmButton;

        [Header("UI Sections")]
        [SerializeField] private GameObject bandTurnUI;
        [SerializeField] private GameObject songPerfomanceUI;

        [Header("Panels")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject lossPanelRoot;
        [SerializeField] private TextMeshProUGUI lossTitle;
        [SerializeField] private TextMeshProUGUI lossBody;

        [Header("Song Hype")]
        [SerializeField] private GameObject songHypeRoot;
        [SerializeField] private Image songHypeImage;
        [SerializeField] private TextMeshProUGUI songHypeLabel; // % text

        public System.Action OnLossConfirm; // set by GigManager

        [Header("References")]
        [SerializeField] private SceneChanger sceneChanger;

        public GameObject WinPanel => winPanel;
        public GameObject LosePanel => lossPanelRoot;

        private void OnEnable()
        {
            lossConfirmButton.onClick.AddListener(OnClick_LossConfirm);

            GigManager.OnPlayerTurnStarted += ShowBandTurnUI;
            GigManager.OnSongPerformanceStarted += ShowSongPerformanceUI;
            GigManager.OnEnemyTurnStarted += ShowAudienceTurnUI;
        }

        private void OnDisable()
        {
            GigManager.OnPlayerTurnStarted -= ShowBandTurnUI;
            GigManager.OnSongPerformanceStarted -= ShowSongPerformanceUI;
            GigManager.OnEnemyTurnStarted -= ShowAudienceTurnUI;
        }

        public void SetPileTexts()
        {
            drawPileTextField.text = $"{DeckManager.DrawPile.Count.ToString()}";
            discardPileTextField.text = $"{DeckManager.DiscardPile.Count.ToString()}";
            exhaustPileTextField.text = $"{DeckManager.ExhaustPile.Count.ToString()}";
        }

        public void ShowLoss(string title, string body)
        {
            lossTitle.text = title;
            lossBody.text = body;
            lossPanelRoot.SetActive(true);
        }

        public void OnClick_LossConfirm()
        {
            lossPanelRoot.SetActive(false);
            OnLossConfirm?.Invoke();
        }

        public void ShowBandTurnUI()
        {
            bandTurnUI.SetActive(true);
            songPerfomanceUI.SetActive(false);
        }

        public void ShowSongPerformanceUI()
        {
            bandTurnUI.SetActive(false);
            songPerfomanceUI.SetActive(true);
        }

        public void ShowAudienceTurnUI()
        {
            bandTurnUI.SetActive(false);
            songPerfomanceUI.SetActive(false);
        }

        public void SetSongHype(float hype01)
        {
            float t = Mathf.Clamp01(hype01);

            if (songHypeRoot != null)
                songHypeRoot.SetActive(true);

            if (songHypeImage != null)
                songHypeImage.fillAmount = t;

            if (songHypeLabel != null)
                songHypeLabel.text = $"{Mathf.RoundToInt(t * 100f)}%";
        }

        public void ClearSongHype()
        {
            SetSongHype(0f);
        }
    }
}