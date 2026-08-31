using ALWTTT.Enums;
using ALWTTT.Extentions;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// [AUDIO-CHAR-PROFILES phase 1 / D-CHAR-SFX=C] Per-character SFX clip source.
    /// Assigned on a character's data asset (AudienceCharacterData.sfxProfile) and
    /// consulted by SensoryAudioAdapter BEFORE the global SoundBankSO when a reaction
    /// fires. This is a clip SOURCE for the existing SensorySfxType reaction keys
    /// (ReactionPositive / ReactionNegative) � it introduces NO new key, so the
    /// "two SFX keys, one authority each" invariant is untouched (SSoT_Audio �7 inv.2).
    ///
    /// Resolution + fallback (D-CHAR-SFX-FALLBACK = per-polarity):
    ///   reaction polarity -> GetClipFor(polarity)
    ///     non-null -> adapter plays it (jitter preserved, inv.10)
    ///     null     -> adapter falls back to SoundBankSO for THAT polarity
    ///                 (a positive-only profile still gets the bank's negative sting;
    ///                  a truly-missing surface warns-once + no-ops at the bank, inv.3).
    ///
    /// Neutral reactions stay FT-only (SensorySfxPresentation.ForReaction returns null
    /// for impression 0) � there is intentionally NO neutral slot here, to avoid a
    /// per-member neutral noise floor. Mirrors SoundBankSO.SensorySoundEntry's
    /// clip-list + random-pick shape.
    ///
    /// Forward (AUDIO-CHAR-PROFILES-2): ability SFX is NOT hosted here. Per
    /// D-ABILITY-SFX-HOME=(i), the per-ability one-shot lives inline on
    /// AudienceAbilityData (one ability = one authoring spot, no string key); this SO
    /// stays reaction-only. A status-apply clip (D-CHAR-SFX-2 option B) is deferred and,
    /// if built, would hook the status-apply site, not this asset.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterSfxProfile",
        menuName = "ALWTTT/Containers/CharacterSfxProfileSO")]
    public class CharacterSfxProfileSO : ScriptableObject
    {
        [Header("Reaction clips (per-character source for ReactionPositive/Negative)")]
        [Tooltip("Played when this character reacts positively (impression >= 1). " +
                 "Empty -> adapter falls back to SoundBankSO's ReactionPositive entry.")]
        [SerializeField] private List<AudioClip> positiveClips = new();

        [Tooltip("Played when this character reacts negatively (impression <= -1). " +
                 "Empty -> adapter falls back to SoundBankSO's ReactionNegative entry.")]
        [SerializeField] private List<AudioClip> negativeClips = new();


        [Header("Status-apply overrides (WINK-1 D2=C top layer, keyed by StatusKey)")]
        [Tooltip("Per-character clip override for a status APPLICATION. Key must match " +
                 "StatusEffectSO.StatusKey exactly. No entry / empty clips -> caller falls " +
                 "back to StatusEffectSO.applySfx, then deliberate silence (never a warn).")]
        [SerializeField] private List<StatusSfxOverride> statusOverrides = new();

        [System.Serializable]
        public class StatusSfxOverride
        {
            [Tooltip("StatusEffectSO.StatusKey of the variant this override targets.")]
            [SerializeField] private string statusKey;
            [SerializeField] private List<AudioClip> clips = new();

            public string StatusKey => statusKey;
            public bool HasClips => clips != null && clips.Count > 0;
            public AudioClip GetRandomClip() => HasClips ? clips.RandomItem() : null;
        }

        public bool HasPositiveClips => positiveClips != null && positiveClips.Count > 0;
        public bool HasNegativeClips => negativeClips != null && negativeClips.Count > 0;

        /// <summary>
        /// [WINK-1 D2=C top layer] Random clip for a status APPLICATION, or null
        /// when this profile has no override for the key — the caller
        /// (SensoryAudioAdapter) then falls back to StatusEffectSO.applySfx and
        /// finally to deliberate silence. Key comparison is exact (StatusKey is
        /// normalized at authoring time by StatusEffectSO.OnValidate).
        /// </summary>
        public AudioClip GetClipForStatus(string statusKey)
        {
            if (string.IsNullOrEmpty(statusKey) || statusOverrides == null) return null;
            for (int i = 0; i < statusOverrides.Count; i++)
            {
                var o = statusOverrides[i];
                if (o != null && o.StatusKey == statusKey) return o.GetRandomClip();
            }
            return null;
        }

        /// <summary>
        /// Random clip for a reaction polarity, or null if this profile has nothing for
        /// it (the caller then falls back to the global bank � D-CHAR-SFX-FALLBACK
        /// per-polarity). Only the two reaction keys are handled; any other
        /// SensorySfxType returns null (this SO is reaction-only in phase 1).
        /// </summary>
        public AudioClip GetClipFor(SensorySfxType polarity)
        {
            switch (polarity)
            {
                case SensorySfxType.ReactionPositive:
                    return HasPositiveClips ? positiveClips.RandomItem() : null;
                case SensorySfxType.ReactionNegative:
                    return HasNegativeClips ? negativeClips.RandomItem() : null;
                default:
                    return null;
            }
        }
    }
}