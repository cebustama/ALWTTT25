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
    /// NEW:
    /// - statusKey: human-readable stable identifier used by JSON/tooling to reference a specific variant.
    /// - isDefaultVariant: allows selecting a "default" variant when resolving by primitive id.
    /// - iconSprite (M1.2 refactor): presentation sprite, read directly by CharacterCanvas via
    ///   StatusEffectContainer. Replaces the former StatusIconsData lookup table.
    /// - Auto-rename (M1.2 polish): asset file renames itself to
    ///   "StatusEffect_{DisplayName}_{EffectId}" whenever DisplayName or EffectId changes.
    /// </summary>
    [CreateAssetMenu(
        fileName = "StatusEffect",
        menuName = "ALWTTT/Status/Status Effect",
        order = 20)]
    public sealed class StatusEffectSO : ScriptableObject
    {
        [Header("Identity (ALWTTT)")]
        [Tooltip("Stable human-readable key for this specific status variant (e.g. 'strength', 'super_strength'). Used for JSON/tooling.")]
        [SerializeField] private string statusKey;

        [Tooltip("If multiple variants share the same EffectId, this marks the default one when resolving by primitive id.")]
        [SerializeField] private bool isDefaultVariant = false;

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
        [SerializeField] private int durationTurns = 0;

        [SerializeField] private TickTiming tickTiming = TickTiming.None;
        [SerializeField] private ValueType valueType = ValueType.Flat;

        [Header("Semantics")]
        [Tooltip("Purely semantic: helps UI coloring, filtering, and design intent.")]
        [SerializeField] private bool isBuff = true;

        [Header("Presentation")]
        [Tooltip("Icon sprite displayed on character canvases when this status is active. " +
                 "Read directly by CharacterCanvas via the StatusEffectContainer.")]
        [SerializeField] private Sprite iconSprite;

        [Tooltip("Short player-facing description shown in status tooltips. " +
         "1–2 sentences, rich-text friendly. Example: 'Adds flat bonus to outgoing Vibe gains.'")]
        [TextArea(2, 4)]
        [SerializeField] private string description;

        public string Description => description;

        // ─────────────────────────────────────────────────────────────────────────────
        // Public API (read-only)
        // ─────────────────────────────────────────────────────────────────────────────
        public string StatusKey => statusKey;
        public bool IsDefaultVariant => isDefaultVariant;

        public CharacterStatusId EffectId => effectId;
        public string DisplayName => displayName;

        public StackMode Stacking => stackMode;
        public int MaxStacks => maxStacks;

        public DecayMode Decay => decayMode;
        public int DurationTurns => durationTurns;

        public TickTiming Tick => tickTiming;
        public ValueType Value => valueType;

        public bool IsBuff => isBuff;

        public Sprite IconSprite => iconSprite;

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

            // Ensure stable key exists (generate only if empty).
            if (string.IsNullOrWhiteSpace(statusKey))
                statusKey = GenerateSuggestedKey(displayName);

            // Basic hygiene: normalize key to slug-ish format.
            statusKey = NormalizeKey(statusKey);

            if (maxStacks < 1) maxStacks = 1;

            // Auto-find the primitive database if not assigned.
            if (primitiveDatabase == null)
                primitiveDatabase = TryFindPrimitiveDatabaseAsset();

            // Validate effectId exists in primitive DB.
            if (primitiveDatabase != null && !primitiveDatabase.Contains(effectId))
            {
                Debug.LogError(
                    $"[StatusEffectSO] EffectId '{effectId}' is not present in the CSO Primitive Database '{primitiveDatabase.name}'. " +
                    $"This StatusEffectSO is invalid against the ontology.",
                    this);
            }

            // Duration sanity check.
            if (decayMode == DecayMode.DurationTurns && durationTurns <= 0)
            {
                Debug.LogWarning(
                    $"[StatusEffectSO] '{name}' uses DecayMode.DurationTurns but durationTurns is {durationTurns}. " +
                    $"Set durationTurns > 0 (or change decay mode).",
                    this);
            }

            // Auto-rename asset to "StatusEffect_{DisplayName}_{EffectId}".
            // Deferred via delayCall because AssetDatabase ops are not legal inside OnValidate.
            TryScheduleAutoRename();
        }

        private void TryScheduleAutoRename()
        {
            // Skip during import worker runs — no filesystem ops during reimport.
            if (AssetDatabase.IsAssetImportWorkerProcess()) return;

            // Capture a reference for the deferred call (OnValidate's 'this' is fine).
            var target = this;
            EditorApplication.delayCall += () =>
            {
                if (target == null) return;
                target.DoAutoRename();
            };
        }

        private void DoAutoRename()
        {
            string path = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path)) return;

            // Only proceed if this asset is on disk (not a runtime-created SO).
            string currentFileName = System.IO.Path.GetFileNameWithoutExtension(path);
            string desiredName = BuildDesiredFileName();

            if (string.Equals(currentFileName, desiredName, StringComparison.Ordinal))
                return;

            // Check for a collision at the target name (another SO with the same filename).
            string dir = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string collisionPath = $"{dir}/{desiredName}.asset";
            if (AssetDatabase.LoadAssetAtPath<StatusEffectSO>(collisionPath) != null &&
                collisionPath != path)
            {
                Debug.LogWarning(
                    $"[StatusEffectSO] Auto-rename skipped: '{desiredName}.asset' already exists at '{dir}'. " +
                    $"Resolve the conflict manually. Current asset: '{currentFileName}'.",
                    this);
                return;
            }

            string error = AssetDatabase.RenameAsset(path, desiredName);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning(
                    $"[StatusEffectSO] Auto-rename failed for '{currentFileName}' → '{desiredName}': {error}",
                    this);
                return;
            }

            AssetDatabase.SaveAssets();
        }

        private string BuildDesiredFileName()
        {
            // Sanitize DisplayName for filesystem use: keep alphanumerics + underscore.
            string safeDisplay = SanitizeForFileName(
                string.IsNullOrWhiteSpace(displayName) ? effectId.ToString() : displayName);

            string effectIdStr = effectId.ToString();
            return $"StatusEffect_{safeDisplay}_{effectIdStr}";
        }

        private static string SanitizeForFileName(string input)
        {
            if (string.IsNullOrEmpty(input)) return "Unnamed";

            char[] buf = new char[input.Length];
            int j = 0;
            foreach (var c in input)
            {
                bool isAlpha = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool isNum = (c >= '0' && c <= '9');
                if (isAlpha || isNum)
                    buf[j++] = c;
                else if (c == ' ' || c == '-' || c == '_')
                    buf[j++] = '_';
                // skip all other chars
            }
            return j == 0 ? "Unnamed" : new string(buf, 0, j);
        }

        private static CharacterStatusPrimitiveDatabaseSO TryFindPrimitiveDatabaseAsset()
        {
            var guids = AssetDatabase.FindAssets("t:CharacterStatusPrimitiveDatabaseSO");
            if (guids == null || guids.Length == 0) return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<CharacterStatusPrimitiveDatabaseSO>(path);
        }

        private static string GenerateSuggestedKey(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) input = "status";
            return NormalizeKey(input);
        }

        private static string NormalizeKey(string key)
        {
            key = key.Trim().ToLowerInvariant();

            char[] buf = new char[key.Length];
            int j = 0;
            bool lastWasUnderscore = false;

            for (int i = 0; i < key.Length; i++)
            {
                char c = key[i];

                bool isAlpha = (c >= 'a' && c <= 'z');
                bool isNum = (c >= '0' && c <= '9');

                if (isAlpha || isNum)
                {
                    buf[j++] = c;
                    lastWasUnderscore = false;
                    continue;
                }

                if (c == ' ' || c == '-' || c == '/' || c == '\\')
                {
                    if (!lastWasUnderscore && j > 0)
                    {
                        buf[j++] = '_';
                        lastWasUnderscore = true;
                    }
                    continue;
                }
            }

            while (j > 0 && buf[j - 1] == '_') j--;

            return j == 0 ? "status" : new string(buf, 0, j);
        }
#endif
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Supporting enums (MVP). Keep these stable once serialized assets exist.
    // ─────────────────────────────────────────────────────────────────────────────

    public enum StackMode
    {
        Additive = 0,
        Replace = 1,
        RefreshDuration = 2,
        AdditiveClamped = 3,
    }

    public enum DecayMode
    {
        None = 0,
        LinearStacks = 1,
        DurationTurns = 2,
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
        PlayerTurnStart = 8,
        AudienceTurnStart = 9,
    }

    public enum ValueType
    {
        Flat = 0,
        Percent = 1,
    }
}