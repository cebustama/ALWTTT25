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
        // [S7 / ELENCO-1] First non-self audience member flagged isLeader that is NOT yet
        // convinced (D2=A): once the boss is won over, his minions have no one to cheer.
        // Null target list in that case (the action fizzles).
        AudienceLeader = 101,
    }
}