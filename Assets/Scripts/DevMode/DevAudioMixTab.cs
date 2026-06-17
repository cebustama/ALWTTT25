#if ALWTTT_DEV
using ALWTTT.Characters.Band;
using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.DevMode
{
    /// <summary>
    /// Dev Mode "Audio Mix" tab — IMGUI helper for DevModeController (M-AUDIO-MIX).
    /// Centralized mixing surface with three groups:
    ///   - Global music level (folds into every musician's effective volume)
    ///   - Per-musician music sliders (live via GigManager → MidiMusicManager.SetMusicianVolume01)
    ///   - Master SFX level (AudioManager.sfxSource + buttonSource)
    /// All edits route through GigManager.DevSet… wrappers, which apply live and,
    /// in the editor, persist to AudioMixSettingsSO. Live mix works even with no
    /// asset wired (banner shows "won't persist"); the SO is persistence/default
    /// only (D-MIX-FALLBACK=B). Mirrors the DevStatsTab IMGUI pattern.
    ///
    /// Also hosts the highlight trigger (ST-AM-6 / future highlight mechanic):
    /// Solo / Duck / Clear on a picked musician via MidiMusicManager.Highlight.
    ///
    /// Authority: tab surface → SSoT_Dev_Mode.md; mix model → SSoT_Audio.md.
    /// </summary>
    public static class DevAudioMixTab
    {
        private static Vector2 _scrollPos;
        private static GUIStyle _sectionHeader;
        private static GUIStyle _hint;
        private static int _highlightIndex;

        public static void Draw()
        {
            var gm = GigManager.Instance;
            if (gm == null)
            {
                GUILayout.Label("GigManager not available.");
                return;
            }

            EnsureStyles();

            if (!gm.DevHasAudioMixAsset)
            {
                GUILayout.Label(
                    "⚠ No AudioMixSettings wired — sliders work live but won't persist.",
                    _hint);
                GUILayout.Space(4);
            }

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            DrawGlobalMusic(gm);
            DrawPerMusician(gm);
            DrawMasterSfx(gm);
            DrawHighlightTrigger(gm);

            GUILayout.EndScrollView();
        }

        private static void EnsureStyles()
        {
            if (_sectionHeader == null)
                _sectionHeader = new GUIStyle(GUI.skin.label)
                { fontStyle = FontStyle.Bold, fontSize = 13 };

            if (_hint == null)
                _hint = new GUIStyle(GUI.skin.label)
                { fontStyle = FontStyle.Italic, fontSize = 11 };
        }

        private static void DrawGlobalMusic(GigManager gm)
        {
            GUILayout.Label("── Global Music ──", _sectionHeader);

            float cur = gm.DevGlobalMusicVolume01;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Music:", GUILayout.Width(110));
            float next = GUILayout.HorizontalSlider(cur, 0f, 1f, GUILayout.ExpandWidth(true));
            GUILayout.Label($"{cur:0.00}", GUILayout.Width(50));
            GUILayout.EndHorizontal();
            if (Mathf.Abs(next - cur) > 0.005f)
                gm.DevSetGlobalMusicVolume01(next);

            GUILayout.Space(8);
        }

        private static void DrawPerMusician(GigManager gm)
        {
            GUILayout.Label("── Per-Musician Music ──", _sectionHeader);

            var musicians = gm.CurrentMusicianCharacterList;
            if (musicians == null || musicians.Count == 0)
            {
                GUILayout.Label("No musicians spawned.");
                GUILayout.Space(8);
                return;
            }

            for (int i = 0; i < musicians.Count; i++)
            {
                MusicianBase m = musicians[i];
                if (m == null) continue;

                float cur = gm.DevGetMusicianVolume01(m.CharacterId);
                GUILayout.BeginHorizontal();
                GUILayout.Label(m.CharacterName, GUILayout.Width(110));
                float next = GUILayout.HorizontalSlider(cur, 0f, 1f, GUILayout.ExpandWidth(true));
                GUILayout.Label($"{cur:0.00}", GUILayout.Width(50));
                GUILayout.EndHorizontal();
                if (Mathf.Abs(next - cur) > 0.005f)
                    gm.DevSetMusicianVolume01(m, next);
            }

            GUILayout.Space(8);
        }

        private static void DrawMasterSfx(GigManager gm)
        {
            GUILayout.Label("── Master SFX ──", _sectionHeader);

            float cur = gm.DevMasterSfxVolume01;
            GUILayout.BeginHorizontal();
            GUILayout.Label("SFX:", GUILayout.Width(110));
            float next = GUILayout.HorizontalSlider(cur, 0f, 1f, GUILayout.ExpandWidth(true));
            GUILayout.Label($"{cur:0.00}", GUILayout.Width(50));
            GUILayout.EndHorizontal();
            if (Mathf.Abs(next - cur) > 0.005f)
                gm.DevSetMasterSfxVolume01(next);

            GUILayout.Space(4);
            GUILayout.Label(
                "Edits persist to AudioMixSettings.asset (editor) and load at gig start.",
                _hint);
            GUILayout.Space(8);
        }

        private static void DrawHighlightTrigger(GigManager gm)
        {
            GUILayout.Label("── Highlight (dev) ──", _sectionHeader);

            var musicians = gm.CurrentMusicianCharacterList;
            if (musicians == null || musicians.Count == 0)
            {
                GUILayout.Label("No musicians spawned.");
                return;
            }

            _highlightIndex = Mathf.Clamp(_highlightIndex, 0, musicians.Count - 1);
            var sel = musicians[_highlightIndex];
            string selName = sel != null ? sel.CharacterName : "(null)";

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◄", GUILayout.Width(25)))
                _highlightIndex = (_highlightIndex - 1 + musicians.Count) % musicians.Count;
            GUILayout.Label(selName, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("►", GUILayout.Width(25)))
                _highlightIndex = (_highlightIndex + 1) % musicians.Count;
            GUILayout.EndHorizontal();

            var mm = MidiMusicManager.Instance;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Solo") && sel != null)
                mm?.Highlight(sel.CharacterId, MidiMusicManager.HighlightMode.Solo);
            if (GUILayout.Button("Duck") && sel != null)
                mm?.Highlight(sel.CharacterId, MidiMusicManager.HighlightMode.DuckOthers);
            if (GUILayout.Button("Clear"))
                mm?.Highlight(null, MidiMusicManager.HighlightMode.None);
            GUILayout.EndHorizontal();
        }
    }
}
#endif