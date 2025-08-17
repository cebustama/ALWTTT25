using ALWTTT.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Tooltips
{
    public class TooltipManager : MonoBehaviour
    {
        private const string DebugTag = "<color=green>[TooltipManager]</color>";


        public static TooltipManager Instance;

        [Header("References")]
        [SerializeField] private TooltipController tooltipController;
        //[SerializeField] private CursorController cursorController;
        [SerializeField] private TooltipText tooltipTextPrefab;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private SpecialKeywordData specialKeywordData;

        [Header("Settings")]
        [SerializeField] private AnimationCurve fadeCurve;
        [SerializeField] private float showDelayTime = 0.5f;

        public SpecialKeywordData SpecialKeywordData => specialKeywordData;

        private List<TooltipText> tooltipTextList = new List<TooltipText>();
        private TooltipController TooltipController => tooltipController;

        private int currentShownTooltipCount;

        private void Awake()
        {
            if (Instance == null)
            {
                transform.parent = null;
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void ShowTooltip(
            string contentText = "", string headerText = "", 
            Transform tooltipTargetTransform = null,
            Camera cam = null, float delayShow = 0)
        {
            Debug.Log($"{DebugTag} Showing tooltip for '{headerText}");

            StartCoroutine(ShowRoutine(delayShow));

            currentShownTooltipCount++;
            if (tooltipTextList.Count < currentShownTooltipCount)
            {
                var newTooltip = Instantiate(tooltipTextPrefab, TooltipController.transform);
                tooltipTextList.Add(newTooltip);
            }

            tooltipTextList[currentShownTooltipCount - 1].gameObject.SetActive(true);
            tooltipTextList[currentShownTooltipCount - 1].SetText(contentText, headerText);

            TooltipController.SetFollowPos(tooltipTargetTransform, cam);
        }

        public void HideTooltip()
        {
            StopAllCoroutines();
            currentShownTooltipCount = 0;
            canvasGroup.alpha = 0;
            foreach (var tooltipText in tooltipTextList)
                tooltipText.gameObject.SetActive(false);
        }

        private IEnumerator ShowRoutine(float delay = 0)
        {
            var waitFrame = new WaitForEndOfFrame();
            var timer = 0f;

            canvasGroup.alpha = 0;

            yield return new WaitForSeconds(delay);

            while (true)
            {
                timer += Time.deltaTime;

                var invValue = Mathf.InverseLerp(0, showDelayTime, timer);
                canvasGroup.alpha = fadeCurve.Evaluate(invValue);

                if (timer >= showDelayTime)
                {
                    canvasGroup.alpha = 1;
                    break;
                }
                yield return waitFrame;
            }
        }
    }
}