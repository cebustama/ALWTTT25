// [VOZ-2] Variantes de la regla de fin de frase contra el finFrase real del compositor del taller.
import { createRequire } from 'module';
const require = createRequire(import.meta.url);
const MC = require('../../motor-canto.js');
const MEL = { tonica: 60, escala: 'Mayor (jónico)', progresion: 'I–V–vi–IV', bpm: 100, compases: 8, frase: 4, figura: 'Zig-zag', sube: 2, baja: 1, rango: 1.5, direccion: 'sube', alTope: 'rebota', ritmo: 'celdas', densidad: 0.45, sincopa: 0.3, silencios: 0.1, swing: 0, variacion: 0.1, postSalto: true, acordeFuerte: true, cadencia: true, repetir: 0.05, crescendo: 0.5, firma: true, semilla: 7, periodo: 8, pedal: 'tonica' };
const eps = 0.05;
const REGLAS = {
  'A: silencio>=1 o última': (n, nx) => !nx || nx.beat - (n.beat + n.dur) >= 1 - eps,
  'B: A o dur>=2 (cerrada)': (n, nx) => !nx || nx.beat - (n.beat + n.dur) >= 1 - eps || n.dur >= 2 - eps,
  'C: silencio>=2 o última': (n, nx) => !nx || nx.beat - (n.beat + n.dur) >= 2 - eps,
  'D: (dur>=2 o silencio>=1) y termina en fin de compás o cruza el 4.º tiempo': (n, nx) => { const fin = n.beat + n.dur; const gapOk = !nx || nx.beat - fin >= 1 - eps; return (!nx) || ((gapOk || n.dur >= 2 - eps) && (fin % 4 > 3 - eps || fin % 4 < eps)); },
  'F: B y la frase cierra en compás par (fin de 2.º, 4.º… compás)': (n, nx) => { const fin = n.beat + n.dur; const cerca = (x, m) => { const r = ((x % m) + m) % m; return r > m - 1 - eps || r < eps; }; return !nx || ((nx.beat - fin >= 1 - eps || n.dur >= 2 - eps) && cerca(fin, 8)); },
  'G: B y cierra en el compás 4, 8…': (n, nx) => { const fin = n.beat + n.dur; const cerca = (x, m) => { const r = ((x % m) + m) % m; return r > m - 1 - eps || r < eps; }; return !nx || ((nx.beat - fin >= 1 - eps || n.dur >= 2 - eps) && cerca(fin, 16)); },
  'E: dur>=2 o última': (n, nx) => !nx || n.dur >= 2 - eps,
};
const FIG = ['Escalera', 'Zig-zag', 'Saltos', 'Ola', 'Pedal', 'Secuencia', 'Paseo'];
for (const [nombre, regla] of Object.entries(REGLAS)) {
  let vp = 0, fp = 0, fn = 0;
  for (const figura of FIG) for (let s = 1; s <= 30; s++) for (const densidad of [0.25, 0.45, 0.7]) for (const silencios of [0, 0.1, 0.3]) {
    const notas = MC.componer(Object.assign({}, MEL, { figura, semilla: s, densidad, silencios })).notas;
    notas.forEach((n, i) => {
      if (n.firma) return;
      const r = regla(n, notas[i + 1]);
      if (n.finFrase && r) vp++; else if (!n.finFrase && r) fp++; else if (n.finFrase && !r) fn++;
    });
  }
  console.log(`${nombre}: precisión ${(100 * vp / (vp + fp)).toFixed(1)} %, exhaustividad ${(100 * vp / (vp + fn)).toFixed(1)} %`);
}
