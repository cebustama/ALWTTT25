using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ALWTTT.Cards
{
    /// <summary>
    /// A band's deck definition.
    ///
    /// M4.4 Deck Contract Evolution
    /// ============================
    /// The deck is a multiset of <see cref="BandDeckEntry"/> values: each entry
    /// pairs a <see cref="CardDefinition"/> with a copy count. Runtime
    /// materialization (see <see cref="ALWTTT.Data.PersistentGameplayData.SetBandDeck"/>)
    /// expands counts into independent references in the action / composition
    /// pile lists.
    ///
    /// Migration from the pre-M4.4 contract is lazy:
    ///   - Pre-M4.4 assets serialized a flat <c>List&lt;CardDefinition&gt; cards</c>.
    ///     That field is preserved here as <c>legacyCards</c> via
    ///     <see cref="FormerlySerializedAsAttribute"/>, so existing assets
    ///     deserialize without manual touch.
    ///   - <see cref="Entries"/> returns the new <see cref="entries"/> list when
    ///     populated, otherwise materializes a count-1 view from
    ///     <c>legacyCards</c> on access.
    ///   - The Deck Editor's save path writes <see cref="entries"/> and clears
    ///     <c>legacyCards</c> on every save, so an asset upgrades the first time
    ///     it is saved through the editor.
    /// </summary>
    [CreateAssetMenu(menuName = "ALWTTT/Decks/BandDeck")]
    public class BandDeckData : ScriptableObject
    {
        [SerializeField] private string deckId;
        [SerializeField] private string displayName;

        [TextArea]
        [SerializeField] private string description;

        // M4.4 — primary storage. List of (card, count) entries.
        [SerializeField] private List<BandDeckEntry> entries = new List<BandDeckEntry>();

        // M4.4 — legacy migration field. Pre-M4.4 assets serialized this as
        // "cards". The FormerlySerializedAs attribute keeps existing assets
        // readable without re-authoring. This field is cleared by the Deck
        // Editor on the first save through the editor.
        [SerializeField, FormerlySerializedAs("cards")]
        private List<CardDefinition> legacyCards;

        /// <summary>
        /// The deck's entries, as authored. Returns <see cref="entries"/> when
        /// populated; otherwise materializes a count-1 view from the legacy
        /// pre-M4.4 <c>cards</c> field. The legacy view is computed at access
        /// and is not written back to the asset; the upgrade happens the next
        /// time the deck is saved through the Deck Editor.
        /// </summary>
        public IReadOnlyList<BandDeckEntry> Entries
        {
            get
            {
                if (entries != null && entries.Count > 0)
                    return entries;

                if (legacyCards == null || legacyCards.Count == 0)
                    return System.Array.Empty<BandDeckEntry>();

                var view = new List<BandDeckEntry>(legacyCards.Count);
                for (int i = 0; i < legacyCards.Count; i++)
                {
                    var c = legacyCards[i];
                    if (c == null) continue;
                    view.Add(new BandDeckEntry { card = c, count = 1 });
                }
                return view;
            }
        }

        /// <summary>
        /// Enumerates the deck as a flat sequence of <see cref="CardDefinition"/>
        /// references with multiplicity expanded (a count-3 entry yields the
        /// same card three times). Honors the legacy <c>cards</c> field via
        /// <see cref="Entries"/>.
        ///
        /// Use this when a consumer needs "all cards in the deck" as a flat
        /// list — e.g. routing into separate Action / Composition piles.
        /// <see cref="ALWTTT.Data.PersistentGameplayData.SetBandDeck"/> works
        /// directly off <see cref="Entries"/> instead because it computes
        /// per-domain totals; this helper is for callers that don't.
        /// </summary>
        public IEnumerable<CardDefinition> EnumerateCards()
        {
            var src = Entries;
            for (int i = 0; i < src.Count; i++)
            {
                var entry = src[i];
                if (entry == null || entry.card == null) continue;
                int copies = entry.count < 1 ? 1 : entry.count;
                for (int k = 0; k < copies; k++)
                    yield return entry.card;
            }
        }
    }
}