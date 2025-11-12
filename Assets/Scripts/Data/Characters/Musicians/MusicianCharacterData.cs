using ALWTTT.Cards;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using MidiGenPlay;
using MidiGenPlay.Composition;
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
        [SerializeField] private MusicianCharacterType characterType;
        [SerializeField] private MusicianBase characterPrefab;
        [SerializeField] private Sprite characterSprite; // TEMP
        [SerializeField] private Sprite characterIcon;

        [Header("Cards")]
        [SerializeField] private List<CardData> baseActionCards;
        [SerializeField] private List<CardData> baseCompositionCards;

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
        public MusicianCharacterType CharacterType => characterType;
        public MusicianBase CharacterPrefab => characterPrefab;
        public Sprite CharacterSprite => characterSprite;
        public Sprite CharacterIcon => characterIcon;

        public List<CardData> BaseActionCards => baseActionCards;
        public List<CardData> BaseCompositionCards => baseCompositionCards;

        public MusicianProfileData Profile => profile;
        public MelodicLeadingConfig DefaultMelodicLeading =>
            profile != null ? profile.defaultMelodicLeading : null;

        public int CHR => chr;
        public int TCH => tch;
        public int EMT => emt;
        #endregion

        [Serializable]
        public class MusicianProfileData
        {
            [Header("Instrument Types")]
            public List<InstrumentType> backingInstruments;
            public List<InstrumentType> leadInstruments; 

            [Header("Melodic Instruments Whitelist")]
            public List<MIDIInstrumentSO> backingMelodicInstruments;
            public List<MIDIInstrumentSO> leadMelodicInstruments;

            [Header("Percussion Instruments Whitelist")]
            public List<MIDIPercussionInstrumentSO> percussionInstruments;

            [Header("Composition")]
            public MelodicLeadingConfig defaultMelodicLeading;
            public HarmonicLeadingConfig defaultHarmonicLeading;
        }
    }
}