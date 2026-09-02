// Placement: Assets/Scripts/UI/GigMessageUI.cs
//
// [R6 / D-R6-7] Minimal player-facing message line for the gig HUD.
//
// WHY A STATIC ACCESSOR AND NOT AN INJECTED REFERENCE: the callers are a
// coroutine-free static local function inside CompositionSession and three
// early-return gates in GigManager. Threading a reference through both would
// touch ICompositionContext (a governed seam) for a temporary UI affordance.
// The static is deliberately fail-soft: with no instance in the scene, Show()
// is a no-op and nothing breaks.
//
// THIS IS A STOPGAP. It communicates the reason; it does not design the
// feedback. A real solution (position, animation, severity, queueing) is a
// separate UI batch.

using System.Collections;
using TMPro;
using UnityEngine;

namespace ALWTTT.UI
{
    public class GigMessageUI : MonoBehaviour
    {
        [Header("Wiring")]
        [Tooltip("The label that shows the message. Its GameObject is toggled.")]
        [SerializeField] private TMP_Text label;

        [Tooltip("Optional container toggled instead of the label itself " +
                 "(use when the message sits on a panel/backdrop).")]
        [SerializeField] private GameObject container;

        [Header("Behaviour")]
        [Tooltip("Seconds the message stays on screen before hiding. " +
                 "Set 0 to keep it until the next message or an explicit Hide().")]
        [SerializeField] private float defaultSeconds = 5f;

        private static GigMessageUI _instance;
        private Coroutine _hideRoutine;

        private void Awake()
        {
            // Last one wins rather than first: a scene reload must not leave a
            // destroyed instance answering Show().
            _instance = this;
            HideNow();
        }

        private void OnDestroy() { if (_instance == this) _instance = null; }

        /// <summary>Show a message. seconds &lt; 0 uses the authored default.</summary>
        public static void Show(string message, float seconds = -1f)
        {
            if (_instance == null || string.IsNullOrWhiteSpace(message)) return;
            _instance.ShowInstance(message, seconds < 0f ? _instance.defaultSeconds : seconds);
        }

        public static void Hide() { if (_instance != null) _instance.HideNow(); }

        private void ShowInstance(string message, float seconds)
        {
            if (label != null) label.text = message;
            SetVisible(true);

            if (_hideRoutine != null) StopCoroutine(_hideRoutine);
            _hideRoutine = seconds > 0f ? StartCoroutine(HideAfter(seconds)) : null;
        }

        private IEnumerator HideAfter(float seconds)
        {
            // Unscaled: a paused or slowed gig must not freeze the message.
            yield return new WaitForSecondsRealtime(seconds);
            HideNow();
        }

        private void HideNow()
        {
            if (_hideRoutine != null) { StopCoroutine(_hideRoutine); _hideRoutine = null; }
            SetVisible(false);
        }

        private void SetVisible(bool on)
        {
            if (container != null) container.SetActive(on);
            else if (label != null) label.gameObject.SetActive(on);
        }
    }
}