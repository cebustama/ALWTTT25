using UnityEngine;

namespace ALWTTT.Characters
{
    /// <summary>
    /// Toggles a sprite outline via MaterialPropertyBlock on a SpriteRenderer.
    ///
    /// Requires the SpriteRenderer to use a material built from the
    /// <c>ALWTTT/SpriteOutlineURP</c> shader.
    ///
    /// Implementation notes:
    /// <list type="bullet">
    ///   <item>Uses MaterialPropertyBlock instead of direct material assignment so
    ///   we do not create per-renderer material instances. This preserves SRP
    ///   batching eligibility and avoids allocating GC on hover.</item>
    ///   <item>Intended to be placed on the same GameObject as the character's
    ///   <see cref="SpriteRenderer"/> (under <c>SpriteParent/Sprite</c> in the
    ///   character prefab hierarchy).</item>
    ///   <item><see cref="CharacterBase"/> is expected to hold a reference to
    ///   this component and call <see cref="SetOutline"/> from
    ///   <c>OnPointerEnter</c>/<c>OnPointerExit</c>.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteOutlineController : MonoBehaviour
    {
        [Tooltip("Outline width in texels of the sprite's main texture. 0 = no outline.")]
        [SerializeField, Range(0f, 8f)] private float outlineWidth = 2f;

        [Tooltip("Outline color applied when the outline is active.")]
        [SerializeField] private Color outlineColor = new Color(1f, 0.85f, 0.2f, 1f);

        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");

        private SpriteRenderer _renderer;
        private MaterialPropertyBlock _block;
        private bool _active;

        /// <summary>
        /// Whether the outline is currently visible.
        /// </summary>
        public bool IsActive => _active;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _block = new MaterialPropertyBlock();
            ApplyBlock(active: false);
        }

        /// <summary>
        /// Enable or disable the outline. Idempotent.
        /// </summary>
        public void SetOutline(bool on)
        {
            if (_active == on) return;
            _active = on;
            ApplyBlock(on);
        }

        private void ApplyBlock(bool active)
        {
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_block);
            _block.SetFloat(OutlineWidthId, active ? outlineWidth : 0f);
            _block.SetColor(OutlineColorId, outlineColor);
            _renderer.SetPropertyBlock(_block);
        }

#if UNITY_EDITOR
        /// <summary>
        /// In the editor, keep the preview in sync with inspector edits while
        /// playing (so designers can tune width/color live).
        /// </summary>
        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            if (_block == null) _block = new MaterialPropertyBlock();
            ApplyBlock(_active);
        }
#endif
    }
}
