using ALWTTT.Status;
using ALWTTT.Status.Runtime;
using ALWTTT.Tooltips;
using ALWTTT.UI;
using ALWTTT.Tutorial;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ALWTTT.Characters
{
    [RequireComponent(typeof(Canvas))]
    public class CharacterCanvas : MonoBehaviour, I2DTooltipTarget
    {
        [Header("References")]
        [SerializeField] protected Slider currentHealthBar;
        [SerializeField] protected TextMeshProUGUI currentHealthText;
        [SerializeField] protected TextMeshProUGUI characterNameText;
        [SerializeField] protected Transform highlightRoot;
        [SerializeField] protected Transform statusIconRoot;
        [SerializeField] protected Transform descriptionRoot;
        [SerializeField] protected HealthBarController healthBar;

        [Header("Status Icons")]
        [Tooltip("Prefab instantiated for each active status. Sprite and stack count " +
                 "are assigned at runtime from the character's StatusEffectContainer.")]
        [SerializeField] protected StatusIconBase statusIconBasePrefab;

        // M1.2: lazy dictionary � entries created on first status application, removed on clear.
        private readonly Dictionary<CharacterStatusId, StatusIconBase> _activeIcons = new();

        private StatusEffectContainer _boundContainer;

        // [S5e-ext] Last meter values pushed through SetCurrentVibe /
        // BandCharacterCanvas.SetCurrentStress. Drives full-bar concealment.
        private int _meterCurrent = -1;
        private int _meterMax = -1;

        /// <summary>[S5e-ext] True once a meter value has been cached and it sits at Max.</summary>
        protected bool IsMeterFull => _meterMax > 0 && _meterCurrent >= _meterMax;

        /// <summary>[CARD-UX-1] Rect of the primary meter bar (musician Stress /
        /// audience Vibe) for world→screen tutorial highlights. Null-safe.</summary>
        public RectTransform MeterBarRect =>
            currentHealthBar != null ? currentHealthBar.transform as RectTransform : null;

        /// <summary>
        /// [S5e-ext] Cache the latest meter state. Called by SetCurrentVibe here
        /// and by BandCharacterCanvas.SetCurrentStress. Must run BEFORE
        /// UpdateVisibility so the steady-state rule sees fresh values.
        /// </summary>
        protected void CacheMeterValue(int current, int max)
        {
            _meterCurrent = current;
            _meterMax = max;
        }

        #region Setup

        public void InitCanvas(string characterName)
        {
            characterNameText.text = characterName;
            // No pre-population � icons are created lazily via container events.
        }

        /// <summary>
        /// Wire this canvas to a character's StatusEffectContainer.
        /// Call once after the container exists (e.g. in BuildCharacter, after base.BuildCharacter).
        /// </summary>
        public void BindStatusContainer(StatusEffectContainer container)
        {
            // Unbind previous if any
            if (_boundContainer != null)
            {
                _boundContainer.OnStatusChanged -= HandleStatusChanged;
                _boundContainer.OnStatusCleared -= HandleStatusCleared;
                _boundContainer.OnStatusApplied -= HandleStatusApplied;
            }

            _boundContainer = container;

            if (_boundContainer != null)
            {
                _boundContainer.OnStatusChanged += HandleStatusChanged;
                _boundContainer.OnStatusCleared += HandleStatusCleared;
                _boundContainer.OnStatusApplied += HandleStatusApplied;
            }
        }

        private void OnDestroy()
        {
            // Safety: unsubscribe to avoid leaks if canvas is destroyed before unbind.
            BindStatusContainer(null);
        }

        #endregion

        #region Status Icon Event Handlers

        private void HandleStatusApplied(CharacterStatusId id, int deltaStacks)
        {
            // If icon doesn't exist yet, create it.
            if (!_activeIcons.ContainsKey(id))
                TryCreateIcon(id);

            // Update stack count (container fires OnStatusApplied with delta,
            // but we want total stacks � read from container).
            if (_activeIcons.TryGetValue(id, out var icon) && _boundContainer != null)
            {
                int totalStacks = _boundContainer.GetStacks(id);
                icon.SetStatusValue(totalStacks);
            }
        }

        private void HandleStatusChanged(CharacterStatusId id, int newStacks)
        {
            if (!_activeIcons.TryGetValue(id, out var icon))
            {
                // Status changed but no icon yet � try to create.
                TryCreateIcon(id);
                if (!_activeIcons.TryGetValue(id, out icon)) return;
            }

            icon.SetStatusValue(newStacks);
        }

        private void HandleStatusCleared(CharacterStatusId id)
        {
            if (_activeIcons.TryGetValue(id, out var icon))
            {
                // M1.8: detach from dictionary BEFORE playing disappear.
                // If the status is re-applied while this icon is still animating out,
                // HandleStatusApplied will create a fresh icon rather than collide.
                // The detached icon self-destroys when its disappear coroutine finishes.
                _activeIcons.Remove(id);

                if (icon != null)
                    icon.PlayDisappear();
            }
        }

        private void TryCreateIcon(CharacterStatusId id)
        {
            if (_activeIcons.ContainsKey(id)) return;

            if (statusIconBasePrefab == null)
            {
                Debug.LogWarning(
                    $"[CharacterCanvas] '{name}' has no StatusIconBase prefab assigned. " +
                    $"Cannot display status icon for '{id}'. " +
                    $"Assign 'statusIconBasePrefab' on the canvas component.",
                    this);
                return;
            }

            if (_boundContainer == null) return;
            if (!_boundContainer.TryGet(id, out var instance) || instance == null) return;

            var def = instance.Definition;
            if (def == null) return;

            if (def.IconSprite == null)
            {
                Debug.LogWarning(
                    $"[CharacterCanvas] StatusEffectSO '{def.name}' (key='{def.StatusKey}') " +
                    $"has no IconSprite assigned. Status will apply but no icon will display.",
                    def);
                return;
            }

            var clone = Instantiate(statusIconBasePrefab, statusIconRoot);
            clone.SetStatus(def.IconSprite);
            clone.BindTooltipSource(def, _boundContainer, id);
            _activeIcons[id] = clone;

            // [CARD-UX-1 / D1=C] Register the fresh icon as a tutorial highlight
            // target. Icons spawn exactly when the status applies (= when the
            // dialog fires), so registry last-registered-wins points at the right
            // icon by construction.
            ALWTTT.Tutorial.TutorialHighlightSpawnHook.AttachToStatusIcon(
                clone, id, musicianSide: this is Band.BandCharacterCanvas);

            // M1.8: trigger appear popup animation.
            clone.PlayAppear();
        }

        #endregion

        #region Public Methods

        public void UpdateHealthText(int currentHealth, int maxHealth)
        {
            float fill = (float)currentHealth / maxHealth;
            currentHealthBar.value = fill;
            currentHealthText.text = $"{currentHealth}/{maxHealth}";
        }

        public void SetCurrentVibe(int current, int max, float duration)
        {
            CacheMeterValue(current, max); // [S5e-ext]
            healthBar?.SetCurrentValue(current, max, duration);
        }

        public void SetHighlight(bool open) =>
            highlightRoot.gameObject.SetActive(open);

        // [S5e-ext] Bar visibility policy (supersedes the pre-S5e "hidden until
        // damaged" convention and the transitional post-S5e "always visible"):
        //   - meter at Max  -> bar hidden (no information to convey)
        //   - meter below Max -> bar persistently visible (including at 0:
        //     an empty bar reads as collapsed/conquered)
        //   - hover (pointer enter/exit via MusicianBase/AudienceCharacterBase)
        //     -> reveal while hovered; on exit, re-conceal only if full.
        // UpdateVisibility applies the steady-state rule directly on the bar and
        // deliberately does NOT route through the Show/HideContextual virtuals,
        // so meter changes no longer toggle hover-only chrome (e.g. the band
        // stats panel) as a side effect.

        private void SetBarVisible(bool visible)
        {
            if (healthBar != null && healthBar.CanvasGroup != null)
                healthBar.CanvasGroup.alpha = visible ? 1f : 0f;
        }

        /// <summary>Steady-state rule; called on every meter change.</summary>
        public void UpdateVisibility()
        {
            SetBarVisible(!IsMeterFull);
        }

        /// <summary>Hover-exit / build-time conceal: hides the bar only when full.</summary>
        public virtual void HideContextual()
        {
            if (IsMeterFull)
                SetBarVisible(false);
        }

        /// <summary>Hover-enter reveal: always shows the bar, full or not.</summary>
        public virtual void ShowContextual()
        {
            SetBarVisible(true);
        }

        #endregion

        #region Pointer Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltipInfo();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltipInfo(TooltipManager.Instance);
        }
        #endregion

        #region Tooltips
        /// <summary>
        /// Status tooltip content is intentionally not displayed in M1.2.
        /// M1.3 (Tooltip pipeline extension) will source tooltip content from
        /// StatusEffectSO directly (DisplayName + description field to be added).
        /// Subclasses may override to show their own tooltips (e.g. audience ability).
        /// </summary>
        protected virtual void ShowTooltipInfo()
        {
            // M1.3 will populate this from SO-derived data.
        }

        public void ShowTooltipInfo(TooltipManager tooltipManager,
            string content, string header = "",
            Transform tooltipStaticTransform = null, Camera cam = null, float delayShow = 0)
        {
            if (tooltipManager == null) return;
            tooltipManager.ShowTooltip(
                content, header, tooltipStaticTransform, cam, delayShow);
        }

        public void HideTooltipInfo(TooltipManager tooltipManager)
        {
            if (tooltipManager == null) return;
            tooltipManager.HideTooltip();
        }
        #endregion
    }
}