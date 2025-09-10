using ALWTTT.Enums;
using ALWTTT.Extentions;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "SoundProfileData",
    menuName = "ALWTTT/Containers/SoundProfileData")]
    public class SoundProfileData : ScriptableObject
    {
        [SerializeField] private AudioActionType audioType;
        [SerializeField] private List<AudioClip> randomClipList;

        public AudioActionType AudioType => audioType;

        public List<AudioClip> RandomClipList => randomClipList;

        public AudioClip GetRandomClip() => 
            RandomClipList.Count > 0 ? RandomClipList.RandomItem() : null;
    }
}