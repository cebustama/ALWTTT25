// Place at: Assets/Scripts/Data/Cards/Part Effects/MeterEffect.cs
// FULL REPLACEMENT (CARD-FACE-1). Built on the repo copy attached 2026-09-16 (R4).
// [D-SP1-6=B, strong form] `meterLabel` (asset text, population 12) is DELETED: the asset keeps the
// DATUM (timeSignature) and the player text is derived from it (ui_ctx_meter_<value>), so "5/4"
// has one home. A MeterEffect asset whose label disagreed with its enum can no longer hide it.
// GetLabel() is a DEVELOPER label since D-CF-5=B (inspector / editor window only).
using UnityEngine;

using MidiGenPlay.MusicTheory;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(
        fileName = "MeterEffect_",
        menuName = "ALWTTT/Composition/Meter Effect")]
    public sealed class MeterEffect : PartEffect
    {
        public MusicTheory.TimeSignature timeSignature = MusicTheory.TimeSignature.FourFour;
        public override string GetLabel() => $"Meter {timeSignature}";   // developer label
    }
}