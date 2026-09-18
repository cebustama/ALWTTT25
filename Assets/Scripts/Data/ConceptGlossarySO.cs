// Place at: Assets/Scripts/Data/ConceptGlossarySO.cs
// [TXT-2 / D-TAG-2=C → TXT-3 / D-TXT3-0=B → TXT-4 / D-TXT4-3=b′ · D-TXT4-4] Per-language registry
// of ALL player-facing text that is not tutorial copy:
//   entries   — concept definitions: meters and game objects (TXT-2), statuses (id = StatusKey) and
//               card keywords (id = lowercased SpecialKeywords name) (TXT-3).
//   cardNames — player-facing card names by CardDefinition.Id (TXT-4). CardDefinition.displayName is
//               a developer label since TXT-4, exactly as StatusEffectSO.displayName since TXT-3.
//   templates — composable fragments with {0}-style placeholders (TXT-4): card-effect lines that
//               CardEffectDescriptionBuilder composes, target phrases, and UI labels that used to be
//               C# literals. Ids are declared in GameTextTemplateIds; the editor reports coverage.
// Three lists, ONE asset per language, ONE seat (TooltipManager.conceptGlossary): adding lists here
// rather than a sibling asset is what keeps language selection at a single inspector assignment
// (R-1 constraint — no third assignment point). Runtime never reads languageCode (D-S5f-2=B / D2=B).
// No public setters — the editor writes through SerializedObject (TXT-1 invariant); the property
// names below are the editor's contract.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "ConceptGlossary", menuName = "ALWTTT/Text/Concept Glossary", order = 30)]
    public sealed class ConceptGlossarySO : ScriptableObject
    {
        [Tooltip("Language code of this glossary's text, e.g. 'en' / 'es'. Authoring tooling only; runtime never reads it.")]
        [SerializeField] private string languageCode = "";


        [Tooltip("[TEXT-5 / D-T5-2=B] .NET culture name that formats the NUMBERS inside this language's text " +
                 "(decimal separator), e.g. 'es-ES'. Empty = invariant culture (point). Read by GameText.Number. " +
                 "This is data the asset carries, like its strings — runtime still never branches on languageCode.")]
        [SerializeField] private string numberCulture = "";

        [SerializeField] private List<ConceptEntry> entries = new();

        [Header("Card names (TXT-4)")]
        [Tooltip("Player-facing card names, keyed by CardDefinition.Id. Every CardDefinition must have one (coverage is reported in GameTextWindow ▸ Cards).")]
        [SerializeField] private List<CardNameEntry> cardNames = new();

        [Header("Text templates (TXT-4)")]
        [Tooltip("Composable fragments with {0}-style placeholders. Required ids are declared in GameTextTemplateIds; gaps are reported in GameTextWindow ▸ Templates.")]
        [SerializeField] private List<TextTemplateEntry> templates = new();

        [NonSerialized] private Dictionary<string, ConceptEntry> _byId;
        [NonSerialized] private Dictionary<string, string> _cardNameById;
        [NonSerialized] private Dictionary<string, string> _templateById;

        public string LanguageCode => languageCode;


        [NonSerialized] private System.Globalization.CultureInfo _numberFormat;

        /// <summary>[TEXT-5 / D-T5-2=B] Culture used to format numbers composed into this glossary's text.
        /// Empty/unknown name ⇒ InvariantCulture (an unknown name warns once per asset).</summary>
        public System.Globalization.CultureInfo NumberFormat
        {
            get
            {
                if (_numberFormat != null) return _numberFormat;
                if (string.IsNullOrWhiteSpace(numberCulture))
                    return _numberFormat = System.Globalization.CultureInfo.InvariantCulture;
                try { _numberFormat = System.Globalization.CultureInfo.GetCultureInfo(numberCulture.Trim()); }
                catch (System.Globalization.CultureNotFoundException)
                {
                    Debug.LogWarning($"[ConceptGlossary] '{name}': numberCulture '{numberCulture}' is unknown on this platform — numbers use the invariant culture.");
                    _numberFormat = System.Globalization.CultureInfo.InvariantCulture;
                }
                return _numberFormat;
            }
        }

        public IReadOnlyList<ConceptEntry> Entries => entries;
        public IReadOnlyList<CardNameEntry> CardNames => cardNames;
        public IReadOnlyList<TextTemplateEntry> Templates => templates;

        /// <summary>Case-insensitive lookup by concept id (the id used in &lt;link=id&gt;).</summary>
        public bool TryGet(string id, out ConceptEntry entry)
        {
            entry = null;
            if (string.IsNullOrEmpty(id)) return false;
            if (_byId == null) BuildIndex();
            return _byId.TryGetValue(id, out entry);
        }

        /// <summary>[TXT-4] Case-insensitive lookup of a player-facing card name by CardDefinition.Id.</summary>
        public bool TryGetCardName(string cardId, out string name)
        {
            name = null;
            if (string.IsNullOrEmpty(cardId)) return false;
            if (_cardNameById == null) BuildIndex();
            return _cardNameById.TryGetValue(cardId, out name);
        }

        /// <summary>[TXT-4] Case-insensitive lookup of a template's raw text (placeholders unresolved).</summary>
        public bool TryGetTemplate(string templateId, out string text)
        {
            text = null;
            if (string.IsNullOrEmpty(templateId)) return false;
            if (_templateById == null) BuildIndex();
            return _templateById.TryGetValue(templateId, out text);
        }

        public void BuildIndex()
        {
            _byId = new Dictionary<string, ConceptEntry>(entries.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.Id)) continue;
                _byId[e.Id.Trim()] = e;     // last duplicate wins at runtime; the editor tab flags duplicates
            }
            _cardNameById = new Dictionary<string, string>(cardNames.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var c in cardNames)
            {
                if (c == null || string.IsNullOrWhiteSpace(c.Id)) continue;
                _cardNameById[c.Id.Trim()] = c.DisplayName ?? "";
            }
            _templateById = new Dictionary<string, string>(templates.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var t in templates)
            {
                if (t == null || string.IsNullOrWhiteSpace(t.Id)) continue;
                _templateById[t.Id.Trim()] = t.Text ?? "";
            }
        }

        private void OnEnable() { _byId = null; _cardNameById = null; _templateById = null; _numberFormat = null; }   // rebuilt lazily after domain reload / asset edit
    }

    [Serializable]
    public sealed class ConceptEntry
    {
        [Tooltip("Stable id used in <link=id> tags. Lowercase, [a-z0-9_]. Language-neutral.")]
        [SerializeField] private string id;
        [Tooltip("Tooltip header in this glossary's language.")]
        [SerializeField] private string displayName;
        [Tooltip("Tooltip body in this glossary's language. Plain register, no voice.")]
        [SerializeField, TextArea(2, 5)] private string description;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
    }

    /// <summary>[TXT-4 / D-TXT4-4] Player-facing card name. Id = CardDefinition.Id, verbatim (card ids
    /// are mixed-case in the catalogue; lookups are case-insensitive).</summary>
    [Serializable]
    public sealed class CardNameEntry
    {
        [Tooltip("CardDefinition.Id, verbatim.")]
        [SerializeField] private string id;
        [Tooltip("Card name shown on the card face and the detail modal, in this glossary's language.")]
        [SerializeField] private string displayName;

        public string Id => id;
        public string DisplayName => displayName;
    }

    /// <summary>[TXT-4 / D-TXT4-3] Composable text fragment. Placeholders are positional ({0}, {1}…) so
    /// a translation may reorder them; the editor checks that every language uses the same set.</summary>
    [Serializable]
    public sealed class TextTemplateEntry
    {
        [Tooltip("Template id. Lowercase, [a-z0-9_]. Declared in GameTextTemplateIds.")]
        [SerializeField] private string id;
        [Tooltip("Text with {0}-style placeholders. Allowed rich text: <sprite name=…>, <color>, <b>, <i>, <u>, <size>, <link>.")]
        [SerializeField, TextArea(1, 3)] private string text;

        public string Id => id;
        public string Text => text;
    }
}