using ALWTTT.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Cards
{
    [CreateAssetMenu(fileName = "New MusicianCardCatalog",
        menuName = "ALWTTT/Cards/Musician Card Catalog")]
    public class MusicianCardCatalogData : ScriptableObject
    {
        [SerializeField] private MusicianCharacterType musicianType;
        [SerializeField] private List<MusicianCardEntry> entries = new();

        public MusicianCharacterType MusicianType => musicianType;
        public List<MusicianCardEntry> Entries => entries;
    }
}
