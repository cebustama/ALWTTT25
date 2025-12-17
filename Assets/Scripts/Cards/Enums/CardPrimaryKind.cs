namespace ALWTTT.Cards
{
    /// <summary>
    /// High-level primary kind for a Composition card.
    /// Track: create/replace/update a musician's track in a part.
    /// Part: create/mark/structure a song part (intro/solo/outro/bridge/etc.).
    /// </summary>
    /// TODO: Better name
    public enum CardPrimaryKind
    {
        None = 0,
        Track = 1,
        Part = 2
    }
}
