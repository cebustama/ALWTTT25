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
/// anchor and burst. Every musician prefab carries all lanes; only the one
/// holding the rhythm track ever receives drum-channel events, and only the
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
/// ------------------------------------------------------------------
/// [RFX-2] WHAT CHANGED
/// ------------------------------------------------------------------
/// (1) [D-S2-PERC=A] The GM percussion range is now a CHECK, not a clamp. The
///     old Mathf.Clamp(e.note, 35, 81) promoted every sub-35 note to 35 =
///     AcousticBassDrum, so sticks (31), square click (32) and metronome click
///     (33) each drew a phantom kick.
///
/// (2) [D-S2-CHORD=A / D1=B] A single Chord lane became a five-rung ladder,
///     selected by the chord's number of DISTINCT PITCH CLASSES. See LadderFor.
///
/// (3) [D2=B] A rung that is not yet authored falls back to RhythmLane.Chord,
///     so the gig scene never loses chord particles mid-authoring. The fallback
///     asks RhythmParticleEmitter.IsLaneReady rather than reading Emit()'s
///     return value, because Emit() also returns false on throttle.
///
/// Placement: scene-placed in the gig scene, sibling of
/// FloatingTextMidiListener - NOT on a DontDestroyOnLoad managers object, so
/// OnEnable/OnDisable actually exercise subscribe/unsubscribe across scene
/// reloads (same pattern as SensoryFxAdapter).
/// </summary>
public sealed class RhythmParticleMidiListener :
    MonoBehaviour, IMidiNoteListener, IChordListener
{
    // General MIDI percussion occupies note numbers 35..81 inclusive. Outside
    // that range a cast to GeneralMidiPercussion is meaningless - which is
    // exactly the defect the RFX-1 clamp introduced.
    private const int GmPercussionLow = 35;   // AcousticBassDrum
    private const int GmPercussionHigh = 81;  // OpenTriangle

    [Header("Surfaces")]
    [SerializeField] private bool percussionParticles = true;
    [SerializeField] private bool chordParticles = true;

    [Header("Config")]
    [Tooltip("Keep in sync with MidiMusicManager.drumChannel (default 9 = GM ch10). " +
             "BuildChannelMap hard-assigns TrackRole.Rhythm to 9.")]
    [SerializeField] private int drumChannel = 9;

    [Tooltip("[D9 / RFX-2 D3=A] Minimum simultaneous RAW notes for a chord " +
             "burst. MidiMusicManager raises OnChord for ANY non-drum channel " +
             "with 2+ notes at the same tick, so a bass double-stop or a " +
             "melodic dyad would otherwise burst like harmony. 3 keeps triads " +
             "and richer voicings, drops two-note incidentals.\n\n" +
             "This gate counts RAW notes; the ladder that follows counts " +
             "DISTINCT PITCH CLASSES. That is deliberate and it has one visible " +
             "consequence: a bare two-note power chord (root + fifth, no octave " +
             "doubling) is rejected here and never reaches ChordPower. Lowering " +
             "this to 2 would let it through, and would also let every bass " +
             "double-stop through with it.")]
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

    /// <summary>
    /// [RFX-2 / D2=B] Chord bursts that fell back to RhythmLane.Chord because
    /// their ladder rung was unauthored. This is the RETIREMENT GATE: once this
    /// reads 0 across a full song, RhythmLane.Chord can be retired under D2=A.
    /// </summary>
    public long ChordLadderFallbacks { get; private set; }

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

    // [RFX-2] Drum-channel note numbers already reported by a diagnostic. One
    // set serves both branches because a given note number falls into exactly
    // one of them: it is either outside the GM range, or it maps to Perc.
    private readonly HashSet<int> _reportedNotes = new HashSet<int>();

    private void OnEnable()
    {
        PercussionBursts = 0;
        ChordBursts = 0;
        ChordsSuppressed = 0;
        ChordsBelowMinNotes = 0;
        ChordLadderFallbacks = 0;

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
        _reportedNotes.Clear();
    }

    #region Callbacks

    public void OnMidiNote(MidiTaggedEvent e)
    {
        if (!percussionParticles) return;
        if (e.channel != drumChannel || e.anchor == null) return;

        NoteChannelOwnership(e.channel, e.musicianId, e.anchor);

        // [RFX-2 / D-S2-PERC=A] A RANGE CHECK, not a clamp.
        //
        // Mathf.Clamp does not reject out-of-range input - it PROMOTES it to
        // the nearest bound. Under the RFX-1 line every drum-channel note below
        // 35 became 35 = AcousticBassDrum, so sticks (31), square click (32)
        // and metronome click (33) each drew a phantom kick, usually on the
        // downbeat, where a real kick is expected and the fake one is therefore
        // invisible as a bug.
        if (e.note < GmPercussionLow || e.note > GmPercussionHigh)
        {
            NoteOutOfGmRange(e.note);
            return;
        }

        var gm = (GeneralMidiPercussion)e.note;
        var lane = LaneFor(gm);

        // Reported BEFORE Resolve, so an unmapped note stays visible even on a
        // musician that carries no emitter at all.
        if (lane == RhythmLane.Perc) NoteUnmappedToPerc(e.note);

        var emitter = Resolve(e.anchor);
        if (emitter == null) return;

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

        // [D9 / D3=A] MidiMusicManager raises OnChord for any non-drum channel
        // with 2+ simultaneous notes. That includes bass double-stops and
        // melodic dyads, which are not harmony and should not read as a
        // harmonic event. This gate counts RAW notes on purpose - see the
        // chordMinNotes tooltip.
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

        // [RFX-2 / D1=B] Classify into a ladder rung, then fall back (D2=B).
        //
        // IsLaneReady, not Emit()'s return value: Emit() returns false both for
        // "unauthored" and for "throttled", and treating a throttled rung as
        // unauthored would draw a SECOND burst on the legacy lane - the
        // throttle would produce more particles instead of fewer.
        var rung = LadderFor(e.notes);
        var lane = emitter.IsLaneReady(rung) ? rung : RhythmLane.Chord;
        if (lane != rung) ChordLadderFallbacks++;

        // [RFX-2] Velocity is a flat 1f, where RFX-1 passed n/4f.
        //
        // The RUNG already encodes harmonic richness, through its own sprite and
        // its own asset entry. Scaling the burst by note count as well would
        // encode it twice, and would mean a triad could never reach the size its
        // own entry authors. Per-rung feel belongs in RhythmFxConfig, not here.
        // Consequently every ladder entry should carry velocityInfluence = 0;
        // any other value is an inert knob that looks live in the inspector.
        if (emitter.Emit(lane, 1f))
            ChordBursts++;

        if (logEveryHit)
            Debug.Log($"[RhythmFx] Chord '{id ?? "(unlabelled)"}' raw={n} " +
                      $"lane={lane} ch={e.channel} mus='{e.musicianId}'");
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

    /// <summary>
    /// [RFX-2] A drum-channel note outside GM percussion. Dropped, not drawn.
    /// Warning level: this is either a non-percussion note routed to channel 9,
    /// or a percussion map the generator and the game disagree about. Either is
    /// worth knowing.
    /// </summary>
    private void NoteOutOfGmRange(int note)
    {
        if (!_reportedNotes.Add(note)) return;
        Debug.LogWarning(
            $"[RhythmFx] Drum-channel note {note} is outside the GM percussion " +
            $"range {GmPercussionLow}-{GmPercussionHigh}. Dropped, no particle. " +
            $"(Reported once per note number.)", this);
    }

    /// <summary>
    /// [RFX-2] A valid GM percussion note with no dedicated lane. Info level,
    /// not warning: hitting Perc is normal for shakers, cowbells and the like.
    /// The line exists so unmapped percussion is discoverable WITHOUT turning on
    /// logEveryHit, which is unreadable under a real groove.
    /// </summary>
    private void NoteUnmappedToPerc(int note)
    {
        if (!_reportedNotes.Add(note)) return;
        Debug.Log(
            $"[RhythmFx] Drum-channel note {note} " +
            $"({(GeneralMidiPercussion)note}) has no lane mapping -> " +
            $"RhythmLane.Perc, which is unauthored by design (no particle). " +
            $"(Reported once per note number.)", this);
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

    /// <summary>
    /// [RFX-2 / D-S2-CHORD=A / D1=B] Map a chord onto a ladder rung by its
    /// number of DISTINCT PITCH CLASSES.
    ///
    /// Not notes.Count: ChordEvent.notes is the raw list of simultaneous NoteOns
    /// on one channel at one tick, so an octave-doubled root inflates it. The
    /// standard guitar power chord - root, fifth, octave-root - is three raw
    /// notes but two pitch classes. Counting raw would call it a triad and draw
    /// the wrong sprite. The rung NAMES are harmonic, so the count has to be
    /// harmonic too.
    ///
    /// Not quality either: ChordEvent.quality is null on every LABEL MISS, so a
    /// quality-keyed ladder would have holes exactly where the label pipeline
    /// fails, which is when the visual matters most.
    ///
    /// Allocation-free on purpose. This runs BEFORE change detection can
    /// suppress a re-strum, so it fires on every simultaneous attack. LINQ
    /// Distinct() would allocate a HashSet and an enumerator per chord attack,
    /// on the audio-visual hot path. There are exactly twelve pitch classes, so
    /// the whole set fits in one int by construction, and Kernighan's loop
    /// counts the bits in at most twelve iterations.
    /// </summary>
    private static RhythmLane LadderFor(IReadOnlyList<int> notes)
    {
        int mask = 0;
        if (notes != null)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                int pc = notes[i] % 12;
                if (pc < 0) pc += 12;   // defensive: MIDI notes are 0..127
                mask |= 1 << pc;
            }
        }

        int voices = 0;
        while (mask != 0) { mask &= mask - 1; voices++; }   // Kernighan

        return voices switch
        {
            <= 1 => RhythmLane.ChordSingle,
            2 => RhythmLane.ChordPower,
            3 => RhythmLane.ChordTriad,
            4 => RhythmLane.ChordSeventh,
            _ => RhythmLane.ChordExtended
        };
    }

    #endregion
}