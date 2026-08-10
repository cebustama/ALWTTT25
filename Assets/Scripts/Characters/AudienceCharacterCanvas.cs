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

        // [R4 / D-R4-4=A] Taste reveal panel (Read the Room). Optional/null-guarded
        // like the S5a telegraph slots: an unwired prefab degrades to "no panel"
        // rather than an NRE.
        [Header("Taste Reveal (R4)")]
        [SerializeField] private GameObject tastePanelRoot;
        [SerializeField] private TextMeshProUGUI tasteText;

        /// <summary>[R4-SMOKE] True when both reveal slots are wired on the prefab.</summary>
        public bool IsTastePanelWired => tastePanelRoot != null && tasteText != null;

        protected override void ShowTooltipInfo()
        {
            base.ShowTooltipInfo();

            if (NextAbility != null && CurrentIntention != null)
            {
                var abilityName = NextAbility.AbilityName;
                var contentText = CurrentIntention.ContentText;

                ShowTooltipInfo(
                    TooltipManager.Instance, contentText, abilityName, descriptionRoot);
            }
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
                case VibeEffectiveness.SuperEffective: return "�S�per!";
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
        // default; D-S5f-7=A � hardcoded until the S5f-ext localization pass).
        // ENG: "Blocked � someone tall is in the way. Immune to persuasion
        // from this position."
        private const string BlockedTooltipHeader = "Bloqueado";
        private const string BlockedTooltipBody =
            "Alguien alto le tapa el escenario. Es inmune a la persuasi�n " +
            "mientras est� en esta posici�n.";

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

        // ---- [R4 / D-R0-1=A + D-R4-4=A] Taste reveal --------------------------
        //
        // Presentation only: no state lives here. AudienceCharacterBase owns the
        // "has been revealed" flag; AudienceCharacterData owns the preference data.
        // ESP copy is hardcoded, matching the Blocked tooltip and the telegraph
        // labels (D-S5f-7=A / D-S5f-8=A); all three migrate together in the S5f-ext
        // localization pass.

        private const string TasteNeutralText = "Le da igual todo";

        /// <summary>
        /// [R4] Show this member's taste panel. Persistent once shown - the caller
        /// (AudienceCharacterBase.RevealPreferences) guarantees one call per gig.
        /// </summary>
        public void ShowTastePanel(TastePreferences taste)
        {
            if (tastePanelRoot == null || tasteText == null) return;

            tasteText.text = BuildTasteText(taste);
            tastePanelRoot.SetActive(true);
        }

        /// <summary>[R4] Hide the taste panel (gig teardown / dev reset).</summary>
        public void HideTastePanel()
        {
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