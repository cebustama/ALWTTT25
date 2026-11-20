using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// [DEV-TUNING-1] Capa de ajustes de gameplay por encima de los assets.
    ///
    /// Los consumidores preguntan <c>DevTuning.Int(clave, valorDelAsset)</c>: si nadie cambió
    /// esa clave, vuelve el valor del asset tal cual. Los cambios viven en memoria y, al
    /// «Guardar», en <c>persistentDataPath/DevTuning.json</c> (solo lo cambiado). Los assets no
    /// se tocan nunca: en el editor, escribir un ScriptableObject en Play lo guardaría en disco.
    ///
    /// Sin ALWTTT_DEV todo devuelve el valor del asset (la build pública no cambia).
    /// El registro visible (nombre, rango, grupo) vive en DevTuningTab (solo dev).
    /// </summary>
    public static class DevTuning
    {
        public const string Tag = "[DEV-TUNING-1]";
        private const string FileName = "DevTuning.json";

        /// <summary>Claves. Añadir un ajuste: una clave aquí, una línea en DevTuningTab.Registry
        /// y una lectura en el consumidor.</summary>
        public static class Keys
        {
            public const string PlaysMode = "jugadas.modo";       // 0 asset · 1 una en total · 2 sin límite
            public const string DrawTurn = "robo.turno";
            public const string DrawOpening = "robo.inicial";   // mano inicial (primer turno del gig)
            public const string DrawLoop = "robo.loop";
            public const string DrawOnPlay = "robo.play";
            public const string HandMax = "mano.max";
            public const string InspStart = "insp.inicio";
            public const string CollapseStress = "colapso.stress";
            public const string ComboPair = "combo.pareja";      // [COMBOS-1]
            public const string ComboLayers = "combo.capas";     // [COMBOS-1]
        }

        public const int PlaysModeAsset = 0;
        public const int PlaysModeOneTotal = 1;
        public const int PlaysModeUnlimited = 2;
        public const int UnlimitedPlays = 99;

        [Serializable] private class Entry { public string key; public string value; }
        [Serializable] private class FileDto { public List<Entry> entries = new List<Entry>(); }

#if ALWTTT_DEV
        private static Dictionary<string, string> _overrides;
        private static bool _dirty;

        private static Dictionary<string, string> Overrides
        {
            get { if (_overrides == null) Load(); return _overrides; }
        }

        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        /// <summary>True si hay algún ajuste distinto del asset (guardado o no).</summary>
        public static bool HasOverrides => Overrides.Count > 0;

        /// <summary>True si hay cambios en memoria sin guardar.</summary>
        public static bool IsDirty => _dirty;

        /// <summary>Claves cambiadas, para la telemetría ("" = de fábrica).</summary>
        public static string Describe()
        {
            if (Overrides.Count == 0) return "";
            var parts = new List<string>();
            foreach (var kv in Overrides) parts.Add($"{kv.Key}={kv.Value}");
            parts.Sort(StringComparer.Ordinal);
            return string.Join(";", parts);
        }

        public static bool IsOverridden(string key) => Overrides.ContainsKey(key);

        public static int Int(string key, int assetValue) =>
            Overrides.TryGetValue(key, out var s) && int.TryParse(s, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : assetValue;

        public static float Float(string key, float assetValue) =>
            Overrides.TryGetValue(key, out var s) && float.TryParse(s, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : assetValue;

        public static bool Bool(string key, bool assetValue) =>
            Overrides.TryGetValue(key, out var s) && bool.TryParse(s, out var v) ? v : assetValue;

        /// <summary>Fija una clave. Si el valor es el del asset, la quita (solo se guarda lo cambiado).</summary>
        public static void Set(string key, string value, string assetValue, string label)
        {
            string before = Overrides.TryGetValue(key, out var b) ? b : assetValue;
            if (before == value) return;

            if (value == assetValue) Overrides.Remove(key);
            else Overrides[key] = value;
            _dirty = true;

            Debug.Log($"{Tag} {key} ({label}): {before} → {value}" +
                      (value == assetValue ? " (valor del asset)" : ""));
        }

        public static void Save()
        {
            try
            {
                var dto = new FileDto();
                foreach (var kv in Overrides) dto.entries.Add(new Entry { key = kv.Key, value = kv.Value });
                if (dto.entries.Count == 0)
                {
                    if (File.Exists(FilePath)) File.Delete(FilePath);
                }
                else File.WriteAllText(FilePath, JsonUtility.ToJson(dto, true));
                _dirty = false;
                Debug.Log($"{Tag} Guardado en {FilePath}: {(dto.entries.Count == 0 ? "de fábrica (archivo borrado)" : Describe())}");
            }
            catch (Exception ex) { Debug.LogWarning($"{Tag} No se pudo guardar: {ex.Message}"); }
        }

        /// <summary>«Valores por defecto»: borra el archivo y vuelve a los assets.</summary>
        public static void ResetAll()
        {
            string was = Describe();
            Overrides.Clear();
            try { if (File.Exists(FilePath)) File.Delete(FilePath); }
            catch (Exception ex) { Debug.LogWarning($"{Tag} No se pudo borrar {FilePath}: {ex.Message}"); }
            _dirty = false;
            Debug.Log($"{Tag} Valores por defecto (assets). Antes: {(string.IsNullOrEmpty(was) ? "de fábrica" : was)}");
        }

        /// <summary>Descarta lo no guardado y relee el archivo.</summary>
        public static void Load()
        {
            _overrides = new Dictionary<string, string>();
            _dirty = false;
            try
            {
                if (!File.Exists(FilePath))
                {
                    Debug.Log($"{Tag} Sin ajustes guardados: valores de los assets.");
                    return;
                }
                var dto = JsonUtility.FromJson<FileDto>(File.ReadAllText(FilePath));
                if (dto?.entries != null)
                    foreach (var e in dto.entries)
                        if (!string.IsNullOrEmpty(e?.key)) _overrides[e.key] = e.value ?? "";
                Debug.Log($"{Tag} Ajustes cargados de {FilePath}: {Describe()}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{Tag} {FilePath} ilegible, uso los assets. {ex.Message}");
                _overrides.Clear();
            }
        }
#else
        public static bool HasOverrides => false;
        public static string Describe() => "";
        public static int Int(string key, int assetValue) => assetValue;
        public static float Float(string key, float assetValue) => assetValue;
        public static bool Bool(string key, bool assetValue) => assetValue;
#endif

        // ─── Valores efectivos compartidos ───────────────────────────────

        /// <summary>Máximo de la mano: GameplayData.MaxCardsOnHand o el ajuste.</summary>
        public static int HandMax(int assetValue) => Mathf.Max(1, Int(Keys.HandMax, assetValue));
    }
}
