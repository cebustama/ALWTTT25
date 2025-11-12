using ALWTTT.Cards;
using ALWTTT.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class InventoryCanvas : CanvasBase
    {
        [SerializeField] private TextMeshProUGUI titleTextField;
        [Header("Cards")]
        [SerializeField] private LayoutGroup cardSpawnRoot;
        [SerializeField] private CardBase cardUIPrefab;

        [Header("Songs")]
        [SerializeField] private LayoutGroup songSpawnRoot;
        [SerializeField] private GameObject songUIPrefab;

        public TextMeshProUGUI TitleTextField => titleTextField;
        public LayoutGroup CardSpawnRoot => cardSpawnRoot;

        private List<CardBase> spawnedCardList = new List<CardBase>();
        private List<GameObject> spawnedSongList = new List<GameObject>();

        public void ChangeTitle(string newTitle) => TitleTextField.text = newTitle;

        public void SetCards(List<CardData> cardDataList)
        {
            cardSpawnRoot.gameObject.SetActive(true);
            songSpawnRoot.gameObject.SetActive(false);

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

        public void SetSongs(List<SongData> songDataList)
        {
            if (songDataList.Count == 0)
            {
                Debug.Log("No songs.");
                return;
            }

            cardSpawnRoot.gameObject.SetActive(false);
            songSpawnRoot.gameObject.SetActive(true);

            var count = 0;
            // Reuse existing song UI elements
            for (int i = 0; i < spawnedSongList.Count; i++)
            {
                count++;
                if (i >= songDataList.Count)
                {
                    spawnedSongList[i].gameObject.SetActive(false);
                }
                else
                {
                    var s = songDataList[i];
                    spawnedSongList[i].GetComponentInChildren<TextMeshProUGUI>().text =
                        s.SongTitle + " - " + s.SongTheme + " - " + s.Complexity;
                    spawnedSongList[i].gameObject.SetActive(true);
                }
            }

            // New
            var cal = songDataList.Count - count;
            if (cal > 0)
            {
                for (var i = 0; i < cal; i++)
                {
                    var songData = songDataList[count + i];
                    var songUI = Instantiate(songUIPrefab, songSpawnRoot.transform);
                    songUI.GetComponentInChildren<TextMeshProUGUI>().text =
                        songData.SongTitle + " - " + songData.SongTheme + " - " 
                        + songData.Complexity;
                    spawnedSongList.Add(songUI);
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