using ALWTTT.Data;
using UnityEngine;

namespace ALWTTT.Interfaces
{
    public interface IAudienceStats
    {
        int CurrentVibe { get; }
        int MaxVibe { get; }
        void AddVibe(int amount, float duration = 2f);
        void RemoveVibe(int amount, float duration = 2f);
        void SetCurrentVibe(int vibe, float duration = 2f);
        void ApplySongVibe(SongData song, float duration = 2f);
    }

}