const LIB = [];
const $ = () => null;

/* ---------- theory ---------- */
const MODES = ["Ionian","Dorian","Phrygian","Lydian","Mixolydian","Aeolian","Locrian"];
const MODE_ES = {Ionian:"Jónico (mayor)",Dorian:"Dórico",Phrygian:"Frigio",Lydian:"Lidio",Mixolydian:"Mixolidio",Aeolian:"Eólico (menor)",Locrian:"Locrio"};
const MOOD = {Ionian:"Alegre",Dorian:"Con groove",Phrygian:"Oscuro",Lydian:"Soñador",Mixolydian:"Funky",Aeolian:"Triste",Locrian:"Tenso"};
const IONIAN = [0,2,4,5,7,9,11];
const STEPS = Object.fromEntries(MODES.map((m,i) => [m, IONIAN.map((_,k) => (IONIAN[(i+k)%7] - IONIAN[i] + 12) % 12)]));
const NOTE = ["C","D♭","D","E♭","E","F","F♯","G","A♭","A","B♭","B"];
const NUM = ["I","II","III","IV","V","VI","VII"];
// quality key -> intervals, canonical import suffix, lower-case numeral, chord-symbol suffix
const Q = {
  maj:{iv:[0,4,7],suf:"maj",lo:false,sym:""},       min:{iv:[0,3,7],suf:"m",lo:true,sym:"m"},
  dim:{iv:[0,3,6],suf:"dim",lo:true,sym:"°"},        aug:{iv:[0,4,8],suf:"aug",lo:false,sym:"+"},
  maj7:{iv:[0,4,7,11],suf:"maj7",lo:false,sym:"maj7"}, m7:{iv:[0,3,7,10],suf:"m7",lo:true,sym:"m7"},
  "7":{iv:[0,4,7,10],suf:"7",lo:false,sym:"7"},      hdim7:{iv:[0,3,6,10],suf:"ø7",lo:true,sym:"m7♭5"},
  dim7:{iv:[0,3,6,9],suf:"dim7",lo:true,sym:"°7"},   sus2:{iv:[0,2,7],suf:"sus2",lo:false,sym:"sus2"},
  sus4:{iv:[0,5,7],suf:"sus4",lo:false,sym:"sus4"},  "6":{iv:[0,4,7,9],suf:"6",lo:false,sym:"6"},
  m6:{iv:[0,3,7,9],suf:"m6",lo:true,sym:"m6"},       "7sus4":{iv:[0,5,7,10],suf:"7sus4",lo:false,sym:"7sus4"},
  "9":{iv:[0,4,7,10,14],suf:"9",lo:false,sym:"9"},   maj9:{iv:[0,4,7,11,14],suf:"maj9",lo:false,sym:"maj9"},
  m9:{iv:[0,3,7,10,14],suf:"m9",lo:true,sym:"m9"},
};
const MINORISH = new Set(["min","m7","m6","m9","dim","hdim7","dim7"]);
// Mirror of RomanProgressionParser.TryParseQualitySuffix (after removing spaces, Δ→maj, lower-casing).
const ALIAS = {};
[["maj","maj ma mjr mja"],["min","m min mi mn - min3 mtri mtriad"],["dim","dim o °"],["aug","aug + +5"],
 ["7","7 dom dom7"],["maj7","maj7 ma7 m7+ mm7 mmaj7"],["m7","m7 -7 min7"],["hdim7","ø ø7 m7b5 min7b5"],
 ["dim7","dim7 o7 °7"],["sus2","sus2"],["sus4","sus4 sus"],["6","6"],["m6","m6 min6"],["7sus4","7sus4"],
 ["9","9 dom9"],["maj9","maj9 ma9"],["m9","m9 min9"]].forEach(([k,v]) => v.split(" ").forEach(a => ALIAS[a] = k));

let mode = "Ionian";
const st = () => STEPS[mode];
const inScale = pc => st().includes(((pc%12)+12)%12);
const mod12 = x => ((x%12)+12)%12;
function diatonicQ(deg){
  const s = st(), t = mod12(s[(deg+2)%7]-s[deg]), f = mod12(s[(deg+4)%7]-s[deg]);
  return ({"4,7":"maj","3,7":"min","3,6":"dim","4,8":"aug"})[t+","+f] || "maj";
}
function romanOf(pc, q){
  const s = st(); pc = mod12(pc);
  let d = s.indexOf(pc), acc = "";
  if(d < 0 && s.indexOf(mod12(pc+1)) > 0){ d = s.indexOf(mod12(pc+1)); acc = "b"; }
  if(d < 0){ d = s.indexOf(mod12(pc-1)); acc = "#"; }
  const n = NUM[d];
  return acc + (Q[q].lo ? n.toLowerCase() : n) + Q[q].suf;
}
const fmtDur = d => String(Number(d.toFixed(3)));
const chordName = (pc, q, key) => NOTE[mod12(pc+key)] + Q[q].sym;

/* ---------- parser (mirrors MidiGenPlay's Roman import) ---------- */
function parse(text){
  const chords = [], issues = [];
  const toks = text.replace(/\n/g," ").replace(/\|/g,"–").split(/[–—-]/).map(t => t.trim()).filter(Boolean);
  for(const tok of toks){
    const m = tok.match(/^(.*?)\s*(\(([^)]*)\))?\s*$/);
    let core = (m[1]||"").trim(), dur = 1;
    if(m[2] !== undefined){
      dur = parseFloat(String(m[3]).trim());
      if(!(dur > 0) || /,/.test(m[3])){ issues.push({e:1, msg:"«"+tok+"»: duración no válida. Usa un número con punto, como (0.5) o (2)."}); continue; }
    } else if(/[()]/.test(core)){ issues.push({e:1, msg:"«"+tok+"»: falta un paréntesis."}); continue; }
    if(core === "" || /^(s|r|rest)$/i.test(core)){ chords.push({rest:true, dur}); continue; }
    const r = core.match(/^([b♭#♯]?)([IVXivx]+)(.*)$/);
    if(!r){ issues.push({e:1, msg:"«"+tok+"»: no empieza por un grado romano (I a VII)."}); continue; }
    const deg = NUM.indexOf(r[2].toUpperCase());
    if(deg < 0){ issues.push({e:1, msg:"«"+tok+"»: «"+r[2]+"» no es un grado de I a VII."}); continue; }
    const acc = /[b♭]/.test(r[1]) ? -1 : /[#♯]/.test(r[1]) ? 1 : 0;
    const rawSuf = r[3].trim();
    let q;
    if(rawSuf){
      const s = rawSuf.replace(/\s/g,"").replace(/[Δ∆]/g,"maj").toLowerCase();
      q = ALIAS[s];
      if(/M7|M9/.test(rawSuf) && !/maj|ma/i.test(rawSuf)) issues.push({e:0, msg:"«"+tok+"»: MidiGenPlay pasa el sufijo a minúsculas, así que «M7» se lee como m7 (menor). Para mayor séptima escribe maj7."});
      if(!q){ q = diatonicQ(deg); issues.push({e:1, msg:"«"+tok+"»: el sufijo «"+rawSuf+"» no existe en MidiGenPlay. El Import lo rechaza; aquí suena como el acorde de la escala ("+Q[q].suf+")."}); }
    } else {
      const lo = /^[ivx]+$/.test(r[2]), up = /^[IVX]+$/.test(r[2]);
      if(lo) q = "min"; else if(up) q = "maj";
      else { q = diatonicQ(deg); issues.push({e:0, msg:"«"+tok+"»: mezcla mayúsculas y minúsculas; MidiGenPlay lo ignora y usa el acorde de la escala."}); }
    }
    const pc = mod12(st()[deg] + acc);
    if(acc && inScale(pc)) issues.push({e:0, msg:"«"+tok+"»: en "+MODE_ES[mode]+" la alteración se aplica al grado del modo, así que suena como "+romanOf(pc, q)+". Si querías el acorde de la escala mayor, escríbelo sin alteración."});
    chords.push({pc, q, dur});
  }
  if(!chords.some(c => !c.rest)) issues.push({e:1, msg:"No hay ningún acorde que tocar."});
  return {chords, issues};
}

/* ---------- levels (v3, after the Music Lab review of 2026-11-02) ----------
   Rules: original chords are never removed (an addition only takes the second part of a chord);
   no events in lap 1; budget per lap: level 2 = 1 event, level 3 = 2; ninths only if the 9th is in the scale;
   modal approach chords instead of V7 outside Ionian/Aeolian; no half-step modulation inside the card. */
const majorish = () => mod12(st()[2]) === 4;
function color(c, nextPc, rich){
  let q = c.q; const r = c.pc;
  if(q === "maj"){
    if(mod12(r) !== 0 && mod12(nextPc - c.pc) === 5) q = "7";
    else if(inScale(r+11)) q = "maj7";
    else if(inScale(r+10)) q = "7";
    else q = "maj7";
  } else if(q === "min") q = "m7";
  else if(q === "dim") q = inScale(r+10) ? "hdim7" : "dim7";
  if(rich && inScale(r+2)) q = ({maj7:"maj9", m7:"m9", "7":"9"})[q] || q;
  return q;
}
// Chord that leads into the tonic in each mode (Music Lab, change 3). null = no approach (Locrian).
function tonicApproach(level, tMin){
  switch(mode){
    case "Ionian":  return level === 2 ? [{d:7, q:"7", why:"V7"}] : [{d:5, q:"m7", why:"backdoor iv7"}, {d:10, q:"9", why:"♭VII9"}];
    case "Aeolian": return level === 2 ? [{d:7, q:"7", why:"V7"}] : [{d:2, q:"hdim7", why:"iiø7"}, {d:1, q:"7", why:"sustituto tritonal ♭II7"}];
    case "Phrygian":   return [{d:1, q:"maj7", why:"♭IImaj7, el acorde frigio"}];
    case "Dorian":     return [{d:5, q:"7", why:"IV7, el acorde dórico"}];
    case "Lydian":     return [{d:2, q:"7", why:"II7, el acorde lidio"}];
    case "Mixolydian": return [{d:10, q: level === 3 ? "maj7" : "maj", why:"♭VII, el acorde mixolidio"}];
    default: return null;
  }
}
const nextNonRest = (seq, i) => { for(let k=1;k<=seq.length;k++){ const c = seq[(i+k)%seq.length]; if(!c.rest) return c; } return seq[i]; };
const CADENCE = new Set([5, 7, 2, 11]); // root motion into the first chord: V→I, IV→I, ♭VII→I, ♭II→i
function arrange(base, level, nV, beats){
  const B = beats || 4, h = B === 3 ? 1 : 0.5;          // the split unit: half a bar in 4/4, one whole bar in 3/4
  const vs = [];
  const distinct = new Set(base.filter(c => !c.rest).map(c => c.pc + c.q)).size;
  for(let v=0; v<nV; v++){
    let seq = base.map(c => c.rest ? {...c} : {...c, orig:true});
    if(level >= 2) seq = seq.map((c,i) => { if(c.rest) return c; const q = color(c, nextNonRest(seq,i).pc, level === 3); return q === c.q ? c : {...c, q, col:true}; });
    if(level >= 2 && v >= 1){
      const budget = (level === 2 || distinct <= 2) ? 1 : 2, used = new Set(), plan = [];   // plan: {i, tail:[chords], head?:chord}
      const can = (i, n) => { const c = seq[i]; return c && !c.rest && !used.has(i) && c.dur >= h*(n+1) - 1e-9; };
      const take = p => { if(plan.length < budget && !used.has(p.i)){ used.add(p.i); plan.push(p); return true; } return false; };
      const li = seq.length - 1, first = seq.find(c => !c.rest);
      // 1) turnaround on the last lap: second part of the last chord, unless the loop already closes by cadence
      if(v === nV-1 && !seq[li].rest && distinct > 2 && !CADENCE.has(mod12(first.pc - seq[li].pc))){
        const tR = romanOf(first.pc, first.q), toTonic = mod12(first.pc) === 0;
        let ap = toTonic ? tonicApproach(level, MINORISH.has(first.q)) : [{d:7, q:"7", why:"V7 de "+tR}];
        if(ap && !toTonic && level === 3) ap = [{d:1, q:"7", why:"sustituto tritonal hacia "+tR}];
        if(ap && ap.length === 2 && !can(li, 2)) ap = [ap[1]];
        if(ap && can(li, ap.length)) take({i:li, tail: ap.map((a,k) => ({pc:mod12(first.pc + a.d), q:a.q, dur:h, add:true, why: k ? undefined : "turnaround hacia "+tR+": "+ap.map(x => x.why).join("–")}))});
      }
      // 2) level 2: sus4 on the functional dominant (resolves a fifth down)
      if(level === 2) seq.forEach((c,i) => { if(plan.length >= budget || !c.orig || c.rest) return;
        const t = nextNonRest(seq,i);
        if(c.q === "7" && mod12(t.pc - c.pc) === 5 && can(i, 1)) take({i, head:{...c, q:"7sus4", dur:h, add:true, why:"sus4 que se resuelve en "+romanOf(c.pc,"7")}, rest:"tail"}); });
      // 3) level 3: one borrowed colour after IV (iv6 in major modes, IV7 in Aeolian)
      if(level === 3){
        const bi = seq.findIndex((c,i) => !c.rest && mod12(c.pc) === 5 && can(i,1) && (
          (["Ionian","Lydian","Mixolydian"].includes(mode) && !MINORISH.has(c.q)) || (mode === "Aeolian" && MINORISH.has(c.q))));
        if(bi >= 0){ const c = seq[bi], maj = mode !== "Aeolian";
          take({i:bi, tail:[{pc:c.pc, q: maj ? "m6" : "7", dur:h, add:true, why: maj ? "iv menor prestado (iv6) después del IV" : "IV7 dórico prestado después del iv"}]}); }
      }
      // 4) secondary dominant (level 2 fallback) or its tritone substitute (level 3) into a diatonic non-tonic chord
      seq.forEach((c,i) => { if(plan.length >= budget || c.rest || i === li) return;
        const t = nextNonRest(seq,i);
        if(mod12(t.pc) === 0 || t.pc === c.pc || !inScale(t.pc) || t.q === "dim" || t.q === "hdim7" || mod12(t.pc - c.pc) === 5 || !can(i,1)) return;
        const tR = romanOf(t.pc, t.q);
        take({i, tail:[level === 2 ? {pc:mod12(t.pc+7), q:"7", dur:h, add:true, why:"dominante secundaria: V7 de "+tR}
                                   : {pc:mod12(t.pc+1), q:"7", dur:h, add:true, why:"sustituto tritonal de la dominante de "+tR}]}); });
      // 5) level 3: ♭VImaj7 inserted before the return to I (Ionian, Mixolydian)
      if(level === 3 && (mode === "Ionian" || mode === "Mixolydian")) seq.forEach((c,i) => { if(plan.length >= budget || c.rest) return;
        const t = nextNonRest(seq,i);
        if(mod12(t.pc) === 0 && mod12(c.pc) !== 0 && mod12(c.pc) !== 8 && !CADENCE.has(mod12(t.pc - c.pc)) && can(i,1)) take({i, tail:[{pc:8, q:"maj7", dur:h, add:true, why:"♭VImaj7 prestado antes de volver a I"}]}); });
      // apply: the original keeps the first part
      const out = [];
      seq.forEach((c,i) => {
        const p = plan.find(x => x.i === i);
        if(!p){ out.push(c); return; }
        if(p.head){ out.push(p.head); out.push({...c, dur:c.dur-h}); return; }
        out.push({...c, dur:c.dur - h*p.tail.length}); p.tail.forEach(a => out.push(a));
      });
      seq = out;
    }
    vs.push({seq, off:0});
  }
  return vs;
}
