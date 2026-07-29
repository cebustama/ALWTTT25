using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiGenPlay.Composition;
using MidiPlayerTK;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

// Pink Trombone POC — optional backing playback (D-S3-D=B, MPTK route). v3.
//
// WHY THIS EXISTS: a solo articulatory voice is judged far more harshly than
// the same voice inside a mix. Forming the deliverable-4 verdict on a naked
// melody would bias it toward "no" for reasons unrelated to the instrument.
//
// DESIGN: PinkTromboneSinger knows nothing about MPTK. It raises RenderReady
// with the rendered file + the chunk it claimed; this component subscribes,
// removes that chunk (so the melody is never heard twice — once sung, once as
// a GM patch), strips the metronome, and hands the remainder to MPTK.
// Deleting this ONE file removes MPTK from the POC entirely.
//
// The melody chunk is removed by REFERENCE, not by channel. Channels are
// config-order-dependent (SongOrchestrator.BuildChannelMap), so muting "the
// melody channel" would be the same hard-coding D-POC-4 exists to avoid.
//
// ---- v3: written against the ACTUAL MidiExternalPlayer source -------------
// v1 and v2 were both built on guessed API. Corrections, all from reading
// MidiExternalPlayer.MPTK_Play():
//
//  A. THERE IS NO MPTK_Play(byte[]) OVERLOAD. The only override is
//     MPTK_Play(bool alreadyLoaded = false). v2's byte-array route would not
//     have compiled. External MIDI goes through UnityWebRequest, so a URI is
//     mandatory and the file MUST be written to disk.
//
//  B. THE SCHEME PREFIX IS EXACTLY "file://" — TWO SLASHES. MPTK validates
//     with pathmidiNameToPlay.Remove(0, 7), stripping precisely 7 chars. The
//     spec-correct "file:///C:/..." leaves "/C:/..." and File.Exists fails on
//     Windows. v2 got this wrong. MPTK's own browse button uses "file://" +
//     path, which is the form replicated here.
//
//  C. EVERY FAILURE IS SILENT. All five Debug.LogWarning calls in MPTK's
//     validation block are commented out; each path only sets
//     MPTK_StatusLastMidiLoaded and returns. That is why v1 produced no MPTK
//     output whatsoever despite "Log MIDI Events Played" being enabled — it
//     was rejected as MidiNameInvalid before the loader ever ran. This
//     component now READS and LOGS that status, which is the only diagnostic
//     channel MPTK offers.
//
//  D. MPTK_Play() DOES NOTHING IF ALREADY PLAYING (status = AlreadyPlaying,
//     silently). Playback is therefore driven from a coroutine that stops the
//     transport and waits for it to settle before starting.
//
//  E. MPTK ONLY SYNTHESISES IN PLAY MODE. MPTK_Play calls MPTK_InitSynth and
//     MPTK_StartSequencerMidi and renders through a live AudioSource; in Edit
//     mode it will silently produce nothing.
// --------------------------------------------------------------------------
//
// REQUIRES MPTK PRO (MidiExternalPlayer). The free MidiFilePlayer reads the
// MidiDB, populated at edit time, and cannot take a runtime render.
//
// SYNC: loading is a UnityWebRequest coroutine, so start latency is neither
// small nor deterministic. Trim by ear with the singer's syncTrimMs (positive
// = sing later). That two independent audio sources share no clock is a
// finding for the ALWTTT integration phase, not something to solve at POC tier.
//
// BOUNDARY: consumer-side only. No runtime or SSoT change.
public class PinkTromboneBackingPlayer : MonoBehaviour
{
    [SerializeField] private PinkTromboneSinger singer;
    [Tooltip("MidiExternalPlayer prefab instance (MPTK Pro).")]
    [SerializeField] private MidiExternalPlayer player;

    [Header("Playback")]
    [Range(0f, 1f)][SerializeField] private float backingVolume = 0.5f;
    [Tooltip("Seconds to wait for MPTK's async load before reporting its status.")]
    [SerializeField, Range(0.5f, 5f)] private float statusPollSeconds = 2f;

    [Header("Content")]
    [Tooltip("Also strip the metronome click from the backing.")]
    [SerializeField] private bool stripMetronome = true;

    private bool _quitting;
    private bool _subscribed;
    private Coroutine _playRoutine;

    private void OnEnable()
    {
        if (singer == null) singer = GetComponent<PinkTromboneSinger>();
        if (singer == null)
        {
            Debug.LogError("[PTBacking] No PinkTromboneSinger assigned.");
            return;
        }
        singer.RenderReady += OnRenderReady;

        if (player != null && !_subscribed)
        {
            player.OnEventStartPlayMidi.AddListener(OnMptkStart);
            _subscribed = true;
        }
    }

    private void OnDisable()
    {
        if (singer != null) singer.RenderReady -= OnRenderReady;

        // Teardown guard: during scene close MPTK's RoutineTimingController is
        // already gone and MPTK_Stop trips an 'go.IsActive()' assertion.
        if (_quitting || player == null) return;
        if (player.MPTK_IsPlaying) player.MPTK_Stop();
    }

    private void OnApplicationQuit() => _quitting = true;

    /// <summary>MPTK resets settings on play, so volume is applied here.</summary>
    private void OnMptkStart(string midiName)
    {
        player.MPTK_Volume = backingVolume;
        Debug.Log($"[PTBacking] MPTK started — volume {backingVolume:F2}, " +
                  $"duration {player.MPTK_Duration.TotalSeconds:F1}s");
    }

    private void OnRenderReady(MidiFile file, TrackChunk melodyChunk)
    {
        if (player == null)
        {
            Debug.LogWarning("[PTBacking] No MidiExternalPlayer assigned — singing dry.");
            return;
        }
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[PTBacking] Not in Play mode — MPTK cannot synthesise.");
            return;
        }

        // Safe to mutate: the singer's schedule is already a value-type snapshot.
        if (melodyChunk != null) file.Chunks.Remove(melodyChunk);
        if (stripMetronome) SmokeRenderUtil.StripMetronomeChunks(file);

        int remaining = file.GetTrackChunks().Count();
        int backingNotes = file.GetTrackChunks().Sum(c => c.GetNotes().Count());
        if (remaining == 0 || backingNotes == 0)
        {
            Debug.LogWarning($"[PTBacking] Backing is empty ({remaining} chunks, " +
                             $"{backingNotes} notes) — add Backing / Bassline / " +
                             "Rhythm rows to the SmokeSetupSO.");
            return;
        }

        string path = WriteBackingFile(file);
        if (path == null) return;

        Debug.Log($"[PTBacking] Backing = {remaining} chunk(s), {backingNotes} notes -> {path}");

        if (_playRoutine != null) StopCoroutine(_playRoutine);
        _playRoutine = StartCoroutine(PlayRoutine(path));
    }

    private IEnumerator PlayRoutine(string path)
    {
        // (D) MPTK_Play is a no-op while the transport runs. Stop, then let it
        // settle — MPTK's stop is coroutine-driven, not immediate.
        if (player.MPTK_IsPlaying)
        {
            player.MPTK_Stop();
            for (int i = 0; i < 30 && player.MPTK_IsPlaying; i++) yield return null;
        }

        // (B) EXACTLY two slashes: MPTK strips 7 chars and calls File.Exists on
        // the remainder. Forward slashes throughout — Path.Combine on Windows
        // yields backslashes, which UnityWebRequest handles poorly.
        string clean = path.Replace('\\', '/');
        string uri = "file://" + clean;

        // Pre-flight the exact check MPTK will perform, so a mismatch is caught
        // here with a readable message rather than silently inside MPTK.
        if (!File.Exists(uri.Remove(0, 7)))
        {
            Debug.LogError($"[PTBacking] MPTK's own existence check will fail for " +
                           $"'{uri}' (it tests '{uri.Remove(0, 7)}'). Aborting.");
            yield break;
        }

        Debug.Log($"[PTBacking] MPTK_MidiName = {uri}");
        player.MPTK_MidiName = uri;
        player.MPTK_Play();

        // (C) MPTK's failure warnings are all commented out in source. This
        // status code is the ONLY diagnostic channel it exposes.
        float t = 0f;
        while (t < statusPollSeconds && !player.MPTK_IsPlaying)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (player.MPTK_IsPlaying)
        {
            Debug.Log($"[PTBacking] Playing. Status={player.MPTK_StatusLastMidiLoaded}");
        }
        else
        {
            Debug.LogError(
                $"[PTBacking] MPTK did not start after {statusPollSeconds:F1}s. " +
                $"Status={player.MPTK_StatusLastMidiLoaded} " +
                $"WebRequestError='{player.MPTK_WebRequestError}'. " +
                "MidiNameInvalid = missing file:// prefix. NotFound = the path " +
                "after 'file://' does not resolve. SoundFontNotLoaded = no SF " +
                "ready. AlreadyPlaying = transport never stopped.");
        }
        _playRoutine = null;
    }

    private string WriteBackingFile(MidiFile file)
    {
        try
        {
            string dir = Path.Combine(Application.persistentDataPath, "PinkTrombonePOC");
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "backing.mid");
            file.Write(path, overwriteFile: true);
            return path;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[PTBacking] Failed to write backing .mid: " + ex);
            return null;
        }
    }
}