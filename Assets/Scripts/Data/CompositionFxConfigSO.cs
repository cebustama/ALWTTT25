using System;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Per-category configuration for composition-card floating text spawned
    /// by <c>SongCompositionUI.SpawnCompositionFx</c>.
    ///
    /// Each <see cref="FxEntry"/> bundles a per-category toggle, label, and color.
    /// The master <see cref="Enabled"/> kills the whole system in one click.
    ///
    /// Authority: B2 / #3 (operational). Not an SSoT-governed contract — purely
    /// presentation tuning. Path convention: alongside <c>GigPresentation.asset</c>.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CompositionFxConfig",
        menuName = "ALWTTT/Gig/CompositionFxConfig",
        order = 13)]
    public sealed class CompositionFxConfigSO : ScriptableObject
    {
        /// <summary>
        /// Per-category entry: toggle, label, and color. Designer-tuneable in the inspector.
        /// </summary>
        [Serializable]
        public class FxEntry
        {
            [Tooltip("Disable to suppress this category without removing the asset reference.")]
            public bool enabled = true;

            [Tooltip("Text shown on screen when this category fires. Empty = silent.")]
            public string label = "CHANGE";

            [Tooltip("Floating-text color when this category fires.")]
            public Color color = Color.yellow;
        }

        [Header("Master")]
        [SerializeField, Tooltip("Master toggle. Off = no composition floating text at all.")]
        private bool enabled = true;

        // ---- Modifier categories (single-effect) ----

        [Header("Modifier categories")]
        [SerializeField]
        private FxEntry tempo = new FxEntry
        {
            enabled = true,
            label = "TEMPO!",
            color = new Color(1.00f, 0.80f, 0.30f)
        };

        [SerializeField]
        private FxEntry meter = new FxEntry
        {
            enabled = true,
            label = "METER!",
            color = new Color(1.00f, 0.60f, 0.30f)
        };

        [SerializeField, Tooltip("First-time tonality set (before was uninitialized).")]
        private FxEntry tonality = new FxEntry
        {
            enabled = true,
            label = "TONALITY!",
            color = new Color(0.90f, 0.50f, 1.00f)
        };

        [SerializeField, Tooltip("Mid-song tonality/key change (modulation — already had a key).")]
        private FxEntry modulation = new FxEntry
        {
            enabled = true,
            label = "KEY!",
            color = new Color(0.70f, 0.50f, 1.00f)
        };

        [SerializeField]
        private FxEntry instrument = new FxEntry
        {
            enabled = true,
            label = "INSTRUMENT!",
            color = new Color(0.50f, 0.90f, 1.00f)
        };

        [SerializeField, Tooltip("Catch-all for single modifier effects not matched above.")]
        private FxEntry modifier = new FxEntry
        {
            enabled = true,
            label = "MODIFIER!",
            color = new Color(1.00f, 0.95f, 0.25f)
        };

        [Header("Aggregate")]
        [SerializeField, Tooltip("Fires when 2+ real diffs are detected in a single card apply.")]
        private FxEntry majorChange = new FxEntry
        {
            enabled = true,
            label = "MAJOR CHANGE",
            color = new Color(1.00f, 0.40f, 0.40f)
        };

        [SerializeField, Tooltip("Fallback when the classifier matches no specific category " +
            "but a change was detected anyway. Should rarely fire after change detection lands.")]
        private FxEntry fallback = new FxEntry
        {
            enabled = false,
            label = "CHANGE",
            color = new Color(1.00f, 0.95f, 0.25f)
        };

        // ---- Track roles (per-role label/color on new/replaced track) ----

        [Header("Track roles")]
        [SerializeField]
        private FxEntry rhythm = new FxEntry
        {
            enabled = true,
            label = "RHYTHM!",
            color = new Color(1.00f, 0.70f, 0.30f)
        };

        [SerializeField]
        private FxEntry backing = new FxEntry
        {
            enabled = true,
            label = "BACKING!",
            color = new Color(0.80f, 1.00f, 0.50f)
        };

        [SerializeField]
        private FxEntry melody = new FxEntry
        {
            enabled = true,
            label = "MELODY!",
            color = new Color(0.50f, 1.00f, 1.00f)
        };

        [SerializeField]
        private FxEntry harmony = new FxEntry
        {
            enabled = true,
            label = "HARMONY!",
            color = new Color(1.00f, 0.60f, 0.90f)
        };

        // ---- Public accessors ----

        public bool Enabled => enabled;

        public FxEntry Tempo => tempo;
        public FxEntry Meter => meter;
        public FxEntry Tonality => tonality;
        public FxEntry Modulation => modulation;
        public FxEntry Instrument => instrument;
        public FxEntry Modifier => modifier;
        public FxEntry MajorChange => majorChange;
        public FxEntry Fallback => fallback;

        public FxEntry Rhythm => rhythm;
        public FxEntry Backing => backing;
        public FxEntry Melody => melody;
        public FxEntry Harmony => harmony;
    }
}