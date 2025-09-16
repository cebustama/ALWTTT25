using ALWTTT.Actions;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using System.Text;
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

        [Header("Character")]
        [SerializeField] private MusicianCharacterType musicianCharacterType = 
            MusicianCharacterType.None;

        [Header("Synergies")]
        [SerializeField] private CardType cardType;

        [Header("Effects")]
        [SerializeField] private bool usableWithoutTarget;
        [SerializeField] private bool exhaustAfterPlay;
        [SerializeField] private List<CardConditionData> cardConditionDataList;
        [SerializeField] private List<CharacterActionData> cardActionDataList;

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
        public MusicianCharacterType MusicianCharacterType => musicianCharacterType;
        public bool ExhaustAfterPlay => exhaustAfterPlay;
        public CardType CardType => cardType;
        public List<CardConditionData> CardConditionDataList => cardConditionDataList;
        public List<CharacterActionData> CardActionDataList => cardActionDataList;
        public List<SpecialKeywords> KeywordsList => keywordsList;
        public AudioActionType AudioType => audioType;
        #endregion
    
        public string GetDescription(BandCharacterStats stats = null)
        {
            // TODO Multiple actions
            var cardAction = cardActionDataList[0];
            var value = cardAction.ActionValue;

            string synergyText = "";
            if (stats != null)
            {
                int finalValue;
                switch (CardType)
                {
                    case CardType.CHR:
                        finalValue = Mathf.RoundToInt(stats.Charm * value);
                        break;
                    case CardType.TCH:
                        finalValue = Mathf.RoundToInt(stats.Technique * value);
                        break;
                    case CardType.EMT:
                        finalValue = Mathf.RoundToInt(stats.Emotion * value);
                        break;
                    default:
                        finalValue = Mathf.RoundToInt(value);
                        break;
                }
                synergyText = finalValue.ToString();
            }
            else
            {
                synergyText = cardType.ToString();
            }

            string actionText = cardAction.GetActionTypeText();
            string targetText = cardAction.ActionTargetType.ToString();

            var descriptionText = $"Apply {synergyText} {actionText} to {targetText}";

            return descriptionText;
        }

        // TODO: Color
    }

    [Serializable]
    public class CardConditionData
    {
        [SerializeField] private CardConditionType cardConditionType;
        [SerializeField] private float conditionValue;
    }
}