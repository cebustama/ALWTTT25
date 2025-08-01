using ALWTTT.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ALWTTT.Characters
{
    [RequireComponent(typeof(Canvas))]
    public class CharacterCanvas : MonoBehaviour, I2DTooltipTarget
    {
        [Header("References")]
        [SerializeField] protected TextMeshProUGUI currentHealthText;
        [SerializeField] protected TextMeshProUGUI characterNameText;

        #region Setup
        public void InitCanvas()
        {
            // TODO
            // Highlight
            // Status Dict
        }
        #endregion

        #region Public Methods

        public void UpdateHealthText(int currentHealth, int maxHealth) =>
            currentHealthText.text = $"{currentHealth}/{maxHealth}";

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