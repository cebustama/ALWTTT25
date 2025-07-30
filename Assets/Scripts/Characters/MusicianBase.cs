using ALWTTT.Data;
using ALWTTT.Interfaces;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    public class MusicianBase : CharacterBase, IMusician
    {
        [SerializeField] private MusicianCharacterData musicianCharacterData;

        public MusicianCharacterData MusicianCharacterData => musicianCharacterData;

        public override void BuildCharacter()
        {
            base.BuildCharacter();

            // TODO
        }
    }
}