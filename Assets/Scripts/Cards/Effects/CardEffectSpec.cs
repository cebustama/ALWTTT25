using System;

namespace ALWTTT.Cards.Effects
{
    /// <summary>
    /// Base type for all card effects (polymorphic via [SerializeReference]).
    /// Keep abstract + small; concrete specs hold only data.
    /// </summary>
    [Serializable]
    public abstract class CardEffectSpec
    {
        // Intentionally empty: concrete effect specs define their own fields.
        // YTODO: metadata (e.g., debugLabel)
    }
}
