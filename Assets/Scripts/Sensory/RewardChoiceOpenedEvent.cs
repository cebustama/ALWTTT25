namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S5h] Published by GigManager when the end-of-gig reward screen opens
    /// with at least one reward box. Consumers: SensoryAudioAdapter
    /// (SensorySfxType.RewardOpened sting) and TutorialController
    /// (tut_first_reward_choice). NOT published when the reward step is
    /// skipped (all pools empty/exhausted) — no tutorial for an unseen screen.
    /// </summary>
    public readonly struct RewardChoiceOpenedEvent : ISensoryEvent { }
}