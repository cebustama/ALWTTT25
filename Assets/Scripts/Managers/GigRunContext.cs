using ALWTTT.Cards;
using ALWTTT.Encounters;
using UnityEngine;

namespace ALWTTT.Managers
{
    /// <summary>
    /// Persisted "dev run" configuration used when starting a gig from GigSetupScene.
    /// Keeps test overrides out of PersistentGameplayData and avoids static fields.
    /// </summary>
    public class GigRunContext : MonoBehaviour
    {
        public static GigRunContext Instance { get; private set; }


        public enum GigReturnDestination
        {
            Map = 0,
            GigSetup = 1
        }

        [System.Serializable]
        public class RunConfig
        {
            public BandDeckData bandDeck;
            public GigEncounter encounter;

            public bool overrideRequiredSongCount;
            public int requiredSongCount;

            public bool overrideInitialGigInspiration;
            public int initialGigInspiration;

            public bool overrideInspirationPerLoop;
            public int inspirationPerLoop;

            // Optional future knobs
            public bool overrideDiscardHandBetweenTurns;
            public bool discardHandBetweenTurns;

            public bool overrideKeepInspirationBetweenTurns;
            public bool keepInspirationBetweenTurns;

            // Where to go after gig
            public GigReturnDestination returnDestination = GigReturnDestination.Map;
        }

        public bool HasActiveRun => _hasActiveRun;
        public RunConfig Current => _current;

        private bool _hasActiveRun;
        private RunConfig _current;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning(
                    $"[GigRunContext] Duplicate detected. Destroying self. " +
                    $"SelfId={GetInstanceID()} ExistingId={Instance.GetInstanceID()}"
                );
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log($"[GigRunContext] Awake | InstanceId={GetInstanceID()}");
        }

        public void BeginRun(RunConfig config)
        {
            _current = config;
            _hasActiveRun = config != null;

            Debug.Log(
                $"[GigRunContext] BeginRun | " +
                $"InstanceId={GetInstanceID()} | " +
                $"HasActiveRun={_hasActiveRun} | " +
                $"ReturnDest={_current?.returnDestination} | " +
                $"Deck={_current?.bandDeck?.name} | " +
                $"Encounter={_current?.encounter?.GetLabel()}"
            );
        }

        public void Clear()
        {
            _current = null;
            _hasActiveRun = false;
        }

        public int ResolveRequiredSongCount(int fallback)
        {
            if (!_hasActiveRun || _current == null) return fallback;
            if (!_current.overrideRequiredSongCount) return fallback;
            return Mathf.Max(1, _current.requiredSongCount);
        }

        public int ResolveStartingInspiration(int fallback)
        {
            if (!_hasActiveRun || _current == null) return fallback;
            if (!_current.overrideInitialGigInspiration) return fallback;
            return Mathf.Max(0, _current.initialGigInspiration);
        }

        public bool ResolveDiscardHandBetweenTurns(bool fallback)
        {
            if (!_hasActiveRun || _current == null) return fallback;
            if (!_current.overrideDiscardHandBetweenTurns) return fallback;
            return _current.discardHandBetweenTurns;
        }

        public bool ResolveKeepInspirationBetweenTurns(bool fallback)
        {
            if (!_hasActiveRun || _current == null) return fallback;
            if (!_current.overrideKeepInspirationBetweenTurns) return fallback;
            return _current.keepInspirationBetweenTurns;
        }

        public bool TryGetEncounter(out GigEncounter encounter)
        {
            encounter = null;
            if (!_hasActiveRun || _current == null) return false;
            encounter = _current.encounter;
            return encounter != null;
        }

        public bool TryGetBandDeck(out BandDeckData deck)
        {
            deck = null;
            if (!_hasActiveRun || _current == null) return false;
            deck = _current.bandDeck;
            return deck != null;
        }

        public int ResolveInitialGigInspiration(int fallback)
        {
            if (!_hasActiveRun || _current == null) return fallback;
            if (!_current.overrideInitialGigInspiration) return fallback;
            return Mathf.Max(0, _current.initialGigInspiration);
        }

        public int ResolveInspirationPerLoop(int fallback)
        {
            if (!_hasActiveRun || _current == null) return fallback;
            if (!_current.overrideInspirationPerLoop) return fallback;
            return Mathf.Max(0, _current.inspirationPerLoop);
        }
    }
}