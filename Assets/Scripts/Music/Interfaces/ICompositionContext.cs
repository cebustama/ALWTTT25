using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Managers;
using ALWTTT.Music;
using ALWTTT.UI;
using MidiGenPlay;
using System;
using System.Collections.Generic;

namespace ALWTTT.Interfaces
{
    public interface ICompositionContext
    {
        SongCompositionUI CompositionUI { get; }
        LoopsTimerUI LoopsTimerUI { get; }
        DeckManager Deck { get; }
        MidiMusicManager Music { get; }
        IReadOnlyList<MusicianBase> Band { get; } // channel order

        void ShowCompositionUI(bool visible);
        void ShowHand(bool visible);

        MusicianBase ResolveMusicianByType(MusicianCharacterType type);
        MusicianBase ResolveMusicianById(string id);

        bool TryGetPartCache(int partIndex, out CompositionSession.PartCache cache);
        CompositionSession.PartCache GetOrCreatePartCache(int partIndex);

        void OnSessionStarted();
        void OnSessionEnded();

        void Log(string msg, bool highlight = false);
    }

    [Serializable]
    public class JamRules
    {
        public int loopsPerPart = 3;
        public int drawPerPart = 5;
        public int inspirationPerPart = 3;
    }
}