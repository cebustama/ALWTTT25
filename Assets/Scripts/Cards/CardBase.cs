using ALWTTT.Cards;
using ALWTTT.Cards.Effects;
using ALWTTT.Characters;
using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Extentions;
using ALWTTT.Managers;
using ALWTTT.Status;
using ALWTTT.Tooltips;

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ALWTTT
{
    public class CardBase : MonoBehaviour,
        I2DTooltipTarget, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] protected Transform descriptionRoot;

        [Tooltip("Anchor Transform for card hover tooltips (keyword + status). " +
         "If unset, falls back to descriptionRoot for backward compatibility.")]
        [SerializeField] protected Transform tooltipAnchor;

        [SerializeField] protected Image cardImage;
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected Image passiveImage;
        [SerializeField] protected TextMeshProUGUI nameTextField;
        [SerializeField] protected TextMeshProUGUI descTextField;
        [SerializeField] protected TextMeshProUGUI inspirationCostTextField;
        [SerializeField] protected TextMeshProUGUI inspirationGenTextField;

        [Tooltip("[S5e-ext] Optional root for the gen badge (background circle + " +
                 "number). Hidden when the card generates 0 inspiration. If " +
                 "unassigned, only inspirationGenTextField's GameObject is toggled.")]
        [SerializeField] protected GameObject inspirationGenBadgeRoot;

        [Tooltip("[DEMO-FIXES-A / DF-COST0] Optional root for the COST badge " +
                 "(background circle + number). Hidden when InspirationCost == 0 " +
                 "(post-ECON-1 all starters cost 0 — the badge is noise). If " +
                 "unassigned, only inspirationCostTextField's GameObject is toggled. " +
                 "WIRE IN BOTH PREFABS (gameplay card + CardUI inventory — " +
                 "two-prefab recurrence vector, CURRENT_STATE §4).")]
        [SerializeField] protected GameObject inspirationCostBadgeRoot;

        [SerializeField] protected TextMeshProUGUI typeTextField;

        [Header("Type background [S5b / Item 1]")]
        [Tooltip("Shown when CardDefinition.IsAction. Toggled in SetCard.")]
        [SerializeField] private GameObject actionBackground;
        [Tooltip("Shown when CardDefinition.IsComposition. Toggled in SetCard.")]
        [SerializeField] private GameObject compositionBackground;

        [Header("Card face [CARTAS-ARTE-1]")]
        [Tooltip("Shown when CardDefinition.IsFinisher (instead of the composition/action frame). " +
                 "Built by ALWTTT ▸ CARTAS-ARTE-1 ▸ Montar la cara nueva.")]
        [SerializeField] private GameObject finisherBackground;
        [Tooltip("The windowed art (BackImage + Mask) used by actions and Finishers. Hidden on full-art compositions.")]
        [SerializeField] private GameObject windowArtRoot;
        [Tooltip("Full-card illustration for compositions (Matías's 611×974 template: the sprite carries its own window margin).")]
        [SerializeField] private Image fullArtImage;
        [Tooltip("Dark band behind name + description, only on full-art compositions (D6 bis = A).")]
        [SerializeField] private GameObject fullArtDescBand;
        [Tooltip("Matías's instrument word in the composition frame's top border. It names the TRACK the " +
                 "card plays, not the musician: Conito's instrument sounds as bass or guitar.")]
        [SerializeField] private Image trackLabelImage;
        [SerializeField] private Sprite labelVoz;
        [SerializeField] private Sprite labelBajo;
        [SerializeField] private Sprite labelGuitarra;
        [SerializeField] private Sprite labelOrgano;
        [SerializeField] private Sprite labelBateria;

        [Header("Owner icon [S5b / Item 4]")]
        [Tooltip("Performer/owner icon (MusicianCharacterData.CharacterIcon). " +
                 "Hidden when the card has no fixed performer or its musician isn't in the band.")]
        [SerializeField] private Image ownerIconImage;

        public CardDefinition CardDefinition { get; private set; }

        /// <summary>[S10] The frame shown now (action, composition or finisher): the card's outline.</summary>
        public RectTransform ActiveFrameRect
        {
            get
            {
                GameObject go = finisherBackground != null && finisherBackground.activeSelf ? finisherBackground
                    : compositionBackground != null && compositionBackground.activeSelf ? compositionBackground
                    : actionBackground;
                return go != null ? go.transform as RectTransform : null;
            }
        }
        public bool IsInactive { get; protected set; }
        public bool IsPlayable { get; protected set; } = true;
        public bool IsExhausted { get; protected set; }

        #region Encapsulation
        protected Transform CachedTransform { get; set; }
        protected WaitForEndOfFrame CachedWaitFrame { get; set; }
        #endregion

        #region Cache
        protected DeckManager DeckManager => DeckManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        #endregion

        #region Setup
        protected virtual void Awake()
        {
            CachedTransform = transform;
            CachedWaitFrame = new WaitForEndOfFrame();
        }

        public virtual void SetCard(CardDefinition targetProfile, bool isPlayable = true)
        {
            CardDefinition = targetProfile;
            IsPlayable = isPlayable;

            /*
            backgroundImage.color = 
                GameManager.GameplayData.GetCardTypeColor(CardData.CardType);*/

            cardImage.sprite = CardDefinition.CardSprite;
            // [CARD-FACE-2b / D-CF-25=B] The action card TYPE is hidden from the player for now. One switch
            // turns off face, hover panel and modal together. The field is emptied, not deactivated: its
            // GameObject may carry layout, and an empty TMP field costs nothing.
            typeTextField.text = CardDefinition.IsFinisher ?
                GameText.Format(GameTextTemplateIds.UiCardTypeFinisher)          // [ECON-2]
                : CardDefinition.IsComposition ?
                GameText.Format(GameTextTemplateIds.UiCardTypeComposition)
                : !CardFacePresentation.ShowCardType ? string.Empty
                : (CardFacePresentation.UseSymbols
                    ? GameSymbols.CardTypeGlyph(CardDefinition.CardType)                          // [CARD-FACE-2 / F-TXT4-2] symbol
                    : GameText.Format(GameTextTemplateIds.UiCardType(CardDefinition.CardType)));   // [D-CF-19=B] word
            nameTextField.text = GameText.CardName(CardDefinition);   // [TXT-4] glossary cardNames by Id; displayName is a developer label
            descTextField.text = CardDefinition.GetDescription();

            // [DEMO-FIXES-A / DF-COST0 / D-DF-5=A] Mirror of the S5e-ext
            // gen-badge hide. Symmetric toggle so pooled/reused cards recover.
            // [ECON-2] Coste y ★ efectivos: las reglas (F12 ▸ Inspiración) pueden pisarlos.
            int effCost = ALWTTT.Data.InspirationRules.EffectiveCost(CardDefinition);
            int effGen = CardDefinition.IsComposition
                ? ALWTTT.Data.InspirationRules.EffectiveGenerated(CardDefinition)
                : CardDefinition.InspirationGenerated;
            bool showCost = effCost > 0;
            if (inspirationCostBadgeRoot != null)
                inspirationCostBadgeRoot.SetActive(showCost);
            else if (inspirationCostTextField != null)
                inspirationCostTextField.gameObject.SetActive(showCost);
            if (inspirationCostTextField != null && showCost)
                inspirationCostTextField.text = effCost.ToString();

            // [S5e-ext] Post-D3 all content authors gen=0; hide the dead badge.
            // Symmetric toggle so pooled/reused hand cards recover if gen>0 returns.
            bool showGen = effGen > 0;
            if (inspirationGenBadgeRoot != null)
                inspirationGenBadgeRoot.SetActive(showGen);
            else if (inspirationGenTextField != null)
                inspirationGenTextField.gameObject.SetActive(showGen);
            if (inspirationGenTextField != null && showGen)
                inspirationGenTextField.text = effGen.ToString();

            ApplyTypeBackground(CardDefinition);   // [S5b / Item 1]
            ApplyFaceLayout(CardDefinition);       // [CARTAS-ARTE-1]
            ApplyOwnerIcon(CardDefinition);        // [S5b / Item 4]
        }

        // [CARTAS-ARTE-1] Matías's composition illustrations are re-exported on the frame's own
        // 612×948 canvas (Assets/Art/Cards/Marcos), with the drawing already under the frame's
        // window, so art and frame both stretch over the whole card and line up. Only sprites with
        // that shape go full-card; old window art (900×1334 or square) keeps the windowed layout,
        // so a card without new art never shows a tiny drawing floating in the middle.
        private const float FullArtAspect = 612f / 948f;
        private const float FullArtAspectTolerance = 0.02f;
        private static readonly HashSet<string> s_loggedFaces = new HashSet<string>();

        public static bool IsFullArtSprite(Sprite s)
        {
            if (s == null) return false;
            var r = s.rect;
            return r.height > 0f && Mathf.Abs(r.width / r.height - FullArtAspect) <= FullArtAspectTolerance;
        }

        // Face geometry, as fractions of the card (x, then y from the bottom). Measured on Matías's
        // three frames, which share one 612×948 canvas (Assets/Art/Cards/Marcos). If a frame is
        // redrawn with a different layout, these are the numbers to re-measure.
        private enum Face { Composition, Ability, Finisher }
        private static readonly Rect WindowArt = R(0.170f, 0.825f, 0.411f, 0.791f);  // Habilidad / neutral window
        private static readonly Rect CompWindow = R(0.150f, 0.819f, 0.186f, 0.892f); // composition window
        // [CARTAS-ARTE-1 / D8 = A] A thin strip right under the track word: the name, then one row
        // with only the modifiers the audience's tastes read (tempo, meter, tonality = mood). The
        // rest of the art stays free; the full effect text moves to the first hover panel.
        // Revised the same day (Claudio): the name goes DOWN, over the frame's redundant
        // «COMPOSICIÓN» box, on a plate of that box's own blue; the taste row sits in a thin strip
        // right above it, at the bottom edge of the art.
        private static readonly Rect CompName = R(0.142f, 0.822f, 0.112f, 0.181f);   // the word's box
        private static readonly Rect CompBand = R(0.150f, 0.819f, 0.181f, 0.226f);
        private static readonly Rect CompDesc = R(0.16f, 0.81f, 0.183f, 0.224f);
        private const float GenNumberY = 0.31f;   // "+N" centre: above the strip (0.226), low on the art
        private static readonly Color CompNamePlate = new Color(0.243f, 0.220f, 0.780f, 1f); // sampled from the box
        private static readonly Rect BoxName = R(0.15f, 0.85f, 0.285f, 0.355f);    // Habilidad text box, 1st line
        private static readonly Rect BoxDesc = R(0.15f, 0.85f, 0.110f, 0.285f);
        private static readonly Rect FinName = R(0.19f, 0.81f, 0.832f, 0.902f);    // neutral frame's title box
        private static readonly Rect FinDesc = R(0.15f, 0.85f, 0.110f, 0.355f);
        private static readonly Color FinNamePlate = new Color(0.93f, 0.96f, 1f, 1f);
        private static readonly Color FinNameText = new Color(0.08f, 0.06f, 0.20f, 1f);
        private static Rect R(float x0, float x1, float y0, float y1) => Rect.MinMaxRect(x0, y0, x1, y1);

        private static void Place(Transform t, Rect r)
        {
            if (!(t is RectTransform rt)) return;
            rt.anchorMin = r.min; rt.anchorMax = r.max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private void ApplyFaceLayout(CardDefinition def)
        {
            // Old prefab (builder not run): leave the face exactly as it was.
            if (fullArtImage == null || def == null) return;

            var face = def.IsFinisher ? Face.Finisher : def.IsComposition ? Face.Composition : Face.Ability;
            bool fullArt = face == Face.Composition && IsFullArtSprite(def.CardSprite);

            fullArtImage.gameObject.SetActive(fullArt);
            if (fullArt) fullArtImage.sprite = def.CardSprite;
            // The full art is the card's hover surface: without it only the name plate received
            // the pointer, so the tooltips opened only over the name (Claudio, 2026-11-17).
            fullArtImage.raycastTarget = true;

            // The big "+N" sits low on the art, just above the taste strip, not in the middle
            // (Claudio, 2026-11-17). Anchored at bottom-centre as a fraction of the card height.
            if (inspirationGenBadgeRoot != null && inspirationGenBadgeRoot.transform is RectTransform genRt)
            {
                float y = face == Face.Composition ? GenNumberY : 0.5f;
                genRt.anchorMin = genRt.anchorMax = new Vector2(0.5f, y);
            }
            if (windowArtRoot != null)
            {
                windowArtRoot.SetActive(!fullArt);
                Place(windowArtRoot.transform, face == Face.Composition ? CompWindow : WindowArt);
            }
            string tasteRow = face == Face.Composition ? TasteModifierRow(def) : string.Empty;
            if (face == Face.Composition && descTextField != null) descTextField.text = tasteRow;
            if (fullArtDescBand != null)
            {
                // The strip only exists when there is something the audience reads.
                fullArtDescBand.SetActive(face == Face.Composition && tasteRow.Length > 0);
                Place(fullArtDescBand.transform, CompBand);
            }

            var namePlate = nameTextField != null ? nameTextField.transform.parent : null;
            var descBox = descTextField != null ? descTextField.transform.parent : null;
            Place(namePlate, face == Face.Composition ? CompName : face == Face.Finisher ? FinName : BoxName);
            Place(descBox, face == Face.Composition ? CompDesc : face == Face.Finisher ? FinDesc : BoxDesc);
            var plateImg = namePlate != null ? namePlate.GetComponent<Image>() : null;
            if (plateImg != null)
            {
                // Finisher: the neutral frame's empty title box. Composition: covers the word.
                plateImg.enabled = face == Face.Finisher || face == Face.Composition;
                plateImg.sprite = null;
                plateImg.color = face == Face.Composition ? CompNamePlate : FinNamePlate;
            }
            if (nameTextField != null) nameTextField.color = face == Face.Finisher ? FinNameText : Color.white;

            Sprite label = face == Face.Composition ? TrackLabelFor(def) : null;
            if (trackLabelImage != null)
            {
                trackLabelImage.sprite = label;
                trackLabelImage.gameObject.SetActive(label != null);
            }

            if (s_loggedFaces.Add(def.Id ?? def.name))
            {
                string kind = face == Face.Finisher ? "Finisher" : face == Face.Composition ? "Composición" : "Habilidad";
                Debug.Log($"[CARTAS-ARTE-1] cara {def.Id}: {kind}, " +
                          $"{(fullArt ? "arte a toda la carta" : "arte en ventana")}, " +
                          $"rótulo {(label != null ? label.name : "-")}, " +
                          $"coste ✦{ALWTTT.Data.InspirationRules.EffectiveCost(def)}, " +
                          $"da +{(def.IsComposition ? ALWTTT.Data.InspirationRules.EffectiveGenerated(def) : def.InspirationGenerated)}");
            }
        }

        // True when the face shows the strip (name + taste row) instead of the effect text.
        private bool ShowsTasteRow => fullArtImage != null && CardDefinition != null
                                      && CardDefinition.IsComposition && !CardDefinition.IsFinisher;

        // Only the families an audience taste reads: tempo (fast/slow), meter, tonality (mood).
        // Instrument and key change do not touch any taste, so they live in the hover panels.
        // Same formatter as the modal (PartEffectDescriptionBuilder.Build), so hand and modal agree.
        private static string TasteModifierRow(CardDefinition def)
        {
            var cp = def?.CompositionPayload;
            if (cp?.ModifierEffects == null) return string.Empty;
            var parts = new List<string>();
            foreach (var fx in cp.ModifierEffects)
                if (fx is TempoEffect || fx is MeterEffect || fx is TonalityEffect)
                    parts.Add(PartEffectDescriptionBuilder.Build(fx));
            return string.Join("   ", parts);
        }

        // The word says which TRACK the card plays (Claudio, D6 bis): drums for rhythm, bass for the
        // bassline, voice for melody and vocal harmony, organ for chords — guitar when the chords or
        // the lead come from Conito, whose instrument sounds as either. Part cards get no word.
        private Sprite TrackLabelFor(CardDefinition def)
        {
            var cp = def.CompositionPayload;
            if (cp == null || cp.PrimaryKind != CardPrimaryKind.Track || cp.TrackAction == null) return null;
            bool conito = def.FixedPerformerType == MusicianCharacterType.Conito;
            switch (cp.TrackAction.role)
            {
                case MidiGenPlay.TrackRole.Rhythm: return labelBateria;
                case MidiGenPlay.TrackRole.Bassline: return labelBajo;
                case MidiGenPlay.TrackRole.Melody:
                case MidiGenPlay.TrackRole.Harmony: return labelVoz;
                case MidiGenPlay.TrackRole.Backing: return conito ? labelGuitarra : labelOrgano;
                case MidiGenPlay.TrackRole.Lead: return labelGuitarra;
                default: return null;
            }
        }

        // [S5b / Item 1] Unmistakable Action vs Composition frame. The text type label
        // (typeTextField) is preserved; this adds the background distinction playtesters
        // asked for. A card that is neither (no payload / non-card payload) shows neither
        // background -- the base card frame stands in (D-S5b-BG-NEITHER=A). Null-guarded
        // for the prefab rollout.
        private void ApplyTypeBackground(CardDefinition def)
        {
            bool isAction = def != null && def.IsAction;
            bool isComposition = def != null && def.IsComposition;
            // [CARTAS-ARTE-1] The Finisher gets its own frame. Without it wired, it keeps the
            // composition frame (previous behaviour).
            bool isFinisher = def != null && def.IsFinisher && finisherBackground != null;
            if (actionBackground != null) actionBackground.SetActive(isAction);
            if (compositionBackground != null) compositionBackground.SetActive(isComposition && !isFinisher);
            if (finisherBackground != null) finisherBackground.SetActive(isFinisher);
            // [CARTAS-ARTE-1] The frame covers the whole card: make it the hover surface too, so a
            // windowed card (Habilidad, Finisher) opens its tooltips anywhere, not only on the name.
            foreach (var bg in new[] { actionBackground, compositionBackground, finisherBackground })
                if (bg != null && bg.TryGetComponent<Image>(out var bgImg)) bgImg.raycastTarget = true;
        }

        // [S5b / Item 4 / D-S5b-ICON-RESOLVER=A] Resolve the owner icon from the current
        // band via GigManager. None / AnyMusician / generic / out-of-gig -> no icon (Image
        // hidden). Null-guarded for the prefab rollout.
        private static readonly bool s_showOwnerIcon = false;
        private void ApplyOwnerIcon(CardDefinition def)
        {
            if (ownerIconImage == null) return;

            // [CARTAS-ARTE-1] The track word on top now says whose card it is; the musician's
            // face on the card is off (Claudio, 2026-11-17). Flip to true to bring it back.
            if (!s_showOwnerIcon) { ownerIconImage.gameObject.SetActive(false); return; }

            Sprite icon = null;
            if (def != null && def.FixedPerformerType != MusicianCharacterType.None)
            {
                var gm = GigManager.Instance;
                if (gm != null) icon = gm.TryGetMusicianIcon(def.FixedPerformerType);
            }

            bool hasIcon = icon != null;
            ownerIconImage.sprite = icon;
            ownerIconImage.gameObject.SetActive(hasIcon);
        }
        #endregion

        public virtual void Use(CharacterBase bandCharacter, CharacterBase audienceCharacter,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters)
        {
            if (!IsPlayable) return;

            // AUDIO-SFX-FIX (D-SFXDEF=B): card SFX is opt-in by type. The default/legacy
            // Button value and explicit None play nothing here (Button is the UI-click type
            // used by ButtonSoundPlayer, never a card sound; unset cards default to Button).
            var sfxType = CardDefinition.AudioType;
            if (sfxType != AudioActionType.Button && sfxType != AudioActionType.None)
                AudioManager.Instance?.PlayOneShot(sfxType);

            if (CardDefinition.CardType == CardType.SFX)
            {
                GameManager.PersistentGameplayData.SongModifierCardsList.Add(CardDefinition);
                SpendInspiration(CardDefinition.InspirationCost);
                GenerateInspiration(CardDefinition.InspirationGenerated);
                DeckManager.OnCardPlayed(this);
            }
            else
            {
                StartCoroutine(CardUseRoutine(bandCharacter, audienceCharacter,
                    allAudienceCharacters, allBandCharacters));
            }
        }

        public virtual void UpdateDescription(MusicianBase musician)
        {
            if (ShowsTasteRow) { descTextField.text = TasteModifierRow(CardDefinition); return; } // [CARTAS-ARTE-1]
            if (musician != null)
            {
                descTextField.text = CardDefinition.GetDescription(musician.Stats);
            }
            else
            {
                descTextField.text = CardDefinition.GetDescription(null);
            }
        }

        #region Routines
        private IEnumerator CardUseRoutine(
            CharacterBase performer, CharacterBase target,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters)
        {
            Debug.Log($"<color=cyan> Playing card (coroutine)...</color>");

            SpendInspiration(CardDefinition.InspirationCost);
            GenerateInspiration(CardDefinition.InspirationGenerated);

            // Effects pipeline (CardEffectSpec via [SerializeReference])
            yield return ExecuteEffects(
                performer, target,
                allAudienceCharacters, allBandCharacters
            );

            DeckManager.OnCardPlayed(this);
        }

        private IEnumerator ExecuteEffects(
    CharacterBase performer,
    CharacterBase primaryTarget,
    List<AudienceCharacterBase> allAudienceCharacters,
    List<MusicianBase> allBandCharacters)
            => RunEffects(CardDefinition, performer, primaryTarget,
                allAudienceCharacters, allBandCharacters, skipBonusLoop: false);

        /// <summary>
        /// [ECON-2] El pipeline de efectos de siempre, sin necesitar la carta en la
        /// mano: los Finisher lo disparan al empezar el loop final, cuando su
        /// CardBase ya se descartó. El parámetro se llama CardDefinition a
        /// propósito: el cuerpo es el de ExecuteEffects, sin tocar.
        /// skipBonusLoop: el loop final del Finisher ya se concedió al jugarlo.
        /// </summary>
        public static IEnumerator RunEffects(
            CardDefinition CardDefinition,
            CharacterBase performer,
            CharacterBase primaryTarget,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters,
            bool skipBonusLoop)
        {
            var payload = CardDefinition != null ? CardDefinition.Payload : null;
            var effects = payload != null ? payload.Effects : null;

            if (effects == null || effects.Count == 0)
                yield break;

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null) continue;

                if (effect is ApplyStatusEffectSpec ase)
                {
                    if (ase.stacksDelta == 0) continue;

                    if (ase.delay > 0f)
                        yield return new WaitForSeconds(ase.delay);

                    if (ase.status == null)
                    {
                        Debug.LogError(
                            $"[CardBase] ApplyStatusEffectSpec has null StatusEffectSO. Card='{CardDefinition?.name}'.");
                        continue;
                    }

                    var targets = DetermineTargets(
                        performer,
                        primaryTarget,
                        allAudienceCharacters,
                        allBandCharacters,
                        ase.targetType);

                    for (int t = 0; t < targets.Count; t++)
                    {
                        var trg = targets[t];
                        if (trg == null) continue;

                        if (trg.Statuses == null)
                        {
                            Debug.LogWarning(
                                $"[CardBase] Target '{trg.name}' has no Statuses container. Card='{CardDefinition?.name}'.");
                            continue;
                        }

                        trg.Statuses.Apply(ase.status, ase.stacksDelta);

#if UNITY_EDITOR
                        Debug.Log(
                            $"[Effects] {performer?.name} applied {ase.stacksDelta}x " +
                            $"'{ase.status.StatusKey}' ({ase.status.DisplayName}, primitive={ase.status.EffectId}) " +
                            $"to {trg.name} via card '{CardDefinition?.DisplayName}'.");
#endif
                    }

                    continue;
                }


                // [R4 / D-R0-1=A] Reveal the target audience member's taste panel.
                // The spec carries no taste data: AudienceCharacterData owns the
                // preferences and AudienceCharacterCanvas owns presentation, so this
                // branch is a pure dispatch. Non-audience targets are an authoring
                // error and are logged, not silently dropped.
                if (effect is RevealPreferencesSpec reveal)
                {
                    var revealTargets = DetermineTargets(
                        performer,
                        primaryTarget,
                        allAudienceCharacters,
                        allBandCharacters,
                        reveal.targetType);

                    if (revealTargets == null || revealTargets.Count == 0)
                    {
                        Debug.LogWarning(
                            $"[CardBase] RevealPreferencesSpec resolved 0 targets. Card='{CardDefinition?.name}', targetType={reveal.targetType}.");
                        continue;
                    }

                    for (int t = 0; t < revealTargets.Count; t++)
                    {
                        var revealTarget = revealTargets[t];
                        if (revealTarget == null) continue;

                        var revealAudience = revealTarget as AudienceCharacterBase;
                        if (revealAudience == null)
                        {
                            Debug.LogError(
                                $"[CardBase] RevealPreferencesSpec target must be AudienceCharacterBase. Got='{revealTarget.GetType().Name}' ('{revealTarget.name}'). Card='{CardDefinition?.name}'.");
                            continue;
                        }

                        revealAudience.RevealPreferences();
                    }

                    continue;
                }

                if (effect is ModifyVibeSpec vibe)
                {
                    var targets = DetermineTargets(
                        performer,
                        primaryTarget,
                        allAudienceCharacters,
                        allBandCharacters,
                        vibe.targetType);

                    if (targets == null || targets.Count == 0)
                    {
                        Debug.LogWarning(
                            $"[CardBase] ModifyVibeSpec resolved 0 targets. Card='{CardDefinition?.name}', targetType={vibe.targetType}.");
                        continue;
                    }

                    // [JUICE-PW D1=A] Fan-out counter for AudienceVibeImpactEvent.
                    // Counts PUBLISHED events (valid audience targets), not raw
                    // list slots, so FanoutIndex==0 is a reliable play-once key.
                    int vibeFanout = 0;

                    for (int t = 0; t < targets.Count; t++)
                    {
                        var trg = targets[t];
                        if (trg == null) continue;

                        var audience = trg as AudienceCharacterBase;
                        if (audience == null || audience.Stats == null)
                        {
                            Debug.LogError(
                                $"[CardBase] ModifyVibeSpec target must be AudienceCharacterBase. Got='{trg.GetType().Name}' ('{trg.name}'). Card='{CardDefinition?.name}'.");
                            continue;
                        }

                        int baseDelta = vibe.amount;
                        int finalDelta = baseDelta;
                        int flowStacks = 0;

                        // M4.2 bifurcated Flow → Vibe:
                        // Action cards: flat bonus per stack.
                        // Composition cards: multiplier per stack.
                        var gm = GigManager.Instance;
                        if (baseDelta > 0 && gm != null)
                        {
                            bool isAction = CardDefinition != null && CardDefinition.IsAction;
                            if (isAction && gm.FlowActionFlatBonus)
                            {
                                // Action path: only the performer's Flow stacks.
                                int performerFlow = GetPerformerFlowStacks(performer);
                                if (performerFlow > 0)
                                {
                                    int flowBonus = performerFlow * gm.FlowActionVibeBonusPerStack;
                                    finalDelta = baseDelta + flowBonus;
                                    flowStacks = performerFlow; // for debug log
                                }
                            }
                            else if (!isAction)
                            {
                                // Composition path: whole band's Flow.
                                flowStacks = ComputeBandFlowStacks(allBandCharacters);
                                if (flowStacks > 0)
                                {
                                    float mult = 1f + flowStacks * gm.FlowVibeMultiplier;
                                    finalDelta = Mathf.RoundToInt(baseDelta * mult);
                                }
                            }
                        }

                        // Prefer the newer AudienceStats pathway when available.
                        // [B3] Route positive Vibe through ApplyIncomingVibe canonical
                        // path. Indifference (NegateIncomingPositive stacks > 0) gates
                        // the result to 0 applied. Negative-Vibe deltas would go through
                        // a different path; ModifyVibeSpec callers with negative amounts
                        // currently rely on AddVibe accepting negatives (it does via
                        // SetCurrentVibe clamping at 0). For B3, only positives route
                        // through the helper; negatives stay on legacy AddVibe.
                        int appliedVibe = finalDelta;
                        if (audience.AudienceStats != null)
                        {
                            if (finalDelta > 0)
                            {
                                appliedVibe = audience.AudienceStats.ApplyIncomingVibe(
                                    audience.Statuses, finalDelta);
                            }
                            else
                            {
                                audience.AudienceStats.AddVibe(finalDelta);
                            }
                        }
                        else
                        {
                            // [S5e] negative delta restores the enemy's resistance pool
                            int before = audience.Stats.CurrentVibe;
                            audience.Stats.SetCurrentVibe(before - finalDelta);
                        }

#if UNITY_EDITOR
                        bool blockedVibe = finalDelta > 0 && appliedVibe == 0;
                        if (blockedVibe)
                        {
                            Debug.Log(
                                $"<color=#888888>[Effects] {performer?.name} Vibe on " +
                                $"{audience.name}: BLOCKED (Indifference) " +
                                $"intended=+{finalDelta} applied=0 via card " +
                                $"'{CardDefinition?.DisplayName}'.</color>");
                        }
                        else if (flowStacks > 0)
                        {
                            bool isActionLog = CardDefinition != null && CardDefinition.IsAction;
                            Debug.Log(isActionLog
                                ? $"[Effects][Flow→Vibe:Flat] {performer?.name} Vibe on {audience.name}: " +
                                  $"baseΔ={baseDelta} flowStacks={flowStacks} finalΔ={finalDelta} " +
                                  $"via card '{CardDefinition?.DisplayName}'."
                                : $"[Effects][Flow→Vibe:Mult] {performer?.name} Vibe on {audience.name}: " +
                                  $"baseΔ={baseDelta} flowStacks={flowStacks} " +
                                  $"mult={1f + flowStacks * (gm?.FlowVibeMultiplier ?? 0f):F2} " +
                                  $"finalΔ={finalDelta} via card '{CardDefinition?.DisplayName}'.");
                        }
                        else
                        {
                            Debug.Log(
                                $"[Effects] {performer?.name} modified Vibe on {audience.name}: " +
                                $"Δ{finalDelta} via card '{CardDefinition?.DisplayName}'.");
                        }
#endif

                        // [JUICE-PW D1=A] Publish the per-target impact to the
                        // sensory bus AT RESOLUTION (gameplay only publishes;
                        // FT/SFX/anim derivation lives in the adapters). This
                        // runs strictly before DeckManager.OnCardPlayed at the
                        // end of CardUseRoutine, so it precedes the beat-8
                        // hold release keyed on CardPlayedEvent (task d).
                        var sensoryBus = ALWTTT.Sensory.SensoryEventBus.Instance;
                        if (sensoryBus != null)
                        {
                            int audienceIdx = allAudienceCharacters != null
                                ? allAudienceCharacters.IndexOf(audience)
                                : -1;
                            sensoryBus.Publish(new ALWTTT.Sensory.AudienceVibeImpactEvent(
                                audience,
                                audienceIdx,
                                audience.CharacterId,
                                performer,
                                CardDefinition,
                                baseDelta,
                                finalDelta,
                                appliedVibe,
                                vibeFanout,
                                targets.Count));
                            vibeFanout++;
                        }
                    }

                    continue;
                }

                if (effect is ModifyStressSpec stress)
                {
                    var targets = DetermineTargets(
                        performer,
                        primaryTarget,
                        allAudienceCharacters,
                        allBandCharacters,
                        stress.targetType);

                    if (targets == null || targets.Count == 0)
                    {
                        Debug.LogWarning(
                            $"[CardBase] ModifyStressSpec resolved 0 targets. Card='{CardDefinition?.name}', targetType={stress.targetType}.");
                        continue;
                    }

                    for (int t = 0; t < targets.Count; t++)
                    {
                        var trg = targets[t];
                        if (trg == null) continue;

                        var musician = trg as MusicianBase;
                        if (musician == null || musician.Stats == null)
                        {
                            Debug.LogError(
                                $"[CardBase] ModifyStressSpec target must be MusicianBase. Got='{trg.GetType().Name}' ('{trg.name}'). Card='{CardDefinition?.name}'.");
                            continue;
                        }

                        int delta = stress.amount;
                        if (delta == 0) continue;

                        if (delta > 0)
                        {
                            var (absorbed, applied) = musician.Stats.ApplyIncomingStressWithComposure(
                                musician.Statuses, delta);

#if UNITY_EDITOR
                            Debug.Log(
                                $"[Effects] {performer?.name} stress on {musician.name}: " +
                                $"incoming={delta} absorbed={absorbed} applied={applied} " +
                                $"via card '{CardDefinition?.DisplayName}'.");
#endif
                            continue;
                        }

                        // [S5e] negative delta heals: route through the canonical heal path
                        // Negative stress heals (restores fortitude).
                        {
                            int before = musician.Stats.CurrentStress;
                            musician.Stats.HealStress(-delta);

#if UNITY_EDITOR
                            Debug.Log(
                                $"[Effects] {performer?.name} modified Stress on {musician.name}: {before} -> {musician.Stats.CurrentStress} (Δ{delta}) via card '{CardDefinition?.DisplayName}'.");
#endif
                        }
                    }

                    continue;
                }

                if (effect is DrawCardsSpec draw)
                {
                    if (draw.count > 0)
                    {
                        ALWTTT.Managers.DeckManager.Instance?.DrawCards(draw.count);

#if UNITY_EDITOR
                        Debug.Log(
                            $"[Effects] {performer?.name} drew {draw.count} card(s) via card '{CardDefinition?.DisplayName}'.");
#endif
                    }
                    continue;
                }

                // [R5-d / D-R0-5=A] Bonus loop. The COST is deliberately NOT
                // here: it was checked and spent upstream, before the play was
                // committed (D-R5-26=A). A cost inside a spec would run after
                // inspiration and the ECON-1 budget were already burned, so the
                // card could be played with an empty resource and then fail
                // silently. By the time this runs the grant has been pre-checked
                // at the play path, so a failure here is a genuine anomaly and
                // is logged as one rather than swallowed.
                if (effect is GrantBonusLoopSpec bonusLoop)
                {
                    if (skipBonusLoop) continue;   // [ECON-2]
                    var gm = ALWTTT.Managers.GigManager.Instance;
                    if (gm == null || !gm.TryGrantBonusLoop(bonusLoop.soloOverBonusLoop))
                    {
                        Debug.LogWarning(
                            $"[CardBase] GrantBonusLoopSpec did not apply. " +
                            $"Card='{CardDefinition?.name}'. The play was already " +
                            "committed — check the pre-commit gate at the play path.");
                    }
                    continue;
                }

                if (effect is AddInspirationPerLoopSpec)
                {
                    // [DF-INSPLOOP] Consumed at track-binding time
                    // (EvalPerLoopInsp via TrackEntry.sourceCardDefinition),
                    // not by the CardBase pipeline. No-op here by design.
                    continue;
                }

                Debug.LogWarning(
                    $"[CardBase] Unhandled CardEffectSpec type '{effect.GetType().Name}'. Card='{CardDefinition?.name}'.");
            }
        }

        private static int GetPerformerFlowStacks(CharacterBase performer)
        {
            if (performer == null) return 0;
            var musician = performer as MusicianBase;
            if (musician?.Statuses == null) return 0;
            return musician.Statuses.GetStacks(CharacterStatusId.DamageUpFlat);
        }

        private static int ComputeBandFlowStacks(List<MusicianBase> allBandCharacters)
        {
            if (allBandCharacters == null || allBandCharacters.Count == 0)
                return 0;

            int total = 0;
            for (int i = 0; i < allBandCharacters.Count; i++)
            {
                var m = allBandCharacters[i];
                if (m == null || m.Statuses == null) continue;

                // MVP mapping: Flow == DamageUpFlat.
                total += Mathf.Max(0, m.Statuses.GetStacks(CharacterStatusId.DamageUpFlat));
            }
            return total;
        }

        private static List<CharacterBase> DetermineTargets(
            CharacterBase performer,
            CharacterBase targetCharacter,
            List<AudienceCharacterBase> allAudienceCharacters,
            List<MusicianBase> allBandCharacters,
            ActionTargetType targetType)
        {
            var targetList = new List<CharacterBase>();

            switch (targetType)
            {
                case ActionTargetType.Self:
                    if (performer != null)
                        targetList.Add(performer);
                    break;

                case ActionTargetType.AudienceCharacter:
                case ActionTargetType.Musician:
                    if (targetCharacter != null)
                        targetList.Add(targetCharacter);
                    break;

                case ActionTargetType.AllAudienceCharacters:
                    foreach (var enemyBase in allAudienceCharacters)
                        if (enemyBase != null && !enemyBase.IsBlocked)
                            targetList.Add(enemyBase);
                    break;

                case ActionTargetType.AllMusicians:
                    foreach (var allyBase in allBandCharacters)
                        if (allyBase != null)
                            targetList.Add(allyBase);
                    break;

                case ActionTargetType.RandomAudienceCharacter:
                    if (allAudienceCharacters != null && allAudienceCharacters.Count > 0)
                        targetList.Add(allAudienceCharacters.RandomItem());
                    break;

                case ActionTargetType.RandomMusician:
                    if (allBandCharacters != null && allBandCharacters.Count > 0)
                        targetList.Add(allBandCharacters.RandomItem());
                    break;

                // [S10 / D16=A] From a card: the first tall member still to convince
                // (Levántate). None left = the effect fizzles.
                case ActionTargetType.AudienceTall:
                    if (allAudienceCharacters != null)
                        foreach (var a in allAudienceCharacters)
                            if (a != null && a.IsTall && a.Stats != null && !a.Stats.IsConvinced)
                            {
                                targetList.Add(a);
                                Debug.Log($"[S10] Efecto al Alto: '{a.CharacterId}'");
                                break;
                            }
                    break;

                default:
                    Debug.LogWarning($"[CardBase] Unhandled ActionTargetType for Effects: {targetType}");
                    break;
            }

            return targetList;
        }


        protected virtual IEnumerator DiscardRoutine(bool destroy = true)
        {
            var timer = 0f;
            transform.SetParent(DeckManager.HandController.DiscardTransform);

            var startPos = CachedTransform.localPosition;
            var endPos = Vector3.zero;

            var startScale = CachedTransform.localScale;
            var endScale = Vector3.zero;

            var startRot = CachedTransform.localRotation;
            var endRot = Quaternion.Euler(
                Random.value * 360,
                Random.value * 360,
                Random.value * 360
            );

            float discardDuration = DeckManager.HandController.DiscardDuration;

            while (timer < discardDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / discardDuration);

                CachedTransform.localPosition = Vector3.Lerp(startPos, endPos, t);
                CachedTransform.localRotation = Quaternion.Lerp(startRot, endRot, t);
                CachedTransform.localScale = Vector3.Lerp(startScale, endScale, t);

                yield return CachedWaitFrame;
            }

            if (destroy)
                Destroy(gameObject);
        }
        #endregion

        public virtual void SetInactiveMaterialState(bool isInactive)
        {
            if (!IsPlayable) return;
            if (isInactive == this.IsInactive) return; // No change

            IsInactive = isInactive;
            passiveImage.gameObject.SetActive(isInactive);
        }

        /// <summary>
        /// [CARD-UX-1 / D4=A] Red "unplayable" overlay. Semantic wrapper over the
        /// existing passiveImage mechanism — same guards apply (no-op on
        /// IsPlayable=false instances like the inventory/minicard CardUI, and on
        /// no-change). Driven EXCLUSIVELY by HandController from
        /// GigManager.EvaluateCardPlayability; never compute playability here.
        /// </summary>
        public virtual void SetUnplayableOverlay(bool unplayable)
            => SetInactiveMaterialState(unplayable);

        public virtual void Discard()
        {
            // TODO: Necessary?
            if (IsExhausted) return;
            if (!IsPlayable) return;
            DeckManager.OnCardDiscarded(this);
            StartCoroutine(DiscardRoutine());
        }

        public virtual void Exhaust(bool destroy = true)
        {
            // TODO: Necessary?
            if (IsExhausted) return;
            if (!IsPlayable) return;
        }

        protected virtual void SpendInspiration(int value)
        {
            if (value <= 0) return;

            var gm = GigManager.Instance;
            if (gm != null)
            {
                // MB4: session-aware routing. Negative delta = spend.
                gm.AdjustInspiration(-value);
                return;
            }

            // Defense-in-depth fallback for code paths where no GigManager exists
            // (shouldn't fire in normal gig play).
            var pd = GameManager.PersistentGameplayData;
            if (pd != null) pd.CurrentInspiration -= value;
        }

        protected virtual void GenerateInspiration(int value)
        {
            if (value <= 0) return;

            var gm = GigManager.Instance;
            if (gm != null)
            {
                // MB4: session-aware routing. Positive delta = generate.
                gm.AdjustInspiration(+value);
                return;
            }

            var pd = GameManager.PersistentGameplayData;
            if (pd != null) pd.CurrentInspiration += value;
        }

        #region Pointer Events

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltipInfo();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            HideTooltipInfo();
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                HideTooltipInfo();
                var ctrl = UI.CardDetailViewController.Instance;
                if (ctrl != null)
                    ctrl.Toggle(CardDefinition);
                return;
            }

            // Left-click: existing behavior.
            HideTooltipInfo();
        }

        // [CARD-FACE-3 / O-CF2b-2] uGUI delivers the RELEASE to the object that took the PRESS, even
        // when the pointer has already left it. A right press hid the stack and opened the modal; its
        // release then re-showed the stack with the pointer over the modal's dim background, so no
        // OnPointerExit ever hid it again (observed 2026-09-22, R1). Canvas order was NOT the cause
        // (R2: tooltip canvas 10 below detail canvas 100, both root Overlay canvases).
        // Only a LEFT release restores the hover (R4 baseline: press hides, release re-shows), and
        // never while the modal is open.
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            var ctrl = UI.CardDetailViewController.Instance;
            if (ctrl != null && ctrl.IsVisible) return;
            ShowTooltipInfo();
        }
        #endregion

        #region Tooltip
        protected virtual void ShowTooltipInfo()
        {
            if (!descriptionRoot) return;
            var tooltipManager = TooltipManager.Instance;
            if (tooltipManager == null) return;

            // [CARTAS-ARTE-1] The stack sits on the card's right edge and grows down, instead of
            // following the cursor over the art and the text (Claudio, 2026-11-17). No anchor
            // (old prefab without the new face) => cursor-follow, as before.
            // The hand canvas is World Space and Card3D gives it the HAND camera; TooltipController
            // only reads canvas.worldCamera for Screen Space - Camera and otherwise projects with
            // Camera.main, which put the panel far away or off screen. Pass the canvas's camera.
            var anchor = GetTooltipAnchor();
            var tipCanvas = anchor != null ? anchor.GetComponentInParent<Canvas>() : null;
            var tipCam = tipCanvas != null ? tipCanvas.rootCanvas.worldCamera : null;
            void Tip(string body, string header, TooltipKind kind = TooltipKind.Default) =>
                tooltipManager.ShowTooltip(body, header, anchor, tipCam, 0f,
                    TooltipAnchorOffset, growDown: anchor != null, kind: kind);

            // [GIG-UI-1] Why a card with the red overlay cannot be played, as the FIRST panel of
            // the stack. Same single source as the overlay (EvaluateCardPlayability, CARD-UX-1):
            // display only, nothing re-derived here. No GigManager (inventory, ship) => no panel.
            var gigForReason = GigManager.Instance;
            if (gigForReason != null && CardDefinition != null)
            {
                var reason = gigForReason.EvaluateCardPlayability(CardDefinition);
                if (reason != UnplayableReason.None)
                {
                    // [GIG-UI-1] Budget names WHICH card the musician already used (Claudio,
                    // 2026-11-02): "this kind of play" did not say action vs composition.
                    string reasonId = reason == UnplayableReason.Budget
                        ? (CardDefinition.IsComposition
                            ? GameTextTemplateIds.UiUnplayableBudgetComposition
                            : GameTextTemplateIds.UiUnplayableBudgetAction)
                        : GameTextTemplateIds.UiUnplayable(reason);
                    Tip(
                        GameText.Format(reasonId),
                        GameText.Format(GameTextTemplateIds.UiUnplayableHeader),
                        kind: TooltipKind.Warning);
                }
            }


            // 0) Track-role tooltip [CARD-FACE-1 / D-CF-13=C]. The face shows the role as a GLYPH
            //    with no word (density); its meaning arrives here, exactly as a keyword's does.
            //    Read straight from the glossary by derived id (role_<trackrole>): ConceptTooltipResolver
            //    keys by StatusKey and keyword name, and a TrackRole is neither, so adding a case
            //    there would be a change to the tooltip pipeline (out of scope, D-CF-2).
            //    Miss policy is GameText's: the header shows the raw id, never nothing.
            if (CardDefinition.HasPayload
                && CardDefinition.Payload is CompositionCardPayload compPayload
                && compPayload.PrimaryKind == CardPrimaryKind.Track
                && compPayload.TrackAction != null)
            {
                string roleConceptId = GameSymbols.RoleConcept(compPayload.TrackAction.role);
                Tip(
                    GameText.ConceptDescription(roleConceptId),
                    CardFacePresentation.HeaderWithGlyph(                                    // [CARD-FACE-2b / D-CF-26=A]
                        GameSymbols.Role(compPayload.TrackAction.role), GameText.ConceptName(roleConceptId)));
            }

            // 0a) [CARD-FACE-2] Card-type glyph (action cards) and part-action glyph (Part cards): the two
            //     enums CARD-FACE-1 left as words (F-TXT4-2). Same mechanism as the role: glyph on the
            //     face, meaning here, read by derived concept id, not through ConceptTooltipResolver (D-CF-2).
            if (!CardDefinition.IsComposition && CardFacePresentation.ShowCardType)   // [CARD-FACE-2b / D-CF-25=B]
            {
                string typeConceptId = GameSymbols.CardTypeConcept(CardDefinition.CardType);
                Tip(
                    GameText.ConceptDescription(typeConceptId),
                    CardFacePresentation.HeaderWithGlyph(
                        GameSymbols.CardTypeGlyph(CardDefinition.CardType), GameText.ConceptName(typeConceptId)));
            }
            if (CardDefinition.HasPayload
                && CardDefinition.Payload is CompositionCardPayload partPayload
                && partPayload.PrimaryKind == CardPrimaryKind.Part
                && partPayload.PartAction != null)
            {
                string paConceptId = GameSymbols.PartActionConcept(partPayload.PartAction.action);
                Tip(
                    GameText.ConceptDescription(paConceptId),
                    CardFacePresentation.HeaderWithGlyph(
                        GameSymbols.PartActionGlyph(partPayload.PartAction.action), GameText.ConceptName(paConceptId)));
            }

            // 0b) Modifier-family tooltips [CARD-FACE-1 / D-CF-14=B]. The face now shows a glyph per
            //     modifier; this is where those glyphs acquire meaning, same mechanism as the role.
            //     DEDUPED BY FAMILY on purpose: on the face, repetition is information (two tempo
            //     effects = two glyphs = two modifiers); in the stack it would be two identical
            //     panels, so the family is shown once. Unmapped subclasses contribute no panel.
            if (CardDefinition.HasPayload
                && CardDefinition.Payload is CompositionCardPayload modPayload
                && modPayload.ModifierEffects != null)
            {
                var seenFamilies = new HashSet<string>();
                foreach (var fx in modPayload.ModifierEffects)
                {
                    if (fx == null) continue;
                    // [CARTAS-ARTE-1] The track word on top already says the instrument family;
                    // the "Instrument" panel only repeated it (Claudio, 2026-11-17).
                    if (fx is InstrumentEffect) continue;
                    string conceptId = PartEffectDescriptionBuilder.FamilyConceptId(fx);
                    if (conceptId == null || !seenFamilies.Add(conceptId)) continue;
                    // [CARTAS-ARTE-1] First line = what THIS card does on that axis ("Rápido",
                    // "Triste"), so the player matches it to the audience's tastes; then the
                    // family's general meaning. Same value formatter as the face and the modal.
                    var values = new List<string>();
                    foreach (var other in modPayload.ModifierEffects)
                        if (other != null && PartEffectDescriptionBuilder.FamilyConceptId(other) == conceptId)
                            values.Add(PartEffectDescriptionBuilder.ValueOnly(other));
                    string thisCard = values.Count > 0 ? $"<b>{string.Join(" · ", values)}</b>\n" : string.Empty;
                    Tip(
                        thisCard + GameText.ConceptDescription(conceptId),
                        CardFacePresentation.HeaderWithGlyph(                                // [CARD-FACE-2b]
                            PartEffectDescriptionBuilder.FamilySpriteName(fx) is string famSprite
                                ? GameSymbols.Tag(famSprite) : null,
                            GameText.ConceptName(conceptId)));
                }
            }

            // 1) Keyword tooltips — text from the active ConceptGlossarySO, keyed by the enum
            //    name (TXT-3 / D-TXT3-0=B). SpecialKeywordData no longer exists. A keyword the
            //    glossary lacks shows its raw id and warns once: visible, never silent.
            if (CardDefinition.Keywords != null)
            {
                foreach (var kw in CardDefinition.Keywords)
                {
                    ConceptTooltipResolver.TryGetKeywordText(kw, out var kwHeader, out var kwBody);
                    Tip(kwBody,
                        CardFacePresentation.HeaderWithGlyph(GameSymbols.Keyword(kw), kwHeader));   // [CARD-FACE-2b]
                }
            }

            // 1b) [S10 / D16] A COMPOSITION with an on-play effect (Levántate: Cautivado al Alto).
            //     Its full-art face shows no effect text, so the effect gets its own box here.
            if (CardDefinition.IsComposition && CardDefinition.HasPayload && CardDefinition.Payload != null
                && CardDefinition.Payload.Effects != null && CardDefinition.Payload.Effects.Count > 0)
            {
                var lines = new List<string>();
                foreach (var fx in CardDefinition.Payload.Effects)
                {
                    if (fx == null) continue;
                    string line = CardEffectDescriptionBuilder.Build(fx);
                    if (!string.IsNullOrWhiteSpace(line)) lines.Add(line);
                }
                if (lines.Count > 0)
                    Tip(string.Join("\n", lines), GameText.Format(GameTextTemplateIds.UiCardOnPlay));
            }

            // 2) Status-effect tooltips — unique StatusEffectSOs from payload.Effects.
            if (CardDefinition.HasPayload && CardDefinition.Payload != null)
            {
                var effects = CardDefinition.Payload.Effects;
                if (effects != null)
                {
                    var seen = new HashSet<StatusEffectSO>();
                    for (int i = 0; i < effects.Count; i++)
                    {
                        if (effects[i] is ApplyStatusEffectSpec ase
                            && ase.status != null
                            && seen.Add(ase.status))
                        {
                            // [TXT-3] Name + body from the glossary by StatusKey. DisplayName is
                            // a developer label since TXT-3 and never reaches the player here.
                            ConceptTooltipResolver.TryGetStatusText(ase.status, out var header, out var body);
                            Tip(body,
                                CardFacePresentation.HeaderWithGlyph(                        // [CARD-FACE-2b]
                                    GameSymbols.StatusGlyph(ase.status.StatusKey), header));
                        }
                    }
                }
            }
        }

        private static readonly Vector2 TooltipAnchorOffset = new Vector2(8f, 0f);
        private RectTransform _tooltipAnchor;

        // A zero-size child at the card's top-right corner, under the same root the frames fill
        // (FullArt's parent). It moves and scales with the card, so the hover zoom is followed.
        private Transform GetTooltipAnchor()
        {
            if (_tooltipAnchor != null) return _tooltipAnchor;
            if (fullArtImage == null || fullArtImage.transform.parent == null) return null;
            var go = new GameObject("TooltipAnchor", typeof(RectTransform));
            _tooltipAnchor = (RectTransform)go.transform;
            _tooltipAnchor.SetParent(fullArtImage.transform.parent, false);
            _tooltipAnchor.anchorMin = _tooltipAnchor.anchorMax = new Vector2(1f, 1f);
            _tooltipAnchor.pivot = new Vector2(0.5f, 0.5f);
            _tooltipAnchor.sizeDelta = Vector2.zero;
            _tooltipAnchor.anchoredPosition = Vector2.zero;
            return _tooltipAnchor;
        }

        protected virtual void HideTooltipInfo()
        {
            HideTooltipInfo(TooltipManager.Instance);
        }

        public void ShowTooltipInfo(TooltipManager tooltipManager,
            string content, string header = "",
            Transform tooltipStaticTransform = null, Camera cam = null, float delayShow = 0)
        {
            if (!descriptionRoot) return;
            if (CardDefinition.Keywords.Count == 0) return;

            tooltipManager.ShowTooltip(
                content, header, tooltipStaticTransform, cam, delayShow);
        }

        public void HideTooltipInfo(TooltipManager tooltipManager)
        {
            tooltipManager.HideTooltip();
        }
        #endregion
    }
}