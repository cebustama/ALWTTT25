using System;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Per-category configuration for composition-card floating text spawned
    /// by <c>SongCompositionUI.SpawnCompositionFx</c>.
    ///
    /// Each <see cref="FxEntry"/> bundles a per-category toggle and color.
    /// The master <see cref="Enabled"/> kills the whole system in one click.
    ///
    /// [TEXT-VIBE-1 / D-SP1-6=B] The per-entry <c>label</c> field is GONE. It was
    /// population 8 of the text map (F-T6-9): player text with one language slot,
    /// no editor tab, no CSV and no coverage check. The text now lives in the
    /// concept glossary under <c>ui_fx_*</c> template ids, one per category, and
    /// the id is chosen by <c>SongCompositionUI.FxTextIdOf</c> from WHICH entry the
    /// classifier picked -- the category is the key, so there is nothing to author
    /// here. The field was deleted rather than migrated for the SONG-PANEL-1 reason:
    /// a serialized text field is the surface, and a migrated literal would have
    /// left an English default one inspector edit away from returning.
    /// "Empty label = silent" no longer exists; <c>enabled</c> is the only gate.
    ///
    /// Authority: B2 / #3 (operational) for toggles and colors; text ids in
    /// systems/SSoT_Game_Text.md section 1 (population 8). Not an SSoT-governed
    /// contract otherwise -- purely presentation tuning. Path convention:
    /// alongside <c>GigPresentation.asset</c>.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CompositionFxConfig",
        menuName = "ALWTTT/Gig/CompositionFxConfig",
        order = 13)]
    public sealed class CompositionFxConfigSO : ScriptableObject
    {
        /// <summary>
        /// Per-category entry: toggle and color. Designer-tuneable in the inspector.
        /// The on-screen word is NOT here (see class summary).
        /// </summary>
        [Serializable]
        public class FxEntry
        {
            [Tooltip("Disable to suppress this category without removing the asset reference.")]
            public bool enabled = true;

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
            color = new Color(1.00f, 0.80f, 0.30f)
        };

        [SerializeField]
        private FxEntry meter = new FxEntry
        {
            enabled = true,
            color = new Color(1.00f, 0.60f, 0.30f)
        };

        [SerializeField, Tooltip("First-time tonality set (before was uninitialized). " +
            "Design hook: no code path selects this entry today (D-B2.5-3=B), so it has no text id.")]
        private FxEntry tonality = new FxEntry
        {
            enabled = true,
            color = new Color(0.90f, 0.50f, 1.00f)
        };

        [SerializeField, Tooltip("Mid-song tonality/key change (modulation -- already had a key).")]
        private FxEntry modulation = new FxEntry
        {
            enabled = true,
            color = new Color(0.70f, 0.50f, 1.00f)
        };

        [SerializeField]
        private FxEntry instrument = new FxEntry
        {
            enabled = true,
            color = new Color(0.50f, 0.90f, 1.00f)
        };

        [SerializeField, Tooltip("Catch-all for single modifier effects not matched above. " +
            "Design hook: no code path selects this entry today, so it has no text id.")]
        private FxEntry modifier = new FxEntry
        {
            enabled = true,
            color = new Color(1.00f, 0.95f, 0.25f)
        };

        [Header("Aggregate")]
        [SerializeField, Tooltip("Fires when 2+ real diffs are detected in a single card apply.")]
        private FxEntry majorChange = new FxEntry
        {
            enabled = true,
            color = new Color(1.00f, 0.40f, 0.40f)
        };

        [SerializeField, Tooltip("Fallback when a replaced track has a role outside the four " +
            "authored below. Should rarely fire.")]
        private FxEntry fallback = new FxEntry
        {
            enabled = false,
            color = new Color(1.00f, 0.95f, 0.25f)
        };

        // ---- Track roles (per-role color on replaced track) ----

        [Header("Track roles")]
        [SerializeField]
        private FxEntry rhythm = new FxEntry
        {
            enabled = true,
            color = new Color(1.00f, 0.70f, 0.30f)
        };

        [SerializeField]
        private FxEntry backing = new FxEntry
        {
            enabled = true,
            color = new Color(0.80f, 1.00f, 0.50f)
        };

        [SerializeField]
        private FxEntry melody = new FxEntry
        {
            enabled = true,
            color = new Color(0.50f, 1.00f, 1.00f)
        };

        [SerializeField]
        private FxEntry harmony = new FxEntry
        {
            enabled = true,
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