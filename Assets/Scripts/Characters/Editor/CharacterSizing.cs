// S7 / Lineup (2026-11-11) — one number per character: its height (D6=A).
// Repo path: Assets/Scripts/Characters/Editor/CharacterSizing.cs
// Editor-only.
//
// Height = the sprite's height on the child "Sprite", in the units of the prefab root's parent
// (i.e. with the root's own scale applied, as the prefab is authored). Musicians and audience share
// the convention: BandCharacter - Base (root 0.8 × SpriteParent 0.55 × Sprite) and
// AudienceCharacter - Base (root 0.5 × SpriteRoot 1 × Sprite).
//
// SetHeight scales the "Sprite" transform only — never the texture import (PPU) and never the root —
// and keeps the feet on the same floor and the body on the same x. Children of "Sprite"
// named HeartEyes keep their on-screen size (instruments/effects scale with the body); HeadRoot (FX anchor) keeps its fraction of the body.
// CharacterAnimator captures the Sprite's rest scale in Awake, so the beat animation follows.

using UnityEditor;
using UnityEngine;

namespace ALWTTT.Characters.Editor
{
    public static class CharacterSizing
    {
        public const string SpriteChild = "Sprite";
        // Sibi's prefab renamed the child (and hangs InstrumentSprite / EffectsSprite under it).
        private static readonly string[] SpriteChildNames = { "Sprite", "CharacterSprite" };
        // Only these children keep their on-screen size when the body is resized (audience heart eyes).
        // Everything else (instruments, effects) is part of the body and scales with it.
        private static readonly string[] KeepSizeChildren = { "HeartEyes" };

        public static SpriteRenderer FindSpriteRenderer(GameObject root)
        {
            foreach (var n in SpriteChildNames)
            {
                var t = FindDeep(root.transform, n);
                var sr = t != null ? t.GetComponent<SpriteRenderer>() : null;
                if (sr != null) return sr;
            }
            return null;
        }

        /// <summary>Height of a prefab asset (or any root), in units of the root's parent. -1 if not measurable.</summary>
        public static float GetHeight(GameObject root)
        {
            var sr = root != null ? FindSpriteRenderer(root) : null;
            if (sr == null || sr.sprite == null) return -1f;
            float parentScale = root.transform.parent != null ? Mathf.Abs(root.transform.parent.lossyScale.y) : 1f;
            return sr.sprite.bounds.size.y * Mathf.Abs(sr.transform.lossyScale.y) / Mathf.Max(parentScale, 1e-5f);
        }

        public static float GetWidth(GameObject root)
        {
            var sr = root != null ? FindSpriteRenderer(root) : null;
            if (sr == null || sr.sprite == null) return 1f;
            float parentScale = root.transform.parent != null ? Mathf.Abs(root.transform.parent.lossyScale.x) : 1f;
            return sr.sprite.bounds.size.x * Mathf.Abs(sr.transform.lossyScale.x) / Mathf.Max(parentScale, 1e-5f);
        }

        /// <summary>Writes a new height into the prefab file at <paramref name="prefabPath"/>. Returns false if it can't.</summary>
        public static bool SetHeight(string prefabPath, float newHeight)
        {
            if (newHeight <= 0.01f) return false;
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var sr = FindSpriteRenderer(root);
                if (sr == null || sr.sprite == null)
                {
                    Debug.LogWarning($"[S7] Lineup | {prefabPath}: no child '{SpriteChild}' with a sprite — height not changed.");
                    return false;
                }
                float oldHeight = GetHeight(root);
                if (oldHeight <= 0f) return false;
                float k = newHeight / oldHeight;
                if (Mathf.Abs(k - 1f) < 1e-4f) return true;

                var tf = sr.transform;
                Bounds b = sr.sprite.bounds;
                float topBefore = SpriteTop(root, sr);
                var head = root.transform.Find("HeadRoot");
                float headFrac = -1f;
                if (head != null)
                {
                    float bot = root.transform.InverseTransformPoint(tf.TransformPoint(new Vector3(0, b.min.y, 0))).y;
                    float top = root.transform.InverseTransformPoint(tf.TransformPoint(new Vector3(0, b.max.y, 0))).y;
                    if (top - bot > 1e-4f) headFrac = (head.localPosition.y - bot) / (top - bot);
                }

                Vector3 os = tf.localScale, op = tf.localPosition;
                float bottom = op.y + b.min.y * os.y;
                float centerX = op.x + b.center.x * os.x;
                var ns = new Vector3(os.x * k, os.y * k, os.z);
                tf.localScale = ns;
                tf.localPosition = new Vector3(centerX - b.center.x * ns.x, bottom - b.min.y * ns.y, op.z);
                foreach (Transform child in tf)
                    if (System.Array.IndexOf(KeepSizeChildren, child.name) >= 0) child.localScale /= k;

                if (head != null && headFrac >= 0f)
                {
                    float bot = root.transform.InverseTransformPoint(tf.TransformPoint(new Vector3(0, b.min.y, 0))).y;
                    float top = root.transform.InverseTransformPoint(tf.TransformPoint(new Vector3(0, b.max.y, 0))).y;
                    var hp = head.localPosition;
                    head.localPosition = new Vector3(hp.x, bot + headFrac * (top - bot), hp.z);
                }

                // [S7] The head UI follows the head: shift by how much the top moved. Relative, so
                // hand-tuned canvases (Big Guy, Influencer, Cool Dude) keep their tuning.
                float topAfter = SpriteTop(root, sr);
                ShiftHeadUI(root, topAfter - topBefore, null);

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[S7] Lineup | {System.IO.Path.GetFileNameWithoutExtension(prefabPath)} height {oldHeight:0.00} → {newHeight:0.00}");
                return true;
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        // ───────────────────────── Head UI (S7) ─────────────────────────
        // Variants inherit the Base canvas, laid out for Kid's height, so on a taller body the
        // intention, the status icons and the Vibe telegraph land on the sprite. Intention and
        // telegraph follow the head; status icons go just above the head (PlaceStatusOverHead).
        public const string AudienceBasePrefab = "Assets/Prefabs/Characters/Audience/AudienceCharacter - Base.prefab";
        private static readonly string[] HeadUIFields = { "intentImage", "vibeTelegraphRoot" };  // status icons: PlaceStatusOverHead

        /// <summary>Top of the body sprite, in the root's local units.</summary>
        public static float SpriteTop(GameObject root, SpriteRenderer sr)
        {
            if (sr == null || sr.sprite == null) return 0f;
            return root.transform.InverseTransformPoint(sr.transform.TransformPoint(new Vector3(0, sr.sprite.bounds.max.y, 0))).y;
        }

        /// <summary>
        /// Absolute fit: the head group is put where the Base has it, plus the difference between
        /// this sprite's top and the Base sprite's top. Idempotent. For new variants (import) and
        /// the «Ajustar UI» button. Returns false if the prefab has no audience canvas.
        /// </summary>
        public static bool FitHeadUI(string prefabPath)
        {
            var baseRoot = PrefabUtility.LoadPrefabContents(AudienceBasePrefab);
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var sr = FindSpriteRenderer(root);
                var baseSr = FindSpriteRenderer(baseRoot);
                if (sr == null || baseSr == null || root.GetComponentInChildren<AudienceCharacterCanvas>(true) == null)
                {
                    Debug.LogWarning($"[S7] Lineup | {prefabPath}: no audience canvas or sprite — UI not fitted.");
                    return false;
                }
                float delta = SpriteTop(root, sr) - SpriteTop(baseRoot, baseSr);
                ShiftHeadUI(root, delta, baseRoot);
                bool box = FitColliderToSprite(root, sr);
                bool status = PlaceStatusOverHead(root, sr);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[S7] Lineup | {System.IO.Path.GetFileNameWithoutExtension(prefabPath)} head UI fitted | top Δ={delta:0.00} vs Base | collider={(box ? "fitted" : "none")} | status={(status ? "over head" : "unchanged")}");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
                PrefabUtility.UnloadPrefabContents(baseRoot);
            }
        }

        /// <summary>
        /// Moves the head group up by <paramref name="delta"/> (root-local units). With
        /// <paramref name="from"/> (the Base contents) the start is the Base position of the same
        /// element (absolute fit); without it, the element's current position (relative shift).
        /// </summary>
        public static void ShiftHeadUI(GameObject root, float delta, GameObject from)
        {
            if (Mathf.Abs(delta) < 1e-4f && from == null) return;
            var canvas = root.GetComponentInChildren<AudienceCharacterCanvas>(true);
            if (canvas == null) return;
            var baseCanvas = from != null ? from.GetComponentInChildren<AudienceCharacterCanvas>(true) : null;
            Vector3 worldDelta = root.transform.TransformVector(new Vector3(0f, delta, 0f));

            foreach (var field in HeadUIFields)
            {
                var rt = HeadRect(canvas, field);
                if (rt == null || !(rt.parent is RectTransform parent)) continue;
                float startY = rt.anchoredPosition.y;
                if (baseCanvas != null)
                {
                    var brt = HeadRect(baseCanvas, field);
                    if (brt != null) startY = brt.anchoredPosition.y;
                }
                float localDy = parent.InverseTransformVector(worldDelta).y;
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, startY + localDy);
            }
        }

        /// <summary>
        /// [S7] The BoxCollider is the body: cards target it and the hover column hangs from its
        /// top-right corner (AudienceCharacterBase.BuildCharacter). Variants inherited Kid's box,
        /// so on Tentáculos the column started mid-body. Fit it to the sprite, keeping z.
        /// </summary>
        public static bool FitColliderToSprite(GameObject root, SpriteRenderer sr)
        {
            var box = root.GetComponent<BoxCollider>();
            if (box == null || sr == null || sr.sprite == null) return false;
            var b = sr.sprite.bounds;
            var t = root.transform;
            Vector3 min = t.InverseTransformPoint(sr.transform.TransformPoint(b.min));
            Vector3 max = t.InverseTransformPoint(sr.transform.TransformPoint(b.max));
            Vector3 lo = Vector3.Min(min, max), hi = Vector3.Max(min, max);
            box.center = new Vector3((lo.x + hi.x) * 0.5f, (lo.y + hi.y) * 0.5f, box.center.z);
            box.size = new Vector3(hi.x - lo.x, hi.y - lo.y, box.size.z);
            return true;
        }

        /// <summary>
        /// [S7] Status icons sit just above the head: under the feet the card hand covers them
        /// (first try, reverted after the GUIÑO smoke). At runtime the intention stacks above this
        /// row when it has icons (AudienceCharacterCanvas.LateUpdate). Only in the absolute fit
        /// (new variants and «Ajustar UI»): hand-tuned canvases are not touched.
        /// </summary>
        public static bool PlaceStatusOverHead(GameObject root, SpriteRenderer sr)
        {
            var canvas = root.GetComponentInChildren<AudienceCharacterCanvas>(true);
            if (canvas == null || sr == null || sr.sprite == null) return false;
            var status = HeadRect(canvas, "statusIconRoot");
            if (status == null) return false;
            float topWorld = sr.transform.TransformPoint(new Vector3(0f, sr.sprite.bounds.max.y, 0f)).y;
            Vector3 pivotToBottom = status.TransformVector(new Vector3(0f, status.rect.yMin, 0f));
            float gap = root.transform.TransformVector(new Vector3(0f, 0.05f, 0f)).y;
            status.position = new Vector3(status.position.x, topWorld + gap - pivotToBottom.y, status.position.z);
            return true;
        }

        private static RectTransform HeadRect(AudienceCharacterCanvas canvas, string field)
        {
            var p = new SerializedObject(canvas).FindProperty(field);
            var obj = p != null ? p.objectReferenceValue : null;
            if (obj is Component c) return c.transform as RectTransform;
            if (obj is GameObject g) return g.transform as RectTransform;
            return null;
        }

        private static Transform FindDeep(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t)
            {
                var r = FindDeep(c, name);
                if (r != null) return r;
            }
            return null;
        }
    }
}
