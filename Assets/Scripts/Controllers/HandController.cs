using ALWTTT.Characters;
using ALWTTT.Characters.Band;
using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace ALWTTT
{
    public class HandController : MonoBehaviour
    {
        private const string DebugTag = "<color=green>GigManager:</color>";

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
                GigManager.HighlightCardTarget(
                    heldCard.CardData.CardActionDataList[0].ActionTargetType);

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
                    GigManager.DeactivateCardHighlights();

                    return;
                }

                var mouseButtonUp = Input.GetMouseButtonUp(0);
                if (mouseButtonUp) PlayCard(mousePos);
            }
        }

        private void PlayCard(Vector2 mousePos)
        {
            Debug.Log($"{DebugTag} Playing card...");

            GigManager.DeactivateCardHighlights();

            bool backToHand = true;

            // If enough groove
            if (GameManager.PersistentGameplayData.CanUseCards && 
                GameManager.PersistentGameplayData.CurrentGroove >=
                    heldCard.CardData.GrooveCost)
            {
                //RaycastHit hit;
                var mainRay = mainCam.ScreenPointToRay(mousePos);
                var canUse = false;

                CharacterBase bandCharacter = GigManager.SelectedMusician;
                CharacterBase targetCharacter = null;

                canUse = heldCard.CardData.UsableWithoutTarget ||
                    CheckPlayOnCharacter(mainRay, canUse,
                        ref bandCharacter, ref targetCharacter) ||
                        heldCard.CardData.CardType == CardType.SFX;

                if (canUse)
                {
                    backToHand = false;
                    heldCard.Use(bandCharacter, targetCharacter,
                        GigManager.CurrentAudienceCharacterList,
                        GigManager.CurrentMusicianCharacterList);
                }
            }

            if (backToHand)
            {
                Debug.Log($"{DebugTag} <color=red>Card couldn't be played.</color>");
                AddCardToHand(heldCard, selected);
            }

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

                if (character != null)
                {
                    var firstActionTargetType =
                        heldCard.CardData.CardActionDataList[0].ActionTargetType;

                    var checkEnemy =
                        ((firstActionTargetType == ActionTargetType.AudienceCharacter ||
                        firstActionTargetType == ActionTargetType.AllAudienceCharacters ||
                        firstActionTargetType == ActionTargetType.RandomAudienceCharacter) 
                        &&
                        character.GetCharacterType() == CharacterType.Audience);

                    var checkAlly =
                        ((firstActionTargetType == ActionTargetType.Musician ||
                        firstActionTargetType == ActionTargetType.AllMusicians ||
                        firstActionTargetType == ActionTargetType.RandomMusician) 
                        &&
                        character.GetCharacterType() == CharacterType.Musician);

                    // TODO: Modify this part
                    //if (checkEnemy || checkAlly)
                    if (character.GetCharacterType() == CharacterType.Musician)
                    {
                        canUse = true;
                        bandCharacter = GigManager.SelectedMusician;
                        audienceCharacter = character.GetCharacterBase();
                    }
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
        }
#endif
        #endregion
    }
}