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

            // [B3-content-audience pass1] Parameterized step count from
            // CharacterActionData.ActionValue. 0 / unset → default 1 step per call.
            // Replaces the prior "jump to front" semantics. Each call slides the
            // performer forward by N positions; displaced members shift back by 1 each.
            int requestedSteps = Mathf.RoundToInt(actionParameters.Value);
            int stepsPerTurn = requestedSteps <= 0 ? 1 : requestedSteps;

            int fromIndex = Mathf.Clamp(performer.ColumnIndex, 0, positions.Count - 1);
            int toIndex = Mathf.Max(0, fromIndex - stepsPerTurn);

            if (toIndex >= fromIndex)
            {
                // Already at (or beyond) the target slot — no-op except for a
                // defensive snap in case of stray transform drift.
                ReparentAndLerpToZero(performer.transform, positions[fromIndex]);
                GigManager.RecalculateAudienceObstructions();
                return;
            }

            // Members occupying slots [toIndex .. fromIndex - 1] each shift BACK
            // by one slot. Performer takes toIndex.
            for (int i = fromIndex - 1; i >= toIndex; i--)
            {
                var displaced = audience[i];
                int newIndex = i + 1;

                displaced.ColumnIndex = newIndex;
                displaced.transform.SetParent(positions[newIndex], true);
            }

            performer.ColumnIndex = toIndex;
            performer.transform.SetParent(positions[toIndex], true);

            // Reorder logical list to match new column layout.
            audience.RemoveAt(fromIndex);
            audience.Insert(toIndex, performer);

            // Smooth-slide only the affected range into their slot root (local zero).
            int lerpFrom = toIndex;
            int lerpTo = Mathf.Min(fromIndex, audience.Count - 1);
            for (int i = lerpFrom; i <= lerpTo; i++)
            {
                var member = audience[i];
                if (member == null) continue;

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