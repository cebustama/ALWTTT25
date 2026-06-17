using ALWTTT.Enums;
using System;
using UnityEngine;

namespace ALWTTT.Encounters
{
    [Serializable]
    public abstract class EncounterBase
    {
        [SerializeField] private VenueType targetVenueType;

        public VenueType TargetVenueType => targetVenueType;

        protected EncounterBase() { }

        protected EncounterBase(VenueType targetVenueType)
        {
            this.targetVenueType = targetVenueType;
        }
    }
}
