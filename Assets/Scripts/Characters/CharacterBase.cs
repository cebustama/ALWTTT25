using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.Characters
{
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        public virtual IMusicianStats MusicianStats => null;
        public virtual IAudienceStats AudienceStats => null;

        [Header("Base settings")]
        [SerializeField] private CharacterType characterType;
        [SerializeField] private Transform textSpawnRoot;
        [SerializeField] private SpriteRenderer spriteRenderer;

        #region Encapsulation
        public CharacterType CharacterType => characterType;
        public Transform TextSpawnRoot => textSpawnRoot;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        #endregion

        #region Cache
        protected GigManager GigManager => GigManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
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