using ALWTTT.Enums;
using System;
using UnityEngine;

namespace ALWTTT.Encounters
{
    [Serializable]
    public abstract class EncounterBase
    {
        // TODO: Maybe change to BackgroundType instead, to be more general
        [SerializeField] private VenueType targetVenueType;

        public VenueType TargetVenueType => targetVenueType;
    }
}