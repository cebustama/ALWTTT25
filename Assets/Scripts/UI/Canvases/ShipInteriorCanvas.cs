using ALWTTT.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Managers
{
    public class ShipInteriorCanvas : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button composeButton;
        [SerializeField] private Button relaxButton;
        [SerializeField] private Button bandTalkButton;

        [Header("Panels")]
        [SerializeField] private NewSongPanelUI newSongPanel;

        [Header("Toggles")]
        [SerializeField] private Toggle metronomeToggle;

        public NewSongPanelUI NewSongPanel => newSongPanel;

        public void Setup(Action onCompose, Action onRelax, Action onBandTalk)
        {
            gameObject.SetActive(true);

            composeButton.onClick.RemoveAllListeners();
            relaxButton.onClick.RemoveAllListeners();
            bandTalkButton.onClick.RemoveAllListeners();

            composeButton.onClick.AddListener(() => onCompose?.Invoke());
            relaxButton.onClick.AddListener(() => onRelax?.Invoke());
            bandTalkButton.onClick.AddListener(() => onBandTalk?.Invoke());
        }

        public void HookMetronomeToggle(Action<bool> onChanged, bool initialValue)
        {
            if (!metronomeToggle) return;
            metronomeToggle.onValueChanged.RemoveAllListeners();
            metronomeToggle.isOn = initialValue;
            metronomeToggle.onValueChanged.AddListener(v => onChanged?.Invoke(v));
        }
    }
}
