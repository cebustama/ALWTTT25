using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class InventoryCanvas : CanvasBase
    {
        [SerializeField] private TextMeshProUGUI titleTextField;
        [SerializeField] private LayoutGroup cardSpawnRoot;
        [SerializeField] private CardBase cardUIPrefab;

        public TextMeshProUGUI TitleTextField => titleTextField;
        public LayoutGroup CardSpawnRoot => cardSpawnRoot;

        private List<CardBase> spawnedCardList = new List<CardBase>();

        public void ChangeTitle(string newTitle) => TitleTextField.text = newTitle;

        public void SetCards(List<CardData> cardDataList)
        {
            var count = 0;
            for (int i = 0; i < spawnedCardList.Count; i++)
            {
                count++;
                if (i >= cardDataList.Count)
                {
                    spawnedCardList[i].gameObject.SetActive(false);
                }
                else
                {
                    spawnedCardList[i].SetCard(cardDataList[i], false);
                    spawnedCardList[i].gameObject.SetActive(true);
                }

            }

            var cal = cardDataList.Count - count;
            if (cal > 0)
            {
                for (var i = 0; i < cal; i++)
                {
                    var cardData = cardDataList[count + i];
                    var cardBase = Instantiate(cardUIPrefab, CardSpawnRoot.transform);
                    cardBase.SetCard(cardData, false);
                    spawnedCardList.Add(cardBase);
                }
            }
        }

        public override void OpenCanvas()
        {
            base.OpenCanvas();
            if (DeckManager)
                DeckManager.HandController.DisableDragging();
        }

        public override void CloseCanvas()
        {
            base.CloseCanvas();
            if (DeckManager)
                DeckManager.HandController.EnableDragging();
        }

        public override void ResetCanvas()
        {
            base.ResetCanvas();
        }
    }
}