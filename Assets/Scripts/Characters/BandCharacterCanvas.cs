using TMPro;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    public class BandCharacterCanvas : CharacterCanvas
    {
        [Header("Character")]
        [SerializeField] private CanvasGroup statsCanvasGroup;
        [SerializeField] private TextMeshProUGUI chrTextField;
        [SerializeField] private TextMeshProUGUI tchTextField;
        [SerializeField] private TextMeshProUGUI emtTextField;

        public void UpdateStats(int chr, int tch, int emt)
        {
            chrTextField.text = $"CHR: {chr}";
            tchTextField.text = $"TCH: {tch}";
            emtTextField.text = $"EMT: {emt}";
        }

        public override void ShowContextual()
        {
            base.ShowContextual();

            statsCanvasGroup.alpha = 1;
        }

        public override void HideContextual()
        {
            base.HideContextual();

            statsCanvasGroup.alpha = 0;
        }

        public void SetCurrentStress(int current, int max, float duration)
        {
            healthBar?.SetCurrentValue(current, max, duration);
        }
    }
}