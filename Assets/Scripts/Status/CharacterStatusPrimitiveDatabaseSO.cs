#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Status
{
    /// <summary>
    /// CSO Registry asset:
    /// Stores the canonical list of Character Status Ontology (CSO) primitives as data
    /// so designers can browse, and systems can validate StatusEffect assets against it.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CharacterStatusPrimitiveDatabase",
        menuName = "ALWTTT/Status/CSO/Character Status Primitive Database",
        order = 10)]
    public sealed class CharacterStatusPrimitiveDatabaseSO : ScriptableObject
    {
        [SerializeField]
        private List<CharacterStatusPrimitiveEntry> _entries = new();

        // Non-serialized runtime cache for fast lookups.
        [NonSerialized] private Dictionary<CharacterStatusId, CharacterStatusPrimitiveEntry> _cache;
        [NonSerialized] private bool _cacheBuilt;

        /// <summary>All primitive entries (authoritative list stored in the asset).</summary>
        public IReadOnlyList<CharacterStatusPrimitiveEntry> Entries => _entries;

        /// <summary>Returns true if an entry exists for the given id.</summary>
        public bool Contains(CharacterStatusId id)
        {
            EnsureCache();
            return _cache.ContainsKey(id);
        }

        /// <summary>Try-get an ontology primitive entry by its EffectId.</summary>
        public bool TryGet(CharacterStatusId id, out CharacterStatusPrimitiveEntry entry)
        {
            EnsureCache();
            return _cache.TryGetValue(id, out entry);
        }

        /// <summary>Get an ontology primitive entry by its EffectId. Throws if missing.</summary>
        public CharacterStatusPrimitiveEntry GetOrThrow(CharacterStatusId id)
        {
            EnsureCache();
            if (_cache.TryGetValue(id, out var entry))
                return entry;

            throw new KeyNotFoundException($"CSO primitive entry not found for id: {id}");
        }

        private void OnEnable()
        {
            // Ensure cache rebuild when entering playmode / domain reload.
            _cacheBuilt = false;
            _cache = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Light validation: rebuild cache and check duplicates/missing.
            _cacheBuilt = false;
            _cache = null;

            // Duplicate detection (helps prevent broken registries).
            var seen = new HashSet<CharacterStatusId>();
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (e == null) continue;

                if (!seen.Add(e.EffectId))
                {
                    Debug.LogError(
                        $"[CSO] Duplicate EffectId '{e.EffectId}' in {name}. " +
                        $"Remove/merge duplicates to keep the ontology stable.",
                        this);
                }
            }
        }
#endif

        private void EnsureCache()
        {
            if (_cacheBuilt) return;

            _cache = new Dictionary<CharacterStatusId, CharacterStatusPrimitiveEntry>(_entries.Count);
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                if (e == null) continue;

                // First one wins; duplicates will be surfaced in OnValidate (editor) and ignored here.
                if (!_cache.ContainsKey(e.EffectId))
                    _cache.Add(e.EffectId, e);
            }

            _cacheBuilt = true;
        }

#if UNITY_EDITOR
        [ContextMenu("Populate From CSO Canonical")]
        private void PopulateFromCanonical()
        {
            // Use SerializedObject properly (single instance).
            var so = new SerializedObject(this);
            var listProp = so.FindProperty("_entries");

            so.Update();

            // Clear via SerializedProperty (not via _entries.Clear()) to avoid sync issues.
            listProp.ClearArray();

            foreach (CharacterStatusId id in Enum.GetValues(typeof(CharacterStatusId)))
            {
                if (!TryGetCanonicalData(id, out var data))
                {
                    Debug.LogWarning(
                        $"[CSO] No canonical data mapping found for {id}. Add it to TryGetCanonicalData().",
                        this);
                    continue;
                }

                int index = listProp.arraySize;
                listProp.InsertArrayElementAtIndex(index);

                var element = listProp.GetArrayElementAtIndex(index);

                // ✅ Correct: set enum by underlying value
                element.FindPropertyRelative("effectId").intValue = (int)id;

                element.FindPropertyRelative("category").stringValue = data.Category;
                element.FindPropertyRelative("abstractFunction").stringValue = data.AbstractFunction;
                element.FindPropertyRelative("slayTheSpireReference").stringValue = data.Slay;
                element.FindPropertyRelative("monsterTrainReference").stringValue = data.MonsterTrain;
                element.FindPropertyRelative("griftlandsReference").stringValue = data.Griftlands;
            }

            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CSO] Populated {listProp.arraySize} primitive entries in {name}.", this);
        }

        private static bool TryGetCanonicalData(
            CharacterStatusId id,
            out CanonicalPrimitiveData data)
        {
            switch (id)
            {
                case CharacterStatusId.DamageUpFlat:
                    data = new CanonicalPrimitiveData(
                        "Offensive",
                        "+X damage per hit / action",
                        "Strength",
                        "Rage",
                        "Power");
                    return true;

                case CharacterStatusId.DamageUpMultiplier:
                    data = new CanonicalPrimitiveData(
                        "Offensive",
                        "Multiplies outgoing damage",
                        "Strength scaling relics",
                        "Rage scaling variants",
                        "Power variants");
                    return true;

                case CharacterStatusId.DamageDownFlat:
                    data = new CanonicalPrimitiveData(
                        "Offensive Control",
                        "-X damage from attacker",
                        "Weak (approx)",
                        "Sap",
                        "Impair");
                    return true;

                case CharacterStatusId.DamageDownMultiplier:
                    data = new CanonicalPrimitiveData(
                        "Offensive Control",
                        "-% damage from attacker",
                        "Weak",
                        "Sap (indirect)",
                        "Impair");
                    return true;

                case CharacterStatusId.DamageTakenUpFlat:
                    data = new CanonicalPrimitiveData(
                        "Burst",
                        "Target receives +X extra damage",
                        "Vulnerable (approx)",
                        "—",
                        "Wound");
                    return true;

                case CharacterStatusId.DamageTakenUpMultiplier:
                    data = new CanonicalPrimitiveData(
                        "Burst",
                        "Target receives +% extra damage",
                        "Vulnerable",
                        "—",
                        "Vulnerability (Negotiation)");
                    return true;

                case CharacterStatusId.TempShieldTurn:
                    data = new CanonicalPrimitiveData(
                        "Defense",
                        "Temporary shield, resets per turn",
                        "Block",
                        "—",
                        "Defense");
                    return true;

                case CharacterStatusId.TempShieldPersistent:
                    data = new CanonicalPrimitiveData(
                        "Defense",
                        "Shield persists until depleted",
                        "Plated Armor",
                        "Armor",
                        "—");
                    return true;

                case CharacterStatusId.NegateNextHit:
                    data = new CanonicalPrimitiveData(
                        "Defense",
                        "Negates next damage instance",
                        "Intangible (partial)",
                        "Damage Shield",
                        "Evasion");
                    return true;

                case CharacterStatusId.NegateNextNInstances:
                    data = new CanonicalPrimitiveData(
                        "Defense",
                        "Negates N damage instances",
                        "Artifact (analog)",
                        "Damage Shield (stacks)",
                        "Evasion (stacks)");
                    return true;

                case CharacterStatusId.AntiShieldGain:
                    data = new CanonicalPrimitiveData(
                        "Control",
                        "Reduces shield generation",
                        "Frail",
                        "—",
                        "Exposed");
                    return true;

                case CharacterStatusId.DamageReflection:
                    data = new CanonicalPrimitiveData(
                        "Control",
                        "Reflects damage back",
                        "Thorns",
                        "Spikes",
                        "Counter");
                    return true;

                case CharacterStatusId.DamageOverTime:
                    data = new CanonicalPrimitiveData(
                        "Pressure",
                        "Automatic periodic damage",
                        "Poison",
                        "Frostbite",
                        "Bleed / Heated");
                    return true;

                case CharacterStatusId.DisableActions:
                    data = new CanonicalPrimitiveData(
                        "Tempo Control",
                        "Target cannot act",
                        "Entangle / stun-like",
                        "Dazed",
                        "Stun");
                    return true;

                case CharacterStatusId.DisableMovement:
                    data = new CanonicalPrimitiveData(
                        "Control",
                        "Prevents movement / state changes",
                        "—",
                        "Rooted",
                        "—");
                    return true;

                case CharacterStatusId.InitiativeBoost:
                    data = new CanonicalPrimitiveData(
                        "Tempo",
                        "Acts earlier",
                        "—",
                        "Quick",
                        "—");
                    return true;

                case CharacterStatusId.MultiHitModifier:
                    data = new CanonicalPrimitiveData(
                        "Scaling",
                        "Grants additional hits",
                        "Multi-hit cards",
                        "Multistrike",
                        "—");
                    return true;

                case CharacterStatusId.PiercingDamage:
                    data = new CanonicalPrimitiveData(
                        "Penetration",
                        "Ignores shields / mitigation",
                        "Penetrating attacks",
                        "Piercing",
                        "Piercing");
                    return true;

                case CharacterStatusId.DebuffImmunityStacks:
                    data = new CanonicalPrimitiveData(
                        "Resistance",
                        "Blocks debuff application",
                        "Artifact",
                        "—",
                        "—");
                    return true;

                case CharacterStatusId.DebuffCleanse:
                    data = new CanonicalPrimitiveData(
                        "Recovery",
                        "Removes debuffs",
                        "Cleanse effects",
                        "Purify",
                        "Cleanse cards");
                    return true;

                case CharacterStatusId.ArchetypeAmplifier:
                    data = new CanonicalPrimitiveData(
                        "Meta",
                        "Boosts card archetypes",
                        "Form powers / relic synergies",
                        "Clan mechanics",
                        "Influence / Dominance");
                    return true;

                case CharacterStatusId.TempoAcceleration:
                    data = new CanonicalPrimitiveData(
                        "Meta",
                        "Faster loops / turns",
                        "Energy relics / draw engines",
                        "Floor scaling engines",
                        "Momentum cards");
                    return true;

                case CharacterStatusId.ResourceGenerationModifier:
                    data = new CanonicalPrimitiveData(
                        "Meta",
                        "Modifies resource generation",
                        "Energy / draw scaling",
                        "Ember / draw scaling",
                        "Action / draw engines");
                    return true;
            }

            data = default;
            return false;
        }

        private readonly struct CanonicalPrimitiveData
        {
            public readonly string Category;
            public readonly string AbstractFunction;
            public readonly string Slay;
            public readonly string MonsterTrain;
            public readonly string Griftlands;

            public CanonicalPrimitiveData(
                string category,
                string abstractFunction,
                string slay,
                string monsterTrain,
                string griftlands)
            {
                Category = category;
                AbstractFunction = abstractFunction;
                Slay = slay;
                MonsterTrain = monsterTrain;
                Griftlands = griftlands;
            }
        }
#endif

    }

    /// <summary>
    /// A single CSO primitive entry (the ontology row).
    /// This is NOT a gameplay status by itself; it is a catalog/manifest record.
    /// </summary>
    [Serializable]
    public sealed class CharacterStatusPrimitiveEntry
    {
        [SerializeField] private CharacterStatusId effectId;

        // Keeping Category as a string for maximum flexibility and zero extra enums/files.
        // If you later want strict categories, you can replace with an enum without changing EffectId.
        [SerializeField] private string category;

        [TextArea(2, 6)]
        [SerializeField] private string abstractFunction;

        // Cross-game references: designer mental anchors (optional but very useful).
        [SerializeField] private string slayTheSpireReference;
        [SerializeField] private string monsterTrainReference;
        [SerializeField] private string griftlandsReference;

        public CharacterStatusId EffectId => effectId;
        public string Category => category;
        public string AbstractFunction => abstractFunction;

        public string SlayTheSpireReference => slayTheSpireReference;
        public string MonsterTrainReference => monsterTrainReference;
        public string GriftlandsReference => griftlandsReference;
    }
}
