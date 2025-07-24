using UnityEngine;

namespace ALWTTT
{
    public class Card3D : CardBase
    {
        [Header("3D Settings")]
        [SerializeField] private Canvas canvas;

        public override void SetCard(CardData targetProfile, bool isPlayable = true)
        {
            base.SetCard(targetProfile, isPlayable);

            if (canvas)
            {
                canvas.worldCamera = DeckManager.HandController.Cam;    
            }
        }
    }
}