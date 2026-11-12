using ALWTTT.Characters.Band;
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

        [Header("Placement for this venue (S9) - empty = the gig scene's")]
        [SerializeField, Tooltip("Band slots for this venue (a BandStageLayout inside this prefab). " +
            "Empty = GigManager's Band Stage Layout. Slot scale = musician scale.")]
        private BandStageLayout bandStageLayout;

        [SerializeField, Tooltip("Audience slots for this venue, in encounter order. " +
            "Empty = GigManager's Audience Member Positions. Slot scale = member scale.")]
        private List<Transform> audiencePositions = new List<Transform>();

        public VenueType VenueType => venueType;
        public BandStageLayout BandStageLayout => bandStageLayout;
        public List<Transform> AudiencePositions => audiencePositions;
        public bool HasAudiencePositions
        {
            get
            {
                if (audiencePositions == null) return false;
                foreach (var t in audiencePositions) if (t != null) return true;
                return false;
            }
        }

        // [S9] Null-guarded: the Colmena (Venue2) has no lights or foreground animator.
        public void SetBPM(int bpm)
        {
            if (foregrounAnimation != null) foregrounAnimation.SetBPM(bpm);

            if (stageLights != null)
                foreach (var light in stageLights)
                    if (light != null) light.SetBPM(bpm / 2);
        }

        public void SetLights(bool state)
        {
            if (stageLightsRoot != null) stageLightsRoot.gameObject.SetActive(state);
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