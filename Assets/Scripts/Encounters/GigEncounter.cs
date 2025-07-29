using ALWTTT.Audience;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Encounters
{
    [Serializable]
    public class GigEncounter : EncounterBase
    {
        [SerializeField] private List<AudienceCharacterData> audienceMemberList;

        public List<AudienceCharacterData> AudienceMemberList => audienceMemberList;
    }
}