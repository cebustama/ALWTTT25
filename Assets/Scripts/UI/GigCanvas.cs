using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Managers;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class GigCanvas : CanvasBase
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI drawPileTextField;
        [SerializeField] private TextMeshProUGUI discardPileTextField;
        [SerializeField] private TextMeshProUGUI exhaustPileTextField;
        [SerializeField] private TextMeshProUGUI grooveTextField;
        [SerializeField] private TextMeshProUGUI nowPlayingSongTextField;

        [Header("Dropdowns")]
        [SerializeField] private TMP_Dropdown songDropdown;

        [Header("Buttons")]
        [SerializeField] private Button endTurnButton;

        [Header("UI Sections")]
        [SerializeField] private GameObject bandTurnUI;
        [SerializeField] private GameObject songPerfomanceUI;

        private readonly List<SongData> filteredSongs = new List<SongData>();

        private void OnEnable()
        {
            endTurnButton.onClick.AddListener(EndTurn);
            songDropdown.onValueChanged.AddListener(OnSongSelected);

            GigManager.OnPlayerTurnStarted += ShowBandTurnUI;
            GigManager.OnSongPerformanceStarted += ShowSongPerformanceUI;
            GigManager.OnEnemyTurnStarted += ShowAudienceTurnUI;
        }

        private void OnDisable()
        {
            endTurnButton.onClick.RemoveListener(EndTurn);
            songDropdown.onValueChanged.RemoveListener(OnSongSelected);

            GigManager.OnPlayerTurnStarted -= ShowBandTurnUI;
            GigManager.OnSongPerformanceStarted -= ShowSongPerformanceUI;
            GigManager.OnEnemyTurnStarted -= ShowAudienceTurnUI;
        }

        public void FillSongDropdown(List<SongData> playedSongs = null)
        {
            songDropdown.onValueChanged.RemoveListener(OnSongSelected);

            songDropdown.ClearOptions();
            filteredSongs.Clear();

            var allSongs = GameManager.PersistentGameplayData.CurrentSongList;
            var exclude = playedSongs != null ? new HashSet<SongData>(playedSongs) : null;

            var options = new List<string>();

            options.Add("- Select Song -");
            filteredSongs.Add(null); // keep indices aligned with dropdown

            foreach (var song in allSongs)
            {
                if (song == null) continue;
                if (exclude != null && exclude.Contains(song)) continue;

                filteredSongs.Add(song);
                options.Add(song.GetDropdownText());
            }

            // TODO: What should happen in this case?
            if (options.Count == 1)
            {
                options.Add("— No songs available —");
                songDropdown.AddOptions(options);
                songDropdown.value = 0;
                songDropdown.interactable = false;
                endTurnButton.interactable = false; // nothing to play
            }
            // TODO: Add "- Select Song -" element
            else
            {
                songDropdown.AddOptions(options);
                songDropdown.value = 0;
                songDropdown.interactable = true;
                endTurnButton.interactable = false; // force a real selection
            }

            songDropdown.RefreshShownValue();
            songDropdown.onValueChanged.AddListener(OnSongSelected);
        }

        public void SetPileTexts()
        {
            drawPileTextField.text = $"{DeckManager.DrawPile.Count.ToString()}";
            discardPileTextField.text = $"{DeckManager.DiscardPile.Count.ToString()}";
            exhaustPileTextField.text = $"{DeckManager.ExhaustPile.Count.ToString()}";
            grooveTextField.text = $"{GameManager.PersistentGameplayData.CurrentGroove.ToString()}" +
                $"/{GameManager.PersistentGameplayData.MaxGroove.ToString()}";
        }

        private void OnSongSelected(int index)
        {
            if (index > 0 && index < filteredSongs.Count && filteredSongs[index] != null)
            {
                var song = filteredSongs[index];
                GameManager.PersistentGameplayData.CurrentSong = song;
                nowPlayingSongTextField.text = $"NOW PLAYING: {song.SongTitle}";
                endTurnButton.interactable = true;
                // TODO: Update all other UI elements according to selected song
            }
            else
            {
                GameManager.PersistentGameplayData.CurrentSong = null;
                endTurnButton.interactable = false; // can’t proceed without a real song
            }
        }

        public void ShowBandTurnUI()
        {
            bandTurnUI.SetActive(true);
            songPerfomanceUI.SetActive(false);
        }

        public void ShowSongPerformanceUI()
        {
            bandTurnUI.SetActive(false);
            songPerfomanceUI.SetActive(true);
        }

        public void ShowAudienceTurnUI()
        {
            bandTurnUI.SetActive(false);
            songPerfomanceUI.SetActive(false);
        }

        private void EndTurn()
        {
            if (GameManager.PersistentGameplayData.CurrentSong == null)
                return;

            if (GigManager.CurrentGigPhase == GigPhase.PlayerTurn)
            {
                GigManager.EndTurn();
            }
        }
    }
}