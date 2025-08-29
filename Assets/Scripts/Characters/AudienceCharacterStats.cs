using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Characters.Audience
{
    public class AudienceCharacterStats : CharacterStats, IAudienceStats
    {
        public int MaxVibe { get; set; } // "HP"
        public int CurrentVibe { get; private set; }
        public bool IsConvinced { get; private set; } // "Death"
        public bool IsStunned { get; private set; }

        public Action OnConvinced;
        public Action<int, int> OnVibeChanged;

        public Dictionary<StatusType, StatusStats> statusDict => base.statusDict;

        public override string ToString()
        {
            return $"[Audience Stats] Vibe: {CurrentVibe}/{MaxVibe}, " +
                $"IsConvinced: {IsConvinced}";
        }

        #region Setup
        public AudienceCharacterStats(int maxVibe, CharacterCanvas canvas)
        {
            Setup(canvas, maxVibe);
        }

        protected override void Setup(CharacterCanvas canvas, int maxHp)
        {
            base.Setup(canvas, maxHp);

            MaxVibe = maxHp;
            CurrentVibe = 0;

            OnVibeChanged += characterCanvas.UpdateHealthText;
        }
        #endregion

        #region Public Methods
        public override void Dispose()
        {
            base.Dispose();

            if (characterCanvas != null)
            {
                OnVibeChanged -= characterCanvas.UpdateHealthText;
            }
        }

        public void SetCurrentVibe(int targetCurrentVibe, float duration = 2f)
        {
            CurrentVibe =
                targetCurrentVibe < 0 ? 0 :
                    targetCurrentVibe > MaxVibe ?
                        MaxVibe :
                        targetCurrentVibe;

            characterCanvas.SetCurrentVibe(targetCurrentVibe, MaxVibe, duration);
            characterCanvas.UpdateVisibility();

            OnVibeChanged?.Invoke(CurrentVibe, MaxVibe);
        }

        public void AddVibe(int amount, float duration = 2f)
        {
            SetCurrentVibe(CurrentVibe + amount, duration);
            if (CurrentVibe >= MaxVibe && !IsConvinced)
            {
                IsConvinced = true;

                ApplyStatus(StatusType.Convinced, 1);
                ClearStatus(StatusType.Tall);

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
        
        public void ApplyStatus(StatusType targetStatus, int value)
        {
            if (statusDict[targetStatus].IsActive)
            {
                statusDict[targetStatus].StatusValue += value;
                OnStatusChanged?.Invoke(targetStatus, statusDict[targetStatus].StatusValue);
            }
            else
            {
                statusDict[targetStatus].StatusValue = value;
                statusDict[targetStatus].IsActive = true;
                OnStatusApplied?.Invoke(targetStatus, statusDict[targetStatus].StatusValue);
            }
        }

        protected override void DamagePoison()
        {
            throw new NotImplementedException();
        }

        protected override void CheckStunStatus()
        {
            throw new NotImplementedException();
        }

        protected override void TriggerStatus(StatusType targetStatus)
        {
            
        }

        #endregion
    }
}