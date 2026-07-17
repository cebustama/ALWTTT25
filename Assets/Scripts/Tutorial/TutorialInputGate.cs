// Place at: Assets/Scripts/Tutorial/TutorialInputGate.cs
// [TUT-R2c] v2 — REEMPLAZA el archivo TUT-R2. Añade el allow-list de
// composiciones "básicas" para el beat 3: el gate CompositionOnly puede
// restringirse a un subconjunto de ids (las cartas de la mano forzada), de modo
// que cartas modificadoras (Key Lift, Push It, Half Time) queden bloqueadas
// durante la primera lección aunque caigan en el 5º robo. Allow-list null =
// comportamiento TUT-R2 (cualquier composición).
using System;
using System.Collections.Generic;
using ALWTTT.Cards;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [TUT-R2 / D4] Cooperative DIRECTIVE gate for the guided gig-1 beats 3
    /// and 5. See TUT-R2 header notes; v2 adds the beat-3 allow-list.
    /// </summary>
    public static class TutorialInputGate
    {
        public enum GateMode
        {
            None = 0,
            CompositionOnly = 1, // beat 3: drag (allow-listed) composition cards only
            PlayOnly = 2,        // beat 5: press Play only
            SingleCardOnly = 3   // [CARD-UX-1] beat 8: only ONE card id is playable
        }

        public static GateMode Mode { get; private set; } = GateMode.None;
        public static bool IsActive => Mode != GateMode.None;

        // [TUT-R2c] ids permitidos bajo CompositionOnly; null = todas las
        // composiciones. Comparación case-insensitive contra CardDefinition.Id.
        private static HashSet<string> _compositionAllowList;

        /// <summary>Raised from GigManager.OnPlayPressed when Play proceeds.</summary>
        public static event Action PlayPressed;

        /// <summary>Called only by TutorialGuidedDriver.</summary>
        public static void Set(GateMode mode) => Mode = mode;

        // [CARD-UX-1] Id permitido bajo SingleCardOnly (beat 8 = finisher).
        private static string _singleCardId;

        /// <summary>[CARD-UX-1] Arm SingleCardOnly for one card id. The CALLER must
        /// guarantee the card is in hand: a gate whose only allowed card is absent
        /// leaves zero playable cards (soft-lock inside a held loop).</summary>
        public static void SetSingleCard(string cardId)
        {
            _singleCardId = cardId;
            Mode = string.IsNullOrWhiteSpace(cardId)
                ? GateMode.None : GateMode.SingleCardOnly;
        }

        /// <summary>[TUT-R2c] Arm CompositionOnly restricted to specific card
        /// ids ("basic" compositions). Null/empty ⇒ all compositions allowed.</summary>
        public static void SetCompositionAllowList(IEnumerable<string> cardIds)
        {
            if (cardIds == null) { _compositionAllowList = null; return; }
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var id in cardIds)
                if (!string.IsNullOrWhiteSpace(id)) set.Add(id);
            _compositionAllowList = set.Count > 0 ? set : null;
        }

        public static void Clear()
        {
            Mode = GateMode.None;
            _compositionAllowList = null;
            _singleCardId = null;              // [CARD-UX-1]
        }

        public static bool BlocksCardDrag(CardDefinition def)
        {
            switch (Mode)
            {
                case GateMode.CompositionOnly:
                    if (def == null || !def.IsComposition) return true;
                    return _compositionAllowList != null &&
                           !_compositionAllowList.Contains(def.Id);
                case GateMode.PlayOnly:
                    return true;
                case GateMode.SingleCardOnly:
                    return def == null || !string.Equals(
                        def.Id, _singleCardId, StringComparison.OrdinalIgnoreCase);
                default:
                    return false;
            }
        }

        public static bool BlocksPlay => Mode == GateMode.CompositionOnly;

        // [CARD-UX-1 / D6=A] SingleCardOnly's scope is card drag ONLY. It does not
        // block End Turn: the loop hold already prevents the song from advancing,
        // and leaving End Turn open preserves an escape hatch in the held loop.
        public static bool BlocksEndTurn =>
            Mode == GateMode.CompositionOnly || Mode == GateMode.PlayOnly;

        /// <summary>Invoked by GigManager.OnPlayPressed AFTER the BlocksPlay
        /// check passes.</summary>
        public static void NotifyPlayPressed() => PlayPressed?.Invoke();
    }
}