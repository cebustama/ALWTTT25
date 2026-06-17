using ALWTTT.Characters;
using ALWTTT.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Interfaces
{
    public interface ICharacterStats
    {
        void ApplyStatus(StatusType targetStatus, int value);
        void ClearAllStatus();
    }
}