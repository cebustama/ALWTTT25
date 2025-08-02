using System;
using UnityEngine;

namespace ALWTTT.Characters.Audience
{
    public class AudienceCharacterStats
    {
        public int MaxVibe { get; set; } // "HP"
        public int CurrentVibe { get; private set; }
        public bool IsConvinced { get; private set; } // "Death"

        public Action OnConvinced;
        public Action<int, int> OnVibeChanged;
        // TODO: Statuses

        private CharacterCanvas characterCanvas;

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
        // TODO: Move to CharacterStats base class as virtual
        public void Dispose()
        {
            if (characterCanvas != null)
            {
                OnVibeChanged -= characterCanvas.UpdateHealthText;
            }
        }

        // TODO: Move to CharacterStats base class
        public void TriggerAllStatus()
        {

        }

        public void SetCurrentVibe(int targetCurrentVibe)
        {
            CurrentVibe =
                targetCurrentVibe < 0 ? 0 :
                    targetCurrentVibe > MaxVibe ?
                        MaxVibe :
                        targetCurrentVibe;

            OnVibeChanged?.Invoke(CurrentVibe, MaxVibe);
        }
        #endregion
    }
}