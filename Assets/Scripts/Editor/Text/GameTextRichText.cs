#if UNITY_EDITOR
// Place at: Assets/Scripts/Editor/Text/GameTextRichText.cs
// [TXT-4 / D-TXT4-5 · D-TXT4-6 · D-TXT4-7] Editor-side checks over authored rich text. Nothing here
// runs at runtime and nothing here strips anything: the stored form is TMP-native and TMP renders
// whatever it is given (D-TAG-1=A′ — the failure mode of an unchecked tag is invisible, not broken).
// The editor's job is to make the invisible visible BEFORE the build:
//   · TAG unsupported  — a tag outside the declared subset {sprite, color, b, i, u, size, link}
//   · PLACEHOLDERS differ — {n} set differs between languages (a reordered translation is fine,
//                          a dropped argument is not: string.Format would throw or drop a number)
//   · SPRITES differ   — <sprite name=…> set differs between languages (same hole as a lost tag)
//   · SPRITE index     — <sprite=N> / <sprite index=N>: refused by decision, names only, because
//                        an atlas index is renumbered by every re-import and a name is not
//   · SPRITE unknown   — name not in TMP_Settings.defaultSpriteAsset (+ its fallbacks). Only
//                        checked when a default sprite asset is assigned; otherwise reported once as
//                        unverifiable by the window header.
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;

namespace ALWTTT.TextAuthoring
{
    internal static class GameTextRichText
    {
        public static readonly HashSet<string> AllowedTags =
            new(StringComparer.OrdinalIgnoreCase) { "sprite", "color", "b", "i", "u", "size", "link" };

        private static readonly Regex TagRx = new(@"</?\s*([a-zA-Z]+)", RegexOptions.Compiled);
        private static readonly Regex PlaceholderRx = new(@"\{(\d+)\}", RegexOptions.Compiled);
        private static readonly Regex SpriteNameRx =
            new(@"<sprite\s+name\s*=\s*""?(?<n>[^"">\s]+)""?[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SpriteIndexRx =
            new(@"<sprite\s*(=|index\s*=)\s*""?\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>Tag names outside the allowed subset, deduplicated, in order of appearance.</summary>
        public static List<string> UnsupportedTags(string text)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(text)) return list;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match m in TagRx.Matches(text))
            {
                string name = m.Groups[1].Value;
                if (AllowedTags.Contains(name)) continue;
                if (seen.Add(name)) list.Add(name);
            }
            return list;
        }

        /// <summary>Set of placeholder indices ({0}, {1}, …) used by the text.</summary>
        public static HashSet<int> Placeholders(string text)
        {
            var set = new HashSet<int>();
            if (string.IsNullOrEmpty(text)) return set;
            foreach (Match m in PlaceholderRx.Matches(text)) set.Add(int.Parse(m.Groups[1].Value));
            return set;
        }

        /// <summary>Set of sprite NAMES referenced by &lt;sprite name=…&gt;.</summary>
        public static HashSet<string> SpriteNames(string text)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(text)) return set;
            foreach (Match m in SpriteNameRx.Matches(text)) set.Add(m.Groups["n"].Value);
            return set;
        }

        public static bool UsesSpriteIndex(string text)
            => !string.IsNullOrEmpty(text) && SpriteIndexRx.IsMatch(text);

        public static bool SetsEqual<T>(HashSet<T> a, HashSet<T> b) => a.Count == b.Count && a.IsSubsetOf(b);

        // ── Sprite-asset lookup (D-TXT4-5: one global asset, TMP Settings default) ──
        private static HashSet<string> _knownSprites;
        private static bool _spriteAssetPresent;

        /// <summary>True when TMP_Settings has a default sprite asset; false ⇒ names are unverifiable here.</summary>
        public static bool SpriteAssetAvailable
        {
            get { if (_knownSprites == null) LoadSprites(); return _spriteAssetPresent; }
        }

        public static bool SpriteExists(string name)
        {
            if (_knownSprites == null) LoadSprites();
            return !_spriteAssetPresent || _knownSprites.Contains(name);
        }

        public static void ResetSpriteCache() => _knownSprites = null;

        private static void LoadSprites()
        {
            _knownSprites = new HashSet<string>(StringComparer.Ordinal);
            _spriteAssetPresent = false;
            var root = TMP_Settings.defaultSpriteAsset;
            if (root == null) return;
            _spriteAssetPresent = true;
            var stack = new Stack<TMP_SpriteAsset>();
            var visited = new HashSet<TMP_SpriteAsset>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var a = stack.Pop();
                if (a == null || !visited.Add(a)) continue;
                if (a.spriteCharacterTable != null)
                    foreach (var c in a.spriteCharacterTable)
                        if (c != null && !string.IsNullOrEmpty(c.name)) _knownSprites.Add(c.name);
                if (a.fallbackSpriteAssets != null)
                    foreach (var f in a.fallbackSpriteAssets) stack.Push(f);
            }
        }
    }
}
#endif