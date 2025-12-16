using ALWTTT.Data;
using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Encounters
{
    [Serializable]
    public class GigEncounter : EncounterBase
    {
        [SerializeField] private string displayName;
        [SerializeField] private List<AudienceCharacterData> audienceMemberList;
        [SerializeField] private int numberOfSongs;
        [SerializeField] private int fansOnWin;
        [SerializeField] private int cohesionPenaltyOnLoss;

        public string DisplayName => displayName;
        public List<AudienceCharacterData> AudienceMemberList => audienceMemberList;
        public int NumberOfSongs => numberOfSongs;
        public int FansOnWin => fansOnWin;
        public int CohesionPenaltyOnLoss => cohesionPenaltyOnLoss;

        public GigEncounter() : base() { }

        public GigEncounter(
            VenueType targetVenueType,
            string displayName,
            List<AudienceCharacterData> audience,
            int numberOfSongs,
            int fansOnWin,
            int cohesionPenaltyOnLoss
        ) : base(targetVenueType)
        {
            this.displayName = displayName;
            this.audienceMemberList = audience ?? new List<AudienceCharacterData>();
            this.numberOfSongs = Mathf.Max(1, numberOfSongs);
            this.fansOnWin = fansOnWin;
            this.cohesionPenaltyOnLoss = cohesionPenaltyOnLoss;
        }

        public string GetLabel()
        {
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName;

            return $"{TargetVenueType} | " +
                   $"Songs:{numberOfSongs} | " +
                   $"FansWin:{fansOnWin} | " +
                   $"CohLoss:{cohesionPenaltyOnLoss}";
        }
    }
}