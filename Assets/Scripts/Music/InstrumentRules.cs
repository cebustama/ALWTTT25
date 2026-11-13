using ALWTTT.Characters.Band;
using MidiGenPlay;
using MidiGenPlay.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class InstrumentRules
{
    /// <summary>
    /// Returns the melodic instruments this musician is permitted to use for
    /// the given role.
    ///
    /// Precedence (top-down):
    ///   1. [D-Sibi-Pool=A, 2026-05-20] Role-specific SO whitelist on
    ///      <see cref="ALWTTT.Musicians.MusicianProfileData"/>:
    ///        - Backing / Bassline → <c>backingMelodicInstruments</c>
    ///        - Melody / Harmony   → <c>leadMelodicInstruments</c>
    ///      If the role's whitelist is non-empty and at least one entry is
    ///      present in the global melodic catalog, the result is restricted
    ///      to that intersection.
    ///   2. Existing <see cref="InstrumentType"/>-based filter (unchanged
    ///      behavior). Per-role primary/secondary type lists with fallback
    ///      to <c>allMelodic</c>.
    ///
    /// An empty SO whitelist for a given role means "no opinion" — the
    /// pre-existing type-filter chain runs unchanged. There is NO cross-role
    /// whitelist fallback (an empty backing whitelist does not fall through
    /// to the lead whitelist or vice versa).
    /// </summary>
    public static IEnumerable<MIDIInstrumentSO> GetPermittedMelodic(
        MusicianBase musician,
        TrackRole role,
        IInstrumentRepository instrumentRepo)
        => GetPermittedMelodic(musician?.MusicianCharacterData, role, instrumentRepo);

    /// <summary>[MGP-BENCH-1] Same rule from the data asset alone, so editor
    /// tooling (the card bench, no scene) resolves exactly what the gig does.</summary>
    public static IEnumerable<MIDIInstrumentSO> GetPermittedMelodic(
        ALWTTT.Musicians.MusicianCharacterData data,
        TrackRole role,
        IInstrumentRepository instrumentRepo)
    {
        var allMelodic = instrumentRepo.GetMelodicInstruments();

        if (data?.Profile == null)
            return allMelodic;

        var prof = data.Profile;

        // ── [D-Sibi-Pool=A] SO whitelist (per role, opt-in) ──
        // If the role's SO whitelist is populated, restrict to its
        // intersection with the global melodic catalog. Empty list →
        // skip this layer and fall through to the existing type-filter
        // chain (no behavior change vs prior).
        List<MIDIInstrumentSO> roleWhitelist;
        switch (role)
        {
            case TrackRole.Backing:
            case TrackRole.Bassline:
                roleWhitelist = prof.backingMelodicInstruments;
                break;
            case TrackRole.Melody:
            case TrackRole.Harmony:
                roleWhitelist = prof.leadMelodicInstruments;
                break;
            default:
                roleWhitelist = prof.backingMelodicInstruments;
                break;
        }

        if (roleWhitelist != null && roleWhitelist.Count > 0)
        {
            var soFiltered = allMelodic
                .Where(i => i != null && roleWhitelist.Contains(i))
                .ToList();
            if (soFiltered.Count > 0)
                return soFiltered;
            // Whitelist populated but no overlap with the global catalog
            // (e.g. orphan refs, or all entries null). Treat as "no opinion"
            // and fall through to the type-filter chain rather than locking
            // the musician out of all melodic instruments.
        }

        // ── Existing InstrumentType filter (unchanged behavior) ──
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
    /// Melody, Harmony). Uses the same filtering rules as GetPermittedMelodic
    /// (and therefore picks up the SO whitelist per role automatically).
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

    /// <summary>
    /// [MGP-REPRO-1 / D3=A] Drum kits this musician may use: the profile's
    /// <c>percussionInstruments</c> whitelist intersected with the catalog.
    /// Empty whitelist (or no overlap) = "no opinion" → every kit, same as
    /// the melodic whitelist rule.
    /// </summary>
    public static IReadOnlyList<MIDIPercussionInstrumentSO> GetPermittedPercussion(
        MusicianBase musician,
        IInstrumentRepository instrumentRepo)
        => GetPermittedPercussion(musician?.MusicianCharacterData, instrumentRepo);

    /// <summary>[MGP-BENCH-1] Data-asset overload (see GetPermittedMelodic).</summary>
    public static IReadOnlyList<MIDIPercussionInstrumentSO> GetPermittedPercussion(
        ALWTTT.Musicians.MusicianCharacterData data,
        IInstrumentRepository instrumentRepo)
    {
        var all = instrumentRepo?.GetPercussionInstruments()
                  ?? (IReadOnlyList<MIDIPercussionInstrumentSO>)Array.Empty<MIDIPercussionInstrumentSO>();

        var wl = data?.Profile?.percussionInstruments;
        if (wl != null && wl.Count > 0)
        {
            var filtered = all.Where(k => k != null && wl.Contains(k)).ToList();
            if (filtered.Count > 0)
                return filtered;
        }
        return all;
    }
}