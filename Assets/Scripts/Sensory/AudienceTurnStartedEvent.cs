namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S4 D-S4-SRC=A] Bus bridge of the existing C# <c>GigManager.OnEnemyTurnStarted</c>.
    /// Published in the phase transition (case GigPhase.AudienceTurn) right after
    /// OnEnemyTurnStarted?.Invoke(), before AudienceTurnRoutine starts.
    ///
    /// Tutorial gate — tut_first_audience_action: first AudienceTurnStartedEvent
    /// AFTER a SfxStageCrossedEvent has been seen (the design's "first audience turn
    /// after first SongHype stage crossing"). The single-modal queue + the
    /// AudienceTurnRoutine gate-wait keep this ordered relative to stage escalation;
    /// the two are distinct (stage SFX ≠ audience pressure) and are not merged.
    /// </summary>
    public readonly struct AudienceTurnStartedEvent : ISensoryEvent
    {
    }
}
