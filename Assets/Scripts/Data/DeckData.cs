using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT
{
    [CreateAssetMenu(fileName = "New DeckData", menuName = "ALWTTT/Cards/DeckData")]
    public class DeckData : ScriptableObject
    {
        [SerializeField] private string deckID;
        [SerializeField] private string deckName;

        [SerializeField] private List<CardData> cardList;

        public List<CardData> CardList => cardList;
    }
}