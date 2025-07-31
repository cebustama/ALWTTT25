using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Characters
{
    public class AudienceCharacterCanvas : CharacterCanvas
    {
        [Header("Audience Character Canvas Settings")]
        [SerializeField] private Image intentImage;
        [SerializeField] private TextMeshProUGUI nextActionValueText;
        public Image IntentImage => intentImage;
        public TextMeshProUGUI NextActionValueText => nextActionValueText;
    }
}