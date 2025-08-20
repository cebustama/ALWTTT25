using ALWTTT.Backgrounds;
using ALWTTT.Enums;
using ALWTTT.Encounters;
using System;
using UnityEngine;
using System.Collections.Generic;
using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;
using System.Collections;
using ALWTTT.Data;

namespace ALWTTT.Managers
{
    public class GigManager : MonoBehaviour
    {
        private const string DebugTag = "<color=magenta>GigManager:</color>";

        public static GigManager Instance;

        public bool debug = true;

        [Header("References")]
        [SerializeField] private BackgroundContainer backgroundContainer;
        [SerializeField] private List<Transform> musicianPosList;
        [SerializeField] private List<Transform> audienceMemberPosList;

        private GigPhase currentGigPhase;
        private List<SongData> playedSongs = new List<SongData>();

        #region Cache
        public GigEncounter CurrentGigEncounter { get; private set; }

        public List<MusicianBase> CurrentMusicianCharacterList
        {
            get;
            private set;
        } = new List<MusicianBase>();

        public List<AudienceCharacterBase> CurrentAudienceCharacterList
        {
            get;
            private set;
        } = new List<AudienceCharacterBase>();

        private GameManager GameManager => GameManager.Instance;
        private DeckManager DeckManager => DeckManager.Instance;
        private UIManager UIManager => UIManager.Instance;
        private MidiMusicManager MidiMusicManager => MidiMusicManager.Instance;

        public List<Transform> MusicianPosList => musicianPosList;
        public List<Transform> AudienceMemberPosList => audienceMemberPosList;

        // TODO: Implement dual target cards Musician -> Audience Character
        public MusicianBase SelectedMusician => 
            CurrentMusicianCharacterList.Count > 0 ?
            CurrentMusicianCharacterList[0] : null;

        public GigPhase CurrentGigPhase
        {
            get => currentGigPhase;
            private set
            {
                ExecuteGigPhase(value);
                currentGigPhase = value;
            }
        }
        #endregion

        #region Callbacks

        public Action OnPlayerTurnStarted;
        public Action OnSongPerformanceStarted;
        public Action OnEnemyTurnStarted;

        #endregion

        #region Setup
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
                currentGigPhase = GigPhase.PrepareGig;
            }
        }

        private void Start()
        {
            MidiMusicManager.GenerateSongs(GameManager.PersistentGameplayData.CurrentSongList);
            StartGig();
        }

        private void StartGig()
        {
            if (debug) Debug.Log($"{DebugTag} Starting gig...");

            SetupEncounter();
            BuildBackground();
            BuildBand();
            BuildAudience();

            DeckManager.SetGameDeck();

            UIManager.GigCanvas.gameObject.SetActive(true);

            CurrentGigPhase = GigPhase.PlayerTurn;
        }

        private void SetupEncounter()
        {
            if (debug) Debug.Log($"{DebugTag} Setting up gig encounter...");

            CurrentGigEncounter = GameManager.EncounterData.GetGigEncounter(
                GameManager.PersistentGameplayData.CurrentSectorId,
                GameManager.PersistentGameplayData.CurrentEncounterId,
                GameManager.PersistentGameplayData.IsFinalEncounter
            );

            GameManager.PersistentGameplayData.CurrentEncounter = CurrentGigEncounter;

            UIManager.SetupEncounterUI(CurrentGigEncounter);
        }

        private void BuildBackground()
        {
            if (debug) Debug.Log($"{DebugTag} Building background...");
            backgroundContainer.OpenSelectedBackground();
            backgroundContainer.SetBPM(0);
        }

        private void BuildBand()
        {
            if (debug) Debug.Log($"{DebugTag} Building band and musicians...");
            for (var i = 0; 
                i < GameManager.PersistentGameplayData.MusicianList.Count; i++)
            {
                MusicianBase clone = Instantiate(
                    GameManager.PersistentGameplayData.MusicianList[i],
                    MusicianPosList.Count >= i ?
                        MusicianPosList[i] :
                        MusicianPosList[0]
                );

                clone.BuildCharacter();

                // Front or Back of the Stage
                // TODO: Use a single layer per musician
                if (i < 2) clone.SetSpriteLayerOrder(10);
                else clone.SetSpriteLayerOrder(0);

                CurrentMusicianCharacterList.Add(clone);
            }
        }

        private void BuildAudience()
        {
            if (debug) Debug.Log($"{DebugTag} Building audience...");
            var audienceMemberList = CurrentGigEncounter.AudienceMemberList;
            for (var i = 0; i < audienceMemberList.Count; i++)
            {
                var clone = Instantiate(
                    audienceMemberList[i].CharacterPrefab,
                    AudienceMemberPosList.Count >= i ? 
                        AudienceMemberPosList[i] : 
                        AudienceMemberPosList[0]
                );

                clone.BuildCharacter();
                clone.ColumnIndex = Mathf.Min(i, AudienceMemberPosList.Count - 1);
                CurrentAudienceCharacterList.Add(clone);
            }
        }
        #endregion

        public void EndTurn()
        {
            if (debug) Debug.Log($"{DebugTag} Ending turn...");

            CurrentGigPhase = GigPhase.SongPerformance;
        }

        public void HighlightCardTarget(ActionTargetType targetType)
        {
            // TODO
            switch (targetType)
            {
                case ActionTargetType.AudienceCharacter:

                    break;
                case ActionTargetType.Musician:

                    break;
                case ActionTargetType.AllAudienceCharacters:

                    break;
                case ActionTargetType.AllMusicians:

                    break;
                case ActionTargetType.RandomAudienceCharacter:

                    break;
                case ActionTargetType.RandomMusician:

                    break;
            }
        }

        public void DeactivateCardHighlights()
        {
            // TODO
            // Foreach enemy canvas SetHighlight(false)
            // Foreach ally canvas SetHighlight(false)
        }

        private void ExecuteGigPhase(GigPhase targetGigPhase)
        {
            if (debug)
                Debug.Log($"{DebugTag} Executing gig phase: {targetGigPhase}");

            switch (targetGigPhase)
            {
                case GigPhase.PrepareGig:
                    break;
                case GigPhase.PlayerTurn:

                    OnPlayerTurnStarted?.Invoke();
                    GameManager.PersistentGameplayData.SongModifierCardsList.Clear();

                    // TODO: Stunned

                    // Initial groove per turn
                    GameManager.PersistentGameplayData.CurrentGroove =
                        GameManager.PersistentGameplayData.TurnStartingGroove;
                    // TODO: Special case for first turn (e.g. Deployment Phase in Monster Train 2)

                    DeckManager.DrawCards(GameManager.PersistentGameplayData.DrawCount);

                    GameManager.PersistentGameplayData.CanSelectCards = true;
                    break;
                case GigPhase.SongPerformance:

                    OnSongPerformanceStarted?.Invoke();

                    GameManager.PersistentGameplayData.CurrentSongIndex++;

                    if (GameManager.PersistentGameplayData.DiscardHandBetweenTurns)
                    {
                        DeckManager.DiscardHand();
                    }

                    StartCoroutine(SongPerformanceRoutine());

                    GameManager.PersistentGameplayData.CanSelectCards = false;
                    break;
                case GigPhase.AudienceTurn:

                    OnEnemyTurnStarted?.Invoke();

                    StartCoroutine(AudienceTurnRoutine());

                    GameManager.PersistentGameplayData.CanSelectCards = false;
                    break;
                case GigPhase.EndGig:

                    GameManager.PersistentGameplayData.CanSelectCards = false;
                    break;
            }
        }

        private IEnumerator SongPerformanceRoutine()
        {
            // Activate SFX cards
            foreach (var smCard in 
                GameManager.PersistentGameplayData.SongModifierCardsList)
            {
                if (smCard.CardType == CardType.SFX)
                {
                    // TODO: Generalize
                    backgroundContainer.ActivateSFX("lights");
                }
            }

            var song = GameManager.PersistentGameplayData.CurrentSong;

            playedSongs.Add(song);
            UIManager.GigCanvas.FillSongDropdown(playedSongs);
            backgroundContainer.SetBPM(song.BPM);

            // TODO: Playing Musician Animator Settings
            foreach (var musician in CurrentMusicianCharacterList)
            {
                musician.CharacterAnimator.SetBPM(song.BPM);
                musician.CharacterAnimator.SkipEveryNBeats = 1;
                musician.CharacterAnimator.BeatOffsetBeats = 
                    UnityEngine.Random.Range(0f, 0.15f);
                musician.CharacterAnimator.JumpOnBeat = true;
                musician.CharacterAnimator.RotateOnBeat = false;
                musician.CharacterAnimator.EmitOnBeat = true;
            }

            var songDuration = MidiMusicManager.Play(song);

            Debug.Log($"Playing {song.SongTitle} for {songDuration}[s]");

            yield return new WaitForSeconds(songDuration);

            backgroundContainer.SetBPM(0);
            foreach (var musician in CurrentMusicianCharacterList)
            {
                musician.CharacterAnimator.SetBPM(120);
                musician.CharacterAnimator.SkipEveryNBeats = 2;
                musician.CharacterAnimator.BeatOffsetBeats = 
                    UnityEngine.Random.Range(0.45f, 0.55f);
                musician.CharacterAnimator.JumpOnBeat = false;
                musician.CharacterAnimator.RotateOnBeat = true;
                musician.CharacterAnimator.EmitOnBeat = false;
            }

            var reactionDuration = 5f;

            Debug.Log("Audience Reaction");

            // TODO: Apply equipped SongModifier Effects
            foreach (var smCard in
                GameManager.PersistentGameplayData.SongModifierCardsList)
            { 
                
            }

            foreach (var ac in CurrentAudienceCharacterList)
            {
                if (ac.IsBlocked) continue; // No vibe for Blocked characters
                ac.AudienceStats.ApplySongVibe(song, reactionDuration);
            }

            // TODO: Apply Vibe to enemies
            yield return new WaitForSeconds(reactionDuration);

            if (CurrentGigPhase != GigPhase.EndGig)
            {
                CurrentGigPhase = GigPhase.AudienceTurn;
            }
        }

        private IEnumerator AudienceTurnRoutine()
        {
            var waitDelay = new WaitForSeconds(0.1f);

            foreach (var currentCharacter in CurrentAudienceCharacterList)
            {
                yield return currentCharacter.StartCoroutine(
                    nameof(AudienceCharacterSimple.ActionRoutine));
                yield return waitDelay;
            }

            if (CurrentGigPhase != GigPhase.EndGig)
            {
                CurrentGigPhase = GigPhase.PlayerTurn;
            }
        }

        private void LoseGig()
        {
            if (CurrentGigPhase == GigPhase.EndGig) return;
            CurrentGigPhase = GigPhase.EndGig;

            DeckManager.ClearPiles();

            UIManager.GigCanvas.gameObject.SetActive(true);
            UIManager.GigCanvas.LosePanel.SetActive(true);
        }

        private void WinGig()
        {
            if (CurrentGigPhase == GigPhase.EndGig) return;
            CurrentGigPhase = GigPhase.EndGig;

            // Keep current stress
            foreach (var musicianBase in CurrentMusicianCharacterList)
            {
                GameManager.PersistentGameplayData.SetMusicianHealthData(
                    musicianBase.MusicianCharacterData.CharacterId,
                    musicianBase.MusicianStats.CurrentStress,
                    musicianBase.MusicianStats.MaxStress);
            }

            DeckManager.ClearPiles();

            if (GameManager.PersistentGameplayData.IsFinalEncounter)
            {
                UIManager.GigCanvas.WinPanel.SetActive(true);
            }
            else
            {
                foreach (var musicianBase in CurrentMusicianCharacterList)
                {
                    musicianBase.MusicianStats.ClearAllStatus();
                }

                GameManager.PersistentGameplayData.CurrentEncounterId++;
                UIManager.GigCanvas.gameObject.SetActive(false);

                UIManager.RewardCanvas.gameObject.SetActive(true);
                UIManager.RewardCanvas.PrepareCanvas();
                UIManager.RewardCanvas.BuildReward(RewardType.Card);
            }
        }

        // TODO: Call
        public void RecalculateAudienceObstructions()
        {
            // Clear all
            foreach (var c in CurrentAudienceCharacterList) c.IsBlocked = false;

            for (int i = 0; i < CurrentAudienceCharacterList.Count; i++)
            {
                var member = CurrentAudienceCharacterList[i];
                if (member.IsTall)
                {
                    // Block all non-tall audience members behind tall one
                    for (int j = i + 1; j < CurrentAudienceCharacterList.Count; j++)
                    {
                        var otherMember = CurrentAudienceCharacterList[j];
                        if (!otherMember.IsTall)
                        {
                            otherMember.IsBlocked = true;
                        }
                    }

                    break;
                }
            }
        }
    }
}

