using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "New Song", menuName = "ALWTTT/Songs/SongData")]
    public class SongData : ScriptableObject
    {
        [Header("Song Profile")]
        [SerializeField] private string id;
        [SerializeField] private string songTitle;
        [SerializeField] private SongGenre genre;
        [SerializeField] private LyricsTheme theme;
        [SerializeField] private ComplexityLevel complexity;
        [SerializeField] private SongPopularity popularity;
        [SerializeField] private SongStatus status;

        [Header("Audio")]
        [SerializeField] private float duration = 5f;
        [SerializeField] private int bpm = 80;

        #region Encapsulation
        public string SongTitle => songTitle;
        public float Duration => duration;
        public int BPM => bpm;
        #endregion

        public string GetDropdownText()
        {
            return $"{songTitle} ({GetThemeColorText()})";
        }

        private string GetThemeColorText()
        {
            switch (theme)
            {
                case LyricsTheme.Love:
                    return "<color=red>Love</color>";
                case LyricsTheme.Injustice:
                    return "<color=purple>Injustice</color>";
                case LyricsTheme.Partying:
                    return "<color=blue>Partying</color>";
            }

            return "Theme Color Not Found.";
        }
    }

    public enum SongGenre
    {
        Rock,
        Pop,
        Jazz
    }

    public enum LyricsTheme
    {
        Love,
        Injustice,
        Partying
    }

    public enum ComplexityLevel
    {
        NotComplex,
        Complex,
        VeryComplex
    }

    public enum SongStatus
    {
        New,
        Rehearsed,
        NotRehearsed,
        Forgotten
    }

    public enum SongPopularity
    {
        Unknown,
        Familiar,
        Famous
    }
}