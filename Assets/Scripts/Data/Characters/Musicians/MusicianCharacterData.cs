using ALWTTT.Characters.Band;
using MidiGenPlay;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "New MusicianCharacterData",
    menuName = "ALWTTT/Characters/MusicianCharacterData")]
    public class MusicianCharacterData : ScriptableObject
    {
        [Header("Profile")]
        [SerializeField] private string characterId;
        [SerializeField] private string characterName;
        [SerializeField] private string characterDescription;
        [SerializeField] private int initialMaxStress;
        [SerializeField] private MusicianBase characterPrefab;
        [SerializeField] private Sprite characterSprite; // TEMP

        [Header("Cards")]
        [SerializeField] private List<CardData> baseCards;

        [Header("Stats")]
        [SerializeField] private int chr;
        [SerializeField] private int tch;
        [SerializeField] private int emt;

        [Header("Audio")]
        [SerializeField] private MusicianProfileData profile;

        #region Encapsulation
        public string CharacterId => characterId;
        public string CharacterName => characterName;
        public string CharacterDescription => characterDescription;
        public int InitialMaxStress => initialMaxStress;
        public MusicianBase CharacterPrefab => characterPrefab;
        public Sprite CharacterSprite => characterSprite;

        public List<CardData> BaseCards => baseCards;

        public MusicianProfileData Profile => profile;

        public int CHR => chr;
        public int TCH => tch;
        public int EMT => emt;
        #endregion

        [Serializable]
        public class MusicianProfileData
        {
            public List<InstrumentType> backingInstruments;
            public List<InstrumentType> leadInstruments;
            // TODO percussion
        }
    }
}