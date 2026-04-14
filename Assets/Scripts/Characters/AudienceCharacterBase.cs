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

        private AudienceCharacterStats stats;
        public override IAudienceStats AudienceStats => stats;
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

                // M1.2 (Decision E3): Blocked is a visual indicator only (sprite tint).
                // Legacy stats.ApplyStatus/ClearStatus(StatusType.Blocked) removed.
                // If Blocked needs a status icon in the future, create a Blocked SO.

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

            stats = new AudienceCharacterStats(
                AudienceCharacterData.MaxVibe,
                AudienceCharacterCanvas
            );
            stats.OnConvinced += OnConvinced;
            stats.SetCurrentVibe(stats.CurrentVibe);

            Debug.Log("{AudienceCharacterBase} Stats: " + stats.ToString());

            GigManager.OnPlayerTurnStarted += ShowNextAbility;
            GigManager.OnEnemyTurnStarted += stats.TriggerAllStatus;

            // M1.2: Wire canvas to SO-based StatusEffectContainer for icon display.
            AudienceCharacterCanvas.BindStatusContainer(Statuses);

            AudienceCharacterCanvas.HideContextual();
        }

        protected void OnConvinced()
        {
            Debug.Log($"<color=green>{CharacterId} CONVINCED!");

            GigManager.RecalculateAudienceObstructions();

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

        public virtual int ResolveLoopEffect(LoopFeedbackContext ctx)
        {
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

            if (stats != null && stats.ConsumeStun())
            {
                Debug.Log($"{CharacterId} is stunned. Skipping Ability.");
                yield break;
            }

            AudienceCharacterCanvas.IntentImage.gameObject.SetActive(false);

            if (NextAbility == null)
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} AbilityRoutine: " +
                    "NextAbility is null. Skipping turn.");
                yield break;
            }

            if (NextAbility.ActionList == null || NextAbility.ActionList.Count == 0)
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} AbilityRoutine: " +
                    $"Ability '{NextAbility.AbilityName}' has no actions. Nothing to execute.");
                yield break;
            }

            var ctx = new AudienceActionContext();

            foreach (var action in NextAbility.ActionList)
            {
                if (action == null) continue;

                var targets = ResolveTargetsFor(action);
                if (targets == null || targets.Count == 0) continue;

                yield return StartCoroutine(
                    ExecuteActionWithTiming(action, targets, ctx));
            }
        }

        protected virtual IEnumerator ExecuteActionWithTiming(
            CharacterActionData action,
            List<CharacterBase> targets,
            AudienceActionContext ctx)
        {
            var executor = CharacterActionProcessor.GetAction(action.CardActionType);
            if (executor == null)
            {
                Debug.LogWarning(
                    $"[AudienceCharacterBase] {CharacterId} ExecuteActionWithTiming: " +
                    $"No CharacterActionProcessor registered for {action.CardActionType}.");
                yield break;
            }

            float actionDelay = (action.ActionDelay > 0f)
                ? action.ActionDelay
                : 0.1f;

            Debug.Log($"<color=red>{CharacterId} " +
                $"action {action.CardActionType.ToString()} " +
                $"delay {actionDelay}</color>");

            FxManager.Instance?.SpawnFloatingText(
                    TextSpawnRoot,
                    $"{action.CardActionType.ToString()}",
                    0, 1, Color.cyan);

            if (actionDelay > 0f)
                yield return new WaitForSeconds(actionDelay);

            foreach (var target in targets)
            {
                if (target == null) continue;

                float reactionDuration = GetPerTargetReactionDuration(action, target);

                var p = new CharacterActionParameters(
                    action.ActionValue, this, target, ctx,
                    duration: reactionDuration);

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
            return 2f;
        }

        private List<CharacterBase> ResolveTargetsFor(CharacterActionData action)
        {
            var gm = GigManager;

            switch (action.ActionTargetType)
            {
                case ActionTargetType.Self:
                    return new List<CharacterBase>() { this };

                case ActionTargetType.Musician:
                    {
                        var list = gm.CurrentMusicianCharacterList;
                        if (list.Count == 0) return null;

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
                    return new List<CharacterBase>(gm.CurrentMusicianCharacterList);

                case ActionTargetType.AudienceCharacter:
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
                    return new List<CharacterBase>(gm.CurrentAudienceCharacterList);

                default:
                    return null;
            }
        }

        protected virtual IEnumerator AttackRoutine(AudienceAbilityData targetAbility)
        {
            var waitFrame = new WaitForEndOfFrame();

            var target = GigManager.CurrentMusicianCharacterList.RandomItem();

            var startPos = transform.position + Vector3.up * 2;
            var endPos = target.transform.position + Vector3.up * 2;

            var startRot = transform.localRotation;
            var endRot = transform.localRotation;

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