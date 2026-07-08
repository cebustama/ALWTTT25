using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [S4 D-TUT-9 / §6 grouping] Revisit-menu category for a dialog.
    /// Maps to the §5 groups: Cards / Meters / Jam / Audience / Run / Boss.
    /// </summary>
    public enum TutorialCategory
    {
        Cards,
        Meters,
        Jam,
        Audience,
        Run,
        Boss
    }

    /// <summary>
    /// [S4 D-TUT-3] Canonical trigger ids. These strings are the keys stored in the
    /// persisted firedDialogs set (PersistentGameplayData) and referenced by the
    /// §2.4 coverage matrix + §6A beat list. Do not rename without migrating the
    /// persisted set.
    /// </summary>
    public static class TutorialTriggerId
    {
        // Standalone (combat / lifecycle)
        public const string WelcomeToGig    = "tut_welcome_to_gig";
        public const string FirstActionCard = "tut_first_action_card";
        public const string FirstAudienceAction = "tut_first_audience_action";
        public const string FirstStatusApplied = "tut_first_status_applied";
        public const string FirstGigWon     = "tut_first_gig_won";
        public const string FirstRewardChoice = "tut_first_reward_choice"; // [S5h]

        // Jam sequence beats 1..6 (§6A)
        public const string FirstCompositionCard = "tut_first_composition_card"; // beat 1
        public const string FirstInspirationSpend = "tut_first_inspiration_spend"; // beat 2
        public const string FirstLoopInspiration = "tut_first_loop_inspiration"; // beat 3
        public const string FirstSfxStage   = "tut_first_sfx_stage";   // beat 4
        public const string FirstSoundCard  = "tut_first_sound_card";  // beat 5 (opportunistic)
        public const string FirstSongEnd    = "tut_first_song_end";    // beat 6

        // ── [TUT-R2 / D3=B] Guided gig-1 curriculum (TUT-R1 §1) ──
        public const string JamWelcome = "tut_jam_welcome";          // beat 1
        public const string YourTurn = "tut_your_turn";            // beat 2
        public const string PlayComposition = "tut_play_composition";     // beat 3 (gate: input)
        public const string TracksThree = "tut_tracks_three";         // beat 4
        public const string PressPlay = "tut_press_play";           // beat 5 (gate: input)
        public const string LoopsStructure = "tut_loops_structure";      // beat 6
        public const string InspirationEconomy = "tut_inspiration_economy";  // beat 7 (+ scripted draw)
        public const string PlayFinisher = "tut_play_finisher";        // beat 8 (gate: holdLoop)
        public const string SongEndVibe = "tut_song_end_vibe";        // beat 9
        public const string AudienceTurn = "tut_audience_turn";        // beat 10

        // ── [TUT-R2] Rewritten reactives (TUT-R1 §4) ──
        public const string StatusBuffMusician = "tut_status_buff_musician";
        public const string StatusDebuffAudience = "tut_status_debuff_audience";
        public const string StatusBlockedFront = "tut_status_blocked_front";
        public const string GigWon = "tut_gig_won";
        public const string GigLost = "tut_gig_lost";
        public const string MusicianBreakdown = "tut_musician_breakdown";
        public const string Composure = "tut_composure";

        // ── Reserved, no trigger wired (D6 / D-TUT-R1-4, Phase C pattern) ──
        public const string AudiencePreferences = "tut_audience_preferences";
        public const string Flow = "tut_flow";
    }

    /// <summary>
    /// [S4 D-TUT-4 = portrait + dialog box; D-TUT-1 authoring discipline]
    /// One authored tutorial dialog: a trigger id, the queue priority that orders
    /// same-resolution multi-fires (D-TUT-10), the revisit category, the captain
    /// portrait, the page text(s), and an optional highlight key resolved by the
    /// controller to an on-screen RectTransform (R1 spotlight).
    ///
    /// Pages: one entry == one page. Authoring discipline (D-TUT-1) keeps this to a
    /// single page for the demo cut unless a beat genuinely needs two.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialDialog",
        menuName = "ALWTTT/Tutorial/Tutorial Dialog",
        order = 0)]
    public class TutorialDialogSO : ScriptableObject
    {
        [Tooltip("Canonical trigger id (use TutorialTriggerId constants).")]
        [SerializeField] private string triggerId;

        [Tooltip("Lower shows first when several fire in one event resolution " +
                 "(D-TUT-10 single-modal queue). Authored as the §6A beat order.")]
        [SerializeField] private int priority = 100;

        [SerializeField] private TutorialCategory category = TutorialCategory.Cards;

        [Tooltip("Short title shown in the revisit submenu list.")]
        [SerializeField] private string revisitTitle;

        [Tooltip("Captain portrait for this dialog. Null falls back to the " +
                 "controller's default portrait.")]
        [SerializeField] private Sprite portrait;

        [Tooltip("One string per page. Keep to one page per D-TUT-1 unless needed.")]
        [TextArea(2, 5)]
        [SerializeField] private List<string> pages = new();

        [Tooltip("Optional. Controller resolves this key to a RectTransform via its " +
                 "highlight bindings and spotlights it (R1). Empty = no spotlight.")]
        [SerializeField] private string highlightKey;

        public string TriggerId => triggerId;
        public int Priority => priority;
        public TutorialCategory Category => category;
        public string RevisitTitle =>
            string.IsNullOrWhiteSpace(revisitTitle) ? triggerId : revisitTitle;
        public Sprite Portrait => portrait;
        public IReadOnlyList<string> Pages => pages;
        public string HighlightKey => highlightKey;
        public bool HasHighlight => !string.IsNullOrWhiteSpace(highlightKey);

#if UNITY_EDITOR
        /// <summary>Editor seeding helper used by the catalog's default-author menu.</summary>
        public void EditorSeed(
            string id, int prio, TutorialCategory cat, string title,
            string highlight, params string[] pageText)
        {
            triggerId = id;
            priority = prio;
            category = cat;
            revisitTitle = title;
            highlightKey = highlight;
            pages = new List<string>(pageText);
        }
#endif
    }
}
