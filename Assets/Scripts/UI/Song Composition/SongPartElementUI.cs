using ALWTTT.Cards;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ALWTTT.UI
{
    public class SongPartElementUI : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI partLabelText;
        [SerializeField] private TextMeshProUGUI partInfoText;

        [Header("Tracks")]
        [SerializeField] private Transform tracksRoot;
        [SerializeField] private SongTrackElementUI trackPrefab;

        [Header("Dev")]
        [SerializeField] private bool useLogs = false;

        // [BASS-1 / D1=A] Row registry keyed by "musicianId|role" so a
        // musician can host multiple role-tracks (e.g. Melody + Bassline),
        // each as an independent row. Placeholder rows (roster musician with
        // no track yet) are keyed by musicianId alone and are upgraded in
        // place by the first AddOrUpdateTrack for that musician.
        private readonly Dictionary<string, SongTrackElementUI> trackByMusicianRole = new();
        private List<string> rosterOrder = new();

        private static string RowKey(string musicianId, string role)
            => $"{musicianId}|{role}";

        private SongCompositionUI.PartEntry boundModel;
        // [B1 / #1+#2] Cached pending-set from the latest Bind, so SetRosterOrder
        // re-binds with the same pending state.
        private HashSet<string> _boundPendingSet;

        private void Log(string log)
        {
            if (useLogs) Debug.Log($"{log}");
        }

        public void SetRosterOrder(List<string> order)
        {
            rosterOrder = order != null ? new List<string>(order) : new List<string>();
            if (boundModel != null) Bind(boundModel, _boundPendingSet);
        }

        public void Bind(
            SongCompositionUI.PartEntry model,
            HashSet<string> pendingMusicianIds = null)
        {
            Log($"<color=red>Binding SongPartElement {model.label}</color>");

            boundModel = model;
            _boundPendingSet = pendingMusicianIds;

            if (partLabelText) partLabelText.text = model.label;
            if (partInfoText) partInfoText.text =
                    $"{model.timeSignature} - {model.tempo} - {model.tonality}";

            // Rebuild tracks
            foreach (Transform c in tracksRoot) Destroy(c.gameObject);
            trackByMusicianRole.Clear();

            // [BASS-1 / D1=A] One row per (musicianId, role) in roster order;
            // a roster musician with no tracks gets one placeholder row.
            // Pending granularity stays per-musician (R6, documented): all of
            // a musician's rows show pending together — conservative-correct,
            // since a re-render refreshes the whole part.
            if (rosterOrder != null && rosterOrder.Count > 0)
            {
                foreach (var id in rosterOrder)
                {
                    bool isPending = pendingMusicianIds != null
                        && pendingMusicianIds.Contains(id);

                    var owned = model.tracks?.FindAll(x => x.musicianId == id);
                    if (owned == null || owned.Count == 0)
                    {
                        var ui = Instantiate(trackPrefab, tracksRoot);
                        ui.gameObject.SetActive(true);
                        trackByMusicianRole[id] = ui; // placeholder key
                        ui.Bind("—", "", placeholder: true);
                        continue;
                    }

                    foreach (var t in owned)
                    {
                        var ui = Instantiate(trackPrefab, tracksRoot);
                        ui.gameObject.SetActive(true);
                        trackByMusicianRole[RowKey(id, t.role.ToString())] = ui;

                        ui.Bind(
                            t.role.ToString(), t.info, placeholder: false,
                            // [DF-INSPLOOP] complexity + card per-loop bonus
                            inspirationNext: t.inspirationGenerated +
                                ALWTTT.Cards.Effects.AddInspirationPerLoopSpec.SumFor(
                                    t.sourceCardDefinition),
                            pending: isPending,
                            sourceCard: t.sourceCardDefinition); // [B2 / #3]
                    }
                }
            }
            else
            {
                // Fallback: no roster known → just add existing tracks
                if (model.tracks != null)
                    foreach (var t in model.tracks)
                    {
                        var ui = Instantiate(trackPrefab, tracksRoot);
                        ui.gameObject.SetActive(true);
                        trackByMusicianRole[
                            RowKey(t.musicianId, t.role.ToString())] = ui;

                        bool isPending = pendingMusicianIds != null
                            && pendingMusicianIds.Contains(t.musicianId);

                        ui.Bind(
                            t.role.ToString(), t.info, placeholder: false,
                            // [DF-INSPLOOP] complexity + card per-loop bonus
                            inspirationNext: t.inspirationGenerated +
                                ALWTTT.Cards.Effects.AddInspirationPerLoopSpec.SumFor(
                                    t.sourceCardDefinition),
                            pending: isPending,
                            sourceCard: t.sourceCardDefinition); // [B2 / #3]
                    }
            }
        }

        public void AddOrUpdateTrack(
            string musicianId, string role, string info,
            int inspirationNext = 0, bool pending = false,
            CardDefinition sourceCard = null) // [B2 / #3]
        {
            Log($"<color=red>Add/Update Track {musicianId} {role} {info}</color>");

            var key = RowKey(musicianId, role);

            // 1) Exact (musicianId, role) row exists → update it in place.
            if (!trackByMusicianRole.TryGetValue(key, out var trackUI))
            {
                // 2) [BASS-1] Placeholder row for this musician exists →
                //    upgrade it in place to this role's row.
                if (trackByMusicianRole.TryGetValue(musicianId, out trackUI))
                {
                    trackByMusicianRole.Remove(musicianId);
                    trackByMusicianRole[key] = trackUI;
                }
                // 3) New row. Place it after the musician's last existing row
                //    when they already have one; otherwise at roster position.
                else
                {
                    trackUI = Instantiate(trackPrefab, tracksRoot);
                    trackUI.gameObject.SetActive(true);
                    trackByMusicianRole[key] = trackUI;

                    int siblingIdx = -1;
                    var prefix = musicianId + "|";
                    foreach (var kv in trackByMusicianRole)
                    {
                        if (kv.Value == trackUI) continue;
                        if (kv.Key == musicianId || kv.Key.StartsWith(prefix))
                            siblingIdx = Mathf.Max(siblingIdx,
                                kv.Value.transform.GetSiblingIndex());
                    }

                    if (siblingIdx >= 0)
                    {
                        trackUI.transform.SetSiblingIndex(
                            Mathf.Min(siblingIdx + 1, tracksRoot.childCount - 1));
                    }
                    else if (rosterOrder != null && rosterOrder.Count > 0)
                    {
                        int idx = rosterOrder.IndexOf(musicianId);
                        if (idx >= 0)
                            trackUI.transform.SetSiblingIndex(
                                Mathf.Min(idx, tracksRoot.childCount - 1));
                    }
                }
            }

            trackUI.Bind(
                role, info, placeholder: false,
                inspirationNext: inspirationNext, pending: pending,
                sourceCard: sourceCard); // [B2 / #3]
        }
    }
}