using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    [Serializable]
    public class SerializableStringIntDictionary : ISerializationCallbackReceiver
    {
        [SerializeField] private List<string> keys = new();
        [SerializeField] private List<int> values = new();

        private Dictionary<string, int> dict = new();

        public int this[string key]
        {
            get => dict[key];
            set => dict[key] = value;
        }

        public bool TryGetValue(string key, out int value) => dict.TryGetValue(key, out value);

        public void OnBeforeSerialize()
        {
            keys.Clear(); values.Clear();
            foreach (var kv in dict) { keys.Add(kv.Key); values.Add(kv.Value); }
        }

        public void OnAfterDeserialize()
        {
            dict.Clear();
            var n = Mathf.Min(keys.Count, values.Count);
            for (int i = 0; i < n; i++) dict[keys[i]] = values[i];
        }
    }
}