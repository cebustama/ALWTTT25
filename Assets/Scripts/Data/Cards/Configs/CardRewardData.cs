using ALWTTT.Cards;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "Card Reward Data", menuName = "ALWTTT/Rewards/CardRW")]
    public class CardRewardData : RewardDatabase
    {
        [SerializeField] private List<CardData> rewardCardList;
        public List<CardData> RewardCardList => rewardCardList;
    }
}