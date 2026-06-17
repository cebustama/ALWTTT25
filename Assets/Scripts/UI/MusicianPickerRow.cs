using ALWTTT.Characters.Band;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// <summary>
    /// Single row in the band roster picker. M4.6-prep merged (1)/(4).
    /// </summary>
    public class MusicianPickerRow : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text idLabel;

        public MusicianBase Musician { get; private set; }
        public bool IsSelected => toggle != null && toggle.isOn;

        public event System.Action<MusicianPickerRow> OnSelectionChanged;

        public void Init(MusicianBase musician, bool selected)
        {
            Musician = musician;

            var data = musician != null ? musician.MusicianCharacterData : null;
            if (nameLabel != null)
                nameLabel.text = data != null ? data.CharacterName : "<null musician>";
            if (idLabel != null)
                idLabel.text = data != null ? $"id:{data.CharacterId}" : string.Empty;

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