using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Cards
{
    /// <summary>
    /// Generic (non-per-musician) card catalogue contributed to every gig's
    /// auto-assembled starter deck regardless of selected band roster.
    ///
    /// M4.6-prep batch (2): introduced as the home for "Owner: Any" starter
    /// cards (e.g. Warm Up, Take Five) per Design_Starter_Deck_v1.md §4. The
    /// per-musician path (<see cref="MusicianCardCatalogData"/>) covers
    /// musician-identity cards; this SO covers generics that should appear in
    /// every starter deck regardless of which musicians are in the band.
    ///
    /// Plugged into the gig setup via
    /// <see cref="ALWTTT.Data.GigSetupConfigData.GenericStarterCatalog"/>.
    /// Consumed by
    /// <see cref="ALWTTT.Data.PersistentGameplayData.SetBandDeckFromMusicians"/>.
    ///
    /// Reuses <see cref="MusicianCardEntry"/> as the entry type — there is no
    /// musician-specific concept on the entry itself (the per-musician scoping
    /// lives on <see cref="MusicianCardCatalogData.MusicianType"/>, not on the
    /// entry). Renaming the entry type to a more neutral name was rejected as
    /// out-of-scope cross-cutting churn for batch (2).
    ///
    /// Provenance: generic-catalogue contributions are NOT recorded in
    /// <c>musicianGrantedActionCards</c>/<c>musicianGrantedCompositionCards</c>.
    /// They are not "from" any specific musician, so
    /// <c>RemoveMusicianFromBand</c> correctly leaves them in the deck when a
    /// musician departs mid-run.
    /// </summary>
    [CreateAssetMenu(fileName = "New GenericCardCatalog",
        menuName = "ALWTTT/Cards/Generic Card Catalog")]
    public class GenericCardCatalogSO : ScriptableObject
    {
        [SerializeField] private List<MusicianCardEntry> entries = new();

        public List<MusicianCardEntry> Entries => entries;
    }
}