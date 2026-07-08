// Place at: Assets/Scripts/Tutorial/TutorialTokenResolver.cs
using ALWTTT.Managers;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [TUT-R2 / D8] Resolves the three approved copy tokens at DISPLAY time
    /// (TutorialOverlayView resolves each page as it copies it), so revisit-panel
    /// replays also resolve against current values. No rich-text, no authoring
    /// window (D8 scope limit) — plain string substitution of exactly:
    ///
    ///   {$loops_per_part}        ← GigManager.flow.JamRules.loopsPerPart
    ///   {$inspiration_per_loop}  ← PersistentGameplayData.InspirationPerLoop
    ///   {$audience_hp}           ← MAX MaxVibe across the CURRENT encounter's
    ///                              audience members at fire time (TUT-R1 §3
    ///                              proposed rule, ratified in TUT-R2: "hasta"
    ///                              in the copy absorbs the mixed encounter).
    ///
    /// Unresolvable tokens (no gig running, empty audience) degrade to "?" —
    /// never to an exception, never to the raw token.
    /// </summary>
    public static class TutorialTokenResolver
    {
        private const string TokLoops = "{$loops_per_part}";
        private const string TokInsp = "{$inspiration_per_loop}";
        private const string TokHp = "{$audience_hp}";

        public static string Resolve(string page)
        {
            if (string.IsNullOrEmpty(page)) return page;

            if (page.Contains(TokLoops))
                page = page.Replace(TokLoops, ResolveLoopsPerPart());
            if (page.Contains(TokInsp))
                page = page.Replace(TokInsp, ResolveInspirationPerLoop());
            if (page.Contains(TokHp))
                page = page.Replace(TokHp, ResolveAudienceHp());

            return page;
        }

        private static string ResolveLoopsPerPart()
        {
            var gig = GigManager.Instance;
            var flow = gig != null ? gig.Flow : null; // [TUT-R2] new accessor
            return flow != null ? flow.JamRules.loopsPerPart.ToString() : "?";
        }

        private static string ResolveInspirationPerLoop()
        {
            var pd = GameManager.Instance != null
                ? GameManager.Instance.PersistentGameplayData
                : null;
            return pd != null ? pd.InspirationPerLoop.ToString() : "?";
        }

        private static string ResolveAudienceHp()
        {
            var gig = GigManager.Instance;
            var list = gig != null ? gig.CurrentAudienceCharacterList : null;
            if (list == null || list.Count == 0) return "?";

            int max = 0;
            bool any = false;
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a == null || a.Stats == null) continue;
                any = true;
                if (a.Stats.MaxVibe > max) max = a.Stats.MaxVibe;
            }
            return any ? max.ToString() : "?";
        }
    }
}