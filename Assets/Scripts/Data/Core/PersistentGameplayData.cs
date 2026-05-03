using ALWTTT.Cards;
using ALWTTT.Characters.Band;
using ALWTTT.Encounters;
using ALWTTT.Managers;
using ALWTTT.Musicians;
using MidiGenPlay.Composition;
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
        [SerializeField] private List<MusicianGameplayData> musicianGameplayDataList;
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

        // Deckbuilding
        [SerializeField] private List<CardDefinition> currentActionCards = new();
        [SerializeField] private List<CardDefinition> currentCompositionCards = new();

        [SerializeField]
        private SerializableCardInventory musicianGrantedActionCards =
            new SerializableCardInventory();
        [SerializeField]
        private SerializableCardInventory musicianGrantedCompositionCards =
            new SerializableCardInventory();

        // Gig Gameplay
        [SerializeField] private int drawCount;
        [SerializeField] private bool discardHandBetweenTurns;

        // Inspiration
        [SerializeField] private int maxInspiration;
        [SerializeField] private int currentInspiration;
        [SerializeField] private int turnStartingInspiration;
        [SerializeField] private bool keepInspirationBetweenTurns;

        [SerializeField] private int initialGigInspiration;
        [SerializeField] private int inspirationPerLoop;

        [SerializeField] private bool isRandomDeck;
        [SerializeField] private bool canUseCards;
        [SerializeField] private bool canSelectCards;

        [SerializeField] private SongData currentSong;
        [SerializeField] private int currentSongIndex;
        [SerializeField] private List<CardDefinition> songModifierCardsList;


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


        // Records
        [SerializeField] private int gigsWon;

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

        public List<MusicianGameplayData> MusicianGameplayDataList
        {
            get => musicianGameplayDataList;
            set => musicianGameplayDataList = value;
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

        public int MaxInspiration
        {
            get => maxInspiration;
            set => maxInspiration = value;
        }

        public int CurrentInspiration
        {
            get => currentInspiration;
            set => currentInspiration = value;
        }

        public int TurnStartingInspiration
        {
            get => turnStartingInspiration;
            set => turnStartingInspiration = value;
        }

        public int InitialGigInspiration
        {
            get => initialGigInspiration;
            set => initialGigInspiration = value;
        }

        public int InspirationPerLoop
        {
            get => inspirationPerLoop;
            set => inspirationPerLoop = value;
        }

        public List<BandConflict> BandConflicts => bandConflicts;

        public List<CardDefinition> CurrentActionCards
        {
            get => currentActionCards;
            set => currentActionCards = value;
        }

        public List<CardDefinition> CurrentCompositionCards
        {
            get => currentCompositionCards;
            set => currentCompositionCards = value;
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

        public bool KeepInspirationBetweenTurns
        {
            get => keepInspirationBetweenTurns;
            set => keepInspirationBetweenTurns = value;
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

        public List<CardDefinition> SongModifierCardsList
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

        public int GigsWon
        {
            get => gigsWon;
            set => gigsWon = value;
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
            AvailableMusiciansList = new List<MusicianBase>();
            foreach (var mus in gameplayData.AllMusiciansList)
            {
                if (MusicianList.Contains(mus)) continue;
                AvailableMusiciansList.Add(mus);
            }

            drawCount = gameplayData.DrawCount;
            maxInspiration = gameplayData.MaxInspiration;
            turnStartingInspiration = 0;
            currentInspiration = turnStartingInspiration;

            CanUseCards = true;
            CanSelectCards = true;
            DiscardHandBetweenTurns = gameplayData.DiscardHandBetweenTurns;
            KeepInspirationBetweenTurns = gameplayData.KeepInspirationBetweenTurns;

            isRandomDeck = gameplayData.IsRandomDeck;

            CurrentActionCards = new List<CardDefinition>();
            CurrentCompositionCards = new List<CardDefinition>();

            CurrentSongList = gameplayData.InitialSongList;
            CurrentSongIndex = 0;
            SongModifierCardsList = new List<CardDefinition>();

            CurrentSectorId = 0;
            CurrentEncounterId = 0;
            IsFinalEncounter = false;

            musicianHealthDataList = new List<MusicianHealthData>();
            musicianGameplayDataList = new List<MusicianGameplayData>();

            CurrentSectorMapState = null; // must be generated on first entry to SectorMap scene
            Fans = 0;
            BandCohesion = gameplayData.InitialCohesion;

            // Records
            GigsWon = 0;
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
        public void AddCardToDeck(CardDefinition card)
        {
            if (card == null) return;
            if (card.IsAction) currentActionCards.Add(card);
            else if (card.IsComposition) currentCompositionCards.Add(card);
        }

        // Grants cards to the deck AND records they came from musicianId
        public void GrantCardsToMusician(string musicianId, IEnumerable<CardDefinition> cards)
        {
            if (string.IsNullOrEmpty(musicianId) || cards == null) return;

            foreach (var c in cards)
            {
                if (c == null) continue;
                if (c.IsAction)
                {
                    currentActionCards.Add(c);
                    musicianGrantedActionCards.AddCard(musicianId, c);
                }
                else if (c.IsComposition)
                {
                    currentCompositionCards.Add(c);
                    musicianGrantedCompositionCards.AddCard(musicianId, c);
                }
            }
        }

        // Overload convenience for single card
        public void GrantCardToMusician(string musicianId, CardDefinition card)
        {
            if (string.IsNullOrEmpty(musicianId) || card == null) return;
            if (card.IsAction)
            {
                currentActionCards.Add(card);
                musicianGrantedActionCards.AddCard(musicianId, card);
            }
            else if (card.IsComposition)
            {
                currentCompositionCards.Add(card);
                musicianGrantedCompositionCards.AddCard(musicianId, card);
            }
        }
        #endregion

        #region Band

        public void SetBandDeck(BandDeckData bandDeck)
        {
            // Clear runtime decks
            if (currentActionCards == null)
                currentActionCards = new List<CardDefinition>();

            if (currentCompositionCards == null)
                currentCompositionCards = new List<CardDefinition>();

            currentActionCards.Clear();
            currentCompositionCards.Clear();

            // Reset provenance of granted cards, because this is a full deck replace
            musicianGrantedActionCards = new SerializableCardInventory();
            musicianGrantedCompositionCards = new SerializableCardInventory();

            isRandomDeck = false;

            if (bandDeck == null)
            {
                Debug.LogWarning("[PersistentGameplayData] SetBandDeck called with null deck.");
                return;
            }

            // M4.4 Deck Contract Evolution: deck is a multiset of
            // (card, count) entries. Each entry contributes `count` independent
            // references to the action / composition pile lists. The pre-M4.4
            // dedup-by-reference is gone � multiplicity is now the contract.
            var entries = bandDeck.Entries;
            if (entries == null)
            {
                Debug.LogWarning($"[PersistentGameplayData] " +
                    $"BandDeck '{bandDeck.name}' has null Entries.");
                return;
            }

            int totalAction = 0;
            int totalComposition = 0;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null) continue;

                var card = entry.card;
                if (card == null) continue;

                int copies = Mathf.Max(1, entry.count);

                if (card.IsAction)
                {
                    for (int k = 0; k < copies; k++) currentActionCards.Add(card);
                    totalAction += copies;
                }
                else if (card.IsComposition)
                {
                    for (int k = 0; k < copies; k++) currentCompositionCards.Add(card);
                    totalComposition += copies;
                }
                else
                {
                    Debug.LogWarning($"[PersistentGameplayData] " +
                        $"Card '{card.name}' is neither Action nor Composition. Skipped (�{copies}).");
                }
            }

            Debug.Log($"[PersistentGameplayData] SetBandDeck -> " +
                      $"Action={totalAction}, " +
                      $"Composition={totalComposition} " +
                      $"(Deck='{bandDeck.name}', uniqueEntries={entries.Count})");
        }

        /// <summary>
        /// M4.6-prep batch (2): per-musician auto-assembly path. Builds the
        /// runtime deck by reading each musician's <c>CardCatalog</c>
        /// (starter-flagged entries, expanded by <c>starterCopies</c>) and
        /// merging an optional generic catalogue.
        ///
        /// Mirrors <see cref="SetBandDeck"/>'s reset semantics: clears piles,
        /// resets per-musician granted-card inventories, sets
        /// <c>isRandomDeck = false</c>.
        ///
        /// Per-musician contributions populate
        /// <c>musicianGrantedActionCards</c> /
        /// <c>musicianGrantedCompositionCards</c> so
        /// <see cref="RemoveMusicianFromBand"/> can clean up correctly when a
        /// musician departs mid-run. Generic-catalogue contributions are NOT
        /// provenance-tracked — they are not "from" any specific musician,
        /// so they correctly survive a musician removal.
        ///
        /// D2b: an entry with <c>StarterDeck</c> flag and
        /// <c>starterCopies &lt;= 0</c> is an author error. The runtime
        /// warns and skips rather than silently coercing to 1, so
        /// authoring bugs surface in the logs.
        /// </summary>
        public void SetBandDeckFromMusicians(
            IList<MusicianCharacterData> musicians,
            GenericCardCatalogSO genericCatalog)
        {
            // Clear runtime piles
            if (currentActionCards == null)
                currentActionCards = new List<CardDefinition>();
            if (currentCompositionCards == null)
                currentCompositionCards = new List<CardDefinition>();

            currentActionCards.Clear();
            currentCompositionCards.Clear();

            // Reset provenance (full deck replace, mirrors SetBandDeck)
            musicianGrantedActionCards = new SerializableCardInventory();
            musicianGrantedCompositionCards = new SerializableCardInventory();

            isRandomDeck = false;

            if (musicians == null || musicians.Count == 0)
            {
                Debug.LogWarning(
                    "[PersistentGameplayData] SetBandDeckFromMusicians: " +
                    "empty roster. No cards added.");
                return;
            }

            int totalAction = 0;
            int totalComposition = 0;
            int skippedNoCatalog = 0;
            int skippedZeroCopies = 0;
            int skippedNoDomain = 0;

            // --- Per-musician contributions ---
            for (int mi = 0; mi < musicians.Count; mi++)
            {
                var m = musicians[mi];
                if (m == null) continue;

                var catalog = m.CardCatalog;
                if (catalog == null)
                {
                    Debug.LogWarning(
                        $"[PersistentGameplayData] SetBandDeckFromMusicians: " +
                        $"musician '{m.CharacterName}' (id={m.CharacterId}) " +
                        $"has no CardCatalog. Skipping contribution.");
                    skippedNoCatalog++;
                    continue;
                }

                var entries = catalog.Entries;
                if (entries == null) continue;

                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (entry?.card == null) continue;
                    if (!entry.IsStarter) continue;

                    // D2b: starterCopies <= 0 with StarterDeck flag set is
                    // an author error. Warn + skip rather than silently
                    // coercing to 1 — the editor service clamps at write
                    // time, so this only happens via direct asset edit.
                    if (entry.starterCopies <= 0)
                    {
                        Debug.LogWarning(
                            $"[PersistentGameplayData] SetBandDeckFromMusicians: " +
                            $"entry '{entry.card.name}' on '{m.CharacterName}' " +
                            $"has StarterDeck flag but " +
                            $"starterCopies={entry.starterCopies}. Skipping.");
                        skippedZeroCopies++;
                        continue;
                    }

                    int copies = entry.starterCopies;
                    var card = entry.card;

                    if (card.IsAction)
                    {
                        for (int k = 0; k < copies; k++)
                        {
                            currentActionCards.Add(card);
                            musicianGrantedActionCards.AddCard(m.CharacterId, card);
                        }
                        totalAction += copies;
                    }
                    else if (card.IsComposition)
                    {
                        for (int k = 0; k < copies; k++)
                        {
                            currentCompositionCards.Add(card);
                            musicianGrantedCompositionCards.AddCard(m.CharacterId, card);
                        }
                        totalComposition += copies;
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[PersistentGameplayData] SetBandDeckFromMusicians: " +
                            $"card '{card.name}' on '{m.CharacterName}' is " +
                            $"neither Action nor Composition. " +
                            $"Skipping (×{copies}).");
                        skippedNoDomain++;
                    }
                }
            }

            // --- Generic catalogue contributions (no provenance) ---
            int genericAction = 0;
            int genericComposition = 0;
            if (genericCatalog != null && genericCatalog.Entries != null)
            {
                var gEntries = genericCatalog.Entries;
                for (int i = 0; i < gEntries.Count; i++)
                {
                    var entry = gEntries[i];
                    if (entry?.card == null) continue;
                    if (!entry.IsStarter) continue;

                    if (entry.starterCopies <= 0)
                    {
                        Debug.LogWarning(
                            $"[PersistentGameplayData] SetBandDeckFromMusicians: " +
                            $"generic entry '{entry.card.name}' has StarterDeck " +
                            $"flag but starterCopies={entry.starterCopies}. " +
                            $"Skipping.");
                        skippedZeroCopies++;
                        continue;
                    }

                    int copies = entry.starterCopies;
                    var card = entry.card;

                    if (card.IsAction)
                    {
                        for (int k = 0; k < copies; k++)
                            currentActionCards.Add(card);
                        genericAction += copies;
                    }
                    else if (card.IsComposition)
                    {
                        for (int k = 0; k < copies; k++)
                            currentCompositionCards.Add(card);
                        genericComposition += copies;
                    }
                    else
                    {
                        Debug.LogWarning(
                            $"[PersistentGameplayData] SetBandDeckFromMusicians: " +
                            $"generic card '{card.name}' is neither Action nor " +
                            $"Composition. Skipping (×{copies}).");
                        skippedNoDomain++;
                    }
                }

                totalAction += genericAction;
                totalComposition += genericComposition;
            }

            Debug.Log(
                $"[PersistentGameplayData] SetBandDeckFromMusicians -> " +
                $"Action={totalAction} " +
                $"(per-musician={totalAction - genericAction}, " +
                $"generic={genericAction}), " +
                $"Composition={totalComposition} " +
                $"(per-musician={totalComposition - genericComposition}, " +
                $"generic={genericComposition}), " +
                $"musicians={musicians.Count}, " +
                $"skippedNoCatalog={skippedNoCatalog}, " +
                $"skippedZeroCopies={skippedZeroCopies}, " +
                $"skippedNoDomain={skippedNoDomain}");
        }


        public void ResetBandForSetup(List<MusicianBase> all)
        {
            MusicianList = new List<MusicianBase>();
            AvailableMusiciansList = new List<MusicianBase>(all);
            musicianHealthDataList = new List<MusicianHealthData>();

            CurrentSongList = new List<SongData>();
            CurrentSongIndex = 0;
            SongModifierCardsList = new List<CardDefinition>();

            CurrentActionCards = new List<CardDefinition>();
            CurrentCompositionCards = new List<CardDefinition>();
            musicianGrantedActionCards = new SerializableCardInventory();
            musicianGrantedCompositionCards = new SerializableCardInventory();
        }

        /// <summary>
        /// M4.6-prep merged (1)/(4): replace the band roster with the picked subset.
        /// Distinct from <see cref="AddMusicianToBand"/> (which also grants base cards
        /// for the recruit/meta path). This method handles roster identity only:
        /// MusicianList, AvailableMusiciansList, MusicianHealthData, MusicianGameplayData.
        /// Cards are owned entirely by the deck path (auto-assembly via
        /// <see cref="SetBandDeckFromMusicians"/>, or legacy via <see cref="SetBandDeck"/>).
        /// Health/gameplay-data entries are preserved if they already exist for a picked
        /// musician (idempotent across re-entries to GigSetupScene).
        /// </summary>
        public void SetBandRoster(IList<MusicianBase> picked)
        {
            if (picked == null)
            {
                Debug.LogWarning(
                    "[PersistentGameplayData] SetBandRoster called with null list. " +
                    "Treating as empty.");
                picked = new List<MusicianBase>();
            }

            // 1) Replace MusicianList (dedupe by reference; skip nulls)
            MusicianList = new List<MusicianBase>(picked.Count);
            for (int i = 0; i < picked.Count; i++)
            {
                var m = picked[i];
                if (m == null) continue;
                if (MusicianList.Contains(m)) continue;
                MusicianList.Add(m);
            }

            // 2) Rebuild AvailableMusiciansList = AllMusiciansList \ MusicianList
            AvailableMusiciansList = new List<MusicianBase>();
            if (gameplayData != null && gameplayData.AllMusiciansList != null)
            {
                foreach (var m in gameplayData.AllMusiciansList)
                {
                    if (m == null) continue;
                    if (MusicianList.Contains(m)) continue;
                    AvailableMusiciansList.Add(m);
                }
            }

            // 3) Ensure health and gameplay data exist for picked musicians (idempotent)
            if (musicianHealthDataList == null)
                musicianHealthDataList = new List<MusicianHealthData>();
            if (musicianGameplayDataList == null)
                musicianGameplayDataList = new List<MusicianGameplayData>();

            for (int i = 0; i < MusicianList.Count; i++)
            {
                var m = MusicianList[i];
                var data = m != null ? m.MusicianCharacterData : null;
                if (data == null) continue;

                if (musicianHealthDataList.Find(h => h.CharacterId == data.CharacterId) == null)
                {
                    SetMusicianHealthData(data.CharacterId, 0, data.InitialMaxStress);
                }

                if (musicianGameplayDataList.Find(g => g.CharacterId == data.CharacterId) == null)
                {
                    var startingMelodicLeading = data.Profile != null
                        ? data.Profile.defaultMelodicLeading
                        : null;
                    SetMusicianGameplayData(data.CharacterId, startingMelodicLeading);
                }
            }

            // 4) Log
            var ids = new List<string>(MusicianList.Count);
            for (int i = 0; i < MusicianList.Count; i++)
            {
                var d = MusicianList[i] != null ? MusicianList[i].MusicianCharacterData : null;
                ids.Add(d != null ? d.CharacterId : "<null>");
            }

            Debug.Log(
                $"[PersistentGameplayData] SetBandRoster -> " +
                $"musicians={MusicianList.Count} " +
                $"(ids=[{string.Join(",", ids)}]), " +
                $"available={AvailableMusiciansList.Count}");
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

        public MusicianGameplayData SetMusicianGameplayData(
            string id,
            MelodicLeadingConfig startingMelodicLeading)
        {
            var newData = new MusicianGameplayData
            {
                CharacterId = id,
                CurrentMelodicLeading = startingMelodicLeading,
                UnlockedMelodicLeadings = new List<MelodicLeadingConfig>()
            };

            if (startingMelodicLeading != null)
                newData.UnlockedMelodicLeadings.Add(startingMelodicLeading);

            // Replace if exists
            var existing = musicianGameplayDataList.Find(x => x.CharacterId == id);
            if (existing != null)
            {
                musicianGameplayDataList.Remove(existing);
                musicianGameplayDataList.Add(newData);
            }
            else
            {
                musicianGameplayDataList.Add(newData);
            }

            return newData;
        }

        public MusicianGameplayData GetMusicianGameplayData(string id)
        {
            foreach (var data in musicianGameplayDataList)
                if (data.CharacterId == id)
                    return data;
            return null;
        }

        public void AddMusicianToBand(MusicianCharacterData newMusician)
        {
            var musicianPrefab = newMusician.CharacterPrefab;
            MusicianList.Add(musicianPrefab);

            // Record base cards as coming from this musician
            GrantCardsToMusician(newMusician.CharacterId, newMusician.BaseActionCards);
            GrantCardsToMusician(newMusician.CharacterId, newMusician.BaseCompositionCards);

            AvailableMusiciansList.Remove(musicianPrefab);

            SetMusicianHealthData(newMusician.CharacterId, 0, newMusician.InitialMaxStress);

            var startingMelodicLeading = newMusician.Profile != null
                ? newMusician.Profile.defaultMelodicLeading
                : null;

            SetMusicianGameplayData(newMusician.CharacterId, startingMelodicLeading);
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
            if (musicianGrantedActionCards.TryRemoveAll(musicianId, out var grantedA))
            {
                foreach (var card in grantedA) currentActionCards.Remove(card);
            }
            if (musicianGrantedCompositionCards.TryRemoveAll(musicianId, out var grantedC))
            {
                foreach (var card in grantedC) currentCompositionCards.Remove(card);
            }

            return true;
        }

        public bool HasActiveConflictBetween(string musicianAId, string musicianBId)
        {
            bool wantInternal = string.IsNullOrEmpty(musicianBId);

            for (int i = 0; i < bandConflicts.Count; i++)
            {
                var c = bandConflicts[i];
                var ca = c.musicianAId;
                var cb = string.IsNullOrEmpty(c.musicianBId) ? null : c.musicianBId;

                if (wantInternal)
                {
                    // Internal conflict: match (A, null)
                    if (cb == null && ca == musicianAId) return true;
                }
                else
                {
                    // Pair conflict: unordered match (A,B) or (B,A)
                    if (cb != null)
                    {
                        if ((ca == musicianAId && cb == musicianBId) ||
                            (ca == musicianBId && cb == musicianAId))
                            return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Songs
        public SongData GenerateSong()
        {
            var pool = gameplayData.PossibleSongList;
            if (pool == null || pool.Count == 0) return null;

            var pick = pool[UnityEngine.Random.Range(0, pool.Count)];
            if (currentSongList == null) currentSongList = new List<SongData>();
            currentSongList.Add(pick);
            return pick;
        }

        public void SetNextThemeTag(string theme)
        {

        }
        #endregion

        public void ApplyRunConfig(
            GigRunContext.RunConfig config,
            GigSetupConfigData defaults)
        {
            if (config == null)
            {
                Debug.LogWarning("[PersistentGameplayData] ApplyRunConfig called with null config.");
                return;
            }

            // --- Reset gig state ---
            CurrentSongIndex = 0;

            SongModifierCardsList ??= new List<CardDefinition>();
            SongModifierCardsList.Clear();

            // --- Deck ---
            // M4.6-prep batch (2): branch on useMusicianStarters. When ON,
            // assemble from each musician's CardCatalog + optional generic
            // catalogue. When OFF, legacy BandDeckData asset path runs.
            if (config.useMusicianStarters)
            {
                var roster = new List<MusicianCharacterData>(MusicianList?.Count ?? 0);
                if (MusicianList != null)
                {
                    for (int i = 0; i < MusicianList.Count; i++)
                    {
                        var m = MusicianList[i];
                        if (m == null) continue;
                        var data = m.MusicianCharacterData;
                        if (data == null) continue;
                        roster.Add(data);
                    }
                }
                SetBandDeckFromMusicians(roster, defaults?.GenericStarterCatalog);
            }
            else
            {
                SetBandDeck(config.bandDeck);
            }

            // --- Encounter ---
            CurrentEncounter = config.encounter;

            // Prevent sector-based re-derivation
            CurrentSectorId = 0;
            CurrentEncounterId = 0;
            IsFinalEncounter = false;

            // --- Inspiration ---

            // 1) Initial Gig Inspiration (applied once at gig start)
            int fallbackInitialGigInspiration =
                defaults != null
                    ? defaults.DefaultInitialGigInspiration
                    : InitialGigInspiration;

            InitialGigInspiration =
                config.overrideInitialGigInspiration
                    ? Mathf.Max(0, config.initialGigInspiration)
                    : fallbackInitialGigInspiration;

            // Note: do NOT set CurrentInspiration from TurnStartingInspiration anymore.
            // GigManager.StartGig should do: CurrentInspiration = InitialGigInspiration;

            // 2) Turn Starting Inspiration (only used when KeepInspirationBetweenTurns == false)
            int fallbackTurnStartingInspiration =
                defaults != null
                    ? defaults.DefaultStartingInspiration
                    : TurnStartingInspiration;

            // TODO: Remove this field completely, replaced by InspirationPerLoop
            TurnStartingInspiration = fallbackTurnStartingInspiration;

            // 3) Inspiration Per Loop
            int fallbackInspirationPerLoop =
                defaults != null
                    ? defaults.DefaultInspirationPerLoop
                    : InspirationPerLoop;

            InspirationPerLoop =
                config.overrideInspirationPerLoop
                    ? Mathf.Max(0, config.inspirationPerLoop)
                    : fallbackInspirationPerLoop;

            // --- Policies ---
            DiscardHandBetweenTurns =
                config.overrideDiscardHandBetweenTurns
                    ? config.discardHandBetweenTurns
                    : (defaults != null
                        ? defaults.DefaultDiscardHandBetweenTurns
                        : DiscardHandBetweenTurns);

            KeepInspirationBetweenTurns =
                config.overrideKeepInspirationBetweenTurns
                    ? config.keepInspirationBetweenTurns
                    : (defaults != null
                        ? defaults.DefaultKeepInspirationBetweenTurns
                        : KeepInspirationBetweenTurns);

            Debug.Log(
                $"[PersistentGameplayData] ApplyRunConfig -> " +
                $"Deck={config.deckLabel ?? config.bandDeck?.name ?? "<unset>"}, " +
                $"Encounter={config.encounter?.GetLabel()}, " +
                $"StartInspiration={TurnStartingInspiration}, " +
                $"DiscardBetweenTurns={DiscardHandBetweenTurns}, " +
                $"KeepInspiration={KeepInspirationBetweenTurns}"
            );
        }

    }

    [Serializable]
    public class MusicianHealthData
    {
        [SerializeField] private string characterId;
        [SerializeField] private int maxStress;
        [SerializeField] private int currentStress;

        public int MaxStress
        {
            get => maxStress;
            set => maxStress = value;
        }

        public int CurrentStress
        {
            get => currentStress;
            set => currentStress = value;
        }

        public string CharacterId
        {
            get => characterId;
            set => characterId = value;
        }
    }

    [Serializable]
    public class MusicianGameplayData
    {
        [SerializeField] private string characterId;

        // Default
        [SerializeField] private MelodicLeadingConfig currentMelodicLeading;
        [SerializeField] private HarmonicLeadingConfig currentHarmonicLeading;
        // Progression
        [SerializeField] private List<MelodicLeadingConfig> unlockedMelodicLeadings;
        [SerializeField] private List<HarmonicLeadingConfig> unlockedHarmonicLeadings;

        public string CharacterId
        {
            get => characterId;
            set => characterId = value;
        }

        public MelodicLeadingConfig CurrentMelodicLeading
        {
            get => currentMelodicLeading;
            set => currentMelodicLeading = value;
        }

        public List<MelodicLeadingConfig> UnlockedMelodicLeadings
        {
            get => unlockedMelodicLeadings;
            set => unlockedMelodicLeadings = value;
        }

        public HarmonicLeadingConfig CurrentHarmonicLeading
        {
            get => currentHarmonicLeading;
            set => currentHarmonicLeading = value;
        }

        public List<HarmonicLeadingConfig> UnlockedHarmonicLeadings
        {
            get => unlockedHarmonicLeadings;
            set => unlockedHarmonicLeadings = value;
        }
    }
}