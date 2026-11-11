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

        [Tooltip("[PRES-1b T6b] Sprite outline used as the hover highlight. Lives on " +
         "the SpriteRenderer GameObject. Null = character degrades to no outline.")]
        [SerializeField] private SpriteOutlineController spriteOutline;

        public SpriteOutlineController SpriteOutline => spriteOutline;

        [Header("Status (Runtime)")]
        [Tooltip("Optional: assign the catalogue to allow systems to resolve ids to StatusEffectSO later. Not required for Step 3.")]
        [SerializeField] private StatusEffectCatalogueSO statusCatalogue;

        public StatusEffectCatalogueSO StatusCatalogue => statusCatalogue;

        /// <summary>
        /// Runtime active statuses for this character (Step 3).
        /// </summary>
        public StatusEffectContainer Statuses { get; private set; }

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

        /// <summary>[S4b] The character under the mouse (topmost by sorting), or null.
        /// Read by the hover views; written only by each character's own enter/exit.</summary>
        public static CharacterBase Hovered { get; private set; }

        protected virtual void Awake()
        {
            if (spriteOutline == null && spriteRenderer != null)
                spriteOutline = spriteRenderer.GetComponent<SpriteOutlineController>();

            Statuses = new StatusEffectContainer();

            Statuses.SetOwner(this); // [TUT-R2] owner identity for bus consumers

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
            if (RaycastTopmost(ray, 100f, characterLayerMask, out var hit))
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

        // [GIG-UI-1] Overlapping colliders: Physics.Raycast returns the hit nearest the
        // camera in 3D, which is not the character DRAWN on top (that is sortingLayer /
        // sortingOrder; GigManager puts front-stage musicians at order 10). Pick the hit
        // whose character renders on top, nearest distance as tie-break. Used by hover
        // (here) and by card targeting (HandController) so outline and target agree.
        private static readonly RaycastHit[] _pickBuffer = new RaycastHit[16];

        public static bool RaycastTopmost(Ray ray, float maxDistance, LayerMask mask,
            out RaycastHit best)
        {
            best = default;
            int n = Physics.RaycastNonAlloc(ray, _pickBuffer, maxDistance, mask);
            bool found = false;
            int bestLayer = int.MinValue, bestOrder = int.MinValue;
            for (int i = 0; i < n; i++)
            {
                var h = _pickBuffer[i];
                var ch = h.collider.GetComponent<ICharacter>();
                var sr = ch != null && ch.GetCharacterBase() != null
                    ? ch.GetCharacterBase().SpriteRenderer : null;
                int layer = sr != null ? SortingLayer.GetLayerValueFromID(sr.sortingLayerID) : int.MinValue;
                int order = sr != null ? sr.sortingOrder : int.MinValue;

                bool better = !found
                    || layer > bestLayer
                    || (layer == bestLayer && order > bestOrder)
                    || (layer == bestLayer && order == bestOrder && h.distance < best.distance);
                if (better)
                {
                    best = h; bestLayer = layer; bestOrder = order; found = true;
                }
            }
            return found;
        }

        public virtual void BuildCharacter() { }

        public CharacterBase GetCharacterBase() => this;
        public CharacterType GetCharacterType() => CharacterType;

        // [PRES-1b T6b] Hover highlight lives here so every character type inherits it;
        // subclasses already call base.OnPointerEnter/Exit before their own chrome.
        protected virtual void OnPointerEnter()
        {
            Hovered = this;
            if (spriteOutline != null) spriteOutline.SetOutline(true);
        }

        protected virtual void OnPointerExit()
        {
            // Enter of the next character may run before this exit in the same frame.
            if (Hovered == this) Hovered = null;
            if (spriteOutline != null) spriteOutline.SetOutline(false);
        }

        protected virtual void OnDisable()
        {
            if (Hovered == this) Hovered = null;
        }

        /// <summary>
        /// [B2.5 / #3] Broadcast the song BPM to every <see cref="CharacterAnimator"/>
        /// under this character (body + any instrument / sub-part animators). Only
        /// the BPM cascades — other animator settings (<c>SkipEveryNBeats</c>,
        /// <c>BeatOffsetBeats</c>, <c>JumpOnBeat</c>, etc.) remain author-controlled
        /// on the body animator referenced by <see cref="CharacterAnimator"/>.
        ///
        /// Iteration uses <c>GetComponentsInChildren&lt;CharacterAnimator&gt;</c> so
        /// future sub-animators are picked up automatically without touching callers.
        /// When a character has only the body animator (current default) this
        /// degenerates to the previous single-component behavior.
        /// </summary>
        public void BroadcastBPM(int bpm)
        {
            var anims = GetComponentsInChildren<CharacterAnimator>(includeInactive: false);
            for (int i = 0; i < anims.Length; i++)
            {
                if (anims[i] != null) anims[i].SetBPM(bpm);
            }
        }
    }
}