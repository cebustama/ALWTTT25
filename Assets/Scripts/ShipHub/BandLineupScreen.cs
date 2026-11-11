using ALWTTT.Characters.Band;
using ALWTTT.Data;
using ALWTTT.Managers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.ShipHub
{
    /// <summary>
    /// [S6 NAVE-2] The dormitory is the lineup screen (D-PLAN-1=A). Built in
    /// code over the «Personajes» art, like the room screen of
    /// <see cref="ShipHubController"/>: no panel to wire.
    ///
    /// - Two copies of the same art: <see cref="litArt"/> (everyone in colour)
    ///   fills the frame; each card is a RectMask2D window with the
    ///   <see cref="hiddenArt"/> (everyone in silhouette) aligned underneath, so
    ///   a musician who is out of the band shows as a silhouette in place
    ///   (reference: Into the Breach squads, Griftlands). No cut-out art needed.
    /// - Presets (dúo/trío/cuarteto, D1=B) set the band in one click; clicking
    ///   a card toggles that musician freely (Into the Breach «Custom Squad»),
    ///   never below <see cref="minMusicians"/>. A pick that matches no preset
    ///   is "libre". Every change is stored at once in PersistentGameplayData
    ///   (D2=A); the first map node plays it.
    /// - After the first gig (LinearMapNextNode &gt; 0) it is read-only and
    ///   shows the band on tour (D3=A).
    /// </summary>
    public class BandLineupScreen : MonoBehaviour
    {
        private const string Tag = "[S6]";
        public const string FreeLineupId = "libre";

        [Serializable]
        public class Card
        {
            [Tooltip("Name shown on screen (the art's name).")]
            public string label = "Músico";
            public MusicianBase musician;

            [Tooltip("Card rectangle over the art in 0..1, from the TOP-LEFT " +
                "corner like an image editor: x, y, width, height.")]
            public Rect area = new Rect(0.1f, 0.2f, 0.18f, 0.6f);
        }

        [Serializable]
        public class Preset
        {
            [Tooltip("Telemetry id: duo, trio, cuarteto.")]
            public string id = "duo";
            public string label = "Dúo";
            public List<MusicianBase> members = new List<MusicianBase>();
        }

        [Header("Art (same size and framing)")]
        [SerializeField, Tooltip("Everyone in colour.")] private Sprite litArt;
        [SerializeField, Tooltip("Everyone in silhouette.")] private Sprite hiddenArt;

        [Header("Cards (left to right = band order on stage)")]
        [SerializeField] private List<Card> cards = new List<Card>();

        [Header("Presets")]
        [SerializeField] private List<Preset> presets = new List<Preset>();
        [SerializeField, Tooltip("Preset applied the first time the screen " +
            "opens without a pick (D2=A: the full band).")]
        private int defaultPreset = 2;
        [SerializeField, Min(1)] private int minMusicians = 2;

        [Header("Look")]
        [SerializeField] private Color cardHover = new Color(1f, 0.9f, 0.4f, 0.18f);
        [SerializeField] private string title = "Dormitorio · Elige la banda";
        [SerializeField] private string lockedText = "La banda ya está de gira";
        [SerializeField] private string minText = "Mínimo {0} músicos";

        public bool IsOpen => _root != null && _root.activeSelf;
        public event Action Closed;

        private GameObject _root;
        private readonly List<GameObject> _silhouettes = new List<GameObject>();
        private readonly List<ShipRoomZone> _zones = new List<ShipRoomZone>();
        private readonly List<GameObject> _presetButtons = new List<GameObject>();
        private readonly HashSet<int> _selected = new HashSet<int>();
        private TextMeshProUGUI _status;
        private bool _locked;

        private static PersistentGameplayData PD =>
            GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;

        // ---------------------------------------------------------------- open / close

        /// <summary>Open the screen as the last child of <paramref name="parent"/>.</summary>
        public void Open(RectTransform parent)
        {
            var pd = PD;
            if (pd == null)
            {
                Debug.LogError($"{Tag} Lineup: PersistentGameplayData is null.");
                return;
            }
            if (_root == null) Build(parent);

            _locked = pd.LinearMapNextNode > 0;
            _selected.Clear();

            if (_locked)
                SelectMusicians(pd.MusicianList);
            else if (pd.HasChosenBand)
                SelectMusicians(pd.ChosenBandRoster);
            else if (defaultPreset >= 0 && defaultPreset < presets.Count)
            {
                SelectMusicians(presets[defaultPreset].members);
                Commit(silent: true); // entering the dormitory = choosing the default
            }

            foreach (var b in _presetButtons) b.SetActive(!_locked);
            Refresh();
            _root.SetActive(true);
            _root.transform.SetAsLastSibling();

            Debug.Log($"{Tag} Ship → Dormitorio | lineup={CurrentId()} | " +
                      $"band=[{Names()}] | locked={_locked}");
        }

        private void Close()
        {
            if (!IsOpen) return;
            _root.SetActive(false);
            Debug.Log($"{Tag} Dormitorio → Ship | lineup={CurrentId()} | band=[{Names()}]");
            Closed?.Invoke();
        }

        // ---------------------------------------------------------------- selection

        private void SelectMusicians(IEnumerable<MusicianBase> musicians)
        {
            _selected.Clear();
            if (musicians == null) return;
            var set = new HashSet<MusicianBase>(musicians);
            for (int i = 0; i < cards.Count; i++)
                if (cards[i].musician != null && set.Contains(cards[i].musician))
                    _selected.Add(i);
        }

        private void OnCardClicked(ShipRoomZone zone)
        {
            int i = zone.Index;
            if (_locked)
            {
                SetStatus(lockedText);
                return;
            }
            if (cards[i].musician == null)
            {
                Debug.LogError($"{Tag} Card '{cards[i].label}' has no musician.");
                return;
            }
            if (_selected.Contains(i))
            {
                if (_selected.Count <= minMusicians)
                {
                    SetStatus(string.Format(minText, minMusicians));
                    Debug.Log($"{Tag} Lineup | '{cards[i].label}' stays: minimum {minMusicians}.");
                    return;
                }
                _selected.Remove(i);
            }
            else _selected.Add(i);

            Commit(silent: false);
            Refresh();
        }

        private void OnPresetClicked(int p)
        {
            if (_locked) return;
            SelectMusicians(presets[p].members);
            Commit(silent: false);
            Refresh();
        }

        private void Commit(bool silent)
        {
            var pd = PD;
            if (pd == null) return;
            pd.SetChosenBand(Roster(), CurrentId());
            if (!silent)
                Debug.Log($"{Tag} Lineup → {CurrentId()} | band=[{Names()}]");
        }

        /// <summary>Selected musicians in card order (= stage order).</summary>
        private List<MusicianBase> Roster()
        {
            var list = new List<MusicianBase>();
            for (int i = 0; i < cards.Count; i++)
                if (_selected.Contains(i) && cards[i].musician != null)
                    list.Add(cards[i].musician);
            return list;
        }

        private string CurrentId()
        {
            var roster = new HashSet<MusicianBase>(Roster());
            foreach (var p in presets)
            {
                var members = new HashSet<MusicianBase>(p.members);
                members.Remove(null);
                if (members.SetEquals(roster)) return p.id;
            }
            return FreeLineupId;
        }

        private string Names()
        {
            var names = new List<string>();
            for (int i = 0; i < cards.Count; i++)
                if (_selected.Contains(i)) names.Add(cards[i].label);
            return string.Join(", ", names);
        }

        private string CurrentLabel()
        {
            string id = CurrentId();
            foreach (var p in presets) if (p.id == id) return p.label;
            return "Libre";
        }

        private void Refresh()
        {
            for (int i = 0; i < _silhouettes.Count; i++)
                _silhouettes[i].SetActive(!_selected.Contains(i));
            SetStatus(_locked ? lockedText : null);
        }

        private void SetStatus(string extra)
        {
            if (_status == null) return;
            string band = $"{CurrentLabel()}: {Names()}";
            _status.text = string.IsNullOrEmpty(extra) ? band : $"{band}\n<size=75%>{extra}</size>";
        }

        // ---------------------------------------------------------------- build

        private void Build(RectTransform parent)
        {
            _root = new GameObject("LineupScreen", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)_root.transform;
            rt.SetParent(parent, false);
            Stretch(rt);
            var bg = _root.GetComponent<Image>();
            bg.color = Color.black;
            bg.raycastTarget = true; // blocks the ship zones underneath

            // Frame at the art's aspect: lit art, cards and silhouettes share it.
            var frame = new GameObject("ArtFrame", typeof(RectTransform), typeof(AspectRatioFitter));
            var frameRt = (RectTransform)frame.transform;
            frameRt.SetParent(rt, false);
            Stretch(frameRt);
            var fitter = frame.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = litArt != null ? litArt.rect.width / litArt.rect.height : 16f / 9f;

            var lit = NewImage("Lit", frameRt, litArt);
            Stretch(lit.rectTransform);

            for (int i = 0; i < cards.Count; i++)
            {
                var a = cards[i].area;
                var card = new GameObject($"Card_{cards[i].label}", typeof(RectTransform), typeof(RectMask2D));
                var cardRt = (RectTransform)card.transform;
                cardRt.SetParent(frameRt, false);
                SetAnchorsFromArea(cardRt, a);

                // Silhouette: the whole hidden art, placed so it lines up with
                // the frame; the card's mask shows only this card's window.
                var hidden = NewImage("Silhouette", cardRt, hiddenArt);
                var hRt = hidden.rectTransform;
                float bottom = 1f - (a.y + a.height);
                hRt.anchorMin = new Vector2(-a.x / a.width, -bottom / a.height);
                hRt.anchorMax = new Vector2((1f - a.x) / a.width, (1f - bottom) / a.height);
                hRt.offsetMin = Vector2.zero;
                hRt.offsetMax = Vector2.zero;
                _silhouettes.Add(hidden.gameObject);

                // Hit area on top, so the hover tint shows over the silhouette.
                var hit = new GameObject("Hit", typeof(RectTransform), typeof(Image));
                var hitRt = (RectTransform)hit.transform;
                hitRt.SetParent(cardRt, false);
                Stretch(hitRt);
                var zone = hit.AddComponent<ShipRoomZone>();
                zone.Init(i, new Color(1f, 1f, 1f, 0f), cardHover);
                zone.HoverEnter += z => z.SetHighlighted(true);
                zone.HoverExit += z => z.SetHighlighted(false);
                zone.Clicked += OnCardClicked;
                _zones.Add(zone);
            }

            var titleText = NewText("Title", rt, 44f);
            titleText.text = title;
            Place(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(1400f, 70f));

            _status = NewText("Status", rt, 40f);
            _status.textWrappingMode = TextWrappingModes.Normal;
            Place(_status.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(1400f, 110f));

            for (int p = 0; p < presets.Count; p++)
            {
                int index = p;
                _presetButtons.Add(NewButton(presets[p].label, rt, new Vector2(0f, 0f),
                    new Vector2(180f + 300f * p, 60f), () => OnPresetClicked(index)));
            }
            NewButton("Volver", rt, new Vector2(1f, 0f), new Vector2(-180f, 60f), Close);

            _root.SetActive(false);
        }

        // ---------------------------------------------------------------- UI helpers

        private static void SetAnchorsFromArea(RectTransform rt, Rect a)
        {
            rt.anchorMin = new Vector2(a.xMin, 1f - (a.y + a.height));
            rt.anchorMax = new Vector2(a.xMin + a.width, 1f - a.y);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

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

        private static Image NewImage(string name, Transform parent, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.enabled = sprite != null;
            img.raycastTarget = false;
            return img;
        }

        private static TextMeshProUGUI NewText(string name, Transform parent, float size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = Color.black;
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

            var label = NewText("Text", rt, 38f);
            Stretch(label.rectTransform);
            label.text = text;
            return go;
        }

        // ---------------------------------------------------------------- editor

#if UNITY_EDITOR
        private void Reset()
        {
            // Cards measured on «personajes.png» (5400×3058), name included.
            // Musicians and presets are wired in the Inspector.
            cards = new List<Card>
            {
                new Card { label = "Zit",    area = new Rect(0.095f, 0.18f, 0.180f, 0.63f) },
                new Card { label = "Conito", area = new Rect(0.285f, 0.18f, 0.170f, 0.62f) },
                new Card { label = "Sivi",   area = new Rect(0.475f, 0.17f, 0.185f, 0.62f) },
                new Card { label = "A-23",   area = new Rect(0.685f, 0.16f, 0.195f, 0.63f) },
            };
            presets = new List<Preset>
            {
                new Preset { id = "duo",      label = "Dúo" },
                new Preset { id = "trio",     label = "Trío" },
                new Preset { id = "cuarteto", label = "Cuarteto" },
            };
            defaultPreset = 2;
        }
#endif
    }
}
