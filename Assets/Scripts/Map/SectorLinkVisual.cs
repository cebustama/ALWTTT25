using UnityEngine;

namespace ALWTTT.Map
{
    /// <summary>
    /// Visual for a link between two nodes. Uses a LineRenderer.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class SectorLinkVisual : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float baseWidth = 0.035f;
        [SerializeField] private float emphasisWidth = 0.06f;

        public Transform A { get; private set; }
        public Transform B { get; private set; }

        public void Bind(Transform a, Transform b, float z = 0f)
        {
            if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();
            A = a; B = b;

            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.enabled = true;
            lineRenderer.startWidth = baseWidth;
            lineRenderer.endWidth = baseWidth;

            UpdatePositions(z);
        }

        public void UpdatePositions(float z = 0f)
        {
            if (!A || !B || !lineRenderer) return;
            lineRenderer.SetPosition(0, new Vector3(A.position.x, A.position.y, z));
            lineRenderer.SetPosition(1, new Vector3(B.position.x, B.position.y, z));
        }

        public void SetVisible(bool visible)
        {
            if (!lineRenderer) return;
            lineRenderer.enabled = visible;
        }

        public void SetEmphasis(bool on)
        {
            if (!lineRenderer) return;
            float w = on ? emphasisWidth : baseWidth;
            lineRenderer.startWidth = w;
            lineRenderer.endWidth = w;
        }
    }
}
