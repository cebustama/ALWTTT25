// Casos de P1–P5 (2026-11-11): cada perilla nueva, encendida sola sobre una progresión escrita a mano,
// en las vueltas 1, 2 y 3 (la última, la del remate). Sirve para comprobar las reglas de revision-music-lab-reglas-P1-P5.md.
// Uso: node scripts/p1p5.js            (correr antes python -I -X utf8 build.py)
const fs = require("fs"), path = require("path"), vm = require("vm");
const dir = path.join(__dirname, "..");
const tpl = fs.readFileSync(path.join(dir, "template.html"), "utf8");
const a = tpl.indexOf("/* ---------- theory"), b = tpl.indexOf("/* ---------- state");
const ctx = {}; vm.createContext(ctx);
vm.runInContext("const LIB = [];\nconst $ = () => null;\n" + tpl.slice(a, b) +
  "\nthis.api = {parse, arrange, V3: PERILLAS_V3, romanOf, setMode: m => { mode = m; }, fmtDur};", ctx);
const { parse, arrange, romanOf, setMode, fmtDur, V3 } = ctx.api;
const show = v => v.seq.map(c => c.rest ? "S" : romanOf(c.pc, c.q) + (c.dur !== 1 ? "(" + fmtDur(c.dur) + ")" : "") + (c.add ? "+" : "")).join(" ");
function run(label, text, mode, lvl, change, beats){
  setMode(mode);
  const P = JSON.parse(JSON.stringify(V3)); Object.assign(P[lvl], change);
  const p = parse(text); if(p.issues.length) console.log("  aviso:", p.issues.map(i => i.msg).join(" | "));
  const base = arrange(p.chords, lvl, 3, beats || 4), vs = arrange(p.chords, lvl, 3, beats || 4, P);
  console.log(label + "  [" + mode + ", N" + lvl + " " + JSON.stringify(change) + "]");
  for(const [n, x] of [["v3", base], ["P ", vs]]) console.log("   " + n + " v1: " + show(x[0]) + "\n          v2: " + show(x[1]) + "\n          v3 (última): " + show(x[2]));
}
const N = {presupuesto: 3}, solo = {sus4:false, remate:"no"};
console.log("— P1 · paso ii–V");
run("I (2) → IV (Misty A1)", "I (2) – IV (2) – ii – V", "Ionian", 2, {paso:"iiV", ...solo});
run("I7 (2) → ii (Billie's c. 7–9)", "I (2) – ii (2) – V (2) – I (2)", "Ionian", 2, {paso:"iiV", ...solo});
run("un compás: solo V7 (D1 = A)", "I – vi – ii – V", "Ionian", 2, {paso:"iiV", ...solo});
run("ii de la escala hacia ii (Billie's: Am7)", "I (2) – ii (2) – V – I", "Ionian", 2, {paso:"iiV", ...solo});
run("ii según el destino (perilla): ø7", "I (2) – ii (2) – V – I", "Ionian", 2, {paso:"iiV", iiDelDestino:true, ...solo});
run("en Eólico, iv → v", "i (2) – iv (2) – v – i", "Aeolian", 2, {paso:"iiV", ...solo});
console.log("— P2 · cadena de dominantes");
run("All of Me A", "I (2) – iii (2) – vi (2) – ii (2)", "Ionian", 2, {cadenaDominantes:true});
run("Cincuentas: el ii que va a V se queda", "I – vi – ii – V", "Ionian", 2, {cadenaDominantes:true});
run("en Eólico no hace nada", "i – iv – VII – III", "Aeolian", 2, {cadenaDominantes:true});
console.log("— P3 · disminuido");
run("sube: I → ii", "I – ii – V – I", "Ionian", 2, {disminuido:true, paso:"no", ...solo});
run("vecino: IV → I", "I – iii – vi – IV", "Ionian", 2, {disminuido:true, paso:"no", sus4:false, remate:"no"});
run("baja: I → V (Wave)", "I (2) – V (2) – IV – I", "Ionian", 2, {disminuido:true, paso:"no", ...solo});
run("los tres, nivel 3", "I – ii – V – vi – IV – I", "Ionian", 3, {disminuido:true, ...N});
console.log("— P4 · ♭VI7 en menor");
run("blues menor: VI → V", "i – iv – VI – V", "Aeolian", 2, {bVI7Menor:true});
run("SIBLY: VI → iv", "i – VI – iv – V", "Aeolian", 2, {bVI7Menor:true});
run("VI → III: no", "i – VI – III – VII", "Aeolian", 2, {bVI7Menor:true});
console.log("— P5 · remate deslizado");
run("Jónico", "I – vi – IV (2) – ii (2)", "Ionian", 2, {remate:"deslizado"});
run("Eólico", "i – iv – VII – III (2)", "Aeolian", 2, {remate:"deslizado"});
run("último acorde = ♭VI", "i – iv – VII – VI (2)", "Aeolian", 2, {remate:"deslizado"});
run("Dórico: su acorde modal", "i – VII – III – v", "Dorian", 2, {remate:"deslizado"});
console.log("— D9 · remate ii–V");
run("Jónico", "I – vi – IV – iii (2)", "Ionian", 2, {remate:"iiV"});
run("Eólico", "i – iv – VII – III (2)", "Aeolian", 2, {remate:"iiV"});
run("último acorde = ii", "I – vi – iii – ii (2)", "Ionian", 2, {remate:"iiV"});
run("3/4: el último dura 2 compases → solo V7", "I – vi – IV – iii (2)", "Ionian", 2, {remate:"iiV"}, 3);
console.log("— Perfil «jazz» de prueba, nivel 3");
run("Cincuentas larga", "I (2) – vi (2) – ii (2) – V (2)", "Ionian", 3, {paso:"iiV", cadenaDominantes:true, disminuido:true, remate:"iiV", ivPrestado:false, bVI:false, ...N});
