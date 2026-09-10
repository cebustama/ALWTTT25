// Place at: Assets/Scripts/Data/ConceptGlossarySO.cs
// [TXT-2 / D-TAG-2=C → TXT-3 / D-TXT3-0=B] Per-language registry of ALL player-facing concept
// definitions: meters and game objects (TXT-2), and — since TXT-3 — statuses (id = StatusKey) and
// card keywords (id = lowercased SpecialKeywords name). It is the ONLY text home; StatusEffectSO
// and the SpecialKeywords enum are identity, not text. GameTextWindow ▸ Concepts reports
// COVERAGE gaps (a status key or keyword name with no entry in some language).
//
// One asset per language, selected by inspector assignment (mirror of TutorialDialogCatalogSO,
// D-S5f-2=B / D2=B): runtime never reads languageCode. No public setters — the editor writes
// through SerializedObject (TXT-1 invariant); property names below are the editor's contract.
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

        [SerializeField] private List<ConceptEntry> entries = new();

        [NonSerialized] private Dictionary<string, ConceptEntry> _byId;

        public string LanguageCode => languageCode;
        public IReadOnlyList<ConceptEntry> Entries => entries;

        /// <summary>Case-insensitive lookup by concept id (the id used in &lt;link=id&gt;).</summary>
        public bool TryGet(string id, out ConceptEntry entry)
        {
            entry = null;
            if (string.IsNullOrEmpty(id)) return false;
            if (_byId == null) BuildIndex();
            return _byId.TryGetValue(id, out entry);
        }

        public void BuildIndex()
        {
            _byId = new Dictionary<string, ConceptEntry>(entries.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.Id)) continue;
                _byId[e.Id.Trim()] = e;     // last duplicate wins at runtime; the editor tab flags duplicates
            }
        }

        private void OnEnable() => _byId = null;   // rebuilt lazily after domain reload / asset edit
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
}