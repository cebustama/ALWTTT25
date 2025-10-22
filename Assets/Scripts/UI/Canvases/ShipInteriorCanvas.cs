using ALWTTT.UI;
using System;
using System.Collections.Generic;
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
        [SerializeField] private Button playButton;

        [Header("Panels")]
        [SerializeField] private NewSongPanelUI newSongPanel;

        [Header("Dev")]
        [SerializeField] private GameObject devPanel;
        [SerializeField] private Toggle metronomeToggle;
        [SerializeField] private TMP_Dropdown highlightDropdown;
        [SerializeField] private TMP_Dropdown tempoScaleDropdown;
        [SerializeField] private Toggle layeredEntranceToggle;
        [SerializeField] private Toggle enablePPToggle;
        [SerializeField] private Toggle enablePersonalityToggle;

        [Header("Mutators (Dev)")]
        [SerializeField] private TMP_Dropdown introDropdown;
        [SerializeField] private TMP_Dropdown outroDropdown;
        [SerializeField] private TMP_Dropdown soloDropdown;
        // Alt track
        [SerializeField] private TMP_Dropdown altTrackDropdown;
        [SerializeField] private TMP_Dropdown altStrategyDropdown;

        public NewSongPanelUI NewSongPanel => newSongPanel;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                devPanel.SetActive(!devPanel.activeSelf);
            }
        }

        public void Setup(Action onCompose, Action onRelax, Action onBandTalk)
        {
            gameObject.SetActive(true);

            composeButton.onClick.RemoveAllListeners();
            relaxButton.onClick.RemoveAllListeners();
            bandTalkButton.onClick.RemoveAllListeners();

            composeButton.onClick.AddListener(() => onCompose?.Invoke());
            relaxButton.onClick.AddListener(() => onRelax?.Invoke());
            bandTalkButton.onClick.AddListener(() => onBandTalk?.Invoke());

            SetMainButtonsVisible(true);
        }

        public void HookPlayButton(Action onPlay)
        {
            if (playButton == null) return;
            playButton.onClick.RemoveAllListeners();
            if (onPlay != null) playButton.onClick.AddListener(() => onPlay());
        }

        public void HookMetronomeToggle(Action<bool> onChanged, bool initialValue)
        {
            if (!metronomeToggle) return;
            metronomeToggle.onValueChanged.RemoveAllListeners();
            metronomeToggle.isOn = initialValue;
            metronomeToggle.onValueChanged.AddListener(v => onChanged?.Invoke(v));
        }

        public void HookLayeredEntranceToggle(Action<bool> onChanged, bool initialValue)
        {
            if (!layeredEntranceToggle) return;
            layeredEntranceToggle.onValueChanged.RemoveAllListeners();
            layeredEntranceToggle.isOn = initialValue;
            layeredEntranceToggle.onValueChanged.AddListener(v => onChanged?.Invoke(v));
        }

        public void HookEnablePPToggle(Action<bool> onChanged, bool initialValue)
        {
            if (!enablePPToggle) return;
            enablePPToggle.onValueChanged.RemoveAllListeners();
            enablePPToggle.isOn = initialValue;
            enablePPToggle.onValueChanged.AddListener(v => onChanged?.Invoke(v));
        }

        public void HookEnablePersonalityToggle(Action<bool> onChanged, bool initialValue)
        {
            if (!enablePersonalityToggle) return;
            enablePersonalityToggle.onValueChanged.RemoveAllListeners();
            enablePersonalityToggle.isOn = initialValue;
            enablePersonalityToggle.onValueChanged.AddListener(v => onChanged?.Invoke(v));
        }
        public void PopulateHighlightDropdown(
            IReadOnlyList<(string id, string name)> items,
            Action<string> onSelected,
            bool includeNoneOption = true)
        {
            if (!highlightDropdown) return;

            highlightDropdown.onValueChanged.RemoveAllListeners();

            var options = new List<TMP_Dropdown.OptionData>();
            var backingIds = new List<string>();

            if (includeNoneOption)
            {
                options.Add(new TMP_Dropdown.OptionData("— None —"));
                backingIds.Add(null);
            }

            foreach (var (id, name) in items)
            {
                options.Add(new TMP_Dropdown.OptionData(name));
                backingIds.Add(id);
            }

            highlightDropdown.options = options;
            highlightDropdown.value = 0;
            highlightDropdown.RefreshShownValue();

            highlightDropdown.onValueChanged.AddListener(idx =>
            {
                var chosenId = (idx >= 0 && idx < backingIds.Count) ? backingIds[idx] : null;
                onSelected?.Invoke(chosenId);
            });
        }

        public void PopulateTempoScaleDropdown(
            IReadOnlyList<(string label, float factor)> optionsInOrder,
            Action<float> onSelected,
            int defaultIndex = 0)
        {
            if (!tempoScaleDropdown) return;
            tempoScaleDropdown.onValueChanged.RemoveAllListeners();

            var options = new List<TMP_Dropdown.OptionData>();
            var backing = new List<float>();

            foreach (var (label, factor) in optionsInOrder)
            {
                options.Add(new TMP_Dropdown.OptionData(label));
                backing.Add(factor);
            }

            tempoScaleDropdown.options = options;
            tempoScaleDropdown.value = Mathf.Clamp(defaultIndex, 0, options.Count - 1);
            tempoScaleDropdown.RefreshShownValue();

            tempoScaleDropdown.onValueChanged.AddListener(idx =>
            {
                if (idx < 0 || idx >= backing.Count) return;
                onSelected?.Invoke(backing[idx]);
            });
        }

        public void PopulateIntroDropdown(
            IReadOnlyList<(string id, string name)> items,
            Action<string> onSelected,
            bool includeNoneOption = true)
        {
            if (!introDropdown) return;

            introDropdown.onValueChanged.RemoveAllListeners();

            var options = new List<TMP_Dropdown.OptionData>();
            var backingIds = new List<string>();

            if (includeNoneOption)
            {
                options.Add(new TMP_Dropdown.OptionData("— None —"));
                backingIds.Add(null);
            }

            foreach (var (id, name) in items)
            {
                options.Add(new TMP_Dropdown.OptionData(name));
                backingIds.Add(id);
            }

            introDropdown.options = options;
            introDropdown.value = 0; // default to NONE → no intro
            introDropdown.RefreshShownValue();

            introDropdown.onValueChanged.AddListener(idx =>
            {
                var chosenId = (idx >= 0 && idx < backingIds.Count) ? backingIds[idx] : null;
                onSelected?.Invoke(chosenId); // null => NONE
            });
        }

        public void PopulateSoloDropdown(
            IReadOnlyList<(string id, string name)> items,
            Action<string> onSelected,
            bool includeNoneOption = true)
        {
            if (!soloDropdown) return;

            soloDropdown.onValueChanged.RemoveAllListeners();

            var options = new List<TMP_Dropdown.OptionData>();
            var backingIds = new List<string>();

            if (includeNoneOption)
            {
                options.Add(new TMP_Dropdown.OptionData("— None —"));
                backingIds.Add(null);
            }

            foreach (var (id, name) in items)
            {
                options.Add(new TMP_Dropdown.OptionData(name));
                backingIds.Add(id);
            }

            soloDropdown.options = options;
            soloDropdown.value = 0; // default to NONE → no solo
            soloDropdown.RefreshShownValue();

            soloDropdown.onValueChanged.AddListener(idx =>
            {
                var chosenId = (idx >= 0 && idx < backingIds.Count) ? backingIds[idx] : null;
                onSelected?.Invoke(chosenId); // null => NONE
            });
        }

        public void PopulateAlternateTrackDropdown(
            IReadOnlyList<(string id, string name)> items,
            Action<string> onSelected,
            bool includeNoneOption = true)
        {
            if (!altTrackDropdown) return;

            altTrackDropdown.onValueChanged.RemoveAllListeners();

            var options = new List<TMP_Dropdown.OptionData>();
            var backingIds = new List<string>();

            if (includeNoneOption)
            {
                options.Add(new TMP_Dropdown.OptionData("— None —"));
                backingIds.Add(null);
            }

            foreach (var (id, name) in items)
            {
                options.Add(new TMP_Dropdown.OptionData(name));
                backingIds.Add(id);
            }

            altTrackDropdown.options = options;
            altTrackDropdown.value = 0;
            altTrackDropdown.RefreshShownValue();

            altTrackDropdown.onValueChanged.AddListener(idx =>
            {
                var chosenId = (idx >= 0 && idx < backingIds.Count) ? backingIds[idx] : null;
                onSelected?.Invoke(chosenId); // null => NONE
            });
        }

        // strategy dropdown (NONE means “don’t queue alternate”)
        public void PopulateAlternateStrategyDropdown(
            IReadOnlyList<(string id, string label)> strategies,
            Action<string> onSelected,
            bool includeNoneOption = true)
        {
            if (!altStrategyDropdown) return;

            altStrategyDropdown.onValueChanged.RemoveAllListeners();

            var options = new List<TMP_Dropdown.OptionData>();
            var backingIds = new List<string>();

            if (includeNoneOption)
            {
                options.Add(new TMP_Dropdown.OptionData("— None —"));
                backingIds.Add(null);
            }

            foreach (var (id, label) in strategies)
            {
                options.Add(new TMP_Dropdown.OptionData(label));
                backingIds.Add(id);
            }

            altStrategyDropdown.options = options;
            altStrategyDropdown.value = 0; // NONE
            altStrategyDropdown.RefreshShownValue();

            altStrategyDropdown.onValueChanged.AddListener(idx =>
            {
                var chosenId = (idx >= 0 && idx < backingIds.Count) ? backingIds[idx] : null;
                onSelected?.Invoke(chosenId); // null => NONE
            });
        }

        public void PopulateOutroDropdown(
            IReadOnlyList<(string id, string name)> items,
            Action<string> onSelected,
            bool includeNoneOption = true)
        {
            if (!outroDropdown) return;

            outroDropdown.onValueChanged.RemoveAllListeners();

            var options = new List<TMP_Dropdown.OptionData>();
            var backingIds = new List<string>();

            if (includeNoneOption)
            {
                options.Add(new TMP_Dropdown.OptionData("— None —"));
                backingIds.Add(null);
            }

            foreach (var (id, name) in items)
            {
                options.Add(new TMP_Dropdown.OptionData(name));
                backingIds.Add(id);
            }

            outroDropdown.options = options;
            outroDropdown.value = 0; // NONE → no outro
            outroDropdown.RefreshShownValue();

            outroDropdown.onValueChanged.AddListener(idx =>
            {
                var chosenId = (idx >= 0 && idx < backingIds.Count) ? backingIds[idx] : null;
                onSelected?.Invoke(chosenId); // null => NONE
            });
        }

        public void SetMainButtonsVisible(bool visible)
        {
            if (composeButton) composeButton.gameObject.SetActive(visible);
            if (relaxButton) relaxButton.gameObject.SetActive(visible);
            if (bandTalkButton) bandTalkButton.gameObject.SetActive(visible);

            // Optional: also hide the dev panel when buttons are hidden
            //if (devPanel) devPanel.SetActive(visible);
        }
    }
}
