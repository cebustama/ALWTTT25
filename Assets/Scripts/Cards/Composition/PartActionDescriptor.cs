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

        // [CARD-FACE-2 / D-CF-21, D-SP1-6=B] customLabel DELETED (population 12). The part label is
        // derived from `action` at apply time: letters for CreatePart, ui_part_bridge for MarkBridge.
        // Serialized values are dropped, not migrated (SONG-PANEL-1 rule); no shipped card used it.

        [Tooltip("If marking Solo, optionally tie to a musician (by id).")]
        public string musicianId;
    }
}