// [VOZ-3] Reads a voice style copied from the Taller de melodías: TV1 (D1 = A, the workshop's
// own names), TM1 (the whole workshop state: its voice on top of the mode's base) and the legacy
// «Copiar para la carta» JSON (C# names + propuestaVoiceProfileV2). Pure C#: no UnityEngine, so
// the parity harness (Docs/reference/taller-melodia/paridad/cantante) compiles it as is.
//
// One translation table (Keys) from the workshop's names to VoiceV8Params: the same one serves
// TV1 and TM1, so the parity of an import is checked in a single place.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ALWTTT.Music.Voice.V8
{
    /// <summary>A style read from a code, before it becomes assets.</summary>
    public sealed class VoiceStyleSpec
    {
        public string Source = "";        // "TV1", "TM1", "JSON (Copiar para la carta)"
        public string Name = "";
        public string Mode = "";          // the workshop mode it started from
        public readonly VoiceV8Params P = new VoiceV8Params();
        public float Hardness = 0.5f;     // zigués hardness (JS dureza; lives on the bank)
        public float Gain = 1f;
        public bool HasGain;              // D3 = B: TV1 brings it; TM1 and the legacy JSON do not
        public string SyllableMode = "zigues";   // zigues | lista | texto
        public int LexiconSeed = 4242;
        public readonly List<WeightedSyllable> List = new List<WeightedSyllable>();
        public string Text = "";
        public readonly List<string> Warnings = new List<string>();
        /// <summary>The workshop's voice values as read (for the preview), in the code's order.</summary>
        public readonly List<KeyValuePair<string, string>> Raw = new List<KeyValuePair<string, string>>();
    }

    /// <summary>Fills a workshop mode's full voice into <paramref name="into"/>; false = unknown mode.</summary>
    public delegate bool VoiceModeBase(string mode, VoiceV8Params into, out float hardness);

    public static class VoiceStyleCode
    {
        public const int ZigLexiconSeed = 4242;

        // workshop name -> setter. Names: motor-canto.js BASE.
        static readonly Dictionary<string, Action<VoiceV8Params, object>> Keys = new Dictionary<string, Action<VoiceV8Params, object>>
        {
            ["looseness"] = (p, v) => p.Looseness = F(v),
            ["vibratoDepth"] = (p, v) => p.VibratoDepth = F(v),
            ["vibratoSpeedHz"] = (p, v) => p.VibratoSpeedHz = F(v),
            ["vibratoDelay"] = (p, v) => p.VibratoDelay = F(v),
            ["vibratoRamp"] = (p, v) => p.VibratoRamp = F(v),
            ["brightness"] = (p, v) => p.Brightness = F(v),
            ["transpose"] = (p, v) => p.Transpose = I(v),
            ["tensLo"] = (p, v) => p.TensLo = F(v),
            ["tensHi"] = (p, v) => p.TensHi = F(v),
            ["minLoudness"] = (p, v) => p.MinLoudness = F(v),
            ["pitchLead"] = (p, v) => p.PitchLead = F(v),
            ["tracto"] = (p, v) => p.TractLength = I(v),
            ["aire"] = (p, v) => p.Breath = F(v),
            ["growl"] = (p, v) => p.Growl = F(v),
            ["aspereza"] = (p, v) => p.Roughness = F(v),
            ["saturacion"] = (p, v) => p.Drive = F(v),
            ["scoop"] = (p, v) => p.Scoop = F(v),
            ["scoopMs"] = (p, v) => p.ScoopMs = F(v),
            ["caida"] = (p, v) => p.Fall = F(v),
            ["glideMs"] = (p, v) => p.GlideMs = F(v),
            ["ataque"] = (p, v) => p.Attack = F(v),
            ["legato"] = (p, v) => p.Legato = F(v),
            ["vocalBase"] = (p, v) => p.BaseVowel = Vowel(v as string),
        };

        // Legacy JSON: C# names (VoiceProfileSO + propuestaVoiceProfileV2) -> workshop names.
        static readonly Dictionary<string, string> LegacyNames = new Dictionary<string, string>
        {
            ["looseness"] = "looseness", ["vibratoDepth"] = "vibratoDepth", ["vibratoSpeedHz"] = "vibratoSpeedHz",
            ["brightness"] = "brightness", ["transposeSemitones"] = "transpose", ["tensenessAtVel0"] = "tensLo",
            ["tensenessAtVel127"] = "tensHi", ["vibratoDelaySeconds"] = "vibratoDelay", ["vibratoRampSeconds"] = "vibratoRamp",
            ["pitchLeadSeconds"] = "pitchLead", ["minLoudness"] = "minLoudness",
            ["tractLength"] = "tracto", ["breath"] = "aire", ["growl"] = "growl", ["roughness"] = "aspereza", ["drive"] = "saturacion",
            ["scoopSemitones"] = "scoop", ["scoopMs"] = "scoopMs", ["fallSemitones"] = "caida", ["glideMs"] = "glideMs",
            ["attackPress"] = "ataque", ["legato"] = "legato", ["baseVowel"] = "vocalBase", ["vibratoDepthUnclamped"] = "vibratoDepth",
        };

        /// <summary>
        /// Parses a TV1 / TM1 code or the legacy JSON. <paramref name="modeBase"/> fills a workshop
        /// mode's full voice (TM1 only carries part of it): returns false for an unknown mode.
        /// Throws FormatException with a readable message.
        /// </summary>
        public static VoiceStyleSpec Parse(string text, VoiceModeBase modeBase = null)
        {
            text = (text ?? "").Trim();
            if (text.Length == 0) throw new FormatException("Pega un código TV1:, TM1: o el JSON de «Copiar para la carta».");
            var spec = new VoiceStyleSpec();
            if (text.StartsWith("TV1:", StringComparison.Ordinal))
            {
                spec.Source = "TV1";
                var o = Obj(Json.Parse(B64(text.Substring(4))), "TV1");
                int v = o.TryGetValue("v", out var vv) ? I(vv) : 0;
                if (v != 1) spec.Warnings.Add($"TV1 versión {v}: este importador conoce la 1.");
                spec.Name = Str(o, "nombre"); spec.Mode = Str(o, "modo");
                ApplyVoice(spec, Obj(Get(o, "voz"), "voz"), spec.Warnings);
                ReadSyllables(spec, o.TryGetValue("silabas", out var so) ? so as Dictionary<string, object> : null);
                if (o.TryGetValue("gain", out var g) && g is double gd) { spec.Gain = (float)gd; spec.HasGain = true; }
            }
            else if (text.StartsWith("TM1:", StringComparison.Ordinal))
            {
                spec.Source = "TM1";
                var o = Obj(Json.Parse(B64(text.Substring(4))), "TM1");
                spec.Mode = Str(o, "modo"); spec.Name = spec.Mode;
                // As the workshop's aplicar(): game modes ("Zig hoy…") start from Pop.
                if (modeBase == null) spec.Warnings.Add("Sin tabla de modos: los campos que TM1 no trae quedan por defecto.");
                else if (modeBase(spec.Mode, spec.P, out float h)) spec.Hardness = h;
                else
                {
                    if (modeBase("Pop", spec.P, out float hp)) spec.Hardness = hp;
                    spec.Warnings.Add($"Modo «{spec.Mode}» sin estilo en el juego: la base es Pop (como hace el taller).");
                }
                var voz = o.TryGetValue("voz", out var vz) ? vz as Dictionary<string, object> : null;
                if (voz != null) ApplyVoice(spec, voz, spec.Warnings);
                if (o.TryGetValue("vozHoy", out var vh) && vh is bool b && b)
                    spec.Warnings.Add("El código tenía «la voz de hoy» (v7) encendida: se importa la voz v8 del modo.");
                string silMode = Str(o, "silMode");
                int lex = ZigLexiconSeed;
                if (o.TryGetValue("mel", out var m) && m is Dictionary<string, object> mel && mel.TryGetValue("semilla", out var sem)) lex = I(sem);
                var sil = new Dictionary<string, object>
                {
                    ["modo"] = silMode == "modo" ? "lista" : silMode == "texto" ? "texto" : silMode == "letra" ? "texto" : "zigues",
                    ["lexiconSeed"] = (double)lex,
                    ["texto"] = o.TryGetValue("texto", out var t) ? t : "",
                };
                if (voz != null && voz.TryGetValue("silabas", out var sl)) sil["lista"] = sl;
                if (silMode == "letra") spec.Warnings.Add("Sílabas «letra original»: TM1 no las trae; usa «Copiar TV1» en el taller.");
                ReadSyllables(spec, sil);
                spec.Warnings.Add("TM1 no trae el gain: queda en 1 (D3). «Copiar TV1» lo trae calibrado.");
            }
            else if (text.StartsWith("{", StringComparison.Ordinal))
            {
                spec.Source = "JSON (Copiar para la carta)";
                var o = Obj(Json.Parse(text), "JSON");
                var voz = new Dictionary<string, object>();
                foreach (var block in new[] { "VoiceProfileSO", "propuestaVoiceProfileV2" })
                    if (o.TryGetValue(block, out var bo) && bo is Dictionary<string, object> bd)
                        foreach (var kv in bd)
                            if (LegacyNames.TryGetValue(kv.Key, out var wk)) voz[wk] = kv.Value;
                if (voz.Count == 0) throw new FormatException("El JSON no tiene VoiceProfileSO ni propuestaVoiceProfileV2.");
                if (!(o.TryGetValue("propuestaVoiceProfileV2", out var pv) && pv is Dictionary<string, object> pvd && pvd.ContainsKey("vibratoDepthUnclamped")))
                    spec.Warnings.Add("JSON antiguo sin vibratoDepthUnclamped: el vibrato venía recortado a 1.");
                ApplyVoice(spec, voz, spec.Warnings);
                if (o.TryGetValue("propuestaVoiceProfileV2", out var pv2) && pv2 is Dictionary<string, object> p2) spec.Mode = Str(p2, "modo");
                spec.Name = spec.Mode;
                var silo = o.TryGetValue("silabas", out var s0) ? s0 as Dictionary<string, object> : null;
                var sil = new Dictionary<string, object> { ["modo"] = "zigues", ["lexiconSeed"] = (double)ZigLexiconSeed };
                if (silo != null)
                {
                    string sm = Str(silo, "modo");
                    sil["modo"] = sm == "modo" ? "lista" : sm == "texto" || sm == "letra" ? "texto" : "zigues";
                    if (silo.TryGetValue("dureza", out var du)) sil["dureza"] = du;
                    if (silo.TryGetValue("texto", out var tx)) sil["texto"] = tx;
                }
                if (pv2 is Dictionary<string, object> p3 && p3.TryGetValue("syllables", out var sy)) sil["lista"] = sy;
                if (o.TryGetValue("melodiaPropuesta", out var mp) && mp is Dictionary<string, object> mpd && mpd.TryGetValue("semilla", out var ms))
                    sil["lexiconSeed"] = ms;
                ReadSyllables(spec, sil);
                spec.Warnings.Add("El JSON no trae el gain: queda en 1 (D3). «Copiar TV1» lo trae calibrado.");
            }
            else throw new FormatException("No reconozco el código: empieza por TV1:, TM1: o «{» (JSON de «Copiar para la carta»).");

            if (string.IsNullOrWhiteSpace(spec.Name)) spec.Name = "Estilo importado";
            if (spec.HasGain && (spec.Gain < 0f || spec.Gain > 2f)) { spec.Warnings.Add($"gain {spec.Gain} fuera de [0, 2]: se recorta."); spec.Gain = Math.Max(0f, Math.Min(2f, spec.Gain)); }
            if (spec.P.TractLength < 36 || spec.P.TractLength > 56) spec.Warnings.Add($"tracto {spec.P.TractLength} fuera del rango del Inspector (36–56).");
            if (spec.P.VibratoDepth > 3f) spec.Warnings.Add($"vibratoDepth {spec.P.VibratoDepth} > 3 (rango del Inspector).");
            return spec;
        }

        static void ApplyVoice(VoiceStyleSpec spec, Dictionary<string, object> voz, List<string> warn)
        {
            foreach (var kv in voz)
            {
                if (kv.Key == "silabas" || kv.Key == "nivel") continue;
                if (kv.Key == "dureza") { spec.Hardness = F(kv.Value); spec.Raw.Add(new KeyValuePair<string, string>("dureza", Show(kv.Value))); continue; }
                if (!Keys.TryGetValue(kv.Key, out var set)) { warn.Add($"Campo «{kv.Key}» desconocido: se ignora."); continue; }
                if (kv.Value == null) continue;
                set(spec.P, kv.Value);
                spec.Raw.Add(new KeyValuePair<string, string>(kv.Key, Show(kv.Value)));
            }
        }

        static void ReadSyllables(VoiceStyleSpec spec, Dictionary<string, object> s)
        {
            if (s == null) return;
            string mode = Str(s, "modo");
            spec.SyllableMode = mode == "lista" || mode == "texto" ? mode : "zigues";
            if (s.TryGetValue("lexiconSeed", out var lx) && lx is double) spec.LexiconSeed = I(lx);
            if (s.TryGetValue("dureza", out var du) && du is double) spec.Hardness = F(du);
            spec.Text = s.TryGetValue("texto", out var t) && t is string ts ? ts : "";
            if (s.TryGetValue("lista", out var l) && l is List<object> list)
                foreach (var it in list)
                {
                    if (it is List<object> a && a.Count >= 1 && a[0] is string syl)
                        spec.List.Add(new WeightedSyllable(syl, a.Count > 1 && a[1] is double w ? w : 1.0, a.Count > 2 && (a[2] as string) == "fin"));
                    else if (it is string plain && plain.Length > 0)
                        spec.List.Add(new WeightedSyllable(plain, 1.0, false));
                }
            if (spec.SyllableMode == "lista" && spec.List.Count == 0) { spec.Warnings.Add("Sílabas «del modo» sin lista: se usa el zigués."); spec.SyllableMode = "zigues"; }
            if (spec.SyllableMode == "texto" && spec.Text.Trim().Length == 0) { spec.Warnings.Add("Sílabas «texto» vacías: se usa el zigués."); spec.SyllableMode = "zigues"; }
        }

        // ---- helpers ----
        static string B64(string s)
        {
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(s.Trim())); }
            catch (Exception e) { throw new FormatException("El código no es base64 válido: " + e.Message); }
        }
        static Dictionary<string, object> Obj(object o, string what) =>
            o as Dictionary<string, object> ?? throw new FormatException($"{what}: se esperaba un objeto JSON.");
        static object Get(Dictionary<string, object> o, string k) =>
            o.TryGetValue(k, out var v) ? v : throw new FormatException($"Falta el campo «{k}».");
        static string Str(Dictionary<string, object> o, string k) => o.TryGetValue(k, out var v) && v is string s ? s : "";
        static float F(object v) => v is double d ? (float)d : v is bool b ? (b ? 1f : 0f) : throw new FormatException($"Se esperaba un número y llegó «{v}».");
        static int I(object v) => v is double d ? (int)Math.Round(d) : throw new FormatException($"Se esperaba un entero y llegó «{v}».");
        static string Show(object v) => v is double d ? d.ToString("0.####", CultureInfo.InvariantCulture) : v?.ToString() ?? "null";
        static char Vowel(string s) => string.IsNullOrEmpty(s) ? (char)0x0259 : "aeiou".IndexOf(s[0]) >= 0 ? s[0] : (char)0x0259;

        /// <summary>Minimal JSON reader: objects, arrays, strings, numbers (double), bools, null.</summary>
        public static class Json
        {
            public static object Parse(string s)
            {
                int i = 0; var v = Value(s, ref i); Ws(s, ref i);
                if (i != s.Length) throw new FormatException($"JSON: sobra texto en la posición {i}.");
                return v;
            }
            static void Ws(string s, ref int i) { while (i < s.Length && char.IsWhiteSpace(s[i])) i++; }
            static object Value(string s, ref int i)
            {
                Ws(s, ref i);
                if (i >= s.Length) throw new FormatException("JSON: se acabó el texto.");
                char c = s[i];
                if (c == '{')
                {
                    var o = new Dictionary<string, object>(); i++; Ws(s, ref i);
                    if (s[i] == '}') { i++; return o; }
                    while (true)
                    {
                        Ws(s, ref i); string k = Str(s, ref i); Ws(s, ref i);
                        if (s[i] != ':') throw new FormatException($"JSON: falta «:» en {i}."); i++;
                        o[k] = Value(s, ref i); Ws(s, ref i);
                        if (s[i] == ',') { i++; continue; }
                        if (s[i] == '}') { i++; return o; }
                        throw new FormatException($"JSON: se esperaba «,» o «}}» en {i}.");
                    }
                }
                if (c == '[')
                {
                    var a = new List<object>(); i++; Ws(s, ref i);
                    if (s[i] == ']') { i++; return a; }
                    while (true)
                    {
                        a.Add(Value(s, ref i)); Ws(s, ref i);
                        if (s[i] == ',') { i++; continue; }
                        if (s[i] == ']') { i++; return a; }
                        throw new FormatException($"JSON: se esperaba «,» o «]» en {i}.");
                    }
                }
                if (c == '"') return Str(s, ref i);
                if (s.Length - i >= 4 && string.CompareOrdinal(s, i, "true", 0, 4) == 0) { i += 4; return true; }
                if (s.Length - i >= 5 && string.CompareOrdinal(s, i, "false", 0, 5) == 0) { i += 5; return false; }
                if (s.Length - i >= 4 && string.CompareOrdinal(s, i, "null", 0, 4) == 0) { i += 4; return null; }
                int st = i;
                while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
                if (st == i) throw new FormatException($"JSON: carácter inesperado «{c}» en {i}.");
                return double.Parse(s.Substring(st, i - st), NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            static string Str(string s, ref int i)
            {
                if (s[i] != '"') throw new FormatException($"JSON: se esperaba una cadena en {i}.");
                i++; var sb = new StringBuilder();
                while (s[i] != '"')
                {
                    char c = s[i++];
                    if (c != '\\') { sb.Append(c); continue; }
                    char e = s[i++];
                    switch (e)
                    {
                        case 'n': sb.Append('\n'); break; case 't': sb.Append('\t'); break; case 'r': sb.Append('\r'); break;
                        case 'b': sb.Append('\b'); break; case 'f': sb.Append('\f'); break;
                        case 'u': sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16)); i += 4; break;
                        default: sb.Append(e); break;
                    }
                }
                i++; return sb.ToString();
            }
        }
    }
}
