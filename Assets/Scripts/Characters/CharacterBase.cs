using ALWTTT.Enums;
using ALWTTT.Interfaces;
using UnityEngine;

namespace ALWTTT.Characters
{
    public class CharacterBase : MonoBehaviour, ICharacter
    {
        [Header("Base settings")]
        [SerializeField] private CharacterType characterType;
        [SerializeField] private Transform textSpawnRoot;

        #region Cache
        public CharacterType CharacterType => characterType;
        public Transform TextSpawnRoot => textSpawnRoot;
        #endregion

        public virtual void BuildCharacter()
        {

        }

        public CharacterBase GetCharacterBase()
        {
            return this;
        }

        public CharacterType GetCharacterType()
        {
            return CharacterType;
        }
    }
}