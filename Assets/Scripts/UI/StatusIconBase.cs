using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class StatusIconBase : MonoBehaviour
    {
        [SerializeField] private Image statusImage;
        [SerializeField] private TextMeshProUGUI statusValueText;

        public Image StatusImage => statusImage;
        public TextMeshProUGUI StatusValueText => statusValueText;
    }
}