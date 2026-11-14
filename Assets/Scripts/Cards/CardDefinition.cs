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
        [SerializeField]
        private CardPerformerRule performerRule =
            CardPerformerRule.FixedMusicianType;
        [SerializeField]
        private MusicianCharacterType musicianCharacterType =
            MusicianCharacterType.None;

        [Header("Visuals")]
        [SerializeField] private Sprite cardSprite;

        [Header("Inspiration")]
        [SerializeField] private int inspirationCost = 1;
        [SerializeField] private int inspirationGenerated = 0;

        // ─── [R5-d / D-R5-26=A] Resource cost ────────────────────────────
        // Generic (statusKey, amount) pair, NOT a `voltageCost` field. The
        // container is keyed by primitive and the variant is the authority,
        // so a string key + an int cover Voltage today and any future
        // character resource without migrating this asset again.
        // Empty key or amount 0 ⇒ no resource cost (the default).
        // ─── [ECON-2] Finisher ───────────────────────────────────────────
        // Una carta de COMPOSICIÓN marcada como Finisher invierte el bloqueo del
        // último loop: solo se juega mientras suena (regla ajustable), gasta su ★
        // y concede UN loop final con solo de su músico. Si se juegan varios, todos
        // tocan juntos en ese mismo loop y sus efectos estallan al empezarlo.
        // No toca pistas: su música es el solo, no un cambio de la parte.
        [Header("Finisher (ECON-2)")]
        [SerializeField, Tooltip("Carta Finisher: solo en el último loop, gasta ★ y " +
            "añade un loop final con solo de su músico. Debe ser de composición.")]
        private bool isFinisher;

        [SerializeField, Tooltip("Estrategia de melodía del solo del Finisher.")]
        private MidiGenPlay.Composition.MelodyStrategyId finisherSoloStrategy =
            MidiGenPlay.Composition.MelodyStrategyId.ScaleFlow;

        [SerializeField, Tooltip("Opcional: cómo se mueve la melodía del solo " +
            "(MelodicLeadingConfig). Vacío = la del músico. Sirve para el solo " +
            "«hipnótico» de Sibi: ScaleOnly, pasos cortos, muchas notas repetidas.")]
        private MidiGenPlay.Composition.MelodicLeadingConfig finisherSoloLeading;

        [Header("Resource Cost (R5-d)")]
        [SerializeField, Tooltip("StatusKey of the resource this card spends " +
            "(e.g. \"voltage\"). Empty = no resource cost. Resolved from the " +
            "PAYER's catalogue; a bearer without it cannot play the card.")]
        private string resourceCostStatusKey = "";

        [SerializeField, Min(0), Tooltip("Stacks spent on play. Checked before " +
            "any spend and consumed via StatusEffectContainer.SpendStacks.")]
        private int resourceCostAmount = 0;

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

                        // [S9c / D1=B] A single-target Stress change (Calm Down heals the
                        // musician the card is dropped on) also needs a hovered target.
                        if (e is ModifyStressSpec mss)
                        {
                            switch (mss.targetType)
                            {
                                case ActionTargetType.Musician:
                                case ActionTargetType.AudienceCharacter:
                                    return true;
                            }
                        }

                        // [R4] RevealPreferencesSpec is the second spec kind that can
                        // demand a hovered target. Without this branch a reveal-only
                        // card would be playable with no hover, resolve an empty target
                        // list and no-op silently.
                        if (e is RevealPreferencesSpec reveal)
                        {
                            switch (reveal.targetType)
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

        /// <summary>[R5-d / D-R5-26=A] StatusKey of the spent resource, or empty.</summary>
        public string ResourceCostStatusKey => resourceCostStatusKey;

        /// <summary>[R5-d] Stacks spent on play. Never negative.</summary>
        public int ResourceCostAmount => Mathf.Max(0, resourceCostAmount);

        /// <summary>[R5-d] True when both halves of the pair are authored.</summary>
        public bool HasResourceCost =>
            !string.IsNullOrWhiteSpace(resourceCostStatusKey) && resourceCostAmount > 0;

        public int InspirationGenerated => inspirationGenerated;

        /// <summary>[ECON-2] Finisher: solo de composición, ver el bloque de campos.</summary>
        public bool IsFinisher => isFinisher && IsComposition;
        public MidiGenPlay.Composition.MelodyStrategyId FinisherSoloStrategy => finisherSoloStrategy;
        public MidiGenPlay.Composition.MelodicLeadingConfig FinisherSoloLeading => finisherSoloLeading;

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
            return $"{Id} � {DisplayName} ({domain})";
        }

        private void OnValidate()
        {
            _cachedEditorLabel = null;
        }
#endif
    }
}