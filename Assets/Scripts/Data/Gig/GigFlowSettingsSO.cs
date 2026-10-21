using ALWTTT.Interfaces;
using UnityEngine;

namespace ALWTTT.Data
{
    /// <summary>
    /// Gameplay-flow rules and setup-screen defaults consumed by GigManager
    /// (runtime sequencing) and GigSetupController (setup-screen defaults).
    /// One of four SOs produced by M4.6F-2 from the former monolithic GigManager
    /// inspector / GigSetupConfigData "Default Values" header.
    ///
    /// Authority: SSoT_Gig_Combat_Core (runtime) + SSoT_Gig_Encounter (defaults
    /// consumption by setup screen). PersistentGameplayData.ApplyRunConfig reads
    /// the Default* properties as fallbacks when RunConfig overrides are absent.
    /// </summary>
    [CreateAssetMenu(
        fileName = "GigFlowSettings",
        menuName = "ALWTTT/Gig/GigFlowSettings",
        order = 10)]
    public sealed class GigFlowSettingsSO : ScriptableObject
    {
        // --- Composition / Jam ---

        [Header("Composition")]
        [SerializeField] private JamRules jamRules = new JamRules();
        public JamRules JamRules => jamRules;

        [SerializeField, Tooltip("Cards drawn after each loop completion. " +
            "0 = disabled. Hand-cap clamp is applied by DeckManager.DrawCards.")]
        private int drawPerLoop = 0;
        public int DrawPerLoop => drawPerLoop;

        [SerializeField, Tooltip("Cards drawn when Play is pressed (start of " +
            "the jam, before the first loop renders). 0 = disabled. " +
            "Independent of DrawPerLoop � set equal if you want them to match. " +
            "Hand-cap clamp is applied by DeckManager.DrawCards.")]
        private int drawCardsOnPlay = 0;
        public int DrawCardsOnPlay => drawCardsOnPlay;

        // --- Action card gating ---

        [Header("Action Card Gating (MVP)")]
        [SerializeField, Tooltip("MVP: when Play is pressed, discard remaining " +
            "Action cards from the hand.")]
        private bool discardActionCardsOnPlay = true;
        public bool DiscardActionCardsOnPlay => discardActionCardsOnPlay;

        [SerializeField, Tooltip("Allow Action cards with timing=Always to be " +
            "playable during performance.")]
        private bool allowActionCardsDuringPerformance = false;
        public bool AllowActionCardsDuringPerformance => allowActionCardsDuringPerformance;

        // --- Gig end behavior ---

        [Header("Gig End Behavior")]
        [SerializeField] private bool skipAudienceActionsAfterFinalSong = true;
        public bool SkipAudienceActionsAfterFinalSong => skipAudienceActionsAfterFinalSong;

        // --- Setup-screen defaults (formerly on GigSetupConfigData) ---

        [Header("Setup Defaults � Inspiration")]
        [SerializeField] private int defaultInitialGigInspiration = 0;
        [SerializeField] private int defaultInspirationPerLoop = 3;   // [S5e / D2] was 0

        public int DefaultInitialGigInspiration => defaultInitialGigInspiration;
        public int DefaultInspirationPerLoop => defaultInspirationPerLoop;

        /// <summary>
        /// Legacy alias preserved for callers written before F-2: matches
        /// GigSetupConfigData.DefaultStartingInspiration (which itself
        /// aliased DefaultInitialGigInspiration). Equivalent to
        /// <see cref="DefaultInitialGigInspiration"/>.
        /// </summary>
        public int DefaultStartingInspiration => defaultInitialGigInspiration;

        // ─── [ECON-1] Per-turn play economy defaults ─────────────────────
        [Header("Setup Defaults — Per-Turn Play Economy (ECON-1)")]
        [SerializeField, Min(0), Tooltip("Action-card plays each musician gets " +
            "per PERIOD (pre-song action window, and each performance loop). " +
            "D-ECON-4=A: strict 1 in all periods. 0 disables Action plays entirely.")]
        private int defaultActionPlaysPerTurn = 1;

        [SerializeField, Min(0), Tooltip("Composition-card plays each musician " +
            "gets per PERIOD. D-ECON-4=A: strict 1 in all periods. " +
            "0 disables Composition plays entirely.")]
        private int defaultCompositionPlaysPerTurn = 1;

        public int DefaultActionPlaysPerTurn => Mathf.Max(0, defaultActionPlaysPerTurn);
        public int DefaultCompositionPlaysPerTurn => Mathf.Max(0, defaultCompositionPlaysPerTurn);

        // ─── [R5-b / D-R5-12=A] Voltage generation switch ────────────────
        [Header("Gig Rules — Voltage (R5)")]
        [SerializeField, Tooltip("When ON, every GENUINELY consumed play by " +
            "Conito (action or composition, any Inspiration cost including 0) " +
            "grants +1 Voltage. When OFF, Voltage is only applied by cards " +
            "that carry an explicit ApplyStatusEffectSpec for it. Read per " +
            "play, so toggling during Play mode takes effect on the next play.")]
        private bool generateVoltageOnConsumedPlay = true;

        public bool GenerateVoltageOnConsumedPlay => generateVoltageOnConsumedPlay;

        // ─── [R5-c / D-R5-13..17] Overload — consumidor de Voltage ───────
        [SerializeField, Tooltip("When ON, a loop boundary where the Voltage " +
            "bearer holds >= OverloadThreshold stacks discharges Overload: it " +
            "spends OverloadCost stacks and multiplies THAT loop's SongHype " +
            "contribution by OverloadHypeFactor. Read once per loop boundary, " +
            "so toggling during Play takes effect on the next finished loop.")]
        // [R5-d / D-R5-20=B] DEFAULT FLIPPED to false: the player-facing
        // Overload is the Action card. This automatic discharge survives as a
        // tuning/dev fallback only. NOTE: flipping this default does NOT change
        // an already-serialized asset — the checkbox on the live
        // GigFlowSettingsSO must be unticked by hand. Review at R8: if still
        // OFF, retire the consumer (D-R5-20 option C).
        private bool overloadConsumerEnabled = false;


        [SerializeField, Min(1), Tooltip("Voltage stacks required to discharge " +
            "Overload. Tuning rule (D-R5-10 rider): if Overload never fires in " +
            "short songs, LOWER this — never raise Voltage generation.")]
        private int overloadThreshold = 6;

        [SerializeField, Min(1), Tooltip("Voltage stacks spent per discharge. " +
            "Surplus above the cost survives and keeps accumulating. Values " +
            "above the threshold are harmless: the spend clamps to the stacks " +
            "actually held.")]
        private int overloadCost = 6;

        [SerializeField, Tooltip("Multiplier applied to the loop's hype delta " +
            "on discharge. Applied in the expression, AFTER ComputeHypeDelta " +
            "and after MeterTuningSO.SongHypeDeltaMultiplier — the asset is " +
            "never mutated. Clamped to >= 1: Overload cannot make a loop worse.")]
        private float overloadHypeFactor = 1.5f;

        public bool OverloadConsumerEnabled => overloadConsumerEnabled;
        public int OverloadThreshold => Mathf.Max(1, overloadThreshold);
        public int OverloadCost => Mathf.Max(1, overloadCost);
        public float OverloadHypeFactor => Mathf.Max(1f, overloadHypeFactor);

        // ─── [R5-d] Bonus loop (Overload card) ───────────────────────────
        [Header("Gig Rules — Bonus Loop (R5-d)")]
        [SerializeField, Min(0), Tooltip("Maximum bonus loops a single part may " +
            "receive (D-R5-21 rider). 1 stops a banked resource from chaining " +
            "several extra loops into one part. 0 disables bonus loops entirely.")]
        private int maxBonusLoopsPerPart = 1;

        [SerializeField, Tooltip("When on, a bonus loop is rendered with the " +
            "soloist's extra track layered over the unchanged base. When off, " +
            "the bonus loop is a plain repeat. Read at grant time.")]
        private bool bonusSoloEnabled = true;

        [SerializeField, Range(0f, 1f), Tooltip("Live volume applied to every " +
            "non-solo channel for the duration of the solo loop. 1 = no duck. " +
            "Composes multiplicatively with the mix; it does not overwrite it.")]
        private float bonusSoloDuck01 = 0.55f;

        public int MaxBonusLoopsPerPart => Mathf.Max(0, maxBonusLoopsPerPart);
        public bool BonusSoloEnabled => bonusSoloEnabled;
        public float BonusSoloDuck01 => Mathf.Clamp01(bonusSoloDuck01);
        // ─── [SONG-TAIL-1] La banda sigue sonando durante el turno del público ───
        // D-ST6=A: el conmutador A/B vive aquí (precedente: bonusSoloDuck01). Dev Mode
        // puede sobrescribirlo por sesión (Audio Mix) sin escribir este asset.
        [Header("Song Tail (SONG-TAIL-1)")]
        [SerializeField, Tooltip("ON: al terminar una canción, el último loop sigue sonando " +
            "(fuera de la CompositionSession: sin LoopScore, hype ni impresiones) hasta el " +
            "siguiente punto de parada. OFF: comportamiento previo, silencio + crowd bed. " +
            "Se lee al terminar cada canción.")]
        private bool songTailEnabled = true;

        [SerializeField, Range(0f, 1f), Tooltip("Plano de atenuación del tail (D-ST3=B), " +
            "multiplicativo dentro de MidiMusicManager.WriteChannelVolume01. 1 = mismo " +
            "nivel que la canción. Se mezcla contra el crowd bed y, en CROWD-SFX-1, " +
            "contra aplausos/abucheos.")]
        private float songTailLevel01 = 0.7f;

        [SerializeField, Min(0f), Tooltip("Fundido de salida del tail, en segundos de " +
            "tiempo escalado (una pausa lo congela). Se usa en victoria y derrota " +
            "(D-ST5=C). 0 = corte seco. La canción siguiente no funde: entra en la barra (D-ST7=C).")]
        private float songTailFadeSeconds = 0.75f;

        [SerializeField, Range(0f, 120f), Tooltip("D-ST7=C: al pulsar Play con el tail " +
            "sonando, la parte nueva se retiene hasta la siguiente barra del tail. Este " +
            "adelanto (ms) dispara el cambio un poco antes de la barra para absorber la " +
            "latencia de arranque de MPTK. 0 = en la barra exacta (llega tarde un frame + " +
            "latencia, como una costura de loop normal).")]
        private float songTailHandoffLeadMs = 30f;

        public bool SongTailEnabled => songTailEnabled;
        public float SongTailLevel01 => Mathf.Clamp01(songTailLevel01);
        public float SongTailFadeSeconds => Mathf.Max(0f, songTailFadeSeconds);
        public float SongTailHandoffLeadMs => Mathf.Clamp(songTailHandoffLeadMs, 0f, 120f);
        // ─── [PLAY-TIMING-0] Sonda de timing del Play: SOLO LOG, sin efecto de juego ───
        // Mide cuánto le faltó (o le sobró) al jugador respecto a la barra que cierra el
        // loop en curso (D-PT1=A), a ambos lados (D-PT2=B), en ms absolutos (D-PT3=A),
        // desde el pointer-down del botón Play (D-PT4=B).
        [Header("Play Timing Probe (PLAY-TIMING-0)")]
        [SerializeField, Tooltip("ON: cada Play escribe una línea [PLAY-TIMING] con la " +
            "distancia a la barra más cercana del loop que suena y su grado. Sin efecto de juego.")]
        private bool playTimingProbeEnabled = true;

        [SerializeField, Range(0.02f, 1f), Tooltip("Ventana a cada lado de la barra (s). " +
            "Es también el límite exterior de GOOD; fuera de ella: FUERA DE VENTANA.")]
        private float playTimingWindowSeconds = 0.25f;

        [SerializeField, Min(0f), Tooltip("|error| <= esto (ms) → PERFECT. Por debajo de " +
            "~1 frame (16 ms a 60 fps) el grado es ruido.")]
        private float playTimingPerfectMs = 30f;

        [SerializeField, Min(0f), Tooltip("|error| <= esto (ms) → EXCELLENT.")]
        private float playTimingExcellentMs = 60f;

        [SerializeField, Range(-200f, 200f), Tooltip("Calibración (ms). Positivo = el " +
            "altavoz suena después del tick del secuenciador (latencia de salida); se resta " +
            "al error medido. Calibrar mirando la media de los errores brutos en el log.")]
        private float playTimingOffsetMs = 0f;

        public bool PlayTimingProbeEnabled => playTimingProbeEnabled;
        public float PlayTimingWindowSeconds => Mathf.Clamp(playTimingWindowSeconds, 0.02f, 1f);
        public float PlayTimingExcellentMs =>
            Mathf.Clamp(playTimingExcellentMs, 0f, PlayTimingWindowSeconds * 1000f);
        public float PlayTimingPerfectMs => Mathf.Clamp(playTimingPerfectMs, 0f, PlayTimingExcellentMs);
        public float PlayTimingOffsetMs => Mathf.Clamp(playTimingOffsetMs, -200f, 200f);
        // ─── [CROWD-SFX-1] Veredicto del público al final de cada canción ───
        // D-CS1=B: el público juzga cada canción una vez, por la impresión media de la
        // canción (media, entre miembros, de la media de loops de cada miembro; [-2, 2]).
        // Solo presentación: no toca Vibe, hype ni economía.
        [Header("Crowd Verdict (CROWD-SFX-1)")]
        [SerializeField, Range(0f, 2f), Tooltip("Media >= esto → aplauso (CrowdApplause). " +
            "Subirlo a 2 apaga el aplauso salvo canciones perfectas.")]
        private float crowdApplauseAtMean = 0.5f;

        [SerializeField, Range(-2f, 0f), Tooltip("Media <= esto → abucheo (CrowdBoo). " +
            "Entre los dos umbrales no suena nada: el crowd bed que vuelve lo cubre.")]
        private float crowdBooAtMean = -0.5f;

        [SerializeField, Range(0f, 2f), Tooltip("D-CS4=A: espera (s, tiempo ESCALADO: la pausa " +
            "la congela) entre el fin de la canción y el veredicto. Debe superar el jitter de " +
            "las reacciones del último loop (AudioManager.sfxMaxJitterSeconds, 0.15) y quedar " +
            "bien dentro de GigPresentationSO.SongEndPause.")]
        private float crowdVerdictDelaySeconds = 0.35f;

        public float CrowdApplauseAtMean => Mathf.Clamp(crowdApplauseAtMean, 0f, 2f);
        public float CrowdBooAtMean => Mathf.Clamp(crowdBooAtMean, -2f, 0f);
        public float CrowdVerdictDelaySeconds => Mathf.Clamp(crowdVerdictDelaySeconds, 0f, 2f);


        [Header("Setup Defaults � Hand / Inspiration Policies")]
        [SerializeField] private bool defaultDiscardHandBetweenTurns = false;
        [SerializeField] private bool defaultKeepInspirationBetweenTurns = false;

        public bool DefaultDiscardHandBetweenTurns => defaultDiscardHandBetweenTurns;
        public bool DefaultKeepInspirationBetweenTurns => defaultKeepInspirationBetweenTurns;

        [Header("Setup Defaults � Required Songs")]
        [SerializeField] private bool allowOverrideRequiredSongCount = true;
        [SerializeField, Min(1)] private int defaultRequiredSongCount = 1;

        public bool AllowOverrideRequiredSongCount => allowOverrideRequiredSongCount;
        public int DefaultRequiredSongCount => Mathf.Max(1, defaultRequiredSongCount);
    }
}