using System.Collections.Generic;
using MidiGenPlay;
using MidiGenPlay.Composition; // MusicianTrackKey
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// [BAL-1] Bytes-plane ensemble mix gains (Boundary §8.3, D-BAL-1=C).
    ///
    /// Content SO, keyed (musicianId, TrackRole), implicit default 1.0: a track
    /// WITHOUT an entry gets no CC7 emitted at all (byte-identity guarantee).
    /// An explicit entry — including an explicit 1.0 — emits a CC7 on that
    /// track (1.0 ⇒ CC7=100, audibly identical to no entry, byte-different).
    ///
    /// Rules:
    ///  - D-BAL-2=A: hand-authored ensemble intent ONLY. Never calibrate these
    ///    to offset per-patch loudness (anti-compensation rule; that is
    ///    package-side volume01 / D-MIX-6's job).
    ///  - D-BAL-5=A: no Rhythm entries. Channel 9 is shared; the package
    ///    warn+ignores them, and BuildGainMap drops them with a warning.
    ///  - D-BAL-3=A: resolved once at gig start (GigManager), fixed per gig.
    ///  - D-BAL-4=A: content data, versioned with the game; not player save.
    ///
    /// Range: [0 .. 1.27]. The package law clamp(round(volume01 × gain × 100),
    /// 0, 127) saturates at ~1.27 while volume01 is unauthored (1.0).
    /// gain = 0 mutes at playback but keeps note events (stems/hashes intact).
    /// </summary>
    [CreateAssetMenu(
        fileName = "MixGainProfile",
        menuName = "ALWTTT/Audio/Mix Gain Profile (bytes plane)")]
    public sealed class MixGainProfileSO : ScriptableObject
    {
        public const float MaxGain = 1.27f;

        [System.Serializable]
        public sealed class Entry
        {
            [Tooltip("MusicianCharacterData.CharacterId of the gained musician.")]
            public string musicianId;

            [Tooltip("Track role for this gain. Rhythm entries are invalid " +
                     "(channel 9 is shared) and are dropped with a warning.")]
            public TrackRole role = TrackRole.Melody;

            [Tooltip("Ensemble gain. 1.0 = identity (CC7=100). 0 = mute. " +
                     "Saturates at 1.27 (CC7=127).")]
            [Range(0f, MaxGain)]
            public float gain = 1f;
        }

        [SerializeField]
        private List<Entry> entries = new();

        /// <summary>
        /// Builds the (musicianId, TrackRole) → gain map consumed by
        /// MidiMusicManager.SetGigMixGains. Drops Rhythm and empty-id entries
        /// with a warning; clamps to [0, MaxGain]; last duplicate wins (warned).
        /// Returns null when nothing valid is authored (⇒ no CC7 anywhere,
        /// byte-identical output).
        /// </summary>
        public Dictionary<MusicianTrackKey, float> BuildGainMap()
        {
            if (entries == null || entries.Count == 0) return null;

            Dictionary<MusicianTrackKey, float> map = null;
            foreach (var e in entries)
            {
                if (e == null) continue;
                if (string.IsNullOrEmpty(e.musicianId))
                {
                    Debug.LogWarning($"[MixGainProfile:{name}] entry with empty musicianId skipped.");
                    continue;
                }
                if (e.role == TrackRole.Rhythm)
                {
                    Debug.LogWarning(
                        $"[MixGainProfile:{name}] Rhythm entry for '{e.musicianId}' dropped " +
                        "(channel 9 shared; D-BAL-5=A / package D-MIX-4=A).");
                    continue;
                }

                var key = new MusicianTrackKey(e.musicianId, e.role);
                map ??= new Dictionary<MusicianTrackKey, float>();
                if (map.ContainsKey(key))
                    Debug.LogWarning(
                        $"[MixGainProfile:{name}] duplicate entry ({e.musicianId},{e.role}) — last wins.");
                map[key] = Mathf.Clamp(e.gain, 0f, MaxGain);
            }
            return map != null && map.Count > 0 ? map : null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (entries == null) return;
            foreach (var e in entries)
                if (e != null) e.gain = Mathf.Clamp(e.gain, 0f, MaxGain);
        }
#endif
    }
}