// Place at: Assets/Editor/SymbolMetricsTuner.cs   (FULL REPLACEMENT, v3)
// [CARD-FACE-1 / paso 17] Uniform metrics for every sprite in the symbol atlas: size, vertical
// placement and now HORIZONTAL placement.
//
// v3 adds the two horizontal controls. Symptom that forced them: the glyph hugged the text on its
// left, even touching the previous letter, while leaving a wide gap on its right. That is not an art
// centring problem -- it is the glyph BOX sitting left of the cursor. TMP's generator appears to
// derive BX (the left side bearing) from the sprite PIVOT, which is Center, giving BX = -width/2:
// the glyph draws half a box to the LEFT of the cursor while the cursor still advances a full box.
// Verify by reading BX in the Sprite Glyph Table: ~-64 confirms it, 0 means look elsewhere.
//
// WHAT THE FOUR NUMBERS MEAN (all written uniformly to all 27 sprites)
//   Scale         TMP_SpriteCharacter.scale. 1.0 = the glyph BOX is as tall as the font's ascent
//                 line. The art fills only the 96 px safe area of a 128 px cell, so >1 is normal.
//   Baseline      BY = height * ratio. BY is the distance from the baseline to the TOP of the box,
//                 so 1.0 rests the box on the baseline; lower drops it, higher lifts it.
//   Left bearing  BX = width * ratio. 0 puts the box's left edge at the cursor -- the correct value
//                 for a centred glyph in a square cell. Negative pulls the art left, positive right.
//   Advance       horizontalAdvance = width * ratio. 1.0 steps the cursor by exactly one cell.
//                 Below 1 tightens the text around the icon, above 1 loosens it.
// Width and height are never written, which is what makes repeated Apply idempotent.
//
// Face Info stays at ZERO on purpose: that is TMP's auto-fit mode (size derived from the text's own
// font). Filling it switches TMP to the branch where the atlas dictates size and has to be kept in
// agreement with every font by hand.

using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

namespace ALWTTT.EditorTools
{
    public class SymbolMetricsTuner : EditorWindow
    {
        private const string AssetPath = "Assets/Art/UI/Symbols/game_symbols.asset";

        [SerializeField] private TMP_SpriteAsset spriteAsset;
        [SerializeField] private float scale = 1.6f;          // locked by eye 2026-09-17
        [SerializeField] private float baselineRatio = 0.7f;  // locked by eye 2026-09-17
        [SerializeField] private float bearingRatio = 0f;     // 0 = box left edge at the cursor
        [SerializeField] private float advanceRatio = 1f;     // 1 = cursor steps one full cell
        [SerializeField] private bool autoApply = true;

        private float _lS = -1f, _lB = -1f, _lX = -99f, _lA = -1f;

        [MenuItem("ALWTTT/Game Text/Tune symbol metrics")]
        private static void Open()
        {
            var w = GetWindow<SymbolMetricsTuner>(true, "Symbol metrics", true);
            w.minSize = new Vector2(440f, 330f);
            if (w.spriteAsset == null)
                w.spriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(AssetPath);
            w.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Vertical:   Scale = how TALL the icon's invisible box is (1 = a capital letter). " +
                "Baseline = where that box SITS.\n\n" +
                "Horizontal: Left bearing = the gap BEFORE the icon (0 = box starts at the cursor, " +
                "the right value for a centred glyph). Advance = how far the cursor moves AFTER it.\n\n" +
                "Leave Face Info at zero: that is TMP's auto-fit mode.",
                MessageType.Info);

            spriteAsset = (TMP_SpriteAsset)EditorGUILayout.ObjectField(
                "Sprite asset", spriteAsset, typeof(TMP_SpriteAsset), false);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Vertical", EditorStyles.boldLabel);
            scale = EditorGUILayout.Slider("Scale", scale, 0.5f, 3.0f);
            baselineRatio = EditorGUILayout.Slider("Baseline ratio", baselineRatio, 0.2f, 1.6f);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Horizontal", EditorStyles.boldLabel);
            bearingRatio = EditorGUILayout.Slider("Left bearing ratio", bearingRatio, -1.0f, 1.0f);
            advanceRatio = EditorGUILayout.Slider("Advance ratio", advanceRatio, 0.3f, 2.0f);

            EditorGUILayout.Space();
            autoApply = EditorGUILayout.ToggleLeft(
                "Apply while dragging (live preview on every TMP_Text)", autoApply);

            using (new EditorGUI.DisabledScope(spriteAsset == null))
            {
                if (GUILayout.Button("Apply to all sprites", GUILayout.Height(28f)))
                    Apply();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Fix horizontal (0 / 1.0)"))
                    { bearingRatio = 0f; advanceRatio = 1f; Apply(); }
                    if (GUILayout.Button("Reset all (1 / 1 / 0 / 1)"))
                    { scale = 1f; baselineRatio = 1f; bearingRatio = 0f; advanceRatio = 1f; Apply(); }
                }

                if (autoApply && (!Mathf.Approximately(scale, _lS) ||
                                  !Mathf.Approximately(baselineRatio, _lB) ||
                                  !Mathf.Approximately(bearingRatio, _lX) ||
                                  !Mathf.Approximately(advanceRatio, _lA)))
                    Apply();
            }

            if (spriteAsset != null && spriteAsset.spriteGlyphTable.Count > 0)
            {
                var m = spriteAsset.spriteGlyphTable[0].metrics;
                EditorGUILayout.LabelField(
                    $"{spriteAsset.spriteCharacterTable.Count} sprites \u00b7 face point size " +
                    $"{spriteAsset.faceInfo.pointSize} (0 = auto-fit, correct)", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(
                    $"glyph[0]: W {m.width:0} H {m.height:0} BX {m.horizontalBearingX:0.#} " +
                    $"BY {m.horizontalBearingY:0.#} ADV {m.horizontalAdvance:0.#}", EditorStyles.miniLabel);
            }
        }

        private void Apply()
        {
            if (spriteAsset == null) return;
            Undo.RecordObject(spriteAsset, "Tune symbol metrics");

            var glyphs = spriteAsset.spriteGlyphTable;
            for (int i = 0; i < glyphs.Count; i++)
            {
                var g = glyphs[i];
                var m = g.metrics;
                // Every field is rebuilt from width/height, which this tool never writes,
                // so applying twice produces the same result as applying once.
                g.metrics = new GlyphMetrics(
                    m.width,
                    m.height,
                    m.width * bearingRatio,
                    m.height * baselineRatio,
                    m.width * advanceRatio);
            }

            var chars = spriteAsset.spriteCharacterTable;
            for (int i = 0; i < chars.Count; i++)
                chars[i].scale = scale;

            spriteAsset.UpdateLookupTables();
            EditorUtility.SetDirty(spriteAsset);

            foreach (var t in Resources.FindObjectsOfTypeAll<TMP_Text>())
                t.SetAllDirty();
            SceneView.RepaintAll();

            _lS = scale; _lB = baselineRatio; _lX = bearingRatio; _lA = advanceRatio;
        }

        private void OnLostFocus() { AssetDatabase.SaveAssets(); }
    }
}