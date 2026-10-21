using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ALWTTT.Managers;
using Melanchall.DryWetMidi.Core;
using MidiGenPlay;
using MidiGenPlay.Composition;
using UnityEngine;
using static MidiGenPlay.MusicTheory.MusicTheory;

namespace ALWTTT.Music.Voice
{
    /// <summary>
    /// [SINGER-1] Payload of CompositionSession.LoopPlaybackStarting — the
    /// stems + musical context of the loop that is about to play. Raised
    /// once per loop, immediately before MidiMusicManager.PlayRaw.
    /// </summary>
    public struct SingerLoopContext
    {
        public int partIndex;
        public IReadOnlyDictionary<MusicianTrackKey, byte[]> stemsByTrack;
        public Tonality tonality;
        public Melanchall.DryWetMidi.MusicTheory.NoteName rootNote;
        public TimeSignature timeSignature;
        public int bpm;
        public float seconds;
    }

    /// <summary>
    /// [SINGER-1] Owns the Pink Trombone singer inside the gig loop.
    ///
    /// Per-loop cycle (D1=A, D2=A):
    ///   1. CompositionSession.LoopPlaybackStarting → find the opted-in
    ///      musician's Melody/Lead stem, arm SingerVoice from it (main
    ///      thread; the voice waits armed, silent).
    ///   2. IPlayMidi.OnSongStarted (MidiToolkitAdapter) → capture
    ///      AudioSettings.dspTime, start the voice at that anchor, and
    ///      assert the melody-channel mute through
    ///      MidiMusicManager.SetChannelVolume (writes through
    ///      _lastKnownVol01, so MMM's own mix reapply preserves it; we
    ///      re-assert every loop anyway because GigManager repopulates
    ///      musician volumes after Play).
    ///   3. Loop end → next LoopPlaybackStarting re-arms from whatever the
    ///      part cache holds (live regeneration is free: card mutations
    ///      land in the next loop's stems).
    ///
    /// Budget (D5=A): hard ceiling 2 voices (POC verdict §3), demo cap 1.
    /// A refused acquisition sits the singer out with a logged reason —
    /// never queued, never instantiated.
    ///
    /// Opt-in (D4=B mechanism for the demo): serialized musicianId +
    /// VoiceProfileSO on this component. D4=A (profile reference on
    /// MusicianCharacterData) is a one-field follow-up once profiles settle.
    ///
    /// Boundary: consumer-side only. MidiGenPlay is not modified.
    /// </summary>
    public class SingerVoiceDirector : MonoBehaviour
    {
        private const string Tag = "<color=#ff88cc>[SingerDirector]</color>";

        /// <summary>POC verdict §3: never exceed. Not configurable.</summary>
        public const int HardVoiceCap = 2;
        /// <summary>Demo cut: one voice comfortable.</summary>
        public const int ActiveVoiceCap = 1;
        private static int s_activeVoices;

        [Header("Refs")]
        [SerializeField] private MidiToolkitAdapter adapter;
        [SerializeField] private SingerVoice voice;

        [Header("Opt-in (D4=B demo switch)")]
        [Tooltip("MusicianId whose Melody/Lead stem is sung. Empty = singer off.")]
        [SerializeField] private string musicianId;
        [SerializeField] private VoiceProfileSO profile;

        [Header("Options")]
        [SerializeField] private bool logDebug = true;

        private CompositionSession _session;
        private bool _budgetHeld;
        private bool _armedForLoop;
        private int _pendingChannel = -1;
        private int _mutedChannel = -1;
        private bool _adapterBound;
        // [SONG-TAIL-1 / D-ST2=C] The singer keeps singing the tail: the last loop's
        // context is kept and re-armed on every MidiMusicManager.SongTailLoopStarting.
        // D-ST7=C: a session loop announced while the tail plays is armed at the handoff.
        private SingerLoopContext _lastLoopCtx;
        private bool _hasLastLoopCtx;
        private SingerLoopContext _deferredSessionCtx;
        private bool _hasDeferredSessionCtx;
        private MidiMusicManager _tailHost;

        private double _startAnchor;
        private bool _awaitingFirstEvent;
        private int _loopCounter;

        public bool IsSingerActive => _budgetHeld && _armedForLoop;

        private void OnEnable()
        {
            TryBindAdapter();
            Debug.Log($"{Tag} Awake on '{name}' (scene={gameObject.scene.name}) " +
                      $"musicianId='{musicianId}' profile={(profile ? profile.name : "NULL")} " +
                      $"voice={(voice ? "ok" : "NULL")}");
        }

        private void TryBindAdapter()
        {
            if (adapter == null) adapter = FindFirstObjectByType<MidiToolkitAdapter>();
            if (adapter != null && !_adapterBound)
            {
                adapter.OnSongStarted += HandleSongStarted;
                adapter.OnMidiEvents += HandleFirstMidiEvents;
                _adapterBound = true;
            }
        }

        private void OnDisable()
        {
            if (_adapterBound && adapter != null)
            {
                adapter.OnSongStarted -= HandleSongStarted;
                adapter.OnMidiEvents -= HandleFirstMidiEvents;
                _adapterBound = false;
            }
            UnbindTailHost();   // [SONG-TAIL-1]
            BindSession(null);
            Deactivate("component disabled");
        }

        private void HandleFirstMidiEvents(System.Collections.Generic.List<MidiPlayerTK.MPTKEvent> _)
        {
            if (!_awaitingFirstEvent) return;
            _awaitingFirstEvent = false;
            double delta = (AudioSettings.dspTime - _startAnchor) * 1000.0;
            Debug.Log($"{Tag} [SYNC] loop={_loopCounter} " +
                      $"anchor→firstMidiEvent = {delta:F1} ms");
        }

        private void Update()
        {
            if (adapter == null || !_adapterBound) TryBindAdapter();
            TryBindTailHost();   // [SONG-TAIL-1] MMM may appear after OnEnable
            // [SONG-TAIL-1 / R-V] The voice follows the tail plane (level and fades);
            // 1 whenever no tail plays.
            if (voice != null) voice.SetExternalLevel01(_tailHost != null ? _tailHost.TailPlane01 : 1f);

            // Sessions are created per gig; lazily (re)bind to the live one.
            var gm = GigManager.Instance;
            var live = gm != null ? gm.CompositionSession : null;
            if (!ReferenceEquals(live, _session)) BindSession(live);
        }

        private void BindSession(CompositionSession s)
        {
            if (_session != null)
            {
                _session.LoopPlaybackStarting -= HandleLoopPlaybackStarting;
                _session.SongFinished -= HandleSongFinished;
            }
            _session = s;
            if (_session != null)
            {
                _session.LoopPlaybackStarting += HandleLoopPlaybackStarting;
                _session.SongFinished += HandleSongFinished;
                if (logDebug) Debug.Log($"{Tag} Bound to CompositionSession.");
            }
        }

        // ── 1. Loop about to play: arm from the melody stem ────────────────

        private void HandleLoopPlaybackStarting(SingerLoopContext ctx)
        {
            // [SONG-TAIL-1 / D-ST7=C] While the tail plays, MidiMusicManager holds this
            // loop until the tail's next bar line. Arming now would Stop() the voice that
            // is still singing the tail, so the arm waits for SongTailHandoff.
            if (_tailHost != null && _tailHost.IsSongTailActive)
            {
                _deferredSessionCtx = ctx;
                _hasDeferredSessionCtx = true;
                if (logDebug) Debug.Log($"{Tag} Arm deferred to the song-tail handoff.");
                return;
            }

            _lastLoopCtx = ctx;        // [SONG-TAIL-1] what the tail re-sings
            _hasLastLoopCtx = true;
            ArmForLoop(ctx);
        }

        // [SONG-TAIL-1] Former body of HandleLoopPlaybackStarting, unchanged; shared by the
        // session loop, the tail loop and the handoff so all arm through the same path.
        private void ArmForLoop(SingerLoopContext ctx)
        {
            _armedForLoop = false;
            _pendingChannel = -1;

            if (voice == null || profile == null || string.IsNullOrEmpty(musicianId))
            {
                SitOut($"not configured (voice={(voice ? "ok" : "NULL")}, " +
                       $"profile={(profile ? "ok" : "NULL")}, " +
                       $"musicianId='{musicianId}')");
                return;
            }

            byte[] stem = FindMelodyStem(ctx.stemsByTrack);
            if (stem == null)
            {
                string keys = ctx.stemsByTrack == null ? "(null map)"
                    : string.Join(", ", ctx.stemsByTrack.Select(k =>
                        $"'{k.Key.MusicianId}':{k.Key.Role}({k.Value?.Length ?? 0}B)"));
                SitOut($"no Melody/Lead stem for '{musicianId}'. Stems present: [{keys}]");
                return;
            }

            if (!_budgetHeld && !TryAcquireBudget())
            { SitOut("voice budget refused"); return; }

            int channel = ReadFirstNoteChannel(stem);
            if (channel < 0)
            { SitOut("melody stem has no note events"); return; }

            bool armed = voice.ArmFromStemBytes(
                stem,
                new SingerVoice.ExprContextInputs
                {
                    tonality = ctx.tonality,
                    rootNote = ctx.rootNote,
                    timeSignature = ctx.timeSignature,
                    bpm = ctx.bpm,
                },
                profile);
            if (!armed) { SitOut("SingerVoice refused to arm"); return; }

            _pendingChannel = channel;
            _armedForLoop = true;
            if (logDebug)
                Debug.Log($"{Tag} Armed for part {ctx.partIndex} " +
                          $"(musician={musicianId}, ch={channel}, bpm={ctx.bpm}).");
        }

        // ── 2. Backing actually started: anchor + mute ─────────────────────

        private void HandleSongStarted()
        {
            if (!_armedForLoop)
            {
                // Singer sits this loop out — the GM melody must be audible.
                RestoreMute();
                return;
            }

            double anchor = AudioSettings.dspTime;
            _startAnchor = anchor;
            _awaitingFirstEvent = true;
            _loopCounter++;
            voice.StartAtDspTime(anchor);

            var mm = MidiMusicManager.Instance;
            if (mm != null && _pendingChannel >= 0)
            {
                mm.SetChannelVolume(_pendingChannel, 0f);
                _mutedChannel = _pendingChannel;
            }
            if (logDebug)
                Debug.Log($"{Tag} Started at dsp={anchor:F4}, muted ch={_pendingChannel}.");
        }

        private void HandleSongFinished(SongFeedbackContext _)
            => Deactivate("song finished");
        // ── [SONG-TAIL-1] tail hooks ───────────────────────────────────────
        private void TryBindTailHost()
        {
            var mm = MidiMusicManager.Instance;
            if (ReferenceEquals(mm, _tailHost)) return;
            UnbindTailHost();
            _tailHost = mm;
            if (_tailHost == null) return;
            _tailHost.SongTailLoopStarting += HandleSongTailLoopStarting;
            _tailHost.SongTailHandoff += HandleSongTailHandoff;
            _tailHost.SongTailStopped += HandleSongTailStopped;
        }

        private void UnbindTailHost()
        {
            if (_tailHost == null) return;
            _tailHost.SongTailLoopStarting -= HandleSongTailLoopStarting;
            _tailHost.SongTailHandoff -= HandleSongTailHandoff;
            _tailHost.SongTailStopped -= HandleSongTailStopped;
            _tailHost = null;
        }

        // Same order as a session loop: arm here, anchor + mute on OnSongStarted.
        private void HandleSongTailLoopStarting()
        {
            if (!_hasLastLoopCtx) { SitOut("song tail: no loop context captured"); return; }
            ArmForLoop(_lastLoopCtx);
        }

        // The held session loop is about to replace the tail: arm it now. Without a
        // deferred context the tail voice must not keep singing over the new song.
        private void HandleSongTailHandoff()
        {
            // [SONG-TAIL-1 fix F-ST-1] The handoff lands on a bar line, usually MID-schedule
            // of the tail's voice. ArmFromStemBytes would Stop() it, but ArmForLoop can sit
            // out BEFORE arming (new part without this musician's Melody/Lead stem, budget,
            // no notes...), leaving the tail voice singing over the new song. Stop it here,
            // unconditionally: the new song never inherits the tail's voice.
            if (voice != null) voice.Stop();
            if (!_hasDeferredSessionCtx) { Deactivate("song tail handoff (no session context)"); return; }
            var ctx = _deferredSessionCtx;
            _hasDeferredSessionCtx = false;
            _lastLoopCtx = ctx;
            _hasLastLoopCtx = true;
            ArmForLoop(ctx);
        }

        private void HandleSongTailStopped()
        {
            _hasLastLoopCtx = false;
            _hasDeferredSessionCtx = false;
            Deactivate("song tail stopped");
        }

        // ── helpers ────────────────────────────────────────────────────────

        private byte[] FindMelodyStem(
            IReadOnlyDictionary<MusicianTrackKey, byte[]> stems)
        {
            if (stems == null) return null;
            foreach (var wanted in new[] { TrackRole.Melody, TrackRole.Lead })
                foreach (var kv in stems)
                    if (kv.Key.MusicianId == musicianId && kv.Key.Role == wanted
                        && kv.Value != null && kv.Value.Length > 0)
                        return kv.Value;
            return null;
        }

        /// <summary>Channel of the stem's first NoteOn — role-scoped, so a
        /// musician holding a second (non-melody) track keeps that channel
        /// audible. Stems are small; a per-loop parse is negligible.</summary>
        private static int ReadFirstNoteChannel(byte[] stemBytes)
        {
            try
            {
                using var ms = new MemoryStream(stemBytes);
                var midi = MidiFile.Read(ms);
                foreach (var chunk in midi.GetTrackChunks())
                    foreach (var e in chunk.Events)
                        if (e is Melanchall.DryWetMidi.Core.NoteOnEvent on)
                            return on.Channel;
            }
            catch (Exception ex)
            { Debug.LogError("[SingerDirector] Stem parse failed: " + ex); }
            return -1;
        }

        private bool TryAcquireBudget()
        {
            if (s_activeVoices >= ActiveVoiceCap)
            {
                Debug.LogWarning(
                    $"{Tag} Voice budget refused: {s_activeVoices} active, " +
                    $"cap {ActiveVoiceCap} (hard ceiling {HardVoiceCap}, " +
                    "POC verdict §3 — ~10.5% DSP per voice). Singer sits out.");
                return false;
            }
            s_activeVoices++;
            _budgetHeld = true;
            return true;
        }

        private void ReleaseBudget()
        {
            if (!_budgetHeld) return;
            s_activeVoices = Mathf.Max(0, s_activeVoices - 1);
            _budgetHeld = false;
        }

        private void SitOut(string reason)
        {
            Debug.LogWarning($"{Tag} [{name}] Sitting out this loop: {reason}.");
        }

        /// <summary>Known limitation (SINGER-1): restores to full volume, which
        /// can stomp a gameplay duck/Highlight on that channel. Interaction
        /// with MMM Highlight is deferred to Dev Mode validation.</summary>
        private void RestoreMute()
        {
            if (_mutedChannel < 0) return;
            MidiMusicManager.Instance?.SetChannelVolume(_mutedChannel, 1f);
            _mutedChannel = -1;
        }

        private void Deactivate(string reason)
        {
            _armedForLoop = false;
            _pendingChannel = -1;
            if (voice != null) voice.Stop();
            RestoreMute();
            ReleaseBudget();
            if (logDebug) Debug.Log($"{Tag} Deactivated: {reason}.");
        }
    }
}