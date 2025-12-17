using System.Collections.Generic;
using UnityEngine;

using ALWTTT.Actions;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(fileName = "New ActionCardPayload", 
        menuName = "ALWTTT/Cards/Payloads/Action Card Payload")]
    public class ActionCardPayload : CardPayload
    {
        public override CardDomain Domain => CardDomain.Action;

        [Header("Action Timing")]
        [SerializeField] private CardActionTiming actionTiming = CardActionTiming.Always;

        [Header("Conditions / Actions")]
        [SerializeField] private List<CardConditionData> conditions = new();
        [SerializeField] private List<CharacterActionData> actions = new();

        public CardActionTiming ActionTiming => actionTiming;
        public IReadOnlyList<CardConditionData> Conditions => conditions;
        public IReadOnlyList<CharacterActionData> Actions => actions;
    }
}
