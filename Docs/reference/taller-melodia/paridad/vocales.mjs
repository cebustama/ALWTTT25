// Busca (lengua, labios) para las 5 vocales del español, voz masculina (valores medios de Quilis:
// a 700/1300, e 450/1900, i 300/2300, o 500/950, u 320/800). Uso: node vocales.mjs
import { medir } from './formantes.mjs';
const T = { a: [700, 1300], e: [450, 1900], i: [300, 2300], o: [500, 950], u: [320, 800] };
for (const [v, [t1, t2]] of Object.entries(T)) {
  let best = null;
  for (let idx = 10; idx <= 30; idx += 1) for (let dia = 1.8; dia <= 3.5; dia += 0.15) for (const lip of [null, 1.2, 0.9, 0.7, 0.5]) {
    const fm = medir(idx, dia, lip == null ? [] : [{ index: 41, diameter: lip }]);
    if (fm.length < 2) continue;
    const err = Math.abs(Math.log(fm[0] / t1)) + Math.abs(Math.log(fm[1] / t2));
    if (!best || err < best.err) best = { err, idx, dia: +dia.toFixed(2), lip, fm };
  }
  console.log(v, JSON.stringify(best));
}
