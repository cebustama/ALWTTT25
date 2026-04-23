using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.Status;
using ALWTTT.Status.Runtime;
using UnityEngine;

namespace ALWTTT.Characters
{
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        public virtual IMusicianStats MusicianStats => null;
        public virtual IAudienceStats AudienceStats => null;

        [Header("Base settings")]
        [SerializeField] private CharacterType characterType;
        [SerializeField] private Transform textSpawnRoot;
        [SerializeField] private Transform headRoot;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] protected Color obscuredColor;
        [SerializeField] protected Transform speechBubblePrefab;
        [SerializeField] protected LayerMask characterLayerMask;
        [SerializeField] protected CharacterAnimator characterAnimator;
        [SerializeField] protected Animator animator;

        [Header("Status (Runtime)")]
        [Tooltip("Optional: assign the catalogue to allow systems to resolve ids to StatusEffectSO later. Not required for Step 3.")]
        [SerializeField] private StatusEffectCatalogueSO statusCatalogue;

        public StatusEffectCatalogueSO StatusCatalogue => statusCatalogue;

        /// <summary>
        /// Runtime active statuses for this character (Step 3).
        /// </summary>
        public StatusEffectContainer Statuses { get; private set; }

        [Header("Hover Highlight (M1.7)")]
        [Tooltip("Optional: assign the SpriteOutlineController component on this character's sprite child. If null, hover highlight is a no-op (prefab migration-safe).")]
        [SerializeField] private SpriteOutlineController outlineController;

        #region Encapsulation
        public CharacterType CharacterType => characterType;
        public Transform TextSpawnRoot => textSpawnRoot;
        public Transform HeadRoot => headRoot;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public CharacterAnimator CharacterAnimator => characterAnimator;
        public Animator Animator => animator;
        #endregion

        #region Cache
        protected GigManager GigManager => GigManager.Instance;
        protected GameManager GameManager => GameManager.Instance;

        // Legacy backing field (migration).
        [SerializeField, Tooltip("Legacy stun flag (migration). Prefer deriving from CSO.DisableActions.")]
        private bool legacyIsStunned;

        /// <summary>
        /// Migration rule:
        /// - If runtime statuses exist, IsStunned is derived from DisableActions stacks.
        /// - Otherwise fallback to legacyIsStunned.
        /// </summary>
        public bool IsStunned
        {
            get
            {
                if (Statuses != null)
                    return Statuses.HasActive(CharacterStatusId.DisableActions);
                return legacyIsStunned;
            }
            set
            {
                // Keep existing code from breaking.
                legacyIsStunned = value;
            }
        }
        #endregion

        private bool isPointerOver = false;

        protected virtual void Awake()
        {
            Statuses = new StatusEffectContainer();

            // Optional: keep legacy field synced for debugging/temporary old UI.
            Statuses.OnStatusChanged += (_, __) => SyncLegacyStunFromStatuses();
            Statuses.OnStatusCleared += _ => SyncLegacyStunFromStatuses();
            Statuses.OnStatusApplied += (_, __) => SyncLegacyStunFromStatuses();

            SyncLegacyStunFromStatuses();
        }

        private void SyncLegacyStunFromStatuses()
        {
            if (Statuses == null) return;
            legacyIsStunned = Statuses.HasActive(CharacterStatusId.DisableActions);
        }

        protected virtual void Update()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, characterLayerMask))
            {
                var charHit = hit.collider.GetComponent<ICharacter>();
                if (charHit != null && charHit.GetCharacterBase() == this)
                {
                    if (!isPointerOver)
                    {
                        isPointerOver = true;
                        OnPointerEnter();
                    }
                    return;
                }
            }

            if (isPointerOver)
            {
                isPointerOver = false;
                OnPointerExit();
            }
        }

        public virtual void BuildCharacter() { }

        public CharacterBase GetCharacterBase() => this;
        public CharacterType GetCharacterType() => CharacterType;

        protected virtual void OnPointerEnter()
        {
            if (outlineController != null) outlineController.SetOutline(true);
        }

        protected virtual void OnPointerExit()
        {
            if (outlineController != null) outlineController.SetOutline(false);
        }
    }
}
