#if UNITY_EDITOR && ALWTTT_DEV
// [INV-PROG-1] Progression CONTENT view + export.
//
// Every other chord-progression surface in this window reports metadata
// (TS, measures, event COUNT, refs, health flags). This file reports what the
// progression actually says musically - one chord per event, readable as a
// Roman line - and which cards can put it in front of a player, walking the
// full chain card -> bundle -> palette/override -> progression.
//
// Two rules shape everything below:
//
//  1. EVENTS ARE THE TRUTH, originalInput IS PROVENANCE. The Roman line is
//     rebuilt from ChordProgressionData.events, never copied from
//     originalInput: a grid edit after import changes the events and leaves
//     originalInput stale. originalInput is exported beside it so the two can
//     be compared by eye; ORIG-COUNT flags the one mismatch that can be
//     detected without guessing spellings (different chord count).
//
//  2. AUTHORED, NOT RENDERED. Under DiatonicToPart / DiatonicToPartFunctional
//     (and the color table, and SECDOM) the chord that SOUNDS depends on the
//     part tonality at render time. This export shows what is authored plus
//     the policy that transforms it. It never simulates the render.
//
// The Roman line is a DISPLAY format with its own legend. It is NOT the
// ChordProgressionEditor import grammar and is not designed to be re-imported.
// Enum names (ScaleDegree / ChordQuality) are package-owned and append-only,
// so spelling couples by NAME, not by ordinal (same boundary rule as LOG-1,
// D-LOG-1=B). An unknown name is surfaced as a flag, never guessed silently.
//
// Strictly read-only, like the rest of the window (ST-CSV-7).
using MidiGenPlay;
using MidiGenPlay.Composition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.DevMode.Editor
{
    public sealed partial class CompositionInventoryWindow
    {
        // ══════════════════════════════════════════════════════════════════
        // Model
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Best reach of a progression, reusing the §17.6 vocabulary one layer
        /// down. Order matters: lower = better; a progression takes the best
        /// tier over all of its card links.
        /// "Usable" (D2=B) means Wired. Same three-hop floor as BundleCardFlags:
        /// a roster no scene loads still reads as wired.
        /// </summary>
        private enum ProgTier { Wired = 0, UnwiredSrc = 1, Unsourced = 2, Unreachable = 3 }

        private static string TierLabel(ProgTier t) => t switch
        {
            ProgTier.Wired => "WIRED",
            ProgTier.UnwiredSrc => "UNWIRED-SRC",
            ProgTier.Unsourced => "UNSOURCED",
            _ => "UNREACHABLE"
        };

        /// <summary>One bundle that reaches a progression, and how.</summary>
        private sealed class ProgBundleReach
        {
            public BackingCardConfigSO bundle;
            public string via;            // "override" or "palette:<name>"
        }

        private sealed class ProgEventRow
        {
            public int index;
            public int startStep;
            public int lengthSteps;
            public string position;       // "c<measure>.<beat>", 1-based
            public float lengthMeasures;
            public string degree;
            public int degreeAccidental;
            public string quality;
            public bool isDiatonic;
            public int velocity;
            public bool hasAppliedTarget;
            public string appliedTarget;  // "" unless hasAppliedTarget
            public string token;          // Roman display token
        }

        private sealed class ProgCardRow
        {
            public string cardId;
            public string owner;
            public string cardRole;
            public string bundle;
            public string via;
            public string sources;
            public ProgTier tier;
            public string articulation;   // [INV-ARTIC-1] Backing figure token of `bundle`
        }

        private sealed class ProgRow
        {
            public ChordProgressionData asset;
            public string roman;
            public int beatsPerMeasure;
            public readonly List<ProgEventRow> events = new();
            public readonly List<string> bundles = new();
            public readonly List<ProgCardRow> cards = new();
            public bool inConfigLibrary;
            public ProgTier tier = ProgTier.Unreachable;
            public readonly List<string> flags = new();
            public bool Usable => tier == ProgTier.Wired;
        }

        [SerializeField] private bool _progOnlyUsable;

        // progression -> bundles that reach it. Rebuilt lazily whenever the
        // catalogue is refreshed: RefreshCatalog REASSIGNS _chords, so the list
        // instance is a free and exact cache key (no edit to RefreshCatalog).
        private readonly Dictionary<ChordProgressionData, List<ProgBundleReach>> _progReach = new();
        private List<ChordProgressionData> _progReachKey;

        private void EnsureProgressionReach()
        {
            if (ReferenceEquals(_progReachKey, _chords)) return;
            _progReach.Clear();

            void Add(ChordProgressionData p, BackingCardConfigSO b, string via)
            {
                if (p == null || b == null) return;
                if (!_progReach.TryGetValue(p, out var list))
                    _progReach[p] = list = new List<ProgBundleReach>();
                if (list.Any(r => r.bundle == b && r.via == via)) return;
                list.Add(new ProgBundleReach { bundle = b, via = via });
            }

            foreach (var b in _bundles)
            {
                if (!(b is BackingCardConfigSO bk)) continue;
                Add(bk.progressionOverride, bk, "override");
                var pal = bk.progressionPalette;
                if (pal != null && pal.entries != null)
                    foreach (var e in pal.entries)
                        Add(e?.progression, bk, $"palette:{pal.name}");
            }
            _progReachKey = _chords;
        }

        // ══════════════════════════════════════════════════════════════════
        // Row construction
        // ══════════════════════════════════════════════════════════════════

        private List<ProgRow> BuildProgressionRows()
        {
            EnsureProgressionReach();
            return _chords.Where(p => p != null)
                          .Select(BuildProgressionRow)
                          .OrderBy(r => r.tier)
                          .ThenBy(r => r.bundles.FirstOrDefault() ?? "~")
                          .ThenBy(r => r.asset.name, StringComparer.OrdinalIgnoreCase)
                          .ToList();
        }

        private ProgRow BuildProgressionRow(ChordProgressionData p)
        {
            var row = new ProgRow { asset = p };
            int bpm = Mathf.Max(1, TimeSignatureProperties[p.TimeSignature].BeatsPerMeasure);
            int sub = Mathf.Max(1, p.subdivisions);
            int stepsPerMeasure = bpm * sub;
            row.beatsPerMeasure = bpm;

            bool unknownDegree = false, degreeByOrder = false, unknownQuality = false;
            var ordered = (p.events ?? new List<ChordProgressionData.ChordEvent>())
                .Where(e => e != null)
                .OrderBy(e => e.startStep)
                .ToList();

            int prevEnd = -1;
            bool innerGap = false, overlap = false;
            for (int i = 0; i < ordered.Count; i++)
            {
                var e = ordered[i];
                string token = ChordToken(e.degree, e.degreeAccidental, e.quality,
                    out bool degKnown, out bool degOrder, out bool qualKnown);
                unknownDegree |= !degKnown;
                degreeByOrder |= degOrder;
                unknownQuality |= !qualKnown;

                if (e.hasAppliedTarget)
                {
                    // Authored values are ignored at render while the flag is on
                    // (D-SD-OWN=A). Show both: what is stored, and what it
                    // becomes. Target case depends on the part mode, so the
                    // target numeral is shown uppercase and the legend says so.
                    token += "→V7/" + DegreeNumeral(e.appliedTarget, out _, out _);
                }

                float beat = e.startStep / (float)sub;
                int measure = Mathf.FloorToInt(beat / bpm) + 1;
                float beatInMeasure = beat - (measure - 1) * bpm + 1f;

                row.events.Add(new ProgEventRow
                {
                    index = i + 1,
                    startStep = e.startStep,
                    lengthSteps = e.lengthSteps,
                    position = $"c{measure}.{Num(beatInMeasure)}",
                    lengthMeasures = e.lengthSteps / (float)stepsPerMeasure,
                    degree = e.degree.ToString(),
                    degreeAccidental = e.degreeAccidental,
                    quality = e.quality.ToString(),
                    isDiatonic = e.isDiatonic,
                    velocity = e.velocity,
                    hasAppliedTarget = e.hasAppliedTarget,
                    appliedTarget = e.hasAppliedTarget ? e.appliedTarget.ToString() : "",
                    token = token
                });

                if (prevEnd >= 0 && e.startStep > prevEnd) innerGap = true;
                if (prevEnd >= 0 && e.startStep < prevEnd) overlap = true;
                prevEnd = Mathf.Max(prevEnd, e.startStep + Mathf.Max(1, e.lengthSteps));
            }

            row.roman = row.events.Count == 0
                ? "(sin eventos)"
                : string.Join(" - ", row.events.Select(ev => $"{ev.token} ({Num(ev.lengthMeasures)})"));

            // ---- Reach ----
            if (_progReach.TryGetValue(p, out var reaches))
            {
                foreach (var r in reaches)
                {
                    string bundleLabel = $"{r.bundle.name} [{r.via}]";
                    if (!row.bundles.Contains(bundleLabel)) row.bundles.Add(bundleLabel);

                    if (!_bundleToCards.TryGetValue(r.bundle, out var links)) continue;
                    foreach (var l in links)
                    {
                        var t = l.anyWiredSource ? ProgTier.Wired
                              : l.AnySource ? ProgTier.UnwiredSrc
                              : ProgTier.Unsourced;
                        string id = l.card.Id ?? l.card.name;
                        if (row.cards.Any(c => c.cardId == id && c.bundle == r.bundle.name)) continue;
                        row.cards.Add(new ProgCardRow
                        {
                            cardId = id,
                            owner = LinkOwnerLabel(l),
                            cardRole = l.cardRole.ToString(),
                            bundle = r.bundle.name,
                            via = r.via,
                            sources = l.AnySource ? string.Join(";", l.sources) : "—",
                            articulation = BackingArticulationShort(r.bundle), // [INV-ARTIC-1]
                            tier = t
                        });
                        if (t < row.tier) row.tier = t;
                    }
                }
            }

            row.inConfigLibrary = _cfg != null && _cfg.progressionLibrary != null
                && _cfg.progressionLibrary.entries != null
                && _cfg.progressionLibrary.entries.Any(e => e?.progression == p);

            // ---- Flags: existing health flags + content-specific ones ----
            row.flags.AddRange(ProgressionFlags(p));
            if (innerGap) row.flags.Add("INNER-GAP");
            if (overlap) row.flags.Add("OVERLAP");
            if (unknownDegree) row.flags.Add("UNKNOWN-DEGREE");
            if (degreeByOrder) row.flags.Add("DEGREE-BY-ORDER");
            if (unknownQuality) row.flags.Add("UNKNOWN-QUALITY");
            int origCount = OriginalInputChordCount(p.originalInput);
            if (origCount >= 0 && origCount != row.events.Count)
                row.flags.Add($"ORIG-COUNT orig={origCount} events={row.events.Count}");
            return row;
        }

        // ══════════════════════════════════════════════════════════════════
        // Roman display spelling (display-only; see file header)
        // ══════════════════════════════════════════════════════════════════

        private static readonly string[] RomanNumerals = { "I", "II", "III", "IV", "V", "VI", "VII" };

        private static string DegreeNumeral(ScaleDegree d, out bool known, out bool byOrder)
        {
            known = true; byOrder = false;
            switch (d.ToString())
            {
                case "Tonic": return "I";
                case "Supertonic": return "II";
                case "Mediant": return "III";
                case "Subdominant": return "IV";
                case "Dominant": return "V";
                case "Submediant": return "VI";
                case "LeadingTone":
                case "Leading":
                case "Subtonic": return "VII";
            }
            // Name not recognised: fall back to enum VALUE order (Enum.GetValues sorts by value), and SAY so
            // (DEGREE-BY-ORDER) rather than guess silently.
            int idx = Array.IndexOf(Enum.GetValues(typeof(ScaleDegree)), d);
            if (idx >= 0 && idx < RomanNumerals.Length) { byOrder = true; return RomanNumerals[idx]; }
            known = false;
            return "?" + d;
        }

        /// <summary>
        /// Numeral case follows the triad family (minor/diminished lowercase),
        /// suffix is explicit - the convention the fase-A spec already uses
        /// ("im7", "IV9", "Imaj7", "vim9").
        /// </summary>
        private static bool QualitySpelling(ChordQuality q, out bool lower, out string suffix)
        {
            switch (q.ToString())
            {
                case "Major": lower = false; suffix = ""; return true;
                case "Minor": lower = true; suffix = ""; return true;
                case "Diminished": lower = true; suffix = "°"; return true;
                case "Augmented": lower = false; suffix = "+"; return true;
                case "Dominant7": lower = false; suffix = "7"; return true;
                case "Major7": lower = false; suffix = "maj7"; return true;
                case "Minor7": lower = true; suffix = "m7"; return true;
                case "HalfDiminished7": lower = true; suffix = "ø7"; return true;
                case "Diminished7": lower = true; suffix = "°7"; return true;
                case "Sus2": lower = false; suffix = "sus2"; return true;
                case "Sus4": lower = false; suffix = "sus4"; return true;
                case "Major6": lower = false; suffix = "6"; return true;
                case "Minor6": lower = true; suffix = "m6"; return true;
                case "Dominant7sus4": lower = false; suffix = "7sus4"; return true;
                case "Dominant9": lower = false; suffix = "9"; return true;
                case "Major9": lower = false; suffix = "maj9"; return true;
                case "Minor9": lower = true; suffix = "m9"; return true;
                default: lower = false; suffix = "[" + q + "]"; return false;
            }
        }

        private static string ChordToken(ScaleDegree d, int accidental, ChordQuality q,
            out bool degKnown, out bool degByOrder, out bool qualKnown)
        {
            string numeral = DegreeNumeral(d, out degKnown, out degByOrder);
            qualKnown = QualitySpelling(q, out bool lower, out string suffix);
            if (lower) numeral = numeral.ToLowerInvariant();
            string prefix = accidental < 0 ? new string('b', -accidental)
                          : accidental > 0 ? new string('#', accidental)
                          : "";
            return prefix + numeral + suffix;
        }

        /// <summary>
        /// Chord count of originalInput, or -1 when there is none.
        /// [INV-PROG-1b] Counts ROMAN TOKENS (optional accidental + numeral +
        /// optional known suffix, bounded by non-alphanumerics), not
        /// dash-separated pieces. The v1 dash split counted every piece, so
        /// rest tokens ("S (0.22)", imported as gaps, not events) doubled the
        /// count: that was the orig=36 events=18 row ("44 Test", verified
        /// 2026-09-28 from the export). "S" is not a numeral, so it never
        /// matches. [INV-PROG-1c] Bare "m" / "maj" (IIm, vim, Imaj) added
        /// to the suffix set; checked against all 48 real originalInputs:
        /// 0 mismatches. Words such
        /// as "Ionian" or "via" do not match: a numeral followed by a letter
        /// outside the suffix alphabet is rejected. Known limits, accepted
        /// because the flag is advisory: slash notation (V7/V) counts two, an
        /// out-of-alphabet suffix (add9) counts zero.
        /// </summary>
        private static readonly System.Text.RegularExpressions.Regex RomanTokenRx =
            new System.Text.RegularExpressions.Regex(
                @"(?<![A-Za-z0-9])[b#♭♯]?(?:VII|VI|IV|V|III|II|I|vii|vi|iv|v|iii|ii|i)" +
                @"(?:maj9|maj7|maj|m7b5|m9|m7|m6|m|7sus4|sus2|sus4|dim7|dim|aug|ø7|°7|°|\+|9|7|6)?" +
                @"(?![A-Za-z0-9])",
                System.Text.RegularExpressions.RegexOptions.CultureInvariant);

        private static int OriginalInputChordCount(string original)
        {
            if (string.IsNullOrWhiteSpace(original)) return -1;
            return RomanTokenRx.Matches(original).Count;
        }

        private static string Num(float v)
            => v.ToString("0.##", CultureInfo.InvariantCulture);

        private static string PolicyShort(ChordProgressionData p)
        {
            string pol = p.qualityRenderPolicy switch
            {
                ChordProgressionData.QualityRenderPolicy.AsAuthored => "AA",
                ChordProgressionData.QualityRenderPolicy.DiatonicToPart => "DTP",
                ChordProgressionData.QualityRenderPolicy.DiatonicToPartFunctional => "DTPF",
                _ => p.qualityRenderPolicy.ToString()
            };
            // Color table is only effective under a DTP* policy (REQUALITY-2).
            if (p.useColorTable)
                pol += p.qualityRenderPolicy == ChordProgressionData.QualityRenderPolicy.AsAuthored
                    ? " +color(inerte)" : " +color";
            return pol;
        }

        private static string TonalitiesLabel(ChordProgressionData p)
            => p.tonalities == null || p.tonalities.Count == 0
                ? "Any"
                : string.Join(", ", p.tonalities.Select(t => t.ToString()));

        // ══════════════════════════════════════════════════════════════════
        // View
        // ══════════════════════════════════════════════════════════════════

        private void DrawProgressionContent()
        {
            var rows = BuildProgressionRows();

            using (new EditorGUILayout.HorizontalScope())
            {
                _progOnlyUsable = GUILayout.Toggle(
                    _progOnlyUsable, "Only usable (WIRED)", GUILayout.Width(150));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Copy Markdown", GUILayout.Width(110)))
                {
                    EditorGUIUtility.systemCopyBuffer = BuildProgressionMarkdown(rows);
                    Debug.Log($"[CompositionInventory] Progression content Markdown copied " +
                              $"({rows.Count} progressions).");
                }
                if (GUILayout.Button("Export Markdown…", GUILayout.Width(130)))
                    ExportProgressionMarkdown(rows);
            }

            EditorGUILayout.HelpBox(
                "Authored content, rebuilt from events (not from originalInput). " +
                "DTP/DTPF/color/SECDOM transform it at render - the policy column says how.\n" +
                "Usable = at least one card reaching it sits in a WIRED catalogue or deck " +
                "(three-hop floor; see the Cards → Bundles source notes).\n" +
                "Exports ignore the view filters: they always contain every progression.",
                MessageType.None);

            EditorGUILayout.LabelField(
                $"Progressions: {rows.Count}  |  " +
                string.Join("  ·  ", Enum.GetValues(typeof(ProgTier)).Cast<ProgTier>()
                    .Select(t => $"{TierLabel(t)} {rows.Count(r => r.tier == t)}")),
                EditorStyles.boldLabel);

            foreach (var r in rows)
            {
                var p = r.asset;
                if (_progOnlyUsable && !r.Usable) continue;
                // [INV-PROG-1b] In this view the free-text filter also searches the
                // Roman line, the flags, the tier and the card ids, so "UNKNOWN",
                // "V7/" or a card id find rows. Passed through the displayName
                // argument: the shared PassesCommonFilters is NOT changed, so every
                // other view keeps filtering by name only.
                string searchText = p.DisplayName + " " + r.roman + " " + TierLabel(r.tier) + " " +
                    string.Join(" ", r.flags) + " " + string.Join(" ", r.cards.Select(c => c.cardId));
                if (!PassesCommonFilters(p, searchText, p.TimeSignature, r.flags)) continue;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"[{SourceTag(p)}]", GUILayout.Width(40));
                        GUILayout.Label(p.name, EditorStyles.boldLabel, GUILayout.Width(240));
                        GUILayout.Label($"{p.TimeSignature} {p.Measures}c", GUILayout.Width(110));
                        GUILayout.Label(PolicyShort(p), GUILayout.Width(110));
                        GUILayout.Label($"ton={TonalitiesLabel(p)}", GUILayout.Width(340));
                        GUILayout.Label($"cad={p.cadence}", GUILayout.Width(100));
                        GUILayout.Label(TierLabel(r.tier), GUILayout.Width(100));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Ping", GUILayout.Width(44)))
                            EditorGUIUtility.PingObject(p);
                    }
                    EditorGUILayout.SelectableLabel(r.roman, EditorStyles.textField,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight + 2));
                    GUILayout.Label(r.cards.Count == 0
                        ? $"cards: —   bundles: {(r.bundles.Count == 0 ? "—" : string.Join(", ", r.bundles))}"
                        : "cards: " + string.Join(", ", r.cards.Select(c =>
                            $"{c.cardId} ({c.owner}, {TierLabel(c.tier)}) via {c.bundle} [{c.via}] · {c.articulation}")),
                        EditorStyles.wordWrappedMiniLabel);
                    // [INV-PROG-1b] Make ORIG-COUNT diagnosable in place: show the raw
                    // provenance string, line breaks rendered as ⏎.
                    if (!string.IsNullOrWhiteSpace(p.originalInput) &&
                        r.flags.Any(f => f.StartsWith("ORIG-COUNT", StringComparison.Ordinal)))
                        GUILayout.Label("originalInput: " +
                            p.originalInput.Replace("\r", "").Replace("\n", " ⏎ "),
                            EditorStyles.wordWrappedMiniLabel);
                    DrawFlags(r.flags);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // Print (Console)
        // ══════════════════════════════════════════════════════════════════

        private void AppendProgressionContent(StringBuilder sb)
        {
            foreach (var r in BuildProgressionRows())
                sb.AppendLine(
                    $"  {TierLabel(r.tier),-11} | [{SourceTag(r.asset)}] {r.asset.name} | " +
                    $"{r.asset.TimeSignature} {r.asset.Measures}c | {PolicyShort(r.asset)} | " +
                    $"ton={TonalitiesLabel(r.asset)} | {r.roman} | cards=" +
                    (r.cards.Count == 0 ? "—" : string.Join(",", r.cards.Select(c => $"{c.cardId}<{c.articulation}>"))) +
                    (r.flags.Count > 0 ? $" | flags={string.Join(";", r.flags)}" : ""));
        }

        // ══════════════════════════════════════════════════════════════════
        // JSON export (called from BuildJsonForView -> Export JSON / Export All)
        // ══════════════════════════════════════════════════════════════════

        [Serializable]
        private class JsonProgEvent
        {
            public int index;
            public int startStep;
            public int lengthSteps;
            public string position;
            public float lengthMeasures;
            public string degree;
            public int degreeAccidental;
            public string quality;
            public bool isDiatonic;
            public int velocity;
            public bool hasAppliedTarget;
            public string appliedTarget;
            public string token;
        }

        [Serializable]
        private class JsonProgCard
        {
            public string cardId;
            public string owner;
            public string cardRole;
            public string bundle;
            public string via;
            public string sources;
            public string tier;
            public string articulation;   // [INV-ARTIC-1]
        }

        [Serializable]
        private class JsonProgression
        {
            public string assetName;
            public string displayName;
            public string source;
            public string assetPath;
            public string timeSignature;
            public int measures;
            public int beatsPerMeasure;
            public int subdivisions;
            public string roman;
            public string originalInput;
            public List<string> songReferences = new();
            public string qualityRenderPolicy;
            public bool useColorTable;
            public string cadence;
            public List<string> tonalities = new();   // empty = any tonality
            public List<JsonProgEvent> events = new();
            public string refs;
            public List<string> bundles = new();
            public List<JsonProgCard> cards = new();
            public bool inConfigLibrary;
            public string tier;
            public bool usable;
            public string flags;
        }

        [Serializable]
        private class WrapProgressionContent
        {
            public List<string> sourceNotes = new();
            public List<string> legend = new();
            public List<JsonProgression> progressions = new();
        }

        private static readonly string[] ProgLegend =
        {
            "roman: display format rebuilt from events; NOT the editor import grammar.",
            "(n) = duration in measures. Numeral case = triad family; suffix explicit.",
            "° dim · + aug · ø7 half-dim · °7 dim7 · b/# = degreeAccidental.",
            "X→V7/Y = SECDOM: authored X is ignored at render and becomes V7 of Y; Y's case depends on the part mode.",
            "policy AA/DTP/DTPF: under DTP* the sounding quality follows the part tonality; +color = REQUALITY-2 color table.",
            "tier (best over card links): WIRED > UNWIRED-SRC > UNSOURCED > UNREACHABLE. usable = WIRED (D2=B).",
            "inConfigLibrary: listed in MidiGenPlayConfig.progressionLibrary. Not counted as card reach (runtime fallback semantics unverified).",
            "cards[].articulation: Backing figure token. Figure[{pool} reroll=r][ @Rate][ ±jitter][ +adopt]; '(not admitted)' = ignored by the package today; '@Random' currently drops articulation (F-ARTIC-RATE-RANDOM-1)." // [INV-ARTIC-1]
        };

        private string BuildProgressionContentJson()
        {
            var w = new WrapProgressionContent();
            w.sourceNotes.AddRange(_cardSourceNotes);
            w.sourceNotes.Add($"Reference part measures used for BASS-GAP: {_referencePartMeasures}");
            w.legend.AddRange(ProgLegend);

            foreach (var r in BuildProgressionRows())
            {
                var p = r.asset;
                var jp = new JsonProgression
                {
                    assetName = p.name,
                    displayName = p.DisplayName,
                    source = SourceTag(p),
                    assetPath = AssetDatabase.GetAssetPath(p),
                    timeSignature = p.TimeSignature.ToString(),
                    measures = p.Measures,
                    beatsPerMeasure = r.beatsPerMeasure,
                    subdivisions = p.subdivisions,
                    roman = r.roman,
                    originalInput = p.originalInput ?? "",
                    qualityRenderPolicy = p.qualityRenderPolicy.ToString(),
                    useColorTable = p.useColorTable,
                    cadence = p.cadence.ToString(),
                    refs = RefsLabel(p),
                    inConfigLibrary = r.inConfigLibrary,
                    tier = TierLabel(r.tier),
                    usable = r.Usable,
                    flags = string.Join(";", r.flags)
                };
                if (p.songReferences != null) jp.songReferences.AddRange(p.songReferences);
                if (p.tonalities != null) jp.tonalities.AddRange(p.tonalities.Select(t => t.ToString()));
                jp.bundles.AddRange(r.bundles);
                foreach (var e in r.events)
                    jp.events.Add(new JsonProgEvent
                    {
                        index = e.index,
                        startStep = e.startStep,
                        lengthSteps = e.lengthSteps,
                        position = e.position,
                        lengthMeasures = e.lengthMeasures,
                        degree = e.degree,
                        degreeAccidental = e.degreeAccidental,
                        quality = e.quality,
                        isDiatonic = e.isDiatonic,
                        velocity = e.velocity,
                        hasAppliedTarget = e.hasAppliedTarget,
                        appliedTarget = e.appliedTarget,
                        token = e.token
                    });
                foreach (var c in r.cards)
                    jp.cards.Add(new JsonProgCard
                    {
                        cardId = c.cardId,
                        owner = c.owner,
                        cardRole = c.cardRole,
                        bundle = c.bundle,
                        via = c.via,
                        sources = c.sources,
                        tier = TierLabel(c.tier),
                        articulation = c.articulation // [INV-ARTIC-1]
                    });
                w.progressions.Add(jp);
            }
            return JsonUtility.ToJson(w, true);
        }

        // ══════════════════════════════════════════════════════════════════
        // Markdown export (the human-readable list)
        // ══════════════════════════════════════════════════════════════════

        private void ExportProgressionMarkdown(List<ProgRow> rows)
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Progression Content (Markdown)", "",
                "CompositionInventory_ProgressionContent.md", "md");
            if (string.IsNullOrEmpty(path)) return;
            File.WriteAllText(path, BuildProgressionMarkdown(rows), new UTF8Encoding(false));
            Debug.Log($"[CompositionInventory] Exported progression content to {path}");
            EditorUtility.RevealInFinder(path);
        }

        private static string Md(string s)
            => string.IsNullOrEmpty(s) ? "—" : s.Replace("|", "\\|").Replace("\r", "").Replace("\n", " ");

        private string BuildProgressionMarkdown(List<ProgRow> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Progresiones de acordes — contenido autorado");
            sb.AppendLine();
            sb.AppendLine($"Generado {DateTime.Now:yyyy-MM-dd HH:mm} desde Composition Inventory " +
                          "(vista Progression Content). Contiene **todas** las progresiones " +
                          "descubiertas; los filtros de la ventana no se aplican.");
            sb.AppendLine();
            sb.AppendLine("**Leyenda.**");
            sb.AppendLine("- La línea Roman se reconstruye desde los eventos del asset. Es formato de lectura, " +
                          "no la gramática de importación del editor.");
            sb.AppendLine("- `(n)` = duración en compases. Mayúscula/minúscula del numeral = familia de la tríada; " +
                          "el sufijo es explícito (`im7`, `IV9`, `Imaj7`). `°` dim · `+` aum · `ø7` semidisminuido · " +
                          "`°7` dim7 · `b`/`#` = alteración del grado.");
            sb.AppendLine("- `X→V7/Y` = SECDOM: el acorde autorado X se ignora al renderizar y suena como V7 de Y; " +
                          "la caja de Y depende del modo de la parte.");
            sb.AppendLine("- Política: **AA** suena como está escrita; **DTP/DTPF** re-resuelve las calidades a la " +
                          "tonalidad de la parte (DTPF conserva la dominante funcional); `+color` = tabla de color. " +
                          "Bajo DTP* el acorde que suena puede diferir del escrito.");
            sb.AppendLine("- **Utilizable (WIRED)** = al menos una carta que la alcanza está en un catálogo o mazo " +
                          "conectado (catálogo en un músico, mazo en un roster). Es un suelo de tres saltos, no un censo.");
            sb.AppendLine("- **Articulación** (por carta): figura del bundle de Backing. `{…}` = pool de `Random` con " +
                          "pesos (`(not admitted)` = el paquete la ignora hoy); `reroll` = probabilidad de cambiar de " +
                          "figura por acorde; `@Rate` = velocidad, solo si la figura la usa o si es `Random`; " +
                          "`±n` = jitter de intensidad; `+adopt` = adopta la tonalidad de la progresión. " +
                          "**`@Random` hoy pierde la articulación sin aviso** (F-ARTIC-RATE-RANDOM-1)."); // [INV-ARTIC-1]
            sb.AppendLine();

            sb.AppendLine("## Resumen");
            sb.AppendLine();
            sb.AppendLine("| Nivel | Progresiones |");
            sb.AppendLine("|---|---|");
            foreach (ProgTier t in Enum.GetValues(typeof(ProgTier)))
                sb.AppendLine($"| {TierLabel(t)} | {rows.Count(r => r.tier == t)} |");
            sb.AppendLine($"| **Total** | **{rows.Count}** |");
            sb.AppendLine();
            sb.AppendLine("Fuentes escaneadas:");
            foreach (var n in _cardSourceNotes) sb.AppendLine($"- {Md(n)}");
            sb.AppendLine();

            var sections = new (ProgTier tier, string title)[]
            {
                (ProgTier.Wired, "Utilizables en juego (WIRED)"),
                (ProgTier.UnwiredSrc, "Alcanzadas por cartas en fuentes no conectadas (UNWIRED-SRC)"),
                (ProgTier.Unsourced, "Alcanzadas por cartas que ninguna fuente reparte (UNSOURCED)"),
                (ProgTier.Unreachable, "Sin ninguna carta que las alcance (UNREACHABLE)")
            };
            foreach (var (tier, title) in sections)
            {
                var sec = rows.Where(r => r.tier == tier).ToList();
                sb.AppendLine($"## {title} — {sec.Count}");
                sb.AppendLine();
                if (sec.Count == 0) { sb.AppendLine("_(ninguna)_"); sb.AppendLine(); continue; }
                sb.AppendLine("| Progresión | Roman | Compás | Política | Tonalidades | Cadencia | Cartas (vía) |");
                sb.AppendLine("|---|---|---|---|---|---|---|");
                foreach (var r in sec)
                {
                    var p = r.asset;
                    string cards = r.cards.Count == 0
                        ? (r.bundles.Count == 0 ? "—" : "sin carta · bundles: " + string.Join(", ", r.bundles))
                        : string.Join("<br>", r.cards.Select(c =>
                            $"{c.cardId} ({c.owner}) · {c.bundle} [{c.via}] · {c.articulation}"
                            + (c.tier != r.tier ? $" · {TierLabel(c.tier)}" : "")));
                    sb.AppendLine($"| `{Md(p.name)}` | `{Md(r.roman)}` | {p.TimeSignature} · {p.Measures}c | " +
                                  $"{Md(PolicyShort(p))} | {Md(TonalitiesLabel(p))} | {p.cadence} | {Md(cards)} |");
                }
                sb.AppendLine();
            }

            // [INV-ARTIC-1] The composer's question: which figures play over which
            // progressions. Grouped by articulation token; only card-reached links
            // count (a bundle that no card uses plays nothing).
            sb.AppendLine("## Articulación × progresión");
            sb.AppendLine();
            var articGroups = rows
                .SelectMany(r => r.cards.Select(c => (row: r, card: c)))
                .GroupBy(t => t.card.articulation ?? "—")
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .ToList();
            if (articGroups.Count == 0)
            {
                sb.AppendLine("_(ninguna carta alcanza una progresión)_");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("| Articulación | Progresiones | Cartas |");
                sb.AppendLine("|---|---|---|");
                foreach (var g in articGroups)
                {
                    string articProgs = string.Join("<br>", g.Select(t => t.row.asset.name)
                        .Distinct()
                        .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                        .Select(n => $"`{Md(n)}`"));
                    string articCards = string.Join(", ", g
                        .Select(t => $"{t.card.cardId} ({TierLabel(t.card.tier)})")
                        .Distinct());
                    sb.AppendLine($"| `{Md(g.Key)}` | {articProgs} | {Md(articCards)} |");
                }
                sb.AppendLine();
            }
            sb.AppendLine("## Detalle por progresión");
            sb.AppendLine();
            foreach (var r in rows)
            {
                var p = r.asset;
                sb.AppendLine($"### {Md(p.name)} — {TierLabel(r.tier)}");
                sb.AppendLine();
                sb.AppendLine($"- Ruta: `{Md(AssetDatabase.GetAssetPath(p))}` ({SourceTag(p)})");
                sb.AppendLine($"- {p.TimeSignature} · {p.Measures} compases · subdiv {p.subdivisions} · " +
                              $"política {Md(PolicyShort(p))} · tonalidades {Md(TonalitiesLabel(p))} · " +
                              $"cadencia {p.cadence}");
                sb.AppendLine($"- Roman (desde eventos): `{Md(r.roman)}`");
                sb.AppendLine($"- originalInput: {(string.IsNullOrWhiteSpace(p.originalInput) ? "—" : "`" + Md(p.originalInput) + "`")}");
                if (p.songReferences != null && p.songReferences.Count > 0)
                    sb.AppendLine($"- Referencias: {Md(string.Join("; ", p.songReferences))}");
                sb.AppendLine($"- Referenciada por: {Md(RefsLabel(p))}" +
                              (r.inConfigLibrary ? " · en `MidiGenPlayConfig.progressionLibrary`" : ""));
                if (r.flags.Count > 0) sb.AppendLine($"- Flags: `{Md(string.Join(" | ", r.flags))}`");
                sb.AppendLine();
                if (r.events.Count == 0) { sb.AppendLine("_(sin eventos)_"); sb.AppendLine(); continue; }
                sb.AppendLine("| # | Posición | Dur. (c) | Grado | Alt. | Calidad | Diat. | SECDOM | Vel | Token |");
                sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|");
                foreach (var e in r.events)
                    sb.AppendLine($"| {e.index} | {e.position} | {Num(e.lengthMeasures)} | {e.degree} | " +
                                  $"{(e.degreeAccidental == 0 ? "" : e.degreeAccidental.ToString("+0;-0"))} | " +
                                  $"{e.quality} | {(e.isDiatonic ? "sí" : "no")} | " +
                                  $"{(e.hasAppliedTarget ? "→ " + e.appliedTarget : "")} | {e.velocity} | " +
                                  $"`{Md(e.token)}` |");
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
#endif