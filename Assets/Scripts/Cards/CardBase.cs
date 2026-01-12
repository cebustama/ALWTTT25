using ALWTTT.Cards;
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

            // Status actions pipeline (StatusEffectActionData)
            yield return ExecuteStatusActions(
                performer, target,
                allAudienceCharacters, allBandCharacters
            );

            DeckManager.OnCardPlayed(this);
        }

        private IEnumerator ExecuteStatusActions(
            CharacterBase performer,
            CharacterBase primaryTarget,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters)
        {
            var payload = CardDefinition != null ? CardDefinition.Payload : null;
            var statusActions = payload != null ? payload.StatusActions : null;

            if (statusActions == null || statusActions.Count == 0)
                yield break;

            // Prefer performer catalogue (authoritative “owner”), fallback to target.
            var catalogue = (performer != null ? performer.StatusCatalogue : null)
                            ?? (primaryTarget != null ? primaryTarget.StatusCatalogue : null);

            if (catalogue == null)
            {
                Debug.LogWarning(
                    $"[CardBase] Card '{CardDefinition?.name}' has StatusActions but no StatusEffectCatalogueSO found. " +
                    $"Assign it on the performer/target CharacterBase (StatusCatalogue field).");
                yield break;
            }

            for (int i = 0; i < statusActions.Count; i++)
            {
                var sa = statusActions[i];
                if (sa == null) continue;
                if (sa.StacksDelta == 0) continue;

                if (sa.Delay > 0f)
                    yield return new WaitForSeconds(sa.Delay);

                // Resolve targets using same ActionTargetType rules.
                var targets = DetermineTargets(
                    primaryTarget,
                    allAudienceCharacters,
                    allBandCharacters,
                    sa.TargetType);

                // Resolve definition from catalogue (source of truth).
                StatusEffectSO def;
                try
                {
                    def = catalogue.GetOrThrow(sa.EffectId);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"[CardBase] Missing StatusEffectSO for id '{sa.EffectId}' in catalogue '{catalogue.name}'. " +
                        $"Card='{CardDefinition?.name}'. Exception: {e.Message}");
                    continue;
                }

                for (int t = 0; t < targets.Count; t++)
                {
                    var trg = targets[t];
                    if (trg == null) continue;

                    if (trg.Statuses == null)
                    {
                        Debug.LogWarning(
                            $"[CardBase] Target '{trg.name}' has no Statuses container. " +
                            $"Did Step 3 (CharacterBase.Statuses) run?");
                        continue;
                    }

                    trg.Statuses.Apply(def, sa.StacksDelta);

#if UNITY_EDITOR
                    Debug.Log(
                        $"[StatusActions] {performer?.name} applied {sa.StacksDelta}x '{def.EffectId}' to {trg.name} " +
                        $"via card '{CardDefinition?.DisplayName}'.");
#endif
                }
            }
        }

        // Overload target resolution for status actions (ActionTargetType only)
        private List<CharacterBase> DetermineTargets(
            CharacterBase targetCharacter,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters,
            ActionTargetType targetType)
        {
            var targetList = new List<CharacterBase>();

            switch (targetType)
            {
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
                    // If you later add more ActionTargetType values, this will force you to decide behavior.
                    Debug.LogWarning($"[CardBase] Unhandled ActionTargetType for StatusActions: {targetType}");
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