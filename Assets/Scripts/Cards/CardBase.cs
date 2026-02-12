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

using System;
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

                        int before = audience.Stats.CurrentVibe;
                        audience.Stats.SetCurrentVibe(before + vibe.amount);

#if UNITY_EDITOR
                        Debug.Log(
                            $"[Effects] {performer?.name} modified Vibe on {audience.name}: {before} -> {audience.Stats.CurrentVibe} (Δ{vibe.amount}) via card '{CardDefinition?.DisplayName}'.");
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

                        // Positive stress is mitigated by Composure (CharacterStatusId.TempShieldTurn) first.
                        if (delta > 0)
                        {
                            int remaining = delta;
                            int absorbed = 0;

                            if (musician.Statuses != null &&
                                musician.Statuses.TryGet(CharacterStatusId.TempShieldTurn, out var compInst) &&
                                compInst != null)
                            {
                                int compStacks = compInst.Stacks;
                                absorbed = Mathf.Min(compStacks, remaining);

                                if (absorbed > 0)
                                {
                                    // Consume stacks directly.
                                    musician.Statuses.Apply(compInst.Definition, -absorbed);
                                    remaining -= absorbed;
                                }
                            }

#if UNITY_EDITOR
                            if (absorbed > 0)
                            {
                                Debug.Log(
                                    $"[Effects] {performer?.name} stress mitigated by Composure on {musician.name}: absorbed {absorbed}/{delta}. Remaining={remaining}. Card='{CardDefinition?.DisplayName}'.");
                            }
#endif

                            if (remaining > 0)
                            {
                                int before = musician.Stats.CurrentStress;
                                musician.Stats.SetCurrentStress(before + remaining);

#if UNITY_EDITOR
                                Debug.Log(
                                    $"[Effects] {performer?.name} modified Stress on {musician.name}: {before} -> {musician.Stats.CurrentStress} (Δ+{remaining}) via card '{CardDefinition?.DisplayName}'.");
#endif
                            }

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
                    // No invento API de DeckManager aquí. Deja stub + log.
                    Debug.LogWarning(
                        $"[CardBase] DrawCardsSpec present (count={draw.count}) but runtime execution not implemented yet. " +
                        $"Card='{CardDefinition?.name}'.");
                    continue;
                }

                Debug.LogWarning(
                    $"[CardBase] Unhandled CardEffectSpec type '{effect.GetType().Name}'. Card='{CardDefinition?.name}'.");
            }
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
            if (CardDefinition.Keywords.Count <= 0) return; // No keywords no tooltips

            var tooltipManager = TooltipManager.Instance;
            foreach (var cardDataSpecialKeyword in CardDefinition.Keywords)
            {
                var specialKeyword = tooltipManager
                    .SpecialKeywordData.SpecialKeywordBaseList
                        .Find(x => x.SpecialKeyword == cardDataSpecialKeyword);

                if (specialKeyword != null)
                    ShowTooltipInfo(tooltipManager, specialKeyword.GetContent(),
                        specialKeyword.GetHeader(), descriptionRoot,
                        DeckManager ? DeckManager.HandController.Cam : Camera.main);
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