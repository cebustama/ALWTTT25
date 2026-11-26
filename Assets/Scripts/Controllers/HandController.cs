using ALWTTT.Cards;
using ALWTTT.Cards.Effects;
using ALWTTT.Characters;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using ALWTTT.Sensory;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace ALWTTT
{
    public enum CardDropZone { None, CurrentPart, NextPart }

    public class HandController : MonoBehaviour
    {
        private const string DebugTag = "<color=green>HandController:</color>";

        // ---- [LOG-1 / D-LOG-3=B] Log gating ------------------------------
        // This class shipped entirely ungated and fires on every draw, drag
        // and discard, which is a large share of the console during a gig.
        // Every Debug.Log here was mechanically rewritten to LogV; the message
        // text is byte-identical, only the gate is new. Debug.LogWarning and
        // Debug.LogError were deliberately NOT touched.
        //
        // Fail-OPEN when there is no GigManager: this switch is a gig dev
        // setting, and outside a gig (deck editor, menus) it does not apply --
        // going silent there would hide diagnostics the toggle never governed.
        private static bool VerboseLogs
        {
            get
            {
                // [SONG-PANEL-1 / F-SP1-5] Ask GigManager the question instead of reaching for
                // the dev ScriptableObject: DevSettings is compiled out of player builds
                // (DBG-C1) and this gate is not dev-only. Behaviour is unchanged - the
                // property fails open on a null dev asset exactly as the old null check did.
                var gm = ALWTTT.Managers.GigManager.Instance;
                if (gm == null) return true;         // no gig: unchanged behaviour
                return gm.VerboseLogsEnabled;
            }
        }

        private static void LogV(string msg)
        {
            if (VerboseLogs) Debug.Log(msg);
        }


        // ---- [R5-g] Log de tier MAESTRO ----------------------------------
        // LogV cuelga de UseLogs && LogVerbose. Una DENEGACIÓN no es
        // bookkeeping: es la explicación de por qué una carta no hizo nada, y
        // la doctrina de LOG-1 (ver GigManager.TryConsumePlay, ~l.2124, y
        // SSoT_Dev_Mode §19) la mantiene en el tier normal. La ruta de
        // composición ya lo hace así (GigManager.Log en TryPlayCompositionCard);
        // esta es la mitad de acción de esa misma regla.
        //
        // Fail-OPEN sin GigManager, idéntico a VerboseLogs y por la misma razón.
        private static void LogGate(string msg)
        {
            // [SONG-PANEL-1 / F-SP1-5] See VerboseLogs above. Same fail-open semantics.
            var gm = ALWTTT.Managers.GigManager.Instance;
            if (gm == null || gm.LogsEnabled) Debug.Log(msg);
        }

        [Header("Card Settings")]
        [SerializeField] private bool cardUprightWhenSelected = true;
        [SerializeField] private bool cardTilt = true;
        [SerializeField] private float discardDuration = 0.2f;

        [Header("Card Sizing")]
        [SerializeField] private float cardBaseScale = 1.0f;
        [Tooltip("Multiplier on cardBaseScale when hovered/dragged. 1.25 = 25% bigger.")]
        [SerializeField] private float cardHoverScaleMultiplier = 1.25f;
        [SerializeField] private float scaleLerpSpeed = 12f;
        [Tooltip("How far the card pops up on hover (relative mode). Scaled by cardBaseScale.")]
        [SerializeField] private float hoverLiftY = 0.3f;
        [Tooltip("When enabled, hovered cards rise to a fixed Y height instead of a relative offset.")]
        [SerializeField] private bool useFixedHoverHeight = false;
        [Tooltip("Fixed local-space Y (relative to handPosition) that hovered cards arrive at.")]
        [SerializeField] private float fixedHoverHeight = 0.5f;

        /// <summary>
        /// Derived from cardBaseScale. Multiplied into curve width and hand bounds
        /// so that larger cards don't overlap.
        /// </summary>
        private float HandScaleFactor => cardBaseScale;

        [Header("Hand Settings")]
        [SerializeField][Range(0, 5)] private float selectionSpacing = 1;
        [SerializeField] private Vector3 curveStart = new Vector3(2f, -0.7f, 0);
        [SerializeField] private Vector3 curveEnd = new Vector3(-2f, -0.7f, 0);
        [Tooltip("Local-space offset for the entire hand. Use this instead of moving the GameObject.")]
        [SerializeField] private Vector3 handPosition = Vector3.zero;
        [Tooltip("Extra arc height for the Bézier midpoint. Positive = more curvature upward.")]
        [SerializeField] private float curveHeight = 0f;
        [SerializeField] private Vector2 handOffset = new Vector2(0, -0.3f);
        [SerializeField] private Vector2 handSize = new Vector2(9, 1.7f);

        [Header("Tuck (S9): the hand drops when the pointer leaves the bottom band")]
        [Tooltip("Lower the hand while the pointer is away from it, so it does not hide the stage.")]
        [SerializeField] private bool tuckWhenAway = true;
        [Tooltip("How far the hand drops (world units along the hand's up).")]
        [SerializeField, Min(0f)] private float tuckDrop = 0.9f;
        [Tooltip("Height of the band above the hand bounds that still counts as 'near the hand'.")]
        [SerializeField, Min(0f)] private float tuckZoneExtraTop = 0.4f;
        [Tooltip("Extra width at each side of the hand bounds that counts as 'near the hand'.")]
        [SerializeField, Min(0f)] private float tuckZoneExtraSide = 0.5f;
        [SerializeField, Min(0.1f)] private float tuckSpeed = 10f;

        [Header("Context")]
        [SerializeField] private bool useGigContext = true;
        [SerializeField] private ShipInteriorManager shipInteriorManager;

        [Header("Drop Zones (optional)")]
        [SerializeField] private bool enableDropZones = true;
        // Local-space rects (center + size) in the HandController transform space
        [SerializeField] private Vector2 currentZoneCenter = new Vector2(-2.5f, 0.0f);
        [SerializeField] private Vector2 currentZoneSize = new Vector2(5.0f, 3.0f);
        [SerializeField] private Vector2 nextZoneCenter = new Vector2(2.5f, 0.0f);
        [SerializeField] private Vector2 nextZoneSize = new Vector2(5.0f, 3.0f);

        [Header("References")]
        [SerializeField] private Transform drawTransform;
        [SerializeField] private Transform discardTransform;
        [SerializeField] private Transform exhaustTransform;
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask targetLayer;

        [Header("Debug")]
        [SerializeField] private List<CardBase> hand; // cards currently in hand

        private Camera mainCam;

        private Plane plane; // world XY plane, used for mouse position raycasts
        private Vector3 a, b, c; // Used for shaping hand into curve
        private Vector3 _handBasePos; // World-space hand origin (transform + handPosition)
        private float _tuck;          // [S9] 0 = up, 1 = fully dropped
        private bool _tuckAway;       // [S9] last state, for the log
        private int selected = -1; // Card index that is nearest to mouse
        private int dragged = -1; // Card index that is held by mouse (inside of hand)
        // Card that is held by mouse (when outside of hand), ready to be used.
        private CardBase heldCard;
        private Vector3 heldCardOffset;

        private Rect handBounds;
        private bool mouseInsideHand;

        private Vector3 mouseWorldPos;
        private Vector2 prevMousePos;
        private Vector2 mousePosDelta;

        private Vector2 heldCardTilt;
        private Vector2 force;

        // Drop Zone logic
        private Rect _currentZoneRectLocal;
        private Rect _nextZoneRectLocal;

        private CardDropZone _hoverZone = CardDropZone.None;
        private CardDropZone _prevHoverZone = CardDropZone.None;
        private CardDropZone _pendingDropZone = CardDropZone.None;

        private Func<MusicianCharacterType, MusicianBase> _resolveTargetByType;

        #region Cache

        private GameManager GameManager => GameManager.Instance;
        private GigManager GigManager => GigManager.Instance;

        #endregion

        #region Encapsulation

        public float DiscardDuration => discardDuration;
        public Transform DrawTransform => drawTransform;
        public Transform DiscardTransform => discardTransform;
        public Transform ExhaustTransform => exhaustTransform;
        public Camera Cam => cam;
        public List<CardBase> Hand => hand;

        /// <summary>[S4b] A card is out of the hand, following the mouse (targeting).
        /// Hover views stand down so they never fight the card's target outline.</summary>
        public bool IsHoldingCard => heldCard != null;

        #endregion

        public bool IsDraggingActive { get; private set; } = true;
        private bool showDebugGizmos = true;
        private bool updateHierarchyOrder = false;
        private bool _hasLoggedScaleInit = false; // M1.9 temp log flag

        #region Setup
        private void Awake()
        {
            mainCam = Camera.main;
        }

        private void Start()
        {
            InitHand();
        }

        private void InitHand()
        {
            var sf = HandScaleFactor;

            var scaledHandSize = new Vector2(handSize.x * sf, handSize.y);
            // Shift handBounds by handPosition so mouse-inside-hand detection follows the offset
            var boundsCenter = handOffset + new Vector2(handPosition.x, handPosition.y);
            handBounds = new Rect((boundsCenter - scaledHandSize / 2), scaledHandSize);

            prevMousePos = Input.mousePosition;

            _currentZoneRectLocal =
                new Rect(currentZoneCenter - currentZoneSize * 0.5f, currentZoneSize);

            _nextZoneRectLocal =
                new Rect(nextZoneCenter - nextZoneSize * 0.5f, nextZoneSize);

            // Compute initial curve points
            UpdateCurvePoints();

            // M1.9 temp log
            LogV($"{DebugTag} [M1.9] InitHand — cardBaseScale={cardBaseScale} " +
                $"hoverMult={cardHoverScaleMultiplier} scaleFactor={sf} " +
                $"scaledHandSize={scaledHandSize}");
        }

        /// <summary>
        /// Recomputes Bézier control points and raycast plane from current transform position.
        /// Called every frame so the curve follows the GameObject if moved.
        /// handPosition shifts the entire hand in local space.
        /// curveHeight raises the Bézier midpoint for more/less arc curvature.
        /// </summary>
        private void UpdateCurvePoints()
        {
            var sf = HandScaleFactor;
            var scaledStart = new Vector3(curveStart.x * sf, curveStart.y, curveStart.z);
            var scaledEnd = new Vector3(curveEnd.x * sf, curveEnd.y, curveEnd.z);

            // Base position = transform origin + local handPosition offset
            _handBasePos = transform.TransformPoint(handPosition)
                           - transform.up * (tuckDrop * _tuck);   // [S9] tuck

            a = _handBasePos + transform.TransformDirection(scaledStart);
            b = _handBasePos + transform.up * curveHeight;
            c = _handBasePos + transform.TransformDirection(scaledEnd);

            plane = new Plane(-Vector3.forward, _handBasePos);
        }
        #endregion

        // [S10 / D9] Hand animation clock: real time while paused (the Space pause shows hover).
        private static float HandDt => ALWTTT.UI.PauseGate.IsPaused ? Time.unscaledDeltaTime : Time.deltaTime;

        private void Update()
        {
            // [LEGIB-1a] No input this frame → no card preview on the Big Num.
            if (!IsDraggingActive || (ALWTTT.UI.PauseGate.IsPaused && !ALWTTT.UI.PauseGate.InspectOnly))
            {
                if (useGigContext && GigManager != null) GigManager.SetCardPreview(null, null);
            }

            if (!IsDraggingActive) return;


            // [PAUSE-1 / D12=C] Pause is its own gate, ANDed with IsDraggingActive (which the
            // tutorial modal and SetHandVisible already share). timeScale = 0 does not stop
            // Update or Input, so without this line cards stay playable behind the menu.
            // [S10 / D9] The Space pause (InspectOnly) keeps hover and preview alive.
            if (ALWTTT.UI.PauseGate.IsPaused && !ALWTTT.UI.PauseGate.InspectOnly) return;

            UpdateTuck();   // [S9] before the curve, so this frame's cards use it

            // Recompute curve every frame so it follows the GameObject if moved (M1.9)
            UpdateCurvePoints();

            var mousePos =
                HandleMouseInput(out var count, out var sqrDistance, out var mouseButton);

            HandleCardsInHand(count, mouseButton, sqrDistance);

            HandleDraggedCardInsideHand(mouseButton, count);

            HandleDraggedCardOutsideHand(mouseButton, mousePos);

            UpdateCardPreview(mousePos);   // [LEGIB-1a]
        }

        // [LEGIB-1a] Index of the card the pointer is on (hover lift or drag inside
        // the hand), -1 if none. Written by HandleCardsInHand every frame.
        private int _previewHandIndex = -1;

        /// <summary>
        /// [LEGIB-1a] Tells GigManager which card the player is looking at: the held
        /// card if any, else the hovered one. The target is resolved exactly like
        /// TryPlayInGig does for a composition drop (fixed performer → musician under
        /// the pointer while held → selected musician), so the preview scores the
        /// same track the drop would create.
        /// </summary>
        private void UpdateCardPreview(Vector2 mousePos)
        {
            if (!useGigContext || GigManager == null) return;

            CardBase card = heldCard != null ? heldCard
                : (_previewHandIndex >= 0 && _previewHandIndex < hand.Count
                    ? hand[_previewHandIndex] : null);
            var def = card != null ? card.CardDefinition : null;

            MusicianBase target = null;
            if (def != null && def.IsComposition)
            {
                if (def.RequiresFixedPerformer && _resolveTargetByType != null)
                    target = _resolveTargetByType(def.FixedPerformerType);
                if (target == null && heldCard != null)
                    target = IsOverMusician(mousePos);
                if (target == null)
                    target = GigManager.SelectedMusician as MusicianBase;
            }

            GigManager.SetCardPreview(def, target);
        }

        public void EnableDragging() => IsDraggingActive = true;
        public void DisableDragging() => IsDraggingActive = false;


        /// <summary>
        /// [PAUSE-1] Return a card held OUTSIDE the hand back into it and end any drag.
        /// Mirrors the "card has gone back into hand" branch of HandleDraggedCardOutsideHand,
        /// minus the resumed drag (dragged = -1): while paused the mouse-up is never read, so a
        /// held card would otherwise stay glued to the cursor and drop on the next click.
        /// </summary>
        public void ReleaseHeldCardToHand()
        {
            dragged = -1;
            heldCardOffset = Vector3.zero;
            if (heldCard == null) return;

            heldCard.UpdateDescription(null);
            AddCardToHand(heldCard, selected);
            selected = -1;
            heldCard = null;

            if (useGigContext && GigManager != null) GigManager.DeactivateCardHighlights();
        }

        /// <summary>
        /// Visually adds a card to the hand. Optional param to insert it at a given index.
        /// </summary>
        public void AddCardToHand(CardBase card, int index = -1)
        {
            LogV($"{DebugTag} Adding card {card.CardDefinition.DisplayName} to hand");

            // Set initial scale immediately so cards don't pop in at prefab size (M1.9)
            card.transform.localScale = Vector3.one * cardBaseScale;

            if (index < 0)
            {
                // Add to end
                hand.Add(card);
                index = hand.Count - 1;
            }
            else
            {
                // Insert at index
                hand.Insert(index, card);
            }
        }

        public void SetTargetResolver(Func<MusicianCharacterType, MusicianBase> resolver)
        {
            _resolveTargetByType = resolver;
        }

        private Vector2 HandleMouseInput(
            out int count, out float sqrDistance, out bool mouseButton
        )
        {
            Vector2 mousePos = Input.mousePosition;

            // Allows mouse to go outside game window but keeps cards within window
            mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width);
            mousePos.y = Mathf.Clamp(mousePos.y, 0, Screen.height);

            // TODO: Card Tilt if enabled
            if (cardTilt) TiltCard(mousePos);

            GetMouseWorldPosition(mousePos);

            // Get distance to current selected card
            // (for comparing against other cards later, to find closest)
            GetDistanceToCurrentSelectedCard(out count, out sqrDistance);

            // Check if mouse is inside hand bounds
            CheckMouseInsideHandBounds(out mouseButton);

            return mousePos;
        }

        private void HandleCardsInHand(int count, bool mouseButton, float sqrDistance)
        {
            _previewHandIndex = -1;   // [LEGIB-1a]
            for (int i = 0; i < count; i++)
            {
                var card = hand[i];
                var cardTransform = card.transform;

                // If not enough groove, set inactive visual
                /*card.SetInactiveMaterialState(
                    GameManager.PersistentGameplayData.CurrentGroove < 
                    card.CardData.GrooveCost
                );*/

                // [CARD-UX-1] Unplayable red overlay — single source of truth.
                // Replaces the legacy groove-check pattern above. Advisory display
                // only: enforcement stays in the play paths, which read the SAME
                // underlying gates via GigManager.EvaluateCardPlayability.
                if (useGigContext && GigManager != null && card.CardDefinition != null)
                {
                    card.SetUnplayableOverlay(
                        GigManager.EvaluateCardPlayability(card.CardDefinition)
                            != UnplayableReason.None);
                }

                var noCardHeld = heldCard == null; // Whether a card is "held" (outside of hand)
                var onSelectedCard = noCardHeld && selected == i;
                var onDraggedCard = noCardHeld && dragged == i;

                // Offset that shifts cards to the sides of selected one (fanning effect)
                float selectOffset = 0;
                if (noCardHeld)
                {
                    selectOffset = 0.02f * HandScaleFactor * // scaled fanning factor (M1.9)
                        Mathf.Clamp01(
                            1 - Mathf.Abs(Mathf.Abs(i - selected) - 1) / (float)count * 3) *
                        Mathf.Sign(i - selected);
                }

                // Position along Curve
                var t = (i + 0.5f) / count + selectOffset * selectionSpacing;
                var p = GetCurvePoint(a, b, c, t);

                // Mouse interaction
                var d = (p - mouseWorldPos).sqrMagnitude;
                var mouseCloseToCard = d < 0.5f * cardBaseScale * cardBaseScale; // M1.9: scaled threshold
                var mouseHoveringOnSelected =
                    onSelectedCard && mouseCloseToCard && mouseInsideHand;
                if (mouseHoveringOnSelected || onDraggedCard) _previewHandIndex = i;   // [LEGIB-1a]

                // Card Position & Rotation
                var cardUp = GetCurveNormal(a, b, c, t);
                // Hover lift: either relative offset or fixed Y target
                Vector3 cardPos;
                if (mouseHoveringOnSelected)
                {
                    if (useFixedHoverHeight)
                    {
                        // Fixed mode: card arrives at handBase.y + fixedHoverHeight
                        cardPos = p;
                        cardPos.y = _handBasePos.y + fixedHoverHeight;
                    }
                    else
                    {
                        // Relative mode: pop up along world Y by hoverLiftY * cardBaseScale
                        cardPos = p + Vector3.up * (hoverLiftY * cardBaseScale);
                    }
                }
                else
                {
                    cardPos = p;
                }
                var cardForward = Vector3.forward;

                // Sorting Order (z-position closer to camera).
                if (mouseHoveringOnSelected || onDraggedCard)
                {
                    // When selected bring card to front
                    if (cardUprightWhenSelected) cardUp = Vector3.up;
                    cardPos.z = transform.position.z - 0.2f;
                }
                else
                {
                    cardPos.z = transform.position.z + t * 0.5f;
                }

                // Smoothly rotates card to face forward with the correct "up" orientation.
                cardTransform.rotation = Quaternion.RotateTowards(
                    cardTransform.rotation,
                    Quaternion.LookRotation(cardForward, cardUp),
                    80f * HandDt
                );

                // Handle Start Dragging
                if (mouseHoveringOnSelected)
                {
                    var mouseButtonDown = Input.GetMouseButtonDown(0) && !ALWTTT.UI.PauseGate.IsPaused;
                    // [TUT-R2 / D4] Directive gate: beat 3 allows composition
                    // drags only; beat 5 blocks all drags. Modal gate (global
                    // IsDraggingActive) is unchanged and orthogonal.
                    if (mouseButtonDown &&
                        !ALWTTT.Tutorial.TutorialInputGate.BlocksCardDrag(
                            hand[i] != null ? hand[i].CardDefinition : null))
                    {
                        dragged = i;
                        // keeps the relative position so the card doesn’t "jump" to the cursor.
                        heldCardOffset = cardTransform.position - mouseWorldPos;
                        heldCardOffset.z = -0.1f;
                    }
                }

                // Handle Card Position
                if (onDraggedCard && mouseButton)
                {
                    // Held by mouse / dragging
                    cardPos = mouseWorldPos + heldCardOffset;
                    cardTransform.position = cardPos;
                }
                else
                {
                    // animates back to its curve position
                    cardPos =
                        Vector3.MoveTowards(cardTransform.position, cardPos, 16f * HandDt);
                    cardTransform.position = cardPos;
                }

                // Card Scale Lerp (M1.9)
                {
                    var isHighlighted = mouseHoveringOnSelected || onDraggedCard;
                    var targetScale = isHighlighted
                        ? cardBaseScale * cardHoverScaleMultiplier
                        : cardBaseScale;
                    var targetVec = Vector3.one * targetScale;
                    cardTransform.localScale = Vector3.MoveTowards(
                        cardTransform.localScale, targetVec,
                        scaleLerpSpeed * HandDt);

                    // M1.9 temp log — fires once per session on first hover
                    if (isHighlighted && !_hasLoggedScaleInit)
                    {
                        _hasLoggedScaleInit = true;
                        LogV($"{DebugTag} [M1.9] First hover scale: " +
                            $"card={card.CardDefinition.DisplayName} " +
                            $"target={targetScale:F3} current={cardTransform.localScale.x:F3}");
                    }
                }

                // Get Selected Card
                if (GameManager.PersistentGameplayData.CanSelectCards)
                {
                    if (d < sqrDistance)
                    {
                        sqrDistance = d;
                        selected = i;
                    }
                }
                else
                {
                    selected = -1;
                    dragged = -1;
                }

                // Debug Gizmos
                if (showDebugGizmos)
                {
                    var c = new Color(0, 0, 0, 0.2f);
                    if (i == selected)
                    {
                        c = Color.red;
                        if (sqrDistance > 2) c = Color.blue;
                    }

                    Debug.DrawLine(p, mouseWorldPos, c);
                }
            }
        }

        private void HandleDraggedCardInsideHand(bool mouseButton, int count)
        {
            if (!mouseButton)
            {
                // Stop dragging
                heldCardOffset = Vector3.zero;
                dragged = -1;
            }

            if (dragged != -1)
            {
                var card = hand[dragged];
                if (mouseButton && !mouseInsideHand)
                {
                    // Card is outside of the hand, so is considered "held" ready to be used
                    heldCard = card;
                    // Remove from hand, so that cards in hand fill the hole that the card left
                    RemoveCardFromHand(dragged);
                    count--;
                    dragged = -1;
                }
            }

            if (heldCard == null &&
                mouseButton && dragged != -1 &&
                selected != -1 &&
                dragged != selected)
            {
                // Move dragged card
                MoveCardToIndex(dragged, selected);
                dragged = selected;
            }
        }

        private void HandleDraggedCardOutsideHand(bool mouseButton, Vector2 mousePos)
        {
            if (heldCard != null)
            {
                var cardTransform = heldCard.transform;
                var cardUp = Vector3.up;
                var cardPos = mouseWorldPos + heldCardOffset;
                var cardForward = Vector3.forward;
                if (cardTilt && mouseButton)
                {
                    cardForward -= new Vector3(heldCardTilt.x, heldCardTilt.y, 0);
                }

                // Bring card to front
                cardPos.z = transform.position.z - 0.2f;

                // Smooth Rotation and Position Update
                cardTransform.rotation = Quaternion.RotateTowards(cardTransform.rotation,
                    Quaternion.LookRotation(cardForward, cardUp), 80f * HandDt);
                cardTransform.position = cardPos;

                // Held card stays at hover scale (M1.9)
                var heldTargetScale = Vector3.one * (cardBaseScale * cardHoverScaleMultiplier);
                cardTransform.localScale = Vector3.MoveTowards(
                    cardTransform.localScale, heldTargetScale,
                    scaleLerpSpeed * HandDt);

                // Contextual highlights based on actions and target types
                //if (useGigContext && GigManager != null)
                //    GigManager.HighlightCardTarget(heldCard.CardData.CardActionDataList[0].ActionTargetType);

                var musician = IsOverMusician(mousePos);
                if (musician != null)
                {
                    //LogV(musician.MusicianCharacterData.CharacterName);
                }

                heldCard.UpdateDescription(musician);

                // [S10] Conito and the low audience sit inside the hand band: a targeted card
                // over them used to snap back to the hand before the drop (playtest 25-nov).
                if (!GameManager.PersistentGameplayData.CanSelectCards ||
                    (mouseInsideHand && !HeldOverTargetCharacter(mousePos)))
                {
                    heldCard.UpdateDescription(null);
                    //  || sqrDistance <= 2
                    // Card has gone back into hand
                    AddCardToHand(heldCard, selected);
                    dragged = selected;
                    selected = -1;
                    heldCard = null;

                    // Contextual highlights
                    if (useGigContext && GigManager != null) GigManager.DeactivateCardHighlights();

                    return;
                }

                // Update zone hover state (when dragging outside hand)
                var localPoint = transform.InverseTransformPoint(mouseWorldPos);
                _hoverZone = GetDropZoneForLocalPoint(localPoint);
                if (_hoverZone != _prevHoverZone)
                {
                    /*
                    // ENTER / EXIT logs
                    if (_prevHoverZone != CardDropZone.None)
                        LogV($"{DebugTag} Exited zone: {_prevHoverZone}");

                    if (_hoverZone != CardDropZone.None)
                        LogV($"{DebugTag} Entered zone: {_hoverZone}");
                    */

                    _prevHoverZone = _hoverZone;
                }

                var mouseButtonUp = Input.GetMouseButtonUp(0);
                if (mouseButtonUp)
                {
                    if (_hoverZone != CardDropZone.None)
                    {
                        _pendingDropZone = _hoverZone;
                        LogV($"<color=yellow>{DebugTag} DROPPED on zone: " +
                            $"{_pendingDropZone} " +
                            $"(card='{heldCard.CardDefinition.DisplayName}')</color>");
                    }

                    PlayCard(mousePos);
                }
            }
        }

        private void PlayCard(Vector2 mousePos)
        {
            LogV($"{DebugTag} Playing card...");

            // Turn off Gig highlights if they were active
            if (useGigContext && GigManager != null)
                GigManager.DeactivateCardHighlights();

            bool backToHand = true;
            bool played = false;

            // Global guard: if cards cannot be selected, just return to hand
            if (!GameManager.PersistentGameplayData.CanSelectCards)
            {
                ReturnHeldToHand();
                return;
            }

            // Route by context
            var zoneUsed = _pendingDropZone;
            if (useGigContext)
            {
                played = TryPlayInGig(mousePos, zoneUsed);
            }
            else // Ship / Rehearsal context
            {
                played = TryPlayInShip(mousePos, zoneUsed);
            }

            // Handle piles / return to hand
            if (played)
            {
                backToHand = false;

                // Send the card to the appropriate pile (discard/exhaust).
                //
                // Only Composition cards rely on HandController for the discard
                // transition. Composition cards bypass CardBase.Use() entirely
                // (they route through GigManager.TryPlayCompositionCard).
                //
                // Action cards (SFX or otherwise) handle their own discard inside
                // CardBase.Use():
                //   - SFX type: synchronous OnCardPlayed at CardBase.Use line 93.
                //   - Non-SFX: deferred OnCardPlayed at CardUseRoutine line 131,
                //     called AFTER ExecuteEffects yields so effects resolve before
                //     the card is destroyed by DiscardRoutine.
                //
                // Calling OnCardPlayed here for action cards produced a double-
                // discard (each played card added two entries to DiscardPile and
                // removed two entries from HandPile, surfacing as "card duplication"
                // post-reshuffle). Bug fix: M4.6F-1, 2026-05-07.
                bool isComposition =
                    heldCard != null
                    && heldCard.CardDefinition != null
                    && heldCard.CardDefinition.IsComposition;

                if (DeckManager.Instance != null && isComposition)
                    DeckManager.Instance.OnCardPlayed(heldCard);
            }

            if (backToHand)
            {
                LogV($"{DebugTag} <color=red>Card couldn't be played.</color>");
                AddCardToHand(heldCard, selected);
            }

            heldCard = null;
        }

        private bool TryPlayInGig(Vector2 mousePos, CardDropZone zoneUsed)
        {
            var data = heldCard.CardDefinition;
            if (data == null) return false;

            // ─────────────────────────────────────────────
            // 1) COMPOSITION CARDS → live composition pipeline
            // ─────────────────────────────────────────────
            if (data.IsComposition)
            {
                // [S10] Tutorial, song 2: one composition per loop (each new musician adds Capas).
                if (ALWTTT.Tutorial.TutorialScriptedDrawQueue.OneCompositionPerLoop &&
                    ALWTTT.Tutorial.TutorialScriptedDrawQueue.CompositionUsedThisLoop && !data.IsFinisher)
                {
                    ALWTTT.Managers.GigManager.ReportPlayDenied(PlayDenyReason.Other,
                        "En esta canción, una composición por loop: la siguiente, en el próximo loop.", data, null);
                    Debug.Log($"[S10][CARTAS] no se jugó {ALWTTT.Managers.DeckManager.CardNames(new[] { data })}: " +
                              "ya entró una composición en este loop (tutorial, canción 2)");
                    return false;
                }

                MusicianBase target = null;

                // 1a) If the card has a fixed musician type, resolve that FIRST and ignore hover.
                if (data.RequiresFixedPerformer && _resolveTargetByType != null)
                {
                    target = _resolveTargetByType(data.FixedPerformerType);

                    if (target != null)
                        LogV($"{DebugTag} [Gig] Fixed-target card -> " +
                            $"{target.MusicianCharacterData.CharacterName} " +
                            $"({data.FixedPerformerType}).");
                    else
                        LogV($"{DebugTag} [Gig] Fixed-target card but resolver returned null " +
                            $"for {data.FixedPerformerType}.");
                }

                // 1b) Otherwise (or if fixed-target failed), use hover → selected fallback
                if (target == null)
                {
                    var hovered = IsOverMusician(mousePos);
                    if (hovered != null)
                    {
                        target = hovered;
                        LogV($"{DebugTag} [Gig] Hover-target -> " +
                            $"{target.MusicianCharacterData.CharacterName}.");
                    }
                    else
                    {
                        target = GigManager.SelectedMusician as MusicianBase;
                        LogV($"{DebugTag} [Gig] Selected/Default target -> " +
                            $"{(target != null ? target.MusicianCharacterData.CharacterName : "null")}.");
                    }
                }

                // 1c) If the card REQUIRES a musician and still none, abort.
                if (data.RequiresMusicianTarget && target == null)
                {
                    LogV($"{DebugTag} [Gig] " +
                        $"Card requires musician target but none resolved.");
                    return false;
                }

                // 1d) Route to Gig composition session (same as in ship)
                return GigManager.TryPlayCompositionCard(heldCard, target, zoneUsed);
            }

            // ─────────────────────────────────────────────
            // 2) ACTION CARDS → timing + legacy gig Use()
            // ─────────────────────────────────────────────
            if (data.IsAction)
            {
                // 2a) Timing gate (song playing / between songs)
                if (!GigManager.CanPlayActionCard(data))
                {
                    GigManager.ReportPlayDenied(PlayDenyReason.Timing,
                        "Not now — wait for your turn.", data, null);

                    LogV($"{DebugTag} [Gig] Cannot play action card " +
                              $"'{data.DisplayName}' in current timing. Returning to hand.");
                    return false;
                }

                // [B2 / #4] 2a.5) Inspiration cost check. Action cards spend inspiration
                // out of the same session pool as composition cards. Outside an active
                // composition session, only free cards (cost==0) may play.
                int actionCost = Math.Max(0, data.InspirationCost);
                if (actionCost > 0)
                {
                    var session = GigManager.CompositionSession;
                    if (session == null || !session.CanAffordInspiration(actionCost))
                    {
                        session?.FlashInspirationDenied();

                        // [TUT-REDESIGN-B] The flash is the visual; publish only.
                        GigManager.ReportPlayDenied(PlayDenyReason.InspirationCost,
                            "Not enough Inspiration.", data, null, showMessage: false);

                        LogV($"{DebugTag} [Gig] Action card '{data.DisplayName}' " +
                            $"cost={actionCost} but session={(session == null ? "null" : "short")}. Denied.");
                        return false;
                    }
                }

                // 2a.6) [R5-d] Bonus-loop precondition. Checked BEFORE any spend
                // so a card that cannot extend anything never burns its
                // resource. CanPlayActionCard (2a) is domain-wide and returns
                // true in the between-songs action window; this gate is
                // card-specific and closes exactly that hole.
                if (GigManager != null && GigManager.CardGrantsBonusLoop(data)
                    && !GigManager.CanGrantBonusLoop())
                {
                    GigManager.ReportPlayDenied(PlayDenyReason.BonusLoopPrecondition,
                        "Nothing running to extend right now.", data, null);

                    LogV($"{DebugTag} [Gig][R5-d] '{data.DisplayName}' needs a running " +
                         "part with a bonus loop available. Returning to hand.");
                    return false;
                }

                // NOTE: for now we IGNORE the old groove checks so we don’t depend
                // on the legacy gig state machine / groove economy.
                // If you want them back later, re-enable:
                //
                // if (!GameManager.PersistentGameplayData.CanUseCards) return false;
                // if (GameManager.PersistentGameplayData.CurrentGroove < data.GrooveCost) return false;

                // 2b) Resolve targets
                var mainRay = mainCam.ScreenPointToRay(mousePos);
                CharacterBase targetCharacter = null;

                // Caster: prefer card's FixedPerformerType. Fall back to list[0] if not set.
                CharacterBase bandCharacter = null;
                if (data.RequiresFixedPerformer && _resolveTargetByType != null)
                {
                    bandCharacter = _resolveTargetByType(data.FixedPerformerType);
                    if (bandCharacter != null)
                        LogV($"{DebugTag} [Gig] Action performer resolved by FixedPerformerType ({data.FixedPerformerType}).");
                }
                if (bandCharacter == null)
                {
                    bandCharacter = GigManager.SelectedMusician;
                    LogV($"{DebugTag} [Gig] Action performer fallback to SelectedMusician.");
                }

                // If card requires a target, we must raycast and validate it.
                // If it does NOT require a target, we pass null targetCharacter.
                if (data.RequiresTargetSelection)
                {
                    if (!TryResolveCardTarget(mainRay, data, out targetCharacter))
                        return false;
                }
                else
                {
                    targetCharacter = null;
                }

                // SFX is still allowed without a target (and CardBase will handle it).
                // (If you want SFX to require a target in the future, remove this.)
                if (data.CardType == CardType.SFX)
                {
                    targetCharacter = null;
                }

                // 2b.5) [R5-g / D1=B] Coste de recurso — COMPROBACIÓN, no cobro.
                // Va aquí, y no en 2a.6-bis como decía el contrato, porque este
                // es el primer punto donde el pagador existe (2b lo resuelve) y
                // el último donde todavía no se ha consumido nada.
                //
                // Por qué DELANTE del presupuesto: el hook de generación de
                // Voltage de R5-b vive dentro de TryConsumePlay (GigManager
                // ~l.2131), así que comprobar después del presupuesto compara
                // contra un saldo que el jugador aún no tenía al decidir. Con
                // 2 Voltage y coste 3, la comparación pasaba a 3 >= 3 y la carta
                // se colaba. El saldo FINAL no cambia con este arreglo — el +1 y
                // el -N ocurren igual, y eso es lo que el comentario viejo
                // llamaba NET-NEUTRAL; lo que cambia es QUÉ JUGADAS SON LEGALES.
                //
                // Espeja exactamente la ruta de composición
                // (GigManager.TryPlayCompositionCard: CanConsumePlay →
                // CanPayResourceCost → animación → sesión → cobros en éxito).
                // Efecto lateral deseado: una denegación de recurso ya no quema
                // la jugada de ECON-1.
                if (bandCharacter is MusicianBase resourceChecker &&
                    !GigManager.CanPayResourceCost(data, resourceChecker))
                {
                    GigManager.ReportPlayDenied(PlayDenyReason.ResourceCost,
                        "Not enough resources for this card.", data, resourceChecker);

                    LogGate($"{DebugTag} [Gig][R5-g] '{data.DisplayName}' denied — " +
                        $"{resourceChecker.CharacterName} cannot pay " +
                        $"{data.ResourceCostAmount} '{data.ResourceCostStatusKey}'. " +
                        "Returning to hand.");
                    return false;
                }

                // [ECON-1 / T4] Per-turn play budget — consumed LAST, once
                // nothing else in this pipeline can fail (timing 2a,
                // inspiration 2a.5, target resolution 2b, resource 2b.5 all
                // passed) and
                // BEFORE the one-shot animation, so a denied play neither
                // animates nor fires. D-ECON-3=A: AnyMusician cards bill the
                // performer resolved above (fixed → SelectedMusician
                // fallback). Denied → return false → card back to hand.
                if (bandCharacter is MusicianBase budgetPayer &&
                    !GigManager.TryConsumePlay(budgetPayer, isComposition: false))
                {
                    GigManager.ReportPlayDenied(PlayDenyReason.EconBudget,
                        "No plays left this loop.", data, budgetPayer);

                    LogV($"{DebugTag} [Gig][ECON-1] Action play denied — " +
                        $"{budgetPayer.CharacterName} has no action plays left " +
                        "this period. Returning to hand.");
                    return false;
                }

                // [R5-d / D-R5-26=A · reordenado en R5-g] Cobro del recurso.
                // La decisión ya se tomó en 2b.5, contra el saldo previo a la
                // generación de Voltage; esto es solo la contabilidad, y por eso
                // puede correr después del presupuesto sin alterar nada.
                //
                // Este `false` es ahora, de verdad, casi inalcanzable: 2b.5
                // acaba de comprobar el mismo saldo con la misma API sobre el
                // mismo pagador, y entre ambos puntos solo corre TryConsumePlay,
                // que puede AÑADIR Voltage pero nunca restarlo. Queda como
                // guardia por si alguien inserta algo consumidor en medio. Nota
                // heredada corregida: el comentario anterior justificaba lo
                // inalcanzable citando «2a.6 y el overlay», pero 2a.6 solo mira
                // el bonus-loop y el overlay excluye a los pagadores no
                // resolubles estáticamente (§10.5). Ninguno de los dos era una
                // pre-comprobación de recurso. Ahora sí existe una: 2b.5.
                if (bandCharacter is MusicianBase resourcePayer &&
                    !GigManager.TryPayResourceCost(data, resourcePayer))
                {
                    Debug.LogWarning($"{DebugTag} [Gig][R5-g] '{data.DisplayName}' passed " +
                        "the 2b.5 resource check and failed the spend. Something " +
                        "consumed the resource between the two gates.");
                    return false;
                }

                LogV($"{DebugTag} [Gig] Zone hint = {zoneUsed}");

                // musician one-shot animation if the caster is a musician
                if (bandCharacter is MusicianBase bandMusician)
                {
                    bandMusician.PlayCardOneShotAnimation(data);


                    // [WINK-1 D-WINK-1=A] Commit beat: past every denial gate,
                    // before effects. Bus-published so presentation stays out
                    // of the play pipeline.
                    SensoryEventBus.Instance?.Publish(
                        new CardPerformedEvent(bandMusician, data));
                }

                // 2c) Execute the card in gig context
                heldCard.Use(
                    bandCharacter,
                    targetCharacter,
                    GigManager.CurrentAudienceCharacterList,
                    GigManager.CurrentMusicianCharacterList
                );

                return true;
            }

            // Any other domains → not handled in gig
            Debug.LogWarning($"{DebugTag} [Gig] " +
                $"Card '{data.DisplayName}' has unsupported domain '{data.Domain}'.");
            return false;
        }

        private bool TryPlayInShip(Vector2 mousePos, CardDropZone zoneUsed)
        {
            if (shipInteriorManager == null || heldCard == null) return false;

            var data = heldCard.CardDefinition;
            if (data == null || !data.IsComposition) return false;

            MusicianBase target = null;

            // FIX: If the card has a fixed musician type, resolve that FIRST and ignore hover.
            if (data.RequiresFixedPerformer && _resolveTargetByType != null)
            {
                target = _resolveTargetByType(data.FixedPerformerType);

                if (target != null)
                    LogV($"{DebugTag} [Ship] " +
                        $"Fixed-target card -> {target.MusicianCharacterData.CharacterName} " +
                        $"({data.FixedPerformerType}).");
                else
                    LogV($"{DebugTag} [Ship] " +
                        $"Fixed-target card but resolver returned null for " +
                        $"{data.FixedPerformerType}.");
            }

            // Otherwise (or if fixed-target failed to resolve),
            // use hover → selected fallback
            if (target == null)
            {
                // Prefer hovered musician
                var hovered = IsOverMusician(mousePos);
                if (hovered != null)
                {
                    target = hovered;
                    LogV($"{DebugTag} [Ship] Hover-target -> " +
                        $"{target.MusicianCharacterData.CharacterName}.");
                }
                else
                {
                    // Fallback to Ship-selected
                    // (may be the first musician if none highlighted)
                    target =
                        shipInteriorManager.GetSelectedMusicianOrDefault() as MusicianBase;
                    LogV($"{DebugTag} [Ship] Selected/Default target -> " +
                        $"{(target != null ? target.MusicianCharacterData.CharacterName : "null")}.");
                }
            }

            // If the card REQUIRES a musician and still none, abort.
            if (data.RequiresMusicianTarget && target == null)
            {
                LogV($"{DebugTag} [Ship] " +
                    $"Card requires musician target but none resolved.");
                return false;
            }

            // Route to Ship: enqueues intents (Intro/Outro/Solo/Tempo/Track/Theme)
            var ok = shipInteriorManager.TryPlayCompositionCard(heldCard, target, zoneUsed);
            return ok;
        }

        private void ReturnHeldToHand()
        {
            AddCardToHand(heldCard, selected);
            heldCard = null;
        }

        private CardBase _heldOverLoggedFor;

        /// <summary>[S10] A held card that needs a target, over any character (collider or drawn
        /// sprite), stays held even inside the hand band; the drop then resolves the target.</summary>
        private bool HeldOverTargetCharacter(Vector2 mousePos)
        {
            if (!useGigContext || heldCard == null || heldCard.CardDefinition == null) return false;
            if (!heldCard.CardDefinition.RequiresTargetSelection || mainCam == null) return false;
            var ray = mainCam.ScreenPointToRay(mousePos);
            ICharacter over = null;
            if (CharacterBase.RaycastTopmost(ray, 1000, targetLayer, null, out var hit))
                over = hit.collider.gameObject.GetComponent<ICharacter>();
            if (over == null) over = PickBySpriteBounds(ray, null);
            if (over != null && _heldOverLoggedFor != heldCard)
            {
                _heldOverLoggedFor = heldCard;
                Debug.Log($"[S10][CARTAS] '{heldCard.CardDefinition.Id}' sobre la franja de la mano pero encima de " +
                          $"'{over.GetCharacterBase()?.name}': sigue en el aire (antes volvía a la mano)");
            }
            return over != null;
        }

        /// <summary>[S10] Drop fallback: the character whose drawn sprite contains the pointer
        /// (topmost by sorting, like RaycastTopmost), among those <paramref name="accept"/> admits.</summary>
        private static ICharacter PickBySpriteBounds(Ray ray, System.Func<ICharacter, bool> accept)
        {
            ICharacter best = null;
            int bestLayer = int.MinValue, bestOrder = int.MinValue;
            foreach (var cb in UnityEngine.Object.FindObjectsByType<CharacterBase>(FindObjectsSortMode.None))
            {
                if (cb == null || !cb.isActiveAndEnabled || !(cb is ICharacter ic)) continue;
                if (accept != null && !accept(ic)) continue;
                var sr = cb.SpriteRenderer;
                if (sr == null || !sr.enabled || !sr.gameObject.activeInHierarchy) continue;
                var b = sr.bounds;
                if (!new Plane(Vector3.forward, b.center).Raycast(ray, out var d)) continue;
                var p = ray.GetPoint(d);
                if (p.x < b.min.x || p.x > b.max.x || p.y < b.min.y || p.y > b.max.y) continue;
                int layer = SortingLayer.GetLayerValueFromID(sr.sortingLayerID), order = sr.sortingOrder;
                if (best == null || layer > bestLayer || (layer == bestLayer && order > bestOrder))
                {
                    best = ic; bestLayer = layer; bestOrder = order;
                }
            }
            return best;
        }

        private MusicianBase IsOverMusician(Vector2 mousePos)
        {
            var mainRay = mainCam.ScreenPointToRay(mousePos);
            RaycastHit hit;
            // [S10] Musicians only: the audience drawn in front of Conito no longer steals the drop.
            if (CharacterBase.RaycastTopmost(mainRay, 1000, targetLayer,
                    c => c.GetCharacterType() == CharacterType.Musician, out hit))
            {
                var musician = hit.collider.gameObject.GetComponent<MusicianBase>();
                return musician;
            }

            return null;
        }

        private bool TryResolveCardTarget(
            Ray ray, CardDefinition data, out CharacterBase targetCharacter)
        {
            targetCharacter = null;

            // CSO-based validation:
            // If the card only has single-target status actions for musicians,
            // require musician.
            // If only for audience, require audience.
            // If both (or neither), accept either.
            var expectsMusicianSingle = false;
            var expectsAudienceSingle = false;

            var payload = data.Payload;
            var effects = payload != null ? payload.Effects : null;

            if (effects != null)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    // [R4] Reveal cards constrain targeting the same way single-target
                    // status specs do. Kept as its own branch (not folded into the
                    // ApplyStatusEffectSpec cast) so the two spec kinds stay legible.
                    if (effects[i] is RevealPreferencesSpec reveal)
                    {
                        switch (reveal.targetType)
                        {
                            case ActionTargetType.Musician:
                                expectsMusicianSingle = true;
                                break;

                            case ActionTargetType.AudienceCharacter:
                                expectsAudienceSingle = true;
                                break;
                        }
                        continue;
                    }

                    // [S9c / D1=B] Single-target Stress change: musician only.
                    if (effects[i] is ModifyStressSpec mss)
                    {
                        if (mss.targetType == ActionTargetType.Musician)
                            expectsMusicianSingle = true;
                        continue;
                    }

                    if (effects[i] is not ApplyStatusEffectSpec ase)
                        continue;

                    // si no tiene status asignado, igual “cuenta” para targeting
                    // (porque el diseñador intenta targetear algo)
                    switch (ase.targetType)
                    {
                        case ActionTargetType.Musician:
                            expectsMusicianSingle = true;
                            break;

                        case ActionTargetType.AudienceCharacter:
                            expectsAudienceSingle = true;
                            break;
                    }
                }
            }

            // [S10] Pick only among the characters this card can target, so the one drawn on
            // top of the wanted target (audience over Conito, musicians over the crowd) does
            // not swallow the drop. Validation below stays as the final word.
            System.Func<ICharacter, bool> accept = null;
            if (expectsMusicianSingle && !expectsAudienceSingle)
                accept = c => c.GetCharacterType() == CharacterType.Musician;
            else if (expectsAudienceSingle && !expectsMusicianSingle)
                accept = c => c.GetCharacterType() == CharacterType.Audience;

            ICharacter character = null;
            if (CharacterBase.RaycastTopmost(ray, 1000, targetLayer, accept, out var hit))
                character = hit.collider.gameObject.GetComponent<ICharacter>();
            // [S10] The colliders are smaller than the drawings: a drop on the visible body
            // (where the hover tooltip already shows) falls back to the sprite's bounds.
            if (character == null)
            {
                character = PickBySpriteBounds(ray, accept);
                if (character != null)
                    Debug.Log($"[S10][CARTAS] objetivo por silueta: '{character.GetCharacterBase()?.name}' " +
                              $"para {ALWTTT.Managers.DeckManager.CardNames(new[] { data })}");
            }
            if (character == null)
            {
                Debug.Log($"[S10][CARTAS] no se jugó {ALWTTT.Managers.DeckManager.CardNames(new[] { data })}: " +
                          $"soltada sin un personaje válido debajo (ratón {Input.mousePosition.x:F0},{Input.mousePosition.y:F0} " +
                          $"de {Screen.width}×{Screen.height})");
                return false;
            }

            if (character.IsStunned)
            {
                Debug.Log($"[S10][CARTAS] no se jugó {ALWTTT.Managers.DeckManager.CardNames(new[] { data })}: " +
                          $"'{character.GetCharacterBase()?.name}' está aturdido");
                return false;
            }

            var baseTarget = character.GetCharacterBase();
            if (baseTarget == null)
            {
                LogV($"{DebugTag} [Gig] ICharacter.GetCharacterBase returned null.");
                return false;
            }

            if (expectsMusicianSingle && !expectsAudienceSingle)
            {
                if (baseTarget.GetCharacterType() != CharacterType.Musician)
                {
                    Debug.Log($"[S10][CARTAS] no se jugó '{data.Id}': pide un músico y cayó sobre '{baseTarget.name}'");
                    return false;
                }
            }
            else if (expectsAudienceSingle && !expectsMusicianSingle)
            {
                if (baseTarget.GetCharacterType() != CharacterType.Audience)
                {
                    Debug.Log($"[S10][CARTAS] no se jugó '{data.Id}': pide público y cayó sobre '{baseTarget.name}'");
                    return false;
                }
            }

            targetCharacter = baseTarget;
            return true;
        }

        private void GetDistanceToCurrentSelectedCard(out int count, out float sqrDistance)
        {
            count = hand.Count;
            sqrDistance = 1000;

            // If a card is currently selected
            if (selected >= 0 && selected < count)
            {
                // Calculate normalized position of the card along the curve
                var t = (selected + 0.5f) / count;
                // Get the 3D position of that card along the curve
                var p = GetCurvePoint(a, b, c, t);
                // Calculate squared distance from mouse to card
                sqrDistance = (p - mouseWorldPos).sqrMagnitude;
            }
        }

        /// <summary>
        /// [S9] Tuck: while the pointer is away from the bottom band (the hand bounds, a bit taller
        /// and wider) and no card is held or dragged, the whole curve slides down by tuckDrop, so
        /// the stage above it (the Reina's brood, low audience) stays visible. The band is fixed in
        /// the hand's local space (it does not drop with the cards), so reaching for the visible
        /// card tops always brings the hand back up. Uses last frame's pointer position.
        /// </summary>
        private void UpdateTuck()
        {
            bool away = false;
            if (tuckWhenAway && heldCard == null && dragged < 0)
            {
                var pt = (Vector2)transform.InverseTransformPoint(mouseWorldPos);
                bool near = pt.y <= handBounds.yMax + tuckZoneExtraTop
                            && pt.x >= handBounds.xMin - tuckZoneExtraSide
                            && pt.x <= handBounds.xMax + tuckZoneExtraSide;
                away = !near;
            }
            _tuck = Mathf.MoveTowards(_tuck, away ? 1f : 0f, tuckSpeed * Time.unscaledDeltaTime);
            if (away != _tuckAway)
            {
                _tuckAway = away;
                Debug.Log($"[S9] Mano | {(away ? "baja (ratón fuera)" : "sube (ratón cerca)")}");
            }
        }

        private void CheckMouseInsideHandBounds(out bool mouseButton)
        {
            var point = transform.InverseTransformPoint(mouseWorldPos);
            mouseInsideHand = handBounds.Contains(point);
            mouseButton = Input.GetMouseButton(0) && !ALWTTT.UI.PauseGate.IsPaused;   // [S10] no plays in the Space pause
        }

        private void TiltCard(Vector2 mousePos)
        {
            Vector2 referenceResolution = new Vector2(1600f, 900f); // TODO
            mousePosDelta = (mousePos - prevMousePos)
               * new Vector2(referenceResolution.x / Screen.width,
                             referenceResolution.y / Screen.height)
               * HandDt;
            prevMousePos = mousePos;

            var tiltStrength = 3f;
            var tiltDrag = 3f;
            var tiltSpeed = 50f;

            force += (mousePosDelta * tiltStrength - heldCardTilt) * HandDt;
            force *= 1 - tiltDrag * HandDt;
            heldCardTilt += force * (HandDt * tiltSpeed);

            if (showDebugGizmos)
            {
                Debug.DrawRay(mouseWorldPos, mousePosDelta, Color.red);
                Debug.DrawRay(mouseWorldPos, heldCardTilt, Color.cyan);
            }
        }

        private void GetMouseWorldPosition(Vector2 mousePos)
        {
            var ray = cam.ScreenPointToRay(mousePos);
            if (plane.Raycast(ray, out var enter)) mouseWorldPos = ray.GetPoint(enter);
        }

        /// <summary>
        /// Obtains a point along a curve based on 3 points (quadratic Bézier curve). 
        /// Equal to Lerp(Lerp(a, b, t), Lerp(b, c, t), t).
        /// </summary>
        public static Vector3 GetCurvePoint(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            t = Mathf.Clamp01(t);       // Ensure t is between 0 and 1
            float oneMinusT = 1f - t;   // (1 - t)

            return (oneMinusT * oneMinusT * a)   // (1 - t)^2 * a
                 + (2f * oneMinusT * t * b)      // 2 * (1 - t) * t * b
                 + (t * t * c);                  // t^2 * c
        }

        /// <summary>
        /// Obtains the derivative of the curve (tangent)
        /// </summary>
        public static Vector3 GetCurveTangent(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            return 2f * (1f - t) * (b - a) + 2f * t * (c - b);
        }

        /// <summary>
        /// Obtains a direction perpendicular to the tangent of the curve
        /// </summary>
        public static Vector3 GetCurveNormal(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            Vector3 tangent = GetCurveTangent(a, b, c, t);
            return Vector3.Cross(tangent, Vector3.forward);
        }

        private CardDropZone GetDropZoneForLocalPoint(Vector3 localPoint)
        {
            if (!enableDropZones) return CardDropZone.None;

            bool inCurrent =
                _currentZoneRectLocal.Contains(new Vector2(localPoint.x, localPoint.y));

            if (inCurrent) return CardDropZone.CurrentPart;

            // [B1 / D-D=β] NextPart drop gesture removed in Phase B. The
            // _nextZoneRectLocal rect is preserved (the enum value + field
            // exist for migration coexistence) but any drop in it is now
            // treated as CurrentPart. The visual hint (NEXT PART gizmo)
            // is also removed in OnDrawGizmos.
            bool inNext =
                _nextZoneRectLocal.Contains(new Vector2(localPoint.x, localPoint.y));

            if (inNext) return CardDropZone.CurrentPart;

            return CardDropZone.None;
        }

        /// <summary>
        /// Remove the card at the specified index from the hand.
        /// </summary>
        public void RemoveCardFromHand(int index)
        {
            if (updateHierarchyOrder)
            {
                CardBase card = hand[index];
                card.transform.SetParent(transform.parent);
                card.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
            }

            hand.RemoveAt(index);
        }

        /// <summary>
        /// Moves the card in hand from the currentIndex to the toIndex. 
        /// If you want to move a card that isn't in hand, use AddCardToHand
        /// </summary>
        public void MoveCardToIndex(int currentIndex, int toIndex)
        {
            if (currentIndex == toIndex) return; // Same index, do nothing
            CardBase card = hand[currentIndex];
            hand.RemoveAt(currentIndex);
            hand.Insert(toIndex, card);

            if (updateHierarchyOrder)
            {
                card.transform.SetSiblingIndex(toIndex);
            }
        }

        /// <summary>
        /// Recalculates curve endpoints and hand bounds using current cardBaseScale.
        /// Call after changing sizing fields at runtime.
        /// </summary>
        public void RecalculateCurve()
        {
            InitHand();
            LogV($"{DebugTag} [M1.9] RecalculateCurve — curve recomputed with cardBaseScale={cardBaseScale}");
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Live-tuning support: recalculate curve when sizing fields change in Inspector
            if (Application.isPlaying)
            {
                RecalculateCurve();
            }
        }
#endif

        #region Editor
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.blue;

            var sf = HandScaleFactor;
            var gizmoStart = handPosition + new Vector3(curveStart.x * sf, curveStart.y, curveStart.z);
            var gizmoMid = handPosition + new Vector3(0, curveHeight, 0);
            var gizmoEnd = handPosition + new Vector3(curveEnd.x * sf, curveEnd.y, curveEnd.z);

            Gizmos.DrawSphere(gizmoStart, 0.03f);
            Gizmos.DrawSphere(gizmoEnd, 0.03f);

            Vector3 p1 = gizmoStart;
            for (int i = 0; i < 20; i++)
            {
                float t = (i + 1) / 20f;
                Vector3 p2 = GetCurvePoint(gizmoStart, gizmoMid, gizmoEnd, t);
                Gizmos.DrawLine(p1, p2);
                p1 = p2;
            }

            if (mouseInsideHand)
            {
                Gizmos.color = Color.red;
            }

            var boundsCenter = handOffset + new Vector2(handPosition.x, handPosition.y);
            Gizmos.DrawWireCube(boundsCenter, handSize);

            if (enableDropZones)
            {
                Gizmos.matrix = transform.localToWorldMatrix;

                // CURRENT zone — cyan
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(currentZoneCenter, currentZoneSize);

                // [B1 / D-D=β] NEXT zone gizmo + label removed.
                // _nextZoneRectLocal still treated as CurrentPart in
                // GetDropZoneForLocalPoint for migration coexistence.

#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.Label(
                    transform.TransformPoint(currentZoneCenter
                    + new Vector2(0, currentZoneSize.y * 0.5f + 0.2f)), "CURRENT PART");
#endif
            }
        }
#endif
        #endregion
    }
}