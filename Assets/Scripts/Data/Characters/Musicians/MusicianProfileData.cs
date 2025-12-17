using ALWTTT.Characters.Band;
using MidiGenPlay;
using MidiGenPlay.Composition;
using MidiGenPlay.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALWTTT.Musicians
{
    [Serializable]
    public class MusicianProfileData
    {
        [Header("Instrument Types")]
        public List<InstrumentType> backingInstruments;
        public List<InstrumentType> leadInstruments;

        [Header("Melodic Instruments Whitelist")]
        public List<MIDIInstrumentSO> backingMelodicInstruments;
        public List<MIDIInstrumentSO> leadMelodicInstruments;

        [Header("Percussion Instruments Whitelist")]
        public List<MIDIPercussionInstrumentSO> percussionInstruments;

        [Header("Composition")]
        public MelodicLeadingConfig defaultMelodicLeading;
        public HarmonicLeadingConfig defaultHarmonicLeading;

        /// <summary>
        /// Returns the union of all explicitly whitelisted melodic instruments
        /// (backing + lead), with nulls removed and duplicates stripped.
        /// </summary>
        public IReadOnlyList<MIDIInstrumentSO> GetAllWhitelistedMelodicInstrumentSOs()
        {
            var result = new List<MIDIInstrumentSO>();

            if (backingMelodicInstruments != null)
                result.AddRange(backingMelodicInstruments);

            if (leadMelodicInstruments != null)
                result.AddRange(leadMelodicInstruments);

            return result
                .Where(i => i != null)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// True if this profile has any explicit melodic whitelist entries.
        /// </summary>
        public bool HasMelodicWhitelist()
        {
            bool hasBacking =
                backingMelodicInstruments != null &&
                backingMelodicInstruments.Count > 0 &&
                backingMelodicInstruments.Any(i => i != null);

            bool hasLead =
                leadMelodicInstruments != null &&
                leadMelodicInstruments.Count > 0 &&
                leadMelodicInstruments.Any(i => i != null);

            return hasBacking || hasLead;
        }

        /// <summary>
        /// Returns a combined list of melodic instrument SOs for debug:
        /// - If there is an explicit whitelist → use that (union of backing+lead).
        /// - Otherwise → derive from InstrumentType + repository.
        /// </summary>
        public IReadOnlyList<MIDIInstrumentSO> GetDebugMelodicInstrumentOptions(
            MusicianBase musician,
            InstrumentRepositoryResources instrumentRepo)
        {
            var result = new List<MIDIInstrumentSO>();

            // 1) If there is a whitelist, just reuse the existing helper
            //    (so debug picker shows exactly what the rules see).
            if (HasMelodicWhitelist())
            {
                result.AddRange(GetAllWhitelistedMelodicInstrumentSOs());
            }
            // 2) No explicit whitelist → derive from InstrumentType + repo.
            else if (instrumentRepo != null && musician != null)
            {
                result.AddRange(
                    InstrumentRules.GetPermittedMelodicAllRoles(musician, instrumentRepo));
            }

            return result
                .Where(i => i != null)
                .Distinct()
                .ToList();
        }

        public bool HasPercussionWhitelist()
        {
            return percussionInstruments != null &&
                   percussionInstruments.Any(i => i != null);
        }

        /// <summary>
        /// Very cheap heuristic: if they have any percussion whitelisted,
        /// treat this musician as a percussionist for debug purposes.
        /// (You can extend this later if you add a more explicit flag.)
        /// </summary>
        public bool IsPercussionist()
        {
            return HasPercussionWhitelist();
        }

        /// <summary>
        /// Returns a combined list of percussion instruments for debug:
        /// - If there is an explicit whitelist → use it.
        /// - Otherwise → list every percussion instrument in the repo.
        /// </summary>
        public IReadOnlyList<MIDIPercussionInstrumentSO>
            GetDebugPercussionInstrumentOptions(
            InstrumentRepositoryResources instrumentRepo)
        {
            var result = new List<MIDIPercussionInstrumentSO>();

            if (HasPercussionWhitelist())
            {
                result.AddRange(percussionInstruments.Where(i => i != null));
            }
            else if (instrumentRepo != null)
            {
                // Adjust this method name to whatever your repo actually exposes.
                var allPerc = instrumentRepo.GetPercussionInstruments();
                if (allPerc != null)
                    result.AddRange(allPerc.Where(i => i != null));
            }

            return result
                .Distinct()
                .ToList();
        }
    }
}