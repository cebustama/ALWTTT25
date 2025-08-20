using ALWTTT.Characters.Audience;
using ALWTTT.Enums;
using System.Collections;
using UnityEngine;

namespace ALWTTT.Actions
{
    // TODO: Use AudienceActionBase instead for all audience-specific actions
    public class AudienceMoveToFrontAction : CharacterActionBase
    {
        public override CharacterActionType ActionType => CharacterActionType.MoveToFront;

        public override string ActionName => "Move To Front";

        public override void DoAction(CharacterActionParameters actionParameters)
        {
            var performer = actionParameters.PerformerCharacter as AudienceCharacterBase;
            if (performer == null)
            {
                Debug.LogWarning($"[{ActionName}] Performer is not an AudienceCharacterBase.");
                return;
            }

            var positions = GigManager.AudienceMemberPosList;
            var audience = GigManager.CurrentAudienceCharacterList;

            if (positions == null || positions.Count == 0 || audience == null || audience.Count == 0)
            {
                Debug.LogWarning($"[{ActionName}] Missing audience positions or list.");
                return;
            }

            var fromIndex = Mathf.Clamp(performer.ColumnIndex, 0, positions.Count - 1);
            if (fromIndex <= 0)
            {
                // Ensure we visually snap/slide into place anyway
                ReparentAndLerpToZero(performer.transform, positions[0]);
                GigManager.RecalculateAudienceObstructions();
                return;
            }

            // Shift everyone in front of the performer back by one
            // Example: [0][1][2][3] with performer at 3 -> 0→1, 1→2, 2→3, performer→0
            for (int i = fromIndex - 1; i >= 0; i--)
            {
                var member = audience[i];
                var newIndex = Mathf.Min(i + 1, positions.Count - 1);

                member.ColumnIndex = newIndex;
                member.transform.SetParent(positions[newIndex], true);
            }

            // Move performer to the very front (index 0)
            performer.ColumnIndex = 0;
            performer.transform.SetParent(positions[0], true);

            // Update logical order to match columns
            audience.Remove(performer);
            audience.Insert(0, performer);

            // Smoothly slide everyone into their slot root (local zero)
            for (int i = 0; i < audience.Count && i < positions.Count; i++)
            {
                var member = audience[i];

                if (member.transform.parent != positions[i])
                    member.transform.SetParent(positions[i], true);

                ReparentAndLerpToZero(member.transform, positions[i]);
            }

            GigManager.RecalculateAudienceObstructions();
        }

        private void ReparentAndLerpToZero(Transform t, Transform parent)
        {
            // Parent is already set with worldPositionStays = true; now animate localPosition → zero.
            var host = t.GetComponent<AudienceCharacterBase>();
            if (host != null)
            {
                host.StartCoroutine(LerpLocalPositionToZero(t));
            }
        }

        private IEnumerator LerpLocalPositionToZero(Transform t)
        {
            var start = t.localPosition;
            const float duration = 0.25f;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t01 = Mathf.Clamp01(timer / duration);
                t.localPosition = Vector3.Lerp(start, Vector3.zero, t01);
                yield return null;
            }

            t.localPosition = Vector3.zero;
        }
    }
}
