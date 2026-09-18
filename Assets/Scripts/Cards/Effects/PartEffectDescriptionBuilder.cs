// Place at: Assets/Scripts/Cards/Effects/PartEffectDescriptionBuilder.cs
// [CARD-FACE-1 / D-CF-5=B] Central formatter for PartEffect -> player-facing modifier line.
// Mirror of CardEffectDescriptionBuilder (M1.3 / TXT-4): the DATA asset does not format its own
// player text. PartEffect.GetLabel() is demoted to a DEVELOPER label (inspector, editor window,
// logs) and is never read by a player surface again.
//
// Every line is: <modifier-family glyph> + a glossary value. Notation-valued enums (TimeSignature,
// TempoRange, NoteName, ScaleDegree, InstrumentType) are TEXT through a derived id family
// (D-CF-10=A): "5/4", "Mediant" and "C#" are notation the player reads, and Spanish may want
// "Mediante" or "Do#". Categorical enums (scope, timing) are SYMBOLS (D-CF-1).
//
// The meter value comes from MeterEffect.timeSignature through UiCtxMeter - the SAME id the
// context row formats - so "5/4" has one home (task 2). MeterEffect.meterLabel no longer exists.
//
// Diagnostics stay literal (D-SP1-4=B precedent "Apply <missing status>."): an effect with no
// asset assigned, an empty pool, and an unmapped subclass (its type name). Until Part B maps
// TonalityEffect, that subclass shows its type name - visible, never silent.
//
// SOURCE IS PURE ASCII ON PURPOSE (F-T6-1 / F-SP1-2).

using ALWTTT.Tooltips;
using MidiGenPlay.Composition;   // ModulationOctaveHint
using static ALWTTT.Tooltips.GameTextTemplateIds;

namespace ALWTTT.Cards.Effects
{
    public static class PartEffectDescriptionBuilder
    {
        /// <summary>The modifier line: family glyph + value. Empty for null.</summary>
        public static string Build(PartEffect fx)
        {
            string glyph = FamilySpriteName(fx);
            string value = ValueOnly(fx);
            return glyph == null ? value : GameSymbols.Tag(glyph) + " " + value;
        }

        /// <summary>[D-CF-14b] The VALUE with no family glyph, for surfaces that draw the glyph
        /// themselves -- the card face. One value formatter, two surfaces: if the face composed its
        /// own text, a card could read one thing in hand and another in the modal.</summary>
        public static string ValueOnly(PartEffect fx)
        {
            switch (fx)
            {
                case null: return string.Empty;
                case MeterEffect m: return BuildMeter(m);
                case TempoEffect t: return BuildTempo(t);
                case ModulationEffect mod: return BuildModulation(mod);
                case InstrumentEffect inst: return BuildInstrument(inst);
                default: return fx.GetType().Name;   // developer diagnostic: unmapped subclass
            }
        }

        /// <summary>[D-CF-13=C] The (scope, timing) suffix: each symbol followed by its concept
        /// displayName. The modal has no hover handler (F-CF-5), so an opaque symbol must carry its
        /// word; the concept's description stays dormant until a link handler exists.</summary>
        public static string ScopeAndTiming(PartEffect fx)
        {
            if (fx == null) return string.Empty;
            return GameSymbols.Scope(fx.scope) + " " + GameText.ConceptName(GameSymbols.ScopeConcept(fx.scope))
                 + "  " + GameSymbols.Timing(fx.timing) + " " + GameText.ConceptName(GameSymbols.TimingConcept(fx.timing));
        }

        /// <summary>[D-CF-14] The modifier FAMILY of an effect, as a sprite name. Public because the
        /// card FACE now shows these glyphs too: one mapping from subclass to family, used by both
        /// surfaces. If this lived twice, a new PartEffect subclass could show one family on the face
        /// and another in the modal.</summary>
        public static string FamilySpriteName(PartEffect fx) => fx switch
        {
            MeterEffect => GameSymbols.ModMeter,
            TempoEffect => GameSymbols.ModTempo,
            ModulationEffect => GameSymbols.ModKey,
            InstrumentEffect => GameSymbols.ModInstrument,
            _ => null      // unmapped subclass: no glyph, and the modal shows its type name
        };

        /// <summary>The glossary concept id for that family, for the card hover stack.</summary>
        public static string FamilyConceptId(PartEffect fx) => fx switch
        {
            MeterEffect => GameSymbols.ModMeterConcept,
            TempoEffect => GameSymbols.ModTempoConcept,
            ModulationEffect => GameSymbols.ModKeyConcept,
            InstrumentEffect => GameSymbols.ModInstrumentConcept,
            _ => null
        };

        private static string BuildMeter(MeterEffect m)
            => GameText.Format(UiCtxMeter(m.timeSignature));

        private static string BuildTempo(TempoEffect t)
        {
            switch (t.mode)
            {
                case TempoEffect.TempoEffectMode.Range:
                    return GameText.Format(UiCtxTempo(t.tempoRange));
                case TempoEffect.TempoEffectMode.AbsoluteBpm:
                    // One concept, one id (D-T6-3=B): the context row already formats this phrase.
                    return GameText.Format(UiCtxBpmAbsolute, t.absoluteBpm);
                case TempoEffect.TempoEffectMode.ScaleFactor:
                    // Culture-correct decimal (D-T5-2=B / F-TXT4-10): "0,66" in es.
                    return GameText.Format(UiModTempoScale, GameText.Number(t.tempoScale, "0.##"));
                default:
                    return string.Empty;
            }
        }

        private static string BuildModulation(ModulationEffect mod)
        {
            string body;
            switch (mod.mode)
            {
                case ModulationEffect.ModulationMode.AbsoluteKey:
                    body = GameText.Format(UiModKey, GameText.Format(UiNote(mod.absoluteRoot)));
                    break;
                case ModulationEffect.ModulationMode.IntervalWithinScale:
                    body = GameText.Format(UiModDegree, GameText.Format(UiDegree(mod.targetDegree)));
                    break;
                case ModulationEffect.ModulationMode.RandomAny:
                    body = GameText.Format(UiModRandomKey);
                    break;
                case ModulationEffect.ModulationMode.RandomWithinScale:
                    body = GameText.Format(UiModRandomInScale);
                    break;
                default:
                    body = string.Empty;
                    break;
            }

            // [D-A6=B carried over] direction glyph only when non-default. Auto adds nothing.
            string dir = mod.octaveHint switch
            {
                ModulationOctaveHint.Up => " " + GameSymbols.Tag(GameSymbols.ModDirUp),
                ModulationOctaveHint.Down => " " + GameSymbols.Tag(GameSymbols.ModDirDown),
                _ => string.Empty
            };
            return body + dir;
        }

        private static string BuildInstrument(InstrumentEffect inst)
        {
            switch (inst.mode)
            {
                case InstrumentEffect.InstrumentTargetMode.SpecificMelodic:
                    // InstrumentName is MidiGenPlay asset data, passed through untranslated like
                    // ui_track_instrument {0} (TEXT-6 precedent, boundary section 8.13).
                    return inst.melodicInstrument != null
                        ? GameText.Format(UiModInstrument, inst.melodicInstrument.InstrumentName)
                        : "Set Instrument (Melodic)";                 // authoring-error diagnostic
                case InstrumentEffect.InstrumentTargetMode.SpecificPercussion:
                    return inst.percussionInstrument != null
                        ? GameText.Format(UiModDrums, inst.percussionInstrument.InstrumentName)
                        : "Set Drums";                                // authoring-error diagnostic
                case InstrumentEffect.InstrumentTargetMode.InstrumentType:
                    return GameText.Format(UiModInstrumentType,
                        GameText.Format(UiInstrumentType(inst.instrumentType)));
                case InstrumentEffect.InstrumentTargetMode.RandomFromList:
                    {
                        // [R2c / D-R2-7] count only usable entries (same rule as the apply path).
                        int usable = 0;
                        if (inst.melodicInstrumentPool != null)
                            for (int i = 0; i < inst.melodicInstrumentPool.Count; i++)
                                if (inst.melodicInstrumentPool[i] != null) usable++;
                        return usable > 0
                            ? GameText.Format(UiModInstrumentRandom, usable)
                            : "Random Instrument (empty pool)";        // authoring-error diagnostic
                    }
                default:
                    return string.Empty;
            }
        }
    }
}