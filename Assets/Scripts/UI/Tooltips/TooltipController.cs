using System;
using UnityEngine;

namespace ALWTTT.Tooltips
{
    public class TooltipController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRectTransform;

        private RectTransform rectTransform;
        private Vector2 followPos = Vector2.zero;
        private Camera cachedCamera;
        private Camera followCamera;
        private Transform lastStaticTarget;
        private bool isFollowEnabled;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetFollowPos(Transform staticTargetTransform = null, Camera cam = null)
        {
            if (staticTargetTransform)
            {
                var mainCam = cam;
                if (mainCam == null)
                {
                    if (!cachedCamera)
                        cachedCamera = Camera.main;

                    mainCam = cachedCamera;
                }

                if (mainCam == null)
                {
                    SetFollowPos();
                    return;
                }
                else
                {
                    followCamera = mainCam;
                }

                lastStaticTarget = staticTargetTransform;
                isFollowEnabled = false;
            }
            else
            {
                followCamera = null;
                lastStaticTarget = null;
                isFollowEnabled = true;
            }
        }

        private void Update()
        {
            SetPosition();
        }

        private void SetPosition()
        {
            if (isFollowEnabled)
            {
                followPos = Input.mousePosition;
            }
            else
            {
                if (followCamera && lastStaticTarget)
                {
                    followPos = followCamera.WorldToScreenPoint(lastStaticTarget.position);
                }
            }

            var anchoredPos = followPos / canvasRectTransform.localScale.x;

            if (anchoredPos.x + rectTransform.rect.width > canvasRectTransform.rect.width)
                anchoredPos.x = canvasRectTransform.rect.width - rectTransform.rect.width;

            if (anchoredPos.y + rectTransform.rect.height > canvasRectTransform.rect.height)
                anchoredPos.y = canvasRectTransform.rect.height - rectTransform.rect.height;

            rectTransform.anchoredPosition = anchoredPos;
        }
    }
}