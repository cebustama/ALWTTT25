using ALWTTT.Tutorial;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.EditorTools
{
    /// <summary>
    /// [S10] Data for the minimal tutorial's newer dialogs (D11=A and the 25-nov round), registered
    /// in the ES catalog, plus the finisher text naming Redoble Final (drum solo). Texts are ✦ IA
    /// drafts for Claudio to review. Idempotent: running it again rewrites the same values.
    /// </summary>
    public static class S10_TutorialScriptTool
    {
        private const string Dir = "Assets/Resources/Data/Tutorial/Dialogs/ES/";
        private const string Template = Dir + "tut_your_turn.asset";
        private const string CatalogEs = "Assets/Resources/Data/Tutorial/TutorialDialogCatalog_ES.asset";

        private sealed class Entry
        {
            public string Id, Title, Highlight, Text;
            public int? Priority;   // null = keep the template's
            // Title / Highlight null = keep what the asset has.
        }

        private static readonly List<Entry> Entries = new List<Entry>
        {
            // ---- 26-nov: the drop goes anywhere (the card already knows its musician) ----
            new Entry
            {
                Id = TutorialTriggerId.PlayComposition, Title = null, Highlight = null,
                Text = "Arrastra una carta de composición hacia arriba y suéltala en cualquier parte del " +
                       "escenario. Cada carta ya sabe qué músico la toca: su pista se suma a la canción.",
            },
            new Entry
            {
                Id = TutorialTriggerId.AbilitiesIntro, Title = "Habilidades", Highlight = "",
                Text = "El público ataca: tus músicos acumulan <link=stress>Stress</link>. Las cartas de " +
                       "<link=action_card>habilidad</link> no suenan, pero cuidan a la banda: " +
                       "{$card:crd_sibi_calm_down} quita Stress; {$card:starter_static_rush} roba 2 cartas.",
            },
            new Entry
            {
                Id = TutorialTriggerId.OnYourOwn, Title = "Por tu cuenta", Highlight = "",
                Text = "Desde ahora vas por tu cuenta. En esta canción, <b>una composición por loop</b>: juega una, " +
                       "dale a Tocar y suma la siguiente en cada loop. Las habilidades puedes jugarlas cuando quieras.",
            },
            new Entry
            {
                Id = TutorialTriggerId.StatusIntro, Title = "Estados", Highlight = "",
                Text = "Última canción: los estados. {$card:crd_c2_spotlight} pone a Robot C2 bajo el " +
                       "<link=spotlight>Foco</link>: recibe los golpes del público. {$card:wink} deja " +
                       "<link=captivated>Cautivado</link> a un oyente. Úsalo en Rayas: es <link=tall>Alto</link> y, " +
                       "mientras no caiga, la rana de atrás no recibe <link=vibe>Vibe</link>. Cuando caiga, sí. " +
                       "{$card:starter_rise_up} también cautiva al Alto.",
            },
            // ---- 25-nov ----
            new Entry
            {
                Id = TutorialTriggerId.LoopMore, Title = "Una más por loop", Highlight = "", Priority = 65,
                Text = "¡Ya suena! Mientras suena puedes sumar otra composición: acaba de llegar " +
                       "{$card:starter_wormus_major}. Juégala ahora y entrará en el próximo loop. Así, loop a " +
                       "loop, la canción crece.",
            },
            new Entry
            {
                Id = TutorialTriggerId.PauseHint, Title = "Pausa", Highlight = "", Priority = 66,
                Text = "¿Te falta tiempo para leer las cartas? Pulsa <b>Espacio</b> cuando quieras: la música y el " +
                       "público se detienen y puedes mirar tu mano con calma. <b>Espacio</b> otra vez y todo sigue.",
            },
            new Entry
            {
                Id = TutorialTriggerId.LayersIntro, Title = "Una por loop", Highlight = "", Priority = 66,
                Text = "¿Por qué una por loop? Cada loop en que entra un músico nuevo suma " +
                       "<link=combo_layers>Capas</link> (×1,5 el segundo, ×2,25 el tercero…), que multiplican todo " +
                       "lo que suena. Las cartas con borde celeste traen a un músico que aún no toca; las rojas " +
                       "esperan al próximo loop. El juego está en pausa: pulsa <b>Espacio</b> cuando quieras para " +
                       "pararlo y mirar tus cartas.",
            },
            new Entry
            {
                Id = TutorialTriggerId.TracksIntro, Title = "Pistas y combos", Highlight = "song_panel_tracks", Priority = 67,
                // ✦ IA [S10, 26-nov] combos por pareja completada (regla nueva) y colores de las filas.
                Text = "Arriba a la izquierda, tu canción: una fila por músico. Ahora entran varias por loop. " +
                       "Completa una pareja sin meter a nadie más en ese loop y hay combo: " +
                       "<link=combo_rhythm_section>Sección rítmica</link> o <link=combo_media_luz>A media luz</link>; " +
                       "con los cuatro sonando, <link=combo_full_band>¡Toda la banda!</link> Fila celeste: entra; " +
                       "naranja: hace combo. <b>C</b>: la tabla.",
            },
            // ---- 26-nov: song 3, two reactive lessons ----
            new Entry
            {
                Id = TutorialTriggerId.Spotlight, Title = "Foco", Highlight = null,
                Text = "Robot C2 está bajo el <link=spotlight>Foco</link>: las líneas discontinuas del público " +
                       "ahora apuntan a él (<b>Tab</b> las muestra todas). Mira su barra de " +
                       "<link=stamina>Aguante</link>: este turno se lleva los golpes y protege al resto.",
            },
            new Entry
            {
                Id = TutorialTriggerId.TrackReplaced, Title = null, Highlight = null,
                Text = "Esa carta cambió la pista de su músico: cada músico toca una pista, y otra carta suya la " +
                       "reemplaza (otros acordes, otro ritmo, otra melodía). Así la canción cambia sin que nadie " +
                       "deje de tocar.",
            },
        };

        public const string FinisherText =
            "Último loop: hora del remate. Juega {$card:c2_redoble_final}: gasta ★ y añade un loop extra con " +
            "el solo de batería de Robot C2. El cierre lo es todo.";

        [MenuItem("ALWTTT/S10 ▸ Guion de cartas del tutorial (diálogos nuevos)")]
        public static void Run()
        {
            var log = new List<string>();
            var catalog = AssetDatabase.LoadAssetAtPath<TutorialDialogCatalogSO>(CatalogEs);
            if (catalog == null) { Done("FALTA el catálogo " + CatalogEs); return; }
            var cso = new SerializedObject(catalog);
            var list = cso.FindProperty("dialogs");
            int created = 0, updated = 0, listed = 0;

            foreach (var e in Entries)
            {
                string path = Dir + e.Id + ".asset";
                var d = AssetDatabase.LoadAssetAtPath<TutorialDialogSO>(path);
                if (d == null)
                {
                    if (!AssetDatabase.CopyAsset(Template, path)) { log.Add("no pude crear " + path); continue; }
                    d = AssetDatabase.LoadAssetAtPath<TutorialDialogSO>(path);
                    log.Add("creado " + e.Id);
                    created++;
                }
                else updated++;

                var so = new SerializedObject(d);
                so.FindProperty("triggerId").stringValue = e.Id;
                if (e.Highlight != null) so.FindProperty("highlightKey").stringValue = e.Highlight;
                var rt = so.FindProperty("revisitTitle");
                if (rt != null && e.Title != null) rt.stringValue = e.Title;
                if (e.Priority.HasValue) so.FindProperty("priority").intValue = e.Priority.Value;
                SetText(so, e.Text);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(d);

                bool inList = false;
                for (int i = 0; i < list.arraySize; i++)
                    if (list.GetArrayElementAtIndex(i).objectReferenceValue == d) { inList = true; break; }
                if (!inList)
                {
                    list.arraySize++;
                    list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = d;
                    log.Add("al catálogo: " + e.Id);
                    listed++;
                }
            }
            cso.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            log.Insert(0, $"diálogos: {created} creados, {updated} actualizados, {listed} añadidos al catálogo");

            var fin = AssetDatabase.LoadAssetAtPath<TutorialDialogSO>(Dir + "tut_play_finisher.asset");
            if (fin != null)
            {
                var so = new SerializedObject(fin);
                SetText(so, FinisherText);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(fin);
                log.Add("remate: Redoble Final");
            }
            else log.Add("FALTA tut_play_finisher");

            log.Add("íconos actualizados en " + S10_InspirationIconTool.SwapAllDialogs() + " diálogo(s)");
            AssetDatabase.SaveAssets();
            Done(string.Join(" | ", log));
        }

        private static void SetText(SerializedObject so, string text)
        {
            text = text.Replace("★", S10_InspirationIconTool.Tag);
            so.FindProperty("mechanicText").stringValue = text;
            var pages = so.FindProperty("pages");
            pages.arraySize = 1;
            pages.GetArrayElementAtIndex(0).stringValue = text;
        }

        private static void Done(string msg)
        {
            Debug.Log("[S10] Guion de cartas del tutorial | " + msg);
            EditorUtility.DisplayDialog("S10", msg.Replace(" | ", "\n"), "OK");
        }
    }
}
