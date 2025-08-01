using ALWTTT.Data;
using ALWTTT.Interfaces;
using UnityEngine;

namespace ALWTTT.Characters.Audience
{
    public class AudienceCharacterBase : CharacterBase, IAudienceMember
    {
        [SerializeField] protected AudienceCharacterData audienceCharacterData;
        [SerializeField] protected AudienceCharacterCanvas characterCanvas;
        // TODO Sound profile

        public AudienceCharacterData AudienceCharacterData => audienceCharacterData;
        public AudienceCharacterCanvas CharacterCanvas => characterCanvas;
        public AudienceCharacterStats CharacterStats { get; protected set; }

        public override void BuildCharacter()
        {
            base.BuildCharacter();
            CharacterCanvas.InitCanvas();

            // Stats
            CharacterStats = new AudienceCharacterStats(
                AudienceCharacterData.MaxVibe,
                CharacterCanvas
            );
            CharacterStats.OnConvinced += OnConvinced;
            CharacterStats.SetCurrentVibe(CharacterStats.CurrentVibe);

            GigManager.OnPlayerTurnStarted += ShowNextIntent;
            GigManager.OnEnemyTurnStarted += CharacterStats.TriggerAllStatus;
        }

        protected void OnConvinced()
        {
            // TODO
        }

        public void Dispose()
        {
            if (CharacterStats != null)
            {
                CharacterStats.OnConvinced -= OnConvinced;
            }

            if (GigManager != null)
            {
                GigManager.OnPlayerTurnStarted -= ShowNextIntent;
            }

            if (GigManager != null && CharacterStats != null)
            {
                GigManager.OnEnemyTurnStarted -= CharacterStats.TriggerAllStatus;
            }

            CharacterStats.Dispose();
        }

        private void ShowNextIntent()
        {
            // TODO
        }
    }
}