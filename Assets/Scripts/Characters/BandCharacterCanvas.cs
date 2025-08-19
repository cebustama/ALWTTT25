using TMPro;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    public class BandCharacterCanvas : CharacterCanvas
    {
        [Header("Character")]
        [SerializeField] private TextMeshProUGUI chrTextField;
        [SerializeField] private TextMeshProUGUI tchTextField;
        [SerializeField] private TextMeshProUGUI emtTextField;

        public void UpdateStats(int chr, int tch, int emt)
        {
            chrTextField.text = $"CHR: {chr}";
            tchTextField.text = $"TCH: {tch}";
            emtTextField.text = $"EMT: {emt}";
        }
    }
}