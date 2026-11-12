using ALWTTT.Actions;
using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Extentions;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.Music;
using ALWTTT.Status;
using ALWTTT.Tooltips;
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

        // [LEGIB-1b / D5=A] The single musician this ability will hit, fixed when the
        // intention is SHOWN, not when it fires. Before S4 the target only existed at fire
        // time (RandomMusician rolled then, Musician read the stress ranking then), so the
        // screen could not say "a quién". Now: one pick per ability, shared by all its
        // Musician / RandomMusician actions (Heckle exposes and hits the same musician).
        // Spotlight still overrides at projection AND fire time (taunt rule, §5.9).
        // AllMusicians is not locked: it is everyone, at fire time, as before.
        private MusicianBase _lockedMusician;
        private readonly List<MusicianBase> _projectedMusicians = new();

        /// <summary>[LEGIB-1b] The ability shown as intention (null when none).</summary>
        public AudienceAbilityData PendingAbility => NextAbility;

        /// <summary>
        /// [LEGIB-1b / D5=A] Roll or pick the ability's musician now. Consumes RNG once per
        /// shown intention instead of once per fired action: same count for today's data,
        /// moved earlier in the turn.
        /// </summary>
        private void LockMusicianTarget(AudienceAbilityData ability)
        {
            _lockedMusician = null;
            if (ability?.ActionList == null || GigManager == null) return;

            var list = GigManager.CurrentMusicianCharacterList;
            if (list == null || list.Count == 0) return;

            bool anyFixed = false, anyRandom = false;
            foreach (var a in ability.ActionList)
            {
                if (a == null) continue;
                if (a.ActionTargetType == ActionTargetType.Musician) anyFixed = true;
                else if (a.ActionTargetType == ActionTargetType.RandomMusician) anyRandom = true;
            }

            if (anyFixed)
            {
                _lockedMusician = SelectDefaultMusicianTarget(list);
                if (_lockedMusician != null)
                    Debug.Log($"[S7] Weakest | {CharacterId} → {_lockedMusician.name} | " +
                              string.Join(", ", list.ConvertAll(m => m == null ? "-" : $"{m.name}={BreakdownDistance(m):0.#}")));
            }
            else if (anyRandom)
                _lockedMusician = list[Random.Range(0, list.Count)];
        }

        /// <summary>Locked pick if still in the band; otherwise the pure default selector
        /// (Musician) or null (RandomMusician: unknown until fire, legacy roll there).</summary>
        private MusicianBase CurrentLockedMusician(ActionTargetType t)
        {
            var list = GigManager != null ? GigManager.CurrentMusicianCharacterList : null;
            if (list == null || list.Count == 0) return null;
            if (_lockedMusician != null && list.Contains(_lockedMusician)) return _lockedMusician;
            return t == ActionTargetType.Musician ? SelectDefaultMusicianTarget(list) : null;
        }

        /// <summary>
        /// [LEGIB-1b] Musicians this action will reach, WITHOUT side effects (no RNG, no
        /// bus): the same answer ResolveTargetsFor gives at fire time. Empty for actions
        /// that do not target musicians. The returned list is reused; copy it to keep it.
        /// </summary>
        public List<MusicianBase> GetProjectedMusicianTargets(CharacterActionData action)
        {
            _projectedMusicians.Clear();
            if (action == null || GigManager == null) return _projectedMusicians;

            var list = GigManager.CurrentMusicianCharacterList;
            if (list == null) return _projectedMusicians;

            switch (action.ActionTargetType)
            {
                case ActionTargetType.AllMusicians:
                    _projectedMusicians.AddRange(list);
                    break;
                case ActionTargetType.Musician:
                case ActionTargetType.RandomMusician:
                    {
                        var pick = FindSpotlitMusician() ?? CurrentLockedMusician(action.ActionTargetType);
                        if (pick != null) _projectedMusicians.Add(pick);
                        break;
                    }
            }
            return _projectedMusicians;
        }

        /// <summary>
        /// [LEGIB-1b] Deterministic audience targets (Self, AudienceTall, AudienceCharacter,
        /// all) for projecting audience-on-audience statuses (Hyped). Random targets are left
        /// out: they are unknown until fire time.
        /// </summary>
        public void GetProjectedAudienceTargets(CharacterActionData action, List<AudienceCharacterBase> into)
        {
            into.Clear();
            if (action == null || GigManager == null) return;

            switch (action.ActionTargetType)
            {
                case ActionTargetType.Self:
                case ActionTargetType.AllAudienceCharacters:
                case ActionTargetType.AudienceTall:
                case ActionTargetType.AudienceLeader: // [S7]
                case ActionTargetType.AudienceCharacter:
                    {
                        // These branches of ResolveTargetsFor are pure (no RNG, no bus).
                        var r = ResolveTargetsFor(action);
                        if (r != null)
                            foreach (var c in r)
                                if (c is AudienceCharacterBase a) into.Add(a);
                        break;
                    }
            }
        }

        /// <summary>
        /// [S7 / Croar] 2 when another member sings this ability's duet with us: its pending
        /// ability has the same DuetKey, it can act (active, unconvinced, not stunned) and one of
        /// its musician-targeting actions reaches <paramref name="target"/>; otherwise 1.
        /// Pure (no RNG, no bus): StressProjector and the real hit both call it, so the bar and
        /// the hit agree. Targets are locked at intention time, so both frogs keep the same
        /// answer through the audience turn (the first to croak still counts for the second).
        /// </summary>
        public int DuetMultiplier(CharacterActionData action, MusicianBase target)
            => FindDuetPartner(action, target) != null ? 2 : 1;

        public AudienceCharacterBase FindDuetPartner(CharacterActionData action, MusicianBase target)
        {
            if (action == null || target == null || action.CardActionType != CharacterActionType.AddStress) return null;
            var key = NextAbility != null ? NextAbility.DuetKey : null;
            if (string.IsNullOrEmpty(key) || GigManager == null) return null;
            var audience = GigManager.CurrentAudienceCharacterList;
            if (audience == null) return null;

            foreach (var b in audience)
            {
                if (b == null || b == this || !b.gameObject.activeInHierarchy) continue;
                if (b.Stats == null || b.Stats.IsConvinced || b.Stats.IsStunned) continue;
                var ab = b.PendingAbility;
                if (ab == null || ab.ActionList == null || ab.DuetKey != key) continue;
                foreach (var other in ab.ActionList)
                {
                    if (other == null || other.CardActionType != CharacterActionType.AddStress) continue;
                    if (b.GetProjectedMusicianTargets(other).Contains(target)) return b;
                }
            }
            return null;
        }

        public AudienceCharacterData AudienceCharacterData => audienceCharacterData;
        public AudienceCharacterCanvas AudienceCharacterCanvas => characterCanvas;
        public bool IsTall => AudienceCharacterData.IsTall;

        private bool isBlocked;

        public bool IsBlocked
        {
            get => isBlocked;
            set
            {
                // [CONV-TINT-1] Sprite colour is composed in RefreshSpriteTint (below).

                // [TUT-R2] Publish on the false→true transition only.
                bool wasBlocked = isBlocked;
                isBlocked = value;
                RefreshStatePresentation(); // [CONV-TINT-1b] tint + canvas; after the field is updated
                if (!wasBlocked && value)
                    ALWTTT.Sensory.SensoryEventBus.Instance?.Publish(
                        new ALWTTT.Sensory.AudienceBlockedEvent(this));
            }
        }
        public int ColumnIndex { get; set; }


        // [CONV-TINT-1 / D-T1] Presentation rule: once convinced, "blocked" stops being
        // shown. IsBlocked keeps its gameplay truth (occlusion, AoE targeting, Vibe gate);
        // only the sprite tint and the "oscurito" tooltip read the presented predicate.
        private bool IsConvincedPresented => stats != null && stats.IsConvinced;
        private bool IsBlockedPresented => IsBlocked && !IsConvincedPresented;

        /// <summary>
        /// [CONV-TINT-1 / D-T1] Sole writer of this member's SpriteRenderer.color.
        /// Recomputes from state instead of restoring a fixed colour, so the call order
        /// between the IsBlocked setter, OnConvinced and Dev resets does not matter.
        /// Priority: convinced > blocked > plain.
        /// </summary>
        private void RefreshSpriteTint()
        {
            if (SpriteRenderer == null) return;

            if (IsConvincedPresented)
            {
                var pres = GigManager != null ? GigManager.Presentation : null;
                SpriteRenderer.color = pres != null
                    ? pres.ConvincedSpriteTint
                    : GigPresentationSO.DefaultConvincedSpriteTint;
                return;
            }

            SpriteRenderer.color = IsBlocked ? obscuredColor : Color.white;
        }

        /// <summary>
        /// [CONV-TINT-1b] Single refresh of everything the convinced state changes on
        /// screen: sprite tint + canvas concealment. Runs from the IsBlocked setter
        /// because RecalculateAudienceObstructions drives that setter on every member
        /// after any convince or Dev un-convince; that funnel keeps both in step.
        /// </summary>
        private void RefreshStatePresentation()
        {
            RefreshSpriteTint();

            if (AudienceCharacterCanvas != null)
                AudienceCharacterCanvas.SetConvincedHidden(IsConvincedPresented);
        }

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

            // [GIG-UI-1] Hover column: same content from icon and body, placed at the
            // top-right corner of the body collider (the box cards target), so every
            // member's column sits beside its own sprite whatever its canvas offsets.
            AudienceCharacterCanvas.BlockedQuery = () => IsBlockedPresented;
            var box = GetComponent<BoxCollider>();
            if (box != null)
            {
                var anchor = new GameObject("TooltipAnchor").transform;
                anchor.SetParent(transform, false);
                anchor.localPosition = box.center + new Vector3(box.size.x * 0.5f, box.size.y * 0.5f, 0f);
                AudienceCharacterCanvas.SetTooltipAnchor(anchor);
            }

            AudienceCharacterCanvas.HideContextual();
        }

        protected void OnConvinced()
        {
            Debug.Log($"<color=green>{CharacterId} CONVINCED!");

            GigManager.RecalculateAudienceObstructions();

            // [CONV-TINT-1] Explicit, so the tint does not depend on the recalculation
            // happening to touch this member's IsBlocked setter. [CONV-TINT-1b] tint + canvas.
            RefreshStatePresentation();

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

                // Axis 5 [S7 / D4=B]: track roles. One point per side, however many
                // listed roles are playing, so the axis weighs the same as the others.
                if (taste.preferredRoles != null && taste.preferredRoles.Count > 0)
                    foreach (var role in taste.preferredRoles)
                        if (ctx.HasRole(role)) { impression += 1; break; }
                if (taste.dislikedRoles != null && taste.dislikedRoles.Count > 0)
                    foreach (var role in taste.dislikedRoles)
                        if (ctx.HasRole(role)) { impression -= 1; break; }
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

            // [LEGIB-1b / D5=A] Fix the target now, so the screen can name it.
            LockMusicianTarget(NextAbility);

            // [LEGIB-1b / D7 -> icons] The "xN" of ActionList[0] is gone: it showed the FIRST
            // action's value (Heckle read "x1", its Exposed stack, not its 2 Stress; F-T6-5).
            // The number and the target icons now come from the stress projection, which
            // GigManager refreshes every frame of the player turn (SetIntentSummary).
            AudienceCharacterCanvas.ClearIntentSummary();

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

            // [LEGIB-1b] One floater per ability with its PLAYER name (glossary, by abilityId),
            // replacing the per-action enum name ("AddStress", "ApplyStatusEffect") that leaked
            // developer text onto the screen.
            FxManager.Instance?.SpawnFloatingText(
                TextSpawnRoot,
                GameText.ConceptName(NextAbility.AbilityId),
                new Vector2(0f, 1f), Color.cyan);

            var ctx = new AudienceActionContext();

            foreach (var action in NextAbility.ActionList)
            {
                if (action == null) continue;

                var targets = ResolveTargetsFor(action);

                // [S7] Once per fire, never in the per-frame projection.
                if (action.ActionTargetType == ActionTargetType.AudienceLeader)
                    Debug.Log(targets == null || targets.Count == 0
                        ? $"[S7] {CharacterId} → leader: none unconvinced, {action.CardActionType} fizzles"
                        : $"[S7] {CharacterId} → leader {targets[0].name}: {action.CardActionType}");

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

            if (actionDelay > 0f)
                yield return new WaitForSeconds(actionDelay);

            foreach (var target in targets)
            {
                if (target == null) continue;

                float reactionDuration = GetPerTargetReactionDuration(action, target);

                // [S7 / Croar] Duet doubles the base before Hyped (AddStressAction adds Hyped
                // on top), the same order StressProjector uses.
                float value = action.ActionValue;
                var partner = FindDuetPartner(action, target as MusicianBase);
                if (partner != null)
                {
                    value *= 2f;
                    Debug.Log($"[S7] Croak duet | {CharacterId} + {partner.CharacterId} → {target.name} | {action.ActionValue} ×2 = {value}");
                }

                var p = new CharacterActionParameters(
                    value, this, target, ctx,
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
        /// [PRES-1 extraction · R5 / D-R5-2=A] The default single-target pick:
        /// the musician CLOSEST TO BREAKDOWN — the LOWEST remaining fortitude
        /// (CurrentStress is the S5e-inverted HP-style pool; 0 = Breakdown).
        ///
        /// The pre-R5 comparator picked the HIGHEST CurrentStress. That was
        /// correct pre-S5e and was silently inverted by the S5e storage flip:
        /// this method reads CurrentStress RAW, outside the direction-agnostic
        /// AddStress/HealStress API that S5e was careful to protect, so the
        /// '>' survived the flip and started meaning "the healthiest musician".
        /// Restored at R5 (finding F-PRES1b-1). Absolute, not proportional:
        /// Breakdown fires at 0 absolute, so remaining points ARE the distance
        /// to collapse.
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

            // [S7 / D3=A] "Closest to Breakdown" now counts what the musician is wearing:
            // hits it can take = (remaining fortitude + Composure) / (1 + Exposed × mult).
            // Composure absorbs first and Exposed amplifies what is left, the same order as
            // ApplyIncomingStressWithComposure. Ties keep band order (first wins), as before.
            MusicianBase best = null;
            float bestScore = float.MaxValue;
            foreach (var m in list)
            {
                if (m == null) continue;
                float score = BreakdownDistance(m);
                if (best == null || score < bestScore)
                {
                    best = m;
                    bestScore = score;
                }
            }

            // [R5-pre, 2026-08-11] The ALWTTT_DEV [PRES-1][Selector] ranking log was
            // REMOVED here once ST-R5pre-1..4 passed (Task 5). It fired twice per
            // redirect and existed only to build those smoke setups. The two
            // [PRES-1][Spotlight] logs in PublishSpotlightRedirect stay: they are
            // load-bearing evidence for ST-PRES1-6.

            return best;
        }

        /// <summary>[S7 / D3=A] Pure: effective distance to Breakdown (lower = weaker).</summary>
        public static float BreakdownDistance(MusicianBase m)
        {
            if (m == null || m.MusicianStats == null) return float.MaxValue;
            float remaining = m.MusicianStats.CurrentStress;
            int composure = 0, exposed = 0;
            if (m.Statuses != null)
            {
                composure = Mathf.Max(0, m.Statuses.GetStacks(CharacterStatusId.TempShieldTurn));
                exposed = Mathf.Max(0, m.Statuses.GetStacks(CharacterStatusId.DamageTakenUpFlat));
            }
            float mult = m.Stats != null ? m.Stats.ExposedStressMultiplierPerStack : 0f;
            return (remaining + composure) / (1f + exposed * mult);
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
                        // [LEGIB-1b / D5=A] The target was fixed when the intention was
                        // shown (CurrentLockedMusician); only Spotlight changes it now.
                        var locked = CurrentLockedMusician(ActionTargetType.Musician);

                        var spotlitDirected = FindSpotlitMusician();
                        if (spotlitDirected != null)
                        {
                            Debug.Log(
                                $"[R4][Spotlight] {CharacterId}: Musician target redirected " +
                                $"-> '{spotlitDirected.name}'.");

                            // [PRES-1 / D-PRES1-2=A] Presentation only. The original is
                            // the locked pick, the same one the screen was showing.
                            PublishSpotlightRedirect(spotlitDirected, locked);

                            return new List<CharacterBase>() { spotlitDirected };
                        }

                        if (locked == null) return null;
                        return new List<CharacterBase>() { locked };
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

                            // [LEGIB-1b / D5=A] The roll now happens when the intention is
                            // shown, so the original target exists and is passed (it was
                            // NULL before S4 because rolling here would have consumed RNG).
                            PublishSpotlightRedirect(spotlitRandom,
                                CurrentLockedMusician(ActionTargetType.RandomMusician));

                            return new List<CharacterBase>() { spotlitRandom };
                        }

                        var lockedRandom = CurrentLockedMusician(ActionTargetType.RandomMusician);
                        if (lockedRandom != null)
                            return new List<CharacterBase>() { lockedRandom };

                        // Legacy fallback: no lock (the locked musician left the band).
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

                case ActionTargetType.AudienceLeader:
                    {
                        // [S7 / D2=A] The boss, while still unconvinced. A convinced boss
                        // leaves his minions with no one to cheer: the action fizzles.
                        foreach (var a in gm.CurrentAudienceCharacterList)
                        {
                            if (a == this || a.AudienceCharacterData == null) continue;
                            if (!a.AudienceCharacterData.IsLeader) continue;
                            if (a.AudienceStats.IsConvinced) return null;
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

            // [GIG-UI-1] Body hover = the full column (Blocked legend when tinted, ability,
            // revealed tastes). The canvas owns the rebuild; see RefreshHoverTooltip.
            if (AudienceCharacterCanvas != null)
            {
                AudienceCharacterCanvas.SetBodyHover(true);
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
                    AudienceCharacterCanvas.SetBodyHover(false);
                _blockedTooltipShown = false;
            }
        }
    }
}