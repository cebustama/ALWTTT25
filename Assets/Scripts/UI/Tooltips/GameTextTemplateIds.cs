// Place at: Assets/Scripts/UI/Tooltips/GameTextTemplateIds.cs
// [TXT-4 / D-TXT4-3] The template vocabulary. One declaration, two readers: the producers
// (CardEffectDescriptionBuilder, CardDefinitionDescriptionExtensions, AudienceCharacterCanvas)
// compose through GameText.Format(<const>, …); the editor (GameTextWindow ▸ Templates) treats
// `All` as the COVERAGE set every ConceptGlossarySO must contain — the same rule TXT-3 applies to
// status keys and keyword names. A new template is added HERE first; a producer that formats an id
// not listed here is a bug the coverage check cannot see.
//
// Naming: fx_* card-effect lines · tgt_* target phrases · ui_card_* card face / detail modal labels ·
// ui_aud_* audience canvas tooltip lines · ui_track_* song-panel track hover lines (TEXT-6) ·
// ui_song_* / ui_part_* song-panel title, theme and part labels (SONG-PANEL-1, D-SP1-3=B) ·
// ui_fx_* floating-text labels that are player-reachable by default (SONG-PANEL-1, D-SP1-4=B; composition
// floaters added at TEXT-VIBE-1, population 8) / ui_readout_* C1 Vibe readout hover (TEXT-VIBE-1, population 11) /
// ui_ctx_meter_<value> / ui_ctx_tempo_<value> context-row chip labels, DERIVED from the enum value (TEXT-VIBE-1,
// population 10, D-SP1-6=B): declared as helpers, not consts, and enumerated into All at type init.
// Placeholders are positional; the comment on each id is
// its argument contract.

// [TEXT-VIBE-1 / D5] Population 10 ids are enumerated from two MidiGenPlay enums (see UiCtxMeter below).
using MidiGenPlay;
using static MidiGenPlay.MusicTheory.MusicTheory;

// [CARD-FACE-1 / D-CF-10=A] Notation enums formatted as TEXT through derived id families. Aliases,
// not namespace imports: DryWetMidi also declares a ScaleDegree, and the static import above
// already brings MidiGenPlay's (same alias ModulationEffect.cs uses).
using NoteName = Melanchall.DryWetMidi.MusicTheory.NoteName;
using ScaleDegree = MidiGenPlay.MusicTheory.MusicTheory.ScaleDegree;

using ALWTTT.Cards;   // [CARD-FACE-2] PartActionKind
using ALWTTT.Enums;   // [CARD-FACE-2] CardType

namespace ALWTTT.Tooltips
{
    public static class GameTextTemplateIds
    {
        // ── Card-effect lines (CardEffectDescriptionBuilder) ─────────────────
        public const string FxNoEffects = "fx_no_effects";                       // —
        public const string FxGainStatus = "fx_gain_status";                     // {0} signed amount · {1} status name (both pre-coloured)
        public const string FxApplyStatus = "fx_apply_status";                   // {0} signed amount · {1} status name · {2} target phrase
        public const string FxDealVibe = "fx_deal_vibe";                         // {0} amount · {1} target phrase
        public const string FxRestoreVibe = "fx_restore_vibe";                   // {0} amount · {1} target phrase
        public const string FxDealStress = "fx_deal_stress";                     // {0} amount · {1} target phrase
        public const string FxHealStress = "fx_heal_stress";                     // {0} amount · {1} target phrase
        public const string FxDrawOne = "fx_draw_one";                           // {0} amount (always 1)
        public const string FxDrawMany = "fx_draw_many";                         // {0} amount
        public const string FxInspirationPerLoop = "fx_inspiration_per_loop";    // {0} signed amount
        public const string FxRevealTastes = "fx_reveal_tastes";                 // {0} target phrase
        public const string FxGrantBonusLoop = "fx_grant_bonus_loop";            // —
        public const string FxGrantBonusLoopSolo = "fx_grant_bonus_loop_solo";   // —

        // ── Target phrases (ActionTargetType) ────────────────────────────────
        public const string TgtSelf = "tgt_self";
        public const string TgtMusician = "tgt_musician";
        public const string TgtAllMusicians = "tgt_all_musicians";
        public const string TgtRandomMusician = "tgt_random_musician";
        public const string TgtAudience = "tgt_audience";
        public const string TgtAllAudience = "tgt_all_audience";
        public const string TgtRandomAudience = "tgt_random_audience";

        // ── Card face / detail modal (CardDefinitionDescriptionExtensions) ───
        public const string UiCardKindTrack = "ui_card_kind_track";              // —
        public const string UiCardKindPart = "ui_card_kind_part";                // —
        public const string UiCardKindComposition = "ui_card_kind_composition";  // —
        // [TEXT-5 / D-T5-3=A] Card-domain label on the card face (CardBase.typeTextField) and the detail
        // modal (CardDetailViewController.cardTypeText). Distinct from ui_card_kind_composition: "kind" is
        // PrimaryKind (Track/Part/Composition); "type" is the domain, shown on EVERY composition card.
        // The other branch of both sites is CardType.ToString() — an enum, F-TXT4-2 → CARD-FACE-1.
        public const string UiCardTypeComposition = "ui_card_type_composition";  // —
        // [ECON-2] Finisher: etiqueta de tipo en la cara y la línea de regla de su descripción.
        public const string UiCardTypeFinisher = "ui_card_type_finisher";        // —
        public const string UiCardFinisherRule = "ui_card_finisher_rule";        // —
        public const string UiCardModifierOne = "ui_card_modifier_one";          // {0} count (always 1)
        public const string UiCardModifierMany = "ui_card_modifier_many";        // {0} count
        public const string UiCardDetailStyle = "ui_card_detail_style";          // {0} style bundle asset name
        public const string UiCardDetailMusician = "ui_card_detail_musician";    // {0} musician id
        public const string UiCardDetailModifiers = "ui_card_detail_modifiers";  // —
        public const string UiCardDetailEffects = "ui_card_detail_effects";      // —

        // ── Audience canvas tooltip (AudienceCharacterCanvas) ────────────────
        public const string UiAudTasteBlockHeader = "ui_aud_taste_block_header"; // — (bold added in code)
        public const string UiAudTasteHeader = "ui_aud_taste_header";            // —
        public const string UiAudTasteNeutral = "ui_aud_taste_neutral";          // —
        public const string UiAudTasteFast = "ui_aud_taste_fast";                // —
        public const string UiAudTasteSlow = "ui_aud_taste_slow";                // —
        public const string UiAudTasteTracks = "ui_aud_taste_tracks";            // {0} minimum role count
        public const string UiAudVibeHype = "ui_aud_vibe_hype";                  // {0} hype base
        public const string UiAudVibeBlocked = "ui_aud_vibe_blocked";            // —
        public const string UiAudVibeSongEnd = "ui_aud_vibe_song_end";           // {0} final value text
        public const string UiAudVibeTastes = "ui_aud_vibe_tastes";              // {0} factor · {1} result
        public const string UiAudVibeStatusMult = "ui_aud_vibe_status_mult";     // {0} factor · {1} status name · {2} stacks · {3} result
        public const string UiAudVibeSfx = "ui_aud_vibe_sfx";                    // {0} sfx bonus · {1} subtotal
        public const string UiAudVibeIndifferent = "ui_aud_vibe_indifferent";    // {0} status name
        public const string UiAudVibeKo = "ui_aud_vibe_ko";                      // —
        public const string UiAudEffSuper = "ui_aud_eff_super";                  // —
        public const string UiAudEffResists = "ui_aud_eff_resists";              // —
        public const string UiAudEffImmune = "ui_aud_eff_immune";                // —
        public const string UiAudEffNormal = "ui_aud_eff_normal";                // —

        // ── Song-panel track hover (TrackHoverPanel) ─────────────────────────
        // [TEXT-6 / F-T5-2 / D-T6-3=B] Words only; separators (" · "), ": " and "-" stay in code as
        // punctuation. The row's "Modifiers" header reuses UiCardDetailModifiers (one concept, one home).
        public const string UiTrackNoTrack = "ui_track_no_track";                     // —
        public const string UiTrackPending = "ui_track_pending";                      // —
        public const string UiTrackLevel = "ui_track_level";                          // {0} level · {1} max level
        public const string UiTrackLevelMax = "ui_track_level_max";                   // —
        public const string UiTrackInspirationPerLoop = "ui_track_inspiration_per_loop"; // {0} inspiration next loop
        public const string UiTrackInstrument = "ui_track_instrument";                // {0} resolved instrument name, or "-"

        // [SONG-PANEL-1 / D-SP1-3=B] Card-less rows created by Part cards. These are NOT card names
        // (TryAddIntro/TryAddOutro/TryAddSolo receive no CardDefinition), so GameText.CardName cannot
        // supply them and they need ids of their own.
        public const string UiTrackLeadIn = "ui_track_lead_in";                       // —
        public const string UiTrackFinale = "ui_track_finale";                        // —
        public const string UiTrackImprovisation = "ui_track_improvisation";          // —

        // ── Song panel: title, theme, part labels (SongCompositionUI) ────────
        // [SONG-PANEL-1 / D-SP1-3=B] Replace three serialized English defaults and five literals.
        // UiPartLabel takes the part LETTER, not the index: the old code mixed "Part A" and "Part 2".
        public const string UiSongTitleDefault = "ui_song_title_default";             // —
        public const string UiSongTheme = "ui_song_theme";                            // {0} theme text
        public const string UiSongThemeDefault = "ui_song_theme_default";             // —
        public const string UiPartLabel = "ui_part_label";                            // {0} part letter (A, B, …)
        public const string UiPartIntro = "ui_part_intro";                            // —
        public const string UiPartOutro = "ui_part_outro";                            // —
        public const string UiPartSolo = "ui_part_solo";                              // —

        // [CARD-FACE-2 / D-CF-21] The MarkBridge part label. Was the literal "Bridge" in
        // SongCompositionUI.ApplyCardToPart (F-SP1-10 shape: a literal hiding behind a deleted field).
        public const string UiPartBridge = "ui_part_bridge";                          // —

        // ── Floating text (FloatingTextMidiListener) ─────────────────────
        // [SONG-PANEL-1 / D-SP1-4=B] Only the label reachable in a player build by default
        // (showChords ships ON). The percussion family ("Kick", "Snare", "HH cl", …) is DEV-only
        // diagnostics from RFX-1 with both gates OFF, so it stays literal behind ALWTTT_DEV — the
        // same reasoning that keeps the Bundle line out of the glossary (D-T6-4=A).
        public const string UiFxChordFallback = "ui_fx_chord_fallback";               // {0} comma-joined MIDI notes

        // ── Song panel: context row hover (CompositionContextRowUI) ───────
        // [SONG-PANEL-1b / F-SP1-8] Population 9. NOT covered here: meterLabel / tempoLabel / mood,
        // which are authored on CompositionStripThemeSO (asset text, population 10, D-SP1-6).
        public const string UiCtxLoop = "ui_ctx_loop";                                // {0} current loop · {1} total
        public const string UiCtxLoopNoTotal = "ui_ctx_loop_no_total";                // {0} current loop (total unknown)
        public const string UiCtxFinalLoopLocks = "ui_ctx_final_loop_locks";          // —
        public const string UiCtxBpmAbsolute = "ui_ctx_bpm_absolute";                 // {0} bpm
        public const string UiCtxPendingRender = "ui_ctx_pending_render";             // —
        public const string UiCtxPartOf = "ui_ctx_part_of";                           // {0} index · {1} total
        public const string UiCtxMusicianSilentOne = "ui_ctx_musician_silent_one";    // {0} count (always 1)
        public const string UiCtxMusiciansSilentMany = "ui_ctx_musicians_silent_many";// {0} count


        // -- C1 Vibe readout hover (GigCanvas.BuildReadoutTooltip) ------------------------------
        // [TEXT-VIBE-1 / F-TIP-2 / D3=A] Population 11. Words only; the line break between the total
        // and its note stays in code as layout. The SFX line is suppressed by D1 when sfx == 0.
        public const string UiReadoutHeader = "ui_readout_header";                   // --
        public const string UiReadoutHype = "ui_readout_hype";                       // {0} hype percent (int), {1} L part
        public const string UiReadoutSfx = "ui_readout_sfx";                         // {0} banked venue SFX
        public const string UiReadoutTotal = "ui_readout_total";                     // {0} L + SFX per member
        public const string UiReadoutTotalNote = "ui_readout_total_note";            // --  ("before tastes and statuses")
        public const string UiReadoutAvgResist = "ui_readout_avg_resist";            // {0} average max Vibe of unconvinced

        // -- Composition floaters (SongCompositionUI.SpawnCompositionFx) --------------------------
        // [TEXT-VIBE-1 / F-T6-9 / D-SP1-6=B] Population 8. One id per REACHABLE FxEntry of
        // CompositionFxConfigSO; the entry no longer carries text. Tonality and Modifier entries are
        // unreachable design hooks (no code path selects them) and get no id -- an id nobody can
        // format is coverage dead weight, and wiring the hook is the moment to add it.
        public const string UiFxTempo = "ui_fx_tempo";                               // --
        public const string UiFxMeter = "ui_fx_meter";                               // --
        public const string UiFxKey = "ui_fx_key";                                   // --  (mid-song modulation)
        public const string UiFxInstrument = "ui_fx_instrument";                     // --
        public const string UiFxMajorChange = "ui_fx_major_change";                  // --  (2+ diffs in one apply)
        public const string UiFxChange = "ui_fx_change";                             // --  (Fallback entry, off by default)
        public const string UiFxTrackRhythm = "ui_fx_track_rhythm";                  // --
        public const string UiFxTrackBacking = "ui_fx_track_backing";                // --
        public const string UiFxTrackMelody = "ui_fx_track_melody";                  // --
        public const string UiFxTrackHarmony = "ui_fx_track_harmony";                // --

        // -- Context-row chip labels, keyed by enum VALUE (CompositionContextRowUI) ---------------
        // [TEXT-VIBE-1 / F-SP1-13 / D-SP1-6=B, D5] Population 10. The id is derived from the MidiGenPlay
        // enum name, lower-cased: TimeSignature.FourFour -> "ui_ctx_meter_fourfour". Coverage is
        // complete BY CONSTRUCTION because All enumerates both enums (BuildAll): a package version
        // that adds a value turns the Templates tab red instead of shipping an enum name. Values no
        // card can reach today are live-but-inert ids (ui_track_level precedent). The enum names are
        // package-owned; ALWTTT owns only the text per value (SSoT_ALWTTT_MidiGenPlay_Boundary).
        public const string UiCtxMeterPrefix = "ui_ctx_meter_";
        public const string UiCtxTempoPrefix = "ui_ctx_tempo_";
        public static string UiCtxMeter(TimeSignature meter) => UiCtxMeterPrefix + meter.ToString().ToLowerInvariant();
        public static string UiCtxTempo(TempoRange tempo) => UiCtxTempoPrefix + tempo.ToString().ToLowerInvariant();


        // -- Card face / detail modal, CARD-FACE-1 -------------------------------------------------
        // [D-CF-5=B] Modifier lines are composed by PartEffectDescriptionBuilder: family GLYPH (GameSymbols)
        // + one of these values. Meter and tempo REUSE ui_ctx_meter_* / ui_ctx_tempo_* / ui_ctx_bpm_absolute
        // (D-T6-3=B: one concept, one id across surfaces).
        public const string UiModTempoScale = "ui_mod_tempo_scale";                 // {0} factor, culture-formatted
        public const string UiModKey = "ui_mod_key";                                 // {0} note name text (ui_note_*)
        public const string UiModDegree = "ui_mod_degree";                           // {0} degree text (ui_degree_*)
        public const string UiModRandomKey = "ui_mod_random_key";                    // --
        public const string UiModRandomInScale = "ui_mod_random_in_scale";           // --
        public const string UiModInstrument = "ui_mod_instrument";                   // {0} instrument name (MidiGenPlay asset data, untranslated)
        public const string UiModDrums = "ui_mod_drums";                             // {0} percussion instrument name
        public const string UiModInstrumentType = "ui_mod_instrument_type";          // {0} instrument type text (ui_instrumenttype_*)
        public const string UiModInstrumentRandom = "ui_mod_instrument_random";      // {0} pool size

        // -- Gig HUD (GigCanvas.SetSongsLeft) [F-TV-1 / D-CF-7] --
        public const string UiGigSongsLeft = "ui_gig_songs_left";                    // {0} songs left

        // -- [GIG-UI-1] Consola del manager: cohesión, pips de jugada, resumen del músico, carta no jugable --
        public const string UiGigCohesionHeader = "ui_gig_cohesion_header";          // {0} current · {1} max
        public const string UiGigCohesionBody = "ui_gig_cohesion_body";              // --
        public const string UiEconActionHeader = "ui_econ_action_header";            // --
        public const string UiEconCompositionHeader = "ui_econ_composition_header";  // --
        public const string UiEconActionOne = "ui_econ_action_one";                  // --
        public const string UiEconActionMany = "ui_econ_action_many";                // {0} remaining
        public const string UiEconActionNone = "ui_econ_action_none";                // --
        public const string UiEconCompositionOne = "ui_econ_composition_one";        // --
        public const string UiEconCompositionMany = "ui_econ_composition_many";      // {0} remaining
        public const string UiEconCompositionNone = "ui_econ_composition_none";      // --
        public const string UiMusMeter = "ui_mus_meter";                             // {0} current · {1} max
        public const string UiMusMeterShield = "ui_mus_meter_shield";                // {0} current · {1} max · {2} Composure stacks
        public const string UiMusBreakdownRule = "ui_mus_breakdown_rule";            // --
        // [S9c / D3=C] Last line of a column whose concept panels wait for Shift.
        public const string UiTipConceptsHint = "ui_tip_concepts_hint";              // --
        // [S4b / D4=A] Who the audience member's ability hits, first line of its tooltip.
        public const string UiAudHitOne = "ui_aud_hit_one";                          // {0} musician · {1} Stress
        public const string UiAudHitEach = "ui_aud_hit_each";                        // {0} musicians · {1} Stress each
        public const string UiAudHitAll = "ui_aud_hit_all";                          // {0} Stress each, whole band
        public const string UiAudHitHidden = "ui_aud_hit_hidden";                    // {0} musician(s), value hidden
        public const string UiAudHitAllHidden = "ui_aud_hit_all_hidden";            // -- whole band, value hidden
        public const string UiUnplayableHeader = "ui_unplayable_header";             // --
        public const string UiGigPlayButton = "ui_gig_play_button";                  // -- label of the play/end-turn button
        public const string UiUnplayableBudgetAction = "ui_unplayable_budget_action";           // -- Budget, action card
        public const string UiUnplayableBudgetComposition = "ui_unplayable_budget_composition"; // -- Budget, composition card
        // Keyed by enum VALUE, same contract as ui_ctx_meter_*: All enumerates the enum (BuildAll), so a
        // new UnplayableReason turns the Templates tab red instead of showing a raw id.
        public const string UiUnplayablePrefix = "ui_unplayable_";
        public static string UiUnplayable(ALWTTT.Managers.UnplayableReason r) =>
            UiUnplayablePrefix + r.ToString().ToLowerInvariant();

        // -- Banked SFX Vibe floater (GigManager.ApplySfxBonusVibe) [F-TV-4 / D-CF-8] --
        public const string UiFxVibeBonus = "ui_fx_vibe_bonus";                      // {0} banked Vibe (int)

        // -- Derived families, CARD-FACE-1 (D-CF-6, D-CF-10=A). Same contract as ui_ctx_meter_*: the id is
        // the enum name lower-cased, All enumerates the enum in BuildAll, coverage is complete BY CONSTRUCTION,
        // inert values keep their id. TrackRole, ScaleDegree and InstrumentType are MidiGenPlay-owned;
        // NoteName is DryWetMidi's (ALWTTT owns only the text per value, boundary section 8.13).
        public const string UiRolePrefix = "ui_role_";
        public const string UiNotePrefix = "ui_note_";
        public const string UiDegreePrefix = "ui_degree_";
        public const string UiInstrumentTypePrefix = "ui_instrumenttype_";
        public static string UiRole(TrackRole role) => UiRolePrefix + role.ToString().ToLowerInvariant();
        public static string UiNote(NoteName note) => UiNotePrefix + note.ToString().ToLowerInvariant();
        public static string UiDegree(ScaleDegree degree) => UiDegreePrefix + degree.ToString().ToLowerInvariant();
        public static string UiInstrumentType(InstrumentType type) => UiInstrumentTypePrefix + type.ToString().ToLowerInvariant();


        // -- Derived families, CARD-FACE-2 -------------------------------------------------------------
        // [D-CF-17=D / D-CF-15] Tonality is a NOTATION enum (D-CF-10=A): its player text is a derived id,
        // exactly like ui_ctx_meter_<value>. What the text SAYS is the emotional colour of the mode
        // ("Bright", "Alegre"), never the mode's name. MoodTag is NOT printed anywhere any more: it stays
        // a presentation datum (chip glyph + tint) on CompositionStripThemeSO. Tonality is MidiGenPlay's
        // (boundary section 8.13: ALWTTT owns only the text per value).
        public const string UiTonalityPrefix = "ui_tonality_";
        public static string UiTonality(Tonality t) => UiTonalityPrefix + t.ToString().ToLowerInvariant();
        // TonalityEffect's three random modes are not values of the enum: three fixed ids.
        public const string UiTonalityRandomAny = "ui_tonality_random_any";           // —
        public const string UiTonalityRandomMajorish = "ui_tonality_random_majorish"; // —
        public const string UiTonalityRandomMinorish = "ui_tonality_random_minorish"; // —
        // [D-CF-13=C] The WORD beside a categorical glyph where there is no hover (the detail modal),
        // and the whole label when the Dev toggle shows text instead of symbols (D-CF-19=B).
        public const string UiCardTypePrefix = "ui_cardtype_";
        public const string UiPartActionPrefix = "ui_partaction_";
        public static string UiCardType(CardType c) => UiCardTypePrefix + c.ToString().ToLowerInvariant();
        public static string UiPartAction(PartActionKind k) => UiPartActionPrefix + k.ToString().ToLowerInvariant();

        /// <summary>Coverage set: every id a producer may format. Read by the editor only.</summary>
        // [TEXT-VIBE-1 / D5] Fixed ids first (textual order matters: static initialisers run top-down, and
        // BuildAll reads Fixed). All = Fixed + one id per TimeSignature value + one per TempoRange value.
        private static readonly string[] Fixed =
        {
            FxNoEffects, FxGainStatus, FxApplyStatus, FxDealVibe, FxRestoreVibe, FxDealStress, FxHealStress,
            FxDrawOne, FxDrawMany, FxInspirationPerLoop, FxRevealTastes,
            FxGrantBonusLoop, FxGrantBonusLoopSolo,
            TgtSelf, TgtMusician, TgtAllMusicians, TgtRandomMusician, TgtAudience, TgtAllAudience, TgtRandomAudience,
            UiCardKindTrack, UiCardKindPart, UiCardKindComposition, UiCardTypeComposition, UiCardTypeFinisher, UiCardFinisherRule, UiCardModifierOne, UiCardModifierMany,
            UiCardDetailStyle, UiCardDetailMusician, UiCardDetailModifiers, UiCardDetailEffects,
            UiAudTasteBlockHeader, UiAudTasteHeader, UiAudTasteNeutral, UiAudTasteFast, UiAudTasteSlow, UiAudTasteTracks,
            UiAudVibeHype, UiAudVibeBlocked, UiAudVibeSongEnd, UiAudVibeTastes, UiAudVibeStatusMult, UiAudVibeSfx,
            UiAudVibeIndifferent, UiAudVibeKo,
            UiAudEffSuper, UiAudEffResists, UiAudEffImmune, UiAudEffNormal,
            UiTrackNoTrack, UiTrackPending, UiTrackLevel, UiTrackLevelMax, UiTrackInspirationPerLoop, UiTrackInstrument,
            UiTrackLeadIn, UiTrackFinale, UiTrackImprovisation,
            UiSongTitleDefault, UiSongTheme, UiSongThemeDefault, UiPartLabel, UiPartIntro, UiPartOutro, UiPartSolo,
            UiFxChordFallback,
            UiCtxLoop, UiCtxLoopNoTotal, UiCtxFinalLoopLocks, UiCtxBpmAbsolute, UiCtxPendingRender, UiCtxPartOf,
            UiCtxMusicianSilentOne, UiCtxMusiciansSilentMany,
            UiReadoutHeader, UiReadoutHype, UiReadoutSfx, UiReadoutTotal, UiReadoutTotalNote, UiReadoutAvgResist,
            UiFxTempo, UiFxMeter, UiFxKey, UiFxInstrument, UiFxMajorChange, UiFxChange,
            UiFxTrackRhythm, UiFxTrackBacking, UiFxTrackMelody, UiFxTrackHarmony,
            // [CARD-FACE-1]
            UiModTempoScale, UiModKey, UiModDegree, UiModRandomKey, UiModRandomInScale,
            UiModInstrument, UiModDrums, UiModInstrumentType, UiModInstrumentRandom,
            UiGigSongsLeft, UiFxVibeBonus,
            // [GIG-UI-1]
            UiGigCohesionHeader, UiGigCohesionBody,
            UiEconActionHeader, UiEconCompositionHeader,
            UiEconActionOne, UiEconActionMany, UiEconActionNone,
            UiEconCompositionOne, UiEconCompositionMany, UiEconCompositionNone,
            UiMusMeter, UiMusMeterShield, UiMusBreakdownRule, UiUnplayableHeader,
            UiGigPlayButton, UiUnplayableBudgetAction, UiUnplayableBudgetComposition,
            // [CARD-FACE-2]
            UiPartBridge, UiTonalityRandomAny, UiTonalityRandomMajorish, UiTonalityRandomMinorish,
            // [S4b]
            UiAudHitOne, UiAudHitEach, UiAudHitAll, UiAudHitHidden, UiAudHitAllHidden,
            // [S9c]
            UiTipConceptsHint,
        };


        /// <summary>Coverage set: every id a producer may format. Read by the editor only.</summary>
        public static readonly string[] All = BuildAll();

        private static string[] BuildAll()
        {
            var seen = new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            var list = new System.Collections.Generic.List<string>(Fixed.Length + 16);
            foreach (var id in Fixed) if (seen.Add(id)) list.Add(id);
            // Enum.GetValues returns aliased values twice; the set keeps All free of duplicates.
            foreach (TimeSignature m in System.Enum.GetValues(typeof(TimeSignature)))
            { var id = UiCtxMeter(m); if (seen.Add(id)) list.Add(id); }
            foreach (TempoRange t in System.Enum.GetValues(typeof(TempoRange)))
            { var id = UiCtxTempo(t); if (seen.Add(id)) list.Add(id); }
            // [CARD-FACE-1] Four more derived families (see the prefixes above).
            foreach (TrackRole r in System.Enum.GetValues(typeof(TrackRole)))
            { var id = UiRole(r); if (seen.Add(id)) list.Add(id); }
            foreach (NoteName n in System.Enum.GetValues(typeof(NoteName)))
            { var id = UiNote(n); if (seen.Add(id)) list.Add(id); }
            foreach (ScaleDegree d in System.Enum.GetValues(typeof(ScaleDegree)))
            { var id = UiDegree(d); if (seen.Add(id)) list.Add(id); }
            foreach (InstrumentType it in System.Enum.GetValues(typeof(InstrumentType)))
            { var id = UiInstrumentType(it); if (seen.Add(id)) list.Add(id); }
            // [CARD-FACE-2] Three more derived families. Whole enums, None included: a template id is
            // text and costs nothing inert (section 8.13); only the SPRITE set excludes None (D-CF-20).
            foreach (Tonality t in System.Enum.GetValues(typeof(Tonality)))
            { var id = UiTonality(t); if (seen.Add(id)) list.Add(id); }
            foreach (CardType c in System.Enum.GetValues(typeof(CardType)))
            { var id = UiCardType(c); if (seen.Add(id)) list.Add(id); }
            // [GIG-UI-1] One id per UnplayableReason value, None included (inert, §8.13).
            foreach (ALWTTT.Managers.UnplayableReason r in System.Enum.GetValues(typeof(ALWTTT.Managers.UnplayableReason)))
            { var id = UiUnplayable(r); if (seen.Add(id)) list.Add(id); }
            foreach (PartActionKind k in System.Enum.GetValues(typeof(PartActionKind)))
            { var id = UiPartAction(k); if (seen.Add(id)) list.Add(id); }
            return list.ToArray();
        }
    }
}