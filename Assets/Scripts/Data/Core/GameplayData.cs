using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT
{
    [CreateAssetMenu(fileName = "New GameplayData", menuName = "ALWTTT/Containers/GameplayData")]
    public class GameplayData : ScriptableObject
    {
        [Header("Musicians")]
        [SerializeField] private List<MusicianBase> initialMusicianList;
        [SerializeField] private List<SongData> initialSongList;

        [Header("Map")]
        [SerializeField] private int maxCohesion = 10;
        [SerializeField] private int startingCohesion = 10; // Max by default

        [Header("Deck")]
        [SerializeField] private DeckData initialDeck;

        [Header("Gig Gameplay Settings")]
        [SerializeField] private int drawCount = 3;
        [SerializeField] private int maxCardsOnHand = 8;
        [SerializeField] private int maxGroove = 3;
        [SerializeField] private bool discardHandBetweenTurns = true;
        [SerializeField] private bool keepGrooveBetweenTurns = true;

        [Header("Cards")]
        [SerializeField] private List<CardData> allCardsList;
        [SerializeField] private CardBase cardPrefab;

        [Serializable]
        public class CardTypeEntry
        {
            public CardType CardType;
            public Color TypeColor;
        }
        [SerializeField] private List<CardTypeEntry> cardTypeEntryList;

        [Header("Modifiers")]
        [SerializeField] private bool isRandomDeck = false;
        [SerializeField] private int randomCardCount;

        #region Encapsulation
        public List<MusicianBase> InitialMusicianList => initialMusicianList;
        public List<SongData> InitialSongList => initialSongList;

        public int MaxCohesion => maxCohesion;
        public int InitialCohesion => InitialCohesion;

        public int DrawCount => drawCount;
        public int MaxCardsOnHand => maxCardsOnHand;
        public int MaxGroove => maxGroove;
        public bool DiscardHandBetweenTurns => discardHandBetweenTurns;
        public bool KeepGrooveBetweenTurns => keepGrooveBetweenTurns;

        public CardBase CardPrefab => cardPrefab;
        public List<CardData> AllCardsList => allCardsList;
        public DeckData InitialDeck => initialDeck;
        
        public bool IsRandomDeck => isRandomDeck;
        public int RandomCardCount => randomCardCount;
        #endregion
    
        public Color GetCardTypeColor(CardType type)
        {
            foreach (var entry in cardTypeEntryList)
            {
                if (entry.CardType == type)
                    return entry.TypeColor;
            }

            return Color.white;
        }
    }
}