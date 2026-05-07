using ALWTTT.Cards;
using ALWTTT.Encounters;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ALWTTT.Data
{
    /// <summary>
    /// Selectable roster content surfaced on the Gig Setup screen. Renamed
    /// from <c>GigSetupConfigData</c> (M4.6F-2). The former "Default Values"
    /// section moved to <see cref="GigFlowSettingsSO"/>; this SO now carries
    /// only roster content (decks, encounters, audience pool, generic
    /// starter catalog, and audience-count cap).
    ///
    /// Authority: SSoT_Gig_Encounter.
    /// </summary>
    [MovedFrom(autoUpdateAPI: true,
               sourceClassName: "GigSetupConfigData",
               sourceNamespace: "ALWTTT.Data")]
    [CreateAssetMenu(
        fileName = "GigSetupRoster",
        menuName = "ALWTTT/Gig/GigSetupRoster",
        order = 14)]
    public class GigSetupRosterSO : ScriptableObject
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

        public IReadOnlyList<BandDeckData> AvailableBandDecks => availableBandDecks;
        public IReadOnlyList<GigEncounterSO> AvailableEncounters => availableEncounters;

        public GenericCardCatalogSO GenericStarterCatalog => genericStarterCatalog;

        public IReadOnlyList<AudienceCharacterData> AvailableAudienceCharacters
            => availableAudienceCharacters;

        public int MaxAudienceCount => Mathf.Max(1, maxAudienceCount);
    }
}