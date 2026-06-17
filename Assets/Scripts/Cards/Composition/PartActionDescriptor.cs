using ALWTTT.Cards;
using System;
using UnityEngine;

namespace ALWTTT.Cards
{
    /// <summary>
    /// Describes a structural action upon Parts (create, mark as Intro/Solo/Outro/Bridge, etc.).
    /// </summary>
    [Serializable]
    public class PartActionDescriptor
    {
        public PartActionKind action = PartActionKind.CreatePart;

        [Tooltip("Optional custom label for created/marked part (e.g., 'Part B', 'Bridge').")]
        public string customLabel;

        [Tooltip("If marking Solo, optionally tie to a musician (by id).")]
        public string musicianId;
    }
}