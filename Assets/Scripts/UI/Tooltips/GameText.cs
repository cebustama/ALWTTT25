// Place at: Assets/Scripts/UI/Tooltips/GameText.cs
// [TXT-4 / D-TXT4-3=b′ · D-TXT4-4] Second ENTRY POINT to the single text registry — NOT a second
// registry. ConceptTooltipResolver answers hovers (header + body); this class answers everything
// that is not a hover: a card's name, a concept's name inside a composed line, and the templates
// CardEffectDescriptionBuilder / the card detail / the audience canvas compose from. Both read the
// same asset from the same seat, TooltipManager.ConceptGlossary (D-TXT3-5=A), at call time — the
// manager may not exist when a static producer is first called.
//
// Miss policy (SSoT_Game_Text §3, unchanged): visible, never silent, never a fallback to a
// developer label. A card with no name entry shows its CardDefinition.Id; a template with no entry
// shows "id(arg, arg)"; a concept with no entry shows its id. Each miss logs once per id per
// session. Nothing here ever reads CardDefinition.displayName or StatusEffectSO.displayName.
//
// Editor fallback: editor tooling (CardTextDumpMenu, inventory exports) runs without a
// TooltipManager, so it may hand this class a glossary explicitly. That path is #if UNITY_EDITOR and
// is the only place a glossary is chosen by anything other than the manager's inspector field.
using System.Collections.Generic;
using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Status;
using UnityEngine;

namespace ALWTTT.Tooltips
{
    public static class GameText
    {
        private const string DebugTag = "<color=#C8A2C8>[GameText]</color>";

        private static readonly HashSet<string> _warned = new(System.StringComparer.OrdinalIgnoreCase);
        private static ConceptGlossarySO _editorFallback;

        /// <summary>The active-language glossary: the TooltipManager's field, else the editor fallback, else null.</summary>
        public static ConceptGlossarySO Glossary
        {
            get
            {
                var tm = TooltipManager.Instance;
                if (tm != null && tm.ConceptGlossary != null) return tm.ConceptGlossary;
                return _editorFallback;
            }
        }

#if UNITY_EDITOR
        /// <summary>Editor tooling only: resolve against this glossary when no TooltipManager exists.
        /// Pass null to clear. Never called from runtime code.</summary>
        public static void SetEditorFallback(ConceptGlossarySO glossary) => _editorFallback = glossary;
#endif

        // ── Card names ───────────────────────────────────────────────────────
        /// <summary>Player-facing card name. Miss ⇒ CardDefinition.Id, one warning per id.</summary>
        public static string CardName(CardDefinition def)
        {
            if (def == null) return string.Empty;
            var g = Glossary;
            if (g != null && g.TryGetCardName(def.Id, out var name) && !string.IsNullOrWhiteSpace(name))
                return name;
            WarnOnce("card name", def.Id, g);
            return def.Id ?? string.Empty;
        }

        // ── Concept names (for composed lines, not hovers) ───────────────────
        /// <summary>Concept header by id (StatusKey, keyword name or glossary-native id). Miss ⇒ raw id.</summary>
        public static string ConceptName(string conceptId)
        {
            if (string.IsNullOrEmpty(conceptId)) return string.Empty;
            var g = Glossary;
            if (g != null && g.TryGet(conceptId, out var e) && !string.IsNullOrWhiteSpace(e.DisplayName))
                return e.DisplayName;
            WarnOnce("concept", conceptId, g);
            return conceptId;
        }

        /// <summary>Concept body by id. Miss ⇒ empty string (the header already carries the raw id).</summary>
        public static string ConceptDescription(string conceptId)
        {
            if (string.IsNullOrEmpty(conceptId)) return string.Empty;
            var g = Glossary;
            if (g != null && g.TryGet(conceptId, out var e)) return e.Description ?? string.Empty;
            WarnOnce("concept", conceptId, g);
            return string.Empty;
        }

        /// <summary>Status name by StatusKey — the glossary's, never StatusEffectSO.displayName.</summary>
        public static string StatusName(StatusEffectSO status)
            => status == null ? string.Empty : ConceptName(status.StatusKey);

        // ── Templates ────────────────────────────────────────────────────────
        /// <summary>Format a template with positional arguments. Miss ⇒ "id(arg, …)", one warning per id.</summary>
        public static string Format(string templateId, params object[] args)
        {
            var g = Glossary;
            if (g != null && g.TryGetTemplate(templateId, out var text) && !string.IsNullOrEmpty(text))
            {
                if (args == null || args.Length == 0) return text;
                try { return string.Format(text, args); }
                catch (System.FormatException)
                {
                    WarnOnce("template format", templateId, g, " — placeholder/argument mismatch");
                    return MissText(templateId, args);
                }
            }
            WarnOnce("template", templateId, g);
            return MissText(templateId, args);
        }

        // ── Numbers ──────────────────────────────────────────────────────────
        /// <summary>[TEXT-5 / D-T5-2=B] Format a number for composition into player text, using the active
        /// glossary's NumberFormat (its decimal separator is part of the language). No glossary ⇒ invariant.
        /// Replaces bare ToString(format), which formatted by the MACHINE's CurrentCulture — the same tooltip
        /// printed «1,00» or «1.00» depending on the player's OS, not on the game's language (F-TXT4-10).</summary>
        public static string Number(double value, string format)
        {
            var g = Glossary;
            return value.ToString(format, g != null ? g.NumberFormat : System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string MissText(string id, object[] args)
            => args == null || args.Length == 0 ? id : id + "(" + string.Join(", ", args) + ")";

        private static void WarnOnce(string kind, string id, ConceptGlossarySO g, string suffix = "")
        {
            string key = kind + ":" + id;
            if (!_warned.Add(key)) return;
            Debug.LogWarning(g == null
                ? $"{DebugTag} No glossary assigned (TooltipManager.conceptGlossary) — {kind} '{id}' shows its raw id."
                : $"{DebugTag} {kind} '{id}' has no entry in glossary '{g.name}'{suffix} — showing its raw id.");
        }
    }
}