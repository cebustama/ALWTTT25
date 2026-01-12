# ALWTTT --- Card System

## Purpose

Cards are the primary interaction surface between the player and the
game systems in *A Long Way to the Top*.

They express:

-   Musical intent.
-   Tactical decisions.
-   Narrative flavor.
-   Systemic gameplay effects.

This document describes the conceptual design and technical structure of
cards as they exist today.

The focus is intentionally **design-first**, while remaining precise
enough to align with implementation.

------------------------------------------------------------------------

## Card Domains

Cards exist in two distinct domains:

### Action Cards

-   Played between songs.
-   Cost 0 Inspiration in MVP.
-   Limited to one Action per window (MVP rule).
-   Apply immediate gameplay effects through Status Effects.
-   Represent tactical decisions, crowd interaction, and
    moment-to-moment control.

### Composition Cards

-   Played during composition.
-   Define the structure of the song.
-   Modify loop behavior and musical identity.
-   May also apply Status Effects (MVP behavior applies them
    immediately).

------------------------------------------------------------------------

## CardDefinition

A CardDefinition represents the immutable identity and metadata of a
card.

Key responsibilities:

-   Identity (Id, Name, Visuals).
-   Rarity and keywords.
-   CardType (synergy axis).
-   Inspiration cost and generation.
-   Domain derivation (Action vs Composition).
-   Targeting rules (derived or overridden).
-   Reference to a CardPayload.

The payload carries the mechanical meaning of the card.

------------------------------------------------------------------------

## CardPayload (Base)

All card payloads inherit from CardPayload.

Common fields:

-   `StatusActions : List<StatusEffectActionData>`\
    The complete list of gameplay effects expressed by the card.

This unified representation ensures that both Action and Composition
cards use the same effect model.

------------------------------------------------------------------------

## ActionCardPayload

ActionCardPayload represents Action-domain cards.

Fields:

-   `ActionTiming`\
    When the card can be played (BetweenSongsOnly in MVP).
-   `Conditions`\
    Reserved for future gating logic (currently ignored).
-   `StatusActions` (inherited from CardPayload)

Action cards no longer execute procedural action code. They express all
gameplay effects exclusively through CSO StatusActions.

------------------------------------------------------------------------

## CompositionCardPayload

CompositionCardPayload represents Composition-domain cards.

Fields include:

-   `PrimaryKind`
-   `TrackAction`
-   `PartAction`
-   `ModifierEffects`
-   `RequiresMusicianTarget`

StatusActions are inherited from CardPayload and may be used to inject
systemic effects into compositions.

------------------------------------------------------------------------

## Targeting Model

Target selection is derived from:

    StatusEffectActionData.TargetType

Rules:

-   Single-target types require explicit player selection:
    -   Musician
    -   AudienceCharacter
-   Group and random targets do not require selection:
    -   AllMusicians
    -   AllAudienceCharacters
    -   RandomMusician
    -   RandomAudienceCharacter
-   Cards may override targeting explicitly when needed.

Targeting logic is centralized in the HandController.

------------------------------------------------------------------------

## Runtime Execution Pipeline

### Action Cards

1.  Player selects a card from hand.
2.  HandController resolves:
    -   Performer (selected musician).
    -   Target (if required).
3.  CardBase executes:
    -   Inspiration spend / generation.
    -   ExecuteStatusActions:
        -   Resolve targets.
        -   Resolve StatusEffectSO via catalogue.
        -   Apply stacks to runtime container.
4.  Card is moved to discard / appropriate pile.

There is no CharacterActionProcessor involvement for cards anymore.

------------------------------------------------------------------------

### Composition Cards (MVP)

1.  Player plays a composition card.
2.  CompositionSession validates placement and applies it to the model.
3.  StatusActions (if any) are applied immediately to relevant
    musicians.

This timing is explicitly MVP and may evolve later into loop-based
semantics.

------------------------------------------------------------------------

## UI Description Behavior

Action card descriptions currently render **only the first
StatusAction**.

This is intentional for MVP simplicity.

**TODO:** - Support multi-effect descriptions. - Resolve
StatusEffectSO.DisplayName instead of raw EffectId. - Provide clean
access to StatusEffectCatalogue from UI without tight coupling.

------------------------------------------------------------------------

## Design Philosophy

-   Cards express intent, not implementation.
-   All gameplay effects are declarative and data-driven.
-   Ontology prevents duplication and semantic drift.
-   Content scales without multiplying code paths.
-   The system favors clarity and composability over micro-optimization.

Cards should feel expressive, readable, and systemically coherent ---
like musical phrases rather than isolated buttons.

------------------------------------------------------------------------

End of document.
