using UnityEngine;

namespace ALWTTT.Utils
{
    public static class HighscoreStore
    {
        private static string Key(string statKey) => $"HS::{statKey}";

        /// Get the saved highscore for a stat (default 0).
        public static int GetBest(string statKey) => PlayerPrefs.GetInt(Key(statKey), 0);

        /// If value beats current best, update and return true.
        public static bool UpdateBest(string statKey, int value)
        {
            var key = Key(statKey);
            int best = PlayerPrefs.GetInt(key, 0);
            if (value > best)
            {
                PlayerPrefs.SetInt(key, value);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        /// Optional: clear one or all highscores
        public static void ResetBest(string statKey) { PlayerPrefs.DeleteKey(Key(statKey)); }
        public static void ResetAll() { PlayerPrefs.DeleteAll(); }
    }
}
