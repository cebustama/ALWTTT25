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

STYLES = [  # verified by the Music Lab review (revision-music-lab-2026-11-02.md)
    ("Royal road (王道進行)", "Ionian", "IVmaj7 – V7 – iiim7 – vim7", ["J-pop", "city pop"],
     "Wikipedia, citando a Ramage 2023 (Music Theory Spectrum); Yumi Arai (1974), city pop, eurobeat"),
    ("Just the Two of Us (丸サ進行)", "Aeolian", "VImaj7 – V7 – im7 – viim7 (0.5) – III7 (0.5)", ["soul", "city pop"],
     "Withers, MacDonald y Salter (Winelight, 1980), en Fa menor: D♭maj7–C7♭9–Fm7–E♭m7–A♭7 (el ♭9 pasa a 7). Duraciones por verificar"),
    ("Isn't She Lovely", "Ionian", "vim7 – II9 – V7sus4 – I", ["soul", "Stevie Wonder"],
     "Stevie Wonder (1976), en Mi mayor: C♯m7–F♯9–B7sus4–E (Hooktheory)"),
    ("Ipanema, parte A", "Ionian", "Imaj7 (2) – II7 (2) – iim7 – bII7 – Imaj7 – bII7", ["bossa nova"],
     "Jobim, «The Girl from Ipanema» (partitura de 1963; Music Lab, analysis-girl-from-ipanema.md)"),
    ("Rhythm changes, parte A", "Ionian",
     "I6 (0.5) – VI7 (0.5) – iim7 (0.5) – V7 (0.5) – iiim7 (0.5) – VI7 (0.5) – iim7 (0.5) – V7 (0.5) – I (0.5) – I7 (0.5) – IV (0.5) – ivm7 (0.5) – I (0.5) – V7 (0.5) – I",
     ["jazz"], "Gershwin, «I Got Rhythm» (1930); los 8 compases de la A (Wikipedia, «Rhythm changes»)"),
    ("Backdoor ii–V", "Ionian", "ivm7 – bVII7 – Imaj7 (2)", ["jazz", "soul"], "Recurso estándar de jazz: llegar a I desde ♭VII7 (sin ejemplos verificados)"),
    ("Teen Spirit", "Aeolian", "i (0.5) – iv (0.5) – III (0.5) – VI (0.5)",
     ["grunge", "hueco: acordes de quinta"], "Nirvana (1991), en Fa menor, i–iv–III–VI en acordes de quinta (Hooktheory)"),
    ("Riff frigio", "Phrygian", "i (2) – II – i", ["metal", "hueco: acordes de quinta"],
     "Patrón genérico (i–♭II); ejemplo verificado: la salida frigia de «Smells Like Teen Spirit» (Hooktheory)"),
    ("Guantanamera", "Ionian", "I (0.5) – IV (0.5) – V7", ["son cubano"],
     "Joseíto Fernández (crédito confirmado por el Tribunal Supremo de Cuba, 1993); variante I–IV–V–IV"),
]
for name, mode, text, tags, src in STYLES:
    lib.append({"name": name, "mode": mode, "text": text, "origin": "estilo", "tags": tags, "fuente": src})

data = json.dumps(lib, ensure_ascii=False).replace("</", "<\\/")
html = (d / "template.html").read_text(encoding="utf-8").replace("__LIB__", data)
# Claude (/api/llm) only exists on the website, behind Access: the artifact gets the page without those buttons (D4, 2026-11-06).
(d / "index.html").write_text(html.replace("__LLM__", "false"), encoding="utf-8")
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
    (WEB / "index.html").write_text(head + html.replace("__LLM__", "true"), encoding="utf-8")
    print("web copy ->", WEB / "index.html")
