// [COMP-LOG-1] The composition log: one file per run, apart from the game log, that says
// for every loop what was asked for and what actually sounded.
//
//   DevTelemetry/composicion_<fecha>_<hora>.log   (editor: project root; player: persistentDataPath)
//
// Per loop, two blocks:
//   ▶ LOOP START — part, loop n/N, bonus, bpm, key, harmony source; the cards in the part
//     (musician, role, card, bundle); each rendered track (instrument, style) with what its
//     stem holds (notes, range, first/last beat, longest gap — «HUECO» if over a bar); the
//     channel each track plays on and its live CC7; solos.
//   ■ LOOP END — note-ons that actually reached the player per channel versus the stem
//     («⚠ SONÓ MENOS» under half), and the CC7 per channel at the end of the loop.
// Plus every composition line the game and MidiGenPlay already print ([SongOrchestrator],
// [BassTrackComposer]…), copied without colours or stack traces, so the file reads alone.
// Boundary: MidiGenPlay is not touched; its own Debug.Log lines are only captured.
//
// Note-ons are counted from MidiToolkitAdapter.OnMidiEvents (main thread, as the sequencer
// reads them): a note muted by volume still counts (the CC7 column shows that), a note
// lost in the synthesizer (polyphony) would not be seen here.
//
// ALWTTT_DEV only, like DevRunTelemetryLogger; outside it every call is a no-op.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ALWTTT.Managers;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiGenPlay;
using MidiGenPlay.Composition;
using UnityEngine;

namespace ALWTTT.Music.Diagnostics
{
    public static class CompositionTrace
    {
        public const string Tag = "[COMP-LOG-1]";

#if ALWTTT_DEV
        static readonly object Gate = new object();
        static StreamWriter _w;
        static string _path;
        static bool _captureOn;
        static MidiToolkitAdapter _adapter;
        static readonly int[] _played = new int[16];
        static readonly int[] _expected = new int[16];
        static readonly string[] _owner = new string[16];
        static bool _loopOpen;
        static string _loopLabel;

        // Lines worth copying from the general log (game side and MidiGenPlay side).
        static readonly string[] CaptureTags =
        {
            "[SongConfigBuilder]", "[SongOrchestrator]", "[PaletteSelector]", "[ChordTrackComposer]", "[ChordTrack]",
            "[BassTrackComposer]", "[BASS-WALK", "[MelodyTrackComposer]", "[Factory/Melody]", "[MelodySlot]",
            "[RhythmTrackComposer]", "[TonalityAudit]", "[MidiMusicManager]", "[ORDER-1]", "[SOLO-1]", "[JAM-",
            "[R5-d]", "[ECON-2]", "[AUDIO-LOOP", "[SingerDirector]", "[VOZ-", "[Pin]", "[Override]", "[B1/Pending]",
            "[TryPlay]", "[Session]",
        };
        static readonly Regex RichText = new Regex("<[^>]+>", RegexOptions.Compiled);
#endif

        /// <summary>Folder of the composition logs (same as the run telemetry).</summary>
        public static string Folder()
        {
#if UNITY_EDITOR
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "DevTelemetry"));
#else
            return Path.Combine(Application.persistentDataPath, "DevTelemetry");
#endif
        }

        /// <summary>Song start: opens the file on first use and starts copying the composition lines.</summary>
        public static void BeginSong(int songSeed, string reason)
        {
#if ALWTTT_DEV
            try
            {
                EnsureOpen();
                Write($"\n══════ CANCIÓN · semilla {songSeed} ({reason}) · {DateTime.Now:HH:mm:ss} ══════");
            }
            catch (Exception e) { Debug.LogWarning($"{Tag} no pude abrir el log de composición: {e.Message}"); }
#endif
        }

        public static void EndSong(string summary)
        {
#if ALWTTT_DEV
            CloseLoop("canción terminada");
            Write($"══════ FIN DE CANCIÓN · {summary} ══════");
            Flush();
#endif
        }

        /// <summary>
        /// Loop about to play. Call after the render and the solo ducks, before or right
        /// after PlayRaw. Reads only: the config, the stems, the strip model, the mixer.
        /// </summary>
        public static void LoopStart(SongConfig cfg, int partIndex, string partName, int loopNumber, int loopsTotal,
            bool bonus, int bpm, float seconds, IReadOnlyDictionary<MusicianTrackKey, byte[]> stems,
            IEnumerable<ALWTTT.UI.SongCompositionUI.TrackEntry> modelTracks,
            IEnumerable<MusicianTrackKey> soloKeys)
        {
#if ALWTTT_DEV
            try
            {
                EnsureOpen();
                CloseLoop("sin cierre (se encadenó otro loop)");
                var mm = MidiMusicManager.Instance;
                var part = cfg?.Parts != null && partIndex >= 0 && partIndex < cfg.Parts.Count ? cfg.Parts[partIndex] : null;
                var sb = new StringBuilder();
                _loopLabel = $"parte {partIndex} «{partName}» loop {loopNumber}/{loopsTotal}{(bonus ? " BONUS" : "")}";
                sb.AppendLine($"\n▶ LOOP START · {_loopLabel} · {DateTime.Now:HH:mm:ss.fff}");
                if (part != null)
                    sb.AppendLine($"  {bpm} BPM · {seconds:0.00} s · {part.TimeSignature} · {part.RootNote} {part.Tonality} · armonía: " +
                                  $"{(mm != null ? $"{mm.LastSharedProgressionSource} {mm.LastSharedProgressionAssetName}" : "?")}");

                sb.AppendLine("  Cartas en la parte (modelo):");
                if (modelTracks != null)
                    foreach (var t in modelTracks)
                        sb.AppendLine($"    · {t.musicianId,-3} {t.role,-9} {(t.sourceCardDefinition != null ? t.sourceCardDefinition.DisplayName : t.info)}" +
                                      $"{(t.styleBundle != null ? $"  [{t.styleBundle.name}]" : "")}");

                Array.Clear(_expected, 0, 16); Array.Clear(_played, 0, 16);
                for (int i = 0; i < 16; i++) _owner[i] = null;
                var solos = soloKeys != null ? new HashSet<MusicianTrackKey>(soloKeys) : new HashSet<MusicianTrackKey>();

                sb.AppendLine("  Pistas renderizadas:");
                if (part?.Tracks != null)
                    foreach (var t in part.Tracks)
                    {
                        if (t == null) continue;
                        var key = new MusicianTrackKey(t.MusicianId, t.Role);
                        string inst = t.PercussionInstrument != null ? t.PercussionInstrument.InstrumentName
                                    : t.Instrument != null ? t.Instrument.InstrumentName : "—";
                        string style = t.Parameters?.Style != null ? t.Parameters.Style.name : "sin bundle";
                        int ch = mm != null ? mm.GetChannelForTrack(cfg, t.MusicianId, t.Role) : -1;
                        string stemInfo = "sin stem";
                        if (stems != null && stems.TryGetValue(key, out var bytes) && bytes != null)
                        {
                            var st = StemStats.Read(bytes, part.TimeSignature, seconds, bpm);
                            stemInfo = st.ToString();
                            int sch = st.Channel >= 0 ? st.Channel : ch;
                            if (sch >= 0 && sch < 16) { _expected[sch] += st.Notes; _owner[sch] = $"{t.MusicianId}/{t.Role}"; }
                        }
                        int cc7 = mm != null && ch >= 0 ? mm.GetLiveComposedCc7(ch) : -1;
                        sb.AppendLine($"    · {t.MusicianId,-3} {t.Role,-9} ch{ch,-2} CC7 {cc7,3}  {inst}  [{style}]{(solos.Contains(key) ? "  ★ SOLO" : "")}");
                        sb.AppendLine($"        stem: {stemInfo}");
                    }
                Write(sb.ToString().TrimEnd());
                HookAdapter();
                _loopOpen = true;
            }
            catch (Exception e) { Debug.LogWarning($"{Tag} LoopStart: {e.Message}"); }
#endif
        }

        /// <summary>Loop boundary: what actually reached the player, and the mixer.</summary>
        public static void LoopEnd()
        {
#if ALWTTT_DEV
            CloseLoop(null);
#endif
        }

#if ALWTTT_DEV
        static void CloseLoop(string note)
        {
            if (!_loopOpen) return;
            _loopOpen = false;
            try
            {
                var mm = MidiMusicManager.Instance;
                var sb = new StringBuilder();
                sb.AppendLine($"■ LOOP END · {_loopLabel}{(note != null ? $" ({note})" : "")} · {DateTime.Now:HH:mm:ss.fff}");
                int warnings = 0;
                for (int ch = 0; ch < 16; ch++)
                {
                    if (_expected[ch] == 0 && _played[ch] == 0) continue;
                    int cc7 = mm != null ? mm.GetLiveComposedCc7(ch) : -1;
                    bool low = _expected[ch] > 0 && _played[ch] * 2 < _expected[ch];
                    if (low) warnings++;
                    sb.AppendLine($"    ch{ch,-2} {_owner[ch] ?? "?",-12} sonaron {_played[ch],4} de {_expected[ch],4} notas · CC7 al final {cc7,3}" +
                                  $"{(low ? "  ⚠ SONÓ MENOS DE LA MITAD" : "")}{(cc7 == 0 && _expected[ch] > 0 ? "  ⚠ CANAL MUDO" : "")}");
                }
                Write(sb.ToString().TrimEnd());
                if (warnings > 0)
                    Debug.LogWarning($"{Tag} {_loopLabel}: {warnings} canal(es) sonaron menos de la mitad de sus notas. Detalle en {_path}");
                Flush();
            }
            catch (Exception e) { Debug.LogWarning($"{Tag} LoopEnd: {e.Message}"); }
        }

        static void HookAdapter()
        {
            if (_adapter == null)
            {
                _adapter = UnityEngine.Object.FindFirstObjectByType<MidiToolkitAdapter>();
                if (_adapter != null) _adapter.OnMidiEvents += CountNotes;
            }
        }

        static void CountNotes(List<MidiPlayerTK.MPTKEvent> evts)
        {
            if (!_loopOpen || evts == null) return;
            foreach (var e in evts)
                if (e != null && e.Command == MidiPlayerTK.MPTKCommand.NoteOn && e.Velocity > 0 && e.Channel >= 0 && e.Channel < 16)
                    _played[e.Channel]++;
        }

        static void EnsureOpen()
        {
            if (_w != null) return;
            lock (Gate)
            {
                if (_w != null) return;
                Directory.CreateDirectory(Folder());
                _path = Path.Combine(Folder(), $"composicion_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
                _w = new StreamWriter(_path, true, new UTF8Encoding(false));
                _w.WriteLine($"ALWTTT · log de composición (COMP-LOG-1) · {Application.version} · {(Application.isEditor ? "editor" : "build")} · {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }
            if (!_captureOn)
            {
                _captureOn = true;
                Application.logMessageReceivedThreaded += Capture;
                Application.quitting += () => { lock (Gate) { _w?.Flush(); _w?.Dispose(); _w = null; } };
            }
            Debug.Log($"{Tag} Log de composición → {_path}");
        }

        static void Capture(string msg, string stack, LogType type)
        {
            if (string.IsNullOrEmpty(msg) || msg.StartsWith(Tag, StringComparison.Ordinal)) return;
            bool hit = false;
            for (int i = 0; i < CaptureTags.Length && !hit; i++) hit = msg.IndexOf(CaptureTags[i], StringComparison.Ordinal) >= 0;
            if (!hit) return;
            string clean = RichText.Replace(msg, "").Replace("\n", "\n      ");
            string mark = type == LogType.Error || type == LogType.Exception ? "✖ " : type == LogType.Warning ? "! " : "· ";
            Write($"  {mark}{clean}");
        }

        static void Write(string s)
        {
            lock (Gate) { _w?.WriteLine(s); }
        }

        static void Flush()
        {
            lock (Gate) { _w?.Flush(); }
        }

        /// <summary>What a stem holds: notes, range, first/last beat, longest silence.</summary>
        struct StemStats
        {
            public int Notes, MinPitch, MaxPitch, Channel;
            public double FirstBeat, LastBeat, LongestGapBeats, LoopBeats;
            public int BeatsPerBar;

            public static StemStats Read(byte[] bytes, MidiGenPlay.MusicTheory.MusicTheory.TimeSignature ts, float seconds, int bpm)
            {
                var s = new StemStats { MinPitch = 127, MaxPitch = 0, Channel = -1 };
                var (bpb, unit) = MidiGenPlay.MusicTheory.MusicTheory.TimeSignatureProperties[ts];
                s.BeatsPerBar = bpb;
                using var ms = new MemoryStream(bytes);
                var file = MidiFile.Read(ms);
                int tpqn = file.TimeDivision is TicksPerQuarterNoteTimeDivision tq ? tq.TicksPerQuarterNote : 480;
                double beatsPerTick = (unit / 4.0) / tpqn;
                s.LoopBeats = bpm > 0 ? seconds * bpm / 60.0 : 0;
                var notes = file.GetNotes().OrderBy(n => n.Time).ToList();
                s.Notes = notes.Count;
                if (notes.Count == 0) { s.LongestGapBeats = s.LoopBeats; return s; }
                s.Channel = (int)notes[0].Channel;
                double cursor = 0, gap = 0;
                foreach (var n in notes)
                {
                    double st = n.Time * beatsPerTick, en = (n.Time + n.Length) * beatsPerTick;
                    gap = Math.Max(gap, st - cursor);
                    cursor = Math.Max(cursor, en);
                    int p = (int)n.NoteNumber; s.MinPitch = Math.Min(s.MinPitch, p); s.MaxPitch = Math.Max(s.MaxPitch, p);
                }
                s.FirstBeat = notes[0].Time * beatsPerTick;
                s.LastBeat = cursor;
                if (s.LoopBeats > 0) gap = Math.Max(gap, s.LoopBeats - cursor);
                s.LongestGapBeats = gap;
                return s;
            }

            public override string ToString()
            {
                if (Notes == 0) return "0 notas  ⚠ VACÍO";
                string gapTxt = $"hueco máx {LongestGapBeats:0.#} tiempos";
                bool hole = BeatsPerBar > 0 && LongestGapBeats > BeatsPerBar + 0.01;
                return $"{Notes} notas · ch{Channel} · {MinPitch}–{MaxPitch} · tiempos {FirstBeat:0.#}–{LastBeat:0.#} de {LoopBeats:0.#} · {gapTxt}" +
                       (hole ? $"  ⚠ HUECO > 1 compás" : "");
            }
        }
#endif
    }
}
