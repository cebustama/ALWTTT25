namespace ALWTTT.Managers
{
    /// <summary>
    /// [COH-1 / D-DIAG-1=A] What a lost gig means for the run, and what the loss
    /// panel says. Pure (no Unity state) so it is covered by an EditMode test.
    ///
    /// Rules:
    /// - Cohesion already 0 when the gig ends (breakdown collapse, Dev stepper)
    ///   → the band breaks up.
    /// - Map run (the demo) + final encounter → losing to the Queen ends the
    ///   tour (D1, COH-1).
    /// - Map run + the loss penalty takes cohesion to 0 → the band breaks up.
    /// - Otherwise the tour goes on: penalty applied, back to the map.
    /// Outside a map run (GigSetup) the run never ends here: the panel keeps
    /// Retry/Exit for testing (D4, COH-1), only the text changes.
    /// </summary>
    public readonly struct GigLossOutcome
    {
        public bool RunOver { get; }
        public int CohesionBefore { get; }
        public int CohesionAfter { get; }
        public string Title { get; }
        public string Body { get; }
        public string ButtonLabel { get; }

        private GigLossOutcome(bool runOver, int before, int after,
            string title, string body, string button)
        {
            RunOver = runOver;
            CohesionBefore = before;
            CohesionAfter = after;
            Title = title;
            Body = body;
            ButtonLabel = button;
        }

        public const string ContinueLabel = "Continuar";
        public const string EndOfTourLabel = "Fin de la gira";

        public static GigLossOutcome Resolve(
            int cohesionBefore, int penalty, bool mapRun, bool finalEncounter)
        {
            if (cohesionBefore < 0) cohesionBefore = 0;
            if (penalty < 0) penalty = 0;

            if (cohesionBefore == 0)
            {
                return new GigLossOutcome(mapRun, 0, 0,
                    "La banda se separa",
                    "Un músico se vino abajo y a la banda no le quedaba cohesión.\n" +
                    "Cohesión: 0.",
                    mapRun ? EndOfTourLabel : null);
            }

            int after = cohesionBefore - penalty;
            if (after < 0) after = 0;
            string cohesionLine = $"Cohesión {cohesionBefore} → {after}.";

            // Losing the Queen ends the tour outright: the cohesion change is
            // irrelevant to the player, so the body does not show it.
            if (mapRun && finalEncounter)
            {
                return new GigLossOutcome(true, cohesionBefore, after,
                    "La Reina no se convenció",
                    "La gira termina aquí.",
                    EndOfTourLabel);
            }

            if (mapRun && after == 0)
            {
                return new GigLossOutcome(true, cohesionBefore, after,
                    "Gig perdido",
                    "El público no se convenció y la banda ya no aguanta más.\n" +
                    cohesionLine,
                    EndOfTourLabel);
            }

            return new GigLossOutcome(false, cohesionBefore, after,
                "Gig perdido",
                "El público no se convenció, pero la gira sigue.\n" + cohesionLine,
                mapRun ? ContinueLabel : null);
        }
    }
}
