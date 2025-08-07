using ALWTTT.Interfaces;
using System;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    public class BandCharacterStats : IMusicianStats
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

        private void SetAllStatus()
        {
            // TODO
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

        public void ApplyBlock(int amount)
        {
            // Add block to a status system here
        }
    }
}