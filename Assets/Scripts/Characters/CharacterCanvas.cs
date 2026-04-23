using ALWTTT.Status;
using ALWTTT.Status.Runtime;
using ALWTTT.Tooltips;
using ALWTTT.UI;
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

        // M1.2: lazy dictionary — entries created on first status application, removed on clear.
        private readonly Dictionary<CharacterStatusId, StatusIconBase> _activeIcons = new();

        private StatusEffectContainer _boundContainer;

        #region Setup

        public void InitCanvas(string characterName)
        {
            characterNameText.text = characterName;
            // No pre-population — icons are created lazily via container events.
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
            // but we want total stacks — read from container).
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
                // Status changed but no icon yet — try to create.
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
            healthBar?.SetCurrentValue(current, max, duration);
        }

        public void SetHighlight(bool open) =>
            highlightRoot.gameObject.SetActive(open);

        public void UpdateVisibility()
        {
            ShowContextual();
            HideContextual();
        }

        public virtual void HideContextual()
        {
            if (healthBar.CurrentValue == 0)
            {
                healthBar.CanvasGroup.alpha = 0;
            }
        }

        public virtual void ShowContextual()
        {
            if (healthBar.CurrentValue > 0)
                healthBar.CanvasGroup.alpha = 1;
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