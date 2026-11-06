// Tests the page's own export code (the /*EXPORT*/ block of template.html):
// 1) same text as build.py for the catalogue, 2) reshape keeps hits in place,
// 3) writes random patterns to probar-export.json for the real parser (dotnet run -- probar-export.json).
// Run from this folder: node probar-export.mjs
import fs from "node:fs";

const tpl = fs.readFileSync("../template.html", "utf8");
const code = tpl.slice(tpl.indexOf("/*EXPORT*/"), tpl.indexOf("/*/EXPORT*/"));
const { exportInfo, reshape, GM_NAME, TS_GRID } = new Function(code + "; return {exportInfo, reshape, GM_NAME, TS_GRID};")();

let diff = 0;
const data = JSON.parse(fs.readFileSync("../patrones.json", "utf8"));
for (const p of data.patterns) {
  const r = exportInfo(p);
  if (r.text !== p.export || r.snapped !== p.snapped) { diff++; console.log("DIFF", p.id); }
}
console.log(`catalogue: ${data.patterns.length - diff} equal to build.py, ${diff} different`);

let seed = 20261106;
const rnd = n => (seed = (seed * 1664525 + 1013904223) >>> 0) % n;
const pick = a => a[rnd(a.length)];
const gms = Object.keys(GM_NAME).map(Number), tss = Object.keys(TS_GRID);
const NAMES = ["Measures: 9", "Subdivisions: 1", "Time signature: FiveFour", "xXo", "1. BassDrum1 default velocity 3", "lanes", "```", "ñandú (prueba)", "---", ""];
const patterns = [];
for (let n = 0; n < 400; n++) {
  const ts = pick(tss), [beats, unit] = TS_GRID[ts], subdivisions = 1 + rnd(4), measures = 1 + rnd(16), totalSteps = beats * subdivisions * measures;
  const lanes = Array.from({ length: 1 + rnd(8) }, () => {
    const gm = pick(gms), def = 1 + rnd(127), empty = rnd(6) === 0;
    return { gm, def, steps: Array.from({ length: totalSteps }, () => empty ? -1 : pick([-1, -1, -1, 0, 120, 50, ...(n % 4 === 0 ? [1 + rnd(127)] : [])])) };
  });
  const p = { id: n < NAMES.length ? NAMES[n] : "Prueba " + n, ts, beats, unit, measures, subdivisions, totalSteps, lanes };
  const r = exportInfo(p);
  patterns.push({ ...p, export: r.text, snapped: r.snapped });
}
fs.writeFileSync("probar-export.json", JSON.stringify({ patterns }));
console.log(patterns.length, "random patterns written");

// reshape: 2 -> 4 -> 2 subdivisions gives back the same grid; shrinking reports what it drops.
let bad = 0;
for (const p of patterns.slice(0, 100)) {
  const up = reshape(p, p.ts, p.measures, p.subdivisions * 2);
  const down = reshape({ ...p, ...up.grid }, p.ts, p.measures, p.subdivisions);
  if (up.dropped || down.dropped || JSON.stringify(down.grid.lanes) !== JSON.stringify(p.lanes)) bad++;
}
const a = { ts: "FourFour", beats: 4, unit: 4, measures: 2, subdivisions: 2, lanes: [{ gm: 36, def: 100, steps: [0, 0, -1, -1, -1, -1, -1, -1, 0, -1, -1, -1, -1, -1, 0, -1] }] };
const b = reshape(a, "ThreeFour", 1, 1);
console.log("reshape round trip failures:", bad, "| 4/4 x2 -> 3/4 x1 sub 1:", JSON.stringify(b.grid.lanes[0].steps), "dropped", b.dropped);
process.exit(diff || bad ? 1 : 0);
