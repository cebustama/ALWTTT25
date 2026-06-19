using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [S4 D-TUT-9 / §6] The set of authored tutorial dialogs, looked up by trigger
    /// id at runtime. Authored content (the 11 dialogs) is seeded by the editor menu
    /// below so the strings live in version control as real .asset files rather than
    /// hand-written YAML.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialDialogCatalog",
        menuName = "ALWTTT/Tutorial/Tutorial Dialog Catalog",
        order = 1)]
    public class TutorialDialogCatalogSO : ScriptableObject
    {
        [SerializeField] private List<TutorialDialogSO> dialogs = new();

        private Dictionary<string, TutorialDialogSO> _byId;

        public IReadOnlyList<TutorialDialogSO> Dialogs => dialogs;

        public TutorialDialogSO Get(string triggerId)
        {
            if (string.IsNullOrEmpty(triggerId)) return null;
            if (_byId == null) BuildIndex();
            return _byId.TryGetValue(triggerId, out var d) ? d : null;
        }

        public void BuildIndex()
        {
            _byId = new Dictionary<string, TutorialDialogSO>(dialogs.Count);
            foreach (var d in dialogs)
            {
                if (d == null || string.IsNullOrEmpty(d.TriggerId)) continue;
                _byId[d.TriggerId] = d;
            }
        }

#if UNITY_EDITOR
        // [S4 D-TUT-1 authoring] One-shot author of the 11 demo-cut dialogs.
        // Re-running overwrites the seeded assets' content (idempotent authoring).
        // Text is intentionally short/sharp (D-TUT-1). Beat 5 must not promise an
        // audible key direction (MGP-ALWTTT-MOD-DIR-1).
        private const string SeedDir = "Assets/Resources/Data/Tutorial/Dialogs";

        [ContextMenu("Author/Seed demo-cut dialogs (11)")]
        private void SeedDemoCutDialogs()
        {
            if (!Directory.Exists(SeedDir)) Directory.CreateDirectory(SeedDir);
            dialogs.Clear();

            // (id, priority, category, revisitTitle, highlightKey, pages...)
            Add(TutorialTriggerId.WelcomeToGig, 5, TutorialCategory.Run, "Welcome",
                "",
                "Welcome to the gig. Win the crowd over before you run out of songs — play cards on your turn, then the song performs.");

            Add(TutorialTriggerId.FirstActionCard, 15, TutorialCategory.Cards, "Action cards",
                "hand",
                "Action cards hit right now — buffs, blocks, pressure on the crowd. Play them when the moment calls for it.");

            // --- Jam beats ---
            Add(TutorialTriggerId.FirstCompositionCard, 10, TutorialCategory.Jam, "Building the song",
                "song_panel",
                "This is the jam — you build the song as you play. Composition cards add to it: a track, an instrument. Play one and hear it join.");

            Add(TutorialTriggerId.FirstInspirationSpend, 20, TutorialCategory.Jam, "Inspiration",
                "inspiration_counter",
                "Composition cards cost Inspiration — the counter on the song panel. You start with a few; spend it to shape the song.");

            Add(TutorialTriggerId.FirstLoopInspiration, 30, TutorialCategory.Jam, "Tracks pay you back",
                "inspiration_counter",
                "Every track you've added feeds Inspiration back each time the loop comes around. Build the song and it funds itself.");

            Add(TutorialTriggerId.FirstSfxStage, 40, TutorialCategory.Jam, "The stage reacts",
                "",
                "The crowd's heating up — the stage reacts as the song's hype climbs. Keep it going for bigger moments.");

            Add(TutorialTriggerId.FirstSoundCard, 50, TutorialCategory.Jam, "Sound cards",
                "song_panel",
                "That card reshaped the music itself — its speed, or its key — not your meters. Sound cards change how the song sounds.");

            Add(TutorialTriggerId.FirstSongEnd, 60, TutorialCategory.Jam, "Song's payoff",
                "song_hype",
                "Song's done. The hype you built converts into Vibe on the crowd — and Vibe is how you convince them.");

            // --- Standalone combat ---
            Add(TutorialTriggerId.FirstAudienceAction, 45, TutorialCategory.Audience, "The crowd pushes back",
                "",
                "Now the crowd pushes back. Their pressure builds Stress; your Vibe is how you win them over. Keep your Vibe climbing.");

            Add(TutorialTriggerId.FirstStatusApplied, 25, TutorialCategory.Meters, "Status effects",
                "",
                "That's a status effect. Its icon shows what it does and how long it lasts — right-click anything to read the details.");

            Add(TutorialTriggerId.FirstGigWon, 70, TutorialCategory.Run, "Gig won",
                "",
                "You won them over — that's one gig down. String wins together and the tour really begins.");

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildIndex();
            Debug.Log($"[TutorialDialogCatalog] Seeded {dialogs.Count} demo-cut dialogs into {SeedDir}.");
        }

        private void Add(string id, int prio, TutorialCategory cat, string title,
            string highlight, params string[] pages)
        {
            string path = $"{SeedDir}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<TutorialDialogSO>(path);
            if (so == null)
            {
                so = CreateInstance<TutorialDialogSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.EditorSeed(id, prio, cat, title, highlight, pages);
            EditorUtility.SetDirty(so);
            dialogs.Add(so);
        }
#endif
    }
}
