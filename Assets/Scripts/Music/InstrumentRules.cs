using ALWTTT.Characters.Band;
using MidiGenPlay;
using MidiGenPlay.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class InstrumentRules
{
    public static IEnumerable<MIDIInstrumentSO> GetPermittedMelodic(
        MusicianBase musician,
        TrackRole role,
        IInstrumentRepository instrumentRepo)
    {
        var allMelodic = instrumentRepo.GetMelodicInstruments();

        if (musician?.MusicianCharacterData?.Profile == null)
            return allMelodic;

        var prof = musician.MusicianCharacterData.Profile;

        List<InstrumentType> primary, secondary;
        switch (role)
        {
            case TrackRole.Backing:
            case TrackRole.Bassline:
                primary = prof.backingInstruments;
                secondary = prof.leadInstruments;
                break;
            case TrackRole.Melody:
            case TrackRole.Harmony:
                primary = prof.leadInstruments;
                secondary = prof.backingInstruments;
                break;
            default:
                primary = prof.backingInstruments;
                secondary = prof.leadInstruments;
                break;
        }

        IEnumerable<MIDIInstrumentSO> FilterBy(List<InstrumentType> list) =>
            (list == null || list.Count == 0)
                ? Enumerable.Empty<MIDIInstrumentSO>()
                : allMelodic.Where(i => list.Contains(i.InstrumentType));

        var filtered = FilterBy(primary).ToList();
        if (filtered.Count == 0) filtered = FilterBy(secondary).ToList();

        return filtered.Count > 0 ? filtered : allMelodic;
    }
}
