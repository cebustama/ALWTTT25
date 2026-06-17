using System;
using UnityEngine;
using ALWTTT.Enums;
using ALWTTT.Status;

namespace ALWTTT.Cards.Effects
{
    /// <summary>
    /// Applies a specific authored StatusEffectSO variant
    /// This enables composite authored status definitions.
    /// </summary>
    [Serializable]
    public sealed class ApplyStatusEffectSpec : CardEffectSpec
    {
        [Tooltip("The specific ALWTTT StatusEffectSO variant to apply.")]
        public StatusEffectSO status;

        [Tooltip("Who receives the status application.")]
        public ActionTargetType targetType = ActionTargetType.Self;

        [Tooltip("How many stacks to add/remove. Can be negative.")]
        public int stacksDelta = 1;

        [Tooltip("Optional delay (seconds) before applying.")]
        [Min(0f)]
        public float delay = 0f;
    }
}
