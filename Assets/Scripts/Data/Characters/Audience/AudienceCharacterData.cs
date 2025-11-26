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
        [SerializeField] private bool isTall; // TODO Generalize
        [SerializeField] private List<AudienceAbilityData> abilityList;
        [SerializeField] private bool followAbilityPattern;

        #region Encapsulation
        public string CharacterName => characterName;
        public AudienceCharacterBase CharacterPrefab => characterPrefab;
        public int MaxVibe => maxVibe;
        public List<AudienceAbilityData> AbilityList => abilityList;
        public bool IsTall => isTall;
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
        [SerializeField] private float abilityDuration;
        [SerializeField] private List<CharacterActionData> actionList;

        [Header("Presentation")]
        [SerializeField] private AbilityAnimationData animation;

        #region Encapsulation
        public string AbilityName => abilityName;
        public AudienceIntentionData Intention => intention;
        public bool HideActionValue => hideActionValue;
        public float AbilityDuration => abilityDuration;
        public List<CharacterActionData> ActionList => actionList;
        public AbilityAnimationData Animation => animation;
        #endregion
    }

    [Serializable]
    public class AbilityAnimationData
    {
        [Header("Animator")]
        [SerializeField] private string animatorTrigger;

        [Tooltip("If > 0, overrides AbilityDuration as the wait time for this animation.")]
        [SerializeField] private float animationDuration = -1f;

        [Tooltip("Disable beat-based CharacterAnimator while this ability plays.")]
        [SerializeField] private bool disableBeatAnimator = true;

        public string AnimatorTrigger => animatorTrigger;
        public float AnimationDuration => animationDuration;
        public bool DisableBeatAnimator => disableBeatAnimator;
    }
}