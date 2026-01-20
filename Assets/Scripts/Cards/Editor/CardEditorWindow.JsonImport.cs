#if UNITY_EDITOR
using ALWTTT.Enums;
using ALWTTT.Status;
using System.IO;
using System.Reflection;
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

            public string cardSpritePath;    // optional AssetDatabase path to a Sprite

            public StatusActionJson[] statusActions;

            public ActionJson action;
            public CompositionJson composition;

            public EntryJson entry;          // optional defaults when adding to catalog
        }

        [System.Serializable]
        private class StatusActionJson
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

            CardJsonImport dto;
            try
            {
                dto = JsonUtility.FromJson<CardJsonImport>(json);
            }
            catch (System.Exception ex)
            {
                _jsonImportError = "Invalid JSON: " + ex.Message;
                return;
            }

            if (dto == null)
            {
                _jsonImportError = "Invalid JSON: could not parse payload.";
                return;
            }

            if (string.IsNullOrWhiteSpace(dto.kind) || string.IsNullOrWhiteSpace(dto.id))
            {
                _jsonImportError =
                    "JSON must include at least: " +
                    "{ \"kind\": \"Action|Composition\", \"id\": \"...\" }";
                return;
            }

            // ---------------------------------------------------------------------
            // Step 2A: Reject legacy action.actions usage (only if statusActions missing/empty)
            // ---------------------------------------------------------------------
            bool hasStatusActions = dto.statusActions != null && dto.statusActions.Length > 0;
            bool hasLegacyActions = dto.action != null &&
                                    dto.action.actions != null &&
                                    dto.action.actions.Length > 0;

            if (!hasStatusActions && hasLegacyActions)
            {
                _jsonImportError =
                    "Legacy JSON detected: dto.action.actions is no longer supported.\n" +
                    "Use root-level \"statusActions\" (CSO) instead.";
                return;
            }

            // Create staged instances (NOT assets)
            _stagedJsonCard = CreateInstance<CardDefinition>();
            _stagedJsonCard.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;

            _stagedJsonPayload = CreatePayloadInstance(dto.kind);
            if (_stagedJsonPayload == null)
            {
                _jsonImportError = $"Unknown kind '{dto.kind}'. Use 'Action' or 'Composition'.";
                DiscardStagedJson();
                return;
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

            if (!ApplyStatusActionsJson(
                pso, dto.statusActions, _registries?.StatusCatalogue, out var statusErr))
            {
                _jsonImportError = statusErr;
                DiscardStagedJson();
                return;
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
                        _jsonImportError = $"Invalid entry.flags value: '{dto.entry.flags}'. " +
                                          $"Example: \"UnlockedByDefault,StarterDeck\"";
                        DiscardStagedJson();
                        return;
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

            Repaint();
        }

        private static bool ApplyStatusActionsJson(
            SerializedObject pso,
            StatusActionJson[] actions,
            StatusEffectCatalogueSO catalogue,
            out string error)
        {
            error = null;

            if (actions == null || actions.Length == 0)
                return true;

            var listProp = pso.FindProperty("statusActions");
            if (listProp == null)
            {
                error = "Payload missing 'statusActions' field (expected private field on CardPayload).";
                return false;
            }

            listProp.arraySize = actions.Length;

            for (int i = 0; i < actions.Length; i++)
            {
                var row = actions[i];
                var el = listProp.GetArrayElementAtIndex(i);

                var effectIdProp = el.FindPropertyRelative("effectId");
                var targetTypeProp = el.FindPropertyRelative("targetType");
                var stacksDeltaProp = el.FindPropertyRelative("stacksDelta");
                var delayProp = el.FindPropertyRelative("delay");

                if (effectIdProp == null || targetTypeProp == null || stacksDeltaProp == null || delayProp == null)
                {
                    error = $"statusActions[{i}] structure mismatch (expected fields effectId/targetType/stacksDelta/delay).";
                    return false;
                }

                // --- resolve effect id ---
                int resolvedEffectId = row.effectId;

                if (!string.IsNullOrWhiteSpace(row.statusKey))
                {
                    if (catalogue == null)
                    {
                        error = $"statusActions[{i}]: statusKey provided but StatusCatalogue is not loaded in registries.";
                        return false;
                    }

                    if (!TryResolveEffectIdFromKey(catalogue, row.statusKey, out resolvedEffectId, out var keyErr))
                    {
                        error = $"statusActions[{i}]: {keyErr}";
                        return false;
                    }

                    // if both provided, enforce consistency
                    if (row.effectId >= 0 && row.effectId != resolvedEffectId)
                    {
                        error =
                            $"statusActions[{i}]: effectId ({row.effectId}) does not match statusKey '{row.statusKey}' (resolves to {resolvedEffectId}).";
                        return false;
                    }
                }

                if (!System.Enum.IsDefined(typeof(CharacterStatusId), resolvedEffectId))
                {
                    error = $"statusActions[{i}]: invalid effectId '{resolvedEffectId}' for CharacterStatusId.";
                    return false;
                }

                // targetType parse (string strategy)
                if (!System.Enum.TryParse<ActionTargetType>(row.targetType, true, out var parsedTarget))
                {
                    error = $"statusActions[{i}]: invalid targetType '{row.targetType}' (must match ActionTargetType enum name).";
                    return false;
                }

                // write serialized
                effectIdProp.intValue = resolvedEffectId;
                targetTypeProp.intValue = (int)parsedTarget; // enum stored as int
                stacksDeltaProp.intValue = row.stacksDelta;
                delayProp.floatValue = row.delay;
            }

            pso.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool TryResolveEffectIdFromKey(
            StatusEffectCatalogueSO catalogue,
            string key,
            out int effectId,
            out string err)
        {
            effectId = -1;
            err = null;

            key = key.Trim();

            // Scan catalogue.Effects
            foreach (var se in catalogue.Effects)
            {
                if (se == null) continue;

                // 1) If StatusEffectSO has a StatusKey/statusKey, use it
                var maybeKey = ReadStatusKeyViaReflection(se);
                if (!string.IsNullOrEmpty(maybeKey) &&
                    string.Equals(maybeKey.Trim(), key, System.StringComparison.OrdinalIgnoreCase))
                {
                    effectId = (int)se.EffectId;
                    return true;
                }

                // 2) Fallbacks (optional, but super practical)
                if (string.Equals(se.DisplayName, key, System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(se.name, key, System.StringComparison.OrdinalIgnoreCase))
                {
                    effectId = (int)se.EffectId;
                    return true;
                }
            }

            err = $"No StatusEffectSO found in catalogue for statusKey '{key}'.";
            return false;
        }

        private static string ReadStatusKeyViaReflection(StatusEffectSO se)
        {
            var t = se.GetType();

            // Property: StatusKey
            var p = t.GetProperty("StatusKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.PropertyType == typeof(string))
                return p.GetValue(se) as string;

            // Field: statusKey
            var f = t.GetField("statusKey", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(string))
                return f.GetValue(se) as string;

            return null;
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
            // (ActionCardPayload no longer has an "actions" list; CSO uses root-level "statusActions" instead.)
            if (aj.actions != null && aj.actions.Length > 0)
            {
                Debug.LogWarning(
                    "[CardEditorWindow] JSON import: 'action.actions' is deprecated and ignored. " +
                    "Use root 'statusActions' (CSO) instead.");
            }
        }

        private bool ApplyStatusActionsJson(SerializedObject pso, StatusActionJson[] actions, out string error)
        {
            error = null;

            var statusActionsProp = pso.FindProperty("statusActions");
            if (statusActionsProp == null)
            {
                error = "Payload missing 'statusActions' (expected private field 'statusActions' on CardPayload).";
                return false;
            }

            if (actions == null || actions.Length == 0)
            {
                statusActionsProp.arraySize = 0;
                return true;
            }

            statusActionsProp.arraySize = actions.Length;

            for (int i = 0; i < actions.Length; i++)
            {
                var row = actions[i];
                if (row == null)
                {
                    error = $"statusActions[{i}] is null.";
                    return false;
                }

                // Validate + normalize
                if (row.effectId < 0)
                {
                    error = $"statusActions[{i}].effectId must be a valid CharacterStatusId backing int (>= 0).";
                    return false;
                }

                if (!System.Enum.IsDefined(typeof(CharacterStatusId), row.effectId))
                {
                    error = $"statusActions[{i}].effectId '{row.effectId}' is not a defined CharacterStatusId backing value.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(row.targetType))
                {
                    error = $"statusActions[{i}].targetType is required (ActionTargetType enum name).";
                    return false;
                }

                if (!System.Enum.TryParse<ActionTargetType>(row.targetType, true, out _))
                {
                    error = $"statusActions[{i}].targetType '{row.targetType}' is invalid. " +
                            $"Valid: {string.Join(", ", System.Enum.GetNames(typeof(ActionTargetType)))}";
                    return false;
                }

                if (row.delay < 0f)
                {
                    error = $"statusActions[{i}].delay must be >= 0.";
                    return false;
                }

                // Write serialized fields on StatusEffectActionData: effectId, targetType, stacksDelta, delay
                var el = statusActionsProp.GetArrayElementAtIndex(i);

                var effectProp = el.FindPropertyRelative("effectId");
                if (effectProp != null) effectProp.intValue = row.effectId; // ✅ stable CSO ints

                var targetProp = el.FindPropertyRelative("targetType");
                if (targetProp != null) SetEnumByName(targetProp, row.targetType); // ✅ case-insensitive

                var stacksProp = el.FindPropertyRelative("stacksDelta");
                if (stacksProp != null) stacksProp.intValue = row.stacksDelta;

                var delayProp = el.FindPropertyRelative("delay");
                if (delayProp != null) delayProp.floatValue = row.delay;
            }

            return true;
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