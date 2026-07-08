using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "Reward Data", menuName = "ALWTTT/Rewards/Generic")]
    public class RewardDatabase : ScriptableObject
    {
        [SerializeField] private Sprite rewardSprite;
        [TextArea][SerializeField] private string rewardDescription;
        public Sprite RewardSprite => rewardSprite;
        public string RewardDescription => rewardDescription;
    }
}