// Place at: Assets/Scripts/Tutorial/TutorialOptInPrompt.cs
using System;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [DEMO-FIXES-A / DEMO-TUT-TOGGLE / D-DF-1=A] Gig-open modal: "play with
    /// tutorial?". GigManager.Start defers StartGig() until answered, so the
    /// choice lands BEFORE the initial draw (beat-2 forced hand) and before
    /// GigStartedEvent. Shown on every gig open, including Retry's scene
    /// reload (D-DF-2=A); the previous answer (PersistentGameplayData.
    /// TutorialEnabled) is only the visual default. One-shot per gig
    /// (D-DF-3=A): no mid-gig re-arm; enabling later takes effect on the
    /// next gig open, where GigStartedEvent re-anchors beats 1-2.
    ///
    /// Wiring: own screen-space canvas, active at scene load (GigCanvas is
    /// still inactive at this point — it activates inside StartGig). The
    /// panelRoot starts INACTIVE; Show() raises it.
    /// </summary>
    public class TutorialOptInPrompt : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField, Tooltip("Panel shown while the prompt is up. Starts inactive.")]
        private GameObject panelRoot;

        [SerializeField] private Button withTutorialButton;
        [SerializeField] private Button withoutTutorialButton;

        [SerializeField, Tooltip("Optional highlight frame re-parented onto the " +
            "default button (last answer, D-DF-2=A). Null = no default marker.")]
        private GameObject defaultMarker;

        private Action<bool> _onAnswered;
        private bool _answered;

        /// <summary>All mandatory refs assigned. GigManager treats an unwired
        /// prompt as absent and starts the gig immediately (dev scenes).</summary>
        public bool IsWired =>
            panelRoot != null && withTutorialButton != null && withoutTutorialButton != null;

        public bool IsShowing => panelRoot != null && panelRoot.activeSelf;

        public void Show(bool defaultEnabled, Action<bool> onAnswered)
        {
            _onAnswered = onAnswered;
            _answered = false;

            withTutorialButton.onClick.RemoveListener(OnYes);
            withTutorialButton.onClick.AddListener(OnYes);
            withoutTutorialButton.onClick.RemoveListener(OnNo);
            withoutTutorialButton.onClick.AddListener(OnNo);

            if (defaultMarker != null)
            {
                var host = defaultEnabled ? withTutorialButton : withoutTutorialButton;
                defaultMarker.transform.SetParent(host.transform, false);
                defaultMarker.SetActive(true);
            }

            panelRoot.SetActive(true);
        }

        private void OnYes() => Answer(true);
        private void OnNo() => Answer(false);

        private void Answer(bool enabled)
        {
            if (_answered) return; // double-click guard
            _answered = true;
            panelRoot.SetActive(false);

            var cb = _onAnswered;
            _onAnswered = null;
            cb?.Invoke(enabled);
        }
    }
}