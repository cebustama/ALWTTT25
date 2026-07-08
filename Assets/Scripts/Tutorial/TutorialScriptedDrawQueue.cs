// Place at: Assets/Scripts/Tutorial/TutorialScriptedDrawQueue.cs
using System;
using System.Collections.Generic;
using ALWTTT.Cards;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [TUT-R2 / D1=B] Scripted draw queue layered OVER the M4.5 filtered-draw
    /// seam. DeckManager.DrawCardsForPlayerTurn consumes this queue FIRST (each
    /// consumed entry uses one budget slot), before its Phase 1 normal draws and
    /// the Phase 2 M4.5 guarantees — which therefore remain the final fallback
    /// when the queue is unsatisfiable (D1 degrade path).
    ///
    /// Each entry is two-stage: a Primary predicate (typically an exact card-id
    /// match) and an optional Fallback predicate (typically a role/domain match).
    /// A miss on both stages simply drops the entry — the slot returns to the
    /// normal draw budget.
    ///
    /// Static (mirrors TutorialModalGate) so it is immune to Awake/Start ordering
    /// between the driver and the GigManager phase machine: the driver fills it
    /// in Awake, DeckManager drains it at the first PlayerTurn draw. Cleared
    /// defensively when the driver disables.
    /// </summary>
    public static class TutorialScriptedDrawQueue
    {
        public sealed class Entry
        {
            public readonly string Label;
            public readonly Func<CardDefinition, bool> Primary;
            public readonly Func<CardDefinition, bool> Fallback;

            public Entry(string label,
                         Func<CardDefinition, bool> primary,
                         Func<CardDefinition, bool> fallback = null)
            {
                Label = label ?? "scripted";
                Primary = primary ?? (_ => false);
                Fallback = fallback;
            }
        }

        private static readonly Queue<Entry> _queue = new Queue<Entry>();

        public static int Count => _queue.Count;

        public static void Enqueue(Entry entry)
        {
            if (entry != null) _queue.Enqueue(entry);
        }

        public static bool TryDequeue(out Entry entry)
        {
            if (_queue.Count > 0) { entry = _queue.Dequeue(); return true; }
            entry = null;
            return false;
        }

        public static void Clear() => _queue.Clear();

        /// <summary>Convenience: id match by CardDefinition.Id OR asset name
        /// (starter assets are authored with matching names; robust to either).</summary>
        public static Func<CardDefinition, bool> ById(string cardId) =>
            c => c != null &&
                 (string.Equals(c.Id, cardId, StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(c.name, cardId, StringComparison.OrdinalIgnoreCase));
    }
}