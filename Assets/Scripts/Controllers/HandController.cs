using ALWTTT.Characters;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
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

        [Header("Card Settings")]
        [SerializeField] private bool cardUprightWhenSelected = true;
        [SerializeField] private bool cardTilt = true;
        [SerializeField] private float discardDuration = 0.2f;

        [Header("Hand Settings")]
        [SerializeField][Range(0, 5)] private float selectionSpacing = 1;
        [SerializeField] private Vector3 curveStart = new Vector3(2f, -0.7f, 0);
        [SerializeField] private Vector3 curveEnd = new Vector3(-2f, -0.7f, 0);
        [SerializeField] private Vector2 handOffset = new Vector2(0, -0.3f);
        [SerializeField] private Vector2 handSize = new Vector2(9, 1.7f);

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

        #endregion

        public bool IsDraggingActive { get; private set; } = true;
        private bool showDebugGizmos = true;
        private bool updateHierarchyOrder = false;

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
            a = transform.TransformPoint(curveStart);
            b = transform.position;
            c = transform.TransformPoint(curveEnd);
            handBounds = new Rect((handOffset - handSize / 2), handSize);
            plane = new Plane(-Vector3.forward, transform.position);
            prevMousePos = Input.mousePosition;

            _currentZoneRectLocal = 
                new Rect(currentZoneCenter - currentZoneSize * 0.5f, currentZoneSize);

            _nextZoneRectLocal = 
                new Rect(nextZoneCenter - nextZoneSize * 0.5f, nextZoneSize);
        }
        #endregion

        private void Update()
        {
            if (!IsDraggingActive) return;

            var mousePos = 
                HandleMouseInput(out var count, out var sqrDistance, out var mouseButton);

            HandleCardsInHand(count, mouseButton, sqrDistance);

            HandleDraggedCardInsideHand(mouseButton, count);

            HandleDraggedCardOutsideHand(mouseButton, mousePos);
        }

        public void EnableDragging() => IsDraggingActive = true;
        public void DisableDragging() => IsDraggingActive = false;
        
        /// <summary>
        /// Visually adds a card to the hand. Optional param to insert it at a given index.
        /// </summary>
        public void AddCardToHand(CardBase card, int index = -1)
        {
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
        ) {
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
            for (int i = 0; i < count; i++)
            {
                var card = hand[i];
                var cardTransform = card.transform;

                // If not enough groove, set inactive visual
                /*card.SetInactiveMaterialState(
                    GameManager.PersistentGameplayData.CurrentGroove < 
                    card.CardData.GrooveCost
                );*/

                var noCardHeld = heldCard == null; // Whether a card is "held" (outside of hand)
                var onSelectedCard = noCardHeld && selected == i;
                var onDraggedCard = noCardHeld && dragged == i;

                // Offset that shifts cards to the sides of selected one (fanning effect)
                float selectOffset = 0;
                if (noCardHeld)
                {
                    selectOffset = 0.02f * // TODO: serialize as fanning factor
                        Mathf.Clamp01(
                            1 - Mathf.Abs(Mathf.Abs(i - selected) - 1) / (float)count * 3) *
                        Mathf.Sign(i - selected);
                }

                // Position along Curve
                var t = (i + 0.5f) / count + selectOffset * selectionSpacing;
                var p = GetCurvePoint(a, b, c, t);

                // Mouse interaction
                var d = (p - mouseWorldPos).sqrMagnitude;
                var mouseCloseToCard = d < 0.5f;
                var mouseHoveringOnSelected = 
                    onSelectedCard && mouseCloseToCard && mouseInsideHand;

                // Card Position & Rotation
                var cardUp = GetCurveNormal(a, b, c, t);
                // If hovered, the card slightly “pops up”
                var cardPos = 
                    p + (mouseHoveringOnSelected ? cardTransform.up * 0.3f : Vector3.zero);
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
                    80f * Time.deltaTime
                );

                // Handle Start Dragging
                if (mouseHoveringOnSelected)
                {
                    var mouseButtonDown = Input.GetMouseButtonDown(0);
                    if (mouseButtonDown)
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
                        Vector3.MoveTowards(cardTransform.position, cardPos, 16f * Time.deltaTime);
                    cardTransform.position = cardPos;
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
                    Quaternion.LookRotation(cardForward, cardUp), 80f * Time.deltaTime);
                cardTransform.position = cardPos;

                // Contextual highlights based on actions and target types
                //if (useGigContext && GigManager != null)
                //    GigManager.HighlightCardTarget(heldCard.CardData.CardActionDataList[0].ActionTargetType);

                var musician = IsOverMusician(mousePos);
                if (musician != null)
                {
                    //Debug.Log(musician.MusicianCharacterData.CharacterName);
                }
                
                heldCard.UpdateDescription(musician);

                if (!GameManager.PersistentGameplayData.CanSelectCards || mouseInsideHand)
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
                        Debug.Log($"{DebugTag} Exited zone: {_prevHoverZone}");

                    if (_hoverZone != CardDropZone.None)
                        Debug.Log($"{DebugTag} Entered zone: {_hoverZone}");
                    */

                    _prevHoverZone = _hoverZone;
                }

                var mouseButtonUp = Input.GetMouseButtonUp(0);
                if (mouseButtonUp)
                {
                    if (_hoverZone != CardDropZone.None)
                    {
                        _pendingDropZone = _hoverZone;
                        Debug.Log($"<color=yellow>{DebugTag} DROPPED on zone: " +
                            $"{_pendingDropZone} (card='{heldCard.CardData.CardName}')</color>");
                    }

                    PlayCard(mousePos);
                }
            }
        }

        private void PlayCard(Vector2 mousePos)
        {
            Debug.Log($"{DebugTag} Playing card...");

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

                // Send the card to the appropriate pile (discard/exhaust) like in gig
                if (DeckManager.Instance != null)
                    DeckManager.Instance.OnCardPlayed(heldCard);
            }

            if (backToHand)
            {
                Debug.Log($"{DebugTag} <color=red>Card couldn't be played.</color>");
                AddCardToHand(heldCard, selected);
            }

            heldCard = null;
        }

        private bool TryPlayInGig(Vector2 mousePos, CardDropZone zoneUsed)
        {
            var data = heldCard.CardData;
            if (data == null) return false;

            // ─────────────────────────────────────────────
            // 1) COMPOSITION CARDS → live composition pipeline
            // ─────────────────────────────────────────────
            if (data.IsComposition)
            {
                MusicianBase target = null;

                // 1a) If the card has a fixed musician type, resolve that FIRST and ignore hover.
                if (data.HasFixedMusicianTarget && _resolveTargetByType != null)
                {
                    target = _resolveTargetByType(data.MusicianCharacterType);

                    if (target != null)
                        Debug.Log($"{DebugTag} [Gig] Fixed-target card -> " +
                            $"{target.MusicianCharacterData.CharacterName} " +
                            $"({data.MusicianCharacterType}).");
                    else
                        Debug.Log($"{DebugTag} [Gig] Fixed-target card but resolver returned null " +
                            $"for {data.MusicianCharacterType}.");
                }

                // 1b) Otherwise (or if fixed-target failed), use hover → selected fallback
                if (target == null)
                {
                    var hovered = IsOverMusician(mousePos);
                    if (hovered != null)
                    {
                        target = hovered;
                        Debug.Log($"{DebugTag} [Gig] Hover-target -> " +
                            $"{target.MusicianCharacterData.CharacterName}.");
                    }
                    else
                    {
                        target = GigManager.SelectedMusician as MusicianBase;
                        Debug.Log($"{DebugTag} [Gig] Selected/Default target -> " +
                            $"{(target != null ? target.MusicianCharacterData.CharacterName : "null")}.");
                    }
                }

                // 1c) If the card REQUIRES a musician and still none, abort.
                if (data.RequiresMusicianTarget && target == null)
                {
                    Debug.Log($"{DebugTag} [Gig] Card requires musician target but none resolved.");
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
                    Debug.Log($"{DebugTag} [Gig] Cannot play action card " +
                              $"'{data.CardName}' in current timing. Returning to hand.");
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
                CharacterBase bandCharacter = GigManager.SelectedMusician;
                CharacterBase targetCharacter = null;

                bool canUse = false;

                // No target or SFX → can use directly
                if (data.UsableWithoutTarget || data.CardType == CardType.SFX)
                {
                    canUse = true;
                }
                else
                {
                    // Raycast against musicians / audience
                    canUse = CheckPlayOnCharacter(mainRay, canUse, ref bandCharacter, ref targetCharacter);
                }

                if (!canUse) return false;

                Debug.Log($"{DebugTag} [Gig] Zone hint = {zoneUsed}");

                // musician one-shot animation if the caster is a musician
                if (bandCharacter is MusicianBase bandMusician)
                {
                    bandMusician.PlayCardOneShotAnimation(data);
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
            Debug.LogWarning($"{DebugTag} [Gig] Card '{data.CardName}' has unsupported domain '{data.Domain}'.");
            return false;
        }

        private bool TryPlayInShip(Vector2 mousePos, CardDropZone zoneUsed)
        {
            if (shipInteriorManager == null || heldCard == null) return false;

            var data = heldCard.CardData;
            if (data == null || !data.IsComposition) return false;

            MusicianBase target = null;

            // FIX: If the card has a fixed musician type, resolve that FIRST and ignore hover.
            if (data.HasFixedMusicianTarget && _resolveTargetByType != null)
            {
                target = _resolveTargetByType(data.MusicianCharacterType);
                
                if (target != null)
                    Debug.Log($"{DebugTag} [Ship] Fixed-target card -> {target.MusicianCharacterData.CharacterName} ({data.MusicianCharacterType}).");
                else
                    Debug.Log($"{DebugTag} [Ship] Fixed-target card but resolver returned null for {data.MusicianCharacterType}.");
            }

            // Otherwise (or if fixed-target failed to resolve), use hover → selected fallback
            if (target == null)
            {
                // Prefer hovered musician
                var hovered = IsOverMusician(mousePos);
                if (hovered != null)
                {
                    target = hovered;
                    Debug.Log($"{DebugTag} [Ship] Hover-target -> {target.MusicianCharacterData.CharacterName}.");
                }
                else
                {
                    // Fallback to Ship-selected (may be the first musician if none highlighted)
                    target = shipInteriorManager.GetSelectedMusicianOrDefault() as MusicianBase;
                    Debug.Log($"{DebugTag} [Ship] Selected/Default target -> {(target != null ? target.MusicianCharacterData.CharacterName : "null")}.");
                }
            }

            // If the card REQUIRES a musician and still none, abort.
            if (data.RequiresMusicianTarget && target == null)
            {
                Debug.Log($"{DebugTag} [Ship] Card requires musician target but none resolved.");
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

        private MusicianBase IsOverMusician(Vector2 mousePos)
        {
            var mainRay = mainCam.ScreenPointToRay(mousePos);
            RaycastHit hit;
            if (Physics.Raycast(mainRay, out hit, 1000, targetLayer))
            {
                var musician = hit.collider.gameObject.GetComponent<MusicianBase>();
                return musician;
            }

            return null;
        }

        private bool CheckPlayOnCharacter(Ray mainRay, bool canUse, 
            ref CharacterBase bandCharacter, ref CharacterBase audienceCharacter)
        {
            RaycastHit hit;
            if (Physics.Raycast(mainRay, out hit, 1000, targetLayer))
            {
                var character = hit.collider.gameObject.GetComponent<ICharacter>();
                if (character == null || character.IsStunned) return false;

                var data = heldCard?.CardData;
                if (data == null) return false;

                var actions = data.CardActionDataList;
                if (data.IsComposition || actions == null || actions.Count == 0)
                {
                    Debug.Log($"{DebugTag} [Gig] CheckPlayOnCharacter: " +
                        $"card '{data.CardName}' has no  actions; skipping old targeting.");
                    return false;
                }


                var firstActionTargetType = actions[0].ActionTargetType;

                var checkEnemy =
                    ((firstActionTargetType == ActionTargetType.AudienceCharacter ||
                      firstActionTargetType == ActionTargetType.AllAudienceCharacters ||
                      firstActionTargetType == ActionTargetType.RandomAudienceCharacter) &&
                     character.GetCharacterType() == CharacterType.Audience);

                var checkAlly =
                    ((firstActionTargetType == ActionTargetType.Musician ||
                      firstActionTargetType == ActionTargetType.AllMusicians ||
                      firstActionTargetType == ActionTargetType.RandomMusician) &&
                     character.GetCharacterType() == CharacterType.Musician);

                if (checkEnemy || checkAlly)
                {
                    canUse = true;
                    bandCharacter = GigManager.SelectedMusician;
                    audienceCharacter = character.GetCharacterBase();
                }
            }
            else
            {
                Debug.Log($"<color=red>No character hit by raycast.</color>");
            }

            return canUse;
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

        private void CheckMouseInsideHandBounds(out bool mouseButton)
        {
            var point = transform.InverseTransformPoint(mouseWorldPos);
            mouseInsideHand = handBounds.Contains(point);
            mouseButton = Input.GetMouseButton(0);
        }

        private void TiltCard(Vector2 mousePos)
        {
            Vector2 referenceResolution = new Vector2(1600f, 900f); // TODO
            mousePosDelta = (mousePos - prevMousePos)
               * new Vector2(referenceResolution.x / Screen.width,
                             referenceResolution.y / Screen.height)
               * Time.deltaTime;
            prevMousePos = mousePos;

            var tiltStrength = 3f;
            var tiltDrag = 3f;
            var tiltSpeed = 50f;

            force += (mousePosDelta * tiltStrength - heldCardTilt) * Time.deltaTime;
            force *= 1 - tiltDrag * Time.deltaTime;
            heldCardTilt += force * (Time.deltaTime * tiltSpeed);

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

            bool inNext = 
                _nextZoneRectLocal.Contains(new Vector2(localPoint.x, localPoint.y));

            if (inNext) return CardDropZone.NextPart;

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

        #region Editor
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.blue;

            Gizmos.DrawSphere(curveStart, 0.03f);
            //Gizmos.DrawSphere(Vector3.zero, 0.03f);
            Gizmos.DrawSphere(curveEnd, 0.03f);

            Vector3 p1 = curveStart;
            for (int i = 0; i < 20; i++)
            {
                float t = (i + 1) / 20f;
                Vector3 p2 = GetCurvePoint(curveStart, Vector3.zero, curveEnd, t);
                Gizmos.DrawLine(p1, p2);
                p1 = p2;
            }

            if (mouseInsideHand)
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawWireCube(handOffset, handSize);

            if (enableDropZones)
            {
                Gizmos.matrix = transform.localToWorldMatrix;

                // CURRENT zone (left) — cyan
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(currentZoneCenter, currentZoneSize);

                // NEXT zone (right) — yellow
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(nextZoneCenter, nextZoneSize);

#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.Label(
                    transform.TransformPoint(currentZoneCenter 
                    + new Vector2(0, currentZoneSize.y * 0.5f + 0.2f)), "CURRENT PART");

                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.Label(
                    transform.TransformPoint(nextZoneCenter 
                    + new Vector2(0, nextZoneSize.y * 0.5f + 0.2f)), "NEXT PART");
#endif
            }
        }
#endif
        #endregion
    }
}