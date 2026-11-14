/* motor-canto.js — melodías, sílabas y los dos cantantes del Taller de melodías.
 *
 *  cantarJuego(notas, perfil, opts)  Port de SingerVoice.cs v7 (ALWTTT) tal como corre en el gig:
 *                                    characterEnabled = true (ApplyProfile), bloque de 1024 muestras,
 *                                    tone stage de 2 polos, expressivityEnabled = false (prefab
 *                                    Zig_SingerVoice). opts.diccion = true enciende el C-lite M1–M3.
 *  cantarV2(notas, voz, opts)        Propuesta: sílabas con consonantes (X1), longitud del tracto (X2),
 *                                    growl/aspereza/aire (X3), scoop, caída, glide, ataque, saturación.
 *  componer(p)                       Melodía por figura (escalera, zig-zag, saltos, ola, pedal,
 *                                    secuencia, paseo) + ritmo por celdas + reglas (post-salto,
 *                                    acorde en tiempos fuertes, cadencia, firma).
 * Requiere motor-voz.js (MV). Sin DOM.
 */
(function (root) {
  'use strict';
  const MV = root.MV || (typeof require !== 'undefined' ? require('./motor-voz.js') : null);
  const SR = 48000;
  const clamp = (v, a, b) => Math.min(b, Math.max(a, v));
  const clamp01 = (v) => clamp(v, 0, 1);
  const lerp = (a, b, t) => a + (b - a) * t;

  // ======================================================================= escalas y progresiones
  const ESCALAS = {
    'Mayor (jónico)': [0, 2, 4, 5, 7, 9, 11], 'Menor natural (eólico)': [0, 2, 3, 5, 7, 8, 10],
    'Dórico': [0, 2, 3, 5, 7, 9, 10], 'Frigio': [0, 1, 3, 5, 7, 8, 10], 'Lidio': [0, 2, 4, 6, 7, 9, 11],
    'Mixolidio': [0, 2, 4, 5, 7, 9, 10], 'Locrio': [0, 1, 3, 5, 6, 8, 10],
    'Menor armónica': [0, 2, 3, 5, 7, 8, 11], 'Menor melódica': [0, 2, 3, 5, 7, 9, 11],
    'Frigio dominante': [0, 1, 4, 5, 7, 8, 10], 'Húngara menor': [0, 2, 3, 6, 7, 8, 11],
    'Pentatónica mayor': [0, 2, 4, 7, 9], 'Pentatónica menor': [0, 3, 5, 7, 10], 'Blues': [0, 3, 5, 6, 7, 10],
    'Hirajoshi': [0, 2, 3, 7, 8], 'Tonos enteros': [0, 2, 4, 6, 8, 10],
  };
  const EN_EL_JUEGO = new Set(['Mayor (jónico)', 'Menor natural (eólico)', 'Dórico', 'Frigio', 'Lidio', 'Mixolidio', 'Locrio']);
  const PROGRESIONES = { 'I–V–vi–IV': [0, 4, 5, 3], 'I–vi–IV–V': [0, 5, 3, 4], 'I–IV–V–I': [0, 3, 4, 0], 'i–VI–III–VII': [0, 5, 2, 6], 'ii–V–I–I': [1, 4, 0, 0], 'Pedal (solo I)': [0, 0, 0, 0] };

  const hash = (s) => { let h = 2166136261 >>> 0; for (const c of String(s)) { h ^= c.charCodeAt(0); h = Math.imul(h, 16777619) >>> 0; } return h | 0; };
  const rngDe = (seed, sal) => MV.mulberry32((seed | 0) ^ hash(sal));
  const elegir = (r, items, pesos) => { let t = 0; for (const w of pesos) t += w; let x = r() * t; for (let i = 0; i < items.length; i++) { x -= pesos[i]; if (x <= 0) return items[i]; } return items[items.length - 1]; };

  function gradoAMidi(esc, tonica, g) {
    const L = esc.length, oct = Math.floor(g / L), k = ((g % L) + L) % L;
    return tonica + 12 * oct + esc[k];
  }
  /** Notas del acorde (en grados) del compás: tríada por terceras en la escala; si la escala no es de 7, raíz y quinta. */
  function acordeEnGrados(esc, raizGrado) {
    if (esc.length === 7) return [raizGrado, raizGrado + 2, raizGrado + 4];
    // raíz + el grado más cercano a una quinta justa
    let best = 0, bd = 99; for (let k = 1; k < esc.length; k++) { const d = Math.abs(esc[k] - 7); if (d < bd) { bd = d; best = k; } }
    return [raizGrado, raizGrado + best];
  }

  // ======================================================================= ritmo
  // Celdas de un compás de 4/4 en negras. d = densidad, s = síncopa.
  const CELDAS = [
    { c: [4], d: 0.0 }, { c: [2, 2], d: 0.2 }, { c: [3, 1], d: 0.25 }, { c: [1, 1, 2], d: 0.4 }, { c: [2, 1, 1], d: 0.4 },
    { c: [1.5, 1.5, 1], d: 0.45, s: 1 }, { c: [1, 1, 1, 1], d: 0.55 }, { c: [0.5, 1, 0.5, 2], d: 0.6, s: 1 },
    { c: [1, 0.5, 0.5, 1, 1], d: 0.7 }, { c: [0.5, 1, 1, 1, 0.5], d: 0.72, s: 1 }, { c: [0.5, 0.5, 0.5, 0.5, 1, 1], d: 0.8 },
    { c: [0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5], d: 1.0 }, { c: [0.75, 0.75, 0.5, 0.75, 0.75, 0.5], d: 0.85, s: 1 },
  ];
  function ritmo(p, r) {
    const out = [];
    for (let b = 0; b < p.compases; b++) {
      const finFrase = (b % p.frase) === p.frase - 1;
      let cel;
      if (p.ritmo === 'riseup') cel = [3, 1];
      else if (finFrase && p.cadencia) cel = p.densidad > 0.6 ? [1, 1, 2] : (p.densidad > 0.3 ? [2, 2] : [4]);
      else {
        const pesos = CELDAS.map((x) => Math.exp(-((x.d - p.densidad) ** 2) / 0.03) * (x.s ? 0.1 + 2 * p.sincopa : 1 - 0.6 * p.sincopa));
        cel = elegir(r, CELDAS, pesos).c;
      }
      let t = 0;
      cel.forEach((dur, i) => {
        const beat = b * 4 + t;
        const silencio = i > 0 && !(finFrase && i === cel.length - 1) && r() < p.silencios;
        let start = beat;
        if (p.swing > 0 && Math.abs((t % 1) - 0.5) < 1e-6) start += p.swing / 6;  // corchea a contratiempo
        if (!silencio) out.push({ beat: start, dur: Math.max(0.25, dur - (p.swing > 0 && dur === 0.5 ? p.swing / 6 : 0)), bar: b, enBar: t, finFrase: finFrase && i === cel.length - 1 });
        t += dur;
      });
    }
    return out;
  }

  // ======================================================================= figuras
  const FIGURAS = ['Rise Up (exacto)', 'Escalera', 'Zig-zag', 'Saltos', 'Ola', 'Pedal', 'Secuencia', 'Paseo'];

  function componer(p) {
    const esc = ESCALAS[p.escala] || ESCALAS['Mayor (jónico)'];
    const L = esc.length, r = rngDe(p.semilla, 'mel'), rr = rngDe(p.semilla, 'rit');
    const prog = PROGRESIONES[p.progresion] || PROGRESIONES['I–V–vi–IV'];
    const acordeDe = (bar) => acordeEnGrados(esc, prog[bar % prog.length]);
    const tonica = p.tonica;
    if (p.figura === 'Rise Up (exacto)') {
      // MelodyPattern_RiseUp_44_8m.asset, tal cual (grado, inicio, duración, velocidad)
      const RU = [[0, 0, 3, 92], [1, 3, 1, 90], [1, 4, 3, 96], [2, 7, 1, 94], [2, 8, 3, 100], [3, 11, 1, 98], [3, 12, 3, 104], [4, 15, 1, 102], [4, 16, 3, 108], [5, 19, 1, 100], [5, 20, 3, 112], [6, 23, 1, 110], [6, 24, 3, 116], [7, 27, 1, 114], [7, 28, 4, 120]];
      const notas = RU.map(([g, s, d, v]) => ({ beat: s, dur: d, grado: g, midi: gradoAMidi(esc, tonica, g), vel: v, bar: Math.floor(s / 4), finFrase: g === 7 && d === 4 }));
      return { notas, compases: 8, prog, esc };
    }
    const ritmos = ritmo(p, rr);
    const N = ritmos.length;
    const top = Math.round(L * p.rango), bottom = p.direccion === 'baja' || p.direccion === 'valle' ? -Math.round(L * p.rango / 2) : 0;
    let g = p.direccion === 'baja' ? top : (p.direccion === 'valle' ? 0 : 0);
    let dir = (p.direccion === 'baja' || p.direccion === 'valle') ? -1 : 1;
    let prevMove = 0, kk = 0;
    const motivo = [0, 1, -1, 2].map((x, i) => (i === 0 ? 0 : x * (r() < 0.5 ? 1 : -1)));
    for (let i = 1; i < 4; i++) motivo[i] = [1, 2, -1, 1, -2, 0][Math.floor(r() * 6)];
    const notas = [];
    const anclaPedal = p.pedal === 'quinta' ? Math.min(4, L - 1) : 0;
    let linea = 0;
    for (let i = 0; i < N; i++) {
      const rt = ritmos[i];
      const prog01 = i / Math.max(1, N - 1);
      if (p.direccion === 'arco') dir = prog01 < 0.5 ? 1 : -1;
      if (p.direccion === 'valle') dir = prog01 < 0.5 ? -1 : 1;
      let mov = 0;
      if (i > 0) {
        const k = kk - 1;
        switch (p.figura) {
          case 'Escalera': mov = dir * p.sube; break;
          case 'Zig-zag': mov = (k % 2 === 0) ? dir * p.sube : -dir * p.baja; break;
          case 'Saltos': mov = dir * (r() < 0.25 ? 4 : 2); if (r() < 0.3) mov = -mov / 2 | 0 || -dir; break;
          case 'Ola': { const per = Math.max(4, p.periodo || 8), amp = Math.max(1, (p.sube + p.baja)); const tgt = Math.round(amp * Math.sin(2 * Math.PI * k / per) + dir * p.sube * k / per); mov = tgt - g; break; }
          case 'Pedal': if (k % 2 === 1) { mov = anclaPedal - g; } else { linea += dir * p.sube; if (anclaPedal + linea > top || anclaPedal + linea < bottom) { dir = -dir; linea += 2 * dir * p.sube; } mov = (anclaPedal + linea) - g; } break;
          case 'Secuencia': { const m = motivo.length, rep = Math.floor(k / m), j = k % m; let off = 0; for (let q = 1; q <= j; q++) off += motivo[q]; mov = rep * dir * p.sube + off - g; break; }
          case 'Paseo': default: { const opts = [-2, -1, 0, 1, 2]; const w = opts.map((s) => (s === 0 ? 0.4 : 1 / Math.abs(s)) * (Math.sign(s) === dir ? 1 + p.sube * 0.5 : 1)); mov = elegir(r, opts, w); }
        }
        // variación: la figura no es una regla de hierro
        if (p.variacion > 0 && r() < p.variacion) mov += r() < 0.5 ? 1 : -1;
        // regla post-salto (von Hippel y Huron 2000): tras un salto de 3+ grados, paso en contra
        if (p.postSalto && Math.abs(prevMove) >= 3 && Math.sign(mov) === Math.sign(prevMove)) mov = -Math.sign(prevMove);
        if (p.repetir > 0 && r() < p.repetir) mov = 0;
      }
      let ng = g + mov;
      // rango
      if (ng > top) { if (p.alTope === 'octava') ng -= L; else { dir = -1; ng = Math.min(top, g) - 1; } }
      if (ng < bottom - 1) { if (p.alTope === 'octava') ng += L; else { dir = 1; ng = Math.max(bottom - 1, g) + 1; } }
      // nota del acorde en tiempos fuertes
      const fuerte = Math.abs(rt.enBar % 2) < 1e-6;
      if (p.acordeFuerte && fuerte) {
        const ac = acordeDe(rt.bar);
        let best = ng, bd = 99;
        for (let o = -2; o <= 2; o++) for (const a of ac) { const cand = a + o * L; const d = Math.abs(cand - ng); if (d < bd) { bd = d; best = cand; } }
        if (bd <= 1) ng = best;
      }
      // cadencia al cerrar frase: última frase a la tónica, las demás a la quinta o la segunda
      if (p.cadencia && rt.finFrase) {
        const ultima = rt.bar === p.compases - 1;
        const meta = ultima ? 0 : (r() < 0.6 ? Math.min(4, L - 1) : 1);
        let best = ng, bd = 99; for (let o = -2; o <= 2; o++) { const cand = meta + o * L; const d = Math.abs(cand - ng); if (d < bd) { bd = d; best = cand; } }
        ng = best;
      }
      prevMove = ng - g; g = ng; kk++;
      const accent = Math.abs(rt.enBar) < 1e-6 ? 1 : (Math.abs(rt.enBar - 2) < 1e-6 ? 0.6 : 0.3);
      const vel = clamp(Math.round(78 + 22 * accent + p.crescendo * 18 * ((rt.bar % p.frase) / Math.max(1, p.frase - 1)) + (rt.finFrase ? 6 : 0) + (r() * 8 - 4)), 40, 127);
      notas.push({ beat: rt.beat, dur: rt.dur, grado: g, midi: gradoAMidi(esc, tonica, g), vel, bar: rt.bar, finFrase: rt.finFrase });
    }
    // firma de Ziggy: vecina superior rápida antes del cierre de cada frase + caída al final
    if (p.firma) {
      const extra = [];
      for (const n of notas) if (n.finFrase && n.dur >= 1) {
        extra.push({ beat: n.beat - 0.25, dur: 0.25, grado: n.grado + 1, midi: gradoAMidi(esc, tonica, n.grado + 1), vel: Math.min(127, n.vel + 6), bar: n.bar, firma: true });
        n.caida = true;
      }
      for (const e of extra) {
        // D-MELFIG-12 (2026-11-14): la nota previa real (no otra firma), recortada justo hasta la firma.
        const prev = notas.filter((n) => !n.firma && n.beat < e.beat).pop();
        if (prev && prev.beat + prev.dur > e.beat) prev.dur = e.beat - prev.beat;
        notas.push(e);
      }
      notas.sort((a, b) => a.beat - b.beat);
    }
    return { notas, compases: p.compases, prog, esc };
  }

  // ======================================================================= sílabas
  // Lugar de articulación en el índice del tracto (44 secciones), de la interfaz original de Pink
  // Trombone y de la tabla de zakaton/pink-trombone-midi. d = diámetro (0 = cierre).
  const CONS = {
    b: { i: 41, d: 0, sonora: 1, tipo: 'ocl' }, p: { i: 41, d: 0, sonora: 0, tipo: 'ocl' },
    d: { i: 36, d: 0, sonora: 1, tipo: 'ocl' }, t: { i: 36, d: 0, sonora: 0, tipo: 'ocl' },
    g: { i: 21, d: 0, sonora: 1, tipo: 'ocl' }, k: { i: 21, d: 0, sonora: 0, tipo: 'ocl' },
    m: { i: 41, d: 0, sonora: 1, tipo: 'nas' }, n: { i: 36, d: 0, sonora: 1, tipo: 'nas' },
    s: { i: 36, d: 0.55, sonora: 0, tipo: 'fri' }, z: { i: 36, d: 0.55, sonora: 1, tipo: 'fri' },
    x: { i: 31, d: 0.55, sonora: 0, tipo: 'fri' }, f: { i: 41, d: 0.5, sonora: 0, tipo: 'fri' }, v: { i: 41, d: 0.5, sonora: 1, tipo: 'fri' },
    l: { i: 38, d: 0.9, sonora: 1, tipo: 'apr' }, w: { i: 41, d: 0.6, sonora: 1, tipo: 'apr' }, y: { i: 30, d: 0.9, sonora: 1, tipo: 'apr' },
    r: { i: 36, d: 0, sonora: 1, tipo: 'vib' }, h: { tipo: 'asp', sonora: 0 },
  };
  // Vocales del español calibradas midiendo F1/F2 del motor (paridad/vocales.mjs) contra Quilis.
  const VOCALES = {
    a: { i: 18, d: 2.7, lab: 1.2 }, e: { i: 29, d: 3.15, lab: 0.9 }, i: { i: 29, d: 2.4, lab: 0.9 },
    o: { i: 11, d: 2.1, lab: 0.9 }, u: { i: 13, d: 2.1, lab: 0.5 }, 'ə': { i: 12.9, d: 2.43, lab: null },
  };
  function parseSilaba(s) {
    s = String(s || '').toLowerCase().replace('sh', 'x').replace('ch', 'x').replace('ñ', 'n');
    if (s === 'mm' || s === 'm') return { onset: [], vocal: 'mm' };
    const onset = []; let j = 0;
    while (j < s.length && CONS[s[j]] && !VOCALES[s[j]]) { onset.push(s[j]); j++; }
    const vs = s.slice(j); const vocal = [...vs].find((c) => VOCALES[c]) || 'ə';
    const coda = [...vs].reverse().find((c) => CONS[c] && !VOCALES[c]);
    return { onset, vocal, vocal2: [...vs].filter((c) => VOCALES[c])[1] || null, coda: coda || null };
  }
  function asignarSilabas(notas, banco, seed, texto) {
    const r = rngDe(seed, 'sil');
    let lista = null;
    if (texto && texto.trim()) lista = texto.trim().split(/[\s,\-]+/).filter(Boolean);
    let motivo = null;
    notas.forEach((n, i) => {
      if (lista) { n.sil = lista[i % lista.length]; return; }
      if (!banco || !banco.length) { n.sil = ''; return; }
      if (!motivo || i % 4 === 0) motivo = [0, 1, 2, 3].map(() => elegir(r, banco.map((b) => b[0]), banco.map((b) => b[1])));
      n.sil = n.firma ? motivo[0] : (n.finFrase && n.dur >= 1 ? (banco.find((b) => b[2] === 'fin') || [motivo[3]])[0] : motivo[i % 4]);
    });
  }

  // ======================================================================= zigués
  // Sílabas con la fonotaxis del español (CV, CCV con r/l, codas n s l r) y un sesgo extraterrestre:
  // más u, i, z, x (sh) y k, diptongos, codas raras (x, k) y palabras que se repiten como un idioma.
  // dureza 0 = l m n y w (tierno); 1 = k t x z g d r y codas duras (punk, death metal).
  function zigues(seed, dureza) {
    const r = rngDe(seed, 'zig'), d = clamp01(dureza == null ? 0.5 : dureza);
    const suaves = ['l', 'm', 'n', 'y', 'w', 's', ''], duras = ['k', 't', 'x', 'z', 'g', 'd', 'r', 'b'];
    const grupos = ['kr', 'tr', 'gl', 'bl', 'kl', 'gr', 'zr'];
    const vocales = ['a', 'e', 'i', 'o', 'u', 'u', 'i', 'o'];
    const dipt = ['ue', 'ia', 'io', 'ai', 'oi', 'au', 'ui'];
    const silaba = () => {
      let on;
      const x = r();
      if (x < 0.12 * d) on = grupos[Math.floor(r() * grupos.length)];
      else if (x < 0.12 * d + 0.55 * d) on = duras[Math.floor(r() * duras.length)];
      else on = suaves[Math.floor(r() * suaves.length)];
      const v = r() < 0.15 ? dipt[Math.floor(r() * dipt.length)] : vocales[Math.floor(r() * vocales.length)];
      let coda = '';
      if (r() < 0.06 + 0.12 * d) coda = d > 0.5 ? ['x', 'k', 's'][Math.floor(r() * 3)] : ['n', 's', 'l'][Math.floor(r() * 3)];
      return on + v + coda;
    };
    const lex = [];
    for (let i = 0; i < 9; i++) {
      const n = 1 + Math.floor(r() * 3), w = [];
      for (let k = 0; k < n; k++) w.push(silaba());
      if (n >= 2 && r() < 0.3) w[1] = w[0];          // reduplicación: «zuzu», «kalka»
      lex.push(w);
    }
    return lex;
  }
  function asignarZigues(notas, seed, dureza) {
    const lex = zigues(seed, dureza), r = rngDe(seed, 'zigfrase');
    // un «verso» de 4 palabras que vuelve, con variaciones: suena a idioma, no a ruido
    const verso = [0, 1, 2, 3].map(() => Math.floor(r() * lex.length));
    let cola = [], vi = 0;
    const usadas = [];
    for (const n of notas) {
      if (!cola.length) { let w = verso[vi % verso.length]; if (r() < 0.25) w = Math.floor(r() * lex.length); vi++; cola = lex[w].slice(); usadas.push(lex[w].join('')); }
      n.sil = cola.shift();
      if (n.finFrase && cola.length) cola = [];
    }
    return { lexico: lex.map((w) => w.join('')), usadas };
  }

  // ======================================================================= segunda voz (Doble Armonía)
  // 'juego' = NearestChordToneHarmonyStrategy de MidiGenPlay: la nota del acorde más cercana a la
  // melodía entre 3 y 14 semitonos (min/maxDistanceFromMelody por defecto), desempate por cercanía a
  // la nota anterior. Si no, un intervalo diatónico fijo en grados.
  const RELACIONES = { 'Como el juego (nota del acorde)': 'juego', 'Tercera arriba': 2, 'Tercera abajo': -2, 'Sexta abajo': -5, 'Quinta abajo': -4, 'Octava abajo': 'oct', 'Unísono (doblar)': 0 };
  function armonizar(mel, tonica, rel, ritmo) {
    const esc = mel.esc, L = esc.length;
    const acordePcs = (bar) => acordeEnGrados(esc, mel.prog[bar % mel.prog.length]).map((g) => (((tonica + esc[((g % L) + L) % L]) % 12) + 12) % 12);
    const out = [];
    let last = null;
    const voz = (n) => {
      if (rel === 'oct') return n.midi - 12;
      if (typeof rel === 'number') return gradoAMidi(esc, tonica, n.grado + rel);
      const pcs = acordePcs(n.bar);
      let best = null, bm = 999, bl = 999;
      for (let m = n.midi - 14; m <= n.midi + 14; m++) {
        if (!pcs.includes(((m % 12) + 12) % 12)) continue;
        const dm = Math.abs(m - n.midi); if (dm < 3) continue;
        const dl = last == null ? 0 : Math.abs(m - last);
        if (dm < bm || (dm === bm && dl < bl)) { best = m; bm = dm; bl = dl; }
      }
      return best;
    };
    if (ritmo === 'coro') {
      for (let h = 0; h < mel.compases * 2; h++) {
        const t0 = h * 2, ns = mel.notas.filter((n) => n.beat >= t0 && n.beat < t0 + 2);
        if (!ns.length) continue;
        const m = voz(ns[0]); if (m == null) continue;
        out.push({ beat: t0, dur: 2, midi: m, grado: ns[0].grado, vel: Math.max(40, ns[0].vel - 18), bar: ns[0].bar, sil: null, finFrase: ns[ns.length - 1].finFrase });
        last = m;
      }
      return out;
    }
    const shift = ritmo === 'eco' ? 1 : 0, total = mel.compases * 4;
    for (const n of mel.notas) {
      const m = voz(n); if (m == null) continue;
      const beat = n.beat + shift;
      if (beat >= total) continue;
      out.push({ beat, dur: Math.min(n.dur, total - beat), midi: m, grado: n.grado, vel: Math.max(40, n.vel - 8), bar: Math.floor(beat / 4), sil: n.sil, finFrase: n.finFrase, caida: n.caida });
      last = m;
    }
    return out;
  }

  // ======================================================================= modos de voz
  // Cada modo es una posición de las perillas. Los cinco primeros campos existen hoy en VoiceProfileSO.
  const BASE = {
    looseness: 0.15, vibratoDepth: 0.4, vibratoSpeedHz: 6, vibratoDelay: 0.35, vibratoRamp: 0.4, brightness: 0.5,
    transpose: -12, tensLo: 0.4, tensHi: 0.6, minLoudness: 0.15, pitchLead: 0.06,
    tracto: 44, aire: 1, growl: 0, aspereza: 0, saturacion: 0, scoop: 0, scoopMs: 90, caida: 0, glideMs: 0,
    ataque: 0, legato: 0.3, vocalBase: 'ə', silabas: [], nivel: 0.12, dureza: 0.5,
  };
  const MODOS = {
    'Zig hoy': { nota: 'El perfil VoiceProfile_Zig y el prefab, tal cual: vocal fija «uh» (expressivity apagada).', juego: true },
    'Zig hoy + dicción': { nota: 'Lo mismo, con expressivityEnabled encendido: un booleano en el prefab.', juego: true, diccion: true },
    'Tierno y calmado': { nota: 'Voz soplada (tensión baja = Rd alto), mucho aire, vibrato lento y tardío, brillo bajo, vocales redondas y legato. Bocca chiusa en las notas largas.', v: { dureza: 0.1, looseness: 0.08, vibratoDepth: 0.35, vibratoSpeedHz: 5, vibratoDelay: 0.5, vibratoRamp: 0.6, brightness: 0.28, tensLo: 0.22, tensHi: 0.42, minLoudness: 0.1, tracto: 46, aire: 1.8, glideMs: 110, scoop: 0.4, scoopMs: 140, legato: 0.9, vocalBase: 'u', silabas: [['mm', 2, 'fin'], ['lu', 2], ['u', 2], ['no', 1], ['la', 1]] } },
    'Pop': { nota: 'Modal, vibrato corto al final de las notas largas, pequeños scoops, sílabas de coro.', v: { dureza: 0.4, looseness: 0.12, vibratoDepth: 0.5, vibratoSpeedHz: 5.8, vibratoDelay: 0.25, brightness: 0.55, tensLo: 0.45, tensHi: 0.66, scoop: 0.6, scoopMs: 70, caida: 0.5, legato: 0.6, silabas: [['na', 3], ['la', 2], ['da', 1], ['ye', 1], ['oh', 1, 'fin']] } },
    'Punk rock': { nota: 'Voz apretada (tensión alta = Rd bajo), sin vibrato, laringe alta (tracto corto, más brillo), ataques empujados, caídas al final y saturación de micro. Sin fuente específica de punk: rasgos de rock y belt (Guzman 2013).', v: { dureza: 0.85, looseness: 0.3, vibratoDepth: 0, brightness: 0.82, tensLo: 0.72, tensHi: 0.92, minLoudness: 0.5, tracto: 40, transpose: -7, aspereza: 0.25, saturacion: 0.45, scoop: 1.5, scoopMs: 60, caida: 2.5, ataque: 0.25, legato: 0.05, pitchLead: 0.02, silabas: [['hey', 3], ['go', 2], ['oi', 2], ['ya', 2], ['no', 1], ['hey', 2, 'fin']] } },
    'Death metal': { nota: 'Growl: vibración supraglótica con subarmónicos (Sakakibara et al. 2004; Aaen et al. 2020), más flujo de aire (Guzman et al. 2019), laringe baja y oscura, registro grave.', v: { dureza: 0.95, looseness: 0.35, vibratoDepth: 0, brightness: 0.32, tensLo: 0.78, tensHi: 0.95, minLoudness: 0.55, tracto: 52, transpose: -24, aire: 3, growl: 0.85, aspereza: 0.6, saturacion: 0.55, scoop: 0, caida: 1.5, ataque: 0.15, legato: 0.4, vocalBase: 'o', silabas: [['gro', 2], ['uo', 2], ['bro', 1], ['ro', 1], ['dru', 1], ['o', 2, 'fin']] } },
    'Ópera': { nota: 'Laringe baja (tracto largo), vibrato amplio y regular a 5,6 Hz desde casi el inicio (Prame 1994: 5,5–6,7 Hz, ±60 cents), legato, vocales abiertas.', v: { dureza: 0.3, looseness: 0.04, vibratoDepth: 2.6, vibratoSpeedHz: 5.6, vibratoDelay: 0.12, vibratoRamp: 0.25, brightness: 0.62, tensLo: 0.52, tensHi: 0.72, tracto: 50, glideMs: 60, legato: 0.85, vocalBase: 'a', silabas: [['a', 3], ['o', 2], ['la', 1], ['ah', 1, 'fin']] } },
    'Crooner': { nota: 'Grave, cerca del micro: aire, brillo bajo, vibrato lento y profundo, deslices (glide) entre notas.', v: { dureza: 0.3, looseness: 0.18, vibratoDepth: 1.4, vibratoSpeedHz: 4.8, vibratoDelay: 0.4, brightness: 0.35, tensLo: 0.35, tensHi: 0.55, tracto: 48, transpose: -14, aire: 1.4, glideMs: 140, scoop: 1, scoopMs: 120, legato: 0.8, silabas: [['bu', 2], ['du', 2], ['ba', 1], ['oh', 1, 'fin']] } },
    'Falsete': { nota: 'Registro alto y soplado, tracto corto: voz de pájaro o de fantasma.', v: { dureza: 0.15, looseness: 0.1, vibratoDepth: 0.8, vibratoSpeedHz: 6.2, brightness: 0.45, tensLo: 0.18, tensHi: 0.32, transpose: 0, tracto: 38, aire: 2.2, glideMs: 60, legato: 0.7, vocalBase: 'u', silabas: [['u', 3], ['i', 1], ['hu', 1]] } },
    'Robot': { nota: 'Cero vida: sin jitter, sin vibrato, sin glide, staccato y sílabas de máquina.', v: { dureza: 0.75, looseness: 0, vibratoDepth: 0, brightness: 0.7, tensLo: 0.6, tensHi: 0.6, glideMs: 0, pitchLead: 0, legato: 0, silabas: [['bi', 2], ['bo', 2], ['di', 1], ['du', 1], ['ti', 1]] } },
    'Borracho': { nota: 'El Pink Trombone original sin domar: looseness 1 (35 cents de desviación) y vibrato lento y hondo.', v: { dureza: 0.45, looseness: 1, vibratoDepth: 1.2, vibratoSpeedHz: 4.5, brightness: 0.35, tensLo: 0.4, tensHi: 0.55, glideMs: 160, legato: 0.9, aspereza: 0.2, silabas: [['la', 2], ['lo', 2], ['ay', 1], ['hic', 1]] } },
  };
  function vozDeModo(nombre) { return Object.assign({}, BASE, (MODOS[nombre] && MODOS[nombre].v) || {}); }
  const PERFIL_ZIG = { looseness: 0.15, vibratoDepth: 0.4, vibratoSpeedHz: 6, diction: 1, mouth: 0, brightness: 0.5, transposeSemitones: -12, tensenessAtVel0: 0.4, tensenessAtVel127: 0.6, vibratoDelaySeconds: 0.35, vibratoRampSeconds: 0.4, pitchLeadSeconds: 0.06, leadFullInterval: 7, minLoudness: 0.15, startTrimMs: 20, gain: 1 };

  // ======================================================================= notas a segundos
  function aSegundos(mel, bpm) {
    const sb = 60 / bpm;
    return mel.notas.map((n) => ({ start: n.beat * sb, end: (n.beat + n.dur) * sb - 0.001, note: n.midi, vel: n.vel, sil: n.sil, caida: n.caida, grado: n.grado }));
  }

  // ======================================================================= cantante del juego (SingerVoice v7)
  const PRESETS_JUEGO = [[12.9, 2.43], [14.0, 2.9], [27.0, 2.1], [18.0, 3.2]];
  function cantarJuego(notas, perfil, opts) {
    opts = opts || {};
    const p = Object.assign({}, PERFIL_ZIG, perfil || {});
    const dur = opts.duracion || (notas.length ? notas[notas.length - 1].end + 1 : 1);
    const frames = Math.ceil(dur * SR / 1024) * 1024;
    const out = new Float32Array(frames);
    const v = new MV.PinkTrombone(SR, opts.seed == null ? 1 : opts.seed);
    // Arm: lead por intervalo + C-lite (ApplyExpressivity)
    const ev = notas.map((n) => Object.assign({}, n));
    for (let i = 0; i < ev.length; i++) {
      if (i === 0) { ev[i].lead = 0; continue; }
      const interval = Math.abs(ev[i].note - ev[i - 1].note);
      const want = p.pitchLeadSeconds * clamp01(interval / Math.max(1, p.leadFullInterval));
      ev[i].lead = Math.min(want, (ev[i - 1].end - ev[i - 1].start) * 0.6);
    }
    const base = PRESETS_JUEGO[clamp(p.mouth, 0, 3)];
    const expr = !!opts.diccion, ctx = opts.ctx;
    for (let i = 0; i < ev.length; i++) {
      ev[i].vi = base[0]; ev[i].vd = base[1]; ev[i].tb = 0;
      if (!expr || !ctx) continue;
      const dic = p.diction, m1 = 0.7 * dic, m2 = 2 * dic, m3 = 0.08 * dic;
      const d = ev[i].end - ev[i].start;
      let w = 0.5;
      if (ctx.beatSeconds > 0) {
        const beats = ev[i].start / ctx.beatSeconds, inBar = beats % ctx.beatsPerBar;
        const onBeat = Math.abs(inBar - Math.round(inBar)) < 0.10;
        const nearest = Math.round(inBar) % ctx.beatsPerBar;
        w = !onBeat ? 0.25 : (nearest === 0 ? 1 : 0.6);
      }
      const open01 = clamp01(clamp01(0.5 * clamp01(d / 0.8) + 0.5 * w) * m1);
      ev[i].vi = lerp(base[0], PRESETS_JUEGO[1][0], open01); ev[i].vd = lerp(base[1], PRESETS_JUEGO[1][1], open01);
      if (i > 0) ev[i].vi = clamp(ev[i].vi + Math.sign(ev[i].note - ev[i - 1].note) * m2, 0, 44);
      const rel = (((ev[i].note % 12) - ctx.rootPc) % 12 + 12) % 12, deg = ctx.scalePcs.indexOf(rel);
      const b = { 0: -1, 4: -0.6, 2: -0.4, 5: -0.2, 1: 0.3, 3: 0.5, 6: 1 }[deg]; ev[i].tb = (b == null ? 0.8 : b) * m3;
    }
    v.reset(); v.vibratoGain = 0; v.vibratoFrequency = p.vibratoSpeedHz; v.vibratoWobble = false;
    v.pitchJitterGain = p.looseness; v.tensenessJitterGain = p.looseness; v.alwaysVoice = false; v.isTouched = false;
    let clock = 0, nextIdx = 0, nextPitch = 0, gate = false, activeEnd = 0, noteOn = 0, curVi = base[0], curVd = base[1], lp1 = 0, lp2 = 0;
    const a = 1 - Math.exp(-2 * Math.PI * (1200 * Math.pow(2, 3.1 * p.brightness)) / SR);
    const snaps = [];
    for (let off = 0; off < frames; off += 1024) {
      if (expr) v.setTongue(curVi, curVd); else v.setTongue(base[0], base[1]);
      const blockEnd = clock + 1024 / SR;
      while (nextPitch < ev.length && ev[nextPitch].start - ev[nextPitch].lead < blockEnd) {
        const pv = ev[nextPitch]; v.setMusicalNote(pv.note + p.transposeSemitones - 69); curVi = pv.vi; curVd = pv.vd; nextPitch++;
      }
      while (nextIdx < ev.length && ev[nextIdx].start < blockEnd) {
        const e = ev[nextIdx], vel01 = clamp01(e.vel / 127);
        v.loudness = clamp01(p.minLoudness + (1 - p.minLoudness) * vel01);
        let t = lerp(p.tensenessAtVel0, p.tensenessAtVel127, vel01);
        if (expr) t = clamp(t + e.tb, Math.min(p.tensenessAtVel0, p.tensenessAtVel127), Math.max(p.tensenessAtVel0, p.tensenessAtVel127));
        v.targetTenseness = clamp01(t);
        v.isTouched = true; gate = true; activeEnd = e.end; noteOn = clock; nextIdx++;
      }
      if (gate && clock >= activeEnd) { gate = false; v.isTouched = false; }
      const held = clock - noteOn, vr = p.vibratoRampSeconds <= 0 ? 1 : clamp01((held - p.vibratoDelaySeconds) / p.vibratoRampSeconds);
      v.vibratoGain = clamp(p.vibratoDepth * 0.012 * vr, 0, 0.1);
      v.synthesize(out, off, 1024);
      for (let i = off; i < off + 1024; i++) { lp1 += a * (out[i] - lp1); lp2 += a * (lp1 - lp2); out[i] = lp2 * p.gain; }
      snaps.push(snap(v, clock, ev[nextIdx - 1]));
      clock = blockEnd;
    }
    return { audio: out, snaps, sr: SR, bloque: 1024 };
  }

  function snap(v, t, nota, sil) {
    const T = v.tract, g = v.glottis;
    return { t, d: Float32Array.from(T.diameter), velo: T.noseDiameter[0], f0: g.smoothFrequency, tens: g.targetTenseness, int: g.intensity, n: T.n, ti: v.shaper.tongueIndex, td: v.shaper.tongueDiameter, sil: sil != null ? sil : (nota && nota.sil) || '' };
  }

  // ======================================================================= cantante propuesto (v2)
  function cantarV2(notas, voz, opts) {
    opts = opts || {};
    const V = Object.assign({}, BASE, voz || {});
    const dur = opts.duracion || (notas.length ? notas[notas.length - 1].end + 1 : 1);
    const B = 256, frames = Math.ceil(dur * SR / B) * B, out = new Float32Array(frames);
    const v = new MV.PinkTrombone(SR, opts.seed == null ? 1 : opts.seed, { tractLength: Math.round(V.tracto) });
    const g = v.glottis, T = v.tract, S = v.shaper, sc = T.n / 44;
    g.x.sub = V.growl; g.x.jitter = V.aspereza; g.x.shimmer = V.aspereza * 0.8; g.x.air = V.aire;
    v.vibratoWobble = false; v.pitchJitterGain = clamp01(V.looseness); v.tensenessJitterGain = clamp01(V.looseness);
    v.alwaysVoice = false; v.isTouched = false; v.vibratoFrequency = V.vibratoSpeedHz;
    const ev = notas.map((n, i) => {
      const s = parseSilaba(n.sil == null || n.sil === '' ? V.vocalBase : n.sil);
      return Object.assign({}, n, { s, i });
    });
    // consonantes: cuánto antes del ataque empiezan
    const preDe = (c) => ({ ocl: 0.075, nas: 0.08, fri: 0.1, apr: 0.07, vib: 0.03, asp: 0.07 }[c.tipo] || 0.06);
    ev.forEach((e, i) => {
      const prevEnd = i > 0 ? ev[i - 1].end : -1;
      e.c = e.s.onset.length ? CONS[e.s.onset[0]] : null;
      e.c2 = e.s.onset.length > 1 ? CONS[e.s.onset[1]] : null;
      const pre = e.c ? preDe(e.c) : 0;
      e.cStart = e.start - Math.min(pre, Math.max(0.02, (e.start - prevEnd) + 0.06));
      e.legatoNext = i < ev.length - 1 && ev[i + 1].start - e.end < 0.03 + 0.15 * V.legato;
      e.vowel = e.s.vocal === 'mm' ? 'u' : e.s.vocal;
    });
    let cur = -1, semis = null, lp1 = 0, lp2 = 0, touched = false, noteOnT = 0;
    const a = 1 - Math.exp(-2 * Math.PI * (1200 * Math.pow(2, 3.1 * V.brightness)) / SR);
    const snaps = []; let turb = null; let lastNote = null;
    v.reset();
    for (let off = 0; off < frames; off += B) {
      const t = off / SR;
      // nota activa: la última cuyo inicio de consonante ya pasó (prioridad a la última)
      while (cur + 1 < ev.length && ev[cur + 1].cStart <= t) cur++;
      const e = cur >= 0 ? ev[cur] : null;
      let constr = [], velo = null, wantTouch = false, tens = V.tensLo, loud = 0.5, sil = '';
      if (e) {
        sil = e.sil || '';
        const enCons = t < e.start, hasta = e.legatoNext ? ev[cur + 1].cStart : e.end;
        const vivo = t < hasta;
        const vel01 = clamp01(e.vel / 127);
        loud = clamp01(V.minLoudness + (1 - V.minLoudness) * vel01);
        tens = lerp(V.tensLo, V.tensHi, vel01);
        if (t - e.start < 0.08 && t >= e.start) tens += V.ataque;          // ataque empujado
        // vocal
        const vo = VOCALES[e.vowel] || VOCALES['ə'];
        v.setTongue(vo.i, vo.d);
        if (vo.lab != null && vo.lab < 1.5) constr.push({ index: 41, diameter: vo.lab });
        // segunda vocal (diptongo: «oi», «ay», «ye»): pasa a la mitad de la nota
        if (e.s.vocal2 && t > (e.start + e.end) / 2) { const v2 = VOCALES[e.s.vocal2]; v.setTongue(v2.i, v2.d); constr = v2.lab != null ? [{ index: 41, diameter: v2.lab }] : []; }
        if (e.s.vocal === 'mm') { constr.push({ index: 41, diameter: 0 }); velo = 0.4; }
        // consonante de ataque
        if (e.c && enCons) {
          const c = (e.c2 && e.start - t < 0.035) ? e.c2 : e.c;
          if (c.tipo === 'asp') { tens = 0.08; wantTouch = true; }
          else {
            constr.push({ index: c.i, diameter: c.d });
            if (c.tipo === 'nas') velo = 0.4;
            wantTouch = !!c.sonora;
          }
        } else if (vivo) wantTouch = true;
        // coda (consonante final: «hey» no, «oh» no; «hic» sí) en los últimos 80 ms
        if (e.s.coda && vivo && hasta - t < 0.08) { const c = CONS[e.s.coda]; if (c && c.i) constr.push({ index: c.i, diameter: c.d }); }
        if (!vivo) wantTouch = false;
        // altura: scoop, caída, glide
        let target = e.note + V.transpose;
        const sinceOn = t - e.start;
        if (V.scoop > 0 && sinceOn >= 0 && sinceOn < V.scoopMs / 1000) target -= V.scoop * (1 - sinceOn / (V.scoopMs / 1000));
        const finalDeFrase = !e.legatoNext;
        if ((V.caida > 0 && finalDeFrase) || e.caida) { const fall = e.caida ? Math.max(V.caida, 2) : V.caida, fw = 0.18; const left = hasta - t; if (left < fw && left >= 0) target -= fall * (1 - left / fw) ** 2; }
        if (e !== lastNote) { noteOnT = e.start; lastNote = e; if (semis == null) semis = target; }
        if (semis == null || V.glideMs <= 0) semis = target;
        else semis += (target - semis) * (1 - Math.exp(-(B / SR) / (V.glideMs / 1000)));
        if (V.pitchLead > 0 && cur + 1 < ev.length && ev[cur + 1].start - t < V.pitchLead && V.glideMs <= 0) semis = ev[cur + 1].note + V.transpose - (V.scoop > 0 ? V.scoop : 0);
        v.setMusicalNote(semis - 69);
      }
      const fric = e && e.c && e.c.tipo === 'fri' && t < e.start;
      if (fric && !turb) { turb = { position: e.c.i * sc, diameter: e.c.d, startTime: T.time, endTime: NaN }; T.turbulencePoints.push(turb); }
      if (!fric && turb && Number.isNaN(turb.endTime)) turb.endTime = T.time;
      if (turb && !Number.isNaN(turb.endTime) && T.time - turb.endTime > 0.15) { T.turbulencePoints.splice(T.turbulencePoints.indexOf(turb), 1); turb = null; }
      S.setConstrictions(constr);
      S.velumOverride = velo;
      if (wantTouch !== touched) { v.isTouched = wantTouch; touched = wantTouch; }
      v.loudness = loud;
      v.targetTenseness = clamp01(tens);
      const held = t - noteOnT, vr = V.vibratoRamp <= 0 ? 1 : clamp01((held - V.vibratoDelay) / V.vibratoRamp);
      v.vibratoGain = clamp(V.vibratoDepth * 0.012 * vr, 0, 0.1);
      v.synthesize(out, off, B);
      const drive = V.saturacion, k = 1 + 12 * drive, norm = Math.tanh(k * 0.25);
      for (let i = off; i < off + B; i++) {
        lp1 += a * (out[i] - lp1); lp2 += a * (lp1 - lp2);
        let s = lp2;
        if (drive > 0) s = Math.tanh(k * s) / norm * 0.25;
        out[i] = s;
      }
      if ((off / B) % 4 === 0) snaps.push(snap(v, t, e, sil));
    }
    return { audio: out, snaps, sr: SR, bloque: B * 4 };
  }

  /** Normaliza al RMS objetivo (para comparar modos con el mismo volumen). */
  function normalizar(audio, rmsObj) {
    let s = 0, c = 0; for (let i = 0; i < audio.length; i++) { const x = audio[i]; if (Math.abs(x) > 1e-4) { s += x * x; c++; } }
    const rms = Math.sqrt(s / Math.max(1, c)); const k = rms > 0 ? rmsObj / rms : 1;
    let pk = 0; for (let i = 0; i < audio.length; i++) { audio[i] *= k; pk = Math.max(pk, Math.abs(audio[i])); }
    if (pk > 0.98) { const q = 0.98 / pk; for (let i = 0; i < audio.length; i++) audio[i] *= q; }
    return k;
  }

  const api = { zigues, asignarZigues, armonizar, RELACIONES, SR, ESCALAS, EN_EL_JUEGO, PROGRESIONES, FIGURAS, CONS, VOCALES, MODOS, BASE, PERFIL_ZIG, PRESETS_JUEGO, componer, asignarSilabas, parseSilaba, aSegundos, cantarJuego, cantarV2, vozDeModo, normalizar, gradoAMidi, acordeEnGrados };
  if (typeof module !== 'undefined' && module.exports) module.exports = api;
  else root.MC = api;
})(typeof window !== 'undefined' ? window : globalThis);
