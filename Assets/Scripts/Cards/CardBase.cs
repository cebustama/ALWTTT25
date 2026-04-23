using ALWTTT.Cards;
using ALWTTT.Cards.Effects;
using ALWTTT.Characters;
using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Extentions;
using ALWTTT.Managers;
using ALWTTT.Status;
using ALWTTT.Tooltips;

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ALWTTT
{
    public class CardBase : MonoBehaviour,
        I2DTooltipTarget, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] protected Transform descriptionRoot;

        [Tooltip("Anchor Transform for card hover tooltips (keyword + status). " +
         "If unset, falls back to descriptionRoot for backward compatibility.")]
        [SerializeField] protected Transform tooltipAnchor;

        [SerializeField] protected Image cardImage;
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected Image passiveImage;
        [SerializeField] protected TextMeshProUGUI nameTextField;
        [SerializeField] protected TextMeshProUGUI descTextField;
        [SerializeField] protected TextMeshProUGUI inspirationCostTextField;
        [SerializeField] protected TextMeshProUGUI inspirationGenTextField;
        [SerializeField] protected TextMeshProUGUI typeTextField;

        public CardDefinition CardDefinition { get; private set; }
        public bool IsInactive { get; protected set; }
        public bool IsPlayable { get; protected set; } = true;
        public bool IsExhausted { get; protected set; }

        #region Encapsulation
        protected Transform CachedTransform { get; set; }
        protected WaitForEndOfFrame CachedWaitFrame { get; set; }
        #endregion

        #region Cache
        protected DeckManager DeckManager => DeckManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        #endregion

        #region Setup
        protected virtual void Awake()
        {
            CachedTransform = transform;
            CachedWaitFrame = new WaitForEndOfFrame();
        }

        public virtual void SetCard(CardDefinition targetProfile, bool isPlayable = true)
        {
            CardDefinition = targetProfile;
            IsPlayable = isPlayable;

            /*
            backgroundImage.color = 
                GameManager.GameplayData.GetCardTypeColor(CardData.CardType);*/

            cardImage.sprite = CardDefinition.CardSprite;
            typeTextField.text = CardDefinition.IsComposition ?
                "COMPOSITION" : CardDefinition.CardType.ToString();
            nameTextField.text = CardDefinition.DisplayName;
            descTextField.text = CardDefinition.GetDescription();
            inspirationCostTextField.text = CardDefinition.InspirationCost.ToString();
            inspirationGenTextField.text = CardDefinition.InspirationGenerated.ToString();
        }
        #endregion

        public virtual void Use(CharacterBase bandCharacter, CharacterBase audienceCharacter,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters)
        {
            if (!IsPlayable) return;

            if (CardDefinition.CardType == CardType.SFX)
            {
                GameManager.PersistentGameplayData.SongModifierCardsList.Add(CardDefinition);
                SpendInspiration(CardDefinition.InspirationCost);
                GenerateInspiration(CardDefinition.InspirationGenerated);
                DeckManager.OnCardPlayed(this);
            }
            else
            {
                StartCoroutine(CardUseRoutine(bandCharacter, audienceCharacter,
                    allAudienceCharacters, allBandCharacters));
            }
        }

        public virtual void UpdateDescription(MusicianBase musician)
        {
            if (musician != null)
            {
                descTextField.text = CardDefinition.GetDescription(musician.Stats);
            }
            else
            {
                descTextField.text = CardDefinition.GetDescription(null);
            }
        }

        #region Routines
        private IEnumerator CardUseRoutine(
            CharacterBase performer, CharacterBase target,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters)
        {
            Debug.Log($"<color=cyan> Playing card (coroutine)...</color>");

            SpendInspiration(CardDefinition.InspirationCost);
            GenerateInspiration(CardDefinition.InspirationGenerated);

            // Effects pipeline (CardEffectSpec via [SerializeReference])
            yield return ExecuteEffects(
                performer, target,
                allAudienceCharacters, allBandCharacters
            );

            DeckManager.OnCardPlayed(this);
        }

        private IEnumerator ExecuteEffects(
    CharacterBase performer,
    CharacterBase primaryTarget,
    List<AudienceCharacterBase> allAudienceCharacters,
    List<MusicianBase> allBandCharacters)
        {
            var payload = CardDefinition != null ? CardDefinition.Payload : null;
            var effects = payload != null ? payload.Effects : null;

            if (effects == null || effects.Count == 0)
                yield break;

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null) continue;

                if (effect is ApplyStatusEffectSpec ase)
                {
                    if (ase.stacksDelta == 0) continue;

                    if (ase.delay > 0f)
                        yield return new WaitForSeconds(ase.delay);

                    if (ase.status == null)
                    {
                        Debug.LogError(
                            $"[CardBase] ApplyStatusEffectSpec has null StatusEffectSO. Card='{CardDefinition?.name}'.");
                        continue;
                    }

                    var targets = DetermineTargets(
                        performer,
                        primaryTarget,
                        allAudienceCharacters,
                        allBandCharacters,
                        ase.targetType);

                    for (int t = 0; t < targets.Count; t++)
                    {
                        var trg = targets[t];
                        if (trg == null) continue;

                        if (trg.Statuses == null)
                        {
                            Debug.LogWarning(
                                $"[CardBase] Target '{trg.name}' has no Statuses container. Card='{CardDefinition?.name}'.");
                            continue;
                        }

                        trg.Statuses.Apply(ase.status, ase.stacksDelta);

#if UNITY_EDITOR
                        Debug.Log(
                            $"[Effects] {performer?.name} applied {ase.stacksDelta}x '{ase.status.EffectId}' to {trg.name} " +
                            $"via card '{CardDefinition?.DisplayName}'.");
#endif
                    }

                    continue;
                }


                if (effect is ModifyVibeSpec vibe)
                {
                    var targets = DetermineTargets(
                        performer,
                        primaryTarget,
                        allAudienceCharacters,
                        allBandCharacters,
                        vibe.targetType);

                    if (targets == null || targets.Count == 0)
                    {
                        Debug.LogWarning(
                            $"[CardBase] ModifyVibeSpec resolved 0 targets. Card='{CardDefinition?.name}', targetType={vibe.targetType}.");
                        continue;
                    }

                    for (int t = 0; t < targets.Count; t++)
                    {
                        var trg = targets[t];
                        if (trg == null) continue;

                        var audience = trg as AudienceCharacterBase;
                        if (audience == null || audience.Stats == null)
                        {
                            Debug.LogError(
                                $"[CardBase] ModifyVibeSpec target must be AudienceCharacterBase. Got='{trg.GetType().Name}' ('{trg.name}'). Card='{CardDefinition?.name}'.");
                            continue;
                        }

                        int baseDelta = vibe.amount;
                        int flowStacks = 0;
                        int flowBonus = 0;

                        // Strength-like MVP: Flow adds a flat bonus to *any* positive Vibe gain.
                        var gm = GigManager.Instance;
                        if (baseDelta > 0 && gm != null && gm.FlowAddsFlatVibeBonus)
                        {
                            flowStacks = ComputeBandFlowStacks(allBandCharacters);
                            flowBonus = flowStacks * gm.FlowVibeFlatBonusPerStack;
                        }

                        int finalDelta = baseDelta + flowBonus;

                        // Prefer the newer AudienceStats pathway when available.
                        if (audience.AudienceStats != null)
                        {
                            audience.AudienceStats.AddVibe(finalDelta);
                        }
                        else
                        {
                            int before = audience.Stats.CurrentVibe;
                            audience.Stats.SetCurrentVibe(before + finalDelta);
                        }

#if UNITY_EDITOR
                        Debug.Log(
                            flowBonus > 0
                                ? $"[Effects][Flow→Vibe] {performer?.name} Vibe on {audience.name}: baseΔ={baseDelta} flowStacks={flowStacks} bonus=+{flowBonus} finalΔ={finalDelta} via card '{CardDefinition?.DisplayName}'."
                                : $"[Effects] {performer?.name} modified Vibe on {audience.name}: Δ{finalDelta} via card '{CardDefinition?.DisplayName}'.");
#endif
                    }

                    continue;
                }

                if (effect is ModifyStressSpec stress)
                {
                    var targets = DetermineTargets(
                        performer,
                        primaryTarget,
                        allAudienceCharacters,
                        allBandCharacters,
                        stress.targetType);

                    if (targets == null || targets.Count == 0)
                    {
                        Debug.LogWarning(
                            $"[CardBase] ModifyStressSpec resolved 0 targets. Card='{CardDefinition?.name}', targetType={stress.targetType}.");
                        continue;
                    }

                    for (int t = 0; t < targets.Count; t++)
                    {
                        var trg = targets[t];
                        if (trg == null) continue;

                        var musician = trg as MusicianBase;
                        if (musician == null || musician.Stats == null)
                        {
                            Debug.LogError(
                                $"[CardBase] ModifyStressSpec target must be MusicianBase. Got='{trg.GetType().Name}' ('{trg.name}'). Card='{CardDefinition?.name}'.");
                            continue;
                        }

                        int delta = stress.amount;
                        if (delta == 0) continue;

                        if (delta > 0)
                        {
                            var (absorbed, applied) = musician.Stats.ApplyIncomingStressWithComposure(
                                musician.Statuses, delta);

#if UNITY_EDITOR
                            Debug.Log(
                                $"[Effects] {performer?.name} stress on {musician.name}: " +
                                $"incoming={delta} absorbed={absorbed} applied={applied} " +
                                $"via card '{CardDefinition?.DisplayName}'.");
#endif
                            continue;
                        }

                        // Negative stress removes stress directly.
                        {
                            int before = musician.Stats.CurrentStress;
                            musician.Stats.SetCurrentStress(before + delta);

#if UNITY_EDITOR
                            Debug.Log(
                                $"[Effects] {performer?.name} modified Stress on {musician.name}: {before} -> {musician.Stats.CurrentStress} (Δ{delta}) via card '{CardDefinition?.DisplayName}'.");
#endif
                        }
                    }

                    continue;
                }

                if (effect is DrawCardsSpec draw)
                {
                    if (draw.count > 0)
                    {
                        DeckManager.DrawCards(draw.count);

#if UNITY_EDITOR
                        Debug.Log(
                            $"[Effects] {performer?.name} drew {draw.count} card(s) via card '{CardDefinition?.DisplayName}'.");
#endif
                    }
                    continue;
                }

                Debug.LogWarning(
                    $"[CardBase] Unhandled CardEffectSpec type '{effect.GetType().Name}'. Card='{CardDefinition?.name}'.");
            }
        }

        private static int ComputeBandFlowStacks(List<MusicianBase> allBandCharacters)
        {
            if (allBandCharacters == null || allBandCharacters.Count == 0)
                return 0;

            int total = 0;
            for (int i = 0; i < allBandCharacters.Count; i++)
            {
                var m = allBandCharacters[i];
                if (m == null || m.Statuses == null) continue;

                // MVP mapping: Flow == DamageUpFlat.
                total += Mathf.Max(0, m.Statuses.GetStacks(CharacterStatusId.DamageUpFlat));
            }
            return total;
        }

        private List<CharacterBase> DetermineTargets(
    CharacterBase performer,
    CharacterBase targetCharacter,
    List<AudienceCharacterBase> allAudienceCharacters,
    List<MusicianBase> allBandCharacters,
    ActionTargetType targetType)
        {
            var targetList = new List<CharacterBase>();

            switch (targetType)
            {
                case ActionTargetType.Self:
                    if (performer != null)
                        targetList.Add(performer);
                    break;

                case ActionTargetType.AudienceCharacter:
                case ActionTargetType.Musician:
                    if (targetCharacter != null)
                        targetList.Add(targetCharacter);
                    break;

                case ActionTargetType.AllAudienceCharacters:
                    foreach (var enemyBase in allAudienceCharacters)
                        if (enemyBase != null && !enemyBase.IsBlocked)
                            targetList.Add(enemyBase);
                    break;

                case ActionTargetType.AllMusicians:
                    foreach (var allyBase in allBandCharacters)
                        if (allyBase != null)
                            targetList.Add(allyBase);
                    break;

                case ActionTargetType.RandomAudienceCharacter:
                    if (allAudienceCharacters != null && allAudienceCharacters.Count > 0)
                        targetList.Add(allAudienceCharacters.RandomItem());
                    break;

                case ActionTargetType.RandomMusician:
                    if (allBandCharacters != null && allBandCharacters.Count > 0)
                        targetList.Add(allBandCharacters.RandomItem());
                    break;

                default:
                    Debug.LogWarning($"[CardBase] Unhandled ActionTargetType for Effects: {targetType}");
                    break;
            }

            return targetList;
        }


        protected virtual IEnumerator DiscardRoutine(bool destroy = true)
        {
            var timer = 0f;
            transform.SetParent(DeckManager.HandController.DiscardTransform);

            var startPos = CachedTransform.localPosition;
            var endPos = Vector3.zero;

            var startScale = CachedTransform.localScale;
            var endScale = Vector3.zero;

            var startRot = CachedTransform.localRotation;
            var endRot = Quaternion.Euler(
                Random.value * 360,
                Random.value * 360,
                Random.value * 360
            );

            float discardDuration = DeckManager.HandController.DiscardDuration;

            while (timer < discardDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / discardDuration);

                CachedTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
                CachedTransform.localRotation = Quaternion.Lerp(startRot, endRot, t);
                CachedTransform.localScale = Vector3.Lerp(startScale, endScale, t);

                yield return CachedWaitFrame;
            }

            if (destroy)
                Destroy(gameObject);
        }
        #endregion

        public virtual void SetInactiveMaterialState(bool isInactive)
        {
            if (!IsPlayable) return;
            if (isInactive == this.IsInactive) return; // No change

            IsInactive = isInactive;
            passiveImage.gameObject.SetActive(isInactive);
        }

        public virtual void Discard()
        {
            // TODO: Necessary?
            if (IsExhausted) return;
            if (!IsPlayable) return;
            DeckManager.OnCardDiscarded(this);
            StartCoroutine(DiscardRoutine());
        }

        public virtual void Exhaust(bool destroy = true)
        {
            // TODO: Necessary?
            if (IsExhausted) return;
            if (!IsPlayable) return;
        }

        protected virtual void SpendInspiration(int value)
        {
            GameManager.PersistentGameplayData.CurrentInspiration -= value;
        }

        protected virtual void GenerateInspiration(int value)
        {
            GameManager.PersistentGameplayData.CurrentInspiration += value;
        }

        #region Pointer Events

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltipInfo();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            HideTooltipInfo();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"[CardBase] OnPointerDown fired. Button={eventData.button}, Card={CardDefinition?.DisplayName}");

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log($"[CardBase] Right-click detected. DetailController.Instance={UI.CardDetailViewController.Instance != null}");
                HideTooltipInfo();
                var ctrl = UI.CardDetailViewController.Instance;
                if (ctrl != null)
                    ctrl.Toggle(CardDefinition);
                return;
            }

            // Left-click: existing behavior.
            HideTooltipInfo();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            ShowTooltipInfo();
        }
        #endregion

        #region Tooltip
        protected virtual void ShowTooltipInfo()
        {
            if (!descriptionRoot) return;
            var tooltipManager = TooltipManager.Instance;
            if (tooltipManager == null) return;

            // 1) SpecialKeywords (existing behavior, now non-early-returning).
            if (CardDefinition.Keywords != null && tooltipManager.SpecialKeywordData != null)
            {
                foreach (var kw in CardDefinition.Keywords)
                {
                    var sk = tooltipManager.SpecialKeywordData.SpecialKeywordBaseList
                        .Find(x => x.SpecialKeyword == kw);
                    if (sk == null) continue;
                    tooltipManager.ShowTooltip(sk.GetContent(), sk.GetHeader());
                }
            }

            // 2) Status-effect tooltips — unique StatusEffectSOs from payload.Effects.
            if (CardDefinition.HasPayload && CardDefinition.Payload != null)
            {
                var effects = CardDefinition.Payload.Effects;
                if (effects != null)
                {
                    var seen = new HashSet<StatusEffectSO>();
                    for (int i = 0; i < effects.Count; i++)
                    {
                        if (effects[i] is ApplyStatusEffectSpec ase
                            && ase.status != null
                            && seen.Add(ase.status))
                        {
                            var header = string.IsNullOrWhiteSpace(ase.status.DisplayName)
                                ? ase.status.name : ase.status.DisplayName;
                            var body = ase.status.Description ?? string.Empty;
                            tooltipManager.ShowTooltip(body, header);
                        }
                    }
                }
            }
        }

        protected virtual void HideTooltipInfo()
        {
            HideTooltipInfo(TooltipManager.Instance);
        }

        public void ShowTooltipInfo(TooltipManager tooltipManager,
            string content, string header = "",
            Transform tooltipStaticTransform = null, Camera cam = null, float delayShow = 0)
        {
            if (!descriptionRoot) return;
            if (CardDefinition.Keywords.Count == 0) return;

            tooltipManager.ShowTooltip(
                content, header, tooltipStaticTransform, cam, delayShow);
        }

        public void HideTooltipInfo(TooltipManager tooltipManager)
        {
            tooltipManager.HideTooltip();
        }
        #endregion
    }
}