// Mide F1/F2 de cada vocal del motor (LPC sobre una vocal sostenida a 100 Hz) y los compara
// con Peterson y Barney (1952), hombres. Uso: node formantes.mjs
import { createRequire } from 'module'; const require = createRequire(import.meta.url);
const MV = require('../motor-voz.js');
const SR = 48000;
export function medir(idx, dia, constr = [], n = 44) {
  const pt = new MV.PinkTrombone(SR, 99, { tractLength: n });
  pt.alwaysVoice = false; pt.isTouched = true; pt.pitchJitterGain = 0; pt.tensenessJitterGain = 0; pt.vibratoWobble = false; pt.vibratoGain = 0;
  pt.targetTenseness = 0.6; pt.setMusicalNote(-33 + 0.3); pt.setTongue(idx, dia); pt.shaper.setConstrictions(constr); pt.reset();
  const buf = new Float32Array(SR); pt.synthesize(buf);
  // decimate ×4 → 12 kHz (box filter), last 0.5 s, hamming
  const D = 4, seg = []; for (let i = SR / 2; i + D <= SR; i += D) { let s = 0; for (let k = 0; k < D; k++) s += buf[i + k]; seg.push(s / D); }
  const N = seg.length, x = seg.map((v, i) => v * (0.54 - 0.46 * Math.cos(2 * Math.PI * i / (N - 1))));
  for (let i = N - 1; i > 0; i--) x[i] -= 0.9 * x[i - 1];          // pre-emphasis
  const p = 14, R = []; for (let k = 0; k <= p; k++) { let s = 0; for (let i = k; i < N; i++) s += x[i] * x[i - k]; R.push(s); }
  let a = [1], e = R[0]; for (let i = 1; i <= p; i++) { let acc = R[i]; for (let j = 1; j < i; j++) acc += a[j] * R[i - j]; const k = -acc / e; const na = a.slice(); na[i] = k; for (let j = 1; j < i; j++) na[j] = a[j] + k * a[i - j]; a = na; e *= 1 - k * k; }
  const fs = SR / D, env = []; for (let fq = 0; fq <= 4000; fq += 10) { const w = 2 * Math.PI * fq / fs; let re = 0, im = 0; for (let k = 0; k <= p; k++) { re += a[k] * Math.cos(w * k); im -= a[k] * Math.sin(w * k); } env.push([fq, -10 * Math.log10(re * re + im * im)]); }
  const peaks = []; for (let i = 1; i < env.length - 1; i++) if (env[i][1] > env[i - 1][1] && env[i][1] > env[i + 1][1] && env[i][0] > 150) peaks.push(env[i][0]);
  return peaks.slice(0, 3);
}
if (process.argv[1].endsWith('formantes.mjs')) {
  const PB = { i: [270, 2290], ɪ: [390, 1990], e: [530, 1840], æ: [660, 1720], ɑ: [730, 1090], ɔ: [570, 840], ʊ: [440, 1020], u: [300, 870], ʌ: [640, 1190], ɝ: [490, 1350] };
  const V = [
    ['juego Neutral «uh»', 12.9, 2.43], ['juego Open «ah»', 14, 2.9], ['juego Front «eh/ee»', 27, 2.1], ['juego Back «oh»', 18, 3.2],
    ['zk æ', 14.93, 2.78, 'æ'], ['zk ɑ', 12.75, 2.3, 'ɑ'], ['zk ɔ', 17.7, 2.05, 'ɔ'], ['zk ɪ', 26.11, 2.87, 'ɪ'], ['zk i', 27.2, 2.2, 'i'],
    ['zk e', 19.4, 3.43, 'e'], ['zk ʌ', 17.8, 2.46, 'ʌ'], ['zk u', 22.8, 2.05, 'u'], ['zk ə', 20.7, 2.8, 'ɝ'],
  ];
  for (const [nm, i, d, pb] of V) { const fm = medir(i, d); console.log(nm.padEnd(22), 'F1', fm[0], 'F2', fm[1], pb ? `  (P&B ${PB[pb]})` : ''); }
  console.log('u + labios 0.7', medir(22.8, 2.05, [{ index: 41, diameter: 0.7 }]));
  console.log('ɔ + labios 0.9', medir(17.7, 2.05, [{ index: 41, diameter: 0.9 }]));
  for (const n of [36, 44, 52]) console.log('ɑ tracto', n, medir(12.75, 2.3, [], n));
}
