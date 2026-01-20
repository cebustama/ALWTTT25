using System.Collections.Generic;
using UnityEngine;
using ALWTTT.Status;
using ALWTTT.Cards.Effects;

namespace ALWTTT.Cards
{
    public abstract class CardPayload : ScriptableObject
    {
        public abstract CardDomain Domain { get; }

        [Header("Card Effects (New)")]
        [SerializeReference] private List<CardEffectSpec> effects = new();
        public IReadOnlyList<CardEffectSpec> Effects => effects;
    }
}
