using ALWTTT.Actions;
using ALWTTT.Characters.Audience;
using ALWTTT.Extentions;
using MidiGenPlay;
using System;
using System.Collections.Generic;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Data
{
    // TODO: CharacterDataBase class
    [CreateAssetMenu(fileName = " New AudienceCharacterData",
        menuName = "ALWTTT/Characters/AudienceCharacterData")]
    public class AudienceCharacterData : ScriptableObject
    {
        [Header("Base")]
        [SerializeField] private string characterId;
        [SerializeField] private string characterName;
        [SerializeField][TextArea] private string characterDescription;

        [Header("Audience")]
        [SerializeField] private int maxVibe;
        [SerializeField] private AudienceCharacterBase characterPrefab;

        [Header("Abilities")]
        [SerializeField] private bool isTall; // TODO Generalize
        [SerializeField, Tooltip("[S7] The group's boss (punk boss). Target of ActionTargetType.AudienceLeader " +
            "while not convinced.")]
        private bool isLeader;
        [SerializeField] private List<AudienceAbilityData> abilityList;
        [SerializeField] private bool followAbilityPattern;

        [Header("Taste preferences (B3-code-F)")]
        [Tooltip("Per-archetype musical preferences. Empty/disabled axes contribute 0 to the loop impression. " +
                 "Algorithm walks enabled axes, tallies matches/mismatches against the loop's TempoScale, ActiveTracks, " +
                 "TimeSignature, and Tonality, clamps to [-2, +2]. See SSoT_Audience_and_Reactions §6.")]
        [SerializeField] private TastePreferences taste = new TastePreferences();

        [Header("Audio (AUDIO-CHAR-PROFILES)")]
        [Tooltip("Per-character SFX clip source. Consulted by SensoryAudioAdapter before " +
                 "the global SoundBankSO when this character reacts; empty/absent -> bank " +
                 "fallback (per-polarity). Named 'sfxProfile' to avoid colliding with the " +
                 "musician-side music 'profile'. See SSoT_Audio §3.")]
        [SerializeField] private CharacterSfxProfileSO sfxProfile;

        #region Encapsulation
        public string CharacterName => characterName;
        /// <summary>[S8] Stable id (JSON key, shout id 'shout_&lt;characterId&gt;_&lt;abilityId&gt;').</summary>
        public string CharacterId => characterId;
        public AudienceCharacterBase CharacterPrefab => characterPrefab;
        public int MaxVibe => maxVibe;
        public List<AudienceAbilityData> AbilityList => abilityList;
        public bool IsTall => isTall;
        public bool IsLeader => isLeader;
        public TastePreferences Taste => taste;
        public CharacterSfxProfileSO SfxProfile => sfxProfile;
        #endregion

        public AudienceAbilityData GetAbility()
        {
            if (abilityList == null || abilityList.Count == 0)
            {
                Debug.LogError($"Enemy [{characterName}] has no abilities.");
                return null;
            }

            return abilityList.RandomItem();
        }

        public AudienceAbilityData GetAbility(int usedAbilityCount)
        {
            if (followAbilityPattern)
            {
                var index = usedAbilityCount % AbilityList.Count;
                return AbilityList[index];
            }

            return GetAbility();
        }
    }

    [Serializable]
    public class AudienceAbilityData
    {
        [Header("Settings")]
        [SerializeField] private string abilityName;
        [Tooltip("[TEXT-5 / D-T5-1a=A] Stable id of this ability's PLAYER TEXT in ConceptGlossarySO.entries " +
         "(name = tooltip header, description = tooltip body). Lowercase [a-z0-9_], 'ab_' prefix by " +
         "convention. Seed once with ALWTTT ▸ Text ▸ Seed Ability Ids; an empty id is a COVERAGE error. " +
         "abilityName is a developer label since TEXT-5 and reaches no player surface.")]
        [SerializeField] private string abilityId;
        [SerializeField] private AudienceIntentionData intention;
        [SerializeField] private bool hideActionValue;
        [SerializeField] private float abilityDuration;
        [SerializeField] private List<CharacterActionData> actionList;

        [Tooltip("[S7 / Croar] Duet key. Empty = no duet. When another unconvinced audience member's " +
                 "pending ability has the SAME key and hits the SAME musician, each one's AddStress base " +
                 "is doubled (before Hyped). Same rule in StressProjector, so the bar shows it.")]
        [SerializeField] private string duetKey;

        [Header("Presentation")]
        [SerializeField] private AbilityAnimationData animation;

        [Tooltip("[AUDIO-CHAR-PROFILES-2 / D-ABILITY-SFX-HOME=(i)] One-shot SFX fired once " +
                 "when this ability activates, beside the animator trigger in " +
                 "AudienceCharacterBase.AbilityRoutine. Single-source -> IMMEDIATE (not jittered, " +
                 "inv.10). Null -> silent (content gap, inv.3). Independent of 'animation': an " +
                 "ability may have a sound and no animator trigger. See SSoT_Audio §3 / invariant 17.")]
        [SerializeField] private AudioClip abilitySfx;

        [Header("Poses (S8 / GRITOS-1, D3=A)")]
        [Tooltip("[S8] Sprite while this ability is the shown intention (player turn). Empty = normal sprite.")]
        [SerializeField] private Sprite pendingSprite;
        [Tooltip("[S8] Sprite while this ability acts (shout + hits). Empty = normal sprite. " +
                 "An animator trigger that animates the sprite overrides it while it plays.")]
        [SerializeField] private Sprite actingSprite;

        [Tooltip("[S8b] Optional full-screen cut-in shown when this ability fires (the punk boss's " +
                 "'ataque jefe'). A TEXTURE, not a Sprite: the source art is sliced (Multiple) and a RawImage " +
                 "shows the whole sheet. Empty = no cut-in. Once per gig unless GigPresentationSO says otherwise.")]
        [SerializeField] private Texture2D shoutSplash;

        #region Encapsulation
        public string AbilityName => abilityName;
        public string AbilityId => abilityId;
        public AudienceIntentionData Intention => intention;
        public bool HideActionValue => hideActionValue;
        public float AbilityDuration => abilityDuration;
        public List<CharacterActionData> ActionList => actionList;
        public string DuetKey => duetKey;
        public AbilityAnimationData Animation => animation;
        public AudioClip AbilitySfx => abilitySfx;
        public Sprite PendingSprite => pendingSprite;
        public Sprite ActingSprite => actingSprite;
        public Texture2D ShoutSplash => shoutSplash;
        #endregion
    }

    [Serializable]
    public class AbilityAnimationData
    {
        [Header("Animator")]
        [SerializeField] private string animatorTrigger;

        [Tooltip("If > 0, overrides AbilityDuration as the wait time for this animation.")]
        [SerializeField] private float animationDuration = -1f;

        [Tooltip("Disable beat-based CharacterAnimator while this ability plays.")]
        [SerializeField] private bool disableBeatAnimator = true;

        public string AnimatorTrigger => animatorTrigger;
        public float AnimationDuration => animationDuration;
        public bool DisableBeatAnimator => disableBeatAnimator;
    }

    /// <summary>
    /// Inline taste profile authored per AudienceCharacterData asset (B3 D2=A).
    /// Drives AudienceCharacterBase.ResolveLoopEffect's discrete per-axis count
    /// algorithm (B3 D3=A): for each enabled axis, +1 on match, -1 on mismatch,
    /// then clamp the sum to [-2, +2]. Empty / unchecked axes contribute 0.
    ///
    /// Authoring convention:
    /// - Leave everything at defaults → archetype is "neutral" (always returns 0).
    /// - Check tempoMatchOnFast + tempoMismatchOnSlow with threshold = 1.0 →
    ///   archetype prefers above-default tempo, dislikes below-default. (Kid.)
    /// - Populate preferred/dislikedTimeSignatures + preferred/dislikedTonalities →
    ///   archetype reacts to modal / metric choices. (Cool Dude.)
    /// - Check roleCountMatchOnRich with threshold = 3 or 4 →
    ///   archetype rewards full arrangements. (Both Kid and Cool Dude.)
    ///
    /// Promotion path: if archetype count grows past ~5 and inline fields become
    /// hard to manage, promote to a separate AudienceTasteProfileSO and replace
    /// this field with a reference. See D2 design pass.
    /// </summary>
    [Serializable]
    public class TastePreferences
    {
        [Header("Tempo Scale (cumulative TempoEffect ScaleFactor)")]
        [Tooltip("If true, TempoScale > preferAboveTempoScale counts as +1 on the tempo axis.")]
        public bool tempoMatchOnFast;
        [Tooltip("Threshold above which TempoScale is considered 'fast'. 1.0 = above song-authored default.")]
        public float preferAboveTempoScale = 1f;

        [Tooltip("If true, TempoScale < dislikeBelowTempoScale counts as -1 on the tempo axis.")]
        public bool tempoMismatchOnSlow;
        [Tooltip("Threshold below which TempoScale is considered 'slow'. 1.0 = below song-authored default.")]
        public float dislikeBelowTempoScale = 1f;

        [Header("Role count (arrangement density)")]
        [Tooltip("If true, ActiveTracks count >= preferAtLeastRoles counts as +1 on the density axis.")]
        public bool roleCountMatchOnRich;
        [Range(1, 5)]
        public int preferAtLeastRoles = 3;

        [Header("Time signature")]
        [Tooltip("Loop's TimeSignature appearing in this list counts as +1 on the meter axis. Empty = axis disabled on the match side.")]
        public List<TimeSignature> preferredTimeSignatures = new List<TimeSignature>();
        [Tooltip("Loop's TimeSignature appearing in this list counts as -1 on the meter axis. Empty = axis disabled on the mismatch side.")]
        public List<TimeSignature> dislikedTimeSignatures = new List<TimeSignature>();

        [Header("Tonality")]
        [Tooltip("Loop's Tonality appearing in this list counts as +1 on the mode axis. Empty = axis disabled on the match side.")]
        public List<Tonality> preferredTonalities = new List<Tonality>();
        [Tooltip("Loop's Tonality appearing in this list counts as -1 on the mode axis. Empty = axis disabled on the mismatch side.")]
        public List<Tonality> dislikedTonalities = new List<Tonality>();

        [Header("Track roles (S7 / D4=B)")]
        [Tooltip("+1 when ANY of these roles is playing in the loop (e.g. Rayas wants the Bassline). " +
                 "Empty = axis disabled on the match side.")]
        public List<TrackRole> preferredRoles = new List<TrackRole>();
        [Tooltip("-1 when ANY of these roles is playing in the loop. Empty = axis disabled on the mismatch side.")]
        public List<TrackRole> dislikedRoles = new List<TrackRole>();
    }
}