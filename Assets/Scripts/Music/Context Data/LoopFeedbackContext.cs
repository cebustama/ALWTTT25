using ALWTTT.Characters.Audience;
using ALWTTT.Enums;
using MidiGenPlay;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALWTTT.Music
{
    [Serializable]
    public readonly struct LoopTrackSnapshot
    {
        public string MusicianId { get; }
        public TrackRole Role { get; }
        public CardType SynergyType { get; }
        public int InspirationGenerated { get; }
        public string Info { get; }

        public LoopTrackSnapshot(
            string musicianId,
            TrackRole role,
            CardType synergyType,
            int inspirationGenerated,
            string info)
        {
            MusicianId = musicianId ?? string.Empty;
            Role = role;
            SynergyType = synergyType;
            InspirationGenerated = inspirationGenerated;
            Info = info ?? string.Empty;
        }

        public override string ToString()
        {
            return $"[{Role}] {MusicianId} ({SynergyType}) +{InspirationGenerated} '{Info}'";
        }
    }

    /// <summary>
    /// Snapshot of information about a loop that just finished playing.
    /// This is what gets broadcast to audience members so they can
    /// evaluate it and turn it into an impression (-2..2).
    /// </summary>
    public readonly struct LoopFeedbackContext
    {
        public int PartIndex { get; }
        public int LoopIndexWithinPart { get; }
        public int LoopsInPart { get; }

        public string PartLabel { get; }

        public int InspirationGainedThisLoop { get; }
        public int InspirationAfterLoop { get; }

        /// <summary>Arrangement snapshot for this loop.</summary>
        public IReadOnlyList<LoopTrackSnapshot> Tracks { get; }
        /// <summary>Per-audience impression values (-2..2).</summary>

        // --- Helpers ---

        public int ActiveTracks => Tracks?.Count ?? 0;
        public int TotalComplexity => Tracks?.Sum(t => t.InspirationGenerated) ?? 0;

        public bool HasRole(TrackRole role) =>
            Tracks != null && Tracks.Any(t => t.Role == role);

        public bool HasRhythm => HasRole(TrackRole.Rhythm);
        public bool HasBass => HasRole(TrackRole.Bassline);
        public bool HasMelody => HasRole(TrackRole.Melody);
        public bool HasHarmony => HasRole(TrackRole.Harmony);

        public bool IsLastLoopOfPart => LoopIndexWithinPart == LoopsInPart - 1;
        // ======

        public LoopFeedbackContext(
            int partIndex,
            int loopIndexWithinPart,
            int loopsInPart,
            string partLabel,
            int inspirationGainedThisLoop,
            int inspirationAfterLoop,
            IReadOnlyList<LoopTrackSnapshot> tracks)
        {
            PartIndex = partIndex;
            LoopIndexWithinPart = loopIndexWithinPart;
            LoopsInPart = loopsInPart;
            PartLabel = partLabel ?? $"Part {partIndex}";
            InspirationGainedThisLoop = inspirationGainedThisLoop;
            InspirationAfterLoop = inspirationAfterLoop;
            Tracks = tracks ?? Array.Empty<LoopTrackSnapshot>();
        }

        public override string ToString()
        {
            return $"[LoopFeedback] Part={PartIndex} ({PartLabel}) " +
                   $"Loop={LoopIndexWithinPart + 1}/{LoopsInPart} " +
                   $"Tracks={ActiveTracks} ΔInsp={InspirationGainedThisLoop} " +
                   $"Total={InspirationAfterLoop}";
        }
    }
}