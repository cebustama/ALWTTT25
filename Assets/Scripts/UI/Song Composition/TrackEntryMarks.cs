using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.UI
{
    /// <summary>
    /// [S10] Who ENTERS the loop now playing, and which combo each of them completes.
    /// Written by GigManager from the same combo evaluation that scores the loop
    /// (COMBOS-1: previsto = real), read by every <see cref="SongTrackElementUI"/> row.
    /// Static so a row rebuilt mid-loop picks the marks up on Bind.
    /// </summary>
    public static class TrackEntryMarks
    {
        private static readonly HashSet<string> _entering = new HashSet<string>();
        private static readonly Dictionary<string, string> _combo = new Dictionary<string, string>();
        private static string _signature = "";

        public static event Action Changed;

        public static bool IsEntering(string musicianId) =>
            !string.IsNullOrEmpty(musicianId) && _entering.Contains(musicianId);

        /// <summary>The combo this musician's entry completes, or null.</summary>
        public static string ComboOf(string musicianId) =>
            musicianId != null && _combo.TryGetValue(musicianId, out var n) ? n : null;

        public static void Set(IEnumerable<string> entering, IReadOnlyList<string> handNames,
            IReadOnlyList<List<string>> handMusicians)
        {
            var e = new List<string>();
            if (entering != null) e.AddRange(entering);
            var c = new Dictionary<string, string>();
            if (handNames != null && handMusicians != null)
                for (int i = 0; i < handNames.Count && i < handMusicians.Count; i++)
                    if (handMusicians[i] != null)
                        foreach (var id in handMusicians[i])
                            if (!string.IsNullOrEmpty(id) && !c.ContainsKey(id)) c[id] = handNames[i];

            e.Sort(StringComparer.Ordinal);
            var parts = new List<string>();
            foreach (var id in e) parts.Add(c.TryGetValue(id, out var n) ? $"{id}({n})" : id);
            // [S10] A combo partner that was already playing is marked too (orange row).
            var already = new List<string>();
            foreach (var kv in c) if (!e.Contains(kv.Key)) already.Add($"{kv.Key}({kv.Value}, ya sonaba)");
            already.Sort(StringComparer.Ordinal);
            parts.AddRange(already);
            string sig = string.Join(", ", parts);
            if (sig == _signature) return;

            _signature = sig;
            _entering.Clear();
            foreach (var id in e) _entering.Add(id);
            _combo.Clear();
            foreach (var kv in c) _combo[kv.Key] = kv.Value;

            if (sig.Length > 0)
                Debug.Log($"[S10] Pistas que entran este loop: {sig}");
            Changed?.Invoke();
        }

        public static void Clear() => Set(null, null, null);
    }
}
