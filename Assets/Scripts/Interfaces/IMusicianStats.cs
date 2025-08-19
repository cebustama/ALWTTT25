using UnityEngine;

namespace ALWTTT.Interfaces
{
    public interface IMusicianStats : ICharacterStats
    {
        int CurrentStress { get; }
        int MaxStress { get; }
        void SetCurrentStress(int targetCurrentStress, float duration = 1f);
        void AddStress(int amount);
        void HealStress(int amount);
        int Charm { get; }
        int Technique { get; }
        int Emotion { get; }
    }
}