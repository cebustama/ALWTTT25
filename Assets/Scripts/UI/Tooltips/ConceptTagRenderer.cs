// Place at: Assets/Scripts/UI/Tooltips/ConceptTagRenderer.cs
// [TXT-2 / D-TAG-1=A′] The ONE point where an authored <link=id>text</link> gains its look.
// Authored form is TMP-native: without this pass a tag renders as plain text (link tags are
// invisible), never as literal brackets. This pass only wraps KNOWN ids in colour + underline;
// unknown ids are left untouched (plain, no hover) and reported once per call via onUnknown.
// Any surface that shows tagged text calls Decorate() after resolving {$tokens}; nobody
// re-implements the look per consumer (D-TAG-4: capability for pages, cards and statuses).
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ALWTTT.Tooltips
{
    public static class ConceptTagRenderer
    {
        /// <summary>Opening tag, quoted or unquoted id: &lt;link=vibe&gt; or &lt;link="vibe"&gt;.</summary>
        public static readonly Regex OpenTagRx =
            new Regex(@"<link=""?(?<id>[A-Za-z0-9_]+)""?>", RegexOptions.Compiled);

        private static readonly Regex LinkRx =
            new Regex(@"<link=""?(?<id>[A-Za-z0-9_]+)""?>(?<body>.*?)</link>",
                      RegexOptions.Compiled | RegexOptions.Singleline);

        private const string OpenLiteral = "<link=";
        private const string CloseLiteral = "</link>";

        public static string Decorate(
            string text, ConceptTooltipResolver resolver, Color color, bool underline,
            Action<string> onUnknown = null)
        {
            if (string.IsNullOrEmpty(text) || resolver == null) return text;
            if (text.IndexOf(OpenLiteral, StringComparison.Ordinal) < 0) return text;

            string hex = ColorUtility.ToHtmlStringRGB(color);
            HashSet<string> reported = null;

            return LinkRx.Replace(text, m =>
            {
                string id = m.Groups["id"].Value;
                if (!resolver.IsKnown(id))
                {
                    if (onUnknown != null && (reported ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase)).Add(id))
                        onUnknown(id);
                    return m.Value;                       // plain: TMP ignores the bare link tag
                }
                string body = m.Groups["body"].Value;
                if (underline) body = "<u>" + body + "</u>";
                return "<link=" + id + "><color=#" + hex + ">" + body + "</color></link>";
            });
        }

        /// <summary>Ids of every opening tag, in order of appearance (editor parity + unknown checks).</summary>
        public static IEnumerable<string> ExtractIds(string text)
        {
            if (string.IsNullOrEmpty(text)) yield break;
            foreach (Match m in OpenTagRx.Matches(text)) yield return m.Groups["id"].Value;
        }

        /// <summary>True when the count of &lt;link= and &lt;/link&gt; differ. An unclosed link
        /// extends to the end of the text in TMP, so the editor flags it.</summary>
        public static bool HasUnbalancedLinks(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return Count(text, OpenLiteral) != Count(text, CloseLiteral);
        }

        private static int Count(string s, string needle)
        {
            int n = 0, i = 0;
            while ((i = s.IndexOf(needle, i, StringComparison.Ordinal)) >= 0) { n++; i += needle.Length; }
            return n;
        }
    }
}