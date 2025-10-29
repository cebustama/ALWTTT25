using ALWTTT.Actions;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using MidiGenPlay.Composition;
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

        public enum CardDomain { Action, Composition }
        public enum CompositionCardType
        {
            None,
            // Meter
            TimeSignature_4_4, TimeSignature_3_4, TimeSignature_6_8, TimeSignature_5_4,
            // Tempo
            Tempo_Slow, Tempo_Fast, Tempo_VeryFast,
            // Themes
            Theme_Love, Theme_Injustice, Theme_Party,
            // Track Roles
            Track_Rhythm, Track_Backing, Track_Bassline, Track_Melody, Track_Harmony,
            // Parts
            Part_Intro, Part_Solo, Part_Outro,
            // Tonality
            Tonality_Ionian, Tonality_Dorian, Tonality_Phrygian, Tonality_Lydian,
            Tonality_Mixolydian, Tonality_Aeolian, Tonality_Locrian,
        }

        [Header("Domain")]
        [SerializeField] private CardDomain domain = CardDomain.Action;
        [SerializeField] private CompositionCardType compositionType = CompositionCardType.None;

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

        [Header("Composition Style")]
        [SerializeField] private bool overrideMelodyStrategy = false;
        [SerializeField] private MelodyStrategyId melodyStrategyIdOverride;
        [SerializeField] private bool overrideMelodicLeading = false;
        [SerializeField] private MelodicLeadingConfig melodicLeadingOverride;
        [SerializeField] private bool overrideHarmonyStrategy = false;
        [SerializeField] private HarmonyStrategyId harmonyStrategyIdOverride;
        [SerializeField] private bool overrideHarmonicLeading = false;
        [SerializeField] private HarmonicLeadingConfig harmonicLeadingOverride;

        [Header("Description")]
        [SerializeField] private List<SpecialKeywords> keywordsList;

        [Header("Fx")]
        [SerializeField] private AudioActionType audioType;

        // Fallback read-only empties to avoid null checks everywhere
        private static readonly List<CharacterActionData> EmptyActions = new();
        private static readonly List<SpecialKeywords> EmptyKeywords = new();

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
        public List<CharacterActionData> CardActionDataList => cardActionDataList ?? EmptyActions;

        public bool OverrideMelodyStrategy => overrideMelodyStrategy;
        public MelodyStrategyId MelodyStrategyIdOverride => melodyStrategyIdOverride;
        public bool OverrideMelodicLeading => overrideMelodicLeading;
        public MelodicLeadingConfig MelodicLeadingOverride => melodicLeadingOverride;

        public bool OverrideHarmonyStrategy => overrideHarmonyStrategy;
        public HarmonyStrategyId HarmonyStrategyIdOverride => harmonyStrategyIdOverride;
        public bool OverrideHarmonicLeading => overrideHarmonicLeading;
        public HarmonicLeadingConfig HarmonicLeadingOverride => harmonicLeadingOverride;

        public List<SpecialKeywords> KeywordsList => keywordsList ?? EmptyKeywords;
        public AudioActionType AudioType => audioType;

        public CardDomain Domain => domain;
        public CompositionCardType CompositionType => compositionType;
        public bool IsComposition => domain == CardDomain.Composition;
        public bool IsAction => domain == CardDomain.Action;
        #endregion

        #region Descriptions
        // Human-friendly default descriptions for Composition cards
        // TODO: Read from SO
        private static readonly Dictionary<CompositionCardType, string> CompositionDescriptions =
            new Dictionary<CompositionCardType, string>
        {
            // Time Signatures
            { CompositionCardType.TimeSignature_4_4,   "Set the song (or next part) to 4/4 time." },
            { CompositionCardType.TimeSignature_3_4,   "Set the song (or next part) to 3/4 (waltz) time." },
            { CompositionCardType.TimeSignature_6_8,   "Set the song (or next part) to 6/8 (swing/ternary) time." },
            { CompositionCardType.TimeSignature_5_4,   "Set the song (or next part) to 5/4 time." },

            // Tempo
            { CompositionCardType.Tempo_Slow,          "Slow down the next performance section." },
            { CompositionCardType.Tempo_Fast,          "Speed up the next performance section." },
            { CompositionCardType.Tempo_VeryFast,      "Play the next section much faster." },

            // Themes
            { CompositionCardType.Theme_Love,          "Set the song’s theme to Love." },
            { CompositionCardType.Theme_Injustice,     "Set the song’s theme to Injustice." },
            { CompositionCardType.Theme_Party,         "Set the song’s theme to Party." },

            // Track roles (we use pattern replacement/intensification in the generator)
            { CompositionCardType.Track_Rhythm,        "Change the rhythm track pattern for the target musician." },
            { CompositionCardType.Track_Backing,       "Change the backing/chord pattern for the target musician." },
            { CompositionCardType.Track_Bassline,      "Change the bassline pattern for the target musician." },
            { CompositionCardType.Track_Melody,        "Change the melody pattern for the target musician." },
            { CompositionCardType.Track_Harmony,       "Change the harmony/voicing pattern for the target musician." },

            // Parts
            { CompositionCardType.Part_Intro,          "Add an Intro played by the target musician." },
            { CompositionCardType.Part_Solo,           "Insert a Solo section by the target musician." },
            { CompositionCardType.Part_Outro,          "Add an Outro played by the target musician." },

            // Tonality
            {CompositionCardType.Tonality_Ionian,       "Set the part's tonality to ionian mode."},
            {CompositionCardType.Tonality_Dorian,       "Set the part's tonality to dorian mode."},
            {CompositionCardType.Tonality_Phrygian,     "Set the part's tonality to phrygian mode."},
            {CompositionCardType.Tonality_Lydian,       "Set the part's tonality to lydian mode."},
            {CompositionCardType.Tonality_Mixolydian,   "Set the part's tonality to mixolydian mode."},
            {CompositionCardType.Tonality_Aeolian,      "Set the part's tonality to aeolian mode."},
            {CompositionCardType.Tonality_Locrian,      "Set the part's tonality to locrian mode."},
        };
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
            if (compositionType == CompositionCardType.None)
                return "Composition tool.";

            if (CompositionDescriptions.TryGetValue(compositionType, out var text))
                return text;

            // Generic fallback if a new enum value is added without a description
            var label = compositionType.ToString()
                .Replace("TimeSignature_", "Time Signature ")
                .Replace('_', ' ');
            return label;
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