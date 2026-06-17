using UnityEngine;

namespace ALWTTT.Data
{
    public class RewardDatabase : ScriptableObject
    {
        [SerializeField] private Sprite rewardSprite;
        [TextArea][SerializeField] private string rewardDescription;
        public Sprite RewardSprite => rewardSprite;
        public string RewardDescription => rewardDescription;
    }
}