// Paridad del compositor por figuras (MGP-MEL-FIGURE-1): genera casos.json con componer(p) de
// motor-canto.js; Paridad.cs corre las mismas entradas por MelodyFigurePlanner.cs (el archivo REAL
// del paquete MidiGenPlay, compilado fuera de Unity) y compara nota a nota.
// Uso:  node casos.mjs && dotnet run -c Release
import { createRequire } from 'module'; import fs from 'fs';
const require = createRequire(import.meta.url);
const MC = require('../../motor-canto.js');

const DEF = { tonica: 60, escala: 'Mayor (jónico)', progresion: 'I–V–vi–IV', bpm: 100, compases: 8, frase: 4, figura: 'Zig-zag', sube: 2, baja: 1, rango: 1.5, direccion: 'sube', alTope: 'rebota', ritmo: 'celdas', densidad: 0.45, sincopa: 0.3, silencios: 0.1, swing: 0, variacion: 0.1, postSalto: true, acordeFuerte: true, cadencia: true, repetir: 0.05, crescendo: 0.5, firma: true, semilla: 7, periodo: 8, pedal: 'tonica' };
const FIGS = MC.FIGURAS.filter((f) => f !== 'Rise Up (exacto)');
const DIRS = ['sube', 'baja', 'arco', 'valle'];
const ESC = Object.keys(MC.ESCALAS);
const PROGS = Object.keys(MC.PROGRESIONES);
const SEEDS = [7, 1, 42, 1234, 99991, -5];

const r = MC.SR ? require('../../motor-voz.js').mulberry32(20261114) : null;
const pick = (arr) => arr[Math.floor(r() * arr.length)];
const step = (lo, hi, st) => +(lo + st * Math.floor(r() * (Math.round((hi - lo) / st) + 1))).toFixed(4);

const casos = [];
const add = (id, p) => {
  const m = MC.componer(p);
  casos.push({ id, p, prog: m.prog, esc: m.esc,
    notas: m.notas.map((n) => ({ beat: n.beat, dur: n.dur, grado: n.grado, midi: n.midi, vel: n.vel, bar: n.bar, fin: !!n.finFrase, firma: !!n.firma })) });
};

// 1) Catálogo base: figura × dirección × escala × semilla (parámetros por defecto, progresión rotando).
let i = 0;
for (const figura of FIGS) for (const direccion of DIRS) for (const escala of ESC) for (const semilla of SEEDS)
  add(`base-${i}`, { ...DEF, figura, direccion, escala, semilla, progresion: PROGS[i++ % PROGS.length] });

// 2) Barridos: todos los controles al azar dentro de los rangos de la página.
for (let k = 0; k < 2400; k++) {
  add(`barrido-${k}`, { ...DEF,
    figura: pick(FIGS), direccion: pick(DIRS), escala: pick(ESC), progresion: pick(PROGS),
    tonica: 48 + Math.floor(r() * 25), compases: pick([4, 8, 8, 16]), frase: pick([2, 4]),
    sube: step(1, 5, 1), baja: step(0, 4, 1), rango: step(0.5, 2, 0.25), alTope: pick(['rebota', 'octava']),
    ritmo: r() < 0.15 ? 'riseup' : 'celdas', densidad: step(0, 1, 0.05), sincopa: step(0, 1, 0.05),
    silencios: step(0, 0.5, 0.05), swing: r() < 0.4 ? 0 : step(0, 1, 0.05), variacion: step(0, 0.6, 0.05),
    crescendo: step(0, 1, 0.05), repetir: pick([0, 0.05, 0.1, 0.3]),
    postSalto: r() < 0.7, acordeFuerte: r() < 0.7, cadencia: r() < 0.7, firma: r() < 0.6,
    periodo: pick([4, 5, 6, 8, 10, 12, 16]), pedal: pick(['tonica', 'quinta']),
    semilla: Math.floor(r() * 200000) - 100000 });
}

fs.writeFileSync(new URL('./casos.json', import.meta.url), JSON.stringify(casos));
const notas = casos.reduce((a, c) => a + c.notas.length, 0);
console.log(`casos.json: ${casos.length} casos, ${notas} notas.`);
