using ALWTTT.Data;
using ALWTTT.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Audio
{
    public class ButtonSoundPlayer : MonoBehaviour
    {
        [SerializeField] private SoundProfileData soundProfileData;

        private Button btn;
        private SoundProfileData SoundProfileData => soundProfileData;
        private AudioManager AudioManager => AudioManager.Instance;

        private void Awake()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(PlayButton);
        }

        public void PlayButton() =>
            AudioManager.PlayOneShotButton(SoundProfileData.GetRandomClip());
    }
}