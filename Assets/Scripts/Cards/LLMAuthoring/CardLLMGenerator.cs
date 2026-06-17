using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BCS.LLM.Core.Clients;
using BCS.LLM.Core.Execution;

namespace ALWTTT.Cards.LLMAuthoring
{
    /// <summary>
    /// Wraps LLM Core's <see cref="PromptExecutionHelper"/> for card generation
    /// (CE-L1 B2, stage 3). Card sibling of <c>ChordProgressionLLMGenerator</c>:
    /// build → execute (single-shot, D-L10=α) → extract the fenced JSON block.
    ///
    /// Division of labor vs. the chord twin: the chord generator also parses its
    /// domain content (the Roman string) and the handler re-parses the raw
    /// response for Generate/Import unification. For cards the "domain parse"
    /// IS the DTO parse, so to avoid parsing twice the generator stops at
    /// extraction; <see cref="CardLLMResponseHandler"/> owns the single DTO
    /// parse, shared verbatim with the Import path.
    ///
    /// Never throws for an LLM failure; failures come back as a typed
    /// <see cref="Result"/> with the raw response preserved when one exists.
    /// </summary>
    public static class CardLLMGenerator
    {
        /// <summary>Outcome of a generation pass.</summary>
        public readonly struct Result
        {
            public readonly bool Success;

            /// <summary>Raw LLM response before any extraction.</summary>
            public readonly string RawResponse;

            /// <summary>The JSON block extracted from the response (valid when Success).</summary>
            public readonly string ExtractedJson;

            /// <summary>Populated when Success is false.</summary>
            public readonly string FailureReason;

            public readonly int InputTokens;
            public readonly int OutputTokens;

            private Result(bool success, string raw, string json, string failure, int inTok, int outTok)
            {
                Success = success;
                RawResponse = raw;
                ExtractedJson = json;
                FailureReason = failure;
                InputTokens = inTok;
                OutputTokens = outTok;
            }

            public static Result Ok(string raw, string json, int inTok, int outTok) =>
                new Result(true, raw, json, null, inTok, outTok);

            public static Result Fail(string reason, string raw = null, int inTok = 0, int outTok = 0) =>
                new Result(false, raw, null, reason, inTok, outTok);
        }

        /// <summary>
        /// Run one generation. Awaitable; call from an <c>async void</c> handler
        /// and await — never block (SSoT §3.1).
        /// </summary>
        public static async Task<Result> GenerateAsync(
            ILLMClient client,
            CardLLMVocabulary vocabulary,
            CardLLMPromptBuilder.Input input)
        {
            if (client == null)
                return Result.Fail("ILLMClient is null.");

            // ---- 1. Build prompts (pre-network cost cap lives here, SSoT §3.6) ----
            var build = CardLLMPromptBuilder.Build(vocabulary, input);
            if (!build.Success)
                return Result.Fail($"Prompt build failed: {build.FailureReason}");

            // ---- 2. Execute via LLM Core (single-shot) ----
            LLMCompletionResult completion;
            try
            {
                completion = await PromptExecutionHelper.ExecuteAsync(
                    client: client,
                    prompt: build.UserPrompt,
                    instructions: build.SystemPrompt);
            }
            catch (Exception ex)
            {
                return Result.Fail($"LLM call failed: {ex.GetType().Name}: {ex.Message}");
            }

            if (completion == null)
                return Result.Fail("LLM Core returned a null completion.");

            int inTok = completion.InputTokens;
            int outTok = completion.OutputTokens;

            // ---- 3. Extract response text ----
            string rawResponse = completion.OutputText;
            if (string.IsNullOrWhiteSpace(rawResponse))
                return Result.Fail("LLM returned empty OutputText.", rawResponse, inTok, outTok);

            // ---- 4. Locate the JSON block ----
            string json = ExtractJsonBlock(rawResponse);
            if (string.IsNullOrWhiteSpace(json))
                return Result.Fail(
                    "Could not locate a JSON block in the response (no ```json fence, " +
                    "no bare object).", rawResponse, inTok, outTok);

            return Result.Ok(rawResponse, json, inTok, outTok);
        }

        // -------------------------------------------------------------------
        // JSON block extraction (CRLF-safe by construction: regex spans lines
        // with Singleline and never splits on raw '\r'/'\n' chars, SSoT §3.4)
        // -------------------------------------------------------------------

        private static readonly Regex FencedJsonRegex = new Regex(
            "```(?:json)?\\s*(?<body>\\{.*?\\})\\s*```",
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Find the JSON object in a model response. Preference order:
        /// (1) the first fenced block whose body is a {...} object;
        /// (2) the whole trimmed response if it is itself a {...} object;
        /// (3) the outermost first-'{' .. last-'}' slice as a last resort —
        /// the DTO parser fails loudly if the slice is not valid JSON, so this
        /// never silently fabricates content.
        /// </summary>
        internal static string ExtractJsonBlock(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) return null;

            var m = FencedJsonRegex.Match(response);
            if (m.Success)
                return m.Groups["body"].Value.Trim();

            string trimmed = response.Trim();
            if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                return trimmed;

            int first = response.IndexOf('{');
            int last = response.LastIndexOf('}');
            if (first >= 0 && last > first)
                return response.Substring(first, last - first + 1).Trim();

            return null;
        }
    }
}
