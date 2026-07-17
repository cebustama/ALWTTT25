// Place at: Assets/Scripts/Tutorial/TutorialHighlightTarget.cs
using ALWTTT.UI;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [TUT-R2c / RT6 fix] Inverted highlight registration. The TUT-R2 model
    /// (serialized HighlightBinding list on TutorialController) cannot reference
    /// scene objects: the controller lives on a prefab in ALWTTTCore loaded at
    /// runtime, so gig-scene objects (Play button, inspiration counter, audience
    /// area) are unassignable from its inspector. This component flips the
    /// direction: it sits ON the scene object, carries the highlight key, and
    /// self-registers in a static registry that the controller consults at
    /// show time. Scene load/unload handles lifetime automatically.
    ///
    /// The controller resolves in this order:
    ///   1. Registry (this component) — mask target + optional pulse.
    ///   2. Serialized HighlightBinding list — legacy fallback for anything
    ///      that DOES live with the controller prefab.
    ///   3. No highlight (existing degradation).
    ///
    /// Duplicate keys: last-enabled wins (logged). Known keys live in
    /// <see cref="TutorialHighlightKeys"/>; OnValidate warns on unknown keys
    /// (a dropdown drawer can come with the TUT-R4 tooling window).
    /// </summary>
    public class TutorialHighlightTarget : MonoBehaviour
    {
        [Tooltip("Highlight key este objeto responde (ver TutorialHighlightKeys). " +
                 "Debe coincidir con el HighlightKey del TutorialDialogSO.")]
        [SerializeField] private string key;

        [Tooltip("RectTransform que el spotlight debe enmascarar. Vacío = el " +
                 "RectTransform de este mismo GameObject (si lo tiene).")]
        [SerializeField] private RectTransform maskTarget;

        [Tooltip("Opcional: objeto world-space a resaltar (p.ej. un personaje). Si se asigna, " +
         "el spotlight usa conversión world→screen con la cámara en vez de MaskTarget.")]
        [SerializeField] private Transform worldTarget;
        [Tooltip("Opcional: Renderer del que tomar bounds para auto-tamaño world-space. " +
                 "Vacío = GetComponentInChildren<Renderer>() en worldTarget.")]
        [SerializeField] private Renderer worldRenderer;
        [Tooltip("Opcional: cámara para la proyección world→screen. Vacío = Camera.main.")]
        [SerializeField] private Camera worldCamera;

        public Transform WorldTarget => worldTarget;
        public Camera WorldCamera => worldCamera;
        public bool IsWorldSpace => worldTarget != null;
        public bool TryGetWorldBounds(out Bounds bounds)
        {
            var r = worldRenderer != null ? worldRenderer
                  : (worldTarget != null ? worldTarget.GetComponentInChildren<Renderer>() : null);
            if (r != null) { bounds = r.bounds; return true; }
            if (worldTarget is RectTransform rt)          // elemento de canvas world-space
            {
                var c = new Vector3[4]; rt.GetWorldCorners(c);
                var b = new Bounds(c[0], Vector3.zero);
                for (int i = 1; i < 4; i++) b.Encapsulate(c[i]);
                bounds = b; return true;
            }
            bounds = default; return false;
        }

        [Tooltip("Opcional: UIPulseAnimator a pulsar rítmicamente mientras el " +
                 "diálogo de esta key esté en pantalla.")]
        [SerializeField] private UIPulseAnimator pulse;

        public string Key => key;
        public RectTransform MaskTarget =>
            maskTarget != null ? maskTarget : transform as RectTransform;
        public UIPulseAnimator Pulse => pulse;

        /// <summary>
        /// [CARD-UX-1 / D1=C] Runtime init for spawn-hook attached instances.
        /// AddComponent fires OnEnable BEFORE the key is set (Register no-ops on
        /// a blank key), so this assigns the fields and registers explicitly.
        /// Only TutorialHighlightSpawnHook calls this.
        /// </summary>
        public void InitRuntime(
            string highlightKey, RectTransform mask,
            Transform world, Renderer renderer, Camera camera)
        {
            key = highlightKey;
            maskTarget = mask;
            worldTarget = world;
            worldRenderer = renderer;
            worldCamera = camera;
            TutorialHighlightRegistry.Register(this);
        }

        private void OnEnable() => TutorialHighlightRegistry.Register(this);
        private void OnDisable() => TutorialHighlightRegistry.Unregister(this);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!string.IsNullOrWhiteSpace(key) &&
                System.Array.IndexOf(TutorialHighlightKeys.Known, key) < 0)
            {
                Debug.LogWarning(
                    $"[TutorialHighlightTarget] '{name}': key '{key}' no está en " +
                    "TutorialHighlightKeys.Known. ¿Typo? (Se registrará igual.)", this);
            }
        }
#endif
    }

    /// <summary>Runtime lookup used by TutorialController (spotlight + pulse).</summary>
    public static class TutorialHighlightRegistry
    {
        private static readonly Dictionary<string, TutorialHighlightTarget> _byKey = new();

        public static void Register(TutorialHighlightTarget t)
        {
            if (t == null || string.IsNullOrWhiteSpace(t.Key)) return;
            if (_byKey.TryGetValue(t.Key, out var prev) && prev != null && prev != t)
                Debug.Log($"[TutorialHighlightRegistry] key '{t.Key}' re-registrada " +
                          $"por '{t.name}' (antes '{prev.name}') — last-enabled wins.");
            _byKey[t.Key] = t;
        }

        public static void Unregister(TutorialHighlightTarget t)
        {
            if (t == null || string.IsNullOrWhiteSpace(t.Key)) return;
            if (_byKey.TryGetValue(t.Key, out var cur) && cur == t)
                _byKey.Remove(t.Key);
        }

        public static bool TryGet(string key, out TutorialHighlightTarget target)
        {
            target = null;
            return !string.IsNullOrWhiteSpace(key) &&
                   _byKey.TryGetValue(key, out target) && target != null;
        }
    }

    /// <summary>
    /// [TUT-R2c] Keys propuestas en TUT-R1 §3/§4 (registro formal en TUT-R3).
    /// Fuente única para el warning de OnValidate y el futuro dropdown (TUT-R4).
    /// </summary>
    public static class TutorialHighlightKeys
    {
        public static readonly string[] Known =
        {
            "hand",
            "card_default_mode",
            "song_panel_tracks",
            "play_button",
            "loops_bar",
            "inspiration_counter",
            "card_psychic_waves",
            "audience_vibe_bars",
            "audience_area",
            "status_icon_musician",
            "status_icon_audience",
            "status_icon_blocked",
            "musician_stress_bar",
            "status_icon_composure",
        };
    }
}