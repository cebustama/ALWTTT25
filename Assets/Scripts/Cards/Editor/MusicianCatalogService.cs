#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// Editor-only catalog mutation helpers (add/remove entries safely with Undo + dirty).
    /// </summary>
    public static class MusicianCatalogService
    {
        public static bool ContainsCard(MusicianCardCatalogData catalog, CardDefinition card)
        {
            if (catalog == null || card == null) return false;
            var list = catalog.Entries;
            if (list == null) return false;

            foreach (var e in list)
            {
                if (e == null) continue;
                if (e.card == card) return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a new entry for card, unless it already exists. Returns the resulting index.
        /// </summary>
        public static bool TryAddEntry(
            MusicianCardCatalogData catalog,
            CardDefinition card,
            CardAcquisitionFlags flags,
            int starterCopies,
            string unlockId,
            out int index,
            out string error)
        {
            index = -1;
            error = null;

            if (catalog == null) { error = "Catalog is null."; return false; }
            if (card == null) { error = "Card is null."; return false; }

            if (ContainsCard(catalog, card))
            {
                error = "Card already exists in this catalog.";
                return false;
            }

            Undo.RecordObject(catalog, "Add Musician Card Entry");

            var entry = new MusicianCardEntry
            {
                card = card,
                flags = flags,
                starterCopies = Mathf.Max(1, starterCopies),
                unlockId = unlockId
            };

            catalog.Entries.Add(entry);

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            index = catalog.Entries.Count - 1;
            return true;
        }
    }
}

#endif