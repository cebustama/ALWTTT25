using System.Collections.Generic;
using UnityEngine;
using ALWTTT.Data;
using ALWTTT.Enums;

namespace ALWTTT.Encounters
{
    [CreateAssetMenu(fileName = "GigEncounter", menuName = "ALWTTT/Gig/Gig Encounter")]
    public class GigEncounterSO : ScriptableObject
    {
        [Header("Encounter")]
        [SerializeField] private VenueType targetVenueType;
        [SerializeField] private string displayName;

        [Header("Audience")]
        [SerializeField] private List<AudienceCharacterData> audienceMemberList = new();

        [Header("Gig Requirements")]
        [SerializeField, Min(1)] private int numberOfSongs = 1;

        [Header("Rewards / Penalties")]
        [SerializeField] private int fansOnWin = 0;
        [SerializeField] private int cohesionPenaltyOnLoss = 0;

        public VenueType TargetVenueType => targetVenueType;
        public string DisplayName => displayName;
        public IReadOnlyList<AudienceCharacterData> AudienceMemberList => audienceMemberList;
        public int NumberOfSongs => numberOfSongs;
        public int FansOnWin => fansOnWin;
        public int CohesionPenaltyOnLoss => cohesionPenaltyOnLoss;

        /// <summary>
        /// Builds a runtime GigEncounter instance from this authoring asset.
        /// </summary>
        public GigEncounter BuildRuntime()
        {
            return new GigEncounter(
                targetVenueType,
                displayName,
                new List<AudienceCharacterData>(audienceMemberList ?? 
                    new List<AudienceCharacterData>()),
                numberOfSongs,
                fansOnWin,
                cohesionPenaltyOnLoss
            );
        }

        public string GetLabel()
        {
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName;

            // Lightweight label for UI (no allocation-heavy list formatting)
            return $"{targetVenueType} | " +
                $"Songs:{numberOfSongs} | " +
                $"FansWin:{fansOnWin} | " +
                $"CohLoss:{cohesionPenaltyOnLoss}";
        }
    }
}
