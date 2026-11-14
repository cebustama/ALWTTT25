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
// VOZ-1 (2026-11-14): X1 consonantes (constricciones, velo, turbulencia), X2 tracto, X3 glotis extra.
// Consonantes con la tabla de motor-canto.js: [índice 44, diámetro, tipo].
const CONS = [[21, 0, 'ocl'], [36, 0.55, 'fri'], [41, 0, 'nas'], [31, 0.55, 'fri'], [38, 0.9, 'apr'], [41, 0.5, 'fri'], [36, 0, 'ocl']];
function guionX(seed, blocks, x) {
  const steps = guion(seed, blocks, true), r = MV.mulberry32(seed + 100);
  steps.forEach((s, b) => {
    if (b === 0 && x) Object.assign(s, x);
    if (b % 12 === 0) {
      const c = CONS[Math.floor(r() * CONS.length)];
      s.constr = [[c[0], c[1]]];
      if (c[2] === 'nas') s.velum = 0.4;
      if (c[2] === 'fri') s.turbOn = [c[0], c[1]];
    }
    if (b % 12 === 2) { s.constr = []; s.velum = null; s.turbOff = true; }
    if (b % 12 === 8) s.turbClear = true;
    if (x && x.growlRamp && b % 24 === 12) s.growl = r() * 0.9;
  });
  return steps;
}
const casos = [
  { name: 'upstream-defaults', seed: 12345, steps: guion(1, 94, false) },
  { name: 'cantante-a', seed: 777, steps: guion(2, 188, true) },
  { name: 'cantante-b', seed: -42, steps: guion(3, 188, true) },
  { name: 'x1-consonantes', seed: 501, steps: guionX(4, 188, null) },
  { name: 'x2-tracto-38', seed: 502, tract: 38, steps: guionX(5, 188, null) },
  { name: 'x2-tracto-52', seed: 503, tract: 52, steps: guionX(6, 188, null) },
  { name: 'x3-death', seed: 504, tract: 52, steps: guionX(7, 188, { growl: 0.85, jit: 0.6, shim: 0.48, air: 3 }) },
  { name: 'x3-aspereza', seed: -505, steps: guionX(8, 188, { jit: 0.25, shim: 0.2, air: 1.4, growlRamp: true }) },
  { name: 'todo-punk', seed: 506, tract: 40, steps: guionX(9, 188, { jit: 0.25, shim: 0.2, growl: 0.1, air: 0.8 }) },
];
fs.writeFileSync('casos.json', JSON.stringify(casos));
function render(c) {
  const pt = new MV.PinkTrombone(SR, c.seed, { tractLength: c.tract || 44 }); const out = new Float32Array(c.steps.length * BLOCK);
  const T = pt.tract, g = pt.glottis, sc = T.n / 44;
  pt.reset();
  c.steps.forEach((s, b) => {
    if ('alwaysVoice' in s) pt.alwaysVoice = s.alwaysVoice;
    if ('wobble' in s) pt.vibratoWobble = s.wobble;
    if ('pj' in s) pt.pitchJitterGain = s.pj; if ('tj' in s) pt.tensenessJitterGain = s.tj;
    if ('vib' in s) pt.vibratoGain = s.vib; if ('vibF' in s) pt.vibratoFrequency = s.vibF;
    if ('note' in s) pt.setMusicalNote(s.note); if ('touched' in s) pt.isTouched = s.touched;
    if ('loud' in s) pt.loudness = s.loud; if ('tens' in s) pt.targetTenseness = s.tens;
    if ('idx' in s) pt.setTongue(s.idx, s.dia);
    if ('growl' in s) g.x.sub = s.growl; if ('jit' in s) g.x.jitter = s.jit; if ('shim' in s) g.x.shimmer = s.shim; if ('air' in s) g.x.air = s.air;
    if ('constr' in s) pt.shaper.setConstrictions(s.constr.map(([index, diameter]) => ({ index, diameter })));
    if ('velum' in s) pt.shaper.velumOverride = s.velum;
    if (s.turbOn) T.turbulencePoints.push({ position: s.turbOn[0] * sc, diameter: s.turbOn[1], startTime: T.time, endTime: NaN });
    if (s.turbOff) for (const p of T.turbulencePoints) if (Number.isNaN(p.endTime)) p.endTime = T.time;
    if (s.turbClear) T.turbulencePoints = T.turbulencePoints.filter((p) => Number.isNaN(p.endTime));
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
