using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT 
{
    [CreateAssetMenu(fileName = "New CardData", menuName = "ALWTTT/Cards/CardData")]
    public class CardData : ScriptableObject
    {
        [Header("Card Profile")]
        [SerializeField] private string id;
        [SerializeField] private string cardName;
        [SerializeField] private CardPhase phase;
        [SerializeField] private RarityType rarity;
        [SerializeField] private int grooveCost;
        [SerializeField] private Sprite cardSprite;
        [SerializeField] private int grooveGenerated;

        [Header("Synergies")]
        [SerializeField] private List<CardType> typesList;

        [Header("Effects")]
        [SerializeField] private List<CardConditionData> cardConditionDataList;
        [SerializeField] private List<CardActionData> cardActionDataList;

        [Header("Description")]
        [SerializeField] private List<SpecialKeywords> keywordsList;

        [Header("Fx")]
        [SerializeField] private AudioActionType audioType;

        #region Cache

        public string Id => id;
        public string CardName => cardName;
        public CardPhase Phase => phase;
        public RarityType Rarity => rarity;
        public int GrooveCost => grooveCost;
        public Sprite CardSprite => cardSprite;
        public int GrooveGenerated => grooveGenerated;

        #endregion
    }

    [Serializable]
    public class CardConditionData
    {
        [SerializeField] private CardConditionType cardConditionType;
        [SerializeField] private float conditionValue;
    }

    [Serializable]
    public class CardActionData
    {
        [SerializeField] private CardActionType cardConditionType;
        [SerializeField] private ActionTargetType actionTargetType;
        [SerializeField] private float actionValue;
        [SerializeField] private float actionDelay;
    }
}