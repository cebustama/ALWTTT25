// Place at: Assets/Scripts/Editor/GameSymbolsTableMenu.cs
// [SYM-TABLE-1] Exports the symbol atlas as a visual HTML table with three columns:
// icon, type (symbol family) and the player-facing Spanish name of the concept the glyph stands for.
//
// Read-only contact sheet for humans. It is NOT a coverage check ("Check symbol coverage" is).
//
// Names come from the ONE text registry (SSoT_Game_Text section 3): the Spanish ConceptGlossarySO,
// read through GameText.ConceptName via the declared editor seat GameText.SetEditorFallback.
// The glossary is chosen by its languageCode field, never by its path (TXT-1 D2=B).
// Sprite id -> concept id is DERIVED from the same enum values as GameSymbols uses (F-TV-5), never
// by string surgery on the prefix, except st_ (keyed by StatusKey, an asset string) and kw_ (the
// resolver keys keywords by enum name lowercased, SSoT_Game_Text section 3).
// A glyph with no concept, or a concept with no entry, shows its sprite id in grey (miss policy:
// visible, never silent).
//
// Pixels are read from the PNG FILE, so importer Read/Write / Max Size do not matter.
// SOURCE IS PURE ASCII ON PURPOSE (F-T6-1 / F-SP1-2). Spanish labels use HTML entities.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ALWTTT.Cards;      // EffectScope, ApplyTiming
using ALWTTT.Data;
using ALWTTT.Enums;      // SpecialKeywords, CardType
using ALWTTT.Status;
using ALWTTT.Tooltips;
using MidiGenPlay;       // TrackRole
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.TextAuthoring   // house namespace for Game Text tooling (SSoT_Editor_Authoring_Tools namespace note)
{
    public static class GameSymbolsTableMenu
    {
        private const string MenuPath = "ALWTTT/Game Text/Export symbol table (HTML)";
        private const string TargetLanguage = "es";

        // Display order and Spanish type labels. Prefixes read from GameSymbols, never retyped.
        private static readonly (string prefix, string label)[] Families =
        {
            (GameSymbols.StatusPrefix,     "Estado"),
            (GameSymbols.KeywordPrefix,    "Palabra clave"),
            (GameSymbols.CardTypePrefix,   "Tipo de carta"),
            (GameSymbols.RolePrefix,       "Rol de pista"),
            (GameSymbols.PartActionPrefix, "Acci&oacute;n de parte"),
            (GameSymbols.ModifierPrefix,   "Modificador"),
            (GameSymbols.ScopePrefix,      "Alcance"),
            (GameSymbols.TimingPrefix,     "Momento"),
        };

        private sealed class Row
        {
            public string SpriteId;
            public string Name;        // resolved Spanish name, or null on miss
            public int Family;         // index into Families, Families.Length = unknown
            public string PngBase64;
        }

        [MenuItem(MenuPath)]
        public static void Export()
        {
            if (!TryLocateTexture(out Texture2D texture, out string texturePath)) return;
            var glossary = FindGlossary(TargetLanguage);
            if (glossary == null) return;

            var sourcePng = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!sourcePng.LoadImage(File.ReadAllBytes(texturePath)))
            {
                Debug.LogError("[SymbolTable] Could not decode " + texturePath);
                return;
            }

            float scale = (float)sourcePng.width / texture.width;
            var conceptOf = BuildConceptMap();
            var rows = new List<Row>();

            GameText.SetEditorFallback(glossary);
            try
            {
                foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>())
                {
                    string id = sprite.name;
                    string concept = ConceptIdFor(id, conceptOf);
                    string name = null;
                    if (concept != null)
                    {
                        string resolved = GameText.ConceptName(concept);
                        // Miss policy returns the raw id: treat that as "no name".
                        if (!string.IsNullOrWhiteSpace(resolved) &&
                            !string.Equals(resolved, concept, System.StringComparison.OrdinalIgnoreCase))
                            name = resolved;
                    }

                    rows.Add(new Row
                    {
                        SpriteId = id,
                        Name = name,
                        Family = FamilyIndex(id),
                        PngBase64 = Crop(sourcePng, ToFileRect(sprite.rect, scale)),
                    });
                }
            }
            finally
            {
                GameText.SetEditorFallback(null);
                Object.DestroyImmediate(sourcePng);
            }

            if (rows.Count == 0)
            {
                Debug.LogWarning("[SymbolTable] The texture has no slices. Is the sheet sliced?");
                return;
            }

            string html = BuildHtml(rows);

            string docsDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs"));
            string outPath = EditorUtility.SaveFilePanel("Save symbol table",
                Directory.Exists(docsDir) ? docsDir : Application.dataPath, "game_symbols_table", "html");
            if (string.IsNullOrEmpty(outPath)) return;

            File.WriteAllText(outPath, html, new UTF8Encoding(false));
            int misses = rows.Count(r => r.Name == null);
            Debug.Log("[SymbolTable] " + rows.Count + " glyphs written to " + outPath +
                      (misses > 0 ? " (" + misses + " without a Spanish name, shown as sprite id)" : ""));
            Application.OpenURL("file:///" + outPath.Replace('\\', '/'));
        }

        // -- Sprite id -> concept id --------------------------------------------------------------

        private static Dictionary<string, string> BuildConceptMap()
        {
            var map = new Dictionary<string, string>(System.StringComparer.Ordinal);
            map[GameSymbols.ModMeter] = GameSymbols.ModMeterConcept;
            map[GameSymbols.ModTempo] = GameSymbols.ModTempoConcept;
            map[GameSymbols.ModKey] = GameSymbols.ModKeyConcept;
            map[GameSymbols.ModInstrument] = GameSymbols.ModInstrumentConcept;
            map[GameSymbols.ModTonality] = GameSymbols.ModTonalityConcept;
            // mod_dir_up / mod_dir_down have no concept id in GameSymbols: they fall to the miss path.

            foreach (TrackRole r in System.Enum.GetValues(typeof(TrackRole)))
                map[GameSymbols.RoleName(r)] = GameSymbols.RoleConcept(r);
            foreach (EffectScope s in System.Enum.GetValues(typeof(EffectScope)))
                map[GameSymbols.ScopeName(s)] = GameSymbols.ScopeConcept(s);
            foreach (ApplyTiming t in System.Enum.GetValues(typeof(ApplyTiming)))
                map[GameSymbols.TimingName(t)] = GameSymbols.TimingConcept(t);
            foreach (CardType c in System.Enum.GetValues(typeof(CardType)))
                map[GameSymbols.CardTypeName(c)] = GameSymbols.CardTypeConcept(c);
            foreach (PartActionKind k in System.Enum.GetValues(typeof(PartActionKind)))
                map[GameSymbols.PartActionName(k)] = GameSymbols.PartActionConcept(k);
            foreach (SpecialKeywords k in System.Enum.GetValues(typeof(SpecialKeywords)))
                map[GameSymbols.KeywordName(k)] = k.ToString().ToLowerInvariant();
            return map;
        }

        private static string ConceptIdFor(string spriteId, Dictionary<string, string> map)
        {
            if (map.TryGetValue(spriteId, out var c)) return c;
            // st_ is keyed by StatusKey (asset string), so it cannot be enumerated from an enum here.
            if (spriteId.StartsWith(GameSymbols.StatusPrefix, System.StringComparison.Ordinal))
                return spriteId.Substring(GameSymbols.StatusPrefix.Length);
            return null;
        }

        // -- Location ------------------------------------------------------------------------------

        private static bool TryLocateTexture(out Texture2D texture, out string texturePath)
        {
            texture = null; texturePath = null;

            // Prefer the texture the TMP atlas is built from (the one the game actually uses).
            var assets = AssetDatabase.FindAssets("t:TMP_SpriteAsset game_symbols")
                .Select(g => AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();
            if (assets.Count > 1)
                Debug.LogWarning("[SymbolTable] " + assets.Count + " TMP Sprite Assets named game_symbols; using "
                                 + AssetDatabase.GetAssetPath(assets[0]));
            if (assets.Count > 0) texture = assets[0].spriteSheet as Texture2D;

            if (texture == null)
            {
                var guid = AssetDatabase.FindAssets("t:Texture2D game_symbols").FirstOrDefault();
                if (guid != null) texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
            }

            if (texture == null)
            {
                EditorUtility.DisplayDialog("Symbol table", "game_symbols texture not found.", "OK");
                return false;
            }

            texturePath = AssetDatabase.GetAssetPath(texture);
            if (!texturePath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("Symbol table", "Atlas texture is not a PNG: " + texturePath, "OK");
                return false;
            }
            return true;
        }

        /// <summary>The glossary whose languageCode matches. No path inference (TXT-1 D2=B): if none
        /// matches, a ConceptGlossarySO selected in the Project window is used instead.</summary>
        private static ConceptGlossarySO FindGlossary(string language)
        {
            var all = AssetDatabase.FindAssets("t:ConceptGlossarySO")
                .Select(g => AssetDatabase.LoadAssetAtPath<ConceptGlossarySO>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();

            var matches = all.Where(g =>
            {
                var p = new SerializedObject(g).FindProperty("languageCode");
                return p != null && p.propertyType == SerializedPropertyType.String &&
                       p.stringValue.StartsWith(language, System.StringComparison.OrdinalIgnoreCase);
            }).ToList();

            if (matches.Count > 1)
                Debug.LogWarning("[SymbolTable] " + matches.Count + " glossaries with languageCode '" + language
                                 + "'; using " + AssetDatabase.GetAssetPath(matches[0]));
            if (matches.Count > 0) return matches[0];

            if (Selection.activeObject is ConceptGlossarySO selected) return selected;

            EditorUtility.DisplayDialog("Symbol table",
                "No ConceptGlossarySO with languageCode '" + language + "' found.\n" +
                "Select the Spanish glossary in the Project window and run the menu again.", "OK");
            return null;
        }

        // -- Pixels --------------------------------------------------------------------------------

        private static RectInt ToFileRect(Rect r, float scale) => new RectInt(
            Mathf.RoundToInt(r.x * scale), Mathf.RoundToInt(r.y * scale),
            Mathf.RoundToInt(r.width * scale), Mathf.RoundToInt(r.height * scale));

        private static string Crop(Texture2D src, RectInt r)
        {
            int x = Mathf.Clamp(r.x, 0, src.width - 1), y = Mathf.Clamp(r.y, 0, src.height - 1);
            int w = Mathf.Clamp(r.width, 1, src.width - x), h = Mathf.Clamp(r.height, 1, src.height - y);
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels(src.GetPixels(x, y, w, h));
            tex.Apply();
            string b64 = System.Convert.ToBase64String(tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            return b64;
        }

        // -- HTML ----------------------------------------------------------------------------------

        private static int FamilyIndex(string id)
        {
            for (int i = 0; i < Families.Length; i++)
                if (id.StartsWith(Families[i].prefix, System.StringComparison.Ordinal)) return i;
            return Families.Length;
        }

        private static string Esc(string s) => string.IsNullOrEmpty(s) ? "" :
            s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

        private static string BuildHtml(List<Row> rows)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo("es-ES");
            var byName = System.StringComparer.Create(culture, true);
            rows = rows.OrderBy(r => r.Family).ThenBy(r => r.Name ?? r.SpriteId, byName).ToList();

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html lang=\"es\"><head><meta charset=\"utf-8\">")
              .Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">")
              .Append("<title>ALWTTT - S&iacute;mbolos</title><style>")
              .Append("body{font-family:system-ui,sans-serif;background:#f4f4f6;color:#1d1d22;margin:24px}")
              .Append("h1{font-size:20px;margin:0 0 16px}")
              .Append("table{border-collapse:collapse;background:#fff;min-width:420px}")
              .Append("th,td{border-bottom:1px solid #e3e3e8;padding:6px 14px;text-align:left;font-size:15px}")
              .Append("th{background:#ececf1;position:sticky;top:0}")
              .Append("td.icon{background:#26262e;width:72px;text-align:center}")
              .Append("td.icon img{width:56px;height:56px;display:block;margin:auto}")
              .Append("td.type{color:#555}code.miss{color:#999;font-size:14px}")
              .Append("</style></head><body>")
              .Append("<h1>S&iacute;mbolos del juego</h1>")
              .Append("<table><thead><tr><th>Icono</th><th>Tipo</th><th>Nombre</th></tr></thead><tbody>");

            foreach (var r in rows)
            {
                string type = r.Family < Families.Length ? Families[r.Family].label : "Otro";
                string name = r.Name != null ? Esc(r.Name) : "<code class=\"miss\">" + Esc(r.SpriteId) + "</code>";
                sb.Append("<tr><td class=\"icon\"><img alt=\"").Append(Esc(r.Name ?? r.SpriteId))
                  .Append("\" src=\"data:image/png;base64,").Append(r.PngBase64).Append("\"></td>")
                  .Append("<td class=\"type\">").Append(type).Append("</td>")
                  .Append("<td>").Append(name).Append("</td></tr>");
            }

            sb.Append("</tbody></table></body></html>");
            return sb.ToString();
        }
    }
}