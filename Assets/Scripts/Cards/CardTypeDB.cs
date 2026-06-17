using ALWTTT.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT
{
    /// <summary>
    /// Bridge enum ↔ data at runtime
    /// </summary>
    public static class CardTypeDB
    {
        private static readonly Dictionary<CardType, CardTypeData> dict
            = new Dictionary<CardType, CardTypeData>();

        // runs once on domain reload
        static CardTypeDB()
        {
            foreach (var data in Resources.LoadAll<CardTypeData>("Data/CardTypes"))
            {
                if (dict.ContainsKey(data.CardType))
                    Debug.LogError($"Duplicate CardTypeData for {data.CardType} – asset {data.name}");
                else
                    dict.Add(data.CardType, data);
            }
        }

        public static CardTypeData Get(CardType ct) => dict[ct];
    }
}