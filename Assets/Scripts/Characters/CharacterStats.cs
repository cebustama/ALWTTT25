using ALWTTT.Enums;
using System;
using UnityEngine;

namespace ALWTTT.Characters
{
    public abstract class CharacterStats
    {
        protected readonly Action<StatusType, int> OnStatusChanged;
        protected readonly Action<StatusType, int> OnStatusApplied;
        protected readonly Action<StatusType> OnStatusCleared;

        // TODO: How to make virtual but access to Dict?
        protected abstract void SetAllStatus();

        protected abstract void DamagePoison();
        protected abstract void CheckStunStatus();
    }
}