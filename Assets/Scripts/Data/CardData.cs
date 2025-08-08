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
        [SerializeField] private bool usableWithoutTarget;
        [SerializeField] private bool exhaustAfterPlay;
        [SerializeField] private List<CardConditionData> cardConditionDataList;
        [SerializeField] private List<CardActionData> cardActionDataList;

        [Header("Description")]
        [SerializeField] private List<SpecialKeywords> keywordsList;

        [Header("Fx")]
        [SerializeField] private AudioActionType audioType;

        #region Encapsulation

        public string Id => id;
        public bool UsableWithoutTarget => usableWithoutTarget;
        public string CardName => cardName;
        public CardPhase Phase => phase;
        public RarityType Rarity => rarity;
        public int GrooveCost => grooveCost;
        public Sprite CardSprite => cardSprite;
        public int GrooveGenerated => grooveGenerated;
        public bool ExhaustAfterPlay => exhaustAfterPlay;

        public List<CardType> TypesList => typesList;

        public List<CardConditionData> CardConditionDataList => cardConditionDataList;
        public List<CardActionData> CardActionDataList => cardActionDataList;

        #endregion
    }

    [Serializable]
    public class CardConditionData
    {
        [SerializeField] private CardConditionType cardConditionType;
        [SerializeField] private float conditionValue;
    }

    // TODO: Change name to CharacterActionData, move to separate file
    [Serializable]
    public class CardActionData
    {
        [SerializeField] private CardActionType cardActionType;
        [SerializeField] private ActionTargetType actionTargetType;

        // TODO Target Conditions (First Enemy, Most Stressed Musician, etc)

        [SerializeField] private float actionValue;
        [SerializeField] private float actionDelay;

        public CardActionType CardActionType => cardActionType;
        public ActionTargetType ActionTargetType => actionTargetType;
        public float ActionValue => actionValue;
        public float ActionDelay => actionDelay;
    }
}