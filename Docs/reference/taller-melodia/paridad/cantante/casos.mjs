// [VOZ-2] Paridad del cantante v8: cantarV2 (motor-canto.js) = VoiceRenderer.cs.
// Genera casos.json (modos del taller × melodías × semillas), Cantante.cs los canta con las
// fuentes REALES (Assets/Scripts/Music/Voice/V8 + el fork) y este script compara:
//   1. fin de frase (regla D1 = B) idéntico,  2. sílabas idénticas (zigués, lista, texto),
//   3. audio con error < −60 dB.  Además mide la regla de fin de frase contra el finFrase real
//   del compositor del taller.
// Uso:  node casos.mjs && dotnet run -c Release && node casos.mjs comparar
import { createRequire } from 'module'; import fs from 'fs';
const require = createRequire(import.meta.url);
const MC = require('../../motor-canto.js');
const f = Math.fround;

const MEL = { tonica: 60, escala: 'Mayor (jónico)', progresion: 'I–V–vi–IV', bpm: 100, compases: 8, frase: 4, figura: 'Zig-zag', sube: 2, baja: 1, rango: 1.5, direccion: 'sube', alTope: 'rebota', ritmo: 'celdas', densidad: 0.45, sincopa: 0.3, silencios: 0.1, swing: 0, variacion: 0.1, postSalto: true, acordeFuerte: true, cadencia: true, repetir: 0.05, crescendo: 0.5, firma: true, semilla: 7, periodo: 8, pedal: 'tonica' };

// ---- la regla del juego (D1 = B), igual que SyllablePlanner.MarkPhraseEnds ----
function marcarFinDeFrase(notas, sb, cae, rejilla) {
  const eps = 0.05, grid = rejilla > 0 ? rejilla * 4 : 0;
  notas.forEach((n, i) => {
    const last = i === notas.length - 1;
    const gap = last ? Infinity : (notas[i + 1].start - n.end) / sb;
    let fin = last || gap >= 1 - eps || n.dur >= 2 - eps;
    if (fin && !last && grid > 0) { let r = (n.startBeats + n.dur) % grid; if (r < 0) r += grid; fin = r > grid - 1 - eps || r < eps; }
    n.finFrase = fin; n.caida = fin && cae && n.dur >= 1 - eps;
  });
}

// ---- voz del taller -> VoiceV8Params (nombres C#), con fround para que los dos lados vean el mismo float
function aParams(V) {
  return {
    Looseness: f(V.looseness), VibratoDepth: f(V.vibratoDepth), VibratoSpeedHz: f(V.vibratoSpeedHz), VibratoDelay: f(V.vibratoDelay),
    VibratoRamp: f(V.vibratoRamp), Brightness: f(V.brightness), Transpose: V.transpose, TensLo: f(V.tensLo), TensHi: f(V.tensHi),
    MinLoudness: f(V.minLoudness), PitchLead: f(V.pitchLead), TractLength: Math.round(V.tracto), Breath: f(V.aire), Growl: f(V.growl),
    Roughness: f(V.aspereza), Drive: f(V.saturacion), Scoop: f(V.scoop), ScoopMs: f(V.scoopMs), Fall: f(V.caida), GlideMs: f(V.glideMs),
    Attack: f(V.ataque), Legato: f(V.legato), BaseVowel: V.vocalBase,
  };
}
function deParams(P) {
  return {
    looseness: P.Looseness, vibratoDepth: P.VibratoDepth, vibratoSpeedHz: P.VibratoSpeedHz, vibratoDelay: P.VibratoDelay, vibratoRamp: P.VibratoRamp,
    brightness: P.Brightness, transpose: P.Transpose, tensLo: P.TensLo, tensHi: P.TensHi, minLoudness: P.MinLoudness, pitchLead: P.PitchLead,
    tracto: P.TractLength, aire: P.Breath, growl: P.Growl, aspereza: P.Roughness, saturacion: P.Drive, scoop: P.Scoop, scoopMs: P.ScoopMs,
    caida: P.Fall, glideMs: P.GlideMs, ataque: P.Attack, legato: P.Legato, vocalBase: P.BaseVowel,
  };
}

// ---- notas del juego: lo que trae el stem (sin finFrase, sin firma)
function notasDe(mel, bpm) {
  const sb = 60 / bpm;
  return { sb, notas: mel.notas.map((n) => ({ start: n.beat * sb, end: (n.beat + n.dur) * sb - 0.001, note: n.midi, vel: n.vel, startBeats: n.beat, durBeats: n.dur })) };
}

const CASOS = [
  // [nombre, modo, figura, semilla melodía, sílabas, semilla ruido]
  ['base-zigues', null, 'Zig-zag', 7, 'zigues', 11],
  ['tierno-zigues', 'Tierno y calmado', 'Ola', 3, 'zigues', 12],
  ['pop-lista', 'Pop', 'Escalera', 5, 'lista', 13],
  ['punk-zigues', 'Punk rock', 'Saltos', 9, 'zigues', -14],
  ['death-zigues', 'Death metal', 'Zig-zag', 21, 'zigues', 15],
  ['opera-lista', 'Ópera', 'Secuencia', 4, 'lista', 16],
  ['crooner-zigues', 'Crooner', 'Paseo', 8, 'zigues', 17],
  ['falsete-lista', 'Falsete', 'Pedal', 2, 'lista', 18],
  ['robot-texto', 'Robot', 'Escalera', 6, 'texto', 19],
  ['borracho-lista', 'Borracho', 'Ola', 12, 'lista', 20],
  ['riseup-zigues', null, 'Rise Up (exacto)', 1, 'zigues', 21],
  ['pop-zigues-rejilla2', 'Pop', 'Zig-zag', 14, 'zigues', 22, 2],
  ['death-lista-rejilla4', 'Death metal', 'Saltos', 15, 'lista', 23, 4],
];

function construir() {
  return CASOS.map(([name, modo, figura, semilla, silMode, seed, rejilla = 0]) => {
    const V = MC.vozDeModo(modo);
    const mel = MC.componer(Object.assign({}, MEL, { figura, semilla }));
    const { notas } = notasDe(mel, MEL.bpm);
    return {
      name, seed, bpm: MEL.bpm, rejilla, p: aParams(V), notes: notas,
      sil: { mode: silMode, lexSeed: 4242, verseSeed: semilla * 31 + 5, hardness: f(V.dureza), text: silMode === 'texto' ? 'zi ga, bu-ra kosh mm' : '',
        bank: (V.silabas || []).map((b) => [b[0], b[1], b[2] === 'fin']) },
    };
  });
}

function cantarJS(c) {
  const P = c.p, V = deParams(P), sb = 60 / c.bpm;
  const notas = c.notes.map((n) => ({ start: n.start, end: n.end, note: n.note, vel: n.vel, dur: n.durBeats, startBeats: n.startBeats }));
  marcarFinDeFrase(notas, sb, P.Fall > 0, c.rejilla);
  let lexico = null;
  if (c.sil.mode === 'zigues') lexico = MC.asignarZigues(notas, c.sil.lexSeed, c.sil.hardness, c.sil.verseSeed).lexico;
  else if (c.sil.mode === 'texto') MC.asignarSilabas(notas, [], c.sil.verseSeed, c.sil.text);
  else MC.asignarSilabas(notas, c.sil.bank.map(([s, w, e]) => (e ? [s, w, 'fin'] : [s, w])), c.sil.verseSeed, '');
  const dur = notas[notas.length - 1].end + 1;
  const r = MC.cantarV2(notas, V, { seed: c.seed, duracion: dur });
  return { audio: r.audio, sil: notas.map((n) => n.sil), fin: notas.map((n) => n.finFrase), caida: notas.map((n) => !!n.caida), lexico };
}

// ---- la regla D1 = B contra el finFrase real del compositor del taller
function medirRegla() {
  const FIG = ['Escalera', 'Zig-zag', 'Saltos', 'Ola', 'Pedal', 'Secuencia', 'Paseo'];
  let vp = 0, fp = 0, fn = 0, total = 0;
  for (const figura of FIG) for (let s = 1; s <= 30; s++) for (const densidad of [0.25, 0.45, 0.7]) {
    const mel = MC.componer(Object.assign({}, MEL, { figura, semilla: s, densidad }));
    const reales = mel.notas.filter((n) => !n.firma);
    const { sb, notas } = notasDe({ notas: mel.notas }, MEL.bpm);
    const ns = notas.map((n) => ({ start: n.start, end: n.end, dur: n.durBeats, startBeats: n.startBeats }));
    marcarFinDeFrase(ns, sb, false, 0);
    mel.notas.forEach((n, i) => {
      if (n.firma) return;
      total++;
      if (n.finFrase && ns[i].finFrase) vp++; else if (!n.finFrase && ns[i].finFrase) fp++; else if (n.finFrase && !ns[i].finFrase) fn++;
    });
    void reales;
  }
  const prec = vp / Math.max(1, vp + fp), rec = vp / Math.max(1, vp + fn);
  return { total, vp, fp, fn, precision: prec, recall: rec };
}

if (process.argv[2] === 'bloques') {
  // Depuración: primer bloque de 256 donde JS y C# se separan, con la nota que suena.
  const c = JSON.parse(fs.readFileSync('casos.json', 'utf8')).find((x) => x.name === process.argv[3]);
  const js = cantarJS(c);
  const b = fs.readFileSync(`out/${c.name}.f32`); const a = new Float32Array(b.buffer, b.byteOffset, b.length / 4);
  for (let k = 0; k * 256 < a.length; k++) {
    let d = 0; for (let i = k * 256; i < (k + 1) * 256; i++) d = Math.max(d, Math.abs(a[i] - js.audio[i]));
    if (d > 1e-5) {
      const t = k * 256 / 48000, i = c.notes.findIndex((n) => n.end > t);
      console.log(`bloque ${k}, t=${t.toFixed(4)} s, dif ${d.toExponential(2)}, nota ${i}:`, c.notes[i], 'sil', js.sil[i], 'fin', js.fin[i], 'caida', js.caida[i]);
      break;
    }
  }
  process.exit(0);
}
if (process.argv[2] === 'comparar') {
  const casos = JSON.parse(fs.readFileSync('casos.json', 'utf8'));
  let ok = true;
  const filas = [];
  for (const c of casos) {
    const js = cantarJS(c);
    const cs = JSON.parse(fs.readFileSync(`out/${c.name}.json`, 'utf8'));
    const silOk = JSON.stringify(js.sil) === JSON.stringify(cs.sil);
    const finOk = JSON.stringify(js.fin) === JSON.stringify(cs.fin) && JSON.stringify(js.caida) === JSON.stringify(cs.caida);
    const lexOk = js.lexico == null || JSON.stringify(js.lexico) === JSON.stringify(cs.lexico);
    const b = fs.readFileSync(`out/${c.name}.f32`); const a = new Float32Array(b.buffer, b.byteOffset, b.length / 4);
    let maxd = 0, se = 0, sd = 0, same = 0, pk = 0;
    const n = Math.min(js.audio.length, a.length);
    for (let i = 0; i < n; i++) { const d = Math.abs(js.audio[i] - a[i]); maxd = Math.max(maxd, d); pk = Math.max(pk, Math.abs(a[i])); se += a[i] * a[i]; sd += d * d; if (js.audio[i] === a[i]) same++; }
    const db = sd === 0 ? -Infinity : 10 * Math.log10(sd / se);
    const lenOk = js.audio.length === a.length;
    const caseOk = silOk && finOk && lexOk && lenOk && db < -60;
    if (!caseOk) ok = false;
    let rms = 0, cnt = 0; for (let i = 0; i < a.length; i++) if (Math.abs(a[i]) > 1e-4) { rms += a[i] * a[i]; cnt++; }
    rms = Math.sqrt(rms / Math.max(1, cnt));
    filas.push({ caso: c.name, muestras: n, idénticas: `${(100 * same / n).toFixed(1)} %`, errorDb: db.toFixed(1), difMax: maxd.toExponential(2), pico: pk.toFixed(3), rms: rms.toFixed(4), silabas: silOk ? 'ok' : 'DIF', finFrase: finOk ? 'ok' : 'DIF', lexico: lexOk ? 'ok' : 'DIF', largo: lenOk ? 'ok' : `${js.audio.length}≠${a.length}` });
    if (!silOk) { console.log(`  ${c.name} JS: ${js.sil.slice(0, 12).join(' ')}\n  ${c.name} C#: ${cs.sil.slice(0, 12).join(' ')}`); }
  }
  console.table(filas);
  const regla = medirRegla();
  console.log(`Regla de fin de frase (D1 = B) contra el compositor del taller: ${regla.total} notas, precisión ${(100 * regla.precision).toFixed(1)} %, exhaustividad ${(100 * regla.recall).toFixed(1)} % (VP ${regla.vp}, FP ${regla.fp}, FN ${regla.fn})`);
  fs.writeFileSync('resultados.json', JSON.stringify({ fecha: new Date().toISOString().slice(0, 10), filas, regla }, null, 1));
  console.log(ok ? 'PARIDAD OK (fin de frase y sílabas idénticos, audio < −60 dB)' : 'PARIDAD FALLA');
  process.exit(ok ? 0 : 1);
} else {
  fs.writeFileSync('casos.json', JSON.stringify(construir()));
  console.log('casos.json escrito; ahora: dotnet run -c Release && node casos.mjs comparar');
}
