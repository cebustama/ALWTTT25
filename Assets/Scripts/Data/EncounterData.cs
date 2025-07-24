using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT
{
    [CreateAssetMenu(fileName = "New EncounterData", menuName = "ALWTTT/EncounterData")]
    public class EncounterData : ScriptableObject
    {

    }

    [Serializable]
    public class EnemyEncounter : EncounterBase 
    {
        [SerializeField] private List<EnemyCharacterData> enemyList;

        public List<EnemyCharacterData> EnemyList => enemyList;
    }

    [Serializable]
    public abstract class EncounterBase
    {
        [SerializeField] private VenueType targetVenueType;

        public VenueType TargetVenueType => targetVenueType;
    }
}