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
                    UIManager.OpenCardsInventory(
                        GameManager.PersistentGameplayData.CurrentActionCards, "Deck");
                    break;
                case InventoryType.DrawPile:
                    UIManager.OpenCardsInventory(DeckManager.DrawPile, "Draw Pile");
                    break;
                case InventoryType.DiscardPile:
                    UIManager.OpenCardsInventory(DeckManager.DiscardPile, "Discard Pile");
                    break;
                case InventoryType.ExhaustPile:
                    UIManager.OpenCardsInventory(DeckManager.ExhaustPile, "Exhaust Pile");
                    break;
                case InventoryType.BandSongs:
                    UIManager.OpenSongsInventory(
                        GameManager.PersistentGameplayData.CurrentSongList, "Song List");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}