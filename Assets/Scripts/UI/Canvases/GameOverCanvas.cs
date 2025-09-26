using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Utils;
using System;
using System.Text;
using TMPro;
using UnityEngine;

namespace ALWTTT.UI
{
    public class GameOverCanvas : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI gameplayInfoText;

        // Add new stats by adding more descriptors here.
        // Key = unique id for highscores; Label = how it prints; Getter = read from PersistentGameplayData.
        [Serializable]
        private class StatDescriptor
        {
            public string Key;
            public string Label;
            public Func<PersistentGameplayData, int> Getter;
        }

        private PersistentGameplayData PD => GameManager.Instance?.PersistentGameplayData;

        private void Start()
        {
            BuildAndShow();
        }

        private void BuildAndShow()
        {
            if (gameplayInfoText == null)
            {
                Debug.LogWarning("[GameOverCanvas] gameplayInfoText is not assigned.");
                return;
            }
            if (PD == null)
            {
                gameplayInfoText.text = "No run data available.";
                return;
            }

            // === Define the stats to show ===
            var stats = new StatDescriptor[]
            {
            new StatDescriptor
            {
                Key = "fans",
                Label = "Fans",
                Getter = pd => pd.Fans
            },
            new StatDescriptor 
            { 
                Key = "gigs_won", 
                Label = "Gigs Won", 
                Getter = pd => pd.GigsWon 
            },
                // Example (future):
                // new StatDescriptor { Key = "songs_composed", Label = "Songs Composed", Getter = pd => pd.CurrentSongList?.Count ?? 0 }
            };

            var sb = new StringBuilder();
            sb.AppendLine("RUN STATS");
            sb.AppendLine("---------");

            foreach (var s in stats)
            {
                int value = SafeGet(s.Getter, PD);
                bool isNewRecord = HighscoreStore.UpdateBest(s.Key, value);
                int best = HighscoreStore.GetBest(s.Key);

                // "Fans: 123 (Best: 456)" plus marker for new record
                sb.Append(s.Label).Append(": ").Append(value)
                  .Append(" (Best: ").Append(best).Append(')');
                if (isNewRecord) sb.Append("   NEW PERSONAL RECORD!");
                sb.AppendLine();
            }

            gameplayInfoText.text = sb.ToString();
        }

        private static int SafeGet(Func<PersistentGameplayData, int> getter, PersistentGameplayData pd)
        {
            try { return getter?.Invoke(pd) ?? 0; }
            catch (Exception e) { Debug.LogWarning($"[GameOverCanvas] Stat getter error: {e.Message}"); return 0; }
        }
    }
}