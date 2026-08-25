using System;

namespace ALWTTT.Cards.LLMAuthoring
{
    /// <summary>
    /// Card-import DTO schema, hoisted out of <c>CardEditorWindow.JsonImport</c>
    /// (CE-L1 B1) so the LLM generation path and the legacy "Create from JSON"
    /// box parse and stage the exact same shape — the card analog of the chord
    /// surface unifying Generate and Import through one importer
    /// (SSoT_Authoring_LLM_Generation §2 stage 4).
    ///
    /// All fields are plain strings / numbers / bools on purpose: this assembly
    /// cannot reference ALWTTT game types (it is consumed BY Assembly-CSharp,
    /// never the reverse), and string-shaped fields are what an LLM can emit.
    /// Enum-name resolution and asset resolution happen window-side, in the
    /// existing staging path.
    ///
    /// Field names are serialization contract (JsonUtility) — do not rename
    /// without migrating authored JSON snippets.
    /// </summary>
    [Serializable]
    public class CardJsonImport
    {
        public string kind;              // "Action" or "Composition"
        public string id;
        public string displayName;

        public string performerRule;     // e.g. "FixedMusicianType" or "AnyMusician"
        public string fixedMusician;     // e.g. "Cantante" (optional)

        public string cardType;          // e.g. "CHR"
        public string rarity;            // e.g. "Common"
        public string audioType;         // e.g. "Button"

        public int inspirationCost = 1;
        public int inspirationGenerated = 0;

        // [R5-d / D-R5-26=A] Resource cost. Generic (statusKey, amount) pair —
        // NOT a voltage-specific field. Empty key or amount 0 = no cost, which
        // is what every pre-R5-d card imports as.
        public string resourceCostStatusKey;
        public int resourceCostAmount = 0;

        public bool exhaustAfterPlay;
        public bool overrideRequiresTargetSelection;
        public bool requiresTargetSelectionOverrideValue;

        public string[] keywords;        // e.g. ["Exhaust", "Consume"]

        /// <summary>
        /// Optional AssetDatabase path to a Sprite. Accepted from hand-authored
        /// JSON only; LLM-origin payloads carrying this field are hard-rejected
        /// by the response handler (CE-L1 banned-asset-path guard).
        /// </summary>
        public string cardSpritePath;

        // New pipeline: polymorphic card effects (SerializeReference list on CardPayload)
        public EffectJson[] effects;

        // Legacy (rejected): kept only so we can emit a helpful error instead of silently ignoring.
        public LegacyStatusActionJson[] statusActions;

        public ActionJson action;
        public CompositionJson composition;

        public EntryJson entry;          // optional defaults when adding to catalog
    }

    [Serializable]
    public class CardBatchJsonImport
    {
        // Batch wrapper: { "cards": [ { ...CardJsonImport... }, ... ] }
        public CardJsonImport[] cards;

        // Optional: shared entry defaults applied to any card whose own "entry" is null.
        public EntryJson defaultEntry;
    }

    [Serializable]
    public class EffectJson
    {
        // Discriminator. Supported:
        // - "ApplyStatusEffect"
        // - "DrawCards"
        // - "ModifyVibe"
        // - "ModifyStress"
        // - "AddInspirationPerLoop"   [DF-INSPLOOP] Track cards only; uses `amount` as per-loop bonus (>= 1)
        // - "RevealPreferences"        [R4 / D-R0-1] Info-only; uses `targetType` only (no amount/stacks)
        // - "GrantBonusLoop"           [R5-d / D-R0-5=A] Uses `soloOverBonusLoop` only.
        //                              The COST is not here — it lives on the card
        //                              definition (resourceCostStatusKey/Amount).
        public string type;

        // ─ ApplyStatusEffect ─
        public string statusKey;         // resolves to a StatusEffectSO (by StatusKey, DisplayName, or asset name)
        public int effectId = -1;        // optional fallback: CharacterStatusId backing int
        public string targetType;        // ActionTargetType enum name
        public int stacksDelta = 1;
        public float delay = 0f;

        // ─ ModifyVibe / ModifyStress / AddInspirationPerLoop ─
        public int amount = 1;           // vibe/stress delta

        // ─ DrawCards ─
        public int count = 1;

        // ─ GrantBonusLoop ─  [R5-d]
        // Defaults to true: JsonUtility runs the field initializer and only
        // overwrites keys actually present in the JSON, so omitting this gives
        // a solo, matching GrantBonusLoopSpec's own default.
        public bool soloOverBonusLoop = true;
    }

    [Serializable]
    public class LegacyStatusActionJson
    {
        public string statusKey;
        public int effectId = -1;        // legacy fallback
        public string targetType;        // ActionTargetType enum name
        public int stacksDelta = 1;
        public float delay = 0f;
    }

    // Deprecated
    [Serializable]
    public class ActionJson
    {
        public string actionTiming;

        public ConditionJson[] conditions;
        public CharacterActionJson[] actions;
    }

    [Serializable]
    public class ConditionJson
    {
        // Prefer "type" once CardConditionType has values.
        public string type;

        // Fallback while enum is empty / unstable.
        public int typeIndex = -1;

        public float value;
    }

    [Serializable]
    public class CharacterActionJson
    {
        public string actionType;   // matches enum names in payload (by SerializedProperty)
        public string targetType;   // matches enum names in payload (by SerializedProperty)
        public float value;
        public float delay;
    }

    [Serializable]
    public class CompositionJson
    {
        public string primaryKind;

        public TrackActionJson trackAction;
        public PartActionJson partAction;

        /// <summary>
        /// Asset references (path or guid) to existing PartEffect assets.
        /// Hand-authored JSON only; LLM-origin payloads carrying path/guid-shaped
        /// entries here are hard-rejected by the response handler. The LLM path
        /// uses <see cref="modifierEffectNames"/> instead.
        /// </summary>
        public string[] modifierEffects;

        /// <summary>
        /// CE-L1: modifier-effect intent by asset name (case-insensitive exact
        /// match against existing PartEffect assets). Resolved window-side;
        /// ambiguous or missing names fail with a warning — never silently
        /// skipped. Usable from hand-authored JSON too.
        /// </summary>
        public string[] modifierEffectNames;

        /// <summary>
        /// CE-L1: palette intent ("a random 6/8 palette"). Resolved to a concrete
        /// DrumPatternPaletteSO / ChordProgressionPaletteSO (by trackAction.role)
        /// via <see cref="CardPaletteIntentResolver"/> — never by raw asset name.
        /// Null = no palette requested.
        /// </summary>
        public PaletteIntentJson palette;
    }

    /// <summary>
    /// Palette intent emitted by the LLM (or hand-authored). Describes the
    /// palette wanted, not which asset it is; resolution to a real project
    /// palette is deterministic and seeded (CE-F1 PaletteSelector underneath).
    /// </summary>
    [Serializable]
    public class PaletteIntentJson
    {
        /// <summary>
        /// Explicit-presence flag: set true to request a palette. Needed because
        /// Unity's JsonUtility default-constructs absent nested objects, so the
        /// palette object's mere presence is not a signal. Intent is recognized
        /// when this is true OR any filter below has content
        /// (<c>CardLLMResponseHandler.HasPaletteIntent</c>); "any palette, no
        /// filters" is expressed as <c>{"requested": true}</c>.
        /// </summary>
        public bool requested;

        /// <summary>
        /// MidiGenPlay MusicTheory.TimeSignature enum name (e.g. "SixEight").
        /// Empty/null = no meter preference (uniform pick among keyword matches).
        /// </summary>
        public string timeSignature;

        /// <summary>
        /// Optional keywords; ALL must match (case-insensitive substring of the
        /// palette display name + notes). No match = hard failure with the list
        /// of available palettes (no silent fallback).
        /// </summary>
        public string[] keywords;
    }

    [Serializable]
    public class BundleFieldJson
    {
        public string name;   // serialized field name on the bundle SO
        public string value;  // coerced by property type
    }

    [Serializable]
    public class StyleBundleCreateJson
    {
        public bool requested;             // explicit-presence flag (JsonUtility default-constructs nested objects)
        public BundleFieldJson[] fields;   // optional field writes on the minted bundle
    }

    [Serializable]
    public class TrackActionJson
    {
        public string role;              // enum name as shown in inspector dropdown
        public string styleBundle;        // asset path or guid (hand-authored JSON only; banned from LLM output)
        public StyleBundleCreateJson styleBundleCreate;
    }

    [Serializable]
    public class PartActionJson
    {
        public string action;            // enum name as shown in inspector dropdown
        public string customLabel;
        public string musicianId;
    }

    [Serializable]
    public class EntryJson
    {
        public string flags;             // e.g. "UnlockedByDefault,StarterDeck"
        public int starterCopies = 1;
        public string unlockId;
    }
}