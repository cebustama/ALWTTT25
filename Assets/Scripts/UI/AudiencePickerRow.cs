using ALWTTT.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// <summary>
    /// Single row in the audience roster picker. M4.6-prep merged (1)/(4).
    /// </summary>
    public class AudiencePickerRow : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text typeLabel;

        public AudienceCharacterData Audience { get; private set; }
        public bool IsSelected => toggle != null && toggle.isOn;

        public event System.Action<AudiencePickerRow> OnSelectionChanged;

        public void Init(AudienceCharacterData audience, bool selected)
        {
            Audience = audience;

            if (nameLabel != null)
                nameLabel.text = audience != null ? audience.CharacterName : "<null audience>";
            if (typeLabel != null)
                typeLabel.text = audience != null
                    ? (audience.IsTall ? "tall" : "short")
                    : string.Empty;

            if (toggle != null)
            {
                toggle.SetIsOnWithoutNotify(selected);
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener(_ => OnSelectionChanged?.Invoke(this));
            }
        }

        public void SetSelected(bool selected)
        {
            if (toggle != null) toggle.SetIsOnWithoutNotify(selected);
        }
    }
}