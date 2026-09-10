using ALWTTT.Managers;
using ALWTTT.Utils;
using ALWTTT.Tooltips; // [BIGNUM-1r] VibeReadoutTooltipTarget
using System.Collections;
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
        [SerializeField] private TextMeshProUGUI songsLeftTextField;

        [Header("Buttons")]
        // [B3-demo-polish / D-UX2] Win/Loss panels each get Retry + Exit buttons.
        // Retry reloads the gig scene (preserving GigRunContext / PersistentGameplayData
        // so the same encounter config replays from scratch). Exit returns to the
        // main menu / gig setup. lossConfirmButton retained for backwards compatibility
        // and treated as semantic equivalent of lossExitButton.
        [SerializeField] private Button lossConfirmButton; // existing — kept; equivalent to lossExitButton
        [SerializeField] private Button winRetryButton;
        [SerializeField] private Button winExitButton;
        [SerializeField] private Button lossRetryButton;
        [SerializeField] private Button lossExitButton;

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
        [SerializeField] private TextMeshProUGUI vibeReadoutLabel; // [S5a] "L + SFX = N" under the SongHype bar


        // [BIGNUM-1 / D-BN-1=A + D-BN-7=A] The C1 readout is now its own surface.
        // PREFAB RULE: vibeReadoutLabel must live under vibeReadoutRoot, a SIBLING of
        // songHypeRoot — not a child — or SetSongHypeVisible(false) keeps hiding it.
        // [BIGNUM-1r / D-BN-13=A] The label shows ONLY the big total; the band-wide
        // chain (Hype % → L, + SFX = N, one member's worth) is a hover tooltip on the
        // number (vibeReadoutTooltip, optional). vibeReadoutPulse is optional: when
        // null, the label is scaled directly and does not beat.
        [Header("Vibe Readout (BIGNUM-1)")]
        [SerializeField] private GameObject vibeReadoutRoot;
        [SerializeField] private VibeReadoutTooltipTarget vibeReadoutTooltip; // [BIGNUM-1r / D-BN-13=A]
        [SerializeField] private VibeReadoutBeatPulse vibeReadoutPulse;
        [SerializeField, Min(0.1f)] private float readoutMinScale = 1f;
        [SerializeField, Min(0.1f)] private float readoutMaxScale = 2.2f;
        [SerializeField] private Color readoutColdColor = Color.white;
        [SerializeField] private Color readoutHotColor = new Color(1f, 0.35f, 0.2f);
        [SerializeField]
        private AnimationCurve readoutGrowthCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

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

        // [B3-demo-polish / D-UX2] Wired by GigManager.
        public System.Action OnLossConfirm; // existing — kept for back-compat
        public System.Action OnWinRetry;
        public System.Action OnWinExit;
        public System.Action OnLossRetry;
        public System.Action OnLossExit;

        [Header("References")]
        [SerializeField] private SceneChanger sceneChanger;

        public GameObject WinPanel => winPanel;
        public GameObject LosePanel => lossPanelRoot;

        private void OnEnable()
        {
            // [B3-demo-polish / F6] Skip lossConfirmButton subscription if it
            // points to the same Button as the new explicit Retry/Exit refs.
            // Prevents a single click on Retry/Exit from also firing the legacy
            // Confirm path (which calls ReturnToMap → clears pd.CurrentEncounter,
            // triggering wrong-encounter load on the immediately-following Retry).
            // New explicit OnLossRetry/OnLossExit handlers supersede OnLossConfirm.
            if (lossConfirmButton != null
                && lossConfirmButton != lossRetryButton
                && lossConfirmButton != lossExitButton)
            {
                lossConfirmButton.onClick.AddListener(OnClick_LossConfirm);
            }
            else if (lossConfirmButton != null)
            {
                Debug.Log("[GigCanvas / F6] lossConfirmButton points to the same " +
                          "Button as lossRetryButton or lossExitButton — skipping " +
                          "duplicate OnClick_LossConfirm subscription.");
            }

            // [B3-demo-polish / D-UX2] Subscribe new win/loss Retry & Exit buttons.
            // Null-guarded so the canvas survives prefab states where some buttons
            // haven't been authored yet.
            if (winRetryButton != null) winRetryButton.onClick.AddListener(OnClick_WinRetry);
            if (winExitButton != null) winExitButton.onClick.AddListener(OnClick_WinExit);
            if (lossRetryButton != null) lossRetryButton.onClick.AddListener(OnClick_LossRetry);
            if (lossExitButton != null) lossExitButton.onClick.AddListener(OnClick_LossExit);

            GigManager.OnPlayerTurnStarted += ShowBandTurnUI;
            GigManager.OnSongPerformanceStarted += ShowSongPerformanceUI;
            GigManager.OnEnemyTurnStarted += ShowAudienceTurnUI;
            GigManager.OnSongHypeChanged01 += SetSongHype;
            GigManager.OnSongsLeftChanged += SetSongsLeft;

            SetSongsLeft(GigManager.SongsLeft, GigManager.RequiredSongCount);

            if (songHypeRoot != null)
                songHypeRoot.SetActive(false);

            // [BIGNUM-1 / D-BN-7=A] The readout starts hidden too; GigManager shows it
            // at song start from its own switch (ShowVibeReadout), never from the bar.
            SetVibeReadoutVisible(false);
            _songHypeVisible = false;
        }

        private void OnDisable()
        {
            // [B3-demo-polish / F6] Mirror the conditional subscription.
            if (lossConfirmButton != null
                && lossConfirmButton != lossRetryButton
                && lossConfirmButton != lossExitButton)
            {
                lossConfirmButton.onClick.RemoveListener(OnClick_LossConfirm);
            }

            // [B3-demo-polish / D-UX2]
            if (winRetryButton != null) winRetryButton.onClick.RemoveListener(OnClick_WinRetry);
            if (winExitButton != null) winExitButton.onClick.RemoveListener(OnClick_WinExit);
            if (lossRetryButton != null) lossRetryButton.onClick.RemoveListener(OnClick_LossRetry);
            if (lossExitButton != null) lossExitButton.onClick.RemoveListener(OnClick_LossExit);

            GigManager.OnPlayerTurnStarted -= ShowBandTurnUI;
            GigManager.OnSongPerformanceStarted -= ShowSongPerformanceUI;
            GigManager.OnEnemyTurnStarted -= ShowAudienceTurnUI;
            GigManager.OnSongHypeChanged01 -= SetSongHype;
            GigManager.OnSongsLeftChanged -= SetSongsLeft;
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

        // [B3-demo-polish / D-UX2] New button handlers. Each closes its panel
        // and fires its action; GigManager subscribes and routes Retry to
        // scene reload, Exit to main menu scene change.
        public void OnClick_WinRetry()
        {
            winPanel.SetActive(false);
            OnWinRetry?.Invoke();
        }

        public void OnClick_WinExit()
        {
            winPanel.SetActive(false);
            OnWinExit?.Invoke();
        }

        public void OnClick_LossRetry()
        {
            lossPanelRoot.SetActive(false);
            OnLossRetry?.Invoke();
        }

        public void OnClick_LossExit()
        {
            lossPanelRoot.SetActive(false);
            OnLossExit?.Invoke();
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

        // [S5a/T7] C1 global accumulator readout: "L + SFX = N" under the SongHype bar.
        // L = SongHype-driven base Vibe (volatile), SFX = banked flat bonus (monotonic),
        // N = total. Driven by GigManager.RefreshVibeProjection at loop boundaries.
        // [BIGNUM-1] The label lives under vibeReadoutRoot (own switch); the bar no
        // longer governs it. Size and colour scale with magnitude01 (D-BN-4: relative
        // to the crowd — 1.0 = one average unconvinced member's worth this song); the
        // beat pulse, when wired, gets the same magnitude as intensity (D-BN-3=A).
        public void SetVibeReadout(int lPart, int sfxPart, float magnitude01,
            float hype01, float avgMaxVibe)
        {
            if (vibeReadoutLabel == null) return;

            int total = lPart + sfxPart;
            float m = Mathf.Clamp01(magnitude01);

            vibeReadoutLabel.text = total.ToString();

            if (vibeReadoutTooltip != null)
                vibeReadoutTooltip.SetContent("Vibe de la canción",
                    BuildReadoutTooltip(lPart, sfxPart, hype01, avgMaxVibe));

            float k = readoutGrowthCurve != null ? Mathf.Clamp01(readoutGrowthCurve.Evaluate(m)) : m;
            float scale = Mathf.Lerp(readoutMinScale, readoutMaxScale, k);
            vibeReadoutLabel.color = Color.Lerp(readoutColdColor, readoutHotColor, k);

            if (vibeReadoutPulse != null)
            {
                vibeReadoutPulse.SetBaseScale(scale);   // the pulse owns localScale
                vibeReadoutPulse.SetIntensity01(m);
            }
            else
            {
                vibeReadoutLabel.rectTransform.localScale = Vector3.one * scale;
            }
        }

        // [BIGNUM-1r / D-BN-13=A] Band-wide chain only: per-member tastes/status live on
        // each member's bar tooltip. ESP copy hardcoded like the rest (D-S5f-7=A).
        private static string BuildReadoutTooltip(int lPart, int sfxPart, float hype01, float avgMaxVibe)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("Hype de la canción: ")
              .Append(Mathf.RoundToInt(Mathf.Clamp01(hype01) * 100f)).Append(" % → ").Append(lPart);
            sb.Append("\n+ SFX del local → ").Append(sfxPart);
            sb.Append("\n= ").Append(lPart + sfxPart)
              .Append(" de Vibe por espectador,\nantes de gustos y estados.");
            if (avgMaxVibe > 0f)
                sb.Append("\nUn espectador medio aguanta ").Append(Mathf.RoundToInt(avgMaxVibe)).Append('.');
            return sb.ToString();
        }

        // [BIGNUM-1r45 / D-BN-3c] Tempo passthrough for the readout pulse. Called from
        // GigManager.ApplyBpmToStage; a no-op when no pulse component is wired.
        public void SetReadoutTempo(int bpm)
        {
            if (vibeReadoutPulse != null) vibeReadoutPulse.SetFallbackBpm(bpm);
        }

        // [BIGNUM-1 / D-BN-7=A] Independent of SetSongHypeVisible. Falls back to the
        // label's own GameObject when no root is wired (degrades, never NREs).
        public void SetVibeReadoutVisible(bool visible)
        {
            GameObject root = vibeReadoutRoot != null
                ? vibeReadoutRoot
                : (vibeReadoutLabel != null ? vibeReadoutLabel.gameObject : null);
            if (root != null) root.SetActive(visible);

            if (!visible)
            {
                if (vibeReadoutLabel != null) vibeReadoutLabel.text = string.Empty;
                if (vibeReadoutTooltip != null) vibeReadoutTooltip.Clear();
            }
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

            // [BIGNUM-1 / D-BN-7=A] The Vibe readout no longer follows the bar; see
            // SetVibeReadoutVisible.
        }

        public void SetSongsLeft(int songsLeft, int requiredSongCount)
        {
            if (songsLeftTextField == null) return;
            songsLeftTextField.text = $"Songs left: {songsLeft}";
        }
    }
}