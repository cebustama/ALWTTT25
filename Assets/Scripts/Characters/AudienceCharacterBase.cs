using ALWTTT.Actions;
using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Extentions;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.Music;
using ALWTTT.Status;
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

                // [TUT-R2] Publish on the false→true transition only.
                bool wasBlocked = isBlocked;
                isBlocked = value;
                if (!wasBlocked && value)
                    ALWTTT.Sensory.SensoryEventBus.Instance?.Publish(
                        new ALWTTT.Sensory.AudienceBlockedEvent(this));
            }
        }
        public int ColumnIndex { get; set; }

        // [R4 / D-R0-1=A] Taste-reveal state. Instance-scoped: a reveal lasts for
        // the rest of the gig (the audience GameObject's lifetime) and is not
        // persisted across gigs. Idempotent by design - re-casting a reveal card on
        // an already-revealed member is a deliberate silent no-op, not an error.
        public bool PreferencesRevealed { get; private set; }

        /// <summary>
        /// [R4 / D-R0-1=A] Expose this member's TastePreferences on their canvas.
        /// Called from the RevealPreferencesSpec branch in CardBase.ExecuteEffects.
        /// Data stays owned by AudienceCharacterData; presentation stays owned by
        /// AudienceCharacterCanvas - this method only connects the two.
        ///
        /// [PRES-1] Untouched by D-PRES1-3=A. The canvas changed HOW it presents
        /// (tooltip composition + icon instead of a persistent panel); the
        /// idempotence and per-gig scope guaranteed here are unchanged, which is
        /// what ST-PRES1-8 regresses.
        /// </summary>
        public void RevealPreferences()
        {
            if (PreferencesRevealed) return;
            if (AudienceCharacterData == null) return;

            PreferencesRevealed = true;
            AudienceCharacterCanvas?.ShowTastePanel(AudienceCharacterData.Taste);

            Debug.Log($"[R4][Reveal] {CharacterId} preferences revealed.");
        }

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
            return .5f;
        }

        /// <summary>
        /// [R4 / D-R4-3=A] First musician holding an active Spotlight (taunt).
        /// Guards on StatusKey in addition to the RedirectIncoming primitive, the
        /// same defensive pattern Earworm and Captivated use, so a future
        /// RedirectIncoming variant cannot inherit taunt behaviour by accident.
        /// Returns null when nobody is spotlit (the common case).
        /// </summary>
        private MusicianBase FindSpotlitMusician()
        {
            var list = GigManager != null ? GigManager.CurrentMusicianCharacterList : null;
            if (list == null || list.Count == 0) return null;

            for (int i = 0; i < list.Count; i++)
            {
                var m = list[i];
                if (m == null || m.Statuses == null) continue;

                if (!m.Statuses.TryGet(CharacterStatusId.RedirectIncoming, out var inst)) continue;
                if (inst == null || inst.Stacks <= 0 || inst.Definition == null) continue;

                if (!string.Equals(inst.Definition.StatusKey, "spotlight",
                        System.StringComparison.OrdinalIgnoreCase))
                    continue;

                return m;
            }

            return null;
        }

        /// <summary>
        /// [PRES-1 / behaviour-preserving extraction] The pre-Spotlight default
        /// single-target pick: the most-stressed musician (S5e inverted meter —
        /// higher remaining fortitude = least-stressed; semantic preserved).
        ///
        /// Extracted so the normal targeting path and the redirect FLOATER use
        /// literally the same selector. If they were two copies they could drift,
        /// and the floater would start naming a musician the game would not
        /// actually have hit — a lie in the UI is worse than no UI.
        /// Pure: no side effects, no RNG, safe to call for presentation.
        /// </summary>
        private static MusicianBase SelectDefaultMusicianTarget(
            List<MusicianBase> list)
        {
            if (list == null || list.Count == 0) return null;

            MusicianBase best = null;
            foreach (var m in list)
            {
                // [S5e] Inverted meter: higher remaining fortitude = least-stressed (semantic preserved)
                if (best == null ||
                    m.MusicianStats.CurrentStress > best.MusicianStats.CurrentStress)
                    best = m;
            }

#if ALWTTT_DEV
            // [PRES-1b T1-diag] Makes the selector's ranking auditable when a smoke
            // setup depends on who the default target is.
            var sb = new System.Text.StringBuilder("[PRES-1][Selector] candidates: ");
            foreach (var m in list)
                sb.Append($"{m.name}={m.MusicianStats.CurrentStress} ");
            sb.Append($"-> winner='{best?.name}'");
            Debug.Log(sb.ToString());
#endif

            return best;
        }

        /// <summary>
        /// [PRES-1 / D-PRES1-2=A] Presentation-only redirect announcement.
        ///
        /// Skips the visual no-op: when the default target already WAS the spotlit
        /// musician, nothing was redirected and a "-&gt; himself" floater would be
        /// noise. Publishing never affects targeting — the caller has already
        /// decided and returned its list shape.
        /// </summary>
        private void PublishSpotlightRedirect(
    MusicianBase protectedMusician, MusicianBase original)
        {
            if (protectedMusician == null) return;

            // [PRES-1b T1-diag] The suppression used to be a silent return, which made
            // a legitimate no-op indistinguishable from a broken presentation path.
            if (original == protectedMusician)
            {
                Debug.Log(
                    $"[PRES-1][Spotlight] {CharacterId}: redirect SUPPRESSED (visual " +
                    $"no-op) — default target ALREADY was '{protectedMusician.name}' " +
                    $"(CurrentStress={protectedMusician.MusicianStats.CurrentStress}). " +
                    $"No event published, no floater expected.");
                return;
            }

            Debug.Log(
                $"[PRES-1][Spotlight] {CharacterId}: original=" +
                $"'{(original != null ? original.name : "RANDOM/none")}' -> protected=" +
                $"'{protectedMusician.name}'. Publishing SpotlightRedirectEvent.");

            ALWTTT.Sensory.SensoryEventBus.Instance?.Publish(
                new ALWTTT.Sensory.SpotlightRedirectEvent(
                    this, original, protectedMusician));
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
                        // [R4 / Spotlight] Taunt redirect. Placed at the funnel rather
                        // than per-ability so every present and future single-target
                        // musician ability respects the taunt with no per-ability edit.
                        var spotlitDirected = FindSpotlitMusician();
                        if (spotlitDirected != null)
                        {
                            Debug.Log(
                                $"[R4][Spotlight] {CharacterId}: Musician target redirected " +
                                $"-> '{spotlitDirected.name}'.");

                            // [PRES-1 / D-PRES1-2=A] Presentation only. The same pure
                            // selector the normal path uses names the would-be target,
                            // so the floater cannot contradict the game's own choice.
                            PublishSpotlightRedirect(spotlitDirected,
                                SelectDefaultMusicianTarget(gm.CurrentMusicianCharacterList));

                            return new List<CharacterBase>() { spotlitDirected };
                        }

                        var list = gm.CurrentMusicianCharacterList;
                        if (list.Count == 0) return null;

                        // [PRES-1] Same selection as before, now single-sourced.
                        return new List<CharacterBase>()
                            { SelectDefaultMusicianTarget(list) };
                    }

                case ActionTargetType.RandomMusician:
                    {
                        // [R4 / Spotlight] Same redirect as the Musician branch: a
                        // taunt that random targeting could dodge would not read as a
                        // taunt to the player.
                        var spotlitRandom = FindSpotlitMusician();
                        if (spotlitRandom != null)
                        {
                            Debug.Log(
                                $"[R4][Spotlight] {CharacterId}: RandomMusician target redirected " +
                                $"-> '{spotlitRandom.name}'.");

                            // [PRES-1 / D-PRES1-2=A] Original target is passed as NULL
                            // on purpose: it does not exist until Random.Range is
                            // called, and rolling for presentation would consume global
                            // RNG state and shift every later roll in the gig. The
                            // floater anchors on the protected musician instead.
                            PublishSpotlightRedirect(spotlitRandom, null);

                            return new List<CharacterBase>() { spotlitRandom };
                        }

                        var list = gm.CurrentMusicianCharacterList;
                        if (list.Count == 0) return null;
                        var index = Random.Range(0, list.Count);
                        return new List<CharacterBase>() { list[index] };
                    }

                case ActionTargetType.AllMusicians:
                    // [R4 / SSoT_Status_Effects §5.9] NEVER redirected. A taunt that
                    // absorbed an everyone-hits attack would delete the attack.
                    // [PRES-1] Consequently no redirect event can fire here either
                    // (ST-PRES1-9 regresses both halves).
                    return new List<CharacterBase>(gm.CurrentMusicianCharacterList);

                case ActionTargetType.AudienceCharacter:
                    {
                        AudienceCharacterBase best = null;
                        foreach (var a in gm.CurrentAudienceCharacterList)
                        {
                            if (a == this) continue;
                            // [S5e] Inverted meter: higher remaining resistance = least-persuaded (semantic preserved)
                            if (best == null || a.AudienceStats.CurrentVibe > best.AudienceStats.CurrentVibe)
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

        // [S5f / E-lite] Tracks whether THIS hover opened the Blocked tooltip,
        // so exit only hides what enter showed (avoids clobbering the canvas
        // ability tooltip, which lives on a separate hover surface).
        private bool _blockedTooltipShown;

        protected override void OnPointerEnter()
        {
            base.OnPointerEnter();
            AudienceCharacterCanvas.ShowContextual();

            // [S5f / E-lite] Explain the "oscurito" tint on sprite hover.
            if (IsBlocked && AudienceCharacterCanvas != null)
            {
                AudienceCharacterCanvas.ShowBlockedTooltip();
                _blockedTooltipShown = true;
            }
        }

        protected override void OnPointerExit()
        {
            base.OnPointerExit();
            AudienceCharacterCanvas.HideContextual();

            if (_blockedTooltipShown)
            {
                if (AudienceCharacterCanvas != null)
                    AudienceCharacterCanvas.HideBlockedTooltip();
                _blockedTooltipShown = false;
            }
        }
    }
}