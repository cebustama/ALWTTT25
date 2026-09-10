using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class HealthBarController : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private CanvasGroup canvasGroup;

        // [BIGNUM-1r / D-BN-14=A] Predicted-loss "ghost" segment. A plain Image (NOT the
        // slider's fill) parented under the slider's Fill Area, stretched by anchors to
        // cover [current − predictedLoss, current] of the bar. The real fill keeps
        // telling the truth (current); the ghost says "this much is about to go".
        // At song end GigManager clears it right before ApplyIncomingVibe, so the real
        // bar lerps down through where the ghost was: prediction becomes payment on
        // screen. Null-guarded: an unwired prefab degrades to "no ghost".
        [Header("Predicted segment (BIGNUM-1r)")]
        [SerializeField] private RectTransform predictedFill;

        // [BIGNUM-1r2 / ST-BN-12] Probe. The ghost has exactly three ways to stay
        // invisible — unwired slot, loss == 0 (nobody called SetPredictedLoss with a
        // real number), or a zero-size parent — and from the outside they look the
        // same. This says which one it is. Turn OFF once ST-BN-12 is green.
        [SerializeField] private bool logPredicted = true;

        private Coroutine lerpRoutine;

        public CanvasGroup CanvasGroup => canvasGroup;

        private int currentValue = 0;
        public int CurrentValue { get { return currentValue; } }

        private int _maxValue = 1;
        private int _predictedLoss;
        private bool _warnedUnwired;
        private int _logCount;

        public void SetCurrentValue(int current, int max, float duration)
        {
            slider.minValue = 0f;
            slider.maxValue = Mathf.Max(1, max);
            _maxValue = Mathf.Max(1, max);

            currentValue = current;
            float target = Mathf.Clamp(current, 0, max);

            if (lerpRoutine != null) StopCoroutine(lerpRoutine);

            // [BIGNUM-1r] Re-lay the ghost against the NEW truth (mid-song card Vibe
            // moves the bar without a projection refresh; the ghost must follow).
            LayoutPredicted();

            if (duration <= 0f || !gameObject.activeInHierarchy)
            {
                slider.value = target;
                return;
            }

            lerpRoutine = StartCoroutine(LerpValue(target, duration));
        }

        /// <summary>
        /// [BIGNUM-1r / D-BN-14=A] Show <paramref name="loss"/> units of predicted loss
        /// as a ghost segment at the top end of the current fill. 0 hides it.
        /// </summary>
        public void SetPredictedLoss(int loss)
        {
            _predictedLoss = Mathf.Max(0, loss);

            if (logPredicted && _logCount < 12)
            {
                _logCount++;
                Debug.Log($"[VibeGhost] SetPredictedLoss({loss}) on '{name}' " +
                          $"current={currentValue}/{_maxValue} " +
                          $"predictedFill={(predictedFill ? predictedFill.name : "NULL — slot not wired")}",
                          this);
            }

            LayoutPredicted();
        }

        private void LayoutPredicted()
        {
            if (predictedFill == null)
            {
                if (logPredicted && !_warnedUnwired && _predictedLoss > 0)
                {
                    _warnedUnwired = true;
                    Debug.LogWarning($"[VibeGhost] '{name}': Predicted Fill slot is EMPTY on " +
                        "HealthBarController — assign the PredictedFill RectTransform in the " +
                        "audience prefab (BIGNUM-1r step 9.2).", this);
                }
                return;
            }

            int cur = Mathf.Clamp(currentValue, 0, _maxValue);
            int loss = Mathf.Min(_predictedLoss, cur);
            if (loss <= 0)
            {
                predictedFill.gameObject.SetActive(false);
                return;
            }

            float xMax = (float)cur / _maxValue;
            float xMin = (float)(cur - loss) / _maxValue;

            predictedFill.anchorMin = new Vector2(xMin, 0f);
            predictedFill.anchorMax = new Vector2(xMax, 1f);
            predictedFill.offsetMin = Vector2.zero;
            predictedFill.offsetMax = Vector2.zero;
            predictedFill.localScale = Vector3.one;   // a duplicated Fill can carry a stale scale
            predictedFill.gameObject.SetActive(true);

            if (logPredicted && _logCount < 12)
                Debug.Log($"[VibeGhost] LAYOUT '{name}' loss={loss} x=[{xMin:0.00},{xMax:0.00}] " +
                          $"rect={predictedFill.rect.width:0.0}x{predictedFill.rect.height:0.0} " +
                          $"parent={(predictedFill.parent ? predictedFill.parent.name : "none")} " +
                          $"activeInHierarchy={predictedFill.gameObject.activeInHierarchy}", this);
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