// Place at: Assets/Scripts/Sensory/BonusLoopStartedEvent.cs
namespace ALWTTT.Sensory
{
    /// <summary>
    /// [TUT-REDESIGN-B] The loop that is about to play is a bonus loop
    /// (R5-d: granted by Overload; band ducks, solo on top). Published from
    /// CompositionSession at the moment <c>_currentLoopIsBonus</c> flips true
    /// for the loop, before the render call.
    /// Tutorial gate — tut_bonus_loop: first fire.
    /// </summary>
    public readonly struct BonusLoopStartedEvent : ISensoryEvent
    {
        public int PartIndex { get; }
        public BonusLoopStartedEvent(int partIndex) { PartIndex = partIndex; }
    }
}