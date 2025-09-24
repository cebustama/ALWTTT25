using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ALWTTT.Data;

namespace ALWTTT.UI
{
    public class NewSongPanelUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Button confirmButton;

        private Action _onClose;

        public void Show(
            SongData song, string[] musicianNames, float approxSeconds, Action onClose)
        {
            gameObject.SetActive(true);
            _onClose = onClose;

            if (titleText) titleText.text = "New Song Composed!";
            if (bodyText)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Title: {song.SongTitle}");
                sb.AppendLine($"BPM: {song.BPM}");
                if (approxSeconds > 0f)
                    sb.AppendLine($"Duration: {approxSeconds:0.0}s");
                if (musicianNames != null && musicianNames.Length > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Band:");
                    foreach (var n in musicianNames) sb.AppendLine($"• {n}");
                }
                bodyText.text = sb.ToString();
            }

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                Hide();
                _onClose?.Invoke();
            });
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onClose = null;
        }
    }
}
