namespace ALWTTT.Status
{
    /// <summary>
    /// Character Status Ontology (CSO)
    /// Canonical list of deduplicated status effect primitives.
    ///
    /// IMPORTANT:
    /// - Numeric values are part of the serialization contract.
    /// - NEVER change existing values.
    /// - NEVER reorder entries.
    /// - Only append new values in unused ranges.
    /// </summary>
    public enum CharacterStatusId
    {
        // ───────────── Offensive (100–199)
        DamageUpFlat = 100,
        DamageUpMultiplier = 101,

        // ───────────── Offensive Control (200–299)
        DamageDownFlat = 200,
        DamageDownMultiplier = 201,

        // ───────────── Burst (300–399)
        DamageTakenUpFlat = 300,
        DamageTakenUpMultiplier = 301,

        // ───────────── Defense (400–499)
        TempShieldTurn = 400,
        TempShieldPersistent = 401,
        NegateNextHit = 402,
        NegateNextNInstances = 403,

        // ───────────── Control (500–599)
        AntiShieldGain = 500,
        DamageReflection = 501,
        DisableMovement = 502,
        ShakenRestriction = 503,

        // ───────────── Pressure (600–699)
        DamageOverTime = 600,

        // ───────────── Tempo Control (700–799)
        DisableActions = 700,

        // ───────────── Tempo (800–899)
        InitiativeBoost = 800,

        // ───────────── Scaling (900–949)
        MultiHitModifier = 900,

        // ───────────── Penetration (950–979)
        PiercingDamage = 950,

        // ───────────── Resistance / Recovery (980–989)
        DebuffImmunityStacks = 980,
        DebuffCleanse = 981,

        // ───────────── Meta (990–1099)
        ArchetypeAmplifier = 990,
        TempoAcceleration = 991,
        ResourceGenerationModifier = 992,
    }
}
