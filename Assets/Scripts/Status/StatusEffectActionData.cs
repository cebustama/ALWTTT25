using System;
using UnityEngine;
using ALWTTT.Enums;

namespace ALWTTT.Status
{
    [Serializable]
    public sealed class StatusEffectActionData
    {
        [SerializeField] private CharacterStatusId effectId;
        [SerializeField] private ActionTargetType targetType;
        [SerializeField] private int stacksDelta = 1;
        [SerializeField] private float delay = 0f; // optional symmetry with CharacterActionData

        public CharacterStatusId EffectId => effectId;
        public ActionTargetType TargetType => targetType;
        public int StacksDelta => stacksDelta;
        public float Delay => delay;
    }
}