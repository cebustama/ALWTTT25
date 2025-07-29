using ALWTTT.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT
{
    public class CardBase : MonoBehaviour
    {
        [SerializeField] protected Image cardImage;
        [SerializeField] protected Image passiveImage;
        [SerializeField] protected TextMeshProUGUI nameTextField;
        [SerializeField] protected TextMeshProUGUI descTextField;
        [SerializeField] protected TextMeshProUGUI grooveCostTextField;
        [SerializeField] protected TextMeshProUGUI grooveGenTextField;

        public CardData CardData { get; private set; }
        public bool IsInactive { get; protected set; }
        protected Transform CachedTransform { get; set; }
        protected WaitForEndOfFrame CachedWaitFrame { get; set; }
        public bool IsPlayable { get; protected set; } = true;

        protected DeckManager DeckManager => DeckManager.Instance;

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

        public virtual void SetInactiveMaterialState(bool isInactive)
        {
            if (!IsPlayable) return;
            if (isInactive == this.IsInactive) return; // No change

            IsInactive = isInactive;
            passiveImage.gameObject.SetActive(isInactive);
        }

        #endregion
    }
}