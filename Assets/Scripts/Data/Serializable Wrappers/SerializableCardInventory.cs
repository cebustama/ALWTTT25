using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Unity-serializable map: musicianId -> list of CardData
    /// </summary>
    [Serializable]
    public class SerializableCardInventory : ISerializationCallbackReceiver
    {
        [Serializable]
        public class CardList
        {
            public List<CardData> cards = new List<CardData>();
        }

        // Serialized backing
        [SerializeField] private List<string> keys = new List<string>();
        [SerializeField] private List<CardList> values = new List<CardList>();

        // Runtime map
        private readonly Dictionary<string, List<CardData>> dict = new Dictionary<string, List<CardData>>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(new CardList { cards = new List<CardData>(kvp.Value) });
            }
        }

        public void OnAfterDeserialize()
        {
            dict.Clear();
            var count = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
            {
                var id = keys[i];
                var list = values[i]?.cards ?? new List<CardData>();
                dict[id] = list;
            }
        }

        private List<CardData> Ensure(string musicianId)
        {
            if (!dict.TryGetValue(musicianId, out var list))
            {
                list = new List<CardData>();
                dict[musicianId] = list;
            }
            return list;
        }

        public void AddCard(string musicianId, CardData card)
        {
            if (string.IsNullOrEmpty(musicianId) || card == null) return;
            Ensure(musicianId).Add(card);
        }

        public void AddCards(string musicianId, IEnumerable<CardData> cards)
        {
            if (string.IsNullOrEmpty(musicianId) || cards == null) return;
            Ensure(musicianId).AddRange(cards);
        }

        public List<CardData> Get(string musicianId)
        {
            return !string.IsNullOrEmpty(musicianId) && dict.TryGetValue(musicianId, out var list)
                ? list
                : null;
        }

        public bool TryRemoveAll(string musicianId, out List<CardData> removed)
        {
            removed = null;
            if (string.IsNullOrEmpty(musicianId)) return false;
            if (!dict.TryGetValue(musicianId, out var list)) return false;
            removed = list;
            return dict.Remove(musicianId);
        }
    }
}
