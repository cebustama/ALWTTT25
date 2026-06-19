using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [S4 D-TUT-2 / D-TUT-11] Minimal, self-contained revisit + reset surface.
    ///
    /// Lists ONLY already-fired dialogs (revisit cannot re-trigger unseen ones) and
    /// replays a chosen one modally via TutorialController.ReplayDialog (no gate, no
    /// fired-state change). A reset button (behind a confirm step) clears the whole
    /// fired set so tutorials re-show on their next trigger.
    ///
    /// There is currently no pause/settings menu in the project to host this; this
    /// panel is standalone so it can be parented under one when it exists. Call
    /// <see cref="Open"/> from a future "Tutorials" button; the panel rebuilds its
    /// list each open.
    ///
    /// Scene wiring: listContent (a vertical layout group), entryTemplate (an
    /// inactive Button with a Text/TMP child), resetButton, confirmRoot
    /// (inactive group with confirmYesButton / confirmNoButton).
    /// </summary>
    public class TutorialRevisitPanel : MonoBehaviour
    {
        [SerializeField] private TutorialController controller;
        [SerializeField] private RectTransform listContent;
        [SerializeField] private Button entryTemplate;
        [SerializeField] private Button resetButton;

        [Header("Reset confirm")]
        [SerializeField] private GameObject confirmRoot;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        private void Awake()
        {
            if (entryTemplate != null) entryTemplate.gameObject.SetActive(false);
            if (confirmRoot != null) confirmRoot.SetActive(false);
            if (resetButton != null) resetButton.onClick.AddListener(ShowConfirm);
            if (confirmYesButton != null) confirmYesButton.onClick.AddListener(ConfirmReset);
            if (confirmNoButton != null) confirmNoButton.onClick.AddListener(HideConfirm);
        }

        private void OnDestroy()
        {
            if (resetButton != null) resetButton.onClick.RemoveListener(ShowConfirm);
            if (confirmYesButton != null) confirmYesButton.onClick.RemoveListener(ConfirmReset);
            if (confirmNoButton != null) confirmNoButton.onClick.RemoveListener(HideConfirm);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            HideConfirm();
            Rebuild();
        }

        public void Close() => gameObject.SetActive(false);

        private void Rebuild()
        {
            if (controller == null || listContent == null || entryTemplate == null) return;

            // Clear previous entries (everything except the inactive template).
            for (int i = listContent.childCount - 1; i >= 0; i--)
            {
                var child = listContent.GetChild(i);
                if (child == entryTemplate.transform) continue;
                Destroy(child.gameObject);
            }

            foreach (var dialog in controller.GetFiredDialogs())
            {
                if (dialog == null) continue;
                var entry = Instantiate(entryTemplate, listContent);
                entry.gameObject.SetActive(true);

                var label = entry.GetComponentInChildren<Text>();
                if (label != null) label.text = dialog.RevisitTitle;
                var tmp = entry.GetComponentInChildren<TMPro.TMP_Text>();
                if (tmp != null) tmp.text = dialog.RevisitTitle;

                var captured = dialog;
                entry.onClick.AddListener(() => controller.ReplayDialog(captured));
            }
        }

        private void ShowConfirm() { if (confirmRoot != null) confirmRoot.SetActive(true); }
        private void HideConfirm() { if (confirmRoot != null) confirmRoot.SetActive(false); }

        private void ConfirmReset()
        {
            controller?.ResetAllTutorials();
            HideConfirm();
            Rebuild();
        }
    }
}
