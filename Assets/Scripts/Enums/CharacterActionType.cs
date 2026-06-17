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

        // [B3-content-audience pass2] SO-based status application from audience abilities.
        // Audience-side counterpart to the card pipeline's ApplyStatusEffectSpec.
        // Reads StatusEffectSO from CharacterActionData.StatusEffect; uses ActionValue as stacksDelta.
        ApplyStatusEffect = 11,

        GainInspiration = 100,
        LoseInspiration = 101
    }
}