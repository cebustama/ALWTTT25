using ALWTTT.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Backgrounds
{
    public class BackgroundContainer : MonoBehaviour
    {
        [SerializeField] private List<BackgroundRoot> backgroundRootList;

        public List<BackgroundRoot> BackgroundRootList => backgroundRootList;

        private GigManager GigManager => GigManager.Instance;

        private BackgroundRoot CurrentBackground { get; set; }

        public void OpenSelectedBackground()
        {
            var encounter = GigManager.CurrentGigEncounter;
            if (encounter != null)
            {
                foreach (var backgroundRoot in BackgroundRootList)
                {
                    if (encounter.TargetVenueType == backgroundRoot.VenueType)
                    {
                        backgroundRoot.gameObject.SetActive(true);
                        CurrentBackground = backgroundRoot;
                    }
                }
            }
            else
            {
                Debug.LogError("[BackgroundContainer]" +
                    " No encounter found in GigManager.");
            }
        }

        public void SetBPM(int bpm)
        {
            foreach (var root in backgroundRootList)
            {
                root.SetBPM(bpm);
            }
        }

        /// <summary>
        /// [B2 / #6] Activate a venue SFX by tag.
        ///
        /// Tag convention (used by GigManager.FireSongHypeStage):
        ///   "lights" → stage 1 (1/3 SongHype): turn on stage lights.
        ///   "smoke"  → stage 2 (2/3 SongHype): smoke machines (per-venue extension).
        ///   "fire"   → stage 3 (3/3 SongHype): pyro (per-venue extension).
        ///
        /// Smoke/fire dispatch is venue-prefab-specific: BackgroundRoot may not
        /// yet expose SetSmoke/SetFire on all venues. Unknown tags fall through to
        /// SetLights(true) as a safe default and log so the gap is observable.
        /// Per-venue extension hooks live on BackgroundRoot itself.
        /// </summary>
        public void ActivateSFX(string sfxTag)
        {
            if (CurrentBackground == null)
            {
                Debug.LogWarning($"[BackgroundContainer] ActivateSFX('{sfxTag}'): " +
                    "no current background; ignoring.");
                return;
            }

            switch (sfxTag)
            {
                case "lights":
                case "hype_1":
                    CurrentBackground.SetLights(true);
                    break;

                // Hooks for "smoke" / "fire" land per-venue on BackgroundRoot.
                // Until those land, fall through to SetLights so stage 2/3 are
                // still observable in playtest (just less differentiated).
                case "smoke":
                case "hype_2":
                case "fire":
                case "hype_3":
                    CurrentBackground.SetLights(true);
                    Debug.Log($"[BackgroundContainer] ActivateSFX('{sfxTag}'): " +
                        "per-venue hook not wired; defaulted to SetLights(true). " +
                        "Extend BackgroundRoot with SetSmoke/SetFire to differentiate.");
                    break;

                default:
                    Debug.LogWarning($"[BackgroundContainer] ActivateSFX('{sfxTag}'): " +
                        "unrecognized tag; ignoring.");
                    break;
            }
        }

        /// <summary>
        /// [B2.5 / #2] Clear all venue SFX on a song boundary. Called from
        /// <see cref="GigManager.ResetSongHype"/> so the next song starts visually fresh.
        ///
        /// Currently only stage lights are wired on every venue. Smoke/fire hooks
        /// will land on <see cref="BackgroundRoot"/> per-venue; once <c>SetSmoke</c>
        /// and <c>SetFire</c> exist there, mirror the SetLights(false) line here so
        /// that all stage-N effects are zeroed together. No-op if no background is
        /// currently open (e.g. between gigs).
        /// </summary>
        public void DeactivateAllSFX()
        {
            if (CurrentBackground == null) return;

            CurrentBackground.SetLights(false);

            // Per-venue extension hooks:
            // CurrentBackground.SetSmoke(false);   // when SetSmoke lands on BackgroundRoot
            // CurrentBackground.SetFire(false);    // when SetFire lands on BackgroundRoot
        }
    }
}