using UnityEngine;

namespace ALWTTT.Interfaces
{
    public interface IMusicianStats : ICharacterStats
    {
        int CurrentStress { get; }
        int MaxStress { get; }
        void SetCurrentStress(int targetCurrentStress, float duration = 1f);
        void AddStress(int amount, float duration = 1f);
        void HealStress(int amount, float duration = 1f);
        int Charm { get; }
        int Technique { get; }
        int Emotion { get; }
    }
}