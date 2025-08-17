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
        [SerializeField] protected TextMeshProUGUI currentHealthText;
        [SerializeField] protected Slider currentHealthBar;
        [SerializeField] protected TextMeshProUGUI characterNameText;
        [SerializeField] protected Transform highlightRoot;
        [SerializeField] protected HealthBarController healthBar;
        [SerializeField] protected StatusIconsData statusIconsData;
        [SerializeField] protected Transform statusIconRoot;

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
            healthBar?.SetCurrentVibe(current, max, duration);
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

        #endregion

        #region Pointer Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            //ShowTooltipInfo();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //HideTooltipInfo();
        }
        #endregion

        #region Tooltips
        public void ShowTooltipInfo(TooltipManager tooltipManager, 
            string content, string header = "", 
            Transform tooltipStaticTransform = null, Camera cam = null, float delayShow = 0)
        {
            throw new NotImplementedException();
        }

        public void HideTooltipInfo(TooltipManager tooltipManager)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}