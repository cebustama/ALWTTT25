using System;
using UnityEngine;

namespace ALWTTT.Cards
{
    [Serializable]
    public class MusicianCardEntry
    {
        public CardDefinition card;

        [Tooltip("How this card is obtained/used for this musician.")]
        public CardAcquisitionFlags flags = CardAcquisitionFlags.UnlockedByDefault;

        [Tooltip("Optional progression key used to unlock this card (design-time).")]
        public string unlockId; // eg RandomEvent or Quest

        [Min(1)]
        public int starterCopies = 1;

        public bool IsStarter => (flags & CardAcquisitionFlags.StarterDeck) != 0;
        public bool IsReward => (flags & CardAcquisitionFlags.RewardPool) != 0;
        public bool UnlockedByDefault => 
            (flags & CardAcquisitionFlags.UnlockedByDefault) != 0;
    }
}
