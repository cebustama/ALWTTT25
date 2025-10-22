using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ALWTTT.UI
{
    public class SongPartElementUI : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI partLabelText;
        [SerializeField] private TextMeshProUGUI timeSigAndTempoText;

        [Header("Tracks")]
        [SerializeField] private Transform tracksRoot;  // layout group
        [SerializeField] private SongTrackElementUI trackPrefab;

        [Header("Dev")]
        [SerializeField] private bool useLogs = false;

        private readonly Dictionary<string, SongTrackElementUI> trackByMusician = new();
        private List<string> rosterOrder = new(); // musicianId visual order

        private SongCompositionUI.PartEntry boundModel;

        private void Log(string log)
        {
            if (useLogs) Debug.Log($"{log}");
        }

        public void SetRosterOrder(List<string> order)
        {
            rosterOrder = order != null ? new List<string>(order) : new List<string>();
            if (boundModel != null) Bind(boundModel);
        }

        public void Bind(SongCompositionUI.PartEntry model)
        {
            Log($"<color=red>Binding SongPartElement {model.label}</color>");

            boundModel = model;

            if (partLabelText) partLabelText.text = model.label;
            if (timeSigAndTempoText) timeSigAndTempoText.text = 
                    $"{model.timeSignature}   {model.tempo}";

            // Rebuild tracks
            foreach (Transform c in tracksRoot) Destroy(c.gameObject);
            trackByMusician.Clear();

            if (model.tracks == null) return;

            // Rebuild respecting roster order if available
            if (rosterOrder != null && rosterOrder.Count > 0)
            {
                // Add any tracks we have, ordered by roster
                foreach (var id in rosterOrder)
                {
                    var t = model.tracks.Find(x => x.musicianId == id);
                    if (t != null) AddOrUpdateTrack(t.musicianId, t.role, t.info);
                }
                // Add any remaining tracks (edge cases)
                foreach (var t in model.tracks)
                    if (!trackByMusician.ContainsKey(t.musicianId))
                        AddOrUpdateTrack(t.musicianId, t.role, t.info);
            }
            else
            {
                foreach (var t in model.tracks)
                    AddOrUpdateTrack(t.musicianId, t.role, t.info);
            }
        }

        public void AddOrUpdateTrack(string musicianId, string role, string info)
        {
            Log($"<color=red>Add/Update Track {musicianId} {role} {info}</color>");

            if (tracksRoot == null || trackPrefab == null) return;
            if (string.IsNullOrEmpty(musicianId)) return;

            if (!trackByMusician.TryGetValue(musicianId, out var trackUI))
            {
                trackUI = Instantiate(trackPrefab, tracksRoot);
                trackUI.gameObject.SetActive(true);
                trackByMusician[musicianId] = trackUI;
            }

            trackUI.Bind(role, info);

            // Place under the correct row index (to match the icon column)
            if (rosterOrder != null && rosterOrder.Count > 0)
            {
                int idx = rosterOrder.IndexOf(musicianId);
                if (idx >= 0 && idx < tracksRoot.childCount)
                    trackUI.transform.SetSiblingIndex(idx);
            }
        }
    }
}