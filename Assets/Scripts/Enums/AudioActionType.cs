namespace ALWTTT.Enums
{
    public enum AudioActionType
    {
        Button,      // 0 — UI click type (ButtonSoundPlayer); also the default for unset card fields
        AddStress,   // 1
        AddVibe,     // 2
        HealStress,  // 3
        HealVibe,    // 4
        None         // 5 — explicit "no card SFX" (appended last; existing ints unchanged)
    }
}