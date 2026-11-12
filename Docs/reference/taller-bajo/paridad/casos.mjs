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
const { FX, BASS, DATA } = new Function(`const DATA = ${data};\n${engine}\nreturn {FX, BASS, DATA};`)();

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
