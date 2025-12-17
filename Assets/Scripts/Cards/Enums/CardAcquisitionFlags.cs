using System;

namespace ALWTTT.Cards
{
    [Flags]
    public enum CardAcquisitionFlags
    {
        None = 0,

        // Where it shows up
        StarterDeck = 1 << 0,
        RewardPool = 1 << 1,

        // Progression defaults
        UnlockedByDefault = 1 << 2
    }
}
