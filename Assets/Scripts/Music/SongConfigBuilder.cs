using ALWTTT.Characters.Band;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.UI;
using Melanchall.DryWetMidi.Composing;
using MidiGenPlay;
using MidiGenPlay.Composition;
using MidiGenPlay.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Music
{
    public static class SongConfigBuilder
    {
        private const string DebugTag = "<color=green>[SongConfigBuilder]</color>";

        public static void Log(string log, bool highlight = false, string customColor = "")
        {
            if (highlight)
                Debug.Log($"{DebugTag} <color=yellow>{log}</color>");
            else if (!string.IsNullOrWhiteSpace(customColor))
                Debug.Log($"{DebugTag} <color={customColor}>{log}</color>");
            else
                Debug.Log($"{DebugTag} {log}");
        }

        public static SongConfig FromUI(
            ICompositionContext ctx,
            IInstrumentRepository instruments,
            IPatternRepository patterns,
            Func<MusicianBase, TrackRole, IEnumerable<MIDIInstrumentSO>> getPermittedMelodic,
            System.Random rng)
        {
            var ui = ctx.CompositionUI;
            if (ui == null)
            {
                Log("CompositionUI is null.");
                return null;
            }

            var model = ui.Model;
            if (model == null || model.parts.Count == 0)
            {
                Log("No parts in composition model.");
                return null;
            }

            // Defensive refresh
            instruments?.Refresh();
            patterns?.Refresh();

            var band = ctx.Band;
            var channels = band.Select(m => m.MusicianCharacterData.CharacterId).ToList();

            var cfg = new SongConfig
            {
                ChannelMusicianOrder = channels ?? new List<string>(),
                ChannelRoles = new List<TrackRole>(),
                Parts = new List<SongConfig.PartConfig>(),
                Structure = new List<SongConfig.PartSequenceEntry>()
            };

            var firstPartRoles = new List<TrackRole>();
            int partIndex = 0;

            foreach (var p in model.parts)
            {
                var tonality = p.tonality;

                var part = new SongConfig.PartConfig
                {
                    Name = string.IsNullOrWhiteSpace(p.label) 
                            ? $"Part {partIndex + 1}" : p.label,
                    Measures = p.measures <= 0 ? 8 : p.measures,
                    TimeSignature = p.timeSignature,
                    Tracks = new List<SongConfig.PartConfig.TrackConfig>(),
                    Tonality = tonality,
                    RootNote = Melanchall.DryWetMidi.MusicTheory.NoteName.C,
                    // Tempo
                    TempoRange = p.tempoRangeOverride,
                    ExplicitBpm = p.absoluteBpmOverride,
                    TempoScale = p.tempoScale
                };

                Log($"Building Part[{partIndex}] '{part.Name}'  " +
                    $"TS={p.timeSignature.ToString()} " +
                    $"Tempo={p.tempo} " +
                    $"Measures={part.Measures} " +
                    $"Tonality: {part.Tonality} over {part.RootNote}", true);

                var scale = GetScaleFromTonality(part.Tonality, part.RootNote);
                var scaleNotes = GetNotesFromScale(scale, part.RootNote, 4, 7)
                                    .Select(n => n.NoteName)
                                    .Distinct()
                                    .ToArray();

                var scaleStr = string.Join("  ", scaleNotes);
                Log($"Scale notes ({part.Tonality} " +
                    $"over {part.RootNote}): {scaleStr}", customColor: "orange");

                // LOG
                // --- Diatonic triads for this mode / root -----------------------------
                var diatonic = new List<string>();
                for (int degIdx = 0; degIdx < 7; degIdx++)
                {
                    var degree = (ScaleDegree)degIdx;

                    // Diatonic triad quality (Ionian template rotated by mode)
                    var q = GetDiatonicTriadQuality(part.Tonality, degree);

                    // Pitch-classes of the chord (R, 3, 5)
                    var pcs = ChordPitchClasses(part.Tonality, part.RootNote, degree, q);
                    if (pcs == null || pcs.Length == 0)
                        continue;

                    // Spell the root nicely relative to the key (C, D♭, F♯, etc.)
                    var rootPc = pcs[0];
                    var rootLabel = SpellNoteForDegree(rootPc, part.RootNote, degIdx);

                    // Just show raw pitch-class names for the full chord (debug-oriented)
                    var notesStr = string.Join(" ", pcs.Select(n => n.ToString()));

                    // Roman numeral with quality (I, ii, V<sup>7</sup>, etc.)
                    var rn = ToRomanRich(degree, q);

                    diatonic.Add($"{rn} {rootLabel} [{notesStr}]");
                }

                Log($"Diatonic triads: {string.Join("  ", diatonic)}",
                    customColor: "orange");
                //

                // one track per musician that has a placed card in this part
                int trackId = 0;
                foreach (var trModel in p.tracks)
                {
                    var role = trModel.role;
                    var musicianId = trModel.musicianId;
                    if (string.IsNullOrEmpty(musicianId))
                    {
                        Log($"Skipping track with empty musicianId (role {role}).");
                        continue;
                    }

                    var musician = ctx.ResolveMusicianById(musicianId);

                    MIDIInstrumentSO melInst = null;
                    MIDIPercussionInstrumentSO percInst = null;
                    IEnumerable<MIDIInstrumentSO> candidates = null;

                    RhythmRecipe recipe = null;
                    BackingRecipe backingRecipe = null;

                    switch (role)
                    {
                        case TrackRole.Rhythm:
                            // TODO: Get instrument some other way?
                            percInst = instruments.GetPercussionInstruments()
                                .OrderBy(_ => rng.Next()).FirstOrDefault();

                            recipe = new RhythmRecipe
                            {
                                HatDensity = RhythmRecipe.HiHatDensity.From_Style,
                                HatMode = RhythmRecipe.HatDensityMode.Fixed
                            };

                            break;

                        case TrackRole.Backing:

                            candidates = (getPermittedMelodic != null)
                                ? getPermittedMelodic(musician, role)
                                : instruments.GetMelodicInstruments();

                            melInst = candidates.OrderBy(_ => rng.Next()).FirstOrDefault();

                            backingRecipe = new BackingRecipe
                            {

                            };

                            break;

                        case TrackRole.Bassline:

                            candidates = (getPermittedMelodic != null)
                                ? getPermittedMelodic(musician, role)
                                : instruments.GetMelodicInstruments();

                            melInst = candidates.OrderBy(_ => rng.Next()).FirstOrDefault();
                            break;

                        case TrackRole.Melody:
                        case TrackRole.Harmony:

                            candidates = (getPermittedMelodic != null)
                                ? getPermittedMelodic(musician, role)
                                : instruments.GetMelodicInstruments();

                            melInst = candidates.OrderBy(_ => rng.Next()).FirstOrDefault();
                            break;
                    }

                    // PINNED INSTRUMENT OVERRIDE
                    /*if (role != TrackRole.Rhythm &&
                        _partCache.TryGetValue(partIndex, out var partCache) &&
                        partCache != null &&
                        !string.IsNullOrEmpty(musicianId) &&
                        partCache.resolvedMelInstByMusician.TryGetValue(
                            musicianId, out var pinned) &&
                        pinned != null)
                    {
                        melInst = pinned; // make the cached one authoritative
                        Log($"[Jam] [Pin] Using cached instrument for " +
                            $"part={partIndex} " +
                            $"mus='{musicianId}' -> '{pinned.InstrumentName}'");
                    }*/
                    if (ctx.TryGetPartCache(partIndex, out var partCache))
                    {
                        if (!string.IsNullOrEmpty(musicianId) &&
                            partCache.resolvedMelInstByMusician
                                .TryGetValue(musicianId, out var pinned))
                        {
                            melInst = pinned;
                            ctx.Log($"[Pin] Using cached instrument for mus='{musicianId}' " +
                                $"-> '{pinned?.InstrumentName ?? "-"}'", true);
                        }
                    }

                    var instName = melInst != null ? melInst.InstrumentName :
                                   percInst != null ? percInst.InstrumentName : "(none)";

                    Log($"[Jam] Track {trackId++} " +
                        $"role={role} " +
                        $"mus={musicianId} " +
                        $"inst='{instName}'", true);

                    // Look up persistent per-musician gameplay state
                    var pd = GameManager.Instance.PersistentGameplayData;
                    var mgd = pd != null
                        ? pd.GetMusicianGameplayData(musicianId)
                        : null;

                    // Build track config
                    var tcfg = new SongConfig.PartConfig.TrackConfig
                    {
                        Role = role,
                        MusicianId = musicianId,
                        Instrument = melInst,
                        PercussionInstrument = percInst,
                        Parameters = new TrackParameters
                        {
                            RhythmRecipe = recipe,
                            Style = trModel.styleBundle,

                            // Legacy fallbacks TODO: REMOVE
                            melodyStrategyId = MelodyStrategyId.ScaleFlow,
                            melodicLeadingOverride = mgd.CurrentMelodicLeading,
                            harmonyStrategyId = HarmonyStrategyId.NearestChordTone,
                            harmonicLeadingOverride = mgd.CurrentHarmonicLeading,
                        }
                    };

                    part.Tracks.Add(tcfg);

                    // Remember roles present in Part 0 to seed ChannelRoles (layout)
                    if (cfg.Parts.Count == 0)
                        firstPartRoles.Add(role);
                }

                cfg.Parts.Add(part);
                // Structure: by default add this part once
                cfg.Structure.Add(new SongConfig.PartSequenceEntry
                {
                    PartIndex = partIndex,
                    RepeatCount = 1
                });

                partIndex++;
            }

            // If ChannelRoles not provided yet, seed it from the first part’s roles
            if (cfg.ChannelRoles.Count == 0)
                cfg.ChannelRoles.AddRange(firstPartRoles);

            return cfg;
        }
    }
}