using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

namespace ALWTTT.UI
{
    /// <summary>
    /// [FEEDBACK-2] Feedback panel inside the pause (D2=A). Text box, Send, Cancel, one privacy
    /// line, a status line (Enviando… / ¡Gracias! / No se pudo enviar) and, only after a
    /// failure, "Copiar" with the S9c fallback (D3=B).
    ///
    /// Built by ALWTTT ▸ FEEDBACK-2 ▸ Crear panel (D4=B) as a copy of the pause ConfirmPanel.
    ///
    /// Why the send survives the pause: Time.timeScale = 0 freezes WaitForSeconds, not a
    /// UnityWebRequest. The coroutine yields on the request itself, which completes on real
    /// time. It runs on PauseMenuController (always active), not on this panel, so closing the
    /// panel or resuming the gig mid-send does not kill it.
    ///
    /// The typed text is kept on Cancel/Esc and on failure; it is cleared only after Google
    /// confirms the response.
    /// </summary>
    public sealed class FeedbackPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField messageInput;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text privacyText;
        [SerializeField] private GameObject sendButton;
        [SerializeField] private GameObject copyButton;
        [SerializeField, Tooltip("Seconds before the request is abandoned.")]
        private int timeoutSeconds = 15;

        public const string PrivacyLine =
            "Se envía tu texto, la versión del juego, tu sistema y en qué gig estabas. Nada más.\n" +
            "We send your text, the game version, your OS and which gig you were in. Nothing else.";

        private const string Tag = FeedbackForm.Tag;
        private static FeedbackPanelController s_open;

        private Action _onClosed;
        private bool _sending;

        public bool IsOpen => s_open == this;

        private void Awake()
        {
            // TMP default restores the text from when the field was selected: Esc would wipe it.
            if (messageInput != null) messageInput.restoreOriginalTextOnEscape = false;
            if (privacyText != null) privacyText.text = PrivacyLine;
        }

        public void Open(Action onClosed)
        {
            _onClosed = onClosed;
            s_open = this;
            gameObject.SetActive(true);
            if (copyButton != null) copyButton.SetActive(false);
            SetStatus(_sending ? "Enviando… / Sending…" : "");
            if (messageInput != null)
            {
                messageInput.Select();
                messageInput.ActivateInputField();
            }
            Debug.Log($"{Tag} Panel abierto. Formulario: {(FeedbackForm.IsConfigured ? "configurado" : "SIN CONFIGURAR")}.");
        }

        /// <summary>invokeCallback=false for teardown (resume, scene change).</summary>
        public void Close(bool invokeCallback)
        {
            bool wasOpen = s_open == this;
            if (wasOpen) s_open = null;
            ReleaseFocus();
            gameObject.SetActive(false);
            var cb = _onClosed;
            _onClosed = null;
            if (wasOpen && invokeCallback) cb?.Invoke();
        }

        /// <summary>Esc layer (UIManager.HandleEscapeKey): closes the panel if open.</summary>
        public static bool TryCloseOpen()
        {
            if (s_open == null) return false;
            Debug.Log($"{Tag} Panel cerrado con Esc (el texto se conserva).");
            s_open.Close(true);
            return true;
        }

        private void OnDisable()
        {
            // Hidden by a parent (pause root hidden on resume / scene load).
            if (s_open == this) { s_open = null; _onClosed = null; }
        }

        #region Buttons (wired by the builder)
        public void OnCancelPressed()
        {
            Debug.Log($"{Tag} Cancelar (el texto se conserva).");
            Close(true);
        }

        public void OnSendPressed()
        {
            if (_sending) return;
            string message = messageInput != null ? messageInput.text.Trim() : "";
            if (message.Length == 0)
            {
                SetStatus("Escribe algo primero. / Write something first.");
                return;
            }

            var context = FeedbackForm.BuildContext();
            if (!FeedbackForm.IsConfigured)
            {
                Debug.LogWarning($"{Tag} Formulario sin configurar: {FeedbackForm.Describe()}");
                ShowFailure("formulario sin configurar");
                return;
            }

            MonoBehaviour host = PauseMenuController.Instance;
            if (host == null) host = this;
            host.StartCoroutine(Send(message, context));
        }

        public void OnCopyPressed()
        {
            string message = messageInput != null ? messageInput.text : "";
            GUIUtility.systemCopyBuffer = PauseMenuController.BuildMailFallback(message);
            SetStatus($"Copiado. Pégalo en un correo a {PauseMenuController.FeedbackAddress}\n" +
                      $"Copied. Paste it in an e-mail to {PauseMenuController.FeedbackAddress}");
            Debug.Log($"{Tag} Respaldo copiado al portapapeles ({message.Length} caracteres).");
        }
        #endregion

        private IEnumerator Send(string message, System.Collections.Generic.Dictionary<string, string> context)
        {
            _sending = true;
            if (sendButton != null) sendButton.SetActive(false);
            if (copyButton != null) copyButton.SetActive(false);
            SetStatus("Enviando… / Sending…");
            Debug.Log($"{Tag} Enviando {message.Length} caracteres. {FeedbackForm.ContextLine(context)}");

            float t0 = Time.realtimeSinceStartup;
            using (var req = UnityWebRequest.Post(FeedbackForm.PostUrl, FeedbackForm.BuildForm(message, context)))
            {
                req.timeout = timeoutSeconds;
                yield return req.SendWebRequest();

                int ms = Mathf.RoundToInt((Time.realtimeSinceStartup - t0) * 1000f);
                _sending = false;
                if (sendButton != null) sendButton.SetActive(true);

                if (req.result == UnityWebRequest.Result.Success && req.responseCode == 200)
                {
                    Debug.Log($"{Tag} Enviado OK (HTTP {req.responseCode}, {ms} ms).");
                    if (messageInput != null) messageInput.text = "";
                    SetStatus("¡Gracias! Lo recibimos. / Thanks! We got it.");
                }
                else
                {
                    Debug.LogWarning($"{Tag} Falló el envío: {req.result} HTTP {req.responseCode} " +
                                     $"\"{req.error}\" ({ms} ms). El texto se conserva.");
                    ShowFailure(string.IsNullOrEmpty(req.error) ? $"HTTP {req.responseCode}" : req.error);
                }
            }
        }

        private void ShowFailure(string why)
        {
            if (sendButton != null) sendButton.SetActive(true);
            if (copyButton != null) copyButton.SetActive(true);
            SetStatus($"No se pudo enviar ({why}). Tu texto sigue aquí: puedes reintentar o copiarlo.\n" +
                      "Couldn't send. Your text is still here: retry or copy it.");
        }

        private void SetStatus(string s)
        {
            if (statusText != null) statusText.text = s;
        }

        private void ReleaseFocus()
        {
            if (messageInput != null) messageInput.DeactivateInputField();
            var es = EventSystem.current;
            if (es != null) es.SetSelectedGameObject(null);
        }
    }
}
