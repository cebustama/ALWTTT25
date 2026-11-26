// Place at: Assets/Scripts/Tutorial/TutorialCardPulse.cs
using ALWTTT.Cards;
using ALWTTT.Characters.Band;
using ALWTTT.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Tutorial
{
    /// <summary>
    /// [S10] Pulsing outline on hand cards, without touching the card's own scale.
    ///
    ///   Gold  : the tutorial named the card ({$card:id} in the dialog text). It lasts
    ///           until a copy of it is played, or the song ends.
    ///   Cyan  : (from the track-list dialog on) a composition whose musician has no track
    ///           in the part a play would land in: «this card adds a new track».
    ///
    /// The outline is two procedural rounded rectangles placed BEHIND the card frame:
    /// a thin steady ring and a copy that grows and fades, once per period. Unscaled time,
    /// so it keeps pulsing during the Space pause (the moment the player looks at cards).
    /// </summary>
    public static class TutorialCardPulse
    {
        public static readonly Color Gold = new Color(1f, 0.80f, 0.25f, 1f);
        public static readonly Color Cyan = new Color(0.35f, 0.85f, 1f, 1f);

        private static readonly HashSet<string> _wanted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Cyan hint on compositions that would add a missing track.</summary>
        public static bool MissingTrackMode { get; set; }

        public static bool IsWanted(string id) => id != null && _wanted.Contains(id);

        public static void Attach(CardBase card)
        {
            if (card == null || card.GetComponent<TutorialCardPulseView>() != null) return;
            card.gameObject.AddComponent<TutorialCardPulseView>();
        }

        /// <summary>Every {$card:id} in the dialog's text (mechanic text and pages).</summary>
        public static void WantCardsNamedIn(TutorialDialogSO dialog)
        {
            if (dialog == null) return;
            var ids = new List<string>();
            TutorialTokenResolver.CollectCardIds(dialog.MechanicText, ids);
            if (dialog.Pages != null)
                foreach (var p in dialog.Pages) TutorialTokenResolver.CollectCardIds(p, ids);
            foreach (var id in ids)
                if (_wanted.Add(id))
                    Debug.Log($"[S10][TUT] contorno dorado: {id} (lo nombra {dialog.TriggerId})");
        }

        public static void Release(string id)
        {
            if (id != null && _wanted.Remove(id))
                Debug.Log($"[S10][TUT] contorno dorado apagado: {id} (jugada)");
        }

        public static void ClearWanted() => _wanted.Clear();

        public static void ResetAll()
        {
            _wanted.Clear();
            MissingTrackMode = false;
        }

        // ---- missing tracks, computed once per frame ----
        private static int _missingFrame = -1;
        private static HashSet<string> _withTrack = new HashSet<string>();

        public static bool AddsMissingTrack(CardDefinition def)
        {
            if (!MissingTrackMode || def == null || !def.IsComposition || def.IsFinisher ||
                !def.RequiresFixedPerformer) return false;
            var gig = GigManager.Instance;
            if (gig == null || gig.CompositionSession == null) return false;

            if (_missingFrame != Time.frameCount)
            {
                _missingFrame = Time.frameCount;
                _withTrack = gig.CompositionSession.GetMusicianIdsWithTrackNow();
            }

            var band = gig.CurrentMusicianCharacterList;
            if (band == null) return false;
            foreach (var m in band)
            {
                if (m == null || m.MusicianCharacterData == null) continue;
                if (m.MusicianCharacterData.CharacterType != def.FixedPerformerType) continue;
                return !_withTrack.Contains(m.CharacterId);
            }
            return false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad() => ResetAll();
    }

    /// <summary>[S10] The outline itself, one per hand card (added by TutorialCardPulse.Attach).</summary>
    public class TutorialCardPulseView : MonoBehaviour
    {
        private const float Period = 1.1f;      // seconds per pulse
        private const float Grow = 0.10f;       // the expanding ring ends 10% larger
        private const float SteadyAlpha = 0.95f;
        private const float WaveAlpha = 0.85f;

        private CardBase _card;
        private RectTransform _steady, _wave;
        private Image _steadyImg, _waveImg;
        private bool _built, _shown;

        internal static Sprite _ring;

        private void Awake() => _card = GetComponent<CardBase>();

        private void Update()
        {
            var def = _card != null ? _card.CardDefinition : null;
            Color? want = null;
            if (def != null && TutorialCardPulse.IsWanted(def.Id)) want = TutorialCardPulse.Gold;
            else if (TutorialCardPulse.AddsMissingTrack(def)) want = TutorialCardPulse.Cyan;

            if (want == null)
            {
                if (_shown) Show(false);
                return;
            }

            if (!_built && !Build()) return;
            if (!_shown) Show(true);

            float t = Mathf.Repeat(Time.unscaledTime, Period) / Period;
            var c = want.Value;
            _steadyImg.color = new Color(c.r, c.g, c.b, SteadyAlpha);
            _waveImg.color = new Color(c.r, c.g, c.b, WaveAlpha * (1f - t) * (1f - t));
            _wave.localScale = Vector3.one * (1f + Grow * t);
        }

        private void Show(bool on)
        {
            _shown = on;
            if (_steady != null) _steady.gameObject.SetActive(on);
            if (_wave != null) _wave.gameObject.SetActive(on);
        }

        private bool Build()
        {
            var frame = _card != null ? _card.ActiveFrameRect : null;
            if (frame == null || frame.parent == null) return false;
            _built = true;
            if (_ring == null) _ring = MakeRingSprite();

            int index = frame.GetSiblingIndex();
            _wave = MakeRing("S10 Pulse Wave", frame, index, out _waveImg);
            _steady = MakeRing("S10 Pulse Ring", frame, index + 1, out _steadyImg);   // still behind the frame
            Debug.Log($"[S10][TUT] contorno montado en '{_card.CardDefinition?.Id}' " +
                      $"(marco {frame.name} {frame.rect.width:0}×{frame.rect.height:0})");
            return true;
        }

        private static RectTransform MakeRing(string name, RectTransform frame, int siblingIndex, out Image img)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(frame.parent, false);
            rt.SetSiblingIndex(siblingIndex);
            rt.anchorMin = frame.anchorMin;
            rt.anchorMax = frame.anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = frame.anchoredPosition +
                                  (new Vector2(0.5f, 0.5f) - frame.pivot) * frame.rect.size;
            // A few percent outside the frame so the ring shows around the card, not under it.
            float pad = Mathf.Max(frame.rect.width, frame.rect.height) * 0.035f;
            rt.sizeDelta = frame.sizeDelta + new Vector2(pad, pad) * 2f;
            rt.localScale = Vector3.one;
            img = go.GetComponent<Image>();
            img.sprite = _ring;
            img.type = Image.Type.Sliced;
            // The 24 px slice border becomes ~6% of the card width, whatever the canvas units.
            var canvas = frame.GetComponentInParent<Canvas>();
            float refPpu = canvas != null ? canvas.referencePixelsPerUnit : 100f;
            float borderUnits = Mathf.Max(0.0001f, frame.rect.width * 0.06f);
            img.pixelsPerUnitMultiplier = 24f * refPpu / (100f * borderUnits);
            img.raycastTarget = false;
            go.SetActive(false);
            return rt;
        }

        // 64×64 rounded-rectangle ring, 9-sliced (border 24 px).
        internal static Sprite MakeRingSprite()
        {
            const int S = 64, R = 22, W = 7;
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[S * S];
            for (int y = 0; y < S; y++)
                for (int x = 0; x < S; x++)
                {
                    // Distance from the rounded rectangle's edge (signed: < 0 inside).
                    float cx = Mathf.Clamp(x + 0.5f, R, S - R), cy = Mathf.Clamp(y + 0.5f, R, S - R);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy)) - (R - 1);
                    float inner = d + W;
                    float a = Mathf.Clamp01(0.5f - d) * Mathf.Clamp01(inner + 0.5f);
                    px[y * S + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f, 0,
                                 SpriteMeshType.FullRect, new Vector4(24, 24, 24, 24));
        }
    }

    /// <summary>
    /// [S10] The same gold pulsing outline, around the song's track list (top left), while a
    /// dialog highlighting "song_panel_tracks" is open. Replaces the spotlight circle, which
    /// pointed at the old panel position (playtest 26-nov).
    /// </summary>
    public static class TutorialTracksOutline
    {
        private static GameObject _go;

        public static bool Show()
        {
            Hide();
            var rows = UnityEngine.Object.FindObjectsByType<ALWTTT.UI.SongTrackElementUI>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var rects = new List<RectTransform>();
            foreach (var r in rows)
                if (r != null && r.isActiveAndEnabled && r.transform is RectTransform rt) rects.Add(rt);
            _go = Build("S10 Contorno de pistas", rects, true);
            return _go != null;
        }

        /// <summary>[S10] The cohesion hearts (bottom left), when the dialog introduces Cohesion.</summary>
        public static bool ShowCohesion()
        {
            Hide();
            var views = UnityEngine.Object.FindObjectsByType<ALWTTT.UI.CohesionPipsView>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var rects = new List<RectTransform>();
            foreach (var v in views)
                if (v != null && v.isActiveAndEnabled && v.transform is RectTransform rt) rects.Add(rt);
            // [S10] In the gig the hearts belong to GigCanvas (its own copy, no CohesionPipsView).
            var ui = ALWTTT.Managers.UIManager.Instance;
            var gigRoot = ui != null && ui.GigCanvas != null ? ui.GigCanvas.CohesionPipRoot : null;
            if (gigRoot != null) rects.Add(gigRoot);
            _go = Build("S10 Contorno de Cohesión", rects, true);
            return _go != null;
        }

        /// <summary>[S10] A gold pulsing outline around the union of <paramref name="targets"/>
        /// (same root canvas). <paramref name="onTop"/>: its own canvas over the dialog's dim.
        /// Null when no target is on screen.</summary>
        public static GameObject Build(string name, IEnumerable<RectTransform> targets, bool onTop, float pad = 12f)
        {
            Canvas root = null;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = -min;
            int n = 0;
            var c = new Vector3[4];
            foreach (var rt in targets)
            {
                if (rt == null || !rt.gameObject.activeInHierarchy) continue;
                var cv = rt.GetComponentInParent<Canvas>();
                if (cv == null) continue;
                if (root == null) root = cv.rootCanvas;
                else if (cv.rootCanvas != root) continue;
                rt.GetWorldCorners(c);
                foreach (var w in c)
                {
                    Vector2 l = root.transform.InverseTransformPoint(w);
                    min = Vector2.Min(min, l); max = Vector2.Max(max, l);
                }
                n++;
            }
            if (n == 0 || root == null)
            {
                Debug.Log($"[S10][TUT] '{name}': nada en pantalla, sin contorno");
                return null;
            }

            var go = new GameObject(name, typeof(RectTransform));
            var box = (RectTransform)go.transform;
            box.SetParent(root.transform, false);
            box.SetAsLastSibling();
            box.anchorMin = box.anchorMax = box.pivot = new Vector2(0.5f, 0.5f);
            box.localPosition = (min + max) * 0.5f;
            box.sizeDelta = (max - min) + new Vector2(pad, pad) * 2f;
            if (onTop)
            {
                var own = go.AddComponent<Canvas>();
                own.overrideSorting = true;
                own.sortingOrder = 900;   // over the dialog's dim
            }

            if (TutorialCardPulseView._ring == null) TutorialCardPulseView._ring = TutorialCardPulseView.MakeRingSprite();
            var view = go.AddComponent<TutorialTracksOutlineView>();
            view.Wave = Ring("Onda", box, root, out view.WaveImg);
            view.Steady = Ring("Anillo", box, root, out view.SteadyImg);
            Debug.Log($"[S10][TUT] contorno dorado '{name}' ({n} objeto(s), " +
                      $"{box.sizeDelta.x:0}×{box.sizeDelta.y:0})");
            return go;
        }

        public static void Hide()
        {
            if (_go != null) UnityEngine.Object.Destroy(_go);
            _go = null;
        }

        private static RectTransform Ring(string name, RectTransform parent, Canvas root, out Image img)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            img = go.GetComponent<Image>();
            img.sprite = TutorialCardPulseView._ring;
            img.type = Image.Type.Sliced;
            float refPpu = root.referencePixelsPerUnit;
            img.pixelsPerUnitMultiplier = 24f * refPpu / (100f * 10f);   // ~10 canvas units thick
            img.raycastTarget = false;
            return rt;
        }
    }

    public class TutorialTracksOutlineView : MonoBehaviour
    {
        public RectTransform Steady, Wave;
        public Image SteadyImg, WaveImg;

        private void Update()
        {
            if (SteadyImg == null || WaveImg == null) return;
            float t = Mathf.Repeat(Time.unscaledTime, 1.1f) / 1.1f;
            var c = TutorialCardPulse.Gold;
            SteadyImg.color = new Color(c.r, c.g, c.b, 0.95f);
            WaveImg.color = new Color(c.r, c.g, c.b, 0.85f * (1f - t) * (1f - t));
            Wave.localScale = Vector3.one * (1f + 0.05f * t);
        }
    }
}
