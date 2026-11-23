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

        /// <summary>[MUSICA-1c, Claudio 2026-11-23] Weights of the octave the style sings in,
        /// drawn per song and part next to the style: [one down, as is, one up]. (0, 1, 0) =
        /// the profile's own register. «Que sea menos común cantar tan grave.»</summary>
        public Vector3 octaveWeights = new Vector3(0f, 1f, 0f);

        static readonly Dictionary<(int, int), VoiceProfileSO> Shifted = new Dictionary<(int, int), VoiceProfileSO>();

        /// <summary>The profile sung <paramref name="octaves"/> octaves away: a hidden runtime
        /// copy with the transpose moved (cached; the asset is never touched).</summary>
        public static VoiceProfileSO Shift(VoiceProfileSO p, int octaves)
        {
            if (p == null || octaves == 0) return p;
            var key = (p.GetInstanceID(), octaves);
            if (Shifted.TryGetValue(key, out var c) && c != null) return c;
            c = Instantiate(p);
            c.hideFlags = HideFlags.HideAndDontSave;
            c.name = p.name + (octaves > 0 ? " +" : " ") + (12 * octaves);
            c.transposeSemitones = p.transposeSemitones + 12 * octaves;
            Shifted[key] = c;
            return c;
        }

        /// <summary>The octave for (song seed, part), from <see cref="octaveWeights"/>: −1, 0 or +1.
        /// Its own salt, so the style pick is unchanged.</summary>
        public int PickOctave(int songSeed, int partIndex)
        {
            float a = Mathf.Max(0f, octaveWeights.x), b = Mathf.Max(0f, octaveWeights.y), u = Mathf.Max(0f, octaveWeights.z);
            double total = a + b + u;
            if (total <= 0 || (a == 0 && u == 0)) return 0;
            var r = SyllablePlanner.Rng(songSeed ^ (partIndex * 7919), "voz-octava");
            double x = r.Next() * total;
            if (x < a) return -1;
            return x < a + b ? 0 : 1;
        }

        /// <summary>Weighted pick by a stable hash of (song seed, part). Null when empty.</summary>
        public VoiceProfileSO Pick(int songSeed, int partIndex) =>
            Shift(PickStyle(songSeed, partIndex), PickOctave(songSeed, partIndex));

        /// <summary>The style alone, without the octave.</summary>
        public VoiceProfileSO PickStyle(int songSeed, int partIndex)
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
