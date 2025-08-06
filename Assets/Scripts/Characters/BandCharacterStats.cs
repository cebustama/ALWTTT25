using ALWTTT.Interfaces;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    public class BandCharacterStats : IMusicianStats
    {
        public int CurrentStress { get; private set; }
        public int MaxStress { get; private set; }

        public int Charm { get; private set; }
        public int Technique { get; private set; }
        public int Emotion { get; private set; }

        public void HealStress(int amount)
        {
            CurrentStress = Mathf.Max(0, CurrentStress - amount);
        }

        public void ApplyBlock(int amount)
        {
            // Add block to a status system here
        }
    }
}