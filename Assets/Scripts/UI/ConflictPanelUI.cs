using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Managers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class ConflictPanelUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image musicianAImage;
        [SerializeField] private Image musicianBImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private Button confirmButton;

        private Action<bool> _onConfirm;

        public void Show(PersistentGameplayData.BandConflict conflict, Action<bool> onConfirm)
        {
            gameObject.SetActive(true);
            _onConfirm = onConfirm;

            var pd = GameManager.Instance.PersistentGameplayData;

            // Find current band members by id to get names/sprites
            var (nameA, spriteA) = GetMusicianVisual(pd, conflict.musicianAId);
            var (nameB, spriteB) = GetMusicianVisual(pd, conflict.musicianBId);

            if (titleText) titleText.text = $"New Conflict!";
            if (descText)
            {
                var who = string.IsNullOrEmpty(conflict.musicianBId)
                    ? $"{nameA} is struggling ({conflict.type}, severity {conflict.severity})."
                    : $"{nameA} and {nameB} are in conflict ({conflict.type}, severity {conflict.severity}).";
                descText.text = who;
            }

            if (musicianAImage) musicianAImage.sprite = spriteA;
            if (musicianBImage)
            {
                musicianBImage.gameObject.SetActive(!string.IsNullOrEmpty(conflict.musicianBId));
                musicianBImage.sprite = spriteB;
            }

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(() =>
            {
                _onConfirm?.Invoke(true);
                Hide();
            });
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onConfirm = null;
        }

        private (string, Sprite) GetMusicianVisual(PersistentGameplayData pd, string id)
        {
            if (string.IsNullOrEmpty(id)) return ("", null);
            foreach (var m in pd.MusicianList)
            {
                if (m && m.MusicianCharacterData.CharacterId == id)
                {
                    var data = m.MusicianCharacterData;
                    return (data.CharacterName, data.CharacterSprite);
                }
            }
            return (id, null);
        }
    }
}
