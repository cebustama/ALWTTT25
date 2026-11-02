"""Builds index.html for the Taller armónico.

The catalogue seeds come from the listening test (24 progressions with their real
events) plus a few style examples proposed by the agent (marked, sources to verify).
Run: python -I -X utf8 build.py
"""
import json
import re
from pathlib import Path

d = Path(__file__).parent
listen = d.parent / "prueba-escucha"
clips = json.loads((listen / "listen_clips.json").read_text(encoding="utf-8"))
v1 = (listen / "template.html").read_text(encoding="utf-8")
label = json.loads(re.sub(r",\s*\}$", "}", re.search(r"const LABEL = (\{.*?\});", v1, re.S).group(1)))
PALETTE = {"casa_mayor": "casa mayor", "casa_menor": "casa menor", "vuelta": "vuelta",
           "giro": "giro", "hipnosis": "hipnosis", "abismo": "abismo"}

lib = []
for c in clips:
    if c.get("repeatOf"):
        continue
    lib.append({"name": label.get(c["id"], c["id"]), "mode": c["mode"], "events": c["events"],
                "origin": "biblioteca" if c["source"] == "biblioteca" else "candidata",
                "tags": [PALETTE[c["palette"]]], "fuente": c["id"]})

STYLES = [
    ("Royal road (王道進行)", "Ionian", "IVmaj7 – V7 – iiim7 – vim7", ["J-pop", "city pop"],
     "Progresión típica del J-pop y el city pop"),
    ("Just the Two of Us", "Ionian", "IVmaj7 – III7 – vim7 – vm7 (0.5) – I7 (0.5)", ["soul", "city pop"],
     "Grover Washington Jr. con Bill Withers (1980)"),
    ("Isn't She Lovely", "Ionian", "iim7 – V7 – I6 – VI7", ["soul", "Stevie Wonder"],
     "Stevie Wonder (1976): C♯m7–F♯7–B6–G♯7"),
    ("Ipanema, parte A", "Ionian", "Imaj7 (2) – II7 (2) – iim7 – bII7 – Imaj7 – bII7", ["bossa nova"],
     "Jobim, «The Girl from Ipanema» (1962)"),
    ("Rhythm changes, parte A", "Ionian",
     "Imaj (0.5) – VI7 (0.5) – iim7 (0.5) – V7 (0.5) – iiim7 (0.5) – VI7 (0.5) – iim7 (0.5) – V7 (0.5)",
     ["jazz"], "Gershwin, «I Got Rhythm» (1930)"),
    ("Backdoor ii–V", "Ionian", "ivm7 – bVII7 – Imaj7 (2)", ["jazz", "soul"], "Recurso de jazz: llegar a I desde ♭VII7"),
    ("Teen Spirit", "Ionian", "Imaj (0.5) – IVmaj (0.5) – bIIImaj (0.5) – bVImaj (0.5)",
     ["grunge", "hueco: acordes de quinta"], "Nirvana (1991): F–B♭–A♭–D♭, en acordes de quinta en el original"),
    ("Riff frigio", "Phrygian", "im (2) – IImaj – im", ["metal", "hueco: acordes de quinta"],
     "Patrón genérico del metal: i–♭II"),
    ("Guantanamera", "Ionian", "Imaj (0.5) – IVmaj (0.5) – V7", ["son cubano"], "Joseíto Fernández: I–IV–V7"),
]
for name, mode, text, tags, src in STYLES:
    lib.append({"name": name, "mode": mode, "text": text, "origin": "estilo", "tags": tags, "fuente": src + " · por verificar"})

data = json.dumps(lib, ensure_ascii=False).replace("</", "<\\/")
html = (d / "template.html").read_text(encoding="utf-8").replace("__LIB__", data)
(d / "index.html").write_text(html, encoding="utf-8")
print(len(lib), "entries,", len(html), "bytes")

# Static copy for the private section of the website (behind Cloudflare Access):
# same page with the artifact skeleton made explicit; no shared db there, saves stay in the browser.
WEB = Path("D:/Projects/web/claudiobustamante-site/public/privado/taller-armonia")
if "--web" in __import__("sys").argv:
    head = chr(10).join([
        '<!doctype html>', '<html lang="es">', '<meta charset="utf-8">',
        '<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">',
        '<meta name="robots" content="noindex, nofollow">',
        '<style>',
        '/* Copia estática; fuente: ALWTTT25/Docs/reference/taller-armonia/ (build.py --web) */',
        'body { margin: 0; font: 14px/1.4 system-ui, -apple-system, "Segoe UI", sans-serif; }',
        '[hidden] { display: none !important; }',
        '</style>', ''])
    WEB.mkdir(parents=True, exist_ok=True)
    (WEB / "index.html").write_text(head + html, encoding="utf-8")
    print("web copy ->", WEB / "index.html")
