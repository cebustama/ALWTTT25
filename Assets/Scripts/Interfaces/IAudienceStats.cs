using ALWTTT.Data;
using UnityEngine;

namespace ALWTTT.Interfaces
{
    public interface IAudienceStats : ICharacterStats
    {
        bool IsConvinced { get; }
        int CurrentVibe { get; }
        int MaxVibe { get; }
        void AddVibe(int amount, float duration = 2f);
        void RemoveVibe(int amount, float duration = 2f);
        void SetCurrentVibe(int vibe, float duration = 2f);

        // Preferences
    }

}