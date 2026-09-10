// Place at: Assets/Scripts/UI/Tooltips/ConceptTooltipResolver.cs
// [TXT-2 / D-TAG-2=C] Resolves a concept id (the id in <link=id>) to tooltip header + body by
// asking the registries that ALREADY own concept text, in this fixed order:
//   1. StatusEffectSO by StatusKey, through the configured StatusEffectCatalogueSO list
//      (SSoT_Status_Effects §3.3 / §7: DisplayName + Description are the single source);
//   2. SpecialKeywordData by SpecialKeywords enum name (SSoT_Card_System §3.3 / §10.2);
//   3. ConceptGlossarySO — only for ids no other registry owns.
// The order is the invariant "no new authority": the glossary is consulted last, so it can
// never shadow an existing home. The editor (GameTextWindow → Concepts) flags ids present in
// more than one registry; runtime just follows the order.
//
// Language: statuses and keywords carry a single language today (O-TXT-3); the glossary is
// per language and is assigned by inspector next to the tutorial catalog. Known debt T-TAG-1.
using System;
using System.Collections.Generic;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Status;

namespace ALWTTT.Tooltips
{
    public enum ConceptSource { None = 0, Status = 1, Keyword = 2, Glossary = 3 }

    public sealed class ConceptTooltipResolver
    {
        private readonly ConceptGlossarySO _glossary;
        private readonly IReadOnlyList<StatusEffectCatalogueSO> _catalogues;
        private readonly SpecialKeywordData _keywords;

        /// <param name="keywords">Optional. When null, TooltipManager.Instance.SpecialKeywordData is
        /// read at resolve time (the manager is a DontDestroyOnLoad singleton that may not exist yet
        /// when the consumer builds this resolver).</param>
        public ConceptTooltipResolver(
            ConceptGlossarySO glossary,
            IReadOnlyList<StatusEffectCatalogueSO> catalogues,
            SpecialKeywordData keywords = null)
        {
            _glossary = glossary;
            _catalogues = catalogues;
            _keywords = keywords;
        }

        private SpecialKeywordData Keywords =>
            _keywords != null ? _keywords
            : TooltipManager.Instance != null ? TooltipManager.Instance.SpecialKeywordData : null;

        public bool IsKnown(string id) => Resolve(id, out _, out _) != ConceptSource.None;

        public ConceptSource Resolve(string id, out string header, out string body)
        {
            header = null; body = null;
            if (string.IsNullOrWhiteSpace(id)) return ConceptSource.None;
            id = id.Trim();

            // 1) Statuses — existing home, first.
            if (_catalogues != null)
            {
                for (int i = 0; i < _catalogues.Count; i++)
                {
                    var cat = _catalogues[i];
                    if (cat == null) continue;
                    if (cat.TryGetByKey(id, out var so) && so != null)
                    {
                        header = so.DisplayName; body = so.Description;
                        return ConceptSource.Status;
                    }
                }
            }

            // 2) Card keywords — existing home, second. Digit guard: Enum.TryParse accepts "3".
            var kd = Keywords;
            if (kd != null && kd.SpecialKeywordBaseList != null && !char.IsDigit(id[0]) &&
                Enum.TryParse(id, true, out SpecialKeywords kw) && Enum.IsDefined(typeof(SpecialKeywords), kw))
            {
                var entry = kd.SpecialKeywordBaseList.Find(x => x != null && x.SpecialKeyword == kw);
                if (entry != null)
                {
                    header = entry.GetHeader(); body = entry.GetContent();
                    return ConceptSource.Keyword;
                }
            }

            // 3) Glossary — only what nobody else owns.
            if (_glossary != null && _glossary.TryGet(id, out var e))
            {
                header = e.DisplayName; body = e.Description;
                return ConceptSource.Glossary;
            }

            return ConceptSource.None;
        }
    }
}