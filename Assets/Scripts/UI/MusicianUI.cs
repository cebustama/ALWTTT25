using ALWTTT.Data;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class MusicianUI : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private Image musicianImage;

        private MusicianCharacterData musicianCharacterData;
        private Action<MusicianUI> onSelected;

        public MusicianCharacterData Data => musicianCharacterData;

        public void Build(MusicianCharacterData data, Action<MusicianUI> onSelectedCallback)
        {
            Debug.Log($"<color=white>Building {data.CharacterName} option...</color>");
            musicianCharacterData = data;
            musicianImage.sprite = data.CharacterSprite;

            onSelected = onSelectedCallback;

            SetHighlight(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onSelected?.Invoke(this);
        }

        public void SetHighlight(bool isSelected)
        {
            musicianImage.color = isSelected ? Color.white : Color.black;
        }
    }
}