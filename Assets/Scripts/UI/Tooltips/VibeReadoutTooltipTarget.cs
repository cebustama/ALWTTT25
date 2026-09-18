// Place at: Assets/Scripts/UI/Tooltips/VibeReadoutTooltipTarget.cs
//
// [BIGNUM-1r / D-BN-13=A] Hover tooltip on the big C1 Vibe number: the band-wide
// chain (Hype % -> L, + SFX = N, and what one average member can take). Content is
// pushed by GigCanvas.SetVibeReadout at every projection refresh. Requires
// raycastTarget ON on the label (TMP -> Extra Settings). Pattern: EconPipTooltipTarget.
//
// [TIP-1] TWO WORKAROUNDS RETIRED, both recorded in Design_Vibe_Telegraph_v0_1.md
// section 10.6 as D-BN-18=B'' and in the BIGNUM-1r4 note on F-BN-7:
//   - the RUNTIME ANCHOR. TooltipController could only place a panel on a target
//     it projected through Camera.main (F-BN-8), so this component created a
//     DontDestroyOnLoad GameObject and parked it every frame at the world point
//     that projected back to the wanted pixel. TooltipController now reads the
//     target's root canvas and, for a Screen Space - Overlay canvas, takes the
//     transform's .position as the screen point (D-TIP-2=B). So the tooltip
//     anchors on THIS transform, with screenOffset passed straight through.
//   - the HIDE-BEFORE-SHOW on every refresh. ShowTooltip pushes a panel per call,
//     so a live surface stacked one per loop unless it hid first, and hiding
//     replayed the fade. TooltipManager.UpdateTooltip rewrites the shown panel in
//     place; the fade plays once, on hover-in.
// anchorOverride and anchorDepth are gone with the anchor. F-BN-9 (no serializable
// cross-scene reference from ALWTTTCore to the gig scene) is no longer relevant
// to this component: it references nothing outside itself.
//
// SOURCE IS PURE ASCII ON PURPOSE (F-T6-1 / F-SP1-2 family).

using UnityEngine;
using UnityEngine.EventSystems;

namespace ALWTTT.Tooltips
{
    public class VibeReadoutTooltipTarget : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Placement")]
        [Tooltip("Pixel offset from the number's own screen position. Negative X = left, " +
                 "negative Y = down. Read when the hover opens: re-enter the number to " +
                 "see a value changed in Play.")]
        [SerializeField] private Vector2 screenOffset = new Vector2(-260f, -60f);

        private string _header;
        private string _body;
        private bool _hovered;

        public void SetContent(string header, string body)
        {
            _header = header;
            _body = body;
            if (_hovered) Refresh();
        }

        public void Clear()
        {
            _header = null;
            _body = null;
            if (_hovered) TooltipManager.Instance?.HideTooltip();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            Refresh();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            TooltipManager.Instance?.HideTooltip();
        }

        private void Refresh()
        {
            var tm = TooltipManager.Instance;
            if (tm == null || string.IsNullOrEmpty(_body)) return;

            // [TIP-1 / F-BN-7 closed] Shown already: rewrite in place, no fade, no stack.
            if (tm.UpdateTooltip(_body, _header)) return;

            // First show of this hover: push one panel, anchored on the number itself.
            // The number lives under a Screen Space - Overlay canvas; TooltipController
            // infers that from the canvas and uses .position as pixels (D-TIP-2=B).
            tm.ShowTooltip(_body, _header, transform, null, 0f, screenOffset);
        }
    }
}