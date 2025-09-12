using ALWTTT.Characters.Band;
using ALWTTT.Encounters;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    [Serializable]
    public class PersistentGameplayData
    {
        private readonly GameplayData gameplayData;

        // Band
        [SerializeField] private List<MusicianBase> musicianList;
        [SerializeField] private List<MusicianHealthData> musicianHealthDataList;
        [SerializeField] private List<SongData> currentSongList;

        // Deckbuilding / Gig Encounters
        [SerializeField] private List<CardData> currentCardsList;
        [SerializeField] private int drawCount;
        [SerializeField] private int maxGroove;
        [SerializeField] private int currentGroove;
        [SerializeField] private int turnStartingGroove;
        [SerializeField] private bool isRandomDeck;
        [SerializeField] private bool canUseCards;
        [SerializeField] private bool canSelectCards;
        [SerializeField] private bool discardHandBetweenTurns;
        [SerializeField] private bool keepGrooveBetweenTurns;
        [SerializeField] private SongData currentSong;
        [SerializeField] private int currentSongIndex;
        [SerializeField] private List<CardData> songModifierCardsList;

        // Sector Info
        [SerializeField] private int currentSectorId;
        [SerializeField] private int currentEncounterId;
        [SerializeField] private GigEncounter currentEncounter;
        [SerializeField] private bool isFinalEncounter;
        [SerializeField] private int lastMapNodeId;

        // --- Sector Map runtime state ---
        [SerializeField] private SectorMapState currentSectorMapState;

        // Global meta for SectorMap HUD (Fans/Level, Band Cohesion)
        [SerializeField] private int fans;           // total Fans (XP equivalent)
        [SerializeField] private int bandCohesion;   // if <= 0, Game Over

        #region Encapsulation

        public List<MusicianBase> MusicianList
        {
            get => musicianList;
            set => musicianList = value;
        }

        public int DrawCount
        {
            get => drawCount;
            set => drawCount = value;
        }

        public int MaxGroove
        {
            get => maxGroove;
            set => maxGroove = value;
        }

        public int CurrentGroove
        {
            get => currentGroove;
            set => currentGroove = value;
        }

        public int TurnStartingGroove
        {
            get => turnStartingGroove;
            set => turnStartingGroove = value;
        }

        public List<CardData> CurrentCardsList
        {
            get => currentCardsList;
            set => currentCardsList = value;
        }

        public List<SongData> CurrentSongList
        {
            get => currentSongList;
            set => currentSongList = value;
        }

        public bool IsRandomDeck => isRandomDeck;

        public bool CanUseCards
        {
            get => canUseCards;
            set => canUseCards = value;
        }

        public bool CanSelectCards
        {
            get => canSelectCards;
            set => canSelectCards = value;
        }

        public bool DiscardHandBetweenTurns
        {
            get => discardHandBetweenTurns;
            set => discardHandBetweenTurns = value;
        }

        public bool KeepGrooveBetweenTurns
        {
            get => keepGrooveBetweenTurns;
            set => keepGrooveBetweenTurns = value;
        }

        public int CurrentSectorId
        {
            get => currentSectorId;
            set => currentSectorId = value;
        }

        public int CurrentEncounterId
        {
            get => currentEncounterId;
            set => currentEncounterId = value;
        }

        public bool IsFinalEncounter
        {
            get => isFinalEncounter;
            set => isFinalEncounter = value;
        }

        public int LastMapNodeId
        {
            get => lastMapNodeId;
            set => lastMapNodeId = value;
        }

        public List<MusicianHealthData> MusicianHealthDataList
        {
            get => musicianHealthDataList;
            set => musicianHealthDataList = value;
        }

        public SongData CurrentSong
        {
            get => currentSong;
            set => currentSong = value;
        }

        public GigEncounter CurrentEncounter
        {
            get => currentEncounter;
            set => currentEncounter = value;
        }

        public int CurrentSongIndex
        {
            get => currentSongIndex;
            set => currentSongIndex = value;
        }

        public List<CardData> SongModifierCardsList
        {
            get => songModifierCardsList;
            set => songModifierCardsList = value;
        }

        public SectorMapState CurrentSectorMapState
        {
            get => currentSectorMapState;
            set => currentSectorMapState = value;
        }

        public int Fans
        {
            get => fans;
            set => fans = value;
        }

        public int BandCohesion
        {
            get => bandCohesion;
            set => bandCohesion = value;
        }
        #endregion

        public PersistentGameplayData(GameplayData gameplayData)
        {
            this.gameplayData = gameplayData;

            InitData();
        }

        private void InitData()
        {
            Debug.Log("<color=white>Initializing PersistentGameplayData...</color>");

            MusicianList = new List<MusicianBase>(gameplayData.InitialMusicianList);

            drawCount = gameplayData.DrawCount;
            maxGroove = gameplayData.MaxGroove;
            turnStartingGroove = 0;
            currentGroove = turnStartingGroove;

            CanUseCards = true;
            CanSelectCards = true;
            DiscardHandBetweenTurns = gameplayData.DiscardHandBetweenTurns;
            KeepGrooveBetweenTurns = gameplayData.KeepGrooveBetweenTurns;

            isRandomDeck = gameplayData.IsRandomDeck;
            CurrentCardsList = new List<CardData>();
            CurrentSongList = gameplayData.InitialSongList;
            CurrentSongIndex = 0;
            SongModifierCardsList = new List<CardData>();

            CurrentSectorId = 0;
            CurrentEncounterId = 0;
            IsFinalEncounter = false;

            musicianHealthDataList = new List<MusicianHealthData>();

            CurrentSectorMapState = null; // must be generated on first entry to SectorMap scene
            Fans = 0;
            BandCohesion = gameplayData.InitialCohesion;
        }

        public void SetMusicianHealthData(string id, int newCurrentStress, int newMaxStress)
        {
            var newData = new MusicianHealthData();
            newData.CharacterId = id;
            newData.CurrentStress = newCurrentStress;
            newData.MaxStress = newMaxStress;

            // Replace old data with new one
            var data = musicianHealthDataList.Find(x => x.CharacterId == id);
            if (data != null)
            {
                musicianHealthDataList.Remove(data);
                musicianHealthDataList.Add(newData);
            }
            else
            {
                musicianHealthDataList.Add(newData);
            }
        }
    }
}