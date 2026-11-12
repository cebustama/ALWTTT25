/* motor-voz.js — Pink Trombone (fork de ALWTTT, POC-FORK 1–7) portado a JavaScript, más extensiones.
 *
 * PARTE 1 · FIEL. Port 1:1 de Assets/PinkTrombonePOC/PinkTromboneSrc (C#, que a su vez es el port de
 * lostmsu del original web de Neil Thapen, MIT). Glottis usa float (MathF) en C#: aquí cada operación
 * pasa por Math.fround. Tract usa double, igual que aquí. System.Random sembrado se reproduce con la
 * misma recurrencia de Knuth que .NET (Net5CompatSeedImpl), así que el ruido es el mismo.
 * Con todas las extensiones en su valor neutro, la salida debe coincidir con C# (paridad/).
 *
 * PARTE 2 · EXTENSIONES (propuesta, no existen en C#; cada una apagada por defecto):
 *   X1 constricciones libres (consonantes: oclusivas, fricativas, nasales) — algoritmo del original web
 *   X2 longitud del tracto (n ≠ 44: laringe alta o baja, voz de niño o de gigante)
 *   X3 glotis extra: subarmónico (growl), aspereza por ciclo (jitter/shimmer), ganancia de aire
 *
 * Sin DOM: corre en el navegador (window.MV) y en Node (module.exports).
 */
(function (root) {
  'use strict';
  const f = Math.fround;
  const PIf = f(Math.PI);

  // ---------------------------------------------------------------- System.Random (.NET, sembrado)
  const MBIG = 2147483647, MSEED = 161803398;
  class DotNetRandom {
    constructor(seed) {
      this.seed = seed | 0;
      const sa = new Int32Array(56);
      const subtraction = (seed === -2147483648) ? 2147483647 : Math.abs(seed | 0);
      let mj = MSEED - subtraction;
      sa[55] = mj;
      let mk = 1, ii = 0;
      for (let i = 1; i < 55; i++) {
        if ((ii += 21) >= 55) ii -= 55;
        sa[ii] = mk;
        mk = mj - mk;
        if (mk < 0) mk += MBIG;
        mj = sa[ii];
      }
      for (let k = 1; k < 5; k++) {
        for (let i = 1; i < 56; i++) {
          let n = i + 30;
          if (n >= 55) n -= 55;
          sa[i] = sa[i] - sa[1 + n];
          if (sa[i] < 0) sa[i] += MBIG;
        }
      }
      this.sa = sa; this.inext = 0; this.inextp = 21;
    }
    nextDouble() {
      let a = this.inext, b = this.inextp;
      if (++a >= 56) a = 1;
      if (++b >= 56) b = 1;
      let r = this.sa[a] - this.sa[b];
      if (r === MBIG) r--;
      if (r < 0) r += MBIG;
      this.sa[a] = r; this.inext = a; this.inextp = b;
      return r * (1.0 / MBIG);
    }
  }

  // ---------------------------------------------------------------- Noise (Noise.cs)
  function createFilteredNoiseSource(f0, q, sampleRate, bufferSize, random) {
    const buf = new Float64Array(bufferSize);
    for (let i = 0; i < bufferSize; i++) buf[i] = 2 * random.nextDouble() - 1;
    let idx = 0;
    const w0 = 2 * Math.PI * f0 / sampleRate;
    const alpha = Math.sin(w0) / (2 * q);
    const b0 = alpha, b1 = 0, b2 = -alpha, a0 = 1 + alpha, a1 = -2 * Math.cos(w0), a2 = 1 - alpha;
    const nb0 = b0 / a0, nb1 = b1 / a0, nb2 = b2 / a0, na1 = a1 / a0, na2 = a2 / a0;
    let x1 = 0, x2 = 0, y1 = 0, y2 = 0;
    return function () {
      if (idx >= bufferSize) idx = 0;
      const x = buf[idx++];
      const y = nb0 * x + nb1 * x1 + nb2 * x2 - na1 * y1 - na2 * y2;
      x2 = x1; x1 = x; y2 = y1; y1 = y;
      return f(y);
    };
  }

  // ---------------------------------------------------------------- NoiseGenerator.cs (simplex 2D, float)
  const GRAD3 = [[1,1,0],[-1,1,0],[1,-1,0],[-1,-1,0],[1,0,1],[-1,0,1],[1,0,-1],[-1,0,-1],[0,1,1],[0,-1,1],[0,1,-1],[0,-1,-1]];
  const P = [151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,190,6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,88,237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,200,196,135,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,221,153,101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,81,51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,184,84,204,176,115,121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180];
  const SQ3 = f(Math.sqrt(3));
  const F2 = f(f(0.5) * f(SQ3 - 1));
  const G2 = f(f(3 - SQ3) / 6);
  const G2x2 = f(2 * G2);
  const C12 = f(1.2), C07 = f(0.7);
  class NoiseGenerator {
    constructor(seed) {
      this.perm = new Int32Array(512); this.gx = new Float32Array(512); this.gy = new Float32Array(512);
      seed &= 0xFFFF;
      if (seed < 256) seed = (seed | (seed << 8)) & 0xFFFF;
      for (let i = 0; i < 256; i++) {
        const v = (i & 1) === 1 ? (P[i] ^ (seed & 255)) : (P[i] ^ ((seed >> 8) & 255));
        this.perm[i] = this.perm[i + 256] = v;
        const g = GRAD3[v % 12];
        this.gx[i] = this.gx[i + 256] = g[0]; this.gy[i] = this.gy[i + 256] = g[1];
      }
    }
    simplex2(xin, yin) {
      const s = f(f(xin + yin) * F2);
      let i = Math.floor(f(xin + s)), j = Math.floor(f(yin + s));
      const t = f(f(i + j) * G2);
      const x0 = f(f(xin - i) + t), y0 = f(f(yin - j) + t);
      let i1, j1;
      if (x0 > y0) { i1 = 1; j1 = 0; } else { i1 = 0; j1 = 1; }
      const x1 = f(f(x0 - i1) + G2), y1 = f(f(y0 - j1) + G2);
      const x2 = f(f(x0 - 1) + G2x2), y2 = f(f(y0 - 1) + G2x2);
      i &= 255; j &= 255;
      const perm = this.perm, gx = this.gx, gy = this.gy;
      const g0 = i + perm[j], g1 = i + i1 + perm[j + j1], g2 = i + 1 + perm[j + 1];
      let n0, n1, n2;
      let t0 = f(f(f(0.5) - f(x0 * x0)) - f(y0 * y0));
      if (t0 < 0) n0 = 0; else { t0 = f(t0 * t0); n0 = f(f(t0 * t0) * f(f(gx[g0] * x0) + f(gy[g0] * y0))); }
      let t1 = f(f(f(0.5) - f(x1 * x1)) - f(y1 * y1));
      if (t1 < 0) n1 = 0; else { t1 = f(t1 * t1); n1 = f(f(t1 * t1) * f(f(gx[g1] * x1) + f(gy[g1] * y1))); }
      let t2 = f(f(f(0.5) - f(x2 * x2)) - f(y2 * y2));
      if (t2 < 0) n2 = 0; else { t2 = f(t2 * t2); n2 = f(f(t2 * t2) * f(f(gx[g2] * x2) + f(gy[g2] * y2))); }
      return f(70 * f(f(n0 + n1) + n2));
    }
    simplex(x) { return this.simplex2(f(x * C12), f(f(-x) * C07)); }
  }

  // ---------------------------------------------------------------- small seeded RNG for the extensions
  function mulberry32(a) {
    return function () {
      a |= 0; a = (a + 0x6D2B79F5) | 0;
      let t = Math.imul(a ^ (a >>> 15), 1 | a);
      t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
      return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
  }

  // float constants used by Glottis
  const K = {
    c013: f(0.13), c005: f(0.05), c01: f(0.1), c002: f(0.02), c004: f(0.04), c02: f(0.2), c04: f(0.4),
    c199: f(1.99), c046: f(0.46), c036: f(0.36), c407: f(4.07), c215: f(2.15), c096: f(0.96), c05: f(0.5),
    c03: f(0.3), c025: f(0.25), m001: f(-0.01), c0048: f(0.048), c0224: f(0.224), c0118: f(0.118),
    c12: f(1.2), c011: f(0.11), c05b: f(0.5), twelfth: f(1 / 12), PI2: f(PIf * 2),
  };

  // ---------------------------------------------------------------- Glottis.cs
  class Glottis {
    constructor(sampleRate, random) {
      this.alwaysVoice = true; this.autoWobble = true; this.isTouched = false;
      this.targetTenseness = f(0.6); this.targetFrequency = 140;
      this.vibratoAmount = f(0.005); this.vibratoFrequency = 6;
      this.pitchJitterGain = 1; this.tensenessJitterGain = 1;
      this.sampleRate = sampleRate; this.sampleCount = 0;
      this.intensity = 0; this.waveformLoudness = 1; this.userLoudness = 1;
      this.smoothFrequency = 140; this.timeInWaveform = 0;
      this.oldTenseness = f(0.6); this.newTenseness = f(0.6);
      this.oldFrequency = 140; this.newFrequency = 140;
      this.noise = new NoiseGenerator(random.seed >>> 0);
      this.aspirationNoiseSource = createFilteredNoiseSource(500, 0.5, sampleRate, 0x8000, random);
      this.invRate = f(1 / sampleRate);
      // X3 extensions (neutral values = exact fork behavior)
      this.x = { sub: 0, jitter: 0, shimmer: 0, air: 1 };
      this.cycle = 0; this.cycAmp = 1; this.xrand = mulberry32(((random.seed | 0) ^ 0x5bd1e995) | 0);
      this.setupWaveform(0);
    }
    setMusicalNote(semitone) {
      this.targetFrequency = f(440 * f(Math.pow(2, f(f(semitone) * K.twelfth))));
    }
    step(lambda) {
      const time = f(f(this.sampleCount) / this.sampleRate);
      if (this.timeInWaveform > this.waveformLength) {
        this.timeInWaveform = f(this.timeInWaveform - this.waveformLength);
        this.setupWaveform(lambda);
      }
      const out1 = this.normalizedLF(f(this.timeInWaveform / this.waveformLength));
      const asp = this.aspirationNoiseSource();
      const tt = this.targetTenseness;
      const aspiration1 = f(f(f(this.intensity * f(1 - f(Math.sqrt(tt)))) * this.getNoiseModulator()) * asp);
      let aspiration2 = f(aspiration1 * f(K.c02 + f(K.c002 * this.noise.simplex(f(time * K.c199)))));
      if (this.x.air !== 1) aspiration2 = f(aspiration2 * this.x.air);
      const result = f(out1 + aspiration2);
      this.sampleCount++;
      this.timeInWaveform = f(this.timeInWaveform + this.invRate);
      return result;
    }
    getNoiseModulator() {
      const voiced = f(K.c01 + f(K.c02 * Math.max(0, f(Math.sin(f(f(K.PI2 * this.timeInWaveform) / this.waveformLength))))));
      const ti = f(this.targetTenseness * this.intensity);
      return f(f(ti * voiced) + f(f(1 - ti) * K.c03));
    }
    adjustParameters(deltaTime) {
      const delta = f(f(deltaTime * this.sampleRate) / 512);
      const oldTime = f(f(this.sampleCount) / this.sampleRate);
      const newTime = f(oldTime + deltaTime);
      // adjustIntensity
      if (this.isTouched || this.alwaysVoice) this.intensity = f(this.intensity + f(K.c013 * delta));
      else this.intensity = f(this.intensity - f(K.c005 * delta));
      this.intensity = Math.min(1, Math.max(this.intensity, 0));
      // calculateNewFrequency
      if (this.intensity === 0) this.smoothFrequency = this.targetFrequency;
      else if (this.targetFrequency > this.smoothFrequency)
        this.smoothFrequency = Math.min(f(this.smoothFrequency * f(1 + f(K.c01 * delta))), this.targetFrequency);
      else if (this.targetFrequency < this.smoothFrequency)
        this.smoothFrequency = Math.max(f(this.smoothFrequency / f(1 + f(K.c01 * delta))), this.targetFrequency);
      this.oldFrequency = this.newFrequency;
      this.newFrequency = Math.max(10, f(this.smoothFrequency * f(1 + this.calculateVibrato(newTime))));
      // calculateNewTenseness
      this.oldTenseness = this.newTenseness;
      const tj = this.tensenessJitterGain, tt = this.targetTenseness, n = this.noise;
      this.newTenseness = Math.max(0, f(f(tt + f(f(tj * K.c01) * n.simplex(f(newTime * K.c046)))) + f(f(tj * K.c005) * n.simplex(f(newTime * K.c036)))));
      if (!this.isTouched && this.alwaysVoice)
        this.newTenseness = f(this.newTenseness + f(f(3 - tt) * f(1 - this.intensity)));
    }
    calculateVibrato(time) {
      const n = this.noise, pj = this.pitchJitterGain;
      let v = f(this.vibratoAmount * f(Math.sin(f(f(K.PI2 * time) * this.vibratoFrequency))));
      v = f(v + f(f(pj * K.c002) * n.simplex(f(time * K.c407))));
      v = f(v + f(f(pj * K.c004) * n.simplex(f(time * K.c215))));
      if (this.autoWobble) {
        v = f(v + f(K.c02 * n.simplex(f(time * K.c096))));
        v = f(v + f(K.c04 * n.simplex(f(time * K.c05))));
      }
      return v;
    }
    setupWaveform(lambda) {
      const frequency = f(this.oldFrequency + f(lambda * f(this.newFrequency - this.oldFrequency)));
      const tenseness = f(this.oldTenseness + f(lambda * f(this.newTenseness - this.oldTenseness)));
      this.waveformLength = f(1 / frequency);
      this.waveformLoudness = f(Math.pow(Math.max(0, tenseness), 0.25));
      const rd = Math.min(2.7, Math.max(f(3 * f(1 - tenseness)), 0.5));
      const ra = f(K.m001 + f(K.c0048 * rd));
      const rk = f(K.c0224 + f(K.c0118 * rd));
      const q = f(K.c05b + f(K.c12 * rk));
      const rg = f(f(f(rk / 4) * q) / f(f(K.c011 * rd) - f(ra * q)));
      const ta = ra;
      const tp = f(1 / f(2 * rg));
      const te = f(tp + f(tp * rk));
      const epsilon = f(1 / ta);
      const shift = f(Math.exp(f(f(-epsilon) * f(1 - te))));
      const delta = f(1 - shift);
      const rhsIntegral = f(f(f(f(1 / epsilon) * f(shift - 1)) + f(f(1 - te) * shift)) / delta);
      const totalLowerIntegral = f(rhsIntegral - f(f(te - tp) / 2));
      const totalUpperIntegral = f(-totalLowerIntegral);
      const omega = f(PIf / tp);
      const s = f(Math.sin(f(omega * te)));
      const y = f(f(f(f(-PIf) * s) * totalUpperIntegral) / f(tp * 2));
      const z = f(Math.log(y));
      const alpha = f(z / f(f(tp / 2) - te));
      const e0 = f(-1 / f(s * f(Math.exp(f(alpha * te)))));
      this.alpha = alpha; this.e0 = e0; this.epsilon = epsilon; this.shift = shift;
      this.delta = delta; this.te = te; this.omega = omega;
      // X3: per-cycle extras (only when active, so the default path is untouched)
      const x = this.x;
      if (x.sub > 0 || x.jitter > 0 || x.shimmer > 0) {
        this.cycle++;
        let per = 1, amp = 1;
        if (x.sub > 0) {               // period doubling: alternate long/strong and short/weak cycles
          const odd = (this.cycle & 1) === 1;
          per *= odd ? 1 + 0.22 * x.sub : 1 - 0.22 * x.sub;
          amp *= odd ? 1 : 1 - 0.75 * x.sub;
        }
        if (x.jitter > 0) per *= 1 + x.jitter * 0.25 * (this.xrand() * 2 - 1);
        if (x.shimmer > 0) amp *= 1 - x.shimmer * 0.6 * this.xrand();
        this.waveformLength = f(this.waveformLength * per);
        this.cycAmp = amp;
      } else this.cycAmp = 1;
    }
    normalizedLF(t) {
      const out = t > this.te
        ? f(f(f(-f(Math.exp(f(f(-this.epsilon) * f(t - this.te))))) + this.shift) / this.delta)
        : f(f(this.e0 * f(Math.exp(f(this.alpha * t)))) * f(Math.sin(f(this.omega * t))));
      let r = f(f(f(out * this.intensity) * this.waveformLoudness) * this.userLoudness);
      if (this.cycAmp !== 1) r = f(r * this.cycAmp);
      return r;
    }
  }

  // ---------------------------------------------------------------- Tract.cs (+ X2 length)
  class Tract {
    constructor(glottis, sampleRate, random, n) {
      n = n || 44;
      this.glottis = glottis; this.sampleRate = sampleRate;
      this.fricationNoiseSource = createFilteredNoiseSource(1000, 0.5, sampleRate, 0x8000, random);
      const sc = n / 44;
      this.n = n;
      this.bladeStart = n === 44 ? 10 : Math.round(10 * sc);
      this.tipStart = n === 44 ? 32 : Math.round(32 * sc);
      this.lipStart = n === 44 ? 39 : Math.round(39 * sc);
      this.noseLength = n === 44 ? 28 : Math.round(28 * sc);
      this.noseStart = n - this.noseLength + 1;
      this.glottalEnd = n === 44 ? 7 : Math.round(7 * sc);
      this.sampleCount = 0; this.time = 0;
      const D = (k) => new Float64Array(k);
      this.right = D(n); this.left = D(n); this.reflection = D(n); this.newReflection = D(n);
      this.junctionOutputRight = D(n); this.junctionOutputLeft = D(n + 1); this.diameter = D(n);
      const NL = this.noseLength;
      this.noseRight = D(NL); this.noseLeft = D(NL); this.noseJunctionOutputRight = D(NL);
      this.noseJunctionOutputLeft = D(NL + 1); this.noseReflection = D(NL); this.noseDiameter = D(NL);
      this.reflectionLeft = this.reflectionRight = this.reflectionNose = 0;
      this.newReflectionLeft = this.newReflectionRight = this.newReflectionNose = 0;
      this.transients = []; this.turbulencePoints = [];
      this.lipOut = 0; this.noseOut = 0;
    }
    calculateNoseReflections() {
      const NL = this.noseLength, a = new Float64Array(NL);
      for (let i = 0; i < NL; i++) a[i] = Math.max(1e-6, this.noseDiameter[i] * this.noseDiameter[i]);
      for (let i = 1; i < NL; i++) this.noseReflection[i] = (a[i - 1] - a[i]) / (a[i - 1] + a[i]);
    }
    calculateNewBlockParameters() {
      const n = this.n, d = this.diameter;
      let prev = d[0] * d[0];
      for (let i = 1; i < n; i++) {
        const ai = d[i] * d[i];
        this.reflection[i] = this.newReflection[i];
        const sum = prev + ai;
        this.newReflection[i] = (Math.abs(sum) > 1e-6) ? (prev - ai) / sum : 1;
        prev = ai;
      }
      this.reflectionLeft = this.newReflectionLeft;
      this.reflectionRight = this.newReflectionRight;
      this.reflectionNose = this.newReflectionNose;
      const NS = this.noseStart;
      const velumA = this.noseDiameter[0] * this.noseDiameter[0];
      const an0 = d[NS] * d[NS], an1 = d[NS + 1] * d[NS + 1];
      const sum = an0 + an1 + velumA;
      const ok = Math.abs(sum) > 1e-6;
      this.newReflectionLeft = ok ? (2 * an0 - sum) / sum : 1;
      this.newReflectionRight = ok ? (2 * an1 - sum) / sum : 1;
      this.newReflectionNose = ok ? (2 * velumA - sum) / sum : 1;
    }
    step(glottalOutput, lambda) {
      const n = this.n;
      const R = this.right, L = this.left, JR = this.junctionOutputRight, JL = this.junctionOutputLeft;
      // transients
      for (let i = this.transients.length - 1; i >= 0; i--) {
        const tr = this.transients[i];
        const alive = f(this.time - tr.startTime);
        if (alive > tr.lifeTime) { this.transients.splice(i, 1); continue; }
        const amp = tr.strength * Math.pow(2, -tr.exponent * alive);
        R[tr.position] += amp * 0.5; L[tr.position] += amp * 0.5;
      }
      // turbulence
      const tps = this.turbulencePoints;
      for (let k = 0; k < tps.length; k++) {
        const p = tps[k];
        if (p.position < 2 || p.position > n) continue;
        if (p.diameter <= 0) continue;
        let intensity;
        if (Number.isNaN(p.endTime)) intensity = Math.min(1, Math.max((this.time - p.startTime) / 0.1, 0));
        else intensity = Math.min(1, Math.max(1 - (this.time - p.endTime) / 0.1, 0));
        if (intensity <= 0) continue;
        const noise = 0.66 * this.fricationNoiseSource() * intensity * this.glottis.getNoiseModulator();
        const i0 = Math.floor(p.position), dl = p.position - i0;
        const thin = Math.min(1, Math.max(8 * (0.7 - p.diameter), 0));
        const open = Math.min(1, Math.max(30 * (p.diameter - 0.3), 0));
        const n0 = noise * (1 - dl) * thin * open, n1 = noise * dl * thin * open;
        if (i0 + 1 < n) { R[i0 + 1] += n0 * 0.5; L[i0 + 1] += n0 * 0.5; }
        if (i0 + 2 < n) { R[i0 + 2] += n1 * 0.5; L[i0 + 2] += n1 * 0.5; }
      }
      JR[0] = L[0] * 0.75 + glottalOutput;
      JL[n] = R[n - 1] * -0.85;
      const rf = this.reflection, nrf = this.newReflection;
      for (let i = 1; i < n; i++) {
        const r = rf[i] + lambda * (nrf[i] - rf[i]);
        const w = r * (R[i - 1] + L[i]);
        JR[i] = R[i - 1] - w;
        JL[i] = L[i] + w;
      }
      const NS = this.noseStart, NLf = this.noseLeft;
      {
        let r = this.reflectionLeft + lambda * (this.newReflectionLeft - this.reflectionLeft);
        JL[NS] = r * R[NS - 1] + (1 + r) * (NLf[0] + L[NS]);
        r = this.reflectionRight + lambda * (this.newReflectionRight - this.reflectionRight);
        JR[NS] = r * L[NS] + (1 + r) * (R[NS - 1] + NLf[0]);
        r = this.reflectionNose + lambda * (this.newReflectionNose - this.reflectionNose);
        this.noseJunctionOutputRight[0] = r * NLf[0] + (1 + r) * (L[NS] + R[NS - 1]);
      }
      for (let i = 0; i < n; i++) { R[i] = JR[i] * 0.999; L[i] = JL[i + 1] * 0.999; }
      const lipOutput = R[n - 1];
      const NL = this.noseLength, NR = this.noseRight, NJR = this.noseJunctionOutputRight, NJL = this.noseJunctionOutputLeft, nrefl = this.noseReflection;
      NJL[NL] = NR[NL - 1] * -0.85;
      for (let i = 1; i < NL; i++) {
        const w = nrefl[i] * (NR[i - 1] + NLf[i]);
        NJR[i] = NR[i - 1] - w;
        NJL[i] = NLf[i] + w;
      }
      for (let i = 0; i < NL; i++) { NR[i] = NJR[i]; NLf[i] = NJL[i + 1]; }
      const noseOutput = NR[NL - 1];
      this.sampleCount++;
      this.time = f(f(this.sampleCount) / this.sampleRate);
      return f(lipOutput + noseOutput);
    }
  }

  // ---------------------------------------------------------------- TractShaper.cs (+ X1 constrictions)
  class TractShaper {
    constructor(tract) {
      this.tract = tract;
      this.velumOpenTarget = f(0.4); this.velumClosedTarget = f(0.01);
      this.targetDiameter = new Float64Array(tract.n);
      this.tongueIndex = 12.9; this.tongueDiameter = 2.43; this.tongueDirty = false;
      this.lastObstruction = -1;
      this.constrictions = [];      // X1: [{index (44-space), diameter}] ; empty = fork behavior
      this.constrDirty = false;
      this.velumOverride = null;    // X1: null = fork behavior; number = velum target
      this.shapeNose(true);
      tract.calculateNoseReflections();
      this.shapeNose(false);
      for (let i = 0; i < tract.n; i++) { const d = this.getRestDiameter(i); tract.diameter[i] = d; this.targetDiameter[i] = d; }
    }
    setTongue(idx, dia) {
      if (idx !== this.tongueIndex) { this.tongueIndex = idx; this.tongueDirty = true; }
      if (dia !== this.tongueDiameter) { this.tongueDiameter = dia; this.tongueDirty = true; }
    }
    setConstrictions(list) { this.constrictions = list || []; this.constrDirty = true; }
    getRestDiameter(i) {
      const T = this.tract;
      if (i < T.glottalEnd) return 0.6;
      if (i < T.bladeStart) return 1.1;
      if (i >= T.lipStart) return 1.5;
      const ti = T.n === 44 ? this.tongueIndex : this.tongueIndex * T.n / 44;
      const t = 1.1 * Math.PI * (ti - i) / (T.tipStart - T.bladeStart);
      const fixed = 2 + (this.tongueDiameter - 2) / 1.5;
      let curve = (1.5 - fixed + 1.7) * Math.cos(t);
      if (i === T.bladeStart - 2 || i === T.lipStart - 1) curve *= 0.8;
      if (i === T.bladeStart || i === T.lipStart - 2) curve *= 0.94;
      return 1.5 - curve;
    }
    applyConstrictions() {
      // Original web Pink Trombone (TractUI.setRestDiameter, touch loop), with the UI's
      // "diameter -= 0.3" already applied by the caller: 0 = closed.
      const T = this.tract, n = T.n, td = this.targetDiameter, sc = n / 44;
      for (const c of this.constrictions) {
        const index = c.index * sc, diameter = Math.max(0, c.diameter);
        let width;
        if (index < 25 * sc) width = 10 * sc;
        else if (index >= T.tipStart) width = 5 * sc;
        else width = (10 - 5 * (index - 25 * sc) / (T.tipStart - 25 * sc)) * sc;
        if (index >= 2 && index < n && diameter < 3) {
          const ii = Math.round(index);
          for (let i = -Math.ceil(width) - 1; i < width + 1; i++) {
            const k = ii + i;
            if (k < 0 || k >= n) continue;
            let rel = Math.abs(k - index) - 0.5, shrink;
            if (rel <= 0) shrink = 0;
            else if (rel > width) shrink = 1;
            else shrink = 0.5 * (1 - Math.cos(Math.PI * rel / width));
            if (diameter < td[k]) td[k] = diameter + (td[k] - diameter) * shrink;
          }
        }
      }
    }
    adjustTractShape(deltaTime) {
      const T = this.tract, n = T.n;
      if (this.tongueDirty || this.constrDirty) {
        this.tongueDirty = false;
        for (let i = 0; i < n; i++) this.targetDiameter[i] = this.getRestDiameter(i);
        if (this.constrictions.length) this.applyConstrictions();
        this.constrDirty = this.constrictions.length > 0;
      }
      const amount = deltaTime * 15;
      let newLast = -1;
      for (let i = 0; i < n; i++) {
        const d = T.diameter[i], td = this.targetDiameter[i];
        if (d <= 0) newLast = i;
        let slow;
        if (i < T.noseStart) slow = 0.6;
        else if (i >= T.tipStart) slow = 1;
        else slow = 0.6 + 0.4 * (i - T.noseStart) / (T.tipStart - T.noseStart);
        const up = slow * amount, down = 2 * amount;
        T.diameter[i] = d < td ? Math.min(d + up, td) : Math.max(d - down, td);
      }
      if (this.lastObstruction >= 0 && newLast < 0 && T.noseDiameter[0] < 0.223) {
        T.transients.push({ position: this.lastObstruction, startTime: T.time, lifeTime: 0.2, strength: 0.3, exponent: 200 });
      }
      this.lastObstruction = newLast;
      const vt = this.velumOverride == null ? this.velumTarget : this.velumOverride;
      const v = T.noseDiameter[0], up = amount * 0.25, down = amount * 0.1;
      T.noseDiameter[0] = v < vt ? Math.min(v + up, vt) : Math.max(v - down, vt);
    }
    shapeNose(open) {
      const T = this.tract, NL = T.noseLength;
      this.velumTarget = open ? this.velumOpenTarget : this.velumClosedTarget;
      for (let i = 0; i < NL; i++) {
        const d = i * 2.0 / NL;
        let dia;
        if (i === 0) dia = this.velumTarget;
        else if (d < 1) dia = 0.4 + 1.6 * d;
        else dia = 0.5 + 1.5 * (2 - d);
        T.noseDiameter[i] = Math.min(dia, 1.9);
      }
    }
  }

  // ---------------------------------------------------------------- PinkTrombone.cs facade
  class PinkTrombone {
    constructor(sampleRate, seed, opts) {
      opts = opts || {};
      const random = new DotNetRandom(seed == null ? 12345 : seed);
      this.sampleRate = sampleRate;
      this.glottis = new Glottis(sampleRate, random);
      this.tract = new Tract(this.glottis, 2 * sampleRate, random, opts.tractLength || 44);
      this.shaper = new TractShaper(this.tract);
    }
    set intensity(v) { this.glottis.intensity = f(v); }
    set loudness(v) { this.glottis.userLoudness = f(v); }
    set isTouched(v) { this.glottis.isTouched = !!v; }
    get isTouched() { return this.glottis.isTouched; }
    set alwaysVoice(v) { this.glottis.alwaysVoice = !!v; }
    set targetFrequency(v) { this.glottis.targetFrequency = f(v); }
    set targetTenseness(v) { this.glottis.targetTenseness = f(v); }
    set vibratoGain(v) { this.glottis.vibratoAmount = f(v); }
    set vibratoFrequency(v) { this.glottis.vibratoFrequency = f(v); }
    set vibratoWobble(v) { this.glottis.autoWobble = !!v; }
    set pitchJitterGain(v) { this.glottis.pitchJitterGain = f(v); }
    set tensenessJitterGain(v) { this.glottis.tensenessJitterGain = f(v); }
    setTongue(idx, dia) { this.shaper.setTongue(idx, dia); }
    setMusicalNote(semitone) { this.glottis.setMusicalNote(semitone); }
    reset() { this.calculateNewBlockParameters(0); }
    calculateNewBlockParameters(dt) {
      this.glottis.adjustParameters(dt);
      this.shaper.adjustTractShape(dt);
      this.tract.calculateNewBlockParameters();
    }
    /** Synthesize buf.length samples into buf (Float32Array) starting at offset off, count len. */
    synthesize(buf, off, len) {
      off = off || 0; len = len == null ? buf.length - off : len;
      let p = 0;
      while (p < len) {
        const bl = Math.min(512, len - p);
        this.synthesizeBlock(buf, off + p, bl);
        p += bl;
      }
    }
    synthesizeBlock(buf, off, len) {
      const dt = f(len / this.sampleRate);
      this.calculateNewBlockParameters(dt);
      const g = this.glottis, t = this.tract, c0125 = f(0.125);
      for (let i = 0; i < len; i++) {
        const l1 = i / len, l2 = (i + 0.5) / len;
        const go = g.step(f(l1));
        const v1 = t.step(go, l1);
        const v2 = t.step(go, l2);
        buf[off + i] = f(f(v1 + v2) * c0125);
      }
    }
  }

  const api = { PinkTrombone, DotNetRandom, NoiseGenerator, mulberry32 };
  if (typeof module !== 'undefined' && module.exports) module.exports = api;
  else root.MV = api;
})(typeof window !== 'undefined' ? window : globalThis);
