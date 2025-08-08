using ALWTTT.Actions;
using ALWTTT.Characters.Audience;
using ALWTTT.Extentions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    // TODO: CharacterDataBase class
    [CreateAssetMenu(fileName = " New AudienceCharacterData", 
        menuName = "ALWTTT/Characters/AudienceCharacterData")]
    public class AudienceCharacterData : ScriptableObject
    {
        [Header("Base")]
        [SerializeField] private string characterId;
        [SerializeField] private string characterName;
        [SerializeField][TextArea] private string characterDescription;

        [Header("Audience")]
        [SerializeField] private int maxVibe;
        [SerializeField] private AudienceCharacterBase characterPrefab;

        [Header("Abilities")]
        [SerializeField] private List<AudienceAbilityData> abilityList;
        [SerializeField] private bool followAbilityPattern;

        #region Encapsulation
        public string CharacterName => characterName;
        public AudienceCharacterBase CharacterPrefab => characterPrefab;
        public int MaxVibe => maxVibe;
        public List<AudienceAbilityData> AbilityList => abilityList;
        #endregion

        public AudienceAbilityData GetAbility()
        {
            if (abilityList == null || abilityList.Count == 0)
            {
                Debug.LogError($"Enemy [{characterName}] has no abilities.");
                return null;
            }

            return abilityList.RandomItem();
        }

        public AudienceAbilityData GetAbility(int usedAbilityCount)
        {
            if (followAbilityPattern)
            {
                var index = usedAbilityCount % AbilityList.Count;
                return AbilityList[index];
            }

            return GetAbility();
        }
    }

    [Serializable]
    public class AudienceAbilityData
    {
        [Header("Settings")]
        [SerializeField] private string abilityName;
        [SerializeField] private AudienceIntentionData intention;
        [SerializeField] private bool hideActionValue;
        [SerializeField] private List<CharacterActionData> actionList;

        public string AbilityName => abilityName;
        public AudienceIntentionData Intention => intention;
        public bool HideActionValue => hideActionValue;
        public List<CharacterActionData> ActionList => actionList;
    }
}