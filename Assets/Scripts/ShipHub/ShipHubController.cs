using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.ShipHub
{
    /// <summary>
    /// [S5 NAVE-1] The ship hub: a thin UI scene over the ship cutaway, in the
    /// spirit of <c>LinearMapController</c> (MAP-DEMO-1). Not the old
    /// "2 - Ship Interior" (rehearsal/composition, Phase C).
    ///
    /// - Rooms are data (name, rectangle over the image, art). The controller
    ///   builds one <see cref="ShipRoomZone"/> per room at runtime as a child
    ///   of <see cref="shipImage"/>, anchored by the room rectangle, so zones
    ///   follow the image at any resolution. Gizmos draw them in the Scene View.
    /// - Hover tints the room and shows its name. Click: the captain's cabin
    ///   opens the linear map; any other room opens its art full screen with
    ///   «EN DESARROLLO» and «Volver» (built in code: no panel to wire).
    /// - Start: cohesion &lt;= 0 → Game Over (D-DIAG-1). Gigs launched from the
    ///   map return here (GigReturnDestination.Ship, D2=A), so a lost gig that
    ///   drops cohesion to 0 lands here first.
    /// - Optional reveal: the exterior art fades out over the cutaway on the
    ///   first visit of a run (same size and framing as the cutaway).
    /// </summary>
    public class ShipHubController : MonoBehaviour
    {
        private const string Tag = "[S5]";

        [Serializable]
        public class Room
        {
            public string name = "Sala";

            [Tooltip("Rectangle over the ship image in 0..1, measured from the " +
                "TOP-LEFT corner like an image editor: x, y, width, height.")]
            public Rect area = new Rect(0.4f, 0.4f, 0.1f, 0.1f);

            [Tooltip("Captain's cabin: opens the linear map instead of a room screen.")]
            public bool opensMap;

            [Tooltip("Full-screen art. More than one: «Siguiente» cycles them. " +
                "Empty: the screen shows only the label.")]
            public Sprite[] art;

            [Tooltip("Shows «EN DESARROLLO» on the room screen and in the hover label.")]
            public bool inDevelopment = true;
        }

        [Header("Ship")]
        [SerializeField, Tooltip("UI Image with the cutaway. Zones are built as " +
            "its children; keep its RectTransform at the sprite's aspect.")]
        private RectTransform shipImage;

        [SerializeField, Tooltip("Optional UI Image with the exterior art, same " +
            "size and framing as the cutaway, on top of it. Fades out to reveal " +
            "the rooms.")]
        private Image exteriorImage;

        [Header("Rooms")]
        [SerializeField] private List<Room> rooms = new List<Room>();

        [Header("Look")]
        [SerializeField] private Color zoneIdle = new Color(1f, 1f, 1f, 0f);
        [SerializeField] private Color zoneHover = new Color(1f, 0.9f, 0.4f, 0.22f);
        [SerializeField] private float labelFontSize = 34f;
        [SerializeField] private string inDevelopmentText = "EN DESARROLLO";
        [SerializeField] private string mapSuffix = "Mapa";

        [Header("Reveal (exterior → cutaway)")]
        [SerializeField] private bool playReveal = true;
        [SerializeField] private float revealDelay = 0.6f;
        [SerializeField] private float revealDuration = 1.6f;

        [Header("Dependencies")]
        [SerializeField] private SceneChanger sceneChanger;
        [SerializeField, Tooltip("Optional HUD text.")]
        private TextMeshProUGUI cohesionText;

        private readonly List<ShipRoomZone> _zones = new List<ShipRoomZone>();
        private Canvas _canvas;
        private RectTransform _label;
        private TextMeshProUGUI _labelText;
        private GameObject _roomScreen;
        private Image _roomArt;
        private TextMeshProUGUI _roomTitle;
        private TextMeshProUGUI _roomDevText;
        private GameObject _nextButton;
        private int _openRoom = -1;
        private int _artIndex;
        private bool _busy;      // reveal playing or leaving the scene
        private bool _leaving;

        private PersistentGameplayData PD =>
            GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;

        private void Awake()
        {
            if (sceneChanger == null)
                sceneChanger = FindFirstObjectByType<SceneChanger>();
        }

        private void Start()
        {
            var pd = PD;
            if (pd == null)
            {
                Debug.LogError($"{Tag} Ship hub: PersistentGameplayData is null; hub inert.");
                return;
            }

            if (cohesionText != null)
                cohesionText.text = $"Cohesión: {pd.BandCohesion}";

            // [D-DIAG-1] Cohesion 0 is the only defeat.
            if (pd.BandCohesion <= 0)
            {
                Debug.Log($"{Tag} Ship hub | Cohesion={pd.BandCohesion} → Game Over.");
                _leaving = true;
                if (sceneChanger != null) sceneChanger.OpenGameOverScene();
                return;
            }

            if (shipImage == null)
            {
                Debug.LogError($"{Tag} Ship hub: shipImage not wired.");
                return;
            }

            _canvas = shipImage.GetComponentInParent<Canvas>();
            BuildZones();
            BuildLabel();
            BuildRoomScreen();

            bool firstVisit = pd.LinearMapNextNode == 0;
            Debug.Log($"{Tag} Ship hub | rooms={_zones.Count} | Cohesion={pd.BandCohesion} | " +
                      $"Fans={pd.Fans} | NextNode={pd.LinearMapNextNode} | firstVisit={firstVisit}");

            if (exteriorImage != null)
            {
                exteriorImage.raycastTarget = false;
                if (playReveal && firstVisit) StartCoroutine(Reveal());
                else exteriorImage.gameObject.SetActive(false);
            }
        }

        // ---------------------------------------------------------------- build

        private void BuildZones()
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                var go = new GameObject($"Zone_{room.name}", typeof(RectTransform), typeof(Image));
                var rt = (RectTransform)go.transform;
                rt.SetParent(shipImage, false);
                SetAnchorsFromArea(rt, room.area);

                var zone = go.AddComponent<ShipRoomZone>();
                zone.Init(i, zoneIdle, zoneHover);
                zone.HoverEnter += OnZoneEnter;
                zone.HoverExit += OnZoneExit;
                zone.Clicked += OnZoneClicked;
                _zones.Add(zone);
            }

            // The exterior must stay on top of the zones while it fades.
            if (exteriorImage != null && exteriorImage.transform.parent == shipImage)
                exteriorImage.transform.SetAsLastSibling();
        }

        private static void SetAnchorsFromArea(RectTransform rt, Rect a)
        {
            // area is top-left based (image editor); UI anchors are bottom-left.
            rt.anchorMin = new Vector2(a.xMin, 1f - (a.y + a.height));
            rt.anchorMax = new Vector2(a.xMin + a.width, 1f - a.y);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private RectTransform Root => _canvas != null
            ? (RectTransform)_canvas.transform : (RectTransform)shipImage.parent;

        private void BuildLabel()
        {
            var bg = new GameObject("HoverLabel", typeof(RectTransform), typeof(Image));
            _label = (RectTransform)bg.transform;
            _label.SetParent(Root, false);
            _label.pivot = new Vector2(0.5f, 0f);
            var img = bg.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.75f);
            img.raycastTarget = false;
            var fitter = bg.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var layout = bg.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 8, 8);
            layout.childAlignment = TextAnchor.MiddleCenter;

            _labelText = NewText("Text", _label, labelFontSize, TextAlignmentOptions.Center);
            bg.SetActive(false);
        }

        private void BuildRoomScreen()
        {
            _roomScreen = new GameObject("RoomScreen", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)_roomScreen.transform;
            rt.SetParent(Root, false);
            Stretch(rt);
            var bg = _roomScreen.GetComponent<Image>();
            bg.color = Color.black;
            bg.raycastTarget = true; // blocks the zones underneath

            var art = new GameObject("Art", typeof(RectTransform), typeof(Image));
            var artRt = (RectTransform)art.transform;
            artRt.SetParent(rt, false);
            Stretch(artRt);
            _roomArt = art.GetComponent<Image>();
            _roomArt.preserveAspect = true;
            _roomArt.raycastTarget = false;

            _roomTitle = NewText("Title", rt, 44f, TextAlignmentOptions.Center);
            Place(_roomTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 70f));

            _roomDevText = NewText("InDevelopment", rt, 96f, TextAlignmentOptions.Center);
            _roomDevText.fontStyle = FontStyles.Bold;
            _roomDevText.color = new Color(1f, 0.85f, 0.3f);
            _roomDevText.outlineWidth = 0.25f;
            _roomDevText.outlineColor = Color.black;
            Place(_roomDevText.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 140f));

            NewButton("Volver", rt, new Vector2(0.5f, 0f), new Vector2(0f, 60f), CloseRoom);
            _nextButton = NewButton("Siguiente", rt, new Vector2(1f, 0f), new Vector2(-180f, 60f), NextArt);

            _roomScreen.transform.SetAsLastSibling();
            _roomScreen.SetActive(false);
        }

        // ---------------------------------------------------------------- hover / click

        private void OnZoneEnter(ShipRoomZone zone)
        {
            if (_busy || _openRoom >= 0) return;
            zone.SetHighlighted(true);
            var room = rooms[zone.Index];
            _labelText.text = room.opensMap ? $"{room.name} · {mapSuffix}"
                : room.inDevelopment ? $"{room.name} · <size=70%>{inDevelopmentText}</size>"
                : room.name;

            // Top-centre of the zone, in world space (works for any canvas mode).
            var corners = new Vector3[4];
            zone.Rect.GetWorldCorners(corners); // 0 BL, 1 TL, 2 TR, 3 BR
            _label.position = (corners[1] + corners[2]) * 0.5f;
            _label.gameObject.SetActive(true);
            _label.SetAsLastSibling();
        }

        private void OnZoneExit(ShipRoomZone zone)
        {
            zone.SetHighlighted(false);
            _label.gameObject.SetActive(false);
        }

        private void OnZoneClicked(ShipRoomZone zone)
        {
            if (_busy || _leaving || _openRoom >= 0) return;
            var room = rooms[zone.Index];

            if (room.opensMap)
            {
                _leaving = true;
                Debug.Log($"{Tag} Ship → Map | room='{room.name}'");
                if (sceneChanger != null) sceneChanger.OpenMapScene();
                else Debug.LogError($"{Tag} SceneChanger missing; cannot open the map.");
                return;
            }

            OpenRoom(zone.Index);
        }

        private void OpenRoom(int index)
        {
            var room = rooms[index];
            _openRoom = index;
            _artIndex = 0;
            _zones[index].SetHighlighted(false);
            _label.gameObject.SetActive(false);

            int artCount = room.art != null ? room.art.Length : 0;
            ShowArt();
            _roomTitle.text = room.name;
            _roomDevText.text = inDevelopmentText;
            _roomDevText.gameObject.SetActive(room.inDevelopment);
            _nextButton.SetActive(artCount > 1);
            _roomScreen.SetActive(true);
            _roomScreen.transform.SetAsLastSibling();

            Debug.Log($"{Tag} Ship → Room '{room.name}' | art={artCount} | inDevelopment={room.inDevelopment}");
        }

        private void ShowArt()
        {
            var art = rooms[_openRoom].art;
            Sprite s = art != null && art.Length > 0 ? art[_artIndex % art.Length] : null;
            _roomArt.sprite = s;
            _roomArt.enabled = s != null;
        }

        private void NextArt()
        {
            if (_openRoom < 0) return;
            _artIndex++;
            ShowArt();
        }

        private void CloseRoom()
        {
            if (_openRoom < 0) return;
            Debug.Log($"{Tag} Room '{rooms[_openRoom].name}' → Ship");
            _openRoom = -1;
            _roomScreen.SetActive(false);
        }

        // ---------------------------------------------------------------- reveal

        private IEnumerator Reveal()
        {
            _busy = true;
            var c = exteriorImage.color;
            c.a = 1f;
            exteriorImage.color = c;
            exteriorImage.gameObject.SetActive(true);
            Debug.Log($"{Tag} Ship reveal | {revealDuration}s");

            float t = -revealDelay;
            while (t < revealDuration)
            {
                yield return null;
                if (Input.anyKeyDown) break; // any key or click skips
                t += Time.deltaTime;
                c.a = 1f - Mathf.Clamp01(t / Mathf.Max(0.01f, revealDuration));
                exteriorImage.color = c;
            }

            exteriorImage.gameObject.SetActive(false);
            _busy = false;
        }

        // ---------------------------------------------------------------- UI helpers

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, anchor.y);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        private static TextMeshProUGUI NewText(string name, Transform parent, float size,
            TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>(); // TMP default font asset
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }

        private static GameObject NewButton(string text, Transform parent, Vector2 anchor,
            Vector2 pos, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject($"Button_{text}", typeof(RectTransform), typeof(Image), typeof(Button));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            Place(rt, anchor, pos, new Vector2(280f, 80f));
            go.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            go.GetComponent<Button>().onClick.AddListener(onClick);

            var label = NewText("Text", rt, 38f, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            label.text = text;
            return go;
        }

        // ---------------------------------------------------------------- editor

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (shipImage == null || rooms == null) return;
            var c = new Vector3[4];
            shipImage.GetWorldCorners(c); // 0 BL, 1 TL, 2 TR, 3 BR
            Vector3 origin = c[1];               // top-left
            Vector3 right = c[2] - c[1];         // full width
            Vector3 down = c[0] - c[1];          // full height (downwards)

            foreach (var room in rooms)
            {
                if (room == null) continue;
                var a = room.area;
                Vector3 tl = origin + right * a.x + down * a.y;
                Vector3 tr = tl + right * a.width;
                Vector3 bl = tl + down * a.height;
                Vector3 br = bl + right * a.width;
                Gizmos.color = room.opensMap ? Color.cyan : Color.yellow;
                Gizmos.DrawLine(tl, tr); Gizmos.DrawLine(tr, br);
                Gizmos.DrawLine(br, bl); Gizmos.DrawLine(bl, tl);
                UnityEditor.Handles.Label(tl, room.name);
            }
        }

        private void Reset()
        {
            // Default rooms measured on Nave_Corte.png (S5, D1=A). Art is
            // wired in the Inspector.
            rooms = new List<Room>
            {
                new Room { name = "Cabina del capitán", area = new Rect(0.355f, 0.17f, 0.09f, 0.15f), opensMap = true, inDevelopment = false },
                new Room { name = "Sala de recreación", area = new Rect(0.30f, 0.33f, 0.12f, 0.105f) },
                new Room { name = "Dormitorio", area = new Rect(0.335f, 0.44f, 0.075f, 0.12f) },
                new Room { name = "Sala de ensayo", area = new Rect(0.11f, 0.12f, 0.17f, 0.26f) },
                new Room { name = "Chatarrero", area = new Rect(0.41f, 0.555f, 0.10f, 0.05f) },
            };
        }
#endif
    }
}
