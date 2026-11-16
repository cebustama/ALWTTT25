// [VOZ-2] Lado C# de la paridad del cantante v8: lee casos.json, marca el fin de frase,
// reparte las sílabas y canta con VoiceRenderer (los archivos REALES del juego). Renderiza
// en trozos de 1000 muestras a propósito: el renderer trocea en 256 por dentro (D3 = A), así
// que el tamaño del búfer del que llama no puede cambiar el resultado.
using System; using System.IO; using System.Linq; using System.Text.Json; using System.Collections.Generic;
using ALWTTT.Music.Voice.V8;

var casos = JsonDocument.Parse(File.ReadAllText("casos.json")).RootElement;
Directory.CreateDirectory("out");
const int SR = 48000;
foreach (var c in casos.EnumerateArray())
{
    string name = c.GetProperty("name").GetString()!;
    var pj = c.GetProperty("p");
    float F(string k) => (float)pj.GetProperty(k).GetDouble();
    var p = new VoiceV8Params
    {
        Looseness = F("Looseness"), VibratoDepth = F("VibratoDepth"), VibratoSpeedHz = F("VibratoSpeedHz"),
        VibratoDelay = F("VibratoDelay"), VibratoRamp = F("VibratoRamp"), Brightness = F("Brightness"),
        Transpose = pj.GetProperty("Transpose").GetInt32(), TensLo = F("TensLo"), TensHi = F("TensHi"),
        MinLoudness = F("MinLoudness"), PitchLead = F("PitchLead"), TractLength = pj.GetProperty("TractLength").GetInt32(),
        Breath = F("Breath"), Growl = F("Growl"), Roughness = F("Roughness"), Drive = F("Drive"), Scoop = F("Scoop"),
        ScoopMs = F("ScoopMs"), Fall = F("Fall"), GlideMs = F("GlideMs"), Attack = F("Attack"), Legato = F("Legato"),
        BaseVowel = pj.GetProperty("BaseVowel").GetString()![0],
    };
    var notes = c.GetProperty("notes").EnumerateArray().Select(n => new SungNote
    {
        Start = n.GetProperty("start").GetDouble(), End = n.GetProperty("end").GetDouble(),
        Note = n.GetProperty("note").GetInt32(), Vel = n.GetProperty("vel").GetInt32(),
        StartBeats = n.GetProperty("startBeats").GetDouble(), DurBeats = n.GetProperty("durBeats").GetDouble(),
    }).ToArray();
    double sb = 60.0 / c.GetProperty("bpm").GetInt32();
    SyllablePlanner.MarkPhraseEnds(notes, sb, p.Fall > 0, 4, c.GetProperty("rejilla").GetInt32());

    var sil = c.GetProperty("sil");
    string mode = sil.GetProperty("mode").GetString()!;
    int verseSeed = sil.GetProperty("verseSeed").GetInt32();
    string[]? lexico = null;
    if (mode == "zigues")
        lexico = SyllablePlanner.AssignZigues(notes, sil.GetProperty("lexSeed").GetInt32(), sil.GetProperty("hardness").GetDouble(), verseSeed);
    else
    {
        var bank = sil.GetProperty("bank").EnumerateArray()
            .Select(b => new WeightedSyllable(b[0].GetString()!, b[1].GetDouble(), b[2].GetBoolean())).ToList();
        SyllablePlanner.AssignWeighted(notes, mode == "texto" ? new List<WeightedSyllable>() : bank, verseSeed,
            mode == "texto" ? sil.GetProperty("text").GetString()! : "");
    }

    var r = new VoiceRenderer(notes, p, SR, c.GetProperty("seed").GetInt32());
    double dur = notes[^1].End + 1;
    int frames = (int)Math.Ceiling(dur * SR / VoiceRenderer.ControlBlock) * VoiceRenderer.ControlBlock;
    var outBuf = new float[frames];
    var sw = System.Diagnostics.Stopwatch.StartNew();
    for (int off = 0; off < frames; off += 1000) r.Render(new Span<float>(outBuf, off, Math.Min(1000, frames - off)));
    sw.Stop();

    var bytes = new byte[outBuf.Length * 4]; Buffer.BlockCopy(outBuf, 0, bytes, 0, bytes.Length);
    File.WriteAllBytes($"out/{name}.f32", bytes);
    File.WriteAllText($"out/{name}.json", JsonSerializer.Serialize(new
    {
        sil = notes.Select(n => n.Syllable).ToArray(),
        fin = notes.Select(n => n.PhraseEnd).ToArray(),
        caida = notes.Select(n => n.Fall).ToArray(),
        lexico,
    }));
    Console.WriteLine($"{name}: {frames} muestras, {notes.Length} notas, CPU {100.0 * sw.Elapsed.TotalSeconds / (frames / (double)SR):F1} % de tiempo real");
}

// ---- D3 = A: el tamaño del búfer del que llama no cambia ni una muestra; y las perillas en vivo sí cambian el sonido.
{
    var c = casos[0];
    var notes0 = c.GetProperty("notes").EnumerateArray().Select(n => new SungNote
    {
        Start = n.GetProperty("start").GetDouble(), End = n.GetProperty("end").GetDouble(),
        Note = n.GetProperty("note").GetInt32(), Vel = n.GetProperty("vel").GetInt32(),
        StartBeats = n.GetProperty("startBeats").GetDouble(), DurBeats = n.GetProperty("durBeats").GetDouble(),
    }).ToArray();
    float[] Run(int chunk, Action<VoiceV8Params, int>? live)
    {
        var p = new VoiceV8Params();
        var r = new VoiceRenderer((SungNote[])notes0.Clone(), p, SR, 11);
        var o = new float[48000 * 6];
        int k = 0;
        for (int off = 0; off < o.Length; off += chunk, k++) { live?.Invoke(p, off); r.Render(new Span<float>(o, off, Math.Min(chunk, o.Length - off))); }
        return o;
    }
    var a256 = Run(256, null); var a441 = Run(441, null); var a1024 = Run(1024, null);
    bool same = a256.SequenceEqual(a441) && a256.SequenceEqual(a1024);
    Console.WriteLine($"D3 búfer 256 / 441 / 1024: {(same ? "IDÉNTICOS" : "DISTINTOS")}");
    var live = Run(1024, (p, off) => { if (off >= 48000 * 2) { p.LiveTensenessOffset = 0.3f; p.LiveVibratoScale = 3f; p.LiveGrowlOffset = 0.7f; } });
    int firstDiff = -1;
    for (int i = 0; i < live.Length; i++) if (live[i] != a1024[i]) { firstDiff = i; break; }
    Console.WriteLine($"Perillas en vivo desde t=2 s: primera muestra distinta en t={(firstDiff < 0 ? double.NaN : firstDiff / (double)SR):F4} s");
}
