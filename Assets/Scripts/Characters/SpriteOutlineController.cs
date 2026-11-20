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

        // [S4b] Second channel: the hover views paint related characters in a musician's
        // identityColor. Hover (the channel above, also the card-target outline) always wins,
        // so the character under the mouse keeps the colour the player learned for "this one".
        private bool _viewActive;
        private Color _viewColor;

        // [S4b / D5=A] Third channel, lowest priority: "this musician will lose fortitude
        // this audience turn". Always red: identity colours are never red (D9), so red keeps
        // meaning damage everywhere on screen.
        [Tooltip("[S4b] Outline colour for a musician who will lose fortitude this audience turn.")]
        [SerializeField] private Color damageColor = new Color(0.95f, 0.2f, 0.2f, 1f);
        private bool _damageActive;

        // [COMBOS-1 / D3] Canal de combo: al pasar una carta que completaría un combo, los
        // músicos que lo forman se encienden. Por debajo del hover (el objetivo de la carta
        // conserva su color) y por encima de las vistas de Tab y del daño, que vuelven solas
        // al soltar la carta.
        private bool _comboActive;
        private Color _comboColor;

        /// <summary>
        /// Whether the hover outline is currently on (the view channel does not count).
        /// </summary>
        public bool IsActive => _active;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _block = new MaterialPropertyBlock();
            Apply();
        }

        /// <summary>
        /// Enable or disable the hover outline. Idempotent.
        /// </summary>
        public void SetOutline(bool on)
        {
            if (_active == on) return;
            _active = on;
            Apply();
        }

        /// <summary>
        /// [S4b] View-channel outline in <paramref name="color"/>; shown only while the hover
        /// channel is off. Idempotent.
        /// </summary>
        public void SetViewHighlight(bool on, Color color)
        {
            if (_viewActive == on && (!on || _viewColor == color)) return;
            _viewActive = on;
            _viewColor = color;
            Apply();
        }

        /// <summary>[COMBOS-1] Contorno de combo en <paramref name="color"/>. Idempotente.</summary>
        public void SetComboHighlight(bool on, Color color)
        {
            if (_comboActive == on && (!on || _comboColor == color)) return;
            _comboActive = on;
            _comboColor = color;
            Apply();
        }

        /// <summary>[S4b / D5=A] Damage-channel outline (red); shown only when neither hover
        /// nor a view is on. Idempotent.</summary>
        public void SetDamageHighlight(bool on)
        {
            if (_damageActive == on) return;
            _damageActive = on;
            Apply();
        }

        private void Apply()
        {
            if (_renderer == null) return;
            bool visible = _active || _comboActive || _viewActive || _damageActive;
            Color c = _active ? outlineColor : _comboActive ? _comboColor
                    : _viewActive ? _viewColor : damageColor;
            _renderer.GetPropertyBlock(_block);
            _block.SetFloat(OutlineWidthId, visible ? outlineWidth : 0f);
            _block.SetColor(OutlineColorId, c);
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
            Apply();
        }
#endif
    }
}
