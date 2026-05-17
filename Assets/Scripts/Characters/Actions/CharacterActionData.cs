using ALWTTT.Enums;
using ALWTTT.Status;
using System;
using UnityEngine;

namespace ALWTTT.Actions
{
    [Serializable]
    public class CharacterActionData
    {
        [SerializeField] private CharacterActionType cardActionType;
        [SerializeField] private ActionTargetType actionTargetType;

        // TODO Target Conditions (First Enemy, Most Stressed Musician, etc)

        [SerializeField] private float actionValue;
        [SerializeField] private float actionDelay;

        [Header("ApplyStatusEffect-only (D10=A)")]
        [Tooltip("Only read when cardActionType == ApplyStatusEffect. " +
                 "The SO to apply to the resolved target(s). " +
                 "ActionValue is reused as stacksDelta (rounded to int). " +
                 "Ignored for all other action types.")]
        [SerializeField] private StatusEffectSO statusEffect;

        public CharacterActionType CardActionType => cardActionType;
        public ActionTargetType ActionTargetType => actionTargetType;
        public float ActionValue => actionValue;
        public float ActionDelay => actionDelay;
        public StatusEffectSO StatusEffect => statusEffect;

        public string GetActionTypeText()
        {
            // TODO
            return CardActionType.ToString();
        }
    }
}