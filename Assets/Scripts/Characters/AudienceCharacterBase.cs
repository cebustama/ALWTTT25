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

            // [B2.5 / #3] BPM broadcasts to body + any sub-animators on the audience.
            BroadcastBPM(120);
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
            // [B3 D3=A] Discrete per-axis count algorithm.
            // For each enabled axis on AudienceCharacterData.Taste:
            //   +1 on match, -1 on mismatch, 0 if axis disabled.
            // Sum across axes, clamp to [-2, +2].
            //
            // Axes (4 today):
            //   1. TempoScale (fast/slow thresholds)
            //   2. ActiveTracks count (arrangement density)
            //   3. TimeSignature (preferred / disliked lists)
            //   4. Tonality (preferred / disliked lists)
            //
            // Empty Taste → all axes contribute 0 → returns 0 (neutral archetype).
            // This is the path for any audience asset authored pre-B3 — backward compat.

            int impression = 0;

            var taste = AudienceCharacterData != null
                ? AudienceCharacterData.Taste
                : null;

            if (taste != null)
            {
                // Axis 1: TempoScale
                if (taste.tempoMatchOnFast && ctx.TempoScale > taste.preferAboveTempoScale)
                    impression += 1;
                if (taste.tempoMismatchOnSlow && ctx.TempoScale < taste.dislikeBelowTempoScale)
                    impression -= 1;

                // Axis 2: Role count (arrangement density)
                if (taste.roleCountMatchOnRich && ctx.ActiveTracks >= taste.preferAtLeastRoles)
                    impression += 1;

                // Axis 3: TimeSignature
                if (taste.preferredTimeSignatures != null &&
                    taste.preferredTimeSignatures.Count > 0 &&
                    taste.preferredTimeSignatures.Contains(ctx.TimeSignature))
                    impression += 1;
                if (taste.dislikedTimeSignatures != null &&
                    taste.dislikedTimeSignatures.Count > 0 &&
                    taste.dislikedTimeSignatures.Contains(ctx.TimeSignature))
                    impression -= 1;

                // Axis 4: Tonality
                if (taste.preferredTonalities != null &&
                    taste.preferredTonalities.Count > 0 &&
                    taste.preferredTonalities.Contains(ctx.Tonality))
                    impression += 1;
                if (taste.dislikedTonalities != null &&
                    taste.dislikedTonalities.Count > 0 &&
                    taste.dislikedTonalities.Contains(ctx.Tonality))
                    impression -= 1;
            }

            int clamped = Mathf.Clamp(impression, -2, 2);

#if UNITY_EDITOR
            // [B3 / ST-B3c-F1+] Diagnostic — surfaces per-axis breakdown for tuning.
            // Strip at B3 closure when taste profiles stabilize.
            Debug.Log(
                $"<color=#88ddff>[ResolveLoopEffect] {CharacterId} " +
                $"raw={impression} clamped={clamped} " +
                $"(TempoScale={ctx.TempoScale:0.##} TS={ctx.TimeSignature} " +
                $"Tonality={ctx.Tonality} Roles={ctx.ActiveTracks})</color>");
#endif

            return clamped;
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
        // [S3 D-F-5a] Restores the audience ability animator trigger (Kid "Tantrum")
        // lost in an earlier refactor. Mirrors MusicianBase.PlayCardOneShotAnimation:
        // fire the trigger, pause the beat animator for the animation's duration,
        // then re-enable. Fire-and-forget so it doesn't stall the action sequence.
        protected Coroutine PlayAbilityAnimation(AudienceAbilityData ability)
        {
            if (ability?.Animation == null) return null;
            return StartCoroutine(PlayAbilityAnimationRoutine(ability));
        }

        private IEnumerator PlayAbilityAnimationRoutine(AudienceAbilityData ability)
        {
            var anim = ability.Animation;

            // Animation.AnimationDuration overrides AbilityDuration when > 0.
            float delay = anim.AnimationDuration > 0f
                ? anim.AnimationDuration
                : ability.AbilityDuration;
            if (delay <= 0f) delay = 2f;

            if (anim.DisableBeatAnimator && CharacterAnimator != null)
                CharacterAnimator.enabled = false;

            if (Animator != null && !string.IsNullOrEmpty(anim.AnimatorTrigger))
            {
                Animator.ResetTrigger(anim.AnimatorTrigger);
                Animator.SetTrigger(anim.AnimatorTrigger);
            }

            yield return new WaitForSeconds(delay);

            if (anim.DisableBeatAnimator && CharacterAnimator != null)
                CharacterAnimator.enabled = true;
        }

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

            // [S3 D-F-5a] Play the ability's animator trigger (e.g. Kid "Tantrum")
            // as the ability begins; fire-and-forget so it doesn't stall actions.
            // [AUDIO-CHAR-PROFILES-2] Ability one-shot at the SAME activation point.
            // Single-source -> jitter:false (immediate, inv.10). Null clip no-ops in the
            // sink (inv.3). Fired BEFORE PlayAbilityAnimation so it is independent of the
            // animation guard: an ability with a sound and no animator trigger still plays.
            AudioManager.Instance?.PlayOneShot(NextAbility.AbilitySfx, jitter: false);

            PlayAbilityAnimation(NextAbility);

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
                    new Vector2(0f, 1f), Color.cyan);

            if (actionDelay > 0f)
                yield return new WaitForSeconds(actionDelay);

            foreach (var target in targets)
            {
                if (target == null) continue;

                float reactionDuration = GetPerTargetReactionDuration(action, target);

                var p = new CharacterActionParameters(
                    action.ActionValue, this, target, ctx,
                    duration: reactionDuration,
                    statusEffect: action.StatusEffect);

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

                case ActionTargetType.AudienceTall:
                    {
                        // [D14=B] First non-self tall audience member. Used by
                        // Kid's "Egg Him On" to target Cool Dude deterministically.
                        foreach (var a in gm.CurrentAudienceCharacterList)
                        {
                            if (a == this) continue;
                            if (a.IsTall)
                                return new List<CharacterBase>() { a };
                        }
                        return null;
                    }

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
                    action.ActionValue, this, target, ctx,
                    statusEffect: action.StatusEffect);

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