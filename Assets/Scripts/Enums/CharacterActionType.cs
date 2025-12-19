namespace ALWTTT.Enums
{
    // TODO: Rename to CharacterActionType
    public enum CharacterActionType
    {
        // Tagets Audience
        AddVibe = 0,
        BlockVibe = 1,
        RemoveVibe = 2,
        // Targets Band
        AddStress = 3,
        BlockStress = 4,
        HealStress = 5,
        // Targets song
        AddHooked = 6,
        AddHeckled = 7,
        AddStun = 8,
        AddDazzled = 9,
        // Other (movement, etc)
        MoveToFront = 10,

        GainInspiration = 100,
        LoseInspiration = 101
    }
}