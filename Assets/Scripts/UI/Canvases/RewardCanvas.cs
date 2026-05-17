using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Extentions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class RewardCanvas : CanvasBase
    {
        [Header("References")]
        [SerializeField] private RewardContainerData rewardContainerData;
        [SerializeField] private Transform rewardRoot;
        [SerializeField] private RewardContainer rewardContainerPrefab;
        [SerializeField] private Transform rewardPanelRoot;
        [SerializeField] private Button rewardButton;

        [Header("Choice")]
        [SerializeField] private Transform choiceCardSpawnRoot;
        [SerializeField] private ChoiceCard choiceCardUIPrefab;
        [SerializeField] private ChoicePanel choicePanel;

        private readonly List<RewardContainer> currentRewardsList = 
            new List<RewardContainer>();
        private readonly List<ChoiceCard> spawnedChoiceList = 
            new List<ChoiceCard>();
        private readonly List<CardDefinition> cardRewardList = 
            new List<CardDefinition>();

        public ChoicePanel ChoicePanel => choicePanel;

        public System.Action OnRewardFinished; // set by GigManager

        public void PrepareCanvas()
        {
            rewardButton.onClick.AddListener(FinishReward);
            rewardPanelRoot.gameObject.SetActive(true);
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

        private void GetFansReward(int amount)
        {

        }

        private void FinishReward()
        {
            gameObject.SetActive(false);
            OnRewardFinished?.Invoke();
        }

        public void BuildReward(RewardType rewardType)
        {
            Debug.Log("Building Reward...");

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

                    // [B3-demo-polish / D-UX1=C] If no card rewards are available,
                    // skip the reward UI entirely. Destroys the spawned container,
                    // closes the canvas, and invokes OnRewardFinished so post-gig
                    // flow proceeds normally.
                    if (cardRewardList.Count == 0)
                    {
                        Debug.LogWarning(
                            "[RewardCanvas] No card rewards available; " +
                            "skipping reward step entirely.");
                        Destroy(rewardClone.gameObject);
                        currentRewardsList.Remove(rewardClone);
                        FinishReward();
                        return;
                    }

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

        private void GetCardReward(RewardContainer rewardContainer, int amount = 3)
        {
            // [B3-demo-polish / D-UX1=C] Defensive guard. Belt-and-suspenders for
            // the case where cardRewardList becomes empty between BuildReward and
            // click (or starts empty if a future caller skips the BuildReward guard).
            if (cardRewardList == null || cardRewardList.Count == 0)
            {
                Debug.LogWarning(
                    "[RewardCanvas] cardRewardList empty at click; " +
                    "closing reward step gracefully.");
                Destroy(rewardContainer.gameObject);
                currentRewardsList.Remove(rewardContainer);
                FinishReward();
                return;
            }

            // Clamp requested amount to available pool. Fixes the original crash
            // where 1 card was present but 3 were requested.
            int actualAmount = Mathf.Min(amount, cardRewardList.Count);

            ChoicePanel.gameObject.SetActive(true);

            for (int i = 0; i < actualAmount; i++)
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