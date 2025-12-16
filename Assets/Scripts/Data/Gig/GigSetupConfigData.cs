using ALWTTT.Cards;
using ALWTTT.Encounters;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "GigSetupConfig", menuName = "ALWTTT/Gig/GigSetupConfig")]
    public class GigSetupConfigData : ScriptableObject
    {
        [Header("Selectable Content")]
        [SerializeField] private List<BandDeckData> availableBandDecks = new();
        [SerializeField] private List<GigEncounterSO> availableEncounters = new();

        [Header("Default Values (Setup Only)")]
        [SerializeField] private int defaultInitialGigInspiration = 0;
        [SerializeField] private int defaultInspirationPerLoop = 0;

        [SerializeField] private bool defaultDiscardHandBetweenTurns = false;
        [SerializeField] private bool defaultKeepInspirationBetweenTurns = false;

        [SerializeField] private bool allowOverrideRequiredSongCount = true;
        [SerializeField, Min(1)] private int defaultRequiredSongCount = 1;

        public IReadOnlyList<BandDeckData> AvailableBandDecks => availableBandDecks;
        public IReadOnlyList<GigEncounterSO> AvailableEncounters => availableEncounters;

        public int DefaultStartingInspiration => defaultInitialGigInspiration;
        public int DefaultInspirationPerLoop => defaultInspirationPerLoop;
        public int DefaultInitialGigInspiration => defaultInitialGigInspiration;

        public bool DefaultDiscardHandBetweenTurns => defaultDiscardHandBetweenTurns;
        public bool DefaultKeepInspirationBetweenTurns => defaultKeepInspirationBetweenTurns;

        public bool AllowOverrideRequiredSongCount => allowOverrideRequiredSongCount;
        public int DefaultRequiredSongCount => Mathf.Max(1, defaultRequiredSongCount);
    }
}