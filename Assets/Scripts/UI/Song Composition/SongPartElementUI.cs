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

        private readonly Dictionary<string, SongTrackElementUI> trackByMusician = new();
        private List<string> rosterOrder = new();

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
            trackByMusician.Clear();

            // Build one row per musician in roster order (placeholder if no track)
            if (rosterOrder != null && rosterOrder.Count > 0)
            {
                foreach (var id in rosterOrder)
                {
                    var t = model.tracks?.Find(x => x.musicianId == id);
                    var ui = Instantiate(trackPrefab, tracksRoot);
                    ui.gameObject.SetActive(true);
                    trackByMusician[id] = ui;

                    bool isPending = pendingMusicianIds != null
                        && pendingMusicianIds.Contains(id);

                    if (t != null)
                        ui.Bind(t.role.ToString(), t.info, placeholder: false,
                            inspirationNext: t.inspirationGenerated, pending: isPending);
                    else
                        ui.Bind("—", "", placeholder: true);
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
                        trackByMusician[t.musicianId] = ui;

                        bool isPending = pendingMusicianIds != null
                            && pendingMusicianIds.Contains(t.musicianId);

                        ui.Bind(t.role.ToString(), t.info, placeholder: false,
                            inspirationNext: t.inspirationGenerated, pending: isPending);
                    }
            }
        }

        public void AddOrUpdateTrack(
            string musicianId, string role, string info,
            int inspirationNext = 0, bool pending = false)
        {
            Log($"<color=red>Add/Update Track {musicianId} {role} {info}</color>");

            // If the row exists (placeholder or not) → update it in place
            if (!trackByMusician.TryGetValue(musicianId, out var trackUI))
            {
                trackUI = Instantiate(trackPrefab, tracksRoot);
                trackUI.gameObject.SetActive(true);
                trackByMusician[musicianId] = trackUI;

                if (rosterOrder != null && rosterOrder.Count > 0)
                {
                    int idx = rosterOrder.IndexOf(musicianId);
                    if (idx >= 0)
                        trackUI.transform.SetSiblingIndex(
                            Mathf.Min(idx, tracksRoot.childCount - 1));
                }
            }

            trackUI.Bind(role, info, placeholder: false,
                inspirationNext: inspirationNext, pending: pending);
        }
    }
}