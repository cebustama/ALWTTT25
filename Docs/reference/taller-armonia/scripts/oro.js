// Prueba de oro de los niveles: corre arrange() de template.html sobre todo el catálogo
// (niveles 1–3 × 1–4 vueltas × 3/4 y 4/4) y escribe una instantánea, o la compara con una guardada.
// Uso: node scripts/oro.js escribir oro-v3.json   |   node scripts/oro.js comparar oro-v3.json
// Con semillas (§16): node scripts/oro.js escribir-semillas oro-v5.json   |   node scripts/oro.js comparar-semillas oro-v5.json
// El catálogo sale de index.html (python -I -X utf8 build.py antes).
const fs = require("fs"), path = require("path"), vm = require("vm");
const dir = path.join(__dirname, "..");
const tpl = fs.readFileSync(path.join(dir, "template.html"), "utf8");
const idx = fs.readFileSync(path.join(dir, "index.html"), "utf8");
const lib = idx.match(/const LIB = (\[.*?\]);\r?\n/s)[1];
const a = tpl.indexOf("/* ---------- theory"), b = tpl.indexOf("/* ---------- state");
const code = "const LIB = " + lib + ";\nconst $ = () => null;\n" + tpl.slice(a, b) +
  "\nthis.api = {LIB, parse, arrange, V3: PERILLAS_V3, romanOf, setMode: m => { mode = m; }, getMode: () => mode, fmtDur};";
const ctx = {}; vm.createContext(ctx); vm.runInContext(code, ctx);
const { LIB, parse, arrange, romanOf, setMode, fmtDur } = ctx.api;
// same text as the page's libText()
const textOf = e => e.text || e.events.map(([s, len, pc, fam]) => { const q = ({min:"min",min7:"m7",maj6:"6",min6:"m6"})[fam] || fam; return romanOf(pc, q) + (len !== 4 ? " (" + fmtDur(len/4) + ")" : ""); }).join(" – ");
// Seeds (2026-11-11, §16): "...-semillas" modes run levels 2–3 with these fixed seeds (oro-v5.json); without them, no seed (v4).
const SEMILLAS = [1, 7, 42, 2026];
const [cmd, file] = process.argv.slice(2), f = path.join(dir, "scripts", file || "oro-v3.json");
const conSemillas = /semillas/.test(cmd || "");
const show = vs => vs.map(v => v.seq.map(c => c.rest ? `S(${fmtDur(c.dur)})` :
  romanOf(c.pc, c.q) + `(${fmtDur(c.dur)})` + (c.add ? "+" : "") + (c.col ? "*" : "") + (c.why ? "[" + c.why + "]" : "")).join(" ")).join(" || ");
function run(){
  const out = {};
  for (const e of LIB.filter(x => x.origin !== "musiclab")) {   // Music Lab songs (2026-11-11) are not part of the v3 gold set
    setMode(e.mode);
    const p = parse(textOf(e));
    for (const lvl of [1,2,3]) for (const nV of [1,2,3,4]) for (const B of [3,4]) {
      const k = `${e.name} | ${e.mode} | N${lvl} | ${nV}v | ${B}/4`;
      if (!conSemillas) out[k] = show(arrange(p.chords, lvl, nV, B));
      else if (lvl > 1) for (const s of SEMILLAS) out[k + " | s" + s] = show(arrange(p.chords, lvl, nV, B, undefined, s));
    }
  }
  return out;
}
const out = run();
if (conSemillas) {   // same seed twice = same output; different seeds = something changes
  const again = run(), noDet = Object.keys(out).filter(k => out[k] !== again[k]).length;
  const base = Object.keys(out).filter(k => k.endsWith(" | s" + SEMILLAS[0])).map(k => k.slice(0, -(" | s" + SEMILLAS[0]).length));
  const varian = l => base.filter(b => b.includes(" | N" + l + " | ") && new Set(SEMILLAS.map(s => out[b + " | s" + s])).size > 1).length;
  const total = l => base.filter(b => b.includes(" | N" + l + " | ")).length;
  console.log(`determinista: ${noDet ? noDet + " casos cambian al repetir" : "sí"} · cambian con la semilla: N2 ${varian(2)}/${total(2)}, N3 ${varian(3)}/${total(3)}`);
  if (noDet) process.exit(1);
}
if (cmd === "perillas") {   // spot checks of the knobs that v3 does not use (Music Lab, 2026-11-06)
  const run = (name, lvl, change) => { const e = LIB.find(x => x.name.startsWith(name)); setMode(e.mode);
    const P = JSON.parse(JSON.stringify(ctx.api.V3)); Object.assign(P[lvl], change);
    const vs = arrange(parse(textOf(e)).chords, lvl, 2, 4, P);
    console.log(name, "N" + lvl, JSON.stringify(change), "\n  ", vs[1].seq.map(c => romanOf(c.pc, c.q) + "(" + fmtDur(c.dur) + ")").join(" ")); };
  run("Royal road", 3, {}); run("Royal road", 3, {remateFiel: true});
  run("Cincuentas", 2, {}); run("Cincuentas", 2, {preferirAcordeNuevo: true});
  run("Axis", 3, {presupuesto: 0}); run("Axis", 3, {presupuesto: 3}); run("Axis", 2, {color: "triadas", paso: "no"});
  process.exit(0);
}
if (cmd === "escribir" || cmd === "escribir-semillas") { fs.writeFileSync(f, JSON.stringify(out, null, 1)); console.log(Object.keys(out).length, "casos ->", f); }
else {
  const ref = JSON.parse(fs.readFileSync(f, "utf8")); let bad = 0;
  for (const k of new Set([...Object.keys(ref), ...Object.keys(out)])) if (ref[k] !== out[k]) { if (bad++ < 5) console.log("DISTINTO", k, "\n  antes:", ref[k], "\n  ahora:", out[k]); }
  console.log(bad ? `${bad} casos distintos de ${Object.keys(ref).length}` : `idéntico: ${Object.keys(ref).length} casos`); process.exit(bad ? 1 : 0);
}
