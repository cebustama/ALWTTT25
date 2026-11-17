// [MGP-BENCH-1] Card bench: hear card combinations without playing a gig.
//
// Two columns (A/B). Each holds a seed, a lineup and an ordered list of
// cards with who plays each one. Render runs the gig's own chain, outside
// Play:  cards → CardBenchModel (mirror of the composition UI's model
// mutation) → SongConfigBuilder.FromModel (the gig's builder) →
// MidiMusicManager.RenderSinglePart (the gig's render, on an offline
// instance wired from the Core prefabs) → PlayRaw's metronome transform.
// The bytes are what the gig plays on the first loop of part A with the same
// seed and the same plays; "Comparar con volcado" checks it against a
// DevDumpMidi file.
//
// Plays MIDI in the editor with Maestro's MidiEditorLib (MidiFileEditorPlayer).
//
// [VOZ-3 / D4 = B] «▶ con voz»: Zig's melody stem is sung offline by the gig's own path
// (SingerVoice.PrepareV8 + VoiceRenderer, D-VOZ-6 = A) and played as an AudioClip that starts
// with Maestro (its melody channel removed, as the Director mutes it in the gig). Approximate
// sync (one editor frame). «.wav» saves the voice alone. The voice is the gig's choice (the
// melody card's palette, else Zig's profile on the prefab) unless «Voz» picks a profile.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ALWTTT.Cards;
using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Musicians;
using ALWTTT.Tooltips;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MidiGenPlay;
using MidiGenPlay.Composition;
using MidiGenPlay.Services;
using MidiPlayerTK;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Music.EditorTools
{
    public class CardBenchWindow : EditorWindow
    {
        private const string Tag = "<color=#7fd7ff>[CardBench]</color>";
        private const string PrefsKey = "ALWTTT.CardBench.State.v1";
        private const string MusicManagerPrefab = "Assets/Prefabs/Core/MidiMusicManager.prefab";
        private const string GigManagerPrefab = "Assets/Prefabs/Core/GigManager.prefab";
        private const string TooltipManagerPrefab = "Assets/Prefabs/UI/TooltipManager.prefab";

        // ── Persisted state (JSON in EditorPrefs; asset refs by GUID) ─────────
        [Serializable]
        private class Slot
        {
            public string cardGuid; public string musicianId = "";
            // [PATRON-BPM-1, Claudio 2026-11-17] Drum pattern forced for this card's
            // rhythm track (RenderSinglePart patternOverrides). "" = the palette draws.
            public string forcedPatternGuid = "";
        }
        [Serializable]
        private class Column
        {
            public string label;
            public int seed = 12345;
            public List<string> lineup = new();      // musician CharacterIds
            public List<Slot> slots = new();
            public string deckJson = "";
            // [BATERIA-SOLO-1] Render the rhythm tracks as a drum solo (A/B sheet).
            public bool drumSolo;
            public int drumSoloShape = -1;    // -1 by seed, 0 groove+fills, 1 chained, 2 trading
            public int drumSoloVariant = -1;  // -1 = by seed
            public int drumSoloHalfTimeAboveBpm = 150; // [D-SOLO-BPM] 0 = never
            public int bpm;                   // 0 = el que da la semilla (como el gig)
        }
        [Serializable]
        private class State
        {
            public Column a = new() { label = "A" };
            public Column b = new() { label = "B" };
            public bool metronome;
            public string configGuid, paletteGuid, mixGuid, glossaryGuid;
            public bool followSeed = true;            // B uses A's seed
        }

        private State _s;

        // ── Render results (not persisted) ───────────────────────────────────
        private class Result
        {
            public byte[] bytes;
            public float seconds;
            public int bpm;
            public string ts, key, harmony, hash;
            public List<(string mus, string role, string card, string bundle, string inst, string why)> tracks = new();
            public Dictionary<string, List<string>> drumLanes = new();   // [Claudio] mus → one line per drum
            public List<string> notes = new();
            public List<string> info = new();   // [BATERIA-SOLO-1] notes that are not warnings
            public string error;
            // [VOZ-3] what «▶ con voz» needs
            public Dictionary<MusicianTrackKey, byte[]> stems;
            public MidiGenPlay.MusicTheory.MusicTheory.TimeSignature timeSig;
            public int seed;
            public ALWTTT.Music.Voice.VoiceStylePaletteSO singerPalette;
            public AudioClip voiceClip;
            public float[] voiceSamples;
            public ALWTTT.Music.Voice.VoiceProfileSO voiceProfile;
            public string voiceInfo;
            public int melodyChannel = -1;
        }
        private Result _ra, _rb;

        // ── Resolved assets ─────────────────────────────────────────────────
        private MidiGenPlayConfig _config;
        private ChordProgressionPaletteSO _palette;
        private MixGainProfileSO _mix;
        private ConceptGlossarySO _glossary;
        private List<MusicianCharacterData> _musicians = new();

        // ── Playback ────────────────────────────────────────────────────────
        private MidiEditorLib _lib;
        private char _playing;   // 'A', 'B' or '\0'
        private double _pendingSeekMs;   // applied once Maestro has loaded the bytes
        private double _playStartedAt;
        private Vector2 _scroll;

        // ── [VOZ-3] Voice ───────────────────────────────────────────────────
        private const string SingerPrefab = "Assets/Prefabs/Core/Zig_SingerVoice.prefab";
        private static ALWTTT.Music.Voice.VoiceProfileSO _voiceOverride;   // null = the gig's choice
        private bool _withVoice;
        private bool _voicePending;
        private GameObject _voiceGo;
        private AudioSource _voiceSrc;
        private string _singerId;
        private ALWTTT.Music.Voice.VoiceProfileSO _singerProfile;

        /// <summary>[VOZ-3] Opens the bench with this profile as the voice of «▶ con voz» (importer).</summary>
        public static void OpenWithVoice(ALWTTT.Music.Voice.VoiceProfileSO profile)
        {
            _voiceOverride = profile;
            var w = GetWindow<CardBenchWindow>("Banco de cartas");
            if (w._ra != null) { w._ra.voiceClip = null; w._ra.voiceSamples = null; }
            if (w._rb != null) { w._rb.voiceClip = null; w._rb.voiceSamples = null; }
            w.Repaint();
        }

        [MenuItem("ALWTTT/Banco de cartas")]
        public static void Open() => GetWindow<CardBenchWindow>("Banco de cartas");

        private void OnEnable()
        {
            LoadState();
            ResolveAssets();
        }

        private void OnDisable()
        {
            SaveState();
            StopPlayback();
            if (_lib != null) { _lib.DestroyMidiObject(); _lib = null; }
            if (_voiceGo != null) { DestroyImmediate(_voiceGo); _voiceGo = null; _voiceSrc = null; }
        }

        private void Update()
        {
            // Maestro's own editor windows re-kick the AudioSource: it can stop by itself.
            if (_playing != '\0' && _lib?.MidiPlayer != null)
            {
                _lib.PlayAudioSource();
                if (_pendingSeekMs > 0 && _lib.MidiPlayer.MPTK_IsPlaying)
                {
                    _lib.MidiPlayer.MPTK_Position = _pendingSeekMs;
                    _pendingSeekMs = 0;
                }
                else if (_voicePending && _lib.MidiPlayer.MPTK_IsPlaying)
                {
                    // [VOZ-3] start the voice where Maestro is now (approximate sync)
                    _voicePending = false;
                    var vr = _playing == 'A' ? _ra : _rb;
                    if (vr?.voiceClip != null && _voiceSrc != null)
                    {
                        _voiceSrc.clip = vr.voiceClip;
                        _voiceSrc.time = Mathf.Clamp((float)(_lib.MidiPlayer.MPTK_Position / 1000.0), 0f, vr.voiceClip.length - 0.01f);
                        _voiceSrc.Play();
                    }
                }
                else if (_pendingSeekMs <= 0 && !_lib.MidiPlayer.MPTK_IsPlaying
                         && EditorApplication.timeSinceStartup - _playStartedAt > 1.0)
                { _playing = '\0'; Repaint(); }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // GUI
        // ════════════════════════════════════════════════════════════════════
        private void OnGUI()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorGUILayout.HelpBox("El banco funciona fuera de Play. Sal de Play para usarlo.",
                    MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUI.BeginChangeCheck();

            DrawShared();
            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width / 2 - 8)))
                    DrawColumn(_s.a, ref _ra, _rb, 'A');
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width / 2 - 8)))
                    DrawColumn(_s.b, ref _rb, _ra, 'B');
            }

            if (EditorGUI.EndChangeCheck()) SaveState();
            EditorGUILayout.EndScrollView();
        }

        private void DrawShared()
        {
            EditorGUILayout.LabelField("Banco de cartas", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Renderiza la parte A con la cadena del gig (modelo → SongConfigBuilder → " +
                "RenderSinglePart) sin entrar en Play. Mismas cartas, mismo orden, mismos " +
                "músicos y misma semilla = los bytes del primer loop del gig.", MessageType.None);

            _config = (MidiGenPlayConfig)EditorGUILayout.ObjectField("MGP Config", _config, typeof(MidiGenPlayConfig), false);
            _palette = (ChordProgressionPaletteSO)EditorGUILayout.ObjectField(
                new GUIContent("Paleta por defecto", "La de MidiMusicManager: armonía de las partes sin Backing."),
                _palette, typeof(ChordProgressionPaletteSO), false);
            _mix = (MixGainProfileSO)EditorGUILayout.ObjectField(
                new GUIContent("Mezcla (BAL-1)", "La de GigManager: ganancias que van en los bytes."),
                _mix, typeof(MixGainProfileSO), false);
            _glossary = (ConceptGlossarySO)EditorGUILayout.ObjectField(
                new GUIContent("Glosario", "Idioma del juego: el nombre de la parte va en los bytes."),
                _glossary, typeof(ConceptGlossarySO), false);
            _s.metronome = EditorGUILayout.Toggle("Metrónomo", _s.metronome);
            _s.followSeed = EditorGUILayout.Toggle(
                new GUIContent("B usa la semilla de A", "Para comparar solo las cartas."), _s.followSeed);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Valores del juego"))
                    ResolveAssets(fromPrefabs: true);
                if (GUILayout.Button("Copiar A → B")) { _s.b = Clone(_s.a, "B"); _rb = null; }
                if (GUILayout.Button("Copiar B → A")) { _s.a = Clone(_s.b, "A"); _ra = null; }
                if (GUILayout.Button("Renderizar las dos"))
                {
                    _ra = Render(_s.a, _s.a.seed);
                    _rb = Render(_s.b, SeedOf(_s.b));
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = _ra?.bytes != null && _rb?.bytes != null;
                if (GUILayout.Button(new GUIContent("⇄ Alternar A/B",
                        "Cambia a la otra columna en el mismo punto de la canción.")))
                    Toggle();
                GUI.enabled = true;
                if (GUILayout.Button("■ Parar")) StopPlayback();
                if (GUILayout.Button(new GUIContent("Limpiar las dos", "Cartas, mazo, solo y resultado de A y B; " +
                        "semillas y formaciones se quedan.")))
                {
                    StopPlayback();
                    ClearColumn(_s.a); ClearColumn(_s.b);
                    _ra = null; _rb = null;
                    GUI.changed = true;
                }
            }

            // [VOZ-3] Voice for «▶ con voz»
            var prevVoice = _voiceOverride;
            _voiceOverride = (ALWTTT.Music.Voice.VoiceProfileSO)EditorGUILayout.ObjectField(
                new GUIContent("Voz (▶ con voz)", "Vacío = la del gig: la paleta de la carta de melodía, si tiene; si no, el perfil de Zig en el prefab."),
                _voiceOverride, typeof(ALWTTT.Music.Voice.VoiceProfileSO), false);
            if (_voiceOverride != prevVoice)
            {
                if (_ra != null) { _ra.voiceClip = null; _ra.voiceSamples = null; }
                if (_rb != null) { _rb.voiceClip = null; _rb.voiceSamples = null; }
            }

            if (_ra?.hash != null && _rb?.hash != null)
            {
                bool same = _ra.hash == _rb.hash;
                EditorGUILayout.HelpBox(same
                        ? "A y B dan exactamente los mismos bytes."
                        : "A y B suenan distinto. En las columnas, en amarillo, lo que cambia.",
                    same ? MessageType.Info : MessageType.None);
            }
        }

        private int SeedOf(Column c) => (c == _s.b && _s.followSeed) ? _s.a.seed : c.seed;

        private void DrawColumn(Column c, ref Result r, Result other, char id)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Columna {id}", EditorStyles.boldLabel);
                // [BATERIA-SOLO-1, pedido de Claudio] Limpiar: cartas, mazo, solo y resultado.
                if (GUILayout.Button(new GUIContent("Limpiar", "Quita las cartas, el mazo JSON, el solo de batería " +
                        "y el resultado. La semilla y la formación se quedan."), GUILayout.Width(70)))
                {
                    if (_playing == id) StopPlayback();
                    ClearColumn(c);
                    r = null;
                    GUI.changed = true;
                }
            }

            using (new EditorGUI.DisabledScope(c == _s.b && _s.followSeed))
                c.seed = EditorGUILayout.IntField("Semilla", SeedOf(c));
            // [Claudio, 2026-11-17] BPM forzado (RenderSinglePart bpmOverride). 0 = el del gig.
            c.bpm = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent("BPM (0 = semilla)",
                "0: el tempo que sortea la semilla, como en el gig. Otro valor lo fuerza " +
                "(entonces los bytes ya no coinciden con el volcado del gig)."), c.bpm), 0, 300);

            // [BATERIA-SOLO-1] ▶ Solo: the rhythm tracks play as a drum solo.
            c.drumSolo = EditorGUILayout.ToggleLeft(
                new GUIContent("Solo de batería", "Las pistas de ritmo suenan como el solo del Finisher de C2 " +
                    "(fills encadenados y cadencia). La banda no se agacha aquí: eso lo hace el gig."), c.drumSolo);
            if (c.drumSolo)
            {
                c.drumSoloShape = GUILayout.Toolbar(c.drumSoloShape + 1, new[] { "Semilla", "Groove + fills", "Encadenados", "Trading" }) - 1;
                c.drumSoloVariant = GUILayout.Toolbar(c.drumSoloVariant + 1,
                    new[] { "Semilla", "Toms 3", "Cascada", "Rudim.", "Rituales", "Máquina" }) - 1;
                c.drumSoloHalfTimeAboveBpm = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent(
                    "Medio tiempo sobre (BPM)", "Por encima de este tempo el solo toca a media resolución " +
                    "(semicorcheas → corcheas, seisillos → tresillos). 0 = nunca."), c.drumSoloHalfTimeAboveBpm), 0, 300);
            }

            // Lineup
            EditorGUILayout.LabelField("Formación", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var m in _musicians)
                {
                    bool on = c.lineup.Contains(m.CharacterId);
                    bool now = GUILayout.Toggle(on, m.CharacterName, "Button");
                    if (now != on)
                    {
                        if (now) c.lineup.Add(m.CharacterId);
                        else c.lineup.Remove(m.CharacterId);
                    }
                }
            }

            // Cards
            EditorGUILayout.LabelField("Cartas, en el orden en que se juegan", EditorStyles.miniBoldLabel);
            var lineup = _musicians.Where(m => c.lineup.Contains(m.CharacterId)).ToList();
            var options = new[] { "(auto)" }.Concat(lineup.Select(m => m.CharacterName)).ToArray();
            for (int i = 0; i < c.slots.Count; i++)
            {
                var slot = c.slots[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    int sel = Mathf.Max(0, lineup.FindIndex(m => m.CharacterId == slot.musicianId) + 1);
                    int newSel = EditorGUILayout.Popup(sel, options, GUILayout.Width(90));
                    slot.musicianId = newSel == 0 ? "" : lineup[newSel - 1].CharacterId;

                    // [MGP-BENCH-1] With a musician chosen: a labelled popup of the
                    // cards that musician can play. "(auto)": any card asset.
                    var card = LoadCard(slot.cardGuid);
                    var who = newSel == 0 ? null : lineup[newSel - 1];
                    if (who != null)
                    {
                        var pool = PoolFor(who);
                        var list = card != null && !pool.Contains(card)
                            ? new List<CardDefinition>(pool) { card } : pool;
                        var labels = new[] { "(elige carta)" }
                            .Concat(list.Select(cd => CardMenuPath(cd, !pool.Contains(cd)))).ToArray();
                        int ci = card == null ? 0 : list.IndexOf(card) + 1;
                        int nci = EditorGUILayout.Popup(ci, labels);
                        if (nci != ci) slot.cardGuid = nci == 0 ? "" : GuidOf(list[nci - 1]);
                        var picked = (CardDefinition)EditorGUILayout.ObjectField(
                            LoadCard(slot.cardGuid), typeof(CardDefinition), false, GUILayout.Width(24));
                        if (picked != LoadCard(slot.cardGuid)) slot.cardGuid = GuidOf(picked);
                    }
                    else
                    {
                        var newCard = (CardDefinition)EditorGUILayout.ObjectField(card, typeof(CardDefinition), false);
                        if (newCard != card) slot.cardGuid = GuidOf(newCard);
                    }

                    if (GUILayout.Button("↑", GUILayout.Width(22)) && i > 0)
                    { (c.slots[i - 1], c.slots[i]) = (c.slots[i], c.slots[i - 1]); GUIUtility.ExitGUI(); }
                    if (GUILayout.Button("✕", GUILayout.Width(22)))
                    { c.slots.RemoveAt(i); GUIUtility.ExitGUI(); }
                }
                DrawPatternRow(c, slot);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Carta")) c.slots.Add(new Slot());
                if (GUILayout.Button(new GUIContent("🎲 Cartas al azar",
                        "Una carta de composición al azar por músico de la formación, sin repetir rol.")))
                    RandomizeCards(c, lineup);
                using (new EditorGUI.DisabledScope(c == _s.b && _s.followSeed))
                    if (GUILayout.Button(new GUIContent("🎲 Semilla", "Semilla nueva al azar."), GUILayout.Width(80)))
                        c.seed = UnityEngine.Random.Range(1, 1_000_000);
            }

            // Fixed deck JSON (DEV-HAND-1 format)
            EditorGUILayout.LabelField("Mazo JSON (formato DEV-HAND-1)", EditorStyles.miniBoldLabel);
            c.deckJson = EditorGUILayout.TextArea(c.deckJson ?? "", GUILayout.MinHeight(34));
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cargar mazo")) LoadDeck(c);
                if (GUILayout.Button("Mazo de GigDevSettings")) LoadDeckFromDevSettings(c);
            }

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Renderizar", GUILayout.Height(24)))
                    r = Render(c, SeedOf(c));
                GUI.enabled = r?.bytes != null;
                if (GUILayout.Button("▶", GUILayout.Width(36), GUILayout.Height(24)))
                { _withVoice = false; Play(id, 0); }
                if (GUILayout.Button(new GUIContent("▶ con voz", "Zig canta la melodía con el cantante v8 (render offline)."), GUILayout.Width(80), GUILayout.Height(24)))
                { _withVoice = true; Play(id, 0); }
                if (GUILayout.Button(new GUIContent(".wav", "Solo la voz, renderizada offline."), GUILayout.Width(44), GUILayout.Height(24)))
                    ExportVoiceWav(r, id);
                if (GUILayout.Button("Exportar .mid", GUILayout.Height(24)))
                    Export(r, id, SeedOf(c));
                GUI.enabled = true;
            }
            GUI.enabled = r?.bytes != null;
            if (GUILayout.Button("Comparar con volcado del gig…")) CompareWithDump(r);
            GUI.enabled = true;

            DrawResult(r, other);
        }

        private void DrawResult(Result r, Result other)
        {
            if (r == null) return;
            if (r.error != null) { EditorGUILayout.HelpBox(r.error, MessageType.Error); return; }

            var warn = new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(1f, .8f, .2f) } };
            GUIStyle St(bool differs) => differs ? warn : EditorStyles.label;
            bool cmp = other?.bytes != null;

            EditorGUILayout.LabelField($"{r.bpm} BPM · {r.ts} · {r.seconds:0.0} s",
                St(cmp && (other.bpm != r.bpm || other.ts != r.ts)));
            EditorGUILayout.LabelField($"Tonalidad: {r.key}", St(cmp && other.key != r.key));
            EditorGUILayout.LabelField($"Armonía: {(r.harmony == "None" ? "ninguna (nadie toca acordes ni bajo)" : r.harmony)}", St(cmp && other.harmony != r.harmony));
            EditorGUILayout.LabelField($"Huella: {r.hash}", EditorStyles.miniLabel);
            if (r.voiceInfo != null) EditorGUILayout.LabelField($"Voz: {r.voiceInfo}", EditorStyles.wordWrappedMiniLabel);

            foreach (var t in r.tracks)
            {
                bool differs = cmp && !other.tracks.Contains(t);
                EditorGUILayout.LabelField($"{t.role} · {t.mus}", St(differs), GUILayout.Height(16));
                EditorGUILayout.LabelField($"    {t.card}  [{t.bundle}]  → {t.inst}",
                    differs ? warn : EditorStyles.miniLabel);
                // [Claudio, 2026-11-17] Por qué suena así: lo que el compositor eligió.
                if (!string.IsNullOrEmpty(t.why))
                    EditorGUILayout.LabelField($"    ↳ {t.why}",
                        differs ? warn : EditorStyles.wordWrappedMiniLabel);
                // [Claudio, 2026-11-17] Each drum of the rhythm stem: hits and velocity.
                if (t.role == nameof(TrackRole.Rhythm) && r.drumLanes.TryGetValue(t.mus, out var lanes))
                    foreach (var l in lanes)
                        EditorGUILayout.LabelField("        " + l, _mono ??= new GUIStyle(EditorStyles.miniLabel) { font = Font.CreateDynamicFontFromOSFont("Consolas", 10) });
            }
            foreach (var n in r.info)
                EditorGUILayout.HelpBox(n, MessageType.Info);
            foreach (var n in r.notes)
                EditorGUILayout.HelpBox(n, MessageType.Warning);
        }

        // ════════════════════════════════════════════════════════════════════
        // Render
        // ════════════════════════════════════════════════════════════════════
        private Result Render(Column c, int seed)
        {
            var res = new Result();
            if (_config == null) ResolveAssets();
            var plays = ResolvePlays(c, res.notes);
            if (plays.Count == 0) { res.error = "No hay cartas que jugar."; return res; }

            var prevGlossary = GameText.Glossary;
            bool prevLog = SongConfigBuilder.LogEnabled;
            GameObject go = null;
            try
            {
                if (_glossary != null) GameText.SetEditorFallback(_glossary);
                SongConfigBuilder.LogEnabled = false;

                var model = CardBenchModel.Build(plays, res.notes);
                var part0 = model.parts[0];
                if (part0.tracks.Count == 0)
                { res.error = "La parte A queda sin pistas: nada que sonar."; return res; }

                var instruments = new InstrumentRepositoryResources(_config);
                instruments.Refresh();
                var byId = _musicians.ToDictionary(m => m.CharacterId);
                MusicianCharacterData Mus(string id) => byId.TryGetValue(id, out var m) ? m : null;

                var cfg = SongConfigBuilder.FromModel(
                    model, instruments,
                    resolvePermittedMelodic: (id, role) =>
                        InstrumentRules.GetPermittedMelodic(Mus(id), role, instruments),
                    resolvePermittedPercussion: id =>
                        InstrumentRules.GetPermittedPercussion(Mus(id), instruments),
                    // A fresh run seeds each musician's gameplay data with the
                    // profile's default melodic leading and no harmonic one
                    // (PersistentGameplayData.SetMusicianGameplayData).
                    resolveLeading: id => (Mus(id)?.Profile?.defaultMelodicLeading, null),
                    resolvePinned: null,   // first loop: nothing pinned yet
                    log: null,
                    rng: new System.Random(seed),
                    songSeed: seed);
                if (cfg == null) { res.error = "SongConfigBuilder no devolvió configuración."; return res; }

                go = new GameObject("CardBench_OfflineRender") { hideFlags = HideFlags.HideAndDontSave };
                var mm = go.AddComponent<MidiMusicManager>();
                mm.InitForOfflineRender(_config, _palette, _mix != null ? _mix.BuildGainMap() : null);

                // [BATERIA-SOLO-1] Same request the gig makes, on every rhythm track.
                if (c.drumSolo)
                    foreach (var t in cfg.Parts[0].Tracks)
                        if (t != null && t.Role == TrackRole.Rhythm)
                        {
                            t.Parameters ??= new TrackParameters();
                            t.Parameters.DrumSolo = new DrumSoloRequest
                            {
                                shape = (DrumSoloShape)Mathf.Clamp(c.drumSoloShape, 0, 2),
                                shapeFromSeed = c.drumSoloShape < 0,
                                variant = c.drumSoloVariant,
                                halfTimeAboveBpm = c.drumSoloHalfTimeAboveBpm,
                            };
                            res.info.Add($"Solo de batería (pista de ritmo): {t.Parameters.DrumSolo}");
                        }

                var forcedPatterns = ForcedPatterns(c, plays, res.info); // [PATRON-BPM-1]
                var (merged, stems, seconds, bpm, _) = mm.RenderSinglePart(cfg, 0,
                    bpmOverride: c.bpm > 0 ? c.bpm : (int?)null, seedOverride: seed,
                    patternOverrides: forcedPatterns.Count > 0 ? forcedPatterns : null);
                if (merged == null || merged.Length == 0)
                { res.error = "El render falló: mira la consola."; return res; }

                res.bytes = MidiMusicManager.ApplyMetronomeVolumeForPlayback(merged, _s.metronome);
                res.seconds = seconds;
                res.bpm = bpm;
                var p = cfg.Parts[0];
                res.ts = p.TimeSignature.ToString();
                res.key = $"{p.RootNote} {p.Tonality}";
                res.harmony = $"{mm.LastSharedProgressionSource} {mm.LastSharedProgressionAssetName ?? ""}".Trim();
                res.hash = Fnv(res.bytes);
                res.stems = stems;                       // [VOZ-3]
                res.timeSig = cfg.Parts[0].TimeSignature;
                res.seed = seed;
                res.singerPalette = SingerPalette(part0);

                foreach (var t in p.Tracks)
                {
                    var entry = part0.tracks.FirstOrDefault(e => e.musicianId == t.MusicianId && e.role == t.Role);
                    var inst = t.PercussionInstrument != null ? t.PercussionInstrument.InstrumentName
                             : t.Instrument != null ? t.Instrument.InstrumentName : "—";
                    if (t.Role == TrackRole.Rhythm && stems != null &&
                        stems.TryGetValue(new MusicianTrackKey(t.MusicianId, t.Role), out var stem))
                        res.drumLanes[Mus(t.MusicianId)?.CharacterName ?? t.MusicianId] =
                            DrumLanes(stem, t.PercussionInstrument);
                    ResolvedTrackChoice choice = null;
                    mm.LastResolvedByTrack?.TryGetValue(new MusicianTrackKey(t.MusicianId, t.Role), out choice);
                    res.tracks.Add((Mus(t.MusicianId)?.CharacterName ?? t.MusicianId, t.Role.ToString(),
                        entry?.sourceCardDefinition != null ? entry.sourceCardDefinition.DisplayName : "—",
                        t.Parameters?.Style != null ? t.Parameters.Style.name : "sin bundle", inst,
                        Why(t.Role, choice)));
                    // [PATRON-BPM-1] Avisa si el patrón de batería quedó fuera de su tempo cómodo.
                    if (t.Role == TrackRole.Rhythm && choice != null)
                    {
                        var who = Mus(t.MusicianId)?.CharacterName ?? t.MusicianId;
                        if (choice.tempoFilter.status == TempoFilterStatus.FellBack)
                            res.notes.Add($"{who}: ningún patrón de la paleta «{choice.paletteName}» " +
                                          $"encaja a {choice.tempoFilter.partBpm} BPM; se sorteó entre todos " +
                                          $"({choice.sourceAssetName}, {BpmRange(choice.patternBpmMin, choice.patternBpmMax)}).");
                        else if (choice.patternOutOfRange)
                            res.notes.Add($"{who}: {choice.sourceAssetName} está fuera de su tempo cómodo " +
                                          $"({BpmRange(choice.patternBpmMin, choice.patternBpmMax)}) a {res.bpm} BPM; " +
                                          "es un patrón fijo, no se filtra.");
                    }
                }

                Debug.Log($"{Tag} {c.label}: seed={seed} {res.bpm} BPM {res.key} {res.ts} " +
                          $"tracks={res.tracks.Count} hash={res.hash}");
            }
            catch (Exception ex)
            {
                res.error = $"{ex.GetType().Name}: {ex.Message}";
                Debug.LogException(ex);
            }
            finally
            {
                GameText.SetEditorFallback(prevGlossary);
                SongConfigBuilder.LogEnabled = prevLog;
                if (go != null) DestroyImmediate(go);
            }
            return res;
        }

        /// <summary>Card + target, with the gig's target rules: a fixed-performer
        /// card goes to that musician (must be in the lineup); a card that needs
        /// a target and has none chosen goes to the first musician of the lineup.</summary>
        // ── [PATRON-BPM-1, Claudio 2026-11-17] Second row under a rhythm card ──
        // Shows the card's palette (or fixed pattern) and lets one pattern be
        // forced for this render. Each option shows its comfortable tempo and
        // whether it fits the column's forced BPM.
        private static RhythmCardConfigSO RhythmBundleOf(CardDefinition card)
        {
            var comp = card != null ? card.CompositionPayload : null;
            if (comp == null || comp.PrimaryKind != CardPrimaryKind.Track) return null;
            var desc = comp.TrackAction;
            return desc != null && desc.role == TrackRole.Rhythm
                ? desc.styleBundle as RhythmCardConfigSO : null;
        }

        private static List<(DrumPatternData p, float w)> PatternsOf(RhythmCardConfigSO r)
        {
            var list = new List<(DrumPatternData, float)>();
            if (r.patternOverride != null) list.Add((r.patternOverride, -1f)); // -1 = fixed on the card
            if (r.patternPalette?.entries != null)
                foreach (var e in r.patternPalette.entries)
                    if (e?.pattern != null && !list.Any(x => x.Item1 == e.pattern)) list.Add((e.pattern, e.weight));
            return list;
        }

        private void DrawPatternRow(Column c, Slot slot)
        {
            var r = RhythmBundleOf(LoadCard(slot.cardGuid));
            if (r == null) { slot.forcedPatternGuid = ""; return; }
            var pats = PatternsOf(r);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(94);
                string src = r.patternOverride != null ? "patrón fijo de la carta"
                           : r.patternPalette != null ? $"paleta «{r.patternPalette.GetDisplayName()}»"
                           : "sin paleta (procedural)";
                if (r.patternPalette != null)
                {
                    if (GUILayout.Button(new GUIContent(src, "Selecciona la paleta en el Project."),
                            EditorStyles.linkLabel, GUILayout.ExpandWidth(false)))
                        EditorGUIUtility.PingObject(r.patternPalette);
                }
                else EditorGUILayout.LabelField(src, EditorStyles.miniLabel, GUILayout.ExpandWidth(false));

                if (pats.Count == 0) return;
                var labels = new List<string> { c.bpm > 0 ? $"(sorteo a {c.bpm} BPM)" : "(sorteo)" };
                foreach (var (p, w) in pats)
                {
                    string range = p.HasBpmRange ? $" · {BpmRange(p.bpmMin, p.bpmMax)}" : "";
                    string fit = c.bpm > 0 && p.HasBpmRange ? (p.FitsBpm(c.bpm) ? " ✓" : " ✗") : "";
                    string weight = w < 0 ? " · fijo" : w <= 0f ? " · peso 0" : "";
                    labels.Add($"{p.name}{range}{fit}{weight}");
                }
                var forced = Load<DrumPatternData>(slot.forcedPatternGuid);
                int sel = forced == null ? 0 : pats.FindIndex(x => x.p == forced) + 1;
                if (forced != null && sel == 0) { slot.forcedPatternGuid = ""; }
                int nsel = EditorGUILayout.Popup(new GUIContent("", "Fuerza un patrón para este render " +
                    "(✓/✗ = encaja o no en el BPM de la columna)."), sel, labels.ToArray());
                if (nsel != sel) slot.forcedPatternGuid = nsel == 0 ? "" : GuidOf(pats[nsel - 1].p);
                using (new EditorGUI.DisabledScope(nsel == 0))
                    if (GUILayout.Button(new GUIContent("◎", "Selecciona el patrón en el Project."), GUILayout.Width(24)))
                        EditorGUIUtility.PingObject(pats[nsel - 1].p);
            }
        }

        // Forced patterns keyed like the gig's tracks. A later card that replaces
        // the same musician's rhythm track replaces (or clears) its override.
        private Dictionary<MusicianTrackKey, PatternDataSO> ForcedPatterns(
            Column c, List<CardBenchModel.Play> plays, List<string> info)
        {
            var dict = new Dictionary<MusicianTrackKey, PatternDataSO>();
            int pi = 0;
            foreach (var slot in c.slots)
            {
                var card = LoadCard(slot.cardGuid);
                if (card == null) continue;
                // ResolvePlays keeps slot order and skips only cards it rejects.
                while (pi < plays.Count && plays[pi].card != card) pi++;
                if (pi >= plays.Count) break;
                var target = plays[pi].target;
                pi++;
                if (RhythmBundleOf(card) == null || target == null) continue;
                var key = new MusicianTrackKey(target.CharacterId, TrackRole.Rhythm);
                var forced = Load<DrumPatternData>(slot.forcedPatternGuid);
                if (forced != null)
                {
                    dict[key] = forced;
                    info.Add($"{target.CharacterName}: patrón forzado {forced.name} (sin sorteo).");
                }
                else dict.Remove(key);
            }
            return dict;
        }

        private List<CardBenchModel.Play> ResolvePlays(Column c, List<string> notes)
        {
            var lineup = _musicians.Where(m => c.lineup.Contains(m.CharacterId)).ToList();
            var plays = new List<CardBenchModel.Play>();
            foreach (var slot in c.slots)
            {
                var card = LoadCard(slot.cardGuid);
                if (card == null) continue;
                var comp = card.CompositionPayload;
                MusicianCharacterData target = null;

                if (card.RequiresFixedPerformer)
                {
                    target = lineup.FirstOrDefault(m => m.CharacterType == card.FixedPerformerType);
                    if (target == null)
                    {
                        notes.Add($"«{card.DisplayName}»: la toca {card.FixedPerformerType}, que no está " +
                                  "en la formación (en el gig no se puede jugar).");
                        continue;
                    }
                    if (!string.IsNullOrEmpty(slot.musicianId) && slot.musicianId != target.CharacterId)
                        notes.Add($"«{card.DisplayName}»: tiene intérprete fijo, {target.CharacterName}.");
                }
                else if (comp != null && comp.RequiresMusicianTarget)
                {
                    target = lineup.FirstOrDefault(m => m.CharacterId == slot.musicianId);
                    if (target == null)
                    {
                        target = lineup.FirstOrDefault();
                        if (target != null)
                            notes.Add($"«{card.DisplayName}»: sin músico elegido; la toca {target.CharacterName}.");
                    }
                }
                plays.Add(new CardBenchModel.Play(card, target));
            }
            return plays;
        }

        // ════════════════════════════════════════════════════════════════════
        // Playback (Maestro, edit mode)
        // ════════════════════════════════════════════════════════════════════
        private void Play(char id, double positionMs)
        {
            var r = id == 'A' ? _ra : _rb;
            if (r?.bytes == null) return;
            StopVoice();
            byte[] bytes = r.bytes;
            // [BATERIA-SOLO-1] Maestro in edit mode ignores the CC7 = 0 that silences the
            // metronome (the gig mutes it through MPTK_Channels instead), so the bench played
            // its D5/D#5 clicks. Playback only: r.bytes (hash, .mid, gig comparison) keep it.
            if (!_s.metronome) bytes = WithoutChannel(bytes, MidiGenerator.MetronomeChannel);
            if (_withVoice)
            {
                if (r.voiceClip == null) RenderVoice(r);
                if (r.voiceClip == null)
                {
                    ShowNotification(new GUIContent("Sin voz: " + (r.voiceInfo ?? "mira la consola")));
                    _withVoice = false;
                }
                else
                {
                    bytes = WithoutChannel(r.bytes, r.melodyChannel);
                    EnsureVoiceSource();
                    _voicePending = true;
                }
            }
            try
            {
                _lib ??= new MidiEditorLib("CardBench_MidiPlayer");
                var mp = _lib.MidiPlayer;
                if (!mp.MPTK_SoundFont.IsReady)
                {
                    ShowNotification(new GUIContent("Cargando el soundfont… pulsa ▶ otra vez en un momento."));
                    return;
                }
                mp.MPTK_Stop();
                _lib.PlayAudioSource();
                // [MGP-BENCH-1] MPTK_Play(byte[]) schedules its coroutine on
                // Segment.RealtimeUpdate, which never ticks outside Play mode
                // (silence in the editor). Same steps, on EditorUpdate — the
                // segment Maestro's own MPTK_Play() uses when !isPlaying.
                mp.MPTK_InitSynth();
                mp.MPTK_StartSequencerMidi();
                Routine.RunCoroutine(mp.ThreadCorePlay(bytes), Segment.EditorUpdate);
                _pendingSeekMs = positionMs;
                _playStartedAt = EditorApplication.timeSinceStartup;
                _playing = id;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Banco de cartas",
                    "Maestro no pudo reproducir en el editor. Usa «Exportar .mid».\n\n" + ex.Message, "OK");
            }
        }

        private void Toggle()
        {
            double pos = 0;
            if (_playing != '\0' && _lib?.MidiPlayer != null) pos = _lib.MidiPlayer.MPTK_Position;
            Play(_playing == 'A' ? 'B' : 'A', pos);
        }

        private void StopPlayback()
        {
            if (_lib?.MidiPlayer != null) _lib.MidiPlayer.MPTK_Stop();
            _playing = '\0';
            StopVoice();
        }

        // ════════════════════════════════════════════════════════════════════
        // [VOZ-3] Voice: offline render of the singer v8, played next to Maestro
        // ════════════════════════════════════════════════════════════════════
        private void StopVoice()
        {
            _voicePending = false;
            if (_voiceSrc != null) _voiceSrc.Stop();
        }

        private void EnsureVoiceSource()
        {
            if (_voiceSrc != null) return;
            _voiceGo = new GameObject("CardBench_Voice") { hideFlags = HideFlags.HideAndDontSave };
            _voiceSrc = _voiceGo.AddComponent<AudioSource>();
            _voiceSrc.playOnAwake = false; _voiceSrc.spatialBlend = 0f;
        }

        private void LoadSinger()
        {
            if (_singerProfile != null && !string.IsNullOrEmpty(_singerId)) return;
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(SingerPrefab);
            var dir = go != null ? go.GetComponentInChildren<ALWTTT.Music.Voice.SingerVoiceDirector>(true) : null;
            if (dir == null) return;
            var so = new SerializedObject(dir);
            _singerId = so.FindProperty("musicianId")?.stringValue;
            _singerProfile = so.FindProperty("profile")?.objectReferenceValue as ALWTTT.Music.Voice.VoiceProfileSO;
        }

        /// <summary>The voice palette of the singer's melody card in part A (CompositionSession.CollectVoicePalettes).</summary>
        private ALWTTT.Music.Voice.VoiceStylePaletteSO SingerPalette(ALWTTT.UI.SongCompositionUI.PartEntry part0)
        {
            LoadSinger();
            if (part0 == null || string.IsNullOrEmpty(_singerId)) return null;
            foreach (var tr in part0.tracks)
            {
                if (tr == null || tr.musicianId != _singerId) continue;
                if (tr.role != TrackRole.Melody && tr.role != TrackRole.Lead) continue;
                var pal = tr.sourceCardDefinition != null ? tr.sourceCardDefinition.CompositionPayload?.TrackAction?.voicePalette : null;
                if (pal != null) return pal;
            }
            return null;
        }

        private void RenderVoice(Result r)
        {
            LoadSinger();
            r.voiceClip = null; r.voiceSamples = null;
            if (string.IsNullOrEmpty(_singerId)) { r.voiceInfo = $"no encuentro el SingerVoiceDirector en {SingerPrefab}."; return; }
            byte[] stem = null;
            if (r.stems != null)
                foreach (var wanted in new[] { TrackRole.Melody, TrackRole.Lead })
                {
                    foreach (var kv in r.stems)
                        if (kv.Key.MusicianId == _singerId && kv.Key.Role == wanted && kv.Value != null && kv.Value.Length > 0)
                        { stem = kv.Value; break; }
                    if (stem != null) break;
                }
            if (stem == null) { r.voiceInfo = $"la parte A no tiene melodía del cantante (musicianId {_singerId})."; return; }

            string source;
            ALWTTT.Music.Voice.VoiceProfileSO profile;
            var picked = r.singerPalette != null ? r.singerPalette.Pick(r.seed, 0) : null;
            if (_voiceOverride != null) { profile = _voiceOverride; source = "elegida en el banco"; }
            else if (picked != null) { profile = picked; source = "paleta " + r.singerPalette.name; }
            else { profile = _singerProfile; source = "perfil del prefab"; }
            if (profile == null) { r.voiceInfo = "sin perfil de voz."; return; }
            if (profile.engine != ALWTTT.Music.Voice.VoiceProfileSO.SingerEngine.V8_Workshop)
            { r.voiceInfo = $"{profile.name} usa el motor V7: el banco solo canta V8."; return; }

            int sr = AudioSettings.outputSampleRate > 0 ? AudioSettings.outputSampleRate : 48000;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var samples = ALWTTT.Music.Voice.SingerVoice.RenderV8Offline(stem, r.bpm, r.timeSig, profile, r.seed, 0, sr,
                out var prep, out string err);
            sw.Stop();
            if (samples == null) { r.voiceInfo = "el render falló: " + err; Debug.LogWarning($"[VOZ-3] Banco: {err}"); return; }
            float peak = 0f; foreach (var x in samples) peak = Mathf.Max(peak, Mathf.Abs(x));
            var clip = AudioClip.Create($"voz_{profile.name}", samples.Length, 1, sr, false);
            clip.SetData(samples, 0);
            r.voiceClip = clip; r.voiceSamples = samples; r.voiceProfile = profile;
            r.melodyChannel = FirstNoteChannel(stem);
            double audioSec = (double)samples.Length / sr;
            r.voiceInfo = $"{profile.name} [{source}] · {prep.Notes.Length} notas · {audioSec:0.0} s en {sw.ElapsedMilliseconds} ms";
            Debug.Log($"[VOZ-3] Banco ▶ con voz: profile={profile.name} [{source}], bank={prep.BankInfo}, seed={r.seed}, " +
                      $"bpm={r.bpm}, notes={prep.Notes.Length}, phraseEnds={prep.PhraseEnds}, sr={sr}, " +
                      $"render {sw.ElapsedMilliseconds} ms para {audioSec:0.0} s ({100.0 * sw.Elapsed.TotalSeconds / audioSec:0.0} % de tiempo real), " +
                      $"pico {peak:0.000}, canal mudo {r.melodyChannel}" +
                      (prep.Lexicon != null ? $", lexicon=[{string.Join(" ", prep.Lexicon)}]" : "") + $"\n  sings: {prep.Sung()}");
        }

        private static int FirstNoteChannel(byte[] stem)
        {
            try
            {
                using var ms = new MemoryStream(stem);
                foreach (var tc in MidiFile.Read(ms).GetTrackChunks())
                    foreach (var e in tc.Events)
                        if (e is NoteOnEvent on && on.Velocity > 0) return on.Channel;
            }
            catch (Exception ex) { Debug.LogWarning("[VOZ-3] Banco: no pude leer el canal de la melodía: " + ex.Message); }
            return -1;
        }

        /// <summary>The merged bytes without the singer's channel notes (the gig mutes that channel).</summary>
        private static byte[] WithoutChannel(byte[] bytes, int channel)
        {
            if (channel < 0) return bytes;
            try
            {
                using var ms = new MemoryStream(bytes);
                var f = MidiFile.Read(ms);
                foreach (var tc in f.GetTrackChunks())
                    tc.Events.RemoveNotes(n => n.Channel == channel);   // keeps the other events' timing
                using var o = new MemoryStream();
                f.Write(o);
                return o.ToArray();
            }
            catch (Exception ex) { Debug.LogWarning($"[VOZ-3] Banco: no pude quitar el canal {channel}: " + ex.Message); return bytes; }
        }

        private void ExportVoiceWav(Result r, char id)
        {
            if (r?.bytes == null) return;
            if (r.voiceSamples == null) RenderVoice(r);
            if (r.voiceSamples == null) { ShowNotification(new GUIContent("Sin voz: " + (r.voiceInfo ?? "mira la consola"))); return; }
            var dir = Path.Combine(Application.persistentDataPath, "CardBench");
            Directory.CreateDirectory(dir);
            var path = EditorUtility.SaveFilePanel("Exportar la voz (.wav)", dir,
                $"voz_{id}_{r.voiceProfile.name}_s{r.seed}.wav", "wav");
            if (string.IsNullOrEmpty(path)) return;
            WriteWav(path, r.voiceSamples, r.voiceClip.frequency);
            Debug.Log($"[VOZ-3] Banco: voz exportada {path} ({r.voiceSamples.Length} muestras @ {r.voiceClip.frequency} Hz, {r.voiceProfile.name}).");
            EditorUtility.RevealInFinder(path);
        }

        /// <summary>32-bit float mono WAV (format 3): the samples exactly, for parity checks.</summary>
        private static void WriteWav(string path, float[] samples, int sr)
        {
            using var fs = new FileStream(path, FileMode.Create);
            using var w = new BinaryWriter(fs);
            int dataBytes = samples.Length * 4;
            w.Write(Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + dataBytes); w.Write(Encoding.ASCII.GetBytes("WAVE"));
            w.Write(Encoding.ASCII.GetBytes("fmt ")); w.Write(16); w.Write((short)3); w.Write((short)1);
            w.Write(sr); w.Write(sr * 4); w.Write((short)4); w.Write((short)32);
            w.Write(Encoding.ASCII.GetBytes("data")); w.Write(dataBytes);
            foreach (var x in samples) w.Write(x);
        }

        // ════════════════════════════════════════════════════════════════════
        // Export and parity
        // ════════════════════════════════════════════════════════════════════
        private void Export(Result r, char id, int seed)
        {
            if (r?.bytes == null) return;
            var dir = Path.Combine(Application.persistentDataPath, "CardBench");
            Directory.CreateDirectory(dir);
            var path = EditorUtility.SaveFilePanel("Exportar .mid", dir,
                $"banco_{id}_s{seed}_{r.hash}.mid", "mid");
            if (string.IsNullOrEmpty(path)) return;
            File.WriteAllBytes(path, r.bytes);
            Debug.Log($"{Tag} Exportado {path}");
            EditorUtility.RevealInFinder(path);
        }

        private void CompareWithDump(Result r)
        {
            var dir = Path.Combine(Application.persistentDataPath, "MidiDumps");
            var path = EditorUtility.OpenFilePanel("Volcado del gig (DevDumpMidi)",
                Directory.Exists(dir) ? dir : Application.persistentDataPath, "mid");
            if (string.IsNullOrEmpty(path)) return;
            var dump = File.ReadAllBytes(path);
            string msg = r.bytes.SequenceEqual(dump)
                ? "IDÉNTICO: el banco da los mismos bytes que el gig."
                : "DISTINTO.\n\n" + DescribeDiff(r.bytes, dump);
            Debug.Log($"{Tag} Paridad con {Path.GetFileName(path)}: {msg}");
            EditorUtility.DisplayDialog("Paridad con el gig", msg, "OK");
        }

        /// <summary>Per-chunk comparison of timed events: where the two files part ways.</summary>
        private static string DescribeDiff(byte[] bench, byte[] gig)
        {
            try
            {
                List<List<string>> Events(byte[] b)
                {
                    using var ms = new MemoryStream(b);
                    return MidiFile.Read(ms).GetTrackChunks()
                        .Select(ch => ch.GetTimedEvents().Select(e => $"{e.Time}: {e.Event}").ToList())
                        .ToList();
                }
                var a = Events(bench); var g = Events(gig);
                var sb = new StringBuilder();
                sb.AppendLine($"Pistas: banco {a.Count}, gig {g.Count}.");
                int lines = 0;
                for (int i = 0; i < Math.Max(a.Count, g.Count) && lines < 8; i++)
                {
                    var ea = i < a.Count ? a[i] : new List<string>();
                    var eg = i < g.Count ? g[i] : new List<string>();
                    if (ea.SequenceEqual(eg)) continue;
                    int k = 0;
                    while (k < ea.Count && k < eg.Count && ea[k] == eg[k]) k++;
                    sb.AppendLine($"Pista {i}: {ea.Count} vs {eg.Count} eventos; primera diferencia en el {k}:");
                    sb.AppendLine($"  banco: {(k < ea.Count ? ea[k] : "(fin)")}");
                    sb.AppendLine($"  gig:   {(k < eg.Count ? eg[k] : "(fin)")}");
                    lines++;
                }
                if (lines == 0) sb.AppendLine("Los eventos coinciden; cambia solo la cabecera o el orden de las pistas.");
                return sb.ToString();
            }
            catch (Exception ex) { return $"No se pudo leer un archivo: {ex.Message}"; }
        }

        // ════════════════════════════════════════════════════════════════════
        // Decks
        // ════════════════════════════════════════════════════════════════════
        [Serializable] private class DeckJson { public string name; public string[] cards; }

        private void LoadDeck(Column c)
        {
            DeckJson parsed = null;
            try { parsed = JsonUtility.FromJson<DeckJson>(c.deckJson ?? ""); } catch { }
            if (parsed?.cards == null || parsed.cards.Length == 0)
            { EditorUtility.DisplayDialog("Banco de cartas", "El JSON no tiene cartas.", "OK"); return; }

            // Same lookup as GigManager.ApplyDevFixedDeckIfConfigured: Id first, then name.
            var all = AssetDatabase.FindAssets("t:CardDefinition")
                .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(d => d != null).ToList();
            var skipped = new List<string>();
            c.slots.Clear();
            foreach (var raw in parsed.cards)
            {
                var key = (raw ?? "").Trim();
                if (key.Length == 0) continue;
                var def = all.FirstOrDefault(d => d.Id == key)
                       ?? all.FirstOrDefault(d => string.Equals(d.DisplayName, key, StringComparison.OrdinalIgnoreCase));
                if (def == null) { skipped.Add(key); continue; }
                c.slots.Add(new Slot { cardGuid = GuidOf(def) });
            }
            SaveState();
            if (skipped.Count > 0)
                EditorUtility.DisplayDialog("Banco de cartas",
                    "No encontré: " + string.Join(", ", skipped), "OK");
        }

        // ── Card pools and labels ─────────────────────────────────────────
        private List<CardDefinition> _allCards = new();
        private readonly Dictionary<string, List<CardDefinition>> _poolCache = new();

        /// <summary>Cards this musician can play: fixed to its type, any-musician
        /// cards, and its catalog entries.</summary>
        private List<CardDefinition> PoolFor(MusicianCharacterData m)
        {
            if (_poolCache.TryGetValue(m.CharacterId, out var cached)) return cached;
            var catalog = new HashSet<CardDefinition>(
                m.CardCatalog?.Entries?.Where(e => e?.card != null).Select(e => e.card)
                ?? Enumerable.Empty<CardDefinition>());
            var pool = _allCards.Where(cd =>
                    catalog.Contains(cd)
                    || cd.CanBePlayedByAnyMusician
                    || (cd.RequiresFixedPerformer && cd.FixedPerformerType == m.CharacterType))
                .ToList();
            _poolCache[m.CharacterId] = pool;
            return pool;
        }

        private static string CardName(CardDefinition cd)
            => string.IsNullOrEmpty(cd.DisplayName) ? cd.name : cd.DisplayName;

        private static string RoleEs(TrackRole r) => r switch
        {
            TrackRole.Rhythm => "Ritmo",
            TrackRole.Bassline => "Bajo",
            TrackRole.Backing => "Acordes",
            TrackRole.Melody => "Melodía",
            TrackRole.Harmony => "Armonía",
            _ => r.ToString()
        };

        /// <summary>"[C·Bajo] Nombre  −1★ +2★". Finisher: no such flag in the
        /// card data yet (COMBOS-1); add the tag when it exists.</summary>
        private static string CardLabel(CardDefinition cd)
        {
            string tag;
            if (cd.IsComposition)
            {
                var cp = cd.CompositionPayload;
                tag = cp.PrimaryKind == CardPrimaryKind.Track && cp.TrackAction != null
                    ? "C·" + RoleEs(cp.TrackAction.role)
                    : cp.PrimaryKind == CardPrimaryKind.Part ? "C·Parte" : "C";
            }
            else tag = cd.IsAction ? "A" : "?";
            var sb = new StringBuilder($"[{tag}] {CardName(cd)}");
            if (cd.InspirationCost > 0) sb.Append($"  −{cd.InspirationCost}★");
            if (cd.InspirationGenerated > 0) sb.Append($"  +{cd.InspirationGenerated}★");
            return sb.ToString();
        }

        /// <summary>Popup path: submenu by kind ('/' in names would open submenus).</summary>
        private static string CardMenuPath(CardDefinition cd, bool foreign)
        {
            string group = cd.IsComposition ? "Composición" : cd.IsAction ? "Acción" : "Otras";
            string label = CardLabel(cd).Replace('/', '∕');
            return foreign ? $"(no es de este músico) {label}" : $"{group}/{label}";
        }

        /// <summary>One random Track composition card per lineup musician, with
        /// no role repeated, musician set explicitly.</summary>
        private void RandomizeCards(Column c, List<MusicianCharacterData> lineup)
        {
            var rng = new System.Random();
            var usedRoles = new HashSet<TrackRole>();
            c.slots.Clear();
            foreach (var m in lineup.OrderBy(_ => rng.Next()))
            {
                var options = PoolFor(m).Where(cd => cd.IsComposition
                        && cd.CompositionPayload.PrimaryKind == CardPrimaryKind.Track
                        && cd.CompositionPayload.TrackAction != null
                        && !usedRoles.Contains(cd.CompositionPayload.TrackAction.role))
                    .ToList();
                if (options.Count == 0) continue;
                var pick = options[rng.Next(options.Count)];
                usedRoles.Add(pick.CompositionPayload.TrackAction.role);
                c.slots.Add(new Slot { cardGuid = GuidOf(pick), musicianId = m.CharacterId });
            }
            // Play order: rhythm first, like a band counting in.
            c.slots = c.slots.OrderBy(s => (int)LoadCard(s.cardGuid).CompositionPayload.TrackAction.role).ToList();
            GUIUtility.ExitGUI();
        }

        private void LoadDeckFromDevSettings(Column c)
        {
            var guid = AssetDatabase.FindAssets("t:GigDevSettingsSO").FirstOrDefault();
            var dev = guid != null
                ? AssetDatabase.LoadAssetAtPath<GigDevSettingsSO>(AssetDatabase.GUIDToAssetPath(guid)) : null;
            if (dev == null) { EditorUtility.DisplayDialog("Banco de cartas", "No hay GigDevSettings.", "OK"); return; }
            c.deckJson = dev.FixedDeckJson;
            if (dev.UsePinnedSongSeed) c.seed = dev.PinnedSongSeed;
            LoadDeck(c);
        }

        // ════════════════════════════════════════════════════════════════════
        // Assets and state
        // ════════════════════════════════════════════════════════════════════
        private void ResolveAssets(bool fromPrefabs = false)
        {
            _musicians = AssetDatabase.FindAssets("t:MusicianCharacterData")
                .Select(g => AssetDatabase.LoadAssetAtPath<MusicianCharacterData>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(m => m != null && !string.IsNullOrEmpty(m.CharacterId))
                .GroupBy(m => m.CharacterId).Select(g => g.First())
                .OrderBy(m => m.CharacterName, StringComparer.Ordinal).ToList();

            _allCards = AssetDatabase.FindAssets("t:CardDefinition")
                .Select(g => AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(cd => cd != null)
                .OrderBy(cd => cd.IsComposition ? 0 : 1)
                .ThenBy(cd => CardName(cd), StringComparer.Ordinal).ToList();
            _poolCache.Clear();

            if (!fromPrefabs)
            {
                _config = Load<MidiGenPlayConfig>(_s.configGuid);
                _palette = Load<ChordProgressionPaletteSO>(_s.paletteGuid);
                _mix = Load<MixGainProfileSO>(_s.mixGuid);
                _glossary = Load<ConceptGlossarySO>(_s.glossaryGuid);
            }
            if (fromPrefabs || _config == null)
            {
                _config = PrefabField<MidiGenPlayConfig>(MusicManagerPrefab, typeof(MidiMusicManager), "settings");
                _palette = PrefabField<ChordProgressionPaletteSO>(MusicManagerPrefab, typeof(MidiMusicManager), "defaultProgressionPalette");
                _mix = PrefabField<MixGainProfileSO>(GigManagerPrefab, typeof(GigManager), "mixGainProfile");
                _glossary = PrefabField<ConceptGlossarySO>(TooltipManagerPrefab, typeof(TooltipManager), "conceptGlossary");
            }
            if (_s.a.lineup.Count == 0) _s.a.lineup = _musicians.Select(m => m.CharacterId).ToList();
            if (_s.b.lineup.Count == 0) _s.b.lineup = _musicians.Select(m => m.CharacterId).ToList();
            SaveState();
        }

        /// <summary>Reads a serialized field of a component on a prefab, so the bench
        /// follows whatever the game's Core prefabs are wired to.</summary>
        private static T PrefabField<T>(string prefabPath, Type owner, string field) where T : UnityEngine.Object
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (root == null) return null;
            foreach (var comp in root.GetComponentsInChildren(owner, true))
            {
                if (comp == null) continue;
                var prop = new SerializedObject(comp).FindProperty(field);
                if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
                    return prop.objectReferenceValue as T;
            }
            return null;
        }

        private void LoadState()
        {
            try { _s = JsonUtility.FromJson<State>(EditorPrefs.GetString(PrefsKey, "")); } catch { }
            _s ??= new State();
            _s.a ??= new Column { label = "A" };
            _s.b ??= new Column { label = "B" };
        }

        private void SaveState()
        {
            if (_s == null) return;
            _s.configGuid = GuidOf(_config);
            _s.paletteGuid = GuidOf(_palette);
            _s.mixGuid = GuidOf(_mix);
            _s.glossaryGuid = GuidOf(_glossary);
            EditorPrefs.SetString(PrefsKey, JsonUtility.ToJson(_s));
        }

        // [Claudio, 2026-11-17] The composer's own readback (MGP-ALWTTT-DBG-1), in words:
        // where the pattern / progression / melody of each track came from.
        private static string Why(TrackRole role, ResolvedTrackChoice c)
        {
            if (c == null) return "el motor no informó qué eligió";
            string a = c.sourceAssetName;
            string s = c.source switch
            {
                ResolvedSource.CardPalette => $"de la paleta «{c.paletteName}»: {a}",
                ResolvedSource.CardOverride => $"patrón fijo de la carta: {a}",
                ResolvedSource.TrackParameters => $"patrón de la pista (sin paleta en la carta): {a}",
                ResolvedSource.RenderOverride => $"patrón forzado para este render: {a}",
                ResolvedSource.SharedProgression => "progresión compartida de la parte",
                ResolvedSource.HostDefault => "progresión por defecto del juego",
                ResolvedSource.CardFigure => $"modo figura de la carta {a}",
                ResolvedSource.Procedural => role == TrackRole.Rhythm && !string.IsNullOrEmpty(c.proceduralStyleId)
                    ? $"procedural, estilo «{c.proceduralStyleId}» (la carta no tiene patrón ni paleta)"
                    : "procedural",
                _ => "nada resuelto",
            };
            // [PATRON-BPM-1] Rango cómodo del patrón y qué hizo el filtro de tempo.
            if (role == TrackRole.Rhythm)
            {
                if (c.patternBpmMin > 0 || c.patternBpmMax > 0)
                    s += $" · {BpmRange(c.patternBpmMin, c.patternBpmMax)} BPM";
                var tf = c.tempoFilter;
                if (tf.status == TempoFilterStatus.Filtered)
                    s += $" · encajan {tf.fitCount} de {tf.totalCount} a {tf.partBpm} BPM";
                else if (tf.status == TempoFilterStatus.FellBack)
                    s += $" · ⚠ nada encaja a {tf.partBpm} BPM: sorteo entre todos";
                else if (c.patternOutOfRange)
                    s += " · ⚠ fuera de su tempo";
            }
            if (!string.IsNullOrEmpty(c.progressionRoman)) s += $" · {c.progressionRoman}";
            if (role == TrackRole.Bassline) s += c.usesSharedProgression ? " · sobre la progresión compartida" : "";
            if (c.resolvedFigures != null && c.resolvedFigures.Count > 0)
                s += $" · figuras: {string.Join(", ", c.resolvedFigures.Distinct())}";
            if (c.melodyArchetypesBySpan != null && c.melodyArchetypesBySpan.Count > 0)
                s += $" · arquetipos: {string.Join(", ", c.melodyArchetypesBySpan.Select(x => x ?? "—").Distinct())}";
            return s;
        }

        private static string BpmRange(int min, int max) =>
            min > 0 && max > 0 ? $"{min}–{max}" : min > 0 ? $"≥ {min}" : max > 0 ? $"≤ {max}" : "sin rango";

        private static GUIStyle _mono;

        private static readonly Dictionary<Melanchall.DryWetMidi.Standards.GeneralMidiPercussion, string> DrumNames = new()
        {
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.AcousticBassDrum, "bombo" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.BassDrum1, "bombo 2" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.AcousticSnare, "caja" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.ElectricSnare, "caja 2" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.SideStick, "aro" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.HandClap, "palmas" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.ClosedHiHat, "hi-hat cerrado" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.PedalHiHat, "hi-hat de pie" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.OpenHiHat, "hi-hat abierto" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.CrashCymbal1, "crash" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.RideCymbal1, "ride" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.RideBell, "campana del ride" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.LowFloorTom, "floor tom grave" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.HighFloorTom, "floor tom" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.LowTom, "tom bajo" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.LowMidTom, "tom medio grave" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.HiMidTom, "tom medio" },
            { Melanchall.DryWetMidi.Standards.GeneralMidiPercussion.HighTom, "tom alto" },
        };

        // [Claudio, 2026-11-17] One line per drum of a rhythm stem: hits, velocity min / mean / max.
        // The key is named through the kit's own mapping (what the composer asked for).
        private static List<string> DrumLanes(byte[] stem, MIDIPercussionInstrumentSO kit)
        {
            var lines = new List<string>();
            try
            {
                using var ms = new MemoryStream(stem);
                var notes = MidiFile.Read(ms).GetNotes();
                var byKey = notes.GroupBy(n => (int)n.NoteNumber).OrderBy(g => g.Key);
                lines.Add($"{"instrumento",-18} {"tecla",5} {"golpes",6}  {"vel mín",7} {"media",5} {"máx",4}");
                foreach (var g in byKey)
                {
                    string name = $"tecla {g.Key}";
                    if (kit != null)
                        foreach (var m in kit.percussionMappings)
                            if (m.ToNote().NoteNumber == g.Key)
                            { name = DrumNames.TryGetValue(m.percussionType, out var es) ? es : m.percussionType.ToString(); break; }
                    var v = g.Select(n => (int)n.Velocity).ToList();
                    lines.Add($"{name,-18} {g.Key,5} {v.Count,6}  {v.Min(),7} {v.Average(),5:0} {v.Max(),4}");
                }
            }
            catch (Exception ex) { lines.Add("no pude leer el stem: " + ex.Message); }
            return lines;
        }

        private static void ClearColumn(Column c)
        {
            c.slots.Clear();
            c.deckJson = "";
            c.drumSolo = false;
            c.drumSoloShape = -1;
            c.drumSoloVariant = -1;
            c.drumSoloHalfTimeAboveBpm = 150;
        }

        private static Column Clone(Column c, string label)
        {
            var copy = JsonUtility.FromJson<Column>(JsonUtility.ToJson(c));
            copy.label = label;
            return copy;
        }

        private static string GuidOf(UnityEngine.Object o)
            => o == null ? "" : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));

        private static T Load<T>(string guid) where T : UnityEngine.Object
            => string.IsNullOrEmpty(guid) ? null
             : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));

        private static CardDefinition LoadCard(string guid) => Load<CardDefinition>(guid);

        private static string Fnv(byte[] data)
        {
            uint h = 2166136261;
            foreach (var b in data) { h ^= b; h *= 16777619; }
            return h.ToString("x8");
        }
    }
}
