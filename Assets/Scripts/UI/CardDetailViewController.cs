using ALWTTT.Cards;
using ALWTTT.Tooltips;   // [TEXT-5] GameText — player text lives in the glossary
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// <summary>
    /// Singleton controller for the right-click card detail modal.
    /// Shows an enlarged card view with full composition detail,
    /// modifier list, style-bundle references, and effect descriptions.
    ///
    /// M1.10: Home for any text cut from the card face.
    /// </summary>
    public class CardDetailViewController : MonoBehaviour
    {
        public static CardDetailViewController Instance { get; private set; }

        [Header("Canvas")]
        [Tooltip("Dedicated Screen Space – Overlay canvas. Disabled by default.")]
        [SerializeField] private Canvas detailCanvas;

        [Header("Background")]
        [Tooltip("Full-screen semi-transparent image. Click dismisses the modal.")]
        [SerializeField] private Button dismissButton;

        [Header("Card Visuals")]
        [SerializeField] private Image cardArtImage;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private TextMeshProUGUI cardTypeText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI genText;

        [Tooltip("[S5e-ext] Optional root for the gen badge/row (icon + label + " +
                 "number). Hidden when the card generates 0 inspiration. If " +
                 "unassigned, only genText's GameObject is toggled.")]
        [SerializeField] private GameObject genBadgeRoot;

        [Header("Detail Content")]
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI keywordsText;

        private CardDefinition _currentCard;
        private bool _isVisible;

        #region Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[CardDetailView] Duplicate instance destroyed.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (dismissButton != null)
                dismissButton.onClick.AddListener(Hide);

            // Ensure canvas starts disabled.
            if (detailCanvas != null)
                detailCanvas.enabled = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        // [CARD-FACE-3 / O-CF3-2 / D5=B] The Esc handler that lived here is gone. Esc is now read in
        // ONE place, UIManager.HandleEscapeKey, which closes this modal first and only then falls
        // through to its scene rule. This class no longer polls any key: it is driven by Toggle/Show/
        // Hide (right click, dismiss button) and by that chain. Do not re-add an Update here; add the
        // new layer to the chain in UIManager instead.

        #endregion

        #region Public API

        /// <summary>
        /// Toggle behavior for right-click:
        /// - Hidden → show this card.
        /// - Visible + same card → hide.
        /// - Visible + different card → swap to new card.
        /// </summary>
        public void Toggle(CardDefinition card)
        {
            if (card == null) return;

            if (_isVisible && _currentCard == card)
            {
                Hide();
                return;
            }

            Show(card);
        }

        public void Show(CardDefinition card)
        {
            if (card == null) return;

            _currentCard = card;
            Populate(card);

            if (detailCanvas != null)
                detailCanvas.enabled = true;

            _isVisible = true;
            SetDragging(false);
        }

        public void Hide()
        {
            if (detailCanvas != null)
                detailCanvas.enabled = false;

            _isVisible = false;
            _currentCard = null;
            SetDragging(true);
        }

        public bool IsVisible => _isVisible;

        #endregion

        #region Internal

        private void Populate(CardDefinition card)
        {
            // Card art
            if (cardArtImage != null)
            {
                cardArtImage.sprite = card.CardSprite;
                cardArtImage.enabled = card.CardSprite != null;
            }

            // Header fields
            if (cardNameText != null)
                cardNameText.text = GameText.CardName(card);   // [TEXT-5 / F-TXT4-3] displayName is a developer label

            if (cardTypeText != null)
            {
                cardTypeText.text = card.IsComposition
                    ? GameText.Format(GameTextTemplateIds.UiCardTypeComposition)   // [TEXT-5 / D-T5-3=A] same id as CardBase.typeTextField
                    // [CARD-FACE-2 / A16, F-TXT4-2] The enum name is gone. The modal has no hover (F-CF-5), so
                    // an opaque symbol carries its WORD: glyph + name (D-CF-13=C). Not gated by UseSymbols: the
                    // toggle compares card FACES (D-CF-19=B).
                    // [CARD-FACE-2b / D-CF-25=B] Hidden while the action card type is off for players.
                    : !CardFacePresentation.ShowCardType ? string.Empty
                    : GameSymbols.CardTypeGlyph(card.CardType) + " " +
                      GameText.Format(GameTextTemplateIds.UiCardType(card.CardType));
            }

            if (costText != null)
                costText.text = card.InspirationCost.ToString();

            // [S5e-ext] Post-D3 all content authors gen=0; the badge is noise.
            // Symmetric toggle so pooled/reused views recover if gen>0 returns.
            bool showGen = card.InspirationGenerated > 0;
            if (genBadgeRoot != null)
                genBadgeRoot.SetActive(showGen);
            else if (genText != null)
                genText.gameObject.SetActive(showGen);
            if (genText != null && showGen)
                genText.text = card.InspirationGenerated.ToString();

            // Full detail description (composition cards get the expanded view).
            if (descriptionText != null)
                descriptionText.text = card.GetDetailDescription();

            // Keywords
            if (keywordsText != null)
            {
                if (card.Keywords != null && card.Keywords.Count > 0)
                {
                    var sb = new System.Text.StringBuilder();
                    for (int i = 0; i < card.Keywords.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        // [TEXT-5 / D-T5-4=A] Keyword ids are the enum names lowercased and are REQUIRED
                        // glossary coverage since TXT-3, so this is not an F-TXT4-2 enum: a registry reaches it.
                        sb.Append(GameText.ConceptName(card.Keywords[i].ToString().ToLowerInvariant()));
                    }
                    keywordsText.text = sb.ToString();
                    keywordsText.gameObject.SetActive(true);
                }
                else
                {
                    keywordsText.gameObject.SetActive(false);
                }
            }
        }

        private void SetDragging(bool enabled)
        {
            var dm = Managers.DeckManager.Instance;
            if (dm == null || dm.HandController == null) return;

            if (enabled)
                dm.HandController.EnableDragging();
            else
                dm.HandController.DisableDragging();
        }

        #endregion
    }
}