// Paridad del motor de voz: genera casos.json (guiones de parámetros por bloque de 1024),
// renderiza cada caso con motor-voz.js y compara con la salida de C# (dotnet run → out/*.f32).
import { createRequire } from 'module'; import fs from 'fs';
const require = createRequire(import.meta.url);
const MV = require('../motor-voz.js');
const SR = 48000, BLOCK = 1024;
function guion(seed, blocks, singer) {
  const r = MV.mulberry32(seed); const steps = [];
  for (let b = 0; b < blocks; b++) {
    const s = {};
    if (singer) {
      if (b === 0) Object.assign(s, { alwaysVoice: false, wobble: false, pj: 0.15, tj: 0.15, vib: 0, vibF: 6 });
      if (b % 12 === 0) Object.assign(s, { note: 36 + Math.floor(r() * 24) - 69, touched: true, loud: 0.3 + 0.7 * r(), tens: 0.4 + 0.2 * r() });
      if (b % 12 === 9) s.touched = false;
      if (b % 12 === 6) s.vib = 0.0048;
      if (b % 24 === 3) { s.idx = 12 + r() * 16; s.dia = 2 + r() * 1.2; }
    } else if (b === 0) { s.note = -12; }
    steps.push(s);
  }
  return steps;
}
const casos = [
  { name: 'upstream-defaults', seed: 12345, steps: guion(1, 94, false) },
  { name: 'cantante-a', seed: 777, steps: guion(2, 188, true) },
  { name: 'cantante-b', seed: -42, steps: guion(3, 188, true) },
];
fs.writeFileSync('casos.json', JSON.stringify(casos));
function render(c) {
  const pt = new MV.PinkTrombone(SR, c.seed); const out = new Float32Array(c.steps.length * BLOCK);
  pt.reset();
  c.steps.forEach((s, b) => {
    if ('alwaysVoice' in s) pt.alwaysVoice = s.alwaysVoice;
    if ('wobble' in s) pt.vibratoWobble = s.wobble;
    if ('pj' in s) pt.pitchJitterGain = s.pj; if ('tj' in s) pt.tensenessJitterGain = s.tj;
    if ('vib' in s) pt.vibratoGain = s.vib; if ('vibF' in s) pt.vibratoFrequency = s.vibF;
    if ('note' in s) pt.setMusicalNote(s.note); if ('touched' in s) pt.isTouched = s.touched;
    if ('loud' in s) pt.loudness = s.loud; if ('tens' in s) pt.targetTenseness = s.tens;
    if ('idx' in s) pt.setTongue(s.idx, s.dia);
    pt.synthesize(out, b * BLOCK, BLOCK);
  });
  return out;
}
if (process.argv[2] === 'comparar') {
  let ok = true;
  for (const c of casos) {
    const js = render(c);
    const b = fs.readFileSync(`out/${c.name}.f32`); const cs = new Float32Array(b.buffer, b.byteOffset, b.length / 4);
    let maxd = 0, pk = 0, se = 0, sd = 0, same = 0;
    for (let i = 0; i < js.length; i++) { const d = Math.abs(js[i] - cs[i]); maxd = Math.max(maxd, d); pk = Math.max(pk, Math.abs(cs[i])); se += cs[i] * cs[i]; sd += d * d; if (js[i] === cs[i]) same++; }
    const db = 10 * Math.log10(sd / se);
    console.log(`${c.name}: ${js.length} muestras, idénticas ${(100 * same / js.length).toFixed(1)} %, dif. máx ${maxd.toExponential(2)} (pico ${pk.toFixed(3)}), error ${db.toFixed(1)} dB`);
    if (db > -60) ok = false;
  }
  console.log(ok ? 'PARIDAD OK (error < −60 dB en todos)' : 'PARIDAD FALLA'); process.exit(ok ? 0 : 1);
} else console.log('casos.json escrito; ahora: dotnet run -c Release && node casos.mjs comparar');
