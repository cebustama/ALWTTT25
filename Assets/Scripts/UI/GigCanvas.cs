using ALWTTT.Enums;
using ALWTTT.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class GigCanvas : CanvasBase
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI drawPileTextField;
        [SerializeField] private TextMeshProUGUI discardPileTextField;
        [SerializeField] private TextMeshProUGUI exhaustPileTextField;
        [SerializeField] private TextMeshProUGUI grooveTextField;

        [Header("Buttons")]
        [SerializeField] private Button endTurnButton;

        private void OnEnable()
        {
            endTurnButton.onClick.AddListener(EndTurn);
        }

        private void OnDisable()
        {
            endTurnButton.onClick.RemoveListener(EndTurn);
        }

        public void SetPileTexts()
        {
            drawPileTextField.text = $"{DeckManager.DrawPile.Count.ToString()}";
            discardPileTextField.text = $"{DeckManager.DiscardPile.Count.ToString()}";
            exhaustPileTextField.text = $"{DeckManager.ExhaustPile.Count.ToString()}";
            grooveTextField.text = $"{GameManager.PersistentGameplayData.CurrentGroove.ToString()}" +
                $"/{GameManager.PersistentGameplayData.MaxGroove.ToString()}";
        }

        private void EndTurn()
        {
            if (GigManager.CurrentGigPhase == GigPhase.PlayerTurn)
            {
                GigManager.EndTurn();
            }
        }
    }
}