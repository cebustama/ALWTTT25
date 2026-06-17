using System;
using UnityEngine;

namespace ALWTTT.Cards.Effects
{
    /// <summary>
    /// Example of a non-status card effect (MVP).
    /// Runtime executor can interpret this as "draw N cards".
    /// </summary>
    [Serializable]
    public sealed class DrawCardsSpec : CardEffectSpec
    {
        [Min(0)]
        public int count = 1;
    }
}