using System.Collections.Generic;
using System.IO;
using System.Linq;
using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Enums;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.DevMode.EditorTools
{
    /// <summary>
    /// [DEV-TEST-1] "ALWTTT ▸ Dev ▸ Create Test Cards". Creates four cards for
    /// testing — TEST Bass, TEST Rhythm, TEST Chords, TEST Melody — each with
    /// its OWN style bundle, so experiments (the Bass Workshop importer, the
    /// Inspector) never touch a real card. Each one is a copy of a starter
    /// card of the same role and musician, with cost 0 and no modifier
    /// effects (no tempo / tonality changes: the card only sets its track).
    ///
    /// Then lists them in GigDevSettings (testCards) and, if the mode is Off,
    /// sets it to GigStart. Shows what it will do and asks before writing.
    /// Idempotent: existing TEST assets are reused, never overwritten.
    /// Unity writes the .meta files.
    /// </summary>
    public static class DevTestCardsCreator
    {
        const string Folder = "Assets/Dev/TestCards";
        const string Bases = "Assets/Resources/Data/Characters/Musicians";

        struct Spec
        {
            public string key, display, baseCard;
            public MusicianCharacterType performer;
            public Spec(string key, string display, string baseCard, MusicianCharacterType performer)
            { this.key = key; this.display = display; this.baseCard = baseCard; this.performer = performer; }
        }

        static readonly Spec[] Specs =
        {
            new Spec("Bass",   "TEST Bass",   "starter_finger_bass",  MusicianCharacterType.Conito),
            new Spec("Rhythm", "TEST Rhythm", "starter_default_mode", MusicianCharacterType.Robot),
            new Spec("Chords", "TEST Chords", "starter_wormus_major", MusicianCharacterType.Gusano),
            new Spec("Melody", "TEST Melody", "starter_rise_up",      MusicianCharacterType.Cantante),
        };

        static string CardPath(Spec s) => $"{Folder}/TEST_{s.key}.asset";
        static string PayloadPath(Spec s) => $"{Folder}/TEST_{s.key}_Payload.asset";
        static string BundlePath(Spec s) => $"{Folder}/TEST_{s.key}_StyleBundle.asset";

        [MenuItem("ALWTTT/Dev/Create Test Cards", priority = 31)]
        public static void Create()
        {
            var settings = FindDevSettings();
            if (settings == null)
            {
                EditorUtility.DisplayDialog("Create Test Cards",
                    "No GigDevSettingsSO asset found in the project.", "OK");
                return;
            }

            // ---- normalize + preview ----
            var plan = new List<string>();
            var problems = new List<string>();
            foreach (var s in Specs)
            {
                if (AssetDatabase.LoadAssetAtPath<CardDefinition>(CardPath(s)) != null)
                {
                    plan.Add($"• {s.display}: already exists, reused.");
                    continue;
                }
                var baseCard = AssetDatabase.LoadAssetAtPath<CardDefinition>($"{Bases}/{s.baseCard}.asset");
                var basePayload = baseCard != null ? baseCard.CompositionPayload : null;
                var baseBundle = basePayload?.TrackAction?.styleBundle;
                if (baseCard == null || basePayload == null || baseBundle == null)
                {
                    problems.Add($"• {s.display}: base card '{s.baseCard}' (or its payload / style bundle) not found.");
                    continue;
                }
                plan.Add($"• {s.display}: new card + payload + style bundle, copied from " +
                         $"'{baseCard.DisplayName}' ({baseBundle.name}); played by {s.performer}.");
            }
            if (problems.Count > 0)
            {
                EditorUtility.DisplayDialog("Create Test Cards", "Cannot continue:\n\n" + string.Join("\n", problems), "OK");
                return;
            }

            var so = new SerializedObject(settings);
            var mode = so.FindProperty("testCardsMode");
            bool setMode = mode != null && mode.enumValueIndex == (int)GigDevSettingsSO.TestCardsMode.Off;

            string msg = $"Folder: {Folder}\n\n" + string.Join("\n", plan) +
                         $"\n\nGigDevSettings ('{settings.name}'): add the four to testCards" +
                         (setMode ? " and set the mode to GigStart." : " (mode stays as it is).") +
                         "\n\nNothing is written to the real cards.";
            if (!EditorUtility.DisplayDialog("Create Test Cards", msg, "Create", "Cancel")) return;

            // ---- apply ----
            if (!AssetDatabase.IsValidFolder("Assets/Dev")) AssetDatabase.CreateFolder("Assets", "Dev");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Dev", "TestCards");

            var cards = new List<CardDefinition>();
            foreach (var s in Specs)
            {
                var existing = AssetDatabase.LoadAssetAtPath<CardDefinition>(CardPath(s));
                cards.Add(existing != null ? existing : CreateOne(s));
            }

            var list = so.FindProperty("testCards");
            foreach (var c in cards)
            {
                if (c == null) continue;
                bool present = false;
                for (int i = 0; i < list.arraySize; i++)
                    if (list.GetArrayElementAtIndex(i).objectReferenceValue == c) { present = true; break; }
                if (present) continue;
                list.InsertArrayElementAtIndex(list.arraySize);
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = c;
            }
            if (setMode) mode.enumValueIndex = (int)GigDevSettingsSO.TestCardsMode.GigStart;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            Debug.Log($"[DEV-TEST-1] Test cards ready in {Folder}: " +
                      string.Join(", ", cards.Where(c => c != null).Select(c => c.DisplayName)) +
                      $". GigDevSettings mode = {(GigDevSettingsSO.TestCardsMode)mode.enumValueIndex}.");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(Folder);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        static CardDefinition CreateOne(Spec s)
        {
            var baseCard = AssetDatabase.LoadAssetAtPath<CardDefinition>($"{Bases}/{s.baseCard}.asset");
            var basePayload = baseCard.CompositionPayload;
            var baseBundle = basePayload.TrackAction.styleBundle;

            var bundle = Object.Instantiate(baseBundle);
            AssetDatabase.CreateAsset(bundle, BundlePath(s));

            var payload = Object.Instantiate(basePayload);
            AssetDatabase.CreateAsset(payload, PayloadPath(s));
            var ps = new SerializedObject(payload);
            ps.FindProperty("trackAction.styleBundle").objectReferenceValue = bundle;
            ps.FindProperty("modifierEffects").ClearArray(); // the card only sets its track
            ps.ApplyModifiedPropertiesWithoutUndo();

            var card = Object.Instantiate(baseCard);
            AssetDatabase.CreateAsset(card, CardPath(s));
            var cs = new SerializedObject(card);
            cs.FindProperty("id").stringValue = $"test_{s.key.ToLowerInvariant()}";
            cs.FindProperty("displayName").stringValue = s.display;
            cs.FindProperty("inspirationCost").intValue = 0;
            cs.FindProperty("performerRule").enumValueIndex = (int)CardPerformerRule.FixedMusicianType;
            cs.FindProperty("musicianCharacterType").enumValueIndex = (int)s.performer;
            cs.FindProperty("payload").objectReferenceValue = payload;
            cs.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(bundle);
            EditorUtility.SetDirty(payload);
            EditorUtility.SetDirty(card);
            return card;
        }

        static GigDevSettingsSO FindDevSettings()
        {
            var guid = AssetDatabase.FindAssets("t:GigDevSettingsSO").FirstOrDefault();
            return guid == null ? null : AssetDatabase.LoadAssetAtPath<GigDevSettingsSO>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}
