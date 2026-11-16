using MidiGenPlay;
using MidiGenPlay.Composition;
using System;
using UnityEngine;

namespace ALWTTT.Cards
{
    /// <summary>
    /// Identifies which track role the card targets (Rhythm/Backing/Bassline/Melody/Harmony).
    /// </summary>
    [Serializable]
    public class TrackActionDescriptor
    {
        public TrackRole role = TrackRole.Rhythm;

        [Tooltip("Optional style bundle for this track (recipe/strategy/archetypes).")]
        public TrackStyleBundleSO styleBundle;

        // [VOZ-2 / D6 = B] Melody cards only: the voice styles the singer may use while this
        // card's melody plays (one picked per song and part). Empty = the musician's own voice.
        [Tooltip("Melody only. Voice styles for the singer (weighted; one per song and part). Empty = the musician's voice.")]
        public ALWTTT.Music.Voice.VoiceStylePaletteSO voicePalette;
    }
}


