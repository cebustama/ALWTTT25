using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class MusicianMapStatusUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI musicianNameText;
        [SerializeField] private Image musicianStressImage;

        public void SetName(string musicianName)
        {
            musicianNameText.text = musicianName;
        }

        public void SetStress(int current, int max)
        {
            musicianStressImage.fillAmount = (float)current / max;
        }
    }
}