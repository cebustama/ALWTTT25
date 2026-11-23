// [VOZ-2] Syllables for the singer v8: phonetics (CONS / VOCALES / parseSilaba), the
// zigués generator (zigues / asignarZigues), the weighted-list and text assignment
// (asignarSilabas) and the game's phrase-end rule (D1 = B). Port of motor-canto.js,
// bit-for-bit on the random draws (FNV-1a salt + mulberry32), so the same seeds give
// the same words in the workshop and in the game. Pure C#; main thread only (allocates).
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ALWTTT.Music.Voice.V8
{
    public enum ConsKind { Stop, Nasal, Fricative, Approximant, Trill, Aspirate }

    public readonly struct ConsSpec
    {
        public readonly double Index, Diameter; public readonly bool Voiced, HasIndex; public readonly ConsKind Kind;
        public ConsSpec(double i, double d, bool voiced, ConsKind k, bool hasIndex = true)
        { Index = i; Diameter = d; Voiced = voiced; Kind = k; HasIndex = hasIndex; }
    }

    public readonly struct VowelSpec
    {
        public readonly double Index, Diameter, Lip; public readonly bool HasLip;
        public VowelSpec(double i, double d, double lip, bool hasLip) { Index = i; Diameter = d; Lip = lip; HasLip = hasLip; }
    }

    /// <summary>Parsed syllable. Consonant/vowel fields are table indices, -1 = none.</summary>
    public struct ParsedSyllable
    {
        public int Onset0, Onset1, Vowel, Vowel2, Coda;
        public bool IsHum;   // «mm»: closed lips + open velum, sung on «u»
    }

    public static class Phonetics
    {
        // Place of articulation in the 44-section tract index (motor-canto.js CONS).
        public const string ConsLetters = "bpdtgkmnszxfvlwyrh";
        public static readonly ConsSpec[] Cons =
        {
            new ConsSpec(41, 0, true, ConsKind.Stop), new ConsSpec(41, 0, false, ConsKind.Stop),        // b p
            new ConsSpec(36, 0, true, ConsKind.Stop), new ConsSpec(36, 0, false, ConsKind.Stop),        // d t
            new ConsSpec(21, 0, true, ConsKind.Stop), new ConsSpec(21, 0, false, ConsKind.Stop),        // g k
            new ConsSpec(41, 0, true, ConsKind.Nasal), new ConsSpec(36, 0, true, ConsKind.Nasal),       // m n
            new ConsSpec(36, 0.55, false, ConsKind.Fricative), new ConsSpec(36, 0.55, true, ConsKind.Fricative), // s z
            new ConsSpec(31, 0.55, false, ConsKind.Fricative), new ConsSpec(41, 0.5, false, ConsKind.Fricative), // x f
            new ConsSpec(41, 0.5, true, ConsKind.Fricative),                                             // v
            new ConsSpec(38, 0.9, true, ConsKind.Approximant), new ConsSpec(41, 0.6, true, ConsKind.Approximant), // l w
            new ConsSpec(30, 0.9, true, ConsKind.Approximant),                                           // y
            new ConsSpec(36, 0, true, ConsKind.Trill),                                                   // r
            new ConsSpec(0, 0, false, ConsKind.Aspirate, hasIndex: false),                               // h
        };

        // Spanish vowels calibrated against Quilis (paridad/vocales.mjs). 'ə' has no lip constriction.
        public const string VowelLetters = "aeiouə";
        public static readonly VowelSpec[] Vowels =
        {
            new VowelSpec(18, 2.7, 1.2, true), new VowelSpec(29, 3.15, 0.9, true), new VowelSpec(29, 2.4, 0.9, true),
            new VowelSpec(11, 2.1, 0.9, true), new VowelSpec(13, 2.1, 0.5, true), new VowelSpec(12.9, 2.43, 0, false),
        };
        public const int Schwa = 5, VowelU = 4;

        public static int ConsOf(char c) => ConsLetters.IndexOf(c);
        public static int VowelOf(char c) => VowelLetters.IndexOf(c);

        static string ReplaceFirst(string s, string a, string b)
        {
            int i = s.IndexOf(a, StringComparison.Ordinal);
            return i < 0 ? s : s.Substring(0, i) + b + s.Substring(i + a.Length);
        }

        /// <summary>motor-canto.js parseSilaba. JS String.replace(str, str) replaces the FIRST match only.</summary>
        public static ParsedSyllable Parse(string s)
        {
            s = (s ?? "").ToLowerInvariant();
            s = ReplaceFirst(s, "sh", "x"); s = ReplaceFirst(s, "ch", "x"); s = ReplaceFirst(s, "ñ", "n");
            var p = new ParsedSyllable { Onset0 = -1, Onset1 = -1, Vowel = -1, Vowel2 = -1, Coda = -1 };
            if (s == "mm" || s == "m") { p.IsHum = true; p.Vowel = VowelU; return p; }
            int j = 0, n = 0;
            while (j < s.Length && ConsOf(s[j]) >= 0 && VowelOf(s[j]) < 0)
            {
                if (n == 0) p.Onset0 = ConsOf(s[j]); else if (n == 1) p.Onset1 = ConsOf(s[j]);
                n++; j++;
            }
            int vowels = 0;
            for (int k = j; k < s.Length; k++)
            {
                int v = VowelOf(s[k]);
                if (v < 0) continue;
                if (vowels == 0) p.Vowel = v; else if (vowels == 1) p.Vowel2 = v;
                vowels++;
            }
            if (p.Vowel < 0) p.Vowel = Schwa;
            for (int k = s.Length - 1; k >= j; k--)
                if (ConsOf(s[k]) >= 0 && VowelOf(s[k]) < 0) { p.Coda = ConsOf(s[k]); break; }
            return p;
        }
    }

    /// <summary>mulberry32, bit-for-bit motor-voz.js.</summary>
    public sealed class Mulberry32Rng
    {
        uint a;
        public Mulberry32Rng(int seed) { a = unchecked((uint)seed); }
        public double Next()
        {
            unchecked
            {
                a += 0x6D2B79F5;
                uint t = (a ^ (a >> 15)) * (1u | a);
                t = (t + (t ^ (t >> 7)) * (61u | t)) ^ t;
                return (t ^ (t >> 14)) / 4294967296.0;
            }
        }
        public int Index(int length) => (int)Math.Floor(Next() * length);
    }

    public struct WeightedSyllable
    {
        public string Syllable; public double Weight; public bool IsEnd;
        public WeightedSyllable(string s, double w, bool end = false) { Syllable = s; Weight = w; IsEnd = end; }
    }

    public static class SyllablePlanner
    {
        /// <summary>motor-canto.js hash: FNV-1a over the UTF-16 units, as int.</summary>
        public static int Hash(string s)
        {
            unchecked
            {
                uint h = 2166136261;
                foreach (char c in s) { h ^= c; h *= 16777619; }
                return (int)h;
            }
        }

        /// <summary>motor-canto.js rngDe(seed, sal).</summary>
        public static Mulberry32Rng Rng(int seed, string salt) => new Mulberry32Rng(seed ^ Hash(salt));

        static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        static readonly string[] Soft = { "l", "m", "n", "y", "w", "s", "" };
        static readonly string[] Hard = { "k", "t", "x", "z", "g", "d", "r", "b" };
        static readonly string[] Clusters = { "kr", "tr", "gl", "bl", "kl", "gr", "zr" };
        static readonly string[] VowelPool = { "a", "e", "i", "o", "u", "u", "i", "o" };
        static readonly string[] Diphthongs = { "ue", "ia", "io", "ai", "oi", "au", "ui" };
        static readonly string[] CodaHard = { "x", "k", "s" };
        static readonly string[] CodaSoft = { "n", "s", "l" };

        /// <summary>motor-canto.js zigues: the 9-word lexicon of a language (seed + hardness 0..1).</summary>
        public static List<string[]> Zigues(int seed, double hardness)
        {
            var r = Rng(seed, "zig");
            double d = Clamp01(hardness);
            string Syl()
            {
                string on;
                double x = r.Next();
                if (x < 0.12 * d) on = Clusters[r.Index(Clusters.Length)];
                else if (x < 0.12 * d + 0.55 * d) on = Hard[r.Index(Hard.Length)];
                else on = Soft[r.Index(Soft.Length)];
                string v = r.Next() < 0.15 ? Diphthongs[r.Index(Diphthongs.Length)] : VowelPool[r.Index(VowelPool.Length)];
                string coda = "";
                if (r.Next() < 0.06 + 0.12 * d) coda = d > 0.5 ? CodaHard[r.Index(3)] : CodaSoft[r.Index(3)];
                return on + v + coda;
            }
            var lex = new List<string[]>(9);
            for (int i = 0; i < 9; i++)
            {
                int n = 1 + r.Index(3);
                var w = new string[n];
                for (int k = 0; k < n; k++) w[k] = Syl();
                if (n >= 2 && r.Next() < 0.3) w[1] = w[0];   // reduplication: «zuzu», «kalka»
                lex.Add(w);
            }
            return lex;
        }

        /// <summary>
        /// motor-canto.js asignarZigues with the seed split of D2 = A: the lexicon comes from
        /// <paramref name="lexiconSeed"/> (Zig's language, the same in every song) and the verse
        /// from <paramref name="verseSeed"/> (the song and part). The workshop passes one seed
        /// for both. Returns the lexicon words (for logs).
        /// </summary>
        /// <summary>[MUSICA-1c] The vowel a melisma keeps singing: the last vowel of the syllable
        /// (a hum or a vowel-less syllable is kept whole).</summary>
        public static string Nucleus(string syllable)
        {
            if (string.IsNullOrEmpty(syllable)) return "";
            for (int i = syllable.Length - 1; i >= 0; i--)
                if ("aeiouáéíóú".IndexOf(char.ToLowerInvariant(syllable[i])) >= 0) return syllable[i].ToString();
            return syllable;
        }

        public static string[] AssignZigues(SungNote[] notes, int lexiconSeed, double hardness, int verseSeed)
        {
            var lex = Zigues(lexiconSeed, hardness);
            var r = Rng(verseSeed, "zigfrase");
            var verse = new int[4];
            for (int i = 0; i < 4; i++) verse[i] = r.Index(lex.Count);
            var queue = new Queue<string>();
            int vi = 0;
            for (int i = 0; i < notes.Length; i++)
            {
                if (queue.Count == 0)
                {
                    int w = verse[vi % verse.Length];
                    if (r.Next() < 0.25) w = r.Index(lex.Count);
                    vi++;
                    foreach (var s in lex[w]) queue.Enqueue(s);
                }
                notes[i].Syllable = queue.Dequeue();
                if (notes[i].PhraseEnd && queue.Count > 0) queue.Clear();
            }
            var words = new string[lex.Count];
            for (int i = 0; i < lex.Count; i++) words[i] = string.Concat(lex[i]);
            return words;
        }

        static string Pick(Mulberry32Rng r, IReadOnlyList<WeightedSyllable> bank)
        {
            double t = 0;
            for (int i = 0; i < bank.Count; i++) t += bank[i].Weight;
            double x = r.Next() * t;
            for (int i = 0; i < bank.Count; i++) { x -= bank[i].Weight; if (x <= 0) return bank[i].Syllable; }
            return bank[bank.Count - 1].Syllable;
        }

        /// <summary>
        /// motor-canto.js asignarSilabas: a 4-syllable motif redrawn every 4 notes from the
        /// weighted bank, the bank's «fin» syllable on long phrase ends; or a fixed text that
        /// cycles. The game has no «firma» flag, so that branch never fires here.
        /// </summary>
        public static void AssignWeighted(SungNote[] notes, IReadOnlyList<WeightedSyllable> bank, int seed, string text)
        {
            var r = Rng(seed, "sil");
            string[] list = null;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var parts = Regex.Split(text.Trim(), @"[\s,\-]+");
                var keep = new List<string>();
                foreach (var p in parts) if (p.Length > 0) keep.Add(p);
                list = keep.ToArray();
            }
            string[] motif = null;
            string endSyl = null;
            if (bank != null) for (int i = 0; i < bank.Count; i++) if (bank[i].IsEnd) { endSyl = bank[i].Syllable; break; }
            for (int i = 0; i < notes.Length; i++)
            {
                if (list != null && list.Length > 0) { notes[i].Syllable = list[i % list.Length]; continue; }
                if (bank == null || bank.Count == 0) { notes[i].Syllable = ""; continue; }
                if (motif == null || i % 4 == 0) { motif = new string[4]; for (int k = 0; k < 4; k++) motif[k] = Pick(r, bank); }
                notes[i].Syllable = notes[i].PhraseEnd && notes[i].DurBeats >= 1
                    ? (endSyl ?? motif[3])
                    : motif[i % 4];
            }
        }

        // ---------------------------------------------------------------- phrase ends (D1 = B)
        public const double PhraseGapBeats = 1.0, PhraseLongBeats = 2.0, BeatEpsilon = 0.05;

        /// <summary>
        /// The game's stem has no finFrase. D1 = B (2026-11-14): a note ends a phrase when it is
        /// the last of the loop, when a rest of at least 1 beat follows it, or when it lasts
        /// 2 beats or more. <paramref name="gridBars"/> &gt; 0 adds the bar-grid filter measured in
        /// VOZ-2 (paridad/cantante/medir-regla.mjs): the note must also end on a multiple of
        /// that many bars (2 = rule F, 68 % precision vs 30 % for B; both 100 % recall against
        /// the workshop's composer). A phrase end of 1 beat or more also gets the forced fall
        /// when the profile has fall &gt; 0 (cantarV2: max(fall, 2) semitones). Returns how many ends.
        /// </summary>
        public static int MarkPhraseEnds(SungNote[] notes, double beatSeconds, bool profileFalls,
            int beatsPerBar = 4, int gridBars = 0)
        {
            int count = 0;
            double grid = gridBars > 0 ? gridBars * (double)beatsPerBar : 0;
            for (int i = 0; i < notes.Length; i++)
            {
                bool last = i == notes.Length - 1;
                double gapBeats = last ? double.PositiveInfinity : (notes[i + 1].Start - notes[i].End) / beatSeconds;
                bool end = last || gapBeats >= PhraseGapBeats - BeatEpsilon || notes[i].DurBeats >= PhraseLongBeats - BeatEpsilon;
                if (end && !last && grid > 0)
                {
                    double r = (notes[i].StartBeats + notes[i].DurBeats) % grid;
                    if (r < 0) r += grid;
                    end = r > grid - 1 - BeatEpsilon || r < BeatEpsilon;
                }
                notes[i].PhraseEnd = end;
                notes[i].Fall = end && profileFalls && notes[i].DurBeats >= 1 - BeatEpsilon;
                if (end) count++;
            }
            return count;
        }
    }
}
