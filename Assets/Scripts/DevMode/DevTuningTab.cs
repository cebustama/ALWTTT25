#if ALWTTT_DEV
using System;
using System.Globalization;
using ALWTTT.Data;
using ALWTTT.Managers;
using UnityEngine;
using K = ALWTTT.Data.DevTuning.Keys;

namespace ALWTTT.DevMode
{
    /// <summary>
    /// [DEV-TUNING-1] Dev Mode ▸ «Ajustes» (F5): variables de gameplay sin tocar Unity.
    ///
    /// La pestaña se dibuja sola a partir de <see cref="Registry"/>. Añadir un ajuste:
    ///   1. una clave en <see cref="DevTuning.Keys"/>,
    ///   2. una línea aquí (nombre, grupo, tipo, rango y de dónde sale el valor del asset),
    ///   3. una lectura <c>DevTuning.Int/Float(clave, valorDelAsset)</c> en el consumidor.
    /// Los cambios se aplican al momento en memoria; «Guardar» los deja en
    /// persistentDataPath/DevTuning.json y «Valores por defecto» lo borra.
    /// </summary>
    public static class DevTuningTab
    {
        private enum Kind { Int, Float, Option }

        private sealed class Setting
        {
            public string Key, Group, Label, When;
            public Kind Kind;
            public float Min, Max, Step;
            public string[] Options;
            public Func<float> Asset;   // valor del asset, en vivo
        }

        // ─── El registro: una línea por ajuste ───────────────────────────
        private static readonly Setting[] Registry =
        {
            Opt(K.PlaysMode,     "Jugadas", "Cartas por turno y músico", new[] { "Una de cada tipo", "Una en total", "Sin límite" },
                () => DevTuning.PlaysModeAsset, "desde el siguiente periodo"),
            Int(K.DrawTurn,      "Robo",    "Robo al empezar el turno", 0, 10, () => Pd?.DrawCount ?? Gd?.DrawCount ?? 0, "siguiente turno"),
            Int(K.DrawOnPlay,    "Robo",    "Robo al pulsar Play",      0, 5,  () => Gm?.Flow?.DrawCardsOnPlay ?? 0, "siguiente Play"),
            Int(K.DrawLoop,      "Robo",    "Robo entre loops",         0, 5,  () => Gm?.Flow?.DrawPerLoop ?? 0, "siguiente loop"),
            Int(K.HandMax,       "Mano",    "Máximo de la mano",        1, 15, () => Gd?.MaxCardsOnHand ?? 8, "al momento"),
            Int(K.InspStart,     "Inspiración", "★ al empezar el gig",  0, 10, () => Pd?.InitialGigInspiration ?? 0, "siguiente gig"),
            Flt(K.CollapseStress, "Colapso", "Vida tras el colapso (fracción de la barra)", 0f, 1f, 0.05f,
                () => Gm?.Meters != null ? Gm.Meters.BreakdownStressResetFraction : 0.5f, "siguiente colapso"),
        };

        private static Setting Int(string key, string group, string label, int min, int max, Func<float> asset, string when) =>
            new Setting { Key = key, Group = group, Label = label, Kind = Kind.Int, Min = min, Max = max, Step = 1, Asset = asset, When = when };
        private static Setting Flt(string key, string group, string label, float min, float max, float step, Func<float> asset, string when) =>
            new Setting { Key = key, Group = group, Label = label, Kind = Kind.Float, Min = min, Max = max, Step = step, Asset = asset, When = when };
        private static Setting Opt(string key, string group, string label, string[] options, Func<float> asset, string when) =>
            new Setting { Key = key, Group = group, Label = label, Kind = Kind.Option, Options = options, Asset = asset, When = when };

        private static GigManager Gm => GigManager.Instance;
        private static GameplayData Gd => GameManager.Instance != null ? GameManager.Instance.GameplayData : null;
        private static PersistentGameplayData Pd => GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;

        private static GUIStyle _header, _hint, _bold;

        // ─── Dibujo ──────────────────────────────────────────────────────

        public static void Draw(DevModeController host)
        {
            EnsureStyles();

            GUILayout.Label(DevTuning.HasOverrides
                ? $"Ajustes distintos del asset: {DevTuning.Describe()}"
                : "Valores de los assets (de fábrica).", _hint);
            if (DevTuning.IsDirty)
                GUILayout.Label("● Hay cambios sin guardar: valen ya en esta partida, pero se pierden al cerrar el juego.", _bold);

            string group = null;
            foreach (var s in Registry)
            {
                if (s.Group != group)
                {
                    group = s.Group;
                    GUILayout.Space(6);
                    GUILayout.Label($"── {group} ──", _header);
                }
                DrawSetting(s);
            }

            // ★ por loop y el resto de la economía de la Inspiración ya viven en su pestaña.
            GUILayout.BeginHorizontal();
            GUILayout.Label("★ entre loops, Finisher y máximo de ★: pestaña Inspiración.", _hint);
            if (GUILayout.Button("Ir", GUILayout.Width(40))) host.ShowTab(DevModeController.TabInspiration);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("── Sesión (no se guarda) ──", _header);
            bool inf = GUILayout.Toggle(DevModeController.InfiniteTurnsEnabled, " Sin límite de canciones (Infinite Turns)");
            if (inf != DevModeController.InfiniteTurnsEnabled) DevModeController.SetInfiniteTurns(inf);

            var gm = Gm;
            if (gm != null)
            {
                GUILayout.BeginHorizontal();
                string label = gm.DevFastForwardSong ? "⏹ Cancelar el salto (F6)" : "⏩ Saltar canción → turno del público (F6)";
                if (GUILayout.Button(label)) gm.DevToggleFastForwardSong();
                GUILayout.EndHorizontal();
                GUILayout.Label(gm.DevFastForwardSong
                    ? "Armado: cada loop se corta al sonar y puntúa como siempre. Si aún no has pulsado Play, salta cuando lo pulses."
                    : "Corta los loops que quedan; la canción puntúa con lo que haya en ella.", _hint);
            }

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Guardar")) DevTuning.Save();
            if (GUILayout.Button("Valores por defecto")) DevTuning.ResetAll();
            GUILayout.EndHorizontal();
            GUILayout.Label($"Se guarda solo lo cambiado, en {DevTuning.FilePath}. Los assets no se tocan.", _hint);
        }

        private static void DrawSetting(Setting s)
        {
            float asset = s.Asset();
            bool overridden = DevTuning.IsOverridden(s.Key);

            GUILayout.BeginHorizontal();
            GUILayout.Label(overridden ? $"● {s.Label}" : s.Label, overridden ? _bold : GUI.skin.label, GUILayout.Width(260));

            switch (s.Kind)
            {
                case Kind.Int:
                {
                    int assetI = Mathf.RoundToInt(asset);
                    int cur = DevTuning.Int(s.Key, assetI);
                    int nv = Mathf.RoundToInt(GUILayout.HorizontalSlider(cur, s.Min, s.Max, GUILayout.Width(160)));
                    GUILayout.Label(nv.ToString(), GUILayout.Width(30));
                    if (nv != cur) DevTuning.Set(s.Key, Str(nv), Str(assetI), s.Label);
                    break;
                }
                case Kind.Float:
                {
                    float cur = DevTuning.Float(s.Key, asset);
                    float nv = GUILayout.HorizontalSlider(cur, s.Min, s.Max, GUILayout.Width(160));
                    if (s.Step > 0f) nv = Mathf.Round(nv / s.Step) * s.Step;
                    GUILayout.Label(nv.ToString("0.00", CultureInfo.InvariantCulture), GUILayout.Width(40));
                    if (Mathf.Abs(nv - cur) > 0.0001f) DevTuning.Set(s.Key, Str(nv), Str(asset), s.Label);
                    break;
                }
                case Kind.Option:
                {
                    int assetI = Mathf.RoundToInt(asset);
                    int cur = Mathf.Clamp(DevTuning.Int(s.Key, assetI), 0, s.Options.Length - 1);
                    int nv = GUILayout.Toolbar(cur, s.Options);
                    if (nv != cur) DevTuning.Set(s.Key, Str(nv), Str(assetI), s.Label);
                    break;
                }
            }

            GUI.enabled = overridden;
            if (GUILayout.Button("↺", GUILayout.Width(24)))
            {
                string assetStr = s.Kind == Kind.Float ? Str(asset) : Str(Mathf.RoundToInt(asset));
                DevTuning.Set(s.Key, assetStr, assetStr, s.Label);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            string assetShown = s.Kind == Kind.Option ? s.Options[Mathf.Clamp(Mathf.RoundToInt(asset), 0, s.Options.Length - 1)]
                : s.Kind == Kind.Float ? asset.ToString("0.00", CultureInfo.InvariantCulture) : Mathf.RoundToInt(asset).ToString();
            GUILayout.Label($"      asset: {assetShown} · se aplica: {s.When}", _hint);
        }

        private static string Str(int v) => v.ToString(CultureInfo.InvariantCulture);
        private static string Str(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);

        private static void EnsureStyles()
        {
            if (_header == null)
                _header = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 13 };
            if (_hint == null)
                _hint = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic, fontSize = 11, wordWrap = true };
            if (_bold == null)
                _bold = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = true };
        }
    }
}
#endif
