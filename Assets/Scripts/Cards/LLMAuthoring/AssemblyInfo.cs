// ALWTTT.Cards.LLMAuthoring assembly metadata.
//
// CE-L1 B1: exposes internal members of ALWTTT.Cards.LLMAuthoring to its
// Editor-only test assembly, mirroring the MidiGenPlay.Runtime precedent
// (MGP-ALWTTT-MOD-DIR-1.1). Used by tests that target internal seams
// (e.g. CardPaletteIntentResolver.RepresentativeFeatures / FeaturesFor)
// where a full public-API fixture would be disproportionately heavy.
//
// Keep the InternalsVisibleTo list narrow — one entry per test assembly.
// Do not add production assemblies here.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ALWTTT.Cards.LLMAuthoring.Tests")]