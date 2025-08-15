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
        // TODO: Statuses

        private CharacterCanvas characterCanvas;

        public Dictionary<StatusType, StatusStats> StatusDict { get; private set; }

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

        protected override void SetAllStatus()
        {
            StatusDict = new Dictionary<StatusType, StatusStats>();

            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
            {
                StatusDict.Add((StatusType)i, new StatusStats((StatusType)i, 0));
            }

            StatusDict[StatusType.Poison].DecreaseOverTurn = true;
            StatusDict[StatusType.Poison].OnTriggerAction += DamagePoison;

            StatusDict[StatusType.BlockVibe].ClearAtNextTurn = true;

            StatusDict[StatusType.Strength].CanNegativeStack = true;

            StatusDict[StatusType.Stun].DecreaseOverTurn = true;
            StatusDict[StatusType.Stun].OnTriggerAction += CheckStunStatus;
        }
        
        public void ApplyStatus(StatusType targetStatus, int value)
        {
            if (StatusDict[targetStatus].IsActive)
            {
                StatusDict[targetStatus].StatusValue += value;
                OnStatusChanged?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
            }
            else
            {
                StatusDict[targetStatus].StatusValue = value;
                StatusDict[targetStatus].IsActive = true;
                OnStatusApplied?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
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

        #endregion
    }
}