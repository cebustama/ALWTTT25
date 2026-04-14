using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class StatusIconBase : MonoBehaviour
    {
        [SerializeField] private Image statusImage;
        [SerializeField] private TextMeshProUGUI statusValueText;

        public Sprite CurrentSprite { get; private set; }

        public Image StatusImage => statusImage;
        public TextMeshProUGUI StatusValueText => statusValueText;

        /// <summary>
        /// Assign the icon sprite for this status.
        /// Sprite is sourced from StatusEffectSO.IconSprite by CharacterCanvas.
        /// </summary>
        public void SetStatus(Sprite sprite)
        {
            CurrentSprite = sprite;
            if (statusImage != null)
                statusImage.sprite = sprite;
        }

        public void SetStatusValue(int statusValue)
        {
            if (statusValueText != null)
                statusValueText.text = statusValue.ToString();
        }
    }
}