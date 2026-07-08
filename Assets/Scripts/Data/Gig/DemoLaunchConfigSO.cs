using ALWTTT.Characters.Band;
using ALWTTT.Encounters;
using ALWTTT.Managers;
using System.Collections.Generic;
using UnityEngine;
using static ALWTTT.Managers.GigRunContext;

namespace ALWTTT.Data
{
    /// <summary>
    /// [§5.3.5] Baked demo-launch configuration consumed by
    /// <c>MainMenuController</c> when <see cref="GigDevSettingsSO.AutoStartFromDefaults"/>
    /// is true. Lets the demo build skip the GigSetup picker UI entirely —
    /// app launches into Main Menu, Start button reads this SO and routes
    /// directly to the Gig scene via <see cref="GigLauncher.Launch"/>.
    /// Single fade cycle (Main Menu → Gig); no intermediate GigSetup
    /// waypoint, no flicker.
    ///
    /// Scope: only the values that genuinely vary per launch-config live here.
    /// Song-shape values (loopsPerPart, partsPerSong) are content-baked on the
    /// canonical <see cref="GigFlowSettingsSO"/> asset and the demo encounter,
    /// respectively — they propagate through the existing runtime path without
    /// requiring per-run overrides in code.
    ///
    /// Forward-design note for ladder mode (post-§5.3.5): this SO type is
    /// expected to evolve into / be aliased by an <c>EncounterLaunchConfigSO</c>
    /// family that a <c>LadderRunner</c> queues. Per-encounter configs would
    /// pass <c>bandRoster: null</c> to <see cref="GigLauncher.Launch"/> so the
    /// band carries over between encounters; only the encounter + tuning vary.
    ///
    /// Authority: §5.3.5 Demo cut prep. Decisions: DC-1=C (locality on
    /// GigDevSettingsSO via reference), DC-2=Custom (audience pool baked on
    /// encounter), DC-3=Custom (4 songs × 1 part × 4 loops/part), DC-4=B
    /// (initialGigInspiration=3, inspirationPerLoop=3), D-FAST-1=C [S5e/D2]
    /// (GigLauncher extraction; auto-launch from Main Menu bypasses GigSetup).
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
        private int initialGigInspiration = 1;

        [SerializeField, Min(0), Tooltip("Inspiration granted at each loop " +
            "boundary during composition. DC-4=B: 1.")]
        private int inspirationPerLoop = 1;   // [S5e / D2] was 1

        // --- Public accessors ---

        public IReadOnlyList<MusicianBase> BandRoster => bandRoster;
        public GigEncounterSO Encounter => encounter;
        public int RequiredSongCount => Mathf.Max(1, requiredSongCount);
        public int InitialGigInspiration => Mathf.Max(0, initialGigInspiration);
        public int InspirationPerLoop => Mathf.Max(0, inspirationPerLoop);

        // --- Conversions ---

        /// <summary>
        /// [§5.3.5] Filtered, null-stripped copy of <see cref="BandRoster"/>
        /// for passing directly into <see cref="GigLauncher.Launch"/>'s
        /// <c>bandRoster</c> parameter or <c>PersistentGameplayData.SetBandRoster</c>.
        /// Always returns a non-null List (may be empty if the SO is
        /// misconfigured — caller should still validate via <see cref="IsValid"/>).
        /// </summary>
        public List<MusicianBase> ResolvedBandRoster
        {
            get
            {
                var resolved = new List<MusicianBase>(
                    bandRoster != null ? bandRoster.Count : 0);
                if (bandRoster != null)
                {
                    for (int i = 0; i < bandRoster.Count; i++)
                    {
                        if (bandRoster[i] != null) resolved.Add(bandRoster[i]);
                    }
                }
                return resolved;
            }
        }

        /// <summary>
        /// [§5.3.5] Build the <see cref="GigRunContext.RunConfig"/> that
        /// <see cref="GigLauncher.Launch"/> consumes. Centralises the mapping
        /// from "baked SO values" to "RunConfig override-flag pairs" so
        /// MainMenuController + future LadderRunner don't have to duplicate it.
        ///
        /// Returns <c>null</c> if the encounter cannot be resolved to a
        /// runtime instance (logged). Caller should also validate via
        /// <see cref="IsValid"/> beforehand for early failure.
        /// </summary>
        public GigRunContext.RunConfig ToRunConfig(
            GigReturnDestination returnDestination = GigReturnDestination.GigSetup)
        {
            if (encounter == null)
            {
                Debug.LogError($"[DemoLaunchConfig:{name}] Encounter is null; " +
                    "cannot build RunConfig.");
                return null;
            }

            var runtime = encounter.BuildRuntime(audienceOverride: null);
            if (runtime == null)
            {
                Debug.LogError($"[DemoLaunchConfig:{name}] " +
                    $"Encounter '{encounter.name}' BuildRuntime returned null.");
                return null;
            }

            // Build deck label from resolved roster's CharacterIds.
            var resolvedRoster = ResolvedBandRoster;
            var idParts = new List<string>(resolvedRoster.Count);
            for (int i = 0; i < resolvedRoster.Count; i++)
            {
                var m = resolvedRoster[i];
                if (m == null || m.MusicianCharacterData == null) continue;
                idParts.Add(m.MusicianCharacterData.CharacterId);
            }
            string deckLabel = idParts.Count > 0
                ? "<auto:" + string.Join("+", idParts) + ">"
                : "<auto:<empty>>";

            return new GigRunContext.RunConfig
            {
                bandDeck = null,
                useMusicianStarters = true,
                deckLabel = deckLabel,
                encounter = runtime,
                audienceOverride = null,

                overrideRequiredSongCount = true,
                requiredSongCount = RequiredSongCount,

                overrideInitialGigInspiration = true,
                initialGigInspiration = InitialGigInspiration,

                overrideInspirationPerLoop = true,
                inspirationPerLoop = InspirationPerLoop,

                overrideDiscardHandBetweenTurns = false,
                discardHandBetweenTurns = false,

                overrideKeepInspirationBetweenTurns = false,
                keepInspirationBetweenTurns = false,

                returnDestination = returnDestination
            };
        }

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