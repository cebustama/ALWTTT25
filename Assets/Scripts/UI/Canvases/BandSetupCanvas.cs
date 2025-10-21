// BandSetupCanvas.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ALWTTT.Data;
using ALWTTT.UI; // for MusicianUI
using ALWTTT;
using TMPro;    // for CardData/CardBase

public class BandSetupCanvas : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private LayoutGroup characterSpawnRoot;
    [SerializeField] private MusicianUI musicianUIPrefab;

    [Header("Cards (preview of clicked/focused musician)")]
    [SerializeField] private LayoutGroup cardSpawnRoot;
    [SerializeField] private CardBase cardUIPrefab;

    [Header("Controls")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI pickCounterText; // "0 / N"
    [SerializeField] private TextMeshProUGUI titleText;       // "Pick your band" etc.

    private readonly List<MusicianUI> _spawned = new();
    private readonly HashSet<string> _selectedIds = new();
    private int _pickCountTarget;
    private Action<List<MusicianCharacterData>> _onConfirm;

    // cache: id -> data
    private Dictionary<string, MusicianCharacterData> _id2data;

    public void Show(
        List<MusicianCharacterData> candidates,
        int pickCountTarget,
        Action<List<MusicianCharacterData>> onConfirm)
    {
        gameObject.SetActive(true);
        _onConfirm = onConfirm;
        _pickCountTarget = Mathf.Max(1, pickCountTarget);
        _id2data = candidates.ToDictionary(c => c.CharacterId, c => c);

        BuildMusicianTiles(candidates);
        ClearCards();
        _selectedIds.Clear();
        UpdatePickCounter();
        confirmButton.interactable = false;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(ConfirmSelection);

        titleText.text = $"Pick {pickCountTarget} from {candidates.Count}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ClearCards();
        _onConfirm = null;
    }

    private void BuildMusicianTiles(List<MusicianCharacterData> list)
    {
        foreach (Transform c in characterSpawnRoot.transform) Destroy(c.gameObject);
        _spawned.Clear();

        foreach (var data in list)
        {
            var ui = Instantiate(musicianUIPrefab, characterSpawnRoot.transform);
            ui.Build(data, OnTileClicked);
            ui.SetHighlight(false);
            _spawned.Add(ui);
        }
    }

    private void OnTileClicked(MusicianUI ui)
    {
        if (ui == null || ui.Data == null) return;
        var id = ui.Data.CharacterId;

        // Toggle selection respecting the target cap
        if (_selectedIds.Contains(id))
        {
            _selectedIds.Remove(id);
            ui.SetHighlight(false);
        }
        else
        {
            if (_selectedIds.Count >= _pickCountTarget)
                return; // ignore extra clicks beyond cap
            _selectedIds.Add(id);
            ui.SetHighlight(true);
        }

        // Always show clicked musician’s base cards as a preview
        BuildCardPreview(ui.Data);
        UpdatePickCounter();
        confirmButton.interactable = (_selectedIds.Count == _pickCountTarget);
    }

    private void UpdatePickCounter()
    {
        if (pickCounterText)
            pickCounterText.text = $"{_selectedIds.Count} / {_pickCountTarget}";
    }

    private void BuildCardPreview(MusicianCharacterData musician)
    {
        ClearCards();
        foreach (var cd in musician.BaseCards)
        {
            var cardUI = Instantiate(cardUIPrefab, cardSpawnRoot.transform);
            cardUI.SetCard(cd, false);
        }
    }

    private void ClearCards()
    {
        foreach (Transform c in cardSpawnRoot.transform) Destroy(c.gameObject);
    }

    private void ConfirmSelection()
    {
        if (_selectedIds.Count != _pickCountTarget) return;

        var result = new List<MusicianCharacterData>(_pickCountTarget);
        foreach (var id in _selectedIds)
            if (_id2data.TryGetValue(id, out var d)) result.Add(d);

        _onConfirm?.Invoke(result);
        //Hide();
    }
}
