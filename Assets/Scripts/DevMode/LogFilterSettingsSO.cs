using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.DevLogs
{
    /// <summary>
    /// [S10] Which Console logs are shown, by category. LogFilter reads it from
    /// Resources/Data/Settings/LogFilterSettings (menu ALWTTT ▸ Logs ▸ Crear/abrir filtro).
    /// Without the asset, the same defaults (Defaults()) apply.
    /// Only Debug.Log is filtered (and warnings when a category says so); errors and
    /// exceptions always pass.
    /// </summary>
    [CreateAssetMenu(fileName = "LogFilterSettings", menuName = "ALWTTT/Dev/Log Filter Settings")]
    public class LogFilterSettingsSO : ScriptableObject
    {
        public const string ResourcePath = "Data/Settings/LogFilterSettings";

        [Serializable]
        public class Category
        {
            public string name;
            [Tooltip("Mostrar estos logs en la Console.")]
            public bool show;
            [Tooltip("Ocultar también sus warnings (los errores siempre se ven).")]
            public bool includeWarnings;
            [Tooltip("Texto que identifica el log (sin etiquetas <color>). Basta con que aparezca uno.")]
            public string[] patterns;
            [Tooltip("De dónde sale (archivo).")]
            public string source;
            [Tooltip("Interruptor propio que ya existía, o «hardcoded» si no tiene.")]
            public string ownToggle;

            public Category(string name, bool show, string source, string ownToggle, params string[] patterns)
            { this.name = name; this.show = show; this.source = source; this.ownToggle = ownToggle; this.patterns = patterns; }
        }

        [Tooltip("Apagado: no se filtra nada.")]
        public bool filterEnabled = true;

        [Tooltip("Un log que contenga cualquiera de estos textos se ve siempre (la etiqueta de la sesión).")]
        public string[] alwaysShow = { "[S10]" };

        [Tooltip("Modo foco: si tiene algo, SOLO se ven los logs que contengan uno de estos textos " +
                 "(más warnings y errores). Vacío = se usan las categorías.")]
        public string[] focusOnly = new string[0];

        [Tooltip("Al salir del Play, una línea con cuántos logs se ocultaron por categoría.")]
        public bool summaryOnExit = true;

        public List<Category> categories = Defaults();

        public static List<Category> Defaults() => new List<Category>
        {
            // ── Visibles: arranque de la partida y lo que falta de contenido ──
            new Category("Partida: lanzamiento y mazo", true,
                "PersistentGameplayData, GigRunContext, GigLauncher", "hardcoded",
                "[PersistentGameplayData]", "[GigRunContext]", "Launching gig", "Stored RunConfig"),
            new Category("Textos sin entrada en el glosario (warning)", true,
                "UI/Tooltips/GameText.cs", "hardcoded (avisa una vez por id)", "[GameText]"),

            // ── Ocultos: ruido del gig ──
            new Category("Vistas de intención y tooltips [S4b]", false,
                "IntentLinesView, AudienceCharacterCanvas, TooltipManager", "hardcoded", "[S4b]"),
            new Category("Pulso de animación [S8]", false,
                "Characters/CharacterAnimator.cs", "hardcoded", "[S8] Beat overlay"),
            new Category("Mano: sube/baja, curva, cartas añadidas", false,
                "Controllers/HandController.cs", "VerboseLogs (HandController) para [M1.9]; [S9] Mano hardcoded",
                "[S9] Mano", "[M1.9]", "HandController:"),
            new Category("Construcción de cartas", false,
                "GameManager.BuildAndGetCard, CardBase", "hardcoded", "Building Card", "[CARTAS-ARTE-1]"),
            new Category("Barra de Vibe fantasma [VibeGhost]", false,
                "UI/HealthBarController.cs", "logPredicted (HealthBarController)", "[VibeGhost]"),
            new Category("Resaltados del tutorial (registro)", false,
                "Tutorial/TutorialHighlightTarget.cs", "hardcoded", "[TutorialHighlightRegistry]"),
            new Category("Tutorial interno (detalle)", false,
                "TutorialController, TutorialGuidedDriver, TutorialOverlayView", "verboseLogging (cada uno)",
                "[Tutorial]", "[TutGuided]", "[TutorialOverlay]"),
            new Category("Estados y stats de personajes", false,
                "CharacterStats, AudienceCharacterBase", "hardcoded",
                "Triggering All Status", "{AudienceCharacterBase}"),
            new Category("Partículas rítmicas y escuchas MIDI", false,
                "RhythmParticleEmitter/MidiListener, MidiListenerCanvasBase",
                "logSetup / logEveryHit / debugRegister", "[RhythmFx]", "[MLBase]"),
            new Category("Psychic Waves (diagnóstico)", false,
                "Sensory/PsychicWaveOverlayController.cs", "logDiagnostics", "[PsychicWave]"),
            new Category("Escena: local, colocación, formación", false,
                "BackgroundRoot, GigManager, BandLayout", "hardcoded", "[S9] Local", "[S9] Colocación", "[S7] Band layout"),

            // ── Ocultos: managers y sistemas ──
            new Category("GigManager", false,
                "Managers/GigManager.cs", "GigDevSettings: useLogs / logVerbose", "GigManager:"),
            new Category("DeckManager y GameManager", false,
                "DeckManager, GameManager", "VerboseLogs (DeckManager); GameManager hardcoded",
                "DeckManager:", "GameManager:", "Initializing PersistentGameplayData"),
            new Category("Economía y ajustes de dev", false,
                "InspirationRules, DevTuning, DevModeController, DevRunTelemetryLogger, COMP-LOG-1",
                "hardcoded (DevMode: _verboseLogs)",
                "[ECON-1]", "[ECON-2]", "[DEV-TUNING-1]", "[DevMode]", "[DevRunTelemetryLogger]", "[COMP-LOG-1]"),
            new Category("Música: MidiGenPlay, voz, director", false,
                "MidiMusicManager, MidiGenerator, SingerVoice*, PinkTrombone, MusicDirector",
                "logDebug / logVerbose (MidiMusicManager, SingerVoiceDirector); resto hardcoded",
                "[MidiMusicManager]", "[MidiGenerator]", "[SingerDirector]", "[PinkTromboneSinger]",
                "[MusicDirector]", "Init MidiSynth", "[Session]", "[GigContext]"),
            new Category("Bus sensorial", false,
                "SensoryEventBus, SensoryAudioAdapter, SensoryFxAdapter",
                "logInitialization / logVerification", "[SensoryEventBus]", "[SensoryAudioAdapter]", "[SensoryFxAdapter]")
                { includeWarnings = true },
            new Category("MidiGenPlay: composición de cada loop (con warnings)", false,
                "SongConfigBuilder, SongOrchestrator, composers, TonalityAudit, PitchBendWriter (paquete MidiGenPlay); " +
                "MUSICA-1, VOZ-2, AUDIO-LOOP-1, LoopCtx (ALWTTT)",
                "hardcoded en su mayoría; MidiMusicManager logDebug",
                "[SongConfigBuilder]", "[SongOrchestrator]", "[ChordTrackComposer]", "[ChordTrack]",
                "[RhythmTrackComposer]", "[PaletteSelector]", "[MelodySlot]", "[MelodyTrackComposer]",
                "[Factory/Melody]", "[BassTrackComposer]", "[BASS-WALK", "[TonalityAudit]", "[PitchBendWriter]",
                "[B1/Pending]", "[BATERIA-SOLO]", "[MUSICA-1]", "[VOZ-2]", "[AUDIO-LOOP-1]", "[LoopCtx]")
                { includeWarnings = true },
            new Category("Voz: Ziggy no canta este loop (warning)", false,
                "Music/Voice/SingerVoiceDirector.cs", "hardcoded", "Sitting out this loop") { includeWarnings = true },
            new Category("Efecto por loop, combos y timing del Play", false,
                "CompositionSession/GigManager (ResolveLoopEffect), COMBOS-1, PlayPressTimingProbe", "hardcoded",
                "[ResolveLoopEffect]", "[COMBOS-1]", "[PLAY-TIMING]"),
            new Category("Público: rutina de cada ataque", false,
                "AudienceCharacterBase, AddStressAction, S7 objetivo", "hardcoded",
                "Ability Routine started", "action AddStress delay", "action MoveToFront delay", "Finished delay",
                "[Add Stress] Target", "[Add Stress] Stats", "[S7] Weakest"),
            new Category("SFX elegidos", false,
                "Managers/AudioManager.cs", "logSfx (AudioManager)", "[AudioManager]"),
            new Category("Maestro MPTK sin MIDI en el menú (warning)", false,
                "MidiPlayerTK.MidiFilePlayer (paquete externo)", "hardcoded",
                "No MIDI found in MIDI DB", "MIDI Load error") { includeWarnings = true },
        };

        [ContextMenu("Restaurar categorías por defecto")]
        private void ResetCategories() => categories = Defaults();
    }
}
