using UnityEngine;

namespace ALWTTT.Map
{
    /// <summary>
    /// Rotates the ship root around Z so a child sprite with an offset 
    /// looks like it's orbiting the node.
    /// </summary>
    public class ShipController : MonoBehaviour
    {
        [Header("Orbit")]
        [SerializeField] private float orbitSpeedDegPerSec = 45f;
        [SerializeField] private bool clockwise = true;
        [SerializeField] private bool autoRotate = true;

        /// <summary>Attach ship root to a new parent (the node visual).</summary>
        public void AttachTo(Transform newParent)
        {
            if (!newParent) return;
            transform.SetParent(newParent, worldPositionStays: false);
            transform.localPosition = Vector3.zero;
            // Keep local rotation zero so orbit starts clean each time
            transform.localRotation = Quaternion.identity;
        }

        public void SetOrbiting(bool value) => autoRotate = value;

        private void Update()
        {
            if (!autoRotate) return;
            float dir = clockwise ? -1f : 1f; // clockwise means negative Z in Unity 2D
            transform.Rotate(0f, 0f, dir * orbitSpeedDegPerSec * Time.deltaTime, Space.Self);
        }
    }
}