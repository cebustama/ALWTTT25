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
        public const string FirstRewardChoice = "tut_first_reward_choice"; // [S5h]

        // Jam sequence beats 1..6 (§6A)
        public const string FirstSfxStage   = "tut_first_sfx_stage";   // beat 4
        public const string FirstSoundCard  = "tut_first_sound_card";  // beat 5 (opportunistic)

        // ── [TUT-R2 / D3=B] Guided gig-1 curriculum (TUT-R1 §1) ──
        public const string JamWelcome = "tut_jam_welcome";          // beat 1
        public const string YourTurn = "tut_your_turn";            // beat 2
        public const string PlayComposition = "tut_play_composition";     // beat 3 (gate: input)
        // [TUT-REDESIGN-B] Replaces tut_tracks_three (retired: it asserted 3 roles; the band of 4 has 5).
        public const string TracksByMusician = "tut_tracks_by_musician";  // beat 4
        public const string PressPlay = "tut_press_play";           // beat 5 (gate: input)
        public const string LoopsStructure = "tut_loops_structure";      // beat 6
        public const string InspirationEconomy = "tut_inspiration_economy";  // beat 7 (+ scripted draw)
        public const string PlayFinisher = "tut_play_finisher";        // beat 8 (gate: holdLoop)
        // [TUT-R2c] Variante del beat 8 cuando el jugador YA jugó el finisher
        // antes del último loop (degrade (a) con guiño; RT5 feedback).
        public const string PlayFinisherEarly = "tut_play_finisher_early";
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


        // ── [TUT-REDESIGN-B] Gig-1 beats over the band of 4 ──
        public const string PlayBudget = "tut_play_budget";          // beat 7 (ECON-1, on first EconBudget denial — D-TUTB-4=A)
        public const string FinalLoopLock = "tut_final_loop_lock";   // reactive gig 1
        public const string TrackReplaced = "tut_track_replaced";    // reactive gig 1

        // ── [TUT-REDESIGN-B / D-TUTR-3=B] Card-anchored reactives, gigs 2+ (suppressed during the guided gig, D-TUTB-5=A) ──
        public const string EarwormTick = "tut_earworm_tick";        // Sibi
        public const string Captivated = "tut_captivated";           // Zig
        public const string VoltageFirst = "tut_voltage_first";      // Conito
        public const string OverloadReady = "tut_overload_ready";    // Conito
        public const string BonusLoop = "tut_bonus_loop";            // Conito
        public const string Spotlight = "tut_spotlight";             // C2
        public const string ReadTheRoom = "tut_read_the_room";       // Sibi (absorbs tut_audience_preferences; trigger wiring pending O5)
        public const string HarmonyTrack = "tut_harmony_track";      // Zig
        public const string HarmonyDenied = "tut_harmony_denied";    // Zig
        public const string SungMelody = "tut_sung_melody";          // Zig

        // [TUT-REDESIGN-B] tut_audience_preferences → absorbed by ReadTheRoom;
        // tut_flow → absorbed by StatusBuffMusician (Flow-keyed). No reserved ids remain.
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


        // [TUT-TXT-1 / D-TT-1=A] Plain mechanical explanation of this beat: no character
        // voice, no narrative. One per language catalog, same as revisitTitle/pages.
        // Shown INSTEAD of pages when TutorialController.showMechanicText is on
        // (D-TT-2=A); empty ⇒ the controller falls back to pages. Written by
        // GameTextWindow through SerializedObject (no setter, TXT-1 invariant).
        // EditorSeed does not touch it, so re-running a seeder keeps this text.
        [Tooltip("[TUT-TXT-1] Plain mechanical text (no voice). Empty = fall back to pages.")]
        [TextArea(3, 8)]
        [SerializeField] private string mechanicText;

        public string TriggerId => triggerId;
        public int Priority => priority;
        public TutorialCategory Category => category;
        public string RevisitTitle =>
            string.IsNullOrWhiteSpace(revisitTitle) ? triggerId : revisitTitle;
        public Sprite Portrait => portrait;
        public IReadOnlyList<string> Pages => pages;
        public string HighlightKey => highlightKey;
        public bool HasHighlight => !string.IsNullOrWhiteSpace(highlightKey);

        // [TUT-TXT-1] Read-only like every other field. Empty ⇒ caller falls back to Pages.
        public string MechanicText => mechanicText;
        public bool HasMechanicText => !string.IsNullOrWhiteSpace(mechanicText);

#if UNITY_EDITOR

        /// <summary>
        /// [TUT-TXT-1c / D-TT-6=C] Aligns ONLY the structural fields of an existing asset.
        /// Never touches revisitTitle, pages or mechanicText: after D-TT-6=C the .asset is the
        /// sole home of player-facing copy (D1=A), and a seeder that could overwrite it is the
        /// exact mechanism that produced F-TT-3 / F-TT-4. Use EditorSeed() only for an asset
        /// being created from nothing.
        /// </summary>
        public void EditorSeedStructure(string id, int prio, TutorialCategory cat, string highlight)
        {
            triggerId = id;
            priority = prio;
            category = cat;
            highlightKey = highlight;
        }

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
