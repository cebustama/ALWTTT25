using ALWTTT.Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// <summary>
    /// [PAUSE-1 / D4=B] Shared Options panel: ONE prefab (OptionsPanel.prefab), instanced
    /// twice - inside the pause menu (DontDestroyOnLoad) and inside the Main Menu canvas.
    /// v1 content (D6): music volume + SFX volume. Language stays out (D-S5f-2=B). The
    /// tutorial revisit stays in the Main Menu's TutorialRevisitPanel (Design_Tutorial_System
    /// v0_3 section 5.2, D7=A) - see PAUSE-1 D6' (not hosted here).
    ///
    /// Volumes write PlayerAudioPrefs only (player plane, D10=B); never AudioMixSettingsSO.
    ///
    /// Esc: every open instance registers in a static stack; UIManager.HandleEscapeKey calls
    /// TryCloseTopmost() BEFORE the pause layer and BEFORE the scene rule, so Esc on the
    /// Main Menu with Options open closes Options instead of quitting (ST-CF3-15 holds).
    ///
    /// Wiring does not depend on Awake: the prefab can be saved inactive (Awake would not
    /// run until first activation), so listeners are attached lazily in Open().
    /// </summary>
    public sealed class OptionsPanelController : MonoBehaviour
    {
        private static readonly List<OptionsPanelController> s_open = new();

        [Header("Layout")]
        [SerializeField, Tooltip("GameObject shown/hidden by Open/Close. Defaults to this " +
            "GameObject when left empty.")]
        private GameObject panelRoot;

        [Header("Controls")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField, Tooltip("Optional percentage readout.")] private TMP_Text musicValueText;
        [SerializeField, Tooltip("Optional percentage readout.")] private TMP_Text sfxValueText;

        private Action _onClosed;
        private bool _wired;

        private GameObject Root => panelRoot != null ? panelRoot : gameObject;

        public bool IsOpen => s_open.Contains(this);

        /// <summary>Open the panel. onClosed runs when the panel is closed by the player
        /// (Back button or Esc), not when it is torn down by a scene change.</summary>
        public void Open(Action onClosed = null)
        {
            EnsureWired();
            _onClosed = onClosed;

            if (musicSlider != null) musicSlider.SetValueWithoutNotify(PlayerAudioPrefs.Music01);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(PlayerAudioPrefs.Sfx01);
            RefreshLabels();

            Root.SetActive(true);
            s_open.Remove(this);
            s_open.Add(this);
        }

        /// <summary>Wire to the Back button OnClick.</summary>
        public void OnBackPressed() => Close(true);

        /// <summary>Close. invokeCallback=false is used for teardown (scene change).</summary>
        public void Close(bool invokeCallback)
        {
            bool wasOpen = s_open.Remove(this);
            Root.SetActive(false);

            var cb = _onClosed;
            _onClosed = null;
            if (!wasOpen) return;

            PlayerAudioPrefs.Save();
            if (invokeCallback) cb?.Invoke();
        }

        /// <summary>Esc layer: closes the most recently opened panel. True if one closed.</summary>
        public static bool TryCloseTopmost()
        {
            for (int i = s_open.Count - 1; i >= 0; i--)
            {
                var p = s_open[i];
                if (p == null) { s_open.RemoveAt(i); continue; }
                p.Close(true);
                return true;
            }
            return false;
        }

        private void OnDisable()
        {
            // Hidden by a parent (pause root hidden, Main Menu unloaded): drop from the stack
            // without running the callback. Save anything already chosen.
            if (s_open.Remove(this))
            {
                _onClosed = null;
                PlayerAudioPrefs.Save();
            }
        }

        private void EnsureWired()
        {
            if (_wired) return;
            _wired = true;

            if (musicSlider != null)
            {
                musicSlider.minValue = 0f;
                musicSlider.maxValue = 1f;
                musicSlider.wholeNumbers = false;
                musicSlider.onValueChanged.AddListener(OnMusicChanged);
            }
            if (sfxSlider != null)
            {
                sfxSlider.minValue = 0f;
                sfxSlider.maxValue = 1f;
                sfxSlider.wholeNumbers = false;
                sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            }
        }

        private void OnMusicChanged(float v)
        {
            PlayerAudioPrefs.SetMusic01(v);
            RefreshLabels();
        }

        private void OnSfxChanged(float v)
        {
            PlayerAudioPrefs.SetSfx01(v);
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            if (musicValueText != null)
                musicValueText.text = Mathf.RoundToInt(PlayerAudioPrefs.Music01 * 100f) + "%";
            if (sfxValueText != null)
                sfxValueText.text = Mathf.RoundToInt(PlayerAudioPrefs.Sfx01 * 100f) + "%";
        }

        // Enter-Play-Mode without domain reload keeps statics alive.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => s_open.Clear();
    }
}