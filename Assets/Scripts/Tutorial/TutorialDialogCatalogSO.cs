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
        // [S4 D-TUT-1 authoring / S5f D-S5f-2=B dual catalog] One-shot authors of the
        // 11 demo-cut dialogs, one seeder per language, seeding into per-language
        // subdirectories. Re-running overwrites the seeded assets' content (idempotent
        // authoring). Trigger ids, priorities, categories, and highlight keys are
        // IDENTICAL across languages — only revisitTitle and pages differ. The
        // persisted firedDialogs set keys on trigger id, so catalog swaps do not
        // reset tutorial progress.
        //
        // Text is intentionally short/sharp (D-TUT-1). Beat 5 must not promise an
        // audible key direction (MGP-ALWTTT-MOD-DIR-1) — in ANY language.
        //
        // [S5f] EN copy for FirstAudienceAction / FirstSongEnd / FirstLoopInspiration
        // corrected to the S5e inverted semantics (Stress & Vibe are depleting pools;
        // inspiration gain is fixed per loop). ES copy voice (D-S5f-1): tú, manager
        // slightly condescending toward the player, genuinely reverent toward music.
        private const string SeedDirEN = "Assets/Resources/Data/Tutorial/Dialogs";
        private const string SeedDirES = "Assets/Resources/Data/Tutorial/Dialogs/ES";

        [ContextMenu("Author/Seed demo-cut dialogs EN (11)")]
        private void SeedDemoCutDialogsEN()
        {
            BeginSeed(SeedDirEN);

            // (dir, id, priority, category, revisitTitle, highlightKey, pages...)
            Add(SeedDirEN, TutorialTriggerId.WelcomeToGig, 5, TutorialCategory.Run, "Welcome",
                "",
                "Welcome to the gig. Win the crowd over before you run out of songs — play cards on your turn, then the song performs.");

            Add(SeedDirEN, TutorialTriggerId.FirstActionCard, 15, TutorialCategory.Cards, "Action cards",
                "hand",
                "Action cards hit right now — buffs, blocks, pressure on the crowd. Play them when the moment calls for it.");

            // --- Jam beats ---
            Add(SeedDirEN, TutorialTriggerId.FirstCompositionCard, 10, TutorialCategory.Jam, "Building the song",
                "song_panel",
                "This is the jam — you build the song as you play. Composition cards add to it: a track, an instrument. Play one and hear it join.");

            Add(SeedDirEN, TutorialTriggerId.FirstInspirationSpend, 20, TutorialCategory.Jam, "Inspiration",
                "inspiration_counter",
                "Composition cards cost Inspiration — the counter on the song panel. You start with a few; spend it to shape the song.");

            Add(SeedDirEN, TutorialTriggerId.FirstLoopInspiration, 30, TutorialCategory.Jam, "The loop pays you back",
                "inspiration_counter",
                "Every time the loop comes around you get Inspiration back — a fixed amount, no matter what. Manage it and the song never stops.");

            Add(SeedDirEN, TutorialTriggerId.FirstSfxStage, 40, TutorialCategory.Jam, "The stage reacts",
                "",
                "The crowd's heating up — the stage reacts as the song's hype climbs. Keep it going for bigger moments.");

            Add(SeedDirEN, TutorialTriggerId.FirstSoundCard, 50, TutorialCategory.Jam, "Sound cards",
                "song_panel",
                "That card reshaped the music itself — its speed, or its key — not your meters. Sound cards change how the song sounds.");

            Add(SeedDirEN, TutorialTriggerId.FirstSongEnd, 60, TutorialCategory.Jam, "Song's payoff",
                "song_hype",
                "Song's done. All the hype you built hits the crowd's Vibe and wears their resistance down. Vibe at zero, crowd convinced.");

            // --- Standalone combat ---
            Add(SeedDirEN, TutorialTriggerId.FirstAudienceAction, 45, TutorialCategory.Audience, "The crowd pushes back",
                "",
                "The crowd won't convince itself. Their pressure wears down your musicians' Stress — if one hits zero, they break down.",
                "Grind the crowd's Vibe to zero before they take your band down.");

            Add(SeedDirEN, TutorialTriggerId.FirstStatusApplied, 25, TutorialCategory.Meters, "Status effects",
                "",
                "That's a status effect. Its icon shows what it does and how long it lasts — right-click anything to read the details.");

            Add(SeedDirEN, TutorialTriggerId.FirstGigWon, 70, TutorialCategory.Run, "Gig won",
                "",
                "You won them over — that's one gig down. String wins together and the tour really begins.");

            EndSeed(SeedDirEN);
        }

        // [S5f copy v2, approved 2026-07-04] Voice: tú; condescending toward the
        // player, reverent toward the music. Mechanical beats stay dry; musical
        // beats carry the poetic layer.
        [ContextMenu("Author/Seed demo-cut dialogs ES (11)")]
        private void SeedDemoCutDialogsES()
        {
            BeginSeed(SeedDirES);

            // [D-S5f-5=B] Every dialog is authored as 2 balanced pages, cut at the
            // rhetorical pause (never mid-sentence). The overlay already paginates
            // (click: reveal → next page → complete). TMP auto-size with a bounded
            // min acts only as a safety net on the overlay prefab.
            Add(SeedDirES, TutorialTriggerId.WelcomeToGig, 5, TutorialCategory.Run, "Bienvenido",
                "",
                "Bienvenido al show, novato. Ahí fuera hay gente esperando algo que los atraviese — convéncelos antes de quedarte sin canciones.",
                "Juega tus cartas en tu turno; la música hará lo que tú todavía no sabes hacer.");

            Add(SeedDirES, TutorialTriggerId.FirstActionCard, 15, TutorialCategory.Cards, "Cartas de acción",
                "hand",
                "Las cartas de acción pegan al instante: mejoras, bloqueos, presión sobre el público.",
                "Úsalas cuando el momento lo pida. Hasta tú puedes con eso.");

            // --- Jam beats ---
            Add(SeedDirES, TutorialTriggerId.FirstCompositionCard, 10, TutorialCategory.Jam, "Construir la canción",
                "song_panel",
                "Esto es la jam: la canción nace mientras se toca, viva, por capas. Las cartas de composición le añaden una pista, un instrumento.",
                "Juega una y escúchala entrar — ese momento no envejece nunca.");

            Add(SeedDirES, TutorialTriggerId.FirstInspirationSpend, 20, TutorialCategory.Jam, "Inspiración",
                "inspiration_counter",
                "Las cartas de composición cuestan Inspiración — el contador del panel de la canción.",
                "Empiezas con poca. La inspiración no se mendiga: se administra.");

            Add(SeedDirES, TutorialTriggerId.FirstLoopInspiration, 30, TutorialCategory.Jam, "El loop te paga",
                "inspiration_counter",
                "Cada vuelta del loop te devuelve Inspiración. Cantidad fija, pase lo que pase.",
                "La música es lo único aquí que nunca deja de dar. Adminístrala y la canción no para.");

            Add(SeedDirES, TutorialTriggerId.FirstSfxStage, 40, TutorialCategory.Jam, "El escenario reacciona",
                "",
                "¿Lo sientes? El hype sube y hasta el escenario responde — cuando la música aprieta, todo lo que la rodea despierta.",
                "Mantenla viva y habrá momentos más grandes.");

            // MGP-ALWTTT-MOD-DIR-1: no audible key-direction promise.
            Add(SeedDirES, TutorialTriggerId.FirstSoundCard, 50, TutorialCategory.Jam, "Cartas de sonido",
                "song_panel",
                "Esa carta no tocó tus medidores — tocó la música misma: su velocidad, su tonalidad.",
                "Las cartas de sonido cambian cómo suena la canción. Es lo más parecido a magia que vas a manejar.");

            Add(SeedDirES, TutorialTriggerId.FirstSongEnd, 60, TutorialCategory.Jam, "El pago de la canción",
                "song_hype",
                "Canción terminada. Todo el hype que acumulaste golpea el Vibe del público y le baja la resistencia — así trabaja una buena canción: por dentro.",
                "Vibe a cero, público convencido.");

            // --- Standalone combat ---
            Add(SeedDirES, TutorialTriggerId.FirstAudienceAction, 45, TutorialCategory.Audience, "El público contraataca",
                "",
                "El público no se rinde solo — nadie entrega el corazón gratis. Su presión reduce el Stress de tus músicos; si el de uno llega a cero, colapsa.",
                "Baja tú su Vibe a cero antes de que ellos tumben a tu banda.");

            Add(SeedDirES, TutorialTriggerId.FirstStatusApplied, 25, TutorialCategory.Meters, "Efectos de estado",
                "",
                "Eso es un efecto de estado. Su icono te dice qué hace y cuánto dura.",
                "Clic derecho sobre cualquier cosa para leer los detalles. Sí, cualquier cosa.");

            Add(SeedDirES, TutorialTriggerId.FirstGigWon, 70, TutorialCategory.Run, "Concierto ganado",
                "",
                "Los convenciste. La música hizo lo suyo; tú, de vez en cuando, no estorbaste.",
                "Encadena victorias y la gira empieza de verdad.");

            EndSeed(SeedDirES);
        }

        private void BeginSeed(string dir)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            dialogs.Clear();
        }

        private void EndSeed(string dir)
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildIndex();
            Debug.Log($"[TutorialDialogCatalog] Seeded {dialogs.Count} demo-cut dialogs into {dir}.");
        }

        private void Add(string dir, string id, int prio, TutorialCategory cat, string title,
            string highlight, params string[] pages)
        {
            string path = $"{dir}/{id}.asset";
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

        // ---------------- [S5f] language-parity check (editor-only tooling) ----------------
        // Compares every TutorialDialogCatalogSO asset in the project against the
        // canonical TutorialTriggerId constant set. Reports missing / extra ids per
        // catalog. Guards D-S5f-2=B (dual catalog) against divergence when new
        // dialogs are authored in one language only.
        [MenuItem("ALWTTT/Tutorial/Validate catalog language parity")]
        private static void ValidateCatalogParity()
        {
            // Canonical set = all public const string fields on TutorialTriggerId.
            var canonical = new HashSet<string>();
            foreach (var f in typeof(TutorialTriggerId).GetFields(
                         System.Reflection.BindingFlags.Public |
                         System.Reflection.BindingFlags.Static))
                if (f.IsLiteral && f.FieldType == typeof(string))
                    canonical.Add((string)f.GetRawConstantValue());

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

                var present = new HashSet<string>();
                foreach (var d in cat.dialogs)
                    if (d != null && !string.IsNullOrEmpty(d.TriggerId))
                        present.Add(d.TriggerId);

                var missing = new List<string>();
                foreach (var id in canonical) if (!present.Contains(id)) missing.Add(id);
                var extra = new List<string>();
                foreach (var id in present) if (!canonical.Contains(id)) extra.Add(id);

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