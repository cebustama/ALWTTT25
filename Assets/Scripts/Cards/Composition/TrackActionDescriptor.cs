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
    }
}


