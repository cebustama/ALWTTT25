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

        // [TUT-R2] Idempotency for the APPEND seeders below: re-running must not
        // duplicate list entries (the shared Add() always list-appends).
        private void RemoveFromListByIds(params string[] ids)
        {
            var set = new HashSet<string>(ids);
            dialogs.RemoveAll(d => d != null && set.Contains(d.TriggerId));
        }

        private static readonly string[] TutR2Ids =
        {
            TutorialTriggerId.JamWelcome, TutorialTriggerId.YourTurn,
            TutorialTriggerId.PlayComposition, TutorialTriggerId.TracksThree,
            TutorialTriggerId.PressPlay, TutorialTriggerId.LoopsStructure,
            TutorialTriggerId.InspirationEconomy, TutorialTriggerId.PlayFinisher,
            TutorialTriggerId.SongEndVibe, TutorialTriggerId.AudienceTurn,
            TutorialTriggerId.StatusBuffMusician, TutorialTriggerId.StatusDebuffAudience,
            TutorialTriggerId.StatusBlockedFront, TutorialTriggerId.MusicianBreakdown,
            TutorialTriggerId.Composure, TutorialTriggerId.GigWon, TutorialTriggerId.GigLost,
        };

        [ContextMenu("Author/Seed TUT-R2 guided+reactive dialogs ES (17, provisional)")]
        private void SeedGuidedDialogsES()
        {
            const string dir = SeedDirES;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            RemoveFromListByIds(TutR2Ids); // NOTE: no BeginSeed — append, don't clear.

            Add(dir, TutorialTriggerId.JamWelcome, 10, TutorialCategory.Run, "Bienvenido a la jam", "",
                "Bienvenido al escenario, novato. Esto es una jam: la banda toca en vivo y el público decide si vales algo. Tu único trabajo es hacer que la música hable. Yo te voy diciendo cómo.");
            Add(dir, TutorialTriggerId.YourTurn, 20, TutorialCategory.Cards, "Tu turno", "hand",
                "Tu turno. En la mano hay dos tipos de carta: las de COMPOSICIÓN construyen la canción; las de ACCIÓN cuidan a la banda y empujan al público. La música primero. Siempre.");
            Add(dir, TutorialTriggerId.PlayComposition, 30, TutorialCategory.Jam, "Juega una composición", "card_default_mode",
                "Juega una carta de composición: arrástrala a la banda. Cada una añade algo real a la canción — no es decorado, es música. Empieza por el ritmo.");
            Add(dir, TutorialTriggerId.TracksThree, 40, TutorialCategory.Jam, "Tres pistas", "song_panel_tracks",
                "¿Ves el panel de la canción? Tres pistas: RITMO, BASE y MELODÍA. Cada carta de composición toca UNA de ellas. La batería marca el pulso, los acordes ponen el mundo, la melodía es lo que se queda en la cabeza.");
            Add(dir, TutorialTriggerId.PressPlay, 50, TutorialCategory.Jam, "Presiona Play", "play_button",
                "Ahora dale al Play. La canción sonará en bucle, y todo lo que juegues desde ahora entra en vivo. Sin ensayo. Así se toca de verdad.");
            Add(dir, TutorialTriggerId.LoopsStructure, 60, TutorialCategory.Jam, "Loops", "loops_bar",
                "Cada canción dura {$loops_per_part} loops, y cada loop es un turno. La barra de arriba te dice cuántos quedan. Cuando acaba el último loop, la canción se cierra — y se cobra.");
            Add(dir, TutorialTriggerId.InspirationEconomy, 70, TutorialCategory.Jam, "Inspiración", "inspiration_counter",
                "¿Ves ese +{$inspiration_per_loop}? Cada loop te da {$inspiration_per_loop} de Inspiración. Algunas cartas la exigen para jugarse — las buenas, claro. Guárdala: te acaba de llegar a la mano una que la vale.");
            Add(dir, TutorialTriggerId.PlayFinisher, 80, TutorialCategory.Cards, "El Gran Final", "card_grand_finale",
                "Último loop. Hora del Gran Final: una carta de ACCIÓN — no toca la canción, golpea al público. Cuesta Inspiración y pega a TODO el público a la vez. Juégala antes de que acabe el loop. El cierre lo es todo.");
            Add(dir, TutorialTriggerId.SongEndVibe, 90, TutorialCategory.Jam, "El pago de la canción", "audience_vibe_bars",
                "Fin de la canción: todo el hype que construiste se convierte en DAÑO de Vibe contra el público. Cada uno aguanta hasta {$audience_hp} — vacíasela y es tuyo: convencido. Así se gana un concierto, canción a canción.");
            Add(dir, TutorialTriggerId.AudienceTurn, 100, TutorialCategory.Audience, "Turno del público", "audience_area",
                "Ahora les toca a ellos. Cada personaje del público tiene sus propias mañas: unos golpean el Stress de tus músicos — su reserva de entereza; a cero, colapso — y otros se cubren entre sí. Míralos bien antes de tu siguiente turno.");

            Add(dir, TutorialTriggerId.StatusBuffMusician, 110, TutorialCategory.Meters, "Efectos de estado", "status_icon_musician",
                "¿Ves ese icono sobre el músico? Es un efecto de estado — y este juega a tu favor. Pasa el cursor por encima y te dice exactamente qué hace. Léelos: la banda vive de ellos.");
            Add(dir, TutorialTriggerId.StatusDebuffAudience, 112, TutorialCategory.Meters, "Efectos sobre el público", "status_icon_audience",
                "Le has colgado un efecto al público — el icono bajo su retrato. Los efectos trabajan solos, turno a turno, sin pedir permiso. Plántalos y deja que la música haga el resto.");
            Add(dir, TutorialTriggerId.StatusBlockedFront, 114, TutorialCategory.Audience, "Bloqueado", "status_icon_blocked",
                "El grandote se ha puesto delante y se ha BLOQUEADO: mientras le dure ese icono, tu Vibe no le entra. No malgastes música contra un muro — espera a que baje la guardia, o gasta la canción en los que sí escuchan.");
            Add(dir, TutorialTriggerId.MusicianBreakdown, 116, TutorialCategory.Meters, "Stress", "musician_stress_bar",
                "Golpe al Stress de tu músico. Esa barra es su entereza, y se gasta: a cero, colapsa y deja de tocar. Algunas cartas de acción la recuperan — o la protegen. Cuida a tu banda: sin banda no hay canción.");
            Add(dir, TutorialTriggerId.Composure, 118, TutorialCategory.Meters, "Compostura", "status_icon_composure",
                "Eso es COMPOSTURA: absorbe el daño al Stress antes de que toque la entereza de tu músico. Dura hasta tu próximo turno y luego se esfuma — es una guardia, no una armadura. Súbela cuando veas venir el golpe.");
            Add(dir, TutorialTriggerId.GigWon, 120, TutorialCategory.Run, "Concierto ganado", "",
                "Todos convencidos. ¿Oyes eso? El silencio de justo después — eso es un público que ya es tuyo. No te lo creas demasiado, novato: fue UN concierto. Pero fue música de verdad.");
            Add(dir, TutorialTriggerId.GigLost, 121, TutorialCategory.Run, "Concierto perdido", "",
                "Se acabó y no cayeron todos. Pasa. La música no perdona los cierres flojos: la próxima vez guarda Inspiración para el final y remata. Venga — otra vez desde arriba.");

            EndSeed(dir);
        }

        [ContextMenu("Author/Seed TUT-R2 guided+reactive dialogs EN (17, provisional)")]
        private void SeedGuidedDialogsEN()
        {
            const string dir = SeedDirEN;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            RemoveFromListByIds(TutR2Ids);

            Add(dir, TutorialTriggerId.JamWelcome, 10, TutorialCategory.Run, "Welcome to the jam", "",
                "Welcome to the stage, rookie. This is a jam: the band plays live and the crowd decides if you're worth anything. Your only job is to make the music talk. I'll tell you how.");
            Add(dir, TutorialTriggerId.YourTurn, 20, TutorialCategory.Cards, "Your turn", "hand",
                "Your turn. Your hand holds two kinds of cards: COMPOSITION cards build the song; ACTION cards protect the band and push the crowd. Music first. Always.");
            Add(dir, TutorialTriggerId.PlayComposition, 30, TutorialCategory.Jam, "Play a composition", "card_default_mode",
                "Play a composition card: drag it onto the band. Each one adds something real to the song — it's not set dressing, it's music. Start with the rhythm.");
            Add(dir, TutorialTriggerId.TracksThree, 40, TutorialCategory.Jam, "Three tracks", "song_panel_tracks",
                "See the song panel? Three tracks: RHYTHM, BACKING and MELODY. Each composition card touches ONE of them. Drums set the pulse, chords build the world, melody is what sticks in your head.");
            Add(dir, TutorialTriggerId.PressPlay, 50, TutorialCategory.Jam, "Hit Play", "play_button",
                "Now hit Play. The song will run in a loop, and everything you play from here lands live. No rehearsal. That's how real music gets made.");
            Add(dir, TutorialTriggerId.LoopsStructure, 60, TutorialCategory.Jam, "Loops", "loops_bar",
                "Each song runs {$loops_per_part} loops, and every loop is a turn. The bar up top shows what's left. When the last loop ends, the song closes — and the bill comes due.");
            Add(dir, TutorialTriggerId.InspirationEconomy, 70, TutorialCategory.Jam, "Inspiration", "inspiration_counter",
                "See that +{$inspiration_per_loop}? Every loop feeds you {$inspiration_per_loop} Inspiration. Some cards demand it to be played — the good ones, naturally. Save it: one just landed in your hand that's worth it.");
            Add(dir, TutorialTriggerId.PlayFinisher, 80, TutorialCategory.Cards, "The Grand Finale", "card_grand_finale",
                "Last loop. Time for the Grand Finale: an ACTION card — it doesn't touch the song, it hits the crowd. It costs Inspiration and it hits the WHOLE crowd at once. Play it before the loop ends. The ending is everything.");
            Add(dir, TutorialTriggerId.SongEndVibe, 90, TutorialCategory.Jam, "Song's payoff", "audience_vibe_bars",
                "Song's over: all the hype you built converts into Vibe DAMAGE on the crowd. Each of them holds up to {$audience_hp} — drain it and they're yours: convinced. That's how you win a gig, song by song.");
            Add(dir, TutorialTriggerId.AudienceTurn, 100, TutorialCategory.Audience, "The crowd's turn", "audience_area",
                "Now it's their turn. Every character in the crowd has their own tricks: some hit your musicians' Stress — their fortitude reserve; at zero, breakdown — and others cover for each other. Watch them before your next turn.");

            Add(dir, TutorialTriggerId.StatusBuffMusician, 110, TutorialCategory.Meters, "Status effects", "status_icon_musician",
                "See that icon over the musician? That's a status effect — and this one's working for you. Hover it and it tells you exactly what it does. Read them: the band lives on them.");
            Add(dir, TutorialTriggerId.StatusDebuffAudience, 112, TutorialCategory.Meters, "Effects on the crowd", "status_icon_audience",
                "You've hung an effect on the crowd — the icon under their portrait. Effects work on their own, turn after turn, no permission needed. Plant them and let the music do the rest.");
            Add(dir, TutorialTriggerId.StatusBlockedFront, 114, TutorialCategory.Audience, "Blocked", "status_icon_blocked",
                "The big guy stepped up front and BLOCKED: while that icon lasts, your Vibe won't get through. Don't waste music on a wall — wait for the guard to drop, or spend the song on the ones actually listening.");
            Add(dir, TutorialTriggerId.MusicianBreakdown, 116, TutorialCategory.Meters, "Stress", "musician_stress_bar",
                "Your musician's Stress took a hit. That bar is their fortitude, and it runs out: at zero, they break down and stop playing. Some action cards restore it — or shield it. Take care of your band: no band, no song.");
            Add(dir, TutorialTriggerId.Composure, 118, TutorialCategory.Meters, "Composure", "status_icon_composure",
                "That's COMPOSURE: it soaks Stress damage before it touches your musician's fortitude. It lasts until your next turn, then it's gone — it's a guard, not armor. Raise it when you see the hit coming.");
            Add(dir, TutorialTriggerId.GigWon, 120, TutorialCategory.Run, "Gig won", "",
                "All of them convinced. Hear that? The silence right after — that's a crowd that belongs to you now. Don't let it go to your head, rookie: it was ONE gig. But it was real music.");
            Add(dir, TutorialTriggerId.GigLost, 121, TutorialCategory.Run, "Gig lost", "",
                "It's over and not all of them fell. Happens. Music doesn't forgive weak endings: next time, bank Inspiration for the finish and close it out. Come on — from the top.");

            EndSeed(dir);
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