using ALWTTT.Managers;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [S10] One always-on line per tutorial dialog: which one, what makes it appear (its
    /// requirement) and the state of the gig at that moment. Tag «[S10][TUT]», so filtering
    /// the Console by it gives the whole tutorial run, clean.
    /// </summary>
    public static class TutorialTrace
    {
        public const string Tag = "[S10][TUT]";

        // What makes each dialog appear (TutorialGuidedDriver / TutorialController).
        private static readonly Dictionary<string, string> Requirements = new Dictionary<string, string>
        {
            [TutorialTriggerId.JamWelcome] = "empieza el gig",
            [TutorialTriggerId.YourTurn] = "empieza el gig, tras la bienvenida",
            [TutorialTriggerId.PlayComposition] = "se cierra «Tu turno»; luego solo se pueden jugar composiciones",
            [TutorialTriggerId.PressPlay] = "se juega la primera composición; luego solo Play",
            [TutorialTriggerId.InspirationEconomy] = "termina el primer loop del gig (si el remate no está en la mano, se roba)",
            [TutorialTriggerId.PlayFinisher] = "empieza el último loop, remate (Redoble Final) en la mano y ★ ≥ su coste; el loop espera",
            [TutorialTriggerId.PlayFinisherEarly] = "empieza el último loop y el remate ya se jugó en esta canción",
            [TutorialTriggerId.SongEndVibe] = "termina una canción",
            [TutorialTriggerId.AudienceTurn] = "empieza el turno del público",
            [TutorialTriggerId.AbilitiesIntro] = "empieza el turno 2 (tras el público): llegan Calm Down y Static Rush",
            [TutorialTriggerId.OnYourOwn] = "empieza el turno 2, tras las habilidades: el jugador arma la canción solo",
            [TutorialTriggerId.StatusIntro] = "empieza el turno 3: llegan Spotlight y Guiño",
            [TutorialTriggerId.LoopMore] = "canción 1, al darle a Tocar: llega otra composición",
            [TutorialTriggerId.PauseHint] = "(retirado: va dentro de «Una por loop»)",
            [TutorialTriggerId.Spotlight] = "canción 3, se aplica Foco: leer las líneas del público, Tab, el Aguante de C2",
            [TutorialTriggerId.TrackReplaced] = "canción 3, una carta reemplaza la pista de su músico",
            [TutorialTriggerId.LayersIntro] = "canción 2, al darle a Tocar: una composición por loop, Capas y Espacio (pausa automática)",
            [TutorialTriggerId.TracksIntro] = "canción 3, termina el primer loop: lista de pistas y combos (pausa automática)",
            [TutorialTriggerId.GigWon] = "fin del gig: todo el público convencido",
            [TutorialTriggerId.GigLost] = "fin del gig: sin canciones o sin Cohesión",
        };

        private static int _shown;

        /// <summary>Call when a run starts (the counter restarts).</summary>
        public static void ResetCount() => _shown = 0;

        public static string RequirementOf(string id) =>
            id != null && Requirements.TryGetValue(id, out var r) ? r : "reactivo (fuera del tutorial mínimo)";

        public static void Shown(string id, int queueLeft)
        {
            _shown++;
            Debug.Log($"{Tag} MUESTRA #{_shown} '{id}' | requisito: {RequirementOf(id)} | " +
                      $"estado: {StateSnapshot()} | en cola: {queueLeft}");
        }

        public static void Closed(string id) => Debug.Log($"{Tag} cerrado '{id}'");

        public static void Skipped(string id, string why) =>
            Debug.Log($"{Tag} no sale '{id}': {why}");

        public static void Note(string msg) => Debug.Log($"{Tag} {msg}");

        public static string StateSnapshot()
        {
            var sb = new StringBuilder();
            var gig = GigManager.Instance;
            var pd = GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;

            if (gig == null) return "fuera del gig";

            sb.Append($"fase={gig.CurrentGigPhase}");
            if (pd != null) sb.Append($" canción={pd.CurrentSongIndex + 1}/{gig.RequiredSongCount}");

            var session = gig.CompositionSession;
            if (session != null && session.TryBuildCurrentLoopContext(out var ctx))
                sb.Append($" loop={ctx.LoopIndexWithinPart + 1}/{ctx.LoopsInPart}");
            else
                sb.Append(" loop=-");
            sb.Append($" sonando={(gig.IsSongPlayingNow ? "sí" : "no")}");

            if (pd != null) sb.Append($" ★={pd.CurrentInspiration} cohesión={pd.BandCohesion}");

            var deck = DeckManager.Instance;
            if (deck != null && deck.HandPile != null) sb.Append($" mano={deck.HandPile.Count}");

            var aud = gig.CurrentAudienceCharacterList;
            if (aud != null && aud.Count > 0)
            {
                int convinced = 0;
                var vibes = new List<string>();
                foreach (var a in aud)
                {
                    if (a == null || a.Stats == null) continue;
                    if (a.Stats.IsConvinced) convinced++;
                    vibes.Add($"{a.Stats.CurrentVibe}/{a.Stats.MaxVibe}");
                }
                sb.Append($" convencidos={convinced}/{aud.Count} (Vibe {string.Join(", ", vibes)})");
            }

            sb.Append($" puerta={TutorialInputGate.Mode}");
            return sb.ToString();
        }
    }
}
