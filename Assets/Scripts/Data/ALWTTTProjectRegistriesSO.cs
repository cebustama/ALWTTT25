using UnityEngine;
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
        [SerializeField] private StatusEffectCatalogueSO statusEffectCatalogue;

        [Header("Optional (add only if you feel pain)")]
        [SerializeField] private GameplayData gameplayData;
        [SerializeField] private GigSetupConfigData gigSetupConfig;
        [SerializeField] private SpecialKeywordData specialKeywordData;
        [SerializeField] private StatusIconsData statusIconsData;
        [SerializeField] private RewardContainerData rewardContainer;

        public CharacterStatusPrimitiveDatabaseSO CSO => csoPrimitiveDatabase;
        public StatusEffectCatalogueSO StatusCatalogue => statusEffectCatalogue;

        public GameplayData Gameplay => gameplayData;
        public GigSetupConfigData GigSetup => gigSetupConfig;
        public SpecialKeywordData SpecialKeywords => specialKeywordData;
        public StatusIconsData StatusIcons => statusIconsData;
        public RewardContainerData Rewards => rewardContainer;

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

            if (statusEffectCatalogue == null)
                Debug.LogWarning("[ALWTTTProjectRegistries] Missing StatusEffectCatalogue reference.", this);
        }
#endif
    }
}
