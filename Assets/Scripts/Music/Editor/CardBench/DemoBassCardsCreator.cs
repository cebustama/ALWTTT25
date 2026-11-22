// [MUSICA-1b, D-BASS-CARDS=B, Claudio 2026-11-22] One click creates the demo's
// new bass cards for Conito (card + payload + BasslineCardConfigSO bundle),
// cloned from Walking Bass, and puts them in Conito's reward pool. Assets are
// created through AssetDatabase, so Unity writes their .meta files. Idempotent:
// a card whose asset already exists is skipped (its settings are left alone).
using System;
using System.Collections.Generic;
using System.Linq;
using ALWTTT.Cards;
using MidiGenPlay;
using MidiGenPlay.Composition;
using UnityEditor;
using UnityEngine;

namespace ALWTTT.Music.EditorTools
{
    public static class DemoBassCardsCreator
    {
        const string Musicians = "Assets/Resources/Data/Characters/Musicians/";
        const string Bundles = "Assets/Resources/Data/Characters/StyleBundles/Bassline/";
        const string BaseCard = Musicians + "starter_finger_bass.asset";
        const string Catalog = Musicians + "Conito_CardCatalogData.asset";
        const string SlapBassGuid = "bf6434b906dc29c43b12b447abd72457";   // starter_slap_bass
        const int RewardFlags = 6;   // Reward | Unlocked (CardAcquisitionFlags)

        struct Spec
        {
            public string id, name, bundle;
            public Action<BasslineCardConfigSO> setup;
        }

        static readonly Spec[] Specs =
        {
            new Spec
            {
                id = "conito_root_pump", name = "Root Pump", bundle = "BassCard_RootPump",
                setup = b =>
                {   // rock / punk: the root in driving eighths
                    b.chordExpression = ChordExpressionType.ArpeggioUp;
                    b.arpeggioRate = ArpeggioRate.Eighth;
                    b.arpeggioToneMode = BasslineCardConfigSO.BassArpeggioToneMode.RepeatedNote;
                },
            },
            new Spec
            {
                id = "conito_root_five", name = "Root Five", bundle = "BassCard_RootFive",
                setup = b =>
                {   // root–fifth–octave–fifth in quarters, approach into the next chord
                    b.chordExpression = ChordExpressionType.ArpeggioUp;
                    b.arpeggioRate = ArpeggioRate.PerBeat;
                    b.arpeggioToneMode = BasslineCardConfigSO.BassArpeggioToneMode.DegreeCycle;
                    b.degreeCycle = new List<BasslineCardConfigSO.BassCycleDegree>
                    {
                        BasslineCardConfigSO.BassCycleDegree.Root, BasslineCardConfigSO.BassCycleDegree.Fifth,
                        BasslineCardConfigSO.BassCycleDegree.Octave, BasslineCardConfigSO.BassCycleDegree.Fifth,
                    };
                    b.degreeCycleApproach = true;
                },
            },
            new Spec
            {
                id = "conito_root_fills", name = "Root & Fills", bundle = "BassCard_RootFills",
                setup = b =>
                {   // the root with ornaments: Music Lab phrases, Pop style, on the kick
                    b.chordExpression = ChordExpressionType.Block;
                    b.accentFollowSettings ??= new AccentFollowSettings();
                    b.accentFollowSettings.mode = BasslineCardConfigSO.AccentFollowMode.Phrases;
                    b.phraseStyle = BasslineCardConfigSO.PhraseStyle.Pop;
                    b.phraseAnticipation = true;
                },
            },
            new Spec
            {
                id = "conito_ballad_hold", name = "Ballad Hold", bundle = "BassCard_BalladHold",
                setup = b =>
                {   // long notes, a fifth and an approach at the phrase end
                    b.chordExpression = ChordExpressionType.Block;
                    b.accentFollowSettings ??= new AccentFollowSettings();
                    b.accentFollowSettings.mode = BasslineCardConfigSO.AccentFollowMode.Phrases;
                    b.phraseStyle = BasslineCardConfigSO.PhraseStyle.Ballad;
                    b.phraseAnticipation = true;
                },
            },
        };

        [MenuItem("ALWTTT/Music/MUSICA-1b ▸ Crear bajos nuevos de Conito")]
        public static void Create()
        {
            var baseCard = AssetDatabase.LoadAssetAtPath<CardDefinition>(BaseCard);
            var catalog = AssetDatabase.LoadAssetAtPath<MusicianCardCatalogData>(Catalog);
            if (baseCard == null || catalog == null)
            {
                Debug.LogError($"[MUSICA-1] Bajos nuevos: falta {(baseCard == null ? BaseCard : Catalog)}.");
                return;
            }
            var baseSo = new SerializedObject(baseCard);
            var basePayload = baseSo.FindProperty("payload").objectReferenceValue;

            var catSo = new SerializedObject(catalog);
            var entries = catSo.FindProperty("entries");
            int created = 0;

            foreach (var spec in Specs)
            {
                string cardPath = Musicians + spec.id + ".asset";
                if (AssetDatabase.LoadAssetAtPath<CardDefinition>(cardPath) != null)
                {
                    Debug.Log($"[MUSICA-1] Bajos nuevos: «{spec.name}» ya existe, no se toca.");
                    continue;
                }

                var bundle = ScriptableObject.CreateInstance<BasslineCardConfigSO>();
                bundle.appliesTo = TrackRole.Bassline;
                bundle.velocityJitter = 6;
                spec.setup(bundle);
                AssetDatabase.CreateAsset(bundle, Bundles + spec.bundle + ".asset");

                var payload = UnityEngine.Object.Instantiate(basePayload);
                AssetDatabase.CreateAsset(payload, Musicians + spec.id + "_Payload.asset");
                var paySo = new SerializedObject(payload);
                paySo.FindProperty("trackAction.styleBundle").objectReferenceValue = bundle;
                paySo.ApplyModifiedPropertiesWithoutUndo();

                var card = UnityEngine.Object.Instantiate(baseCard);
                AssetDatabase.CreateAsset(card, cardPath);
                var cardSo = new SerializedObject(card);
                cardSo.FindProperty("id").stringValue = spec.id;
                cardSo.FindProperty("displayName").stringValue = spec.name;
                cardSo.FindProperty("payload").objectReferenceValue = payload;
                cardSo.ApplyModifiedPropertiesWithoutUndo();

                int i = entries.arraySize;
                entries.InsertArrayElementAtIndex(i);
                var e = entries.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("card").objectReferenceValue = card;
                e.FindPropertyRelative("flags").intValue = RewardFlags;
                e.FindPropertyRelative("unlockId").stringValue = "";
                e.FindPropertyRelative("starterCopies").intValue = 1;
                created++;
                Debug.Log($"[MUSICA-1] Bajos nuevos: «{spec.name}» creada ({spec.id}, {spec.bundle}), en recompensas de Conito.");
            }

            // Disco Octaves: the existing Slap Bass card joins the reward pool.
            for (int i = 0; i < entries.arraySize; i++)
            {
                var e = entries.GetArrayElementAtIndex(i);
                var c = e.FindPropertyRelative("card").objectReferenceValue;
                if (c == null || AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(c)) != SlapBassGuid) continue;
                if (e.FindPropertyRelative("flags").intValue != RewardFlags)
                {
                    e.FindPropertyRelative("flags").intValue = RewardFlags;
                    Debug.Log("[MUSICA-1] Bajos nuevos: «Slap Bass» (octavas disco) pasa a recompensas de Conito.");
                }
            }

            catSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"[MUSICA-1] Bajos nuevos: {created} cartas creadas.");
        }

        // ------------------------------------------------------------------
        // [MUSICA-1b, D-BASS-FAMILIES, MGP-BASS-PALETTE-1, Claudio 2026-11-22]
        // Four bass cards, each a palette of variants by level:
        //   Root Line (new, starter x2) · Slap Groove (starter x1)
        //   Walking Bass (reward)       · Ballad Hold (reward)
        // The single-variant cards made earlier today leave Conito's catalog
        // (their assets stay, for the Card Bench). Idempotent.
        // ------------------------------------------------------------------

        const string SuperSlapCardGuid = "951b1c995b2fabb4c845932b2c05afca";
        static readonly string[] RetiredCards = { "conito_root_pump", "conito_root_five", "conito_root_fills" };

        static BasslineCardConfigSO Bundle(string name) =>
            AssetDatabase.LoadAssetAtPath<BasslineCardConfigSO>(Bundles + name + ".asset");

        static BasslineCardConfigSO EnsureBundle(string name, Action<BasslineCardConfigSO> setup)
        {
            var b = Bundle(name);
            if (b != null) return b;
            b = ScriptableObject.CreateInstance<BasslineCardConfigSO>();
            b.appliesTo = TrackRole.Bassline;
            b.velocityJitter = 6;
            setup(b);
            AssetDatabase.CreateAsset(b, Bundles + name + ".asset");
            Debug.Log($"[MUSICA-1] Paletas de bajo: configuración {name} creada.");
            return b;
        }

        static BasslineCardConfigSO.BassVariant V(string label, BasslineCardConfigSO cfg,
            BasslineCardConfigSO.TrackLevel level) =>
            new BasslineCardConfigSO.BassVariant { label = label, config = cfg, weight = 1f, level = level };

        static BasslineCardConfigSO EnsurePalette(string name, params BasslineCardConfigSO.BassVariant[] vs)
        {
            foreach (var v in vs)
                if (v.config == null) throw new Exception($"falta la configuración de «{v.label}»");
            var p = Bundle(name);
            bool isNew = p == null;
            if (isNew)
            {
                p = ScriptableObject.CreateInstance<BasslineCardConfigSO>();
                p.appliesTo = TrackRole.Bassline;
            }
            p.variants = new List<BasslineCardConfigSO.BassVariant>(vs);
            if (isNew) AssetDatabase.CreateAsset(p, Bundles + name + ".asset");
            else EditorUtility.SetDirty(p);
            Debug.Log($"[MUSICA-1] Paletas de bajo: {name} = " +
                      string.Join(" · ", vs.Select(v => $"{v.label} ({v.level})")));
            return p;
        }

        static void PointCardAt(CardDefinition card, BasslineCardConfigSO palette)
        {
            var payload = new SerializedObject(card).FindProperty("payload").objectReferenceValue;
            var so = new SerializedObject(payload);
            so.FindProperty("trackAction.styleBundle").objectReferenceValue = palette;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"[MUSICA-1] Paletas de bajo: «{card.name}» toca ahora la paleta {palette.name}.");
        }

        static CardDefinition EnsureCard(string id, string displayName)
        {
            string path = Musicians + id + ".asset";
            var card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
            if (card != null) return card;
            var baseCard = AssetDatabase.LoadAssetAtPath<CardDefinition>(BaseCard);
            var basePayload = new SerializedObject(baseCard).FindProperty("payload").objectReferenceValue;
            var payload = UnityEngine.Object.Instantiate(basePayload);
            AssetDatabase.CreateAsset(payload, Musicians + id + "_Payload.asset");
            card = UnityEngine.Object.Instantiate(baseCard);
            AssetDatabase.CreateAsset(card, path);
            var so = new SerializedObject(card);
            so.FindProperty("id").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("payload").objectReferenceValue = payload;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"[MUSICA-1] Paletas de bajo: carta «{displayName}» creada ({id}).");
            return card;
        }

        [MenuItem("ALWTTT/Music/MUSICA-1b ▸ Organizar los bajos de Conito en 4 cartas")]
        public static void Organize()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<MusicianCardCatalogData>(Catalog);
            if (catalog == null) { Debug.LogError($"[MUSICA-1] Paletas de bajo: falta {Catalog}."); return; }
            if (Bundle("BassCard_RootPump") == null)
            { Debug.LogError("[MUSICA-1] Paletas de bajo: primero «Crear bajos nuevos de Conito»."); return; }

            try
            {
                // Variant configs that did not exist yet.
                var funkSlap = EnsureBundle("BassCard_FunkSlapPhrases", b =>
                {
                    b.chordExpression = ChordExpressionType.Block;
                    b.accentFollowSettings ??= new AccentFollowSettings();
                    b.accentFollowSettings.mode = BasslineCardConfigSO.AccentFollowMode.Phrases;
                    b.phraseStyle = BasslineCardConfigSO.PhraseStyle.Funk;
                    b.phraseTechnique = BasslineCardConfigSO.PhraseTechnique.Slap;
                    b.phraseFollowSnare = true;
                    b.phraseDeadNotes = BasslineCardConfigSO.PhraseDeadNotes.Snare;
                    b.phraseAnticipation = true;
                });
                var walkTriad = EnsureBundle("BassCard_WalkTriad", b =>
                {
                    b.chordExpression = ChordExpressionType.ArpeggioUp;
                    b.arpeggioRate = ArpeggioRate.PerBeat;
                    b.arpeggioToneMode = BasslineCardConfigSO.BassArpeggioToneMode.ChordToneWalk;
                });
                var walkPlain = EnsureBundle("BassCard_WalkImprov", b =>
                {   // Walking Bass as it was before today: always walks
                    b.chordExpression = ChordExpressionType.ArpeggioUp;
                    b.arpeggioRate = ArpeggioRate.PerBeat;
                    b.arpeggioToneMode = BasslineCardConfigSO.BassArpeggioToneMode.ImprovisedWalk;
                });
                var balladRoot = EnsureBundle("BassCard_BalladRoot", b =>
                {
                    b.chordExpression = ChordExpressionType.Block;
                });

                var balladHalf = EnsureBundle("BassCard_BalladHalf", b =>
                {   // root and fifth in half notes (MGP-BASS-DEGREES-2)
                    b.chordExpression = ChordExpressionType.ArpeggioUp;
                    b.arpeggioRate = ArpeggioRate.Half;
                    b.arpeggioToneMode = BasslineCardConfigSO.BassArpeggioToneMode.DegreeCycle;
                    b.degreeCycle = new List<BasslineCardConfigSO.BassCycleDegree>
                    {
                        BasslineCardConfigSO.BassCycleDegree.Root, BasslineCardConfigSO.BassCycleDegree.Fifth,
                    };
                    b.degreeCycleApproach = false;
                });

                const BasslineCardConfigSO.TrackLevel Basic = BasslineCardConfigSO.TrackLevel.Basic,
                    Mid = BasslineCardConfigSO.TrackLevel.Mid, Adv = BasslineCardConfigSO.TrackLevel.Advanced;

                var pRoot = EnsurePalette("BassPalette_Root",
                    V("Root Pump", Bundle("BassCard_RootPump"), Basic),
                    V("Root Five", Bundle("BassCard_RootFive"), Mid),
                    V("Root & Fills", Bundle("BassCard_RootFills"), Adv));
                var pSlap = EnsurePalette("BassPalette_Slap",
                    V("Disco Octaves", Bundle("BasslineCardConfig_SlapV1"), Basic),
                    V("Slap Groove", Bundle("BasslineCardConfig_SlapGroove"), Mid),
                    V("Super Slap", Bundle("BasslineCardConfig_SuperSlapV1"), Adv),
                    V("Funk Slap Phrases", funkSlap, Adv));
                var pWalk = EnsurePalette("BassPalette_Walking",
                    V("Walk Triad", walkTriad, Basic),
                    V("Walking", walkPlain, Mid),
                    V("Walking que gira", Bundle("BassCard_W2_Improv"), Adv));
                var pBallad = EnsurePalette("BassPalette_Ballad",
                    V("Ballad Root", balladRoot, Basic),
                    V("Ballad Half", balladHalf, Mid),
                    V("Ballad Hold", Bundle("BassCard_BalladHold"), Adv));

                var rootLine = EnsureCard("conito_root_line", "Root Line");
                var slapGroove = AssetDatabase.LoadAssetAtPath<CardDefinition>(Musicians + "conito_slap_groove.asset");
                var walking = AssetDatabase.LoadAssetAtPath<CardDefinition>(BaseCard);
                var ballad = AssetDatabase.LoadAssetAtPath<CardDefinition>(Musicians + "conito_ballad_hold.asset");
                PointCardAt(rootLine, pRoot);
                PointCardAt(slapGroove, pSlap);
                PointCardAt(walking, pWalk);
                PointCardAt(ballad, pBallad);

                // Catalog: starter Root Line x2 + Slap Groove x1; Walking and Ballad as rewards.
                var catSo = new SerializedObject(catalog);
                var entries = catSo.FindProperty("entries");
                void Set(CardDefinition card, int flags, int copies)
                {
                    for (int i = 0; i < entries.arraySize; i++)
                    {
                        var e = entries.GetArrayElementAtIndex(i);
                        if (e.FindPropertyRelative("card").objectReferenceValue != card) continue;
                        e.FindPropertyRelative("flags").intValue = flags;
                        e.FindPropertyRelative("starterCopies").intValue = copies;
                        return;
                    }
                    int n = entries.arraySize;
                    entries.InsertArrayElementAtIndex(n);
                    var ne = entries.GetArrayElementAtIndex(n);
                    ne.FindPropertyRelative("card").objectReferenceValue = card;
                    ne.FindPropertyRelative("flags").intValue = flags;
                    ne.FindPropertyRelative("unlockId").stringValue = "";
                    ne.FindPropertyRelative("starterCopies").intValue = copies;
                }
                Set(rootLine, 5, 2);     // Starter | Unlocked
                Set(slapGroove, 5, 1);
                Set(walking, 6, 1);      // Reward | Unlocked
                Set(ballad, 6, 1);

                for (int i = entries.arraySize - 1; i >= 0; i--)
                {
                    var e = entries.GetArrayElementAtIndex(i);
                    var c = e.FindPropertyRelative("card").objectReferenceValue as CardDefinition;
                    if (c == null) continue;
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(c));
                    if (RetiredCards.Contains(c.name))
                    {
                        entries.DeleteArrayElementAtIndex(i);
                        Debug.Log($"[MUSICA-1] Paletas de bajo: «{c.name}» sale del catálogo (vive en la paleta Root).");
                    }
                    else if (guid == SlapBassGuid || guid == SuperSlapCardGuid)
                    {   // inside the Slap palette now: back to Unlocked only, like Slap Bass before today
                        e.FindPropertyRelative("flags").intValue = 4;
                        Debug.Log($"[MUSICA-1] Paletas de bajo: «{c.name}» deja las recompensas (vive en la paleta Slap).");
                    }
                }
                catSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
                Debug.Log("[MUSICA-1] Paletas de bajo: listo. Conito: Root Line ×2 y Slap Groove ×1 al empezar; Walking Bass y Ballad Hold en recompensas.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MUSICA-1] Paletas de bajo: {ex.Message}");
            }
        }
    }
}
