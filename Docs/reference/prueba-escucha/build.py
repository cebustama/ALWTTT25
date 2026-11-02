"""Build index.html from template.html + listen_clips.json."""
from pathlib import Path

d = Path(__file__).parent
data = (d / "listen_clips.json").read_text(encoding="utf-8").replace("</", "<\\/")
html = (d / "template.html").read_text(encoding="utf-8").replace("__DATA__", data)
(d / "index.html").write_text(html, encoding="utf-8")
print(len(html), "bytes")
