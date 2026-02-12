using System;
using UnityEngine;
using ALWTTT.Enums;

namespace ALWTTT.Cards.Effects
{
    /// <summary>
    /// Directly modifies Audience Vibe.
    /// MVP: data-only spec executed by CardBase.ExecuteEffects.
    /// </summary>
    [Serializable]
    public sealed class ModifyVibeSpec : CardEffectSpec
    {
        [Tooltip("Who receives the Vibe modification. Expected: AudienceCharacter / AllAudienceCharacters (MVP).")]
        public ActionTargetType targetType = ActionTargetType.AllAudienceCharacters;

        [Tooltip("Vibe delta to apply. Positive adds Vibe; negative removes Vibe.")]
        public int amount = 1;
    }
}
