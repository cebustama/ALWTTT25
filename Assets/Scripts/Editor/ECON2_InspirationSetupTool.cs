#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ALWTTT.Cards;
using ALWTTT.Cards.Effects;
using ALWTTT.Data;
using ALWTTT.Enums;
using MidiGenPlay.Composition;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [ECON-2] Prepara los datos de la Inspiración con un clic. Idempotente: se puede
    /// pasar dos veces y la segunda no cambia nada (lo dice el informe).
    ///
    ///  1. ★1 en las composiciones de los cuatro catálogos de músicos (D1: «1 para este
    ///     primer mazo», editable carta a carta).
    ///  2. Overload (Conito) y Psychic Waves (Sibi) pasan a Finisher de composición, ★3.
    ///     Mismo asset de carta (los catálogos y el tutorial siguen apuntando bien); el
    ///     payload de acción se cambia por uno de composición con los mismos efectos,
    ///     sin el GrantBonusLoop (el Finisher ya concede su loop). Overload deja de
    ///     costar Voltaje.
    ///  3. Finisher nuevos para Zig («Gran Final») y C2 («Redoble Final»), ★3, en sus
    ///     mazos iniciales.
    ///  4. Leading «hipnótico» para el solo de Sibi (ScaleOnly, pasos cortos, muchas
    ///     notas repetidas).
    ///  5. GigFlowSettings ▸ ECON-2: máximo de Inspiración 10.
    ///
    /// complexityMultiplier = 0 ya va en GigMeterTuning.asset (cambio de datos directo).
    /// </summary>
    public static class ECON2InspirationSetupTool
    {
        private const string Tag = "[ECON-2]";
        private const string MusiciansDir = "Assets/Resources/Data/Characters/Musicians";
        private const string LeadingPath = "Assets/Resources/Data/Melody Configs/MelodicLeading-Hipnosis.asset";
        private const string FlowPath = "Assets/Settings/Gig/GigFlowSettings.asset";
        private const int FinisherCost = 3;
        private const int MaxInspiration = 10;

        [MenuItem("ALWTTT/ECON-2/Preparar datos de la Inspiración y los Finisher")]
        public static void Run()
        {
            var sb = new StringBuilder($"{Tag} Preparar datos\n");

            var catalogs = AssetDatabase.FindAssets("t:MusicianCardCatalogData", new[] { MusiciansDir })
                .Select(g => AssetDatabase.LoadAssetAtPath<MusicianCardCatalogData>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(c => c != null).ToList();
            sb.AppendLine($"Catálogos: {catalogs.Count} ({string.Join(", ", catalogs.Select(c => c.name))})");

            var leading = EnsureHypnoticLeading(sb);

            // 2) Conversiones
            ConvertToFinisher("conito_overload", null, MelodyStrategyId.ScaleFlow, sb);
            ConvertToFinisher("Starter_psychic_waves", leading, MelodyStrategyId.ScaleFlow, sb);

            // 3) Nuevos
            var zig = EnsureNewFinisher("zig_gran_final", "Gran Final", MusicianCharacterType.Cantante,
                spriteFromCardId: "starter_rise_up", vibe: 4, sb);
            var c2 = EnsureNewFinisher("c2_redoble_final", "Redoble Final", MusicianCharacterType.Robot,
                spriteFromCardId: "starter_default_mode", vibe: 3, sb);
            AddToStarter(catalogs, MusicianCharacterType.Cantante, zig, sb);
            AddToStarter(catalogs, MusicianCharacterType.Robot, c2, sb);

            // 1) ★1 en composiciones (después de las conversiones: los Finisher se saltan)
            int set = 0, already = 0;
            foreach (var cat in catalogs)
                foreach (var e in cat.Entries)
                {
                    var def = e?.card;
                    if (def == null || !def.IsComposition || def.IsFinisher) continue;
                    if (def.InspirationGenerated > 0) { already++; continue; }
                    var so = new SerializedObject(def);
                    so.FindProperty("inspirationGenerated").intValue = 1;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(def);
                    set++;
                    sb.AppendLine($"  ★1 → {def.Id} ({cat.name})");
                }
            sb.AppendLine($"★1: {set} cartas cambiadas, {already} ya tenían ★.");

            // 5) Máximo de Inspiración en las reglas del asset
            var flow = AssetDatabase.LoadAssetAtPath<GigFlowSettingsSO>(FlowPath);
            if (flow != null)
            {
                var so = new SerializedObject(flow);
                var p = so.FindProperty("inspirationRules.maxInspirationOverride");
                if (p != null && p.intValue != MaxInspiration)
                {
                    p.intValue = MaxInspiration;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(flow);
                    sb.AppendLine($"GigFlowSettings ▸ ECON-2: máximo de Inspiración = {MaxInspiration}.");
                }
                else sb.AppendLine($"GigFlowSettings ▸ ECON-2: máximo ya en {p?.intValue}.");
            }
            else sb.AppendLine($"⚠ No encontré {FlowPath}.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(sb.ToString());
        }

        // ─── Conversión de una carta existente ───────────────────────────
        private static void ConvertToFinisher(string cardId, MelodicLeadingConfig leading,
            MelodyStrategyId strategy, StringBuilder sb)
        {
            var def = FindCard(cardId);
            if (def == null) { sb.AppendLine($"⚠ No encontré la carta '{cardId}'."); return; }

            var so = new SerializedObject(def);
            bool alreadyComp = def.Payload is CompositionCardPayload;

            if (!alreadyComp)
            {
                var oldPayload = def.Payload;
                string defPath = AssetDatabase.GetAssetPath(def);
                string dir = System.IO.Path.GetDirectoryName(defPath).Replace('\\', '/');
                string newPath = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{def.name}_FinisherPayload.asset");

                var newPayload = ScriptableObject.CreateInstance<CompositionCardPayload>();
                AssetDatabase.CreateAsset(newPayload, newPath);

                var pso = new SerializedObject(newPayload);
                var effects = pso.FindProperty("effects");
                int kept = 0;
                if (oldPayload != null && oldPayload.Effects != null)
                    foreach (var fx in oldPayload.Effects)
                    {
                        if (fx == null || fx is GrantBonusLoopSpec) continue;
                        var clone = (CardEffectSpec)Activator.CreateInstance(fx.GetType());
                        EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(fx), clone);
                        effects.arraySize++;
                        effects.GetArrayElementAtIndex(effects.arraySize - 1).managedReferenceValue = clone;
                        kept++;
                    }
                pso.FindProperty("primaryKind").intValue = (int)CardPrimaryKind.None;
                pso.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(newPayload);

                so.FindProperty("payload").objectReferenceValue = newPayload;
                so.ApplyModifiedPropertiesWithoutUndo();

                if (oldPayload != null)
                {
                    string oldPath = AssetDatabase.GetAssetPath(oldPayload);
                    if (!string.IsNullOrEmpty(oldPath) && oldPath != defPath)
                        AssetDatabase.DeleteAsset(oldPath);
                }
                sb.AppendLine($"{cardId}: payload de acción → composición ({kept} efecto(s) copiados, " +
                              "sin GrantBonusLoop).");
                so.Update();
            }

            so.FindProperty("isFinisher").boolValue = true;
            so.FindProperty("inspirationCost").intValue = FinisherCost;
            so.FindProperty("inspirationGenerated").intValue = 0;
            so.FindProperty("resourceCostStatusKey").stringValue = "";
            so.FindProperty("resourceCostAmount").intValue = 0;
            so.FindProperty("finisherSoloStrategy").intValue = (int)strategy;
            if (leading != null) so.FindProperty("finisherSoloLeading").objectReferenceValue = leading;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(def);
            sb.AppendLine($"{cardId}: Finisher ★{FinisherCost}{(leading != null ? $", solo con '{leading.name}'" : "")}" +
                          $"{(alreadyComp ? " (ya era composición)" : "")}.");
        }

        // ─── Carta nueva ─────────────────────────────────────────────────
        private static CardDefinition EnsureNewFinisher(string id, string displayName,
            MusicianCharacterType musician, string spriteFromCardId, int vibe, StringBuilder sb)
        {
            var existing = FindCard(id);
            if (existing != null) { sb.AppendLine($"{id}: ya existe."); return existing; }

            var payload = ScriptableObject.CreateInstance<CompositionCardPayload>();
            AssetDatabase.CreateAsset(payload, $"{MusiciansDir}/{id}_Payload.asset");
            var pso = new SerializedObject(payload);
            var effects = pso.FindProperty("effects");
            effects.arraySize = 1;
            effects.GetArrayElementAtIndex(0).managedReferenceValue = new ModifyVibeSpec
            {
                targetType = ActionTargetType.AllAudienceCharacters,
                amount = vibe,
            };
            pso.FindProperty("primaryKind").intValue = (int)CardPrimaryKind.None;
            pso.ApplyModifiedPropertiesWithoutUndo();

            var def = ScriptableObject.CreateInstance<CardDefinition>();
            AssetDatabase.CreateAsset(def, $"{MusiciansDir}/{id}.asset");
            var so = new SerializedObject(def);
            so.FindProperty("id").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("performerRule").intValue = (int)CardPerformerRule.FixedMusicianType;
            so.FindProperty("musicianCharacterType").intValue = (int)musician;
            var spriteSrc = FindCard(spriteFromCardId);
            if (spriteSrc != null) so.FindProperty("cardSprite").objectReferenceValue = spriteSrc.CardSprite;
            so.FindProperty("inspirationCost").intValue = FinisherCost;
            so.FindProperty("inspirationGenerated").intValue = 0;
            so.FindProperty("isFinisher").boolValue = true;
            so.FindProperty("finisherSoloStrategy").intValue = (int)MelodyStrategyId.ScaleFlow;
            so.FindProperty("payload").objectReferenceValue = payload;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(def);
            EditorUtility.SetDirty(payload);
            sb.AppendLine($"{id}: Finisher nuevo «{displayName}» ({musician}), ★{FinisherCost}, Vibe {vibe} a todo el público.");
            return def;
        }

        private static void AddToStarter(List<MusicianCardCatalogData> catalogs,
            MusicianCharacterType musician, CardDefinition def, StringBuilder sb)
        {
            if (def == null) return;
            var cat = catalogs.FirstOrDefault(c => c.MusicianType == musician);
            if (cat == null) { sb.AppendLine($"⚠ Sin catálogo para {musician}."); return; }
            if (cat.Entries.Any(e => e != null && e.card == def))
            { sb.AppendLine($"{def.Id}: ya está en {cat.name}."); return; }

            var so = new SerializedObject(cat);
            var entries = so.FindProperty("entries");
            entries.arraySize++;
            var el = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            el.FindPropertyRelative("card").objectReferenceValue = def;
            el.FindPropertyRelative("flags").intValue =
                (int)(CardAcquisitionFlags.StarterDeck | CardAcquisitionFlags.UnlockedByDefault);
            el.FindPropertyRelative("unlockId").stringValue = "";
            el.FindPropertyRelative("starterCopies").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(cat);
            sb.AppendLine($"{def.Id}: añadida al mazo inicial de {cat.name} (x1).");
        }

        private static MelodicLeadingConfig EnsureHypnoticLeading(StringBuilder sb)
        {
            var lead = AssetDatabase.LoadAssetAtPath<MelodicLeadingConfig>(LeadingPath);
            if (lead != null) { sb.AppendLine("Leading hipnótico: ya existe."); return lead; }
            lead = ScriptableObject.CreateInstance<MelodicLeadingConfig>();
            lead.noteSource = MelodicLeadingConfig.NoteSource.ScaleOnly;
            lead.maxStepSemitones = 2;
            lead.chanceRepeatNote = 0.45f;
            lead.normalVelMin = 70; lead.normalVelMax = 88;
            lead.accentVelMin = 92; lead.accentVelMax = 105;
            AssetDatabase.CreateAsset(lead, LeadingPath);
            sb.AppendLine($"Leading hipnótico creado: {LeadingPath} (ScaleOnly, pasos ≤2 st, repetir 45 %).");
            return lead;
        }

        private static CardDefinition FindCard(string id)
        {
            foreach (var g in AssetDatabase.FindAssets("t:CardDefinition"))
            {
                var d = AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g));
                if (d != null && string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase)) return d;
            }
            return null;
        }
    }
}
#endif
