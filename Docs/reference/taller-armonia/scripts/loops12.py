"""Pilot: 12tone loop-shape model on McGill Billboard 2.0 (salami_chords). Read-only."""
import re, sys, collections
from fractions import Fraction as F
from pathlib import Path
ROOT = Path(sys.argv[1])
NOTE = {"C":0,"D":2,"E":4,"F":5,"G":7,"A":9,"B":11}
STRONG = {1,2,5,7,10,11}
def fam(q):
    q=q.split("/")[0]
    for p,f in (("hdim","hd"),("dim","dim"),("aug","aug"),("min","min"),("sus","sus"),("maj","maj"),("7","maj"),("9","maj"),("1","maj"),("5","maj"),("11","maj"),("13","maj")):
        if q.startswith(p): return f
    return "maj"
def chord(tok, tonic):
    m=re.match(r"([A-G])([b#]*):(\S+)",tok)
    if not m: return None
    pc=(NOTE[m[1]]+m[2].count("#")-m[2].count("b")-tonic)%12
    return (pc, fam(m[3]))
def song_bars(f):
    tonic=None; meter=F(4,4); bars=[]; title=""
    for line in f.read_text(encoding="utf-8",errors="replace").splitlines():
        if line.startswith("# tonic:"):
            t=line[8:].strip(); tonic=(NOTE[t[0]]+t.count("#")-t.count("b"))%12 if t and t[0] in NOTE else None; continue
        if line.startswith("# metre:"):
            a,b=line[8:].strip().split("/"); meter=F(int(a),int(b)); continue
        if line.startswith("# title:"): title=line[8:].strip(); continue
        if line.startswith("#") or tonic is None: continue
        if "|" not in line:
            bars.append(None); continue   # silence/end: break
        body=line.split("\t",1)[-1]
        seg=body[body.find("|"):body.rfind("|")+1]
        m=re.search(r"\|\s*x(\d+)",body); reps=int(m.group(1)) if m else 1
        start=len(bars)
        line_bars=[]
        for b in seg.split("|")[1:-1]:
            toks=b.split(); length=F(1)
            if toks and toks[0].startswith("("):
                a,c=toks[0].strip("()").split("/"); length=F(int(a),int(c))/meter; toks=toks[1:]
            toks=[t for t in toks if t!="*"]
            if not toks: continue
            ev=[]; prev=None
            for t in toks:
                if t==".": c=prev
                elif t in("N","X","&pause"): c=None
                else: c=chord(t,tonic)
                ev.append(c); prev=c
            d=length/len(ev)
            line_bars.append(tuple((c,d) for c in ev))
        for r in range(reps):
            for i,b in enumerate(line_bars): bars.append((b, r==0 and i==0))  # flag: phrase (line) start
    return title, bars
def unit_events(bars):
    ev=[]
    for b,_ in bars:
        for c,d in b:
            if ev and ev[-1][0]==c: ev[-1][1]+=d
            else: ev.append([c,d])
    if len(ev)>1 and ev[0][0]==ev[-1][0]:  # wrap: same chord across the loop seam
        pass
    return ev
def find_loops(bars):
    """maximal runs of a P-bar unit repeated >=3 times (>=8 bars). returns list of (start,P,span)."""
    n=len(bars); out=[]; covered=[False]*n
    for P in (1,2,3,4,6,8):
        i=0
        while i+P<n:
            if bars[i] is None: i+=1; continue
            j=i
            while j+P<n and bars[j] is not None and bars[j+P] is not None and bars[j][0]==bars[j+P][0]: j+=1
            span=(j-i)+P
            if span>=max(3*P,8) and not any(covered[i:i+span]):
                unit=bars[i:i+P]
                # prime period check
                cs=[c for b,_ in unit for c,_ in b]
                if None not in cs and len(set(cs))>=2:
                    out.append((i,P,span))
                    for k in range(i,i+span): covered[k]=True
                i=j+1
            else: i+=1
    return out
def slots(ev):
    L=sum(d for _,d in ev); w=L/4; iv=[]; t=F(0)
    for c,d in ev: iv.append((c,t,t+d)); t+=d
    at=lambda x: next(c for c,a,b in iv if a<=x<b)
    S=[(at(k*w), at((k+1)*w - L/10000)) for k in range(4)]
    mot=[]
    for k in range(4):
        a=S[k][1][0]; b=S[(k+1)%4][0][0]
        mot.append("S" if (b-a)%12 in STRONG else "W")
    return S,mot
def shape(mot):
    nw=mot.count("W")
    if nw==0: return "extremo-conectores"
    if nw==1: return "cascade"
    if nw==2: return "extremo-senal/destino" if mot[0]==mot[2] else "disc"
    return "apagado"
def anchors(mot):
    r=[]
    for k in range(4):
        i,o=mot[(k-1)%4],mot[k]
        r.append({"WW":"isla","SS":"conector","WS":"senal","SW":"destino"}[i+o])
    return r
songs=0; withloop=0; whole=0; nchords=collections.Counter(); shapes=collections.Counter(); dest=collections.Counter()
shapes_by_n=collections.defaultdict(collections.Counter); tonic_slot1=collections.Counter(); dest_tonic=collections.Counter()
seen=set(); examples=collections.defaultdict(list); semis=collections.Counter()
for f in sorted(ROOT.rglob("salami_chords.txt")):
    title,bars=song_bars(f)
    if title in seen or not bars: continue
    seen.add(title); songs+=1
    loops=find_loops(bars)
    if not loops: continue
    # main loop: group by rotation-invariant unit content, max coverage
    groups=collections.defaultdict(lambda:[0,None])
    for i,P,span in loops:
        ev=unit_events(bars[i:i+P])
        key=tuple(sorted(c for c,_ in ev))+(P,)
        g=groups[key]; g[0]+=span
        if g[1] is None: g[1]=(i,P)
    key,(cov,(i,P))=max(groups.items(),key=lambda kv:kv[1][0])
    ev=unit_events(bars[i:i+P])
    chorded=sum(1 for b in bars if b is not None)
    withloop+=1
    if cov>=0.8*chorded: whole+=1
    n=len(ev) - (1 if len(ev)>1 and ev[0][0]==ev[-1][0] else 0)
    nchords[n]+=1
    if n>8: continue
    S,mot=slots(ev); sh=shape(mot); an=anchors(mot)
    shapes[sh]+=1; shapes_by_n[min(n,6)][sh]+=1
    for k,a in enumerate(an):
        if a=="destino":
            dest[k+1]+=1; dest_tonic[(k+1, S[k][0][0]==0)]+=1
    tonic_slot1[S[0][0][0]==0]+=1
    if len(examples[sh])<4: examples[sh].append((title," ".join(f"{c[0]}{c[1][:3]}" for c,_ in ev)))
print("songs",songs,"with loop",withloop,"whole-song (>=80% bars)",whole)
print("chord count in main loop",sorted(nchords.items()))
tot=sum(shapes.values()); print("shapes (n<=8):",tot)
for k,v in shapes.most_common(): print(f"  {k:24s}{v:4d} {100*v/tot:5.1f}%")
print("destinations by slot",sorted(dest.items()))
print("  (slot, slot-1-is-tonic?)",sorted(dest_tonic.items()))
print("slot 1 starts on tonic:",dict(tonic_slot1))
print("shape by chord count (6 = 6+):")
for n in sorted(shapes_by_n): print("  ",n,dict(shapes_by_n[n]))
for k,v in examples.items(): print(k, v)

# --- control: destinations whose weak exit is a held chord (0) vs a real third/tritone
d_held=collections.Counter(); d_real=collections.Counter(); d_n4=collections.Counter(); weak_kind=collections.Counter()
seen=set()
for f in sorted(ROOT.rglob("salami_chords.txt")):
    title,bars=song_bars(f)
    if title in seen or not bars: continue
    seen.add(title)
    loops=find_loops(bars)
    if not loops: continue
    groups=collections.defaultdict(lambda:[0,None])
    for i,P,span in loops:
        ev=unit_events(bars[i:i+P]); key=tuple(sorted(c for c,_ in ev))+(P,)
        g=groups[key]; g[0]+=span
        if g[1] is None: g[1]=(i,P)
    key,(cov,(i,P))=max(groups.items(),key=lambda kv:kv[1][0])
    ev=unit_events(bars[i:i+P]); n=len(ev)-(1 if len(ev)>1 and ev[0][0]==ev[-1][0] else 0)
    if n>8: continue
    S,mot=slots(ev); an=anchors(mot)
    for k in range(4):
        a=S[k][1][0]; b=S[(k+1)%4][0][0]; iv=(b-a)%12
        if mot[k]=="W": weak_kind["held(0)" if iv==0 and S[k][1]==S[(k+1)%4][0] else "same root, other quality" if iv==0 else "third/tritone"]+=1
        if an[k]=="destino":
            held = S[k][1]==S[(k+1)%4][0]
            (d_held if held else d_real)[k+1]+=1
            if n>=4: d_n4[k+1]+=1
print("weak boundaries by kind",dict(weak_kind))
print("destinations, weak exit = held chord:",sorted(d_held.items()))
print("destinations, weak exit = real third/tritone:",sorted(d_real.items()))
print("destinations in loops with >=4 chords:",sorted(d_n4.items()))

tab=collections.Counter(); seen=set()
for f in sorted(ROOT.rglob("salami_chords.txt")):
    title,bars=song_bars(f)
    if title in seen or not bars: continue
    seen.add(title); loops=find_loops(bars)
    if not loops: continue
    groups=collections.defaultdict(lambda:[0,None])
    for i,P,span in loops:
        ev=unit_events(bars[i:i+P]); key=tuple(sorted(c for c,_ in ev))+(P,)
        g=groups[key]; g[0]+=span
        if g[1] is None: g[1]=(i,P)
    key,(cov,(i,P))=max(groups.items(),key=lambda kv:kv[1][0])
    ev=unit_events(bars[i:i+P]); n=len(ev)-(1 if len(ev)>1 and ev[0][0]==ev[-1][0] else 0)
    if n>8 or n<3: continue
    S,mot=slots(ev); an=anchors(mot)
    for k in range(4):
        held = S[k][1]==S[(k+1)%4][0]
        if held: continue
        tab[(S[k][1][0]==0, an[k])]+=1
for t in (True,False):
    tot=sum(v for (a,b),v in tab.items() if a==t)
    print("exit chord is tonic" if t else "exit chord not tonic", tot, {b:round(v/tot,2) for (a,b),v in tab.items() if a==t})
