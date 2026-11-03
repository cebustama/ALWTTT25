using ALWTTT.Data;
using ALWTTT.Tooltips;
using static ALWTTT.Tooltips.GameTextTemplateIds;   // [TXT-4]
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Characters
{
    public class AudienceCharacterCanvas : CharacterCanvas
    {
        [Header("Audience Character Canvas Settings")]
        [SerializeField] private Image intentImage;
        [SerializeField] private TextMeshProUGUI nextActionValueText;
        public Image IntentImage => intentImage;
        public TextMeshProUGUI NextActionValueText => nextActionValueText;


        // [CONV-TINT-1b / D-H2=A] State-driven concealment of the whole canvas. A
        // CanvasGroup on the canvas root multiplies with every child group (the bar's
        // own S5e-ext group included), so child visibility rules keep running and are
        // simply not seen; lifting the concealment shows whatever they decided meanwhile.
        // Surfaces routed to the GLOBAL TooltipManager live outside this group, so those
        // are gated explicitly on _convincedHidden.
        private CanvasGroup _stateGroup;
        private bool _convincedHidden;

        public bool IsConvincedHidden => _convincedHidden;

        public void SetConvincedHidden(bool hidden)
        {
            _convincedHidden = hidden;

            // TryGetComponent, not "GetComponent() ?? AddComponent()": in the Editor a
            // missing component can come back as a fake-null object that ?? treats as real.
            if (_stateGroup == null && !TryGetComponent(out _stateGroup))
                _stateGroup = gameObject.AddComponent<CanvasGroup>();

            _stateGroup.alpha = hidden ? 0f : 1f;
            _stateGroup.blocksRaycasts = !hidden;
            _stateGroup.interactable = !hidden;
        }

        public AudienceAbilityData NextAbility;
        private AudienceIntentionData CurrentIntention => NextAbility.Intention;

        // [S5a] Vibe Telegraph (C2 effectiveness + C3 projected number). All fields
        // optional/null-guarded so an unwired prefab degrades to "no telegraph"
        // rather than NRE (UI-fix-A recurrence vector). vibeTelegraphRoot is the
        // parent toggled for show/hide; the two texts live under it.
        [Header("Vibe Telegraph (S5a)")]
        [SerializeField] private GameObject vibeTelegraphRoot;
        [SerializeField] private TextMeshProUGUI effectivenessLabel; // C2
        [SerializeField] private TextMeshProUGUI projectedVibeText;  // C3

        // [S5a-SMOKE] True when all three telegraph slots are wired on the prefab.
        // GigManager logs this once per audience at song start (ST-S5a-9 wiring check).
        public bool IsVibeTelegraphWired =>
            vibeTelegraphRoot != null && effectivenessLabel != null && projectedVibeText != null;

        // [R4 / D-R4-4=A -> PRES-1 / D-PRES1-3=A] Taste reveal.
        //
        // R4 shipped this as a persistent panel on the audience canvas. PRES-1
        // keeps WHERE it lives (D-R4-4=A is not reopened) and changes only HOW it
        // presents: the taste TEXT now composes into the single hover tooltip, and
        // an icon is the discreet persistent "revealed" marker. Rationale: one
        // hover surface per character is a canvas invariant, and the persistent
        // panel also overlapped the "Songs left" readout.
        [Header("Taste Reveal (R4 -> PRES-1)")]
        [Tooltip("[PRES-1] Persistent 'tastes revealed' marker. Null-guarded: an " +
                 "unwired icon degrades to no marker; the tooltip still works.")]
        [SerializeField] private GameObject revealedTasteIcon;

        // [R5-pre, 2026-08-11] The retired persistent-panel slots (tastePanelRoot /
        // tasteText) were REMOVED here, together with the TastePanel object in the
        // audience prefab. They were kept inert at PRES-1 only so live wiring would
        // not dangle before that prefab pass. Reveal surface: the hover tooltip
        // (SSoT_Audience_and_Reactions §6.4).

        // [PRES-1] Cached reveal state for tooltip composition. The AUTHORITATIVE
        // "has been revealed" flag still lives on AudienceCharacterBase and the
        // data still lives on AudienceCharacterData (SSoT_Audience_and_Reactions
        // §6.4); this is a presentation cache, not a second source of truth.
        private TastePreferences _revealedTaste;
        private bool _tasteRevealed;

        /// <summary>
        /// [R4-SMOKE -> PRES-1] SEMANTIC SHIFT with D-PRES1-3=A: this used to mean
        /// "both panel slots wired". It now means "the persistent icon is wired".
        /// The tooltip composition needs no prefab wiring at all and works
        /// regardless of this flag.
        /// </summary>
        public bool IsTastePanelWired => revealedTasteIcon != null;

        // [PRES-1] ESP copy, hardcoded per D-S5f-7=A / D-S5f-8=A like the Blocked
        // tooltip and telegraph labels; all migrate together in the S5f-ext pass.
        //
        // [PRES-1c / D-PRES1c-1=A] TMP rich text: the block header is bolded so it
        // reads as a SECTION heading rather than as one more taste line. Without it
        // the taste block ran on visually from the intention text, which is what
        // ST-PRES1-7 surfaced. If the tooltip body's TMP has richText disabled the
        // tags render literally — that is a prefab fix, not a code one (see
        // ST-PRES1-7b).
        // [TXT-4 / F-TXT-2-1] Words from the glossary (ui_aud_*); the bold wrapper is formatting and stays here.
        private static string TasteBlockHeader => "<b>" + GameText.Format(UiAudTasteBlockHeader) + "</b>";
        private static string TasteOnlyHeader => GameText.Format(UiAudTasteHeader);


        // [BIGNUM-1 / D-BN-9=A] The step-by-step Vibe breakdown composes into the SAME
        // hover tooltip as intention + tastes (PRES-1 invariant: one hover surface per
        // character). This is a presentation cache of what GigManager projected — not
        // a second source of truth; GigManager.BuildVibeProjection owns the numbers.
        // ESP copy hardcoded like the rest of this canvas (D-S5f-7=A).
        // [BIGNUM-1r] VibeBlockHeader retired with the D-BN-9 revert; the bar tooltip
        // uses VibeOnlyHeader as its plain header.
        // [TEXT-VIBE-1 / F-TV-3] Was a literal left behind in population 5. Same word in EN/ES today,
        // still a player surface: it reads the `vibe` keyword concept, like the Flow/Captivated lines.
        private static string VibeOnlyHeader => GameText.ConceptName("vibe");
        private VibeProjection _projection;
        private bool _hasProjection;

        private bool _barHovered;

        /// <summary>[BIGNUM-1r / D-BN-15] Set by VibeBarTooltipTarget (child, runs first).</summary>
        public void NotifyBarHover(bool hovered) { _barHovered = hovered; }

        /// <summary>[BIGNUM-1r / D-BN-14=A] The step-by-step breakdown, on the bar's hover only.</summary>
        public void ShowVibeTooltip()
        {
            if (!_hasProjection) return;
            if (_convincedHidden) return; // [CONV-TINT-1b] TooltipManager is outside the group
            HideTooltipInfo(TooltipManager.Instance); // [GIG-UI-1] replace a body-hover column, don't stack on it
            ShowPanel(BuildVibeText(in _projection), VibeOnlyHeader, TooltipKind.Default);
        }

        public void HideVibeTooltip()
        {
            HideTooltipInfo(TooltipManager.Instance);
            RefreshHoverTooltip(); // [GIG-UI-1] back to the body/icon column if still hovered
        }

        /// <summary>
        /// [BIGNUM-1r / D-BN-14=A] Drop the predicted segment. GigManager calls this
        /// right before the song-end ApplyIncomingVibe, so the real bar lerps down
        /// through where the ghost was — prediction becomes payment on screen.
        /// </summary>
        public void ClearPredictedVibe()
        {
            healthBar?.SetPredictedLoss(0);
        }

        // [BIGNUM-1 / D-BN-4] "KO" tint for the C3 number when this song would convince
        // the member (projected >= remaining resistance). The default tint is captured
        // from the prefab the first time the number shows, so an unstyled prefab keeps
        // its own colour and nothing else changes.
        [Header("Vibe Telegraph — KO (BIGNUM-1)")]
        [SerializeField] private Color projectedKoColor = new Color(1f, 0.85f, 0.25f);
        private Color _projectedDefaultColor;
        private bool _projectedDefaultCached;

        // Lines follow the PAYOUT order (SSoT_Scoring 6.1 -> 7.1 -> 6.2 -> gate).
        // [TEXT-VIBE-1 / D1, F-TIP-6] A step line prints only when its OUTPUT differs from its INPUT,
        // compared on RAW values in the int domain each step emits; the tastes step rounds its float
        // input with the same Mathf.RoundToInt that GigManager.ComputeLPart uses. "x 1,00" is NOT the
        // test: a 1,004 that moves the number would pass it, and "+ 0" would need a second rule.
        // Guard rails: the Hype line and the song-end line ALWAYS print; the Blocked and Indifferent
        // branches are untouched because they explain a zero, which is where the reason matters most.
        // This REVERSES BIGNUM-1's "every line is shown even when neutral", by observation
        // (ST-TIP-10): four inert lines buried the two that moved the number.
        // Authority: planning/Design_Vibe_Telegraph_v0_1.md 10.2.
        private static string BuildVibeText(in VibeProjection p)
        {
            // [TXT-4 / F-TXT-2-1] Every line is a ui_aud_vibe_* template; status names resolve by StatusKey
            // (GameText.ConceptName) so "Captivado" stops being a second home for the captivated status.
            var sb = new System.Text.StringBuilder();
            sb.Append(GameText.Format(UiAudVibeHype, GameText.Number(p.HypeBase, "0.#")));

            if (p.Blocked)
            {
                sb.Append('\n').Append(GameText.Format(UiAudVibeBlocked));
                sb.Append('\n').Append(GameText.Format(UiAudVibeSongEnd, "0"));
                return sb.ToString();
            }

            // 6.1 tastes: input HypeBase (float, printed "0.#"), output LAfterImpression (int).
            if (Mathf.RoundToInt(p.HypeBase) != p.LAfterImpression)
                sb.Append('\n').Append(GameText.Format(UiAudVibeTastes, GameText.Number(p.ImpressionFactor, "0.00"), p.LAfterImpression));
            // 7.1 Flow: input LAfterImpression, output LAfterFlow.
            if (p.LAfterFlow != p.LAfterImpression)
                sb.Append('\n').Append(GameText.Format(UiAudVibeStatusMult, GameText.Number(p.FlowMult, "0.00"), GameText.ConceptName("flow"), p.FlowStacks, p.LAfterFlow));
            // 6.2 SFX: input LAfterFlow, output Subtotal (equivalent to Sfx != 0).
            if (p.Subtotal != p.LAfterFlow)
                sb.Append('\n').Append(GameText.Format(UiAudVibeSfx, p.Sfx, p.Subtotal));

            // Gate. Indifferent prints ALWAYS (it explains the zero). Captivated prints when it moved Final.
            if (p.Indifferent)
                sb.Append('\n').Append(GameText.Format(UiAudVibeIndifferent, GameText.ConceptName("indifference")));
            else if (p.Final != p.Subtotal)
                sb.Append('\n').Append(GameText.Format(UiAudVibeStatusMult, GameText.Number(p.CaptivatedMult, "0.00"), GameText.ConceptName("captivated"), p.CaptivatedStacks, p.Final));

            sb.Append('\n').Append(GameText.Format(UiAudVibeSongEnd, p.Final > 0 ? "-" + p.Final : "0"));
            if (p.Ko) sb.Append('\n').Append(GameText.Format(UiAudVibeKo));
            return sb.ToString();
        }

        // [GIG-UI-1] Two hover sources feed ONE rebuild (Claudio, 2026-11-02):
        // the intent icon (UI hover on this canvas) and the body (CharacterBase physics
        // hover on the collider, the same one that lights the outline and that cards
        // target). BOTH show the same column -- Blocked (when tinted), ability, revealed
        // tastes -- because a column that changed with where you pointed read as two
        // different tooltips. Every change hides the whole column and rebuilds it, so the
        // two sources can overlap (icon inside the collider) without duplicating panels,
        // and leaving one does not wipe what the other still owns.
        private bool _iconHovered;
        private bool _bodyHovered;

        /// <summary>[GIG-UI-1] Set by AudienceCharacterBase: is the Blocked tint showing now?</summary>
        public System.Func<bool> BlockedQuery { get; set; }

        // [GIG-UI-1] Column placement: a point at the top-right of the body collider
        // (AudienceCharacterBase creates it). The column grows DOWN from it, a few pixels
        // to the right, so every member gets the same placement whatever its canvas
        // offsets are (the Kid canvas sat lower, so its descriptionRoot did too).
        private Transform _tooltipAnchor;
        // Colours and the column offset live in TooltipStyleSO (on the TooltipManager
        // prefab), so every tooltip kind is tuned in one place.
        private static readonly Vector2 FallbackColumnOffset = new Vector2(8f, 40f);

        public void SetTooltipAnchor(Transform anchor) { _tooltipAnchor = anchor; }

        private void ShowPanel(string content, string header, TooltipKind kind)
        {
            var tm = TooltipManager.Instance;
            if (tm == null) return;
            if (_tooltipAnchor != null)
            {
                var offset = tm.Style != null ? tm.Style.AudienceColumnOffset : FallbackColumnOffset;
                tm.ShowTooltip(content, header, _tooltipAnchor, null, 0f,
                    offset, growDown: true, kind: kind);
            }
            else
                tm.ShowTooltip(content, header, descriptionRoot, null, 0f,
                    default(Vector2), growDown: false, kind: kind);
        }

        /// <summary>[GIG-UI-1] Called by AudienceCharacterBase on sprite hover enter/exit.</summary>
        public void SetBodyHover(bool hovered)
        {
            _bodyHovered = hovered;
            RefreshHoverTooltip();
        }

        protected override void ShowTooltipInfo()
        {
            base.ShowTooltipInfo();
            _iconHovered = true;
            RefreshHoverTooltip();
        }

        protected override void OnCanvasPointerExit()
        {
            _iconHovered = false;
            RefreshHoverTooltip();
        }

        private void RefreshHoverTooltip()
        {
            // [BIGNUM-1r / D-BN-15] The bar's own tooltip is showing; PointerEnter reaches
            // the child (VibeBarTooltipTarget) before this canvas, so the flag is already
            // set when we get here. Stand down instead of overwriting its tooltip.
            if (_barHovered) return;

            HideTooltipInfo(TooltipManager.Instance);
            if (_convincedHidden) return; // [CONV-TINT-1b] TooltipManager is outside the group
            if (!_iconHovered && !_bodyHovered) return;

            // [S5f / E-lite] The Blocked legend explains the tint, so it leads the column.
            if (BlockedQuery != null && BlockedQuery())
                ShowPanel(BlockedTooltipBody, BlockedTooltipHeader, TooltipKind.Blocked);

            string header = null;
            string body = null;

            if (NextAbility != null && CurrentIntention != null)
            {
                // [TEXT-5 / D-T5-1=A] Name + body from the glossary by abilityId (one concept per ability).
                // abilityName / intention.contentText are developer labels; a missing entry shows the raw
                // id and warns once (SSoT_Game_Text §3 miss policy). The null guard above is unchanged:
                // the intention SO still gates the hover (it also carries the sprite).
                header = GameText.ConceptName(NextAbility.AbilityId);
                body = GameText.ConceptDescription(NextAbility.AbilityId);
            }

            // [GIG-UI-1] REVERSES the single-surface half of PRES-1 / D-PRES1-3=A, at Claudio's
            // request (2026-11-02, Monster Train reference): the ability and the revealed tastes
            // are now separate stacked panels, each with its own header and colour, instead of
            // one box with an inline "-- Gustos --" separator. What D-PRES1-3 protected still
            // holds: no persistent taste panel, and tastes still show when there is no
            // intention (the taste panel then stands alone).
            if (body != null)
                ShowPanel(body, header, TooltipKind.Ability);
            if (_tasteRevealed)
                ShowPanel(BuildTasteText(_revealedTaste), TasteOnlyHeader, TooltipKind.Tastes);
        }

        /// <summary>
        /// [S5a/T10] Show the per-enemy telegraph. <paramref name="tier"/> drives the
        /// effectiveness label (C2); <paramref name="projectedNumber"/> is the live
        /// "+N Vibe" this member will receive at song end (C3), hidden when Immune or
        /// when the caller opts out. Pure presentation - no state.
        /// </summary>
        public void SetVibeTelegraph(
            VibeEffectiveness tier, int projectedNumber, bool showNumber,
            bool showLabel = true)
        {
            bool anyVisible = showLabel ||
                (showNumber && tier != VibeEffectiveness.Immune);
            if (vibeTelegraphRoot != null)
                vibeTelegraphRoot.SetActive(anyVisible);

            if (effectivenessLabel != null)
            {
                effectivenessLabel.gameObject.SetActive(showLabel);
                if (showLabel)
                {
                    effectivenessLabel.text = LabelFor(tier);
                    effectivenessLabel.color = ColorFor(tier);
                }
            }

            if (projectedVibeText != null)
            {
                bool show = showNumber && tier != VibeEffectiveness.Immune;
                projectedVibeText.gameObject.SetActive(show);
                if (show)
                {
                    // [BIGNUM-1] Capture the prefab tint once; reset to it on every
                    // show so a previous KO tint never leaks into the next refresh.
                    if (!_projectedDefaultCached)
                    {
                        _projectedDefaultColor = projectedVibeText.color;
                        _projectedDefaultCached = true;
                    }
                    projectedVibeText.color = _projectedDefaultColor;
                    projectedVibeText.text = $"-{projectedNumber}";
                }
            }
        }

        /// <summary>
        /// [BIGNUM-1 / D-BN-2=A] Telegraph from a full projection: caches it for the
        /// tooltip breakdown (D-BN-9=A) and tints the C3 number when this song would
        /// convince the member (D-BN-4 KO). The S5a surface is delegated to the S5a
        /// method; this adds presentation on top, no numbers of its own.
        /// </summary>
        public void SetVibeTelegraph(in VibeProjection p, bool showNumber, bool showLabel = true)
        {
            _projection = p;
            _hasProjection = true;

            SetVibeTelegraph(p.Tier, p.Final, showNumber, showLabel);

            // [BIGNUM-1r2 / D-BN-16=A] The ghost is its OWN surface: it survives the
            // numeric-label toggle. showVibeProjectedNumbers governs the "-N" text, not
            // the bar segment — turning the number off to reduce clutter should leave
            // the bar readable, which is the whole point of the segment.
            // Immune/blocked project 0, so the ghost simply does not show for them.
            healthBar?.SetPredictedLoss(p.Final);

            SetPredictionVisible(p.Final > 0); // [BIGNUM-1r3 / D-BN-17=A]

            if (projectedVibeText != null && projectedVibeText.gameObject.activeSelf && p.Ko)
                projectedVibeText.color = projectedKoColor;
        }

        /// <summary>[S5a] Hide the telegraph (between songs / audience turn).</summary>
        public void HideVibeTelegraph()
        {
            _hasProjection = false; // [BIGNUM-1] no breakdown in the tooltip between songs

            healthBar?.SetPredictedLoss(0); // [BIGNUM-1r] no ghost between songs either

            SetPredictionVisible(false);    // [BIGNUM-1r3] full bars go back to hidden

            // Deactivate the labels explicitly too, so hide works whether they are
            // children of vibeTelegraphRoot or siblings of it (prefab-layout robust).
            if (vibeTelegraphRoot != null)
                vibeTelegraphRoot.SetActive(false);
            if (effectivenessLabel != null)
                effectivenessLabel.gameObject.SetActive(false);
            if (projectedVibeText != null)
                projectedVibeText.gameObject.SetActive(false);
        }

        // [S5f / D-S5f-8=A] ESP copy for the tester build (D-REPLAN-1:
        // unassisted Spanish comprehension). ENG originals: "Super!" /
        // "Resists" / "Immune" / "Normal". Hardcoded like the Blocked
        // tooltip (D-S5f-7=A); migrates in the S5f-ext localization pass.
        private static string LabelFor(VibeEffectiveness tier)
        {
            switch (tier)
            {
                case VibeEffectiveness.SuperEffective: return GameText.Format(UiAudEffSuper);       // [TXT-4]
                case VibeEffectiveness.NotVeryEffective: return GameText.Format(UiAudEffResists);
                case VibeEffectiveness.Immune: return GameText.Format(UiAudEffImmune);
                default: return GameText.Format(UiAudEffNormal);
            }
        }

        // Palette aligned with the impression/floater language (green = good,
        // grey = neutral, red = resisting, muted = gated).
        private static Color ColorFor(VibeEffectiveness tier)
        {
            switch (tier)
            {
                case VibeEffectiveness.SuperEffective: return new Color(0.40f, 1.0f, 0.40f);
                case VibeEffectiveness.NotVeryEffective: return new Color(1.0f, 0.45f, 0.40f);
                case VibeEffectiveness.Immune: return new Color(0.50f, 0.50f, 0.50f);
                default: return new Color(0.80f, 0.80f, 0.80f);
            }
        }

        // [S5f / E-lite] Blocked ("oscurito") legend. Blocked is sprite-tint
        // only per M1.2 Decision E3 (no status icon), so this hover tooltip is
        // the only textual surface explaining the tint. ESP copy (tester build
        // default; D-S5f-7=A — hardcoded until the S5f-ext localization pass).
        // ENG: "Blocked — someone tall is in the way. Immune to persuasion
        // from this position."
        // [TXT-4 / F-TXT-2-1] The Blocked legend IS the `blocked` glossary concept (SSoT_Game_Text §5,
        // glossary-native): header + body come from there, so the tint tooltip and the tutorial tag can
        // never disagree. The former ES literal was a second home for the same concept.
        private static string BlockedTooltipHeader => GameText.ConceptName("blocked");
        private static string BlockedTooltipBody => GameText.ConceptDescription("blocked");

        /// <summary>[S5f / E-lite] Show the Blocked-tint explanation tooltip.</summary>
        public void ShowBlockedTooltip()
        {
            ShowTooltipInfo(TooltipManager.Instance,
                BlockedTooltipBody, BlockedTooltipHeader, descriptionRoot);
        }

        /// <summary>[S5f / E-lite] Hide the Blocked tooltip (pointer exit).</summary>
        public void HideBlockedTooltip()
        {
            HideTooltipInfo(TooltipManager.Instance);
        }

        // ---- [R4 / D-R0-1=A + D-R4-4=A -> PRES-1 / D-PRES1-3=A] Taste reveal ----
        //
        // Presentation only. AudienceCharacterBase owns the "has been revealed"
        // flag and its per-gig idempotence; AudienceCharacterData owns the
        // preference data. ESP copy is hardcoded, matching the Blocked tooltip and
        // the telegraph labels (D-S5f-7=A / D-S5f-8=A); all three migrate together
        // in the S5f-ext localization pass.

        private static string TasteNeutralText => GameText.Format(UiAudTasteNeutral);   // [TXT-4]

        /// <summary>
        /// [R4 -> PRES-1 / D-PRES1-3=A] Register this member's tastes as revealed.
        ///
        /// Method name deliberately unchanged: the caller
        /// (AudienceCharacterBase.RevealPreferences) is NOT touched by PRES-1, so
        /// its idempotence and one-call-per-gig guarantee carry over untouched.
        /// What changed is the surface — the persistent panel is retired; this now
        /// caches the taste for tooltip composition and lights the discreet icon.
        /// </summary>
        public void ShowTastePanel(TastePreferences taste)
        {
            _revealedTaste = taste;
            _tasteRevealed = true;

            if (revealedTasteIcon != null)
                revealedTasteIcon.SetActive(true);
        }

        /// <summary>
        /// [R4 -> PRES-1 -> R5-pre] Clear the reveal (gig teardown / dev reset).
        /// The defensive hide of the retired legacy panel is gone with the panel
        /// itself (R5-pre prefab pass): there is no longer an object to strand.
        /// </summary>
        public void HideTastePanel()
        {
            _revealedTaste = null;
            _tasteRevealed = false;

            if (revealedTasteIcon != null)
                revealedTasteIcon.SetActive(false);
        }

        // Mirrors the four frozen taste axes of SSoT_Audience_and_Reactions section 6.1
        // in the same order the impression algorithm walks them. An axis with no
        // authored data prints nothing, so a neutral archetype (all axes disabled,
        // always 0 impression) reads as "doesn't care" instead of an empty box.
        private static string BuildTasteText(TastePreferences t)
        {
            if (t == null) return TasteNeutralText;

            var sb = new System.Text.StringBuilder();

            if (t.tempoMatchOnFast)
                AppendTasteLine(sb, GameText.Format(UiAudTasteFast));      // [TXT-4]
            if (t.tempoMismatchOnSlow)
                AppendTasteLine(sb, GameText.Format(UiAudTasteSlow));

            if (t.roleCountMatchOnRich)
                AppendTasteLine(sb, GameText.Format(UiAudTasteTracks, t.preferAtLeastRoles));

            if (t.preferredTimeSignatures != null && t.preferredTimeSignatures.Count > 0)
                AppendTasteLine(sb, "+ " + string.Join(", ", t.preferredTimeSignatures));
            if (t.dislikedTimeSignatures != null && t.dislikedTimeSignatures.Count > 0)
                AppendTasteLine(sb, "- " + string.Join(", ", t.dislikedTimeSignatures));

            if (t.preferredTonalities != null && t.preferredTonalities.Count > 0)
                AppendTasteLine(sb, "+ " + string.Join(", ", t.preferredTonalities));
            if (t.dislikedTonalities != null && t.dislikedTonalities.Count > 0)
                AppendTasteLine(sb, "- " + string.Join(", ", t.dislikedTonalities));

            return sb.Length == 0 ? TasteNeutralText : sb.ToString();
        }

        private static void AppendTasteLine(System.Text.StringBuilder sb, string line)
        {
            if (sb.Length > 0) sb.Append('\n');
            sb.Append(line);
        }
    }
}