using UnityEngine;
using Vocal;


// Pink Trombone POC — parameter playground (deliverable 2), Session 2 (fork-aware).
// Requires the POC-FORK source (D-POC-8=A): live tongue, working Loudness,
// IsTouched/AlwaysVoice on the facade, no per-block Debug.WriteLine.
//
// Envelope wiring (D-S2-2=A, layered + toggleable):
//   - Internal path (default): the gate drives _voice.IsTouched; with AlwaysVoice
//     off, the model's own intensity envelope articulates notes and breath
//     persists through rests. Velocity goes through _voice.Loudness (POC-FORK 1).
//   - External path ("Hard-gate output" ON): the one-pole envelope additionally
//     multiplies the buffer. Its target is now pure gate (0/1), NOT velocity —
//     velocity lives in Loudness, so nothing is applied twice.
//
// NOTE (Defect 3 discipline): the attack/release MATH, RANGES and (non-)RETRIGGER
// below are UNCHANGED from Session 1 on purpose. First settle empirically with the
// on-screen envelope readout whether they are broken or merely never retriggered.
//
// Main thread (OnGUI/Update) writes plain floats; audio thread reads them.
// Float/bool writes are atomic; fine at POC rigor. No allocs in the audio callback.
public class PinkTrombonePlayground : MonoBehaviour
{
    PinkThrombone _voice;
    float[] _mono;

    // ---- UI scaling (IMGUI dibuja en píxeles físicos: en QHD/4K hay que escalar) ----
    [Range(1f, 4f)] public float uiScale = 2f;   // ajustable en el Inspector en Play
    GUISkin _scaledSkin;

    // ---- UI-controlled state (main thread writes, audio thread reads) ----
    [Range(0f, 1f)] public float gain = 0.5f;
    float _semitone = 0f;            // relative to A4 (MIDI n - 69)
    float _tenseness = 0.7f;
    float _velLoudness = 1f;         // "velocity" → _voice.Loudness (works post-fork)
    float _tongueIndex = 12.9f;      // source defaults; LIVE post-fork (glides ~15/s)
    float _tongueDiameter = 2.43f;
    float _vibGain = 0.005f;
    float _vibFreq = 6f;
    bool _wobble = false;
    bool _alwaysVoice = false;       // false ⇒ IsTouched articulates (internal env)
    bool _hardGateOutput = false;    // true: ALSO multiply buffer by external envelope
    volatile bool _gateOpen = true;  // note on/off

    // ---- audio-thread state (Session-1 envelope, UNCHANGED — see header note) ----
    float _loudSmooth = 0f;          // one-pole envelope toward gate target
    float _attackMs = 10f, _releaseMs = 60f;
    int _sampleRate;

    // dirty-flag mirror so we only touch _voice properties when values change
    float _pSemi = float.NaN, _pTen, _pTi, _pTd, _pVg, _pVf, _pVl;
    bool _pW = true, _pAv = true, _pGate;

    static readonly int[] Scale = { 0, 2, 4, 5, 7, 9, 11, 12 }; // C major
    static readonly KeyCode[] Keys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
                                       KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K };
    int _heldKey = -1;

    void Awake()
    {
        _sampleRate = AudioSettings.outputSampleRate;
        _voice = new PinkThrombone(_sampleRate, new StandardRandomSource());
        AudioSettings.GetDSPBufferSize(out int len, out _);
        _mono = new float[len * 2];
        PushParams(force: true);
    }

    void Update()
    {
        // QWERTY scale keyboard: last key pressed wins (D-POC-6 last-note, previewed)
        for (int i = 0; i < Keys.Length; i++)
        {
            if (Input.GetKeyDown(Keys[i]))
            {
                _heldKey = i;
                _semitone = (60 + Scale[i]) - 69; // C4-based
                _gateOpen = true;
            }
            if (Input.GetKeyUp(Keys[i]) && _heldKey == i)
            {
                _heldKey = -1;
                _gateOpen = false;
            }
        }
        PushParams(force: false);

        // +/- ajustan el tamaño del panel en vivo
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            uiScale = Mathf.Min(4f, uiScale + 0.25f);
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            uiScale = Mathf.Max(1f, uiScale - 0.25f);
    }

    void PushParams(bool force)
    {
        if (_voice == null) return;
        if (force || _semitone != _pSemi) { _voice.SetMusicalNote(_semitone); _pSemi = _semitone; }
        if (force || _tenseness != _pTen) { _voice.TargetTenseness = _tenseness; _pTen = _tenseness; }
        if (force || _tongueIndex != _pTi) { _voice.TongueIndex = _tongueIndex; _pTi = _tongueIndex; }
        if (force || _tongueDiameter != _pTd) { _voice.TongueDiameter = _tongueDiameter; _pTd = _tongueDiameter; }
        if (force || _vibGain != _pVg) { _voice.VibratoGain = _vibGain; _pVg = _vibGain; }
        if (force || _vibFreq != _pVf) { _voice.VibratoFrequency = _vibFreq; _pVf = _vibFreq; }
        if (force || _wobble != _pW) { _voice.VibratoWobble = _wobble; _pW = _wobble; }
        // ---- fork-enabled controls ----
        if (force || _velLoudness != _pVl) { _voice.Loudness = Mathf.Clamp01(_velLoudness); _pVl = _velLoudness; }
        if (force || _alwaysVoice != _pAv) { _voice.AlwaysVoice = _alwaysVoice; _pAv = _alwaysVoice; }
        if (force || _gateOpen != _pGate) { _voice.IsTouched = _gateOpen; _pGate = _gateOpen; }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (_voice == null) return;
        int frames = data.Length / channels;
        if (_mono.Length < frames) return;

        var span = new System.Span<float>(_mono, 0, frames);
        _voice.Synthesize(span);   // sintetizar primero; envolvente aplicada después

        // External envelope target: pure gate (velocity now travels via Loudness).
        // The envelope is ALWAYS computed (so the on-screen readout works and both
        // paths stay comparable) but only APPLIED when hard-gate is on.
        float target = _gateOpen ? 1f : 0f;
        float ms = target > _loudSmooth ? _attackMs : _releaseMs;
        float k = 1f - Mathf.Exp(-1f / (_sampleRate * ms * 0.001f));

        bool applyExternal = _hardGateOutput;
        for (int i = 0; i < frames; i++)
        {
            _loudSmooth += (target - _loudSmooth) * k;
            float s = _mono[i] * gain * (applyExternal ? _loudSmooth : 1f);
            for (int c = 0; c < channels; c++)
                data[i * channels + c] = s;
        }
    }

    // ---- IMGUI panel ----
    Rect _win = new Rect(10, 10, 420, 640);
    void OnGUI()
    {
        // Escala global: el matrix transforma dibujado Y eventos, así que
        // arrastrar la ventana y mover sliders sigue funcionando.
        var prev = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
                                   new Vector3(uiScale, uiScale, 1f));

        if (_scaledSkin == null)
        {
            _scaledSkin = Instantiate(GUI.skin);
            _scaledSkin.label.fontSize = 12;
            _scaledSkin.button.fontSize = 12;
            _scaledSkin.toggle.fontSize = 12;
            _scaledSkin.window.fontSize = 12;
        }
        GUI.skin = _scaledSkin;

        _win = GUILayout.Window(0, _win, DrawWindow, "Pink Trombone Playground (fork)");

        GUI.matrix = prev;
    }

    void DrawWindow(int id)
    {
        GUILayout.Label($"Keys A–K: C major scale (hold = sustain). Gate: {(_gateOpen ? "ON" : "off")}");
        if (GUILayout.Button(_gateOpen ? "Note OFF (release)" : "Note ON (attack)"))
            _gateOpen = !_gateOpen;

        // ---- Defect-3 instrumentation: watch these while toggling the gate. ----
        // Hard-gate ON: if _loudSmooth visibly ramps at different speeds when
        // attack/release change, the envelope works (problem = perceptual/retrigger);
        // if it snaps regardless, the bug is in the envelope code.
        EnvBar($"ext envelope _loudSmooth: {_loudSmooth:F3}" + (_hardGateOutput ? "  (APPLIED)" : "  (computed only)"), _loudSmooth);
        EnvBar($"internal glottis Intensity: {_voice.Intensity:F3}", _voice.Intensity);

        _semitone = Slider("Pitch (semitones vs A4)", _semitone, -24, 12,
            $"{440f * Mathf.Pow(2f, _semitone / 12f):F1} Hz");
        _tenseness = Slider("Tenseness (breathy↔pressed)", _tenseness, 0f, 1f);
        _velLoudness = Slider("Loudness (velocity)", _velLoudness, 0f, 1f);
        _tongueIndex = Slider("Tongue index (live, glides)", _tongueIndex, 0f, 44f);
        _tongueDiameter = Slider("Tongue diameter (live, glides)", _tongueDiameter, 0f, 3.5f);
        _vibGain = Slider("Vibrato gain", _vibGain, 0f, 0.1f);
        _vibFreq = Slider("Vibrato freq (Hz)", _vibFreq, 3f, 9f);
        _wobble = GUILayout.Toggle(_wobble, "Vibrato wobble (random drift)");
        _alwaysVoice = GUILayout.Toggle(_alwaysVoice, "AlwaysVoice (OFF ⇒ gate articulates internally)");
        _hardGateOutput = GUILayout.Toggle(_hardGateOutput, "Hard-gate output (apply ext envelope; no breath in rests)");
        _attackMs = Slider("Attack (ms)", _attackMs, 1f, 60f);
        _releaseMs = Slider("Release (ms)", _releaseMs, 5f, 200f);
        gain = Slider("Master gain", gain, 0f, 1f);

        GUILayout.Space(6);
        GUILayout.Label("Vowel starting points (refine by ear):");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Default")) { _tongueIndex = 12.9f; _tongueDiameter = 2.43f; }
        if (GUILayout.Button("Open-ish")) { _tongueIndex = 14f; _tongueDiameter = 2.9f; }
        if (GUILayout.Button("Front-ish")) { _tongueIndex = 27f; _tongueDiameter = 2.1f; }
        if (GUILayout.Button("Back-ish")) { _tongueIndex = 18f; _tongueDiameter = 3.2f; }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Log current values (for step-6 preset)"))
            Debug.Log($"[PT preset] tongueIndex={_tongueIndex:F2} tongueDiameter={_tongueDiameter:F2} " +
                      $"tenseness={_tenseness:F2} vibGain={_vibGain:F3} vibFreq={_vibFreq:F1} " +
                      $"attack={_attackMs:F0}ms release={_releaseMs:F0}ms hardGate={_hardGateOutput} " +
                      $"alwaysVoice={_alwaysVoice} loudness={_velLoudness:F2}");
        GUI.DragWindow();
    }

    static void EnvBar(string label, float v01)
    {
        GUILayout.Label(label);
        Rect r = GUILayoutUtility.GetRect(360, 10);
        GUI.Box(r, GUIContent.none);
        var fill = new Rect(r.x + 1, r.y + 1, (r.width - 2) * Mathf.Clamp01(v01), r.height - 2);
        GUI.DrawTexture(fill, Texture2D.whiteTexture);
    }

    static float Slider(string label, float v, float min, float max, string extra = null)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(extra == null ? $"{label}: {v:F2}" : $"{label}: {v:F2}  ({extra})",
            GUILayout.Width(200));
        v = GUILayout.HorizontalSlider(v, min, max, GUILayout.Width(160));
        GUILayout.EndHorizontal();
        return v;
    }
}