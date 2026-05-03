using System;
using UnityEngine;

namespace ALWTTT.Cards
{
    /// <summary>
    /// A single (card, count) entry in a <see cref="BandDeckData"/>.
    ///
    /// Introduced in M4.4 as part of Deck Contract Evolution. A deck is a
    /// multiset of these entries: each entry contributes <see cref="count"/>
    /// independent references to the runtime piles when
    /// <see cref="ALWTTT.Data.PersistentGameplayData.SetBandDeck"/> materializes
    /// the deck.
    ///
    /// Identity is the <see cref="card"/> reference. Two entries pointing to the
    /// same <see cref="CardDefinition"/> in the same deck is not the intended
    /// shape — the Deck Editor and the JSON importer combine them on save.
    /// </summary>
    [Serializable]
    public sealed class BandDeckEntry
    {
        [SerializeField] public CardDefinition card;

        [SerializeField, Min(1)]
        public int count = 1;
    }
}