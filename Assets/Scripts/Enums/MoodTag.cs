// Placement: Assets/Scripts/Enums/MoodTag.cs
//
// [HUD-COMP-1 / D3=C] Player-facing emotional label for the part's harmony.
//
// v1 is DERIVED, not authored: CompositionStripThemeSO maps the RENDERED
// Tonality to one of these. When (if) cards ever carry an authored mood, the
// authored value wins and this map becomes the fallback — the enum does not
// change, only its producer. That is the whole point of D3=C.
//
// Unknown is not a failure state: it renders a neutral chip whose hover shows
// the raw enum name, so a Tonality value added package-side never breaks the
// strip and is instantly diagnosable in-game.

namespace ALWTTT.Enums
{
    public enum MoodTag
    {
        Unknown = 0,
        Happy,
        Sad,
        Funky,
        Dreamy,
        Groovy,
        Dark,
        Tense
    }
}