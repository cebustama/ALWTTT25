using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT
{
    public class HandController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform drawTransform;
        [SerializeField] private Transform discardTransform;
        [SerializeField] private Transform exhaustTransform;
        [SerializeField] private Camera cam;

        [Header("Debug")]
        [SerializeField] private List<CardBase> hand; // cards currently in hand

        #region Encapsulation

        public Transform DrawTransform => drawTransform;
        public Camera Cam => cam;
        public List<CardBase> Hand => hand;

        #endregion

        /// <summary>
        /// Adds a card to the hand. Optional param to insert it at a given index.
        /// </summary>
        public void AddCardToHand(CardBase card, int index = -1)
        {
            if (index < 0)
            {
                // Add to end
                hand.Add(card);
                index = hand.Count - 1;
            }
            else
            {
                // Insert at index
                hand.Insert(index, card);
            }
        }
    }
}