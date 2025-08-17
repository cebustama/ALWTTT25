using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Characters
{
    public abstract class CharacterStats
    {
        protected CharacterCanvas characterCanvas;

        protected Action<StatusType, int> OnStatusChanged;
        protected Action<StatusType, int> OnStatusApplied;
        protected Action<StatusType> OnStatusCleared;

        // TODO: How to make virtual but access to Dict?
        protected Dictionary<StatusType, StatusStats> statusDict;

        protected abstract void DamagePoison();
        protected abstract void CheckStunStatus();

        protected virtual void Setup(CharacterCanvas canvas, int maxHp)
        {
            characterCanvas = canvas;
            
            OnStatusChanged += characterCanvas.UpdateStatusText;
            OnStatusApplied += characterCanvas.ApplyStatus;
            OnStatusCleared += characterCanvas.ClearStatus;

            SetAllStatus();
        }

        protected virtual void SetAllStatus()
        {
            statusDict = new Dictionary<StatusType, StatusStats>();

            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
            {
                statusDict.Add((StatusType)i, new StatusStats((StatusType)i, 0));
            }

            // TODO: This should be defined in a StatusTypeData asset List
            statusDict[StatusType.Poison].DecreaseOverTurn = true;
            statusDict[StatusType.Poison].OnTriggerAction += DamagePoison;

            statusDict[StatusType.Skeptical].ClearAtNextTurn = true;

            statusDict[StatusType.Strength].CanNegativeStack = true;

            statusDict[StatusType.Stun].DecreaseOverTurn = true;
            statusDict[StatusType.Stun].OnTriggerAction += CheckStunStatus;
        }

        public virtual void TriggerAllStatus()
        {
            Debug.Log("Triggering All Status.");

            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
                TriggerStatus((StatusType)i);
        }

        protected virtual void TriggerStatus(StatusType targetStatus)
        {
            statusDict[targetStatus].OnTriggerAction?.Invoke();

            //One turn only statuses
            if (statusDict[targetStatus].ClearAtNextTurn)
            {
                ClearStatus(targetStatus);
                OnStatusChanged?.Invoke(targetStatus, statusDict[targetStatus].StatusValue);
                return;
            }

            //Check status
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
            statusDict[targetStatus].IsActive = false;
            statusDict[targetStatus].StatusValue = 0;
            OnStatusCleared?.Invoke(targetStatus);
        }

        public virtual void Dispose()
        {
            if (characterCanvas != null)
            {
                OnStatusChanged -= characterCanvas.UpdateStatusText;
                OnStatusApplied -= characterCanvas.ApplyStatus;
                OnStatusCleared -= characterCanvas.ClearStatus;
            }
        }
    }
}