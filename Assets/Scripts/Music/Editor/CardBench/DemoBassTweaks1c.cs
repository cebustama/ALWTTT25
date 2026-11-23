// [MUSICA-1c, Claudio 2026-11-22] Bass tweaks heard in MUSICA-1b, as TEST
// variants first. «Crear variantes de prueba» adds one config per tweak to its
// palette at weight 0: the game never draws it, the Card Bench plays it with
// «variant=<label>». «Adoptar» swaps each tested config into the original
// entry (same label, same level, weight 1) and removes the test entry.
// Both menus are idempotent. Assets go through AssetDatabase (Unity writes
// the .meta files).
using System;
using System.Collections.Generic;
using System.Linq;
using MidiGenPlay;
using MidiGenPlay.Composition;
using UnityEditor;
using UnityEngine;
using Deg = MidiGenPlay.Composition.BasslineCardConfigSO.BassCycleDegree;

namespace ALWTTT.Music.EditorTools
{
    public static class DemoBassTweaks1c
    {
        const string Bundles = "Assets/Resources/Data/Characters/StyleBundles/Bassline/";

        struct Tweak
        {
            public string palette, original, label, asset;
            public string cloneFrom;   // optional: start from a copy of this config
            public Action<BasslineCardConfigSO> setup;
        }

        static List<ChordExpressionWeight> W(params (ChordExpressionType f, float w)[] xs) =>
            xs.Select(x => new ChordExpressionWeight { figure = x.f, weight = x.w }).ToList();

        static readonly Tweak[] Tweaks =
        {
            new Tweak
            {   // root held one bar, root–fifth in half notes the next (A/B by bar)
                palette = "BassPalette_Ballad", original = "Ballad Root",
                label = "Ballad Root 1c", asset = "BassCard_BalladRoot1c",
                setup = b =>
                {
                    b.chordExpression = ChordExpressionType.Random;
                    b.randomFigureWeights = W((ChordExpressionType.Block, 1f), (ChordExpressionType.ArpeggioUp, 1f));
                    b.articulationSchedule = ArticulationSchedule.AlternateBars;
                    b.articulationAlternateEvery = 1;
                    b.arpeggioRate = ArpeggioRate.Half;
                    b.arpeggioToneMode = BasslineCardConfigSO.BassArpeggioToneMode.DegreeCycle;
                    b.degreeCycle = new List<Deg> { Deg.Root, Deg.Fifth };
                    b.degreeCycleApproach = false;
                },
            },
            new Tweak
            {   // held three bars; on the fill bar an ornament R‑3‑5‑8 with an
                // approach into the next chord. Weight 20:1 makes A = the hold.
                palette = "BassPalette_Ballad", original = "Ballad Hold",
                label = "Ballad Hold 1c", asset = "BassCard_BalladHold1c",
                setup = b =>
                {
                    b.chordExpression = ChordExpressionType.Random;
                    b.randomFigureWeights = W((ChordExpressionType.Block, 20f), (ChordExpressionType.ArpeggioUp, 1f));
                    b.articulationSchedule = ArticulationSchedule.HoldThenTurn;
                    b.articulationPhraseBars = 4;
                    b.arpeggioRate = ArpeggioRate.PerBeat;
                    b.arpeggioToneMode = BasslineCardConfigSO.BassArpeggioToneMode.DegreeCycle;
                    b.degreeCycle = new List<Deg> { Deg.Root, Deg.Third, Deg.Fifth, Deg.Octave };
                    b.degreeCycleApproach = true;
                },
            },
            new Tweak
            {   // the Music Lab ballad phrases kept, but only on strong accents
                // (crash after the fill, strong downbeats): fewer ornaments.
                palette = "BassPalette_Ballad", original = "Ballad Hold",
                label = "Ballad Hold Sparse 1c", asset = "BassCard_BalladHoldSparse1c",
                setup = b =>
                {
                    b.chordExpression = ChordExpressionType.Block;
                    b.accentFollowSettings.mode = BasslineCardConfigSO.AccentFollowMode.Phrases;
                    b.accentFollowSettings.threshold = 0.9f;
                    b.phraseStyle = BasslineCardConfigSO.PhraseStyle.Ballad;
                    b.phraseAnticipation = true;
                },
            },
            new Tweak
            {   // always walks (20:1), stop-time stabs on the fill bar, louder
                // on the drummer's accents, a bit more human jitter.
                palette = "BassPalette_Walking", original = "Walking que gira",
                label = "Walking que gira 1c", asset = "BassCard_WalkTurn1c",
                setup = b =>
                {
                    b.chordExpression = ChordExpressionType.Random;
                    b.randomFigureWeights = W((ChordExpressionType.ArpeggioUp, 20f), (ChordExpressionType.Staccato, 1f));
                    b.articulationSchedule = ArticulationSchedule.HoldThenTurn;
                    b.articulationPhraseBars = 4;
                    b.arpeggioRate = ArpeggioRate.PerBeat;
                    b.arpeggioToneMode = BasslineCardConfigSO.BassArpeggioToneMode.ImprovisedWalk;
                    b.velocityJitter = 10;
                    b.accentFollowSettings.mode = BasslineCardConfigSO.AccentFollowMode.Reinforce;
                    b.accentFollowSettings.threshold = 0.6f;
                },
            },
            new Tweak
            {   // the same walk in eighths (running bass), stop-time on the fill bar.
                palette = "BassPalette_Walking", original = "Walking que gira",
                label = "Walking doble 1c", asset = "BassCard_WalkDouble1c",
                setup = b =>
                {
                    b.chordExpression = ChordExpressionType.Random;
                    b.randomFigureWeights = W((ChordExpressionType.ArpeggioUp, 20f), (ChordExpressionType.Staccato, 1f));
                    b.articulationSchedule = ArticulationSchedule.HoldThenTurn;
                    b.articulationPhraseBars = 4;
                    b.arpeggioRate = ArpeggioRate.Eighth;
                    b.arpeggioToneMode = BasslineCardConfigSO.BassArpeggioToneMode.ImprovisedWalk;
                    b.velocityJitter = 10;
                    b.accentFollowSettings.mode = BasslineCardConfigSO.AccentFollowMode.Reinforce;
                    b.accentFollowSettings.threshold = 0.6f;
                },
            },
            new Tweak
            {   // MGP-BASS-JAZZ-1: the jazz walk (passing tones, enclosures, skips,
                // 2-and-4 dynamics, contour, triplet run on the fill bar)
                palette = "BassPalette_Walking", original = "Walking que gira",
                label = "Jazz Walk 1c", asset = "BassCard_JazzWalk1c",
                setup = b => JazzSetup(b, 0),
            },
            new Tweak
            {   // the same, two-feel (half notes) in the first 4 bars of the part
                palette = "BassPalette_Walking", original = "Walking que gira",
                label = "Jazz Walk Two-Feel 1c", asset = "BassCard_JazzWalkTwoFeel1c",
                setup = b => JazzSetup(b, 4),
            },
            new Tweak
            {   // Claudio (A/B 2026-11-23): «Walking doble» + the jazz walk — a running
                // eighth-note line with the jazz vocabulary (bebop-ish), turn run in triplets
                palette = "BassPalette_Walking", original = "Walking que gira",
                label = "Jazz Walk doble 1c", asset = "BassCard_JazzWalkDouble1c",
                setup = b =>
                {
                    JazzSetup(b, 0);
                    b.arpeggioRate = ArpeggioRate.Eighth;
                    b.jazzWalk.passingTones = 0.65f;   // more scale steps between the chord tones
                    b.jazzWalk.skips = 0f;             // no room for a triplet rake between eighths
                },
            },
            new Tweak
            {   // J10 jazz-funk with the fingers (Jaco, Rocco Prestia): continuous 16ths,
                // dead notes between jazz pitches, a pop at the end of the bar
                palette = "BassPalette_Walking", original = "Walking que gira",
                label = "Jazz Funk 1c", asset = "BassCard_JazzFunk1c",
                cloneFrom = "BasslineCardConfig_SlapGroove",
                setup = b =>
                {
                    const BasslineCardConfigSO.SelfPocketStep S = BasslineCardConfigSO.SelfPocketStep.Slap,
                        G = BasslineCardConfigSO.SelfPocketStep.Ghost, P = BasslineCardConfigSO.SelfPocketStep.Pop;
                    b.selfPocketPattern = new List<BasslineCardConfigSO.SelfPocketStep>
                        { S, G, S, G,  S, G, G, S,  G, S, G, S,  S, G, P, G };
                    b.selfPocketBarSubstitutions = new List<BasslineCardConfigSO.SelfPocketBarSubstitution>();
                    b.pocketSlapBoost = 0; b.pocketPopBoost = 6;   // fingers, not thumb
                    b.articulationPhraseBars = 4;
                    b.jazzWalk = new JazzWalkSettings
                    {
                        enabled = true, rhythmSource = JazzWalkSettings.RhythmSource.Pocket,
                        passingTones = 0.6f, enclosures = 0.3f, fifthApproach = 0.2f,
                    };
                },
            },
            new Tweak
            {   // J10 jazz-funk slapped (Marcus Miller): Slap Groove's own pocket
                // (bar variations included) with jazz pitches on the thumb
                palette = "BassPalette_Walking", original = "Walking que gira",
                label = "Jazz Funk Slap 1c", asset = "BassCard_JazzFunkSlap1c",
                cloneFrom = "BasslineCardConfig_SlapGroove",
                setup = b =>
                {
                    b.jazzWalk = new JazzWalkSettings
                    {
                        enabled = true, rhythmSource = JazzWalkSettings.RhythmSource.Pocket,
                        passingTones = 0.6f, enclosures = 0.3f, fifthApproach = 0.2f,
                    };
                },
            },
        };

        static void JazzSetup(BasslineCardConfigSO b, int twoFeelBars)
        {
            b.chordExpression = ChordExpressionType.ArpeggioUp;
            b.arpeggioRate = ArpeggioRate.PerBeat;
            b.arpeggioToneMode = BasslineCardConfigSO.BassArpeggioToneMode.ImprovisedWalk;
            b.articulationPhraseBars = 4;       // the fill bar = the drummer's (Default Mode fills every 4)
            b.velocityJitter = 0;               // the jazz walk owns its dynamics
            b.jazzWalk = new JazzWalkSettings { enabled = true, twoFeelBars = twoFeelBars };
        }

        static BasslineCardConfigSO Load(string name) =>
            AssetDatabase.LoadAssetAtPath<BasslineCardConfigSO>(Bundles + name + ".asset");

        static string Key(string s) => (s ?? "").Replace(" ", "").ToLowerInvariant();

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Crear variantes de prueba del bajo")]
        public static void CreateTestVariants()
        {
            int added = 0;
            foreach (var t in Tweaks)
            {
                var palette = Load(t.palette);
                if (palette == null) { Debug.LogError($"[MUSICA-1] Variantes 1c: falta {t.palette}."); return; }

                var cfg = Load(t.asset);
                if (cfg == null)
                {
                    var src = t.cloneFrom != null ? Load(t.cloneFrom) : null;
                    if (t.cloneFrom != null && src == null) { Debug.LogError($"[MUSICA-1] Variantes 1c: falta {t.cloneFrom}."); continue; }
                    cfg = src != null ? UnityEngine.Object.Instantiate(src)
                                      : ScriptableObject.CreateInstance<BasslineCardConfigSO>();
                    cfg.appliesTo = TrackRole.Bassline;
                    if (src == null) cfg.velocityJitter = 6;
                    cfg.accentFollowSettings ??= new AccentFollowSettings();
                    t.setup(cfg);
                    AssetDatabase.CreateAsset(cfg, Bundles + t.asset + ".asset");
                    Debug.Log($"[MUSICA-1] Variantes 1c: configuración {t.asset} creada.");
                }

                var orig = palette.variants.FirstOrDefault(v => Key(v.label) == Key(t.original));
                if (orig == null) { Debug.LogError($"[MUSICA-1] Variantes 1c: {t.palette} no tiene «{t.original}»."); continue; }
                if (palette.variants.Any(v => Key(v.label) == Key(t.label) || v.config == cfg))
                {
                    Debug.Log($"[MUSICA-1] Variantes 1c: «{t.label}» ya está en {t.palette}.");
                    continue;
                }
                palette.variants.Add(new BasslineCardConfigSO.BassVariant
                { label = t.label, config = cfg, weight = 0f, level = orig.level });
                EditorUtility.SetDirty(palette);
                added++;
                Debug.Log($"[MUSICA-1] Variantes 1c: «{t.label}» en {t.palette} con peso 0 " +
                          $"(fuera del sorteo; el Banco la toca con variant={Key(t.label)}).");
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[MUSICA-1] Variantes 1c: {added} añadidas. Siguiente: el A/B en el Banco de cartas.");
        }

        /// <summary>Swaps the tested config into the original entry. Which test
        /// variant wins for an original with two candidates is the caller's
        /// choice (D3 of MUSICA-1c): pass its label.</summary>
        static void Adopt(string paletteName, string testLabel)
        {
            var t = Tweaks.First(x => x.label == testLabel);
            var palette = Load(paletteName);
            var test = palette?.variants.FirstOrDefault(v => Key(v.label) == Key(testLabel));
            var orig = palette?.variants.FirstOrDefault(v => Key(v.label) == Key(t.original));
            if (test == null || orig == null)
            {
                Debug.Log($"[MUSICA-1] Adoptar: «{testLabel}» ya adoptada o sin crear en {paletteName}.");
                return;
            }
            string before = orig.config != null ? orig.config.name : "—";
            orig.config = test.config;
            orig.weight = Mathf.Max(orig.weight, 1f);
            // only the adopted test entry leaves (the jazz variants share the original)
            palette.variants.Remove(test);
            EditorUtility.SetDirty(palette);
            Debug.Log($"[MUSICA-1] Adoptar: «{t.original}» toca ahora {test.config.name} (antes {before}).");
        }

        /// <summary>Keeps a tested variant AS WELL: weight 1, label without « 1c»,
        /// the original stays (Claudio, 2026-11-22: «A y C me gustaron mucho las dos»).</summary>
        static void Promote(string paletteName, string testLabel)
        {
            var palette = Load(paletteName);
            var test = palette?.variants.FirstOrDefault(v => Key(v.label) == Key(testLabel));
            if (test == null) { Debug.Log($"[MUSICA-1] Sumar: «{testLabel}» no está en {paletteName} (¿ya sumada?)."); return; }
            test.label = testLabel.Replace(" 1c", "");
            test.weight = 1f;
            EditorUtility.SetDirty(palette);
            AssetDatabase.SaveAssets();
            Debug.Log($"[MUSICA-1] Sumar: «{test.label}» entra en el sorteo de {paletteName} ({test.level}): " +
                      string.Join(" · ", palette.variants.Where(v => v.weight > 0f).Select(v => v.label)));
        }

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Sumar Ballad Hold Sparse a la paleta")]
        public static void PromoteBalladSparse() => Promote("BassPalette_Ballad", "Ballad Hold Sparse 1c");

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Sumar Jazz Walk a la paleta")]
        public static void PromoteJazz() => Promote("BassPalette_Walking", "Jazz Walk 1c");

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Sumar Jazz Walk doble a la paleta")]
        public static void PromoteJazzDouble() => Promote("BassPalette_Walking", "Jazz Walk doble 1c");

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Sumar Walking doble a la paleta")]
        public static void PromoteWalkDouble() => Promote("BassPalette_Walking", "Walking doble 1c");

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Sumar Jazz Walk Two-Feel a la paleta")]
        public static void PromoteJazzTwoFeel() => Promote("BassPalette_Walking", "Jazz Walk Two-Feel 1c");

        // J2: the upright for Walking Bass (the card's instrument, every variant)
        const string PartEffects = "Assets/Resources/Data/Cards/Composition/_PartEffects/";
        const string AcousticBassGuid = "6577098b4737e8c46a49904c39c2fdbc";
        const string WalkingPayload = "Assets/Resources/Data/Characters/Musicians/starter_finger_bass_Payload.asset";

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Walking Bass con contrabajo (Acoustic Bass)")]
        public static void WalkingUpright()
        {
            string path = PartEffects + "InstrumentEffect_AcousticBass.asset";
            var fx = AssetDatabase.LoadAssetAtPath<ALWTTT.Cards.InstrumentEffect>(path);
            var fingered = AssetDatabase.LoadAssetAtPath<ALWTTT.Cards.InstrumentEffect>(PartEffects + "InstrumentEffect_FingeredBass.asset");
            if (fx == null)
            {
                if (fingered == null) { Debug.LogError("[MUSICA-1] Contrabajo: falta InstrumentEffect_FingeredBass."); return; }
                fx = UnityEngine.Object.Instantiate(fingered);
                fx.mode = ALWTTT.Cards.InstrumentEffect.InstrumentTargetMode.SpecificMelodic;
                fx.melodicInstrument = AssetDatabase.LoadAssetAtPath<MIDIInstrumentSO>(
                    AssetDatabase.GUIDToAssetPath(AcousticBassGuid));
                fx.melodicInstrumentPool = new List<MIDIInstrumentSO>();
                AssetDatabase.CreateAsset(fx, path);
                Debug.Log($"[MUSICA-1] Contrabajo: InstrumentEffect_AcousticBass creado ({(fx.melodicInstrument != null ? fx.melodicInstrument.name : "¡sin instrumento!")}).");
            }
            var payload = AssetDatabase.LoadMainAssetAtPath(WalkingPayload);
            var so = new SerializedObject(payload);
            var list = so.FindProperty("modifierEffects");
            for (int i = 0; i < list.arraySize; i++)
            {
                var el = list.GetArrayElementAtIndex(i);
                if (el.objectReferenceValue == fx) { Debug.Log("[MUSICA-1] Contrabajo: Walking Bass ya toca Acoustic Bass."); return; }
                if (el.objectReferenceValue == fingered)
                {
                    el.objectReferenceValue = fx;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    AssetDatabase.SaveAssets();
                    Debug.Log("[MUSICA-1] Contrabajo: Walking Bass toca ahora Acoustic Bass (antes Fingered Bass).");
                    return;
                }
            }
            Debug.LogWarning("[MUSICA-1] Contrabajo: Walking Bass no tenía InstrumentEffect_FingeredBass; no cambio nada.");
        }

        /// <summary>Moves a test variant to another card's palette, in the draw.</summary>
        static void MoveTo(string fromPalette, string testLabel, string toPalette, int level)
        {
            var from = Load(fromPalette); var to = Load(toPalette);
            var test = from?.variants.FirstOrDefault(v => Key(v.label) == Key(testLabel));
            if (test == null || to == null) { Debug.Log($"[MUSICA-1] Mover: «{testLabel}» ya movida o sin crear."); return; }
            from.variants.Remove(test);
            to.variants.Add(new BasslineCardConfigSO.BassVariant
            { label = testLabel.Replace(" 1c", ""), config = test.config, weight = 1f, level = (BasslineCardConfigSO.TrackLevel)level });
            EditorUtility.SetDirty(from); EditorUtility.SetDirty(to);
            Debug.Log($"[MUSICA-1] Mover: «{testLabel.Replace(" 1c", "")}» entra en el sorteo de {toPalette} (nivel {level}): " +
                      string.Join(" · ", to.variants.Where(v => v.weight > 0f).Select(v => v.label)));
        }

        /// <summary>Weight-0 entries whose config is already in the draw under another label (the
        /// re-created «Ballad Hold Sparse 1c» of 2026-11-23) leave.</summary>
        static void Dedupe(string paletteName)
        {
            var p = Load(paletteName); if (p == null) return;
            int n = p.variants.RemoveAll(v => v.weight <= 0f && p.variants.Any(o => o != v && o.weight > 0f && o.config == v.config));
            if (n > 0) { EditorUtility.SetDirty(p); Debug.Log($"[MUSICA-1] Limpieza: {n} duplicada(s) fuera de {paletteName}."); }
        }

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Aplicar lo aprobado (A/B del 23-nov)")]
        public static void ApplyApproved()
        {
            Dedupe("BassPalette_Ballad");
            Adopt("BassPalette_Walking", "Walking que gira 1c");          // 1c > original (A/B 22-nov)
            Promote("BassPalette_Walking", "Jazz Walk 1c");               // «Suena mucho mejor»
            Promote("BassPalette_Walking", "Jazz Walk doble 1c");         // «B es claramente superior»
            MoveTo("BassPalette_Walking", "Jazz Funk Slap 1c", "BassPalette_Slap", 2);  // Slap Groove, avanzado
            AssetDatabase.SaveAssets();
            Debug.Log("[MUSICA-1] Aprobado aplicado. Jazz Funk 1c (dedos) sigue fuera del sorteo hasta decidir su carta (D7).");
        }

        // D7 = C (Claudio, 2026-11-23): Jazz Funk with the fingers is its own Conito card,
        // a reward (RewardPool | UnlockedByDefault), Fingered Bass, no tempo effect (D4 design).
        // Built from copies of Slap Groove (CopyAsset: Unity gives new guids and .meta files);
        // the sprite is Slap Groove's until the art comes.
        const string Musicians = "Assets/Resources/Data/Characters/Musicians/";

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Crear carta Jazz Funk (recompensa de Conito)")]
        public static void CreateJazzFunkCard()
        {
            const string cardPath = Musicians + "conito_jazz_funk.asset";
            const string payloadPath = Musicians + "conito_jazz_funk_Payload.asset";
            const string palettePath = Bundles + "BassPalette_JazzFunk.asset";
            if (AssetDatabase.LoadMainAssetAtPath(cardPath) != null)
            { Debug.Log("[MUSICA-1] Carta Jazz Funk: ya existe."); return; }

            var cfg = Load("BassCard_JazzFunk1c");
            var fingered = AssetDatabase.LoadAssetAtPath<ALWTTT.Cards.InstrumentEffect>(PartEffects + "InstrumentEffect_FingeredBass.asset");
            var catalog = AssetDatabase.LoadAssetAtPath<ALWTTT.Cards.MusicianCardCatalogData>(Musicians + "Conito_CardCatalogData.asset");
            if (cfg == null || fingered == null || catalog == null)
            { Debug.LogError("[MUSICA-1] Carta Jazz Funk: falta BassCard_JazzFunk1c, InstrumentEffect_FingeredBass o el catálogo de Conito."); return; }

            // 1. the palette (one variant today; room for more)
            var palette = Load("BassPalette_JazzFunk");
            if (palette == null)
            {
                palette = ScriptableObject.CreateInstance<BasslineCardConfigSO>();
                palette.appliesTo = TrackRole.Bassline;
                palette.variants = new List<BasslineCardConfigSO.BassVariant>
                {
                    new BasslineCardConfigSO.BassVariant
                    { label = "Jazz Funk", config = cfg, weight = 1f, level = BasslineCardConfigSO.TrackLevel.Basic },
                };
                AssetDatabase.CreateAsset(palette, palettePath);
            }

            // 2. the payload: Slap Groove's, with the new palette and Fingered Bass only
            if (!AssetDatabase.CopyAsset(Musicians + "conito_slap_groove_Payload.asset", payloadPath))
            { Debug.LogError("[MUSICA-1] Carta Jazz Funk: no pude copiar el payload."); return; }
            var payload = AssetDatabase.LoadMainAssetAtPath(payloadPath);
            var ps = new SerializedObject(payload);
            ps.FindProperty("trackAction.styleBundle").objectReferenceValue = palette;
            var mods = ps.FindProperty("modifierEffects");
            mods.ClearArray(); mods.InsertArrayElementAtIndex(0);
            mods.GetArrayElementAtIndex(0).objectReferenceValue = fingered;
            ps.ApplyModifiedPropertiesWithoutUndo();

            // 3. the card
            if (!AssetDatabase.CopyAsset(Musicians + "conito_slap_groove.asset", cardPath))
            { Debug.LogError("[MUSICA-1] Carta Jazz Funk: no pude copiar la carta."); return; }
            var card = AssetDatabase.LoadAssetAtPath<ALWTTT.Cards.CardDefinition>(cardPath);
            var cs = new SerializedObject(card);
            cs.FindProperty("id").stringValue = "conito_jazz_funk";
            cs.FindProperty("displayName").stringValue = "Jazz Funk";
            cs.FindProperty("payload").objectReferenceValue = payload;
            cs.ApplyModifiedPropertiesWithoutUndo();

            // 4. Conito's catalog: a reward, unlocked by default
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

            // 5. the test entry leaves the Walking palette
            var walking = Load("BassPalette_Walking");
            if (walking != null && walking.variants.RemoveAll(v => Key(v.label) == Key("Jazz Funk 1c")) > 0)
                EditorUtility.SetDirty(walking);

            AssetDatabase.SaveAssets();
            Debug.Log("[MUSICA-1] Carta Jazz Funk creada: conito_jazz_funk (recompensa de Conito, Fingered Bass, " +
                      "paleta BassPalette_JazzFunk: Jazz Funk; coste y sprite de Slap Groove). En el Banco: card=conito_jazz_funk.");
        }

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Adoptar Ballad Root 1c")]
        public static void AdoptBalladRoot() { Adopt("BassPalette_Ballad", "Ballad Root 1c"); AssetDatabase.SaveAssets(); }

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Adoptar Ballad Hold 1c")]
        public static void AdoptBalladHold() { Adopt("BassPalette_Ballad", "Ballad Hold 1c"); AssetDatabase.SaveAssets(); }

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Adoptar Ballad Hold Sparse 1c")]
        public static void AdoptBalladHoldSparse() { Adopt("BassPalette_Ballad", "Ballad Hold Sparse 1c"); AssetDatabase.SaveAssets(); }

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Adoptar Walking que gira 1c")]
        public static void AdoptWalkTurn() { Adopt("BassPalette_Walking", "Walking que gira 1c"); AssetDatabase.SaveAssets(); }

        [MenuItem("ALWTTT/Music/MUSICA-1c ▸ Adoptar Walking doble 1c")]
        public static void AdoptWalkDouble() { Adopt("BassPalette_Walking", "Walking doble 1c"); AssetDatabase.SaveAssets(); }
    }
}
