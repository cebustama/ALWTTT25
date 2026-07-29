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

            var cfg = new SongConfig
            {
                ChannelMusicianOrder = new List<string>(),
                ChannelRoles = new List<TrackRole>(),
                Parts = new List<SongConfig.PartConfig>(),
                Structure = new List<SongConfig.PartSequenceEntry>()
            };

            var firstPartRoles = new List<TrackRole>();
            var firstPartMusicians = new List<string>();

            int partIndex = 0;

            foreach (var p in model.parts)
            {
                var tonality = p.tonality;

                // ensure we have a stable root for this part
                if (!p.hasExplicitRootNote)
                {
                    var randomRoot = (Melanchall.DryWetMidi.MusicTheory.NoteName)
                        rng.Next(0, 12);

                    p.rootNote = randomRoot;
                    p.hasExplicitRootNote = true;
                }

                var root = p.rootNote;

                var part = new SongConfig.PartConfig
                {
                    Name = string.IsNullOrWhiteSpace(p.label)
                            ? $"Part {partIndex + 1}" : p.label,
                    Measures = p.measures <= 0 ? 8 : p.measures,
                    TimeSignature = p.timeSignature,
                    Tracks = new List<SongConfig.PartConfig.TrackConfig>(),
                    Tonality = tonality,
                    RootNote = root,
                    // Tempo
                    TempoRange = p.tempoRangeOverride,
                    ExplicitBpm = p.absoluteBpmOverride,
                    TempoScale = p.tempoScale
                };

                // ALWTTT-MOD-DIR-2: copy staged one-shot modulation transients
                // from PartEntry onto the freshly-built PartConfig, then clear
                // staging so the next render without a new ModulationEffect
                // application defaults to Auto + null (package no-op).
                part.ModulationOctaveHint = p.pendingModulationOctaveHint;
                part.PreviousRootNote = p.pendingPreviousRootNote;

                p.pendingModulationOctaveHint =
                    MidiGenPlay.Composition.ModulationOctaveHint.Auto;
                p.pendingPreviousRootNote = null;

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

                // One track per (musician, role) pair placed in this part.
                // [BASS-1 / R16] Channel budget: BuildChannelMap allocates
                // 0-15 minus ch9 (drums) → at most 15 melodic tracks per part.
                if (p.tracks.Count > 15)
                    Log($"[BASS-1] Part {partIndex} has {p.tracks.Count} tracks " +
                        $"— exceeds the 15-melodic-channel budget; channel " +
                        $"allocation will overflow to ch0.", true);

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

                    // 1) Explicit overrides from the composition UI (InstrumentEffect)
                    if (trModel.overrideMelodicInstrument != null)
                    {
                        melInst = trModel.overrideMelodicInstrument;
                        ctx.Log($"<color=blue>[Override] Using override melodic instrument " +
                                $"for mus='{musicianId}': '{melInst.InstrumentName}'</color>");
                    }
                    else if (trModel.overridePercussionInstrument != null)
                    {
                        percInst = trModel.overridePercussionInstrument;
                        ctx.Log($"<color=blue>[Override] Using override percussion instrument " +
                                $"for mus='{musicianId}': '{percInst.InstrumentName}'</color>");
                    }
                    else if (trModel.hasOverrideInstrumentType)
                    {
                        // TODO: TAKE MUSICIAN ALLOWED INSTs INTO ACCOUNT
                        var allMelodic = instruments.GetMelodicInstruments();
                        var candidatesByType = allMelodic
                            .Where(i => i.InstrumentType == trModel.overrideInstrumentType);

                        melInst = candidatesByType.OrderBy(_ => rng.Next()).FirstOrDefault();

                        ctx.Log($"[InstrumentEffect] Choosing random instrument of type " +
                                $"{trModel.overrideInstrumentType} for mus='{musicianId}' " +
                                $"-> '{melInst?.InstrumentName ?? "-"}'", true);
                    }

                    // PINNED INSTRUMENT OVERRIDE
                    // [DBG-C1] The part-cache pin map is keyed (musicianId,
                    // role) — unambiguous for multi-track musicians; the
                    // BASS-1 skip is retired.
                    if (ctx.TryGetPartCache(partIndex, out var partCache))
                    {
                        if (!string.IsNullOrEmpty(musicianId) &&
                            partCache.resolvedMelInstByTrack.TryGetValue(
                                new MusicianTrackKey(musicianId, role), out var pinned))
                        {
                            melInst = pinned;
                            ctx.Log($"[Pin] Using cached instrument for mus='{musicianId}' " +
                                $"role={role} -> '{pinned?.InstrumentName ?? "-"}'", true);
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
                    {
                        firstPartRoles.Add(role);
                        firstPartMusicians.Add(musicianId);
                    }
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
            {
                cfg.ChannelRoles.AddRange(firstPartRoles);
                cfg.ChannelMusicianOrder.AddRange(firstPartMusicians);
            }

            return cfg;
        }

        // [DBG-C1] Per-track (musicianId, role) hashes. The BASS-1 multi-track
        // omission is retired: composite keys make a musician's two
        // role-tracks two independent cache identities, so multi-track
        // musicians are cacheable again. Single-track hash VALUES are
        // unchanged (ComputeHashFromTrackEntry untouched) — stem-cache keys
        // for single-track parts differ only by the ":{role}" segment.
        public static Dictionary<MusicianTrackKey, string> ComputeTrackInputsHashesForPart(
            ICompositionContext ctx, int partIndex,
            IReadOnlyDictionary<MusicianTrackKey, float> mixGains = null) // [BAL-1]
        {
            var result = new Dictionary<MusicianTrackKey, string>();
            var ui = ctx?.CompositionUI;
            if (ui?.Model?.parts == null) return result;
            if (partIndex < 0 || partIndex >= ui.Model.parts.Count) return result;

            var p = ui.Model.parts[partIndex];
            if (p?.tracks == null) return result;

            foreach (var tr in p.tracks)
            {
                if (tr == null || string.IsNullOrEmpty(tr.musicianId)) continue;
                var key = new MusicianTrackKey(tr.musicianId, tr.role);
                result[key] = ComputeHashFromTrackEntry(tr, GainSegment(mixGains, key));
            }
            return result;
        }

        // [BAL-1] gainSegment folds the bytes-plane mix gain into the hash so
        // stem keys and bundle keys can never replay stale CC7 (D-BAL-3=A,
        // "gain enters trackInputsHash regardless of lifecycle"). "_" = no
        // entry (no CC7 emitted). NOTE: hash VALUES change format vs pre-BAL-1
        // ("|_" suffix) — harmless, caches are session-scoped.
        private static string ComputeHashFromTrackEntry(
            SongCompositionUI.TrackEntry tr, string gainSegment = "_")
        {
            if (tr == null) return "_";
            return string.Join("|",
                tr.role.ToString(),
                AssetKey(tr.styleBundle),
                AssetKey(tr.overrideMelodicInstrument),
                AssetKey(tr.overridePercussionInstrument),
                tr.hasOverrideInstrumentType
                    ? tr.overrideInstrumentType.ToString()
                    : "_",
                gainSegment);
        }

        private static string GainSegment(
            IReadOnlyDictionary<MusicianTrackKey, float> mixGains,
            MusicianTrackKey key)
            => mixGains != null && mixGains.TryGetValue(key, out var g)
                ? g.ToString("F4", System.Globalization.CultureInfo.InvariantCulture)
                : "_";

        private static string AssetKey(UnityEngine.Object obj)
        {
            // GetInstanceID is stable for the loaded session. The per-song
            // cache lifetime (D7=B) is well within that.
            return obj != null
                ? obj.GetInstanceID().ToString(
                    System.Globalization.CultureInfo.InvariantCulture)
                : "_";
        }
    }
}