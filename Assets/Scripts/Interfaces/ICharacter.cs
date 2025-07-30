using ALWTTT.Characters;
using ALWTTT.Enums;

namespace ALWTTT.Interfaces
{
    public interface ICharacter
    {
        public CharacterBase GetCharacterBase();
        public CharacterType GetCharacterType();
    }
}


