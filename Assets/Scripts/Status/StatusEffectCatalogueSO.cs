#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Status
{
    /// <summary>
    /// ALWTTT Status Effect Catalogue:
    /// Owns the list of StatusEffectSO assets used by the game.
    ///
    /// Purpose:
    /// - Prevent duplicates (one StatusEffectSO per CharacterStatusId).
    /// - Provide fast lookup at runtime/editor.
    /// - Be the future hub for cross-referencing cards/abilities/etc (out of MVP).
    /// </summary>
    [CreateAssetMenu(
        fileName = "StatusEffectCatalogue",
        menuName = "ALWTTT/Status/Status Effect Catalogue",
        order = 30)]
    public sealed class StatusEffectCatalogueSO : ScriptableObject
    {
        [SerializeField] private List<StatusEffectSO> _effects = new();

        [NonSerialized] private Dictionary<CharacterStatusId, StatusEffectSO> _cache;
        [NonSerialized] private bool _cacheBuilt;

        public IReadOnlyList<StatusEffectSO> Effects => _effects;

        public bool Contains(CharacterStatusId id)
        {
            EnsureCache();
            return _cache.ContainsKey(id);
        }

        public bool TryGet(CharacterStatusId id, out StatusEffectSO effect)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out effect);
        }

        public StatusEffectSO GetOrThrow(CharacterStatusId id)
        {
            EnsureCache();
            if (_cache.TryGetValue(id, out var effect))
                return effect;

            throw new KeyNotFoundException($"StatusEffect not found in catalogue for id: {id}");
        }

        public void RebuildCache()
        {
            _cacheBuilt = false;
            EnsureCache();
        }

        private void OnEnable()
        {
            _cacheBuilt = false;
            _cache = null;
        }

        private void EnsureCache()
        {
            if (_cacheBuilt) return;

            _cache = new Dictionary<CharacterStatusId, StatusEffectSO>(_effects.Count);
            for (int i = 0; i < _effects.Count; i++)
            {
                var e = _effects[i];
                if (e == null) continue;

                // First one wins; duplicates are flagged in OnValidate (editor) and ignored here.
                if (!_cache.ContainsKey(e.EffectId))
                    _cache.Add(e.EffectId, e);
            }

            _cacheBuilt = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Detect duplicates + nulls early.
            var seen = new HashSet<CharacterStatusId>();
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var e = _effects[i];
                if (e == null) continue;

                if (!seen.Add(e.EffectId))
                {
                    Debug.LogError(
                        $"[StatusEffectCatalogue] Duplicate EffectId '{e.EffectId}' found in catalogue '{name}'. " +
                        $"Keep exactly one StatusEffectSO per CharacterStatusId.",
                        this);
                }
            }

            _cacheBuilt = false;
            _cache = null;
        }

        /// <summary>
        /// Editor-only: add an effect if missing; keeps list stable.
        /// </summary>
        public bool EditorTryAdd(StatusEffectSO effect)
        {
            if (effect == null) return false;
            if (_effects.Contains(effect)) return false;

            // Prevent duplicate ids:
            foreach (var e in _effects)
                if (e != null && e.EffectId == effect.EffectId)
                    return false;

            _effects.Add(effect);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            _cacheBuilt = false;
            return true;
        }
#endif
    }
}
