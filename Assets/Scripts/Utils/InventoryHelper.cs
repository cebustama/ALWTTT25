using ALWTTT.Enums;
using ALWTTT.Managers;
using System;
using UnityEngine;

namespace ALWTTT.Utils
{
    // To be referenced by a button to define the type of inventory to be opened
    public class InventoryHelper : MonoBehaviour
    {
        [SerializeField] private InventoryType inventoryType;

        private UIManager UIManager => UIManager.Instance;
        private GameManager GameManager => GameManager.Instance;
        private DeckManager DeckManager => DeckManager.Instance;

        public void OpenInventory()
        {
            switch (inventoryType)
            {
                case InventoryType.CurrentDeck:
                    UIManager.OpenInventory(
                        GameManager.PersistentGameplayData.CurrentCardsList, "Deck");
                    break;
                case InventoryType.DrawPile:
                    UIManager.OpenInventory(DeckManager.DrawPile, "Draw Pile");
                    break;
                case InventoryType.DiscardPile:
                    UIManager.OpenInventory(DeckManager.DiscardPile, "Discard Pile");
                    break;
                case InventoryType.ExhaustPile:
                    UIManager.OpenInventory(DeckManager.ExhaustPile, "Exhaust Pile");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}