using ALWTTT.Data;
using ALWTTT.Interfaces;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    public class MusicianBase : CharacterBase, IMusician
    {
        [SerializeField] private BandCharacterCanvas bandCharacterCanvas;
        [SerializeField] private MusicianCharacterData musicianCharacterData;
        [SerializeField] private BandCharacterStats stats;
        public override IMusicianStats MusicianStats => stats;

        #region Encapsulate
        public BandCharacterCanvas BandCharacterCanvas => bandCharacterCanvas;
        public MusicianCharacterData MusicianCharacterData => musicianCharacterData;
        #endregion

        public override void BuildCharacter()
        {
            base.BuildCharacter();
            bandCharacterCanvas.InitCanvas(MusicianCharacterData.CharacterName);
            
            
            
        }
    }
}