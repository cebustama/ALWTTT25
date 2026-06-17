using ALWTTT.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Events
{
    [CreateAssetMenu(fileName = "New Random Event",
        menuName = "ALWTTT/Events/RandomEventData")]
    public class RandomEventData : ScriptableObject
    {
        [Header("Content")]
        [SerializeField] private string eventId;
        [SerializeField] private string title;
        [SerializeField][TextArea] private string description;

        [Header("Art")]
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite eventSprite;

        [Header("Options")]
        [SerializeField] private List<RandomEventOption> options;

        #region Encapsulation
        public string EventId => eventId;
        public string Title => title;
        public string Description => description;
        public Sprite BackgroundSprite => backgroundSprite;
        public Sprite EventSprite => eventSprite;
        public List<RandomEventOption> Options => options;
        #endregion
    }

    [Serializable]
    public class RandomEventOption
    {
        public string optionId;
        public string text;
        public List<RandomEventEffect> effects = new();
        // Optional: gate/weight/tags for spawning logic later.
    }
}