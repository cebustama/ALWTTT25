#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using UnityEngine;

namespace ALWTTT.Status
{
    /// <summary>
    /// Gameplay authoring layer (Step 2):
    /// A concrete ALWTTT status definition that references the CSO primitive via CharacterStatusId
    /// and adds gameplay-facing tuning parameters (stacking, decay, timing, etc.).
    ///
    /// This asset should NOT duplicate ontology fields (Category / Abstract Function).
    /// Instead, it cross-references the CharacterStatusPrimitiveDatabaseSO via EffectId.
    /// </summary>
    [CreateAssetMenu(
        fileName = "StatusEffect",
        menuName = "ALWTTT/Status/Status Effect",
        order = 20)]
    public sealed class StatusEffectSO : ScriptableObject
    {
        [Header("Identity (CSO)")]
        [SerializeField] private CharacterStatusId effectId;

        [Tooltip("Designer-facing name shown in UI/tooltips. Can later become a localization key.")]
        [SerializeField] private string displayName;

        [Tooltip("Optional: Assign the CSO registry asset used to validate EffectId and browse ontology metadata.")]
        [SerializeField] private CharacterStatusPrimitiveDatabaseSO primitiveDatabase;

        [Header("Behavior (MVP)")]
        [SerializeField] private StackMode stackMode = StackMode.Additive;
        [Min(1)]
        [SerializeField] private int maxStacks = 999;

        [SerializeField] private DecayMode decayMode = DecayMode.None;
        [Min(0)]
        [SerializeField] private int durationTurns = 0; // Used by some decay modes (0 = ignore)

        [SerializeField] private TickTiming tickTiming = TickTiming.None;
        [SerializeField] private ValueType valueType = ValueType.Flat;

        [Header("Semantics")]
        [Tooltip("Purely semantic: helps UI coloring, filtering, and design intent.")]
        [SerializeField] private bool isBuff = true;

        // ─────────────────────────────────────────────────────────────────────────────
        // Public API (read-only)
        // ─────────────────────────────────────────────────────────────────────────────
        public CharacterStatusId EffectId => effectId;
        public string DisplayName => displayName;

        public StackMode Stacking => stackMode;
        public int MaxStacks => maxStacks;

        public DecayMode Decay => decayMode;
        public int DurationTurns => durationTurns;

        public TickTiming Tick => tickTiming;
        public ValueType Value => valueType;

        public bool IsBuff => isBuff;

        /// <summary>
        /// Convenience: Try get the ontology entry for this status (Category, Abstract Function, references).
        /// </summary>
        public bool TryGetOntologyEntry(out CharacterStatusPrimitiveEntry entry)
        {
            entry = null;
            if (primitiveDatabase == null) return false;
            return primitiveDatabase.TryGet(effectId, out entry);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Keep displayName sane
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = effectId.ToString();

            if (maxStacks < 1) maxStacks = 1;

            // Auto-find the primitive database if not assigned (Editor only).
            if (primitiveDatabase == null)
                primitiveDatabase = TryFindPrimitiveDatabaseAsset();

            // Validate effectId exists in primitive DB (if DB exists).
            if (primitiveDatabase != null && !primitiveDatabase.Contains(effectId))
            {
                Debug.LogError(
                    $"[StatusEffectSO] EffectId '{effectId}' is not present in the CSO Primitive Database '{primitiveDatabase.name}'. " +
                    $"This StatusEffectSO is invalid against the ontology.",
                    this);
            }

            // Small consistency checks:
            if (decayMode == DecayMode.DurationTurns && durationTurns <= 0)
            {
                Debug.LogWarning(
                    $"[StatusEffectSO] '{name}' uses DecayMode.DurationTurns but durationTurns is {durationTurns}. " +
                    $"Set durationTurns > 0 (or change decay mode).",
                    this);
            }
        }

        private static CharacterStatusPrimitiveDatabaseSO TryFindPrimitiveDatabaseAsset()
        {
            // If multiple exist, we pick the first found. You should generally keep exactly one.
            var guids = AssetDatabase.FindAssets("t:CharacterStatusPrimitiveDatabaseSO");
            if (guids == null || guids.Length == 0) return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<CharacterStatusPrimitiveDatabaseSO>(path);
        }
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Supporting enums (MVP). Keep these stable once serialized assets exist.
    // ─────────────────────────────────────────────────────────────────────────────

    public enum StackMode
    {
        /// <summary>Add stacks (e.g., Strength, Poison).</summary>
        Additive = 0,

        /// <summary>Replace current stacks with the new value.</summary>
        Replace = 1,

        /// <summary>Refresh duration but keep stacks (typical “reapply” behavior).</summary>
        RefreshDuration = 2,

        /// <summary>Clamp stacks to MaxStacks.</summary>
        AdditiveClamped = 3,
    }

    public enum DecayMode
    {
        /// <summary>No decay; removed by explicit clear logic.</summary>
        None = 0,

        /// <summary>Decrease stacks by 1 at a timing boundary (turn/loop/etc.).</summary>
        LinearStacks = 1,

        /// <summary>Expires after N turns (durationTurns).</summary>
        DurationTurns = 2,

        /// <summary>Expires after being triggered once (e.g., “next hit” negate).</summary>
        ConsumeOnTrigger = 3,
    }

    public enum TickTiming
    {
        None = 0,
        StartOfTurn = 1,
        EndOfTurn = 2,
        StartOfLoop = 3,
        EndOfLoop = 4,
        OnAction = 5,
        OnHit = 6,
        OnTakeDamage = 7,
    }

    public enum ValueType
    {
        Flat = 0,
        Percent = 1,
    }
}
