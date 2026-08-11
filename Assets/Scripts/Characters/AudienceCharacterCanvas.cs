using ALWTTT.Data;
using ALWTTT.Tooltips;
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

        // [PRES-1] RETIRED persistent-panel slots. Never activated at runtime any
        // more; kept so existing prefab wiring does not dangle before the prefab
        // pass deletes the panel object. Remove both fields in that pass.
        [SerializeField] private GameObject tastePanelRoot;
        [SerializeField] private TextMeshProUGUI tasteText;

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
        private const string TasteBlockHeader = "<b>— Gustos —</b>";
        private const string TasteOnlyHeader = "Gustos";

        protected override void ShowTooltipInfo()
        {
            base.ShowTooltipInfo();

            string header = null;
            string body = null;

            if (NextAbility != null && CurrentIntention != null)
            {
                header = NextAbility.AbilityName;
                body = CurrentIntention.ContentText;
            }

            // [PRES-1 / D-PRES1-3=A] Compose the revealed tastes INTO the one hover
            // surface instead of opening a second one. This also covers the phases
            // where there is no intention to show: under the old panel-less path
            // the tooltip simply did not appear, which would have made revealed
            // tastes vanish between turns.
            if (_tasteRevealed)
            {
                string tasteBlock = TasteBlockHeader + "\n" + BuildTasteText(_revealedTaste);
                body = string.IsNullOrEmpty(body)
                    ? tasteBlock
                    : body + "\n\n" + tasteBlock;

                if (header == null) header = TasteOnlyHeader;
            }

            if (body != null)
                ShowTooltipInfo(TooltipManager.Instance, body, header, descriptionRoot);
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
                    projectedVibeText.text = $"-{projectedNumber}";
            }
        }

        /// <summary>[S5a] Hide the telegraph (between songs / audience turn).</summary>
        public void HideVibeTelegraph()
        {
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
                case VibeEffectiveness.SuperEffective: return "¡Súper!";
                case VibeEffectiveness.NotVeryEffective: return "Resiste";
                case VibeEffectiveness.Immune: return "Inmune";
                default: return "Normal";
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
        private const string BlockedTooltipHeader = "Bloqueado";
        private const string BlockedTooltipBody =
            "Alguien alto le tapa el escenario. Es inmune a la persuasión " +
            "mientras está en esta posición.";

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

        private const string TasteNeutralText = "Le da igual todo";

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
        /// [R4 -> PRES-1] Clear the reveal (gig teardown / dev reset). Also hides
        /// the retired legacy panel defensively, so a prefab that still carries the
        /// old object cannot strand it on screen.
        /// </summary>
        public void HideTastePanel()
        {
            _revealedTaste = null;
            _tasteRevealed = false;

            if (revealedTasteIcon != null)
                revealedTasteIcon.SetActive(false);
            if (tastePanelRoot != null)
                tastePanelRoot.SetActive(false);
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
                AppendTasteLine(sb, "+ Rapido");
            if (t.tempoMismatchOnSlow)
                AppendTasteLine(sb, "- Lento");

            if (t.roleCountMatchOnRich)
                AppendTasteLine(sb, $"+ {t.preferAtLeastRoles}+ pistas");

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