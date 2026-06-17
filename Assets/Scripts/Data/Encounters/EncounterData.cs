using ALWTTT.Encounters;
using ALWTTT.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "New EncounterData", menuName = "ALWTTT/Containers/EncounterData")]
    public class EncounterData : ScriptableObject
    {
        [SerializeField] private List<GigEncounterSector> encounterSectorsList;
        [SerializeField] private bool randomGigs;

        public List<GigEncounterSector> EncounterSectorsList => encounterSectorsList;

        public GigEncounter GetGigEncounter(int sectorId = 0, int encounterId = 0, bool isFinal = false)
        {
            var selectedSector = EncounterSectorsList.First(x => x.SectorId == sectorId);
            if (isFinal) return selectedSector.BossGigEncounterList.RandomItem();

            ListExtensions.SetSeed((int)(UnityEngine.Random.value * 1000));

            return randomGigs ?
                selectedSector.GigEncounterList.RandomItem() :
                selectedSector.GigEncounterList[encounterId] ?? // If it exists
                    selectedSector.GigEncounterList.RandomItem(); // Else random
        }

        public GigEncounter GetGigEncounterByIndex(int sectorId, int encounterId, bool isFinal)
        {
            var sector = EncounterSectorsList.First(x => x.SectorId == sectorId);
            var list = isFinal ? sector.BossGigEncounterList : sector.GigEncounterList;

            if (list == null || list.Count == 0) return null;
            if (encounterId >= 0 && encounterId < list.Count) return list[encounterId];

            // Fallback to random if index is out of range.
            return list.RandomItem();
        }
    }

    [Serializable]
    public class GigEncounterSector
    {
        [SerializeField] private string sectorName;
        [SerializeField] private int sectorId;
        [SerializeField] private List<GigEncounter> gigEncountersList;
        [SerializeField] private List<GigEncounter> bossGigEncounterList;

        #region Encapsulation
        public string SectorName => sectorName;
        public int SectorId => sectorId;
        public List<GigEncounter> GigEncounterList => gigEncountersList;
        public List<GigEncounter> BossGigEncounterList => bossGigEncounterList;
        #endregion
    }
}