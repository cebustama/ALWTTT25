using ALWTTT.Status;
using System.Text;
using UnityEngine;

namespace ALWTTT.Characters.Band
{
    public class MusicianCharacterSimple : MusicianBase
    {
#if UNITY_EDITOR
        [Header("Debug — Status Snapshot (Editor Only)")]
        [SerializeField] // shown via custom display below
        private string _debugStatusSnapshot = "—";

        protected override void Update()
        {
            base.Update();
            _debugStatusSnapshot = BuildStatusSnapshot();
        }

        private string BuildStatusSnapshot()
        {
            if (Statuses == null || Statuses.Active.Count == 0)
                return "none";

            var sb = new StringBuilder();
            foreach (var kvp in Statuses.Active)
            {
                if (sb.Length > 0) sb.Append(" | ");
                sb.Append($"{kvp.Key}={kvp.Value.Stacks}");
            }
            return sb.ToString();
        }
#endif
    }
}