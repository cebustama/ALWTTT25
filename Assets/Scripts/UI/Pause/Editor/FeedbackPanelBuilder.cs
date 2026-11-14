using ALWTTT.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [FEEDBACK-2 / D4=B] Builds the feedback panel inside UIManager.prefab ▸ PauseCanvas ▸
    /// Root as a copy of ConfirmPanel (same frame, font and buttons), wires it to
    /// PauseMenuController and turns FeedbackButton back on. Re-running it replaces the panel.
    ///
    /// Every button copied from ConfirmPanel loses its old OnClick (it pointed at
    /// OnConfirmYesPressed: a copied Send would have quit the gig).
    /// </summary>
    public static class FeedbackPanelBuilder
    {
        private const string PrefabPath = "Assets/Prefabs/Core/UIManager.prefab";
        private const string Tag = "[FEEDBACK-2]";

        // Look (second pass, after the first smoke: title overflowed, panel translucent).
        private const float TitleSize = 46f, InputSize = 28f, SmallSize = 22f, StatusSize = 24f;
        private static readonly Color PanelColor = new Color(0.09f, 0.06f, 0.16f, 0.97f);
        private static readonly Color TextColor = new Color(0.97f, 0.95f, 0.90f);
        private static readonly Color MutedColor = new Color(0.97f, 0.95f, 0.90f, 0.65f);
        private static readonly Color AccentColor = new Color(1f, 0.82f, 0.35f);
        private static readonly Color InputBg = new Color(0.16f, 0.12f, 0.26f, 1f);

        [MenuItem("ALWTTT/FEEDBACK-2/Crear panel de feedback en la pausa")]
        public static void Build()
        {
            var contents = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var pauseCanvas = FindDeep(contents.transform, "PauseCanvas");
                var pause = pauseCanvas != null ? pauseCanvas.GetComponent<PauseMenuController>() : null;
                var root = pauseCanvas != null ? pauseCanvas.Find("Root") : null;
                var confirm = root != null ? root.Find("ConfirmPanel") : null;
                var feedbackButton = root != null ? FindDeep(root, "FeedbackButton") : null;
                if (pause == null || confirm == null || feedbackButton == null)
                {
                    Debug.LogError($"{Tag} No encuentro PauseCanvas/Root/ConfirmPanel/FeedbackButton en {PrefabPath}.");
                    return;
                }

                var old = root.Find("FeedbackPanel");
                if (old != null)
                {
                    if (!EditorUtility.DisplayDialog("FEEDBACK-2", "Ya existe FeedbackPanel. ¿Reemplazarlo?", "Reemplazar", "Cancelar"))
                        return;
                    Object.DestroyImmediate(old.gameObject);
                }

                // 1. Copy of ConfirmPanel, bigger, with a vertical layout.
                var panelGo = Object.Instantiate(confirm.gameObject, root);
                panelGo.name = "FeedbackPanel";
                panelGo.SetActive(false);
                var panelRt = (RectTransform)panelGo.transform;
                panelRt.anchorMin = panelRt.anchorMax = panelRt.pivot = new Vector2(0.5f, 0.5f);
                panelRt.anchoredPosition = Vector2.zero;
                panelRt.sizeDelta = new Vector2(1100f, 780f);
                // Opaque night-purple frame: the copied ConfirmPanel is translucent and the gig
                // art behind made the text unreadable.
                var panelImg = panelGo.GetComponent<Image>();
                if (panelImg != null) panelImg.color = PanelColor;
                foreach (var fitter in panelGo.GetComponents<ContentSizeFitter>()) Object.DestroyImmediate(fitter);
                // No ?? with Unity objects: a missing component is a fake null in the editor.
                var vlg = panelGo.GetComponent<VerticalLayoutGroup>();
                if (vlg == null) vlg = panelGo.AddComponent<VerticalLayoutGroup>();
                vlg.padding = new RectOffset(48, 48, 40, 40);
                vlg.spacing = 18f;
                vlg.childAlignment = TextAnchor.UpperCenter;
                vlg.childControlWidth = vlg.childControlHeight = true;
                vlg.childForceExpandWidth = true;
                vlg.childForceExpandHeight = false;

                // 2. Title (was ConfirmText).
                var title = panelGo.transform.Find("ConfirmText").GetComponent<TMP_Text>();
                title.gameObject.name = "TitleText";
                title.text = "¿Qué pasó? · What happened?";
                title.enableAutoSizing = false;
                title.fontSize = TitleSize;
                title.color = TextColor;
                title.alignment = TextAlignmentOptions.Center;
                SetHeight(title.gameObject, 64f, 0f);

                // 3. Message box.
                var input = CreateInput(title);
                input.transform.SetParent(panelGo.transform, false);
                input.transform.SetSiblingIndex(1);
                SetHeight(input.gameObject, 300f, 1f);

                // 4. Privacy and status lines (copies of the title, smaller).
                var privacy = CloneText(title, panelGo.transform, "PrivacyText", SmallSize, 2);
                privacy.color = MutedColor;
                privacy.fontStyle = FontStyles.Italic;
                privacy.text = FeedbackPanelController.PrivacyLine;
                SetHeight(privacy.gameObject, 70f, 0f);
                var status = CloneText(title, panelGo.transform, "StatusText", StatusSize, 3);
                status.color = AccentColor;
                status.text = "";
                SetHeight(status.gameObject, 70f, 0f);

                // 5. Buttons: Send (was Confirm), Copy (copy of Cancel, hidden), Cancel.
                var buttons = panelGo.transform.Find("ButtonsLayout");
                buttons.SetAsLastSibling();
                SetHeight(buttons.gameObject, 90f, 0f);
                var send = buttons.Find("ConfirmButton").GetComponent<Button>();
                send.gameObject.name = "SendButton";
                var cancel = buttons.Find("CancelButton").GetComponent<Button>();
                var copy = Object.Instantiate(cancel.gameObject, buttons).GetComponent<Button>();
                copy.gameObject.name = "CopyButton";
                copy.transform.SetSiblingIndex(1);
                copy.gameObject.SetActive(false);
                SetLabel(send, "Enviar / Send");
                SetLabel(copy, "Copiar / Copy");
                SetLabel(cancel, "Cancelar / Cancel");

                // 6. Controller and wiring.
                var ctrl = panelGo.AddComponent<FeedbackPanelController>();
                var so = new SerializedObject(ctrl);
                so.FindProperty("messageInput").objectReferenceValue = input;
                so.FindProperty("statusText").objectReferenceValue = status;
                so.FindProperty("privacyText").objectReferenceValue = privacy;
                so.FindProperty("sendButton").objectReferenceValue = send.gameObject;
                so.FindProperty("copyButton").objectReferenceValue = copy.gameObject;
                so.ApplyModifiedPropertiesWithoutUndo();
                Rewire(send.onClick, ctrl.OnSendPressed);
                Rewire(copy.onClick, ctrl.OnCopyPressed);
                Rewire(cancel.onClick, ctrl.OnCancelPressed);

                var pso = new SerializedObject(pause);
                pso.FindProperty("feedbackPanel").objectReferenceValue = ctrl;
                pso.ApplyModifiedPropertiesWithoutUndo();

                // 7. The pause button comes back, pointing at the panel.
                feedbackButton.gameObject.SetActive(true);
                Rewire(feedbackButton.GetComponent<Button>().onClick, pause.OnFeedbackPressed);

                PrefabUtility.SaveAsPrefabAsset(contents, PrefabPath);
                Debug.Log($"{Tag} Panel creado en {PrefabPath} (PauseCanvas/Root/FeedbackPanel) y FeedbackButton activado. " +
                          $"Formulario: {FeedbackForm.Describe()}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        [MenuItem("ALWTTT/FEEDBACK-2/Comprobar formulario")]
        public static void Check()
        {
            Debug.Log($"{Tag} Formulario: {FeedbackForm.Describe()}");
        }

        private static TMP_InputField CreateInput(TMP_Text style)
        {
            var go = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
            go.name = "MessageInput";
            go.layer = style.gameObject.layer;
            var field = go.GetComponent<TMP_InputField>();
            field.lineType = TMP_InputField.LineType.MultiLineNewline;
            field.characterLimit = 2000;
            var area = go.transform.Find("Text Area") as RectTransform;
            if (area != null) { area.offsetMin = new Vector2(18f, 14f); area.offsetMax = new Vector2(-18f, -14f); }
            field.restoreOriginalTextOnEscape = false;
            float size = InputSize;
            field.pointSize = size;
            foreach (var t in go.GetComponentsInChildren<TMP_Text>(true))
            {
                t.font = style.font;
                t.fontSize = size;
                t.alignment = TextAlignmentOptions.TopLeft;
                t.textWrappingMode = TextWrappingModes.Normal;
            }
            var bg = go.GetComponent<Image>();
            if (bg != null) bg.color = InputBg;
            if (field.textComponent != null) field.textComponent.color = TextColor;
            field.caretColor = TextColor;
            field.customCaretColor = true;
            field.selectionColor = new Color(1f, 0.82f, 0.35f, 0.35f);
            if (field.placeholder is TMP_Text ph0) ph0.color = new Color(0.97f, 0.95f, 0.90f, 0.35f);
            if (field.placeholder is TMP_Text ph)
                ph.text = "Escribe aquí: qué hiciste, qué esperabas, qué pasó… / Write here…";
            return field;
        }

        private static TMP_Text CloneText(TMP_Text src, Transform parent, string name, float size, int index)
        {
            var go = Object.Instantiate(src.gameObject, parent);
            go.name = name;
            go.transform.SetSiblingIndex(index);
            var t = go.GetComponent<TMP_Text>();
            t.fontSize = size;
            t.enableAutoSizing = false;
            return t;
        }

        private static void SetHeight(GameObject go, float preferred, float flexible)
        {
            var le = go.GetComponent<LayoutElement>();
            if (le == null) le = go.AddComponent<LayoutElement>();
            le.preferredHeight = preferred;
            le.minHeight = preferred;
            le.flexibleHeight = flexible;
        }

        private static void SetLabel(Button b, string text)
        {
            var label = b.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = text;
        }

        private static void Rewire(UnityEvent evt, UnityAction action)
        {
            for (int i = evt.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(evt, i);
            UnityEventTools.AddPersistentListener(evt, action);
        }

        private static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t)
            {
                var r = FindDeep(c, name);
                if (r != null) return r;
            }
            return null;
        }
    }
}
