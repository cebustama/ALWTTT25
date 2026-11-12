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

        // ── Audience shouts [S8 / GRITOS-1, D1=C] ────────────────────────────
        // Own RNG: picking a line must not consume UnityEngine.Random, which rolls RandomMusician
        // targets (LockMusicianTarget). A cosmetic choice never shifts a gameplay roll.
        private static readonly System.Random _shoutRng = new System.Random();

        /// <summary>[S8] Shout id for one member's ability (most specific first).</summary>
        public static string ShoutIdFor(string characterId, string abilityId)
            => string.IsNullOrEmpty(characterId) || string.IsNullOrEmpty(abilityId)
                ? null : "shout_" + characterId.Trim().ToLowerInvariant() + "_" + abilityId.Trim().ToLowerInvariant();

        /// <summary>[S8] Shout id shared by everyone with this ability (the frogs' chorus).</summary>
        public static string ShoutIdForAbility(string abilityId)
            => string.IsNullOrEmpty(abilityId) ? null : "shout_" + abilityId.Trim().ToLowerInvariant();

        /// <summary>[S8b] Musician key for per-musician variants: the name, lowercased, every run of
        /// characters outside [a-z0-9] turned into '_' ("Robot C2" → "robot_c2").</summary>
        public static string MusicianKey(string musicianName)
        {
            if (string.IsNullOrWhiteSpace(musicianName)) return null;
            var sb = new System.Text.StringBuilder();
            bool sep = false;
            foreach (var ch in musicianName.Trim().ToLowerInvariant())
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9')) { if (sep && sb.Length > 0) sb.Append('_'); sb.Append(ch); sep = false; }
                else sep = true;
            }
            return sb.Length > 0 ? sb.ToString() : null;
        }

        /// <summary>[S8b] Id of the per-musician variant of a shout id: '&lt;shoutId&gt;__&lt;musicianKey&gt;'.</summary>
        public static string ShoutIdForMusician(string shoutId, string musicianName)
        {
            var key = MusicianKey(musicianName);
            return string.IsNullOrEmpty(shoutId) || key == null ? null : shoutId + "__" + key;
        }

        /// <summary>
        /// [S8 / GRITOS-1] A random line for this member's ability, with {musico}/{músico}/{musician}
        /// replaced by <paramref name="musicianName"/>. Lookup, most specific first:
        ///   shout_&lt;characterId&gt;_&lt;abilityId&gt;__&lt;musician&gt; · shout_&lt;abilityId&gt;__&lt;musician&gt;  [S8b]
        ///   shout_&lt;characterId&gt;_&lt;abilityId&gt; · shout_&lt;abilityId&gt;
        /// A per-musician entry REPLACES the general lines for that musician (Robot C2 does not
        /// "go off-key", it loses the tempo). Returns null when there is no line (shouts are optional:
        /// the caller falls back to the ability-name floater; no miss warning). A line that names the
        /// musician when there is none (empty name) is skipped if another line does not need one.
        /// </summary>
        public static string Shout(string characterId, string abilityId, string musicianName, out string usedId)
        {
            usedId = null;
            var g = Glossary;
            if (g == null) return null;

            ShoutEntry entry = null;
            var specific = ShoutIdFor(characterId, abilityId);
            var shared = ShoutIdForAbility(abilityId);
            foreach (var id in new[] {
                         ShoutIdForMusician(specific, musicianName), ShoutIdForMusician(shared, musicianName),
                         specific, shared })
            {
                if (id != null && g.TryGetShout(id, out entry)) { usedId = id; break; }
                entry = null;
            }
            if (entry == null) return null;

            bool haveName = !string.IsNullOrEmpty(musicianName);
            var pool = new List<string>();
            foreach (var l in entry.Lines)
            {
                if (string.IsNullOrWhiteSpace(l)) continue;
                if (!haveName && NamesMusician(l)) continue;
                pool.Add(l);
            }
            if (pool.Count == 0) return null;

            var line = pool[_shoutRng.Next(pool.Count)];
            return haveName ? ReplaceMusician(line, musicianName) : line;
        }

        /// <summary>
        /// [S9 / REINA-1, D3=A] True when this member's ability has lines written for this musician
        /// (specific or shared per-musician entry). Used by abilities that hit no single musician (the
        /// Reina's eggs and hatch): if a musician with own lines is on stage, the shout calls them by
        /// name («la colmena llama a Sibi»). Presentation only; no target or number changes.
        /// </summary>
        public static bool HasMusicianShout(string characterId, string abilityId, string musicianName)
        {
            var g = Glossary;
            if (g == null) return false;
            var a = ShoutIdForMusician(ShoutIdFor(characterId, abilityId), musicianName);
            var b = ShoutIdForMusician(ShoutIdForAbility(abilityId), musicianName);
            return (a != null && g.TryGetShout(a, out _)) || (b != null && g.TryGetShout(b, out _));
        }

        private static bool NamesMusician(string line)
            => line.IndexOf("{musico}", System.StringComparison.OrdinalIgnoreCase) >= 0
            || line.IndexOf("{músico}", System.StringComparison.OrdinalIgnoreCase) >= 0
            || line.IndexOf("{musician}", System.StringComparison.OrdinalIgnoreCase) >= 0;

        private static string ReplaceMusician(string line, string name)
            => line.Replace("{musico}", name).Replace("{músico}", name).Replace("{musician}", name)
                   .Replace("{Musico}", name).Replace("{Músico}", name).Replace("{Musician}", name);

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