#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

using ALWTTT.Actions;
using ALWTTT.Cards.Effects;
using ALWTTT.Cards.LLMAuthoring;
using ALWTTT.Enums;
using ALWTTT.Musicians;
using ALWTTT.Status;

using MidiGenPlay;
using MidiGenPlay.Composition;

using TimeSignature = MidiGenPlay.MusicTheory.MusicTheory.TimeSignature;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// Assembles the <see cref="CardLLMVocabulary"/> snapshot the prompt builder
    /// reads (CE-L1, D-CE-L1.4): live project state captured at generate time.
    ///
    /// Lives in Assembly-CSharp-Editor on purpose — it is the ONLY place the
    /// LLM pipeline touches ALWTTT game types (enum reflection, status assets);
    /// the <c>ALWTTT.Cards.LLMAuthoring</c> assembly cannot reference them.
    /// Untested glue by design (SSoT_Authoring_LLM_Generation §2: stage-7-side
    /// wiring is smoke-verified); everything it feeds is a string POCO consumed
    /// by unit-tested code.
    /// </summary>
    public static class CardLLMVocabularyBuilder
    {
        /// <summary>
        /// Build the snapshot.
        /// </summary>
        /// <param name="registries">
        /// When supplied (the window's resolved <c>ALWTTTProjectRegistriesSO</c>),
        /// status keys are enumerated from BOTH catalogues (musicians + audience),
        /// so the vocabulary offers exactly the keys the staging path's
        /// <c>TryGetStatusEffectByKey</c> can resolve. When null, falls back to a
        /// project scan of StatusEffectSO assets — at worst that offers an
        /// unregistered key, which staging then rejects with a clear error (never
        /// silently wrong).
        /// </param>
        public static CardLLMVocabulary Build(ALWTTTProjectRegistriesSO registries = null)
        {
            return new CardLLMVocabulary
            {
                PerformerRules = Enum.GetNames(typeof(CardPerformerRule)),
                MusicianTypes = Enum.GetNames(typeof(MusicianCharacterType)),
                CardTypes = Enum.GetNames(typeof(CardType)),
                Rarities = Enum.GetNames(typeof(RarityType)),
                AudioTypes = Enum.GetNames(typeof(AudioActionType)),
                SpecialKeywords = Enum.GetNames(typeof(SpecialKeywords)),
                ActionTargetTypes = Enum.GetNames(typeof(ActionTargetType)),
                ActionTimings = Enum.GetNames(typeof(CardActionTiming)),
                TrackRoles = Enum.GetNames(typeof(TrackRole)),
                PrimaryKinds = Enum.GetNames(typeof(CardPrimaryKind)),
                PartActionKinds = Enum.GetNames(typeof(PartActionKind)),
                AcquisitionFlags = Enum.GetNames(typeof(CardAcquisitionFlags)),
                TimeSignatures = Enum.GetNames(typeof(TimeSignature)),

                StatusKeys = CollectStatusKeys(registries),
                ModifierEffectNames = ScanModifierEffectNames(),

                RhythmPalettes = CardPaletteDescriptorScanner.ScanDrumPalettes(),
                BackingPalettes = CardPaletteDescriptorScanner.ScanChordPalettes()
            };
        }

        /// <summary>
        /// Registry-driven status keys (preferred): both catalogues' effects, so
        /// the alphabet matches exactly what staging can resolve. Falls back to
        /// <see cref="ScanStatusKeys"/> when no registries / catalogues are wired.
        /// </summary>
        private static List<string> CollectStatusKeys(ALWTTTProjectRegistriesSO registries)
        {
            if (registries == null)
                return ScanStatusKeys();

            var keys = new List<string>();
            AppendCatalogueKeys(registries.StatusCatalogueMusicians, keys);
            AppendCatalogueKeys(registries.StatusCatalogueAudience, keys);

            if (keys.Count == 0)
                return ScanStatusKeys(); // registries present but catalogues empty/unwired

            keys.Sort(StringComparer.Ordinal);
            return keys;
        }

        private static void AppendCatalogueKeys(StatusEffectCatalogueSO catalogue, List<string> keys)
        {
            if (catalogue == null || catalogue.Effects == null) return;
            foreach (var se in catalogue.Effects)
            {
                if (se == null) continue;
                if (string.IsNullOrWhiteSpace(se.StatusKey)) continue;
                if (!keys.Contains(se.StatusKey)) keys.Add(se.StatusKey);
            }
        }

        /// <summary>
        /// Fallback: status keys from a project scan of all StatusEffectSO assets.
        /// </summary>
        private static List<string> ScanStatusKeys()
        {
            var keys = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:StatusEffectSO"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<StatusEffectSO>(path);
                if (so == null) continue;
                if (string.IsNullOrWhiteSpace(so.StatusKey)) continue;
                if (!keys.Contains(so.StatusKey)) keys.Add(so.StatusKey);
            }
            keys.Sort(StringComparer.Ordinal);
            return keys;
        }

        /// <summary>
        /// Names of all existing PartEffect assets — the only identifiers the
        /// LLM may use for modifier effects (resolved by exact case-insensitive
        /// name match at staging; never by path/guid).
        /// </summary>
        private static List<string> ScanModifierEffectNames()
        {
            var names = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:PartEffect"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<PartEffect>(path);
                if (so == null) continue;
                if (!names.Contains(so.name)) names.Add(so.name);
            }
            names.Sort(StringComparer.Ordinal);
            return names;
        }
    }
}
#endif
