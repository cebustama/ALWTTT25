using ALWTTT.Cards;
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

        private void Update()
        {
            if (!_isVisible) return;

            if (Input.GetKeyDown(KeyCode.Escape))
                Hide();
        }

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
                cardNameText.text = card.DisplayName;

            if (cardTypeText != null)
            {
                cardTypeText.text = card.IsComposition
                    ? "COMPOSITION"
                    : card.CardType.ToString();
            }

            if (costText != null)
                costText.text = card.InspirationCost.ToString();

            if (genText != null)
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
                        sb.Append(card.Keywords[i].ToString());
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