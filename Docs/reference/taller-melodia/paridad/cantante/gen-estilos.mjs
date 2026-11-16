// [VOZ-2 / D5 = A] Tabla de estilos del taller para el menú de Unity (VoiceStylesMenu.cs) con su
// gain calibrado: cada modo se canta sobre la misma melodía de referencia (zigués de Zig con la
// dureza del modo) y su RMS se iguala al de la Zig de hoy en el juego (v7 + dicción, cantarJuego).
// Uso: node gen-estilos.mjs  → escribe estilos.json y la tabla C# a pegar en VoiceStylesMenu.cs.
import { createRequire } from 'module'; import fs from 'fs';
const require = createRequire(import.meta.url);
const MC = require('../../motor-canto.js');

const MEL = { tonica: 60, escala: 'Mayor (jónico)', progresion: 'I–V–vi–IV', bpm: 100, compases: 8, frase: 4, figura: 'Zig-zag', sube: 2, baja: 1, rango: 1.5, direccion: 'sube', alTope: 'rebota', ritmo: 'celdas', densidad: 0.45, sincopa: 0.3, silencios: 0.1, swing: 0, variacion: 0.1, postSalto: true, acordeFuerte: true, cadencia: true, repetir: 0.05, crescendo: 0.5, firma: false, semilla: 7, periodo: 8, pedal: 'tonica' };
const LEX = 4242;   // el idioma de Zig (SyllableBankSO.lexiconSeed por defecto)

function rmsActivo(a) { let s = 0, c = 0; for (const x of a) if (Math.abs(x) > 1e-4) { s += x * x; c++; } return Math.sqrt(s / Math.max(1, c)); }
function notasRef() {
  const mel = MC.componer(MEL), sb = 60 / MEL.bpm;
  const notas = MC.aSegundos(mel, MEL.bpm).map((n, i) => Object.assign(n, { dur: mel.notas[i].dur }));
  notas.forEach((n, i) => {   // regla D1 = B
    const last = i === notas.length - 1, gap = last ? Infinity : (notas[i + 1].start - n.end) / sb;
    n.finFrase = last || gap >= 0.95 || n.dur >= 1.95;
  });
  return { mel, notas };
}

// referencia: Zig hoy en el juego (v7 + dicción, el prefab desde S9c)
const { mel, notas: n0 } = notasRef();
const ctx = { beatSeconds: 60 / MEL.bpm, beatsPerBar: 4, rootPc: 0, scalePcs: [0, 2, 4, 5, 7, 9, 11] };
const ref = MC.cantarJuego(n0.map((n) => Object.assign({}, n)), MC.PERFIL_ZIG, { diccion: true, ctx });
const rmsZig = rmsActivo(ref.audio);
console.log(`RMS de Zig hoy (v7 + dicción): ${rmsZig.toFixed(4)}`);

const salida = [];
const modos = [['Zig V8', null], ...Object.keys(MC.MODOS).filter((k) => MC.MODOS[k].v && !MC.MODOS[k].soloTaller)   // TALLER-MEL-A: los modos «solo taller» no van al juego hasta VOZ-2b.map((k) => [k, k])];
for (const [nombre, modo] of modos) {
  const V = MC.vozDeModo(modo);
  const { notas } = notasRef();
  for (const n of notas) n.caida = n.finFrase && V.caida > 0 && n.dur >= 0.95;
  MC.asignarZigues(notas, LEX, V.dureza, 12345);
  const r = MC.cantarV2(notas, V, { seed: 1 });
  const rms = rmsActivo(r.audio);
  let pk = 0; for (const x of r.audio) pk = Math.max(pk, Math.abs(x));
  const gain = Math.min(2, rmsZig / rms);   // VoiceProfileSO.gain va de 0 a 2
  salida.push({ nombre, modo, V, rms, gain, picoConGain: pk * gain });
  console.log(`${nombre.padEnd(18)} RMS ${rms.toFixed(4)}  gain ${gain.toFixed(3)}  pico con gain ${(pk * gain).toFixed(3)}`);
}
fs.writeFileSync('estilos.json', JSON.stringify({ rmsZig, salida }, null, 1));

// tabla C#
const fl = (x) => `${+x.toFixed(4)}f`;
const vow = { 'ə': 'Neutral', a: 'A', e: 'E', i: 'I', o: 'O', u: 'U' };
const lineas = salida.map(({ nombre, V, gain }) =>
  `            new Style("${nombre}", ${fl(V.dureza)}, ${fl(gain)}, p => { p.looseness = ${fl(V.looseness)}; p.vibratoDepth = ${fl(V.vibratoDepth)}; p.vibratoSpeedHz = ${fl(V.vibratoSpeedHz)}; p.vibratoDelaySeconds = ${fl(V.vibratoDelay)}; p.vibratoRampSeconds = ${fl(V.vibratoRamp)}; p.brightness = ${fl(V.brightness)}; p.transposeSemitones = ${V.transpose}; p.tensenessAtVel0 = ${fl(V.tensLo)}; p.tensenessAtVel127 = ${fl(V.tensHi)}; p.minLoudness = ${fl(V.minLoudness)}; p.pitchLeadSeconds = ${fl(V.pitchLead)}; p.tractLength = ${Math.round(V.tracto)}; p.breath = ${fl(V.aire)}; p.growl = ${fl(V.growl)}; p.roughness = ${fl(V.aspereza)}; p.drive = ${fl(V.saturacion)}; p.scoopSemitones = ${fl(V.scoop)}; p.scoopMs = ${fl(V.scoopMs)}; p.fallSemitones = ${fl(V.caida)}; p.glideMs = ${fl(V.glideMs)}; p.attackPress = ${fl(V.ataque)}; p.legato = ${fl(V.legato)}; p.baseVowel = VoiceProfileSO.BaseVowel.${vow[V.vocalBase] || 'Neutral'}; },\n` +
  `                new (string, float, bool)[] { ${(V.silabas || []).map((s) => `("${s[0]}", ${fl(s[1])}, ${s[2] === 'fin' ? 'true' : 'false'})`).join(', ')} }),`);
fs.writeFileSync('estilos-tabla.cs.txt', lineas.join('\n') + '\n');
console.log('estilos.json y estilos-tabla.cs.txt escritos');
