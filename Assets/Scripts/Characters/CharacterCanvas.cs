using ALWTTT.Tooltips;
using ALWTTT.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ALWTTT.Characters
{
    [RequireComponent(typeof(Canvas))]
    public class CharacterCanvas : MonoBehaviour, I2DTooltipTarget
    {
        [Header("References")]
        [SerializeField] protected TextMeshProUGUI currentHealthText;
        [SerializeField] protected Slider currentHealthBar;
        [SerializeField] protected TextMeshProUGUI characterNameText;
        [SerializeField] protected Transform highlightRoot;
        [SerializeField] protected HealthBarController healthBar;

        #region Setup
        public void InitCanvas(string characterName)
        {
            characterNameText.text = characterName;

            // TODO
            // Highlight
            // Status Dict
        }
        #endregion

        #region Public Methods

        public void UpdateHealthText(int currentHealth, int maxHealth)
        {
            float fill = (float)currentHealth / maxHealth;
            currentHealthBar.value = fill;
            currentHealthText.text = $"{currentHealth}/{maxHealth}";
        }

        public void SetCurrentVibe(int current, int max, float duration)
        {
            healthBar?.SetCurrentVibe(current, max, duration);
        }
            
        public void SetHighlight(bool open) => highlightRoot.gameObject.SetActive(open);

        #endregion

        #region Pointer Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltipInfo();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltipInfo();
        }
        #endregion

        #region Tooltips
        public void HideTooltipInfo()
        {
            // TODO
        }

        public void ShowTooltipInfo()
        {
            // TODO
        }
        #endregion
    }
}