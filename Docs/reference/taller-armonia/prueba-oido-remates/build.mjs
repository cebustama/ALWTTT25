// Builds hoja-oido-remates.html from fuente.html, pasting the theory code of ../template.html
// (the same slice that scripts/oro.js runs), so the page plays exactly what the levels do.
// Uso: node Docs/reference/taller-armonia/prueba-oido-remates/build.mjs
import fs from "fs"; import path from "path"; import { fileURLToPath } from "url";
const dir = path.dirname(fileURLToPath(import.meta.url));
const tpl = fs.readFileSync(path.join(dir, "..", "template.html"), "utf8");
const a = tpl.indexOf("/* ---------- theory"), b = tpl.indexOf("/* ---------- state");
if(a < 0 || b < 0) throw new Error("No encuentro la teoría en template.html");
const src = fs.readFileSync(path.join(dir, "fuente.html"), "utf8");
if(!src.includes("/*__THEORY__*/")) throw new Error("fuente.html sin /*__THEORY__*/");
const out = src.replace("/*__THEORY__*/", () => tpl.slice(a, b));
fs.writeFileSync(path.join(dir, "hoja-oido-remates.html"), out);
console.log("hoja-oido-remates.html", out.length, "bytes");
