const fs=require('fs');
eval(fs.readFileSync(__dirname+'/taller_core.js','utf8').replace(/^let mode/m,'var mode').replace(/^const (st|inScale|mod12|romanOf|arrange|color|tonicApproach|nextNonRest|CADENCE|MINORISH|Q|NUM|STEPS|MODES|IONIAN|majorish|diatonicQ|fmtDur) /gm,'var $1 ').replace(/^function /gm,'function '));
const clips=JSON.parse(fs.readFileSync('D:/Projects/ALWTTT25/Docs/reference/prueba-escucha/listen_clips.json','utf8'));
const QM={maj:'maj',min:'min',min7:'m7','7':'7',hdim7:'hdim7',maj7:'maj7'};
const STRONG=new Set([1,2,5,7,10,11]);
const st_=(a,b)=>STRONG.has(((b-a)%12+12)%12)?'S':'W';
function slots(seq){ // seq: [{pc,dur}] one lap, dur in measures
  const L=seq.reduce((s,c)=>s+c.dur,0), w=L/4; let t=0; const iv=seq.map(c=>{const o={pc:c.pc,a:t,b:t+c.dur,R:romanOf(c.pc,c.q)};t+=c.dur;return o;});
  const at=x=>iv.find(c=>c.a<=x+1e-9&&x<c.b-1e-9);
  const S=[0,1,2,3].map(k=>({ent:at(k*w),ex:at((k+1)*w-1e-6)}));
  const mot=[0,1,2,3].map(k=>st_(S[k].ex.pc,S[(k+1)%4].ent.pc)); // motion leaving slot k
  const anch=S.map((s,k)=>{const i=mot[(k+3)%4],o=mot[k];return i==='W'&&o==='W'?'isla':i==='S'&&o==='S'?'conector':i==='W'?'señal':'destino';});
  const nw=mot.filter(m=>m==='W').length; let type;
  if(nw===0) type='extremo (conectores)'; else if(nw===1) type='equilibrado: cascade'; else if(nw===2) type=(mot[0]===mot[2])?'extremo (señal/destino)':'equilibrado: disc'; else type='apagado';
  const lab=S.map(s=>s.ent.R+(s.ex!==s.ent?'→'+s.ex.R:''));
  return {lab, anch, mot:mot.join(''), type, dest:anch.map((a,k)=>a==='destino'?k+1:0).filter(Boolean)};
}
const seen=new Set();
for(const c of clips){ if(seen.has(c.id)) continue; seen.add(c.id);
  mode=c.mode; const B=4;
  const base=c.events.map(e=>({pc:e[2],q:QM[e[3]]||'maj',dur:e[1]/B}));
  const r1=slots(base);
  const out=[c.palette.padEnd(11),c.id.padEnd(38),r1.lab.join(' | ').padEnd(40),r1.anch.join(','),r1.type,'dest@'+r1.dest.join('+')];
  for(const lv of [2,3]){ for(const nV of [2,3]){ const v=arrange(base,lv,nV,B)[1].seq; const r=slots(v);
    if(r.anch.join()!==r1.anch.join()) out.push(`\n      N${lv} vuelta2${nV===2?'(última)':''}: ${v.map(x=>romanOf(x.pc,x.q)+'('+x.dur+')').join(' ')}  => ${r.anch.join(',')} ${r.type} dest@${r.dest.join('+')}`);}}
  console.log(out.join('  '));
}
