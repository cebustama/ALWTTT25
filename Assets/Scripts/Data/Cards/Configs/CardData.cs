using ALWTTT.Actions;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ALWTTT.Cards
{
    [System.Obsolete("Legacy: migrated to CardDefinition + CardPayload. Do not use.")]
    public class CardData : ScriptableObject
    {
        [Header("Card Profile")]
        [SerializeField] private string id;
        [SerializeField] private string cardName;

        [Header("Character")]
        [SerializeField] private CardPerformerRule performerRule = 
            CardPerformerRule.FixedMusicianType;
        [SerializeField] private MusicianCharacterType musicianCharacterType = 
            MusicianCharacterType.None;
        [SerializeField] private Sprite cardSprite; // Fill according to musician

        [Header("Inspiration")]
        [SerializeField] private int inspirationCost;
        [SerializeField] private int inspirationGenerated;

        [Header("Domain")]
        [SerializeField] private CardDomain domain = CardDomain.Action;

        [Header("Action Card Timing")]
        public CardActionTiming actionTiming = CardActionTiming.Always;

        [Header("Composition")]
        [SerializeField] private CardPrimaryKind primaryKind = CardPrimaryKind.None;
        [SerializeField] private TrackActionDescriptor trackAction;   // used when PrimaryKind=Track
        [SerializeField] private PartActionDescriptor partAction;

        [Tooltip("Zero or more modifier effects that will compose with the primary action.")]
        [SerializeField] private List<PartEffect> modifierEffects = new();

        [Header("Description")]
        [SerializeField] private List<SpecialKeywords> keywordsList;

        [Header("Fx")]
        [SerializeField] private AudioActionType audioType;
        [Header("Animation")]
        [SerializeField] private CardAnimationData musicianAnimation;


        [Header("Synergies")]
        [SerializeField] private CardType cardType;

        [Header("Effects")]
        [SerializeField] private bool usableWithoutTarget;
        [SerializeField] private bool exhaustAfterPlay;
        [SerializeField] private List<CardConditionData> cardConditionDataList;
        [SerializeField] private List<CharacterActionData> cardActionDataList;

        [SerializeField] private RarityType rarity; // Not used for now

        /// <summary>
        /// (TEMP) Whether this card uses the new model (PrimaryKind != None).
        /// </summary>
        public bool UsesNewCompositionModel => IsComposition 
            && primaryKind != CardPrimaryKind.None;

        // Fallback read-only empties to avoid null checks everywhere
        private static readonly List<CharacterActionData> EmptyActions = new();
        private static readonly List<SpecialKeywords> EmptyKeywords = new();

        #region Encapsulation

        public string Id => id;
        public bool UsableWithoutTarget => usableWithoutTarget;
        public string CardName => cardName;
        public RarityType Rarity => rarity;
        public int InspirationCost => inspirationCost;
        public Sprite CardSprite => cardSprite;
        public int InspirationGenerated => inspirationGenerated;

        public CardPerformerRule PerformerRule => performerRule;
        public MusicianCharacterType FixedPerformerType  => musicianCharacterType;
        public bool ExhaustAfterPlay => exhaustAfterPlay;
        public CardType CardType => cardType;
        public List<CardConditionData> CardConditionDataList => cardConditionDataList;
        public List<CharacterActionData> CardActionDataList => 
            cardActionDataList ?? EmptyActions;

        public List<SpecialKeywords> KeywordsList => keywordsList ?? EmptyKeywords;
        public AudioActionType AudioType => audioType;
        public CardAnimationData MusicianAnimation => musicianAnimation;
        public CardDomain Domain => domain;
        public bool IsComposition => domain == CardDomain.Composition;
        public bool IsAction => domain == CardDomain.Action;

        public CardPrimaryKind PrimaryKind => primaryKind;
        public TrackActionDescriptor TrackAction => trackAction;
        public PartActionDescriptor PartAction => partAction;
        public IReadOnlyList<PartEffect> ModifierEffects => modifierEffects;
        #endregion

        #region Type Helpers
        /// <summary>
        /// True if this is a Composition card whose primary action is to
        /// create/replace a musician track (Rhythm / Backing / Bassline / Melody / Harmony).
        /// </summary>
        public bool IsTrackCard =>
            IsComposition && primaryKind == CardPrimaryKind.Track;

        /// <summary>
        /// True if this is a Composition card whose primary action is to
        /// operate on Parts (create/mark Intro/Solo/Outro/Bridge/Final).
        /// </summary>
        public bool IsPartCard =>
            IsComposition && primaryKind == CardPrimaryKind.Part;

        /// <summary>
        /// Utility: does this card have at least one modifier effect of type T?
        /// </summary>
        private bool HasEffect<T>() where T : PartEffect =>
            modifierEffects != null && modifierEffects.Exists(e => e is T);

        /// <summary>
        /// True if this card changes the tempo (via TempoEffect).
        /// </summary>
        public bool IsTempoCard => HasEffect<TempoEffect>();

        /// <summary>
        /// True if this card changes the time signature / meter (via MeterEffect).
        /// </summary>
        public bool IsTimeSignatureCard => HasEffect<MeterEffect>();

        /// <summary>
        /// True if this card changes the tonality / mode (via TonalityEffect).
        /// </summary>
        public bool IsTonalityCard => HasEffect<TonalityEffect>();

        /// <summary>
        /// True if this card changes the Instrument of a Song Track
        /// </summary>
        public bool IsInstrumentCard => HasEffect<InstrumentEffect>();

        /// <summary>
        /// Theme cards will probably become their own effect or keyword later.
        /// For now we expose a simple hook that can be wired when you add a ThemeEffect
        /// or a dedicated keyword.
        /// </summary>
        public bool IsThemeCard => false; // placeholder until Theme is modeled in the new system

        /// <summary>
        /// A card needs a musician target if it affects a specific track
        /// (Track primary) or if any of its effects are scoped to TrackOnly.
        /// </summary>
        public bool RequiresMusicianTarget
        {
            get
            {
                if (!IsComposition) return false; // TODO: Action cards pipeline

                if (IsTrackCard) return true;

                if (modifierEffects != null)
                {
                    foreach (var fx in modifierEffects)
                    {
                        if (fx != null && fx.scope == EffectScope.TrackOnly)
                            return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// True if this card changes how the song sounds in any way:
        ///  - Track creation / replacement
        ///  - Any PartEffect (tempo, meter, tonality, density, feel, etc.)
        /// </summary>
        public bool AffectsSound =>
            IsComposition &&
            (IsTrackCard || (modifierEffects != null && modifierEffects.Count > 0));

        /// <summary>
        /// A card has a fixed musician target when it always applies to a given
        /// character type (e.g. "Drummer") and doesn't require hover selection.
        /// </summary>
        public bool HasFixedMusicianTarget =>
            RequiresMusicianTarget &&
            FixedPerformerType  != MusicianCharacterType.None;

        /// <summary>
        /// Used by the hand/hover logic: a card can be played without hovering
        /// a target if:
        ///  - it's explicitly marked as usable without target, or
        ///  - it has a fixed musician target.
        /// </summary>
        public bool CanBePlayedWithoutHover =>
            UsableWithoutTarget || HasFixedMusicianTarget;
        #endregion

        public string GetDescription(BandCharacterStats stats = null)
        {
            // Composition cards (or cards without actions) use composition descriptions
            if (IsComposition || CardActionDataList.Count == 0)
                return GetCompositionDescription();

            // --- Existing gig/action description ---
            var cardAction = CardActionDataList[0]; // safe: list is non-null and Count > 0
            var value = cardAction.ActionValue;

            string synergyText;
            if (stats != null)
            {
                int finalValue = CardType switch
                {
                    CardType.CHR => Mathf.RoundToInt(stats.Charm * value),
                    CardType.TCH => Mathf.RoundToInt(stats.Technique * value),
                    CardType.EMT => Mathf.RoundToInt(stats.Emotion * value),
                    _ => Mathf.RoundToInt(value),
                };
                synergyText = finalValue.ToString();
            }
            else
            {
                // With no stats, show the card type
                synergyText = cardType.ToString();
            }

            string actionText = cardAction.GetActionTypeText();
            string targetText = cardAction.ActionTargetType.ToString();

            return $"Apply {synergyText} {actionText} to {targetText}";
        }

        private string GetCompositionDescription()
        {
            if (!UsesNewCompositionModel)
                return "Legacy composition card.";

            var sb = new StringBuilder("Composition: ");

            // Primary
            if (primaryKind == CardPrimaryKind.Track)
            {
                string role = trackAction != null
                    ? trackAction.role.ToString()
                    : "Track";
                sb.Append($"Track [{role}]");
            }
            else if (primaryKind == CardPrimaryKind.Part)
            {
                string action = partAction != null ? partAction.action.ToString() : "Part";
                sb.Append($"Part [{action}]");
                if (partAction != null && !string.IsNullOrWhiteSpace(partAction.customLabel))
                    sb.Append($" (“{partAction.customLabel}”)");
            }
            else
            {
                sb.Append("No primary action");
            }

            // Effects
            if (modifierEffects != null && modifierEffects.Count > 0)
            {
                var effects = new List<string>();
                foreach (var fx in modifierEffects)
                {
                    effects.Add(fx.GetLabel());
                }
                sb.Append(" | Effects: " + string.Join(", ", effects));
            }

            return sb.ToString();
        }

        #region Refactor

#if UNITY_EDITOR
        private void OnValidate()
        {
            // --- 0) Null-guard lists so new assets never NRE ---
            if (modifierEffects == null) modifierEffects = new List<PartEffect>();
            if (cardActionDataList == null) cardActionDataList = new List<CharacterActionData>();
            if (keywordsList == null) keywordsList = new List<SpecialKeywords>();

            // --- 1) Honor Domain (legacy bridge) ---
            if (!IsComposition)                        // Action cards: keep composition fields clean
            {
                primaryKind = CardPrimaryKind.None;
                trackAction = null;
                partAction = null;
                modifierEffects.Clear();
                return;
            }

            // --- 2) If PrimaryKind is None, auto-pick when obvious ---
            if (primaryKind == CardPrimaryKind.None)
            {
                if (trackAction != null) primaryKind = CardPrimaryKind.Track;
                else if (partAction != null) primaryKind = CardPrimaryKind.Part;
            }

            // --- 3) Coherence: keep only the fields relevant to the chosen kind ---
            if (primaryKind == CardPrimaryKind.Track)
            {
                if (trackAction == null) trackAction = new TrackActionDescriptor();
                // keep partAction as null; effects remain valid
                partAction = null;
            }
            else if (primaryKind == CardPrimaryKind.Part)
            {
                if (partAction == null) partAction = new PartActionDescriptor();
                // keep trackAction as null; effects remain valid
                trackAction = null;
            }
            else
            {
                // None → keep both null and no dangling effects
                trackAction = null;
                partAction = null;
                modifierEffects.Clear();
            }
        }
#endif

        // ========== LEGACY BRIDGE HELPERS ==========

        /// <summary>
        /// Minimal bridge: tells legacy systems whether this composition card behaves as a "Track" or "Part"
        /// in the new model. Fallbacks to CompositionType heuristics when PrimaryKind is None.
        /// </summary>
        public bool IsPrimaryTrackLike =>
            UsesNewCompositionModel
                ? primaryKind == CardPrimaryKind.Track
                : IsTrackCard; // legacy

        /// <summary>
        /// Minimal bridge: returns a human label of the primary intent for UI ribbons.
        /// </summary>
        public string GetPrimaryLabel()
        {
            if (UsesNewCompositionModel)
            {
                if (primaryKind == CardPrimaryKind.Track)
                    return $"Track: " +
                        $"{(trackAction != null ? trackAction.role : "Unknown")}";
                if (primaryKind == CardPrimaryKind.Part)
                    return $"Part: " +
                        $"{(partAction != null ? partAction.action.ToString() : "Action")}";
            }

            return "Card";
        }
        #endregion
    }
}