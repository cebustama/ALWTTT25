#if UNITY_EDITOR
using System.Collections.Generic;
using ALWTTT.Cards;
using ALWTTT.Cards.Effects;
using ALWTTT.Enums;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [S9c / DEMO-FIX-1] "ALWTTT ▸ S9c ▸ Aplicar datos de la demo". One pass, shown
    /// before it writes, idempotent:
    ///   1. Creates Calm Down (crd_sibi_calm_down): Sibi's action card that removes
    ///      4 Stress from the musician it is dropped on (D1=B). Sprite borrowed from
    ///      Read the Room until CARTAS-ARTE-1. Unity writes the .meta files.
    ///   2. Sibi's starter deck: Read the Room out (flags 5 -> 4, still unlocked),
    ///      Calm Down in (flags 5, 1 copy).
    ///   3. Zig's starter deck: Double Harmony out (flags 5 -> 4). The two-voice
    ///      singing it depends on is POST-DEMO B3.
    /// The card names (EN "Calm Down", ES "Cálmate") live in the glossaries.
    /// </summary>
    public static class S9cDemoFixDataTool
    {
        const string Dir = "Assets/Resources/Data/Characters/Musicians";
        const string ReadTheRoom = Dir + "/crd_sibi_read_the_room.asset";
        const string CalmDown = Dir + "/crd_sibi_calm_down.asset";
        const string CalmDownPayload = Dir + "/crd_sibi_calm_down_Payload.asset";
        const string DoubleHarmony = Dir + "/starter_double_harmony.asset";
        const string SibiCatalog = Dir + "/Sibi_CardCatalogData.asset";
        const string ZigCatalog = Dir + "/Cantante_CardCatalogData.asset";

        const int HealAmount = 4;
        const int StarterUnlocked = (int)(CardAcquisitionFlags.StarterDeck | CardAcquisitionFlags.UnlockedByDefault); // 5
        const int UnlockedOnly = (int)CardAcquisitionFlags.UnlockedByDefault;                                          // 4

        [MenuItem("ALWTTT/S9c/Aplicar datos de la demo (DEMO-FIX-1)", priority = 0)]
        public static void Apply()
        {
            var rtr = AssetDatabase.LoadAssetAtPath<CardDefinition>(ReadTheRoom);
            var dh = AssetDatabase.LoadAssetAtPath<CardDefinition>(DoubleHarmony);
            var sibi = AssetDatabase.LoadAssetAtPath<MusicianCardCatalogData>(SibiCatalog);
            var zig = AssetDatabase.LoadAssetAtPath<MusicianCardCatalogData>(ZigCatalog);
            if (rtr == null || dh == null || sibi == null || zig == null)
            {
                EditorUtility.DisplayDialog("S9c", "Falta alguno de estos assets:\n" +
                    $"{ReadTheRoom}\n{DoubleHarmony}\n{SibiCatalog}\n{ZigCatalog}", "OK");
                return;
            }

            var existing = AssetDatabase.LoadAssetAtPath<CardDefinition>(CalmDown);
            string msg =
                (existing == null
                    ? $"• Crear Calm Down: carta de acción de Sibi, quita {HealAmount} de Stress al músico sobre el que la sueltas.\n"
                    : "• Calm Down ya existe: se reutiliza.\n") +
                "• Mazo de Sibi: sale Read the Room (sigue desbloqueada), entra Calm Down ×1.\n" +
                "• Mazo de Zig: sale Doble Armonía (sigue desbloqueada).";
            if (!EditorUtility.DisplayDialog("S9c · DEMO-FIX-1", msg, "Aplicar", "Cancelar")) return;

            var card = existing != null ? existing : CreateCalmDown(rtr);

            var log = new List<string>();
            log.Add(SetFlags(sibi, rtr, UnlockedOnly, "Read the Room"));
            log.Add(EnsureEntry(sibi, card, StarterUnlocked, 1, "Calm Down"));
            log.Add(SetFlags(zig, dh, UnlockedOnly, "Doble Armonía"));

            AssetDatabase.SaveAssets();
            Debug.Log("[S9c] Datos de la demo aplicados: " + string.Join(" · ", log));
            Selection.activeObject = card;
            EditorGUIUtility.PingObject(card);
        }

        static CardDefinition CreateCalmDown(CardDefinition template)
        {
            var payload = ScriptableObject.CreateInstance<ActionCardPayload>();
            AssetDatabase.CreateAsset(payload, CalmDownPayload);
            var ps = new SerializedObject(payload);
            var effects = ps.FindProperty("effects");
            effects.ClearArray();
            effects.InsertArrayElementAtIndex(0);
            effects.GetArrayElementAtIndex(0).managedReferenceValue =
                new ModifyStressSpec { targetType = ActionTargetType.Musician, amount = -HealAmount };
            ps.ApplyModifiedPropertiesWithoutUndo();

            var card = Object.Instantiate(template); // keeps Sibi as performer, sprite, CHR type, cost 0
            AssetDatabase.CreateAsset(card, CalmDown);
            var cs = new SerializedObject(card);
            cs.FindProperty("id").stringValue = "crd_sibi_calm_down";
            cs.FindProperty("displayName").stringValue = "Calm Down";
            cs.FindProperty("payload").objectReferenceValue = payload;
            cs.FindProperty("overrideRequiresTargetSelection").boolValue = false; // target comes from the spec
            cs.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(payload);
            EditorUtility.SetDirty(card);
            Debug.Log($"[S9c] Calm Down creada en {CalmDown} (ModifyStress {-HealAmount} a un músico).");
            return card;
        }

        static string SetFlags(MusicianCardCatalogData catalog, CardDefinition card, int flags, string label)
        {
            var so = new SerializedObject(catalog);
            var entries = so.FindProperty("entries");
            for (int i = 0; i < entries.arraySize; i++)
            {
                var e = entries.GetArrayElementAtIndex(i);
                if (e.FindPropertyRelative("card").objectReferenceValue != card) continue;
                var f = e.FindPropertyRelative("flags");
                int before = f.intValue;
                f.intValue = flags;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(catalog);
                return $"{label} en {catalog.name}: flags {before} → {flags}";
            }
            return $"{label} no está en {catalog.name} (nada que cambiar)";
        }

        static string EnsureEntry(MusicianCardCatalogData catalog, CardDefinition card, int flags, int copies, string label)
        {
            var so = new SerializedObject(catalog);
            var entries = so.FindProperty("entries");
            for (int i = 0; i < entries.arraySize; i++)
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("card").objectReferenceValue == card)
                    return SetFlags(catalog, card, flags, label);

            entries.InsertArrayElementAtIndex(entries.arraySize);
            var e = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            e.FindPropertyRelative("card").objectReferenceValue = card;
            e.FindPropertyRelative("flags").intValue = flags;
            e.FindPropertyRelative("unlockId").stringValue = string.Empty;
            e.FindPropertyRelative("starterCopies").intValue = copies;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(catalog);
            return $"{label} añadida a {catalog.name} (flags {flags}, ×{copies})";
        }
    }
}
#endif
