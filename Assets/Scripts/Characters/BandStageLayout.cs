using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    /// <summary>
    /// [S7 / D5=A] Where each musician stands, by band size.
    ///
    /// Hand-made formations for 2, 3 and 4 musicians live in the gig scene as
    /// child anchors (one list per size). Slot i receives the i-th musician of
    /// the roster (dormitory card order = stage order). When there is no
    /// formation for the size, or it has fewer slots than musicians, the band is
    /// spread evenly between <see cref="autoLeft"/> and <see cref="autoRight"/>
    /// (runtime anchors created under this object).
    ///
    /// Depth: the slot closest to the audience (lowest y) draws on top
    /// (sorting order 10), the furthest one at 0, the rest in between.
    ///
    /// Player-chosen placement is post-demo (D5 C).
    /// </summary>
    public sealed class BandStageLayout : MonoBehaviour
    {
        [Serializable]
        public sealed class Formation
        {
            [Min(1)] public int musicianCount = 2;
            [Tooltip("One anchor per musician, in roster order.")]
            public List<Transform> slots = new List<Transform>();
        }

        [SerializeField, Tooltip("Hand-made formations, one per band size (2, 3, 4).")]
        private List<Formation> formations = new List<Formation>();

        [Header("Automatic fallback")]
        [SerializeField, Tooltip("Left end of the line used when no formation fits.")]
        private Transform autoLeft;
        [SerializeField, Tooltip("Right end of the line used when no formation fits.")]
        private Transform autoRight;

        [SerializeField] private int frontSortingOrder = 10;
        [SerializeField] private int backSortingOrder = 0;

        private readonly List<Transform> _autoSlots = new List<Transform>();

        /// <summary>True when a hand-made formation covers <paramref name="count"/> musicians.</summary>
        public bool HasFormation(int count) => FindFormation(count) != null;

        /// <summary>
        /// Anchors for a band of <paramref name="count"/> musicians, in roster order.
        /// Never returns fewer than <paramref name="count"/> entries when an automatic
        /// line is configured; returns an empty list when neither source is usable.
        /// </summary>
        public List<Transform> GetSlots(int count)
        {
            var f = FindFormation(count);
            if (f != null) return new List<Transform>(f.slots.GetRange(0, count));
            return BuildAutoSlots(count);
        }

        /// <summary>Sorting order for each slot: lowest y (closest to the audience) on top.</summary>
        /// <summary>
        /// [S7 / Lineup] Slot positions WITHOUT creating auto-slot objects — for editor previews,
        /// which must not add children to the scene. Same order and spacing as GetSlots.
        /// Empty when neither a formation nor both anchors exist.
        /// </summary>
        public List<Vector3> GetSlotPositions(int count)
        {
            var res = new List<Vector3>(Mathf.Max(0, count));
            var f = FindFormation(count);
            if (f != null)
            {
                for (int i = 0; i < count; i++) res.Add(f.slots[i].position);
                return res;
            }
            if (autoLeft == null || autoRight == null) return res;
            for (int i = 0; i < count; i++)
                res.Add(Vector3.Lerp(autoLeft.position, autoRight.position, (i + 0.5f) / count));
            return res;
        }

        public int[] GetSortingOrders(IList<Transform> slots)
        {
            int n = slots.Count;
            var orders = new int[n];
            if (n == 0) return orders;

            var idx = new List<int>(n);
            for (int i = 0; i < n; i++) idx.Add(i);
            idx.Sort((a, b) => slots[a].position.y.CompareTo(slots[b].position.y));

            for (int rank = 0; rank < n; rank++)
            {
                float t = n == 1 ? 0f : rank / (float)(n - 1);
                orders[idx[rank]] = Mathf.RoundToInt(Mathf.Lerp(frontSortingOrder, backSortingOrder, t));
            }
            return orders;
        }

        private Formation FindFormation(int count)
        {
            foreach (var f in formations)
            {
                if (f == null || f.musicianCount != count || f.slots == null) continue;
                if (f.slots.Count < count) continue;
                bool complete = true;
                for (int i = 0; i < count; i++)
                    if (f.slots[i] == null) { complete = false; break; }
                if (complete) return f;
            }
            return null;
        }

        private List<Transform> BuildAutoSlots(int count)
        {
            var result = new List<Transform>(count);
            if (autoLeft == null || autoRight == null || count <= 0) return result;

            while (_autoSlots.Count < count)
            {
                var go = new GameObject($"Auto Slot {_autoSlots.Count + 1}");
                go.transform.SetParent(transform, false);
                _autoSlots.Add(go.transform);
            }

            // Evenly spaced, with half a gap at each end so 2 musicians do not
            // stand on the very edges of the line.
            for (int i = 0; i < count; i++)
            {
                float t = (i + 0.5f) / count;
                _autoSlots[i].position = Vector3.Lerp(autoLeft.position, autoRight.position, t);
                result.Add(_autoSlots[i]);
            }
            return result;
        }
    }
}
