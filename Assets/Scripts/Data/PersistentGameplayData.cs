using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT
{
    [Serializable]
    public class PersistentGameplayData
    {
        private readonly GameplayData gameplayData;

        [SerializeField] private List<CardData> currentCardsList;
        [SerializeField] private int drawCount;
        [SerializeField] private int maxGroove;
        [SerializeField] private int currentGroove;
        [SerializeField] private int turnStartingGroove;
        [SerializeField] private bool isRandomDeck;

        public PersistentGameplayData(GameplayData gameplayData)
        {
            this.gameplayData = gameplayData;

            InitData();
        }

        private void InitData()
        {
            drawCount = gameplayData.DrawCount;
            maxGroove = gameplayData.MaxGroove;
            turnStartingGroove = 0;
            currentGroove = turnStartingGroove;

            isRandomDeck = gameplayData.IsRandomDeck;
            CurrentCardsList = new List<CardData>();
        }

        #region Encapsulation
        public int DrawCount
        {
            get => drawCount;
            set => drawCount = value;
        }

        public int MaxGroove
        {
            get => maxGroove;
            set => maxGroove = value;
        }

        public int CurrentGroove
        {
            get => currentGroove;
            set => currentGroove = value;
        }

        public int TurnStartingGroove
        {
            get => turnStartingGroove;
            set => turnStartingGroove = value;
        }

        public List<CardData> CurrentCardsList
        {
            get => currentCardsList;
            set => currentCardsList = value;
        }

        public bool IsRandomDeck => isRandomDeck;
        #endregion
    }

    // TODO: Band Data, Song Data, Musician Data, etc

}