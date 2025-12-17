using ALWTTT.Cards;
using ALWTTT.Extentions;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "Reward Container", 
        menuName = "ALWTTT/Containers/Reward")]
    public class RewardContainerData : ScriptableObject
    {
        [SerializeField] private List<CardRewardData> cardRewardDataList;

        public List<CardRewardData> CardRewardDataList => cardRewardDataList;

        public List<CardDefinition> GetRandomCardRewardList(out CardRewardData rewardData)
        {
            rewardData = CardRewardDataList.RandomItem();

            List<CardDefinition> cardList = new List<CardDefinition>();

            foreach (var cardData in rewardData.RewardCardList)
                cardList.Add(cardData);

            return cardList;
        }
    }
}