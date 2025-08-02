using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Backgrounds
{
    public class BackgroundRoot : MonoBehaviour
    {
        [SerializeField] private VenueType venueType;
        [SerializeField] private ForegroundAnimator foregrounAnimation;

        public VenueType VenueType => venueType;

        public void SetBPM(int bpm)
        {
            foregrounAnimation.SetBPM(bpm);
        }
    }
}

