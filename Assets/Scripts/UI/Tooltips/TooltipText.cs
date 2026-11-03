using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Tooltips
{
    public class TooltipText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private int characterWrapLimit = 50;

        public void SetText(string content = "", string header = "")
        {
            if (string.IsNullOrEmpty(header))
            {
                headerText.gameObject.SetActive(false);
            }
            else
            {
                headerText.gameObject.SetActive(true);
                headerText.text = header;
            }

            if (string.IsNullOrEmpty(content))
            {
                contentText.gameObject.SetActive(false);
            }
            else
            {
                contentText.gameObject.SetActive(true);
                contentText.text = content;
            }

            if (contentText.gameObject.activeSelf || headerText.gameObject.activeSelf)
            {
                backgroundImage.gameObject.SetActive(true);
            }
            else
            {
                backgroundImage.gameObject.SetActive(false);
            }

            PrepareLayout();
        }

        // [GIG-UI-1] Per-panel colours (TooltipStyleSO). The prefab colours are captured
        // once and restored when no style applies: panels are pooled and reused.
        private Color _defaultBackground, _defaultHeader, _defaultBody;
        private bool _defaultCached;

        public void ApplyStyle(TooltipStyleSO.PanelStyle style)
        {
            if (!_defaultCached)
            {
                if (backgroundImage != null) _defaultBackground = backgroundImage.color;
                _defaultHeader = headerText.color;
                _defaultBody = contentText.color;
                _defaultCached = true;
            }
            if (backgroundImage != null)
                backgroundImage.color = style != null ? style.background : _defaultBackground;
            headerText.color = style != null ? style.header : _defaultHeader;
            contentText.color = style != null ? style.body : _defaultBody;
        }

        private void PrepareLayout()
        {
            var longestTextLength = GetLongestTextLength();
            layoutElement.enabled = (longestTextLength > characterWrapLimit);
        }

        private int GetLongestTextLength()
        {
            if (headerText.text.Length > contentText.text.Length)
                return headerText.text.Length;

            if (headerText.text.Length <= contentText.text.Length)
                return contentText.text.Length;

            return 0;
        }
    }
}