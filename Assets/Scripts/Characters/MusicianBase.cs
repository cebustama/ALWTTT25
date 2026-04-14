using ALWTTT.Cards;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.Musicians;
using MidiGenPlay;
using System;
using System.Collections;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    public class MusicianBase : CharacterBase, IMusician
    {
        [SerializeField] private BandCharacterCanvas bandCharacterCanvas;
        [SerializeField] private MusicianCharacterData musicianCharacterData;
        [SerializeField] private BandCharacterStats stats;
        [SerializeField] private ParticleSystem musicianParticleSystem;

        [NonSerialized] private MIDIInstrumentSO _debugOverrideInstrument;
        [NonSerialized]
        private MIDIPercussionInstrumentSO _debugOverridePercussionInstrument;

        public override IMusicianStats MusicianStats => stats;

        #region Encapsulate
        public BandCharacterCanvas BandCharacterCanvas => bandCharacterCanvas;
        public MusicianCharacterData MusicianCharacterData => musicianCharacterData;
        public BandCharacterStats Stats => stats;
        public string CharacterId => musicianCharacterData.CharacterId;
        public string CharacterName => musicianCharacterData.CharacterName;

        public MIDIInstrumentSO DebugOverrideInstrument
        {
            get => _debugOverrideInstrument;
            set => _debugOverrideInstrument = value;
        }

        public MIDIPercussionInstrumentSO DebugOverridePercussionInstrument
        {
            get => _debugOverridePercussionInstrument;
            set => _debugOverridePercussionInstrument = value;
        }
        #endregion

        private bool _boundToGig;

        public override void BuildCharacter()
        {
            base.BuildCharacter();
            bandCharacterCanvas.InitCanvas(MusicianCharacterData.CharacterName);

            stats = new BandCharacterStats(
                musicianCharacterData.CHR,
                musicianCharacterData.TCH,
                musicianCharacterData.EMT,
                MusicianCharacterData.InitialMaxStress,
                BandCharacterCanvas
            );

            bandCharacterCanvas.UpdateStats(
                stats.Charm,
                stats.Technique,
                stats.Emotion
            );

            var data = GameManager.PersistentGameplayData.MusicianHealthDataList.Find(
                x => x.CharacterId == MusicianCharacterData.CharacterId);

            if (data != null)
            {
                stats.CurrentStress = data.CurrentStress;
                stats.MaxStress = data.MaxStress;
            }
            else
            {
                GameManager.PersistentGameplayData.SetMusicianHealthData(
                    MusicianCharacterData.CharacterId, stats.CurrentStress, stats.MaxStress);
            }

            // --- GameplayData sync (melodic personality etc.) ---
            var pd = GameManager.PersistentGameplayData;
            var gData = pd.GetMusicianGameplayData(MusicianCharacterData.CharacterId);
            if (gData == null)
            {
                var startingMelodicLeading =
                    MusicianCharacterData.Profile != null
                        ? MusicianCharacterData.Profile.defaultMelodicLeading
                        : null;

                pd.SetMusicianGameplayData(
                    MusicianCharacterData.CharacterId,
                    startingMelodicLeading);
            }

            stats.OnBreakdown += OnBreakdown;
            stats.SetCurrentStress(stats.CurrentStress);

            // M1.2: Wire canvas to SO-based StatusEffectContainer for icon display.
            // Statuses is created in CharacterBase.Awake(), which runs before BuildCharacter.
            bandCharacterCanvas.BindStatusContainer(Statuses);

            bandCharacterCanvas.HideContextual();
        }

        public void BindToGigContext()
        {
            if (_boundToGig) return;
            GigManager.OnPlayerTurnStarted += stats.TriggerAllStatus;
            _boundToGig = true;
        }

        public void UnbindFromGigContext()
        {
            if (!_boundToGig) return;
            GigManager.OnPlayerTurnStarted -= stats.TriggerAllStatus;
            _boundToGig = false;
        }

        private void OnDestroy()
        {
            if (_boundToGig) UnbindFromGigContext();
        }

        public void SetSpriteLayerOrder(int targetOrder)
        {
            SpriteRenderer.sortingOrder = targetOrder;
        }

        protected void OnBreakdown()
        {
            // M1.2: Removed legacy stats.ApplyStatus(StatusType.Breakdown, 1).
            // Breakdown visual is now driven by the SO-based Shaken status applied below,
            // which fires StatusEffectContainer events → CharacterCanvas icon display.

            IsStunned = true;

            // Decision C: Cohesion−1
            var pd = GameManager.PersistentGameplayData;
            if (pd != null)
            {
                pd.BandCohesion = Mathf.Max(0, pd.BandCohesion - 1);

                // Decision D: gig loss on Cohesion ≤ 0
                if (pd.BandCohesion <= 0)
                {
                    GigManager.Instance?.LoseGig();
                    return;
                }
            }

            // Decision C: Stress reset to configurable fraction
            float resetFraction = GigManager.Instance != null
                ? GigManager.Instance.BreakdownStressResetFraction
                : 0.5f;
            int resetTarget = Mathf.FloorToInt(stats.MaxStress * resetFraction);
            stats.SetCurrentStress(resetTarget);

            // Decision C: Apply Shaken via catalogue key lookup
            if (StatusCatalogue != null &&
                StatusCatalogue.TryGetByKey("shaken", out var shakenSO))
            {
                Statuses?.Apply(shakenSO, 1);
            }
            else
            {
                Debug.LogWarning(
                    $"[MusicianBase] Shaken SO not found. " +
                    $"Add 'shaken' to StatusEffectCatalogueSO and assign the catalogue to this musician prefab. " +
                    $"Musician='{name}'");
            }
        }

        protected override void OnPointerEnter()
        {
            base.OnPointerEnter();
            bandCharacterCanvas.ShowContextual();
        }

        protected override void OnPointerExit()
        {
            base.OnPointerExit();
            bandCharacterCanvas.HideContextual();
        }

        public void TriggerNoteVFX(int note, int velocity)
        {
            if (musicianParticleSystem)
            {
                var main = musicianParticleSystem.main;
                main.startSpeed = Mathf.Lerp(0.5f, 3f, velocity / 127f);
                musicianParticleSystem.Play();
            }
        }

        public void TriggerChordVFX(System.Collections.Generic.IList<int> notes)
        {
        }

        public void TriggerBeatVFX(int beatIndex)
        {
        }

        public Coroutine PlayCardOneShotAnimation(CardDefinition card)
        {
            if (card == null) return null;

            var anim = card.MusicianAnimation;
            if (anim == null) return null;

            return StartCoroutine(PlayCardAnimationRoutine(anim));
        }

        private IEnumerator PlayCardAnimationRoutine(CardAnimationData anim)
        {
            if (anim == null) yield break;

            float delay = anim.AnimationDuration;
            if (delay <= 0f)
            {
                delay = 2f;
            }

            if (anim.DisableBeatAnimator && CharacterAnimator != null)
                CharacterAnimator.enabled = false;

            if (Animator != null && !string.IsNullOrEmpty(anim.AnimatorTrigger))
            {
                Animator.ResetTrigger(anim.AnimatorTrigger);
                Animator.SetTrigger(anim.AnimatorTrigger);
            }

            yield return new WaitForSeconds(delay);

            if (anim.DisableBeatAnimator && CharacterAnimator != null)
                CharacterAnimator.enabled = true;
        }
    }
}