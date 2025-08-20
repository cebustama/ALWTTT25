using ALWTTT.Actions;
using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Extentions;
using ALWTTT.Interfaces;
using System.Collections;
using System.Collections.Generic;
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
        public AudienceCharacterCanvas AudienceCharacterCanvas => characterCanvas;
        public bool IsTall => AudienceCharacterData.IsTall;
        public bool IsBlocked { get; set; }
        public int ColumnIndex { get; set; }

        public string CharacterId =>
            AudienceCharacterData.CharacterName + "-" + gameObject.GetInstanceID();

        public override void BuildCharacter()
        {
            base.BuildCharacter();
            AudienceCharacterCanvas.InitCanvas(AudienceCharacterData.CharacterName);

            // Stats
            stats = new AudienceCharacterStats(
                AudienceCharacterData.MaxVibe,
                AudienceCharacterCanvas
            );
            stats.OnConvinced += OnConvinced;
            stats.SetCurrentVibe(stats.CurrentVibe);

            Debug.Log("{AudienceCharacterBase} Stats: " + stats.ToString());

            GigManager.OnPlayerTurnStarted += ShowNextAbility;
            GigManager.OnEnemyTurnStarted += stats.TriggerAllStatus;

            AudienceCharacterCanvas.HideContextual();
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
            AudienceCharacterCanvas.IntentImage.sprite = NextAbility.Intention.IntentionSprite;
            AudienceCharacterCanvas.NextAbility = NextAbility;

            if (NextAbility.HideActionValue)
            {
                AudienceCharacterCanvas.NextActionValueText.gameObject.SetActive(false);
            }
            else
            {
                AudienceCharacterCanvas.NextActionValueText.gameObject.SetActive(true);
                AudienceCharacterCanvas.NextActionValueText.text = "x" +
                    NextAbility.ActionList[0].ActionValue.ToString();
            }

            usedAbilityCount++;
            AudienceCharacterCanvas.IntentImage.gameObject.SetActive(true);
        }

        #region Action Routines
        public virtual IEnumerator ActionRoutine()
        {
            Debug.Log($"{CharacterId} Action Routine...");

            if (stats.IsStunned)
            {
                yield break;
            }

            AudienceCharacterCanvas.IntentImage.gameObject.SetActive(false);
            
            /*
            if (NextAbility.Intention.IntentionType == 
                Enums.AudienceIntentionType.DealStress)
            {
                yield return StartCoroutine(AttackRoutine(NextAbility));
            }
            else
            {
                yield return StartCoroutine(BuffRoutine(NextAbility));
            }

            yield return null;*/

            foreach (var action in NextAbility.ActionList)
            {
                var targets = ResolveTargetsFor(action);
                var ctx = new AudienceActionContext(); // extend as needed

                foreach (var target in targets)
                {
                    var p = new CharacterActionParameters(
                        action.ActionValue, this, target, ctx);
                    CharacterActionProcessor.GetAction(action.CardActionType).DoAction(p);
                }

                // Optional short delay between chained actions:
                yield return new WaitForSeconds(0.05f);
            }
        }

        // TODO: Review
        private List<CharacterBase> ResolveTargetsFor(CharacterActionData action)
        {
            // Use the same targeting semantics as CardBase.DetermineTargets
            // and keep behavior deterministic where useful.
            var gm = GigManager;

            switch (action.ActionTargetType)
            {
                case ActionTargetType.Self:
                    return new List<CharacterBase>() { this };

                case ActionTargetType.Musician:
                // Target the “front-most” visible enemy musician by your rules? 
                // Since this is an *audience* action (offense), choose a musician.
                {
                    var list = gm.CurrentMusicianCharacterList;
                    if (list.Count == 0) return null;

                    // Example heuristic: lowest current Stress (focus the least stressed)
                    // TODO: Configurable heuristics
                    MusicianBase best = null;
                    foreach (var m in list)
                    {
                        if (best == null || m.MusicianStats.CurrentStress < best.MusicianStats.CurrentStress)
                            best = m;
                    }
                    return new List<CharacterBase>() { best };
                }

                case ActionTargetType.RandomMusician:
                {
                    var list = gm.CurrentMusicianCharacterList;
                    if (list.Count == 0) return null;
                    var index = Random.Range(0, list.Count);
                    return new List<CharacterBase>() { list[index] };
                }

                case ActionTargetType.AllMusicians:
                    // For multi-target actions you’ll likely loop outside, but the action system
                    // expects a single CharacterBase. Return self (the processor can read ctx).
                    return new List<CharacterBase>(gm.CurrentMusicianCharacterList);

                case ActionTargetType.AudienceCharacter:
                // Pick lowest-Vibe ally (excluding self), fallback to self
                {
                    AudienceCharacterBase best = null;
                    foreach (var a in gm.CurrentAudienceCharacterList)
                    {
                        if (a == this) continue;
                        if (best == null || a.AudienceStats.CurrentVibe < best.AudienceStats.CurrentVibe)
                            best = a;
                    }
                    return new List<CharacterBase>() { best };
                }

                case ActionTargetType.RandomAudienceCharacter:
                {
                    var list = gm.CurrentAudienceCharacterList;
                    if (list.Count == 0) return new List<CharacterBase>() { this };
                    var index = Random.Range(0, list.Count);
                    return new List<CharacterBase>() { list[index] };
                }

                case ActionTargetType.AllAudienceCharacters:
                // Same note as AllAllies: your action impl can fan out via context.
                return new List<CharacterBase>(gm.CurrentAudienceCharacterList);

                default:
                // Safe fallback for unhandled cases
                return null;
            }
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

            // Get target
            //var targetType = targetAbility.


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

        protected override void OnPointerEnter()
        {
            base.OnPointerEnter();
            AudienceCharacterCanvas.ShowContextual();
        }

        protected override void OnPointerExit()
        {
            base.OnPointerExit();
            AudienceCharacterCanvas.HideContextual();
        }
    }
}