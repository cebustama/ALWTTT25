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
        [Header("Presentation (S5h)")]
        [SerializeField, Tooltip("Sprite + description for the card-reward box. " +
            "Card content is flag-sourced (D2=B); any RewardCardList on the asset " +
            "is unused.")]
        private RewardDatabase cardRewardPresentation;

        [SerializeField, Tooltip("Sprite + description for the SFX-unlock box.")]
        private RewardDatabase sfxRewardPresentation;
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
            // [S5h] UIManager (and this canvas) is DontDestroyOnLoad: PrepareCanvas
            // runs once per win, including across Retry reloads. Clear leftovers
            // from a previous reward pass and avoid stacked button listeners.
            ResetCanvas();
            rewardButton.onClick.RemoveAllListeners();
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
            var pd = GameManager != null
                ? GameManager.PersistentGameplayData : null;
            if (pd == null)
            {
                Debug.LogWarning($"[RewardCanvas] No PersistentGameplayData; skipping {rewardType}.");
                return;
            }

            switch (rewardType)
            {
                case RewardType.Card:
                    {
                        // [S5h / D2=B] Flag-sourced pool (RewardPool ∩ UnlockedByDefault,
                        // owned cards excluded per D9). Empty pool → no card box. The
                        // old per-case skip (D-UX1=C) moved to FinishIfEmpty so one
                        // empty pool cannot close the canvas while another reward type
                        // is still being built.
                        cardRewardList.Clear();
                        var pool = pd.BuildRewardCardPool();
                        if (pool.Count == 0)
                        {
                            Debug.LogWarning("[RewardCanvas] Card reward pool empty/exhausted; no card box.");
                            return;
                        }
                        foreach (var c in pool) cardRewardList.Add(c);

                        var cardClone = Instantiate(rewardContainerPrefab, rewardRoot);
                        currentRewardsList.Add(cardClone);
                        cardClone.BuildReward(
                            cardRewardPresentation != null ? cardRewardPresentation.RewardSprite : null,
                            cardRewardPresentation != null ? cardRewardPresentation.RewardDescription : "Card reward");
                        cardClone.RewardButton.onClick.AddListener(() => GetCardReward(cardClone, 3));
                        break;
                    }
                case RewardType.Sfx:
                    {
                        // [S5h / #6b-lite] One venue-SFX unlock per win, sequential
                        // (D6: lights → smoke → fire). All unlocked → no box.
                        if (pd.AllSfxUnlocked)
                        {
                            Debug.Log("[RewardCanvas] All SFX thresholds unlocked; no SFX box.");
                            return;
                        }
                        var sfxClone = Instantiate(rewardContainerPrefab, rewardRoot);
                        currentRewardsList.Add(sfxClone);
                        sfxClone.BuildReward(
                            sfxRewardPresentation != null ? sfxRewardPresentation.RewardSprite : null,
                            sfxRewardPresentation != null ? sfxRewardPresentation.RewardDescription : "Stage SFX unlock");
                        sfxClone.RewardButton.onClick.AddListener(() => ClaimSfxReward(sfxClone));
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(rewardType), rewardType, null);
            }
        }

        private void ClaimSfxReward(RewardContainer container)
        {
            var pd = GameManager != null
                ? GameManager.PersistentGameplayData : null;

            if (pd != null && pd.TryUnlockNextSfxStage(out int stage))
                Debug.Log($"[RewardCanvas] [S5h] SFX threshold unlocked: stage {stage} " +
                          "(1=lights, 2=smoke, 3=fire). Active from the next gig.");

            currentRewardsList.Remove(container);
            Destroy(container.gameObject);
        }

        /// <summary>
        /// [S5h] Post-build guard preserving D-UX1=C at the STEP level: if no
        /// reward boxes were built at all, close the reward step immediately so
        /// post-gig flow proceeds. Caller must assign OnRewardFinished first.
        /// </summary>
        public void FinishIfEmpty()
        {
            if (currentRewardsList.Count == 0)
            {
                Debug.LogWarning("[RewardCanvas] No rewards available; skipping reward step entirely.");
                FinishReward();
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