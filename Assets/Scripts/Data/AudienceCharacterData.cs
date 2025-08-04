using ALWTTT.Characters.Audience;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = " New AudienceCharacterData", 
        menuName = "ALWTTT/Characters/AudienceCharacterData")]
    public class AudienceCharacterData : ScriptableObject
    {
        [SerializeField] private string characterId;
        [SerializeField] private string characterName;
        [SerializeField][TextArea] private string characterDescription;
        [SerializeField] private int maxVibe;
        [SerializeField] private AudienceCharacterBase characterPrefab;

        #region Encapsulation
        public string CharacterName => characterName;
        public AudienceCharacterBase CharacterPrefab => characterPrefab;
        public int MaxVibe => maxVibe;
        #endregion
    }
}