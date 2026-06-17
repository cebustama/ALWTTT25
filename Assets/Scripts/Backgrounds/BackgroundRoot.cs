using ALWTTT.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Backgrounds
{
    public class BackgroundRoot : MonoBehaviour
    {
        [SerializeField] private VenueType venueType;
        [SerializeField] private Transform stageLightsRoot;
        [SerializeField] private ForegroundAnimator foregrounAnimation;
        [SerializeField] private List<StageLightAnimator> stageLights;

        [Header("SongHype stage VFX (S3) — assign per venue; null = stage stays lights-only")]
        [SerializeField] private Transform smokeRoot;
        [SerializeField] private Transform fireRoot;

        public VenueType VenueType => venueType;

        public void SetBPM(int bpm)
        {
            foregrounAnimation.SetBPM(bpm);

            foreach (var light in stageLights)
                if (light != null) light.SetBPM(bpm / 2);
        }

        public void SetLights(bool state)
        {
            stageLightsRoot.gameObject.SetActive(state);
        }

        // [S3 D-S3-6=A] Per-venue SongHype stage VFX. Null-guarded so venues
        // without smoke/fire roots stay lights-only without erroring.
        public void SetSmoke(bool state)
        {
            if (smokeRoot != null) smokeRoot.gameObject.SetActive(state);
            else if (state) Debug.Log($"[BackgroundRoot] {venueType} SetSmoke: no smokeRoot assigned.");
        }

        public void SetFire(bool state)
        {
            if (fireRoot != null) fireRoot.gameObject.SetActive(state);
            else if (state) Debug.Log($"[BackgroundRoot] {venueType} SetFire: no fireRoot assigned.");
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