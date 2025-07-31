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

        public void UpdateHealthText(int currentHealth, int maxHealth) => 
            currentHealthText.text = $"{currentHealth}/{maxHealth}";

        #region Pointer Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region Tooltips
        public void HideTooltipInfo()
        {
            throw new System.NotImplementedException();
        }

        public void ShowTooltipInfo()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}