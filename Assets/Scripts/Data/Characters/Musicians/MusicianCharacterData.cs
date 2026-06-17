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