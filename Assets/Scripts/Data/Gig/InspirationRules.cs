using System;
using ALWTTT.Cards;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// [ECON-2] Las reglas de la Inspiración y de los Finisher, todas ajustables.
    ///
    /// Tres capas, de menos a más prioridad:
    ///   1. Los valores de este código (por si el asset no tiene el bloque).
    ///   2. El bloque <c>inspirationRules</c> de GigFlowSettings (lo que se commitea).
    ///   3. Los ajustes locales de Dev Mode (F12 ▸ Inspiración), guardados en
    ///      PlayerPrefs de la máquina que juega. Así Matías puede probar en su build
    ///      sin tocar assets, y «Restablecer» vuelve al asset.
    ///
    /// Los campos con -1 significan «lo que ya diga el asset de siempre» (la carta,
    /// Gameplay Settings, GigFlowSettings): las reglas solo pisan lo que se pide.
    /// </summary>
    [Serializable]
    public sealed class InspirationRules
    {
        public const string Tag = "[ECON-2]";
        private const string PrefsKey = "ALWTTT.ECON2.InspirationRules";

        [Header("Composición")]
        [Tooltip("Las cartas de composición dan su ★ (inspirationGenerated) una vez al jugarlas. " +
                 "Apagado = la economía de antes: no dan nada.")]
        public bool compositionGrantsInspiration = true;

        [Tooltip("-1 = la ★ de cada carta. 0 o más = todas las composiciones dan esto.")]
        public int compositionInspirationOverride = -1;

        [Header("Contador")]
        [Tooltip("-1 = maxInspiration de Gameplay Settings. 0 o más = este máximo.")]
        public int maxInspirationOverride = -1;

        [Tooltip("-1 = la Inspiración básica por loop del asset. 0 o más = esta.")]
        public int inspirationPerLoopOverride = -1;

        [Tooltip("-1 = el asset. 0 = la Inspiración se pierde entre canciones. 1 = se guarda.")]
        public int keepBetweenSongsOverride = -1;

        [Header("Otros costes")]
        [Tooltip("Ignorar el coste de Voltaje (u otro recurso) de las cartas hasta que la cara " +
                 "nueva lo comunique. Las cartas conservan su coste; solo no se cobra.")]
        public bool ignoreResourceCosts = true;

        [Header("Finisher")]
        [Tooltip("Encendido = el Finisher solo se juega mientras suena el último loop. " +
                 "Apagado = en cualquier loop que esté sonando.")]
        public bool finisherOnlyFinalLoop = true;

        [Tooltip("-1 = el coste de cada carta. 0 o más = todos los Finisher cuestan esto.")]
        public int finisherCostOverride = -1;

        [Tooltip("Cuántos Finisher se pueden jugar por canción. 0 = sin límite.")]
        public int maxFinishersPerSong = 0;

        [Tooltip("Cada Finisher pone a su músico a tocar un solo en el loop final.")]
        public bool finisherSolo = true;

        [Tooltip("Volumen de los solistas a partir del segundo (el primero va a 1).")]
        [Range(0f, 1f)] public float extraSoloistVolume01 = 0.8f;

        [Tooltip("Cuando un Finisher es de un músico sin solo melódico (C2, batería), sus " +
                 "pistas toman el foco y el resto de la banda baja a este volumen: un «break».")]
        [Range(0f, 1f)] public float breakDuck01 = 0.35f;

        [Tooltip("[BATERIA-SOLO-1] El Finisher de un músico sin solo melódico (C2) re-renderiza " +
                 "su pista de batería como un solo para ese loop. Apagado = solo sube el volumen.")]
        public bool drumSolo = true;

        [Tooltip("[BATERIA-SOLO-1] Forma del solo: 0 groove + fills, 1 fills encadenados, 2 trading.")]
        [Range(0, 2)] public int drumSoloShape = 1;

        [Tooltip("[BATERIA-SOLO-1] Carácter: -1 = por semilla; 0 tresillos de toms, 1 cascada, " +
                 "2 rudimentos, 3 toms rituales, 4 máquina.")]
        [Range(-1, 4)] public int drumSoloVariant = -1;

        [Tooltip("Encendido = los efectos del Finisher (Vibe, estados, onda) estallan al " +
                 "empezar el loop final. Apagado = al jugar la carta.")]
        public bool finisherEffectsAtFinalLoopStart = true;

        // ─── Estado estático ─────────────────────────────────────────────

        private static InspirationRules _assetDefaults;
        private static InspirationRules _active;

        /// <summary>Las reglas que rigen ahora (asset + ajustes locales).</summary>
        public static InspirationRules Active
        {
            get
            {
                if (_active == null) Reload(null);
                return _active;
            }
        }

        /// <summary>True cuando hay ajustes locales de Dev Mode encima del asset.</summary>
        public static bool HasLocalOverrides => PlayerPrefsHasKey();

        /// <summary>
        /// Carga las reglas: copia del asset y, encima, los ajustes locales.
        /// GigManager lo llama al empezar cada gig con el bloque de GigFlowSettings.
        /// </summary>
        public static void Reload(InspirationRules assetDefaults)
        {
            if (assetDefaults != null) _assetDefaults = assetDefaults;
            _active = Clone(_assetDefaults ?? new InspirationRules());

            string json = null;
            try { if (PlayerPrefs.HasKey(PrefsKey)) json = PlayerPrefs.GetString(PrefsKey); }
            catch (Exception) { /* PlayerPrefs no disponible: se queda el asset */ }

            if (!string.IsNullOrEmpty(json))
            {
                try { JsonUtility.FromJsonOverwrite(json, _active); }
                catch (Exception ex)
                {
                    Debug.LogWarning($"{Tag} Ajustes locales ilegibles, uso el asset. {ex.Message}");
                    _active = Clone(_assetDefaults ?? new InspirationRules());
                }
            }

            Debug.Log($"{Tag} Reglas cargadas ({(string.IsNullOrEmpty(json) ? "asset" : "asset + ajustes locales")}): {_active.Describe()}");
        }

        /// <summary>Guarda las reglas activas como ajustes locales de esta máquina.</summary>
        public static void SaveLocal()
        {
            try
            {
                PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(Active));
                PlayerPrefs.Save();
                Debug.Log($"{Tag} Ajustes locales guardados: {Active.Describe()}");
            }
            catch (Exception ex) { Debug.LogWarning($"{Tag} No se pudieron guardar: {ex.Message}"); }
        }

        /// <summary>Borra los ajustes locales y vuelve a los del asset.</summary>
        public static void ResetLocal()
        {
            try { PlayerPrefs.DeleteKey(PrefsKey); PlayerPrefs.Save(); }
            catch (Exception) { }
            Reload(null);
        }

        // ─── Valores efectivos ───────────────────────────────────────────

        /// <summary>La ★ que da una composición al jugarla, con las reglas de ahora.</summary>
        public static int EffectiveGenerated(CardDefinition def)
        {
            if (def == null || !def.IsComposition || def.IsFinisher) return 0;
            var r = Active;
            if (!r.compositionGrantsInspiration) return 0;
            return r.compositionInspirationOverride >= 0
                ? r.compositionInspirationOverride
                : Mathf.Max(0, def.InspirationGenerated);
        }

        /// <summary>El coste en ★ de una carta, con las reglas de ahora.</summary>
        public static int EffectiveCost(CardDefinition def)
        {
            if (def == null) return 0;
            if (def.IsFinisher && Active.finisherCostOverride >= 0)
                return Active.finisherCostOverride;
            return Mathf.Max(0, def.InspirationCost);
        }

        public string Describe() =>
            $"compGen={(compositionGrantsInspiration ? (compositionInspirationOverride >= 0 ? compositionInspirationOverride.ToString() : "carta") : "off")} " +
            $"max={(maxInspirationOverride >= 0 ? maxInspirationOverride.ToString() : "asset")} " +
            $"porLoop={(inspirationPerLoopOverride >= 0 ? inspirationPerLoopOverride.ToString() : "asset")} " +
            $"guardar={(keepBetweenSongsOverride < 0 ? "asset" : (keepBetweenSongsOverride > 0 ? "sí" : "no"))} " +
            $"finisherSoloÚltimo={finisherOnlyFinalLoop} coste={(finisherCostOverride >= 0 ? finisherCostOverride.ToString() : "carta")} " +
            $"máxFinisher={(maxFinishersPerSong > 0 ? maxFinishersPerSong.ToString() : "∞")} solo={finisherSolo} " +
            $"volExtra={extraSoloistVolume01:0.##} efectosAlLoopFinal={finisherEffectsAtFinalLoopStart} " +
            $"ignorarVoltaje={ignoreResourceCosts} " +
            $"soloBatería={(drumSolo ? $"forma{drumSoloShape}/var{(drumSoloVariant < 0 ? "semilla" : drumSoloVariant.ToString())}" : "off")}";

        private static InspirationRules Clone(InspirationRules src) =>
            JsonUtility.FromJson<InspirationRules>(JsonUtility.ToJson(src));

        private static bool PlayerPrefsHasKey()
        {
            try { return PlayerPrefs.HasKey(PrefsKey); } catch (Exception) { return false; }
        }
    }
}
