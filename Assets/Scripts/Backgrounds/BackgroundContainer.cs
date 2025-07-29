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

        public void OpenSelectedBackground()
        {
            var encounter = GigManager.CurrentGigEncounter;
            if (encounter != null)
            {
                foreach (var backgroundRoot in BackgroundRootList)
                    backgroundRoot.gameObject.SetActive(
                        encounter.TargetVenueType == backgroundRoot.VenueType);
            }
            else
            {
                Debug.LogError("[BackgroundContainer]" +
                    " No encounter found in GigManager.");
            }
        }
    }
}