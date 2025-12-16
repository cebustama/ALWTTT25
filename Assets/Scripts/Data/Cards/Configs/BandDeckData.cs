using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(menuName = "ALWTTT/Decks/BandDeck")]
    public class BandDeckData : ScriptableObject
    {
        [SerializeField] private string deckId;
        [SerializeField] private string displayName;

        [TextArea]
        [SerializeField] private string description;

        [SerializeField] private List<CardData> cards;

        public IReadOnlyList<CardData> Cards => cards;
    }
}

