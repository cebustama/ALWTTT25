using ALWTTT.Data;
using ALWTTT.Interfaces;
using UnityEngine;

namespace ALWTTT.Characters.Audience
{
    public class AudienceCharacterBase : CharacterBase, IAudienceMember
    {
        [SerializeField] protected AudienceCharacterData audienceCharacterData;
        // TODO Canvas
        // TODO Sound profile

        public AudienceCharacterData AudienceCharacterData => audienceCharacterData;

        public override void BuildCharacter()
        {
            base.BuildCharacter();
            // TODO Init Canvas


        }
    }
}