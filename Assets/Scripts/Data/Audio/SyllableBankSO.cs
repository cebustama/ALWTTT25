using System;
using System.Collections.Generic;
using ALWTTT.Music.Voice.V8;
using UnityEngine;

namespace ALWTTT.Music.Voice
{
    /// <summary>
    /// [VOZ-2] What the singer v8 says (D-VOZ-4 = A): the zigués (Zig's invented language,
    /// from a seed and a hardness), a weighted syllable list, or a fixed text. Each
    /// VoiceProfileSO points to one bank. Same rules and draws as the Taller de melodías
    /// (motor-canto.js zigues / asignarZigues / asignarSilabas), so a bank sounds the same
    /// in the workshop and in the game.
    ///
    /// Seeds (D2 = A): the lexicon comes from <see cref="lexiconSeed"/> (the language, the same
    /// in every song); the verse — which words, in which order — from the song seed and the
    /// part, so each part has its words and its loops repeat them like a chorus.
    /// </summary>
    [CreateAssetMenu(fileName = "SyllableBank", menuName = "ALWTTT/Audio/Syllable Bank")]
    public class SyllableBankSO : ScriptableObject
    {
        public enum Mode { Zigues = 0, WeightedList = 1, Text = 2 }

        [Serializable]
        public struct Entry
        {
            public string syllable;
            [Min(0f)] public float weight;
            [Tooltip("The «fin» syllable: sung on long phrase ends (one per bank; the first wins).")]
            public bool isEnd;
        }

        public Mode mode = Mode.Zigues;

        [Header("Zigués")]
        [Tooltip("The language. Same seed = same 9 words in every song (and in the workshop).")]
        public int lexiconSeed = 4242;
        [Tooltip("0 = l m n y w (tender); 1 = k t x z g d r and hard codas (punk, death metal).")]
        [Range(0f, 1f)] public float hardness = 0.5f;

        [Header("Weighted list")]
        public List<Entry> entries = new List<Entry>();

        [Header("Text")]
        [Tooltip("Syllables separated by spaces, commas or hyphens; they cycle over the notes.")]
        [TextArea] public string text = "";

        [Header("Phrase ends (VOZ-2 D1)")]
        [Tooltip("Default 2 (D8 = B). 0 = rule B: a phrase ends after a rest of 1 beat or on a note of 2+ beats. " +
                 "2 = rule F: the same, but only on an even bar line (fewer cut words; " +
                 "68 % vs 30 % precision against the workshop's composer).")]
        [Range(0, 8)] public int phraseGridBars = 2;   // D8 = B (2026-11-14): rule F by default

        public List<WeightedSyllable> ToWeighted()
        {
            var list = new List<WeightedSyllable>(entries.Count);
            foreach (var e in entries)
                if (!string.IsNullOrWhiteSpace(e.syllable) && e.weight > 0f)
                    list.Add(new WeightedSyllable(e.syllable.Trim(), e.weight, e.isEnd));
            return list;
        }
    }
}
