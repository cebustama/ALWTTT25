using MidiGenPlay;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Characters.Band
{
    public class BandCharacterCanvas : CharacterCanvas
    {
        [Header("Character")]
        [SerializeField] private CanvasGroup statsCanvasGroup;

        [Tooltip("Optional: the StatsRoot GameObject that hosts the stat text fields. " +
                 "If assigned, its active state is toggled alongside the CanvasGroup alpha " +
                 "in ShowContextual/HideContextual. Useful when stat text fields may not " +
                 "all sit under the same CanvasGroup.")]
        [SerializeField] private GameObject statsRoot;

        [SerializeField] private TextMeshProUGUI chrTextField;
        [SerializeField] private TextMeshProUGUI tchTextField;
        [SerializeField] private TextMeshProUGUI emtTextField;

        [Header("Dev")]
        [SerializeField] private TMP_Dropdown instrumentDebugDropdown;
        [SerializeField] private Slider volumeDebugSlider;

        /// <summary>
        /// Defensive initialization: force stats to hidden before BuildCharacter runs
        /// (which also calls HideContextual). Prevents a single-frame flicker where
        /// stats are visible at scene load before the character is built.
        /// </summary>
        private void Awake()
        {
            ApplyStatsVisibility(visible: false);
        }

        public void UpdateStats(int chr, int tch, int emt)
        {
            if (chrTextField != null) chrTextField.text = $"CHR: {chr}";
            if (tchTextField != null) tchTextField.text = $"TCH: {tch}";
            if (emtTextField != null) emtTextField.text = $"EMT: {emt}";
        }

        public override void ShowContextual()
        {
            base.ShowContextual();
            ApplyStatsVisibility(visible: true);
        }

        public override void HideContextual()
        {
            base.HideContextual();
            ApplyStatsVisibility(visible: false);
        }

        /// <summary>
        /// Single code path for stats visibility. Toggles CanvasGroup alpha and,
        /// if assigned, the statsRoot GameObject active state.
        /// </summary>
        private void ApplyStatsVisibility(bool visible)
        {
            if (statsCanvasGroup != null)
                statsCanvasGroup.alpha = visible ? 1f : 0f;

            if (statsRoot != null)
                statsRoot.SetActive(visible);
        }

        public void SetCurrentStress(int current, int max, float duration)
        {
            healthBar?.SetCurrentValue(current, max, duration);
        }

        #region Debug
        public void SetupInstrumentDebugDropdown(
            bool debugEnabled,
            IReadOnlyList<MIDIInstrumentSO> options,
            Action<MIDIInstrumentSO> onSelected)
        {
            if (!instrumentDebugDropdown) return;

            instrumentDebugDropdown.onValueChanged.RemoveAllListeners();
            instrumentDebugDropdown.gameObject.SetActive(debugEnabled);

            // Backing list to map indices → instruments
            var backing = new List<MIDIInstrumentSO>();

            var uiOptions = new List<TMP_Dropdown.OptionData>();

            // Index 0 = "None" → no override → use normal random system
            uiOptions.Add(new TMP_Dropdown.OptionData("— None —"));
            backing.Add(null);

            if (options != null)
            {
                foreach (var inst in options)
                {
                    if (!inst) continue;

                    string label = !string.IsNullOrEmpty(inst.InstrumentName)
                        ? inst.InstrumentName
                        : inst.name;

                    uiOptions.Add(new TMP_Dropdown.OptionData(label));
                    backing.Add(inst);
                }
            }

            instrumentDebugDropdown.options = uiOptions;
            instrumentDebugDropdown.value = 0;
            instrumentDebugDropdown.RefreshShownValue();

            if (!debugEnabled) return;

            instrumentDebugDropdown.onValueChanged.AddListener(idx =>
            {
                MIDIInstrumentSO chosen = null;
                if (idx >= 0 && idx < backing.Count)
                    chosen = backing[idx];

                onSelected?.Invoke(chosen);
            });
        }

        public void SetupPercussionInstrumentDebugDropdown(
            bool debugEnabled,
            IReadOnlyList<MIDIPercussionInstrumentSO> options,
            Action<MIDIPercussionInstrumentSO> onSelected)
        {
            if (!instrumentDebugDropdown) return;

            instrumentDebugDropdown.onValueChanged.RemoveAllListeners();
            instrumentDebugDropdown.gameObject.SetActive(debugEnabled);

            var backing = new List<MIDIPercussionInstrumentSO>();
            var uiOptions = new List<TMP_Dropdown.OptionData>();

            // Index 0 = "None" → no override
            uiOptions.Add(new TMP_Dropdown.OptionData("— Drums: None —"));
            backing.Add(null);

            if (options != null)
            {
                foreach (var inst in options)
                {
                    if (!inst) continue;

                    string label = !string.IsNullOrEmpty(inst.InstrumentName)
                        ? inst.InstrumentName
                        : inst.name;

                    uiOptions.Add(new TMP_Dropdown.OptionData(label));
                    backing.Add(inst);
                }
            }

            instrumentDebugDropdown.options = uiOptions;
            instrumentDebugDropdown.value = 0;
            instrumentDebugDropdown.RefreshShownValue();

            if (!debugEnabled) return;

            instrumentDebugDropdown.onValueChanged.AddListener(idx =>
            {
                MIDIPercussionInstrumentSO chosen = null;
                if (idx >= 0 && idx < backing.Count)
                    chosen = backing[idx];

                onSelected?.Invoke(chosen);
            });
        }

        public void SetupVolumeDebugSlider(
            bool enabled,
            Action<float> onValueChanged,
            float initialValue = 1f)
        {
            if (!volumeDebugSlider) return;

            volumeDebugSlider.onValueChanged.RemoveAllListeners();
            volumeDebugSlider.minValue = 0f;
            volumeDebugSlider.maxValue = 1f;
            volumeDebugSlider.wholeNumbers = false;

            volumeDebugSlider.gameObject.SetActive(enabled);

            if (!enabled)
                return;

            // Avoid feedback-loop on first frame
            volumeDebugSlider.SetValueWithoutNotify(initialValue);

            volumeDebugSlider.onValueChanged.AddListener(v =>
            {
                onValueChanged?.Invoke(v);
            });
        }
        #endregion
    }
}