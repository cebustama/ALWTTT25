using ALWTTT.Data;
using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ALWTTT.Managers
{
    public class AudioManager : MonoBehaviour
    {
        private AudioManager() { }
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource buttonSource;

        [SerializeField] private List<SoundProfileData> soundProfileDataList;

        private Dictionary<AudioActionType, SoundProfileData> audioDict = 
            new Dictionary<AudioActionType, SoundProfileData>();

        #region Setup
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                transform.parent = null;
                Instance = this;
                DontDestroyOnLoad(gameObject);

                for (int i = 0; i < Enum.GetValues(typeof(AudioActionType)).Length; i++)
                    audioDict.Add((AudioActionType)i, 
                        soundProfileDataList.FirstOrDefault(x => x.AudioType == (AudioActionType)i));
            }
        }
        #endregion

        #region Public Methods
        public void PlayMusic(AudioClip clip)
        {
            if (!clip) return;

            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlayOneShot(AudioActionType type)
        {
            var clip = audioDict[type].GetRandomClip();
            if (clip)
                PlayOneShot(clip);
        }

        public void PlayOneShot(AudioClip clip)
        {
            if (clip)
                sfxSource.PlayOneShot(clip);
        }
        #endregion
    }
}