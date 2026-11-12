// Generates paridad/casos.json from the BUILT page (index.html): the page's own engine (FX + BASS)
// computes accents, fill tags and the bass notes; Paridad.cs runs the same inputs through the real
// C# (ChordArticulator.PlanHits, AccentDeriver.MarkFills, BassAccentFollow.PlanEvent) and compares.
// Run: node casos.mjs   (after python -I -X utf8 build.py)
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const html = readFileSync(join(here, "..", "index.html"), "utf8");
const data = html.match(/const DATA = (.*);\r?\n/)[1];
const engine = html.slice(html.indexOf("/*ENGINE*/"), html.indexOf("/*/ENGINE*/"));
const { FX, BASS, PHR, DATA } = new Function(`const DATA = ${data};\n${engine}\nreturn {FX, BASS, PHR, DATA};`)();

const FIGS = [["Block", "Eighth"], ["ArpeggioUp", "Eighth"], ["ArpeggioUp", "Sixteenth"], ["ArpeggioUp", "PerBeat"]];
const SCALE = [0, 2, 4, 5, 7, 9, 11];
const pats = DATA.patterns.filter(p => p.beats === 4 && p.unit === 4);
const progs = DATA.progressions;
const velOf = (raw, def) => raw > 0 ? raw : def;
const cases = [];

pats.forEach((p, i) => {
  const g = { beats: p.beats, unit: p.unit, measures: p.measures, subdivisions: p.subdivisions,
    lanes: p.lanes.map(l => ({ gm: l.gm, def: l.def, steps: l.steps.map(r => r < 0 ? -1 : velOf(r, l.def)) })) };
  const PM = [4, 8, 16][i % 3], bpb = 4, end = PM * bpb;
  const S = { every: [0, 2, 4][i % 3], last: i % 2 ? 0 : 1, crash: true, contrast: [0, .35, .7][i % 3], procW: 1,
    library: (DATA.fills || []).map(f => ({ p: f, w: f.weight })), fillSeed: FX.fillSeedOf(7 + i) };
  const { hits, choices } = FX.buildSurface(g, PM, S);
  const raw = [...FX.accentsOf(hits, end).entries()].map(([k, w]) => ({ b: +k, w })).sort((a, b) => a.b - b.b);
  const accents = BASS.markFills(raw, choices, bpb);

  for (let f = 0; f < FIGS.length; f++) {
    const prog = progs[(i + f) % progs.length], [expr, rate] = FIGS[(i + f) % FIGS.length];
    const plen = prog.measures * bpb, sub = prog.subdivisions, events = [];
    for (let off = 0; off < end - 1e-9; off += plen)
      for (const e of prog.events.slice().sort((a, b) => a.startStep - b.startStep)) {
        const start = off + e.startStep / sub, len = Math.max(1, e.lengthSteps) / sub;
        if (start < end - 1e-9) events.push({ start, len: Math.min(len, end - start), vel: e.velocity, expr, rate });
      }
    for (const mode of ["reinforce", "attacks"])
      for (const fill of ["gap", "follow"])
        for (const cap of [4, 2]) {
          const st = { mode, threshold: [.6, .5, .75][(i + f) % 3], cap, boost: 16, cut: 8, fill };
          const notes = BASS.render(events, accents, st, bpb);
          cases.push({ id: `${p.display} | ${prog.id} | ${expr}/${rate} | ${mode}/${fill}/cap${cap}`, bpb, events,
            accentsRaw: raw, fills: choices.map(c => ({ m: c.m, start: c.start })), expectFill: accents.map(a => a.f),
            settings: st, expect: notes.map(n => ({ s: n.s, d: n.d, v: n.v, atk: !!n.atk, on: !!n.on })) });
        }
  }
});
writeFileSync(join(here, "casos.json"), JSON.stringify(cases));
console.log(cases.length, "cases from", pats.length, "grooves and", progs.length, "progressions");

// ---------- MGP-BASS-PHRASE-1: the C mode (PHR) against BassPhrasePlanner ----------
// Events carry their resolved root (midi), quality and absolute scale, as the page builds them
// (partEvents): in the game the composer resolves those (D10/D12); the planner does not.
const SC = { Ionian: [0, 2, 4, 5, 7, 9, 11], Aeolian: [0, 2, 3, 5, 7, 8, 10] };
const STYLES = ["rock", "pop", "funk", "ballad"], SEEDS = [0, 1, 12345, 4000000000];
const phr = [];
pats.forEach((p, i) => {
  const g = { beats: p.beats, unit: p.unit, measures: p.measures, subdivisions: p.subdivisions,
    lanes: p.lanes.map(l => ({ gm: l.gm, def: l.def, steps: l.steps.map(r => r < 0 ? -1 : velOf(r, l.def)) })) };
  const PM = [4, 8, 16][i % 3], bpb = 4, end = PM * bpb;
  const S = { every: [0, 2, 4][i % 3], last: i % 2 ? 0 : 1, crash: true, contrast: [0, .35, .7][i % 3], procW: 1,
    library: (DATA.fills || []).map(f => ({ p: f, w: f.weight })), fillSeed: FX.fillSeedOf(7 + i) };
  const { hits, choices } = FX.buildSurface(g, PM, S);
  const raw = [...FX.accentsOf(hits, end).entries()].map(([k, w]) => ({ b: +k, w })).sort((a, b) => a.b - b.b);
  const accents = BASS.markFills(raw, choices, bpb);
  const drumEv = hits.filter(h => FX.beatOf(h) < end).map(h => ({ b: FX.beatOf(h), gm: h.gm, vel: h.vel })).sort((a, b) => a.b - b.b);
  for (let f = 0; f < 3; f++) {
    const prog = progs[(i * 3 + f) % progs.length], sub = prog.subdivisions, plen = prog.measures * bpb;
    const minor = prog.events.length && [1, 5, 7, 8, 12, 16].includes(prog.events[0].quality);
    const scale = SC[minor ? "Aeolian" : "Ionian"], events = [];
    const evs = prog.events.slice().sort((a, b) => a.startStep - b.startStep);
    for (let off = 0; off < end - 1e-9; off += plen) evs.forEach(e => {
      const start = off + e.startStep / sub, len = Math.max(1, e.lengthSteps) / sub;
      if (start >= end - 1e-9) return;
      const pc = (((scale[e.degree] + (e.degreeAccidental || 0)) % 12) + 12) % 12;
      events.push({ start, len: Math.min(len, end - start), vel: e.velocity, pc, midi: 28 + ((pc - 4 + 12) % 12), q: e.quality, scale });
    });
    STYLES.forEach((style, si) => {
      for (let k = 0; k < 8; k++) {
        const st = { style, seed: SEEDS[(i + f + si + k) % 4] >>> 0, threshold: [.6, .5, .75][(i + f + k) % 3],
          fill: (i + k) % 2 ? "follow" : "gap", followSnare: !!(k & 1), deadNotes: !!(k & 2), anticipation: !(k & 4) };
        const r = PHR.render(drumEv, accents, events, st, bpb, end);
        phr.push({ id: `${p.display} | ${prog.id} | ${style} k${k} seed${st.seed}`, bpb, end, settings: st,
          onsets: drumEv, accents: accents.map(a => ({ b: a.b, w: a.w, f: !!a.f })),
          events: events.map(e => ({ start: e.start, vel: e.vel, midi: e.midi, q: e.q, scale: e.scale })),
          anchors: r.anchors.length,
          expect: r.notes.map(n => ({ s: n.s, d: n.d, v: n.v, m: n.midi, x: !!n.dead })) });
      }
    });
  }
});
writeFileSync(join(here, "casos-phr.json"), JSON.stringify(phr));
console.log(phr.length, "PHR cases,", phr.reduce((a, c) => a + c.expect.length, 0), "notes,",
  phr.reduce((a, c) => a + c.expect.filter(n => n.x).length, 0), "dead");
