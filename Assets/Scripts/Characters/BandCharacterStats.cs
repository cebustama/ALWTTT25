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

        public Action OnBreakdown;
        public Action<int, int> OnStressChanged;

        private BandCharacterCanvas bandCharacterCanvas;

        public override string ToString()
        {
            return $"[Musician Stats] Stress: {CurrentStress}/{MaxStress}, " +
               $"CHR: {Charm}, THC: {Technique}, EMT: {Emotion}";
        }

        #region Setup
        public BandCharacterStats(int chr, int tch, int emt, 
            int maxStress, BandCharacterCanvas characterCanvas)
        {
            Charm = chr;
            Technique = tch;
            Emotion = emt;

            bandCharacterCanvas = characterCanvas;
            Setup(characterCanvas, maxStress);
        }

        protected override void Setup(CharacterCanvas canvas, int maxHp)
        {
            base.Setup(canvas, maxHp);

            MaxStress = maxHp;
            CurrentStress = 0;

            OnStressChanged += bandCharacterCanvas.UpdateHealthText;
        }
        #endregion

        public override void Dispose()
        {
            base.Dispose();
            if (bandCharacterCanvas != null)
            {
                OnStressChanged -= bandCharacterCanvas.UpdateHealthText;
            }
        }

        public void SetCurrentStress(int targetCurrentStress, float duration = 1f)
        {
            CurrentStress = 
                targetCurrentStress < 0 ? 0 :
                    targetCurrentStress > MaxStress ?
                        MaxStress :
                        targetCurrentStress;

            bandCharacterCanvas.SetCurrentStress(CurrentStress, MaxStress, duration);
            bandCharacterCanvas.UpdateVisibility();

            OnStressChanged?.Invoke(CurrentStress, MaxStress);
        }

        public void AddStress(int amount, float duration = 1f)
        {
            SetCurrentStress(CurrentStress + amount, duration);
            if (CurrentStress >= MaxStress && !IsBreakdown)
            {
                IsBreakdown = true;
                OnBreakdown?.Invoke();
            }
        }

        public void HealStress(int amount, float duration = 1f)
        {
            SetCurrentStress(Mathf.Max(0, CurrentStress - amount), duration);
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
    }
}