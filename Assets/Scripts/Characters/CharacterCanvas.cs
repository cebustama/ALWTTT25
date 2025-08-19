using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Tooltips;
using ALWTTT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] protected Slider currentHealthBar;
        [SerializeField] protected TextMeshProUGUI currentHealthText;
        [SerializeField] protected TextMeshProUGUI characterNameText;
        [SerializeField] protected Transform highlightRoot;
        [SerializeField] protected Transform statusIconRoot;
        [SerializeField] protected Transform descriptionRoot;
        [SerializeField] protected HealthBarController healthBar;
        [SerializeField] protected StatusIconsData statusIconsData;
        
        protected Dictionary<StatusType, StatusIconBase> StatusDict = 
            new Dictionary<StatusType, StatusIconBase>();

        #region Setup
        public void InitCanvas(string characterName)
        {
            characterNameText.text = characterName;

            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
                StatusDict.Add((StatusType)i, null);
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
            healthBar?.SetCurrentValue(current, max, duration);
        }
            
        public void SetHighlight(bool open) => 
            highlightRoot.gameObject.SetActive(open);

        public void ApplyStatus(StatusType targetStatus, int value)
        {
            if (StatusDict[targetStatus] == null)
            {
                var targetData = statusIconsData.
                    StatusIconList.FirstOrDefault(x => x.IconStatus == targetStatus);

                if (targetData == null) return;

                var clone = Instantiate(statusIconsData.StatusIconBasePrefab, statusIconRoot);
                clone.SetStatus(targetData);
                StatusDict[targetStatus] = clone;
            }

            StatusDict[targetStatus].SetStatusValue(value);
        }

        public void UpdateStatusText(StatusType targetStatus, int value)
        {
            if (StatusDict[targetStatus] == null) return;

            StatusDict[targetStatus].StatusValueText.text = $"{value}";
        }

        public void ClearStatus(StatusType targetStatus)
        {
            if (StatusDict[targetStatus])
            {
                Destroy(StatusDict[targetStatus].gameObject);
            }

            StatusDict[targetStatus] = null;
        }

        public void UpdateVisibility()
        {
            ShowContextual();
            HideContextual();
        }

        public virtual void HideContextual()
        {
            if (healthBar.CurrentValue == 0)
            {
                healthBar.CanvasGroup.alpha = 0;
            }
        }

        public virtual void ShowContextual()
        {
            if (healthBar.CurrentValue > 0)
                healthBar.CanvasGroup.alpha = 1;
        }
 
        #endregion

        #region Pointer Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltipInfo();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltipInfo(TooltipManager.Instance);
        }
        #endregion

        #region Tooltips
        protected virtual void ShowTooltipInfo()
        {
            var tooltipManager = TooltipManager.Instance;
            var specialKeywords = new List<SpecialKeywords>();

            foreach (var statusIcon in StatusDict)
            {
                // Ignore inactive
                if (statusIcon.Value == null) continue;

                // Find keyword data
                var statusData = statusIcon.Value.MyStatusIconData;
                foreach (var statusDataSpecialKeyword in statusData.SpecialKeywords)
                {
                    if (specialKeywords.Contains(statusDataSpecialKeyword)) continue;
                    specialKeywords.Add(statusDataSpecialKeyword);
                }
            }

            foreach (var specialKeyword in specialKeywords)
            {
                var specialKeywordData = tooltipManager
                    .SpecialKeywordData.SpecialKeywordBaseList
                        .Find(x => x.SpecialKeyword == specialKeyword);

                if (specialKeywordData != null)
                    ShowTooltipInfo(tooltipManager, specialKeywordData.GetContent(), 
                        specialKeywordData.GetHeader(), descriptionRoot);
            }
        }

        public void ShowTooltipInfo(TooltipManager tooltipManager, 
            string content, string header = "", 
            Transform tooltipStaticTransform = null, Camera cam = null, float delayShow = 0)
        {
            tooltipManager.ShowTooltip(
                content, header, tooltipStaticTransform, cam, delayShow);
        }

        public void HideTooltipInfo(TooltipManager tooltipManager)
        {
            tooltipManager.HideTooltip();
        }
        #endregion
    }
}