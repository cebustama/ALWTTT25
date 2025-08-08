using ALWTTT.Enums;
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

        public CharacterActionType CardActionType => cardActionType;
        public ActionTargetType ActionTargetType => actionTargetType;
        public float ActionValue => actionValue;
        public float ActionDelay => actionDelay;
    }
}