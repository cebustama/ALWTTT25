using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Backgrounds
{
    public class BackgroundRoot : MonoBehaviour
    {
        [SerializeField] private VenueType venueType;

        public VenueType VenueType => venueType;
    }
}

