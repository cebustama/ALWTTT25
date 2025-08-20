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

        public void ActivateSFX(string sfxTag)
        {
            // TODO get each SFX according to tag
            CurrentBackground.SetLights(true);
        }
    }
}