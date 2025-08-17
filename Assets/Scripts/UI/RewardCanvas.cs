using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Extentions;
using AWLTTT.Cards;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.UI
{
    public class RewardCanvas : CanvasBase
    {
        [Header("References")]
        [SerializeField] private RewardContainerData rewardContainerData;
        [SerializeField] private Transform rewardRoot;
        [SerializeField] private RewardContainer rewardContainerPrefab;
        [SerializeField] private Transform rewardPanelRoot;

        [Header("Choice")]
        [SerializeField] private Transform choiceCardSpawnRoot;
        [SerializeField] private ChoiceCard choiceCardUIPrefab;
        [SerializeField] private ChoicePanel choicePanel;

        private readonly List<RewardContainer> currentRewardsList = 
            new List<RewardContainer>();
        private readonly List<ChoiceCard> spawnedChoiceList = 
            new List<ChoiceCard>();
        private readonly List<CardData> cardRewardList = 
            new List<CardData>();

        public ChoicePanel ChoicePanel => choicePanel;

        public void PrepareCanvas()
        {
            rewardPanelRoot.gameObject.SetActive(true);
        }

        public void BuildReward(RewardType rewardType)
        {
            var rewardClone = Instantiate(rewardContainerPrefab, rewardRoot);
            currentRewardsList.Add(rewardClone);

            switch (rewardType)
            {
                case RewardType.Card:

                    var rewardCardList = 
                        rewardContainerData.GetRandomCardRewardList(out var cardRewardData);
                    cardRewardList.Clear();

                    foreach (var cardData in rewardCardList)
                        cardRewardList.Add(cardData);

                    rewardClone.BuildReward(
                        cardRewardData.RewardSprite, cardRewardData.RewardDescription);
                    rewardClone.RewardButton.onClick
                        .AddListener(() => GetCardReward(rewardClone, 3));

                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(rewardType), rewardType, null);
            }
        }

        public override void ResetCanvas()
        {
            ResetRewards();

            ResetChoice();
        }

        private void ResetRewards()
        {
            foreach (var rewardContainer in currentRewardsList)
                Destroy(rewardContainer.gameObject);

            currentRewardsList?.Clear();
        }

        private void ResetChoice()
        {
            foreach (var choice in spawnedChoiceList)
            {
                Destroy(choice.gameObject);
            }

            spawnedChoiceList?.Clear();
            ChoicePanel.DisablePanel();
        }

        private void GetCardReward(RewardContainer rewardContainer, int amount = 3)
        {
            ChoicePanel.gameObject.SetActive(true);

            for (int i = 0; i < amount; i++)
            {
                Transform spawnTransform = choiceCardSpawnRoot;

                var choice = Instantiate(choiceCardUIPrefab, spawnTransform);

                var reward = cardRewardList.RandomItem();
                choice.BuildReward(reward);
                choice.OnCardChose += ResetChoice;

                cardRewardList.Remove(reward);
                spawnedChoiceList.Add(choice);
                currentRewardsList.Remove(rewardContainer);
            }

            Destroy(rewardContainer.gameObject);
        }
    }
}