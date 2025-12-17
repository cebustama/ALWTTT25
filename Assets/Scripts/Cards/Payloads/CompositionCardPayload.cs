using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(fileName = "New CompositionCardPayload", 
        menuName = "ALWTTT/Cards/Payloads/Composition Card Payload")]
    public class CompositionCardPayload : CardPayload
    {
        public override CardDomain Domain => CardDomain.Composition;

        [Header("Composition")]
        [SerializeField] private CardPrimaryKind primaryKind = CardPrimaryKind.None;

        [SerializeField] private TrackActionDescriptor trackAction;
        [SerializeField] private PartActionDescriptor partAction;

        [Header("Modifier Effects")]
        [SerializeField] private List<PartEffect> modifierEffects = new();

        public CardPrimaryKind PrimaryKind => primaryKind;
        public TrackActionDescriptor TrackAction => trackAction;
        public PartActionDescriptor PartAction => partAction;
        public IReadOnlyList<PartEffect> ModifierEffects => modifierEffects;

        public bool RequiresMusicianTarget
        {
            get
            {
                // Track primary always implies a musician track
                if (PrimaryKind == CardPrimaryKind.Track) return true;

                // Any TrackOnly effect implies a musician target
                if (ModifierEffects != null)
                {
                    foreach (var fx in ModifierEffects)
                        if (fx != null && fx.scope == EffectScope.TrackOnly)
                            return true;
                }

                return false;
            }
        }
    }
}
