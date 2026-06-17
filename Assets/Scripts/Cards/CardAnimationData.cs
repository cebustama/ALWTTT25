using System;
using UnityEngine;

namespace ALWTTT.Cards
{
    [Serializable]
    public enum CardAnimationKind
    {
        OneShot,        // fire once, wait for duration, then auto-return to normal
        TrackWide       // override the “playing” loop while active
    }

    [Serializable]
    public class CardAnimationData
    {
        [Header("Animator")]
        [SerializeField] private CardAnimationKind kind = CardAnimationKind.OneShot;
        [SerializeField] private string animatorTrigger;
        [Tooltip("If > 0, this overrides any default wait time for this card anim.")]
        [SerializeField] private float animationDuration = -1f;
        [Tooltip("Disable beat-based CharacterAnimator while the card animation plays.")]
        [SerializeField] private bool disableBeatAnimator = true;
        [Tooltip("For TrackWide: bool parameter that stays true while the override loop is active.")]
        [SerializeField] private string loopBoolParameter;

        public CardAnimationKind Kind => kind;
        public string AnimatorTrigger => animatorTrigger;
        public float AnimationDuration => animationDuration;
        public bool DisableBeatAnimator => disableBeatAnimator;
        public string LoopBoolParameter => loopBoolParameter;
    }
}