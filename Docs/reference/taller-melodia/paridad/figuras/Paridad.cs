// Paridad: componer(p) del Taller de melodías (casos.json, de casos.mjs) contra el C# REAL
// MidiGenPlay.Composition.MelodyFigurePlanner. Uso: dotnet run -c Release [casos.json]
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MidiGenPlay.Composition;
using P = MidiGenPlay.Composition.MelodyFigurePlanner;

static class Paridad
{
    public static P.Figure Fig(string s) => s switch
    {
        "Escalera" => P.Figure.Staircase, "Zig-zag" => P.Figure.ZigZag, "Saltos" => P.Figure.Leaps,
        "Ola" => P.Figure.Wave, "Pedal" => P.Figure.Pedal, "Secuencia" => P.Figure.Sequence,
        "Paseo" => P.Figure.Walk, _ => throw new Exception("figura " + s),
    };
    public static P.Direction Dir(string s) => s switch
    {
        "sube" => P.Direction.Up, "baja" => P.Direction.Down, "arco" => P.Direction.Arch,
        "valle" => P.Direction.Valley, _ => throw new Exception("dirección " + s),
    };

    static int Main(string[] args)
    {
        var path = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "casos.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        int n = 0, bad = 0, notes = 0;
        var badByFig = new Dictionary<string, int>();
        foreach (var c in doc.RootElement.EnumerateArray())
        {
            n++;
            string id = c.GetProperty("id").GetString();
            var p = c.GetProperty("p");
            var s = new P.Settings
            {
                figure = Fig(p.GetProperty("figura").GetString()),
                up = p.GetProperty("sube").GetInt32(),
                down = p.GetProperty("baja").GetInt32(),
                range = p.GetProperty("rango").GetDouble(),
                direction = Dir(p.GetProperty("direccion").GetString()),
                atTop = p.GetProperty("alTope").GetString() == "octava" ? P.AtTop.Octave : P.AtTop.Bounce,
                rhythm = p.GetProperty("ritmo").GetString() == "riseup" ? P.RhythmMode.RiseUp : P.RhythmMode.Cells,
                density = p.GetProperty("densidad").GetDouble(),
                syncopation = p.GetProperty("sincopa").GetDouble(),
                rests = p.GetProperty("silencios").GetDouble(),
                swing = p.GetProperty("swing").GetDouble(),
                variation = p.GetProperty("variacion").GetDouble(),
                postLeap = p.GetProperty("postSalto").GetBoolean(),
                chordOnStrong = p.GetProperty("acordeFuerte").GetBoolean(),
                cadence = p.GetProperty("cadencia").GetBoolean(),
                repeat = p.GetProperty("repetir").GetDouble(),
                crescendo = p.GetProperty("crescendo").GetDouble(),
                signature = p.GetProperty("firma").GetBoolean(),
                phraseBars = p.GetProperty("frase").GetInt32(),
                period = p.GetProperty("periodo").GetInt32(),
                pedal = p.GetProperty("pedal").GetString() == "quinta" ? P.PedalAnchor.Fifth : P.PedalAnchor.Tonic,
            };
            int seed = p.GetProperty("semilla").GetInt32();
            int bars = p.GetProperty("compases").GetInt32();
            int tonic = p.GetProperty("tonica").GetInt32();
            var esc = c.GetProperty("esc").EnumerateArray().Select(x => x.GetInt32()).ToArray();
            var prog = c.GetProperty("prog").EnumerateArray().Select(x => x.GetInt32()).ToArray();

            var got = P.Plan(s, seed, bars, esc, (bar, beat) => prog[bar % prog.Length]);
            var exp = c.GetProperty("notas").EnumerateArray().ToList();
            notes += exp.Count;
            string err = null;
            if (got.Count != exp.Count) err = $"notas: C# {got.Count}, JS {exp.Count}";
            for (int i = 0; err == null && i < exp.Count; i++)
            {
                var e = exp[i]; var g = got[i];
                if (g.beat != e.GetProperty("beat").GetDouble()) err = $"nota {i}: beat C# {g.beat:R}, JS {e.GetProperty("beat").GetDouble():R}";
                else if (g.dur != e.GetProperty("dur").GetDouble()) err = $"nota {i}: dur C# {g.dur:R}, JS {e.GetProperty("dur").GetDouble():R}";
                else if (g.degree != e.GetProperty("grado").GetInt32()) err = $"nota {i}: grado C# {g.degree}, JS {e.GetProperty("grado").GetInt32()}";
                else if (P.DegreeToMidi(esc, tonic, g.degree) != e.GetProperty("midi").GetInt32()) err = $"nota {i}: midi";
                else if (g.velocity != e.GetProperty("vel").GetInt32()) err = $"nota {i}: vel C# {g.velocity}, JS {e.GetProperty("vel").GetInt32()}";
                else if (g.bar != e.GetProperty("bar").GetInt32()) err = $"nota {i}: bar";
                else if (g.phraseEnd != e.GetProperty("fin").GetBoolean()) err = $"nota {i}: finFrase";
                else if (g.signature != e.GetProperty("firma").GetBoolean()) err = $"nota {i}: firma";
            }
            if (err != null)
            {
                bad++;
                string f = p.GetProperty("figura").GetString();
                badByFig[f] = badByFig.TryGetValue(f, out var k) ? k + 1 : 1;
                if (bad <= 12) Console.WriteLine($"DIFF {id} ({f}, {p.GetProperty("escala").GetString()}, semilla {seed}): {err}");
            }
        }
        Console.WriteLine($"{n} casos, {notes} notas: {n - bad} idénticos, {bad} distintos.");
        foreach (var kv in badByFig) Console.WriteLine($"  {kv.Key}: {kv.Value}");
        return bad == 0 ? 0 : 1;
    }
}
