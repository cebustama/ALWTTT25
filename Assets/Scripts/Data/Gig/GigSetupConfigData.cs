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

        // M4.6-prep batch (2): generic ("Owner: Any") cards added to every
        // auto-assembled starter deck regardless of selected band roster.
        // Optional - null is valid (no generics added).
        [Header("Generic Starter Cards (M4.6-prep batch 2)")]
        [SerializeField] private GenericCardCatalogSO genericStarterCatalog;

        // M4.6-prep merged (1)/(4): selectable audience pool for the audience picker.
        // The audience picker shows the union of this list and the currently-selected
        // encounter's AudienceMemberList, so encounter-defined audiences always appear
        // even if absent from this pool. Default selection on encounter pick is the
        // encounter's AudienceMemberList; the picker can deviate from there.
        [Header("Roster Pickers (M4.6-prep merged 1/4)")]
        [SerializeField] private List<AudienceCharacterData> availableAudienceCharacters = new();

        [Tooltip("Mirror of the GigScene's AudienceMemberPosList.Count. " +
                 "Audience picker validates against this at Start press; selecting " +
                 "more than this count blocks gig start with a clear message. " +
                 "Set to match the actual scene's position-list size.")]
        [SerializeField, Min(1)] private int maxAudienceCount = 4;

        [Header("Default Values (Setup Only)")]
        [SerializeField] private int defaultInitialGigInspiration = 0;
        [SerializeField] private int defaultInspirationPerLoop = 0;

        [SerializeField] private bool defaultDiscardHandBetweenTurns = false;
        [SerializeField] private bool defaultKeepInspirationBetweenTurns = false;

        [SerializeField] private bool allowOverrideRequiredSongCount = true;
        [SerializeField, Min(1)] private int defaultRequiredSongCount = 1;

        public IReadOnlyList<BandDeckData> AvailableBandDecks => availableBandDecks;
        public IReadOnlyList<GigEncounterSO> AvailableEncounters => availableEncounters;

        public GenericCardCatalogSO GenericStarterCatalog => genericStarterCatalog;

        public IReadOnlyList<AudienceCharacterData> AvailableAudienceCharacters
            => availableAudienceCharacters;

        public int MaxAudienceCount => Mathf.Max(1, maxAudienceCount);

        public int DefaultStartingInspiration => defaultInitialGigInspiration;
        public int DefaultInspirationPerLoop => defaultInspirationPerLoop;
        public int DefaultInitialGigInspiration => defaultInitialGigInspiration;

        public bool DefaultDiscardHandBetweenTurns => defaultDiscardHandBetweenTurns;
        public bool DefaultKeepInspirationBetweenTurns => defaultKeepInspirationBetweenTurns;

        public bool AllowOverrideRequiredSongCount => allowOverrideRequiredSongCount;
        public int DefaultRequiredSongCount => Mathf.Max(1, defaultRequiredSongCount);
    }
}