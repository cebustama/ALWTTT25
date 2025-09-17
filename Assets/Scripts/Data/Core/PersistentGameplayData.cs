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

        [Serializable]
        public class BandConflict
        {
            public string id;   // guid
            public string musicianAId;  // required
            public string musicianBId;  // optional (null/empty => internal conflict)
            public int severity;    // 1..5?
            public string type; // enum string or code (creative differences, jealousy, etc.)
        }

        [SerializeField] private List<BandConflict> bandConflicts = new();

        // World
        [SerializeField] private List<MusicianBase> availableMusiciansList;
        [SerializeField] private List<string> usedRandomEventIds;
        // eventId -> sector
        [SerializeField] private SerializableStringIntDictionary eventLastSeenSector; 

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
        [SerializeField] private SerializableCardInventory musicianGrantedCards = 
            new SerializableCardInventory();

        // Sector Info
        [SerializeField] private int currentSectorId;
        [SerializeField] private int currentEncounterId;
        [SerializeField] private GigEncounter currentEncounter;
        [SerializeField] private bool isFinalEncounter;
        [SerializeField] private int lastMapNodeId;

        // Sector Map runtime state
        [SerializeField] private SectorMapState currentSectorMapState;

        // Global meta for SectorMap HUD (Fans/Level, Band Cohesion)
        [SerializeField] private int fans;           // total Fans (XP equivalent)
        [SerializeField] private int bandCohesion;   // if <= 0, Game Over

        // Story / Unlock meta
        [SerializeField] private List<string> storyTags = new List<string>();

        #region Encapsulation

        public List<MusicianBase> MusicianList
        {
            get => musicianList;
            set => musicianList = value;
        }

        public List<MusicianHealthData> MusicianHealthDataList
        {
            get => musicianHealthDataList;
            set => musicianHealthDataList = value;
        }

        public List<SongData> CurrentSongList
        {
            get => currentSongList;
            set => currentSongList = value;
        }

        public List<MusicianBase> AvailableMusiciansList
        {
            get => availableMusiciansList;
            set => availableMusiciansList = value;
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

        public List<BandConflict> BandConflicts => bandConflicts;

        public List<CardData> CurrentCardsList
        {
            get => currentCardsList;
            set => currentCardsList = value;
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

        public IReadOnlyList<string> StoryTags => storyTags;
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
            AvailableMusiciansList = new List<MusicianBase>();
            foreach (var mus in gameplayData.AllMusiciansList)
            {
                if (MusicianList.Contains(mus)) continue;
                AvailableMusiciansList.Add(mus);
            }

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

        public MusicianHealthData SetMusicianHealthData(
            string id, int newCurrentStress, int newMaxStress)
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

            return newData;
        }

        public MusicianHealthData GetMusicianHealthData(string id)
        {
            foreach (var data in musicianHealthDataList)
            {
                if (data.CharacterId == id)
                    return data;
            }

            return null;
        }

        #region Story / Events
        public bool HasStoryTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            return storyTags.Contains(tag);
        }

        public void AddStoryTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            if (storyTags == null) storyTags = new List<string>();
            if (!storyTags.Contains(tag)) storyTags.Add(tag);
        }

        public bool RemoveStoryTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag) || storyTags == null) return false;
            return storyTags.Remove(tag);
        }

        public bool HasUsedRandomEvent(string eventId) => usedRandomEventIds.Contains(eventId);

        public void MarkRandomEventUsed(string eventId, int currentSectorId)
        {
            if (string.IsNullOrEmpty(eventId)) return;
            if (!usedRandomEventIds.Contains(eventId)) usedRandomEventIds.Add(eventId);
            eventLastSeenSector[eventId] = currentSectorId;
        }

        public int GetEventLastSeenSector(string eventId)
        {
            return eventLastSeenSector.TryGetValue(eventId, out var s) ? s : -1;
        }
        #endregion

        #region Deck
        public void AddCardToDeck(CardData card)
        {
            if (card == null) return;
            if (CurrentCardsList == null) CurrentCardsList = new List<CardData>();
            CurrentCardsList.Add(card);
            // NOTE: if duplicates should be prevented, guard with:
            // if (!CurrentCardsList.Contains(card)) CurrentCardsList.Add(card);
        }

        // Grants cards to the deck AND records they came from musicianId
        public void GrantCardsToMusician(string musicianId, IEnumerable<CardData> cards)
        {
            if (string.IsNullOrEmpty(musicianId) || cards == null) return;
            if (CurrentCardsList == null) CurrentCardsList = new List<CardData>();

            foreach (var c in cards)
            {
                if (c == null) continue;
                CurrentCardsList.Add(c);
                musicianGrantedCards.AddCard(musicianId, c);
            }
        }

        // Overload convenience for single card
        public void GrantCardToMusician(string musicianId, CardData card)
        {
            if (string.IsNullOrEmpty(musicianId) || card == null) return;
            if (CurrentCardsList == null) CurrentCardsList = new List<CardData>();

            CurrentCardsList.Add(card);
            musicianGrantedCards.AddCard(musicianId, card);
        }
        #endregion

        #region Band
        public void AddMusicianToBand(MusicianCharacterData newMusician)
        {
            var musicianPrefab = newMusician.CharacterPrefab;
            MusicianList.Add(musicianPrefab);

            // Record base cards as coming from this musician
            GrantCardsToMusician(newMusician.CharacterId, newMusician.BaseCards);

            AvailableMusiciansList.Remove(musicianPrefab);
            SetMusicianHealthData(newMusician.CharacterId, 0, newMusician.InitialMaxStress);
        }

        public bool RemoveMusicianFromBand(string musicianId)
        {
            if (string.IsNullOrEmpty(musicianId)) return false;

            // Find the musician prefab in the current band by id
            MusicianBase toRemove = null;
            foreach (var mus in MusicianList)
            {
                if (mus != null && mus.CharacterId == musicianId)
                {
                    toRemove = mus;
                    break;
                }
            }

            if (toRemove == null)
            {
                Debug.LogWarning($"[Persistent] RemoveMusicianFromBand: musician id '{musicianId}' not found in band.");
                return false;
            }

            // 1) Remove musician
            MusicianList.Remove(toRemove);
            if (AvailableMusiciansList != null && !AvailableMusiciansList.Contains(toRemove))
                AvailableMusiciansList.Add(toRemove);

            // 2) Remove health entry
            var health = musicianHealthDataList?.Find(h => h.CharacterId == musicianId);
            if (health != null) musicianHealthDataList.Remove(health);

            // 3) Remove their granted cards from the deck
            if (musicianGrantedCards.TryRemoveAll(musicianId, out var granted))
            {
                if (CurrentCardsList == null) CurrentCardsList = new List<CardData>();

                // We remove ONE instance per recorded grant (deck is a multiset)
                foreach (var card in granted)
                {
                    if (card == null) continue;
                    // Remove only one copy for each grant record
                    CurrentCardsList.Remove(card);
                }
            }

            return true;
        }

        #endregion
    }
}