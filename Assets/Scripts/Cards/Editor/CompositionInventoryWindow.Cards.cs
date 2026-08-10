#if UNITY_EDITOR && ALWTTT_DEV
// [CSV-4c / D-CSV-16=A] Card -> style-bundle REVERSE index.
//
// The rest of the window indexes references DOWNWARD (bundle -> palette ->
// pattern). It could therefore say whether a pattern is used, but never whether
// a BUNDLE is used, because nothing walked the card layer above it. That gap is
// why CSV-4 had to establish the reachable-progression figure from recollection
// instead of from tooling (SSoT_Editor_Authoring_Tools 17.10).
//
// This file closes the gap and nothing else. It is strictly read-only: it
// loads assets through AssetDatabase, never writes, never SetDirty, never
// saves (ST-CSV-7 must keep passing).
using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Musicians;
using MidiGenPlay;
using MidiGenPlay.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.DevMode.Editor
{
    public sealed partial class CompositionInventoryWindow
    {
        // ══════════════════════════════════════════════════════════════════
        // Model
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// One (card -> bundle) edge, plus every discovery source that puts the
        /// card in front of a player.
        ///
        /// ARITY (measured 2026-08-09, not assumed): a CardDefinition reaches at
        /// most ONE bundle, because the only card-side reference in the codebase
        /// is <c>CompositionCardPayload.TrackAction.styleBundle</c>. The
        /// multiplicity is therefore on the other side - many cards may point at
        /// the same bundle - and <see cref="_bundleToCards"/> is the multiset.
        /// The link list is kept edge-shaped anyway so a second card-side bundle
        /// slot would not require reshaping the index.
        /// </summary>
        private sealed class CardBundleLink
        {
            public CardDefinition card;
            public TrackStyleBundleSO bundle;
            public TrackRole cardRole;
            public readonly List<string> sources = new();
            public readonly HashSet<MusicianCharacterType> catalogOwners = new();
            public bool anyWiredSource;

            public bool AnySource => sources.Count > 0;
        }

        // card -> bundle edges, one per composition card that carries a bundle.
        private readonly List<CardBundleLink> _cardLinks = new();
        // bundle -> the cards that reach it. MULTISET: a bundle shared by two
        // cards appears with two entries, which is the whole point of the view.
        private readonly Dictionary<TrackStyleBundleSO, List<CardBundleLink>> _bundleToCards = new();
        // What was scanned, and what was found unwired. Rendered verbatim in the
        // UI and exported: the batch constraint is "do not invent reachability -
        // if a source is not scanned, say so in the UI".
        private readonly List<string> _cardSourceNotes = new();

        private int _cardDefsScanned;
        private int _compositionCardsWithBundle;

        // Cards view sub-filters (deliberately local to the view; the shared
        // filter bar stays untouched).
        [SerializeField] private int _cardRoleFilterIndex;   // 0 = All
        [SerializeField] private bool _cardOnlyFlagged;
        [SerializeField] private bool _cardShowPayload = true;

        private static readonly string[] CardRoleOptions =
            BuildCardRoleOptions();

        private static string[] BuildCardRoleOptions()
        {
            var vals = (TrackRole[])Enum.GetValues(typeof(TrackRole));
            var opts = new string[vals.Length + 1];
            opts[0] = "Role: All";
            for (int i = 0; i < vals.Length; i++) opts[i + 1] = "Role: " + vals[i];
            return opts;
        }

        private TrackRole? CardRoleFilter()
        {
            if (_cardRoleFilterIndex <= 0) return null;
            var vals = (TrackRole[])Enum.GetValues(typeof(TrackRole));
            return vals[_cardRoleFilterIndex - 1];
        }

        // ══════════════════════════════════════════════════════════════════
        // Index construction
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Builds the reverse index. Called from RefreshCatalog AFTER the bundle
        /// list exists, so a bundle discovered by AssetDatabase but referenced by
        /// nothing still gets a row and an UNREACHABLE flag.
        ///
        /// Discovery is the UNION of three card sources, mirroring the pattern
        /// union of D-CSV-12=A+B one layer up:
        ///   - MusicianCardCatalogData  (per-musician identity cards)
        ///   - GenericCardCatalogSO     ("Owner: Any" starter cards)
        ///   - BandDeckData             (hand-authored decks; PersistentGameplayData
        ///                               materialises these directly, so a card can
        ///                               be live without appearing in any catalogue)
        /// plus a full AssetDatabase sweep of CardDefinition so that a bundle
        /// referenced only by an un-sourced card is reported as UNSOURCED rather
        /// than silently counted as reachable.
        /// </summary>
        private void BuildCardBundleIndex()
        {
            _cardLinks.Clear();
            _bundleToCards.Clear();
            _cardSourceNotes.Clear();
            _compositionCardsWithBundle = 0;

            var allCards = FindAllAssets<CardDefinition>();
            _cardDefsScanned = allCards.Count;

            var musCats = FindAllAssets<MusicianCardCatalogData>();
            var genCats = FindAllAssets<GenericCardCatalogSO>();
            var decks = FindAllAssets<BandDeckData>();
            var musicians = FindAllAssets<MusicianCharacterData>();
            var rosters = FindAllAssets<GigSetupRosterSO>();

            // Second hop: is the SOURCE itself wired to anything the game loads?
            // A catalogue attached to no musician, or a deck listed on no roster,
            // is dead one level up - and every card in it is dead with it.
            var wiredMusCats = new HashSet<MusicianCardCatalogData>(
                musicians.Select(m => m.CardCatalog).Where(c => c != null));
            var wiredGenCats = new HashSet<GenericCardCatalogSO>(
                rosters.Select(r => r.GenericStarterCatalog).Where(c => c != null));
            var wiredDecks = new HashSet<BandDeckData>(
                rosters.SelectMany(r => r.AvailableBandDecks ?? (IReadOnlyList<BandDeckData>)Array.Empty<BandDeckData>())
                       .Where(d => d != null));

            // Edge table, keyed by card: arity is 0..1 bundle per card today.
            var byCard = new Dictionary<CardDefinition, CardBundleLink>();

            CardBundleLink EnsureLink(CardDefinition c)
            {
                if (c == null) return null;
                var bundle = c.CompositionPayload?.TrackAction?.styleBundle;
                if (bundle == null) return null;
                if (!byCard.TryGetValue(c, out var link))
                {
                    link = new CardBundleLink
                    {
                        card = c,
                        bundle = bundle,
                        cardRole = c.CompositionPayload.TrackAction.role
                    };
                    byCard[c] = link;
                }
                return link;
            }

            // Pass 1 - every CardDefinition in the project, sourced or not.
            foreach (var c in allCards)
                if (EnsureLink(c) != null) _compositionCardsWithBundle++;

            // Pass 2 - attribute sources.
            void Attribute(CardDefinition c, string source, bool wired,
                           MusicianCharacterType? owner)
            {
                var link = EnsureLink(c);
                if (link == null) return;
                if (!link.sources.Contains(source)) link.sources.Add(source);
                if (wired) link.anyWiredSource = true;
                if (owner.HasValue) link.catalogOwners.Add(owner.Value);
            }

            foreach (var cat in musCats)
            {
                bool wired = wiredMusCats.Contains(cat);
                foreach (var e in cat.Entries ?? new List<MusicianCardEntry>())
                    Attribute(e?.card, $"musCat:{cat.name}{(wired ? "" : " (unwired)")}",
                              wired, cat.MusicianType);
            }

            foreach (var cat in genCats)
            {
                bool wired = wiredGenCats.Contains(cat);
                foreach (var e in cat.Entries ?? new List<MusicianCardEntry>())
                    Attribute(e?.card, $"genCat:{cat.name}{(wired ? "" : " (unwired)")}",
                              wired, null);
            }

            foreach (var d in decks)
            {
                bool wired = wiredDecks.Contains(d);
                foreach (var c in d.EnumerateCards() ?? Enumerable.Empty<CardDefinition>())
                    Attribute(c, $"deck:{d.name}{(wired ? "" : " (unwired)")}", wired, null);
            }

            _cardLinks.AddRange(byCard.Values
                .OrderBy(l => l.cardRole.ToString())
                .ThenBy(l => l.bundle != null ? l.bundle.name : string.Empty)
                .ThenBy(l => l.card.Id ?? l.card.name));

            foreach (var l in _cardLinks)
            {
                if (!_bundleToCards.TryGetValue(l.bundle, out var list))
                    _bundleToCards[l.bundle] = list = new List<CardBundleLink>();
                list.Add(l);
            }

            _cardSourceNotes.Add($"CardDefinition assets scanned: {_cardDefsScanned}");
            _cardSourceNotes.Add(
                $"Composition cards carrying a style bundle: {_compositionCardsWithBundle}");
            _cardSourceNotes.Add(
                $"Musician catalogues: {musCats.Count} ({wiredMusCats.Count} wired to a MusicianCharacterData)");
            _cardSourceNotes.Add(
                $"Generic catalogues: {genCats.Count} ({wiredGenCats.Count} wired to a GigSetupRosterSO)");
            _cardSourceNotes.Add(
                $"Band decks: {decks.Count} ({wiredDecks.Count} listed on a GigSetupRosterSO)");
            _cardSourceNotes.Add(
                "NOT scanned as card sources: reward pools, unlock tables, runtime-generated cards. " +
                "A bundle reachable only through one of those reads as UNSOURCED here.");
        }

        // ══════════════════════════════════════════════════════════════════
        // Reachability flags (see SSoT_Editor_Authoring_Tools 17.6)
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// UNREACHABLE  - no CardDefinition anywhere points at this bundle.
        /// UNSOURCED    - some card does, but no discovered catalogue or deck
        ///                contains any of those cards.
        /// UNWIRED-SRC  - every containing source is itself attached to nothing
        ///                (catalogue on no musician, deck on no roster).
        ///
        /// LIMIT, deliberately not papered over: this is a THREE-HOP walk
        /// (bundle -> card -> source -> owner). Nothing deeper is checked. A deck
        /// listed on a roster that no scene loads, or a musician that no roster
        /// offers, still reads as reachable. Same limitation shape as ORPHAN
        /// (17.6): the flag is a floor, not a census.
        /// </summary>
        private List<string> BundleCardFlags(TrackStyleBundleSO b)
        {
            var flags = new List<string>();
            if (b == null) return flags;
            if (!_bundleToCards.TryGetValue(b, out var links) || links.Count == 0)
            {
                flags.Add("UNREACHABLE");
                return flags;
            }
            if (links.All(l => !l.AnySource)) flags.Add("UNSOURCED");
            else if (links.All(l => !l.anyWiredSource)) flags.Add("UNWIRED-SRC");
            return flags;
        }

        private string BundleCardsLabel(TrackStyleBundleSO b)
        {
            if (b == null) return "—";
            if (!_bundleToCards.TryGetValue(b, out var links) || links.Count == 0) return "—";
            return $"{links.Count}× " + string.Join(", ",
                links.Select(l => l.card.Id ?? l.card.name));
        }

        private static string LinkOwnerLabel(CardBundleLink l)
        {
            if (l.catalogOwners.Count > 0)
                return string.Join("/", l.catalogOwners.Select(o => o.ToString()));
            if (l.card.CanBePlayedByAnyMusician) return "(any)";
            return l.card.FixedPerformerType.ToString();
        }

        // ══════════════════════════════════════════════════════════════════
        // View
        // ══════════════════════════════════════════════════════════════════

        private void DrawCardsToBundles()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _cardRoleFilterIndex = EditorGUILayout.Popup(
                    _cardRoleFilterIndex, CardRoleOptions, GUILayout.Width(130));
                _cardOnlyFlagged = GUILayout.Toggle(
                    _cardOnlyFlagged, "Only flagged", GUILayout.Width(90));
                _cardShowPayload = GUILayout.Toggle(
                    _cardShowPayload, "Payload columns", GUILayout.Width(120));
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.HelpBox(string.Join("\n", _cardSourceNotes), MessageType.None);

            int unreachable = _bundles.Count(b => b != null && BundleCardFlags(b)
                .Contains("UNREACHABLE"));
            EditorGUILayout.LabelField(
                $"Card → bundle edges: {_cardLinks.Count}  |  bundles reached: " +
                $"{_bundleToCards.Count}/{_bundles.Count}  |  UNREACHABLE: {unreachable}",
                EditorStyles.boldLabel);

            var roleFilter = CardRoleFilter();

            foreach (var l in _cardLinks)
            {
                if (roleFilter.HasValue && l.cardRole != roleFilter.Value) continue;
                var flags = BundleCardFlags(l.bundle);
                if (_cardOnlyFlagged && flags.Count == 0) continue;
                if (!string.IsNullOrEmpty(_textFilter))
                {
                    string hay = ((l.card.Id ?? "") + " " + (l.card.DisplayName ?? "") + " " +
                                  l.bundle.name).ToLowerInvariant();
                    if (!hay.Contains(_textFilter.ToLowerInvariant())) continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"[{SourceTag(l.card)}]", GUILayout.Width(40));
                        GUILayout.Label(l.card.Id ?? l.card.name, GUILayout.Width(200));
                        GUILayout.Label(LinkOwnerLabel(l), GUILayout.Width(90));
                        GUILayout.Label($"role={l.cardRole}", GUILayout.Width(100));
                        GUILayout.Label("→", GUILayout.Width(16));
                        GUILayout.Label(l.bundle.name, GUILayout.Width(220));
                        GUILayout.Label(l.bundle.GetType().Name, GUILayout.Width(150));
                        DrawFlags(flags);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Card", GUILayout.Width(46)))
                            EditorGUIUtility.PingObject(l.card);
                        if (GUILayout.Button("Bundle", GUILayout.Width(56)))
                            EditorGUIUtility.PingObject(l.bundle);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(44);
                        GUILayout.Label(
                            $"chain: {BundleChainLabel(l.bundle)}", GUILayout.MinWidth(260));
                        GUILayout.Label(
                            $"src: {(l.AnySource ? string.Join(", ", l.sources) : "— (no catalogue/deck)")}",
                            GUILayout.MinWidth(260));
                        GUILayout.FlexibleSpace();
                    }

                    if (_cardShowPayload && l.bundle is BasslineCardConfigSO bass)
                        DrawBasslinePayloadLines(bass);
                }
            }

            DrawUnreachedBundles(roleFilter);
        }

        /// <summary>
        /// [CSV-4c rider] Bundles that NO card reaches, listed with the same
        /// payload surface as the card rows.
        ///
        /// Added after the first real export: 10 of the 14 Bassline bundles came
        /// back UNREACHABLE, and their payload was invisible everywhere in the
        /// window - Style Bundles shows "(no pattern refs)" for the family, and
        /// this view skipped them for lack of a card. Deciding which dead bundle
        /// to revive and which to delete needs exactly those fields, so the view
        /// would have sent you to the Inspector one asset at a time. With the role
        /// filter set, the two sections together are the complete family.
        /// </summary>
        private void DrawUnreachedBundles(TrackRole? roleFilter)
        {
            var dead = _bundles
                .Where(b => b != null && BundleCardFlags(b).Contains("UNREACHABLE"))
                .Where(b => !roleFilter.HasValue || b.appliesTo == roleFilter.Value)
                .Where(b => string.IsNullOrEmpty(_textFilter)
                            || b.name.ToLowerInvariant().Contains(_textFilter.ToLowerInvariant()))
                .OrderBy(b => b.appliesTo.ToString())
                .ThenBy(b => b.name)
                .ToList();

            if (dead.Count == 0) return;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(
                $"Unreached bundles (no card points at them): {dead.Count}",
                EditorStyles.boldLabel);

            foreach (var b in dead)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"[{SourceTag(b)}]", GUILayout.Width(40));
                        GUILayout.Label("(no card)", GUILayout.Width(200));
                        GUILayout.Label("—", GUILayout.Width(90));
                        GUILayout.Label($"role={b.appliesTo}", GUILayout.Width(100));
                        GUILayout.Label("→", GUILayout.Width(16));
                        GUILayout.Label(b.name, GUILayout.Width(220));
                        GUILayout.Label(b.GetType().Name, GUILayout.Width(150));
                        DrawFlags(BundleCardFlags(b));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Bundle", GUILayout.Width(56)))
                            EditorGUIUtility.PingObject(b);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(44);
                        GUILayout.Label($"chain: {BundleChainLabel(b)}", GUILayout.MinWidth(260));
                        GUILayout.FlexibleSpace();
                    }

                    if (_cardShowPayload && b is BasslineCardConfigSO bass)
                        DrawBasslinePayloadLines(bass);
                }
            }
        }

        private void DrawBasslinePayloadLines(BasslineCardConfigSO bass)
        {
            void Line(string text)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(44);
                    GUILayout.Label(text, EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }
            }

            Line(BasslinePayloadLabel(bass));
            string sp = BasslineSelfPocketLabel(bass);
            if (sp != null) Line(sp);
        }

        /// <summary>
        /// card -> bundle -> palette -> pattern, as far as the bundle family
        /// actually goes. NOTE for Bassline: BasslineCardConfigSO holds no
        /// pattern or palette reference at all - its musical identity is entirely
        /// PAYLOAD-BORN (expression, pocket, walk), which is the same fact behind
        /// D9 (a Fingered bass sitting under a Slap one is a payload difference,
        /// not an instrument difference). So the chain legitimately terminates at
        /// the bundle for bass, and that is reported, not hidden.
        /// </summary>
        private static string BundleChainLabel(TrackStyleBundleSO b)
        {
            string ov = BundleOverrideName(b);
            string pal = BundlePaletteName(b);
            if (ov == null && pal == null) return "(payload-born — no pattern/palette ref)";
            return $"palette={pal ?? "—"} · pattern={ov ?? "—"}";
        }

        private static string BasslinePayloadLabel(BasslineCardConfigSO b)
            => $"expr={b.chordExpression} rate={b.arpeggioRate} tone={b.arpeggioToneMode} " +
               $"pocket={b.pocketMode} slap{b.pocketSlapBoost:+0;-0;+0} pop{b.pocketPopBoost:+0;-0;+0} " +
               $"customLanes={(b.pocketCustomLanes ? "on" : "off")} " +
               $"jitter={b.velocityJitter} reroll={b.randomRerollChance:0.##} " +
               $"weights={b.randomFigureWeights?.Count ?? 0}";

        /// <summary>
        /// One glyph per SelfPocket step, so a cycled articulation pattern reads
        /// as a rhythm instead of as a comma-separated enum list. Deliberately
        /// case-distinguished: capitals are attacked hits, lowercase are the
        /// quiet or legato classes, and the interpunct is silence.
        /// </summary>
        private static char SelfPocketGlyph(BasslineCardConfigSO.SelfPocketStep s) => s switch
        {
            BasslineCardConfigSO.SelfPocketStep.Slap => 'S',
            BasslineCardConfigSO.SelfPocketStep.Pop => 'P',
            BasslineCardConfigSO.SelfPocketStep.Rest => '\u00B7',
            BasslineCardConfigSO.SelfPocketStep.Ghost => 'g',
            BasslineCardConfigSO.SelfPocketStep.GhostPop => 'p',
            BasslineCardConfigSO.SelfPocketStep.HammerOn => 'h',
            BasslineCardConfigSO.SelfPocketStep.PullOff => 'o',
            _ => '?'
        };

        private static string SelfPocketPatternGlyphs(List<BasslineCardConfigSO.SelfPocketStep> steps)
            => steps == null || steps.Count == 0
                ? "(empty)"
                : new string(steps.Select(SelfPocketGlyph).ToArray());

        private static string SelfPocketSubstitutionsLabel(BasslineCardConfigSO b)
        {
            var subs = b.selfPocketBarSubstitutions;
            if (subs == null || subs.Count == 0) return null;
            return string.Join(" ", subs
                .Where(x => x != null)
                .Select(x =>
                {
                    int n = x.variants?.Count ?? 0;
                    string first = n > 0 && x.variants[0] != null
                        ? SelfPocketPatternGlyphs(x.variants[0].steps)
                        : "(none)";
                    return n > 1 ? $"bar{x.barIndex}=[{first} +{n - 1}]" : $"bar{x.barIndex}=[{first}]";
                }));
        }

        /// <summary>
        /// [CSV-4c rider 2] The SelfPocket surface, which the first version of
        /// this view reduced to the four characters "pocket=SelfPocket".
        ///
        /// SelfPocket is not a mode flag, it is a sub-system: its own candidate
        /// grid, a cycled 7-symbol articulation pattern, an optional phrase with
        /// per-bar substitution variants, and the velocity/gate factors for the
        /// ghost and legato classes. The one card in the project that uses it
        /// (starter_slap_bass) is therefore the card the view described least.
        ///
        /// Returns null for Off / SlapPocket, whose musical content genuinely is
        /// on the base line - the extra line only appears where there is extra
        /// content, so the view does not grow noise for the other three bundles.
        /// </summary>
        private static string BasslineSelfPocketLabel(BasslineCardConfigSO b)
        {
            if (b.pocketMode != BasslineCardConfigSO.PocketCouplingMode.SelfPocket)
                return null;

            var pattern = b.selfPocketPattern;
            string glyphs = SelfPocketPatternGlyphs(pattern);
            string subs = SelfPocketSubstitutionsLabel(b);

            // D-PH-BYTE=A: the substitution table is the single ON/OFF gate for
            // the phrase machinery. An empty table means the phrase fields are
            // inert, so reporting a phrase length there would be misleading.
            string phrase = subs == null
                ? "phrase=OFF"
                : $"phrase={b.selfPocketPhraseLengthBars}bar/{b.selfPocketVariantSelection} {subs}";

            string line = $"    selfPocket: grid={b.selfPocketSubdivision} " +
                          $"pattern=[{glyphs}]({pattern?.Count ?? 0}) {phrase}";

            // The ghost and legato tuning knobs only mean anything if the
            // alphabet in use actually contains those steps. Showing them
            // unconditionally would imply the card is shaped by values it never
            // reads.
            var used = new HashSet<BasslineCardConfigSO.SelfPocketStep>();
            void Collect(List<BasslineCardConfigSO.SelfPocketStep> steps)
            {
                if (steps == null) return;
                foreach (var st in steps) used.Add(st);
            }
            Collect(pattern);
            if (b.selfPocketBarSubstitutions != null)
                foreach (var sub in b.selfPocketBarSubstitutions)
                    if (sub?.variants != null)
                        foreach (var v in sub.variants) Collect(v?.steps);

            bool hasGhost = used.Contains(BasslineCardConfigSO.SelfPocketStep.Ghost)
                         || used.Contains(BasslineCardConfigSO.SelfPocketStep.GhostPop);
            bool hasLegato = used.Contains(BasslineCardConfigSO.SelfPocketStep.HammerOn)
                          || used.Contains(BasslineCardConfigSO.SelfPocketStep.PullOff);

            if (hasGhost)
                line += $" | ghost vel={b.ghostVelocityFactor:0.##}/{b.ghostPopVelocityFactor:0.##} " +
                        $"gate={b.ghostGateBeats:0.##}b";
            if (hasLegato)
                line += $" | legato deg={b.hammerOffsetDegrees:+0;-0;+0}/{b.pullOffsetDegrees:+0;-0;+0} " +
                        $"vel={b.hammerOnVelocityFactor:0.##}/{b.pullOffVelocityFactor:0.##}";

            return line;
        }

        // ══════════════════════════════════════════════════════════════════
        // Print (Console) — called from PrintCurrentView (edit E7)
        // ══════════════════════════════════════════════════════════════════

        private void AppendCardBundles(StringBuilder sb)
        {
            foreach (var note in _cardSourceNotes) sb.AppendLine($"  # {note}");
            sb.AppendLine();
            foreach (var l in _cardLinks)
            {
                var flags = BundleCardFlags(l.bundle);
                sb.AppendLine(
                    $"  [{SourceTag(l.card)}] {l.card.Id ?? l.card.name} " +
                    $"({LinkOwnerLabel(l)}, role={l.cardRole}) -> {l.bundle.name} " +
                    $"[{l.bundle.GetType().Name}] | {BundleChainLabel(l.bundle)} | " +
                    $"src={(l.AnySource ? string.Join(";", l.sources) : "—")}" +
                    (flags.Count > 0 ? $" | flags={string.Join(";", flags)}" : string.Empty) +
                    (l.bundle is BasslineCardConfigSO bass
                        ? $"\n        {BasslinePayloadLabel(bass)}" +
                          (BasslineSelfPocketLabel(bass) is string sp ? $"\n    {sp}" : string.Empty)
                        : string.Empty));
            }

            sb.AppendLine();
            sb.AppendLine("  -- UNREACHABLE bundles (no CardDefinition points at them) --");
            int n = 0;
            foreach (var b in _bundles)
            {
                if (b == null || !BundleCardFlags(b).Contains("UNREACHABLE")) continue;
                n++;
                sb.AppendLine($"    [{SourceTag(b)}] {b.GetType().Name} | {b.name} | " +
                              $"{AssetDatabase.GetAssetPath(b)}" +
                              (b is BasslineCardConfigSO deadBass
                                  ? $"\n        {BasslinePayloadLabel(deadBass)}" +
                                    (BasslineSelfPocketLabel(deadBass) is string dsp
                                        ? $"\n    {dsp}" : string.Empty)
                                  : string.Empty));
            }
            if (n == 0) sb.AppendLine("    (none)");
        }

        // ══════════════════════════════════════════════════════════════════
        // Export (same shape contract as every other view)
        // ══════════════════════════════════════════════════════════════════

        [Serializable]
        private class JsonCardLink
        {
            public string cardId;
            public string cardName;
            public string owner;
            public string cardRole;
            public string cardSource;
            public string cardPath;
            public string bundleName;
            public string bundleType;
            public string bundleRole;
            public string bundlePath;
            public string paletteRef;
            public string patternRef;
            public string sources;
            public bool sourced;
            public bool wiredSource;
            public string flags;
            // Bassline payload surface (D-CSV4c-3). `isBassline` is false and
            // every field is at its default for the other three families.
            public JsonBasslinePayload bassline = new JsonBasslinePayload();
        }

        /// <summary>
        /// [CSV-4c rider 2] Grouped because the surface grew from 10 fields to
        /// 24 when the real BasslineCardConfigSO landed: SelfPocket carries its
        /// own grid, cycled pattern, phrase table and articulation factors.
        /// Flat fields would have put 24 bass columns on every Rhythm row.
        ///
        /// JsonUtility never omits a field, so a non-Bassline row still emits the
        /// object with empty strings and zeros. Read `isBassline`, not the
        /// emptiness of a string.
        ///
        /// The SelfPocket group is populated ONLY when pocketMode = SelfPocket.
        /// Under Off or SlapPocket those values exist on the asset but nothing
        /// reads them, and exporting them would suggest the card is shaped by
        /// numbers it ignores.
        /// </summary>
        [Serializable]
        private class JsonBasslinePayload
        {
            public bool isBassline;
            public string chordExpression;
            public string arpeggioRate;
            public string arpeggioToneMode;
            public string pocketMode;
            public int pocketSlapBoost;
            public int pocketPopBoost;
            public bool pocketCustomLanes;
            public int pocketSlapLanes;
            public int pocketPopLanes;
            public int velocityJitter;
            public float randomRerollChance;
            public int randomFigureWeights;
            // --- SelfPocket only ---
            public string selfPocketSubdivision;
            public string selfPocketPattern;          // glyphs, e.g. "SP\u00B7g"
            public int selfPocketPatternSteps;
            public bool selfPocketPhraseActive;       // D-PH-BYTE=A: table non-empty
            public int selfPocketPhraseLengthBars;
            public string selfPocketVariantSelection;
            public string selfPocketSubstitutions;    // "bar3=[SP +1]"
            public int hammerOffsetDegrees;
            public int pullOffsetDegrees;
            public float ghostVelocityFactor;
            public float ghostPopVelocityFactor;
            public float hammerOnVelocityFactor;
            public float pullOffVelocityFactor;
            public float ghostGateBeats;
        }

        private static JsonBasslinePayload BuildBasslinePayload(TrackStyleBundleSO bundle)
        {
            var p = new JsonBasslinePayload();
            if (!(bundle is BasslineCardConfigSO b)) return p;

            p.isBassline = true;
            p.chordExpression = b.chordExpression.ToString();
            p.arpeggioRate = b.arpeggioRate.ToString();
            p.arpeggioToneMode = b.arpeggioToneMode.ToString();
            p.pocketMode = b.pocketMode.ToString();
            p.pocketSlapBoost = b.pocketSlapBoost;
            p.pocketPopBoost = b.pocketPopBoost;
            p.pocketCustomLanes = b.pocketCustomLanes;
            p.pocketSlapLanes = b.pocketSlapLanes?.Count ?? 0;
            p.pocketPopLanes = b.pocketPopLanes?.Count ?? 0;
            p.velocityJitter = b.velocityJitter;
            p.randomRerollChance = b.randomRerollChance;
            p.randomFigureWeights = b.randomFigureWeights?.Count ?? 0;

            if (b.pocketMode != BasslineCardConfigSO.PocketCouplingMode.SelfPocket)
                return p;

            p.selfPocketSubdivision = b.selfPocketSubdivision.ToString();
            p.selfPocketPattern = SelfPocketPatternGlyphs(b.selfPocketPattern);
            p.selfPocketPatternSteps = b.selfPocketPattern?.Count ?? 0;
            p.selfPocketSubstitutions = SelfPocketSubstitutionsLabel(b);
            p.selfPocketPhraseActive = p.selfPocketSubstitutions != null;
            p.selfPocketPhraseLengthBars = b.selfPocketPhraseLengthBars;
            p.selfPocketVariantSelection = b.selfPocketVariantSelection.ToString();
            p.hammerOffsetDegrees = b.hammerOffsetDegrees;
            p.pullOffsetDegrees = b.pullOffsetDegrees;
            p.ghostVelocityFactor = b.ghostVelocityFactor;
            p.ghostPopVelocityFactor = b.ghostPopVelocityFactor;
            p.hammerOnVelocityFactor = b.hammerOnVelocityFactor;
            p.pullOffVelocityFactor = b.pullOffVelocityFactor;
            p.ghostGateBeats = b.ghostGateBeats;
            return p;
        }

        [Serializable]
        private class JsonUnreachableBundle
        {
            public string assetName;
            public string type;
            public string appliesTo;
            public string source;
            public string assetPath;
            public string flags;
            // [CSV-4c rider] Same payload surface as the card rows, so a dead
            // bundle can be judged from the export without opening the Inspector.
            public JsonBasslinePayload bassline = new JsonBasslinePayload();
        }

        [Serializable]
        private class WrapCardLinks
        {
            public List<string> sourceNotes = new();
            public List<JsonCardLink> cards = new();
            public List<JsonUnreachableBundle> unreachableBundles = new();
        }

        private string BuildCardLinksJson()
        {
            var w = new WrapCardLinks();
            w.sourceNotes.AddRange(_cardSourceNotes);

            foreach (var l in _cardLinks)
            {
                w.cards.Add(new JsonCardLink
                {
                    cardId = l.card.Id,
                    cardName = l.card.DisplayName,
                    owner = LinkOwnerLabel(l),
                    cardRole = l.cardRole.ToString(),
                    cardSource = SourceTag(l.card),
                    cardPath = AssetDatabase.GetAssetPath(l.card),
                    bundleName = l.bundle.name,
                    bundleType = l.bundle.GetType().Name,
                    bundleRole = l.bundle.appliesTo.ToString(),
                    bundlePath = AssetDatabase.GetAssetPath(l.bundle),
                    paletteRef = BundlePaletteName(l.bundle),
                    patternRef = BundleOverrideName(l.bundle),
                    sources = string.Join(";", l.sources),
                    sourced = l.AnySource,
                    wiredSource = l.anyWiredSource,
                    flags = string.Join(";", BundleCardFlags(l.bundle)),
                    bassline = BuildBasslinePayload(l.bundle)
                });
            }

            foreach (var b in _bundles)
            {
                if (b == null) continue;
                var flags = BundleCardFlags(b);
                if (!flags.Contains("UNREACHABLE")) continue;
                w.unreachableBundles.Add(new JsonUnreachableBundle
                {
                    assetName = b.name,
                    type = b.GetType().Name,
                    appliesTo = b.appliesTo.ToString(),
                    source = SourceTag(b),
                    assetPath = AssetDatabase.GetAssetPath(b),
                    flags = string.Join(";", flags),
                    bassline = BuildBasslinePayload(b)
                });
            }

            return JsonUtility.ToJson(w, true);
        }
    }
}
#endif