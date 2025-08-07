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

        public AudienceIntentionType IntentionType => intentionType;
        public Sprite IntentionSprite => intentionSprite;
    }
}
