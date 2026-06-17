using ALWTTT.Characters;
using ALWTTT.Enums;
using UnityEngine.EventSystems;

namespace ALWTTT.Interfaces
{
    public interface ICharacter
    {
        public CharacterBase GetCharacterBase();
        public CharacterType GetCharacterType();
        public bool IsStunned { get; }
    }
}


