using MidiGenPlay.Composition;
using MidiGenPlay.Composition.Phrases;
using UnityEngine;

namespace ALWTTT.Cards
{
    /// <summary>
    /// ALWTTT card-level authoring bundle for melody tracks.
    /// Lets a card override the default melodic leading config, swap the phrase palette,
    /// and provide a MelodicStyleSO (base strategy + per-phrase directives).
    /// Gameplay attaches this to TrackConfig.Parameters.Style so the composer can read it.
    /// </summary>
    [CreateAssetMenu(menuName = "ALWTTT/Cards/MelodyCardConfig")]
    public class MelodyCardConfigSO : ScriptableObject
    {
        [Header("Leading (optional override)")]
        public MelodicLeadingConfig leadingOverride;     // if null => use default

        [Header("Palette (optional override of leadingOverride/leading default)")]
        public PhrasePaletteSO phrasePaletteOverride;    // if set, it wins

        [Header("Melodic Style (optional)")]
        public MelodicStyleSO style;                     // base + per-phrase directives
    }
}