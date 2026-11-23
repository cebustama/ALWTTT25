// [MUSICA-1c, Claudio 2026-11-23] «Showtime suena absolutamente random: no sigue una
// estructura de frases ni juega en torno a un patrón o leitmotif.» Showtime draws
// procedural phrase archetypes (Anthem BurstHold / SustainHigh / FlowDense) with no
// motif. Test: figure mode with Sequence (a short motif restated on other degrees),
// the signature motif on and 4-bar phrases in an 8-bar period, varying per song.
// A/B in the Card Bench: render A, run «probar motivo», render B. «volver» undoes it.
using MidiGenPlay;
using MidiGenPlay.Composition;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Music.EditorTools
{
    public static class DemoShowtimeMotif1c
    {
        const string Dir = "Assets/Resources/Data/Cards/Composition/Melody Tracks/MelodyCardConfigs/";

        static MelodyCardConfigSO Load(string rel) => AssetDatabase.LoadAssetAtPath<MelodyCardConfigSO>(Dir + rel);

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Showtime con motivo (Sequence)")]
        public static void TryMotif()
        {
            var show = Load("Showtime/MelodyCardConfig_Showtime.asset");
            var rise = Load("RiseUp/MelodyCardConfig_RiseUp.asset");
            if (show == null || rise == null) { Debug.LogError("[MUSICA-1] Showtime: no encuentro las configuraciones."); return; }
            Undo.RecordObject(show, "Showtime motivo");
            show.figureSettings = JsonUtility.FromJson<MelodyFigurePlanner.Settings>(JsonUtility.ToJson(rise.figureSettings));
            show.figureSettings.figure = MelodyFigurePlanner.Figure.Sequence;
            show.figureSettings.direction = MelodyFigurePlanner.Direction.Arch;
            show.figureSettings.signature = true;
            show.figureSettings.phraseBars = 4;
            show.figureSettings.period = 8;
            show.figureSeed = 11;
            show.figureVaryPerSong = true;
            show.melodyMode = MelodyCardConfigSO.MelodyMode.Figure;
            EditorUtility.SetDirty(show); AssetDatabase.SaveAssets();
            Debug.Log("[MUSICA-1] Showtime: modo figura Sequence (arco, motivo de firma, frases de 4 en periodo de 8, varía por canción). Renderiza B.");
        }

        // [MUSICA-1c, D9 = A] MGP-MEL-FORM-1 over the same Sequence figure: AA′ (the motif
        // twice, only the last two notes change: up into the 5th, then down into the tonic)
        // or AAB (the motif twice, then a free answer that closes on the tonic).
        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Showtime pregunta y respuesta (AA′)")]
        public static void QuestionAnswer() => SetForm(MelodyFigurePlanner.Form.QuestionAnswer, "pregunta y respuesta (AA′)");

        // Claudio: «que la final sea más dramática» (climax, on in every form menu) and
        // «agregar las figuras como random» (figure and direction from the song seed).
        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Showtime AA′ con figura al azar")]
        public static void QuestionAnswerRandom() => SetForm(MelodyFigurePlanner.Form.QuestionAnswer, "pregunta y respuesta (AA′), figura al azar", true);

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Showtime AAB (blues)")]
        public static void Aab() => SetForm(MelodyFigurePlanner.Form.AAB, "AAB");

        static void SetForm(MelodyFigurePlanner.Form form, string label, bool randomFigure = false)
        {
            TryMotif();
            var show = Load("Showtime/MelodyCardConfig_Showtime.asset");
            if (show == null) return;
            Undo.RecordObject(show, "Showtime forma");
            show.figureSettings.form = form;
            show.figureSettings.motifBars = 2;
            show.figureSettings.climax = true;
            show.figureSettings.randomFigure = randomFigure;
            EditorUtility.SetDirty(show); AssetDatabase.SaveAssets();
            Debug.Log($"[MUSICA-1] Showtime: forma {label}, motivo de 2 compases" +
                      (randomFigure ? " (figura y dirección por semilla)" : " sobre la figura Sequence") + ", final culminante. Renderiza B.");
        }

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Showtime con las frases de antes")]
        public static void Revert()
        {
            var show = Load("Showtime/MelodyCardConfig_Showtime.asset");
            if (show == null) return;
            Undo.RecordObject(show, "Showtime frases");
            show.melodyMode = MelodyCardConfigSO.MelodyMode.Procedural;
            EditorUtility.SetDirty(show); AssetDatabase.SaveAssets();
            Debug.Log("[MUSICA-1] Showtime: vuelve al modo procedural (paleta de frases Anthem).");
        }
    }
}
