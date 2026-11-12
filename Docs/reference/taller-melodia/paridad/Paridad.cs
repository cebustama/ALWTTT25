using System; using System.IO; using System.Text.Json; using Vocal;
var casos = JsonDocument.Parse(File.ReadAllText("casos.json")).RootElement;
Directory.CreateDirectory("out");
foreach (var c in casos.EnumerateArray()) {
    var pt = new PinkThrombone(48000, new StandardRandomSource(c.GetProperty("seed").GetInt32()));
    var steps = c.GetProperty("steps"); int n = steps.GetArrayLength();
    var outBuf = new float[n * 1024]; pt.Reset(); int b = 0;
    foreach (var s in steps.EnumerateArray()) {
        float F(string k) => (float)s.GetProperty(k).GetDouble();
        bool H(string k) => s.TryGetProperty(k, out _);
        if (H("alwaysVoice")) pt.AlwaysVoice = s.GetProperty("alwaysVoice").GetBoolean();
        if (H("wobble")) pt.VibratoWobble = s.GetProperty("wobble").GetBoolean();
        if (H("pj")) pt.PitchJitterGain = F("pj"); if (H("tj")) pt.TensenessJitterGain = F("tj");
        if (H("vib")) pt.VibratoGain = F("vib"); if (H("vibF")) pt.VibratoFrequency = F("vibF");
        if (H("note")) pt.SetMusicalNote(F("note")); if (H("touched")) pt.IsTouched = s.GetProperty("touched").GetBoolean();
        if (H("loud")) pt.Loudness = F("loud"); if (H("tens")) pt.TargetTenseness = F("tens");
        if (H("idx")) { pt.TongueIndex = s.GetProperty("idx").GetDouble(); pt.TongueDiameter = s.GetProperty("dia").GetDouble(); }
        pt.Synthesize(new Span<float>(outBuf, b * 1024, 1024)); b++;
    }
    using var fs = File.Create($"out/{c.GetProperty("name").GetString()}.f32");
    var bytes = new byte[outBuf.Length * 4]; Buffer.BlockCopy(outBuf, 0, bytes, 0, bytes.Length); fs.Write(bytes);
    Console.WriteLine($"{c.GetProperty("name").GetString()}: {outBuf.Length} muestras");
}
