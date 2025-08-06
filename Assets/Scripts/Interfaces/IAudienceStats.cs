using UnityEngine;

namespace ALWTTT.Interfaces
{
    public interface IAudienceStats
    {
        int CurrentVibe { get; }
        int MaxVibe { get; }
        void AddVibe(int amount);
        void SetCurrentVibe(int vibe);
    }

}