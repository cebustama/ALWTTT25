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
        NegateIncomingPositive = 404,  // B3: while stacks > 0, blocks 100% of an
                                       // incoming positive value (audience Vibe in
                                       // current scope; future musician analog possible).
                                       // Decay via container Tick, NOT per-application.
                                       // First user: Indifference (audience-side, B3).

        // ───────────── Control (500–599)
        AntiShieldGain = 500,
        DamageReflection = 501,
        DisableMovement = 502,
        ShakenRestriction = 503,

        // [R4 / D-R4-3=A] Taunt primitive. While an instance with stacks > 0 is
        // active on a character, single-target hostile targeting from the audience
        // side (ActionTargetType.Musician / RandomMusician in
        // AudienceCharacterBase.ResolveTargetsFor) redirects to the holder.
        // AllMusicians is deliberately NOT redirected: an AoE already includes the
        // holder, and redirecting it would convert the ability to single-target.
        // Runtime guards on StatusKey in addition to this id (Earworm/Captivated
        // precedent) so a future RedirectIncoming variant cannot silently taunt.
        RedirectIncoming = 504,

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