// Place at: Assets/Scripts/Sensory/EarwormTickEvent.cs
using ALWTTT.Characters.Audience;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [TUT-REDESIGN-B] One Earworm holder processed during the audience turn
    /// (GigManager.AudienceTurnRoutine, the only Earworm Vibe-gain site —
    /// B2.5 D-B2.5-1=A). <see cref="Applied"/> is the Vibe actually taken
    /// (0 when Indifference negated it). <see cref="StacksBeforeDecay"/> is
    /// the stack count read for this tick; decay happens afterwards.
    /// Tutorial gate — tut_earworm_tick: first fire.
    /// </summary>
    public readonly struct EarwormTickEvent : ISensoryEvent
    {
        public AudienceCharacterBase Audience { get; }
        public int Applied { get; }
        public int StacksBeforeDecay { get; }

        public EarwormTickEvent(AudienceCharacterBase audience, int applied, int stacksBeforeDecay)
        {
            Audience = audience;
            Applied = applied;
            StacksBeforeDecay = stacksBeforeDecay;
        }
    }
}