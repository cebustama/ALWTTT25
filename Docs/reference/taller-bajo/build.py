"""Builds the Taller de bajo (MGP-BASS-ACCENT-1).

Reads, read-only: the drum patterns and Music Lab fills the Taller de batería already
exports (taller-bateria/patrones.json + its build.parse_fills), the 4/4 ChordProgressionData
assets, and injects the drum kit and the FX fill/accent engine from taller-bateria/template.html
(one source: change the drum engine there). Writes index.html.
Run: python -I -X utf8 build.py [--web]
"""
import json
import re
import sys
from pathlib import Path

d = Path(__file__).parent
TB = d.parent / "taller-bateria"
CHORDS = Path("D:/Projects/ALWTTT25/Assets/Resources/ScriptableObjects/Patterns/Chords")
FILLS_MD = Path("D:/Projects/MidiGenPlay/MidiGenPlay/Documentation~/research/2026-11-12_biblioteca_fills_music_lab.md")
WEB = Path("D:/Projects/web/claudiobustamante-site/public/privado/taller-bajo")

TS = ["FourFour", "ThreeFour", "TwoFour", "SixEight", "NineEight", "TwelveEight", "FiveFour", "SevenEight"]
BEATS = {"FourFour": 4, "ThreeFour": 3, "TwoFour": 2, "SixEight": 6, "NineEight": 9, "TwelveEight": 12,
         "FiveFour": 5, "SevenEight": 7}


def block(text, start, end):
    i = text.index(start)
    j = text.index(end, i)
    return text[i:j]


def fx_block(text):
    i = text.index("const FX = (() => {")
    j = text.index("})();", i) + len("})();")
    return text[i:j]


def parse_fills():
    """Same parser as taller-bateria/build.py (imported by text so the two never diverge)."""
    src = (TB / "build.py").read_text(encoding="utf-8")
    ns = {"re": re, "Path": Path}
    exec(block(src, "TSB = {", "page_data = dict(data)").replace("TSB = {", "TSB = {", 1), ns)
    ns["ACCENT"], ns["GHOST"] = 120, 50
    return ns["parse_fills"](FILLS_MD.read_text(encoding="utf-8")) if FILLS_MD.exists() else []


def read_progression(path):
    t = path.read_text(encoding="utf-8")
    ts = TS[int(re.search(r"^  TimeSignature: (\d+)", t, re.M).group(1))]
    if ts != "FourFour":
        return None
    name = re.search(r"^  m_Name: (.+)$", t, re.M).group(1).strip()
    meas = int(re.search(r"^  Measures: (\d+)", t, re.M).group(1))
    sub = int(re.search(r"^  subdivisions: (\d+)", t, re.M).group(1))
    evs = []
    for m in re.finditer(r"  - startStep: (\d+)\n    lengthSteps: (\d+)\n    degree: (\d+)\n    quality: (\d+)\n"
                         r"    velocity: (\d+)\n(?:.*\n)*?    degreeAccidental: (-?\d+)", t):
        s, ln, deg, q, v, acc = map(int, m.groups())
        evs.append({"startStep": s, "lengthSteps": ln, "degree": deg, "quality": q, "velocity": v,
                    "degreeAccidental": acc})
    if not evs:
        return None
    return {"id": name, "label": f"{name} ({meas} c.)", "measures": meas, "subdivisions": sub, "events": evs}


pats = json.loads((TB / "patrones.json").read_text(encoding="utf-8"))
patterns = [{k: p[k] for k in ("guid", "display", "beats", "unit", "measures", "subdivisions", "lanes", "bpm")
             if k in p} for p in pats["patterns"]]
progs = [p for p in (read_progression(f) for f in sorted(CHORDS.glob("*.asset"))) if p]
# Real, musical progressions first; the smoke/test assets last.
progs.sort(key=lambda p: (bool(re.search(r"Test|Smoke", p["id"])), p["id"]))
fills = parse_fills()
print(len(patterns), "patterns,", len(progs), "4/4 progressions,", len(fills), "fills")

drum_tpl = (TB / "template.html").read_text(encoding="utf-8")
kit = block(drum_tpl, "/* ---------- kit:", "/* ---------- v2: the part with fills")
fx = fx_block(drum_tpl)

data = {"patterns": patterns, "progressions": progs, "fills": fills}
compact = json.dumps(data, ensure_ascii=False, separators=(",", ":")).replace("</", "<\\/")
html = ((d / "template.html").read_text(encoding="utf-8")
        .replace("__FX__", fx).replace("__KIT__", kit).replace("__DATA__", compact))
(d / "index.html").write_text(html, encoding="utf-8")
print("index.html", len(html), "bytes")

if "--web" in sys.argv:
    head = "\n".join([
        '<!doctype html>', '<html lang="es">', '<meta charset="utf-8">',
        '<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">',
        '<meta name="robots" content="noindex, nofollow">', '<style>',
        '/* Copia estática; fuente: ALWTTT25/Docs/reference/taller-bajo/ (build.py --web) */',
        'body { margin: 0; font: 14px/1.4 system-ui, -apple-system, "Segoe UI", sans-serif; }',
        '[hidden] { display: none !important; }', '</style>', ''])
    WEB.mkdir(parents=True, exist_ok=True)
    (WEB / "index.html").write_text(head + html, encoding="utf-8")
    print("web copy ->", WEB / "index.html")
