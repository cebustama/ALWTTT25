// Place at: Assets/Scripts/UI/Tooltips/ConceptTooltipResolver.cs
// FULL REPLACEMENT — TXT-3 (2026-09-10). Overwrites the TXT-2 file.
//
// [TXT-3 / D-TXT3-4=A] Resolves a concept id (the id in <link=id>, a StatusEffectSO.StatusKey,
// or a SpecialKeywords enum name) to tooltip header + body from ONE registry: the active
// ConceptGlossarySO. Statuses and card keywords no longer carry player text (D-TXT3-0=B, an
// authority MOVE, not a copy): StatusEffectSO.displayName is a developer label (file name, logs,
// editor) and StatusEffectSO.description / SpecialKeywordBase.contentText no longer exist.
//
// Glossary source, in order: the instance's own glossary (tutorial surfaces receive it by
// inspector next to their catalog), else TooltipManager.Instance.ConceptGlossary (D-TXT3-5=A —
// the same seat SpecialKeywordData occupied). Consumers with no natural glossary slot
// (StatusIconBase, CardBase) use the static entry points, which always read the manager.
//
// Miss policy: a missing entry is a visible bug, not a silent blank. The static entry points
// return the raw id as header, an empty body, log ONCE per id per session, and return false.
// They never fall back to StatusEffectSO.displayName — that would quietly make it player text
// again, which is exactly what TXT-3 removes.
//
// Language: the glossary is per language and selected by inspector assignment (D-S5f-2=B);
// runtime never reads languageCode. TooltipManager and TutorialController must be assigned
// glossaries of the same language — nothing enforces it (recorded as a TXT-3 risk).
using System;
using System.Collections.Generic;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Status;
using UnityEngine;

namespace ALWTTT.Tooltips
{
    /// <summary>Since TXT-3 only <see cref="None"/> and <see cref="Glossary"/> are ever
    /// returned. <see cref="Status"/> and <see cref="Keyword"/> are kept so switches and
    /// serialized values in TXT-2 consumers keep compiling; treat them as historical.</summary>
    public enum ConceptSource { None = 0, Status = 1, Keyword = 2, Glossary = 3 }

    public sealed class ConceptTooltipResolver
    {
        private readonly ConceptGlossarySO _glossary;

        // One warning per id per session, shared by every resolver instance and the static path.
        private static readonly HashSet<string> s_warned = new(StringComparer.OrdinalIgnoreCase);

        public ConceptTooltipResolver(ConceptGlossarySO glossary)
        {
            _glossary = glossary;
        }

        /// <summary>TXT-2 signature kept so TutorialOverlayView.SetConceptSources compiles
        /// unchanged. The catalogues are ignored: statuses resolve from the glossary by
        /// StatusKey. Drop this overload when TutorialController stops passing them (TXT-3b).</summary>
        [Obsolete("TXT-3: status catalogues are no longer a text source. Use ConceptTooltipResolver(ConceptGlossarySO).")]
        public ConceptTooltipResolver(ConceptGlossarySO glossary, IReadOnlyList<StatusEffectCatalogueSO> catalogues)
            : this(glossary) { }

        private ConceptGlossarySO Glossary => _glossary != null ? _glossary : ActiveGlossary;

        /// <summary>The glossary of the running build's language, as assigned on TooltipManager.
        /// Null before the manager exists or when nothing is assigned.</summary>
        public static ConceptGlossarySO ActiveGlossary =>
            TooltipManager.Instance != null ? TooltipManager.Instance.ConceptGlossary : null;

        public bool IsKnown(string id) => Resolve(id, out _, out _) != ConceptSource.None;

        /// <summary>Tag path (&lt;link=id&gt;). Returns None on a miss; the caller decides what
        /// to draw (tutorial surfaces leave the word plain and warn once, per SSoT_Game_Text §6).</summary>
        public ConceptSource Resolve(string id, out string header, out string body)
        {
            header = null; body = null;
            if (string.IsNullOrWhiteSpace(id)) return ConceptSource.None;

            var g = Glossary;
            if (g != null && g.TryGet(id.Trim(), out var e))
            {
                header = e.DisplayName; body = e.Description;
                return ConceptSource.Glossary;
            }
            return ConceptSource.None;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Static entry points — non-tag consumers (status icons, card hover).
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Player-facing name + description of a status, by StatusKey, from the active
        /// glossary. On a miss: header = StatusKey, body = "", one warning, returns false.</summary>
        public static bool TryGetStatusText(StatusEffectSO status, out string header, out string body)
        {
            if (status == null) { header = string.Empty; body = string.Empty; return false; }
            return TryGetFromActiveGlossary(status.StatusKey, "status", out header, out body);
        }

        /// <summary>Player-facing name + description of a card keyword, keyed by the enum name
        /// (case-insensitive; glossary ids are lowercase). Same miss policy as statuses.</summary>
        public static bool TryGetKeywordText(SpecialKeywords keyword, out string header, out string body)
        {
            return TryGetFromActiveGlossary(keyword.ToString().ToLowerInvariant(), "keyword", out header, out body);
        }

        private static bool TryGetFromActiveGlossary(string id, string kind, out string header, out string body)
        {
            id = (id ?? string.Empty).Trim();
            var g = ActiveGlossary;
            if (g != null && id.Length > 0 && g.TryGet(id, out var e))
            {
                header = e.DisplayName; body = e.Description;
                return true;
            }

            header = id; body = string.Empty;
            if (s_warned.Add(kind + ":" + id))
            {
                Debug.LogWarning(g == null
                    ? $"[ConceptTooltipResolver] No ConceptGlossarySO assigned on TooltipManager; {kind} '{id}' shows its raw id."
                    : $"[ConceptTooltipResolver] Glossary '{g.name}' has no entry for {kind} '{id}'; showing the raw id. Add it in GameTextWindow ▸ Concepts (coverage badge).");
            }
            return false;
        }
    }
}