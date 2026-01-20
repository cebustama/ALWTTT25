using ALWTTT.Actions;
using ALWTTT.Cards.Effects;
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

                var effects = payload != null ? payload.Effects : null;
                if (effects != null)
                {
                    for (int i = 0; i < effects.Count; i++)
                    {
                        var e = effects[i];

                        if (e is ApplyStatusEffectSpec ase)
                        {
                            switch (ase.targetType)
                            {
                                case ActionTargetType.Musician:
                                case ActionTargetType.AudienceCharacter:
                                    return true;
                            }
                        }
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

        public CardDomain Domain => payload != null ? payload.Domain : CardDomain.Unknown;

#if UNITY_EDITOR
        [System.NonSerialized] private string _cachedEditorLabel;

        public string EditorLabel
        {
            get
            {
                if (string.IsNullOrEmpty(_cachedEditorLabel))
                    _cachedEditorLabel = BuildEditorLabel();
                return _cachedEditorLabel;
            }
        }

        private string BuildEditorLabel()
        {
            var domain = IsAction ? "Action" : (IsComposition ? "Composition" : "Unknown");
            return $"{Id} — {DisplayName} ({domain})";
        }

        private void OnValidate()
        {
            _cachedEditorLabel = null;
        }
#endif
    }
}