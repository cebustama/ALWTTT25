using ALWTTT.Characters.Band;
using MidiGenPlay;
using MidiGenPlay.Interfaces;
using System;
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

    /// <summary>
    /// Debug/utility: returns the union of all melodic instruments this musician
    /// is allowed to use across the main melodic roles (Backing, Bassline,
    /// Melody, Harmony). Uses the same filtering rules as GetPermittedMelodic.
    /// </summary>
    public static IReadOnlyList<MIDIInstrumentSO> GetPermittedMelodicAllRoles(
        MusicianBase musician,
        IInstrumentRepository instrumentRepo)
    {
        if (instrumentRepo == null)
            return Array.Empty<MIDIInstrumentSO>();

        // Roles we consider when building a "global" picker for this musician
        var roles = new[]
        {
            TrackRole.Backing,
            TrackRole.Bassline,
            TrackRole.Melody,
            TrackRole.Harmony
        };

        var result = new HashSet<MIDIInstrumentSO>();

        foreach (var role in roles)
        {
            var pool = GetPermittedMelodic(musician, role, instrumentRepo);
            if (pool == null) continue;

            foreach (var inst in pool)
            {
                if (inst != null)
                    result.Add(inst);
            }
        }

        return result.ToList();
    }
}
