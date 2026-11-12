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
    WEB.mkdir(parents=True, exist_ok=True)
    (WEB / "index.html").write_text(html, encoding="utf-8")
    print("copiado a", WEB)
