using ALWTTT.Enums;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(
        fileName = "New NodeTypeData",
        menuName = "ALWTTT/Map/Node Type Data",
        order = 1100)]
    public class NodeTypeData : ScriptableObject
    {
        [SerializeField] private NodeType type;

        [Header("Tooltip / UI")]
        [SerializeField] private string title;
        [TextArea]
        [SerializeField] private string description;

        [Header("Visuals")]
        [SerializeField] private Color color = Color.white;
        [SerializeField] private Sprite sprite;

        public NodeType Type => type;
        public string Title => string.IsNullOrWhiteSpace(title) ? type.ToString() : title;
        public string Description => description ?? string.Empty;
        public Color Color => color;
        public Sprite Sprite => sprite;
    }
}
