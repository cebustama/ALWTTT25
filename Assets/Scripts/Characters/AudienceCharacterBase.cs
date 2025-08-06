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

        private AudienceCharacterStats stats;

        public override IAudienceStats AudienceStats => stats;

        public AudienceCharacterData AudienceCharacterData => audienceCharacterData;
        public AudienceCharacterCanvas CharacterCanvas => characterCanvas;

        public override void BuildCharacter()
        {
            base.BuildCharacter();
            CharacterCanvas.InitCanvas(AudienceCharacterData.CharacterName);

            // Stats
            stats = new AudienceCharacterStats(
                AudienceCharacterData.MaxVibe,
                CharacterCanvas
            );
            stats.OnConvinced += OnConvinced;
            stats.SetCurrentVibe(stats.CurrentVibe);

            Debug.Log("{AudienceCharacterBase} Stats: " + stats.ToString());

            GigManager.OnPlayerTurnStarted += ShowNextIntent;
            GigManager.OnEnemyTurnStarted += stats.TriggerAllStatus;
        }

        protected void OnConvinced()
        {
            // TODO
        }

        public void Dispose()
        {
            if (stats != null)
            {
                stats.OnConvinced -= OnConvinced;
            }

            if (GigManager != null)
            {
                GigManager.OnPlayerTurnStarted -= ShowNextIntent;
            }

            if (GigManager != null && stats != null)
            {
                GigManager.OnEnemyTurnStarted -= stats.TriggerAllStatus;
            }

            stats.Dispose();
        }

        private void ShowNextIntent()
        {
            // TODO
        }
    }
}