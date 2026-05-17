using ALWTTT.Characters.Band;
using ALWTTT.Encounters;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// [§5.3.5] Baked demo-launch configuration consumed by GigSetupController
    /// when <see cref="GigDevSettingsSO.AutoStartFromDefaults"/> is true. Lets
    /// the demo build skip the GigSetup picker UI entirely — app launches into
    /// Main Menu, Start button loads GigSetup, GigSetupController.Start()
    /// reads this SO and immediately routes to the gig with these baked values.
    ///
    /// Scope: only the values that genuinely vary per launch-config live here.
    /// Song-shape values (loopsPerPart, partsPerSong) are content-baked on the
    /// canonical <see cref="GigFlowSettingsSO"/> asset and the demo encounter,
    /// respectively — they propagate through the existing runtime path without
    /// requiring per-run overrides in code.
    ///
    /// Authority: §5.3.5 Demo cut prep. Decisions: DC-1=C (locality on
    /// GigDevSettingsSO via reference), DC-2=Custom (audience pool baked on
    /// encounter), DC-3=Custom (4 songs × 1 part × 4 loops/part), DC-4=B
    /// (initialGigInspiration=3, inspirationPerLoop=1).
    /// </summary>
    [CreateAssetMenu(
        fileName = "DemoLaunchConfig",
        menuName = "ALWTTT/Gig/DemoLaunchConfig",
        order = 14)]
    public sealed class DemoLaunchConfigSO : ScriptableObject
    {
        [Header("Band Roster")]
        [SerializeField, Tooltip("Musician prefabs that form the demo band. " +
            "Deck is auto-assembled from each musician's CardCatalog + the " +
            "GigSetupRoster.GenericStarterCatalog (useMusicianStarters=true " +
            "path on RunConfig). Bypasses the BandDeckData asset path.")]
        private List<MusicianBase> bandRoster = new List<MusicianBase>();

        [Header("Encounter")]
        [SerializeField, Tooltip("Demo encounter asset. Audience composition " +
            "(2× Kid + 1× Cool Dude per DC-2=Custom) is baked on this asset's " +
            "AudienceMemberList; no override is passed.")]
        private GigEncounterSO encounter;

        [Header("Run Tuning")]
        [SerializeField, Min(1), Tooltip("Songs the player must complete to " +
            "win the demo gig. DC-3=Custom: 4 songs.")]
        private int requiredSongCount = 4;

        [SerializeField, Min(0), Tooltip("Inspiration granted at the start of " +
            "the gig (before the first action window). DC-4=B: 3.")]
        private int initialGigInspiration = 3;

        [SerializeField, Min(0), Tooltip("Inspiration granted at each loop " +
            "boundary during composition. DC-4=B: 1.")]
        private int inspirationPerLoop = 1;

        // --- Public accessors ---

        public IReadOnlyList<MusicianBase> BandRoster => bandRoster;
        public GigEncounterSO Encounter => encounter;
        public int RequiredSongCount => Mathf.Max(1, requiredSongCount);
        public int InitialGigInspiration => Mathf.Max(0, initialGigInspiration);
        public int InspirationPerLoop => Mathf.Max(0, inspirationPerLoop);

        // --- Validation ---

        public bool IsValid(out string reason)
        {
            if (encounter == null)
            {
                reason = "Encounter is null.";
                return false;
            }
            if (bandRoster == null || bandRoster.Count == 0)
            {
                reason = "BandRoster is empty.";
                return false;
            }
            for (int i = 0; i < bandRoster.Count; i++)
            {
                if (bandRoster[i] == null)
                {
                    reason = $"BandRoster[{i}] is null.";
                    return false;
                }
            }
            reason = null;
            return true;
        }
    }
}