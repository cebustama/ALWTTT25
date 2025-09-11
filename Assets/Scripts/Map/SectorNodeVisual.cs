using ALWTTT.Data;
using UnityEngine;

namespace ALWTTT.Map
{
    /// <summary>
    /// Visual for a sector node. Uses a SpriteRenderer and color-coding by NodeType.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SectorNodeVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public SectorNodeState Node { get; private set; }

        public void Bind(SectorNodeState node, Color color, float scale = 0.5f)
        {
            Node = node;
            if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.color = color;
            transform.localScale = Vector3.one * scale;
            name = $"Node_{node.Id}_{node.Type}";
        }

        public void SetVisited(bool visited)
        {
            if (!spriteRenderer) return;
            // Simple visual cue: dim visited nodes slightly
            spriteRenderer.color = visited
                ? new Color(
                    spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b,
                    0.7f)
                : new Color(
                    spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b,
                    1f);
        }

        public void SetSelected(bool selected)
        {
            // Simple scale pop to mark CurrentNode
            transform.localScale = selected ? Vector3.one * 0.65f : Vector3.one * 0.5f;
        }
    }
}
