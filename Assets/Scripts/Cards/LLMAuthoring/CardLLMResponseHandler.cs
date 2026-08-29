using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using BCS.LLM.Core.Clients;

using TimeSignature = MidiGenPlay.MusicTheory.MusicTheory.TimeSignature;

namespace ALWTTT.Cards.LLMAuthoring
{
    /// <summary>
    /// Async glue between <see cref="CardLLMGenerator"/> and the Card Editor
    /// window (CE-L1 B2, stage 6). Turns a generation request — or a pasted
    /// payload — into a single immutable <see cref="Outcome"/> the window stages
    /// through its EXISTING JSON staging path (<c>TryStageCardFromDto</c>) on the
    /// main thread. Card sibling of <c>ChordProgressionLLMResponseHandler</c>;
    /// Generate and Import converge on <see cref="TranslateJson"/>.
    ///
    /// <para><b>Banned-asset-path guard (the card §3.3 enforcement point,
    /// load-bearing).</b> The staging path resolves <c>cardSpritePath</c>,
    /// <c>trackAction.styleBundle</c> and <c>composition.modifierEffects</c> via
    /// <c>LoadAssetByPathOrGuid</c> and silently skips unresolvable modifier
    /// paths — tolerable for hand-authored JSON, a silent fallback for generated
    /// content. The guard therefore moves up (the D-L4.5 doctrine): any payload
    /// arriving through THIS handler that carries a path/guid-shaped asset
    /// reference is a hard <see cref="OutcomeKind.Failed"/>, never staged.
    /// Asset intent travels only through <c>composition.palette</c> (resolved by
    /// <see cref="CardPaletteIntentResolver"/>) and
    /// <c>composition.modifierEffectNames</c> (name-validated here, resolved at
    /// staging). The legacy "Create from JSON" box bypasses this handler and
    /// keeps its historical behavior.</para>
    ///
    /// <para><b>Alphabet validation.</b> The staging path degrades on unknown
    /// enum tokens (<c>SetEnumByName</c> warns and keeps the default;
    /// <c>TryParseEnum</c> branches silently skip) — again fine for a human
    /// reviewing their own JSON, a silent fallback for generated content. This
    /// handler validates every emitted token against the
    /// <see cref="CardLLMVocabulary"/> alphabets BEFORE staging; any
    /// out-of-alphabet token is a hard failure naming the token. Omitted fields
    /// legitimately take defaults — omission is not a wrong token.</para>
    ///
    /// <para><b>Determinism.</b> Everything downstream of the raw response is
    /// pure: same payload + same vocabulary + same <c>intentSeed</c> ⇒ same
    /// outcome, including the palette pick (seeded through the CE-F1 selector).</para>
    ///
    /// <para><b>Async discipline.</b> Call <see cref="GenerateAsync"/> from an
    /// <c>async void</c> handler and await it — never <c>.Result</c> /
    /// <c>.Wait()</c> / <c>.GetAwaiter().GetResult()</c> on the editor thread.</para>
    /// </summary>
    public static class CardLLMResponseHandler
    {
        public enum OutcomeKind
        {
            /// <summary>The call/parse/guard/validation/resolution failed; nothing to stage.</summary>
            Failed,

            /// <summary>A validated DTO (and resolved palette, if requested) is ready to stage for review.</summary>
            Staged,
        }

        /// <summary>
        /// Everything the window needs to stage a generation or import result.
        /// Immutable; read on the main thread after the await, then routed
        /// through the window's existing staging + Save path.
        /// </summary>
        public readonly struct Outcome
        {
            public readonly OutcomeKind Kind;

            /// <summary>The validated card DTO (valid when Staged).</summary>
            public readonly CardJsonImport Dto;

            /// <summary>True when a palette intent was present and resolved.</summary>
            public readonly bool PaletteResolved;

            /// <summary>Descriptor id (asset path) of the resolved palette.</summary>
            public readonly string ResolvedPaletteId;

            /// <summary>Display name of the resolved palette (for preview text).</summary>
            public readonly string ResolvedPaletteDisplayName;

            /// <summary>Human-readable warning/info lines for the editor warning panel.</summary>
            public readonly IReadOnlyList<string> DisplayWarnings;

            public readonly int InputTokens;
            public readonly int OutputTokens;

            public Outcome(
                OutcomeKind kind,
                CardJsonImport dto,
                bool paletteResolved,
                string resolvedPaletteId,
                string resolvedPaletteDisplayName,
                IReadOnlyList<string> displayWarnings,
                int inputTokens,
                int outputTokens)
            {
                Kind = kind;
                Dto = dto;
                PaletteResolved = paletteResolved;
                ResolvedPaletteId = resolvedPaletteId;
                ResolvedPaletteDisplayName = resolvedPaletteDisplayName;
                DisplayWarnings = displayWarnings ?? Array.Empty<string>();
                InputTokens = inputTokens;
                OutputTokens = outputTokens;
            }

            public bool Success => Kind != OutcomeKind.Failed;
        }

        // -------------------------------------------------------------------
        // Generate path
        // -------------------------------------------------------------------

        /// <summary>
        /// Run an LLM generation and translate the response. Never throws for an
        /// LLM failure — failures come back as <see cref="OutcomeKind.Failed"/>.
        /// </summary>
        /// <param name="intentSeed">Seed for the deterministic palette pick.</param>
        /// <param name="minHarmonicSubdivisions">CE-F1 Tier-B knob (MidiGenPlayConfig value; 4 when unknown).</param>
        public static async Task<Outcome> GenerateAsync(
            ILLMClient client,
            CardLLMVocabulary vocabulary,
            CardLLMPromptBuilder.Input input,
            int intentSeed,
            int minHarmonicSubdivisions = 4)
        {
            CardLLMGenerator.Result gen;
            try
            {
                gen = await CardLLMGenerator.GenerateAsync(client, vocabulary, input);
            }
            catch (Exception ex)
            {
                return Failed($"Generation threw: {ex.GetType().Name}: {ex.Message}");
            }

            if (!gen.Success)
                return Failed($"LLM generation failed: {gen.FailureReason}",
                    gen.InputTokens, gen.OutputTokens);

            return TranslateJson(
                gen.ExtractedJson, vocabulary, intentSeed, minHarmonicSubdivisions,
                gen.InputTokens, gen.OutputTokens);
        }

        // -------------------------------------------------------------------
        // Import path (clipboard / pasted payload)
        // -------------------------------------------------------------------

        /// <summary>
        /// Translate a pasted payload (a full fenced model response or bare
        /// JSON) into an <see cref="Outcome"/>. Synchronous — no LLM call.
        /// The banned-path guard and alphabet validation apply equally; this is
        /// the LLM panel's import, not the legacy JSON box.
        /// </summary>
        public static Outcome FromPayload(
            string payload,
            CardLLMVocabulary vocabulary,
            int intentSeed,
            int minHarmonicSubdivisions = 4)
        {
            string json = CardLLMGenerator.ExtractJsonBlock(payload);
            if (string.IsNullOrWhiteSpace(json))
                return Failed("No JSON object found in the pasted payload.");

            return TranslateJson(json, vocabulary, intentSeed, minHarmonicSubdivisions, 0, 0);
        }

        // -------------------------------------------------------------------
        // Shared translation: parse → guard → validate → resolve intent
        // -------------------------------------------------------------------

        internal static Outcome TranslateJson(
            string json,
            CardLLMVocabulary vocab,
            int intentSeed,
            int minHarmonicSubdivisions,
            int inputTokens,
            int outputTokens)
        {
            var warnings = new List<string>();

            if (vocab == null)
                return Failed("Vocabulary is null; cannot validate the payload.", inputTokens, outputTokens);

            // ---- 1. Parse (the one shared DTO parse) ----
            if (!CardImportDtoParser.TryParse(json, out var dtos, out var parseError))
                return Failed($"Payload parse failed: {parseError}", inputTokens, outputTokens);

            if (dtos == null || dtos.Length == 0)
                return Failed("Payload contains no card.", inputTokens, outputTokens);
            if (dtos.Length > 1)
                return Failed(
                    $"Expected exactly one card; payload contains {dtos.Length}. " +
                    "The LLM panel stages one card at a time.", inputTokens, outputTokens);

            var dto = dtos[0];
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.id))
                errors.Add("Card 'id' is required and is empty/missing.");

            // ---- 2. Banned-asset-path guard ----
            ApplyBannedFieldGuard(dto, errors);

            // ---- 3. Alphabet validation (collect ALL violations, then fail once) ----
            ValidateAlphabets(dto, vocab, errors);

            if (errors.Count > 0)
            {
                warnings.AddRange(errors);
                return new Outcome(OutcomeKind.Failed, null, false, null, null,
                    warnings, inputTokens, outputTokens);
            }

            // ---- 4. Palette intent resolution (deterministic, seeded) ----
            bool paletteResolved = false;
            string paletteId = null;
            string paletteName = null;

            var pal = dto.composition?.palette;
            if (HasPaletteIntent(pal))
            {
                var intentOutcome = ResolvePaletteIntent(
                    dto, vocab, intentSeed, minHarmonicSubdivisions, warnings,
                    out paletteId, out paletteName);
                if (!intentOutcome)
                {
                    return new Outcome(OutcomeKind.Failed, null, false, null, null,
                        warnings, inputTokens, outputTokens);
                }
                paletteResolved = true;
            }

            return new Outcome(OutcomeKind.Staged, dto, paletteResolved, paletteId, paletteName,
                warnings, inputTokens, outputTokens);
        }

        /// <summary>
        /// Whether a palette intent is present. Content-based on purpose: Unity's
        /// JsonUtility default-constructs absent nested objects, so object
        /// presence alone is not a signal. Intent = the explicit
        /// <c>requested</c> flag (covers "any palette, no filters") OR any
        /// filter content.
        /// </summary>
        internal static bool HasPaletteIntent(PaletteIntentJson pal)
        {
            if (pal == null) return false;
            if (pal.requested) return true;
            if (!string.IsNullOrWhiteSpace(pal.timeSignature)) return true;
            if (pal.keywords != null)
            {
                foreach (var k in pal.keywords)
                    if (!string.IsNullOrWhiteSpace(k)) return true;
            }
            return false;
        }

        // -------------------------------------------------------------------
        // Guard
        // -------------------------------------------------------------------

        private static void ApplyBannedFieldGuard(CardJsonImport dto, List<string> errors)
        {
            const string why = " Asset references are resolved by the editor, never emitted by " +
                               "generation (use composition.palette / composition.modifierEffectNames).";

            if (!string.IsNullOrWhiteSpace(dto.cardSpritePath))
                errors.Add("Forbidden field 'cardSpritePath' present." + why);

            var comp = dto.composition;
            if (comp != null)
            {
                if (!string.IsNullOrWhiteSpace(comp.trackAction?.styleBundle))
                    errors.Add("Forbidden field 'composition.trackAction.styleBundle' present." + why);

                // [BASS-CARD-1] styleBundleCreate mints a bundle and can write ANY
                // serialized field on it, including object references resolved from
                // an asset path/guid — the exact channel this guard exists to close.
                // The LLM route does not need it: its bundle is minted by the field
                // plan (ApplyLlmPlanOnSave) and its asset intent travels through
                // composition.palette. Hand-authored JSON only.
                if (HasBundleCreateIntent(comp.trackAction?.styleBundleCreate))
                    errors.Add("Forbidden field 'composition.trackAction.styleBundleCreate' present." + why);

                if (HasAnyContent(comp.modifierEffects))
                    errors.Add("Forbidden field 'composition.modifierEffects' (path/guid refs) present." + why);

                // Path/guid-shaped strings smuggled into the name channel.
                if (comp.modifierEffectNames != null)
                {
                    foreach (var n in comp.modifierEffectNames)
                    {
                        if (string.IsNullOrWhiteSpace(n)) continue;
                        if (LooksLikePathOrGuid(n))
                            errors.Add($"modifierEffectNames entry '{n}' looks like an asset path/guid; " +
                                       "only plain asset names are accepted on this route.");
                    }
                }
            }

            if (dto.statusActions != null && dto.statusActions.Length > 0)
                errors.Add("Forbidden legacy field 'statusActions' present. Use root 'effects'.");

            if (dto.action != null)
            {
                if (dto.action.actions != null && dto.action.actions.Length > 0)
                    errors.Add("Forbidden deprecated field 'action.actions' present. Use root 'effects'.");
                if (dto.action.conditions != null && dto.action.conditions.Length > 0)
                    errors.Add("Field 'action.conditions' is not accepted from generation " +
                               "(its enum alphabet is unstable); author conditions by hand after staging.");
            }
        }

        internal static bool LooksLikePathOrGuid(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Trim();
            if (s.IndexOf('/') >= 0 || s.IndexOf('\\') >= 0) return true;
            if (s.StartsWith("guid:", StringComparison.OrdinalIgnoreCase)) return true;
            if (s.Length == 32)
            {
                bool allHex = true;
                foreach (char c in s)
                {
                    bool hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                    if (!hex) { allHex = false; break; }
                }
                if (allHex) return true;
            }
            return false;
        }

        /// <summary>
        /// [BASS-CARD-1] Content-based presence test, for the same JsonUtility
        /// reason as <see cref="HasPaletteIntent"/>: an absent nested object is
        /// default-constructed, so mere non-nullness is not a signal.
        /// </summary>
        internal static bool HasBundleCreateIntent(StyleBundleCreateJson s)
            => s != null && (s.requested || (s.fields != null && s.fields.Length > 0));

        private static bool HasAnyContent(string[] arr)
        {
            if (arr == null) return false;
            foreach (var s in arr)
                if (!string.IsNullOrWhiteSpace(s)) return true;
            return false;
        }

        // -------------------------------------------------------------------
        // Alphabet validation
        // -------------------------------------------------------------------

        private static void ValidateAlphabets(CardJsonImport dto, CardLLMVocabulary v, List<string> errors)
        {
            // kind is required (staging fails opaquely on an unknown kind).
            if (string.IsNullOrWhiteSpace(dto.kind))
                errors.Add("Card 'kind' is required (Action or Composition).");
            else
                CheckToken(dto.kind, v.CardKinds, "kind", errors);

            CheckToken(dto.performerRule, v.PerformerRules, "performerRule", errors);
            CheckToken(dto.fixedMusician, v.MusicianTypes, "fixedMusician", errors);
            CheckToken(dto.cardType, v.CardTypes, "cardType", errors);
            CheckToken(dto.rarity, v.Rarities, "rarity", errors);
            CheckToken(dto.audioType, v.AudioTypes, "audioType", errors);


            // [R5-e / D-R5-26=A] The resource-cost pair lives on the card definition, not
            // on an effect, so it needs its own alphabet check here. An unresolvable key
            // stages silently: HasResourceCost is true (key non-empty, amount > 0) but no
            // catalogue can resolve it, which is exactly the silent-wrong outcome this
            // guard exists to prevent. Empty passes — a card with no cost is legitimate.
            CheckToken(dto.resourceCostStatusKey, v.StatusKeys, "resourceCostStatusKey", errors);

            if (dto.keywords != null)
                foreach (var kw in dto.keywords)
                    CheckToken(kw, v.SpecialKeywords, "keywords", errors);

            if (dto.action != null)
                CheckToken(dto.action.actionTiming, v.ActionTimings, "action.actionTiming", errors);

            ValidateEffects(dto.effects, v, errors);
            ValidateComposition(dto.composition, v, errors);
            ValidateEntry(dto.entry, v, errors);
        }

        private static void ValidateEffects(EffectJson[] effects, CardLLMVocabulary v, List<string> errors)
        {
            if (effects == null) return;

            for (int i = 0; i < effects.Length; i++)
            {
                var e = effects[i];
                if (e == null) { errors.Add($"effects[{i}] is null."); continue; }

                string type = e.type?.Trim();
                if (string.IsNullOrEmpty(type))
                {
                    errors.Add($"effects[{i}].type is required " +
                       "(ApplyStatusEffect, DrawCards, ModifyVibe, ModifyStress, " +
                       "AddInspirationPerLoop, RevealPreferences, GrantBonusLoop).");
                    continue;
                }

                bool isApplyStatus = type.Equals("ApplyStatusEffect", StringComparison.OrdinalIgnoreCase);
                bool isDraw = type.Equals("DrawCards", StringComparison.OrdinalIgnoreCase);
                bool isVibe = type.Equals("ModifyVibe", StringComparison.OrdinalIgnoreCase);
                bool isStress = type.Equals("ModifyStress", StringComparison.OrdinalIgnoreCase);
                bool isInspLoop = type.Equals("AddInspirationPerLoop", StringComparison.OrdinalIgnoreCase);
                // [R4 / D-R0-1=A] Info-only effect; validated on targetType alone.
                bool isReveal = type.Equals("RevealPreferences", StringComparison.OrdinalIgnoreCase);

                // [R5-e] The R5-d effect. The importer has accepted it since R5-d; this
                // guard did not, so the LLM route hard-failed any card carrying it.
                // Nothing to alphabet-check: the spec carries one bool and no token.
                bool isGrantBonusLoop = type.Equals("GrantBonusLoop", StringComparison.OrdinalIgnoreCase);

                if (!isApplyStatus && !isDraw && !isVibe && !isStress && !isInspLoop && !isReveal &&
                    !isGrantBonusLoop)
                {
                    errors.Add($"effects[{i}].type '{e.type}' is not supported " +
                        "(ApplyStatusEffect, DrawCards, ModifyVibe, ModifyStress, " +
                        "AddInspirationPerLoop, RevealPreferences, GrantBonusLoop).");
                    continue;
                }

                if (isApplyStatus)
                {
                    if (string.IsNullOrWhiteSpace(e.statusKey))
                        errors.Add($"effects[{i}]: ApplyStatusEffect requires 'statusKey' on this route " +
                                   "(numeric effectId is not accepted from generation).");
                    else
                        CheckToken(e.statusKey, v.StatusKeys, $"effects[{i}].statusKey", errors);

                    if (e.delay < 0f)
                        errors.Add($"effects[{i}].delay must be >= 0.");
                }

                if (isDraw && e.count < 0)
                    errors.Add($"effects[{i}].count must be >= 0.");

                if (isInspLoop && e.amount < 1)
                    errors.Add($"effects[{i}].amount must be >= 1 for AddInspirationPerLoop.");

                if (isApplyStatus || isVibe || isStress || isReveal)
                    CheckToken(e.targetType, v.ActionTargetTypes, $"effects[{i}].targetType", errors);
            }
        }

        private static void ValidateComposition(CompositionJson comp, CardLLMVocabulary v, List<string> errors)
        {
            if (comp == null) return;

            CheckToken(comp.primaryKind, v.PrimaryKinds, "composition.primaryKind", errors);
            CheckToken(comp.trackAction?.role, v.TrackRoles, "composition.trackAction.role", errors);
            CheckToken(comp.partAction?.action, v.PartActionKinds, "composition.partAction.action", errors);

            if (comp.modifierEffectNames != null)
            {
                foreach (var n in comp.modifierEffectNames)
                {
                    if (string.IsNullOrWhiteSpace(n) || LooksLikePathOrGuid(n)) continue; // guard reports paths
                    CheckToken(n, v.ModifierEffectNames, "composition.modifierEffectNames", errors);
                }
            }

            var pal = comp.palette;
            if (pal != null && !string.IsNullOrWhiteSpace(pal.timeSignature))
                CheckToken(pal.timeSignature, v.TimeSignatures, "composition.palette.timeSignature", errors);
        }

        private static void ValidateEntry(EntryJson entry, CardLLMVocabulary v, List<string> errors)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.flags)) return;

            var parts = entry.flags.Split(new[] { ',', '|', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in parts)
            {
                var token = raw.Trim();
                // The staging path maps these synonyms; accept what it accepts.
                if (token.Equals("Reward", StringComparison.OrdinalIgnoreCase) ||
                    token.Equals("Rewards", StringComparison.OrdinalIgnoreCase))
                    continue;
                CheckToken(token, v.AcquisitionFlags, "entry.flags", errors);
            }
        }

        /// <summary>Empty/omitted passes (defaults apply); a present token must be in the alphabet.</summary>
        private static void CheckToken(
            string value, IReadOnlyList<string> alphabet, string fieldName, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            string trimmed = value.Trim();
            if (alphabet != null)
            {
                for (int i = 0; i < alphabet.Count; i++)
                {
                    if (string.Equals(alphabet[i], trimmed, StringComparison.OrdinalIgnoreCase))
                        return;
                }
            }

            errors.Add(
                $"Out-of-alphabet token '{value}' for '{fieldName}'. The staging path would " +
                "silently keep the default rather than reject it, so it is blocked here " +
                $"(no silent fallback). Allowed: {(alphabet == null || alphabet.Count == 0 ? "(none)" : string.Join(", ", alphabet))}.");
        }

        // -------------------------------------------------------------------
        // Palette intent
        // -------------------------------------------------------------------

        private static bool ResolvePaletteIntent(
            CardJsonImport dto,
            CardLLMVocabulary vocab,
            int intentSeed,
            int minHarmonicSubdivisions,
            List<string> warnings,
            out string paletteId,
            out string paletteName)
        {
            paletteId = null;
            paletteName = null;

            var comp = dto.composition;
            var pal = comp.palette;

            string role = comp.trackAction?.role?.Trim();
            if (string.IsNullOrEmpty(role))
            {
                warnings.Add("Palette intent requires composition.trackAction.role (Rhythm or Backing).");
                return false;
            }

            IReadOnlyList<PaletteDescriptor> palettes;
            if (role.Equals("Rhythm", StringComparison.OrdinalIgnoreCase))
                palettes = vocab.RhythmPalettes;
            else if (role.Equals("Backing", StringComparison.OrdinalIgnoreCase))
                palettes = vocab.BackingPalettes;
            else
            {
                warnings.Add($"Palette intent is only supported for the Rhythm and Backing roles; " +
                             $"no palette type exists for role '{role}'.");
                return false;
            }

            TimeSignature? desiredTs = null;
            if (!string.IsNullOrWhiteSpace(pal.timeSignature))
            {
                // Already alphabet-validated; TryParse for the typed value.
                if (Enum.TryParse<TimeSignature>(pal.timeSignature.Trim(), ignoreCase: true, out var ts))
                    desiredTs = ts;
                else
                {
                    warnings.Add($"Could not parse timeSignature '{pal.timeSignature}'.");
                    return false;
                }
            }

            var result = CardPaletteIntentResolver.Resolve(
                palettes, desiredTs, pal.keywords, minHarmonicSubdivisions,
                new System.Random(intentSeed));

            foreach (var w in result.Warnings) warnings.Add(w);

            if (!result.Success)
                return false;

            paletteId = result.ChosenId;
            paletteName = result.ChosenDisplayName;
            return true;
        }

        // -------------------------------------------------------------------

        private static Outcome Failed(string reason, int inputTokens = 0, int outputTokens = 0) =>
            new Outcome(OutcomeKind.Failed, null, false, null, null,
                new List<string> { reason }, inputTokens, outputTokens);
    }
}