using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Characters
{
    public abstract class CharacterStats
    {
        protected CharacterCanvas characterCanvas;

        // Legacy status delegates — retained for non-icon legacy callers.
        // Icon display is now driven by StatusEffectContainer events → CharacterCanvas directly.
        protected Action<StatusType, int> OnStatusChanged;
        protected Action<StatusType, int> OnStatusApplied;
        protected Action<StatusType> OnStatusCleared;

        protected Dictionary<StatusType, StatusStats> statusDict;

        protected abstract void DamagePoison();
        protected abstract void CheckStunStatus();

        protected virtual void Setup(CharacterCanvas canvas, int maxHp)
        {
            characterCanvas = canvas;

            // M1.2: Icon wiring removed. CharacterCanvas now subscribes to
            // StatusEffectContainer events directly via BindStatusContainer().
            // Legacy delegates remain available for any non-icon consumer.

            SetAllStatus();
        }

        protected virtual void SetAllStatus()
        {
            statusDict = new Dictionary<StatusType, StatusStats>();

            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
            {
                statusDict.Add((StatusType)i, new StatusStats((StatusType)i, 0));
            }

            statusDict[StatusType.Skeptical].ClearAtNextTurn = true;
            statusDict[StatusType.Strength].CanNegativeStack = true;
            statusDict[StatusType.Breakdown].ClearAtNextTurn = true;
            statusDict[StatusType.Breakdown].OnTriggerAction += CheckStunStatus;
        }

        public virtual void TriggerAllStatus()
        {
            Debug.Log("Triggering All Status.");

            var wasStunned = statusDict.ContainsKey(StatusType.Breakdown);

            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
                TriggerStatus((StatusType)i);
        }

        protected virtual void TriggerStatus(StatusType targetStatus)
        {
            statusDict[targetStatus].OnTriggerAction?.Invoke();

            if (statusDict[targetStatus].ClearAtNextTurn)
            {
                ClearStatus(targetStatus);
                OnStatusChanged?.Invoke(targetStatus, statusDict[targetStatus].StatusValue);
                return;
            }

            if (statusDict[targetStatus].StatusValue <= 0)
            {
                if (statusDict[targetStatus].CanNegativeStack)
                {
                    if (statusDict[targetStatus].StatusValue == 0
                        && !statusDict[targetStatus].IsPermanent)
                        ClearStatus(targetStatus);
                }
                else
                {
                    if (!statusDict[targetStatus].IsPermanent)
                        ClearStatus(targetStatus);
                }
            }

            if (statusDict[targetStatus].DecreaseOverTurn)
                statusDict[targetStatus].StatusValue--;

            if (statusDict[targetStatus].StatusValue == 0)
                if (!statusDict[targetStatus].IsPermanent)
                    ClearStatus(targetStatus);

            OnStatusChanged?.Invoke(targetStatus, statusDict[targetStatus].StatusValue);
        }

        public void ClearAllStatus()
        {
            foreach (var status in statusDict)
                ClearStatus(status.Key);
        }

        public virtual void ClearStatus(StatusType targetStatus)
        {
            if (!statusDict.ContainsKey(targetStatus)) return;

            statusDict[targetStatus].IsActive = false;
            statusDict[targetStatus].StatusValue = 0;
            OnStatusCleared?.Invoke(targetStatus);
        }

        public virtual void Dispose()
        {
            // M1.2: No icon delegates to unsubscribe.
            // Legacy delegates are cleared implicitly when the stats object is GC'd.
        }
    }
}