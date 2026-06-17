using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Sensory
{
    /// <summary>
    /// [S2 D-S2-1=A; init-fix D-S2-INIT=C] Typed sensory event bus.
    /// MonoBehaviour singleton with a static accessor, mirroring FxManager
    /// (scene-placed on the managers object, DontDestroyOnLoad,
    /// duplicate-destroy).
    ///
    /// Init-order robustness (D-S2-INIT=C):
    ///   - [DefaultExecutionOrder(-100)] runs this Awake before default-order
    ///     consumers, so Instance is set before any adapter's OnEnable.
    ///   - The Instance getter LAZILY auto-creates a fallback bus in play mode
    ///     if none has claimed the slot yet (covers dynamic instantiation or
    ///     a scene that forgot the component). It NEVER creates in edit mode.
    ///   - Initialization is logged (logInitialization) so pipeline health is
    ///     visible and smoke-testable.
    ///
    /// Design contract: Design_Sensory_Contract_v0_1 �3.
    /// S2 scope (D-S2-2=A): AudienceReactionEvent + SongEndVibeEvent only.
    /// Coexistence (D-S2-3=A): publishers fire ALONGSIDE the existing direct
    /// FxManager calls; deletion of direct calls is an S3 task.
    ///
    /// Known limitation (accepted for S2): a handler that throws aborts the
    /// remaining handlers of that publish (plain multicast invoke). Per-handler
    /// isolation is an S3 concern once multi-subscriber is real.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SensoryEventBus : MonoBehaviour
    {
        private static SensoryEventBus _instance;

        /// <summary>
        /// Canonical bus. Never null in PLAY mode: if no scene-placed bus has
        /// claimed the slot yet, a fallback is auto-created on first access.
        /// In EDIT mode this returns whatever exists (possibly null) and
        /// creates nothing, so editor code can't pollute the open scene.
        /// </summary>
        public static SensoryEventBus Instance
        {
            get
            {
                if (_instance != null) return _instance;
                if (!Application.isPlaying) return null;

                var go = new GameObject("SensoryEventBus (auto)");
                go.AddComponent<SensoryEventBus>(); // Awake sets _instance (field, no recursion)
                Debug.LogWarning(
                    "[SensoryEventBus] No scene-placed instance was ready � " +
                    "auto-created a fallback bus. Pipeline is active. If you " +
                    "see this routinely, confirm a SensoryEventBus component " +
                    "is on the managers object so its serialized settings apply.");
                return _instance;
            }
        }

        private readonly Dictionary<Type, Delegate> _handlers =
            new Dictionary<Type, Delegate>();

        [Header("Diagnostics")]
        [Tooltip("Track publish/delivery counters for smoke tests. " +
                 "Allocates a small array per publish � disable for builds.")]
        [SerializeField] private bool debugCounters = true;

        [Tooltip("Log a confirmation line when the bus initializes. " +
                 "Used as the init go/no-go signal at the top of each test run.")]
        [SerializeField] private bool logInitialization = true;

        public long TotalPublished { get; private set; }
        public long TotalDelivered { get; private set; }

        #region Setup (FxManager pattern + init-order fix)
        private void Awake()
        {
            // _instance != this guard handles a duplicate of the shared
            // managers object on scene reload (the persisted bus already owns
            // the slot; this copy self-destructs without reassigning Instance).
            if (_instance != null && _instance != this)
            {
                if (logInitialization)
                    Debug.Log("[SensoryEventBus] Duplicate detected; " +
                              "destroying this copy, keeping existing Instance.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);

            if (logInitialization)
                Debug.Log("[SensoryEventBus] Initialized � Instance set, " +
                          "DontDestroyOnLoad. Ready for subscribers.");
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
        #endregion

        public void Subscribe<TEvent>(Action<TEvent> handler)
            where TEvent : ISensoryEvent
        {
            if (handler == null) return;
            _handlers.TryGetValue(typeof(TEvent), out var existing);
            _handlers[typeof(TEvent)] = (existing as Action<TEvent>) + handler;
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
            where TEvent : ISensoryEvent
        {
            if (handler == null) return;
            if (!_handlers.TryGetValue(typeof(TEvent), out var existing)) return;

            var remaining = (existing as Action<TEvent>) - handler;
            if (remaining == null) _handlers.Remove(typeof(TEvent));
            else _handlers[typeof(TEvent)] = remaining;
        }

        public void Publish<TEvent>(TEvent evt)
            where TEvent : ISensoryEvent
        {
            if (debugCounters) TotalPublished++;

            if (!_handlers.TryGetValue(typeof(TEvent), out var d) || d == null)
                return;

            var typed = (Action<TEvent>)d;
            var list = typed.GetInvocationList();
            if (debugCounters) TotalDelivered += list.Length;

            // [S3 D-S3-3=A] Per-handler isolation: a throwing subscriber no longer
            // aborts the rest of this publish. Multi-subscriber became real in S3
            // (adapter Spawn + ability animator + future audio).
            for (int i = 0; i < list.Length; i++)
            {
                try { ((Action<TEvent>)list[i]).Invoke(evt); }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"[SensoryEventBus] Handler {i + 1}/{list.Length} for " +
                        $"{typeof(TEvent).Name} threw; continuing. {ex}");
                }
            }
        }

        /// <summary>Current handler count for an event type.</summary>
        public int HandlerCount<TEvent>() where TEvent : ISensoryEvent =>
            _handlers.TryGetValue(typeof(TEvent), out var d) && d != null
                ? d.GetInvocationList().Length
                : 0;
    }
}