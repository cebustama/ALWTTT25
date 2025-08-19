using ALWTTT.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Encounters
{
    [Serializable]
    public class GigEncounter : EncounterBase
    {
        [SerializeField] private List<AudienceCharacterData> audienceMemberList;
        [SerializeField] private int numberOfSongs;

        public List<AudienceCharacterData> AudienceMemberList => audienceMemberList;
        public int NumberOfSongs => numberOfSongs;
    }
}