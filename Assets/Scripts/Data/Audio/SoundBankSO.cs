using ALWTTT.Enums;
using ALWTTT.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// [S3-audio D-SA-7=B] Centralized audio inventory. Holds card-action profiles
    /// (existing SoundProfileData SOs, keyed by AudioActionType) and sensory-surface
    /// profiles (inline entries, keyed by SensorySfxType), plus a coverage audit so
    /// unassigned surfaces are visible without leaving the editor — operationalizing
    /// D1 ("no surface ships silent by accident"). AudioManager loads this asset
    /// instead of an inline list.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundBankSO", menuName = "ALWTTT/Containers/SoundBankSO")]
    public class SoundBankSO : ScriptableObject
    {
        [Header("Card-action audio — authored per card via CardDefinition.AudioType")]
        [Tooltip("Existing SoundProfileData assets; each carries its own AudioActionType key.")]
        [SerializeField] private List<SoundProfileData> cardProfiles = new();

        [Header("Sensory-surface audio — bus-driven (SensoryAudioAdapter)")]
        [Tooltip("Inline clip sets keyed by SensorySfxType. Sensory sounds live in " +
                 "the bank rather than as separate assets.")]
        [SerializeField] private List<SensorySoundEntry> sensoryProfiles = new();

        public IReadOnlyList<SoundProfileData> CardProfiles => cardProfiles;
        public IReadOnlyList<SensorySoundEntry> SensoryProfiles => sensoryProfiles;

        /// <summary>Inline sensory clip set keyed by SensorySfxType. Mirrors
        /// SoundProfileData's clip-list + random-pick shape.</summary>
        [Serializable]
        public class SensorySoundEntry
        {
            [SerializeField] private SensorySfxType type;
            [SerializeField] private List<AudioClip> randomClipList = new();

            public SensorySfxType Type => type;
            public bool HasClips => randomClipList != null && randomClipList.Count > 0;

            public AudioClip GetRandomClip() =>
                HasClips ? randomClipList.RandomItem() : null;
        }

        // ---- Coverage audit ------------------------------------------------

        /// <summary>
        /// Every required surface (all AudioActionType + all SensorySfxType) that has
        /// no profile/entry or an empty clip list. An empty result == full coverage.
        /// </summary>
        public List<string> GetMissing()
        {
            var missing = new List<string>();

            foreach (AudioActionType t in Enum.GetValues(typeof(AudioActionType)))
            {
                if (t == AudioActionType.None) continue;   // explicit silence, not a coverage gap

                var p = cardProfiles?.FirstOrDefault(x => x != null && x.AudioType == t);
                if (p == null)
                    missing.Add($"AudioActionType.{t} — no profile");
                else if ((p.RandomClipList?.Count ?? 0) == 0)
                    missing.Add($"AudioActionType.{t} — profile present, no clips");
            }

            foreach (SensorySfxType t in Enum.GetValues(typeof(SensorySfxType)))
            {
                var e = sensoryProfiles?.FirstOrDefault(x => x != null && x.Type == t);
                if (e == null)
                    missing.Add($"SensorySfxType.{t} — no entry");
                else if (!e.HasClips)
                    missing.Add($"SensorySfxType.{t} — entry present, no clips");
            }

            return missing;
        }

        [ContextMenu("Audit SFX Coverage")]
        private void LogCoverage()
        {
            var missing = GetMissing();
            if (missing.Count == 0)
            {
                Debug.Log($"[SoundBankSO:{name}] SFX coverage COMPLETE — every surface has clips.");
                return;
            }

            Debug.LogWarning(
                $"[SoundBankSO:{name}] {missing.Count} unassigned SFX surface(s):\n - " +
                string.Join("\n - ", missing));
        }
    }
}