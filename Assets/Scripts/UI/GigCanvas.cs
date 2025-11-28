using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Managers;
using ALWTTT.Utils;
using System.Collections;
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

        [Header("Song Hype Visuals")]
        [SerializeField] private float hypeLerpDefaultDuration = 1f;
        [SerializeField] private bool enableHypeWobble = true;
        [SerializeField] private float hypeWobbleAmplitude = 0.02f; // 2%
        [SerializeField] private float hypeWobbleSpeed = 2f;        // wobble cycles / second
        [SerializeField] private Color hypeLowColor = Color.red;
        [SerializeField] private Color hypeMidColor = Color.yellow;
        [SerializeField] private Color hypeHighColor = Color.green;

        private float _baseHype01;
        private Coroutine _hypeLerpRoutine;
        private bool _songHypeVisible = false;

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
            GigManager.OnSongHypeChanged01 += SetSongHype;

            if (songHypeRoot != null)
                songHypeRoot.SetActive(false);
            _songHypeVisible = false;
        }

        private void OnDisable()
        {
            lossConfirmButton.onClick.RemoveListener(OnClick_LossConfirm);

            GigManager.OnPlayerTurnStarted -= ShowBandTurnUI;
            GigManager.OnSongPerformanceStarted -= ShowSongPerformanceUI;
            GigManager.OnEnemyTurnStarted -= ShowAudienceTurnUI;
            GigManager.OnSongHypeChanged01 -= SetSongHype;
        }

        private void Update()
        {
            if (songHypeImage == null)
                return;

            float t = _baseHype01;

            // Only wobble if we have enough "room"
            if (enableHypeWobble
                && hypeWobbleAmplitude > 0f
                && _baseHype01 > hypeWobbleAmplitude * 2f)
            {
                float wobble = Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * hypeWobbleSpeed)
                               * hypeWobbleAmplitude;
                t = Mathf.Clamp01(t + wobble);
            }

            songHypeImage.fillAmount = t;
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
            // Event entry point: use default duration
            SetSongHype(hype01, hypeLerpDefaultDuration);
        }

        public void SetSongHype(float hype01, float lerpDuration)
        {
            float target = Mathf.Clamp01(hype01);

            if (!_songHypeVisible)
            {
                _baseHype01 = target;
                UpdateSongHypeBaseVisuals(); // will only affect label/color if visible
                return;
            }

            if (_hypeLerpRoutine != null)
                StopCoroutine(_hypeLerpRoutine);

            _hypeLerpRoutine = StartCoroutine(HypeLerpRoutine(target, lerpDuration));
        }

        private IEnumerator HypeLerpRoutine(float target, float duration)
        {
            float start = _baseHype01;

            if (duration <= 0f)
            {
                _baseHype01 = target;
                UpdateSongHypeBaseVisuals();
                _hypeLerpRoutine = null;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                _baseHype01 = Mathf.Lerp(start, target, t);
                UpdateSongHypeBaseVisuals();

                yield return null;
            }

            _baseHype01 = target;
            UpdateSongHypeBaseVisuals();
            _hypeLerpRoutine = null;
        }

        public void ClearSongHype()
        {
            if (_hypeLerpRoutine != null)
            {
                StopCoroutine(_hypeLerpRoutine);
                _hypeLerpRoutine = null;
            }

            _baseHype01 = 0f;
            UpdateSongHypeBaseVisuals();

            SetSongHypeVisible(false);
        }

        private Color EvaluateHypeColor(float t)
        {
            t = Mathf.Clamp01(t);

            if (t <= 0.5f)
            {
                // 0..0.5 → red → yellow
                float k = t / 0.5f; // 0..1
                return Color.Lerp(hypeLowColor, hypeMidColor, k);
            }
            else
            {
                // 0.5..1 → yellow → green
                float k = (t - 0.5f) / 0.5f; // 0..1
                return Color.Lerp(hypeMidColor, hypeHighColor, k);
            }
        }

        private void UpdateSongHypeBaseVisuals()
        {
            float t = Mathf.Clamp01(_baseHype01);

            if (songHypeLabel != null)
                songHypeLabel.text = $"{Mathf.RoundToInt(t * 100f)}%";

            if (songHypeImage != null)
                songHypeImage.color = EvaluateHypeColor(t);
        }

        public void SetSongHypeVisible(bool visible)
        {
            _songHypeVisible = visible;

            if (songHypeRoot != null)
                songHypeRoot.SetActive(visible);

            if (!visible && songHypeLabel != null)
                songHypeLabel.text = string.Empty;
        }
    }
}