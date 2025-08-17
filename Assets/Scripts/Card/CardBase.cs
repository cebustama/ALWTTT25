using ALWTTT.Actions;
using ALWTTT.Cards;
using ALWTTT.Characters;
using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Extentions;
using ALWTTT.Managers;
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
        [SerializeField] protected Image passiveImage;
        [SerializeField] protected TextMeshProUGUI nameTextField;
        [SerializeField] protected TextMeshProUGUI descTextField;
        [SerializeField] protected TextMeshProUGUI grooveCostTextField;
        [SerializeField] protected TextMeshProUGUI grooveGenTextField;

        public CardData CardData { get; private set; }
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

        public virtual void SetCard(CardData targetProfile, bool isPlayable = true)
        {
            CardData = targetProfile;
            IsPlayable = isPlayable;

            cardImage.sprite = CardData.CardSprite;
            nameTextField.text = CardData.CardName;
            // TODO: Description
            grooveCostTextField.text = CardData.GrooveCost.ToString();
            grooveGenTextField.text = CardData.GrooveGenerated.ToString();
        }
        #endregion

        public virtual void Use(CharacterBase bandCharacter, CharacterBase audienceCharacter,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters)
        {
            if (!IsPlayable) return;

            StartCoroutine(CardUseRoutine(bandCharacter, audienceCharacter, 
                allAudienceCharacters, allBandCharacters));
        }

        #region Routines
        private IEnumerator CardUseRoutine(
            CharacterBase performer, CharacterBase target,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters)
        {
            Debug.Log($"<color=cyan> Playing card (coroutine)...</color>");

            SpendGroove(CardData.GrooveCost);

            foreach (var playerAction in CardData.CardActionDataList)
            {
                yield return new WaitForSeconds(playerAction.ActionDelay);
                
                var targetList = DetermineTargets(
                    target, allAudienceCharacters, allBandCharacters, playerAction);

                foreach (var t in targetList)
                {
                    var ctx = new CardActionContext(CardData, this);
                    var p = new CharacterActionParameters(
                        playerAction.ActionValue,
                        performer, t,
                        ctx
                    );

                    CharacterActionProcessor.GetAction(playerAction.CardActionType).DoAction(p);
                }
            }

            DeckManager.OnCardPlayed(this);
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

        protected virtual void SpendGroove(int value)
        {
            GameManager.PersistentGameplayData.CurrentGroove -= value;
        }

        private static List<CharacterBase> DetermineTargets(
            CharacterBase targetCharacter,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters,
            CharacterActionData playerAction)
        {
            List<CharacterBase> targetList = new List<CharacterBase>();

            switch (playerAction.ActionTargetType)
            {
                case ActionTargetType.AudienceCharacter:
                    targetList.Add(targetCharacter);
                    break;
                case ActionTargetType.Ally:
                    targetList.Add(targetCharacter);
                    break;
                case ActionTargetType.AllAudienceCharacters:
                    foreach (var enemyBase in allAudienceCharacters)
                        targetList.Add(enemyBase);
                    break;
                case ActionTargetType.AllAllies:
                    foreach (var allyBase in allBandCharacters)
                        targetList.Add(allyBase);
                    break;
                case ActionTargetType.RandomAudienceCharacter:
                    if (allAudienceCharacters.Count > 0)
                        targetList.Add(allAudienceCharacters.RandomItem());

                    break;
                case ActionTargetType.RandomAlly:
                    if (allBandCharacters.Count > 0)
                        targetList.Add(allBandCharacters.RandomItem());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return targetList;
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
            if (CardData.KeywordsList.Count <= 0) return; // No keywords no tooltips

            var tooltipManager = TooltipManager.Instance;
            foreach (var cardDataSpecialKeyword in CardData.KeywordsList)
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