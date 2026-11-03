// Place at: Assets/Scripts/UI/Tooltips/TooltipStyleSO.cs
//
// [GIG-UI-1] ONE home for tooltip colours (Claudio, 2026-11-02): each kind of panel
// (ability, tastes, blocked...) gets its own background, header and body colour, so
// they can be tuned together against the gig art in one Inspector instead of being
// scattered across prefabs. Assigned on the TooltipManager prefab. A kind with no
// entry, or no asset at all, keeps the TooltipText prefab look (TooltipKind.Default).
//
// SOURCE IS PURE ASCII ON PURPOSE (F-T6-1 / F-SP1-2 family).

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Tooltips
{
    public enum TooltipKind
    {
        Default = 0,
        Ability = 1,
        Tastes = 2,
        Blocked = 3,
        Musician = 4,   // band member summary (Aguante, Compostura, breakdown rule)
        Status = 5,     // status icon tooltips (band and audience)
        Warning = 6,    // "No se puede jugar" panel on cards
    }

    [CreateAssetMenu(fileName = "TooltipStyle", menuName = "ALWTTT/UI/Tooltip Style")]
    public class TooltipStyleSO : ScriptableObject
    {
        [Serializable]
        public class PanelStyle
        {
            public TooltipKind kind;
            public Color background = Color.black;
            public Color header = Color.white;
            public Color body = Color.white;

            public PanelStyle() { }
            public PanelStyle(TooltipKind k, Color bg, Color h, Color b)
            { kind = k; background = bg; header = h; body = b; }
        }

        // Starting values: dark panels with a coloured header (Monster Train reads this
        // way): the dark fill separates the panel from the busy art, the header colour
        // says which kind it is. Tune freely; these are only the asset's defaults.
        [SerializeField]
        private List<PanelStyle> panels = new List<PanelStyle>
        {
            new PanelStyle(TooltipKind.Ability,
                new Color(0.13f, 0.09f, 0.03f, 0.94f), new Color(0.95f, 0.71f, 0.11f), new Color(0.93f, 0.90f, 0.84f)),
            new PanelStyle(TooltipKind.Tastes,
                new Color(0.04f, 0.10f, 0.17f, 0.94f), new Color(0.43f, 0.78f, 1.00f), new Color(0.90f, 0.93f, 0.96f)),
            new PanelStyle(TooltipKind.Blocked,
                new Color(0.17f, 0.05f, 0.05f, 0.94f), new Color(1.00f, 0.42f, 0.36f), new Color(0.94f, 0.88f, 0.88f)),
        };

        [Header("Audience hover column")]
        [Tooltip("Pixels from the top-right corner of the member's body collider to the " +
                 "column's top-left corner. +X = right, +Y = up.")]
        [SerializeField] private Vector2 audienceColumnOffset = new Vector2(8f, 40f);

        public Vector2 AudienceColumnOffset => audienceColumnOffset;

        public bool TryGet(TooltipKind kind, out PanelStyle style)
        {
            style = null;
            if (kind == TooltipKind.Default) return false;
            if (panels != null)
            {
                for (int i = 0; i < panels.Count; i++)
                {
                    if (panels[i] != null && panels[i].kind == kind) { style = panels[i]; return true; }
                }
            }
            // A kind added after the asset was created has no row yet: use the built-in
            // starting colours until someone adds the row in the Inspector.
            style = BuiltIn(kind);
            return style != null;
        }

        private static PanelStyle BuiltIn(TooltipKind kind)
        {
            switch (kind)
            {
                case TooltipKind.Musician:
                    return new PanelStyle(kind, new Color(0.04f, 0.14f, 0.10f, 0.94f),
                        new Color(0.45f, 0.95f, 0.70f), new Color(0.90f, 0.95f, 0.92f));
                case TooltipKind.Status:
                    return new PanelStyle(kind, new Color(0.11f, 0.06f, 0.17f, 0.94f),
                        new Color(0.80f, 0.62f, 1.00f), new Color(0.93f, 0.90f, 0.96f));
                case TooltipKind.Warning:
                    return new PanelStyle(kind, new Color(0.17f, 0.05f, 0.05f, 0.94f),
                        new Color(1.00f, 0.42f, 0.36f), new Color(0.94f, 0.88f, 0.88f));
                default:
                    return null;
            }
        }
    }
}
