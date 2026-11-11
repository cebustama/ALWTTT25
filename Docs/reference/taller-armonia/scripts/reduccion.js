// Prueba de reducción (2026-11-11, paso 3 de ARRANQUE_niveles_recursos.md).
// Toma secciones reales de la Biblioteca de Music Lab (las entradas origin "musiclab" del catálogo), las compara con su
// esqueleto (escrito a mano con las reglas R1–R5 de abajo) pasado por los niveles con cada perilla, y mide cuánto se
// parece el resultado a la canción, por medios compases (raíz + familia del acorde).
// Uso: node scripts/reduccion.js [md]      (correr antes python -I -X utf8 build.py; con «md» escribe ../reduccion-resultados.md)
//
// Reglas de reducción (cómo se escribe el esqueleto, lo que escribiría una carta):
//  R1. Los acordes de la escala se quedan, con la tríada del modo (sin séptimas ni color: eso lo pone el nivel).
//  R2. Un acorde de la escala dominantizado que dura un compás o más (III7, VI7, II7) vuelve a su tríada (iii, vi, ii).
//  R3. Lo que anuncia al acorde siguiente dentro de un compás (ii–V hacia otro acorde, dominante secundaria, °7, sustituto
//      tritonal, deslizamiento, backdoor, iv prestado tras el IV) se quita y su tiempo pasa al acorde anterior: la inversa
//      de la regla del taller (lo añadido ocupa la segunda parte del acorde anterior).
//  R4. Los acordes de fuera de la escala que estructuran la sección (el vamp im7–IV7 de Wave) se quedan como están.
//  R5. Los turnarounds de medio compás que son la sección misma (Misty cc. 5–8, Oleo A) se quedan a su ritmo, en tríadas.
const fs = require("fs"), path = require("path"), vm = require("vm");
const dir = path.join(__dirname, "..");
const tpl = fs.readFileSync(path.join(dir, "template.html"), "utf8");
const idx = fs.readFileSync(path.join(dir, "index.html"), "utf8");
const LIB = JSON.parse(idx.match(/const LIB = (\[.*?\]);\r?\n/s)[1]);
const a = tpl.indexOf("/* ---------- theory"), b = tpl.indexOf("/* ---------- state");
const ctx = {}; vm.createContext(ctx);
vm.runInContext("const LIB = [];\nconst $ = () => null;\n" + tpl.slice(a, b) +
  "\nthis.api = {parse, arrange, V3: PERILLAS_V3, romanOf, setMode: m => { mode = m; }, fmtDur};", ctx);
const { parse, arrange, romanOf, setMode, V3 } = ctx.api;

const SECC = [
  {slug:"all-of-me-a1", sk:"I (2) – iii (2) – vi (2) – ii (2)", rec:"P2", nota:"R2: III7 y VI7 vuelven a iii y vi"},
  {slug:"all-of-me-b",  sk:"iii (2) – vi (2) – ii (3) – V", rec:"P2", nota:"R2: II7 → ii, y se une al iim7 que le sigue"},
  {slug:"all-of-me-c",  sk:"IV (2) – I – vi – ii – V – I (2)", rec:"prestado, P1, P2", nota:"R3: el iv (c. 2) va al IV y el iiim7 (c. 3) al I; R2: VI7 → vi"},
  {slug:"billies-bounce-a", sk:"I – IV – I (2) – IV (2) – I (2) – ii – V – I (2)", rec:"P1, P3 vecino", nota:"blues de tres acordes: R3 quita el ii–V del c. 4, el B°7, el ii–V del c. 8 y el turnaround final"},
  {slug:"misty-a1",     sk:"I (2) – IV (2) – I (0.5) – vi (0.5) – ii (0.5) – V (0.5) – iii (0.5) – vi (0.5) – ii (0.5) – V (0.5)", rec:"P1, backdoor, P2", nota:"R3: vm7–I7 al I y la backdoor al IV; R5 cc. 5–8"},
  {slug:"oleo-a1",      sk:"I (0.5) – vi (0.5) – ii (0.5) – V (0.5) – iii (0.5) – vi (0.5) – ii (0.5) – V (0.5) – I – IV – iii (0.5) – vi (0.5) – ii (0.5) – V (0.5)", rec:"P2, P1", nota:"R5; R3: vm7–I7 al I (c. 5) y ♭VII7 al IV (c. 6)"},
  {slug:"oleo-b",       sk:"iii (2) – vi (2) – ii (2) – V (2)", rec:"P2", nota:"R2: la cadena entera vuelve a la escala"},
  {slug:"wave-a1",      sk:"I (4) – IV (2) – iii – vi – ii – V – i (0.5) – IV (0.5) – i (0.5) – IV (0.5)", rec:"P3 baja, P1, prestado, P2, P5", nota:"R3: B♭°7 y el ii–V del IV (vm7–I7) al I, Gm6 al IV, B♭7 al II; R4: el vamp im7–IV7"},
  {slug:"summertime-a1", sk:"i (4) – iv – VI (2) – V", rec:"P4, P1", nota:"R3: el vecino iim6 y el I7 al i, el ii–V del V al VI; R2: VI7 → VI"},
];
const FAM = {maj:"M",maj7:"M",maj9:"M","6":"M",aug:"M",min:"m",m7:"m",m9:"m",m6:"m","7":"7","9":"7","7sus4":"7",hdim7:"ø",dim:"o",dim7:"o",sus2:"s",sus4:"s"};
const slots = seq => seq.flatMap(c => Array(Math.round(c.dur / 0.5)).fill(c.rest ? "-" : c.pc + FAM[c.q]));
const N = {presupuesto: 3};
const CONF = [["Nivel 1 (esqueleto)", 1, {}]];
for(const l of [2, 3]) CONF.push(
  ["N" + l + " v3", l, {}],
  ["N" + l + " + P1 ii–V", l, {paso:"iiV"}],
  ["N" + l + " + P1, ii del destino", l, {paso:"iiV", iiDelDestino:true}],
  ["N" + l + " + P2 cadena", l, {cadenaDominantes:true}],
  ["N" + l + " + P3 disminuido", l, {disminuido:true}],
  ["N" + l + " + P4 ♭VI7", l, {bVI7Menor:true}],
  ["N" + l + " + P5 deslizado", l, {remate:"deslizado"}],
  ["N" + l + " + D9 remate ii–V", l, {remate:"iiV"}],
  ["N" + l + " propuesta", l, l === 2 ? {paso:"iiV", bVI7Menor:true} : {cadenaDominantes:true, disminuido:true, bVI7Menor:true}],
  ["N" + l + " «jazz» (P1+P2+P3+D9)", l, {paso:"iiV", cadenaDominantes:true, disminuido:true, remate:"iiV"}],
  ["N" + l + " P1+P2", l, {paso:"iiV", cadenaDominantes:true}],
  ["N" + l + " P2+P4", l, {cadenaDominantes:true, bVI7Menor:true}],
  ["N" + l + " P1+P2+P4", l, {paso:"iiV", cadenaDominantes:true, bVI7Menor:true}]);

function measure(e, s, lvl, over){
  setMode(e.mode);
  const real = slots(parse(e.text).chords), skc = parse(s.sk), sk = slots(skc.chords);
  if(skc.issues.some(i => i.e) || real.length !== sk.length) throw new Error(s.slug + ": el esqueleto no cuadra (" + sk.length + " frente a " + real.length + " medios compases)");
  const P = JSON.parse(JSON.stringify(V3)); if(lvl > 1) Object.assign(P[lvl], over);
  const vs = arrange(skc.chords, lvl, 3, 4, P);
  const r = {n:0, ok:0, tgt:0, hit:0, chg:0, bad:0, laps: vs.map(v => v.seq)};
  for(const v of vs){ const x = slots(v.seq);
    x.forEach((q, i) => { r.n++; if(q === real[i]) r.ok++;
      if(sk[i] !== real[i]){ r.tgt++; if(q === real[i]) r.hit++; }
      if(q !== sk[i]){ r.chg++; if(q !== real[i]) r.bad++; } }); }
  return r;
}
const pct = (x, n) => n ? Math.round(100 * x / n) + " %" : "—";
const show = seq => seq.map(c => romanOf(c.pc, c.q) + (c.dur !== 1 ? "(" + c.dur + ")" : "")).join(" ");
const out = [];
const P_ = s => out.push(s);
const items = SECC.map(s => ({s, e: LIB.find(x => x.slug === s.slug)}));
for(const it of items) if(!it.e) throw new Error("no está en el catálogo: " + it.s.slug);

P_("| Configuración | Parecido | Adornos de la canción que acierta | Cambios inventados | Parecido, sin tope | Acierta, sin tope | Inventados, sin tope |");
P_("|---|---|---|---|---|---|---|");
const RES = {};
for(const [name, lvl, over] of CONF){
  const row = [name];
  for(const extra of [{}, lvl > 1 ? N : {}]){
    const t = {n:0, ok:0, tgt:0, hit:0, chg:0, bad:0};
    for(const {s, e} of items){ const r = measure(e, s, lvl, {...over, ...extra}); for(const k in t) t[k] += r[k];
      (RES[name + (extra.presupuesto ? " sin tope" : "")] ??= {})[s.slug] = r; }
    row.push(pct(t.ok, t.n), pct(t.hit, t.tgt) + " (" + t.hit + "/" + t.tgt + ")", pct(t.bad, t.chg) + " (" + t.bad + "/" + t.chg + ")");
  }
  P_("| " + row.join(" | ") + " |");
}
P_("");
P_("Por sección: parecido (sobre las 3 vueltas) con el presupuesto del nivel.");
P_("");
const COLS = ["Nivel 1 (esqueleto)", "N2 v3", "N2 propuesta", "N3 v3", "N3 propuesta", "N3 «jazz» (P1+P2+P3+D9)"];
P_("| Sección | Recursos | " + COLS.join(" | ") + " | N3 «jazz», sin tope |");
P_("|---|---|" + COLS.map(() => "---").join("|") + "|---|");
for(const {s} of items) P_("| " + s.slug + " | " + s.rec + " | " + [...COLS, "N3 «jazz» (P1+P2+P3+D9) sin tope"].map(c => { const r = RES[c][s.slug]; return pct(r.ok, r.n); }).join(" | ") + " |");
P_("");
P_("Vuelta 2 del nivel 3 «jazz» sin tope, frente a la canción:");
P_("");
for(const {s, e} of items){ setMode(e.mode); const r = RES["N3 «jazz» (P1+P2+P3+D9) sin tope"][s.slug];
  P_("- **" + s.slug + "** (" + s.nota + ")");
  P_("  - esqueleto: `" + s.sk + "`");
  P_("  - taller:    `" + show(r.laps[1]) + "`");
  P_("  - canción:   `" + e.text + "`"); }
const text = out.join("\n");
console.log(text);
if(process.argv[2] === "md") fs.writeFileSync(path.join(dir, "reduccion-resultados.md"), "# Prueba de reducción · resultados (generado por scripts/reduccion.js)\n\n" + text + "\n");
