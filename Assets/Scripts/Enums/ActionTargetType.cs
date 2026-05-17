namespace ALWTTT.Enums
{
    public enum ActionTargetType
    {
        AudienceCharacter,
        Musician,
        AllAudienceCharacters,
        AllMusicians,
        RandomAudienceCharacter,
        RandomMusician,
        Self,
        // [B3-content-audience pass2 / D14=B] First non-self audience member with IsTall == true.
        // Used by audience-side buff abilities targeting "the tall guy" (Cool Dude). Falls back
        // to null target list when no tall non-self ally exists.
        AudienceTall = 100,  // pin a high value to avoid collision with any future contiguous appends
    }
}