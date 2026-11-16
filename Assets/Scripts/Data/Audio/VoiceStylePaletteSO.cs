using System;
using System.Collections.Generic;
using ALWTTT.Music.Voice.V8;
using UnityEngine;

namespace ALWTTT.Music.Voice
{
    /// <summary>
    /// [VOZ-2] A palette of voice styles (D6 = B, «las cartas son paletas»): a weighted list
    /// of VoiceProfileSO. A melody card points to a palette through its TrackActionDescriptor;
    /// the song seed and the part pick one style, so every part keeps its voice across its
    /// loops and the same pinned seed always sings the same. A one-entry palette is a fixed
    /// style.
    /// </summary>
    [CreateAssetMenu(fileName = "VoiceStylePalette", menuName = "ALWTTT/Audio/Voice Style Palette")]
    public class VoiceStylePaletteSO : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public VoiceProfileSO profile;
            [Min(0f)] public float weight;
        }

        public List<Entry> styles = new List<Entry>();

        /// <summary>Weighted pick by a stable hash of (song seed, part). Null when empty.</summary>
        public VoiceProfileSO Pick(int songSeed, int partIndex)
        {
            double total = 0;
            foreach (var e in styles) if (e.profile != null && e.weight > 0f) total += e.weight;
            if (total <= 0) return null;
            var r = SyllablePlanner.Rng(songSeed ^ (partIndex * 7919), "voz-paleta");
            double x = r.Next() * total;
            VoiceProfileSO last = null;
            foreach (var e in styles)
            {
                if (e.profile == null || e.weight <= 0f) continue;
                last = e.profile;
                x -= e.weight;
                if (x <= 0) return e.profile;
            }
            return last;
        }
    }
}
