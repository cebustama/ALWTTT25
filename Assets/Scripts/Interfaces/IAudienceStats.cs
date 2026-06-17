using ALWTTT.Data;
using ALWTTT.Status.Runtime;
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

        // [B3] Canonical incoming-Vibe path. Returns amount actually applied
        // (0 when Indifference blocks). See AudienceCharacterStats.ApplyIncomingVibe
        // for full semantics. Mirror of M4.1 ApplyIncomingStressWithComposure.
        int ApplyIncomingVibe(
            StatusEffectContainer statuses,
            int incoming,
            float duration = 2f);

        // Preferences
    }

}