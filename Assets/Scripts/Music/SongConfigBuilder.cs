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

        // [LOG-1 / D-LOG-3=B] This class is static and holds no settings
        // reference of its own, so the host pushes the two levels in once,
        // from GigManager.Awake(). Defaults are deliberately permissive: if
        // GigManager never runs (editor tooling, tests) the builder behaves
        // exactly as it did before LOG-1 rather than going silent.
        public static bool LogEnabled = true;
        public static bool LogVerbose = false;

        public static void Log(string log, bool highlight = false, string customColor = "")
        {
            if (!LogEnabled) return;

            if (highlight)
                Debug.Log($"{DebugTag} <color=yellow>{log}</color>");
            else if (!string.IsNullOrWhiteSpace(customColor))
                Debug.Log($"{DebugTag} <color={customColor}>{log}</color>");
            else
                Debug.Log($"{DebugTag} {log}");
        }

        /// <summary>[LOG-1] Verbose tier. Used for the derived music-theory
        /// tables (scale notes, diatonic triads) that are recomputable from
        /// tonality + root and therefore carry no state a smoke test reads.
        /// The part header line stays on <see cref="Log"/>: it is the line
        /// that reports the MODEL tonality, and both the D-R3C-3 premise
        /// correction and the open D-R4-1 question rest on it.</summary>
        public static void LogV(string log, string customColor = "")
        {
            if (LogVerbose) Log(log, false, customColor);
        }

        public static SongConfig FromUI(
            ICompositionContext ctx,
            IInstrumentRepository instruments,
            IPatternRepository patterns,
            Func<MusicianBase, TrackRole, IEnumerable<MIDIInstrumentSO>> getPermittedMelodic,
            System.Random rng,
            int? songSeed = null) // [MGP-REPRO-1 / D2=A]
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

            // [MGP-BENCH-1] The gig binds the pure core to the live context.
            // Same lookups, same order, same NRE when a musician has no
            // gameplay data — behavior unchanged by the extraction.
            return FromModel(
                model,
                instruments,
                resolvePermittedMelodic: (musicianId, role) =>
                {
                    var musician = ctx.ResolveMusicianById(musicianId);
                    return (getPermittedMelodic != null)
                        ? getPermittedMelodic(musician, role)
                        : instruments.GetMelodicInstruments();
                },
                resolvePermittedPercussion: musicianId =>
                    InstrumentRules.GetPermittedPercussion(
                        ctx.ResolveMusicianById(musicianId), instruments),
                resolveLeading: musicianId =>
                {
                    var pd = GameManager.Instance.PersistentGameplayData;
                    var mgd = pd != null
                        ? pd.GetMusicianGameplayData(musicianId)
                        : null;
                    return (mgd.CurrentMelodicLeading, mgd.CurrentHarmonicLeading);
                },
                resolvePinned: (partIndex, key) =>
                    ctx.TryGetPartCache(partIndex, out var partCache)
                    && partCache.resolvedMelInstByTrack.TryGetValue(key, out var pinned)
                        ? (true, pinned) : (false, (MIDIInstrumentSO)null),
                log: (msg, highlight) => ctx.Log(msg, highlight),
                rng: rng,
                songSeed: songSeed);
        }

        /// <summary>
        /// [MGP-BENCH-1] Pure core of <see cref="FromUI"/>: SongModel → SongConfig
        /// with every live-context lookup injected. The gig reaches it through
        /// FromUI; the card bench (editor, no Play) calls it with asset-backed
        /// lookups, so both share one translation. Mutates the model exactly as
        /// FromUI always did: stamps the seeded root on parts without an explicit
        /// one and consumes the one-shot modulation staging.
        /// </summary>
        public static SongConfig FromModel(
            SongCompositionUI.SongModel model,
            IInstrumentRepository instruments,
            Func<string, TrackRole, IEnumerable<MIDIInstrumentSO>> resolvePermittedMelodic,
            Func<string, IEnumerable<MIDIPercussionInstrumentSO>> resolvePermittedPercussion,
            Func<string, (MelodicLeadingConfig melodic, HarmonicLeadingConfig harmonic)> resolveLeading,
            Func<int, MusicianTrackKey, (bool found, MIDIInstrumentSO inst)> resolvePinned,
            Action<string, bool> log,
            System.Random rng,
            int? songSeed = null)
        {
            // [MGP-REPRO-1 / D2=A] With a song seed every choice (root, kit,
            // instrument) is a pure hash of (seed | part | musician | role | what)
            // over a name-sorted pool: independent of draw order, of gameplay
            // draws on the shared rng, and of Resources load order. Without a
            // seed, the legacy rng shuffle (unchanged).
            T Pick<T>(IEnumerable<T> pool, string key) where T : MIDIInstrumentSO
            {
                if (pool == null) return null;
                if (!songSeed.HasValue)
                    return pool.OrderBy(_ => rng.Next()).FirstOrDefault();
                var list = pool.Where(i => i != null)
                    .OrderBy(i => i.InstrumentName, StringComparer.Ordinal)
                    .ThenBy(i => i.name, StringComparer.Ordinal)
                    .ToList();
                if (list.Count == 0) return null;
                uint h = unchecked((uint)SongOrchestrator.StableHash32($"{songSeed.Value}|{key}"));
                return list[(int)(h % (uint)list.Count)];
            }

            if (model == null || model.parts.Count == 0)
            {
                Log("No parts in composition model.");
                return null;
            }

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
                    int rootIdx = songSeed.HasValue
                        ? (int)(unchecked((uint)SongOrchestrator.StableHash32(
                              $"{songSeed.Value}|p={partIndex}|root")) % 12u)
                        : rng.Next(0, 12);
                    var randomRoot = (Melanchall.DryWetMidi.MusicTheory.NoteName)rootIdx;

                    p.rootNote = randomRoot;
                    p.hasExplicitRootNote = true;
                }

                var root = p.rootNote;

                var part = new SongConfig.PartConfig
                {
                    Name = string.IsNullOrWhiteSpace(p.label)
                            ? $"Part {partIndex + 1}" : p.label,
                    // [MUSICA-1] F5 ▸ Música ▸ Compases por loop (sin ajuste = el del modelo, 8).
                    Measures = ALWTTT.Data.DevTuning.Int(ALWTTT.Data.DevTuning.Keys.LoopMeasures,
                        p.measures <= 0 ? 8 : p.measures),
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
                LogV($"Scale notes ({part.Tonality} " +      // [LOG-1] verbose
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

                LogV($"Diatonic triads: {string.Join("  ", diatonic)}",  // [LOG-1] verbose
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

                    var trackKey = $"p={partIndex}|mus={musicianId}|role={role}"; // [MGP-REPRO-1]

                    MIDIInstrumentSO melInst = null;
                    MIDIPercussionInstrumentSO percInst = null;
                    IEnumerable<MIDIInstrumentSO> candidates = null;

                    RhythmRecipe recipe = null;
                    BackingRecipe backingRecipe = null;

                    switch (role)
                    {
                        case TrackRole.Rhythm:
                            // [MGP-REPRO-1 / D3=A] Musician's kit whitelist (empty = all).
                            percInst = Pick(
                                resolvePermittedPercussion(musicianId),
                                $"{trackKey}|kit");

                            recipe = new RhythmRecipe
                            {
                                HatDensity = RhythmRecipe.HiHatDensity.From_Style,
                                HatMode = RhythmRecipe.HatDensityMode.Fixed
                            };

                            break;

                        case TrackRole.Backing:

                            candidates = resolvePermittedMelodic(musicianId, role);

                            melInst = Pick(candidates, $"{trackKey}|inst");

                            backingRecipe = new BackingRecipe
                            {

                            };

                            break;

                        case TrackRole.Bassline:

                            candidates = resolvePermittedMelodic(musicianId, role);

                            melInst = Pick(candidates, $"{trackKey}|inst");
                            break;

                        case TrackRole.Melody:
                        case TrackRole.Harmony:

                            candidates = resolvePermittedMelodic(musicianId, role);

                            melInst = Pick(candidates, $"{trackKey}|inst");
                            break;
                    }

                    // 1) Explicit overrides from the composition UI (InstrumentEffect)
                    if (trModel.overrideMelodicInstrument != null)
                    {
                        melInst = trModel.overrideMelodicInstrument;
                        log?.Invoke($"<color=blue>[Override] Using override melodic instrument " +
                                $"for mus='{musicianId}': '{melInst.InstrumentName}'</color>", false);
                    }
                    else if (trModel.overridePercussionInstrument != null)
                    {
                        percInst = trModel.overridePercussionInstrument;
                        log?.Invoke($"<color=blue>[Override] Using override percussion instrument " +
                                $"for mus='{musicianId}': '{percInst.InstrumentName}'</color>", false);
                    }
                    else if (trModel.hasOverrideInstrumentType)
                    {
                        // TODO: TAKE MUSICIAN ALLOWED INSTs INTO ACCOUNT
                        var allMelodic = instruments.GetMelodicInstruments();
                        var candidatesByType = allMelodic
                            .Where(i => i.InstrumentType == trModel.overrideInstrumentType);

                        melInst = Pick(candidatesByType, $"{trackKey}|insttype");

                        log?.Invoke($"[InstrumentEffect] Choosing random instrument of type " +
                                $"{trModel.overrideInstrumentType} for mus='{musicianId}' " +
                                $"-> '{melInst?.InstrumentName ?? "-"}'", true);
                    }

                    // PINNED INSTRUMENT OVERRIDE
                    // [DBG-C1] The part-cache pin map is keyed (musicianId,
                    // role) — unambiguous for multi-track musicians; the
                    // BASS-1 skip is retired.
                    if (resolvePinned != null && !string.IsNullOrEmpty(musicianId))
                    {
                        var (pinFound, pinned) = resolvePinned(
                            partIndex, new MusicianTrackKey(musicianId, role));
                        if (pinFound)
                        {
                            melInst = pinned;
                            log?.Invoke($"[Pin] Using cached instrument for mus='{musicianId}' " +
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
                    var leading = resolveLeading(musicianId);

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
                            melodicLeadingOverride = leading.melodic,
                            harmonyStrategyId = HarmonyStrategyId.NearestChordTone,
                            harmonicLeadingOverride = leading.harmonic,
                        }
                    };

                    part.Tracks.Add(tcfg);

                    // [MUSICA-1c] what the smoke listens for: figure melodies and bass palettes.
                    if (trModel.styleBundle is MelodyCardConfigSO mel &&
                        mel.melodyMode == MelodyCardConfigSO.MelodyMode.Figure)
                        Debug.Log($"[MUSICA-1] Parte {partIndex + 1}: melodía de {musicianId} en modo figura " +
                                  $"({mel.name}, semilla {mel.figureSeed}, varía por canción: " +
                                  $"{(mel.figureVaryPerSong ? "sí" : "no")}, patrón fijo: " +
                                  $"{(mel.patternOverride != null ? mel.patternOverride.name : "ninguno")}).");
                    else if (trModel.styleBundle is BasslineCardConfigSO bassPal &&
                             bassPal.variants != null && bassPal.variants.Count > 0)
                        Debug.Log($"[MUSICA-1] Parte {partIndex + 1}: bajo de {musicianId} con la paleta " +
                                  $"{bassPal.name} ({string.Join(" · ", bassPal.variants.Where(v => v != null && v.weight > 0f).Select(v => v.label))}), " +
                                  $"instrumento {instName}.");

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
                result[key] = ComputeHashFromTrackEntry(tr, GainSegment(mixGains, key))
                            + ListenSegment(tr, p.tracks);
            }
            return result;
        }

        // MGP-BASS-ACCENT-1 (D8=B), closes the "hash duty" debt of
        // Design_Composition_Variations_v0_1 §6.1: a bass card that LISTENS to
        // the drums (SlapPocket onsets, or accentFollow) renders differently
        // when the drums change, so the identity of the part's Rhythm tracks
        // enters its hash. Otherwise the stem cache would replay a bass
        // composed against the old drums. Non-listening tracks get "" — hash
        // VALUES unchanged for every existing card.
        //
        // MGP-COMP-ACCENT-1 (V4): the Backing listens too (accentFollowSettings
        // on its card), and a listening bass also reads the beats a listening
        // Backing already struck (avoidClaimedAccents, D10), so those Backing
        // tracks enter the bass's hash as "|bx:". Both segments stay "" for
        // every card that doesn't listen — hash VALUES unchanged.
        private static string ListenSegment(
            SongCompositionUI.TrackEntry tr,
            IReadOnlyList<SongCompositionUI.TrackEntry> partTracks)
        {
            bool listens;
            string claims = "";
            if (tr.role == TrackRole.Bassline && tr.styleBundle is BasslineCardConfigSO bass)
            {
                // [MUSICA-1b, MGP-BASS-PALETTE-1] a palette card listens if any
                // of its variants does (the card alone: the pre-palette rule).
                listens = bass.ListensToDrums();
                bool readsClaims = BassConfigs(bass).Any(x =>
                    x.accentFollowSettings != null && x.accentFollowSettings.avoidClaimedAccents &&
                    Strikes(x.accentFollowSettings.mode));
                if (listens && readsClaims)
                {
                    var backing = partTracks
                        .Where(t => t != null && t.role == TrackRole.Backing &&
                                    t.styleBundle is BackingCardConfigSO bc && Follows(bc))
                        .Select(t => (t.musicianId ?? "_") + "~" + ComputeHashFromTrackEntry(t))
                        .ToList();
                    if (backing.Count > 0) claims = "|bx:" + string.Join(";", backing);
                }
            }
            else if (tr.role == TrackRole.Backing && tr.styleBundle is BackingCardConfigSO backingCard)
            {
                listens = Follows(backingCard);
            }
            else return "";
            if (!listens) return "";

            var rhythm = partTracks
                .Where(t => t != null && t.role == TrackRole.Rhythm)
                .Select(t => (t.musicianId ?? "_") + "~" + ComputeHashFromTrackEntry(t));
            return "|rx:" + string.Join(";", rhythm) + claims;
        }

        // [MUSICA-1b] the card and the configs of its palette variants.
        private static IEnumerable<BasslineCardConfigSO> BassConfigs(BasslineCardConfigSO bass)
        {
            yield return bass;
            if (bass.variants == null) yield break;
            foreach (var v in bass.variants)
                if (v?.config != null && v.config != bass) yield return v.config;
        }

        private static bool Strikes(BasslineCardConfigSO.AccentFollowMode m) =>
            m == BasslineCardConfigSO.AccentFollowMode.Attacks ||
            m == BasslineCardConfigSO.AccentFollowMode.AttacksAndGaps ||
            m == BasslineCardConfigSO.AccentFollowMode.Random;   // MGP-COMP-ACCENT-2: may strike

        private static bool Follows(BackingCardConfigSO card)
        {
            var m = card.accentFollowSettings?.mode ?? BasslineCardConfigSO.AccentFollowMode.Off;
            return m == BasslineCardConfigSO.AccentFollowMode.Reinforce || Strikes(m);
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