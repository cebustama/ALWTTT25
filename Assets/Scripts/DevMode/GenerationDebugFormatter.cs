#if ALWTTT_DEV
using System.Collections.Generic;
using System.Text;
using ALWTTT.UI;
using MidiGenPlay;
using MidiGenPlay.Composition;

namespace ALWTTT.DevMode
{
    /// <summary>
    /// [DBG-C1] Role-adaptive text formatter for the composition-debug tab.
    ///
    /// Compact: one line per track — source + primary identity.
    /// Full: every populated ResolvedTrackChoice field for the role.
    ///
    /// '*' convention (Design_Composition_Debug_Tab_v0_1 §3.1 / assumption
    /// A1): a value carries '*' when it is resolved-only truth — it could
    /// not be predicted from the handoff intent because the composer picked
    /// it (CardPalette weighted pick, Procedural generation, or the
    /// per-part SharedProgression). Values that follow deterministically
    /// from intent (RenderOverride, CardOverride, TrackParameters) render
    /// without '*'.
    /// </summary>
    internal static class GenerationDebugFormatter
    {
        // ---------------------------------------------------------------
        // Intent (handoff) lines — from the UI model, pre-render truth.
        // ---------------------------------------------------------------
        public static string FormatIntentLine(SongCompositionUI.TrackEntry t)
        {
            if (t == null) return "(null track)";
            var sb = new StringBuilder();
            sb.Append(t.role).Append('[').Append(t.musicianId ?? "-").Append("]: ");
            sb.Append("style=").Append(t.styleBundle != null ? t.styleBundle.name : "-");
            if (t.overrideMelodicInstrument != null)
                sb.Append(" instOverride=").Append(t.overrideMelodicInstrument.InstrumentName);
            if (t.overridePercussionInstrument != null)
                sb.Append(" percOverride=").Append(t.overridePercussionInstrument.InstrumentName);
            if (t.hasOverrideInstrumentType)
                sb.Append(" typeOverride=").Append(t.overrideInstrumentType);
            if (t.inspirationGenerated != 0)
                sb.Append(" +INS=").Append(t.inspirationGenerated);
            return sb.ToString();
        }

        // ---------------------------------------------------------------
        // Resolved lines — from PartRender.resolvedByTrack.
        // ---------------------------------------------------------------
        public static string FormatResolvedBlock(
            IReadOnlyDictionary<MusicianTrackKey, ResolvedTrackChoice> resolved,
            IReadOnlyDictionary<MusicianTrackKey, MIDIInstrumentSO> pinned,
            bool full)
        {
            if (resolved == null || resolved.Count == 0)
                return "(resolvedByTrack empty — package reported nothing)";

            var sb = new StringBuilder();
            foreach (var kv in resolved)
            {
                sb.AppendLine(FormatResolvedLine(kv.Key, kv.Value, pinned, full));
            }
            return sb.ToString().TrimEnd();
        }

        public static string FormatResolvedLine(
            MusicianTrackKey key,
            ResolvedTrackChoice c,
            IReadOnlyDictionary<MusicianTrackKey, MIDIInstrumentSO> pinned,
            bool full)
        {
            if (c == null) return $"{key}: (null choice)";

            string star = IsResolvedOnly(c.source) ? "*" : "";
            var sb = new StringBuilder();
            sb.Append(key.Role).Append('[').Append(key.MusicianId).Append("]: ");
            sb.Append("src=").Append(c.source).Append(star);

            switch (key.Role)
            {
                case TrackRole.Rhythm:
                    Append(sb, "pattern", c.sourceAssetName, star);
                    Append(sb, "palette", c.paletteName, star);
                    Append(sb, "style", c.proceduralStyleId, star);
                    break;

                case TrackRole.Backing:
                    Append(sb, "pattern", c.sourceAssetName, star);
                    Append(sb, "palette", c.paletteName, star);
                    Append(sb, "roman", c.progressionRoman, star);
                    if (c.resolvedFigures != null && c.resolvedFigures.Count > 0)
                    {
                        sb.Append(" figures").Append(star).Append('=');
                        sb.Append(full
                            ? string.Join(",", c.resolvedFigures)
                            : $"{c.resolvedFigures.Count} (Random)");
                    }
                    break;

                case TrackRole.Melody:
                    Append(sb, "asset", c.sourceAssetName, star);
                    if (c.melodyArchetypesBySpan != null && c.melodyArchetypesBySpan.Count > 0)
                    {
                        sb.Append(" archetypes").Append(star).Append('=');
                        sb.Append(full
                            ? string.Join(",", ReplaceNulls(c.melodyArchetypesBySpan))
                            : $"{c.melodyArchetypesBySpan.Count} spans");
                    }
                    break;

                case TrackRole.Bassline:
                    sb.Append(" sharedProg=").Append(c.usesSharedProgression ? "yes" : "no");
                    Append(sb, "roman", c.progressionRoman, star);
                    break;

                default:
                    // Harmony not reported in v1 (ID-2=A) — anything else
                    // renders generic fields.
                    Append(sb, "asset", c.sourceAssetName, star);
                    break;
            }

            if (pinned != null && pinned.TryGetValue(key, out var inst) && inst != null)
                sb.Append(" inst=").Append(inst.InstrumentName);

            return sb.ToString();
        }

        // ---------------------------------------------------------------
        // Fingerprint export (Copy button — always Full).
        // ---------------------------------------------------------------
        public static string BuildFingerprint(
            int? seed,
            int partIndex,
            int bpm,
            bool fromCache,
            IReadOnlyDictionary<MusicianTrackKey, ResolvedTrackChoice> resolved,
            IReadOnlyDictionary<MusicianTrackKey, MIDIInstrumentSO> pinned)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ALWTTT part fingerprint [DBG-C1] ===");
            sb.Append("seed=").Append(seed?.ToString() ?? "-")
              .Append(" part=").Append(partIndex)
              .Append(" bpm=").Append(bpm)
              .Append(" replay=").Append(fromCache ? "bundle-cache" : "fresh")
              .AppendLine();
            sb.AppendLine(FormatResolvedBlock(resolved, pinned, full: true));
            return sb.ToString();
        }

        // ---------------------------------------------------------------

        private static bool IsResolvedOnly(ResolvedSource src) =>
            src == ResolvedSource.CardPalette
            || src == ResolvedSource.Procedural
            || src == ResolvedSource.SharedProgression;

        private static void Append(StringBuilder sb, string label, string value, string star)
        {
            if (string.IsNullOrEmpty(value)) return;
            sb.Append(' ').Append(label).Append(star).Append('=').Append(value);
        }

        private static IEnumerable<string> ReplaceNulls(List<string> list)
        {
            foreach (var s in list) yield return s ?? "(null-ref)";
        }
    }
}
#endif
