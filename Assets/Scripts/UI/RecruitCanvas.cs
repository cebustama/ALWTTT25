using ALWTTT.Characters.Band;
using ALWTTT.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class RecruitCanvas : MonoBehaviour
    {
        [Header("Characters")]
        [SerializeField] private LayoutGroup characterSpawnRoot;
        [SerializeField] private MusicianUI musicianUIPrefab;

        [Header("Cards")]
        [SerializeField] private LayoutGroup cardSpawnRoot;
        [SerializeField] private CardBase cardUIPrefab;

        [Header("References")]
        [SerializeField] private Button confirmButton;

        private List<MusicianUI> spawnedMusicians = new List<MusicianUI>();
        private MusicianCharacterData currentSelection;

        private Action<MusicianCharacterData> _onConfirmed;

        /// <summary>
        /// Shows the canvas, builds the options, and wires the confirm callback.
        /// Call Hide() to close, or it auto-hides on confirm.
        /// </summary>
        public void Show(
            List<MusicianBase> candidates, Action<MusicianCharacterData> onConfirmed)
        {
            Debug.Log($"<color=white>Showing Recruit Canvas with {candidates.Count} candidates.</color>");
            gameObject.SetActive(true);
            _onConfirmed = onConfirmed;
            BuildMusicianOptions(candidates);
            confirmButton.interactable = false; // until the player clicks one
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            ClearCards();
            _onConfirmed = null;
        }

        public void BuildMusicianOptions(List<MusicianBase> musicians)
        {
            // Clean old
            foreach (Transform child in characterSpawnRoot.transform)
                Destroy(child.gameObject);
            spawnedMusicians.Clear();

            foreach (var musician in musicians)
            {
                var ui = Instantiate(musicianUIPrefab, characterSpawnRoot.transform);
                ui.Build(musician.MusicianCharacterData, OnMusicianSelected);
                spawnedMusicians.Add(ui);
            }

            ClearCards();
            confirmButton.interactable = false;
        }

        private void OnMusicianSelected(MusicianUI selectedUI)
        {
            // Highlight only this one
            foreach (var ui in spawnedMusicians)
                ui.SetHighlight(ui == selectedUI);

            currentSelection = selectedUI.Data;

            // Show cards
            BuildCardOptions(currentSelection);

            confirmButton.interactable = true;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() => ChooseMusician(currentSelection));
        }

        private void BuildCardOptions(MusicianCharacterData musicianData)
        {
            ClearCards();

            foreach (var cardData in musicianData.BaseCards)
            {
                var cardUI = Instantiate(cardUIPrefab, cardSpawnRoot.transform);
                cardUI.SetCard(cardData, false);
            }
        }

        private void ClearCards()
        {
            foreach (Transform child in cardSpawnRoot.transform)
                Destroy(child.gameObject);
        }

        private void ChooseMusician(MusicianCharacterData musicianData)
        {
            Debug.Log("Chose " + musicianData.CharacterName);
            _onConfirmed?.Invoke(musicianData);
            Hide();
        }
    }
}