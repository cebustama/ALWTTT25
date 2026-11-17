#if ALWTTT_DEV
using ALWTTT.Data;
using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.DevMode
{
    /// <summary>
    /// [ECON-2] Dev Mode ▸ «Inspiración»: las reglas de la Inspiración y los Finisher,
    /// en vivo y en la build. Cada cambio se aplica al momento y se guarda en esta
    /// máquina (PlayerPrefs), así Matías puede probar por su cuenta y comparar A/B
    /// sin tocar assets. «Restablecer» borra los ajustes locales y vuelve a
    /// GigFlowSettings ▸ ECON-2.
    /// Lo que se cambia aquí lo dice el log con la etiqueta [ECON-2].
    /// </summary>
    public static class DevInspirationRulesTab
    {
        private static GUIStyle _header;
        private static GUIStyle _hint;

        public static void Draw()
        {
            EnsureStyles();
            var r = InspirationRules.Active;
            var gm = GigManager.Instance;
            bool changed = false;

            GUILayout.Label(InspirationRules.HasLocalOverrides
                ? "Ajustes LOCALES activos (guardados en esta máquina)."
                : "Usando los valores del asset (GigFlowSettings ▸ ECON-2).", _hint);

            if (gm != null)
            {
                var pd = GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;
                GUILayout.Label($"Ahora: {gm.LiveInspiration}/{(pd != null ? pd.MaxInspiration : 0)}★ · " +
                                gm.FinisherStatusForDev(), _hint);
            }

            GUILayout.Space(6);
            GUILayout.Label("── Composición ──", _header);
            changed |= Toggle(ref r.compositionGrantsInspiration, "Las composiciones dan ★ al jugarlas");
            changed |= IntOverride(ref r.compositionInspirationOverride, "★ por composición", "la de la carta", 0, 5);

            GUILayout.Space(6);
            GUILayout.Label("── Contador ──", _header);
            changed |= IntOverride(ref r.maxInspirationOverride, "Máximo de ★", "asset", 1, 20);
            changed |= IntOverride(ref r.inspirationPerLoopOverride, "★ básica por loop", "asset", 0, 5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Guardar ★ entre canciones", GUILayout.Width(200));
            int keep = GUILayout.Toolbar(r.keepBetweenSongsOverride + 1, new[] { "asset", "no", "sí" });
            GUILayout.EndHorizontal();
            if (keep - 1 != r.keepBetweenSongsOverride) { r.keepBetweenSongsOverride = keep - 1; changed = true; }

            changed |= Toggle(ref r.ignoreResourceCosts, "Ignorar el coste de Voltaje");

            GUILayout.Space(6);
            GUILayout.Label("── Finisher ──", _header);
            changed |= Toggle(ref r.finisherOnlyFinalLoop, "Solo en el último loop (apagado: en cualquier loop)");
            changed |= IntOverride(ref r.finisherCostOverride, "Coste del Finisher", "el de la carta", 0, 10);
            changed |= IntOverride(ref r.maxFinishersPerSong, "Finisher por canción", "sin límite", 1, 4, offValue: 0);
            changed |= Toggle(ref r.finisherSolo, "Cada Finisher toca su solo");
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Volumen del 2.º solista en adelante: {r.extraSoloistVolume01:0.00}", GUILayout.Width(260));
            float v = GUILayout.HorizontalSlider(r.extraSoloistVolume01, 0f, 1f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(v - r.extraSoloistVolume01) > 0.005f) { r.extraSoloistVolume01 = Mathf.Round(v * 20f) / 20f; changed = true; }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Banda en el «break» (Finisher sin solo): {r.breakDuck01:0.00}", GUILayout.Width(260));
            float bd = GUILayout.HorizontalSlider(r.breakDuck01, 0f, 1f);
            GUILayout.EndHorizontal();
            if (Mathf.Abs(bd - r.breakDuck01) > 0.005f) { r.breakDuck01 = Mathf.Round(bd * 20f) / 20f; changed = true; }
            changed |= Toggle(ref r.finisherEffectsAtFinalLoopStart, "Efectos al empezar el loop final (apagado: al jugar)");

            // [BATERIA-SOLO-1] El solo de batería del Finisher de C2.
            changed |= Toggle(ref r.drumSolo, "Solo de batería en el «break» (apagado: solo volumen)");
            if (r.drumSolo)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Forma del solo", GUILayout.Width(120));
                int shape = GUILayout.Toolbar(r.drumSoloShape, new[] { "Groove + fills", "Encadenados", "Trading" });
                GUILayout.EndHorizontal();
                if (shape != r.drumSoloShape) { r.drumSoloShape = shape; changed = true; }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Carácter", GUILayout.Width(120));
                int sel = GUILayout.Toolbar(r.drumSoloVariant + 1,
                    new[] { "Semilla", "Toms 3", "Cascada", "Rudim.", "Rituales", "Máquina" });
                GUILayout.EndHorizontal();
                if (sel - 1 != r.drumSoloVariant) { r.drumSoloVariant = sel - 1; changed = true; }

                // [D-SOLO-BPM] tempo above which the solo plays at half resolution
                GUILayout.BeginHorizontal();
                GUILayout.Label(r.drumSoloHalfTimeAboveBpm > 0
                    ? $"Medio tiempo sobre {r.drumSoloHalfTimeAboveBpm} BPM" : "Medio tiempo: nunca", GUILayout.Width(220));
                int ht = Mathf.RoundToInt(GUILayout.HorizontalSlider(r.drumSoloHalfTimeAboveBpm, 0, 240, GUILayout.Width(160)) / 10f) * 10;
                GUILayout.EndHorizontal();
                if (ht != r.drumSoloHalfTimeAboveBpm) { r.drumSoloHalfTimeAboveBpm = ht; changed = true; }
            }

            if (changed)
            {
                InspirationRules.SaveLocal();
                gm?.ApplyInspirationRulesToRun();
                gm?.DevRefreshHandCardFaces();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Restablecer (volver al asset)"))
            {
                InspirationRules.ResetLocal();
                gm?.ApplyInspirationRulesToRun();
                gm?.DevRefreshHandCardFaces();
            }
            GUILayout.Label("Los cambios se aplican al momento y se guardan en esta máquina.", _hint);
        }

        private static bool Toggle(ref bool value, string label)
        {
            bool nv = GUILayout.Toggle(value, " " + label);
            if (nv == value) return false;
            value = nv;
            return true;
        }

        /// <summary>Un entero que puede estar «apagado» (usar otro origen) o fijado.</summary>
        private static bool IntOverride(ref int value, string label, string offLabel,
            int min, int max, int offValue = -1)
        {
            bool on = value != offValue;
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(200));
            bool nOn = GUILayout.Toggle(on, on ? "" : offLabel, GUILayout.Width(on ? 20 : 120));
            int nv = value;
            if (nOn)
            {
                if (!on) nv = Mathf.Clamp(Mathf.Max(min, 1), min, max);
                nv = Mathf.RoundToInt(GUILayout.HorizontalSlider(nv, min, max));
                GUILayout.Label(nv.ToString(), GUILayout.Width(30));
            }
            else nv = offValue;
            GUILayout.EndHorizontal();
            if (nv == value) return false;
            value = nv;
            return true;
        }

        private static void EnsureStyles()
        {
            if (_header == null)
                _header = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 13 };
            if (_hint == null)
                _hint = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic, fontSize = 11, wordWrap = true };
        }
    }
}
#endif
