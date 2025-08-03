using ALWTTT.Encounters;
using ALWTTT.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "New EncounterData", menuName = "ALWTTT/EncounterData")]
    public class EncounterData : ScriptableObject
    {
        [SerializeField] private List<GigEncounterSector> encounterSectorsList;
        [SerializeField] private bool randomGigs;

        public List<GigEncounterSector> EncounterSectorsList => encounterSectorsList;

        public GigEncounter GetGigEncounter(int sectorId = 0, int encounterId = 0, bool isFinal = false)
        {
            var selectedSector = EncounterSectorsList.First(x => x.SectorId == sectorId);
            if (isFinal) return selectedSector.BossGigEncounterList.RandomItem();

            ListExtentions.SetSeed((int)(UnityEngine.Random.value * 1000));

            return randomGigs ?
                selectedSector.GigEncounterList.RandomItem() :
                selectedSector.GigEncounterList[encounterId] ?? // If it exists
                    selectedSector.GigEncounterList.RandomItem(); // Else random
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