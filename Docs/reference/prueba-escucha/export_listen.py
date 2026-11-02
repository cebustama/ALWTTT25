"""Export the listening-test set: playable events (beats, root pc, family) + model numbers.
Usage: python -I -X utf8 export_listen.py <surprise_tool_dir> <wrapped_data.json> <candidates.txt> <out.json> <asset_dir>...
"""
import json, re, sys
from pathlib import Path

tool, wrapped, cands, out = map(Path, sys.argv[1:5])
asset_dirs = [Path(p) for p in sys.argv[5:]]
sys.path.insert(0, str(tool))
import chord_surprise as cs  # noqa: E402

SET = {
    "casa_mayor": ["Prog_Maj_Periodo_IVV_8c", "Prog_Maj_Prestamo_bIII", "Pop_I_IV_I_V", "Rock_Mixo_I_bVII_IV_I"],
    "casa_menor": ["Prog_Min_BluesSoul_i7iv7", "Prog_Min_RotacionVII_8c", "Prog_Min_ivV_T2_8c", "Andino_Relativa2_i_bIII_iv_V"],
    "vuelta": ["Prog_Maj_Cincuentas_8c", "Prog_Maj_Axis_8c", "Pachelbel 8c", "Jazz_ii_V_I"],
    "giro": ["Prog_Maj_Nubarron_bVI", "Prog_Maj_Creep_IVpiv", "Pop_Deceptive_I_IV_V_vi", "Rock_Mario_I_bVI_bVII_I"],
    "hipnosis": ["Prog_Modal_Dorico_Vamp", "Prog_Modal_Mixo_SweetHome", "Prog_Modal_Skel_TonicaCuartoSegundo", "Andino_Relativa_i_bIII"],
    "abismo": ["Prog_Modal_Lidio_Flotante", "Prog_Modal_Frigio_Descenso", "Prog_Min_Andaluza", "Prog_Min_Napolitana_bII"],
}
REPEAT = "Prog_Maj_Axis_8c"  # played twice: consistency check

items = {i["id"]: i for i in json.loads(wrapped.read_text(encoding="utf-8"))["items"]}

def asset_events(path):
    text = path.read_text(encoding="utf-8", errors="replace")
    ts = int(re.search(r"^\s*TimeSignature: (\d+)", text, re.M).group(1))
    sub = int(re.search(r"^\s*subdivisions: (\d+)", text, re.M).group(1))
    measures = int(re.search(r"^\s*Measures: (\d+)", text, re.M).group(1))
    assert ts == 0, f"{path.stem}: only 4/4 in the test"
    evs, cur, tonal, orig, policy = [], None, [], "", 0
    for line in text.splitlines():
        s = line.strip()
        if s.startswith("- startStep:"):
            cur = {"start": int(s.split(":")[1])}
            evs.append(cur)
        elif cur is not None and ":" in s and not s.startswith("-"):
            k, v = s.split(":", 1)
            if k in ("lengthSteps", "degree", "quality", "degreeAccidental", "hasAppliedTarget", "appliedTarget", "isDiatonic"):
                cur[k] = int(v)
            elif k in ("tonalities", "originalInput", "qualityRenderPolicy", "m_Name"):
                cur = None
        if s.startswith("tonalities:"):
            tonal = cs._hex_ints(s.split(":", 1)[1])
        if s.startswith("originalInput:"):
            orig = s.split(":", 1)[1].strip().strip('"')
        if s.startswith("qualityRenderPolicy:"):
            policy = int(s.split(":")[1])
    mode = cs.home_mode(path.stem, orig, [cs.MODES[t] for t in tonal if 0 <= t < 7])
    steps = cs.MODE_STEPS[mode]
    res = []
    for e in sorted(evs, key=lambda e: e["start"]):
        deg, acc = e.get("degree", 0) % 7, e.get("degreeAccidental", 0)
        if e.get("hasAppliedTarget"):
            pc, fam = steps[e.get("appliedTarget", 0) % 7] + 7, "7"
        else:
            pc = steps[deg] + acc
            q = e.get("quality", 0)
            fam = cs.MGP_QUALITY[q] if 0 <= q < len(cs.MGP_QUALITY) else "maj"
            if policy > 0 and acc == 0 and e.get("isDiatonic", 1):
                fam = cs.diatonic_family(mode, deg, fam, functional=(policy == 2))
        res.append([e["start"] / sub, e.get("lengthSteps", sub * 4) / sub, pc % 12, fam])
    return mode, measures * 4, res

def find_asset(name):
    for d in asset_dirs:
        for p in d.rglob("*.asset"):
            t = p.read_text(encoding="utf-8", errors="replace")
            if f"m_Name: {name}\n" in t and "ChordProgressionData" in t:
                return p
    raise FileNotFoundError(name)

cand = {}
for line in cands.read_text(encoding="utf-8").splitlines():
    if line.strip() and not line.startswith("#"):
        n, m, body = [x.strip() for x in line.split("|")]
        cand[n] = (m, body.split())

clips = []
for pal, names in SET.items():
    for n in names:
        if n in cand:
            mode, toks = cand[n]
            evs, b = [], 0
            for i, t in enumerate(toks):
                pc, fam = cs.parse_roman(t, mode).split(":")
                ln = 8 if (len(toks) == 3 and i == 2) else 4
                evs.append([b, ln, int(pc) % 12, fam]); b += ln
            src, beats = "candidata", b
        else:
            mode, beats, evs = asset_events(find_asset(n))
            src = "biblioteca"
        it = items[n]
        zones = [s[2] for s in it["steps"]]
        clips.append({"id": n, "palette": pal, "source": src, "mode": mode, "roman": it["roman"],
                      "beats": beats, "events": evs, "S": it["S"], "U": it["U"], "cheung": it["ch"],
                      "zones": zones, "libPalettes": it.get("palettes") or []})
rep = dict(next(c for c in clips if c["id"] == REPEAT)); rep["repeatOf"] = REPEAT
clips.append(rep)
out.write_text(json.dumps(clips, ensure_ascii=False), encoding="utf-8")
for c in clips:
    print(f"{c['palette']:10} {c['source']:10} {c['mode']:10} {c['beats']:3} {c['id']}")
