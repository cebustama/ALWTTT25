using System.Collections.Generic;
using ALWTTT.Musicians;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [S10 / D18=A] The musicians' faces as icons inside text («Conito 🙂»).
    /// TMP cannot draw a loose Sprite in a text: it needs a TMP Sprite Asset. All four
    /// CharacterIcon sprites are slices of one texture (icons1.png), so this builds ONE
    /// sprite asset, «musicos», with one glyph per musician named «mus_&lt;characterId&gt;».
    /// TMP finds it by name in Resources/Sprite Assets/. The text tag is
    /// <c>&lt;sprite="musicos" name="mus_1"&gt;</c> (see TutorialTokenResolver.MusicianIcon).
    /// Built by code, not by the TMP menu: the menu would add every slice of the texture.
    /// Glyph metrics as the brillo icon (S10_InspirationIconTool): on the baseline, own width.
    /// Idempotent: running it again rebuilds the glyph table.
    /// </summary>
    public static class S10_MusicianIconTool
    {
        public const string AssetName = "musicos";
        private const string TargetPath = "Assets/TextMesh Pro/Resources/Sprite Assets/" + AssetName + ".asset";
        private const float GlyphScale = 1.5f;

        [MenuItem("ALWTTT/S10 ▸ Íconos de músicos en los textos")]
        public static void Run()
        {
            // 1. The musicians and their faces.
            var faces = new List<(string id, string name, Sprite sprite)>();
            Texture2D tex = null;
            foreach (var guid in AssetDatabase.FindAssets("t:MusicianCharacterData"))
            {
                var m = AssetDatabase.LoadAssetAtPath<MusicianCharacterData>(AssetDatabase.GUIDToAssetPath(guid));
                if (m == null || m.CharacterIcon == null || string.IsNullOrEmpty(m.CharacterId)) continue;
                var t = m.CharacterIcon.texture;
                if (tex == null) tex = t;
                if (t != tex)
                {
                    Debug.LogWarning($"[S10] Íconos de músicos: '{m.CharacterName}' usa otra textura ({t.name}); se omite.");
                    continue;
                }
                bool dup = false;
                foreach (var f in faces) if (f.id == m.CharacterId) dup = true;
                if (!dup) faces.Add((m.CharacterId, m.CharacterName, m.CharacterIcon));
            }
            if (faces.Count == 0 || tex == null) { Fail("No encontré músicos con CharacterIcon."); return; }

            // 2. The sprite asset (created once).
            string made = "ya existía";
            var sa = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(TargetPath);
            if (sa == null)
            {
                sa = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
                AssetDatabase.CreateAsset(sa, TargetPath);
                // version has an internal setter; an empty version + a material makes TMP
                // "upgrade" the asset from the legacy list and wipe the tables.
                var so = new SerializedObject(sa);
                var v = so.FindProperty("m_Version");
                if (v != null) { v.stringValue = "1.1.0"; so.ApplyModifiedPropertiesWithoutUndo(); }
                made = "creado";
            }
            sa.spriteSheet = tex;
            sa.hashCode = TMP_TextUtilities.GetSimpleHashCode(sa.name);

            // 3. One glyph + one character per musician.
            sa.spriteGlyphTable.Clear();
            sa.spriteCharacterTable.Clear();
            var names = new List<string>();
            for (int i = 0; i < faces.Count; i++)
            {
                var s = faces[i].sprite;
                float w = s.rect.width, h = s.rect.height;
                var glyph = new TMP_SpriteGlyph
                {
                    index = (uint)i,
                    metrics = new GlyphMetrics(w, h, w * 0.06f, h * 0.85f, w * 1.12f),
                    glyphRect = new GlyphRect(s.rect),
                    scale = GlyphScale,
                    sprite = s,
                };
                sa.spriteGlyphTable.Add(glyph);
                sa.spriteCharacterTable.Add(new TMP_SpriteCharacter(0xFFFE, glyph) { name = "mus_" + faces[i].id, scale = 1f });
                names.Add($"{faces[i].name}=mus_{faces[i].id}");
            }

            // 4. Material (the TMP sprite shader on the icons texture).
            if (sa.material == null)
            {
                var mat = new Material(Shader.Find("TextMeshPro/Sprite")) { name = AssetName + " Material" };
                mat.SetTexture(ShaderUtilities.ID_MainTex, tex);
                AssetDatabase.AddObjectToAsset(mat, sa);
                sa.material = mat;
            }
            else sa.material.SetTexture(ShaderUtilities.ID_MainTex, tex);

            sa.UpdateLookupTables();
            EditorUtility.SetDirty(sa);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(TargetPath);

            string msg = $"[S10] Íconos de músicos | sprite asset '{AssetName}' {made} | textura {tex.name} | " +
                         $"{faces.Count} glifo(s): {string.Join(", ", names)} | escala {GlyphScale}";
            Debug.Log(msg);
            EditorUtility.DisplayDialog("S10", msg.Replace(" | ", "\n"), "OK");
        }

        private static void Fail(string why)
        {
            Debug.LogError("[S10] Íconos de músicos: " + why);
            EditorUtility.DisplayDialog("S10", why, "OK");
        }
    }
}
