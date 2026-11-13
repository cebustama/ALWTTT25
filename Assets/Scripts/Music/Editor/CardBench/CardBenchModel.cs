// [MGP-BENCH-1] Card bench — card list → SongModel, without the composition UI.
//
// The gig mutates the composition model inside SongCompositionUI, a scene
// MonoBehaviour whose apply methods also redraw rows (partUIs) and fire
// sensory events, so they cannot run outside Play. This class MIRRORS ONLY
// THE MODEL MUTATION of those methods, block by block, and each block names
// its source. Everything after the model (SongConfigBuilder.FromModel and
// MidiMusicManager.RenderSinglePart) is the gig's own code, not a copy.
//
// If a mirrored block drifts from its source, the bench stops matching the
// gig: the parity check (dump comparison in the window) is the guard.
using System.Collections.Generic;
using System.Linq;
using ALWTTT.Cards;
using ALWTTT.Musicians;
using ALWTTT.Tooltips;
using ALWTTT.UI;
using Melanchall.DryWetMidi.MusicTheory;
using MidiGenPlay;
using MidiGenPlay.Composition;
using static MidiGenPlay.MusicTheory.MusicTheory;
using PartEntry = ALWTTT.UI.SongCompositionUI.PartEntry;
using SongModel = ALWTTT.UI.SongCompositionUI.SongModel;
using TrackEntry = ALWTTT.UI.SongCompositionUI.TrackEntry;

namespace ALWTTT.Music.EditorTools
{
    public static class CardBenchModel
    {
        /// <summary>One played card and who played it (null = no target).</summary>
        public readonly struct Play
        {
            public readonly CardDefinition card;
            public readonly MusicianCharacterData target;
            public Play(CardDefinition card, MusicianCharacterData target)
            { this.card = card; this.target = target; }
        }

        /// <summary>
        /// Applies the plays in order onto a fresh model whose part 0 is
        /// created the way the gig creates it. Returns the model; every play
        /// that the gig would deny or that the bench does not reproduce is
        /// reported in <paramref name="notes"/> (never silently dropped).
        /// </summary>
        public static SongModel Build(IReadOnlyList<Play> plays, List<string> notes)
        {
            var model = new SongModel();
            EnsurePartAt(model, 0);

            foreach (var play in plays)
            {
                var def = play.card;
                if (def == null) continue;
                string name = def.DisplayName;

                if (!def.IsComposition || def.CompositionPayload == null)
                {
                    notes.Add($"«{name}»: no es de composición; en el banco no suena (se ignora).");
                    continue;
                }
                ApplyCardDefinitionToPart(model, def, play.target, 0, notes);
            }
            return model;
        }

        // ── Mirror of SongCompositionUI.PartLabelFor ────────────────────────
        // The label reaches the MIDI bytes (part marker text), so it has to be
        // the gig's, in the gig's language (the window sets the glossary).
        public static string PartLabelFor(int index)
            => GameText.Format(GameTextTemplateIds.UiPartLabel,
                (char)('A' + UnityEngine.Mathf.Clamp(index, 0, 25)));

        // ── Mirror of SongCompositionUI.EnsurePartAt (model part only) ──────
        private static PartEntry EnsurePartAt(SongModel model, int partIndex)
        {
            if (partIndex < 0) return null;
            while (model.parts.Count <= partIndex)
            {
                model.parts.Add(new PartEntry
                {
                    label = PartLabelFor(model.parts.Count),
                    timeSignature = TimeSignature.FourFour,
                    tempo = "Slow",
                    tonality = Tonality.Ionian,
                    measures = 8,
                    tracks = new List<TrackEntry>()
                });
            }
            return model.parts[partIndex];
        }

        // ── Mirror of SongCompositionUI.ApplyCardDefinitionToPart ───────────
        private static bool ApplyCardDefinitionToPart(
            SongModel model, CardDefinition data, MusicianCharacterData target,
            int partIndex, List<string> notes)
        {
            var comp = data.CompositionPayload;
            string name = data.DisplayName;
            var part = EnsurePartAt(model, partIndex);

            if (comp.RequiresMusicianTarget && target == null)
            {
                notes.Add($"«{name}»: pide un músico y no hay ninguno (el gig la rechaza).");
                return false;
            }

            if (comp.PrimaryKind == CardPrimaryKind.Track)
            {
                var desc = comp.TrackAction;
                if (desc == null) { notes.Add($"«{name}»: carta de pista sin TrackAction."); return false; }
                if (target == null) { notes.Add($"«{name}»: carta de pista sin músico."); return false; }

                // [R6 / D-R6-4=A] mirror of TryGetHarmonyDenial
                if (desc.role == TrackRole.Harmony
                    && !part.tracks.Any(t => t.musicianId == target.CharacterId
                                          && t.role == TrackRole.Melody))
                {
                    notes.Add($"«{name}»: Harmony sin melodía propia de {target.CharacterName} " +
                              "en la parte (el gig la rechaza).");
                    return false;
                }

                TryAddOrReplaceTrackOnPart(part, target, desc.role, data, desc.styleBundle, notes);
            }
            else if (comp.PrimaryKind == CardPrimaryKind.Part)
            {
                var pa = comp.PartAction;
                if (pa == null) { notes.Add($"«{name}»: carta de parte sin PartAction."); return false; }
                if (pa.action == PartActionKind.MarkFinal)
                    part.isFinal = true;
                else
                    notes.Add($"«{name}»: {pa.action} crea o marca otra parte; el banco " +
                              "solo renderiza la parte A (no cambia lo que suena aquí).");
            }
            else
            {
                notes.Add($"«{name}»: PrimaryKind {comp.PrimaryKind} sin soporte.");
                return false;
            }

            var effects = comp.ModifierEffects;
            if (effects != null)
                foreach (var fx in effects)
                    ApplyEffectToModel(model, fx, partIndex, target, name, notes);

            return true;
        }

        // ── Mirror of SongCompositionUI.TryAddOrReplaceTrackOnPart (model) ──
        private static void TryAddOrReplaceTrackOnPart(
            PartEntry part, MusicianCharacterData target, TrackRole role,
            CardDefinition sourceCard, TrackStyleBundleSO styleBundle, List<string> notes)
        {
            string musicianId = target.CharacterId;
            var existing = part.tracks.FirstOrDefault(
                t => t.musicianId == musicianId && t.role == role);

            if (existing != null)
            {
                existing.role = role;
                existing.info = sourceCard.DisplayName;
                existing.inspirationGenerated = UnityEngine.Mathf.Max(0, sourceCard.InspirationGenerated);
                existing.synergyType = sourceCard.CardType;
                if (styleBundle != null) existing.styleBundle = styleBundle;
                existing.sourceCardDefinition = sourceCard;
                return;
            }

            if (styleBundle == null)
            {
                notes.Add($"«{sourceCard.DisplayName}»: sin bundle y sin pista {role} de " +
                          $"{target.CharacterName} que modificar; solo aplica sus efectos.");
                return;
            }

            part.tracks.Add(new TrackEntry
            {
                musicianId = musicianId,
                role = role,
                info = sourceCard.DisplayName,
                inspirationGenerated = UnityEngine.Mathf.Max(0, sourceCard.InspirationGenerated),
                synergyType = sourceCard.CardType,
                styleBundle = styleBundle,
                sourceCardDefinition = sourceCard,
            });
        }

        // ── Mirror of SongCompositionUI.ApplyEffectToModel (model) ──────────
        private static void ApplyEffectToModel(
            SongModel model, PartEffect fx, int partIndex,
            MusicianCharacterData target, string cardName, List<string> notes)
        {
            if (fx == null) return;
            int idx = fx.timing == ApplyTiming.OnNextPartStart ? partIndex + 1 : partIndex;
            var part = EnsurePartAt(model, idx);
            if (idx != partIndex)
                notes.Add($"«{cardName}»: {fx.GetType().Name} actúa en la parte siguiente; " +
                          "no cambia la parte A.");

            switch (fx)
            {
                case TempoEffect t:
                    switch (t.mode)
                    {
                        case TempoEffect.TempoEffectMode.Range:
                            part.tempoRangeOverride = t.tempoRange;
                            part.absoluteBpmOverride = null;
                            part.tempo = t.tempoRange.ToString();
                            break;
                        case TempoEffect.TempoEffectMode.AbsoluteBpm:
                            part.absoluteBpmOverride = t.absoluteBpm;
                            part.tempoRangeOverride = TempoRange.Fast;
                            part.tempo = $"{t.absoluteBpm} BPM";
                            break;
                        case TempoEffect.TempoEffectMode.ScaleFactor:
                            part.tempoScale *= t.tempoScale;
                            part.tempo = $"×{part.tempoScale:0.##}";
                            break;
                    }
                    break;

                case MeterEffect m:
                    part.timeSignature = m.timeSignature;
                    break;

                case TonalityEffect ton:
                    {
                        Tonality chosen = ton.tonality;
                        switch (ton.mode)
                        {
                            case TonalityEffect.TonalityEffectMode.RandomAny:
                                chosen = GetRandomAnyTonality(); WarnUnseeded(); break;
                            case TonalityEffect.TonalityEffectMode.RandomMajorish:
                                chosen = GetRandomMajorishTonality(); WarnUnseeded(); break;
                            case TonalityEffect.TonalityEffectMode.RandomMinorish:
                                chosen = GetRandomMinorishTonality(); WarnUnseeded(); break;
                        }
                        part.tonality = chosen;
                        break;
                    }

                case InstrumentEffect instFx:
                    ApplyInstrumentEffect(instFx, target, part, cardName, notes);
                    break;

                case ModulationEffect mod:
                    {
                        var currentMode = part.tonality;
                        var currentRoot = part.rootNote;
                        NoteName newRoot = currentRoot;
                        switch (mod.mode)
                        {
                            case ModulationEffect.ModulationMode.AbsoluteKey:
                                newRoot = mod.absoluteRoot;
                                break;
                            case ModulationEffect.ModulationMode.IntervalWithinScale:
                                {
                                    var scale = GetScaleFromTonality(currentMode, currentRoot);
                                    if (GetNoteFromScale(scale, mod.targetDegree, currentRoot, 4, out var note))
                                        newRoot = note.NoteName;
                                    break;
                                }
                            case ModulationEffect.ModulationMode.RandomAny:
                                newRoot = GetRandomNote(); WarnUnseeded();
                                break;
                            case ModulationEffect.ModulationMode.RandomWithinScale:
                                {
                                    var scale = GetScaleFromTonality(currentMode, currentRoot);
                                    var ns = GetNotesFromScale(scale, currentRoot, 4, 7);
                                    if (ns != null && ns.Count > 0)
                                    {
                                        int start = (mod.excludeTonicOnRandomWithinScale && ns.Count > 1) ? 1 : 0;
                                        newRoot = ns[UnityEngine.Random.Range(start, ns.Count)].NoteName;
                                    }
                                    WarnUnseeded();
                                    break;
                                }
                        }
                        part.rootNote = newRoot;
                        part.hasExplicitRootNote = true;
                        part.pendingPreviousRootNote = currentRoot;
                        part.pendingModulationOctaveHint = mod.octaveHint;
                        break;
                    }
            }

            void WarnUnseeded() => notes.Add(
                $"«{cardName}»: {fx.GetType().Name} elige al azar sin semilla (también en el gig): " +
                "cada render puede sonar distinto.");
        }

        // ── Mirror of SongCompositionUI.ApplyInstrumentEffect ───────────────
        private static void ApplyInstrumentEffect(
            InstrumentEffect fx, MusicianCharacterData target, PartEntry part,
            string cardName, List<string> notes)
        {
            if (fx.scope != EffectScope.TrackOnly || target == null)
            {
                notes.Add($"«{cardName}»: InstrumentEffect fuera de TrackOnly no está " +
                          "implementado (tampoco en el gig).");
                return;
            }

            bool percussionMode =
                fx.mode == InstrumentEffect.InstrumentTargetMode.SpecificPercussion;
            var matching = part.tracks.Where(t =>
                t.musicianId == target.CharacterId
                && (percussionMode ? t.role == TrackRole.Rhythm : t.role != TrackRole.Rhythm)).ToList();
            if (matching.Count == 0)
            {
                notes.Add($"«{cardName}»: el cambio de instrumento no encuentra pista de " +
                          $"{target.CharacterName} en la parte.");
                return;
            }

            MIDIInstrumentSO randomListPick = null;
            if (fx.mode == InstrumentEffect.InstrumentTargetMode.RandomFromList)
            {
                var pool = fx.melodicInstrumentPool?.Where(i => i != null).Distinct().ToList();
                if (pool == null || pool.Count == 0) return;
                randomListPick = pool[UnityEngine.Random.Range(0, pool.Count)];
                notes.Add($"«{cardName}»: instrumento al azar de una lista, sin semilla " +
                          "(también en el gig).");
            }

            foreach (var track in matching)
            {
                track.overrideMelodicInstrument = null;
                track.overridePercussionInstrument = null;
                track.hasOverrideInstrumentType = false;
                switch (fx.mode)
                {
                    case InstrumentEffect.InstrumentTargetMode.SpecificMelodic:
                        track.overrideMelodicInstrument = fx.melodicInstrument; break;
                    case InstrumentEffect.InstrumentTargetMode.SpecificPercussion:
                        track.overridePercussionInstrument = fx.percussionInstrument; break;
                    case InstrumentEffect.InstrumentTargetMode.InstrumentType:
                        track.hasOverrideInstrumentType = true;
                        track.overrideInstrumentType = fx.instrumentType; break;
                    case InstrumentEffect.InstrumentTargetMode.RandomFromList:
                        track.overrideMelodicInstrument = randomListPick; break;
                }
            }
        }
    }
}
