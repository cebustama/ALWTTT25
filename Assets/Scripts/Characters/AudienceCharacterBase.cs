using ALWTTT.Actions;
using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Extentions;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.Music;
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
        // TODO: Refactor
        public AudienceCharacterStats Stats => stats;

        protected AudienceAbilityData NextAbility;

        public AudienceCharacterData AudienceCharacterData => audienceCharacterData;
        public AudienceCharacterCanvas AudienceCharacterCanvas => characterCanvas;
        public bool IsTall => AudienceCharacterData.IsTall;

        private bool isBlocked;

        public bool IsBlocked 
        {
            get => isBlocked;
            set
            {
                if (SpriteRenderer != null)
                    SpriteRenderer.color = value ? obscuredColor : Color.white;

                if (value)
                    stats.ApplyStatus(StatusType.Blocked, 1);
                else
                    stats.ClearStatus(StatusType.Blocked);

                isBlocked = value;
            }
        }
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
            Debug.Log($"<color=green>{CharacterId} CONVINCED!");

            GigManager.RecalculateAudienceObstructions();

            // TODO
            characterAnimator.SetBPM(120);
            characterAnimator.SkipEveryNBeats = 1;
            characterAnimator.JumpOnBeat = true;
            characterAnimator.RotateOnBeat = false;
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

        /// <summary>
        /// Convert the information about a finished loop into a discrete impression
        /// in the range [-2, 2]:
        /// -2 = very negative, -1 = negative, 0 = neutral, 1 = positive, 2 = very positive.
        /// Default implementation is neutral; override per archetype.
        /// </summary>
        public virtual int ResolveLoopEffect(LoopFeedbackContext ctx)
        {
            // For now we keep it simple and neutral; this is the extensible hook.
            // Later you can use ctx.PartIndex, ctx.LoopIndexWithinPart, inspiration, etc,
            // plus this character's preferences, to compute a meaningful value.
            int impression = 0;

            Debug.Log(
                $"[AudienceCharacterBase] {CharacterId} ResolveLoopEffect " +
                $"part={ctx.PartIndex}, loop={ctx.LoopIndexWithinPart}, " +
                $"label={ctx.PartLabel} → {impression}");

            return impression;
        }

        private int usedAbilityCount;
        private void ShowNextAbility()
        {
            var ability = AudienceCharacterData.GetAbility(usedAbilityCount);

            if (ability == null)
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} ShowNextAbility: " +
                    "AudienceCharacterData.GetAbility returned NULL. " +
                    "Check AbilityList on AudienceCharacterData.");

                NextAbility = null;
                AudienceCharacterCanvas.IntentImage.gameObject.SetActive(false);
                AudienceCharacterCanvas.NextActionValueText.gameObject.SetActive(false);
                return;
            }

            if (ability.ActionList == null || ability.ActionList.Count == 0)
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} ShowNextAbility: " +
                    $"Ability '{ability.AbilityName}' has no ActionList or no actions. " +
                    "Audience will have nothing to do on its turn.");
            }

            NextAbility = ability;

            if (NextAbility.Intention != null && NextAbility.Intention.IntentionSprite != null)
            {
                AudienceCharacterCanvas.IntentImage.sprite = 
                    NextAbility.Intention.IntentionSprite;
                AudienceCharacterCanvas.IntentImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} ShowNextAbility: " +
                    $"Ability '{NextAbility.AbilityName}' has no Intention sprite.");
                AudienceCharacterCanvas.IntentImage.gameObject.SetActive(false);
            }

            AudienceCharacterCanvas.NextAbility = NextAbility;

            var showValue =
                NextAbility.ActionList != null &&
                NextAbility.ActionList.Count > 0 &&
                !NextAbility.HideActionValue &&
                NextAbility.ActionList[0] != null &&
                NextAbility.ActionList[0].ActionValue != 0;

            if (!showValue)
            {
                AudienceCharacterCanvas.NextActionValueText.gameObject.SetActive(false);
            }
            else
            {
                AudienceCharacterCanvas.NextActionValueText.gameObject.SetActive(true);
                AudienceCharacterCanvas.NextActionValueText.text =
                    "x" + NextAbility.ActionList[0].ActionValue.ToString();
            }

            usedAbilityCount++;
        }

        #region Action Routines
        public virtual IEnumerator AbilityRoutine()
        {
            Debug.Log($"{CharacterId} Ability Routine started.");

            if (stats != null && stats.IsStunned)
            {
                Debug.Log($"{CharacterId} is stunned. Skipping Ability.");
                yield break;
            }

            // Hide intent icon
            AudienceCharacterCanvas.IntentImage.gameObject.SetActive(false);

            if (NextAbility == null)
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} AbilityRoutine: " +
                    "NextAbility is NULL. This usually means ShowNextAbility " +
                    "was not called before the audience turn.");
                yield break;
            }

            if (NextAbility.ActionList == null || NextAbility.ActionList.Count == 0)
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} AbilitynRoutine: " +
                    $"Ability '{NextAbility.AbilityName}' has no actions. Skipping turn.");
                yield break;
            }

            float abilityDelay = NextAbility.AbilityDuration;

            var animData = NextAbility.Animation;
            if (animData != null)
            {
                // Override delay if animationDuration > 0
                if (animData.AnimationDuration > 0f)
                    abilityDelay = animData.AnimationDuration;

                // Optionally disable beat animator
                if (animData.DisableBeatAnimator && CharacterAnimator != null)
                    CharacterAnimator.enabled = false;

                // Fire animator trigger
                if (Animator != null && !string.IsNullOrEmpty(animData.AnimatorTrigger))
                {
                    Debug.Log($"<color=red>{CharacterId} triggering " +
                        $"anim id {animData.AnimatorTrigger}</color>");
                    Animator.ResetTrigger(animData.AnimatorTrigger);
                    Animator.SetTrigger(animData.AnimatorTrigger);
                }
            }

            // TODO: Animation, SFX, VFX
            FxManager.Instance?.SpawnFloatingText(
                TextSpawnRoot,
                $"{NextAbility.AbilityName}",
                0, 1, Color.red);

            // Wait for animation to finish
            if (abilityDelay > 0f)
                yield return new WaitForSeconds(abilityDelay);

            // Re-enable beat animator if needed
            if (animData != null && animData.DisableBeatAnimator && 
                CharacterAnimator != null)
                CharacterAnimator.enabled = true;

            foreach (var action in NextAbility.ActionList)
            {
                yield return ExecuteActionWithTiming(action);
            }

            Debug.Log($"{CharacterId} Ability Routine finished.");
        }

        private IEnumerator ExecuteActionWithTiming(CharacterActionData action)
        {
            if (action == null)
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} ExecuteActionWithTiming: " +
                    $"Null CharacterActionData inside ability '{NextAbility.AbilityName}'.");
                yield break;
            }

            var targets = ResolveTargetsFor(action);
            if (targets == null || targets.Count == 0)
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} ExecuteActionWithTiming: " +
                    $"No targets resolved for action {action.CardActionType} " +
                    $"in ability '{NextAbility.AbilityName}'.");
                yield break;
            }

            var ctx = new AudienceActionContext();
            var executor = CharacterActionProcessor.GetAction(action.CardActionType);
            if (executor == null)
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} ExecuteActionWithTiming: " +
                    $"No CharacterActionProcessor registered for {action.CardActionType}.");
                yield break;
            }

            // Delay between Actions inside same Ability
            float actionDelay = (action.ActionDelay > 0f)
                ? action.ActionDelay
                : 0.1f; // default pequeño si no se setea nada

            Debug.Log($"<color=red>{CharacterId} " +
                $"action {action.CardActionType.ToString()} " +
                $"delay {actionDelay}</color>");

            FxManager.Instance?.SpawnFloatingText(
                    TextSpawnRoot,
                    $"{action.CardActionType.ToString()}",
                    0, 1, Color.cyan);

            if (actionDelay > 0f)
                yield return new WaitForSeconds(actionDelay);

            // Execute action per target, wait for reaction
            foreach (var target in targets)
            {
                if (target == null) continue;

                // Get reaction duration based on action and target
                float reactionDuration = GetPerTargetReactionDuration(action, target);

                var p = new CharacterActionParameters(
                    action.ActionValue, this, target, ctx,
                    duration: reactionDuration); // Send for reaction timing

                // Apply effects
                executor.DoAction(p);

                if (reactionDuration > 0f)
                    yield return new WaitForSeconds(reactionDuration);
            }

            Debug.Log($"<color=red>Finished delay.</color>");
        }

        private float GetPerTargetReactionDuration(
            CharacterActionData action, CharacterBase target)
        {
            float actionDelay = action.ActionDelay;

            // TODO: timing values in GameplayData
            return 2f;
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