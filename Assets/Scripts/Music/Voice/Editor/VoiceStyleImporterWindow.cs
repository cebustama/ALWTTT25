// [VOZ-3] ALWTTT ▸ Voz ▸ Importar estilo (TV1 / TM1 / JSON): a voice style made in the
// Taller de melodías becomes a VoiceProfileSO (engine V8) + its SyllableBankSO.
// Flow (D-VOZ-5 = A): paste → read → preview (fields, bank, sample lexicon, warnings) → apply.
// Destination (D2 = C): a new style by default; «sobrescribir» an existing one keeps its GUID,
// so the palettes and prefabs that use it (listed under «usado por») hear the change.
// Undo: overwriting goes through Undo (Ctrl+Z); a new import has «Deshacer la importación»,
// which deletes the assets it created. Gain (D3 = B): TV1 brings it calibrated; otherwise 1.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ALWTTT.Music.Voice.V8;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Music.Voice.EditorTools
{
    public class VoiceStyleImporterWindow : EditorWindow
    {
        const string Root = "Assets/Resources/Data/Characters/Musicians/Voice Profiles";
        const string StylesDir = Root + "/Estilos";
        const string BanksDir = Root + "/Bancos";
        const string Tag = "[VOZ-3]";

        string _code = "";
        VoiceStyleSpec _spec;
        string _error;
        string _name = "";
        bool _zigLanguage;
        bool _overwrite;
        VoiceProfileSO _target;
        VoiceProfileSO _usersOf;
        List<string> _users = new List<string>();
        readonly List<string> _lastCreated = new List<string>();
        VoiceProfileSO _lastImported;
        Vector2 _scroll;

        [MenuItem("ALWTTT/Voz/Importar estilo (TV1, TM1, JSON)")]
        public static void Open() => GetWindow<VoiceStyleImporterWindow>("Importar estilo de voz");

        void OnGUI()
        {
            // The long base64 code is one unbroken word: without wrapping and a max width it
            // widens the whole layout and pushes the buttons off the right edge.
            _scroll = EditorGUILayout.BeginScrollView(_scroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
            float w = Mathf.Max(200f, position.width - 24f);
            EditorGUIUtility.labelWidth = Mathf.Min(240f, w * 0.45f);
            var col = new EditorGUILayout.VerticalScope(GUILayout.MaxWidth(w));
            EditorGUILayout.HelpBox(
                "Pega el código del Taller de melodías: «Copiar TV1» (recomendado: trae el gain calibrado), " +
                "un código TM1 o el JSON de «Copiar para la carta».", MessageType.None);
            var wrapArea = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            _code = EditorGUILayout.TextArea(_code, wrapArea, GUILayout.MinHeight(54), GUILayout.MaxWidth(w));
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Leer", GUILayout.Height(24))) Read();
                if (GUILayout.Button("Pegar del portapapeles y leer", GUILayout.Height(24)))
                { _code = EditorGUIUtility.systemCopyBuffer ?? ""; GUI.FocusControl(null); Read(); }
            }
            if (_error != null) EditorGUILayout.HelpBox(_error, MessageType.Error);
            if (_spec != null) DrawPreview();
            DrawLast();
            col.Dispose();
            EditorGUILayout.EndScrollView();

        }

        void Read()
        {
            _spec = null; _error = null;
            try
            {
                _spec = VoiceStyleCode.Parse(_code, VoiceStylesMenu.TryModeBase);
                _name = _spec.Name;
                _zigLanguage = false;
                Debug.Log($"{Tag} Importador: leído {_spec.Source} «{_spec.Name}» (modo {_spec.Mode}), " +
                          $"sílabas {_spec.SyllableMode}, gain {(_spec.HasGain ? _spec.Gain.ToString("0.####") : "1 (no venía)")}, " +
                          $"avisos {_spec.Warnings.Count}.");
            }
            catch (System.Exception e) { _error = e.Message; Debug.LogWarning($"{Tag} Importador: no se pudo leer: {e.Message}"); }
        }

        void DrawPreview()
        {
            var s = _spec;
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Vista previa", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{s.Source} · modo del taller: {(string.IsNullOrEmpty(s.Mode) ? "—" : s.Mode)}");
            foreach (var w in s.Warnings) EditorGUILayout.HelpBox(w, MessageType.Warning);

            _name = EditorGUILayout.TextField("Nombre", _name);
            EditorGUILayout.LabelField("Gain", s.HasGain ? $"{s.Gain:0.####} (calibrado en el taller)" : "1 (el código no lo trae)");

            EditorGUILayout.LabelField("Voz (nombres del taller → perfil)", EditorStyles.miniBoldLabel);
            var p = s.P;
            Row("tracto / aire / growl", $"{p.TractLength} · {p.Breath:0.##} · {p.Growl:0.##}");
            Row("aspereza / saturación", $"{p.Roughness:0.##} · {p.Drive:0.##}");
            Row("tensión (vel 0 → 127)", $"{p.TensLo:0.##} → {p.TensHi:0.##}");
            Row("vibrato (prof., Hz, retraso, rampa)", $"{p.VibratoDepth:0.##} · {p.VibratoSpeedHz:0.#} · {p.VibratoDelay:0.##} s · {p.VibratoRamp:0.##} s");
            Row("scoop / caída / glide", $"{p.Scoop:0.##} st en {p.ScoopMs:0} ms · {p.Fall:0.##} st · {p.GlideMs:0} ms");
            Row("ataque / legato / vocal base", $"{p.Attack:0.##} · {p.Legato:0.##} · {p.BaseVowel}");
            Row("soltura / brillo / transporte", $"{p.Looseness:0.##} · {p.Brightness:0.##} · {p.Transpose:+0;-0;0} st");
            Row("volumen mínimo / adelanto", $"{p.MinLoudness:0.##} · {p.PitchLead:0.###} s");
            if (s.Raw.Count > 0)
                EditorGUILayout.LabelField("Del código: " + string.Join(", ", s.Raw.Select(kv => $"{kv.Key} {kv.Value}")), EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.LabelField("Sílabas", EditorStyles.miniBoldLabel);
            switch (s.SyllableMode)
            {
                case "zigues":
                    _zigLanguage = EditorGUILayout.ToggleLeft(
                        new GUIContent($"Hablar el idioma de Zig (lexiconSeed {VoiceStyleCode.ZigLexiconSeed}) en vez del del taller ({s.LexiconSeed})",
                            "El taller usa la semilla de su melodía como idioma. Zig, en el juego, habla el 4242."), _zigLanguage);
                    int seed = LexSeed();
                    Row("zigués", $"lexiconSeed {seed}, dureza {s.Hardness:0.##}");
                    var lex = SyllablePlanner.Zigues(seed, s.Hardness).Select(w => string.Concat(w));
                    EditorGUILayout.LabelField("Palabras: " + string.Join(" · ", lex), EditorStyles.wordWrappedMiniLabel);
                    break;
                case "lista":
                    Row("lista", string.Join("  ", s.List.Select(w => $"{w.Syllable}×{w.Weight:0.#}{(w.IsEnd ? " (fin)" : "")}")));
                    break;
                default:
                    Row("texto", s.Text);
                    break;
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Destino", EditorStyles.miniBoldLabel);
            int mode = GUILayout.Toolbar(_overwrite ? 1 : 0, new[] { "Estilo nuevo", "Sobrescribir uno existente" });
            _overwrite = mode == 1;
            if (_overwrite)
            {
                _target = (VoiceProfileSO)EditorGUILayout.ObjectField("Perfil", _target, typeof(VoiceProfileSO), false);
                if (_target != _usersOf) { _usersOf = _target; _users = _target != null ? FindUsers(_target) : new List<string>(); }
                if (_target != null)
                {
                    EditorGUILayout.HelpBox(_users.Count == 0 ? "Nadie lo usa todavía."
                        : "Usado por (oirán el cambio):\n  " + string.Join("\n  ", _users), MessageType.Info);
                    EditorGUILayout.LabelField("Se cambian la voz, el gain y el motor (V8). Dicción, boca y startTrimMs se quedan.", EditorStyles.wordWrappedMiniLabel);
                }
            }
            else
                EditorGUILayout.LabelField($"Se crean {StylesDir}/VoiceProfile_{Slug(_name)}.asset y su banco en {BanksDir}.", EditorStyles.wordWrappedMiniLabel);

            using (new EditorGUI.DisabledScope(_overwrite && _target == null))
                if (GUILayout.Button(_overwrite ? "Aplicar (sobrescribir)" : "Aplicar (crear)", GUILayout.Height(28))) Apply();
        }

        void DrawLast()
        {
            if (_lastImported == null) return;
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField($"Último importado: {_lastImported.name}", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Seleccionar")) { Selection.activeObject = _lastImported; EditorGUIUtility.PingObject(_lastImported); }
                if (GUILayout.Button(new GUIContent("Oír en el Banco de cartas", "Abre el Banco con este perfil como voz de «▶ con voz».")))
                    ALWTTT.Music.EditorTools.CardBenchWindow.OpenWithVoice(_lastImported);
                using (new EditorGUI.DisabledScope(_lastCreated.Count == 0))
                    if (GUILayout.Button(new GUIContent("Deshacer la importación", "Borra los assets que creó (un estilo nuevo). Sobrescribir se deshace con Ctrl+Z.")))
                        UndoCreated();
            }
        }

        static void Row(string label, string value) => EditorGUILayout.LabelField(label, value, EditorStyles.wordWrappedLabel);

        int LexSeed() => _zigLanguage ? VoiceStyleCode.ZigLexiconSeed : _spec.LexiconSeed;

        void Apply()
        {
            var s = _spec;
            string name = string.IsNullOrWhiteSpace(_name) ? "Estilo importado" : _name.Trim();
            string slug = Slug(name);
            EnsureDir(StylesDir); EnsureDir(BanksDir);
            _lastCreated.Clear();
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();

            VoiceProfileSO profile;
            SyllableBankSO bank;
            if (_overwrite)
            {
                profile = _target;
                bank = profile.syllableBank;
                string bankPath = bank != null ? AssetDatabase.GetAssetPath(bank) : null;
                bool bankShared = bank != null && FindUsers(bank).Any(u => u != AssetDatabase.GetAssetPath(profile));
                if (bank == null || bankShared || string.IsNullOrEmpty(bankPath))
                {
                    bank = CreateBank(slug);
                }
                else Undo.RecordObject(bank, "Importar estilo de voz");
                Undo.RecordObject(profile, "Importar estilo de voz");
            }
            else
            {
                bank = CreateBank(slug);
                profile = CreateInstance<VoiceProfileSO>();
                string path = AssetDatabase.GenerateUniqueAssetPath($"{StylesDir}/VoiceProfile_{slug}.asset");
                AssetDatabase.CreateAsset(profile, path);
                _lastCreated.Add(path);
            }

            FillBank(bank, s);
            FillProfile(profile, s);
            profile.syllableBank = bank;
            EditorUtility.SetDirty(bank); EditorUtility.SetDirty(profile);
            Undo.CollapseUndoOperations(group);
            AssetDatabase.SaveAssets();

            _lastImported = profile;
            if (_overwrite) _lastCreated.Clear();   // overwrite is undone with Ctrl+Z

            Selection.activeObject = profile; EditorGUIUtility.PingObject(profile);
            string users = _overwrite ? (_users.Count == 0 ? "nadie" : string.Join(", ", _users)) : "-";
            Debug.Log($"{Tag} Importado {s.Source} «{name}» → {AssetDatabase.GetAssetPath(profile)} " +
                      $"({(_overwrite ? "sobrescrito" : "nuevo")}), banco {AssetDatabase.GetAssetPath(bank)} ({bank.mode}" +
                      $"{(bank.mode == SyllableBankSO.Mode.Zigues ? $", lexiconSeed={bank.lexiconSeed}, hardness={bank.hardness:0.##}" : "")}), " +
                      $"gain {profile.gain:0.####}, tracto {profile.tractLength}, growl {profile.growl:0.##}. Usado por: {users}.");
        }

        SyllableBankSO CreateBank(string slug)
        {
            var b = CreateInstance<SyllableBankSO>();
            string path = AssetDatabase.GenerateUniqueAssetPath($"{BanksDir}/Banco_{slug}.asset");
            AssetDatabase.CreateAsset(b, path);
            _lastCreated.Add(path);
            return b;
        }

        void FillBank(SyllableBankSO b, VoiceStyleSpec s)
        {
            b.entries.Clear();
            switch (s.SyllableMode)
            {
                case "lista":
                    b.mode = SyllableBankSO.Mode.WeightedList;
                    foreach (var w in s.List)
                        b.entries.Add(new SyllableBankSO.Entry { syllable = w.Syllable, weight = (float)w.Weight, isEnd = w.IsEnd });
                    break;
                case "texto":
                    b.mode = SyllableBankSO.Mode.Text; b.text = s.Text;
                    break;
                default:
                    b.mode = SyllableBankSO.Mode.Zigues;
                    break;
            }
            b.lexiconSeed = LexSeed();
            b.hardness = s.Hardness;
        }

        static void FillProfile(VoiceProfileSO v, VoiceStyleSpec s)
        {
            var p = s.P;
            v.engine = VoiceProfileSO.SingerEngine.V8_Workshop;
            v.looseness = p.Looseness; v.vibratoDepth = p.VibratoDepth; v.vibratoSpeedHz = p.VibratoSpeedHz;
            v.vibratoDelaySeconds = p.VibratoDelay; v.vibratoRampSeconds = p.VibratoRamp; v.brightness = p.Brightness;
            v.transposeSemitones = p.Transpose; v.tensenessAtVel0 = p.TensLo; v.tensenessAtVel127 = p.TensHi;
            v.minLoudness = p.MinLoudness; v.pitchLeadSeconds = p.PitchLead; v.tractLength = p.TractLength;
            v.breath = p.Breath; v.growl = p.Growl; v.roughness = p.Roughness; v.drive = p.Drive;
            v.scoopSemitones = p.Scoop; v.scoopMs = p.ScoopMs; v.fallSemitones = p.Fall; v.glideMs = p.GlideMs;
            v.attackPress = p.Attack; v.legato = p.Legato;
            v.baseVowel = p.BaseVowel switch
            {
                'a' => VoiceProfileSO.BaseVowel.A, 'e' => VoiceProfileSO.BaseVowel.E, 'i' => VoiceProfileSO.BaseVowel.I,
                'o' => VoiceProfileSO.BaseVowel.O, 'u' => VoiceProfileSO.BaseVowel.U, _ => VoiceProfileSO.BaseVowel.Neutral,
            };
            v.gain = s.HasGain ? s.Gain : 1f;
        }

        void UndoCreated()
        {
            foreach (var path in _lastCreated) AssetDatabase.DeleteAsset(path);
            Debug.Log($"{Tag} Importación deshecha: borrados {string.Join(", ", _lastCreated)}.");
            _lastCreated.Clear(); _lastImported = null;
        }

        /// <summary>Assets (prefabs and ScriptableObjects) that reference <paramref name="asset"/>, plus the
        /// cards that use a palette that references it (one more level, since cards point to palettes).</summary>
        internal static List<string> FindUsers(Object asset)
        {
            string target = AssetDatabase.GetAssetPath(asset);
            var result = new List<string>();
            if (string.IsNullOrEmpty(target)) return result;
            var all = AssetDatabase.FindAssets("t:Prefab t:ScriptableObject", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => p != target).Distinct().ToArray();
            var direct = new List<string>();
            try
            {
                for (int i = 0; i < all.Length; i++)
                {
                    if (i % 200 == 0) EditorUtility.DisplayProgressBar("Usado por", all[i], (float)i / all.Length);
                    if (AssetDatabase.GetDependencies(all[i], false).Contains(target)) direct.Add(all[i]);
                }
                foreach (var d in direct)
                {
                    result.Add(d);
                    if (AssetDatabase.LoadAssetAtPath<VoiceStylePaletteSO>(d) == null) continue;
                    foreach (var c in all)
                        if (c != d && AssetDatabase.GetDependencies(c, false).Contains(d)) result.Add($"   ↳ {c} (vía {Path.GetFileNameWithoutExtension(d)})");
                }
            }
            finally { EditorUtility.ClearProgressBar(); }
            return result;
        }

        static string Slug(string name)
        {
            var s = (name ?? "").Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (char c in s)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            }
            var r = sb.ToString();
            while (r.Contains("__")) r = r.Replace("__", "_");
            r = r.Trim('_');
            return r.Length == 0 ? "Estilo" : r;
        }

        static void EnsureDir(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
