using System.Collections.Generic;
using UnityEngine;
using ALWTTT.Status;

namespace ALWTTT.Cards
{
    public abstract class CardPayload : ScriptableObject
    {
        public abstract CardDomain Domain { get; }

        [Header("Status Effects (CSO)")]
        [SerializeField] private List<StatusEffectActionData> statusActions = new();

        public IReadOnlyList<StatusEffectActionData> StatusActions => statusActions;
    }
}