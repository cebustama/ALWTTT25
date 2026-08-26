using System.Collections.Generic;
using ALWTTT.Enums;
using ALWTTT.Fx;
using ALWTTT.Managers;
using ALWTTT.Music;
using Melanchall.DryWetMidi.Standards;
using UnityEngine;

/// <summary>
/// [RFX-1] Particle sibling of FloatingTextMidiListener. Consumes
/// IMidiNoteListener (percussion) and IChordListener (harmony) and drives the
/// per-musician RhythmParticleEmitter banks.
///
/// ------------------------------------------------------------------
/// HOW THE DRUMMER GETS THE DRUM PARTICLES
/// ------------------------------------------------------------------
/// Nothing here selects a drummer, and nothing should. MidiMusicManager already
/// did the routing before this listener is called:
///
///   BuildChannelMap  : TrackRole.Rhythm -> MIDI channel 9, always.
///   HandleMidiEvents : per note, musId = owner of that TRACK, falling back to
///                      owner of that CHANNEL.
///   RegisterMusicianAnchor : musId -> that musician's root transform.
///
/// So a note on channel 9 arrives with e.anchor already pointing at whoever is
/// playing the rhythm track in this gig. We resolve the emitter under that
/// anchor and burst. Every musician prefab carries all seven lanes; only the
/// one holding the rhythm track ever receives drum-channel events, and only the
/// one holding a chordal track ever receives chord events.
///
/// This is why per-musician "is this the drummer?" configuration would be a
/// bug, not a feature: band composition changes between gigs, and a guitarist
/// assigned to the rhythm track is the drummer for that gig.
///
/// ------------------------------------------------------------------
/// [D2=A] Deliberately NOT routed through SensoryEventBus. The bus carries
/// player-visible GAME STATE changes with semantic payloads
/// (Design_Sensory_Contract_v0_1 section 3); a MIDI note is audio render, and
/// fires 8-16 times a second.
///
/// [D3=B] Chord bursts fire on chord CHANGE, not on every attack.
///
/// [D9=A] Chord bursts require chordMinNotes simultaneous notes. See the field.
///
/// Does NOT implement IDrumKickListener, on purpose: MidiMusicManager calls
/// BOTH OnMidiNote and OnDrumKick for notes 35/36, so implementing both would
/// double-burst the kick. That double-fire is what ST-RFX-3 regression-tests.
///
/// Placement: scene-placed in the gig scene, sibling of
/// FloatingTextMidiListener - NOT on a DontDestroyOnLoad managers object, so
/// OnEnable/OnDisable actually exercise subscribe/unsubscribe across scene
/// reloads (same pattern as SensoryFxAdapter).
/// </summary>
public sealed class RhythmParticleMidiListener :
    MonoBehaviour, IMidiNoteListener, IChordListener
{
    [Header("Surfaces")]
    [SerializeField] private bool percussionParticles = true;
    [SerializeField] private bool chordParticles = true;

    [Header("Config")]
    [Tooltip("Keep in sync with MidiMusicManager.drumChannel (default 9 = GM ch10). " +
             "BuildChannelMap hard-assigns TrackRole.Rhythm to 9.")]
    [SerializeField] private int drumChannel = 9;

    [Tooltip("[D9] Minimum simultaneous notes for a chord burst. MidiMusicManager " +
             "raises OnChord for ANY non-drum channel with 2+ notes at the same " +
             "tick, so a bass double-stop or a melodic dyad would otherwise burst " +
             "like harmony. 3 keeps triads and richer voicings, drops two-note " +
             "incidentals. Lower to 2 if your backing uses power chords or shell " +
             "voicings and you want those to read as harmony.")]
    [Range(2, 6)]
    [SerializeField] private int chordMinNotes = 3;

    [Header("Diagnostics")]
    [Tooltip("Warn once per anchor when a musician has no RhythmParticleEmitter.")]
    [SerializeField] private bool logMisses = true;

    [Tooltip("Log every resolved burst. Extremely noisy - smoke tests only.")]
    [SerializeField] private bool logEveryHit = false;

    [Tooltip("Log which musician owns each channel the first time it produces " +
             "an event. Cheap, and it is the fastest way to confirm the rhythm " +
             "track landed on the musician you expected.")]
    [SerializeField] private bool logChannelOwnership = true;

    /// <summary>Smoke-test counters. Reset on enable.</summary>
    public long PercussionBursts { get; private set; }
    public long ChordBursts { get; private set; }
    public long ChordsSuppressed { get; private set; }
    public long ChordsBelowMinNotes { get; private set; }

    // Anchor -> emitter. A NULL value is cached on purpose (negative caching):
    // without it, a musician with no emitter would trigger a recursive
    // GetComponentInChildren on every single MIDI note, forever.
    private readonly Dictionary<Transform, RhythmParticleEmitter> _emitters =
        new Dictionary<Transform, RhythmParticleEmitter>();

    // Channel -> last chord identity, for [D3=B] change detection.
    private readonly Dictionary<int, string> _lastChordByChannel =
        new Dictionary<int, string>();

    // Channel -> musicianId, logged once each for routing diagnosis.
    private readonly Dictionary<int, string> _seenChannels =
        new Dictionary<int, string>();

    private void OnEnable()
    {
        PercussionBursts = 0;
        ChordBursts = 0;
        ChordsSuppressed = 0;
        ChordsBelowMinNotes = 0;

        var mm = FindFirstObjectByType<MidiMusicManager>();
        if (mm == null)
        {
            Debug.LogWarning(
                "[RhythmFx] No MidiMusicManager in scene - rhythm particles " +
                "inactive this session.", this);
            return;
        }

        mm.Register((IMidiNoteListener)this);
        mm.Register((IChordListener)this);

        Debug.Log($"[RhythmFx] Subscribed to MidiMusicManager " +
                  $"(percussion={percussionParticles}, chords={chordParticles}, " +
                  $"drumChannel={drumChannel}, chordMinNotes={chordMinNotes}).", this);
    }

    private void OnDisable()
    {
        var mm = FindFirstObjectByType<MidiMusicManager>();
        if (mm != null)
        {
            mm.Unregister((IMidiNoteListener)this);
            mm.Unregister((IChordListener)this);
        }

        // Anchors are runtime-spawned musicians; the cache must not survive a
        // scene reload or it holds destroyed transforms as keys.
        _emitters.Clear();
        _lastChordByChannel.Clear();
        _seenChannels.Clear();
    }

    #region Callbacks

    public void OnMidiNote(MidiTaggedEvent e)
    {
        if (!percussionParticles) return;
        if (e.channel != drumChannel || e.anchor == null) return;

        NoteChannelOwnership(e.channel, e.musicianId, e.anchor);

        var emitter = Resolve(e.anchor);
        if (emitter == null) return;

        var gm = (GeneralMidiPercussion)Mathf.Clamp(e.note, 35, 81);
        var lane = LaneFor(gm);

        if (emitter.Emit(lane, Mathf.Clamp01(e.velocity / 127f)))
            PercussionBursts++;

        if (logEveryHit)
            Debug.Log($"[RhythmFx] {lane} note={e.note} vel={e.velocity} " +
                      $"mus='{e.musicianId}'");
    }

    public void OnChord(ChordEvent e)
    {
        if (!chordParticles) return;
        if (e.anchor == null || e.channel == drumChannel) return;

        NoteChannelOwnership(e.channel, e.musicianId, e.anchor);

        // [D9] MidiMusicManager raises OnChord for any non-drum channel with 2+
        // simultaneous notes. That includes bass double-stops and melodic dyads,
        // which are not harmony and should not read as a harmonic event.
        int n = e.notes != null ? e.notes.Count : 0;
        if (n < chordMinNotes)
        {
            ChordsBelowMinNotes++;
            return;
        }

        // [D3=B] Change detection. A strummed backing re-attacks the same chord
        // several times per bar; the player reads a burst as "new harmony", not
        // as "new attack", so repeats are suppressed.
        //
        // When the chord carries no label at all (LABEL MISS in
        // MidiMusicManager) we fall through and let the emitter's own
        // minInterval throttle handle it - better a slightly noisy burst than
        // total silence on an unlabelled progression.
        string id = !string.IsNullOrEmpty(e.symbol) ? e.symbol : e.roman;
        if (!string.IsNullOrEmpty(id))
        {
            if (_lastChordByChannel.TryGetValue(e.channel, out var prev) && prev == id)
            {
                ChordsSuppressed++;
                return;
            }
            _lastChordByChannel[e.channel] = id;
        }

        var emitter = Resolve(e.anchor);
        if (emitter == null) return;

        if (emitter.Emit(RhythmLane.Chord, Mathf.Clamp01(n / 4f)))
            ChordBursts++;

        if (logEveryHit)
            Debug.Log($"[RhythmFx] Chord '{id ?? "(unlabelled)"}' notes={n} " +
                      $"ch={e.channel} mus='{e.musicianId}'");
    }

    #endregion

    #region Helpers

    // One line per channel, the first time it produces anything. This is the
    // cheapest possible answer to "did the rhythm track land on the musician I
    // expected?" - and to "why is the guitarist drumming?".
    private void NoteChannelOwnership(int channel, string musicianId, Transform anchor)
    {
        if (!logChannelOwnership) return;
        if (_seenChannels.ContainsKey(channel)) return;

        _seenChannels[channel] = musicianId;
        string tag = channel == drumChannel ? " (RHYTHM/drum channel)" : "";
        Debug.Log($"[RhythmFx] Channel {channel}{tag} owned by " +
                  $"'{musicianId ?? "(none)"}' -> anchor '{anchor.name}'.", anchor);
    }

    private RhythmParticleEmitter Resolve(Transform anchor)
    {
        if (_emitters.TryGetValue(anchor, out var cached))
            return cached; // may legitimately be null - see field comment

        var found = anchor.GetComponentInChildren<RhythmParticleEmitter>(true);
        _emitters[anchor] = found;

        if (found == null && logMisses)
            Debug.LogWarning(
                $"[RhythmFx] No RhythmParticleEmitter under '{anchor.name}'. " +
                $"That musician will produce no rhythm particles. " +
                $"(Warned once; result is cached.)", anchor);

        return found;
    }

    private static RhythmLane LaneFor(GeneralMidiPercussion gm) => gm switch
    {
        GeneralMidiPercussion.AcousticBassDrum or
        GeneralMidiPercussion.BassDrum1
            => RhythmLane.Kick,

        GeneralMidiPercussion.AcousticSnare or
        GeneralMidiPercussion.ElectricSnare
            => RhythmLane.Snare,

        GeneralMidiPercussion.ClosedHiHat or
        GeneralMidiPercussion.PedalHiHat
            => RhythmLane.HiHatClosed,

        GeneralMidiPercussion.OpenHiHat
            => RhythmLane.HiHatOpen,

        GeneralMidiPercussion.LowTom or
        GeneralMidiPercussion.LowFloorTom or
        GeneralMidiPercussion.LowMidTom or
        GeneralMidiPercussion.HiMidTom or
        GeneralMidiPercussion.HighTom or
        GeneralMidiPercussion.HighFloorTom
            => RhythmLane.Tom,

        GeneralMidiPercussion.CrashCymbal1 or
        GeneralMidiPercussion.CrashCymbal2 or
        GeneralMidiPercussion.RideCymbal1 or
        GeneralMidiPercussion.RideCymbal2 or
        GeneralMidiPercussion.SplashCymbal or
        GeneralMidiPercussion.ChineseCymbal
            => RhythmLane.Cymbal,

        _ => RhythmLane.Perc
    };

    #endregion
}