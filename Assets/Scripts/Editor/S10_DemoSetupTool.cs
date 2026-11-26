using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Map;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [S10] Data for DEMO-1211 S10, in one click (idempotent: running it again rewrites the
    /// same values):
    ///  1. Cohesion 3/3 (D6=A).
    ///  2. «LaunchConfig - Tutorial» (copy of Ranas y Rayas: full quartet) and its wiring
    ///     on the Main Menu (D1a=A).
    ///  3. The minimal tutorial texts, ES catalog (D1b=A). Draft ✦ IA, to be reviewed.
    ///  4. Gig 1 of «1b - Linear Map» becomes a random node: Ranas y Rayas / Tentáculos,
    ///     weight 1 / 1 (D5=A).
    ///  5. GigDevSettings: fixed deck and pinned song seed OFF (to play the lineups for real).
    /// The menu buttons and the Options panel are arranged at runtime (MainMenuController,
    /// OptionsPanelController), not here.
    /// </summary>
    public static class S10_DemoSetupTool
    {
        private const string GameplaySettings = "Assets/Resources/Data/Settings/Gameplay Settings.asset";
        private const string RanasConfig = "Assets/Settings/Gig/LaunchConfig - Ranas y Rayas.asset";
        private const string TentaculosConfig = "Assets/Settings/Gig/LaunchConfig - Tentaculos.asset";
        private const string TutorialConfig = "Assets/Settings/Gig/LaunchConfig - Tutorial.asset";
        private const string DevSettings = "Assets/Settings/Gig/GigDevSettings.asset";
        private const string MenuScene = "Assets/Scenes/0 - Main Menu.unity";
        private const string MapScene = "Assets/Scenes/1b - Linear Map.unity";
        private const string DialogsEs = "Assets/Resources/Data/Tutorial/Dialogs/ES/";

        // ✦ IA draft (D1b=A). Claudio reviews them in «Textos pendientes».
        private static readonly Dictionary<string, string> Texts = new Dictionary<string, string>
        {
            ["tut_jam_welcome"] =
                "Bienvenido al escenario, novato. Tu objetivo: convencer a todo el público antes de que " +
                "se acaben las canciones. Cada uno tiene su <link=vibe>Vibe</link>: déjalo en cero y es tuyo.",
            ["tut_your_turn"] =
                "Tu turno. Las cartas de <link=composition_card>composición</link> construyen la canción; " +
                "las de <link=action_card>acción</link> cuidan a la banda y empujan al público. Cada músico " +
                "juega una de cada por <link=loop>loop</link>.",
            ["tut_play_composition"] =
                "Arrastra una carta de composición a la banda. Cae sobre el músico que la toca y suma su " +
                "pista a la canción.",
            ["tut_press_play"] =
                "Dale al Play. La canción suena en bucle: cada vuelta es un <link=loop>loop</link> y un turno " +
                "tuyo. Lo que juegues desde ahora entra en vivo.",
            ["tut_inspiration_economy"] =
                "Cada composición que juegas carga <link=inspiration>Inspiración</link> (★). Guárdala: los " +
                "remates la gastan en el último loop.",
            ["tut_play_finisher"] = S10_TutorialScriptTool.FinisherText,   // [S10 / D11=A] drum solo
            ["tut_play_finisher_early"] =
                "Te adelantaste con el remate, pero sonó. La próxima vez guárdalo: brilla en el último loop.",
            ["tut_song_end_vibe"] =
                "Fin de la canción: el <link=hype>hype</link> que construiste se convierte en daño de " +
                "<link=vibe>Vibe</link>. A cero, ese oyente queda <link=convinced>convencido</link>.",
            ["tut_audience_turn"] =
                "Ahora actúa el público: sube el <link=stress>Stress</link> de tus músicos. Si llega al máximo, " +
                "el músico colapsa y la banda pierde <link=cohesion>Cohesión</link>. Sin Cohesión, se acabó la gira.",
            ["tut_gig_won"] =
                "Todos convencidos. Así se gana un concierto. Ahora, a la gira.",
            ["tut_gig_lost"] =
                "Se acabaron las canciones y no cayeron todos. Guarda ★ para los remates y vuelve a intentarlo.",
        };

        [MenuItem("ALWTTT/S10 ▸ Preparar datos de la demo (Cohesión, tutorial, nodo aleatorio)")]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var log = new List<string>();

            // 1. Cohesion 3/3 (D6=A)
            var gs = AssetDatabase.LoadAssetAtPath<ScriptableObject>(GameplaySettings);
            if (gs != null)
            {
                var so = new SerializedObject(gs);
                so.FindProperty("maxCohesion").intValue = 3;
                so.FindProperty("initialCohesion").intValue = 3;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gs);
                log.Add("Cohesión 3/3");
            }
            else log.Add("FALTA Gameplay Settings");

            // 2. Tutorial launch config (copy of Ranas y Rayas)
            if (AssetDatabase.LoadAssetAtPath<DemoLaunchConfigSO>(TutorialConfig) == null)
                AssetDatabase.CopyAsset(RanasConfig, TutorialConfig);
            var tutorialCfg = AssetDatabase.LoadAssetAtPath<DemoLaunchConfigSO>(TutorialConfig);
            log.Add(tutorialCfg != null ? "LaunchConfig - Tutorial listo" : "FALTA LaunchConfig - Tutorial");

            // 3. Tutorial texts (ES): the minimal run shows mechanicText (showMechanicText = on);
            // pages get the same text so the revisit panel and the fallback agree.
            int texts = 0;
            foreach (var kv in Texts)
            {
                var d = AssetDatabase.LoadAssetAtPath<ScriptableObject>(DialogsEs + kv.Key + ".asset");
                if (d == null) { log.Add("FALTA diálogo " + kv.Key); continue; }
                var so = new SerializedObject(d);
                string text = kv.Value.Replace("★", S10_InspirationIconTool.Tag); // [S10] real icon
                so.FindProperty("mechanicText").stringValue = text;
                var pages = so.FindProperty("pages");
                pages.arraySize = 1;
                pages.GetArrayElementAtIndex(0).stringValue = text;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(d);
                texts++;
            }
            log.Add($"textos del tutorial: {texts}/{Texts.Count}");

            // 5. Dev settings off
            var dev = AssetDatabase.LoadAssetAtPath<GigDevSettingsSO>(DevSettings);
            if (dev != null)
            {
                var so = new SerializedObject(dev);
                so.FindProperty("useFixedDeck").boolValue = false;
                so.FindProperty("usePinnedSongSeed").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(dev);
                log.Add("GigDevSettings: mazo fijo y semilla de canción apagados");
            }
            AssetDatabase.SaveAssets();

            // 2b. Wire the config on the Main Menu
            var menuScene = EditorSceneManager.OpenScene(MenuScene, OpenSceneMode.Single);
            var menu = FindInScene<MainMenuController>(menuScene);
            if (menu != null && tutorialCfg != null)
            {
                var so = new SerializedObject(menu);
                so.FindProperty("tutorialLaunchConfig").objectReferenceValue = tutorialCfg;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
                EditorSceneManager.SaveScene(menu.gameObject.scene);
                log.Add("menú: config del tutorial asignada");
            }
            else log.Add(menu == null ? "FALTA MainMenuController en " + MenuScene
                                      : "FALTA el asset LaunchConfig - Tutorial al asignarlo");

            // 4. Random node on the map
            var mapScene = EditorSceneManager.OpenScene(MapScene, OpenSceneMode.Single);
            var map = FindInScene<LinearMapController>(mapScene);
            var ranas = AssetDatabase.LoadAssetAtPath<DemoLaunchConfigSO>(RanasConfig);
            var tent = AssetDatabase.LoadAssetAtPath<DemoLaunchConfigSO>(TentaculosConfig);
            if (map != null && ranas != null && tent != null)
            {
                var so = new SerializedObject(map);
                var node = so.FindProperty("nodes").GetArrayElementAtIndex(0);
                var opts = node.FindPropertyRelative("randomOptions");
                opts.arraySize = 2;
                SetOption(opts.GetArrayElementAtIndex(0), ranas, 1f, "Ranas y Rayas");
                SetOption(opts.GetArrayElementAtIndex(1), tent, 1f, "Tentáculos");
                node.FindPropertyRelative("randomTitle").stringValue = "?";
                node.FindPropertyRelative("randomDescription").stringValue =
                    "Nadie sabe quién vendrá esta noche.";
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
                EditorSceneManager.SaveScene(map.gameObject.scene);
                log.Add("mapa: gig 1 aleatorio (Ranas y Rayas 1 · Tentáculos 1)");
            }
            else log.Add("FALTA LinearMapController o algún LaunchConfig");

            Debug.Log("[S10] Preparar datos | " + string.Join(" | ", log));
            EditorUtility.DisplayDialog("S10", string.Join("\n", log), "OK");
        }

        // Walks the opened scene itself (inactive objects included).
        private static T FindInScene<T>(UnityEngine.SceneManagement.Scene scene) where T : Component
        {
            foreach (var go in scene.GetRootGameObjects())
            {
                var c = go.GetComponentInChildren<T>(true);
                if (c != null) return c;
            }
            Debug.LogWarning($"[S10] No encuentro {typeof(T).Name} en '{scene.path}' " +
                             $"(cargada={scene.isLoaded}, raíces={scene.rootCount}).");
            return null;
        }

        private static void SetOption(SerializedProperty p, DemoLaunchConfigSO cfg, float weight, string title)
        {
            p.FindPropertyRelative("launchConfig").objectReferenceValue = cfg;
            p.FindPropertyRelative("weight").floatValue = weight;
            p.FindPropertyRelative("title").stringValue = title;
            p.FindPropertyRelative("description").stringValue = string.Empty;
        }
    }
}
