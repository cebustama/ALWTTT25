using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Persisted audio-mix balance for ALWTTT (M-AUDIO-MIX, D-VOL=B / D-MIX-HOME=B).
    /// Single home for the three mix concepts the Dev "Audio Mix" tab tunes:
    ///   - global music level   (migrated from GameplayData.globalMusicVolume01)
    ///   - master SFX level     (applied to AudioManager.sfxSource)
    ///   - per-musician music balance defaults (musicianId -> volume01)
    ///
    /// Boundary (hard): this owns the ALWTTT per-MUSICIAN music axis only.
    /// MIDIInstrumentSO.volume01 is MidiGenPlay-side and is NOT wired here
    /// (double-attenuation risk; unknown whether the package applies it
    /// internally). See SSoT_Audio.md §"Audio boundary".
    ///
    /// Persistence model: this is a design-time SO asset. The Dev tab edits it
    /// live and, in the editor, marks it dirty + saves so the tuned balance is
    /// baked into the asset and ships as the default loaded at gig start. There
    /// is no runtime save system; in a player build the baked asset values are
    /// the shipped balance (Dev Mode itself is ALWTTT_DEV-gated).
    ///
    /// Authority: SSoT_Audio.md.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AudioMixSettings",
        menuName = "ALWTTT/Audio/AudioMixSettings",
        order = 20)]
    public sealed class AudioMixSettingsSO : ScriptableObject
    {
        [Serializable]
        public struct MusicianVolumeEntry
        {
            public string musicianId;
            [Range(0f, 1f)] public float volume01;
        }

        [Header("Global levels")]
        [SerializeField, Range(0f, 1f)] private float globalMusicVolume01 = 0.7f;
        [SerializeField, Range(0f, 1f)] private float masterSfxVolume01 = 1f;

        [Header("Per-musician music balance (defaults)")]
        [Tooltip("Default per-musician music volume. Musicians not listed default to 1.0.")]
        [SerializeField] private List<MusicianVolumeEntry> musicianVolumes = new();

        public float GlobalMusicVolume01
        {
            get => Mathf.Clamp01(globalMusicVolume01);
            set => globalMusicVolume01 = Mathf.Clamp01(value);
        }

        public float MasterSfxVolume01
        {
            get => Mathf.Clamp01(masterSfxVolume01);
            set => masterSfxVolume01 = Mathf.Clamp01(value);
        }

        public IReadOnlyList<MusicianVolumeEntry> MusicianVolumes => musicianVolumes;

        /// <summary>Per-musician default; 1.0 when the musician is not present.</summary>
        public float GetMusicianVolume01(string musicianId)
        {
            if (string.IsNullOrEmpty(musicianId) || musicianVolumes == null)
                return 1f;

            for (int i = 0; i < musicianVolumes.Count; i++)
                if (string.Equals(musicianVolumes[i].musicianId, musicianId,
                        StringComparison.Ordinal))
                    return Mathf.Clamp01(musicianVolumes[i].volume01);

            return 1f;
        }

        /// <summary>
        /// Upsert a per-musician default. In-memory only; the caller is
        /// responsible for persisting the asset in-editor (see
        /// GigManager.PersistAudioMixInEditor).
        /// </summary>
        public void SetMusicianVolume01(string musicianId, float volume01)
        {
            if (string.IsNullOrEmpty(musicianId)) return;

            volume01 = Mathf.Clamp01(volume01);
            musicianVolumes ??= new List<MusicianVolumeEntry>();

            for (int i = 0; i < musicianVolumes.Count; i++)
            {
                if (string.Equals(musicianVolumes[i].musicianId, musicianId,
                        StringComparison.Ordinal))
                {
                    var e = musicianVolumes[i];
                    e.volume01 = volume01;
                    musicianVolumes[i] = e;
                    return;
                }
            }

            musicianVolumes.Add(new MusicianVolumeEntry
            {
                musicianId = musicianId,
                volume01 = volume01
            });
        }
    }
}