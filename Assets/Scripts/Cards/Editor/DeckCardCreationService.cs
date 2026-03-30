#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using ALWTTT.Cards.Effects;
using ALWTTT.Enums;
using ALWTTT.Status;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// Stateless service: creates new CardDefinition + CardPayload assets from a DeckCardEntryJson DTO.
    ///
    /// Folder routing on save:
    ///   FixedMusicianType + valid musician  →  Assets/Resources/Data/Characters/Musicians/{Musician}_Cards/
    ///                                          Payloads go in that folder's /Payloads/ subfolder.
    ///   AnyMusician or None                →  fallbackFolder (deck folder). A warning is logged.
    ///
    /// Folders are created automatically via AssetDatabase if they do not exist.
    /// </summary>
    internal static class DeckCardCreationService
    {
        private const string MusicianCardsBasePath =
            "Assets/Resources/Data/Characters/Musicians";

        // ------------------------------------------------------------------
        // Phase 1 — stage in memory (no disk writes)
        // ------------------------------------------------------------------

        public static bool TryStageNewCard(
            DeckCardEntryJson dto,
            StatusEffectCatalogueSO catalogue,
            out StagedCardEntry entry,
            out string error)
        {
            entry = null;
            error = null;

            if (dto == null) { error = "Card entry DTO is null."; return false; }

            string kind = dto.kind?.Trim();
            string id = dto.id?.Trim();

            if (string.IsNullOrEmpty(kind)) { error = "kind is required. Use 'Action' or 'Composition'."; return false; }
            if (string.IsNullOrEmpty(id)) { error = "id is required for new card creation."; return false; }

            var stagedCard = UnityEngine.ScriptableObject.CreateInstance<CardDefinition>();
            stagedCard.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;

            var stagedPayload = CreatePayloadInstance(kind);
            if (stagedPayload == null)
            {
                UnityEngine.Object.DestroyImmediate(stagedCard);
                error = $"Unknown kind '{kind}'. Use 'Action' or 'Composition'.";
                return false;
            }
            stagedPayload.hideFlags = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy;

            var cardSO = new SerializedObject(stagedCard);

            SetString(cardSO, "id", id);
            SetString(cardSO, "displayName",
                string.IsNullOrWhiteSpace(dto.displayName) ? id : dto.displayName.Trim());

            if (TryParseEnum(dto.performerRule, out CardPerformerRule rule))
                SetEnumDirect(cardSO, "performerRule", (int)rule);

            if (TryParseEnum(dto.fixedMusician, out MusicianCharacterType mType))
                SetEnumDirect(cardSO, "musicianCharacterType", (int)mType);

            if (!string.IsNullOrWhiteSpace(dto.cardSpritePath))
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(dto.cardSpritePath.Trim());
                if (sprite != null) SetObject(cardSO, "cardSprite", sprite);
            }

            SetInt(cardSO, "inspirationCost", Mathf.Max(0, dto.inspirationCost));
            SetInt(cardSO, "inspirationGenerated", Mathf.Max(0, dto.inspirationGenerated));

            if (TryParseEnum(dto.cardType, out CardType ctype)) SetEnumDirect(cardSO, "cardType", (int)ctype);
            if (TryParseEnum(dto.rarity, out RarityType rarity)) SetEnumDirect(cardSO, "rarity", (int)rarity);
            if (TryParseEnum(dto.audioType, out AudioActionType audio)) SetEnumDirect(cardSO, "audioType", (int)audio);

            SetBool(cardSO, "exhaustAfterPlay", dto.exhaustAfterPlay);
            SetBool(cardSO, "overrideRequiresTargetSelection", dto.overrideRequiresTargetSelection);
            SetBool(cardSO, "requiresTargetSelectionOverrideValue", dto.requiresTargetSelectionOverrideValue);
            SetObject(cardSO, "payload", stagedPayload);
            cardSO.ApplyModifiedPropertiesWithoutUndo();

            var pso = new SerializedObject(stagedPayload);

            if (stagedPayload is ActionCardPayload && dto.action != null)
                ApplyActionJson(pso, dto.action);

            if (stagedPayload is CompositionCardPayload && dto.composition != null)
                ApplyCompositionJson(pso, dto.composition);

            if (!ApplyEffectsJson(pso, dto.effects, catalogue, out var effectsErr))
            {
                UnityEngine.Object.DestroyImmediate(stagedCard);
                UnityEngine.Object.DestroyImmediate(stagedPayload);
                error = effectsErr;
                return false;
            }

            pso.ApplyModifiedPropertiesWithoutUndo();

            entry = StagedCardEntry.FromPending(stagedCard, stagedPayload, dto);
            return true;
        }

        // ------------------------------------------------------------------
        // Phase 2 — write to disk
        // ------------------------------------------------------------------

        /// <summary>
        /// Saves pending card + payload to disk.
        ///
        /// The target folder is derived automatically from the card's performer rule:
        ///   - FixedMusicianType (non-None) → musician-specific subfolder under MusicianCardsBasePath.
        ///   - AnyMusician / None           → <paramref name="fallbackFolder"/> (deck folder).
        ///
        /// Missing folders are created via AssetDatabase.CreateFolder.
        /// On any failure after file creation, created files are deleted to prevent orphans.
        /// </summary>
        public static bool SavePendingCard(
            StagedCardEntry entry,
            string fallbackFolder,
            out CardDefinition savedCard,
            out string error)
        {
            savedCard = null;
            error = null;

            if (!entry.IsNew) { error = "Entry is not a pending new card."; return false; }

            var pending = entry.pendingCard;
            var payload = entry.pendingPayload;
            string id = pending.Id;

            if (string.IsNullOrWhiteSpace(id)) { error = "Pending card has an empty Id."; return false; }

            if (string.IsNullOrEmpty(fallbackFolder)) fallbackFolder = "Assets";

            // Resolve target folders
            string cardFolder = ResolveCardFolder(pending, fallbackFolder);
            string payloadFolder = ResolvePayloadFolder(pending, fallbackFolder);

            EnsureFolderExists(cardFolder);
            EnsureFolderExists(payloadFolder);

            string cardPath = AssetDatabase.GenerateUniqueAssetPath($"{cardFolder}/{id}.asset");
            string payloadPath = AssetDatabase.GenerateUniqueAssetPath($"{payloadFolder}/{id}_Payload.asset");

            string createdCardPath = null;
            string createdPayloadPath = null;

            try
            {
                string payloadKind = payload is ActionCardPayload ? "Action" : "Composition";
                var cardAsset = UnityEngine.ScriptableObject.CreateInstance<CardDefinition>();
                var payloadAsset = CreatePayloadInstance(payloadKind);

                if (payloadAsset == null)
                {
                    UnityEngine.Object.DestroyImmediate(cardAsset);
                    error = "Could not create payload asset (unknown kind).";
                    return false;
                }

                EditorUtility.CopySerialized(pending, cardAsset);
                EditorUtility.CopySerialized(payload, payloadAsset);

                AssetDatabase.CreateAsset(payloadAsset, payloadPath);
                createdPayloadPath = payloadPath;

                AssetDatabase.CreateAsset(cardAsset, cardPath);
                createdCardPath = cardPath;

                // Wire card -> persisted payload asset
                var so = new SerializedObject(cardAsset);
                var pp = so.FindProperty("payload");
                if (pp != null)
                {
                    pp.objectReferenceValue = payloadAsset;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                EditorUtility.SetDirty(cardAsset);
                EditorUtility.SetDirty(payloadAsset);

                savedCard = cardAsset;
                return true;
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(createdCardPath)) AssetDatabase.DeleteAsset(createdCardPath);
                if (!string.IsNullOrEmpty(createdPayloadPath)) AssetDatabase.DeleteAsset(createdPayloadPath);
                error = ex.Message;
                Debug.LogException(ex);
                return false;
            }
        }

        // ------------------------------------------------------------------
        // Folder resolution
        // ------------------------------------------------------------------

        /// <summary>
        /// Returns the AssetDatabase path where a card asset should be saved.
        /// FixedMusicianType (non-None) → {MusicianCardsBasePath}/{Musician}_Cards
        /// Anything else                → fallbackFolder (with a warning logged).
        /// </summary>
        private static string ResolveCardFolder(CardDefinition card, string fallbackFolder)
        {
            if (card != null &&
                card.PerformerRule == CardPerformerRule.FixedMusicianType &&
                card.FixedPerformerType != MusicianCharacterType.None)
            {
                return $"{MusicianCardsBasePath}/{card.FixedPerformerType}_Cards";
            }

            // AnyMusician / None — save next to the deck asset
            string performer = card == null ? "unknown" : card.PerformerRule.ToString();
            Debug.LogWarning(
                $"[DeckCardCreationService] Card '{card?.Id}' has performerRule={performer} " +
                $"with no specific musician folder. Saving to fallback: '{fallbackFolder}'.");
            return fallbackFolder;
        }

        /// <summary>
        /// Returns the AssetDatabase path where the payload asset should be saved.
        /// Always a /Payloads/ subfolder of the card folder.
        /// </summary>
        private static string ResolvePayloadFolder(CardDefinition card, string fallbackFolder)
            => ResolveCardFolder(card, fallbackFolder) + "/Payloads";

        /// <summary>
        /// Recursively ensures that an AssetDatabase folder path exists,
        /// creating missing levels one at a time via AssetDatabase.CreateFolder.
        /// </summary>
        private static void EnsureFolderExists(string assetPath)
        {
            assetPath = assetPath.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            int lastSlash = assetPath.LastIndexOf('/');
            if (lastSlash <= 0) return;

            string parent = assetPath.Substring(0, lastSlash);
            string name = assetPath.Substring(lastSlash + 1);

            EnsureFolderExists(parent); // recurse until root exists
            AssetDatabase.CreateFolder(parent, name);
        }

        // ------------------------------------------------------------------
        // Payload factory
        // ------------------------------------------------------------------

        internal static CardPayload CreatePayloadInstance(string kind)
        {
            if (string.Equals(kind, "Action", StringComparison.OrdinalIgnoreCase))
                return UnityEngine.ScriptableObject.CreateInstance<ActionCardPayload>();
            if (string.Equals(kind, "Composition", StringComparison.OrdinalIgnoreCase))
                return UnityEngine.ScriptableObject.CreateInstance<CompositionCardPayload>();
            return null;
        }

        // ------------------------------------------------------------------
        // Effect application
        // ------------------------------------------------------------------

        private static bool ApplyEffectsJson(
            SerializedObject pso,
            DeckEffectJson[] effects,
            StatusEffectCatalogueSO catalogue,
            out string error)
        {
            error = null;

            var listProp = pso.FindProperty("effects");
            if (listProp == null) { error = "Payload missing 'effects' field."; return false; }

            listProp.arraySize = 0;
            if (effects == null || effects.Length == 0) return true;

            for (int i = 0; i < effects.Length; i++)
            {
                var row = effects[i];
                if (row == null) { error = $"effects[{i}] is null."; return false; }
                if (string.IsNullOrWhiteSpace(row.type))
                {
                    error = $"effects[{i}].type is required. Supported: ApplyStatusEffect, DrawCards, ModifyVibe, ModifyStress.";
                    return false;
                }

                string type = row.type.Trim();

                if (type.Equals("ApplyStatusEffect", StringComparison.OrdinalIgnoreCase))
                {
                    if (catalogue == null) { error = $"effects[{i}]: ApplyStatusEffect requires a StatusCatalogue. Assign ALWTTTProjectRegistriesSO in the Registries field."; return false; }

                    StatusEffectSO status;
                    if (!string.IsNullOrWhiteSpace(row.statusKey))
                    {
                        if (!TryResolveStatusByKey(catalogue, row.statusKey, out status, out var re)) { error = $"effects[{i}]: {re}"; return false; }
                    }
                    else if (row.effectId >= 0)
                    {
                        if (!Enum.IsDefined(typeof(CharacterStatusId), row.effectId)) { error = $"effects[{i}]: effectId {row.effectId} is not a valid CharacterStatusId."; return false; }
                        if (!catalogue.TryGet((CharacterStatusId)row.effectId, out status) || status == null) { error = $"effects[{i}]: No StatusEffectSO for effectId {(CharacterStatusId)row.effectId}."; return false; }
                    }
                    else { error = $"effects[{i}]: ApplyStatusEffect requires 'statusKey' or 'effectId'."; return false; }

                    var target = ActionTargetType.Self;
                    if (!string.IsNullOrWhiteSpace(row.targetType) &&
                        !Enum.TryParse<ActionTargetType>(row.targetType, true, out target))
                    { error = $"effects[{i}]: invalid targetType '{row.targetType}'. Valid: {string.Join(", ", Enum.GetNames(typeof(ActionTargetType)))}"; return false; }

                    if (row.delay < 0f) { error = $"effects[{i}]: delay must be >= 0."; return false; }
                    AddManagedEffect(listProp, new ApplyStatusEffectSpec { status = status, targetType = target, stacksDelta = row.stacksDelta, delay = row.delay });
                }
                else if (type.Equals("ModifyVibe", StringComparison.OrdinalIgnoreCase))
                {
                    var target = ActionTargetType.AllAudienceCharacters;
                    if (!string.IsNullOrWhiteSpace(row.targetType) &&
                        !Enum.TryParse<ActionTargetType>(row.targetType, true, out target))
                    { error = $"effects[{i}]: invalid targetType '{row.targetType}'."; return false; }
                    AddManagedEffect(listProp, new ModifyVibeSpec { targetType = target, amount = row.amount });
                }
                else if (type.Equals("ModifyStress", StringComparison.OrdinalIgnoreCase))
                {
                    var target = ActionTargetType.Self;
                    if (!string.IsNullOrWhiteSpace(row.targetType) &&
                        !Enum.TryParse<ActionTargetType>(row.targetType, true, out target))
                    { error = $"effects[{i}]: invalid targetType '{row.targetType}'."; return false; }
                    AddManagedEffect(listProp, new ModifyStressSpec { targetType = target, amount = row.amount });
                }
                else if (type.Equals("DrawCards", StringComparison.OrdinalIgnoreCase))
                {
                    if (row.count < 0) { error = $"effects[{i}]: DrawCards.count must be >= 0."; return false; }
                    AddManagedEffect(listProp, new DrawCardsSpec { count = row.count });
                }
                else
                {
                    error = $"effects[{i}]: unsupported type '{row.type}'. Supported: ApplyStatusEffect, DrawCards, ModifyVibe, ModifyStress.";
                    return false;
                }
            }

            pso.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static void AddManagedEffect(SerializedProperty p, CardEffectSpec spec)
        {
            int i = p.arraySize;
            p.InsertArrayElementAtIndex(i);
            p.GetArrayElementAtIndex(i).managedReferenceValue = spec;
        }

        private static bool TryResolveStatusByKey(StatusEffectCatalogueSO cat, string key, out StatusEffectSO status, out string error)
        {
            status = null; error = null;
            key = key?.Trim();
            if (string.IsNullOrEmpty(key)) { error = "statusKey is empty."; return false; }
            if (cat.TryGetByKey(key, out status) && status != null) return true;

            StatusEffectSO first = null; int count = 0;
            foreach (var se in cat.Effects)
            {
                if (se == null) continue;
                if (string.Equals(se.DisplayName, key, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(se.name, key, StringComparison.OrdinalIgnoreCase))
                { count++; if (first == null) first = se; }
            }
            if (first == null) { error = $"No StatusEffectSO found for statusKey '{key}'."; return false; }
            if (count > 1) Debug.LogWarning($"[DeckCardCreationService] statusKey '{key}' matched {count} effects via name fallback. Using first.");
            status = first;
            return true;
        }

        // ------------------------------------------------------------------
        // Action / Composition JSON
        // ------------------------------------------------------------------

        private static void ApplyActionJson(SerializedObject pso, DeckActionJson aj)
        {
            if (aj == null) return;
            SetEnumByName(pso, "actionTiming", aj.actionTiming);
        }

        private static void ApplyCompositionJson(SerializedObject pso, DeckCompositionJson cj)
        {
            if (cj == null) return;
            SetEnumByName(pso, "primaryKind", cj.primaryKind);

            var trackProp = pso.FindProperty("trackAction");
            if (trackProp != null)
            {
                SetEnumByName(trackProp.FindPropertyRelative("role"), cj.trackAction?.role);
                var sp = trackProp.FindPropertyRelative("styleBundle");
                if (sp != null) sp.objectReferenceValue = LoadAssetByPathOrGuid(cj.trackAction?.styleBundle);
            }

            var partProp = pso.FindProperty("partAction");
            if (partProp != null)
            {
                SetEnumByName(partProp.FindPropertyRelative("action"), cj.partAction?.action);
                var lp = partProp.FindPropertyRelative("customLabel");
                if (lp != null) lp.stringValue = cj.partAction?.customLabel;
                var mp = partProp.FindPropertyRelative("musicianId");
                if (mp != null) mp.stringValue = cj.partAction?.musicianId;
            }

            var modProp = pso.FindProperty("modifierEffects");
            if (modProp != null)
            {
                modProp.arraySize = 0;
                if (cj.modifierEffects != null)
                {
                    for (int i = 0; i < cj.modifierEffects.Length; i++)
                    {
                        var obj = LoadAssetByPathOrGuid(cj.modifierEffects[i]);
                        if (obj == null) continue;
                        int idx = modProp.arraySize;
                        modProp.InsertArrayElementAtIndex(idx);
                        modProp.GetArrayElementAtIndex(idx).objectReferenceValue = obj;
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // SerializedObject helpers
        // ------------------------------------------------------------------

        internal static void SetString(SerializedObject so, string prop, string v)
        { var p = so.FindProperty(prop); if (p != null) p.stringValue = v ?? ""; }

        internal static void SetInt(SerializedObject so, string prop, int v)
        { var p = so.FindProperty(prop); if (p != null) p.intValue = v; }

        internal static void SetBool(SerializedObject so, string prop, bool v)
        { var p = so.FindProperty(prop); if (p != null) p.boolValue = v; }

        internal static void SetEnumDirect(SerializedObject so, string prop, int index)
        { var p = so.FindProperty(prop); if (p != null) p.enumValueIndex = index; }

        internal static void SetObject(SerializedObject so, string prop, UnityEngine.Object obj)
        { var p = so.FindProperty(prop); if (p != null) p.objectReferenceValue = obj; }

        private static void SetEnumByName(SerializedObject so, string propName, string enumName)
        {
            if (string.IsNullOrWhiteSpace(enumName)) return;
            SetEnumByName(so.FindProperty(propName), enumName);
        }

        private static void SetEnumByName(SerializedProperty p, string enumName)
        {
            if (p == null || string.IsNullOrWhiteSpace(enumName)) return;
            if (p.propertyType != SerializedPropertyType.Enum) return;
            var names = p.enumNames;
            if (names == null) return;
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(names[i], enumName, StringComparison.OrdinalIgnoreCase))
                { p.enumValueIndex = i; return; }
            }
            Debug.LogWarning($"[DeckCardCreationService] Enum value '{enumName}' not found for '{p.propertyPath}'.");
        }

        private static bool TryParseEnum<T>(string s, out T value) where T : struct
        {
            value = default;
            return !string.IsNullOrWhiteSpace(s) && Enum.TryParse(s.Trim(), true, out value);
        }

        private static UnityEngine.Object LoadAssetByPathOrGuid(string pathOrGuid)
        {
            if (string.IsNullOrWhiteSpace(pathOrGuid)) return null;
            string s = pathOrGuid.Trim();
            if (s.StartsWith("guid:", StringComparison.OrdinalIgnoreCase)) s = s.Substring(5).Trim();
            if (s.Length == 32 && Regex.IsMatch(s, "^[0-9a-fA-F]{32}$"))
            {
                var p = AssetDatabase.GUIDToAssetPath(s);
                return string.IsNullOrEmpty(p) ? null : AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(p);
            }
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s);
        }
    }
}
#endif