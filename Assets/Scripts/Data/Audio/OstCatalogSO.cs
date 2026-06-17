using System;
using System.Collections.Generic;
using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// [AUDIO-OST / D1=A] Inventory of OST (authored-clip) music tracks, keyed by OstTrackId.
    /// This SO answers "what OST tracks exist". MusicDirector decides WHEN to play them
    /// (scene→track map lives on the director), mirroring the SoundBankSO inventory-vs-caller
    /// split: the catalogue is content, the caller owns timing.
    ///
    /// Per-track fields:
    ///   - clip            : the AudioClip.
    ///   - loop            : loop while this is the active OST track (default: set true per entry).
    ///   - defaultLevel01  : per-track trim, multiplied by the app-wide Music level
    ///                       (AudioMixSettingsSO.GlobalMusicVolume01). 1.0 = full track level.
    ///
    /// There is NO AudioMixer in ALWTTT (volume is applied via AudioSource.volume directly), so
    /// the audible OST volume is: AudioSource.volume = musicLevel01 * defaultLevel01. The Music
    /// level scales gig music AND OST — one level, two consumers (see SSoT_Audio §4).
    ///
    /// Authority: SSoT_Audio.md §4 (OST bus).
    /// </summary>
    [CreateAssetMenu(fileName = "OstCatalog", menuName = "ALWTTT/Audio/OstCatalog", order = 21)]
    public sealed class OstCatalogSO : ScriptableObject
    {
        [Serializable]
        public struct OstEntry
        {
            public OstTrackId id;
            public AudioClip clip;

            [Tooltip("Loop the clip while it is the active OST track.")]
            public bool loop;

            [Range(0f, 1f), Tooltip("Per-track trim, multiplied by the Music level. 1.0 = full.")]
            public float defaultLevel01;
        }

        [Tooltip("One entry per OST track. Do not add a None entry (it is the stop sentinel).")]
        [SerializeField] private List<OstEntry> tracks = new();

        public IReadOnlyList<OstEntry> Tracks => tracks;

        /// <summary>
        /// Resolve a track by id. Returns false for None, for ids not present, and is the
        /// single lookup MusicDirector uses. A present-but-clipless entry still returns true
        /// (clip == null); the caller treats a null clip as a content gap (warn-once + no-op),
        /// consistent with the SFX "missing audio is a content gap, not a crash" invariant.
        /// </summary>
        public bool TryGet(OstTrackId id, out OstEntry entry)
        {
            if (id != OstTrackId.None && tracks != null)
            {
                for (int i = 0; i < tracks.Count; i++)
                {
                    if (tracks[i].id == id)
                    {
                        entry = tracks[i];
                        return true;
                    }
                }
            }

            entry = default;
            return false;
        }
    }
}