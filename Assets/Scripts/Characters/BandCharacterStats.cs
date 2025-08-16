using ALWTTT.Enums;
using ALWTTT.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    public class BandCharacterStats : CharacterStats, IMusicianStats
    {
        public int CurrentStress { get; set; }
        public int MaxStress { get; set; }

        public int Charm { get; set; }
        public int Technique { get; set; }
        public int Emotion { get; set; }

        public bool IsBreakdown { get; private set; }

        public Dictionary<StatusType, StatusStats> StatusDict { get; private set; }

        public Action OnBreakdown;
        public Action<int, int> OnStressChanged;

        private BandCharacterCanvas bandCharacterCanvas;

        public override string ToString()
        {
            return $"[Musician Stats] Vibe: {CurrentStress}/{MaxStress}, " +
               $"CHR: {Charm}, THC: {Technique}, EMT: {Emotion}";
        }

        #region Setup
        public BandCharacterStats(int maxStress, BandCharacterCanvas characterCanvas)
        {
            MaxStress = maxStress;
            CurrentStress = 0;

            SetAllStatus();

            bandCharacterCanvas = characterCanvas;
            OnStressChanged += bandCharacterCanvas.UpdateHealthText;
        }
        #endregion

        public void Dispose()
        {
            if (bandCharacterCanvas != null)
            {
                OnStressChanged -= bandCharacterCanvas.UpdateHealthText;
            }
        }

        public void SetCurrentStress(int targetCurrentStress)
        {
            CurrentStress = 
                targetCurrentStress < 0 ? 0 :
                    targetCurrentStress > MaxStress ?
                        MaxStress :
                        targetCurrentStress;

            OnStressChanged?.Invoke(CurrentStress, MaxStress);
        }

        public void AddStress(int amount)
        {
            SetCurrentStress(CurrentStress + amount);
            if (CurrentStress >= MaxStress && !IsBreakdown)
            {
                IsBreakdown = true;
                OnBreakdown?.Invoke();
            }
        }

        public void HealStress(int amount)
        {
            SetCurrentStress(Mathf.Max(0, CurrentStress - amount));
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

            StatusDict[StatusType.Chill].ClearAtNextTurn = true;

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
    }
}