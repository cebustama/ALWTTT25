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
    /// id at runtime.
    ///
    /// [TUT-TXT-1c / D-TT-6=C] Player-facing COPY no longer lives in this file. The
    /// .asset files are the sole home of revisitTitle / pages / mechanicText (D1=A,
    /// TXT-1); the editor menus below seed STRUCTURE only (trigger id, priority,
    /// category, highlight key) and never write text. Recovery of lost text is the
    /// committed CSV export plus GameTextWindow's import, not a re-seed.
    ///
    /// This REVERSES D-TXT-3 ("seeders stay as an emergency seed" — with their copy),
    /// on evidence: while the copy lived in both places it silently diverged in both
    /// directions. F-TT-3 — TUT-R3's em-dash removal reached this file and never
    /// reached the assets. F-TT-4 — tut_first_reward_choice's asset stayed on
    /// pre-S5h copy while this file carried the newer one. Neither was visible to
    /// parity, which keys on ids. Both were repaired by CSV import in TUT-TXT-1c.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialDialogCatalog",
        menuName = "ALWTTT/Tutorial/Tutorial Dialog Catalog",
        order = 1)]
    public class TutorialDialogCatalogSO : ScriptableObject
    {
        [SerializeField] private List<TutorialDialogSO> dialogs = new();

        // [TXT-1 / D2=B] Language of this catalog's copy ("en", "es"). Authoring-only:
        // GameTextWindow builds one column per code and reports parity per catalog.
        // Runtime never reads it — language selection stays inspector assignment on
        // TutorialController.catalog (D-S5f-2=B). Seeders do not touch this field.
        [Tooltip("Language code of this catalog's copy, e.g. 'en' / 'es'. Authoring tooling only.")]
        [SerializeField] private string languageCode = "";

        private Dictionary<string, TutorialDialogSO> _byId;

        public IReadOnlyList<TutorialDialogSO> Dialogs => dialogs;

        /// <summary>[TXT-1] Authoring-only language tag; see the field note.</summary>
        public string LanguageCode => languageCode;

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
        // ──────────────────────────────────────────────────────────────────
        // [TUT-TXT-1c / D-TT-6=C] Structure seeding
        // ──────────────────────────────────────────────────────────────────
        // Two menus, one per language directory, both walking the SAME table: with the
        // text gone, the old split into demo-cut / TUT-R2 / TUT-REDESIGN-B batches no
        // longer described anything, and EN/ES structure is identical by contract
        // (verified across all 34 ids at TUT-TXT-1c).
        //
        // What a seed does:
        //   - asset missing → create it with empty title and one empty page, so the
        //     Game Text window shows an editable row and parity turns green;
        //   - asset present → align id / priority / category / highlightKey only.
        //     Title, pages and mechanicText are NEVER touched.
        // Both menus are APPEND + idempotent: the id set is purged from the list first,
        // then re-added in table order. Nothing here can shrink the catalog (the old
        // demo-cut seeders started with dialogs.Clear(), which would have taken the list
        // from 34 entries to 3 on a misclick — removed at TUT-TXT-1c).
        //
        // Design intent retained from the retired copy blocks (it is intent, not copy):
        //   - ES voice (D-S5f-1): tú; slightly condescending toward the player, reverent
        //     toward the music. Mechanical beats dry, musical beats poetic.
        //   - No em dashes in dialog copy (§5A.3, TUT-R2b).
        //   - MGP-ALWTTT-MOD-DIR-1: the sound-card beat must not promise an audible key
        //     direction, in ANY language.
        //   - D-S5f-5=B: two pages maximum, cut at the rhetorical pause.
        // These now bind whoever writes in GameTextWindow, which is where the text lives.
        private const string SeedDirEN = "Assets/Resources/Data/Tutorial/Dialogs";
        private const string SeedDirES = "Assets/Resources/Data/Tutorial/Dialogs/ES";

        [ContextMenu("Author/Seed structure EN (34 ids, text untouched)")]
        private void SeedStructureEN() => SeedStructure(SeedDirEN);

        [ContextMenu("Author/Seed structure ES (34 ids, text untouched)")]
        private void SeedStructureES() => SeedStructure(SeedDirES);

        private void SeedStructure(string dir)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            RemoveFromListByIds(TutBRetiredIds);
            RemoveFromListByIds(AllStructureIds());

            Add(dir, TutorialTriggerId.JamWelcome, 10, TutorialCategory.Run, "");
            Add(dir, TutorialTriggerId.YourTurn, 20, TutorialCategory.Cards, "hand");
            Add(dir, TutorialTriggerId.PlayComposition, 30, TutorialCategory.Jam, "card_default_mode");
            Add(dir, TutorialTriggerId.FirstSfxStage, 40, TutorialCategory.Jam, "");
            Add(dir, TutorialTriggerId.TracksByMusician, 40, TutorialCategory.Jam, "song_panel_tracks");
            Add(dir, TutorialTriggerId.FirstSoundCard, 50, TutorialCategory.Jam, "song_panel");
            Add(dir, TutorialTriggerId.PressPlay, 50, TutorialCategory.Jam, "play_button");
            Add(dir, TutorialTriggerId.LoopsStructure, 60, TutorialCategory.Jam, "loops_bar");
            Add(dir, TutorialTriggerId.PlayBudget, 65, TutorialCategory.Cards, "hand");
            Add(dir, TutorialTriggerId.InspirationEconomy, 70, TutorialCategory.Jam, "inspiration_counter");
            Add(dir, TutorialTriggerId.PlayFinisher, 80, TutorialCategory.Cards, "card_psychic_waves");
            Add(dir, TutorialTriggerId.PlayFinisherEarly, 81, TutorialCategory.Cards, "");
            Add(dir, TutorialTriggerId.SongEndVibe, 90, TutorialCategory.Jam, "audience_vibe_bars");
            Add(dir, TutorialTriggerId.AudienceTurn, 100, TutorialCategory.Audience, "audience_area");
            Add(dir, TutorialTriggerId.StatusBuffMusician, 110, TutorialCategory.Meters, "status_icon_musician");
            Add(dir, TutorialTriggerId.StatusDebuffAudience, 112, TutorialCategory.Meters, "status_icon_audience");
            Add(dir, TutorialTriggerId.StatusBlockedFront, 114, TutorialCategory.Audience, "status_icon_blocked");
            Add(dir, TutorialTriggerId.MusicianBreakdown, 116, TutorialCategory.Meters, "musician_stress_bar");
            Add(dir, TutorialTriggerId.Composure, 118, TutorialCategory.Meters, "status_icon_composure");
            Add(dir, TutorialTriggerId.GigWon, 120, TutorialCategory.Run, "");
            Add(dir, TutorialTriggerId.GigLost, 121, TutorialCategory.Run, "");
            Add(dir, TutorialTriggerId.FirstRewardChoice, 122, TutorialCategory.Run, "");
            Add(dir, TutorialTriggerId.FinalLoopLock, 130, TutorialCategory.Jam, "loops_bar");
            Add(dir, TutorialTriggerId.TrackReplaced, 131, TutorialCategory.Jam, "song_panel_tracks");
            Add(dir, TutorialTriggerId.EarwormTick, 132, TutorialCategory.Audience, "status_icon_audience");
            Add(dir, TutorialTriggerId.Captivated, 133, TutorialCategory.Audience, "status_icon_audience");
            Add(dir, TutorialTriggerId.VoltageFirst, 134, TutorialCategory.Meters, "status_icon_musician");
            Add(dir, TutorialTriggerId.OverloadReady, 135, TutorialCategory.Cards, "status_icon_musician");
            Add(dir, TutorialTriggerId.BonusLoop, 136, TutorialCategory.Jam, "loops_bar");
            Add(dir, TutorialTriggerId.Spotlight, 137, TutorialCategory.Meters, "status_icon_musician");
            Add(dir, TutorialTriggerId.ReadTheRoom, 138, TutorialCategory.Audience, "audience_area");
            Add(dir, TutorialTriggerId.HarmonyTrack, 139, TutorialCategory.Jam, "song_panel_tracks");
            Add(dir, TutorialTriggerId.HarmonyDenied, 140, TutorialCategory.Jam, "song_panel_tracks");
            Add(dir, TutorialTriggerId.SungMelody, 141, TutorialCategory.Jam, "song_panel_tracks");

            EndSeed(dir);
        }

        /// <summary>Every id the structure table authors, for the idempotency purge.</summary>
        private static string[] AllStructureIds()
        {
            var canonical = CanonicalTriggerIds();
            var arr = new string[canonical.Count];
            canonical.CopyTo(arr);
            return arr;
        }

        // [TUT-R2] Idempotency for the APPEND seeders: re-running must not duplicate
        // list entries (Add() always list-appends).
        private void RemoveFromListByIds(params string[] ids)
        {
            var set = new HashSet<string>(ids);
            dialogs.RemoveAll(d => d != null && set.Contains(d.TriggerId));
        }

        // Literal on purpose: the TracksThree constant no longer exists. The .asset is
        // deleted by hand (TUT-R3 procedure); this only purges the list entry.
        private static readonly string[] TutBRetiredIds = { "tut_tracks_three" };

        private void EndSeed(string dir)
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildIndex();
            Debug.Log($"[TutorialDialogCatalog] Seeded STRUCTURE for {dialogs.Count} dialogs in {dir}. " +
                      "No text was written: copy lives in the .asset files (TUT-TXT-1c / D-TT-6=C). " +
                      "Recover lost text by importing the committed CSV from the Game Text window.");
        }

        /// <summary>
        /// Creates the asset if it is missing (empty title, one empty page) and then
        /// aligns its structural fields. Never writes title, pages or mechanicText on an
        /// asset that already exists — that separation is the whole point of D-TT-6=C.
        /// </summary>
        private void Add(string dir, string id, int prio, TutorialCategory cat, string highlight)
        {
            string path = $"{dir}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<TutorialDialogSO>(path);
            if (so == null)
            {
                so = CreateInstance<TutorialDialogSO>();
                AssetDatabase.CreateAsset(so, path);
                so.EditorSeed(id, prio, cat, "", highlight, "");   // fresh asset only
            }
            else
            {
                so.EditorSeedStructure(id, prio, cat, highlight);  // [TUT-TXT-1c] text untouched
            }
            EditorUtility.SetDirty(so);
            dialogs.Add(so);
        }

        // ---------------- [TXT-1] parity as data (shared by the menu guard and GameTextWindow) ----------------
        /// <summary>Canonical set = every public const string on TutorialTriggerId.</summary>
        public static HashSet<string> CanonicalTriggerIds()
        {
            var canonical = new HashSet<string>();
            foreach (var f in typeof(TutorialTriggerId).GetFields(
                         System.Reflection.BindingFlags.Public |
                         System.Reflection.BindingFlags.Static))
                if (f.IsLiteral && f.FieldType == typeof(string))
                    canonical.Add((string)f.GetRawConstantValue());
            return canonical;
        }

        // [TUT-R3 / O2=A] Ids reservados sin diálogo autorizado: placeholders
        // intencionales, NO divergencia. Se excluyen del reporte "missing" (la
        // divergencia EN/ES y los extras reales se siguen detectando).
        // [TUT-REDESIGN-B] Vacío desde que tut_flow / tut_audience_preferences fueron
        // absorbidos. Se conserva el mecanismo: el próximo id reservado se añade aquí.
        // [TXT-1] Movido de ValidateCatalogParity() a campo estático para que
        // ComputeParity() y el guard usen la misma exención.
        private static readonly HashSet<string> ReservedUnauthored = new();

        /// <summary>[TXT-1] Result of <see cref="ComputeParity"/> for one catalog.</summary>
        public sealed class ParityReport
        {
            public readonly List<string> Missing = new();
            public readonly List<string> Extra = new();
            public bool Ok => Missing.Count == 0 && Extra.Count == 0;
        }

        /// <summary>
        /// [TXT-1] Same computation the menu guard prints, returned as data: canonical ids
        /// absent from the catalog (minus ReservedUnauthored) and catalog ids that are not
        /// canonical. One implementation for both surfaces so the window can never disagree
        /// with the Console.
        /// </summary>
        public static ParityReport ComputeParity(TutorialDialogCatalogSO cat)
        {
            var report = new ParityReport();
            var canonical = CanonicalTriggerIds();
            var present = new HashSet<string>();
            if (cat != null)
                foreach (var d in cat.dialogs)
                    if (d != null && !string.IsNullOrEmpty(d.TriggerId))
                        present.Add(d.TriggerId);
            foreach (var id in canonical)
                if (!present.Contains(id) && !ReservedUnauthored.Contains(id)) report.Missing.Add(id);
            foreach (var id in present)
                if (!canonical.Contains(id)) report.Extra.Add(id);
            return report;
        }

        // ---------------- [S5f] language-parity check (editor-only tooling) ----------------
        // Compares every TutorialDialogCatalogSO asset in the project against the
        // canonical TutorialTriggerId constant set. Reports missing / extra ids per
        // catalog. Guards D-S5f-2=B (dual catalog) against divergence when new
        // dialogs are authored in one language only.
        [MenuItem("ALWTTT/Tutorial/Validate catalog language parity")]
        private static void ValidateCatalogParity()
        {
            // [TXT-1] Canonical set + reserved exemption live in CanonicalTriggerIds() /
            // ReservedUnauthored so GameTextWindow computes the same parity as this guard.
            var canonical = CanonicalTriggerIds();

            var guids = AssetDatabase.FindAssets($"t:{nameof(TutorialDialogCatalogSO)}");
            if (guids.Length == 0)
            {
                Debug.LogWarning("[TutorialParity] No TutorialDialogCatalogSO assets found.");
                return;
            }

            bool anyDrift = false;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var cat = AssetDatabase.LoadAssetAtPath<TutorialDialogCatalogSO>(path);
                if (cat == null) continue;

                var report = ComputeParity(cat);
                var missing = report.Missing;
                var extra = report.Extra;

                if (missing.Count == 0 && extra.Count == 0)
                {
                    Debug.Log($"[TutorialParity] OK — '{path}' covers all {canonical.Count} canonical trigger ids.");
                }
                else
                {
                    anyDrift = true;
                    Debug.LogWarning($"[TutorialParity] DRIFT — '{path}': " +
                        $"missing [{string.Join(", ", missing)}] · extra [{string.Join(", ", extra)}]");
                }
            }
            if (!anyDrift)
                Debug.Log($"[TutorialParity] All {guids.Length} catalog(s) in parity.");
        }
#endif
    }
}