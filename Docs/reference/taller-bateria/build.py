"""Builds the Taller de batería from the real Unity assets (read only).

Reads DrumPatternData and DrumPatternPaletteSO assets (YAML), writes patrones.json
(raw steps and the text for the editor's Import) and, if template.html exists, index.html.
Run: python -I -X utf8 build.py [--web]
"""
import json
import re
import sys
from pathlib import Path

d = Path(__file__).parent
ASSETS = Path("D:/Projects/ALWTTT25/Assets/Resources/ScriptableObjects")
PATTERNS = ASSETS / "Patterns" / "Drums"
PALETTES = ASSETS / "Drums" / "Palettes"
WEB = Path("D:/Projects/web/claudiobustamante-site/public/privado/taller-bateria")

# MusicTheory.TimeSignature, in enum order: (name, beats per measure, beat unit)
TS = [("FourFour", 4, 4), ("ThreeFour", 3, 4), ("TwoFour", 2, 4), ("SixEight", 6, 8),
      ("NineEight", 9, 8), ("TwelveEight", 12, 8), ("FiveFour", 5, 4), ("SevenEight", 7, 8)]
# GeneralMidiPercussion names for the notes in use; the verifier checks them against the DLL.
GM = {35: "AcousticBassDrum", 36: "BassDrum1", 37: "SideStick", 38: "AcousticSnare", 41: "LowFloorTom",
      42: "ClosedHiHat", 43: "HighFloorTom", 44: "PedalHiHat", 46: "OpenHiHat", 47: "LowMidTom",
      48: "HiMidTom", 49: "CrashCymbal1", 51: "RideCymbal1", 56: "Cowbell", 63: "OpenHiConga", 75: "Claves",
      # the rest of the page kit (template GM_NAME), so new assets do not break the build
      39: "HandClap", 40: "ElectricSnare", 45: "LowTom", 50: "HighTom", 53: "RideBell", 64: "LowConga"}
ACCENT, GHOST = 120, 50  # DrumPatternTextParser.AccentVelocity / GhostVelocity


def field(text, key):
    m = re.search(rf"^  {key}: ?(.*)$", text, re.M)
    return m.group(1).strip() if m else ""


def unquote(s):
    return json.loads(s) if s.startswith('"') else s


def glyph(raw, default):
    """Mirrors DrumPatternTextParser.StepToGlyph; returns (glyph, snapped)."""
    if raw < 0:
        return ".", False
    if raw == 0 or raw == default:
        return "x", False
    if raw == ACCENT:
        return "X", False
    if raw == GHOST:
        return "o", False
    dd, da, dg = abs(raw - default), abs(raw - ACCENT), abs(raw - GHOST)
    if dd <= da and dd <= dg:
        return "x", True
    if dg < dd and dg <= da:
        return "o", True
    return "X", True


def render(steps, default, per_measure):
    out, snapped = [], 0
    for i, raw in enumerate(steps):
        if i and i % per_measure == 0:
            out.append("|")
        g, s = glyph(raw, default)
        out.append(g)
        snapped += s
    return "".join(out), snapped


def export_text(p):
    """The "setup card + DSL block" shape that DrumPatternEditorImporter reads."""
    per = p["beats"] * p["subdivisions"]
    rows = [render(l["steps"], l["def"], per) for l in p["lanes"]]
    card = [f"**{p['id']}**", "", "**Setup (configure in Grid mode):**", "",
            f"- Time signature: {p['ts']}", f"- Measures: {p['measures']}",
            f"- Subdivisions: {p['subdivisions']}", "- Lanes (in this order):"]
    card += [f"  {i}. {GM[l['gm']]} (GM {l['gm']}) — default velocity {l['def']}" for i, l in enumerate(p["lanes"], 1)]
    return "\n".join(card + ["", "```"] + [r[0] for r in rows] + ["```", ""]), sum(r[1] for r in rows)


def read_pattern(path):
    t = path.read_text(encoding="utf-8", errors="replace")
    guid = re.search(r"guid: (\w+)", path.with_name(path.name + ".meta").read_text(encoding="utf-8")).group(1)
    ts_name, beats, unit = TS[int(field(t, "TimeSignature"))]
    lanes = []
    for inst, default, body in re.findall(
            r"- instrument: (\d+)\n\s+defaultVelocity: (\d+)\n\s+steps:(.*?)(?=\n  - instrument|\n  noteLength|\Z)", t, re.S):
        steps = [int(v) if a == "1" else -1 for a, v in re.findall(r"active: (\d)\n\s+velocity: (\d+)", body)]
        lanes.append({"gm": int(inst), "name": GM[int(inst)], "def": int(default), "steps": steps})
    p = {"id": path.stem, "guid": guid, "display": unquote(field(t, "DisplayName")), "folder": path.parent.name,
         "ts": ts_name, "beats": beats, "unit": unit, "measures": int(field(t, "Measures")),
         "subdivisions": int(field(t, "subdivisions")), "lanes": lanes, "palettes": []}
    total = p["measures"] * beats * p["subdivisions"]
    assert int(field(t, "beatsPerMeasure")) == beats, path
    assert all(len(l["steps"]) == total for l in lanes), path
    assert 1 <= p["subdivisions"] <= 4, path  # ConfigureGridFromOutcome clamps to 1..4
    p["totalSteps"] = total
    return p


pats = {}
for f in sorted(PATTERNS.rglob("*.asset")):
    if "DrumPatternData" in f.read_text(encoding="utf-8", errors="replace")[:800]:
        p = read_pattern(f)
        pats[p["guid"]] = p

palettes = []
for f in sorted(PALETTES.glob("*.asset")):
    t = f.read_text(encoding="utf-8")
    name = f.stem.replace("DrumPatternPalette-", "")
    palettes.append({"id": name, "display": field(t, "paletteDisplayName")})
    for guid, w in re.findall(r"pattern: \{fileID: \d+, guid: (\w+).*?\n\s+weight: ([\d.]+)", t):
        pats[guid]["palettes"].append({"id": name, "weight": float(w)})

extra_path = d / "datos-music-lab.json"  # bpm, tags and notes per pattern, reviewed by Music Lab
extra = json.loads(extra_path.read_text(encoding="utf-8")) if extra_path.exists() else {}
for p in pats.values():
    p["export"], p["snapped"] = export_text(p)
    p.update(extra.get(p["id"], {}))

out = sorted(pats.values(), key=lambda p: p["id"].lower())
data = {"palettes": palettes, "patterns": out}
(d / "patrones.json").write_text(json.dumps(data, ensure_ascii=False, indent=1), encoding="utf-8")
print(len(out), "patterns,", sum(1 for p in out if not p["palettes"]), "without palette,",
      sum(1 for p in out if p["snapped"]), "with snapped velocities")

# MGP-DRUM-FILL-1: the 13 Music Lab fills, for the "Parte con fills" view (page only; not in patrones.json)
FILLS_MD = Path("D:/Projects/MidiGenPlay/MidiGenPlay/Documentation~/research/2026-11-12_biblioteca_fills_music_lab.md")
TSB = {"FourFour": (4, 4), "ThreeFour": (3, 4), "TwoFour": (2, 4), "SixEight": (6, 8), "NineEight": (9, 8),
       "TwelveEight": (12, 8), "FiveFour": (5, 4), "SevenEight": (7, 8)}


def parse_fills(md):
    fills = []
    heads = list(re.finditer(r"^###\s+(.+?)\s*$", md, re.M))
    for i, h in enumerate(heads):
        body = md[h.end(): heads[i + 1].start() if i + 1 < len(heads) else len(md)]
        cut = re.search(r"^##\s", body, re.M)
        if cut: body = body[:cut.start()]
        fence = re.search(r"```\n(.*?)```", body, re.S)
        if not fence: continue
        w = re.search(r"(?:Peso|Weight)\s*:?\s*([0-9]+(?:[.,][0-9]+)?)", body, re.I)
        ts = re.search(r"Time signature:\s*(\w+)", body).group(1)
        subs = int(re.search(r"Subdivisions:\s*(\d+)", body).group(1))
        lanes = [(int(g), int(v)) for g, v in re.findall(r"\(GM (\d+)\)\s*—\s*default velocity (\d+)", body)]
        rows = [r.strip() for r in fence.group(1).strip().splitlines()]
        beats = TSB[ts][0]
        out = []
        for (gm, dv), row in zip(lanes, rows):
            cells = row.replace("|", "").replace(" ", "")[:beats * subs]
            out.append({"gm": gm, "def": dv, "steps": [-1 if c == "." else dv if c == "x" else ACCENT if c == "X" else GHOST for c in cells]})
        fills.append({"id": h.group(1).replace("`", "").strip(), "beats": beats, "unit": TSB[ts][1], "subdivisions": subs,
                      "weight": float(w.group(1).replace(",", ".")) if w else 1.0, "lanes": out})
    return fills


page_data = dict(data)
page_data["fills"] = parse_fills(FILLS_MD.read_text(encoding="utf-8")) if FILLS_MD.exists() else []
print(len(page_data["fills"]), "fills (Music Lab)")

tpl = d / "template.html"
if tpl.exists():
    compact = json.dumps(page_data, ensure_ascii=False, separators=(",", ":")).replace("</", "<\\/")
    html = tpl.read_text(encoding="utf-8").replace("__DATA__", compact)
    (d / "index.html").write_text(html, encoding="utf-8")
    print("index.html", len(html), "bytes")
    if "--web" in sys.argv:
        head = "\n".join([
            '<!doctype html>', '<html lang="es">', '<meta charset="utf-8">',
            '<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">',
            '<meta name="robots" content="noindex, nofollow">', '<style>',
            '/* Copia estática; fuente: ALWTTT25/Docs/reference/taller-bateria/ (build.py --web) */',
            'body { margin: 0; font: 14px/1.4 system-ui, -apple-system, "Segoe UI", sans-serif; }',
            '[hidden] { display: none !important; }', '</style>', ''])
        WEB.mkdir(parents=True, exist_ok=True)
        (WEB / "index.html").write_text(head + html, encoding="utf-8")
        print("web copy ->", WEB / "index.html")
