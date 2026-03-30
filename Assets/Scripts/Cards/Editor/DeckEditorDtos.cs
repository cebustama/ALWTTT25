#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    // =========================================================================
    // JSON import DTO
    //
    // Root shape expected:
    //   { "deckId": "...", "displayName": "...", "cards": [ ... ] }
    //
    // Each card entry can be one of two forms:
    //
    //   1) Reference existing card (kind is absent / empty):
    //      { "cardId": "my_existing_id" }
    //      { "cardId": "my_id", "assetPath": "Assets/Scripts/Cards/..." }
    //
    //   2) Create new card (kind is present):
    //      {
    //        "kind": "Action",
    //        "id": "my_new_card_id",
    //        "displayName": "My New Card",
    //        "inspirationCost": 2,
    //        "effects": [ { "type": "ModifyVibe", "amount": 3 } ]
    //      }
    //
    // Discriminator: non-empty "kind" field → create new card.
    // =========================================================================

    [Serializable]
    internal class DeckJsonDto
    {
        public string deckId;
        public string displayName;
        public string description;
        public DeckCardEntryJson[] cards;
    }

    /// <summary>
    /// Unified card entry DTO.
    /// Handles both "reference existing" and "create new" in one structure.
    /// </summary>
    [Serializable]
    internal class DeckCardEntryJson
    {
        // --- Reference existing ---
        public string cardId;       // matches CardDefinition.Id
        public string assetPath;    // fallback: explicit AssetDatabase path

        // --- Create new card (all fields below ignored if kind is empty) ---
        public string kind;         // "Action" or "Composition" — discriminator
        public string id;
        public string displayName;
        public string performerRule;
        public string fixedMusician;
        public string cardType;
        public string rarity;
        public string audioType;
        public int    inspirationCost      = 1;
        public int    inspirationGenerated = 0;
        public bool   exhaustAfterPlay;
        public bool   overrideRequiresTargetSelection;
        public bool   requiresTargetSelectionOverrideValue;
        public string cardSpritePath;

        public DeckEffectJson[]      effects;
        public DeckActionJson        action;
        public DeckCompositionJson   composition;
    }

    // -------------------------------------------------------------------------
    // Effect / action / composition DTOs (mirrors CardEditorWindow internals,
    // but owned by the deck tool to avoid coupling to CardEditorWindow's
    // private classes)
    // -------------------------------------------------------------------------

    [Serializable]
    internal class DeckEffectJson
    {
        // Discriminator. Supported: ApplyStatusEffect, DrawCards, ModifyVibe, ModifyStress.
        public string type;

        // ApplyStatusEffect
        public string statusKey;
        public int    effectId   = -1;
        public string targetType;
        public int    stacksDelta = 1;
        public float  delay       = 0f;

        // ModifyVibe / ModifyStress
        public int amount = 1;

        // DrawCards
        public int count = 1;
    }

    [Serializable]
    internal class DeckActionJson
    {
        public string actionTiming;
    }

    [Serializable]
    internal class DeckCompositionJson
    {
        public string              primaryKind;
        public DeckTrackActionJson trackAction;
        public DeckPartActionJson  partAction;
        public string[]            modifierEffects;
    }

    [Serializable]
    internal class DeckTrackActionJson
    {
        public string role;
        public string styleBundle; // asset path or guid
    }

    [Serializable]
    internal class DeckPartActionJson
    {
        public string action;
        public string customLabel;
        public string musicianId;
    }

    // =========================================================================
    // In-memory staged deck
    // =========================================================================

    /// <summary>
    /// Represents a single card slot in the staged deck.
    /// Two modes:
    ///   - Existing: <see cref="existingCard"/> is non-null (serialized, survives domain reload).
    ///   - New/Pending: <see cref="pendingCard"/> and <see cref="pendingPayload"/> are non-null
    ///     (in-memory ScriptableObjects, NOT serialized — lost on domain reload).
    /// </summary>
    [Serializable]
    internal class StagedCardEntry
    {
        // Survives domain reload (it is a project asset reference).
        [SerializeField] public CardDefinition existingCard;

        // In-memory only. HideFlags.DontSaveInEditor on both objects.
        // Lost if Unity reloads scripts. The window warns about this.
        [NonSerialized] public CardDefinition  pendingCard;
        [NonSerialized] public CardPayload     pendingPayload;
        [NonSerialized] public DeckCardEntryJson pendingDto; // original dto for export roundtrip

        public bool IsNew      => existingCard == null && pendingCard != null;
        public bool IsExisting => existingCard != null;
        public bool IsValid    => IsNew || IsExisting;

        public CardDefinition ResolvedCard =>
            existingCard != null ? existingCard : pendingCard;

        public static StagedCardEntry FromExisting(CardDefinition card)
            => new StagedCardEntry { existingCard = card };

        public static StagedCardEntry FromPending(
            CardDefinition staged, CardPayload payload, DeckCardEntryJson dto)
            => new StagedCardEntry
            {
                pendingCard    = staged,
                pendingPayload = payload,
                pendingDto     = dto
            };
    }

    /// <summary>
    /// The live in-memory deck the window operates on.
    /// Written to a BandDeckData asset only on explicit Save.
    /// </summary>
    [Serializable]
    internal class StagedDeck
    {
        public string deckId       = "";
        public string displayName  = "";
        public string description  = "";
        public List<StagedCardEntry> cards = new();

        [NonSerialized] public bool          isDirty;
        [NonSerialized] public BandDeckData  sourceAsset;

        public bool HasPendingNewCards
        {
            get
            {
                if (cards == null) return false;
                foreach (var e in cards) if (e.IsNew) return true;
                return false;
            }
        }
    }

    // =========================================================================
    // Result types returned by services
    // =========================================================================

    internal enum ImportResultStatus { Ok, OkWithWarnings, Failed }

    internal sealed class ImportResult
    {
        public ImportResultStatus Status;
        public StagedDeck         StagedDeck;
        public List<string>       Errors   = new();
        public List<string>       Warnings = new();

        public bool HasErrors   => Errors?.Count   > 0;
        public bool HasWarnings => Warnings?.Count > 0;
    }

    internal enum ValidationResultStatus { Valid, Invalid }

    internal sealed class ValidationResult
    {
        public ValidationResultStatus Status;
        public List<string> Errors   = new();
        public List<string> Warnings = new();

        public bool IsValid     => Status == ValidationResultStatus.Valid;
        public bool HasErrors   => Errors?.Count   > 0;
        public bool HasWarnings => Warnings?.Count > 0;
    }

    internal enum SaveResultStatus { Saved, Failed }

    internal sealed class SaveResult
    {
        public SaveResultStatus Status;
        public BandDeckData     SavedAsset;
        public string           Error;
        public bool             Succeeded => Status == SaveResultStatus.Saved;
    }
}
#endif
