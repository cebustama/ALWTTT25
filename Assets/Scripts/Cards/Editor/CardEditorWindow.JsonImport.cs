#if UNITY_EDITOR
using ALWTTT.Cards.Effects;
using ALWTTT.Enums;
using ALWTTT.Status;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    public sealed partial class CardEditorWindow : EditorWindow
    {
        // JSON import (staged, not saved yet)
        [SerializeField] private bool _showJsonImport = true;
        [SerializeField, TextArea(4, 10)] private string _jsonImportText;

        [SerializeField] private CardDefinition _stagedJsonCard;
        [SerializeField] private CardPayload _stagedJsonPayload;
        [SerializeField] private string _jsonImportError;

        // JSON import entry defaults
        [SerializeField]
        private CardAcquisitionFlags _stagedJsonEntryFlags =
            CardAcquisitionFlags.UnlockedByDefault;
        [SerializeField] private int _stagedJsonStarterCopies = 1;
        [SerializeField] private string _stagedJsonUnlockId;

        [System.Serializable]
        private class CardJsonImport
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

            public bool exhaustAfterPlay;
            public bool overrideRequiresTargetSelection;
            public bool requiresTargetSelectionOverrideValue;

            public string[] keywords;        // e.g. ["Exhaust", "Consume"]

            public string cardSpritePath;    // optional AssetDatabase path to a Sprite

            // New pipeline: polymorphic card effects (SerializeReference list on CardPayload)
            public EffectJson[] effects;

            // Legacy (rejected): kept only so we can emit a helpful error instead of silently ignoring.
            public LegacyStatusActionJson[] statusActions;

            public ActionJson action;
            public CompositionJson composition;

            public EntryJson entry;          // optional defaults when adding to catalog
        }


        [System.Serializable]
        private class CardBatchJsonImport
        {
            // Batch wrapper: { "cards": [ { ...CardJsonImport... }, ... ] }
            public CardJsonImport[] cards;

            // Optional: shared entry defaults applied to any card whose own "entry" is null.
            public EntryJson defaultEntry;
        }


        [System.Serializable]
        private class EffectJson
        {
            // Discriminator. Supported:
            // - "ApplyStatusEffect"
            // - "DrawCards"
            // - "ModifyVibe"
            // - "ModifyStress"
            public string type;

            // ─ ApplyStatusEffect ─
            public string statusKey;         // resolves to a StatusEffectSO (by StatusKey, DisplayName, or asset name)
            public int effectId = -1;        // optional fallback: CharacterStatusId backing int
            public string targetType;        // ActionTargetType enum name
            public int stacksDelta = 1;
            public float delay = 0f;

            // ─ ModifyVibe / ModifyStress ─
            public int amount = 1;           // vibe/stress delta

            // ─ DrawCards ─
            public int count = 1;
        }

        [System.Serializable]
        private class LegacyStatusActionJson
        {
            public string statusKey;
            public int effectId = -1;        // legacy fallback
            public string targetType;        // ActionTargetType enum name
            public int stacksDelta = 1;
            public float delay = 0f;
        }

        // Deprecated
        [System.Serializable]
        private class ActionJson
        {
            public string actionTiming;

            public ConditionJson[] conditions;
            public CharacterActionJson[] actions;
        }

        [System.Serializable]
        private class ConditionJson
        {
            // Prefer "type" once CardConditionType has values.
            public string type;

            // Fallback while enum is empty / unstable.
            public int typeIndex = -1;

            public float value;
        }

        [System.Serializable]
        private class CharacterActionJson
        {
            public string actionType;   // matches enum names in payload (by SerializedProperty)
            public string targetType;   // matches enum names in payload (by SerializedProperty)
            public float value;
            public float delay;
        }

        [System.Serializable]
        private class CompositionJson
        {
            public string primaryKind;

            public TrackActionJson trackAction;
            public PartActionJson partAction;

            // Asset references (path or guid) to existing PartEffect assets.
            public string[] modifierEffects;
        }

        [System.Serializable]
        private class TrackActionJson
        {
            public string role;              // enum name as shown in inspector dropdown
            public string styleBundle;        // asset path or guid
        }

        [System.Serializable]
        private class PartActionJson
        {
            public string action;            // enum name as shown in inspector dropdown
            public string customLabel;
            public string musicianId;
        }

        [System.Serializable]
        private class EntryJson
        {
            public string flags;             // e.g. "UnlockedByDefault,StarterDeck"
            public int starterCopies = 1;
            public string unlockId;
        }

        private void DrawJsonImportBlock()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _showJsonImport =
                    EditorGUILayout.Foldout(_showJsonImport, "Create from JSON", true);
                if (!_showJsonImport) return;

                using (new EditorGUI.DisabledScope(_loadedCatalog == null || _loadedMusicianData == null))
                {
                    _jsonImportText =
                        EditorGUILayout.TextArea(_jsonImportText, GUILayout.MinHeight(70));

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_jsonImportText)))
                        {
                            if (GUILayout.Button("Create from JSON", GUILayout.Width(140)))
                                TryStageCardFromJson(_jsonImportText);
                        }

                        if (GUILayout.Button("Clear", GUILayout.Width(60)))
                        {
                            _jsonImportText = "";
                            _jsonImportError = null;
                        }

                        GUILayout.FlexibleSpace();
                    }
                }

                if (!string.IsNullOrEmpty(_jsonImportError))
                    EditorGUILayout.HelpBox(_jsonImportError, MessageType.Error);

                if (_stagedJsonCard != null)
                {
                    EditorGUILayout.Space(6);
                    EditorGUILayout.HelpBox(
                        "Staged card created in memory. Review/edit below, then click Save to create assets.",
                        MessageType.Info);

                    // (3) Put the staged entry UI in its own small box
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        GUILayout.Label("Catalog Entry Defaults (staged)", EditorStyles.miniBoldLabel);

                        _stagedJsonEntryFlags =
                            (CardAcquisitionFlags)EditorGUILayout.EnumFlagsField("Flags", _stagedJsonEntryFlags);

                        bool isStarter = (_stagedJsonEntryFlags & CardAcquisitionFlags.StarterDeck) != 0;
                        bool unlocked = (_stagedJsonEntryFlags & CardAcquisitionFlags.UnlockedByDefault) != 0;

                        using (new EditorGUI.DisabledScope(!isStarter))
                        {
                            _stagedJsonStarterCopies = EditorGUILayout.IntField("Starter Copies", _stagedJsonStarterCopies);
                            _stagedJsonStarterCopies = Mathf.Max(1, _stagedJsonStarterCopies);
                        }

                        using (new EditorGUI.DisabledScope(unlocked))
                        {
                            _stagedJsonUnlockId = EditorGUILayout.TextField("Unlock Id", _stagedJsonUnlockId);
                        }

                        // Live normalization so the UI stays coherent
                        if (!isStarter) _stagedJsonStarterCopies = 1;
                        if (unlocked) _stagedJsonUnlockId = null;

                        if (!unlocked && string.IsNullOrWhiteSpace(_stagedJsonUnlockId))
                            EditorGUILayout.HelpBox("Locked by default: Unlock Id is required.", MessageType.Warning);
                    }

                    EditorGUILayout.Space(6);

                    // Review/edit staged card fields
                    DrawCardDefinitionCommonFields(_stagedJsonCard);

                    EditorGUILayout.Space(6);
                    DrawPayloadEditors(_stagedJsonCard);

                    // (2) Disable Save when locked but unlockId missing
                    bool unlockedByDefault = (_stagedJsonEntryFlags & CardAcquisitionFlags.UnlockedByDefault) != 0;
                    bool canSave = unlockedByDefault || !string.IsNullOrWhiteSpace(_stagedJsonUnlockId);

                    EditorGUILayout.Space(8);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Discard", GUILayout.Width(90)))
                        {
                            DiscardStagedJson();
                        }

                        GUILayout.FlexibleSpace();

                        using (new EditorGUI.DisabledScope(!canSave))
                        {
                            if (GUILayout.Button("Save (Create Assets)", GUILayout.Width(150)))
                            {
                                SaveStagedJsonToAssetsAndAddToCatalog();
                            }
                        }
                    }
                }
            }
        }
        private void TryStageCardFromJson(string json)
        {
            _jsonImportError = null;

            DiscardStagedJson(); // clear previous

            if (string.IsNullOrWhiteSpace(json))
            {
                _jsonImportError = "Invalid JSON: input is empty.";
                return;
            }

            if (!TryParseJsonToCardDtos(json, out var dtos, out var parseErr))
            {
                _jsonImportError = parseErr;
                return;
            }

            if (dtos == null || dtos.Length == 0)
            {
                _jsonImportError = "Invalid JSON: no card entries found.";
                return;
            }

            // Batch import: immediately create assets for all cards.
            if (dtos.Length > 1)
            {
                TryImportBatchFromDtos(dtos);
                return;
            }

            // Single card: stage in-memory so user can review/edit before saving.
            if (!TryStageCardFromDto(dtos[0], out var stageErr))
            {
                _jsonImportError = stageErr;
                DiscardStagedJson();
                return;
            }

            Repaint();
        }

        private static bool TryParseJsonToCardDtos(string json, out CardJsonImport[] dtos, out string error)
        {
            dtos = null;
            error = null;

            var trimmed = json.TrimStart();
            if (trimmed.StartsWith("["))
            {
                error =
                    "This importer does not accept a raw JSON array at the root.\n\n" +
                    "Use either:\n" +
                    "1) Single card object: { \"kind\": \"Action|Composition\", \"id\": \"...\", ... }\n" +
                    "2) Batch wrapper: { \"cards\": [ { ... }, { ... } ] }";
                return false;
            }

            // Try batch wrapper first: { "cards": [ ... ] }
            try
            {
                var batch = JsonUtility.FromJson<CardBatchJsonImport>(json);
                if (batch != null && batch.cards != null && batch.cards.Length > 0)
                {
                    // Merge batch-level defaultEntry into any card without its own entry.
                    if (batch.defaultEntry != null)
                    {
                        foreach (var card in batch.cards)
                        {
                            if (card != null &&
                                (card.entry == null ||
                                 string.IsNullOrWhiteSpace(card.entry.flags)))
                            {
                                card.entry = batch.defaultEntry;
                            }
                        }
                    }

                    dtos = batch.cards;
                    return true;
                }
            }
            catch
            {
                // ignored; will try single-card parse next
            }

            // Fallback: single card object
            try
            {
                var one = JsonUtility.FromJson<CardJsonImport>(json);
                if (one == null)
                {
                    error = "Invalid JSON: could not parse payload.";
                    return false;
                }

                dtos = new[] { one };
                return true;
            }
            catch (System.Exception ex)
            {
                error = "Invalid JSON: " + ex.Message;
                return false;
            }
        }

        private void TryImportBatchFromDtos(CardJsonImport[] dtos)
        {
            if (dtos == null || dtos.Length == 0)
                return;

            // Preserve user's text; SaveStagedJsonToAssetsAndAddToCatalog clears it.
            var originalJsonText = _jsonImportText;

            int ok = 0;
            var failLines = new System.Collections.Generic.List<string>();

            for (int i = 0; i < dtos.Length; i++)
            {
                var dto = dtos[i];
                var label = dto != null && !string.IsNullOrWhiteSpace(dto.id) ? dto.id : $"cards[{i}]";

                if (!TryStageCardFromDto(dto, out var stageErr))
                {
                    failLines.Add($"{label}: {stageErr}");
                    continue;
                }

                SaveStagedJsonToAssetsAndAddToCatalog();

                // SaveStagedJsonToAssetsAndAddToCatalog clears staged objects on success.
                if (_stagedJsonCard != null || _stagedJsonPayload != null)
                {
                    // Treat as failure (save canceled/failed), then clear staged and keep going.
                    failLines.Add($"{label}: save was canceled or failed (staged card not cleared).");
                    DiscardStagedJson();
                    continue;
                }

                ok++;
                _jsonImportText = originalJsonText;
            }

            _jsonImportText = originalJsonText;

            if (failLines.Count == 0)
            {
                _jsonImportError = null;
                EditorUtility.DisplayDialog(
                    "JSON Batch Import",
                    $"Imported {ok} card(s) successfully.",
                    "OK");
            }
            else
            {
                var previewCount = Mathf.Min(8, failLines.Count);
                var preview = string.Join("\n", failLines.GetRange(0, previewCount));
                var suffix = failLines.Count > previewCount ? $"\n... (+{failLines.Count - previewCount} more)" : "";

                _jsonImportError =
                    $"Batch import finished with errors. Success: {ok}, Failed: {failLines.Count}\n\n" +
                    preview + suffix;

                EditorUtility.DisplayDialog(
                    "JSON Batch Import (errors)",
                    _jsonImportError,
                    "OK");
            }

            Repaint();
        }

        private bool TryStageCardFromDto(CardJsonImport dto, out string error)
        {
            error = null;

            DiscardStagedJson(); // clear previous staged card

            if (dto == null)
            {
                error = "Invalid JSON: could not parse payload.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.kind) || string.IsNullOrWhiteSpace(dto.id))
            {
                error =
                    "JSON must include at least: " +
                    "{ \"kind\": \"Action|Composition\", \"id\": \"...\" }";
                return false;
            }

            // ---------------------------------------------------------------------
            // Step 2A: Reject legacy shapes (we author only into root-level "effects")
            // ---------------------------------------------------------------------
            bool hasEffects = dto.effects != null && dto.effects.Length > 0;
            bool hasLegacyStatusActions = dto.statusActions != null && dto.statusActions.Length > 0;
            bool hasLegacyActions = dto.action != null &&
                                    dto.action.actions != null &&
                                    dto.action.actions.Length > 0;

            if (hasLegacyStatusActions)
            {
                error =
                    "Legacy JSON detected: root-level \"statusActions\" is no longer supported.\n" +
                    "Use root-level \"effects\" (CardEffects) instead.";
                return false;
            }

            if (!hasEffects && hasLegacyActions)
            {
                error =
                    "Legacy JSON detected: dto.action.actions is no longer supported.\n" +
                    "Use root-level \"effects\" (CardEffects) instead.";
                return false;
            }

            // Create staged instances (NOT assets)
            _stagedJsonCard = CreateInstance<CardDefinition>();
            _stagedJsonCard.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;

            _stagedJsonPayload = CreatePayloadInstance(dto.kind);
            if (_stagedJsonPayload == null)
            {
                error = $"Unknown kind '{dto.kind}'. Use 'Action' or 'Composition'.";
                DiscardStagedJson();
                return false;
            }

            _stagedJsonPayload.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;

            // Apply CardDefinition fields via SerializedObject (private SerializeField-safe)
            var cardSO = new SerializedObject(_stagedJsonCard);

            SetString(cardSO, "id", dto.id?.Trim());
            SetString(cardSO, "displayName",
                string.IsNullOrWhiteSpace(dto.displayName) ? dto.id.Trim() : dto.displayName.Trim());

            // performer rule + fixed musician
            if (TryParseEnum(dto.performerRule, out CardPerformerRule rule))
                SetEnum(cardSO, "performerRule", rule);

            // If fixed musician missing, default to selected musician
            var fixedMusicianString = string.IsNullOrWhiteSpace(dto.fixedMusician)
                ? _selectedMusician.ToString()
                : dto.fixedMusician;

            if (TryParseEnum(fixedMusicianString, out MusicianCharacterType mType))
                SetEnum(cardSO, "musicianCharacterType", mType);

            // Sprite (optional path)
            if (!string.IsNullOrWhiteSpace(dto.cardSpritePath))
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(dto.cardSpritePath.Trim());
                if (sprite != null)
                    SetObject(cardSO, "cardSprite", sprite);
            }

            // Fallback: if still missing, use musician default sprite
            var cardSpriteProp = cardSO.FindProperty("cardSprite");
            if (cardSpriteProp != null && cardSpriteProp.objectReferenceValue == null)
            {
                var fallback = _loadedMusicianData != null ? _loadedMusicianData.DefaultCardSprite : null;
                if (fallback != null)
                    SetObject(cardSO, "cardSprite", fallback);
            }

            SetInt(cardSO, "inspirationCost", Mathf.Max(0, dto.inspirationCost));
            SetInt(cardSO, "inspirationGenerated", Mathf.Max(0, dto.inspirationGenerated));

            if (TryParseEnum(dto.cardType, out CardType ctype))
                SetEnum(cardSO, "cardType", ctype);

            if (TryParseEnum(dto.rarity, out RarityType rarity))
                SetEnum(cardSO, "rarity", rarity);

            if (TryParseEnum(dto.audioType, out AudioActionType audio))
                SetEnum(cardSO, "audioType", audio);

            SetBool(cardSO, "exhaustAfterPlay", dto.exhaustAfterPlay);
            SetBool(cardSO, "overrideRequiresTargetSelection", dto.overrideRequiresTargetSelection);
            SetBool(cardSO, "requiresTargetSelectionOverrideValue", dto.requiresTargetSelectionOverrideValue);

            // Keywords
            if (dto.keywords != null && dto.keywords.Length > 0)
            {
                var kwProp = cardSO.FindProperty("keywords");
                if (kwProp != null)
                {
                    kwProp.ClearArray();
                    foreach (var kwStr in dto.keywords)
                    {
                        if (TryParseEnum(kwStr, out SpecialKeywords kw))
                        {
                            kwProp.InsertArrayElementAtIndex(kwProp.arraySize);
                            kwProp.GetArrayElementAtIndex(kwProp.arraySize - 1)
                                .enumValueIndex = (int)kw;
                        }
                        else
                        {
                            Debug.LogWarning(
                                $"[CardEditorWindow] Unknown keyword '{kwStr}' on card '{dto.id}' — skipped.");
                        }
                    }
                }
            }

            // Coherence warning: exhaustAfterPlay bool vs Exhaust keyword
            if (dto.exhaustAfterPlay && (dto.keywords == null ||
                !System.Array.Exists(dto.keywords, k =>
                    string.Equals(k, "Exhaust", System.StringComparison.OrdinalIgnoreCase))))
            {
                Debug.LogWarning(
                    $"[CardEditorWindow] Card '{dto.id}' has exhaustAfterPlay=true " +
                    "but no 'Exhaust' keyword. Players won't see an Exhaust tooltip.");
            }
            else if (!dto.exhaustAfterPlay && dto.keywords != null &&
                System.Array.Exists(dto.keywords, k =>
                    string.Equals(k, "Exhaust", System.StringComparison.OrdinalIgnoreCase)))
            {
                Debug.LogWarning(
                    $"[CardEditorWindow] Card '{dto.id}' has 'Exhaust' keyword " +
                    "but exhaustAfterPlay=false. Tooltip will say Exhaust but card won't exhaust.");
            }

            // Assign payload reference
            SetObject(cardSO, "payload", _stagedJsonPayload);

            // Apply CardDefinition changes
            cardSO.ApplyModifiedPropertiesWithoutUndo();

            // ---------------------------------------------------------------------
            // Apply payload fields
            // ---------------------------------------------------------------------
            var pso = new SerializedObject(_stagedJsonPayload);

            if (_stagedJsonPayload is ActionCardPayload)
            {
                ApplyActionJson(pso, dto.action);
            }
            else if (_stagedJsonPayload is CompositionCardPayload)
            {
                ApplyCompositionJson(pso, dto.composition);
            }

            if (!ApplyEffectsJson(pso, dto.effects, _registries?.StatusCatalogue, out var effectsErr))
            {
                error = effectsErr;
                DiscardStagedJson();
                return false;
            }

            pso.ApplyModifiedPropertiesWithoutUndo();

            // --- Stage catalog entry defaults (optional) ------------------------------
            var entryFlags = CardAcquisitionFlags.UnlockedByDefault;
            var entryStarterCopies = 1;
            string entryUnlockId = null;

            if (dto.entry != null)
            {
                // flags string is optional; if missing -> defaults stay
                if (!string.IsNullOrWhiteSpace(dto.entry.flags))
                {
                    if (!TryParseAcquisitionFlags(dto.entry.flags, out entryFlags))
                    {
                        error = $"Invalid entry.flags value: '{dto.entry.flags}'. Example: \"UnlockedByDefault,StarterDeck\"";
                        DiscardStagedJson();
                        return false;
                    }
                }

                entryStarterCopies = Mathf.Max(1, dto.entry.starterCopies);
                entryUnlockId = dto.entry.unlockId?.Trim();
            }

            // Normalize rules
            bool starter = (entryFlags & CardAcquisitionFlags.StarterDeck) != 0;
            if (!starter) entryStarterCopies = 1;

            bool unlockedByDefault = (entryFlags & CardAcquisitionFlags.UnlockedByDefault) != 0;
            if (unlockedByDefault) entryUnlockId = null;

            // Persist on window state so Save can use it
            _stagedJsonEntryFlags = entryFlags;
            _stagedJsonStarterCopies = entryStarterCopies;
            _stagedJsonUnlockId = entryUnlockId;

            return true;
        }


        private static bool ApplyEffectsJson(
            SerializedObject pso,
            EffectJson[] effects,
            StatusEffectCatalogueSO catalogue,
            out string error)
        {
            error = null;

            var listProp = pso.FindProperty("effects");
            if (listProp == null)
            {
                error = "Payload missing 'effects' field (expected private [SerializeReference] List<CardEffectSpec> effects on CardPayload).";
                return false;
            }

            // Clear first to keep deterministic ordering
            listProp.arraySize = 0;

            if (effects == null || effects.Length == 0)
                return true;

            for (int i = 0; i < effects.Length; i++)
            {
                var row = effects[i];
                if (row == null)
                {
                    error = $"effects[{i}] is null.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(row.type))
                {
                    error = $"effects[{i}].type is required. Supported: ApplyStatusEffect, ModifyVibe, ModifyStress, DrawCards.";
                    return false;
                }

                var type = row.type.Trim();

                if (type.Equals("ApplyStatusEffect", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (catalogue == null)
                    {
                        error = $"effects[{i}]: ApplyStatusEffect requires StatusCatalogue loaded in registries.";
                        return false;
                    }

                    // Resolve StatusEffectSO
                    StatusEffectSO status = null;
                    if (!string.IsNullOrWhiteSpace(row.statusKey))
                    {
                        if (!TryResolveStatusEffectFromKey(catalogue, row.statusKey, out status, out var resolveErr))
                        {
                            error = $"effects[{i}]: {resolveErr}";
                            return false;
                        }
                    }
                    else if (row.effectId >= 0)
                    {
                        if (!System.Enum.IsDefined(typeof(CharacterStatusId), row.effectId))
                        {
                            error = $"effects[{i}]: effectId '{row.effectId}' is not a defined CharacterStatusId backing value.";
                            return false;
                        }

                        if (!catalogue.TryGet((CharacterStatusId)row.effectId, out status) || status == null)
                        {
                            error = $"effects[{i}]: No StatusEffectSO found in catalogue for effectId {(CharacterStatusId)row.effectId}.";
                            return false;
                        }
                    }
                    else
                    {
                        error = $"effects[{i}]: ApplyStatusEffect requires 'statusKey' (preferred) or 'effectId' (CharacterStatusId backing int).";
                        return false;
                    }

                    // targetType (string strategy; defaults to Self)
                    var target = ActionTargetType.Self;
                    if (!string.IsNullOrWhiteSpace(row.targetType) &&
                        !System.Enum.TryParse<ActionTargetType>(row.targetType, true, out target))
                    {
                        error = $"effects[{i}]: invalid targetType '{row.targetType}'. Valid: {string.Join(", ", System.Enum.GetNames(typeof(ActionTargetType)))}";
                        return false;
                    }

                    if (row.delay < 0f)
                    {
                        error = $"effects[{i}]: delay must be >= 0.";
                        return false;
                    }

                    var spec = new ApplyStatusEffectSpec
                    {
                        status = status,
                        targetType = target,
                        stacksDelta = row.stacksDelta,
                        delay = row.delay
                    };

                    AddManagedEffect(listProp, spec);
                }
                else if (type.Equals("ModifyVibe", System.StringComparison.OrdinalIgnoreCase))
                {
                    // targetType (string strategy; defaults to AllAudienceCharacters)
                    var target = ActionTargetType.AllAudienceCharacters;
                    if (!string.IsNullOrWhiteSpace(row.targetType) &&
                        !System.Enum.TryParse<ActionTargetType>(row.targetType, true, out target))
                    {
                        error = $"effects[{i}]: invalid targetType '{row.targetType}'. Valid: {string.Join(", ", System.Enum.GetNames(typeof(ActionTargetType)))}";
                        return false;
                    }

                    var spec = new ModifyVibeSpec
                    {
                        targetType = target,
                        amount = row.amount
                    };

                    AddManagedEffect(listProp, spec);
                }
                else if (type.Equals("ModifyStress", System.StringComparison.OrdinalIgnoreCase))
                {
                    // targetType (string strategy; defaults to Self)
                    var target = ActionTargetType.Self;
                    if (!string.IsNullOrWhiteSpace(row.targetType) &&
                        !System.Enum.TryParse<ActionTargetType>(row.targetType, true, out target))
                    {
                        error = $"effects[{i}]: invalid targetType '{row.targetType}'. Valid: {string.Join(", ", System.Enum.GetNames(typeof(ActionTargetType)))}";
                        return false;
                    }

                    var spec = new ModifyStressSpec
                    {
                        targetType = target,
                        amount = row.amount
                    };

                    AddManagedEffect(listProp, spec);
                }
                else if (type.Equals("DrawCards", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (row.count < 0)
                    {
                        error = $"effects[{i}]: DrawCards.count must be >= 0.";
                        return false;
                    }

                    var spec = new DrawCardsSpec { count = row.count };
                    AddManagedEffect(listProp, spec);
                }
                else
                {
                    error = $"effects[{i}]: unsupported type '{row.type}'. Supported: ApplyStatusEffect, ModifyVibe, ModifyStress, DrawCards.";
                    return false;
                }
            }

            pso.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static void AddManagedEffect(SerializedProperty effectsListProp, CardEffectSpec instance)
        {
            int idx = effectsListProp.arraySize;
            effectsListProp.InsertArrayElementAtIndex(idx);
            var el = effectsListProp.GetArrayElementAtIndex(idx);
            el.managedReferenceValue = instance;
        }

        private static bool TryResolveStatusEffectFromKey(
    StatusEffectCatalogueSO catalogue,
    string key,
    out StatusEffectSO status,
    out string err)
        {
            status = null;
            err = null;

            if (catalogue == null)
            {
                err = "StatusCatalogue is null.";
                return false;
            }

            key = NormalizeStatusLookupKey(key);
            if (string.IsNullOrWhiteSpace(key))
            {
                err = "statusKey is empty.";
                return false;
            }

            // 1) NEW: canonical path — resolve by StatusKey via catalogue index
            // (Esto requiere que StatusEffectCatalogueSO.TryGetByKey sea robusto/case-insensitive; lo veremos en el siguiente paso.)
            if (catalogue.TryGetByKey(key, out status) && status != null)
                return true;

            // 2) LEGACY FALLBACK: DisplayName / asset.name
            StatusEffectSO firstMatch = null;
            int matchCount = 0;

            foreach (var se in catalogue.Effects)
            {
                if (se == null) continue;

                if (string.Equals(se.DisplayName, key, System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(se.name, key, System.StringComparison.OrdinalIgnoreCase))
                {
                    matchCount++;
                    if (firstMatch == null) firstMatch = se;
                }
            }

            if (firstMatch == null)
            {
                err = $"No StatusEffectSO found in catalogue for statusKey '{key}'. " +
                      $"Tried StatusKey first, then DisplayName/asset.name fallback.";
                return false;
            }

            // Warning: se resolvió por ruta legacy (frágil)
            Debug.LogWarning(
                $"[CardEditorWindow] JSON import: '{key}' resolved via legacy name match (DisplayName/asset.name). " +
                $"Prefer using StatusKey='{firstMatch.StatusKey}' in JSON.");

            if (matchCount > 1)
            {
                Debug.LogWarning(
                    $"[CardEditorWindow] Legacy name '{key}' matched {matchCount} StatusEffectSO assets. " +
                    "Using the first match. Prefer StatusKey to avoid ambiguity.");
            }

            status = firstMatch;
            return true;
        }


        private static string NormalizeStatusLookupKey(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        private void ApplyActionJson(SerializedObject pso, ActionJson aj)
        {
            if (pso == null || aj == null) return;

            // actionTiming (enum by name)
            SetEnumByName(pso, "actionTiming", aj.actionTiming);

            // conditions
            var conditionsProp = pso.FindProperty("conditions");
            if (conditionsProp != null)
            {
                conditionsProp.arraySize = 0;

                if (aj.conditions != null)
                {
                    for (int i = 0; i < aj.conditions.Length; i++)
                    {
                        var c = aj.conditions[i];

                        conditionsProp.InsertArrayElementAtIndex(i);
                        var el = conditionsProp.GetArrayElementAtIndex(i);

                        var typeProp = el.FindPropertyRelative("cardConditionType");
                        if (typeProp != null)
                        {
                            if (!string.IsNullOrWhiteSpace(c?.type))
                            {
                                SetEnumByName(typeProp, c.type);
                            }
                            else if (c != null && c.typeIndex >= 0)
                            {
                                typeProp.enumValueIndex =
                                    Mathf.Clamp(c.typeIndex, 0, typeProp.enumNames.Length - 1);
                            }
                        }

                        var valueProp = el.FindPropertyRelative("conditionValue");
                        if (valueProp != null)
                            valueProp.floatValue = c != null ? c.value : 0f;
                    }
                }
            }

            // Legacy field: ActionJson.actions is deprecated and intentionally ignored.
            // (ActionCardPayload no longer has an "actions" list; author card gameplay via root-level "effects" instead.)
            if (aj.actions != null && aj.actions.Length > 0)
            {
                Debug.LogWarning(
                    "[CardEditorWindow] JSON import: 'action.actions' is deprecated and ignored. " +
                    "Use root 'effects' (CardEffects) instead.");
            }
        }

        private void ApplyCompositionJson(SerializedObject pso, CompositionJson cj)
        {
            if (cj == null) return;

            // primaryKind (enum by name)
            SetEnumByName(pso, "primaryKind", cj.primaryKind);

            // trackAction
            var trackProp = pso.FindProperty("trackAction");
            if (trackProp != null)
            {
                SetEnumByName(trackProp.FindPropertyRelative("role"), cj.trackAction?.role);

                var styleProp = trackProp.FindPropertyRelative("styleBundle");
                if (styleProp != null)
                {
                    styleProp.objectReferenceValue = LoadAssetByPathOrGuid(cj.trackAction?.styleBundle);
                }
            }

            // partAction
            var partProp = pso.FindProperty("partAction");
            if (partProp != null)
            {
                SetEnumByName(partProp.FindPropertyRelative("action"), cj.partAction?.action);

                var labelProp = partProp.FindPropertyRelative("customLabel");
                if (labelProp != null) labelProp.stringValue = cj.partAction?.customLabel;

                var musicianIdProp = partProp.FindPropertyRelative("musicianId");
                if (musicianIdProp != null) musicianIdProp.stringValue = cj.partAction?.musicianId;
            }

            // modifierEffects (asset refs)
            var effectsProp = pso.FindProperty("modifierEffects");
            if (effectsProp != null)
            {
                effectsProp.arraySize = 0;

                if (cj.modifierEffects != null)
                {
                    for (int i = 0; i < cj.modifierEffects.Length; i++)
                    {
                        var obj = LoadAssetByPathOrGuid(cj.modifierEffects[i]);
                        if (obj == null) continue;

                        int idx = effectsProp.arraySize;
                        effectsProp.InsertArrayElementAtIndex(idx);
                        effectsProp.GetArrayElementAtIndex(idx).objectReferenceValue = obj;
                    }
                }
            }
        }

        private static void SetEnumByName(SerializedObject so, string propName, string enumName)
        {
            if (so == null || string.IsNullOrWhiteSpace(propName) || string.IsNullOrWhiteSpace(enumName))
                return;

            var p = so.FindProperty(propName);
            SetEnumByName(p, enumName);
        }

        private static void SetEnumByName(SerializedProperty p, string enumName)
        {
            if (p == null || string.IsNullOrWhiteSpace(enumName))
                return;

            if (p.propertyType != SerializedPropertyType.Enum)
                return;

            var names = p.enumNames;
            if (names == null || names.Length == 0)
                return;

            // Case-insensitive match
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(names[i], enumName, System.StringComparison.OrdinalIgnoreCase))
                {
                    p.enumValueIndex = i;
                    return;
                }
            }

            // If not found, do nothing (keep defaults).
            Debug.LogWarning($"[CardEditorWindow] Enum value '{enumName}' not found for '{p.propertyPath}'.");
        }

        private static UnityEngine.Object LoadAssetByPathOrGuid(string pathOrGuid)
        {
            if (string.IsNullOrWhiteSpace(pathOrGuid))
                return null;

            string s = pathOrGuid.Trim();

            // Accept "guid:XXXXXXXX"
            if (s.StartsWith("guid:", System.StringComparison.OrdinalIgnoreCase))
                s = s.Substring("guid:".Length).Trim();

            // Heuristic: GUIDs are 32 hex chars in Unity
            if (s.Length == 32 && System.Text.RegularExpressions.Regex.IsMatch(s, "^[0-9a-fA-F]{32}$"))
            {
                var p = AssetDatabase.GUIDToAssetPath(s);
                if (!string.IsNullOrWhiteSpace(p))
                    return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p);

                return null;
            }

            // Otherwise assume it's an AssetDatabase path
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
        }


        private static bool TryParseAcquisitionFlags(string s, out CardAcquisitionFlags flags)
        {
            flags = 0;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            // Accept "A,B,C" or "A|B|C"
            var parts = s.Split(new[] { ',', '|', ';' },
                System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in parts)
            {
                var token = raw.Trim();

                // Small synonym support
                if (token.Equals("Rewards", System.StringComparison.OrdinalIgnoreCase) ||
                    token.Equals("Reward", System.StringComparison.OrdinalIgnoreCase))
                {
                    token = "RewardPool"; // change if your enum name differs
                }

                if (!System.Enum.TryParse<CardAcquisitionFlags>(token, true, out var one))
                    return false;

                flags |= one;
            }

            return true;
        }

        private CardPayload CreatePayloadInstance(string kind)
        {
            if (string.Equals(kind, "Action", System.StringComparison.OrdinalIgnoreCase))
                return CreateInstance<ActionCardPayload>();

            if (string.Equals(kind, "Composition", System.StringComparison.OrdinalIgnoreCase))
                return CreateInstance<CompositionCardPayload>();

            return null;
        }


        private void SaveStagedJsonToAssetsAndAddToCatalog()
        {
            if (_stagedJsonCard == null || _stagedJsonPayload == null || _loadedCatalog == null)
                return;

            // Save next to the catalog by default
            var catalogPath = AssetDatabase.GetAssetPath(_loadedCatalog);
            var folder = Path.GetDirectoryName(catalogPath) ?? "Assets";

            // Use staged id as filename
            string id = _stagedJsonCard.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                EditorUtility.DisplayDialog(
                    "Save JSON Card",
                    "Staged card has empty Id.",
                    "OK");
                return;
            }

            // --- IMPORTANT: validate entry defaults BEFORE creating assets -------------
            var flags = _stagedJsonEntryFlags;

            bool starter = (flags & CardAcquisitionFlags.StarterDeck) != 0;
            int starterCopies = starter ? Mathf.Max(1, _stagedJsonStarterCopies) : 1;

            bool unlockedByDefault = (flags & CardAcquisitionFlags.UnlockedByDefault) != 0;
            string unlockId = unlockedByDefault ? null : (_stagedJsonUnlockId?.Trim());

            if (!unlockedByDefault && string.IsNullOrWhiteSpace(unlockId))
            {
                EditorUtility.DisplayDialog(
                    "Save JSON Card",
                    "This card is not UnlockedByDefault, but Unlock Id is empty.\n\n" +
                    "Add an unlockId in JSON (entry.unlockId) " +
                    "or include UnlockedByDefault in entry.flags.",
                    "OK");
                return;
            }

            // -------------------------------------------------------------------------

            string cardPath =
                AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, $"{id}.asset"));
            string payloadPath =
                AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, $"{id}_Payload.asset"));

            string createdCardPath = null;
            string createdPayloadPath = null;

            try
            {
                // Create real asset instances
                var cardAsset = CreateInstance<CardDefinition>();
                var payloadAsset =
                    CreatePayloadInstance(_stagedJsonPayload is ActionCardPayload ? "Action" : "Composition");

                if (payloadAsset == null)
                {
                    DestroyImmediate(cardAsset);
                    EditorUtility.DisplayDialog(
                        "Save JSON Card",
                        "Could not create payload instance (unknown kind).",
                        "OK");
                    return;
                }

                // Copy serialized data
                EditorUtility.CopySerialized(_stagedJsonCard, cardAsset);
                EditorUtility.CopySerialized(_stagedJsonPayload, payloadAsset);

                // Create assets ON DISK
                AssetDatabase.CreateAsset(payloadAsset, payloadPath);
                createdPayloadPath = payloadPath;

                AssetDatabase.CreateAsset(cardAsset, cardPath);
                createdCardPath = cardPath;

                // Ensure card points to the saved payload asset (not staged)
                var so = new SerializedObject(cardAsset);
                var payloadProp = so.FindProperty("payload");
                if (payloadProp != null)
                {
                    payloadProp.objectReferenceValue = payloadAsset;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                // Add to catalog
                if (!MusicianCatalogService.TryAddEntry(
                        _loadedCatalog,
                        cardAsset,
                        flags,
                        starterCopies,
                        unlockId,
                        out var newIndex,
                        out var err))
                {
                    // Prevent orphaned assets if catalog add fails
                    AssetDatabase.DeleteAsset(createdCardPath);
                    AssetDatabase.DeleteAsset(createdPayloadPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    EditorUtility.DisplayDialog(
                        "Save JSON Card",
                        "Could not add to catalog (assets were not kept):\n\n" + err,
                        "OK");
                    return;
                }

                _selectedEntryIndex = newIndex;
                Selection.activeObject = cardAsset;
                EditorGUIUtility.PingObject(cardAsset);

                EditorUtility.SetDirty(_loadedCatalog);
                EditorUtility.SetDirty(cardAsset);
                EditorUtility.SetDirty(payloadAsset);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                DiscardStagedJson();
                _jsonImportText = "";

                Repaint();
            }
            catch (System.Exception ex)
            {
                // Best-effort cleanup if anything failed after assets were created
                if (!string.IsNullOrEmpty(createdCardPath))
                    AssetDatabase.DeleteAsset(createdCardPath);

                if (!string.IsNullOrEmpty(createdPayloadPath))
                    AssetDatabase.DeleteAsset(createdPayloadPath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.LogException(ex);
                EditorUtility.DisplayDialog(
                    "Save JSON Card",
                    "An exception occurred while saving JSON card.\n\nSee Console for details.",
                    "OK");
            }
        }

        private void DiscardStagedJson()
        {
            if (_stagedJsonCard != null) DestroyImmediate(_stagedJsonCard);
            if (_stagedJsonPayload != null) DestroyImmediate(_stagedJsonPayload);

            _stagedJsonCard = null;
            _stagedJsonPayload = null;

            _stagedJsonEntryFlags = CardAcquisitionFlags.UnlockedByDefault;
            _stagedJsonStarterCopies = 1;
            _stagedJsonUnlockId = null;
        }
    }
}
#endif