#if UNITY_EDITOR
using ALWTTT.Enums;
using ALWTTT.Musicians;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// Editor-only asset creation for CardDefinition + correct payload asset, 
    /// with default wiring.
    /// Keeps "creation logic" out of the EditorWindow.
    /// </summary>
    public static class CardAssetFactory
    {
        public enum CreateCardKind
        {
            Action = 0,
            Composition = 1
        }

        public sealed class CreateCardRequest
        {
            public MusicianCharacterType musicianType;
            public MusicianCharacterData musicianData;  // for sprite defaults, etc.
            public MusicianCardCatalogData targetCatalog;

            public CreateCardKind kind;

            public string id;
            public string displayName;

            public int inspirationCost = 1;
            public int inspirationGenerated = 0;

            /// <summary>Folder where assets should be created. 
            /// If null/empty, derived from catalog location.</summary>
            public string targetFolder;
        }

        public sealed class CreateCardResult
        {
            public CardDefinition cardDefinition;
            public CardPayload payload;
            public string definitionPath;
            public string payloadPath;
        }

        public static bool TryCreateCard(
            CreateCardRequest req, out CreateCardResult result, out string error)
        {
            result = null;
            error = null;

            if (req == null) 
            { error = "Request is null."; return false; }

            if (req.musicianType == MusicianCharacterType.None) 
            { error = "Musician is None."; return false; }

            if (string.IsNullOrWhiteSpace(req.id)) 
            { error = "Card Id is empty."; return false; }

            // Derive folder from catalog if needed.
            string folder = req.targetFolder;
            if (string.IsNullOrWhiteSpace(folder))
                folder = DeriveDefaultFolder(req.targetCatalog, req.musicianType);

            EnsureFolderExists(folder);
            string payloadFolder = Path.Combine(folder, "Payloads");
            EnsureFolderExists(payloadFolder);

            // Create payload asset
            CardPayload payload = req.kind == CreateCardKind.Action
                ? ScriptableObject.CreateInstance<ActionCardPayload>()
                : ScriptableObject.CreateInstance<CompositionCardPayload>();

            string safeBaseName = MakeSafeFileName(req.id);
            string payloadAssetName = $"{safeBaseName}_{req.kind}Payload.asset";
            string payloadPath = 
                AssetDatabase.GenerateUniqueAssetPath(
                    Path.Combine(payloadFolder, payloadAssetName));
            AssetDatabase.CreateAsset(payload, payloadPath);

            // Create definition asset
            var def = ScriptableObject.CreateInstance<CardDefinition>();
            string defAssetName = $"{safeBaseName}_Card.asset";
            string defPath = 
                AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, defAssetName));
            AssetDatabase.CreateAsset(def, defPath);

            try
            {
                var defSO = new SerializedObject(def);

                SetString(defSO, "id", req.id);
                SetString(defSO, "displayName", string.IsNullOrWhiteSpace(req.displayName) ?
                    req.id : req.displayName);

                SetEnum(defSO, "performerRule", CardPerformerRule.FixedMusicianType);
                SetEnum(defSO, "musicianCharacterType", req.musicianType);

                var sprite = TryGetMusicianSprite(req.musicianData);
                SetObject(defSO, "cardSprite", sprite);

                SetInt(defSO, "inspirationCost", req.inspirationCost);
                SetInt(defSO, "inspirationGenerated", req.inspirationGenerated);

                SetObject(defSO, "payload", payload);

                defSO.ApplyModifiedPropertiesWithoutUndo();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                error =
                    $"[{nameof(CardAssetFactory)}] " +
                    $"Failed to wire CardDefinition via SerializedObject.\n" +
                    $"Likely a renamed/removed [SerializeField] in CardDefinition.\n" +
                    $"Details: {ex.Message}";
                return false;
            }

            EditorUtility.SetDirty(def);
            EditorUtility.SetDirty(payload);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            result = new CreateCardResult
            {
                cardDefinition = def,
                payload = payload,
                definitionPath = defPath,
                payloadPath = payloadPath
            };

            return true;
        }

        private static Sprite TryGetMusicianSprite(MusicianCharacterData musicianData)
        {
            return musicianData != null ? musicianData.DefaultCardSprite : null;
        }

        private static string DeriveDefaultFolder(
            MusicianCardCatalogData catalog, MusicianCharacterType musicianType)
        {
            string baseFolder = "Assets";

            if (catalog != null)
            {
                var path = AssetDatabase.GetAssetPath(catalog);
                if (!string.IsNullOrEmpty(path))
                {
                    var dir = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(dir))
                        baseFolder = dir;
                }
            }

            // Put cards in a stable place near the catalog
            return Path.Combine(baseFolder, $"{musicianType}_Cards");
        }

        private static void EnsureFolderExists(string folderPath)
        {
            folderPath = folderPath.Replace("\\", "/");
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            // Create nested folders gradually
            string[] parts = folderPath.Split('/');
            if (parts.Length == 0) return;

            string current = parts[0]; // "Assets"
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string MakeSafeFileName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "NewCard";
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return s.Trim();
        }

        private static SerializedProperty RequireProp(SerializedObject so, string fieldName)
        {
            var p = so.FindProperty(fieldName);
            if (p == null)
            {
                throw new InvalidOperationException(
                    $"Missing serialized field '{fieldName}' " +
                    $"on '{so.targetObject.GetType().Name}'. " +
                    $"Was it renamed/removed?");
            }
            return p;
        }

        private static void SetString(SerializedObject so, string fieldName, string value)
            => RequireProp(so, fieldName).stringValue = value;

        private static void SetInt(SerializedObject so, string fieldName, int value)
            => RequireProp(so, fieldName).intValue = value;

        private static void SetEnum<T>(SerializedObject so, string fieldName, T value) 
            where T : Enum
            => RequireProp(so, fieldName).enumValueIndex = Convert.ToInt32(value);

        private static void SetObject(
            SerializedObject so, string fieldName, UnityEngine.Object value)
            => RequireProp(so, fieldName).objectReferenceValue = value;
    }
}
#endif