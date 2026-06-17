using System;
using System.Collections.Generic;
using System.Text;

namespace ALWTTT.Cards.LLMAuthoring
{
    /// <summary>
    /// Pure-function prompt builder for the card LLM adopter (CE-L1 B2, stage 2
    /// of SSoT_Authoring_LLM_Generation §2). Takes a <see cref="CardLLMVocabulary"/>
    /// snapshot + an <see cref="Input"/>, returns a system+user prompt pair or a
    /// typed failure. No I/O, no Unity calls; fully unit-testable.
    ///
    /// The system prompt declares the JSON output contract (one fenced block,
    /// one <see cref="CardJsonImport"/> object), every enum alphabet from the
    /// vocabulary, the available status keys / modifier-effect names / palettes,
    /// and the FORBIDDEN asset-reference fields the response handler will
    /// hard-reject (the banned-asset-path guard, D-CE-L1.3/.5).
    ///
    /// Cost cap is pre-network (SSoT §3.6): an over-budget prompt fails here,
    /// before anything is sent.
    /// </summary>
    public static class CardLLMPromptBuilder
    {
        /// <summary>Builder input. Immutable.</summary>
        public readonly struct Input
        {
            /// <summary>The natural-language brief (required).</summary>
            public readonly string Brief;

            /// <summary>Optional card-kind hint ("Action"/"Composition"); null = model decides from the brief.</summary>
            public readonly string KindHint;

            /// <summary>Optional track-role hint (TrackRole name); null = model decides.</summary>
            public readonly string RoleHint;

            /// <summary>Optional default musician (window's selected musician) for fixedMusician context.</summary>
            public readonly string DefaultMusician;

            /// <summary>Pre-network char budget over system+user prompts. 0 = no cap.</summary>
            public readonly int MaxCharBudget;

            public Input(
                string brief,
                string kindHint = null,
                string roleHint = null,
                string defaultMusician = null,
                int maxCharBudget = 0)
            {
                Brief = brief;
                KindHint = kindHint;
                RoleHint = roleHint;
                DefaultMusician = defaultMusician;
                MaxCharBudget = maxCharBudget;
            }
        }

        /// <summary>Builder outcome. Mirrors the chord builder's Result shape.</summary>
        public readonly struct Result
        {
            public readonly bool Success;
            public readonly string SystemPrompt;
            public readonly string UserPrompt;
            public readonly string FailureReason;
            public readonly int TotalCharCount;

            private Result(bool success, string system, string user, string failure, int total)
            {
                Success = success;
                SystemPrompt = system;
                UserPrompt = user;
                FailureReason = failure;
                TotalCharCount = total;
            }

            public static Result Ok(string system, string user) =>
                new Result(true, system, user, null,
                    (system?.Length ?? 0) + (user?.Length ?? 0));

            public static Result Fail(string reason) =>
                new Result(false, null, null, reason, 0);
        }

        public static Result Build(CardLLMVocabulary vocab, Input input)
        {
            if (vocab == null)
                return Result.Fail("Vocabulary is null. Build one via CardLLMVocabularyBuilder.");
            if (string.IsNullOrWhiteSpace(input.Brief))
                return Result.Fail("Brief is empty. Describe the card to author.");

            string system = BuildSystemPrompt(vocab);
            string user = BuildUserPrompt(input);

            int total = system.Length + user.Length;
            if (input.MaxCharBudget > 0 && total > input.MaxCharBudget)
            {
                return Result.Fail(
                    $"Prompt exceeds the char budget: {total} > {input.MaxCharBudget}. " +
                    "Nothing was sent. Raise the budget or trim the brief/vocabulary.");
            }

            return Result.Ok(system, user);
        }

        // -------------------------------------------------------------------
        // System prompt
        // -------------------------------------------------------------------

        private static string BuildSystemPrompt(CardLLMVocabulary v)
        {
            var sb = new StringBuilder(4096);

            sb.AppendLine("You author playing cards for the game ALWTTT.");
            sb.AppendLine();
            sb.AppendLine("OUTPUT CONTRACT");
            sb.AppendLine("Respond with EXACTLY ONE card as a single JSON object inside ONE fenced code block:");
            sb.AppendLine("```json");
            sb.AppendLine("{ ... }");
            sb.AppendLine("```");
            sb.AppendLine("One object only — no batch wrapper, no array, no commentary inside the fence.");
            sb.AppendLine("Omit any optional field to accept its default. Never invent field names.");
            sb.AppendLine();

            sb.AppendLine("SCHEMA (allowed values in angle brackets; all enum values are exact names)");
            sb.AppendLine("{");
            sb.AppendLine("  \"kind\": <" + Join(v.CardKinds) + ">              (required)");
            sb.AppendLine("  \"id\": string                                      (required; short snake_case, e.g. \"crd_flow_draw\")");
            sb.AppendLine("  \"displayName\": string");
            sb.AppendLine("  \"performerRule\": <" + Join(v.PerformerRules) + ">");
            sb.AppendLine("  \"fixedMusician\": <" + Join(v.MusicianTypes) + ">");
            sb.AppendLine("  \"cardType\": <" + Join(v.CardTypes) + ">");
            sb.AppendLine("  \"rarity\": <" + Join(v.Rarities) + ">");
            sb.AppendLine("  \"audioType\": <" + Join(v.AudioTypes) + ">");
            sb.AppendLine("  \"inspirationCost\": int >= 0");
            sb.AppendLine("  \"inspirationGenerated\": int >= 0");
            sb.AppendLine("  \"exhaustAfterPlay\": bool   (if true, also add the \"Exhaust\" keyword)");
            sb.AppendLine("  \"keywords\": [<" + Join(v.SpecialKeywords) + ">]");
            sb.AppendLine("  \"effects\": [ see EFFECTS ]");
            sb.AppendLine("  \"action\": { \"actionTiming\": <" + Join(v.ActionTimings) + "> }       (Action cards only)");
            sb.AppendLine("  \"composition\": { see COMPOSITION }                                    (Composition cards only)");
            sb.AppendLine("  \"entry\": { \"flags\": \"comma-separated <" + Join(v.AcquisitionFlags) + ">\", \"starterCopies\": int >= 1, \"unlockId\": string (required unless flags include UnlockedByDefault) }");
            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("EFFECTS — each entry is one of:");
            sb.AppendLine("- { \"type\": \"ApplyStatusEffect\", \"statusKey\": <see STATUS KEYS>, \"targetType\": <" + Join(v.ActionTargetTypes) + ">, \"stacksDelta\": int, \"delay\": float >= 0 }");
            sb.AppendLine("- { \"type\": \"DrawCards\", \"count\": int >= 0 }");
            sb.AppendLine("- { \"type\": \"ModifyVibe\", \"amount\": int, \"targetType\": <as above> }");
            sb.AppendLine("- { \"type\": \"ModifyStress\", \"amount\": int, \"targetType\": <as above> }");
            sb.AppendLine();
            sb.AppendLine("STATUS KEYS: " + Join(v.StatusKeys));
            sb.AppendLine();

            sb.AppendLine("COMPOSITION");
            sb.AppendLine("\"composition\": {");
            sb.AppendLine("  \"primaryKind\": <" + Join(v.PrimaryKinds) + ">");
            sb.AppendLine("  \"trackAction\": { \"role\": <" + Join(v.TrackRoles) + "> }");
            sb.AppendLine("  \"partAction\": { \"action\": <" + Join(v.PartActionKinds) + ">, \"customLabel\": string, \"musicianId\": string }");
            sb.AppendLine("  \"modifierEffectNames\": [ exact names from MODIFIER EFFECTS ]");
            sb.AppendLine("  \"palette\": { \"requested\": true, \"timeSignature\": <" + Join(v.TimeSignatures) + "> (optional), \"keywords\": [strings] (optional) }");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("MODIFIER EFFECTS (exact names): " + JoinOrNone(v.ModifierEffectNames));
            sb.AppendLine();

            sb.AppendLine("PALETTE INTENT");
            sb.AppendLine("To give a Composition card a musical identity, set composition.palette with");
            sb.AppendLine("\"requested\": true. The editor resolves the intent to a real palette");
            sb.AppendLine("deterministically; you never name a palette asset. timeSignature and keywords");
            sb.AppendLine("are optional filters; keywords are matched (substring, case-insensitive)");
            sb.AppendLine("against the palette names/notes listed below. Palette intent is only valid");
            sb.AppendLine("for the Rhythm and Backing roles.");
            sb.AppendLine("Available drum palettes (Rhythm role): " + DescribePalettes(v.RhythmPalettes));
            sb.AppendLine("Available chord palettes (Backing role): " + DescribePalettes(v.BackingPalettes));
            sb.AppendLine();

            sb.AppendLine("FORBIDDEN FIELDS — never emit these; asset references are resolved by the");
            sb.AppendLine("editor, never authored by you. Output containing any of them is rejected:");
            sb.AppendLine("cardSpritePath, composition.trackAction.styleBundle, composition.modifierEffects,");
            sb.AppendLine("statusActions, action.actions, action.conditions.");

            return sb.ToString();
        }

        // -------------------------------------------------------------------
        // User prompt
        // -------------------------------------------------------------------

        private static string BuildUserPrompt(Input input)
        {
            var sb = new StringBuilder(512);

            sb.AppendLine("Brief: " + input.Brief.Trim());

            if (!string.IsNullOrWhiteSpace(input.KindHint))
                sb.AppendLine("Card kind: " + input.KindHint.Trim());
            if (!string.IsNullOrWhiteSpace(input.RoleHint))
                sb.AppendLine("Track role: " + input.RoleHint.Trim());
            if (!string.IsNullOrWhiteSpace(input.DefaultMusician))
                sb.AppendLine("Default musician (use for fixedMusician unless the brief says otherwise): "
                    + input.DefaultMusician.Trim());

            sb.AppendLine();
            sb.AppendLine("Respond with exactly one fenced ```json block containing one card object.");

            return sb.ToString();
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        private static string Join(IReadOnlyList<string> list)
        {
            if (list == null || list.Count == 0) return "(none)";
            return string.Join(" | ", list);
        }

        private static string JoinOrNone(IReadOnlyList<string> list)
        {
            if (list == null || list.Count == 0) return "(none exist — do not emit modifierEffectNames)";
            return string.Join(", ", list);
        }

        private static string DescribePalettes(IReadOnlyList<PaletteDescriptor> palettes)
        {
            if (palettes == null || palettes.Count == 0)
                return "(none exist — do not request a palette for this role)";

            var parts = new List<string>(palettes.Count);
            foreach (var p in palettes)
            {
                if (p == null) continue;
                var meters = new List<string>();
                if (p.Entries != null)
                {
                    foreach (var e in p.Entries)
                    {
                        string ts = e.TimeSignature.ToString();
                        if (!meters.Contains(ts)) meters.Add(ts);
                    }
                }
                string name = string.IsNullOrWhiteSpace(p.DisplayName) ? p.Id : p.DisplayName;
                parts.Add($"'{name}' [{string.Join(", ", meters)}]");
            }
            return parts.Count > 0 ? string.Join("; ", parts) : "(none)";
        }
    }
}