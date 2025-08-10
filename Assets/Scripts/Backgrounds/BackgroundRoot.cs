using ALWTTT.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Backgrounds
{
    public class BackgroundRoot : MonoBehaviour
    {
        [SerializeField] private VenueType venueType;
        [SerializeField] private ForegroundAnimator foregrounAnimation;
        [SerializeField] private List<StageLightAnimator> stageLights;

        public VenueType VenueType => venueType;

        public void SetBPM(int bpm)
        {
            foregrounAnimation.SetBPM(bpm);

            foreach (var light in stageLights)
                if (light != null) light.SetBPM(bpm / 2);
        }

#if UNITY_EDITOR
        [ContextMenu("Find Stage Lights")]
        private void FindStageLights()
        {
            stageLights = new List<StageLightAnimator>(
                GetComponentsInChildren<StageLightAnimator>(true));
        }
#endif
    }
}

