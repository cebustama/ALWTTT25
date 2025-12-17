using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
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

            // Replace or create MusicianHealthData in PersistentGameplayData structure
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
                // Seed from character profile if not present
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

            //Debug.Log("{MusicianBase} Stats: " + stats.ToString());

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
            // Safety: if this musician was ever bound, unbind to avoid static event leaks.
            if (_boundToGig) UnbindFromGigContext();
        }

        public void SetSpriteLayerOrder(int targetOrder)
        {
            SpriteRenderer.sortingOrder = targetOrder;
            // TODO: Adjust other sprites in the corresponding order
        }

        protected void OnBreakdown()
        {
            // TODO: "Stunned", cannot perform for one turn
            stats.ApplyStatus(StatusType.Breakdown, 1);
            IsStunned = true;
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
                // Simple pulse
                var main = musicianParticleSystem.main;
                main.startSpeed = Mathf.Lerp(0.5f, 3f, velocity / 127f);
                
                musicianParticleSystem.Play();
            }
            // CharacterAnimator?.SetTrigger("Note"); // if you have one
        }

        public void TriggerChordVFX(System.Collections.Generic.IList<int> notes)
        {
            //musicianParticleSystem?.Play();
            // CharacterAnimator?.SetTrigger("Chord");
        }

        public void TriggerBeatVFX(int beatIndex)
        {
            //musicianParticleSystem?.Play();
            // CharacterAnimator?.SetTrigger("Beat");
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
                // Fallback: default duration
                delay = 2f;
            }

            if (anim.DisableBeatAnimator && CharacterAnimator != null)
                CharacterAnimator.enabled = false;

            // Fire Animator trigger if configured
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