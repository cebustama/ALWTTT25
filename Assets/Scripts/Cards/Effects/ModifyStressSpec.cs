using System;
using UnityEngine;
using ALWTTT.Enums;

namespace ALWTTT.Cards.Effects
{
    /// <summary>
    /// Directly modifies musician Stress.
    /// Positive amount = add incoming stress (mitigated by Composure first).
    /// Negative amount = remove stress.
    /// MVP: data-only spec executed by CardBase.ExecuteEffects.
    /// </summary>
    [Serializable]
    public sealed class ModifyStressSpec : CardEffectSpec
    {
        [Tooltip("Who receives the Stress modification. Expected: Self / Musician / AllMusicians (MVP).")]
        public ActionTargetType targetType = ActionTargetType.Self;

        [Tooltip("Stress delta to apply. Positive adds incoming stress; negative removes stress.")]
        public int amount = 1;
    }
}
