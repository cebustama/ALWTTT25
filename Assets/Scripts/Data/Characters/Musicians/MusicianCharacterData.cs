using ALWTTT.Cards;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using MidiGenPlay.Composition;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Musicians
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
        [SerializeField] private MusicianCardCatalogData cardCatalog;
        [SerializeField] private Sprite defaultCardSprite; // TEMP: later replace with a sprite catalog


        [Header("Card Animations (WINK-1 D3=B+)")]
        [Tooltip("Fallback one-shot used when the played card has no usable animation " +
                 "(no trigger) and no override matches. Leave trigger empty for 'none'.")]
        [SerializeField] private CardAnimationData defaultCardAnimation;

        [Tooltip("Per-card overrides: THIS musician plays THIS animation for THAT card, " +
                 "beating the card's own MusicianAnimation.")]
        [SerializeField] private List<CardAnimationOverride> cardAnimationOverrides = new();

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

        public MusicianCardCatalogData CardCatalog => cardCatalog;
        public Sprite DefaultCardSprite => defaultCardSprite;


        /// <summary>
        /// [WINK-1 D3=B+] Resolution: override(this musician, card) -> the
        /// card's own animation -> this musician's default.
        ///
        /// "Usable" means HAS A TRIGGER: CardAnimationData is a plain
        /// [Serializable] class, so Unity auto-instantiates it on every
        /// serialized asset — it is effectively NEVER null. A naive null-chain
        /// would let the card's empty auto-instance always win and make
        /// defaultCardAnimation dead on arrival. The FINAL fallback returns the
        /// card's own animation even when trigger-less, preserving today's
        /// observable behavior byte-for-byte (including the routine's
        /// DisableBeatAnimator pause) when nothing new is authored.
        /// </summary>
        public CardAnimationData ResolveCardAnimation(CardDefinition card)
        {
            if (card != null && cardAnimationOverrides != null)
            {
                for (int i = 0; i < cardAnimationOverrides.Count; i++)
                {
                    var o = cardAnimationOverrides[i];
                    if (o != null && o.card == card && HasTrigger(o.animation))
                        return o.animation;
                }
            }

            if (card != null && HasTrigger(card.MusicianAnimation))
                return card.MusicianAnimation;

            if (HasTrigger(defaultCardAnimation))
                return defaultCardAnimation;

            return card != null ? card.MusicianAnimation : null;
        }

        private static bool HasTrigger(CardAnimationData a)
            => a != null && !string.IsNullOrEmpty(a.AnimatorTrigger);

        // Transitional helpers
        public IReadOnlyList<CardDefinition> BaseActionCards =>
            BuildBaseList(isAction: true);
        public IReadOnlyList<CardDefinition> BaseCompositionCards =>
            BuildBaseList(isAction: false);

        public MusicianProfileData Profile => profile;
        public MelodicLeadingConfig DefaultMelodicLeading =>
            profile != null ? profile.defaultMelodicLeading : null;

        public int CHR => chr;
        public int TCH => tch;
        public int EMT => emt;
        #endregion

        /// <summary>[WINK-1 D3=B+] One per-card animation override entry.</summary>
        [System.Serializable]
        public class CardAnimationOverride
        {
            public CardDefinition card;
            public CardAnimationData animation;
        }

        private List<CardDefinition> BuildBaseList(bool isAction)
        {
            var result = new List<CardDefinition>();
            if (cardCatalog == null || cardCatalog.Entries == null) return result;

            foreach (var e in cardCatalog.Entries)
            {
                if (e?.card == null) continue;
                if (!e.IsStarter) continue;

                // Decide whether this entry is relevant for this list
                bool matches =
                    (isAction && e.card.IsAction) ||
                    (!isAction && e.card.IsComposition);

                if (!matches) continue;

                // Add N copies
                int copies = Mathf.Max(1, e.starterCopies);
                for (int i = 0; i < copies; i++)
                    result.Add(e.card);
            }

            return result;
        }
    }
}