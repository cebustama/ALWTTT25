// Place at: Assets/Scripts/Tutorial/TutorialInputGate.cs
using System;
using ALWTTT.Cards;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [TUT-R2 / D4] Cooperative DIRECTIVE gate for the guided gig-1 beats 3 and 5.
    /// Unlike <see cref="TutorialModalGate"/> (which suspends gameplay under a
    /// modal), this gate restricts INPUT while gameplay keeps running: an
    /// allow-list of permitted actions until the taught action is performed.
    ///
    /// Modes:
    ///   CompositionOnly (beat 3) — only composition-card drags allowed; action
    ///     card drags, Play, and End Turn blocked.
    ///   PlayOnly (beat 5) — all card drags blocked; only the Play button allowed;
    ///     End Turn blocked.
    ///
    /// Consumers cooperate (same pattern as TutorialModalGate):
    ///   - HandController checks <see cref="BlocksCardDrag"/> at drag start.
    ///   - GigManager.OnPlayPressed checks <see cref="BlocksPlay"/> and calls
    ///     <see cref="NotifyPlayPressed"/> when Play actually proceeds.
    ///   - GigManager.EndTurn checks <see cref="BlocksEndTurn"/>.
    ///
    /// Set/cleared ONLY by TutorialGuidedDriver. Driver clears defensively on
    /// disable so no gate outlives its scene.
    /// </summary>
    public static class TutorialInputGate
    {
        public enum GateMode
        {
            None = 0,
            CompositionOnly = 1, // beat 3: drag composition cards only
            PlayOnly = 2         // beat 5: press Play only
        }

        public static GateMode Mode { get; private set; } = GateMode.None;
        public static bool IsActive => Mode != GateMode.None;

        /// <summary>Raised from GigManager.OnPlayPressed when Play proceeds
        /// (i.e. was not blocked). The guided driver satisfies beat 5 on this.</summary>
        public static event Action PlayPressed;

        /// <summary>Called only by TutorialGuidedDriver.</summary>
        public static void Set(GateMode mode) => Mode = mode;

        /// <summary>Called only by TutorialGuidedDriver (and defensively on its disable).</summary>
        public static void Clear() => Mode = GateMode.None;

        /// <summary>True when starting a drag on this card must be denied.
        /// Null-safe: a null definition is blocked while any gate is active.</summary>
        public static bool BlocksCardDrag(CardDefinition def)
        {
            switch (Mode)
            {
                case GateMode.CompositionOnly:
                    return def == null || !def.IsComposition;
                case GateMode.PlayOnly:
                    return true;
                default:
                    return false;
            }
        }

        public static bool BlocksPlay => Mode == GateMode.CompositionOnly;

        public static bool BlocksEndTurn => IsActive;

        /// <summary>Invoked by GigManager.OnPlayPressed AFTER the BlocksPlay check
        /// passes, so subscribers only observe real (allowed) Play presses.</summary>
        public static void NotifyPlayPressed() => PlayPressed?.Invoke();
    }
}