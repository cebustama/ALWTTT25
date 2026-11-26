using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.UI;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [S10] ONE Inspiration icon everywhere: Matías's «brillo» (the one on the cards).
    ///   1. A TMP Sprite Asset from brillo.png in Resources/Sprite Assets/ (TMP finds it by
    ///      name), with glyph metrics fixed: TMP's default puts the pivot on the baseline and
    ///      half the width behind the pen, so the icon sat low and overlapped the letter before.
    ///   2. The ES dialogs: ★ and every older inspiration tag become <see cref="Tag"/>.
    ///   3. The HUD (counter bottom-left and the track rows): star.png becomes brillo.png.
    /// Idempotent: running it again re-applies the metrics and finds nothing left to swap.
    /// </summary>
    public static class S10_InspirationIconTool
    {
        // brillo.png is already gold: no tint.
        // [26-nov] Wrapped in the inspiration concept: hovering the icon shows its tooltip.
        public const string Tag = "<link=inspiration><sprite=\"brillo\" index=0></link>";
        private const string BareTag = "<sprite=\"brillo\" index=0>";
        private static readonly Regex OldTags = new Regex("<sprite=\"inspiration\"[^>]*>");

        private const string IconPath = "Assets/Art/Cards/Simbolos/brillo.png";
        private const string TargetPath = "Assets/TextMesh Pro/Resources/Sprite Assets/brillo.asset";
        private const string DialogsEs = "Assets/Resources/Data/Tutorial/Dialogs/ES";
        private const string HudStar = "Assets/Resources/Sprites/icons/hud/star.png";
        private static readonly string[] HudPrefabs =
        {
            "Assets/Prefabs/UI/Song Composition/SongCompositionUI.prefab",
            "Assets/Prefabs/UI/Song Composition/TrackLayoutElement.prefab",
        };

        [MenuItem("ALWTTT/S10 ▸ Ícono de Inspiración en los textos del tutorial")]
        public static void Run()
        {
            string made;
            if (AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(TargetPath) != null)
                made = "ya existía";
            else
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
                if (tex == null) { Fail("No encuentro " + IconPath); return; }

                string created = AssetDatabase.GenerateUniqueAssetPath(
                    Path.ChangeExtension(IconPath, ".asset").Replace('\\', '/'));
                Selection.activeObject = tex;
                if (!EditorApplication.ExecuteMenuItem("Assets/Create/TextMeshPro/Sprite Asset"))
                { Fail("No pude ejecutar Assets/Create/TextMeshPro/Sprite Asset"); return; }

                if (AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(created) == null)
                { Fail("TMP no creó el sprite asset en " + created); return; }

                string err = AssetDatabase.MoveAsset(created, TargetPath);
                if (!string.IsNullOrEmpty(err)) { Fail("No pude moverlo: " + err); return; }
                made = "creado";
            }

            var sa = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(TargetPath);
            string metrics = FixMetrics(sa);

            int dialogs = SwapAllDialogs();
            int hud = SwapHudIcons();
            AssetDatabase.SaveAssets();

            string msg = $"[S10] Ícono de Inspiración | sprite asset brillo {made} | {metrics} | " +
                         $"diálogos cambiados: {dialogs} | íconos del HUD cambiados: {hud}";
            Debug.Log(msg);
            EditorUtility.DisplayDialog("S10", msg.Replace(" | ", "\n"), "OK");
        }

        // The icon sits on the baseline like a capital letter (a touch below it) and takes
        // its own width plus a small gap, instead of TMP's pivot-centred default.
        private static string FixMetrics(TMP_SpriteAsset sa)
        {
            if (sa == null || sa.spriteGlyphTable == null || sa.spriteGlyphTable.Count == 0)
                return "SIN glifos";
            var g = sa.spriteGlyphTable[0];
            float w = g.glyphRect.width, h = g.glyphRect.height;
            // [26-nov] 35% bigger than a capital letter (playtest: too small to read).
            g.metrics = new GlyphMetrics(w, h, w * 0.06f, h * 0.90f, w * 1.14f);
            g.scale = 1.7f;
            sa.UpdateLookupTables();
            EditorUtility.SetDirty(sa);
            return $"glifo {w}×{h}, métrica fijada (sobre la línea base, escala 1,7)";
        }

        /// <summary>Swaps ★ and every older inspiration tag for <see cref="Tag"/> in every ES dialog.</summary>
        public static int SwapAllDialogs()
        {
            int changed = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject", new[] { DialogsEs }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (obj == null) continue;
                var so = new SerializedObject(obj);
                bool dirty = Swap(so.FindProperty("mechanicText"));
                var pages = so.FindProperty("pages");
                if (pages != null && pages.isArray)
                    for (int i = 0; i < pages.arraySize; i++)
                        dirty |= Swap(pages.GetArrayElementAtIndex(i));
                if (!dirty) continue;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(obj);
                changed++;
            }
            return changed;
        }

        private static bool Swap(SerializedProperty p)
        {
            if (p == null || p.propertyType != SerializedPropertyType.String) return false;
            string s = p.stringValue;
            if (string.IsNullOrEmpty(s)) return false;
            string t = OldTags.Replace(s.Replace(Tag, "★").Replace(BareTag, "★"), "★").Replace("★", Tag);
            if (t == s) return false;
            p.stringValue = t;
            return true;
        }

        // HUD: every Image showing star.png in the two prefabs shows brillo.png instead.
        private static int SwapHudIcons()
        {
            var star = AssetDatabase.LoadAssetAtPath<Sprite>(HudStar);
            var brillo = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
            if (star == null || brillo == null) { Debug.LogWarning("[S10] Falta star.png o brillo.png"); return 0; }

            int changed = 0;
            foreach (var path in HudPrefabs)
            {
                var root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) continue;
                int here = 0;
                foreach (var img in root.GetComponentsInChildren<Image>(true))
                {
                    if (img.sprite != star) continue;
                    img.sprite = brillo;
                    img.preserveAspect = true;   // brillo is taller than wide
                    here++;
                }
                if (here > 0) PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
                if (here > 0) Debug.Log($"[S10] Ícono del HUD: {here} imagen(es) star → brillo en {path}");
                changed += here;
            }
            return changed;
        }

        private static void Fail(string why)
        {
            Debug.LogError("[S10] Ícono de Inspiración: " + why);
            EditorUtility.DisplayDialog("S10", why, "OK");
        }
    }
}
