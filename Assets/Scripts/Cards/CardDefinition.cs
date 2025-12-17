using ALWTTT.Actions;
using ALWTTT.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Cards 
{
    /// <summary>
    /// "What a card is": stable identity, presentation, economy + a payload reference.
    /// Does NOT contain action/composition specifics.
    /// </summary>
    [CreateAssetMenu(fileName = "New CardDefinition", menuName = "ALWTTT/Cards/Card Definition")]
    public class CardDefinition : ScriptableObject
    {
        [Header("Card Profile")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Header("Character")]
        [SerializeField] private CardPerformerRule performerRule =
            CardPerformerRule.FixedMusicianType;
        [SerializeField] private MusicianCharacterType musicianCharacterType = 
            MusicianCharacterType.None;

        [Header("Visuals")]
        [SerializeField] private Sprite cardSprite;

        [Header("Inspiration")]
        [SerializeField] private int inspirationCost = 1;
        [SerializeField] private int inspirationGenerated = 0;

        [Header("Synergies")]
        [SerializeField] private CardType cardType;

        [Header("Meta")]
        [SerializeField] private RarityType rarity = RarityType.Common;
        // TODO: Automate based on effects/other fields
        [SerializeField] private List<SpecialKeywords> keywords = new();

        [Header("FX / Animation")]
        [SerializeField] private AudioActionType audioType;
        [SerializeField] private CardAnimationData musicianAnimation;

        [Header("Play Rules")]
        [SerializeField] private bool exhaustAfterPlay;
        [SerializeField] private bool overrideRequiresTargetSelection;
        [SerializeField] private bool requiresTargetSelectionOverrideValue;

        [Header("Payload")]
        [SerializeField] private CardPayload payload;

        // --- Read-only API  ---
        public string Id => id;
        public string DisplayName => displayName;

        public CardPerformerRule PerformerRule => performerRule;
        public MusicianCharacterType FixedPerformerType => musicianCharacterType;
        public bool RequiresFixedPerformer => 
            performerRule == CardPerformerRule.FixedMusicianType;
        public bool RequiresTargetSelection
        {
            get
            {
                if (overrideRequiresTargetSelection)
                    return requiresTargetSelectionOverrideValue;

                var actions = CardActionDataList;
                if (actions == null || actions.Count == 0) return false;

                foreach (var a in actions)
                {
                    switch (a.ActionTargetType)
                    {
                        case ActionTargetType.Musician:
                        case ActionTargetType.AudienceCharacter:
                            return true;
                    }
                }

                return false;
            }
        }
        public bool CanBePlayedByAnyMusician => performerRule == CardPerformerRule.AnyMusician;

        public bool CanBePlayedWithoutHover
        {
            get
            {
                // You can play without hover if no target selection is needed.
                // If later you add "optional target" cards, this will become more nuanced.
                return !RequiresTargetSelection;
            }
        }

        public bool RequiresMusicianTarget =>
            CompositionPayload != null && CompositionPayload.RequiresMusicianTarget;

        public Sprite CardSprite => cardSprite;

        public int InspirationCost => inspirationCost;
        public int InspirationGenerated => inspirationGenerated;

        public RarityType Rarity => rarity;
        public IReadOnlyList<SpecialKeywords> Keywords => keywords;

        public AudioActionType AudioType => audioType;
        public CardAnimationData MusicianAnimation => musicianAnimation;

        public CardType CardType => cardType;

        public bool ExhaustAfterPlay => exhaustAfterPlay;

        public CardPayload Payload => payload;
        public bool HasPayload => payload != null;

        public bool IsAction => payload is ActionCardPayload;
        public bool IsComposition => payload is CompositionCardPayload;

        public ActionCardPayload ActionPayload => payload as ActionCardPayload;
        public CompositionCardPayload CompositionPayload => payload as CompositionCardPayload;
        

        /// <summary>
        /// Legacy-style access to action list (now stored in ActionPayload).
        /// Returns null for non-action cards.
        /// </summary>
        public IReadOnlyList<CharacterActionData> CardActionDataList => ActionPayload?.Actions;

        public CardDomain Domain => payload != null ? payload.Domain : CardDomain.Unknown;
    }
}