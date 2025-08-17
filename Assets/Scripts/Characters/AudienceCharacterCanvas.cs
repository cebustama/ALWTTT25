using ALWTTT.Data;
using ALWTTT.Tooltips;
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

        public AudienceAbilityData NextAbility;
        private AudienceIntentionData CurrentIntention => NextAbility.Intention;

        protected override void ShowTooltipInfo()
        {
            base.ShowTooltipInfo();

            if (NextAbility != null && CurrentIntention != null)
            {
                var abilityName = NextAbility.AbilityName;
                var contentText = CurrentIntention.ContentText;

                ShowTooltipInfo(
                    TooltipManager.Instance, contentText, abilityName, descriptionRoot);
            }
        }
    }
}