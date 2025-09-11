using ALWTTT.Data;
using ALWTTT.Tooltips;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ALWTTT.Map
{
    /// <summary>
    /// Visual for a sector node. Uses a SpriteRenderer and color-coding by NodeType.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SectorNodeVisual : MonoBehaviour, I2DTooltipTarget, IPointerClickHandler
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public SectorNodeState Node { get; private set; }

        private string _tooltipTitle;
        private string _tooltipDesc;

        public event Action<SectorNodeVisual> Clicked;
        public event Action<SectorNodeVisual> HoverEnter;
        public event Action<SectorNodeVisual> HoverExit;

        public void Bind(SectorNodeState node, Color color, 
            string title, string description, 
            float scale = 0.5f)
        {
            Node = node;
            _tooltipTitle = title;
            _tooltipDesc = description;

            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = color;

            transform.localScale = Vector3.one * scale;
            name = $"Node_{node.Id}_{node.Type}";
        }

        public void SetVisited(bool visited)
        {
            if (!spriteRenderer) return;
            var c = spriteRenderer.color; c.a = visited ? 0.7f : 1f;
            spriteRenderer.color = c;
        }

        public void SetSelected(bool selected)
        {
            transform.localScale = selected ? Vector3.one * 0.65f : Vector3.one * 0.5f;
        }

        public void ShowTooltipInfo(
            TooltipManager tooltipManager,
            string content, string header = "",
            Transform tooltipStaticTransform = null,
            Camera cam = null, float delayShow = 0f)
        {
            tooltipManager.ShowTooltip(
                contentText: content,
                headerText: header,
                tooltipTargetTransform: tooltipStaticTransform ?? transform,
                cam: cam ?? Camera.main,
                delayShow: delayShow
            );
        }

        public void HideTooltipInfo(TooltipManager tooltipManager)
        {
            tooltipManager.HideTooltip();
        }

        // Pointer hover (requires EventSystem + Physics2DRaycaster on the Camera)
        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltipInfo(
                TooltipManager.Instance,
                content: _tooltipDesc,
                header: _tooltipTitle,
                tooltipStaticTransform: transform,
                cam: Camera.main,
                delayShow: 0.15f
            );
            HoverEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltipInfo(TooltipManager.Instance);
            HoverExit?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }
    }
}
