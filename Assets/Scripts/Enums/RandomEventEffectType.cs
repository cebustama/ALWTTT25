using ALWTTT.Cards;
using ALWTTT.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Events
{
    public enum RandomEventEffectType
    {
        GainFans,
        ChangeCohesion,
        AddCard,
        AddCards,          // from a list
        AddMusician,
        RemoveMusician,    // by CharacterId
        AddStoryTag
    }

    [Serializable]
    public class RandomEventEffect
    {
        public RandomEventEffectType type;

        [Header("Numeric")]
        public int amount;             // Fans gained, cohesion delta, etc.

        [Header("Card(s)")]
        public CardDefinition card;
        public List<CardDefinition> cards;

        [Header("Musician")]
        public MusicianCharacterData musician; // for AddMusician
        public string musicianId;              // for RemoveMusician

        [Header("Story")]
        public string storyTag;
    }
}