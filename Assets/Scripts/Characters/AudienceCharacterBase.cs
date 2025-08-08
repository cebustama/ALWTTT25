using ALWTTT.Actions;
using ALWTTT.Data;
using ALWTTT.Extentions;
using ALWTTT.Interfaces;
using System.Collections;
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

        protected AudienceAbilityData NextAbility;

        public AudienceCharacterData AudienceCharacterData => audienceCharacterData;
        public AudienceCharacterCanvas CharacterCanvas => characterCanvas;

        public string CharacterId =>
            AudienceCharacterData.CharacterName + "-" + gameObject.GetInstanceID();

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

            GigManager.OnPlayerTurnStarted += ShowNextAbility;
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
                GigManager.OnPlayerTurnStarted -= ShowNextAbility;
            }

            if (GigManager != null && stats != null)
            {
                GigManager.OnEnemyTurnStarted -= stats.TriggerAllStatus;
            }

            stats.Dispose();
        }

        private int usedAbilityCount;
        private void ShowNextAbility()
        {
            NextAbility = AudienceCharacterData.GetAbility(usedAbilityCount);
            CharacterCanvas.IntentImage.sprite = NextAbility.Intention.IntentionSprite;

            if (NextAbility.HideActionValue)
            {
                CharacterCanvas.NextActionValueText.gameObject.SetActive(false);
            }
            else
            {
                CharacterCanvas.NextActionValueText.gameObject.SetActive(true);
                CharacterCanvas.NextActionValueText.text =
                    NextAbility.ActionList[0].ActionValue.ToString();
            }

            usedAbilityCount++;
            CharacterCanvas.IntentImage.gameObject.SetActive(true);
        }

        #region Action Routines
        public virtual IEnumerator ActionRoutine()
        {
            Debug.Log($"{CharacterId} Action Routine...");

            if (stats.IsStunned)
            {
                yield break;
            }

            CharacterCanvas.IntentImage.gameObject.SetActive(false);
            if (NextAbility.Intention.IntentionType == 
                Enums.AudienceIntentionType.DealStress)
            {
                yield return StartCoroutine(AttackRoutine(NextAbility));
            }
            else
            {
                yield return StartCoroutine(BuffRoutine(NextAbility));
            }

            yield return null;
        }

        protected virtual IEnumerator AttackRoutine(AudienceAbilityData targetAbility)
        {
            var waitFrame = new WaitForEndOfFrame();

            // TODO: Depending on the ActionData TargetType and TargetConditions
            var target = GigManager.CurrentMusicianCharacterList.RandomItem();

            var startPos = transform.position + Vector3.up * 2;
            var endPos = target.transform.position + Vector3.up * 2;

            var startRot = transform.localRotation;
            var endRot = transform.localRotation;

            // TODO: Instantiate a SpeechBubble with garabatos, hurl towards musician
            var speechBubble = Instantiate(
                speechBubblePrefab, startPos, Quaternion.identity);

            yield return StartCoroutine(MoveObjectToTargetRoutine(
                waitFrame, speechBubble,
                startPos, endPos,
                startRot, endRot,
                1f
            ));
            
            foreach (var action in targetAbility.ActionList)
            {
                var ctx = new AudienceActionContext();
                var p = new CharacterActionParameters(
                    action.ActionValue, this, target, ctx);

                CharacterActionProcessor.GetAction(action.CardActionType).DoAction(p);
            }
        }

        protected virtual IEnumerator BuffRoutine(AudienceAbilityData targetAbility)
        {
            var waitFrame = new WaitForEndOfFrame();
            yield return waitFrame;
        }
        #endregion

        private IEnumerator MoveObjectToTargetRoutine(
            WaitForEndOfFrame waitFrame,
            Transform objectTransform,
            Vector3 startPos, Vector3 endPos, 
            Quaternion startRot, Quaternion endRot, 
            float speed)
        {
            var timer = 0f;
            while (true)
            {
                timer += Time.deltaTime * speed;
                objectTransform.position = Vector3.Lerp(startPos, endPos, timer);
                objectTransform.localRotation = Quaternion.Lerp(startRot, endRot, timer);

                if (timer >= 1f)
                {
                    Destroy(objectTransform.gameObject);
                    break;
                }

                yield return waitFrame;
            }
        }
    }
}