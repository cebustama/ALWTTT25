// Place at: Assets/Scripts/Tutorial/TutorialHighlightSpawnHook.cs
using System;
using System.Collections.Generic;
using ALWTTT.Cards;
using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;
using ALWTTT.Status;
using UnityEngine;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [CARD-UX-1 / D1=C] Spawn-hook registration of tutorial highlight targets
    /// on runtime-instantiated objects: characters (BuildBand/BuildAudience),
    /// status icons (CharacterCanvas.TryCreateIcon) and hand cards (DeckManager
    /// BuildAndGetCard tails). The prefab-variant route (D1=B) cannot cover
    /// these keys: status icons spawn per-status, and every card shares one
    /// prefab while the key depends on CardDefinition.Id.
    ///
    /// All attach points are null-tolerant no-ops. With no tutorial active the
    /// attached components merely self-register into a registry nobody reads.
    ///
    /// Precision model:
    ///   - Status icons are precise by construction: they spawn exactly when
    ///     the status applies (= when the dialog fires) and last-registered
    ///     wins in TutorialHighlightRegistry.
    ///   - musician_stress_bar / status_icon_blocked are made precise by the
    ///     [D3=B] re-register in TutorialController.OnMusicianStressHit /
    ///     OnAudienceBlocked (both events carry the character ref).
    ///   - audience_vibe_bars is informational; last-spawned-wins is accepted.
    /// </summary>
    public static class TutorialHighlightSpawnHook
    {
        // Keys must exist in TutorialHighlightKeys.Known (OnValidate parity).
        private const string KeyMusicianStressBar = "musician_stress_bar";
        private const string KeyAudienceVibeBars = "audience_vibe_bars";
        private const string KeyStatusBlocked = "status_icon_blocked";
        private const string KeyStatusMusician = "status_icon_musician";
        private const string KeyStatusComposure = "status_icon_composure";
        private const string KeyStatusAudience = "status_icon_audience";

        // [D1=C] CardDefinition.Id → highlight key. Registered by
        // TutorialGuidedDriver.Awake from its serialized card refs, so this
        // class carries no hardcoded card ids. Empty map (no tutorial in
        // scene) ⇒ AttachToCard no-ops for every card.
        private static readonly Dictionary<string, string> _cardKeysById =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Called by TutorialGuidedDriver.Awake (data-driven map).</summary>
        public static void RegisterCardKey(string cardId, string highlightKey)
        {
            if (string.IsNullOrWhiteSpace(cardId) ||
                string.IsNullOrWhiteSpace(highlightKey)) return;
            _cardKeysById[cardId] = highlightKey;
        }

        /// <summary>GigManager.BuildBand, after BuildCharacter(). Host is the
        /// musician ROOT (always active) so registration survives the S5e-ext
        /// full-bar concealment deactivating the bar GameObject.</summary>
        public static void AttachToMusician(MusicianBase musician)
        {
            if (musician == null) return;

            var bar = musician.BandCharacterCanvas != null
                ? musician.BandCharacterCanvas.MeterBarRect
                : null;

            Attach(musician.gameObject, KeyMusicianStressBar,
                world: bar != null ? (Transform)bar : musician.transform,
                renderer: bar != null ? null : musician.SpriteRenderer);
        }

        /// <summary>GigManager.BuildAudience, after BuildCharacter(). Attaches
        /// BOTH audience keys to the root: the vibe bar rect, and the body
        /// sprite for Blocked (a tint bool per M1.2/E3 — no SO icon exists,
        /// so the highlight targets the tinted character itself).</summary>
        public static void AttachToAudience(AudienceCharacterBase audience)
        {
            if (audience == null) return;

            var bar = audience.AudienceCharacterCanvas != null
                ? audience.AudienceCharacterCanvas.MeterBarRect
                : null;

            Attach(audience.gameObject, KeyAudienceVibeBars,
                world: bar != null ? (Transform)bar : audience.transform,
                renderer: bar != null ? null : audience.SpriteRenderer);

            Attach(audience.gameObject, KeyStatusBlocked,
                world: audience.transform,
                renderer: audience.SpriteRenderer);
        }

        /// <summary>CharacterCanvas.TryCreateIcon, right after the icon spawns.
        /// Key routing mirrors TutorialController.OnStatusApplied: musician +
        /// TempShieldTurn → composure key; any other musician status → generic
        /// musician key; any audience status → generic audience key. The icon
        /// is destroyed on status clear → OnDisable unregisters cleanly.</summary>
        public static void AttachToStatusIcon(
            Component icon, CharacterStatusId id, bool musicianSide)
        {
            if (icon == null) return;

            string key = musicianSide
                ? (id == CharacterStatusId.TempShieldTurn
                    ? KeyStatusComposure
                    : KeyStatusMusician)
                : KeyStatusAudience;

            Attach(icon.gameObject, key, world: icon.transform);
        }

        /// <summary>DeckManager, after each successful BuildAndGetCard (draw,
        /// M4.5 guarantee, Dev spawn). No-op unless the card id is in the
        /// driver-registered map. Camera must be the HandCamera
        /// (HandController.Cam) — hand cards do not project with Camera.main.
        /// Card discard/exhaust destroys the GO → unregisters cleanly.</summary>
        public static void AttachToCard(CardBase card, Camera handCamera)
        {
            if (card == null || card.CardDefinition == null) return;
            if (!_cardKeysById.TryGetValue(card.CardDefinition.Id, out var key))
                return;

            Attach(card.gameObject, key,
                world: card.transform, camera: handCamera);
        }

        // ---- core ----

        private static void Attach(
            GameObject host, string key,
            Transform world = null, Renderer renderer = null,
            Camera camera = null, RectTransform mask = null)
        {
            if (host == null || string.IsNullOrWhiteSpace(key)) return;

            // Re-spawn tolerance: if the host already carries this key,
            // re-register it (last-enabled wins) instead of duplicating.
            var existing = host.GetComponents<TutorialHighlightTarget>();
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i] != null && existing[i].Key == key)
                {
                    TutorialHighlightRegistry.Register(existing[i]);
                    return;
                }
            }

            var t = host.AddComponent<TutorialHighlightTarget>();
            t.InitRuntime(key, mask, world, renderer, camera);
        }
    }
}