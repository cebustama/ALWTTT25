#if UNITY_EDITOR
using ALWTTT.Data;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// Saves a StagedDeck to a BandDeckData asset.
    ///
    /// Save As default folder resolution order:
    ///   1. Folder of the staged deck's source asset (if already loaded from disk).
    ///   2. Folder of the first BandDeckData found in the project (via AssetDatabase search).
    ///   3. "Assets" as last resort.
    ///
    /// Pending new cards are saved first (via DeckCardCreationService, which routes
    /// each card to its musician-specific folder). Then the BandDeckData asset is
    /// written with the now-resolved asset references.
    /// </summary>
    internal static class DeckAssetSaveService
    {
        // ------------------------------------------------------------------
        // Save As
        // ------------------------------------------------------------------

        public static SaveResult SaveAs(StagedDeck staged, string preferredFolder = null)
        {
            if (staged == null) return Fail("StagedDeck is null.");

            // Resolve the best default folder for the save dialog
            string defaultFolder = ResolveDefaultDeckFolder(preferredFolder);

            string suggested = string.IsNullOrWhiteSpace(staged.deckId)
                ? "NewBandDeck"
                : staged.deckId.Trim();

            string chosenPath = EditorUtility.SaveFilePanelInProject(
                "Save Band Deck", suggested, "asset",
                "Choose where to save the BandDeckData asset.",
                defaultFolder);

            if (string.IsNullOrWhiteSpace(chosenPath)) return null; // user cancelled

            return WriteAsset(staged, chosenPath, existingAsset: null);
        }

        // ------------------------------------------------------------------
        // Save
        // ------------------------------------------------------------------

        public static SaveResult Save(StagedDeck staged)
        {
            if (staged == null) return Fail("StagedDeck is null.");
            if (staged.sourceAsset == null) return Fail("No source asset associated. Use Save As.");

            string assetPath = AssetDatabase.GetAssetPath(staged.sourceAsset);
            if (string.IsNullOrWhiteSpace(assetPath)) return Fail("Could not determine path of source asset.");

            return WriteAsset(staged, assetPath, existingAsset: staged.sourceAsset);
        }

        // ------------------------------------------------------------------
        // Gig Setup registration
        // ------------------------------------------------------------------

        public static bool AddToGigSetupConfig(GigSetupConfigData config, BandDeckData deck)
        {
            if (config == null || deck == null) return false;

            var so = new SerializedObject(config);
            so.Update();
            var list = so.FindProperty("availableBandDecks");
            if (list == null)
            {
                Debug.LogError("[DeckAssetSaveService] 'availableBandDecks' not found on GigSetupConfigData.");
                return false;
            }

            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == deck)
                    return true; // already present

            int idx = list.arraySize;
            list.InsertArrayElementAtIndex(idx);
            list.GetArrayElementAtIndex(idx).objectReferenceValue = deck;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);
            return true;
        }

        public static bool RemoveFromGigSetupConfig(GigSetupConfigData config, BandDeckData deck)
        {
            if (config == null || deck == null) return false;

            var so = new SerializedObject(config);
            so.Update();
            var list = so.FindProperty("availableBandDecks");
            if (list == null) return false;

            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == deck)
                {
                    list.GetArrayElementAtIndex(i).objectReferenceValue = null;
                    list.DeleteArrayElementAtIndex(i);
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(config);
                    AssetDatabase.SaveAssetIfDirty(config);
                    return true;
                }
            }
            return false;
        }

        // ------------------------------------------------------------------
        // Internal
        // ------------------------------------------------------------------

        private static SaveResult WriteAsset(
            StagedDeck staged, string assetPath, BandDeckData existingAsset)
        {
            bool createdNewFile = false;
            BandDeckData asset = existingAsset;

            try
            {
                // Determine deck folder — new card assets are routed to their own musician
                // folders by DeckCardCreationService; this is only the fallback for cards
                // whose performer rule has no specific folder (AnyMusician / None).
                string deckFolder = string.IsNullOrEmpty(assetPath)
                    ? "Assets"
                    : (Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? "Assets");

                ResolvePendingCards(staged, deckFolder, out var cardErrors);
                if (cardErrors.Count > 0)
                    return Fail("Failed to save one or more new card assets:\n" +
                                string.Join("\n", cardErrors));

                // Create or update the BandDeckData asset
                if (asset == null)
                {
                    string dir = Path.GetDirectoryName(assetPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    asset = ScriptableObject.CreateInstance<BandDeckData>();
                    asset.name = Path.GetFileNameWithoutExtension(assetPath);
                    createdNewFile = true;
                    AssetDatabase.CreateAsset(asset, assetPath);
                }

                WriteFields(asset, staged);

                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssetIfDirty(asset);
                AssetDatabase.Refresh();

                staged.sourceAsset = asset;
                staged.isDirty = false;

                Debug.Log($"[DeckAssetSaveService] Saved '{asset.name}' at '{assetPath}' ({staged.cards.Count} cards).", asset);
                return new SaveResult { Status = SaveResultStatus.Saved, SavedAsset = asset };
            }
            catch (Exception ex)
            {
                if (createdNewFile && !string.IsNullOrWhiteSpace(assetPath))
                    try { AssetDatabase.DeleteAsset(assetPath); } catch { }

                Debug.LogException(ex);
                return Fail($"Save failed: {ex.Message}");
            }
        }

        private static void ResolvePendingCards(
            StagedDeck staged,
            string deckFolder,
            out System.Collections.Generic.List<string> errors)
        {
            errors = new System.Collections.Generic.List<string>();
            if (staged?.cards == null) return;

            for (int i = 0; i < staged.cards.Count; i++)
            {
                var entry = staged.cards[i];
                if (entry == null || !entry.IsNew) continue;

                // DeckCardCreationService routes musician cards to their specific folders;
                // deckFolder is only used as a fallback for AnyMusician / None cards.
                if (!DeckCardCreationService.SavePendingCard(
                        entry, deckFolder, out var savedCard, out var err))
                {
                    errors.Add($"Card '{entry.pendingCard?.Id ?? "?"}': {err}");
                    continue;
                }

                if (entry.pendingCard != null) UnityEngine.Object.DestroyImmediate(entry.pendingCard);
                if (entry.pendingPayload != null) UnityEngine.Object.DestroyImmediate(entry.pendingPayload);

                staged.cards[i] = StagedCardEntry.FromExisting(savedCard);
            }
        }

        private static void WriteFields(BandDeckData asset, StagedDeck staged)
        {
            var so = new SerializedObject(asset);
            so.Update();

            SetString(so, "deckId", staged.deckId ?? "");
            SetString(so, "displayName", staged.displayName ?? "");
            SetString(so, "description", staged.description ?? "");

            // M4.4: write the new 'entries' multiset (List<BandDeckEntry>).
            // Each element has 'card' (object reference) and 'count' (int).
            var entriesProp = so.FindProperty("entries");
            if (entriesProp == null)
                throw new InvalidOperationException(
                    "Could not find 'entries' on BandDeckData. Has the field been renamed?");

            entriesProp.ClearArray();
            if (staged.cards != null)
            {
                int writeIdx = 0;
                for (int i = 0; i < staged.cards.Count; i++)
                {
                    var entry = staged.cards[i];
                    if (entry?.existingCard == null) continue;

                    int copies = Mathf.Max(1, entry.count);

                    entriesProp.InsertArrayElementAtIndex(writeIdx);
                    var elem = entriesProp.GetArrayElementAtIndex(writeIdx);

                    var cardProp = elem.FindPropertyRelative("card");
                    var countProp = elem.FindPropertyRelative("count");
                    if (cardProp == null || countProp == null)
                        throw new InvalidOperationException(
                            "BandDeckEntry serialized layout missing 'card' or 'count'. " +
                            "Has BandDeckEntry been changed?");

                    cardProp.objectReferenceValue = entry.existingCard;
                    countProp.intValue = copies;
                    writeIdx++;
                }
            }

            // M4.4: clear the pre-M4.4 legacy field. Once 'entries' is
            // populated, 'legacyCards' is dead weight; clearing it here is
            // the upgrade step for previously-loaded assets.
            var legacyProp = so.FindProperty("legacyCards");
            if (legacyProp != null) legacyProp.ClearArray();

            so.ApplyModifiedProperties();
        }

        // ------------------------------------------------------------------
        // Folder resolution
        // ------------------------------------------------------------------

        /// <summary>
        /// Finds the best default folder to open the Save As dialog at.
        /// Order: preferred → existing deck asset folder → "Assets".
        /// </summary>
        private static string ResolveDefaultDeckFolder(string preferred)
        {
            // 1) Caller provided a folder (e.g. same folder as source asset)
            if (!string.IsNullOrWhiteSpace(preferred))
                return preferred.Replace('\\', '/');

            // 2) Find the folder of the first BandDeckData in the project
            string[] guids = AssetDatabase.FindAssets("t:BandDeckData");
            if (guids?.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                string folder = Path.GetDirectoryName(path)?.Replace('\\', '/');
                if (!string.IsNullOrEmpty(folder)) return folder;
            }

            return "Assets";
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static void SetString(SerializedObject so, string prop, string v)
        { var p = so.FindProperty(prop); if (p != null) p.stringValue = v; }

        private static SaveResult Fail(string message)
            => new SaveResult { Status = SaveResultStatus.Failed, Error = message };
    }
}
#endif