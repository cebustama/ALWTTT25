using ALWTTT.Characters.Audience;
using ALWTTT.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using MidiGenPlay;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

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
    ///
    /// [B3] Extended with musical-identity fields (TempoScale, TimeSignature,
    /// RootNote, Tonality). These let audience taste preferences (see
    /// AudienceCharacterData.TastePreferences in B3-code-F) read the actual
    /// musical content of the played loop, not just the arrangement structure.
    /// Values come from the active PartEntry on SongCompositionUI.Model at
    /// loop-finished time. Effective BPM is package-side (MidiGenPlay resolves
    /// TempoRange × TempoScale internally); ALWTTT exposes TempoScale as the
    /// authoring-side multiplicative signal.
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

        // --- Musical identity (B3) ---

        /// <summary>
        /// Multiplicative tempo factor from cumulative TempoEffect ScaleFactor cards.
        /// 1.0 = authored default; > 1.0 = faster; &lt; 1.0 = slower.
        /// Primary signal for archetypes that prefer/dislike tempo intensity.
        /// </summary>
        public float TempoScale { get; }

        /// <summary>Time signature of the part this loop played (4/4, 3/4, 5/4, 6/8, ...).</summary>
        public TimeSignature TimeSignature { get; }

        /// <summary>Root note of the part's key (C, D, F#, ...).</summary>
        public NoteName RootNote { get; }

        /// <summary>Mode/tonality of the part (Ionian, Aeolian, Dorian, ...).</summary>
        public Tonality Tonality { get; }

        // --- Helpers ---

        public int ActiveTracks => Tracks?.Count ?? 0;
        public int TotalComplexity => Tracks?.Sum(t => t.InspirationGenerated) ?? 0;

        public bool HasRole(TrackRole role) =>
            Tracks != null && Tracks.Any(t => t.Role == role);

        public bool HasRhythm => HasRole(TrackRole.Rhythm);
        public bool HasBass => HasRole(TrackRole.Bassline);
        public bool HasMelody => HasRole(TrackRole.Melody);
        public bool HasHarmony => HasRole(TrackRole.Harmony);
        public bool HasBacking => HasRole(TrackRole.Backing);

        public bool IsLastLoopOfPart => LoopIndexWithinPart == LoopsInPart - 1;
        // ======

        public LoopFeedbackContext(
            int partIndex,
            int loopIndexWithinPart,
            int loopsInPart,
            string partLabel,
            int inspirationGainedThisLoop,
            int inspirationAfterLoop,
            IReadOnlyList<LoopTrackSnapshot> tracks,
            float tempoScale,
            TimeSignature timeSignature,
            NoteName rootNote,
            Tonality tonality)
        {
            PartIndex = partIndex;
            LoopIndexWithinPart = loopIndexWithinPart;
            LoopsInPart = loopsInPart;
            PartLabel = partLabel ?? $"Part {partIndex}";
            InspirationGainedThisLoop = inspirationGainedThisLoop;
            InspirationAfterLoop = inspirationAfterLoop;
            Tracks = tracks ?? Array.Empty<LoopTrackSnapshot>();
            TempoScale = tempoScale;
            TimeSignature = timeSignature;
            RootNote = rootNote;
            Tonality = tonality;
        }

        public override string ToString()
        {
            return $"[LoopFeedback] Part={PartIndex} ({PartLabel}) " +
                   $"Loop={LoopIndexWithinPart + 1}/{LoopsInPart} " +
                   $"Tracks={ActiveTracks} ΔInsp={InspirationGainedThisLoop} " +
                   $"Total={InspirationAfterLoop} " +
                   $"TempoScale={TempoScale:0.##} TS={TimeSignature} " +
                   $"Root={RootNote} Tonality={Tonality}";
        }
    }
}