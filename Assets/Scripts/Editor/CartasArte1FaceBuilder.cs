using System.Collections.Generic;
using ALWTTT.Cards;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [CARTAS-ARTE-1] Builds the new card face in CardCanvas.prefab (nested by Card3D and CardUI),
    /// wires the new CardBase fields in both prefabs and assigns Matías's composition art.
    ///
    /// Frames come from Matías's Miro board ("reutilizables"), cropped to one shared 612×948 canvas
    /// in Assets/Art/Cards/Marcos: composición and habilidad are final; the Finisher uses the
    /// neutral frame tinted cyan until Matías draws its own (pedido). The composition illustrations
    /// were re-exported on that same canvas, so frame and art both stretch over the whole card.
    /// Where name and description go per face is decided at runtime by CardBase.ApplyFaceLayout.
    /// Re-running the menu is safe: it reuses what it already built.
    /// </summary>
    public static class CartasArte1FaceBuilder
    {
        private const string Tag = "[CARTAS-ARTE-1]";
        private const string CanvasPath = "Assets/Prefabs/Cards/CardCanvas.prefab";
        private static readonly string[] CardPrefabs =
        {
            "Assets/Prefabs/Cards/Card3D.prefab",
            "Assets/Prefabs/Cards/CardUI.prefab",
        };
        private const string ArtRoot = "Assets/Art/Cards/";
        private const string OutlineMat = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Outline.mat";

        private static readonly Color FinisherTint = new Color(0.55f, 0.95f, 1f, 1f);
        private static readonly Color BandColor = new Color(0.06f, 0.04f, 0.16f, 0.82f);
        private static readonly Color CostColor = new Color(1f, 0.82f, 0.25f, 1f);

        [MenuItem("ALWTTT/CARTAS-ARTE-1/1. Montar la cara nueva (CardCanvas + cableado)")]
        public static void BuildFace()
        {
            EnsureSingleSprites();
            var plus = Load<Sprite>(ArtRoot + "Simbolos/mas.png");
            var spark = Load<Sprite>(ArtRoot + "Simbolos/brillo.png");
            var frameComp = Load<Sprite>(ArtRoot + "Marcos/marco_composicion.png");
            var frameAbility = Load<Sprite>(ArtRoot + "Marcos/marco_habilidad.png");
            var frameNeutral = Load<Sprite>(ArtRoot + "Marcos/marco_neutral.png");
            var outline = AssetDatabase.LoadAssetAtPath<Material>(OutlineMat);
            if (plus == null || spark == null || frameComp == null || frameAbility == null || frameNeutral == null)
            {
                Debug.LogError($"{Tag} Faltan sprites en {ArtRoot}Simbolos/ o Marcos/ (¿importados como Sprite?).");
                return;
            }

            var contents = PrefabUtility.LoadPrefabContents(CanvasPath);
            try
            {
                var root = FindDeep(contents.transform, "Root");
                var backImage = root != null ? root.Find("BackImage") : null;
                var bgSprite = root != null ? root.Find("Background Sprite") : null;
                var actionBg = bgSprite != null ? bgSprite.Find("Action Bg") : null;
                var compBg = bgSprite != null ? bgSprite.Find("Composition Bg") : null;
                var name = root != null ? root.Find("Name") : null;
                var desc = root != null ? root.Find("Description") : null;
                var genRoot = root != null ? root.Find("InspirationGenRoot") : null;
                var costRoot = root != null ? root.Find("InspirationCostRoot") : null;
                if (backImage == null || actionBg == null || compBg == null || name == null || desc == null
                    || genRoot == null || costRoot == null)
                {
                    Debug.LogError($"{Tag} La jerarquía de {CanvasPath} no es la esperada (Root/Background Sprite/…).");
                    return;
                }

                // 1. Matías's frames, full card. The Finisher is a cyan copy of the neutral frame.
                SetFrame(actionBg, frameAbility, Color.white);
                SetFrame(compBg, frameComp, Color.white);
                var finBg = bgSprite.Find("Finisher Bg");
                if (finBg == null)
                {
                    finBg = Object.Instantiate(actionBg.gameObject, bgSprite).transform;
                    finBg.name = "Finisher Bg";
                }
                SetFrame(finBg, frameNeutral, FinisherTint);
                finBg.gameObject.SetActive(false);

                // 2. Full art under the frames (the frame's window shows it).
                var fullArt = GetOrCreate(root, "FullArt");
                Stretch((RectTransform)fullArt);
                var artImg = fullArt.GetComponent<Image>();
                artImg.preserveAspect = false;   // same canvas as the frame: stretch both alike
                artImg.raycastTarget = false;
                fullArt.gameObject.SetActive(false);

                // 3. Track word in the composition frame's top border.
                var label = GetOrCreate(root, "TrackLabel");
                var labelRt = (RectTransform)label;
                labelRt.anchorMin = new Vector2(0.25f, 0.905f);
                labelRt.anchorMax = new Vector2(0.75f, 0.985f);
                labelRt.offsetMin = labelRt.offsetMax = Vector2.zero;
                var labelImg = label.GetComponent<Image>();
                labelImg.preserveAspect = true;
                labelImg.raycastTarget = false;
                label.gameObject.SetActive(false);

                // 4. Dark band behind name + description on compositions.
                var band = GetOrCreate(root, "FullArtDescBand");
                var bandRt = (RectTransform)band;
                bandRt.anchorMin = new Vector2(0.16f, 0.195f);
                bandRt.anchorMax = new Vector2(0.84f, 0.41f);
                bandRt.offsetMin = bandRt.offsetMax = Vector2.zero;
                var bandImg = band.GetComponent<Image>();
                bandImg.color = BandColor;
                bandImg.raycastTarget = false;
                band.gameObject.SetActive(false);

                // Draw order: window art, full art, frames, word, band, name, description, the rest.
                int i = backImage.GetSiblingIndex();
                fullArt.SetSiblingIndex(++i);
                bgSprite.SetSiblingIndex(++i);
                label.SetSiblingIndex(++i);
                band.SetSiblingIndex(++i);
                name.SetSiblingIndex(++i);
                desc.SetSiblingIndex(++i);

                // 5. Name and description texts fill their boxes and shrink to fit.
                var nameText = name.GetComponentInChildren<TextMeshProUGUI>(true);
                FitText(nameText, 4f, 9f, FontStyles.Bold, TextAlignmentOptions.Center);
                var descText = desc.GetComponentInChildren<TextMeshProUGUI>(true);
                FitText(descText, 4f, 8f, FontStyles.Normal, TextAlignmentOptions.Center);

                // 6. What the card GIVES: big "+N" in the middle of the art (Matías).
                var genRt = (RectTransform)genRoot;
                genRt.anchorMin = genRt.anchorMax = new Vector2(0.5f, 0.5f);
                genRt.pivot = new Vector2(0.5f, 0.5f);
                genRt.sizeDelta = new Vector2(14f, 19f);
                genRt.anchoredPosition = new Vector2(-9f, 0f);
                SetImage(genRoot, plus);
                StyleNumber(genRoot.GetComponentInChildren<TextMeshProUGUI>(true), 28f, Color.white, outline, new Vector2(30f, 0f));

                // 7. What the card COSTS: spark + number, top-left.
                var costRt = (RectTransform)costRoot;
                costRt.anchorMin = costRt.anchorMax = new Vector2(0f, 1f);
                costRt.pivot = new Vector2(0.5f, 0.5f);
                costRt.sizeDelta = new Vector2(11f, 17f);
                costRt.anchoredPosition = new Vector2(12f, -13f);
                SetImage(costRoot, spark);
                StyleNumber(costRoot.GetComponentInChildren<TextMeshProUGUI>(true), 17f, CostColor, outline, new Vector2(18f, 0f));

                // 8. Unplayable overlay: Matías's gradient (D9 = A), straightened to the card
                //    canvas and cut to the frames' silhouette; it carries its own transparency.
                var lockSprite = Load<Sprite>(ArtRoot + "Simbolos/bloqueo.png");
                var passive = FindDeep(root, "PassiveImage");
                if (passive != null && lockSprite != null)
                {
                    Stretch((RectTransform)passive);
                    var pImg = passive.GetComponent<Image>();
                    if (pImg != null)
                    {
                        pImg.sprite = lockSprite;
                        pImg.type = Image.Type.Simple;
                        pImg.preserveAspect = false;
                        pImg.color = Color.white;
                    }
                    passive.SetAsLastSibling();

                    // Matías's padlock in the middle, child of the overlay so it shows only with it.
                    var lockIcon = Load<Sprite>(ArtRoot + "Simbolos/candado.png");
                    if (lockIcon != null)
                    {
                        var padlock = GetOrCreate(passive, "Candado");
                        var plRt = (RectTransform)padlock;
                        plRt.anchorMin = new Vector2(0.34f, 0.42f);
                        plRt.anchorMax = new Vector2(0.66f, 0.70f);
                        plRt.offsetMin = plRt.offsetMax = Vector2.zero;
                        var plImg = padlock.GetComponent<Image>();
                        plImg.sprite = lockIcon;
                        plImg.preserveAspect = true;
                        plImg.raycastTarget = false;
                        plImg.color = Color.white;
                        padlock.gameObject.SetActive(true);
                    }
                }
                else Debug.LogWarning($"{Tag} No encontré PassiveImage o Simbolos/bloqueo.png: el rojo de bloqueo queda como estaba.");

                PrefabUtility.SaveAsPrefabAsset(contents, CanvasPath);
                Debug.Log($"{Tag} CardCanvas montado: marcos de Matías, FullArt, TrackLabel, franja, número grande, brillo" +
                          $"{(passive != null && lockSprite != null ? " y bloqueo de Matías" : "")}.");
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }

            foreach (var path in CardPrefabs) WireCardPrefab(path);
        }

        private static void WireCardPrefab(string path)
        {
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var card = contents.GetComponentInChildren<CardBase>(true);
                if (card == null) { Debug.LogError($"{Tag} {path}: no tiene CardBase."); return; }
                var so = new SerializedObject(card);
                int wired = 0;
                wired += SetRef(so, "finisherBackground", FindDeep(contents.transform, "Finisher Bg")?.gameObject);
                wired += SetRef(so, "windowArtRoot", FindDeep(contents.transform, "BackImage")?.gameObject);
                wired += SetRef(so, "fullArtImage", FindDeep(contents.transform, "FullArt")?.GetComponent<Image>());
                wired += SetRef(so, "fullArtDescBand", FindDeep(contents.transform, "FullArtDescBand")?.gameObject);
                wired += SetRef(so, "trackLabelImage", FindDeep(contents.transform, "TrackLabel")?.GetComponent<Image>());
                wired += SetRef(so, "labelVoz", Load<Sprite>(ArtRoot + "Rotulos/rotulo_voz.png"));
                wired += SetRef(so, "labelBajo", Load<Sprite>(ArtRoot + "Rotulos/rotulo_bajo.png"));
                wired += SetRef(so, "labelGuitarra", Load<Sprite>(ArtRoot + "Rotulos/rotulo_guitarra.png"));
                wired += SetRef(so, "labelOrgano", Load<Sprite>(ArtRoot + "Rotulos/rotulo_organo.png"));
                wired += SetRef(so, "labelBateria", Load<Sprite>(ArtRoot + "Rotulos/rotulo_bateria.png"));

                // CardUI had gen/cost crossed (gen text under the cost root and vice versa). Take
                // both texts and both badge roots from the hierarchy so each number sits on its icon.
                var genRootT = FindDeep(contents.transform, "InspirationGenRoot");
                var costRootT = FindDeep(contents.transform, "InspirationCostRoot");
                int badges = 0;
                badges += SetRef(so, "inspirationGenBadgeRoot", genRootT?.gameObject);
                badges += SetRef(so, "inspirationCostBadgeRoot", costRootT?.gameObject);
                badges += SetRef(so, "inspirationGenTextField", genRootT?.GetComponentInChildren<TextMeshProUGUI>(true));
                badges += SetRef(so, "inspirationCostTextField", costRootT?.GetComponentInChildren<TextMeshProUGUI>(true));
                if (badges < 4) Debug.LogWarning($"{Tag} {path}: solo {badges}/4 referencias de la ★ (textos y círculos).");
                so.ApplyModifiedPropertiesWithoutUndo();

                var gen = so.FindProperty("inspirationGenTextField").objectReferenceValue as Component;
                var cost = so.FindProperty("inspirationCostTextField").objectReferenceValue as Component;
                var nameT = so.FindProperty("nameTextField").objectReferenceValue as Component;
                var descT = so.FindProperty("descTextField").objectReferenceValue as Component;

                // CardUI overrides font sizes / positions of these texts on its CardCanvas instance;
                // without reverting them the inventory card would keep the old look.
                int reverted = 0;
                // The lock overlay too, but not its parent (Root): Root's overrides are the
                // inventory card's own layout.
                var passiveC = so.FindProperty("passiveImage")?.objectReferenceValue as Component;
                if (passiveC != null)
                    foreach (var target in new Component[] { passiveC, passiveC.transform })
                        if (PrefabUtility.IsPartOfPrefabInstance(target))
                            PrefabUtility.RevertObjectOverride(target, InteractionMode.AutomatedAction);
                foreach (var c in new[] { gen, cost, nameT, descT })
                {
                    if (c == null) continue;
                    foreach (var target in new Component[] { c, c.transform, c.transform.parent })
                    {
                        if (target != null && PrefabUtility.IsPartOfPrefabInstance(target))
                        {
                            PrefabUtility.RevertObjectOverride(target, InteractionMode.AutomatedAction);
                            reverted++;
                        }
                    }
                }

                Debug.Log($"{Tag} {path}: {wired}/10 campos cableados; texto de lo que da bajo " +
                          $"'{gen?.transform.parent?.name}', texto del coste bajo '{cost?.transform.parent?.name}'; " +
                          $"{reverted} objetos de texto devueltos al CardCanvas.");
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(contents); }
        }

        // ── 2. Art ──────────────────────────────────────────────────────────────────────────
        private static readonly Dictionary<string, string> ArtByCard = new Dictionary<string, string>
        {
            // Ziggy · VOZ
            { "starter_rise_up", "Composicion/Voz/voz5.png" },
            { "starter_showtime", "Composicion/Voz/voz2.png" },
            { "starter_double_harmony", "Composicion/Voz/voz4.png" },
            // Conito · BAJO / GUITARRA
            { "starter_finger_bass", "Composicion/Guitarra/guitarra2.png" },
            { "conito_slap_groove", "Composicion/Guitarra/guitarra1.png" },
            { "conito_super_slap", "Composicion/Guitarra/guitarra3.png" },
            { "starter_slap_bass", "Composicion/Guitarra/guitarra4.png" },
            // Sibi · ÓRGANO
            { "starter_wormus_minor", "Composicion/Organo/organo2.png" },
            { "starter_wormus_major", "Composicion/Organo/organo3.png" },
            { "wormus_modal", "Composicion/Organo/organo1.png" },
            { "Starter_key_lift", "Composicion/Organo/organo4.png" },
            { "sibi_backing_modal", "Composicion/Organo/organo1.png" },
            { "wormus_bossa", "Composicion/Organo/organo4.png" },
            // C2 · BATERÍA (pose sobre fondo, montadas desde las piezas de Matías)
            { "starter_default_mode", "Composicion/Bateria/c2_default_mode.png" },
            { "starter_compound_cycle", "Composicion/Bateria/c2_compound_cycle.png" },
            { "Starter_push_it", "Composicion/Bateria/c2_push_it.png" },
            { "Starter_half_time", "Composicion/Bateria/c2_half_time.png" },
            { "starter_pentameter", "Composicion/Bateria/c2_pentameter.png" },
            { "starter_waltz_protocol", "Composicion/Bateria/c2_waltz_protocol.png" },
        };

        [MenuItem("ALWTTT/CARTAS-ARTE-1/2. Poner las ilustraciones de composición")]
        public static void AssignArt()
        {
            EnsureSingleSprites();
            const string folder = "Assets/Resources/Data/Characters/Musicians/";
            int ok = 0;
            foreach (var kv in ArtByCard)
            {
                var def = AssetDatabase.LoadAssetAtPath<CardDefinition>(folder + kv.Key + ".asset");
                var sprite = Load<Sprite>(ArtRoot + kv.Value);
                if (def == null || sprite == null)
                {
                    Debug.LogWarning($"{Tag} No encuentro {(def == null ? "la carta " + kv.Key : "el sprite " + kv.Value)}.");
                    continue;
                }
                var so = new SerializedObject(def);
                so.FindProperty("cardSprite").objectReferenceValue = sprite;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(def);
                ok++;
                Debug.Log($"{Tag} {def.Id} ← {kv.Value}");
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"{Tag} Ilustraciones puestas: {ok}/{ArtByCard.Count}.");
        }

        // ── helpers ─────────────────────────────────────────────────────────────────────────
        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

        // The project imports PNGs as Sprite (Multiple) auto-sliced by alpha islands, so a label
        // became just its first letter ("B" of BAJO) and frames/art could load as a fragment.
        // Force every card art texture to one whole sprite. Unity rewrites the importer settings.
        private static void EnsureSingleSprites()
        {
            int changed = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtRoot.TrimEnd('/') }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter ti)) continue;
                if (ti.textureType == TextureImporterType.Sprite && ti.spriteImportMode == SpriteImportMode.Single) continue;
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.alphaIsTransparency = true;
                ti.SaveAndReimport();
                changed++;
            }
            Debug.Log($"{Tag} Arte de cartas como sprite único: {changed} texturas cambiadas de Multiple a Single.");
        }

        private static Transform GetOrCreate(Transform parent, string name)
        {
            var t = parent.Find(name);
            if (t != null) return t;
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static void SetFrame(Transform t, Sprite s, Color tint)
        {
            Stretch((RectTransform)t);
            var img = t.GetComponent<Image>();
            if (img == null) return;
            img.sprite = s;
            img.type = Image.Type.Simple;
            img.preserveAspect = false;
            img.color = tint;
            img.raycastTarget = false;
        }

        private static void SetImage(Transform t, Sprite s)
        {
            var img = t.GetComponent<Image>();
            if (img == null) return;
            img.sprite = s;
            img.type = Image.Type.Simple;
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        private static void FitText(TextMeshProUGUI tmp, float min, float max, FontStyles style, TextAlignmentOptions align)
        {
            if (tmp == null) return;
            var rt = tmp.rectTransform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(1f, 0.5f); rt.offsetMax = new Vector2(-1f, -0.5f);
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = min;
            tmp.fontSizeMax = max;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
        }

        // The number sits to the right of its icon, left-aligned, never wrapping.
        private static void StyleNumber(TextMeshProUGUI tmp, float size, Color color, Material outline, Vector2 box)
        {
            if (tmp == null) return;
            var rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(1f, 0f);
            rt.sizeDelta = box;
            tmp.fontSize = size;
            tmp.enableAutoSizing = false;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;
            if (outline != null) tmp.fontSharedMaterial = outline;
        }

        private static int SetRef(SerializedObject so, string field, Object value)
        {
            var p = so.FindProperty(field);
            if (p == null || value == null) { Debug.LogWarning($"{Tag} No pude cablear {field}."); return 0; }
            p.objectReferenceValue = value;
            return 1;
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
