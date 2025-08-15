using UnityEngine;

namespace ALWTTT.Enums
{
    // TODO: Better, thematic names
    public enum StatusType
    {
        None = 0,
        BlockStress = 1,
        BlockVibe = 2,
        Strength = 3, // Added to damage (either Stress or Vibe)
        Poison = 4,
        Stun = 5,
        Dexterity = 6, // Added to Block
    }
}