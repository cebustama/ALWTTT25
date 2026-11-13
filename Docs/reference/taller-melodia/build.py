"""Builds the Taller de melodías: template.html + motor-voz.js + motor-canto.js -> index.html.
The two engines are inlined twice: as page scripts (live play) and as the Web Worker source (renders).
Run: python -I -X utf8 build.py [--web]
"""
import sys
from pathlib import Path

d = Path(__file__).parent
WEB = Path("D:/Projects/web/claudiobustamante-site/public/privado/taller-melodia")

voz = (d / "motor-voz.js").read_text(encoding="utf-8")
canto = (d / "motor-canto.js").read_text(encoding="utf-8")
assert "</script" not in voz + canto
html = (d / "template.html").read_text(encoding="utf-8")
html = html.replace("/*MOTOR_VOZ*/", voz).replace("/*MOTOR_CANTO*/", canto)
(d / "index.html").write_text(html, encoding="utf-8")
print(f"index.html: {len(html.encode('utf-8')):,} bytes")
if "--web" in sys.argv:
    # Same static head as taller-bajo/build.py: the artifact skeleton adds charset and viewport, the web does not.
    head = "\n".join([
        '<!doctype html>', '<html lang="es">', '<meta charset="utf-8">',
        '<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">',
        '<meta name="robots" content="noindex, nofollow">', '<style>',
        '/* Copia estática; fuente: ALWTTT25/Docs/reference/taller-melodia/ (build.py --web) */',
        'body { margin: 0; font: 14px/1.4 system-ui, -apple-system, "Segoe UI", sans-serif; }',
        '[hidden] { display: none !important; }', '</style>', ''])
    WEB.mkdir(parents=True, exist_ok=True)
    (WEB / "index.html").write_text(head + html, encoding="utf-8")
    print("web copy ->", WEB / "index.html")
