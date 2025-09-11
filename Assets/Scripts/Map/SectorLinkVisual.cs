using ALWTTT.Data;
using UnityEngine;

namespace ALWTTT.Map
{
    /// <summary>
    /// Visual for a link between two nodes. Uses a LineRenderer.
    /// </summary>
    public class SectorLinkVisual : MonoBehaviour
    {
        [SerializeField] private LineRenderer lineRenderer;

        public Transform A { get; private set; }
        public Transform B { get; private set; }

        public void Bind(Transform a, Transform b, float z = 0f)
        {
            if (!lineRenderer) lineRenderer = GetComponent<LineRenderer>();
            A = a; B = b;

            // Basic setup
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;

            // Initial update
            UpdatePositions(z);
        }

        public void UpdatePositions(float z = 0f)
        {
            if (!A || !B || !lineRenderer) return;
            lineRenderer.SetPosition(0, new Vector3(A.position.x, B.position.y + 0f, z)); // we’ll override next line anyway
            lineRenderer.SetPosition(0, new Vector3(A.position.x, A.position.y, z));
            lineRenderer.SetPosition(1, new Vector3(B.position.x, B.position.y, z));
        }
    }
}