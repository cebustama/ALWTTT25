// S7 / ELENCO-1 (2026-11-11) — Audience JSON import / export.
// Repo path: Assets/Scripts/Characters/Editor/AudienceJson.cs
// Editor-only. Writes AudienceCharacterData through SerializedObject (same contract as
// AudienceMemberWizard, D-AMW-EDIT), so private fields stay private at runtime.
//
// One file format ("alwttt.audience/1") for three uses:
//   - Export: the selected AudienceCharacterData / GigEncounterSO assets → one .json (full, re-importable).
//   - Import: a .json → creates or updates data assets, prefab variants (sprite swap + scale),
//     glossary entries ES/EN, encounters, launch configs and the GigSetup roster.
//   - "Crear elenco S7": imports Assets/Settings/Audience/Elenco_S7.json.
//
// Matching rules: a character is matched by characterId (update in place, GUID kept);
// an encounter / launch config by asset path. Asset references are written as asset paths;
// on import a bare name is also accepted (searched by type). Unresolved references are
// logged as warnings and left null — never invented.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ALWTTT.Characters.Audience;
using ALWTTT.Data;
using ALWTTT.Encounters;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Characters.Editor
{
    public static class AudienceJson
    {
        public const string Format = "alwttt.audience/1";
        public const string CastS7Path = "Assets/Settings/Audience/Elenco_S7.json";
        private const string LogTag = "[S7]";
        private const string DefaultDataFolder = "Assets/Resources/Data/Characters/Audience";
        private const string DefaultPrefabFolder = "Assets/Prefabs/Characters/Audience";
        private const string DefaultBasePrefab = "Assets/Prefabs/Characters/Audience/AudienceCharacter - Base.prefab";
        private const string DefaultEncounterFolder = "Assets/Resources/Data/Encounters";
        private const string DefaultLaunchTemplate = "Assets/Settings/Gig/DemoLaunchConfig.asset";

        // ──────────────────────────────────────────────────────────────────
        // DTOs (JsonUtility: fields only, enums as strings)
        // ──────────────────────────────────────────────────────────────────
        [Serializable] public class TextJson { public string name = ""; public string description = ""; }

        [Serializable]
        public class ActionJson
        {
            public string type = "AddStress";      // CharacterActionType name
            public string target = "Musician";     // ActionTargetType name
            public float value;
            public float delay;
            public string status = "";             // StatusEffectSO path or name (ApplyStatusEffect only)
        }

        [Serializable]
        public class AbilityJson
        {
            public string id = "";                 // glossary id, 'ab_' prefix
            public string devName = "";            // AudienceAbilityData.abilityName (developer label)
            public string intention = "";          // AudienceIntentionData path or name
            public bool hideActionValue;
            public string duet = "";               // [S7] duetKey: same key + same musician = double Stress
            public float duration = 2f;
            public string animatorTrigger = "";
            public float animationDuration = -1f;
            public bool disableBeatAnimator = true;
            public string sfx = "";                // AudioClip path or name
            public List<ActionJson> actions = new();
            public TextJson es = new();
            public TextJson en = new();
        }

        [Serializable]
        public class TasteJson
        {
            public bool tempoMatchOnFast;
            public float preferAboveTempoScale = 1f;
            public bool tempoMismatchOnSlow;
            public float dislikeBelowTempoScale = 1f;
            public bool roleCountMatchOnRich;
            public int preferAtLeastRoles = 3;
            public List<string> preferredTimeSignatures = new();
            public List<string> dislikedTimeSignatures = new();
            public List<string> preferredTonalities = new();
            public List<string> dislikedTonalities = new();
            public List<string> preferredRoles = new();
            public List<string> dislikedRoles = new();
        }

        [Serializable]
        public class PrefabJson
        {
            public string path = "";               // prefab asset path; empty = keep current reference
            public string basePrefab = "";         // variant source when creating; empty = AudienceCharacter - Base
            public string sprite = "";             // empty = do not touch the sprite
            public float spriteHeight;             // height in prefab-root-parent units (Kid ≈ 1.8); 0 = keep scale
            public float headHeight = 0.75f;       // HeadRoot (FX anchor) as a fraction of the sprite height
            public bool updateExisting;            // true = re-apply sprite/scale to an existing prefab
        }

        [Serializable]
        public class CharacterJson
        {
            public string characterId = "";
            public string characterName = "";
            public string characterDescription = "";
            public string assetPath = "";          // where to create it if new; empty = default folder/characterName.asset
            public int maxVibe = 20;
            public bool isTall;
            public bool isLeader;
            public bool followAbilityPattern = true;
            public string sfxProfile = "";
            public PrefabJson prefab = new();
            public List<AbilityJson> abilities = new();
            public TasteJson taste = new();
        }

        [Serializable]
        public class LaunchJson
        {
            public string path = "";               // empty = no launch config
            public string copyFrom = "";           // template when creating; empty = DemoLaunchConfig
        }

        [Serializable]
        public class EncounterJson
        {
            public string path = "";
            public string displayName = "";
            public string venueType = "Venue1";
            public int numberOfSongs = 1;
            public int fansOnWin;
            public int cohesionPenaltyOnLoss;
            public List<string> audience = new();  // characterIds, in stage order
            public bool addToGigSetupRoster = true;
            public LaunchJson launchConfig = new();
        }

        [Serializable] public class ConceptJson { public string id = ""; public TextJson es = new(); public TextJson en = new(); }

        [Serializable]
        public class FileJson
        {
            public string format = Format;
            public string note = "";
            public List<ConceptJson> glossaryConcepts = new();
            public List<CharacterJson> characters = new();
            public List<EncounterJson> encounters = new();
        }

        // ──────────────────────────────────────────────────────────────────
        // Menu
        // ──────────────────────────────────────────────────────────────────
        [MenuItem("ALWTTT/S7/Crear elenco (ELENCO-1)", priority = 0)]
        public static void CreateCastS7()
        {
            if (!File.Exists(CastS7Path))
            {
                Debug.LogError($"{LogTag} Cast file not found: {CastS7Path}");
                return;
            }
            if (!EditorUtility.DisplayDialog("Crear elenco S7",
                    $"Importa {CastS7Path}:\ncrea o actualiza personajes, prefabs, textos ES/EN, " +
                    "encuentros y configs de lanzamiento.\n\nLos cambios quedan en git (reversibles).",
                    "Importar", "Cancelar"))
                return;
            ImportFile(CastS7Path);
        }

        [MenuItem("ALWTTT/Characters/Audience JSON/Import…")]
        public static void ImportWithDialog()
        {
            string path = EditorUtility.OpenFilePanel("Import audience JSON", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(path)) ImportFile(path);
        }

        [MenuItem("ALWTTT/Characters/Audience JSON/Export selected…")]
        [MenuItem("Assets/ALWTTT/Export audience JSON…")]
        public static void ExportSelectedWithDialog()
        {
            var chars = Selection.objects.OfType<AudienceCharacterData>().ToList();
            var encs = Selection.objects.OfType<GigEncounterSO>().ToList();
            if (chars.Count == 0 && encs.Count == 0)
            {
                EditorUtility.DisplayDialog("Export audience JSON",
                    "Selecciona en el Project uno o más AudienceCharacterData o GigEncounterSO.", "OK");
                return;
            }
            string suggested = chars.Count == 1 && encs.Count == 0 ? chars[0].name : (encs.Count == 1 ? encs[0].name : "Audience");
            string path = EditorUtility.SaveFilePanel("Export audience JSON", "", suggested + ".json", "json");
            if (string.IsNullOrEmpty(path)) return;
            Export(chars, encs, path);
            EditorUtility.RevealInFinder(path);
        }

        // ──────────────────────────────────────────────────────────────────
        // Export
        // ──────────────────────────────────────────────────────────────────
        public static void Export(IList<AudienceCharacterData> characters, IList<GigEncounterSO> encounters, string filePath)
        {
            var file = new FileJson { note = $"Exported {DateTime.Now:yyyy-MM-dd HH:mm}" };
            var gloss = LoadGlossaries();
            var all = new List<AudienceCharacterData>(characters ?? new List<AudienceCharacterData>());

            foreach (var e in encounters ?? new List<GigEncounterSO>())
            {
                if (e == null) continue;
                file.encounters.Add(ExportEncounter(e));
                foreach (var m in e.AudienceMemberList)
                    if (m != null && !all.Contains(m)) all.Add(m);
            }
            foreach (var c in all)
                if (c != null) file.characters.Add(ExportCharacter(c, gloss));

            File.WriteAllText(filePath, JsonUtility.ToJson(file, true));
            Debug.Log($"{LogTag} Audience JSON export | {file.characters.Count} character(s), " +
                      $"{file.encounters.Count} encounter(s) → {filePath}");
        }

        private static CharacterJson ExportCharacter(AudienceCharacterData data, Dictionary<string, ConceptGlossarySO> gloss)
        {
            var so = new SerializedObject(data);
            var j = new CharacterJson
            {
                characterId = so.FindProperty("characterId").stringValue,
                characterName = so.FindProperty("characterName").stringValue,
                characterDescription = so.FindProperty("characterDescription").stringValue,
                assetPath = AssetDatabase.GetAssetPath(data),
                maxVibe = so.FindProperty("maxVibe").intValue,
                isTall = so.FindProperty("isTall").boolValue,
                isLeader = so.FindProperty("isLeader").boolValue,
                followAbilityPattern = so.FindProperty("followAbilityPattern").boolValue,
                sfxProfile = PathOf(so.FindProperty("sfxProfile").objectReferenceValue),
            };

            var prefab = so.FindProperty("characterPrefab").objectReferenceValue as Component;
            if (prefab != null)
            {
                j.prefab.path = AssetDatabase.GetAssetPath(prefab);
                var sr = FindDeep(prefab.transform, "Sprite")?.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    j.prefab.sprite = AssetDatabase.GetAssetPath(sr.sprite);
                    j.prefab.spriteHeight = Round3(sr.sprite.bounds.size.y * Mathf.Abs(sr.transform.lossyScale.y));
                }
            }

            var list = so.FindProperty("abilityList");
            for (int i = 0; i < list.arraySize; i++)
            {
                var a = list.GetArrayElementAtIndex(i);
                var anim = a.FindPropertyRelative("animation");
                var aj = new AbilityJson
                {
                    id = a.FindPropertyRelative("abilityId").stringValue,
                    devName = a.FindPropertyRelative("abilityName").stringValue,
                    intention = PathOf(a.FindPropertyRelative("intention").objectReferenceValue),
                    hideActionValue = a.FindPropertyRelative("hideActionValue").boolValue,
                    duet = a.FindPropertyRelative("duetKey").stringValue,
                    duration = a.FindPropertyRelative("abilityDuration").floatValue,
                    animatorTrigger = anim.FindPropertyRelative("animatorTrigger").stringValue,
                    animationDuration = anim.FindPropertyRelative("animationDuration").floatValue,
                    disableBeatAnimator = anim.FindPropertyRelative("disableBeatAnimator").boolValue,
                    sfx = PathOf(a.FindPropertyRelative("abilitySfx").objectReferenceValue),
                };
                var acts = a.FindPropertyRelative("actionList");
                for (int k = 0; k < acts.arraySize; k++)
                {
                    var ac = acts.GetArrayElementAtIndex(k);
                    aj.actions.Add(new ActionJson
                    {
                        type = EnumName<Enums.CharacterActionType>(ac.FindPropertyRelative("cardActionType").intValue),
                        target = EnumName<Enums.ActionTargetType>(ac.FindPropertyRelative("actionTargetType").intValue),
                        value = ac.FindPropertyRelative("actionValue").floatValue,
                        delay = ac.FindPropertyRelative("actionDelay").floatValue,
                        status = PathOf(ac.FindPropertyRelative("statusEffect").objectReferenceValue),
                    });
                }
                if (gloss.TryGetValue("es", out var es) && es.TryGet(aj.id, out var ee)) aj.es = new TextJson { name = ee.DisplayName, description = ee.Description };
                if (gloss.TryGetValue("en", out var en) && en.TryGet(aj.id, out var ne)) aj.en = new TextJson { name = ne.DisplayName, description = ne.Description };
                j.abilities.Add(aj);
            }

            var t = so.FindProperty("taste");
            j.taste.tempoMatchOnFast = t.FindPropertyRelative("tempoMatchOnFast").boolValue;
            j.taste.preferAboveTempoScale = t.FindPropertyRelative("preferAboveTempoScale").floatValue;
            j.taste.tempoMismatchOnSlow = t.FindPropertyRelative("tempoMismatchOnSlow").boolValue;
            j.taste.dislikeBelowTempoScale = t.FindPropertyRelative("dislikeBelowTempoScale").floatValue;
            j.taste.roleCountMatchOnRich = t.FindPropertyRelative("roleCountMatchOnRich").boolValue;
            j.taste.preferAtLeastRoles = t.FindPropertyRelative("preferAtLeastRoles").intValue;
            j.taste.preferredTimeSignatures = ReadEnumList<MidiGenPlay.MusicTheory.MusicTheory.TimeSignature>(t, "preferredTimeSignatures");
            j.taste.dislikedTimeSignatures = ReadEnumList<MidiGenPlay.MusicTheory.MusicTheory.TimeSignature>(t, "dislikedTimeSignatures");
            j.taste.preferredTonalities = ReadEnumList<MidiGenPlay.MusicTheory.MusicTheory.Tonality>(t, "preferredTonalities");
            j.taste.dislikedTonalities = ReadEnumList<MidiGenPlay.MusicTheory.MusicTheory.Tonality>(t, "dislikedTonalities");
            j.taste.preferredRoles = ReadEnumList<MidiGenPlay.TrackRole>(t, "preferredRoles");
            j.taste.dislikedRoles = ReadEnumList<MidiGenPlay.TrackRole>(t, "dislikedRoles");
            return j;
        }

        private static EncounterJson ExportEncounter(GigEncounterSO e)
        {
            var so = new SerializedObject(e);
            var j = new EncounterJson
            {
                path = AssetDatabase.GetAssetPath(e),
                displayName = so.FindProperty("displayName").stringValue,
                venueType = EnumName<Enums.VenueType>(so.FindProperty("targetVenueType").intValue),
                numberOfSongs = so.FindProperty("numberOfSongs").intValue,
                fansOnWin = so.FindProperty("fansOnWin").intValue,
                cohesionPenaltyOnLoss = so.FindProperty("cohesionPenaltyOnLoss").intValue,
                addToGigSetupRoster = false,
            };
            foreach (var m in e.AudienceMemberList)
                if (m != null) j.audience.Add(new SerializedObject(m).FindProperty("characterId").stringValue);
            return j;
        }

        // ──────────────────────────────────────────────────────────────────
        // Import
        // ──────────────────────────────────────────────────────────────────
        private sealed class Report
        {
            public int created, updated, prefabsCreated, prefabsUpdated, texts, encounters, launches, roster, warnings;
            public void Warn(string msg) { warnings++; Debug.LogWarning($"{LogTag} {msg}"); }
        }

        public static void ImportFile(string filePath)
        {
            FileJson file;
            try { file = JsonUtility.FromJson<FileJson>(File.ReadAllText(filePath)); }
            catch (Exception ex) { Debug.LogError($"{LogTag} Invalid JSON in {filePath}: {ex.Message}"); return; }
            if (file == null) { Debug.LogError($"{LogTag} Empty JSON: {filePath}"); return; }
            if (!string.IsNullOrEmpty(file.format) && file.format != Format)
                Debug.LogWarning($"{LogTag} Unknown format '{file.format}' (expected '{Format}'); importing anyway.");

            var rep = new Report();
            var gloss = LoadGlossaries();

            foreach (var c in file.glossaryConcepts) WriteGlossary(gloss, c.id, c.es, c.en, rep);

            var byId = new Dictionary<string, AudienceCharacterData>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in file.characters)
            {
                var data = ImportCharacter(c, gloss, rep);
                if (data != null) byId[c.characterId] = data;
            }

            foreach (var e in file.encounters) ImportEncounter(e, byId, rep);

            foreach (var g in gloss.Values) EditorUtility.SetDirty(g);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"{LogTag} Audience JSON import | characters created={rep.created} updated={rep.updated} | " +
                      $"prefabs created={rep.prefabsCreated} updated={rep.prefabsUpdated} | texts={rep.texts} | " +
                      $"encounters={rep.encounters} launchConfigs={rep.launches} roster+={rep.roster} | " +
                      $"warnings={rep.warnings} | {Path.GetFileName(filePath)}");
        }

        private static AudienceCharacterData ImportCharacter(CharacterJson c, Dictionary<string, ConceptGlossarySO> gloss, Report rep)
        {
            if (string.IsNullOrWhiteSpace(c.characterId)) { rep.Warn("Character without characterId skipped."); return null; }

            var data = FindCharacter(c.characterId);
            if (data == null)
            {
                string path = !string.IsNullOrWhiteSpace(c.assetPath)
                    ? c.assetPath
                    : $"{DefaultDataFolder}/{(string.IsNullOrWhiteSpace(c.characterName) ? c.characterId : c.characterName)}.asset";
                EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
                data = ScriptableObject.CreateInstance<AudienceCharacterData>();
                AssetDatabase.CreateAsset(data, AssetDatabase.GenerateUniqueAssetPath(path));
                rep.created++;
            }
            else rep.updated++;

            var so = new SerializedObject(data);
            so.FindProperty("characterId").stringValue = c.characterId;
            so.FindProperty("characterName").stringValue = c.characterName;
            so.FindProperty("characterDescription").stringValue = c.characterDescription;
            so.FindProperty("maxVibe").intValue = c.maxVibe;
            so.FindProperty("isTall").boolValue = c.isTall;
            so.FindProperty("isLeader").boolValue = c.isLeader;
            so.FindProperty("followAbilityPattern").boolValue = c.followAbilityPattern;
            so.FindProperty("sfxProfile").objectReferenceValue = Resolve<CharacterSfxProfileSO>(c.sfxProfile, rep, c.characterId);

            var prefab = ImportPrefab(c, rep);
            if (prefab != null) so.FindProperty("characterPrefab").objectReferenceValue = prefab;

            var list = so.FindProperty("abilityList");
            list.arraySize = c.abilities.Count;
            for (int i = 0; i < c.abilities.Count; i++)
            {
                var aj = c.abilities[i];
                var a = list.GetArrayElementAtIndex(i);
                a.FindPropertyRelative("abilityName").stringValue = string.IsNullOrEmpty(aj.devName) ? aj.id : aj.devName;
                a.FindPropertyRelative("abilityId").stringValue = aj.id;
                a.FindPropertyRelative("intention").objectReferenceValue = Resolve<AudienceIntentionData>(aj.intention, rep, aj.id);
                a.FindPropertyRelative("hideActionValue").boolValue = aj.hideActionValue;
                a.FindPropertyRelative("duetKey").stringValue = aj.duet ?? "";
                a.FindPropertyRelative("abilityDuration").floatValue = aj.duration;
                var anim = a.FindPropertyRelative("animation");
                anim.FindPropertyRelative("animatorTrigger").stringValue = aj.animatorTrigger ?? "";
                anim.FindPropertyRelative("animationDuration").floatValue = aj.animationDuration;
                anim.FindPropertyRelative("disableBeatAnimator").boolValue = aj.disableBeatAnimator;
                a.FindPropertyRelative("abilitySfx").objectReferenceValue = Resolve<AudioClip>(aj.sfx, rep, aj.id);

                var acts = a.FindPropertyRelative("actionList");
                acts.arraySize = aj.actions.Count;
                for (int k = 0; k < aj.actions.Count; k++)
                {
                    var x = aj.actions[k];
                    var ac = acts.GetArrayElementAtIndex(k);
                    ac.FindPropertyRelative("cardActionType").intValue = ParseEnum<Enums.CharacterActionType>(x.type, rep, aj.id);
                    ac.FindPropertyRelative("actionTargetType").intValue = ParseEnum<Enums.ActionTargetType>(x.target, rep, aj.id);
                    ac.FindPropertyRelative("actionValue").floatValue = x.value;
                    ac.FindPropertyRelative("actionDelay").floatValue = x.delay;
                    ac.FindPropertyRelative("statusEffect").objectReferenceValue = Resolve<Status.StatusEffectSO>(x.status, rep, aj.id);
                }

                if (!string.IsNullOrWhiteSpace(aj.id)) WriteGlossary(gloss, aj.id, aj.es, aj.en, rep);
                else rep.Warn($"{c.characterId}: ability '{aj.devName}' has no id (COVERAGE error in GameTextWindow).");
            }

            var t = so.FindProperty("taste");
            t.FindPropertyRelative("tempoMatchOnFast").boolValue = c.taste.tempoMatchOnFast;
            t.FindPropertyRelative("preferAboveTempoScale").floatValue = c.taste.preferAboveTempoScale;
            t.FindPropertyRelative("tempoMismatchOnSlow").boolValue = c.taste.tempoMismatchOnSlow;
            t.FindPropertyRelative("dislikeBelowTempoScale").floatValue = c.taste.dislikeBelowTempoScale;
            t.FindPropertyRelative("roleCountMatchOnRich").boolValue = c.taste.roleCountMatchOnRich;
            t.FindPropertyRelative("preferAtLeastRoles").intValue = Mathf.Clamp(c.taste.preferAtLeastRoles, 1, 5);
            WriteEnumList<MidiGenPlay.MusicTheory.MusicTheory.TimeSignature>(t, "preferredTimeSignatures", c.taste.preferredTimeSignatures, rep, c.characterId);
            WriteEnumList<MidiGenPlay.MusicTheory.MusicTheory.TimeSignature>(t, "dislikedTimeSignatures", c.taste.dislikedTimeSignatures, rep, c.characterId);
            WriteEnumList<MidiGenPlay.MusicTheory.MusicTheory.Tonality>(t, "preferredTonalities", c.taste.preferredTonalities, rep, c.characterId);
            WriteEnumList<MidiGenPlay.MusicTheory.MusicTheory.Tonality>(t, "dislikedTonalities", c.taste.dislikedTonalities, rep, c.characterId);
            WriteEnumList<MidiGenPlay.TrackRole>(t, "preferredRoles", c.taste.preferredRoles, rep, c.characterId);
            WriteEnumList<MidiGenPlay.TrackRole>(t, "dislikedRoles", c.taste.dislikedRoles, rep, c.characterId);

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
            if (prefab != null && LinkPrefabToData(prefab, data)) rep.prefabsUpdated++;
            return data;
        }

        // [S7 fix] A variant of the base inherits the base's audienceCharacterData (Kid). GigManager
        // instantiates the PREFAB, so the clone used Kid's abilities whatever data pointed at the prefab.
        // The prefab must point back at its own data. Returns true if it had to change it.
        private static bool LinkPrefabToData(AudienceCharacterBase prefab, AudienceCharacterData data)
        {
            if (prefab.AudienceCharacterData == data) return false;
            string path = AssetDatabase.GetAssetPath(prefab.gameObject);
            if (string.IsNullOrEmpty(path)) return false;
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var comp = root.GetComponent<AudienceCharacterBase>();
                if (comp == null) return false;
                var pso = new SerializedObject(comp);
                pso.FindProperty("audienceCharacterData").objectReferenceValue = data;
                pso.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Debug.Log($"{LogTag} Prefab ↔ data | {Path.GetFileNameWithoutExtension(path)} → {data.name}");
            return true;
        }

        [MenuItem("ALWTTT/Characters/Audience JSON/Reparar enlaces prefab ↔ datos")]
        public static void RepairPrefabLinks()
        {
            int fixedCount = 0, shared = 0;
            var owners = new Dictionary<AudienceCharacterBase, AudienceCharacterData>();
            foreach (var guid in AssetDatabase.FindAssets("t:AudienceCharacterData"))
            {
                var data = AssetDatabase.LoadAssetAtPath<AudienceCharacterData>(AssetDatabase.GUIDToAssetPath(guid));
                var prefab = data != null ? data.CharacterPrefab : null;
                if (prefab == null) continue;
                if (owners.TryGetValue(prefab, out var other))
                {
                    shared++;
                    Debug.LogWarning($"{LogTag} '{prefab.name}' is the prefab of both '{other.name}' and '{data.name}' — not changed.");
                    continue;
                }
                owners[prefab] = data;
            }
            foreach (var kv in owners)
                if (LinkPrefabToData(kv.Key, kv.Value)) fixedCount++;
            AssetDatabase.SaveAssets();
            Debug.Log($"{LogTag} Prefab ↔ data repair | fixed={fixedCount} | checked={owners.Count} | shared={shared}");
        }

        // Prefab: create a variant of the base with the sprite swapped, or re-apply to an existing one.
        private static AudienceCharacterBase ImportPrefab(CharacterJson c, Report rep)
        {
            var p = c.prefab;
            if (p == null || string.IsNullOrWhiteSpace(p.path)) return null;

            Sprite sprite = null;
            if (!string.IsNullOrWhiteSpace(p.sprite))
            {
                sprite = LoadSprite(p.sprite);
                if (sprite == null) rep.Warn($"{c.characterId}: sprite not found '{p.sprite}'.");
            }

            bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(p.path) != null;
            if (!exists)
            {
                if (sprite == null)
                {
                    rep.Warn($"{c.characterId}: prefab '{p.path}' does not exist and no sprite to build it.");
                    return null;
                }
                string basePath = string.IsNullOrWhiteSpace(p.basePrefab) ? DefaultBasePrefab : p.basePrefab;
                var baseGo = AssetDatabase.LoadAssetAtPath<GameObject>(basePath);
                if (baseGo == null) { rep.Warn($"{c.characterId}: base prefab not found '{basePath}'."); return null; }

                EnsureFolder(Path.GetDirectoryName(p.path).Replace('\\', '/'));
                // Preview scene: the open scene is never touched (no dirty flag, no stray object).
                var scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
                try
                {
                    var inst = (GameObject)PrefabUtility.InstantiatePrefab(baseGo, scene);
                    inst.name = Path.GetFileNameWithoutExtension(p.path);
                    ApplySprite(inst, sprite, p.spriteHeight, p.headHeight, rep, c.characterId);
                    PrefabUtility.SaveAsPrefabAsset(inst, p.path);   // instance of base → saved as a variant
                }
                finally { UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene); }
                rep.prefabsCreated++;
                CharacterSizing.FitHeadUI(p.path);   // [S7] head UI to this body's height
            }
            else if (p.updateExisting && sprite != null)
            {
                var root = PrefabUtility.LoadPrefabContents(p.path);
                try
                {
                    ApplySprite(root, sprite, p.spriteHeight, p.headHeight, rep, c.characterId);
                    PrefabUtility.SaveAsPrefabAsset(root, p.path);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
                rep.prefabsUpdated++;
                CharacterSizing.FitHeadUI(p.path);
            }

            var go = AssetDatabase.LoadAssetAtPath<GameObject>(p.path);
            var comp = go != null ? go.GetComponent<AudienceCharacterBase>() : null;
            if (comp == null) rep.Warn($"{c.characterId}: '{p.path}' has no AudienceCharacterBase on its root.");
            return comp;
        }

        /// <summary>
        /// Swaps the sprite on the child "Sprite" and keeps the character standing on the same floor:
        /// the new sprite's bottom edge lands where the old one's was, centered on the same x. Children of
        /// "Sprite" (HeartEyes) keep their relative place on the body and their on-screen size. HeadRoot
        /// (FX anchor) moves to headHeight of the new body.
        /// </summary>
        private static void ApplySprite(GameObject root, Sprite sprite, float height, float headHeight, Report rep, string who)
        {
            var spriteTf = FindDeep(root.transform, "Sprite");
            var sr = spriteTf != null ? spriteTf.GetComponent<SpriteRenderer>() : null;
            if (sr == null) { rep.Warn($"{who}: prefab has no child 'Sprite' with a SpriteRenderer."); return; }

            Bounds ob = sr.sprite != null ? sr.sprite.bounds : new Bounds(Vector3.zero, Vector3.one);
            Vector3 os = spriteTf.localScale;
            Vector3 op = spriteTf.localPosition;
            float oldBottom = op.y + ob.min.y * os.y;
            float oldCenterX = op.x + ob.center.x * os.x;

            sr.sprite = sprite;
            Bounds nb = sprite.bounds;

            float s = Mathf.Abs(os.y);
            if (height > 0f && nb.size.y > 0f)
            {
                float parentScale = spriteTf.parent != null ? Mathf.Abs(spriteTf.parent.lossyScale.y) : 1f;
                s = height / (nb.size.y * Mathf.Max(parentScale, 1e-4f));
            }
            float sx = os.x < 0 ? -s : s;
            spriteTf.localScale = new Vector3(sx, s, os.z);
            spriteTf.localPosition = new Vector3(oldCenterX - nb.center.x * sx, oldBottom - nb.min.y * s, op.z);

            float ratio = Mathf.Abs(os.y) > 1e-4f ? Mathf.Abs(os.y) / s : 1f;
            foreach (Transform child in spriteTf)
            {
                Vector3 cp = child.localPosition;
                float nx = ob.size.x > 0 ? (cp.x - ob.min.x) / ob.size.x : 0.5f;
                float ny = ob.size.y > 0 ? (cp.y - ob.min.y) / ob.size.y : 0.5f;
                child.localPosition = new Vector3(nb.min.x + nx * nb.size.x, nb.min.y + ny * nb.size.y, cp.z);
                child.localScale *= ratio;
            }

            var head = FindDeep(root.transform, "HeadRoot");
            if (head != null && head.parent == root.transform && spriteTf.parent != null)
            {
                var spriteRoot = spriteTf.parent;
                float bottomRoot = spriteRoot.localPosition.y + (spriteTf.localPosition.y + nb.min.y * s) * spriteRoot.localScale.y;
                float heightRoot = nb.size.y * s * spriteRoot.localScale.y;
                var hp = head.localPosition;
                head.localPosition = new Vector3(hp.x, bottomRoot + Mathf.Clamp01(headHeight) * heightRoot, hp.z);
            }
        }

        private static void ImportEncounter(EncounterJson e, Dictionary<string, AudienceCharacterData> byId, Report rep)
        {
            string path = string.IsNullOrWhiteSpace(e.path) ? $"{DefaultEncounterFolder}/{e.displayName}.asset" : e.path;
            var enc = AssetDatabase.LoadAssetAtPath<GigEncounterSO>(path);
            if (enc == null)
            {
                EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
                enc = ScriptableObject.CreateInstance<GigEncounterSO>();
                AssetDatabase.CreateAsset(enc, path);
            }

            var so = new SerializedObject(enc);
            so.FindProperty("displayName").stringValue = e.displayName;
            so.FindProperty("targetVenueType").intValue = ParseEnum<Enums.VenueType>(e.venueType, rep, path);
            so.FindProperty("numberOfSongs").intValue = Mathf.Max(1, e.numberOfSongs);
            so.FindProperty("fansOnWin").intValue = e.fansOnWin;
            so.FindProperty("cohesionPenaltyOnLoss").intValue = e.cohesionPenaltyOnLoss;
            var list = so.FindProperty("audienceMemberList");
            var members = new List<AudienceCharacterData>();
            foreach (var id in e.audience)
            {
                var m = byId.TryGetValue(id, out var d) ? d : FindCharacter(id);
                if (m == null) rep.Warn($"{path}: audience id '{id}' not found.");
                else members.Add(m);
            }
            list.arraySize = members.Count;
            for (int i = 0; i < members.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = members[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(enc);
            rep.encounters++;

            if (e.addToGigSetupRoster) AddToRoster(enc, rep);

            if (e.launchConfig != null && !string.IsNullOrWhiteSpace(e.launchConfig.path))
            {
                var cfg = AssetDatabase.LoadAssetAtPath<DemoLaunchConfigSO>(e.launchConfig.path);
                if (cfg == null)
                {
                    string src = string.IsNullOrWhiteSpace(e.launchConfig.copyFrom) ? DefaultLaunchTemplate : e.launchConfig.copyFrom;
                    EnsureFolder(Path.GetDirectoryName(e.launchConfig.path).Replace('\\', '/'));
                    if (!AssetDatabase.CopyAsset(src, e.launchConfig.path)) { rep.Warn($"Could not copy '{src}' → '{e.launchConfig.path}'."); return; }
                    cfg = AssetDatabase.LoadAssetAtPath<DemoLaunchConfigSO>(e.launchConfig.path);
                }
                var cso = new SerializedObject(cfg);
                cso.FindProperty("encounter").objectReferenceValue = enc;
                cso.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(cfg);
                rep.launches++;
            }
        }

        private static void AddToRoster(GigEncounterSO enc, Report rep)
        {
            foreach (var g in AssetDatabase.FindAssets("t:GigSetupRosterSO"))
            {
                var roster = AssetDatabase.LoadAssetAtPath<GigSetupRosterSO>(AssetDatabase.GUIDToAssetPath(g));
                if (roster == null) continue;
                var so = new SerializedObject(roster);
                var list = so.FindProperty("availableEncounters");
                bool has = false;
                for (int i = 0; i < list.arraySize; i++)
                    if (list.GetArrayElementAtIndex(i).objectReferenceValue == enc) { has = true; break; }
                if (has) continue;
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = enc;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(roster);
                rep.roster++;
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Glossary (ES/EN), through SerializedObject — ConceptGlossarySO has no setters (TXT-1)
        // ──────────────────────────────────────────────────────────────────
        private static Dictionary<string, ConceptGlossarySO> LoadGlossaries()
        {
            var map = new Dictionary<string, ConceptGlossarySO>(StringComparer.OrdinalIgnoreCase);
            foreach (var g in AssetDatabase.FindAssets("t:ConceptGlossarySO"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<ConceptGlossarySO>(AssetDatabase.GUIDToAssetPath(g));
                if (asset == null || string.IsNullOrWhiteSpace(asset.LanguageCode)) continue;
                map[asset.LanguageCode.Trim().ToLowerInvariant()] = asset;
            }
            return map;
        }

        private static void WriteGlossary(Dictionary<string, ConceptGlossarySO> gloss, string id, TextJson es, TextJson en, Report rep)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            WriteEntry(gloss, "es", id, es, rep);
            WriteEntry(gloss, "en", id, en, rep);
        }

        private static void WriteEntry(Dictionary<string, ConceptGlossarySO> gloss, string lang, string id, TextJson text, Report rep)
        {
            if (text == null || (string.IsNullOrEmpty(text.name) && string.IsNullOrEmpty(text.description))) return;
            if (!gloss.TryGetValue(lang, out var g)) { rep.Warn($"No ConceptGlossarySO with languageCode '{lang}'."); return; }

            var so = new SerializedObject(g);
            var entries = so.FindProperty("entries");
            SerializedProperty el = null;
            for (int i = 0; i < entries.arraySize; i++)
            {
                var e = entries.GetArrayElementAtIndex(i);
                if (string.Equals(e.FindPropertyRelative("id").stringValue?.Trim(), id, StringComparison.OrdinalIgnoreCase)) { el = e; break; }
            }
            if (el == null)
            {
                entries.arraySize++;
                el = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                el.FindPropertyRelative("id").stringValue = id;
            }
            if (!string.IsNullOrEmpty(text.name)) el.FindPropertyRelative("displayName").stringValue = text.name;
            if (!string.IsNullOrEmpty(text.description)) el.FindPropertyRelative("description").stringValue = text.description;
            so.ApplyModifiedPropertiesWithoutUndo();
            g.BuildIndex();
            rep.texts++;
        }

        // ──────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────
        private static AudienceCharacterData FindCharacter(string characterId)
        {
            foreach (var g in AssetDatabase.FindAssets("t:AudienceCharacterData"))
            {
                var a = AssetDatabase.LoadAssetAtPath<AudienceCharacterData>(AssetDatabase.GUIDToAssetPath(g));
                if (a == null) continue;
                var id = new SerializedObject(a).FindProperty("characterId").stringValue;
                if (string.Equals(id, characterId, StringComparison.OrdinalIgnoreCase)) return a;
            }
            return null;
        }

        private static T Resolve<T>(string pathOrName, Report rep, string who) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(pathOrName)) return null;
            if (pathOrName.StartsWith("Assets/", StringComparison.Ordinal))
            {
                var a = AssetDatabase.LoadAssetAtPath<T>(pathOrName);
                if (a == null) rep.Warn($"{who}: {typeof(T).Name} not found at '{pathOrName}'.");
                return a;
            }
            foreach (var g in AssetDatabase.FindAssets($"{pathOrName} t:{typeof(T).Name}"))
            {
                var a = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g));
                if (a != null && string.Equals(a.name, pathOrName, StringComparison.OrdinalIgnoreCase)) return a;
            }
            rep.Warn($"{who}: no {typeof(T).Name} named '{pathOrName}'.");
            return null;
        }

        private static Sprite LoadSprite(string path)
        {
            var direct = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (direct != null) return direct;
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();   // spriteMode Multiple
        }

        private static string PathOf(UnityEngine.Object o) => o != null ? AssetDatabase.GetAssetPath(o) : "";

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

        private static string EnumName<T>(int value) where T : struct, Enum
            => Enum.IsDefined(typeof(T), value) ? Enum.GetName(typeof(T), value) : value.ToString();

        private static int ParseEnum<T>(string s, Report rep, string who) where T : struct, Enum
        {
            if (Enum.TryParse<T>(s, true, out var v)) return Convert.ToInt32(v);
            rep.Warn($"{who}: '{s}' is not a {typeof(T).Name}; using {default(T)}.");
            return Convert.ToInt32(default(T));
        }

        private static List<string> ReadEnumList<T>(SerializedProperty parent, string name) where T : struct, Enum
        {
            var res = new List<string>();
            var p = parent.FindPropertyRelative(name);
            if (p == null) return res;
            for (int i = 0; i < p.arraySize; i++) res.Add(EnumName<T>(p.GetArrayElementAtIndex(i).intValue));
            return res;
        }

        private static void WriteEnumList<T>(SerializedProperty parent, string name, List<string> values, Report rep, string who) where T : struct, Enum
        {
            var p = parent.FindPropertyRelative(name);
            if (p == null) { rep.Warn($"{who}: taste field '{name}' not found."); return; }
            values ??= new List<string>();
            p.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++) p.GetArrayElementAtIndex(i).intValue = ParseEnum<T>(values[i], rep, who);
        }

        private static float Round3(float v) => Mathf.Round(v * 1000f) / 1000f;

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath)) return;
            var parts = folderPath.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
