// Feeds every exported text through the real importer and parser, as the editor does:
// DrumPatternEditorImporter.Parse -> grid config -> DrumPatternTextParser.ApplyTextEdits over empty lanes.
// Also renders each asset with the real Render and compares it with what build.py exported.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MidiGenPlay;
using MidiGenPlay.Authoring;

var path = args.Length > 0 ? args[0] : "../patrones.json";
var root = JsonDocument.Parse(File.ReadAllText(path)).RootElement;
int ok = 0, lossy = 0, bad = 0, importWarn = 0, parseWarn = 0, renderDiff = 0;

foreach (var p in root.GetProperty("patterns").EnumerateArray())
{
    string id = p.GetProperty("id").GetString(), text = p.GetProperty("export").GetString();
    var errors = new List<string>();
    int measures = p.GetProperty("measures").GetInt32(), subs = p.GetProperty("subdivisions").GetInt32();
    int beats = p.GetProperty("beats").GetInt32(), total = p.GetProperty("totalSteps").GetInt32();
    var lanes = p.GetProperty("lanes").EnumerateArray().ToList();

    // Windows clipboards may bring CRLF: both line endings must import the same.
    var res = DrumPatternEditorImporter.Parse(text.Replace("\n", "\r\n"), LaneAliasDictionary.TryResolve);
    var resLf = DrumPatternEditorImporter.Parse(text, LaneAliasDictionary.TryResolve);
    importWarn += res.warnings.Count + resLf.warnings.Count;
    foreach (var w in res.warnings) errors.Add(w.ToString());
    if (res.mode != DrumPatternEditorImporter.ImportMode.Full || resLf.mode != res.mode) errors.Add("mode " + res.mode);
    else
    {
        if (res.timeSignature.ToString() != p.GetProperty("ts").GetString()) errors.Add("ts " + res.timeSignature);
        if (res.measures != measures || res.subdivisions != subs) errors.Add("grid");
        if (res.lanes.Count != lanes.Count || res.dslLines.Count != lanes.Count) errors.Add("lane count");
    }

    int changed = 0, snapWarnings = 0;
    for (int i = 0; i < lanes.Count && errors.Count == 0; i++)
    {
        var lane = lanes[i];
        int gm = lane.GetProperty("gm").GetInt32(), def = lane.GetProperty("def").GetInt32();
        var raw = lane.GetProperty("steps").EnumerateArray().Select(e => e.GetInt32()).ToList();
        if ((int)res.lanes[i].instrument != gm)
            errors.Add($"lane {i}: {res.lanes[i].instrument}={(int)res.lanes[i].instrument} vs asset value {gm}");
        if (res.lanes[i].defaultVelocity != def) errors.Add($"lane {i}: default velocity");

        var warnings = new List<DrumPatternTextWarning>();
        var empty = Enumerable.Repeat(DrumPatternData.StepState.Off, total).ToList();
        var parsed = DrumPatternTextParser.ApplyTextEdits(empty, res.dslLines[i], total, def, i, warnings);
        parseWarn += warnings.Count;
        foreach (var w in warnings) errors.Add(w.ToString());

        var asset = raw.Select(v => v < 0 ? DrumPatternData.StepState.Off : DrumPatternData.StepState.On(v)).ToList();
        var renderWarnings = new List<DrumPatternTextWarning>();
        string rendered = DrumPatternTextParser.Render(asset, def, i, renderWarnings, beats * subs);
        snapWarnings += renderWarnings.Count;
        if (rendered != res.dslLines[i]) { renderDiff++; errors.Add($"lane {i}: render differs"); }

        for (int s = 0; s < total; s++)
        {
            if (asset[s].active != parsed[s].active) errors.Add($"lane {i} step {s}: active");
            else if (asset[s].active && asset[s].ResolveVelocity(def) != parsed[s].ResolveVelocity(def)) changed++;
        }
    }
    int declared = p.GetProperty("snapped").GetInt32();
    if (errors.Count == 0 && declared != snapWarnings) errors.Add($"declared {declared} snapped, real {snapWarnings}");
    if (errors.Count == 0 && declared != changed) errors.Add($"declared {declared} snapped, {changed} velocities changed");

    if (errors.Count > 0) { bad++; Console.WriteLine($"FAIL {id}: {string.Join("; ", errors.Take(4))}"); }
    else if (changed > 0) { lossy++; Console.WriteLine($"LOSS {id}: {changed} hits change velocity (declared)"); }
    else ok++;
}
Console.WriteLine($"identical {ok}, declared loss {lossy}, failed {bad}; importer warnings {importWarn}, parser warnings {parseWarn}, render differences {renderDiff}");
return bad == 0 ? 0 : 1;
