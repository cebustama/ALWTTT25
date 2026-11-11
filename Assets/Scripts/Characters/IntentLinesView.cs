using ALWTTT.Characters.Audience;
using ALWTTT.Characters.Band;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Characters
{
    /// <summary>
    /// [S4b / LEGIB-1b] Hover views over the stress projection. Presentation only: it reads
    /// <see cref="StressProjection"/> and never changes targets or numbers.
    ///
    /// One mode per frame, by priority:
    ///   1. Tab held        → every line, no outlines (works with a card held: lines touch no outline).
    ///   2. Card held       → nothing (the card's target outline must not be fought).
    ///   3. Hover musician  → lines from the members that hit them; those members outlined in the
    ///                        musician's identityColor; every other intention faded ("[nada]").
    ///   4. Hover member    → lines to its targets, each target outlined in its own colour; the
    ///                        other members that hit the same musicians outlined and lined, fainter
    ///                        ("otros que atacan al mismo, sin detalles"); the rest faded.
    /// Always (player turn and song): musicians that will lose fortitude get the red damage
    /// outline (D5=A), under hover and views.
    ///
    /// Colour rule: a line, and an outline drawn because of a line, is the colour of the
    /// musician the line reaches. One colour, one meaning: "this goes to that musician".
    ///
    /// Driven by GigManager.RefreshStressProjection, after the projection is rebuilt, so it
    /// always reads the same frame's numbers.
    /// </summary>
    public sealed class IntentLinesView : MonoBehaviour
    {
        [Header("Lines")]
        [Tooltip("Optional. Null = Sprites/Default with a generated dash texture.")]
        [SerializeField] private Material lineMaterial;
        [SerializeField] private float lineWidth = 0.08f;
        [Tooltip("Dash repeats per world unit (LineRenderer.textureScale.x).")]
        [SerializeField] private float dashesPerUnit = 2.5f;
        [Tooltip("Extra arc height, world units, on top of 20% of the distance.")]
        [SerializeField] private float arcHeight = 0.6f;
        [Tooltip("Gap above each sprite's top where the line starts / ends.")]
        [SerializeField] private float headGap = 0.15f;
        [SerializeField, Range(4, 40)] private int segments = 24;
        [SerializeField] private int sortingOrder = 100;
        [Tooltip("Alpha of the fainter lines in the member view (others hitting the same musician).")]
        [SerializeField, Range(0f, 1f)] private float secondaryAlpha = 0.45f;

        [Header("Input")]
        [SerializeField] private KeyCode showAllKey = KeyCode.Tab;

        private enum Mode { None, All, Musician, Audience, CardHeld }

        private struct Link
        {
            public AudienceCharacterBase From;
            public MusicianBase To;
            public bool Secondary;
        }

        private readonly List<Link> _links = new();
        private readonly List<LineRenderer> _pool = new();
        private readonly Dictionary<CharacterBase, Color> _viewOutline = new();
        private readonly List<CharacterBase> _outlined = new();
        private readonly HashSet<AudienceCharacterBase> _focused = new();
        private readonly HashSet<MusicianBase> _damaged = new();
        private readonly HashSet<MusicianBase> _shared = new();
        private readonly List<MusicianBase> _damagedLogScratch = new();
        private Material _runtimeMaterial;
        private Vector3[] _points;

        private Mode _lastMode = Mode.None;
        private CharacterBase _lastFocus;
        private bool _wasActive;

        /// <summary>Called every LateUpdate by GigManager. <paramref name="active"/> is false
        /// outside the player turn and the song: everything is cleared.</summary>
        public void Refresh(StressProjection projection, bool active,
            List<MusicianBase> musicians, List<AudienceCharacterBase> audience, bool holdingCard)
        {
            _links.Clear();
            _viewOutline.Clear();
            _focused.Clear();

            Mode mode = Mode.None;
            CharacterBase focus = null;

            if (active && projection != null)
            {
                var hovered = CharacterBase.Hovered;
                if (Input.GetKey(showAllKey)) mode = Mode.All;
                else if (holdingCard) mode = Mode.CardHeld;
                else if (hovered is MusicianBase hm && musicians != null && musicians.Contains(hm))
                { mode = Mode.Musician; focus = hm; }
                else if (hovered is AudienceCharacterBase ha && audience != null && audience.Contains(ha))
                { mode = Mode.Audience; focus = ha; }

                BuildLinks(mode, focus, projection, audience);
            }

            DrawLines();
            ApplyOutlines(musicians);
            ApplyDimming(mode, audience);
            ApplyDamage(active, projection, musicians);

            if (mode != _lastMode || focus != _lastFocus || active != _wasActive)
            {
                LogMode(mode, focus);
                _lastMode = mode;
                _lastFocus = focus;
                _wasActive = active;
            }
        }

        // ---- what to draw ----

        private void BuildLinks(Mode mode, CharacterBase focus, StressProjection p,
            List<AudienceCharacterBase> audience)
        {
            if (audience == null) return;

            switch (mode)
            {
                case Mode.All:
                    foreach (var a in audience)
                        foreach (var m in TargetsOf(p, a))
                            _links.Add(new Link { From = a, To = m });
                    break;

                case Mode.Musician:
                {
                    var hm = (MusicianBase)focus;
                    foreach (var a in audience)
                        if (TargetsOf(p, a).Contains(hm))
                        {
                            _links.Add(new Link { From = a, To = hm });
                            _focused.Add(a);
                            _viewOutline[a] = ColorOf(hm);
                        }
                    break;
                }

                case Mode.Audience:
                {
                    var ha = (AudienceCharacterBase)focus;
                    _focused.Add(ha);
                    _shared.Clear();
                    foreach (var m in TargetsOf(p, ha))
                    {
                        _links.Add(new Link { From = ha, To = m });
                        _viewOutline[m] = ColorOf(m);
                        _shared.Add(m);
                    }
                    foreach (var b in audience)
                    {
                        if (b == ha) continue;
                        foreach (var m in TargetsOf(p, b))
                        {
                            if (!_shared.Contains(m)) continue;
                            _links.Add(new Link { From = b, To = m, Secondary = true });
                            _focused.Add(b);
                            if (!_viewOutline.ContainsKey(b)) _viewOutline[b] = ColorOf(m);
                        }
                    }
                    break;
                }
            }
        }

        private static readonly List<MusicianBase> NoTargets = new();

        private static List<MusicianBase> TargetsOf(StressProjection p, AudienceCharacterBase a)
        {
            if (a == null || !p.ByAudience.TryGetValue(a, out var s) || s == null) return NoTargets;
            return s.Targets;
        }

        private static Color ColorOf(MusicianBase m)
        {
            var c = m != null && m.MusicianCharacterData != null
                ? m.MusicianCharacterData.IdentityColor : Color.white;
            c.a = 1f;
            return c;
        }

        // ---- lines ----

        private void DrawLines()
        {
            for (int i = 0; i < _links.Count; i++)
            {
                var link = _links[i];
                var lr = i < _pool.Count ? _pool[i] : CreateLine(i);
                if (link.From == null || link.To == null) { lr.enabled = false; continue; }

                Vector3 a = TopOf(link.From), b = TopOf(link.To);
                float dist = Vector3.Distance(a, b);
                Vector3 ctrl = (a + b) * 0.5f + Vector3.up * (arcHeight + 0.2f * dist);

                if (_points == null || _points.Length != segments + 1) _points = new Vector3[segments + 1];
                for (int k = 0; k <= segments; k++)
                {
                    float t = k / (float)segments, u = 1f - t;
                    _points[k] = u * u * a + 2f * u * t * ctrl + t * t * b;
                }
                lr.positionCount = _points.Length;
                lr.SetPositions(_points);

                var c = ColorOf(link.To);
                if (link.Secondary) c.a = secondaryAlpha;
                lr.startColor = lr.endColor = c;
                lr.widthMultiplier = link.Secondary ? lineWidth * 0.75f : lineWidth;
                lr.textureScale = new Vector2(dashesPerUnit, 1f);

                var sr = link.To.SpriteRenderer;
                if (sr != null) lr.sortingLayerID = sr.sortingLayerID;
                lr.sortingOrder = sortingOrder;
                lr.enabled = true;
            }
            for (int i = _links.Count; i < _pool.Count; i++)
                _pool[i].enabled = false;
        }

        private Vector3 TopOf(CharacterBase c)
        {
            var sr = c.SpriteRenderer;
            if (sr == null) return c.transform.position + Vector3.up * headGap;
            var bounds = sr.bounds;
            return new Vector3(bounds.center.x, bounds.max.y + headGap, bounds.center.z);
        }

        private LineRenderer CreateLine(int index)
        {
            var go = new GameObject($"IntentLine{index}");
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.sharedMaterial = lineMaterial != null ? lineMaterial : RuntimeMaterial();
            lr.textureMode = LineTextureMode.Tile;
            lr.numCapVertices = 2;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.enabled = false;
            _pool.Add(lr);
            return lr;
        }

        private Material RuntimeMaterial()
        {
            if (_runtimeMaterial != null) return _runtimeMaterial;

            // 8 px: 5 on, 3 off. Repeat + point filter = crisp dashes at any length.
            var tex = new Texture2D(8, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point,
                name = "S4b_Dash"
            };
            for (int x = 0; x < 8; x++)
                tex.SetPixel(x, 0, x < 5 ? Color.white : new Color(1f, 1f, 1f, 0f));
            tex.Apply();

            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
                Debug.LogWarning("[S4b] Sprites/Default not found: assign a lineMaterial on IntentLinesView.");
            _runtimeMaterial = new Material(shader) { name = "S4b_IntentLine", mainTexture = tex };
            return _runtimeMaterial;
        }

        // ---- outlines, dimming, damage ----

        private void ApplyOutlines(List<MusicianBase> musicians)
        {
            for (int i = _outlined.Count - 1; i >= 0; i--)
            {
                var c = _outlined[i];
                if (c != null && _viewOutline.ContainsKey(c)) continue;
                if (c != null && c.SpriteOutline != null) c.SpriteOutline.SetViewHighlight(false, default);
                _outlined.RemoveAt(i);
            }
            foreach (var kv in _viewOutline)
            {
                if (kv.Key == null || kv.Key.SpriteOutline == null) continue;
                kv.Key.SpriteOutline.SetViewHighlight(true, kv.Value);
                if (!_outlined.Contains(kv.Key)) _outlined.Add(kv.Key);
            }
        }

        private void ApplyDimming(Mode mode, List<AudienceCharacterBase> audience)
        {
            if (audience == null) return;
            bool viewOn = mode == Mode.Musician || mode == Mode.Audience;
            foreach (var a in audience)
            {
                if (a == null || a.AudienceCharacterCanvas == null) continue;
                a.AudienceCharacterCanvas.SetIntentDimmed(viewOn && !_focused.Contains(a));
            }
        }

        private void ApplyDamage(bool active, StressProjection p, List<MusicianBase> musicians)
        {
            if (musicians == null) return;
            bool changed = false;
            foreach (var m in musicians)
            {
                if (m == null) continue;
                int loss = 0;
                bool hit = active && p != null && p.IncomingByMusician.TryGetValue(m, out loss) && loss > 0;
                if (m.SpriteOutline != null) m.SpriteOutline.SetDamageHighlight(hit);
                if (hit ? _damaged.Add(m) : _damaged.Remove(m)) changed = true;
            }
            if (!changed) return;

            _damagedLogScratch.Clear();
            _damagedLogScratch.AddRange(_damaged);
            var names = new List<string>();
            foreach (var m in _damagedLogScratch) names.Add(NameOf(m));
            Debug.Log($"[S4b] Aura roja (recibirán daño): {(names.Count == 0 ? "nadie" : string.Join(", ", names))}");
        }

        // ---- log (once per mode / focus change, never per frame) ----

        private void LogMode(Mode mode, CharacterBase focus)
        {
            switch (mode)
            {
                case Mode.All:
                    Debug.Log($"[S4b] Tab: todas las líneas ({_links.Count})");
                    break;
                case Mode.CardHeld:
                    Debug.Log("[S4b] Carta cogida: vistas de hover apagadas");
                    break;
                case Mode.Musician:
                    Debug.Log($"[S4b] Vista 1, músico {NameOf(focus)}: le golpean {_links.Count} miembro(s): {FromNames()}");
                    break;
                case Mode.Audience:
                {
                    int primary = 0;
                    foreach (var l in _links) if (!l.Secondary) primary++;
                    Debug.Log($"[S4b] Vista 2, público {NameOf(focus)}: {primary} objetivo(s); " +
                              $"{_links.Count - primary} línea(s) de otros que golpean al mismo");
                    break;
                }
                default:
                    Debug.Log("[S4b] Sin vista");
                    break;
            }
        }

        private string FromNames()
        {
            var names = new List<string>();
            foreach (var l in _links) names.Add(NameOf(l.From));
            return names.Count == 0 ? "nadie" : string.Join(", ", names);
        }

        private static string NameOf(CharacterBase c)
        {
            if (c == null) return "?";
            if (c is MusicianBase m && m.MusicianCharacterData != null)
                return m.MusicianCharacterData.CharacterName;
            return c.name;
        }

        private void OnDestroy()
        {
            if (_runtimeMaterial != null)
            {
                Destroy(_runtimeMaterial.mainTexture);
                Destroy(_runtimeMaterial);
            }
        }
    }
}
