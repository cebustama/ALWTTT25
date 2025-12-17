using UnityEngine;

namespace ALWTTT.Cards
{
    /// <summary>
    /// "What a card does": polymorphic payload.
    /// Action vs Composition (and future payload types) live here.
    /// </summary>
    public abstract class CardPayload : ScriptableObject
    {
        // Step 0: to avoid touching legacy enums yet, we reuse CardData's nested enums.
        // In a later step, we can extract these enums to top-level.
        public abstract CardDomain Domain { get; }
    }
}
