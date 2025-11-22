using MidiGenPlay;
using UnityEngine;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(
        fileName = "InstrumentEffect_",
        menuName = "ALWTTT/Composition/Instrument Effect")]
    public class InstrumentEffect : PartEffect
    {
        public enum InstrumentTargetMode
        {
            SpecificMelodic,      // use a concrete MIDIInstrumentSO
            SpecificPercussion,   // use a concrete MIDIPercussionInstrumentSO
            InstrumentType        // pick any instrument of this type later
        }

        [Header("Instrument Effect")]
        public InstrumentTargetMode mode = InstrumentTargetMode.InstrumentType;

        [Tooltip("Used when Mode == SpecificMelodic")]
        public MIDIInstrumentSO melodicInstrument;

        [Tooltip("Used when Mode == SpecificPercussion")]
        public MIDIPercussionInstrumentSO percussionInstrument;

        [Tooltip("Used when Mode == InstrumentType")]
        public InstrumentType instrumentType;

        public override string GetLabel()
        {
            switch (mode)
            {
                case InstrumentTargetMode.SpecificMelodic:
                    return melodicInstrument != null
                        ? $"Set Instrument: {melodicInstrument.InstrumentName}"
                        : "Set Instrument (Melodic)";
                case InstrumentTargetMode.SpecificPercussion:
                    return percussionInstrument != null
                        ? $"Set Drums: {percussionInstrument.InstrumentName}"
                        : "Set Drums";
                case InstrumentTargetMode.InstrumentType:
                    return $"Bias Instrument Type: {instrumentType}";
                default:
                    return "Instrument Effect";
            }
        }
    }
}