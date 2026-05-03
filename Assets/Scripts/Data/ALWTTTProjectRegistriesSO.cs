using UnityEngine;
using UnityEngine.Serialization;
using ALWTTT.Status;
using ALWTTT.Data;

namespace ALWTTT
{
    [CreateAssetMenu(
        fileName = "ALWTTTProjectRegistries",
        menuName = "ALWTTT/Project/Project Registries",
        order = 1)]
    public sealed class ALWTTTProjectRegistriesSO : ScriptableObject
    {
        [Header("Status (CSO)")]
        [SerializeField] private CharacterStatusPrimitiveDatabaseSO csoPrimitiveDatabase;

        [Tooltip("Status catalogue used by musician characters " +
                 "(flow, composure, choke, shaken, exposed, feedback).")]
        [FormerlySerializedAs("statusEffectCatalogue")]
        [SerializeField] private StatusEffectCatalogueSO statusEffectCatalogueMusicians;

        [Tooltip("Status catalogue used by audience characters " +
                 "(earworm; future audience-side statuses).")]
        [SerializeField] private StatusEffectCatalogueSO statusEffectCatalogueAudience;

        [Header("Optional (add only if you feel pain)")]
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private GigSetupConfigData gigSetupConfig;
        [SerializeField] private SpecialKeywordData specialKeywordData;
        [SerializeField] private RewardContainerData rewardContainer;

        public CharacterStatusPrimitiveDatabaseSO CSO => csoPrimitiveDatabase;

        public StatusEffectCatalogueSO StatusCatalogueMusicians => statusEffectCatalogueMusicians;
        public StatusEffectCatalogueSO StatusCatalogueAudience => statusEffectCatalogueAudience;

        /// <summary>
        /// Legacy alias preserved for source compatibility with callers written before
        /// the MB2 catalogue split (musicians/audience). Returns the musicians catalogue,
        /// which matches the pre-split semantics (it was the only catalogue that existed).
        /// New code should prefer <see cref="StatusCatalogueMusicians"/> /
        /// <see cref="StatusCatalogueAudience"/> or the TryGet helpers below.
        /// </summary>
        public StatusEffectCatalogueSO StatusCatalogue => statusEffectCatalogueMusicians;

        public GameplayData Gameplay => gameplayData;
        public GigSetupConfigData GigSetup => gigSetupConfig;
        public SpecialKeywordData SpecialKeywords => specialKeywordData;
        public RewardContainerData Rewards => rewardContainer;

        /// <summary>
        /// Search both status catalogues (musicians first, then audience) for a status
        /// effect by its human-readable key. Returns the first match. Used by tooling
        /// (Card Editor JSON import, etc.) to author cards that may reference statuses
        /// across the MB2-split catalogues from a single import surface.
        /// </summary>
        public bool TryGetStatusEffectByKey(string key, out StatusEffectSO so)
        {
            so = null;
            if (string.IsNullOrWhiteSpace(key)) return false;

            if (statusEffectCatalogueMusicians != null &&
                statusEffectCatalogueMusicians.TryGetByKey(key, out so) && so != null)
                return true;

            if (statusEffectCatalogueAudience != null &&
                statusEffectCatalogueAudience.TryGetByKey(key, out so) && so != null)
                return true;

            so = null;
            return false;
        }

        /// <summary>
        /// Search both catalogues by primitive id. Returns the first match (musicians
        /// then audience). Note: when both catalogues hold a variant of the same primitive
        /// (e.g. Feedback in musicians and Earworm in audience both use DamageOverTime),
        /// this returns the musicians variant. Prefer key-based lookup when authoring
        /// audience-side statuses that share a primitive with a musician variant.
        /// </summary>
        public bool TryGetStatusEffectByPrimitive(CharacterStatusId id, out StatusEffectSO so)
        {
            so = null;

            if (statusEffectCatalogueMusicians != null &&
                statusEffectCatalogueMusicians.TryGet(id, out so) && so != null)
                return true;

            if (statusEffectCatalogueAudience != null &&
                statusEffectCatalogueAudience.TryGet(id, out so) && so != null)
                return true;

            so = null;
            return false;
        }

        public static ALWTTTProjectRegistriesSO FindInResources(
            string resourceName = "ALWTTTProjectRegistries")
        {
            return Resources.Load<ALWTTTProjectRegistriesSO>(resourceName);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (csoPrimitiveDatabase == null)
                Debug.LogWarning("[ALWTTTProjectRegistries] Missing CSO Primitive Database reference.", this);

            if (statusEffectCatalogueMusicians == null)
                Debug.LogWarning("[ALWTTTProjectRegistries] Missing StatusEffectCatalogue (Musicians) reference.", this);

            if (statusEffectCatalogueAudience == null)
                Debug.LogWarning("[ALWTTTProjectRegistries] Missing StatusEffectCatalogue (Audience) reference.", this);
        }
#endif
    }
}