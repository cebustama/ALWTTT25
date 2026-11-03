// Place at: Assets/Scripts/UI/Tooltips/TooltipController.cs
//
// [TIP-1 / F-BN-8 closed / D-TIP-2=B] The tooltip panel's placement.
//
// TWO PLACEMENT MODES, one per SetFollowPos call:
//   - cursor-follow (no target): followPos = Input.mousePosition every frame.
//   - static target: followPos = the target's SCREEN point (+ optional pixel
//     offset) every frame, so a target that moves or rescales is followed.
//
// HOW THE TARGET'S SCREEN POINT IS OBTAINED (D-TIP-2=B, inferred, not declared):
// the target's ROOT canvas decides the projection, and RectTransformUtility
// .WorldToScreenPoint does the math with the camera that canvas implies:
//   root canvas              | camera passed to WorldToScreenPoint
//   Screen Space - Overlay   | null  (the RectTransform's .position IS pixels)
//   Screen Space - Camera    | cam param, else canvas.worldCamera, else Camera.main
//   World Space / no canvas  | cam param, else Camera.main   (pre-TIP-1 behaviour)
// Before TIP-1 every static target went through Camera.main.WorldToScreenPoint,
// which is right for the world-space character canvases and wrong for anything
// under an overlay canvas: pixels projected as if they were metres came back as
// a huge coordinate and the clamp pinned the panel to a corner (F-BN-8). The
// caller does not declare the space because it cannot get it wrong this way:
// the space is a property of the transform, not of the call site.
//
// CLAMP ORDER RULE (same rule TrackHoverPanel.PlaceNextTo validated in camera):
// when the panel cannot fit, the clamp applied LAST wins. Left is applied after
// right and top after bottom, so LEFT and TOP win: the panel's first line, at its
// top-left, carries the identity (header) and is the last thing to sacrifice.
// The pivot is (0,0) and anchors are (0,0), so anchoredPosition is the panel's
// bottom-left corner in canvas-local pixels (Screen pixels / canvas scale).
//
// SOURCE IS PURE ASCII ON PURPOSE (F-T6-1 / F-SP1-2 family).

using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Tooltips
{
    public class TooltipController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRectTransform;

        private RectTransform rectTransform;
        private Vector2 followPos = Vector2.zero;
        private Camera cachedCamera;

        // Static-target state (valid while isFollowEnabled == false).
        private Transform lastStaticTarget;
        // null means "the target is already in screen pixels" (Screen Space - Overlay).
        private Camera projectionCamera;
        private Vector2 screenOffset;
        private bool isFollowEnabled = true;
        private bool anchorTop;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Choose the placement mode. No target = follow the cursor. With a target,
        /// the panel sits at the target's screen point plus <paramref name="offset"/>
        /// (pixels; negative X = left, negative Y = down), re-evaluated every frame.
        /// </summary>
        public void SetFollowPos(Transform staticTargetTransform = null, Camera cam = null,
            Vector2 offset = default(Vector2), bool growDown = false)
        {
            screenOffset = offset;
            // [GIG-UI-1] Static targets only: the point is the panel's TOP-left, so a
            // column stacked under it grows downward (audience hover column).
            anchorTop = growDown && staticTargetTransform != null;

            if (staticTargetTransform == null)
            {
                FollowCursor();
                return;
            }

            // [D-TIP-2=B] The transform's ROOT canvas decides how it projects to screen.
            var canvas = staticTargetTransform.GetComponentInParent<Canvas>();
            if (canvas != null) canvas = canvas.rootCanvas;

            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Overlay: .position is already the screen point. No camera, no projection.
                projectionCamera = null;
            }
            else
            {
                Camera c = cam;
                if (c == null && canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    c = canvas.worldCamera;
                if (c == null)
                {
                    if (!cachedCamera) cachedCamera = Camera.main;
                    c = cachedCamera;
                }
                if (c == null)
                {
                    // Pre-TIP-1 fallback kept: a world target with no camera cannot be
                    // projected, so the panel follows the cursor instead of vanishing.
                    FollowCursor();
                    return;
                }
                projectionCamera = c;
            }

            lastStaticTarget = staticTargetTransform;
            isFollowEnabled = false;
        }

        /// <summary>
        /// [TIP-1 / F-SP1-12 shape] Force the layout to catch up with freshly assigned
        /// text, THEN place. The panel is content-sized (ContentSizeFitter), so its
        /// rect lags one layout pass behind a SetText; clamping against the stale rect
        /// places the panel with the PREVIOUS tooltip's size. Called by TooltipManager
        /// after every text assignment; Update keeps placing every frame afterwards.
        /// </summary>
        public void Reposition()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            SetPosition();
        }

        private void Update()
        {
            SetPosition();
        }

        private void FollowCursor()
        {
            projectionCamera = null;
            lastStaticTarget = null;
            isFollowEnabled = true;
        }

        private void SetPosition()
        {
            if (rectTransform == null || canvasRectTransform == null) return;

            if (isFollowEnabled)
            {
                followPos = Input.mousePosition;
            }
            else if (lastStaticTarget != null)
            {
                // Overlay canvas: projectionCamera is null and this returns .position.xy.
                // Anything else: cam.WorldToScreenPoint. One call, both cases.
                followPos = RectTransformUtility.WorldToScreenPoint(
                    projectionCamera, lastStaticTarget.position) + screenOffset;
            }
            // A static target destroyed mid-hover keeps the last followPos (pre-TIP-1
            // behaviour); the surface's own PointerExit hides the panel.

            var anchoredPos = followPos / canvasRectTransform.localScale.x;

            float w = rectTransform.rect.width;
            float h = rectTransform.rect.height;
            float cw = canvasRectTransform.rect.width;
            float ch = canvasRectTransform.rect.height;

            // [GIG-UI-1] Top-anchored: the target point is the top-left corner.
            if (anchorTop && !isFollowEnabled) anchoredPos.y -= h;

            // Clamp order rule: the LAST clamp wins when both cannot hold.
            // Horizontal: right, then left  => LEFT wins.
            if (anchoredPos.x + w > cw) anchoredPos.x = cw - w;
            if (anchoredPos.x < 0f) anchoredPos.x = 0f;
            // Vertical: bottom, then top    => TOP wins (first line carries identity).
            if (anchoredPos.y < 0f) anchoredPos.y = 0f;
            if (anchoredPos.y + h > ch) anchoredPos.y = ch - h;

            rectTransform.anchoredPosition = anchoredPos;
        }
    }
}