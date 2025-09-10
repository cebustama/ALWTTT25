using ALWTTT.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT
{
    [CreateAssetMenu(fileName = "New CardTypeData", menuName = "ALWTTT/Cards/CardTypeData")]
    public class CardTypeData : ScriptableObject
    {
        [SerializeField] private CardType cardType;

        [Header("Synergies")]
        [SerializeField] private List<MusicianStat> musicianStats;

        [Header("UI")]
        [SerializeField] private Color typeColor;

        [Header("Gameplay")]
        [SerializeField] private float multiplier = 1f;

        public CardType CardType => cardType;
        public Color TypeColor => typeColor;
        public float Multiplier => multiplier;
    }
}