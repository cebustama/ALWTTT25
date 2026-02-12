#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Status
{
    /// <summary>
    /// StatusEffect catalogue updated for human keys + variants.
    ///
    /// - Unique index by StatusEffectSO.StatusKey (human stable id).
    /// - Variant index by primitive CharacterStatusId => list of StatusEffectSO.
    /// - Backwards compatible primitive API: TryGet(CharacterStatusId) returns the default variant:
    ///   - prefer IsDefaultVariant == true
    ///   - otherwise first encountered
    /// </summary>
    [CreateAssetMenu(
        fileName = "StatusEffectCatalogue",
        menuName = "ALWTTT/Status/Status Effect Catalogue",
        order = 30)]
    public sealed class StatusEffectCatalogueSO : ScriptableObject
    {
        [SerializeField] private List<StatusEffectSO> _effects = new();

        [NonSerialized] private bool _cacheBuilt;

        // Unique by human key
        [NonSerialized] private Dictionary<string, StatusEffectSO> _byKey;

        // Variants by primitive
        [NonSerialized] private Dictionary<CharacterStatusId, List<StatusEffectSO>> _byPrimitive;
        [NonSerialized] private Dictionary<CharacterStatusId, StatusEffectSO> _defaultByPrimitive;

        public IReadOnlyList<StatusEffectSO> Effects => _effects;

        private static string NormalizeKey(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Key-based API (new)
        // ─────────────────────────────────────────────────────────────────────────────
        public bool ContainsKey(string statusKey)
        {
            EnsureCache();
            statusKey = NormalizeKey(statusKey);
            return statusKey != null && _byKey.ContainsKey(statusKey);
        }

        public bool TryGetByKey(string statusKey, out StatusEffectSO effect)
        {
            EnsureCache();

            statusKey = NormalizeKey(statusKey);
            if (statusKey == null)
            {
                effect = null;
                return false;
            }

            return _byKey.TryGetValue(statusKey, out effect);
        }

        public StatusEffectSO GetOrThrowByKey(string statusKey)
        {
            EnsureCache();
            if (TryGetByKey(statusKey, out var effect))
                return effect;

            throw new KeyNotFoundException($"StatusEffect not found in catalogue for key: '{statusKey}'");
        }

        // ─────────────────────────────────────────────────────────────────────────────
        // Primitive-based API (kept, now supports variants)
        // ─────────────────────────────────────────────────────────────────────────────
        public bool Contains(CharacterStatusId id)
        {
            EnsureCache();
            return _byPrimitive.ContainsKey(id);
        }

        /// <summary>
        /// Backwards compatible: returns the default variant for this primitive id.
        /// </summary>
        public bool TryGet(CharacterStatusId id, out StatusEffectSO effect)
        {
            EnsureCache();
            return _defaultByPrimitive.TryGetValue(id, out effect);
        }

        public StatusEffectSO GetOrThrow(CharacterStatusId id)
        {
            EnsureCache();
            if (_defaultByPrimitive.TryGetValue(id, out var effect))
                return effect;

            throw new KeyNotFoundException($"StatusEffect not found in catalogue for id: {id}");
        }

        public IReadOnlyList<StatusEffectSO> GetVariants(CharacterStatusId id)
        {
            EnsureCache();
            return _byPrimitive.TryGetValue(id, out var list) ? list : Array.Empty<StatusEffectSO>();
        }

        public bool HasVariants(CharacterStatusId id)
        {
            EnsureCache();
            return _byPrimitive.TryGetValue(id, out var list) && list.Count > 1;
        }

        // ─────────────────────────────────────────────────────────────────────────────

        public void RebuildCache()
        {
            _cacheBuilt = false;
            EnsureCache();
        }

        private void OnEnable()
        {
            _cacheBuilt = false;
            _byKey = null;
            _byPrimitive = null;
            _defaultByPrimitive = null;
        }

        private void EnsureCache()
        {
            if (_cacheBuilt) return;

            // IMPORTANT: keys are treated as case-insensitive + trimmed.
            _byKey = new Dictionary<string, StatusEffectSO>(StringComparer.OrdinalIgnoreCase);
            _byPrimitive = new Dictionary<CharacterStatusId, List<StatusEffectSO>>(capacity: Mathf.Max(16, _effects.Count));
            _defaultByPrimitive = new Dictionary<CharacterStatusId, StatusEffectSO>(capacity: Mathf.Max(16, _effects.Count));

            // Track if we've already assigned a default and whether it was explicit.
            var defaultIsExplicit = new Dictionary<CharacterStatusId, bool>();

            for (int i = 0; i < _effects.Count; i++)
            {
                var e = _effects[i];
                if (e == null) continue;

                // Index by key (unique)
                var key = NormalizeKey(e.StatusKey);
                if (key != null)
                {
                    // First one wins; duplicates are flagged in OnValidate.
                    if (!_byKey.ContainsKey(key))
                        _byKey.Add(key, e);
                }

                // Index by primitive (variants)
                if (!_byPrimitive.TryGetValue(e.EffectId, out var list))
                {
                    list = new List<StatusEffectSO>();
                    _byPrimitive.Add(e.EffectId, list);

                    // First encountered default unless an explicit default arrives later.
                    _defaultByPrimitive.Add(e.EffectId, e);
                    defaultIsExplicit[e.EffectId] = e.IsDefaultVariant;
                }
                list.Add(e);

                // Prefer explicit default variant (if set)
                if (e.IsDefaultVariant)
                {
                    bool alreadyExplicit = defaultIsExplicit.TryGetValue(e.EffectId, out var ex) && ex;
                    if (!alreadyExplicit)
                    {
                        _defaultByPrimitive[e.EffectId] = e;
                        defaultIsExplicit[e.EffectId] = true;
                    }
                }
            }

            _cacheBuilt = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Validation policy:
            // - Duplicate StatusKey is NOT allowed (hard error).
            // - Multiple variants per primitive (EffectId) are allowed.
            // - Multiple defaults for the same primitive: warning.
            //
            // NOTE: StatusKey uniqueness is enforced case-insensitively and after trimming.

            var seenKeys = new Dictionary<string, StatusEffectSO>(StringComparer.OrdinalIgnoreCase);
            var defaultCount = new Dictionary<CharacterStatusId, int>();
            var seenAssets = new HashSet<StatusEffectSO>();

            for (int i = 0; i < _effects.Count; i++)
            {
                var e = _effects[i];
                if (e == null) continue;

                if (!seenAssets.Add(e))
                {
                    Debug.LogWarning(
                        $"[StatusEffectCatalogue] Duplicate reference to the same StatusEffectSO '{e.name}' in catalogue '{name}'.",
                        this);
                }

                var key = NormalizeKey(e.StatusKey);
                if (key == null)
                {
                    Debug.LogError(
                        $"[StatusEffectCatalogue] StatusEffectSO '{e.name}' has an empty StatusKey. " +
                        $"Open the asset so OnValidate assigns one, or set it manually.",
                        this);
                }
                else
                {
                    if (seenKeys.TryGetValue(key, out var existing) && existing != e)
                    {
                        Debug.LogError(
                            $"[StatusEffectCatalogue] Duplicate StatusKey '{key}' in catalogue '{name}'. " +
                            $"Conflicts: '{existing.name}' and '{e.name}'. StatusKey must be unique (case-insensitive, trimmed).",
                            this);
                    }
                    else
                    {
                        seenKeys[key] = e;
                    }
                }

                if (e.IsDefaultVariant)
                {
                    defaultCount.TryGetValue(e.EffectId, out int c);
                    defaultCount[e.EffectId] = c + 1;
                }
            }

            foreach (var kv in defaultCount)
            {
                if (kv.Value > 1)
                {
                    Debug.LogWarning(
                        $"[StatusEffectCatalogue] Primitive '{kv.Key}' has {kv.Value} variants marked as Default. " +
                        $"Only one should be default (the catalogue will pick the first explicit one encountered).",
                        this);
                }
            }

            _cacheBuilt = false;
            _byKey = null;
            _byPrimitive = null;
            _defaultByPrimitive = null;

            // Optional: force rebuild so editor tools immediately see it.
            // EnsureCache();
        }

        /// <summary>
        /// Editor-only: add an effect if missing; keeps list stable.
        /// Duplicates by primitive (EffectId) are allowed (variants).
        /// Duplicates by StatusKey are NOT allowed.
        /// </summary>
        public bool EditorTryAdd(StatusEffectSO effect)
        {
            if (effect == null) return false;
            if (_effects.Contains(effect)) return false;

            // Prevent duplicate keys (case-insensitive, trimmed)
            var newKey = NormalizeKey(effect.StatusKey);
            if (newKey == null)
            {
                Debug.LogError(
                    $"[StatusEffectCatalogue] Cannot add StatusEffectSO '{effect.name}' because StatusKey is empty.",
                    this);
                return false;
            }

            foreach (var e in _effects)
            {
                if (e == null) continue;

                var existingKey = NormalizeKey(e.StatusKey);
                if (existingKey == null) continue;

                if (string.Equals(existingKey, newKey, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            _effects.Add(effect);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            _cacheBuilt = false;
            return true;
        }
#endif
    }
}
