#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

using ALWTTT.Cards.LLMAuthoring;
using BCS.LLM.Core.Clients;

using MidiGenPlay;
using MidiGenPlay.Composition;

namespace ALWTTT.Cards.Editor
{
    /// <summary>
    /// LLM-assisted card authoring panel for <see cref="CardEditorWindow"/>
    /// (CE-L1 B3). Partial-class extension mirroring
    /// <c>ChordProgressionEditorWindow_LLM</c>: client override + auto-resolved
    /// default, brief + hints, pre-network cost cap, Generate / Import buttons,
    /// async and non-blocking. The outcome stages through the window's EXISTING
    /// JSON staging path (<c>TryStageCardFromDto</c>) — the same path a pasted
    /// JSON takes — and nothing touches disk until the user presses the existing
    /// "Save (Create Assets)" button (D-CE-L1.6). The window stays a thin
    /// applier: every decision was made by the unit-tested handler/field-plan.
    /// </summary>
    /// <remarks>
    /// <para><b>Async discipline (load-bearing).</b> Button handlers are
    /// <c>async void</c> and <c>await</c> the response handler; never
    /// <c>.Result</c> / <c>.Wait()</c> / <c>.GetAwaiter().GetResult()</c>, which
    /// would deadlock the editor main thread. The continuation resumes on the
    /// main thread, where staging + Repaint are valid.</para>
    ///
    /// <para><b>Save-step hook (D-CE-L1.6).</b> When a generated/imported card is
    /// staged, the field plan is held in <see cref="_llmPlan"/> until Save.
    /// <see cref="ApplyLlmPlanOnSave"/> (called by
    /// <c>SaveStagedJsonToAssetsAndAddToCatalog</c> after the card/payload assets
    /// exist) creates the role bundle via the existing
    /// <c>CreateAndAssignStyleBundle</c> and assigns the resolved palette to the
    /// bundle's palette field. Discarding the staged card clears the plan.</para>
    ///
    /// <para><b>Determinism.</b> The intent seed shown in the panel fully
    /// determines the palette pick for a given payload + project state; it is
    /// echoed in the status line so a pick can be reproduced.</para>
    /// </remarks>
    public sealed partial class CardEditorWindow
    {
        // -- LLM panel state (serialized = survives domain reload) --
        [SerializeField] private bool _llmFoldout = true;
        [SerializeField] private LLMClientData _llmClientOverride; // null → first LLMClientData in project
        [SerializeField][TextArea(3, 6)] private string _llmBrief = "";
        [SerializeField] private int _llmKindHintIndex; // 0 = auto
        [SerializeField] private int _llmRoleHintIndex; // 0 = auto
        [SerializeField] private int _llmIntentSeed = 12345;
        [SerializeField] private int _llmMaxCharBudget = 8000; // 0 = no cap

        // -- Transient (not serialized): in-flight + last-run reporting --
        [NonSerialized] private bool _llmGenerating;
        [NonSerialized] private string _llmStatus = "";
        [NonSerialized] private bool _llmStatusIsError;
        [NonSerialized] private string _llmWarnings = "";
        [NonSerialized] private int _llmLastInputTokens;
        [NonSerialized] private int _llmLastOutputTokens;

        // -- Pending field plan, consumed by the Save hook --
        [NonSerialized] private CardLLMFieldPlan _llmPlan;
        [NonSerialized] private bool _llmPlanPending;

        private static readonly string[] KindHintOptions = { "(let the model decide)", "Action", "Composition" };
        private static readonly string[] RoleHintOptions = { "(let the model decide)", "Backing", "Melody", "Harmony", "Rhythm" };

        /// <summary>
        /// CE-F1 Tier-B knob for the intent resolver. The editor panel uses the
        /// selector's own default (4); reading it from MidiGenPlayConfig is
        /// deliberately deferred until a card actually needs a non-default value.
        /// </summary>
        private const int LlmMinHarmonicSubdivisions = 4;

        // -------------------------------------------------------------------
        // Panel
        // -------------------------------------------------------------------

        private void DrawCardLLMPanel()
        {
            EditorGUILayout.Space(6);
            _llmFoldout = EditorGUILayout.Foldout(_llmFoldout, "Generate with LLM", true);
            if (!_llmFoldout) return;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (_registries == null)
                {
                    EditorGUILayout.HelpBox(
                        "Project Registries not resolved — status keys will come from an " +
                        "asset scan instead of the catalogues. Resolve the registries above " +
                        "for an exact status-key alphabet.", MessageType.Warning);
                }

                _llmClientOverride = (LLMClientData)EditorGUILayout.ObjectField(
                    new GUIContent("Client (override)",
                        "Optional. When empty, the first LLMClientData asset in the project is used."),
                    _llmClientOverride, typeof(LLMClientData), false);

                EditorGUILayout.LabelField("Brief", EditorStyles.miniBoldLabel);
                _llmBrief = EditorGUILayout.TextArea(_llmBrief, GUILayout.MinHeight(48));

                _llmKindHintIndex = EditorGUILayout.Popup("Card kind hint", _llmKindHintIndex, KindHintOptions);
                _llmRoleHintIndex = EditorGUILayout.Popup("Track role hint", _llmRoleHintIndex, RoleHintOptions);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _llmIntentSeed = EditorGUILayout.IntField(
                        new GUIContent("Intent seed",
                            "Seeds the deterministic palette pick. Same brief/payload + same seed ⇒ same palette."),
                        _llmIntentSeed);
                    if (GUILayout.Button("Randomize", GUILayout.Width(80)))
                        _llmIntentSeed = UnityEngine.Random.Range(1, int.MaxValue);
                }

                _llmMaxCharBudget = EditorGUILayout.IntField(
                    new GUIContent("Max prompt chars",
                        "Pre-network cost cap over system+user prompts. 0 = no cap. " +
                        "An over-budget prompt fails before anything is sent."),
                    _llmMaxCharBudget);

                EditorGUILayout.Space(2);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(_llmGenerating || string.IsNullOrWhiteSpace(_llmBrief)))
                    {
                        if (GUILayout.Button(_llmGenerating ? "Generating…" : "Generate Card"))
                            GenerateCardAsync();
                    }
                    using (new EditorGUI.DisabledScope(_llmGenerating))
                    {
                        if (GUILayout.Button(new GUIContent("Import From Clipboard",
                                "Translate a copied model response (or bare card JSON) through the same " +
                                "guard + validation + palette resolution as Generate.")))
                            ImportCardFromClipboard();
                    }
                }

                if (!string.IsNullOrEmpty(_llmStatus))
                    EditorGUILayout.HelpBox(_llmStatus, _llmStatusIsError ? MessageType.Error : MessageType.Info);

                if (!string.IsNullOrEmpty(_llmWarnings))
                    EditorGUILayout.HelpBox(_llmWarnings, MessageType.Warning);

                if (_llmLastInputTokens > 0 || _llmLastOutputTokens > 0)
                    EditorGUILayout.LabelField(
                        $"Last call: {_llmLastInputTokens} input / {_llmLastOutputTokens} output tokens",
                        EditorStyles.miniLabel);
            }
        }

        // -------------------------------------------------------------------
        // Generate / Import
        // -------------------------------------------------------------------

        private async void GenerateCardAsync()
        {
            var client = ResolveLlmClient(out string clientError);
            if (client == null)
            {
                SetLlmStatus(clientError, isError: true);
                return;
            }

            ResolveRegistries();
            var vocab = CardLLMVocabularyBuilder.Build(_registries);
            var input = BuildPromptInput();

            _llmGenerating = true;
            _llmWarnings = "";
            SetLlmStatus("Generating…");

            try
            {
                var outcome = await CardLLMResponseHandler.GenerateAsync(
                    client, vocab, input, _llmIntentSeed, LlmMinHarmonicSubdivisions);
                ApplyLlmOutcome(outcome);
            }
            catch (Exception ex)
            {
                // Handler is designed not to throw; this is a last-ditch guard so an
                // unexpected exception never strands the panel in "Generating…".
                SetLlmStatus($"Unexpected exception: {ex.GetType().Name}: {ex.Message}", isError: true);
                Debug.LogException(ex);
            }
            finally
            {
                _llmGenerating = false;
                Repaint();
            }
        }

        private void ImportCardFromClipboard()
        {
            string payload = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrWhiteSpace(payload))
            {
                SetLlmStatus("Clipboard is empty.", isError: true);
                return;
            }

            ResolveRegistries();
            var vocab = CardLLMVocabularyBuilder.Build(_registries);
            _llmWarnings = "";

            var outcome = CardLLMResponseHandler.FromPayload(
                payload, vocab, _llmIntentSeed, LlmMinHarmonicSubdivisions);
            ApplyLlmOutcome(outcome);
        }

        private CardLLMPromptBuilder.Input BuildPromptInput()
        {
            string kindHint = _llmKindHintIndex > 0 ? KindHintOptions[_llmKindHintIndex] : null;
            string roleHint = _llmRoleHintIndex > 0 ? RoleHintOptions[_llmRoleHintIndex] : null;

            return new CardLLMPromptBuilder.Input(
                brief: _llmBrief,
                kindHint: kindHint,
                roleHint: roleHint,
                defaultMusician: _selectedMusician.ToString(),
                maxCharBudget: _llmMaxCharBudget);
        }

        // -------------------------------------------------------------------
        // Outcome → stage (the window applies; it does not decide)
        // -------------------------------------------------------------------

        private void ApplyLlmOutcome(CardLLMResponseHandler.Outcome outcome)
        {
            _llmLastInputTokens = outcome.InputTokens;
            _llmLastOutputTokens = outcome.OutputTokens;

            if (outcome.DisplayWarnings != null && outcome.DisplayWarnings.Count > 0)
                _llmWarnings = string.Join("\n", outcome.DisplayWarnings);

            var plan = CardLLMFieldPlan.From(outcome);
            if (!plan.StageCard)
            {
                ClearLlmPendingPlan();
                SetLlmStatus(plan.StatusMessage, isError: plan.StatusIsError);
                return;
            }

            if (!TryStageCardFromDto(plan.Dto, out string stageError))
            {
                ClearLlmPendingPlan();
                SetLlmStatus($"Staging failed: {stageError}", isError: true);
                return;
            }

            // Hold the plan for the Save hook (bundle creation + palette assignment).
            _llmPlan = plan;
            _llmPlanPending = true;

            string seedNote = plan.AssignPalette ? $" (intent seed {_llmIntentSeed})" : "";
            SetLlmStatus(plan.StatusMessage + seedNote, isError: plan.StatusIsError);
            Repaint();
        }

        // -------------------------------------------------------------------
        // Save hook (called by SaveStagedJsonToAssetsAndAddToCatalog) — D-CE-L1.6
        // -------------------------------------------------------------------

        /// <summary>
        /// Consume the pending LLM plan once the staged card has been written to
        /// disk: create the role bundle through the EXISTING
        /// <c>CreateAndAssignStyleBundle</c> and assign the resolved palette to
        /// its palette field. No-op for non-LLM saves. Loud (never silent) when
        /// something cannot be applied.
        /// </summary>
        private void ApplyLlmPlanOnSave(CardPayload payloadAsset)
        {
            if (!_llmPlanPending) return;

            var plan = _llmPlan;
            ClearLlmPendingPlan(); // consume exactly once

            var comp = payloadAsset as CompositionCardPayload;
            if (comp == null)
            {
                if (plan.AssignPalette)
                    Debug.LogWarning(
                        "[CardEditorWindow] LLM plan had a resolved palette but the saved payload " +
                        "is not a Composition payload; palette not assigned.");
                return; // Action cards have no bundle to create.
            }

            var pso = new SerializedObject(comp);
            var trackProp = pso.FindProperty("trackAction");
            var roleProp = trackProp?.FindPropertyRelative("role");
            var styleProp = trackProp?.FindPropertyRelative("styleBundle");

            if (roleProp == null || styleProp == null)
            {
                Debug.LogWarning("[CardEditorWindow] LLM save hook: trackAction properties not found; " +
                                 "no bundle created.");
                return;
            }

            // Create the role bundle (reuses the wizard's path) unless one is
            // already assigned (cannot happen via the LLM route — the field is
            // banned — but a user may have assigned one during review).
            if (styleProp.objectReferenceValue == null)
                CreateAndAssignStyleBundle(roleProp, styleProp, comp);

            pso.Update();
            var bundle = styleProp.objectReferenceValue as TrackStyleBundleSO;
            if (bundle == null)
            {
                Debug.LogError("[CardEditorWindow] LLM save hook: style bundle creation failed; " +
                               (plan.AssignPalette ? "resolved palette NOT assigned." : "no bundle on payload."));
                return;
            }

            if (!plan.AssignPalette) return;

            // Assign the resolved palette to the matching typed field.
            if (bundle is RhythmCardConfigSO rhythm)
            {
                var palette = CardPaletteDescriptorScanner.LoadPalette<DrumPatternPaletteSO>(plan.PaletteId);
                if (palette == null)
                {
                    Debug.LogError($"[CardEditorWindow] LLM save hook: drum palette not found at " +
                                   $"'{plan.PaletteId}'; palette NOT assigned.");
                    return;
                }
                Undo.RecordObject(rhythm, "Assign Drum Palette (LLM)");
                rhythm.patternPalette = palette;
                EditorUtility.SetDirty(rhythm);
            }
            else if (bundle is BackingCardConfigSO backing)
            {
                var palette = CardPaletteDescriptorScanner.LoadPalette<ChordProgressionPaletteSO>(plan.PaletteId);
                if (palette == null)
                {
                    Debug.LogError($"[CardEditorWindow] LLM save hook: chord palette not found at " +
                                   $"'{plan.PaletteId}'; palette NOT assigned.");
                    return;
                }
                Undo.RecordObject(backing, "Assign Chord Palette (LLM)");
                backing.progressionPalette = palette;
                EditorUtility.SetDirty(backing);
            }
            else
            {
                Debug.LogWarning($"[CardEditorWindow] LLM save hook: bundle type '{bundle.GetType().Name}' " +
                                 "has no palette field; resolved palette NOT assigned.");
                return;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[CardEditorWindow] LLM save hook: assigned palette '{plan.PaletteDisplayName}' " +
                      $"to '{bundle.name}'.");
        }

        /// <summary>Drop any pending plan (called on discard and after consumption).</summary>
        private void ClearLlmPendingPlan()
        {
            _llmPlanPending = false;
            _llmPlan = default;
        }

        // -------------------------------------------------------------------
        // Client resolution (mirrors ChordProgressionEditorWindow_LLM)
        // -------------------------------------------------------------------

        private ILLMClient ResolveLlmClient(out string error)
        {
            error = null;
            LLMClientData data = _llmClientOverride;

            if (data == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:LLMClientData");
                if (guids != null && guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    data = AssetDatabase.LoadAssetAtPath<LLMClientData>(path);
                }
            }

            if (data == null)
            {
                error = "No LLMClientData asset assigned or found in the project. " +
                        "Assign one in the Client (override) field.";
                return null;
            }

            var client = LLMClientFactory.CreateClient(data);
            if (client == null)
            {
                error = $"LLMClientFactory returned null for '{data.name}'. " +
                        "Check the provider is supported and the API key is configured.";
                return null;
            }
            return client;
        }

        private void SetLlmStatus(string message, bool isError = false)
        {
            _llmStatus = message ?? "";
            _llmStatusIsError = isError;
        }
    }
}
#endif