using ALWTTT.Status;
using ALWTTT.Status.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Characters
{
    /// <summary>
    /// [WINK-1] Persistent per-status visual driver. Lives on a character
    /// prefab, self-binds to its CharacterBase's StatusEffectContainer, and
    /// toggles registered GameObjects: stacks >= 1 -> SetActive(true),
    /// clear -> SetActive(false).
    ///
    /// [D-WINK-5=A] The OFF path is an INSTANT SetActive(false), no disappear
    /// animation: an animated tail would reopen the re-apply-during-disappear
    /// collision that CharacterCanvas:134 documents for icons. Instant off
    /// avoids that class of bug by construction.
    ///
    /// Binding happens in Start(): CharacterBase.Awake() creates the container
    /// (and SetOwner), and Unity guarantees all Awakes run before Starts even
    /// for runtime-instantiated prefabs — so the container always exists here.
    /// </summary>
    public class StatusVisualDriver : MonoBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            [Tooltip("Primitive status id that drives this visual.")]
            public CharacterStatusId status;
            [Tooltip("OPTIONAL StatusEffectSO.StatusKey of the exact variant. Empty = any " +
                 "variant sharing the primitive id above (legacy behaviour). Fill it when " +
                 "two authored statuses could share one CharacterStatusId — e.g. " +
                 "'captivated' vs a future second DamageTakenUpMultiplier variant.")]
            public string statusKey;
            [Tooltip("GameObject toggled by the status (author it DISABLED in the prefab).")]
            public GameObject visual;
        }

        [SerializeField] private List<Entry> entries = new();

        private StatusEffectContainer _container;

        private void Start()
        {
            var owner = GetComponentInParent<CharacterBase>();
            _container = owner != null ? owner.Statuses : null;

            if (_container == null)
            {
                Debug.LogWarning(
                    $"[StatusVisualDriver] No CharacterBase/StatusEffectContainer " +
                    $"found above '{name}'. Driver inactive.", this);
                enabled = false;
                return;
            }

            _container.OnStatusChanged += OnStatusChanged;
            _container.OnStatusCleared += OnStatusCleared;

            // Initial sync — a status may already be active when the driver binds.
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e?.visual == null) continue;
                e.visual.SetActive(IsEntryActive(e));
            }
        }

        private void OnDestroy()
        {
            if (_container == null) return;
            _container.OnStatusChanged -= OnStatusChanged;
            _container.OnStatusCleared -= OnStatusCleared;
            _container = null;
        }

        /// <summary>
        /// [D-WINK-AUTH-3=B] ON path checks the VARIANT, not just the primitive:
        /// several authored StatusEffectSOs can share one CharacterStatusId
        /// (Captivated is a DamageTakenUpMultiplier), so keying visuals on the
        /// primitive alone would light Captivated's hearts for any future
        /// status built on the same primitive.
        /// </summary>
        private void OnStatusChanged(CharacterStatusId id, int newStacks)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null || e.status != id || e.visual == null) continue;
                e.visual.SetActive(newStacks > 0 && KeyMatchesActive(e));
            }
        }

        /// <summary>
        /// OFF path is primitive-only ON PURPOSE, and it is not a hole: the
        /// container holds at most ONE instance per CharacterStatusId, so a
        /// clear of this id retires whatever variant occupied the slot. A key
        /// check here would be impossible anyway — the instance carrying the
        /// key is already gone when OnStatusCleared fires. Still instant
        /// (D-WINK-5=A).
        /// </summary>
        private void OnStatusCleared(CharacterStatusId id)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null || e.status != id || e.visual == null) continue;
                e.visual.SetActive(false);
            }
        }

        private bool KeyMatchesActive(Entry e)
        {
            if (string.IsNullOrEmpty(e.statusKey)) return true;   // any variant
            if (_container == null) return false;
            if (!_container.TryGet(e.status, out var inst) || inst == null) return false;
            var def = inst.Definition;
            return def != null && def.StatusKey == e.statusKey;
        }

        private bool IsEntryActive(Entry e)
            => _container != null
               && _container.GetStacks(e.status) > 0
               && KeyMatchesActive(e);
    }
}