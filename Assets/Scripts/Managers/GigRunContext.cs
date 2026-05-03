using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Encounters;
using System.Collections.Generic;
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

            // M4.6-prep batch (2): per-musician auto-assembly toggle.
            // When true, ApplyRunConfig ignores `bandDeck` and assembles the
            // deck from each musician's CardCatalog plus the optional
            // GigSetupConfigData.GenericStarterCatalog. When false, the
            // legacy `bandDeck` path runs (BandDeckData asset → SetBandDeck).
            public bool useMusicianStarters;

            // M4.6-prep batch (2): human-readable label for logs. Set by
            // GigSetupController to either bandDeck.name (legacy path) or
            // "<auto:idA+idB>" style (auto-assembly path). Falls back to
            // bandDeck.name if unset to preserve pre-batch log shape.
            public string deckLabel;

            // M4.6-prep merged (1)/(4): audience override list. When non-null and
            // non-empty, the GigSetupController used the audience picker to override
            // the encounter SO's baked audienceMemberList. When null/empty, the SO's
            // baked list is used (regression-safe).
            public List<AudienceCharacterData> audienceOverride;
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
                $"[GigRunContext] BeginRun -> " +
                $"Deck={_current?.deckLabel ?? _current?.bandDeck?.name ?? "<unset>"}, " +
                $"AutoAssembly={_current?.useMusicianStarters}, " +
                $"AudienceOverride={(_current?.audienceOverride != null ? _current.audienceOverride.Count : 0)}, " +
                $"Encounter={_current?.encounter?.GetLabel()}");
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