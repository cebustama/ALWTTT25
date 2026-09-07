// Place at: Assets/Scripts/Sensory/TrackReplacedEvent.cs
using ALWTTT.Cards;
using MidiGenPlay;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [TUT-REDESIGN-B] A composition card replaced the SAME musician's track
    /// of the SAME role (BASS-1 keying by (musicianId, role)). Not published
    /// for bundle-less PartEffect carriers (they augment, they don't replace)
    /// nor for re-playing the identical card.
    /// Tutorial gate — tut_track_replaced: first fire.
    /// </summary>
    public readonly struct TrackReplacedEvent : ISensoryEvent
    {
        public string MusicianId { get; }
        public TrackRole Role { get; }
        /// <summary>May be null if the previous track had no source card recorded.</summary>
        public CardDefinition Previous { get; }
        public CardDefinition Replacement { get; }

        public TrackReplacedEvent(string musicianId, TrackRole role,
            CardDefinition previous, CardDefinition replacement)
        {
            MusicianId = musicianId;
            Role = role;
            Previous = previous;
            Replacement = replacement;
        }
    }
}