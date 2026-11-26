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

        // [S10 / D7=B] Wired in code (found by name when left empty), so the shared prefab
        // works the same in the Main Menu and in the pause menu.
        [Header("S10")]
        [SerializeField, Tooltip("Empty = child named 'BackButton'.")] private Button backButton;
        [SerializeField, Tooltip("Empty = child named 'FullscreenButton' (optional).")]
        private Button fullscreenButton;
        private TMP_Text _fullscreenLabel;

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
            SetFullscreenLabel(Screen.fullScreen); // [S10]

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

            // [S10 / D7=B]
            ApplyLayoutS10();
            if (backButton == null) backButton = FindChildButton("BackButton");
            if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
            if (fullscreenButton == null) fullscreenButton = FindChildButton("FullscreenButton");
            if (fullscreenButton != null)
            {
                _fullscreenLabel = fullscreenButton.GetComponentInChildren<TMP_Text>(true);
                fullscreenButton.onClick.AddListener(OnFullscreenPressed);
            }
        }

        // [S10 / D7=B] Look of the panel, set in code so it wins over the rect overrides the
        // two nested instances (Main Menu, pause) carry: dark veil, a centred card with the
        // title, two aligned volume rows, «Pantalla completa» and «Volver».
        private static readonly Color Veil = new Color(0.04f, 0.02f, 0.09f, 0.82f);
        private static readonly Color CardColor = new Color(0.15f, 0.09f, 0.25f, 0.97f);
        private static readonly Color Accent = new Color(1f, 0.82f, 0.3f, 1f);
        private static readonly Color DarkText = new Color(0.12f, 0.09f, 0.18f, 1f);

        private void ApplyLayoutS10()
        {
            var root = Root.transform as RectTransform;
            if (root == null) return;

            var veil = Root.GetComponent<Image>();
            if (veil != null) { veil.color = Veil; veil.raycastTarget = true; }

            var card = root.Find("Card") as RectTransform;
            if (card == null)
            {
                card = new GameObject("Card", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                card.SetParent(root, false);
                card.SetAsFirstSibling();
                card.GetComponent<Image>().color = CardColor;
            }
            Place(card, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 520f));

            var title = FindDeep(root, "Title");
            if (title != null)
            {
                title.SetParent(card, false);
                PlaceTop(title, -28f, new Vector2(700f, 80f));
                var t = title.GetComponent<TMP_Text>();
                if (t != null)
                {
                    t.enableAutoSizing = false; t.fontSize = 56f;
                    t.alignment = TextAlignmentOptions.Center; t.color = Color.white;
                }
            }

            StyleRow(FindDeep(root, "MusicRow"), card, -140f);
            StyleRow(FindDeep(root, "SfxRow"), card, -220f);

            var back = FindDeep(root, "BackButton");
            if (back != null)
            {
                back.SetParent(card, false);
                back.anchorMin = back.anchorMax = new Vector2(0.5f, 0f);
                back.pivot = new Vector2(0.5f, 0f);
                back.anchoredPosition = new Vector2(0f, 36f);
                back.sizeDelta = new Vector2(240f, 56f);
                StyleButtonLabel(back, "Volver");

                var full = FindDeep(root, "FullscreenButton");
                if (full == null)
                {
                    full = Instantiate(back.gameObject, card, false).GetComponent<RectTransform>();
                    full.name = "FullscreenButton";
                    var b = full.GetComponent<Button>();
                    if (b != null)
                    {
                        for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
                            b.onClick.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                        b.onClick.RemoveAllListeners();
                    }
                }
                PlaceTop(full, -310f, new Vector2(380f, 56f));
                StyleButtonLabel(full, "Pantalla completa: Sí");
            }
        }

        private static void StyleRow(RectTransform row, RectTransform card, float y)
        {
            if (row == null) return;
            row.SetParent(card, false);
            PlaceTop(row, y, new Vector2(700f, 60f));

            var h = row.GetComponent<HorizontalLayoutGroup>();
            if (h != null)
            {
                h.childAlignment = TextAnchor.MiddleCenter;
                h.spacing = 24f;
                h.childControlWidth = true;
                h.childControlHeight = true;
                h.childForceExpandWidth = false;
                h.childForceExpandHeight = false;
                h.padding = new RectOffset(0, 0, 0, 0);
            }

            foreach (RectTransform child in row)
            {
                var le = child.GetComponent<LayoutElement>();
                if (le == null) le = child.gameObject.AddComponent<LayoutElement>();
                var text = child.GetComponent<TMP_Text>();
                var slider = child.GetComponent<Slider>();
                if (slider != null)
                {
                    le.preferredWidth = 320f; le.preferredHeight = 28f;
                    SetImageColor(FindDeep(child, "Background"), new Color(1f, 1f, 1f, 0.25f));
                    SetImageColor(FindDeep(child, "Fill"), Accent);
                    var handle = FindDeep(child, "Handle");
                    if (handle != null) handle.sizeDelta = new Vector2(26f, 10f);
                }
                else if (text != null)
                {
                    bool isValue = child.name.EndsWith("Value");
                    le.preferredWidth = isValue ? 110f : 220f;
                    le.preferredHeight = 50f;
                    text.enableAutoSizing = false;
                    text.fontSize = 34f;
                    text.color = Color.white;
                    text.alignment = isValue ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
                }
            }
        }

        private static void SetImageColor(RectTransform rt, Color c)
        {
            var img = rt != null ? rt.GetComponent<Image>() : null;
            if (img != null) img.color = c;
        }

        private static void StyleButtonLabel(RectTransform button, string label)
        {
            var t = button.GetComponentInChildren<TMP_Text>(true);
            if (t == null) return;
            t.text = label;
            t.enableAutoSizing = false;
            t.fontSize = 30f;
            t.color = DarkText;
            t.alignment = TextAlignmentOptions.Center;
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        // Anchored to the top centre of its parent; y is negative going down.
        private static void PlaceTop(RectTransform rt, float y, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = size;
        }

        private static RectTransform FindDeep(Transform parent, string childName)
        {
            foreach (var t in parent.GetComponentsInChildren<RectTransform>(true))
                if (t.name == childName && t != parent) return t;
            return null;
        }

        private Button FindChildButton(string childName)
        {
            foreach (var b in Root.GetComponentsInChildren<Button>(true))
                if (b.name == childName) return b;
            return null;
        }

        // Screen.fullScreen applies on the next frame, so the label shows the requested value.
        private void OnFullscreenPressed()
        {
            bool next = !Screen.fullScreen;
            Screen.fullScreen = next;
            SetFullscreenLabel(next);
            Debug.Log($"[S10] Opciones | pantalla completa → {(next ? "sí" : "no")}");
        }

        private void SetFullscreenLabel(bool on)
        {
            if (_fullscreenLabel != null)
                _fullscreenLabel.text = on ? "Pantalla completa: Sí" : "Pantalla completa: No";
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