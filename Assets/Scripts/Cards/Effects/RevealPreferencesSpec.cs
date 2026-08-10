// Place at: Assets/Scripts/Cards/Effects/RevealPreferencesSpec.cs
using System;
using UnityEngine;
using ALWTTT.Enums;

namespace ALWTTT.Cards.Effects
{
    /// <summary>
    /// [R4 / D-R0-1=A] Reveals the target audience member's TastePreferences on
    /// their AudienceCharacterCanvas. Consumer: "Read the Room" (Sibi, RewardPool).
    ///
    /// Deliberately carries NO taste data. The four frozen taste axes live on
    /// AudienceCharacterData.TastePreferences (SSoT_Audience_and_Reactions section
    /// 6.1) and the presentation lives on AudienceCharacterCanvas. A spec that
    /// carried axis text or thresholds would become a second home for the same
    /// truth and would silently desynchronise on the first taste retune.
    ///
    /// Targeting: AudienceCharacter (single, hovered) or AllAudienceCharacters.
    /// Musician targets are an authoring error - CardBase logs and skips them.
    /// The effect is idempotent: re-revealing an already-revealed member is a
    /// silent no-op (AudienceCharacterBase.RevealPreferences early-returns).
    ///
    /// Four-layer conformance (SSoT_Card_Authoring_Contracts section 9):
    ///   1. data      - this file
    ///   2. editor    - CardEditorWindow add-menu + BuildEffectLabel;
    ///                  DeckCardCreationService import branch
    ///   3. import    - CardEditorWindow.JsonImport branch + CardImportDtos
    ///                  discriminator; LLM route via CardLLMPromptBuilder +
    ///                  CardLLMResponseHandler
    ///   4. runtime   - CardBase.ExecuteEffects branch, plus the two targeting
    ///                  derivations (CardDefinition.RequiresTargetSelection and
    ///                  HandController.TryResolveCardTarget) which previously
    ///                  only knew about ApplyStatusEffectSpec.
    /// </summary>
    [Serializable]
    public sealed class RevealPreferencesSpec : CardEffectSpec
    {
        [Tooltip("Who gets revealed. Expected: AudienceCharacter (hovered) or " +
                 "AllAudienceCharacters. Musician targets are logged and skipped.")]
        public ActionTargetType targetType = ActionTargetType.AudienceCharacter;
    }
}