using ALWTTT.Managers;
using ALWTTT.Utils;
using ALWTTT.Tooltips; // [BIGNUM-1r] VibeReadoutTooltipTarget
using static ALWTTT.Tooltips.GameTextTemplateIds; // [TEXT-VIBE-1] ui_readout_*
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
        // [GIG-UI-1] Optional root of the exhaust counter: hidden while the pile is empty
        // (Slay the Spire does the same). Null = always visible, as before.
        [SerializeField] private GameObject exhaustPileRoot;
        [SerializeField] private TextMeshProUGUI songsLeftTextField;
        // [DEV-TUNING-1 / D3=A] «N/M» de la mano junto al abanico, como el «8/8» de Balatro.
        // Siempre visible; cambia de color cuando la mano está llena. Optional: null = hidden.
        [SerializeField] private TextMeshProUGUI handCountTextField;
        [SerializeField] private Color handCountColor = new Color(1f, 1f, 1f, 0.85f);
        [SerializeField] private Color handCountFullColor = new Color(1f, 0.55f, 0.35f, 1f);
        // Borde de la letra, puesto por código (el material del TMP se instancia: el asset
        // de la fuente no cambia). 0 = sin borde.
        [SerializeField, Range(0f, 0.5f)] private float handCountOutline = 0.3f;
        [SerializeField] private Color handCountOutlineColor = Color.black;
        private int _handCountShown = -1, _handMaxShown = -1;
        private bool _handCountStyled;
        // [COH-1 / D3] "Cohesión: N" in the gig HUD. Optional: null = hidden.
        [SerializeField] private TextMeshProUGUI cohesionTextField;
        // [COH-1 / GIG-FRAME] Cohesion as discrete pips (Zelda hearts), one per
        // point up to MaxCohesion. Optional: no root or prefab = no pips.
        // cohesionPipRoot carries a Horizontal/Vertical Layout Group; pips are
        // instantiated from cohesionPipPrefab at runtime. Empty sprite null →
        // the full sprite tinted with cohesionPipEmptyTint.
        [Header("Cohesion pips (COH-1)")]
        [SerializeField] private RectTransform cohesionPipRoot;
        [SerializeField] private Image cohesionPipPrefab;
        [SerializeField] private Sprite cohesionPipFull;
        [SerializeField] private Sprite cohesionPipEmpty;
        [SerializeField] private Color cohesionPipEmptyTint = new Color(1f, 1f, 1f, 0.25f);
        private readonly System.Collections.Generic.List<Image> _cohesionPips =
            new System.Collections.Generic.List<Image>();

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

            // [COH-1 / D3]
            ALWTTT.Data.PersistentGameplayData.BandCohesionChanged += SetCohesion;
            var pd = GameManager != null ? GameManager.PersistentGameplayData : null;
            if (pd != null) SetCohesion(pd.BandCohesion);

            if (songHypeRoot != null)
                songHypeRoot.SetActive(false);

            // [BIGNUM-1 / D-BN-7=A] The readout starts hidden too; GigManager shows it
            // at song start from its own switch (ShowVibeReadout), never from the bar.
            SetVibeReadoutVisible(false);
            _songHypeVisible = false;
        }

        private void OnDisable()
        {
            // [COMBOS-1] Un vuelo cortado no deja el Big Num congelado.
            if (_absorbRoutine != null) { StopCoroutine(_absorbRoutine); _absorbRoutine = null; }
            ReleaseAbsorbHold();

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
            ALWTTT.Data.PersistentGameplayData.BandCohesionChanged -= SetCohesion; // [COH-1]
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

        // [DEV-TUNING-1] El tope puede cambiar en vivo (F5), así que se mira cada frame
        // y solo se reescribe el texto cuando cambia.
        private void LateUpdate()
        {
            if (handCountTextField == null) return;
            if (!_handCountStyled)
            {
                _handCountStyled = true;
                handCountTextField.outlineWidth = handCountOutline;
                handCountTextField.outlineColor = handCountOutlineColor;
            }
            var dm = DeckManager.Instance;
            if (dm == null || dm.HandPile == null || GameManager.Instance == null) return;
            int n = dm.HandPile.Count, max = dm.MaxHandSize;
            if (n == _handCountShown && max == _handMaxShown) return;
            _handCountShown = n; _handMaxShown = max;
            handCountTextField.text = $"{n}/{max}";
            handCountTextField.color = n >= max ? handCountFullColor : handCountColor;
        }

        public void SetPileTexts()
        {
            drawPileTextField.text = $"{DeckManager.DrawPile.Count.ToString()}";
            discardPileTextField.text = $"{DeckManager.DiscardPile.Count.ToString()}";
            exhaustPileTextField.text = $"{DeckManager.ExhaustPile.Count.ToString()}";
            if (exhaustPileRoot != null)
                exhaustPileRoot.SetActive(DeckManager.ExhaustPile.Count > 0);
        }

        public void SetCohesion(int value)
        {
            if (cohesionTextField != null)
                cohesionTextField.text = $"Cohesión: {value}";

            if (cohesionPipRoot == null || cohesionPipPrefab == null) return;

            int max = GameManager != null && GameManager.GameplayData != null
                ? GameManager.GameplayData.MaxCohesion : 10;
            // Cohesion can exceed MaxCohesion (Dev stepper has no cap): show them all.
            int count = Mathf.Max(max, value);

            while (_cohesionPips.Count < count)
                _cohesionPips.Add(Instantiate(cohesionPipPrefab, cohesionPipRoot));
            for (int i = 0; i < _cohesionPips.Count; i++)
            {
                var pip = _cohesionPips[i];
                if (pip == null) continue;
                pip.gameObject.SetActive(i < count);
                bool full = i < value;
                pip.raycastTarget = false;
                // [GIG-UI-1] Keep the colour authored on the pip prefab (it was forced to white,
                // which hid a tinted sprite such as white-heart). Empty = prefab colour x tint.
                Color baseColor = cohesionPipPrefab.color;
                if (full)
                {
                    if (cohesionPipFull != null) pip.sprite = cohesionPipFull;
                    pip.color = baseColor;
                }
                else if (cohesionPipEmpty != null)
                {
                    pip.sprite = cohesionPipEmpty;
                    pip.color = baseColor;
                }
                else
                {
                    if (cohesionPipFull != null) pip.sprite = cohesionPipFull;
                    pip.color = baseColor * cohesionPipEmptyTint;
                }
            }
        }

        /// <summary>
        /// [COH-1 / D-COH-1, D4] Map run (the demo): Retry hidden, a single
        /// button with <paramref name="singleLabel"/> that fires OnLossExit.
        /// Otherwise (GigSetup) Retry + Exit as before. The single button is
        /// lossExitButton; lossConfirmButton, when it is a separate button, is
        /// hidden in the demo so only one way out shows.
        /// </summary>
        public void ConfigureLossButtons(bool demo, string singleLabel)
        {
            if (lossRetryButton != null) lossRetryButton.gameObject.SetActive(!demo);
            if (lossConfirmButton != null
                && lossConfirmButton != lossRetryButton
                && lossConfirmButton != lossExitButton)
                lossConfirmButton.gameObject.SetActive(!demo);
            if (lossExitButton != null)
            {
                lossExitButton.gameObject.SetActive(true);
                SetButtonLabel(lossExitButton, ref _lossExitDefaultLabel, demo ? singleLabel : null);
            }
        }

        /// <summary>[COH-1 / D2] Map run: Retry hidden, Exit reads "Volver al menú".</summary>
        public void ConfigureWinButtons(bool demo, string exitLabel)
        {
            if (winRetryButton != null) winRetryButton.gameObject.SetActive(!demo);
            if (winExitButton != null)
            {
                winExitButton.gameObject.SetActive(true);
                SetButtonLabel(winExitButton, ref _winExitDefaultLabel, demo ? exitLabel : null);
            }
        }

        private string _lossExitDefaultLabel;
        private string _winExitDefaultLabel;

        // label null → restore the prefab's own text.
        private static void SetButtonLabel(Button button, ref string defaultLabel, string label)
        {
            var tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp == null) return;
            if (defaultLabel == null) defaultLabel = tmp.text;
            tmp.text = label ?? defaultLabel;
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

            _readoutTotal = total;   // [LEGIB-1a] restored by ClearVibeReadoutPreview
            _readoutPreviewOn = false;   // plain text again; GigManager re-applies the forecast
            WriteReadout(total.ToString());

            if (vibeReadoutTooltip != null)
                vibeReadoutTooltip.SetContent(GameText.Format(UiReadoutHeader),
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
        // each member's bar tooltip.
        // [TEXT-VIBE-1 / F-TIP-2 / D3=A] Population 11: every line is a ui_readout_* template. The
        // line break between the total and its note stays in code as layout (separators rule).
        // [TEXT-VIBE-1 / D1] The SFX line prints only when it moves the number: output == input
        // suppresses, the same rule as AudienceCharacterCanvas.BuildVibeText. First line (Hype) and
        // the total always print.
        private static string BuildReadoutTooltip(int lPart, int sfxPart, float hype01, float avgMaxVibe)
        {
            var sb = new System.Text.StringBuilder();
            int hypePercent = Mathf.RoundToInt(Mathf.Clamp01(hype01) * 100f);
            sb.Append(GameText.Format(UiReadoutHype, hypePercent, lPart));
            if (sfxPart != 0)
                sb.Append('\n').Append(GameText.Format(UiReadoutSfx, sfxPart));
            sb.Append('\n').Append(GameText.Format(UiReadoutTotal, lPart + sfxPart));
            sb.Append('\n').Append(GameText.Format(UiReadoutTotalNote));
            if (avgMaxVibe > 0f)
                sb.Append('\n').Append(GameText.Format(UiReadoutAvgResist, Mathf.RoundToInt(avgMaxVibe)));
            return sb.ToString();
        }

        // ── [LEGIB-1a / D6=A] «Y → Z» always on while a loop runs ─────────────
        // Y = Big Num now. Z = Big Num at the end of the loop now playing: at rest,
        // with what is already played; with a card hovered/held, as if it were
        // played. Z is drawn smaller than Y (Y is the fact, Z the forecast), and
        // the card state nudges Z bigger and coloured so "projecting a card" reads
        // differently from "at rest". Rich-text size/colour on Z only, so the
        // label's own magnitude scaling/colour (BIGNUM-1) still drives Y.
        // The arrow is layout (separators rule), not player text. If the font has
        // no '→' glyph, falls back to '>' instead of showing a missing box.
        [Header("LEGIB-1a: Big Num forecast (Y → Z)")]
        [SerializeField, Range(0.3f, 1f), Tooltip("Size of '→ Z' relative to Y at rest.")]
        private float forecastRestScale = 0.6f;
        [SerializeField, Tooltip("Colour of '→ Z' at rest (subdued).")]
        private Color forecastRestColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField, Range(0.3f, 1f), Tooltip("Size of '→ Z' relative to Y while a card is projected.")]
        private float forecastCardScale = 0.75f;
        [SerializeField, Tooltip("Colour of '→ Z' while a card is projected.")]
        private Color forecastCardColor = new Color(1f, 0.84f, 0.29f, 1f);

        private int _readoutTotal = -1;
        private bool _readoutPreviewOn;

        // ── [COMBOS-1 / D3] Línea de combo y multiplicador que el Big Num «absorbe» ──
        [Header("COMBOS-1: combos")]
        [SerializeField, Range(0.3f, 1f), Tooltip("[COMBOS-1] Tamaño de la línea de combo bajo «Y → Z».")]
        private float comboLineScale = 0.45f;
        [SerializeField, Tooltip("[COMBOS-1] Color de la línea de combo.")]
        private Color comboLineColor = new Color(1f, 0.62f, 0.15f, 1f);
        [SerializeField, Tooltip("[COMBOS-1] Color del multiplicador que vuela al Big Num.")]
        private Color comboAbsorbColor = new Color(1f, 0.62f, 0.15f, 1f);
        [SerializeField, Tooltip("[COMBOS-1] Tamaño de letra del multiplicador que vuela.")]
        private float comboAbsorbFontSize = 64f;

        // Mientras el multiplicador vuela, el Big Num no cambia: el texto nuevo espera y se
        // escribe al llegar («absorbido»). Lo que se escribe es lo mismo que sin vuelo.
        private bool _absorbHold;
        private string _absorbPendingText;
        private Coroutine _absorbRoutine;

        private void WriteReadout(string text)
        {
            if (vibeReadoutLabel == null) return;
            if (_absorbHold) { _absorbPendingText = text; return; }
            vibeReadoutLabel.text = text;
        }

        private void ReleaseAbsorbHold()
        {
            if (!_absorbHold) return;
            _absorbHold = false;
            if (_absorbPendingText != null && vibeReadoutLabel != null)
                vibeReadoutLabel.text = _absorbPendingText;
            _absorbPendingText = null;
        }

        // [COMBOS-1 / D3] Tabla de combos (C). Se crea por código la primera vez: un
        // panel oscuro con un texto, arriba al centro, encima de todo y sin capturar clics.
        private GameObject _comboTableGo;
        private TextMeshProUGUI _comboTableText;

        public void SetComboTable(bool visible, string richText)
        {
            if (!visible)
            {
                if (_comboTableGo != null) _comboTableGo.SetActive(false);
                return;
            }
            if (_comboTableGo == null)
            {
                Canvas root = vibeReadoutLabel != null && vibeReadoutLabel.canvas != null
                    ? vibeReadoutLabel.canvas.rootCanvas : GetComponentInParent<Canvas>();
                if (root == null) return;
                _comboTableGo = new GameObject("COMBOS-1 Tabla", typeof(RectTransform));
                var rt = (RectTransform)_comboTableGo.transform;
                rt.SetParent(root.transform, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.62f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(1080f, 420f);
                // Encima de todo: la barra de la canción vive en otro canvas y se pintaba
                // sobre la tabla (smoke 3). Un Canvas propio con orden alto la tapa.
                var tableCanvas = _comboTableGo.AddComponent<Canvas>();
                tableCanvas.overrideSorting = true;
                tableCanvas.sortingOrder = 500;
                var bg = _comboTableGo.AddComponent<Image>();
                bg.color = new Color(0.05f, 0.04f, 0.09f, 0.92f);
                bg.raycastTarget = false;

                var textGo = new GameObject("Texto", typeof(RectTransform));
                var trt = (RectTransform)textGo.transform;
                trt.SetParent(rt, false);
                trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
                trt.offsetMin = new Vector2(36f, 24f); trt.offsetMax = new Vector2(-36f, -24f);
                _comboTableText = textGo.AddComponent<TextMeshProUGUI>();
                if (vibeReadoutLabel != null) _comboTableText.font = vibeReadoutLabel.font;
                _comboTableText.fontSize = 26f;
                _comboTableText.color = Color.white;
                _comboTableText.alignment = TextAlignmentOptions.TopLeft;
                _comboTableText.richText = true;
                _comboTableText.enableWordWrapping = false;
                _comboTableText.raycastTarget = false;
            }
            _comboTableGo.transform.SetAsLastSibling();
            _comboTableGo.SetActive(true);
            _comboTableText.text = richText;
        }

        /// <summary>
        /// [COMBOS-1 / D3] «×1,5» salta con un rebote en <paramref name="worldFrom"/> (sobre
        /// la banda), vuela al Big Num y el Big Num lo absorbe: el número nuevo aparece al
        /// llegar, con un pulso. Llamar ANTES de sumar el Hype del loop.
        /// </summary>
        public void PlayComboAbsorb(string text, Vector3 worldFrom)
        {
            if (vibeReadoutLabel == null || !isActiveAndEnabled) return;
            if (_absorbRoutine != null) { StopCoroutine(_absorbRoutine); ReleaseAbsorbHold(); }
            _absorbHold = true;
            _absorbPendingText = null;
            _absorbRoutine = StartCoroutine(ComboAbsorbRoutine(text, worldFrom));
        }

        private IEnumerator ComboAbsorbRoutine(string text, Vector3 worldFrom)
        {
            var canvas = vibeReadoutLabel.canvas != null ? vibeReadoutLabel.canvas.rootCanvas : null;
            if (canvas == null) { ReleaseAbsorbHold(); _absorbRoutine = null; yield break; }
            var canvasRt = (RectTransform)canvas.transform;
            Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Camera worldCam = Camera.main;

            Vector2 from = Vector2.zero, to = Vector2.zero;
            if (worldCam != null)
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt,
                    worldCam.WorldToScreenPoint(worldFrom), uiCam, out from);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt,
                RectTransformUtility.WorldToScreenPoint(uiCam, vibeReadoutLabel.rectTransform.position),
                uiCam, out to);

            var go = new GameObject("COMBOS-1 Absorb", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(canvasRt, false);
            rt.SetAsLastSibling();
            rt.sizeDelta = new Vector2(400f, 120f);
            rt.localPosition = from;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = vibeReadoutLabel.font;
            tmp.fontSize = comboAbsorbFontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = comboAbsorbColor;
            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = Color.black;
            tmp.raycastTarget = false;
            tmp.text = text;

            Debug.Log($"[COMBOS-1] Absorción: «{text}» vuela al Big Num");

            // 1. Rebote: 0 → 1,3 → 1.
            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                float u = t / 0.3f;
                float sc = u < 0.6f ? Mathf.Lerp(0f, 1.3f, u / 0.6f) : Mathf.Lerp(1.3f, 1f, (u - 0.6f) / 0.4f);
                rt.localScale = Vector3.one * sc;
                yield return null;
            }
            rt.localScale = Vector3.one;
            // 2. Se queda a la vista un momento.
            for (float t = 0f; t < 0.45f; t += Time.unscaledDeltaTime) yield return null;
            // 3. Vuela al Big Num, acelerando y encogiendo.
            for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime)
            {
                float u = t / 0.4f;
                float e = u * u;
                rt.localPosition = Vector2.LerpUnclamped(from, to, e);
                rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.4f, e);
                yield return null;
            }
            Destroy(go);

            // 4. Absorbido: el número nuevo aparece con un pulso.
            ReleaseAbsorbHold();
            Transform pulseT = vibeReadoutRoot != null ? vibeReadoutRoot.transform : null;
            if (pulseT != null)
            {
                Vector3 baseScale = pulseT.localScale;
                for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
                {
                    pulseT.localScale = baseScale * Mathf.Lerp(1.35f, 1f, t / 0.3f);
                    yield return null;
                }
                pulseT.localScale = baseScale;
            }
            _absorbRoutine = null;
        }

        public void SetVibeReadoutPreview(int y, int z, bool cardProjected, string comboLine = null)
        {
            if (vibeReadoutLabel == null) return;

            // [COMBOS-1 / D3] Segunda línea: el combo que el loop (o la carta) completa,
            // con su valor. Va en la misma etiqueta: nada que montar en el editor.
            string combo = string.IsNullOrEmpty(comboLine) ? string.Empty
                : $"\n<size={Mathf.RoundToInt(comboLineScale * 100f)}%><color=#{ColorUtility.ToHtmlStringRGBA(comboLineColor)}>{comboLine}</color></size>";

            // [LEGIB-1a / D6 amended by Claudio] At rest, no change = no arrow: the plain
            // number already says "this loop adds nothing". A projected card keeps
            // «Y → Y», which answers the hover question "what does this card do?".
            if (!cardProjected && y == z && combo.Length == 0)
            {
                WriteReadout(y.ToString());
                _readoutPreviewOn = false;
                return;
            }

            bool hasArrow = vibeReadoutLabel.font != null
                && vibeReadoutLabel.font.HasCharacter('→', true);
            string arrow = hasArrow ? " → " : " > ";
            float scale = cardProjected ? forecastCardScale : forecastRestScale;
            Color color = cardProjected ? forecastCardColor : forecastRestColor;
            int pct = Mathf.RoundToInt(scale * 100f);
            string forecast = (!cardProjected && y == z) ? $"{y}"
                : $"{y}<size={pct}%><color=#{ColorUtility.ToHtmlStringRGBA(color)}>{arrow}{z}</color></size>";
            WriteReadout(forecast + combo);
            _readoutPreviewOn = true;
        }

        public void ClearVibeReadoutPreview()
        {
            if (!_readoutPreviewOn) return;
            _readoutPreviewOn = false;
            if (vibeReadoutLabel != null && _readoutTotal >= 0)
                WriteReadout(_readoutTotal.ToString());
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
            // [CARD-FACE-1 / F-TV-1 / D-CF-7] Called from OnEnable; GameText resolves the glossary per call
            // and applies the section 3 miss policy, so no Start() detour is needed.
            songsLeftTextField.text = GameText.Format(UiGigSongsLeft, songsLeft);
        }
    }
}