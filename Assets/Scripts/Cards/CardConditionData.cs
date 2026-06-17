using ALWTTT.Enums;
using System;
using UnityEngine;

namespace ALWTTT.Cards
{
    [Serializable]
    public class CardConditionData
    {
        [SerializeField] private CardConditionType cardConditionType;
        [SerializeField] private float conditionValue;
    }
}
