using ALWTTT.Managers;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ALWTTT.Cards
{
    public class ChoiceCard : MonoBehaviour, 
        IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
    {
        [SerializeField] private float showScaleRate = 1.15f;
        private CardBase cardBase;
        private Vector3 initalScale;
        public Action OnCardChose;
        public GameManager GameManager => GameManager.Instance;
        public UIManager UIManager => UIManager.Instance;

        public void BuildReward(CardData cardData)
        {
            cardBase = GetComponent<CardBase>();
            initalScale = transform.localScale;
            cardBase.SetCard(cardData);
            //cardBase.UpdateCardText();
        }

        private void OnChoice()
        {
            GameManager.PersistentGameplayData.CurrentActionCards.Add(cardBase.CardData);
            UIManager.RewardCanvas.ChoicePanel.DisablePanel();

            OnCardChose?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.localScale = initalScale * showScaleRate;
        }

        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = initalScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnChoice();

        }
    }
}

