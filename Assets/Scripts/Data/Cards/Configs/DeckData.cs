using System;
using System.Collections.Generic;
using UnityEngine;
using static ALWTTT.Cards.CardData;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(fileName = "New DeckData", menuName = "ALWTTT/Cards/DeckData")]
    public class DeckData : ScriptableObject
    {
        [SerializeField] private string deckID;
        [SerializeField] private string deckName;

        [SerializeField] private CardDomain deckDomain = CardDomain.Action;

        [SerializeField] private List<CardData> cardList;

        public List<CardData> CardList => cardList;

        public string DeckID => deckID;
        public string DeckName => deckName;
        public CardDomain DeckDomain => deckDomain;

        public IReadOnlyList<CardData> GetValidCards()
        {
            if (cardList == null) return Array.Empty<CardData>();
            List<CardData> filtered = new();
            foreach (var c in cardList)
                if (c != null && c.Domain == deckDomain) filtered.Add(c);
            return filtered;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (cardList == null) return;
            for (int i = cardList.Count - 1; i >= 0; --i)
            {
                var c = cardList[i];
                if (c == null) continue;
                if (c.Domain != deckDomain)
                {
                    Debug.LogWarning($"[DeckData:{name}] Removing card '{c.name}' " +
                        $"because its Domain={c.Domain} != DeckDomain={deckDomain}");
                    cardList.RemoveAt(i);
                }
            }
        }
#endif
    }
}