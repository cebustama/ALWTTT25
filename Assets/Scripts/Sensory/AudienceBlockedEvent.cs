// Place at: Assets/Scripts/Sensory/AudienceBlockedEvent.cs
using ALWTTT.Characters.Audience;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [TUT-R2 / tut_status_blocked_front — CODE-TRUTH DEVIATION from TUT-R1 §4.1]
    /// The TUT-R1 spec assumed Blocked reaches the tutorial via StatusAppliedEvent.
    /// Code truth: since M1.2 (Decision E3) Blocked is a plain bool on
    /// <c>AudienceCharacterBase.IsBlocked</c> (sprite tint only) — no SO-container
    /// application exists, so no StatusAppliedEvent ever fires for it. This event
    /// is the minimal bus source: published from the IsBlocked setter on the
    /// false→true transition only.
    ///
    /// Semantic payload only. Tutorial gate — tut_status_blocked_front: first fire.
    /// The dialog copy talks about "ese icono" — TUT-R3 must reconcile copy vs the
    /// tint-only presentation (or a Blocked SO icon gets created; out of TUT-R2
    /// scope, flagged as an open in the closure notes).
    /// </summary>
    public readonly struct AudienceBlockedEvent : ISensoryEvent
    {
        public AudienceCharacterBase Audience { get; }

        public AudienceBlockedEvent(AudienceCharacterBase audience)
        {
            Audience = audience;
        }
    }
}