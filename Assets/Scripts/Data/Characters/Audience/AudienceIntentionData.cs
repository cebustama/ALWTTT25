using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "AudienceIntentionData", 
        menuName = "ALWTTT/Characters/AudienceIntentionData")]
    public class AudienceIntentionData : ScriptableObject
    {
        [SerializeField] private AudienceIntentionType intentionType;
        [SerializeField] private Sprite intentionSprite;
        [Tooltip("[TEXT-5 / D-T5-1=A] DEVELOPER LABEL. Since TEXT-5 the intention body a player reads is the " +
         "glossary description of the ability (ConceptGlossarySO.entries, keyed by AudienceAbilityData." +
         "abilityId), one per language. No player surface reads this field; it is the seed source for the " +
         "EN column and an editor aid. Same criterion as CardDefinition.displayName (TXT-4).")]
        [SerializeField][TextArea] private string contentText;

        public AudienceIntentionType IntentionType => intentionType;
        public Sprite IntentionSprite => intentionSprite;
        public string ContentText => contentText;
    }
}
