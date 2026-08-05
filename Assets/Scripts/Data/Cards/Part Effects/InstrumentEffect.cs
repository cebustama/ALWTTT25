using MidiGenPlay;
using System.Collections.Generic;
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
            InstrumentType,       // pick any instrument of this type later
            RandomFromList,       // [R2c / D-R2-7] pick ONE from melodicInstrumentPool at card application
        }

        [Header("Instrument Effect")]
        public InstrumentTargetMode mode = InstrumentTargetMode.InstrumentType;

        [Tooltip("Used when Mode == SpecificMelodic")]
        public MIDIInstrumentSO melodicInstrument;

        [Tooltip("Used when Mode == SpecificPercussion")]
        public MIDIPercussionInstrumentSO percussionInstrument;

        [Tooltip("Used when Mode == InstrumentType")]
        public InstrumentType instrumentType;

        [Tooltip("Used when Mode == RandomFromList. Pool of melodic " +
                 "instruments: ONE is picked (uniform) when the card is " +
                 "applied and persists on the track as a normal specific " +
                 "override — deterministic and cache-coherent downstream. " +
                 "Multi-track musicians receive a single coherent pick. " +
                 "Null entries are ignored; an empty pool applies nothing " +
                 "and logs a warning at apply time.")]
        public List<MIDIInstrumentSO> melodicInstrumentPool = new();

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
                case InstrumentTargetMode.RandomFromList:
                    {
                        // [R2c / D-R2-7] Count only usable entries so the label
                        // matches what the apply path will actually roll over.
                        int usable = 0;
                        if (melodicInstrumentPool != null)
                            for (int i = 0; i < melodicInstrumentPool.Count; i++)
                                if (melodicInstrumentPool[i] != null) usable++;
                        return usable > 0
                            ? $"Random Instrument ({usable})"
                            : "Random Instrument (empty pool)";
                    }
                default:
                    return "Instrument Effect";
            }
        }
    }
}