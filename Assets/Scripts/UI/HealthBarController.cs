using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class HealthBarController : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private CanvasGroup canvasGroup;
        private Coroutine lerpRoutine;

        public CanvasGroup CanvasGroup => canvasGroup;

        private int currentValue = 0;
        public int CurrentValue { get { return currentValue; } }

        public void SetCurrentValue(int current, int max, float duration)
        {
            slider.minValue = 0f;
            slider.maxValue = Mathf.Max(1, max);

            currentValue = current;
            float target = Mathf.Clamp(current, 0, max);

            if (lerpRoutine != null) StopCoroutine(lerpRoutine);

            if (duration <= 0f || !gameObject.activeInHierarchy)
            {
                slider.value = target;
                return;
            }

            lerpRoutine = StartCoroutine(LerpValue(target, duration));
        }

        private IEnumerator LerpValue(float target, float duration)
        {
            float start = slider.value;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                slider.value = Mathf.Lerp(start, target, t);
                yield return null;
            }

            slider.value = target;
            lerpRoutine = null;
        }
    }
}