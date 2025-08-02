using ALWTTT.Characters.Band;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "New MusicianCharacterData",
    menuName = "ALWTTT/Characters/MusicianCharacterData")]
    public class MusicianCharacterData : ScriptableObject
    {
        [SerializeField] private string characterId;
        [SerializeField] private string characterName;
        [SerializeField] private string characterDescription;

        [SerializeField] private MusicianBase characterPrefab;

        #region Encapsulation
        public string CharacterId => characterId;
        public string CharacterName => characterName;
        public string CharacterDescription => characterDescription;
        public MusicianBase CharacterPrefab => characterPrefab;
        #endregion
    }
}