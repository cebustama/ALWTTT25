using ALWTTT.Data;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.Enums;
using System;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    public class MusicianBase : CharacterBase, IMusician
    {
        [SerializeField] private BandCharacterCanvas bandCharacterCanvas;
        [SerializeField] private MusicianCharacterData musicianCharacterData;
        [SerializeField] private BandCharacterStats stats;
        [SerializeField] private ParticleSystem musicianParticleSystem;
        public override IMusicianStats MusicianStats => stats;

        #region Encapsulate
        public BandCharacterCanvas BandCharacterCanvas => bandCharacterCanvas;
        public MusicianCharacterData MusicianCharacterData => musicianCharacterData;
        public BandCharacterStats Stats => stats;
        public string CharacterId => musicianCharacterData.CharacterId;
        #endregion

        private bool _boundToGig;

        public override void BuildCharacter()
        {
            base.BuildCharacter();
            bandCharacterCanvas.InitCanvas(MusicianCharacterData.CharacterName);

            stats = new BandCharacterStats(
                musicianCharacterData.CHR,
                musicianCharacterData.TCH,
                musicianCharacterData.EMT,
                MusicianCharacterData.InitialMaxStress,
                BandCharacterCanvas
            );

            bandCharacterCanvas.UpdateStats(
                stats.Charm,
                stats.Technique,
                stats.Emotion
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

            bandCharacterCanvas.HideContextual();
        }

        public void BindToGigContext()
        {
            if (_boundToGig) return;
            GigManager.OnPlayerTurnStarted += stats.TriggerAllStatus;
            _boundToGig = true;
        }

        public void UnbindFromGigContext()
        {
            if (!_boundToGig) return;
            GigManager.OnPlayerTurnStarted -= stats.TriggerAllStatus;
            _boundToGig = false;
        }

        private void OnDestroy()
        {
            // Safety: if this musician was ever bound, unbind to avoid static event leaks.
            if (_boundToGig) UnbindFromGigContext();
        }

        public void SetSpriteLayerOrder(int targetOrder)
        {
            SpriteRenderer.sortingOrder = targetOrder;
            // TODO: Adjust other sprites in the corresponding order
        }

        protected void OnBreakdown()
        {
            // TODO: "Stunned", cannot perform for one turn
            stats.ApplyStatus(StatusType.Breakdown, 1);
            IsStunned = true;
        }

        protected override void OnPointerEnter()
        {
            base.OnPointerEnter();
            bandCharacterCanvas.ShowContextual();
        }

        protected override void OnPointerExit()
        {
            base.OnPointerExit();
            bandCharacterCanvas.HideContextual();
        }
    }

    // TODO: Persistant stats
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