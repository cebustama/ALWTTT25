// [MUSICA-1c, Claudio 2026-11-23] Ziggy. D10 = A (every melody card gets a voice palette,
// and between them they cover every V8 style), D11 = B (no melody palette with levels
// before the demo; it is the first thing after), D12 = C (new cards: Blues, Balada, Grito).
// Idempotent: run it again and it only fills what is missing. Cards and payloads are
// copies of Showtime's (CopyAsset: Unity gives new guids and .meta files).
using System.Collections.Generic;
using ALWTTT.Music.Voice;
using MidiGenPlay;
using MidiGenPlay.Composition;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Music.EditorTools
{
    public static class DemoZiggy1c
    {
        const string Musicians = "Assets/Resources/Data/Characters/Musicians/";
        const string Voices = Musicians + "Voice Profiles/";
        const string Configs = "Assets/Resources/Data/Cards/Composition/Melody Tracks/MelodyCardConfigs/";

        static VoiceProfileSO Voice(string name) =>
            AssetDatabase.LoadAssetAtPath<VoiceProfileSO>(Voices + "Estilos/VoiceProfile_" + name + ".asset");

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Ziggy: voces y cartas nuevas")]
        public static void Run()
        {
            var log = new List<string>();
            var catalog = AssetDatabase.LoadAssetAtPath<ALWTTT.Cards.MusicianCardCatalogData>(Musicians + "Cantante_CardCatalogData.asset");
            var showCfg = AssetDatabase.LoadAssetAtPath<MelodyCardConfigSO>(Configs + "Showtime/MelodyCardConfig_Showtime.asset");
            if (catalog == null || showCfg == null) { Debug.LogError("[MUSICA-1] Ziggy: falta el catálogo de Ziggy o la configuración de Showtime."); return; }

            // 0. Canto llano: the workshop mode that only uses existing knobs (motor-canto.js
            //    'Canto llano'), cloned from Tierno. Its Latin syllables stay in the workshop.
            var llano = Voice("Canto_llano");
            if (llano == null)
            {
                var tierno = Voice("Tierno_y_calmado");
                if (tierno == null) { Debug.LogError("[MUSICA-1] Ziggy: falta VoiceProfile_Tierno_y_calmado."); return; }
                llano = Object.Instantiate(tierno);
                llano.looseness = 0.03f; llano.vibratoDepth = 0f; llano.scoopSemitones = 0f; llano.fallSemitones = 0f;
                llano.glideMs = 40f; llano.legato = 1f; llano.tractLength = 48; llano.tensenessAtVel0 = 0.38f;
                llano.tensenessAtVel127 = 0.52f; llano.brightness = 0.38f; llano.breath = 1.2f; llano.attackPress = 0f;
                llano.minLoudness = 0.3f; llano.baseVowel = VoiceProfileSO.BaseVowel.A;
                AssetDatabase.CreateAsset(llano, Voices + "Estilos/VoiceProfile_Canto_llano.asset");
                log.Add("estilo Canto llano creado (de Tierno)");
            }

            // 1. Showtime: AA′ with a figure per song (Claudio's favourite) + the final climax
            Undo.RecordObject(showCfg, "Showtime AA′");
            showCfg.melodyMode = MelodyCardConfigSO.MelodyMode.Figure;
            showCfg.figureSettings.form = MelodyFigurePlanner.Form.QuestionAnswer;
            showCfg.figureSettings.randomFigure = true;
            showCfg.figureSettings.climax = true;
            showCfg.figureSettings.motifBars = 2;
            EditorUtility.SetDirty(showCfg);

            // 2. palettes for the cards that exist
            SetPalette("starter_showtime", "Paleta_Showtime", log, ("Pop", 1f), ("Crooner", 1f), ("Opera", 1f));
            SetPalette("starter_rise_up", "Paleta_RiseUp", log, ("Falsete", 1f), ("Zig_V8", 1f), ("Robot", 1f));

            // 3. the new cards (rewards, unlocked by default)
            NewCard(catalog, showCfg, "zig_blues", "Blues", "Blues", log, s =>
            {
                s.form = MelodyFigurePlanner.Form.AAB; s.randomFigure = true;
                s.density = 0.5; s.syncopation = 0.45; s.rests = 0.15; s.swing = 1;
            }, 23, ("Borracho", 1f), ("Grunido", 1f), ("Crooner", 1f));

            NewCard(catalog, showCfg, "zig_balada", "Balada", "Balada", log, s =>
            {
                s.form = MelodyFigurePlanner.Form.QuestionAnswer; s.randomFigure = false;
                s.figure = MelodyFigurePlanner.Figure.Wave; s.direction = MelodyFigurePlanner.Direction.Arch;
                s.density = 0.25; s.syncopation = 0.1; s.rests = 0.1; s.swing = 0; s.crescendo = 0.3; s.range = 1.2;
            }, 31, ("Tierno_y_calmado", 1f), ("Canto_llano", 1f));

            NewCard(catalog, showCfg, "zig_grito", "Grito", "Grito", log, s =>
            {
                s.form = MelodyFigurePlanner.Form.QuestionAnswer; s.randomFigure = false;
                s.figure = MelodyFigurePlanner.Figure.Leaps; s.direction = MelodyFigurePlanner.Direction.Up;
                s.density = 0.75; s.syncopation = 0.4; s.rests = 0.05; s.swing = 0; s.crescendo = 0.8;
            }, 41, ("Punk_rock", 1f), ("Death_metal", 1f));

            AssetDatabase.SaveAssets();
            Debug.Log("[MUSICA-1] Ziggy: Showtime en AA′ con figura al azar; " + string.Join("; ", log) +
                      ". En el Banco: melody card=zig_blues / zig_balada / zig_grito who=Ziggy, y «▶ con voz».");
        }

        // [MUSICA-1c, Claudio 2026-11-23] «casi todo muy muy bajo… una weighted list para las
        // octavas para que sea menos común cantar tan grave». Weights [down, as is, up].
        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Ziggy: octavas más agudas")]
        public static void HigherOctaves()
        {
            var w = new (string pal, Vector3 weights)[]
            {
                ("Paleta_Showtime", new Vector3(0, 2, 1)),
                ("Paleta_RiseUp", new Vector3(0, 1, 3)),
                ("Paleta_Blues", new Vector3(0, 1, 3)),
                ("Paleta_Balada", new Vector3(0, 1, 3)),
                ("Paleta_Grito", new Vector3(0, 1, 3)),
            };
            var log = new List<string>();
            foreach (var (name, weights) in w)
            {
                var pal = AssetDatabase.LoadAssetAtPath<VoiceStylePaletteSO>(Voices + "Paletas/" + name + ".asset");
                if (pal == null) { log.Add($"falta {name}"); continue; }
                Undo.RecordObject(pal, "Octavas");
                pal.octaveWeights = weights;
                EditorUtility.SetDirty(pal);
                log.Add($"{name} {weights.x}:{weights.y}:{weights.z}");
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[MUSICA-1] Ziggy: pesos de octava [abajo : igual : arriba]: " + string.Join(" · ", log) +
                      ". La línea [VOZ-3] muestra «+12» en el perfil cuando sube.");
        }

        // [MUSICA-1c D13 + D15] Blues with blue notes (and more space); every card sustains and
        // slides a little (the voice glides on one syllable). The chord fit (D14) needs no data.
        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Ziggy: blues, sostenidos y subidas")]
        public static void BluesHoldsSlides()
        {
            var set = new (string folder, double blue, double holds, double slides, double rests)[]
            {
                ("Blues", 0.5, 0.25, 0.4, 0.2),
                ("Balada", 0, 0.4, 0.35, -1),
                ("Showtime", 0, 0.2, 0.25, -1),
                ("Grito", 0, 0.15, 0.2, -1),
                ("RiseUp", 0, 0.2, 0.4, -1),
            };
            var log = new List<string>();
            foreach (var (folder, blue, holds, slides, rests) in set)
            {
                var cfg = AssetDatabase.LoadAssetAtPath<MelodyCardConfigSO>(Configs + folder + "/MelodyCardConfig_" + folder + ".asset");
                if (cfg == null) { log.Add($"falta {folder}"); continue; }
                Undo.RecordObject(cfg, "Ziggy sostenidos");
                cfg.figureSettings.blueNotes = blue;
                cfg.figureSettings.holds = holds;
                cfg.figureSettings.slides = slides;
                if (rests >= 0) cfg.figureSettings.rests = rests;
                EditorUtility.SetDirty(cfg);
                log.Add($"{folder} (blue {blue}, sostener {holds}, subir {slides})");
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[MUSICA-1] Ziggy: " + string.Join(" · ", log) + ". El ajuste a los acordes prestados va solo.");
        }

        // [MUSICA-1c, Claudio 2026-11-23] «siguen saliendo muchos graves», «Rise Up no sube lo
        // suficiente», «la voz robot no funciona con el personaje» y Borracho «para un status
        // effect». Tessitura per style (median sung note, MIDI), Rise Up climbs an octave and
        // ends on its peak, Robot and Borracho leave the card palettes (the assets stay).
        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Ziggy: registro, Rise Up sube, sin Robot ni Borracho")]
        public static void RegisterRiseAndPalettes()
        {
            var log = new List<string>();
            var tess = new (string voice, int min, int max)[]
            {
                ("Zig_V8", 57, 72), ("Pop", 57, 72), ("Crooner", 55, 70), ("Opera", 57, 74), ("Punk_rock", 57, 72),
                ("Falsete", 64, 79), ("Tierno_y_calmado", 57, 72), ("Canto_llano", 55, 70),
                ("Death_metal", 43, 57), ("Grunido", 43, 57), ("Borracho", 55, 70), ("Robot", 57, 72),
            };
            foreach (var (voice, min, max) in tess)
            {
                var v = Voice(voice);
                if (v == null) { log.Add($"falta {voice}"); continue; }
                Undo.RecordObject(v, "Tesitura");
                v.tessituraMin = min; v.tessituraMax = max;
                EditorUtility.SetDirty(v);
            }
            log.Add("tesituras puestas (mediana cantada: la mayoría 57–72, Falsete 64–79, Death metal y Gruñido 43–57)");

            var rise = AssetDatabase.LoadAssetAtPath<MelodyCardConfigSO>(Configs + "RiseUp/MelodyCardConfig_RiseUp.asset");
            if (rise != null)
            {
                Undo.RecordObject(rise, "Rise Up sube");
                rise.figureSettings.rise = 7;
                EditorUtility.SetDirty(rise);
                log.Add("Rise Up sube una octava frase a frase y acaba en su nota más alta");
            }

            void Drop(string palette, string voice)
            {
                var pal = AssetDatabase.LoadAssetAtPath<VoiceStylePaletteSO>(Voices + "Paletas/" + palette + ".asset");
                if (pal == null) return;
                Undo.RecordObject(pal, "Quitar estilo");
                if (pal.styles.RemoveAll(e => e.profile != null && e.profile.name == "VoiceProfile_" + voice) > 0)
                { EditorUtility.SetDirty(pal); log.Add($"{voice} fuera de {palette}"); }
            }
            Drop("Paleta_RiseUp", "Robot");
            Drop("Paleta_Blues", "Borracho");

            AssetDatabase.SaveAssets();
            Debug.Log("[MUSICA-1] Ziggy: " + string.Join(" · ", log) + ". Todo final de frase cae en una nota del acorde; " +
                      "el último, en la tónica si el acorde la tiene.");
        }

        // [MUSICA-1c D16 = A, Claudio 2026-11-23] Grito sings like the guitar's chug (Anselmo, Tool,
        // Patton): MGP's Riff figure (MGP-MEL-RIFF-1). Idempotent.
        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Ziggy: Grito chug (Riff)")]
        public static void GritoRiff()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<MelodyCardConfigSO>(Configs + "Grito/MelodyCardConfig_Grito.asset");
            if (cfg == null) { Debug.LogWarning("[MUSICA-1] Ziggy: no encuentro MelodyCardConfig_Grito (corre antes «voces y cartas nuevas»)."); return; }
            Undo.RecordObject(cfg, "Grito chug");
            var s = cfg.figureSettings;
            s.figure = MelodyFigurePlanner.Figure.Riff;
            s.randomFigure = false;
            s.form = MelodyFigurePlanner.Form.QuestionAnswer;
            s.holds = 0; s.slides = 0; s.blueNotes = 0; s.rise = 0; s.climax = false;
            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();
            Debug.Log($"[MUSICA-1] Ziggy: Grito pasa a Riff (chug): la fundamental del acorde repetida en corcheas (todo en la rejilla de corchea), " +
                      $"notas cortas con huecos, ♭2, ♭3 o ♭5 en los tiempos débiles al final de cada compás; " +
                      $"AA′ (pregunta en la 5.ª, respuesta en la fundamental), sin sostenidos, subidas ni clímax. Semilla {cfg.figureSeed}.");
        }

        static VoiceStylePaletteSO Palette(string name, List<string> log, params (string voice, float w)[] styles)
        {
            string path = Voices + "Paletas/" + name + ".asset";
            var pal = AssetDatabase.LoadAssetAtPath<VoiceStylePaletteSO>(path);
            if (pal != null) return pal;
            pal = ScriptableObject.CreateInstance<VoiceStylePaletteSO>();
            foreach (var (voice, w) in styles)
            {
                var v = Voice(voice);
                if (v == null) { Debug.LogWarning($"[MUSICA-1] Ziggy: falta VoiceProfile_{voice}; {name} sigue sin él."); continue; }
                pal.styles.Add(new VoiceStylePaletteSO.Entry { profile = v, weight = w });
            }
            AssetDatabase.CreateAsset(pal, path);
            var names = new List<string>(); foreach (var s in styles) names.Add(s.voice);
            log.Add($"{name} = {string.Join(" · ", names)}");
            return pal;
        }

        static void SetPalette(string cardId, string paletteName, List<string> log, params (string, float)[] styles)
        {
            var pal = Palette(paletteName, log, styles);
            var payload = AssetDatabase.LoadMainAssetAtPath(Musicians + cardId + "_Payload.asset");
            if (payload == null) { Debug.LogWarning($"[MUSICA-1] Ziggy: no encuentro {cardId}_Payload."); return; }
            var ps = new SerializedObject(payload);
            ps.FindProperty("trackAction.voicePalette").objectReferenceValue = pal;
            ps.ApplyModifiedPropertiesWithoutUndo();
        }

        static void NewCard(ALWTTT.Cards.MusicianCardCatalogData catalog, MelodyCardConfigSO from, string id, string display,
                            string folder, List<string> log, System.Action<MelodyFigurePlanner.Settings> tune, int seed,
                            params (string, float)[] styles)
        {
            string cardPath = Musicians + id + ".asset", payloadPath = Musicians + id + "_Payload.asset";
            if (AssetDatabase.LoadMainAssetAtPath(cardPath) != null) { log.Add($"{display} ya existía"); return; }

            // the melody config: Showtime's, with its own figure settings
            if (!AssetDatabase.IsValidFolder(Configs + folder)) AssetDatabase.CreateFolder(Configs.TrimEnd('/'), folder);
            string cfgPath = Configs + folder + "/MelodyCardConfig_" + folder + ".asset";
            var cfg = AssetDatabase.LoadAssetAtPath<MelodyCardConfigSO>(cfgPath);
            if (cfg == null)
            {
                cfg = Object.Instantiate(from);
                cfg.figureSettings = JsonUtility.FromJson<MelodyFigurePlanner.Settings>(JsonUtility.ToJson(from.figureSettings));
                cfg.figureSettings.climax = true; cfg.figureSettings.motifBars = 2;
                tune(cfg.figureSettings);
                cfg.melodyMode = MelodyCardConfigSO.MelodyMode.Figure;
                cfg.figureSeed = seed; cfg.figureVaryPerSong = true;
                AssetDatabase.CreateAsset(cfg, cfgPath);
            }

            var pal = Palette("Paleta_" + folder, log, styles);

            if (!AssetDatabase.CopyAsset(Musicians + "starter_showtime_Payload.asset", payloadPath))
            { Debug.LogError($"[MUSICA-1] Ziggy: no pude copiar el payload para {id}."); return; }
            var payload = AssetDatabase.LoadMainAssetAtPath(payloadPath);
            var ps = new SerializedObject(payload);
            ps.FindProperty("trackAction.styleBundle").objectReferenceValue = cfg;
            ps.FindProperty("trackAction.voicePalette").objectReferenceValue = pal;
            ps.ApplyModifiedPropertiesWithoutUndo();

            if (!AssetDatabase.CopyAsset(Musicians + "starter_showtime.asset", cardPath))
            { Debug.LogError($"[MUSICA-1] Ziggy: no pude copiar la carta para {id}."); return; }
            var card = AssetDatabase.LoadAssetAtPath<ALWTTT.Cards.CardDefinition>(cardPath);
            var cs = new SerializedObject(card);
            cs.FindProperty("id").stringValue = id;
            cs.FindProperty("displayName").stringValue = display;
            cs.FindProperty("payload").objectReferenceValue = payload;
            cs.ApplyModifiedPropertiesWithoutUndo();

            var cat = new SerializedObject(catalog);
            var entries = cat.FindProperty("entries");
            int i = entries.arraySize; entries.InsertArrayElementAtIndex(i);
            var e = entries.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("card").objectReferenceValue = card;
            e.FindPropertyRelative("flags").intValue = (int)(ALWTTT.Cards.CardAcquisitionFlags.RewardPool |
                                                             ALWTTT.Cards.CardAcquisitionFlags.UnlockedByDefault);
            e.FindPropertyRelative("unlockId").stringValue = "";
            e.FindPropertyRelative("starterCopies").intValue = 1;
            cat.ApplyModifiedPropertiesWithoutUndo();
            log.Add($"carta {display} ({id})");
        }
    }
}
