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
        public void SetVibeTelegraph(VibeEffectiveness tier, int projectedNumber, bool showNumber)
        {
            if (vibeTelegraphRoot != null)
                vibeTelegraphRoot.SetActive(true);

            if (effectivenessLabel != null)
            {
                effectivenessLabel.gameObject.SetActive(true);
                effectivenessLabel.text = LabelFor(tier);
                effectivenessLabel.color = ColorFor(tier);
            }

            if (projectedVibeText != null)
            {
                bool show = showNumber && tier != VibeEffectiveness.Immune;
                projectedVibeText.gameObject.SetActive(show);
                if (show)
                    projectedVibeText.text = $"+{projectedNumber}";
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

        private static string LabelFor(VibeEffectiveness tier)
        {
            switch (tier)
            {
                case VibeEffectiveness.SuperEffective: return "Super!";
                case VibeEffectiveness.NotVeryEffective: return "Resists";
                case VibeEffectiveness.Immune: return "Immune";
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
    }
}