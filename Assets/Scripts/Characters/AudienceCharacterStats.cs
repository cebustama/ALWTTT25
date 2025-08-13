using ALWTTT.Data;
using ALWTTT.Interfaces;
using System;
using UnityEngine;

namespace ALWTTT.Characters.Audience
{
    public class AudienceCharacterStats : IAudienceStats
    {
        public int MaxVibe { get; set; } // "HP"
        public int CurrentVibe { get; private set; }
        public bool IsConvinced { get; private set; } // "Death"
        public bool IsStunned { get; private set; }

        public Action OnConvinced;
        public Action<int, int> OnVibeChanged;
        // TODO: Statuses

        private CharacterCanvas characterCanvas;

        public override string ToString()
        {
            return $"[Audience Stats] Vibe: {CurrentVibe}/{MaxVibe}, " +
                $"IsConvinced: {IsConvinced}";
        }

        #region Setup
        public AudienceCharacterStats(int maxVibe, CharacterCanvas characterCanvas)
        {
            MaxVibe = maxVibe;
            CurrentVibe = 0;

            SetAllStatus();

            this.characterCanvas = characterCanvas;
            OnVibeChanged += this.characterCanvas.UpdateHealthText;
        }

        private void SetAllStatus()
        {
            // TODO
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            if (characterCanvas != null)
            {
                OnVibeChanged -= characterCanvas.UpdateHealthText;
            }
        }

        public void TriggerAllStatus()
        {

        }

        public void SetCurrentVibe(int targetCurrentVibe, float duration = 2f)
        {
            CurrentVibe =
                targetCurrentVibe < 0 ? 0 :
                    targetCurrentVibe > MaxVibe ?
                        MaxVibe :
                        targetCurrentVibe;

            characterCanvas.SetCurrentVibe(targetCurrentVibe, MaxVibe, duration);

            OnVibeChanged?.Invoke(CurrentVibe, MaxVibe);
        }

        public void AddVibe(int amount, float duration = 2f)
        {
            SetCurrentVibe(CurrentVibe + amount, duration);
            if (CurrentVibe >= MaxVibe && !IsConvinced)
            {
                IsConvinced = true;
                OnConvinced?.Invoke();
            }
        }

        public void RemoveVibe(int amount, float duration = 2f)
        {
            SetCurrentVibe(CurrentVibe - amount, duration);
        }

        public void ApplySongVibe(SongData song, float duration = 2f)
        {
            // TODO: Take into account Audience Member preferences/stats
            var vibeToAdd = song.GetSongBaseVibe();
            AddVibe(vibeToAdd, duration);
        }
        #endregion
    }
}