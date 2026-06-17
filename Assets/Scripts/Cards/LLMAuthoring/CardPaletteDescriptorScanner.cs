using System.Collections.Generic;
using UnityEditor;

using MidiGenPlay;
using MidiGenPlay.Composition;

namespace ALWTTT.Cards.LLMAuthoring
{
    /// <summary>
    /// Scans the project for palette assets and summarizes them as
    /// <see cref="PaletteDescriptor"/>s for the intent resolver and the prompt
    /// vocabulary. Editor-API code (AssetDatabase) — deliberately kept out of
    /// the unit-tested resolver, which only ever sees descriptors.
    ///
    /// The raw per-entry numbers mirror what the CE-F1 typed finders feed the
    /// selector: chord StructuralOnsets = progression event count
    /// (<c>ProgressionFinder.FeaturesFor</c>), drum StructuralOnsets =
    /// foundational (kick) onset count (<c>PatternFinder.FoundationOnsets</c>).
    /// </summary>
    public static class CardPaletteDescriptorScanner
    {
        public static List<PaletteDescriptor> ScanDrumPalettes()
        {
            var result = new List<PaletteDescriptor>();
            foreach (var guid in AssetDatabase.FindAssets("t:DrumPatternPaletteSO"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var palette = AssetDatabase.LoadAssetAtPath<DrumPatternPaletteSO>(path);
                if (palette == null) continue;

                var d = new PaletteDescriptor
                {
                    Id = path,
                    DisplayName = palette.GetDisplayName(),
                    Notes = palette.paletteNotes,
                    IsDrumDomain = true
                };

                if (palette.entries != null)
                {
                    foreach (var e in palette.entries)
                    {
                        if (e == null || e.pattern == null) continue;
                        var p = e.pattern;
                        d.Entries.Add(new PaletteEntryDescriptor
                        {
                            TimeSignature = p.TimeSignature,
                            Subdivisions = p.subdivisions,
                            Measures = p.Measures,
                            StructuralOnsets = PatternFinder.FoundationOnsets(p)
                        });
                    }
                }

                result.Add(d);
            }
            return result;
        }

        public static List<PaletteDescriptor> ScanChordPalettes()
        {
            var result = new List<PaletteDescriptor>();
            foreach (var guid in AssetDatabase.FindAssets("t:ChordProgressionPaletteSO"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var palette = AssetDatabase.LoadAssetAtPath<ChordProgressionPaletteSO>(path);
                if (palette == null) continue;

                var d = new PaletteDescriptor
                {
                    Id = path,
                    DisplayName = palette.GetDisplayName(),
                    Notes = palette.paletteNotes,
                    IsDrumDomain = false
                };

                if (palette.entries != null)
                {
                    foreach (var e in palette.entries)
                    {
                        if (e == null || e.progression == null) continue;
                        var p = e.progression;
                        d.Entries.Add(new PaletteEntryDescriptor
                        {
                            TimeSignature = p.TimeSignature,
                            Subdivisions = p.subdivisions,
                            Measures = p.Measures,
                            StructuralOnsets = p.events != null ? p.events.Count : 0
                        });
                    }
                }

                result.Add(d);
            }
            return result;
        }

        /// <summary>Load a previously scanned palette back from its descriptor id (asset path).</summary>
        public static T LoadPalette<T>(string descriptorId) where T : UnityEngine.Object
        {
            if (string.IsNullOrWhiteSpace(descriptorId)) return null;
            return AssetDatabase.LoadAssetAtPath<T>(descriptorId);
        }
    }
}
