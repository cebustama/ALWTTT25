using UnityEngine;

namespace ALWTTT.Interfaces
{
    public interface IMusicianStats
    {
        int CurrentStress { get; }
        int MaxStress { get; }
        void SetCurrentStress(int targetCurrentStress);
        void AddStress(int amount);
        void HealStress(int amount);
        void ApplyBlock(int amount);
        int Charm { get; }
        int Technique { get; }
        int Emotion { get; }
    }
}