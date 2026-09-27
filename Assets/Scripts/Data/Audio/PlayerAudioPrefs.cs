using System;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// [PAUSE-1 / D10=B] Player-facing volume levels (Options v1), persisted in PlayerPrefs.
    ///
    /// Model: a PLAYER plane that multiplies on top of the DESIGNED mix, never replacing it.
    ///   effective music = AudioMixSettingsSO.GlobalMusicVolume01 (x per-musician, gains, duck)
    ///                     x PlayerAudioPrefs.Music01
    ///   effective SFX   = AudioMixSettingsSO.MasterSfxVolume01 x PlayerAudioPrefs.Sfx01
    /// Defaults are 1.0, so a player who never opens Options hears exactly the baked mix.
    ///
    /// Why multiplicative and not "PlayerPrefs replaces the SO value": the Dev Audio Mix
    /// tab keeps tuning the designed balance in the SO; a replacing override would mask
    /// every dev edit on any machine that ever touched Options. Two planes, two owners.
    ///
    /// Hard rule: the player path NEVER writes AudioMixSettingsSO. In the editor, writing a
    /// ScriptableObject during Play mode mutates the asset and would leak a player choice
    /// into the shipped default (SSoT_Audio section 5).
    ///
    /// Write boundaries that apply the player plane (one per bus):
    ///   gig music -> MidiMusicManager.WriteChannelVolume01 (all channels, re-sent on change)
    ///   OST       -> MusicDirector.MusicLevel01
    ///   SFX group -> AudioManager.ApplySfxLevels (sfx + button + ambience)
    /// Consumers subscribe to Changed. Save() flushes to disk (called on Options close).
    /// </summary>
    public static class PlayerAudioPrefs
    {
        public const string MusicKey = "alwttt.audio.player_music01";
        public const string SfxKey = "alwttt.audio.player_sfx01";

        private static bool s_loaded;
        private static float s_music01 = 1f;
        private static float s_sfx01 = 1f;

        /// <summary>Raised after either level changes (not on load).</summary>
        public static event Action Changed;

        public static float Music01 { get { EnsureLoaded(); return s_music01; } }
        public static float Sfx01 { get { EnsureLoaded(); return s_sfx01; } }

        public static void SetMusic01(float value01)
        {
            EnsureLoaded();
            value01 = Mathf.Clamp01(value01);
            if (Mathf.Approximately(value01, s_music01)) return;
            s_music01 = value01;
            PlayerPrefs.SetFloat(MusicKey, value01);
            Changed?.Invoke();
        }

        public static void SetSfx01(float value01)
        {
            EnsureLoaded();
            value01 = Mathf.Clamp01(value01);
            if (Mathf.Approximately(value01, s_sfx01)) return;
            s_sfx01 = value01;
            PlayerPrefs.SetFloat(SfxKey, value01);
            Changed?.Invoke();
        }

        /// <summary>Flush to disk. Cheap enough per panel close; not per slider frame.</summary>
        public static void Save() => PlayerPrefs.Save();

        private static void EnsureLoaded()
        {
            if (s_loaded) return;
            s_loaded = true;
            s_music01 = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, 1f));
            s_sfx01 = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, 1f));
        }

        // Enter-Play-Mode without domain reload keeps statics alive; start clean each run.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_loaded = false;
            s_music01 = 1f;
            s_sfx01 = 1f;
            Changed = null;
        }
    }
}