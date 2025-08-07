using ALWTTT.Data;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using System;
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

            stats = new BandCharacterStats(
                MusicianCharacterData.InitialMaxStress,
                BandCharacterCanvas
            );

            var data = GameManager.PersistentGameplayData.MusicianHealthDataList.Find(
                x => x.CharacterId == MusicianCharacterData.CharacterId);

            // Replace or create MusicianHealthData in PersistentGameplayData structure
            if (data != null)
            {
                stats.CurrentStress = data.CurrentStress;
                stats.MaxStress = data.MaxStress;
            }
            else
            {
                GameManager.PersistentGameplayData.SetMusicianHealthData(
                    MusicianCharacterData.CharacterId, stats.CurrentStress, stats.MaxStress);
            }

            stats.OnBreakdown += OnBreakdown;
            stats.SetCurrentStress(stats.CurrentStress);

            Debug.Log("{MusicianBase} Stats: " + stats.ToString());
        }

        public void SetSpriteLayerOrder(int targetOrder)
        {
            SpriteRenderer.sortingOrder = targetOrder;
        }

        protected void OnBreakdown()
        {
            // TODO: "Stunned", cannot perform for one turn
        }
    }

    [Serializable]
    public class MusicianHealthData
    {
        [SerializeField] private string characterId;
        [SerializeField] private int maxStress;
        [SerializeField] private int currentStress;

        public int MaxStress
        {
            get => maxStress;
            set => maxStress = value;
        }

        public int CurrentStress
        {
            get => currentStress;
            set => currentStress = value;
        }

        public string CharacterId
        {
            get => characterId;
            set => characterId = value;
        }
    }
}