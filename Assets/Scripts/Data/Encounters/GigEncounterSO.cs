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
        /// Builds a runtime GigEncounter instance from this authoring asset
        /// using the SO's baked audienceMemberList.
        /// </summary>
        public GigEncounter BuildRuntime() => BuildRuntime(null);

        /// <summary>
        /// M4.6-prep merged (1)/(4): builds a runtime GigEncounter using an
        /// audience override when the audience picker has deviated from the
        /// SO's baked list. When <paramref name="audienceOverride"/> is null
        /// or empty, falls back to the SO's audienceMemberList (regression-safe).
        /// </summary>
        public GigEncounter BuildRuntime(IList<AudienceCharacterData> audienceOverride)
        {
            var audienceList = (audienceOverride != null && audienceOverride.Count > 0)
                ? new List<AudienceCharacterData>(audienceOverride)
                : new List<AudienceCharacterData>(
                    audienceMemberList ?? new List<AudienceCharacterData>());

            return new GigEncounter(
                targetVenueType,
                displayName,
                audienceList,
                numberOfSongs,
                fansOnWin,
                cohesionPenaltyOnLoss
            );
        }

        public string GetLabel()
        {
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName;

            return $"{targetVenueType} | " +
                $"Songs:{numberOfSongs} | " +
                $"FansWin:{fansOnWin} | " +
                $"CohLoss:{cohesionPenaltyOnLoss}";
        }
    }
}