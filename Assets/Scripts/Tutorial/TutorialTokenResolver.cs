// Place at: Assets/Scripts/Tutorial/TutorialTokenResolver.cs
using ALWTTT.Managers;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [TUT-R2 / D8] Resolves the three approved copy tokens at DISPLAY time
    /// (TutorialOverlayView resolves each page as it copies it), so revisit-panel
    /// replays also resolve against current values. No rich-text, no authoring
    /// window (D8 scope limit) — plain string substitution of exactly:
    ///
    ///   {$loops_per_part}        ← GigManager.flow.JamRules.loopsPerPart
    ///   {$inspiration_per_loop}  ← PersistentGameplayData.InspirationPerLoop
    ///   {$audience_hp}           ← MAX MaxVibe across the CURRENT encounter's
    ///                              audience members at fire time (TUT-R1 §3
    ///                              proposed rule, ratified in TUT-R2: "hasta"
    ///                              in the copy absorbs the mixed encounter).
    ///
    /// Unresolvable tokens (no gig running, empty audience) degrade to "?" —
    /// never to an exception, never to the raw token.
    /// </summary>
    public static class TutorialTokenResolver
    {
        private const string TokLoops = "{$loops_per_part}";
        private const string TokInsp = "{$inspiration_per_loop}";
        private const string TokHp = "{$audience_hp}";

        public static string Resolve(string page)
        {
            if (string.IsNullOrEmpty(page)) return page;

            if (page.Contains(TokLoops))
                page = page.Replace(TokLoops, ResolveLoopsPerPart());
            if (page.Contains(TokInsp))
                page = page.Replace(TokInsp, ResolveInspirationPerLoop());
            if (page.Contains(TokHp))
                page = page.Replace(TokHp, ResolveAudienceHp());

            // [S10] {$card:<id>} -> the card's name in the game's language, in the card colour.
            if (page.Contains(TokCardPrefix))
                page = CardTokenRx.Replace(page, m => CardName(m.Groups["id"].Value));

            page = ColourMusicianNames(page);   // [S10]
            return page;
        }

        private const string TokCardPrefix = "{$card:";
        /// <summary>[S10] Colour of card names inside tutorial text (concepts are gold).</summary>
        public const string CardNameColor = "#2F7FD8";
        private static readonly System.Text.RegularExpressions.Regex CardTokenRx =
            new System.Text.RegularExpressions.Regex(@"\{\$card:(?<id>[A-Za-z0-9_]+)\}");

        /// <summary>[S10] Card ids named by {$card:id} tokens in a raw (unresolved) text.</summary>
        public static void CollectCardIds(string raw, System.Collections.Generic.List<string> into)
        {
            if (string.IsNullOrEmpty(raw) || into == null || !raw.Contains(TokCardPrefix)) return;
            foreach (System.Text.RegularExpressions.Match m in CardTokenRx.Matches(raw))
            {
                var id = m.Groups["id"].Value;
                if (!into.Contains(id)) into.Add(id);
            }
        }

        private static string CardName(string id)
        {
            string name = id;
            var g = ALWTTT.Tooltips.GameText.Glossary;
            if (g != null && g.TryGetCardName(id, out var n) && !string.IsNullOrWhiteSpace(n)) name = n;
            // [S10] Linked: hovering the name shows the card's tooltip (TutorialCardTooltip).
            return $"<link={ALWTTT.Tooltips.ConceptTagRenderer.CardLinkPrefix}{id}><b><color={CardNameColor}>{name}</color></b></link>";
        }

        /// <summary>[S10] Fourth colour: musicians (concepts gold, cards blue, combos orange).</summary>
        public const string MusicianNameColor = "#2E9B57";

        public static string MusicianTag(string name, string characterId = null) =>
            $"<b><color={MusicianNameColor}>{name}</color></b>{MusicianIcon(characterId)}";

        // [S10 / D18=A] The musician's face after the name, from the «musicos» sprite asset
        // (ALWTTT ▸ S10 ▸ Íconos de músicos en los textos). No asset yet = no icon.
        private static int _iconsAsset = -1;   // -1 unknown, 0 missing, 1 present
        public static string MusicianIcon(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return "";
            if (_iconsAsset < 0)
            {
                string path = (TMPro.TMP_Settings.defaultSpriteAssetPath ?? "Sprite Assets/") + "musicos";
                _iconsAsset = UnityEngine.Resources.Load<TMPro.TMP_SpriteAsset>(path) != null ? 1 : 0;
                UnityEngine.Debug.Log($"[S10] Íconos de músicos en el texto: {(_iconsAsset == 1 ? "sí" : "no (falta ejecutar la herramienta)")} ({path})");
            }
            return _iconsAsset == 1 ? $" <sprite=\"musicos\" name=\"mus_{characterId}\">" : "";
        }

        // Each band member's name, as a whole word outside tags, gets the musician colour.
        private static string ColourMusicianNames(string page)
        {
            var band = GigManager.Instance != null ? GigManager.Instance.CurrentMusicianCharacterList : null;
            if (band == null || string.IsNullOrEmpty(page)) return page;
            foreach (var m in band)
            {
                string n = m != null ? m.CharacterName : null;
                if (string.IsNullOrWhiteSpace(n)) continue;
                string id = m.MusicianCharacterData != null ? m.MusicianCharacterData.CharacterId : null;
                var rx = new System.Text.RegularExpressions.Regex(
                    @"(?<![\w<=/#])" + System.Text.RegularExpressions.Regex.Escape(n) + @"(?![\w>])");
                page = rx.Replace(page, MusicianTag(n, id));
            }
            return page;
        }

        private static string ResolveLoopsPerPart()
        {
            var gig = GigManager.Instance;
            var flow = gig != null ? gig.Flow : null; // [TUT-R2] new accessor
            return flow != null ? flow.JamRules.loopsPerPart.ToString() : "?";
        }

        private static string ResolveInspirationPerLoop()
        {
            var pd = GameManager.Instance != null
                ? GameManager.Instance.PersistentGameplayData
                : null;
            return pd != null ? pd.InspirationPerLoop.ToString() : "?";
        }

        private static string ResolveAudienceHp()
        {
            var gig = GigManager.Instance;
            var list = gig != null ? gig.CurrentAudienceCharacterList : null;
            if (list == null || list.Count == 0) return "?";

            int max = 0;
            bool any = false;
            for (int i = 0; i < list.Count; i++)
            {
                var a = list[i];
                if (a == null || a.Stats == null) continue;
                any = true;
                if (a.Stats.MaxVibe > max) max = a.Stats.MaxVibe;
            }
            return any ? max.ToString() : "?";
        }
    }

    /// <summary>
    /// [S10] Tooltip of a card named in tutorial text: what kind of card, who plays it, its
    /// modifiers and its Inspiration (playtest 26-nov). ES only, like the tutorial dialogs.
    /// </summary>
    public static class TutorialCardTooltip
    {
        private const string Icon = "<sprite=\"brillo\" index=0>";

        public static bool TryBuild(string id, out string header, out string body)
        {
            header = body = null;
            var def = Find(id);
            if (def == null) return false;

            var g = ALWTTT.Tooltips.GameText.Glossary;
            header = g != null && g.TryGetCardName(id, out var n) && !string.IsNullOrWhiteSpace(n) ? n : def.DisplayName;

            var sb = new System.Text.StringBuilder();
            sb.Append("<b>").Append(def.IsFinisher ? "Remate" : def.IsComposition ? "Composición" : "Habilidad").Append("</b>");
            string who = Musician(def);
            if (who != null) sb.Append(" · ").Append(who);

            // [S10 / D16] What the card does when played (Levántate: Cautivado al Alto; Guiño…).
            var fxs = def.Payload != null ? def.Payload.Effects : null;
            if (fxs != null)
                foreach (var fx in fxs)
                {
                    if (fx == null) continue;
                    string line = ALWTTT.Cards.Effects.CardEffectDescriptionBuilder.Build(fx);
                    if (!string.IsNullOrWhiteSpace(line)) sb.Append('\n').Append(line);
                }

            var comp = def.CompositionPayload;
            if (comp != null && comp.ModifierEffects != null)
                foreach (var fx in comp.ModifierEffects)
                {
                    if (fx == null) continue;
                    string line = ALWTTT.Cards.Effects.PartEffectDescriptionBuilder.Build(fx);
                    if (!string.IsNullOrWhiteSpace(line)) sb.Append('\n').Append(line);
                }
            if (def.InspirationGenerated > 0) sb.Append("\nGenera +").Append(def.InspirationGenerated).Append(' ').Append(Icon);
            if (def.InspirationCost > 0) sb.Append("\nCuesta ").Append(def.InspirationCost).Append(' ').Append(Icon);
            body = sb.ToString();
            return true;
        }

        private static string Musician(ALWTTT.Cards.CardDefinition def)
        {
            if (def.CanBePlayedByAnyMusician) return "cualquier músico";
            if (!def.RequiresFixedPerformer) return null;
            var band = GigManager.Instance != null ? GigManager.Instance.CurrentMusicianCharacterList : null;
            if (band != null)
                foreach (var m in band)
                    if (m != null && m.MusicianCharacterData != null &&
                        m.MusicianCharacterData.CharacterType == def.FixedPerformerType)
                        return TutorialTokenResolver.MusicianTag(m.CharacterName, m.MusicianCharacterData.CharacterId);
            return null;
        }

        private static ALWTTT.Cards.CardDefinition Find(string id)
        {
            var pd = GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;
            if (pd == null) return null;
            foreach (var list in new[] { pd.CurrentCompositionCards, pd.CurrentActionCards })
                if (list != null)
                    foreach (var c in list)
                        if (c != null && string.Equals(c.Id, id, System.StringComparison.OrdinalIgnoreCase)) return c;
            return null;
        }
    }
}
