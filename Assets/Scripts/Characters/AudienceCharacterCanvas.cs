using ALWTTT.Data;
using ALWTTT.Tooltips;
using static ALWTTT.Tooltips.GameTextTemplateIds;   // [TXT-4]
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Characters
{
    public class AudienceCharacterCanvas : CharacterCanvas
    {
        [Header("Audience Character Canvas Settings")]
        [SerializeField] private Image intentImage;
        [SerializeField] private TextMeshProUGUI nextActionValueText;
        public Image IntentImage => intentImage;
        public TextMeshProUGUI NextActionValueText => nextActionValueText;

        // [LEGIB-1b / view 3] Who the intention hits, as the musicians' own icons
        // (MusicianCharacterData.characterIcon) to the right of the intention icon. Built at
        // runtime as children of the intention image, so they hide with it (AbilityRoutine,
        // convinced concealment) and need no prefab change. Units are the intention image's
        // local space (30 x 30 in the prefab).
        [Header("Intent targets (LEGIB-1b)")]
        [SerializeField] private float targetIconSize = 40f;
        [SerializeField] private float targetIconGap = 4f;
        [SerializeField] private Vector2 targetIconOffset = new Vector2(14f, 0f);

        // [S4b / D1=B] Legibility over the background: each face sits on a dark disc ringed
        // in the musician's identityColor (the same colour as their lines and outline).
        [Tooltip("[S4b] Dark disc behind each target face.")]
        [SerializeField] private Color targetDiscColor = new Color(0.07f, 0.07f, 0.10f, 0.9f);
        [Tooltip("[S4b] Ring width, in the intention image's local units.")]
        [SerializeField, Min(0f)] private float targetRingWidth = 3f;
        [Tooltip("[S4b] Face size as a fraction of the disc.")]
        [SerializeField, Range(0.3f, 1f)] private float targetFaceScale = 0.8f;

        // [S4b / D10=C] Many targets. The whole band is ONE icon: a ring split in the
        // identity colours of the musicians hit, with "xN" in the middle (Slay the Spire marks
        // an attack on everyone with one icon, not a list). Two or more targets that are not
        // the whole band overlap like an avatar stack, so the group keeps a bounded width.
        // Who exactly is hit stays readable in the hover lines and the tooltip phrase.
        [Tooltip("[S4b / D10] Overlap between stacked faces (0 = side by side, 0.6 = mostly hidden).")]
        [SerializeField, Range(0f, 0.8f)] private float targetStackOverlap = 0.55f;
        [Tooltip("[S4b / D10] Show one 'whole band' icon when every musician is hit.")]
        [SerializeField] private bool groupWholeBand = true;

        private sealed class TargetIcon { public GameObject Root; public Image Ring, Disc, Face; }
        private sealed class AllIcon
        {
            public GameObject Root; public Image Disc; public TextMeshProUGUI Label;
            public readonly System.Collections.Generic.List<Image> Segments = new();
        }
        private AllIcon _allIcon;
        private string _lastLayoutLog;
        private readonly System.Collections.Generic.List<TargetIcon> _targetIcons = new();
        private static Sprite s_circle;
        private int _shownAmount = int.MinValue;
        private readonly System.Collections.Generic.List<Sprite> _shownSprites = new();

        /// <summary>[LEGIB-1b] No number, no target icons (new intention, before projecting).</summary>
        public void ClearIntentSummary()
        {
            _shownAmount = int.MinValue;
            _shownSprites.Clear();
            _hitTargets.Clear();
            if (nextActionValueText != null) nextActionValueText.gameObject.SetActive(false);
            foreach (var t in _targetIcons)
                if (t.Root != null) t.Root.SetActive(false);
            if (_allIcon != null) _allIcon.Root.SetActive(false);
        }

        /// <summary>
        /// [LEGIB-1b / view 3] Number (Stress per target, attacker modifiers included) and the
        /// icons of the musicians the intention reaches. Called every frame by GigManager;
        /// returns early when nothing changed.
        /// </summary>
        public void SetIntentSummary(AudienceIntentSummary s)
        {
            if (s == null) { ClearIntentSummary(); return; }

            int amount = s.ShowAmount ? s.Amount : -1;
            bool same = amount == _shownAmount && _shownSprites.Count == s.Targets.Count;
            for (int i = 0; same && i < s.Targets.Count; i++)
                same = _shownSprites[i] == IconOf(s.Targets[i]);
            if (same) return;

            _shownAmount = amount;
            _shownSprites.Clear();
            foreach (var m in s.Targets) _shownSprites.Add(IconOf(m));
            _hitTargets.Clear();
            _hitTargets.AddRange(s.Targets);
            if (_iconHovered || _bodyHovered) RefreshHoverTooltip(); // the phrase follows the projection

            if (nextActionValueText != null)
            {
                nextActionValueText.gameObject.SetActive(s.ShowAmount);
                if (s.ShowAmount) nextActionValueText.text = s.Amount.ToString();
            }

            if (intentImage == null) return;

            int n = _shownSprites.Count;
            bool whole = groupWholeBand && IsWholeBand(n);
            if (whole)
            {
                ShowAllIcon(s.Targets);
                foreach (var t in _targetIcons) t.Root.SetActive(false);
                LogLayout($"[S4b] Icono de toda la banda (x{n}): {name}");
                return;
            }
            if (_allIcon != null) _allIcon.Root.SetActive(false);

            float step = n > 1 ? targetIconSize * (1f - targetStackOverlap) : targetIconSize + targetIconGap;
            for (int i = 0; i < n; i++)
            {
                if (i >= _targetIcons.Count) _targetIcons.Add(CreateTargetIcon(i));
                var t = _targetIcons[i];
                t.Face.sprite = _shownSprites[i];
                var m = s.Targets[i];
                t.Ring.color = ColorOf(m);
                ((RectTransform)t.Root.transform).anchoredPosition = targetIconOffset + new Vector2(i * step, 0f);
                t.Root.transform.SetSiblingIndex(i); // later faces on top
                t.Root.SetActive(_shownSprites[i] != null);
            }
            for (int i = n; i < _targetIcons.Count; i++)
                _targetIcons[i].Root.SetActive(false);
            if (n > 1) LogLayout($"[S4b] Caras apiladas ({n}): {name}");
        }

        // "All" = more than one target and at least the band on stage (S6: 2, 3 or 4).
        private static bool IsWholeBand(int count)
        {
            var gm = ALWTTT.Managers.GigManager.Instance;
            int bandSize = gm != null ? gm.CurrentMusicianCharacterList.Count : 0;
            return count > 1 && count >= bandSize;
        }

        private static Color ColorOf(Band.MusicianBase m) =>
            m != null && m.MusicianCharacterData != null ? m.MusicianCharacterData.IdentityColor : Color.white;

        private void LogLayout(string msg)
        {
            if (msg == _lastLayoutLog) return;
            _lastLayoutLog = msg;
            Debug.Log(msg);
        }

        private void ShowAllIcon(System.Collections.Generic.IReadOnlyList<Band.MusicianBase> targets)
        {
            if (_allIcon == null) _allIcon = CreateAllIcon();
            int n = targets.Count;
            while (_allIcon.Segments.Count < n)
            {
                var seg = NewImage($"Segment{_allIcon.Segments.Count}", _allIcon.Root.transform, targetIconSize);
                seg.sprite = Circle();
                seg.type = Image.Type.Filled;
                seg.fillMethod = Image.FillMethod.Radial360;
                seg.fillOrigin = (int)Image.Origin360.Top;
                seg.fillClockwise = true;
                seg.raycastTarget = true; // [S4b] hovering "xN" opens the member's column
                _allIcon.Segments.Add(seg);
            }
            for (int k = 0; k < _allIcon.Segments.Count; k++)
            {
                var seg = _allIcon.Segments[k];
                bool on = k < n;
                seg.gameObject.SetActive(on);
                if (!on) continue;
                seg.fillAmount = 1f / n;
                seg.color = ColorOf(targets[k]);
                seg.rectTransform.localEulerAngles = new Vector3(0f, 0f, -360f * k / n);
                seg.transform.SetSiblingIndex(k); // under the disc and the label
            }
            _allIcon.Label.text = "\u00D7" + n;
            _allIcon.Root.SetActive(true);
        }

        private AllIcon CreateAllIcon()
        {
            var go = new GameObject("IntentTargetAll", typeof(RectTransform));
            go.layer = intentImage.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(intentImage.transform, false);
            rt.sizeDelta = new Vector2(targetIconSize, targetIconSize);
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = targetIconOffset;

            var icon = new AllIcon { Root = go };
            icon.Disc = NewImage("Disc", rt, Mathf.Max(0f, targetIconSize - 2f * targetRingWidth));
            icon.Disc.sprite = Circle();
            icon.Disc.color = targetDiscColor;

            var lgo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lgo.layer = go.layer;
            var lrt = (RectTransform)lgo.transform;
            lrt.SetParent(rt, false);
            lrt.sizeDelta = new Vector2(targetIconSize, targetIconSize);
            var label = lgo.GetComponent<TextMeshProUGUI>();
            if (nextActionValueText != null) label.font = nextActionValueText.font;
            label.fontSize = targetIconSize * 0.45f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            icon.Label = label;
            return icon;
        }

        // [S4b] Hover views fade the intention of members that have nothing to do with what
        // the player is looking at (boceto: "[nada]"). CanvasGroups on the icon and the number,
        // so AbilityRoutine / concealment keep toggling the GameObjects as before.
        [Tooltip("[S4b] Alpha of the intention (icon, number, targets) on members outside the hover view.")]
        [SerializeField, Range(0f, 1f)] private float unfocusedIntentAlpha = 0f;
        private CanvasGroup _intentGroup, _valueGroup;
        private bool _intentDimmed;

        /// <summary>[S4b] Dim (true) or restore (false) this member's intention. Idempotent.</summary>
        public void SetIntentDimmed(bool dimmed)
        {
            if (_intentDimmed == dimmed) return;
            _intentDimmed = dimmed;
            float alpha = dimmed ? unfocusedIntentAlpha : 1f;
            if (intentImage != null)
            {
                if (_intentGroup == null && !intentImage.TryGetComponent(out _intentGroup))
                    _intentGroup = intentImage.gameObject.AddComponent<CanvasGroup>();
                _intentGroup.alpha = alpha;
            }
            if (nextActionValueText != null)
            {
                if (_valueGroup == null && !nextActionValueText.TryGetComponent(out _valueGroup))
                    _valueGroup = nextActionValueText.gameObject.AddComponent<CanvasGroup>();
                _valueGroup.alpha = alpha;
            }
        }

        // [S4b] Placement. The canvas sits at a fixed height on every member, so on short
        // sprites the intention floated well above the head, and the target faces grew to the
        // right only, over the neighbour's UI. While the bar is hidden the whole group
        // (icon + number + faces) is centred on the sprite and parked just above its head;
        // while the bar shows, the prefab position (above the bar) is kept.
        [Header("Intent placement (S4b)")]
        [SerializeField] private bool placeIntentOverHead = true;
        [Tooltip("[S4b] Gap between the top of the sprite and the bottom of the intention, in world units.")]
        [SerializeField] private float headGapWorld = 0.1f;

        private bool _intentHomeCaptured, _wasOverHead;
        private Vector2 _intentHome;
        private CharacterBase _owner;

        private void LateUpdate()
        {
            if (!placeIntentOverHead || intentImage == null) return;
            var rt = intentImage.rectTransform;
            if (!_intentHomeCaptured) { _intentHome = rt.anchoredPosition; _intentHomeCaptured = true; }

            bool barShown = healthBar != null && healthBar.CanvasGroup != null && healthBar.CanvasGroup.alpha > 0.01f;
            if (_owner == null) _owner = GetComponentInParent<CharacterBase>();
            var sr = _owner != null ? _owner.SpriteRenderer : null;
            var parent = rt.parent as RectTransform;

            // Centre of the group, in the intention's local units: icon spans [-w/2, w/2],
            // faces start at w/2 + offset.x and take size + gap each.
            // [S4b / D10] faces may overlap, so the right edge is the furthest active icon.
            float w = rt.rect.width;
            float right = w * 0.5f;
            foreach (var t in _targetIcons)
                if (t.Root != null && t.Root.activeSelf)
                    right = Mathf.Max(right, w * 0.5f + ((RectTransform)t.Root.transform).anchoredPosition.x + targetIconSize);
            if (_allIcon != null && _allIcon.Root.activeSelf)
                right = Mathf.Max(right, w * 0.5f + targetIconOffset.x + targetIconSize);
            float centreLocal = (-w * 0.5f + right) * 0.5f;
            float shiftX = -centreLocal * rt.localScale.x;

            if (barShown || sr == null || parent == null)
            {
                rt.anchoredPosition = _intentHome + new Vector2(shiftX, 0f);
                if (_wasOverHead) { _wasOverHead = false; }
                return;
            }

            var b = sr.bounds;
            Vector3 headWorld = new Vector3(b.center.x, b.max.y + headGapWorld, rt.position.z);
            Vector3 headLocal = parent.InverseTransformPoint(headWorld);
            float halfH = rt.rect.height * 0.5f * rt.localScale.y;
            // anchoredPosition is relative to the anchor; the prefab uses centred anchors.
            Vector2 anchorRef = (rt.anchorMin + rt.anchorMax) * 0.5f;
            Vector2 anchorPos = new Vector2(
                (anchorRef.x - parent.pivot.x) * parent.rect.width,
                (anchorRef.y - parent.pivot.y) * parent.rect.height);
            rt.anchoredPosition = new Vector2(headLocal.x - anchorPos.x + shiftX, headLocal.y - anchorPos.y + halfH);
            if (!_wasOverHead)
            {
                _wasOverHead = true;
                Debug.Log($"[S4b] Intencion sobre la cabeza (barra oculta): {name}");
            }
        }

        // [S4b / D4=A] "A Conito: 5 de Stress" -- who the ability hits and how much, from the
        // same projection that draws the number and the faces. Hidden values stay hidden (D2=A).
        private readonly System.Collections.Generic.List<Band.MusicianBase> _hitTargets = new();
        private string _lastHitLog;

        private string BuildHitPhrase()
        {
            if (_hitTargets.Count == 0) return null;
            bool show = _shownAmount >= 0;
            bool all = IsWholeBand(_hitTargets.Count);
            if (all)
                return show ? GameText.Format(UiAudHitAll, _shownAmount) : GameText.Format(UiAudHitAllHidden);

            var names = new System.Text.StringBuilder();
            for (int i = 0; i < _hitTargets.Count; i++)
            {
                if (i > 0) names.Append(", ");
                names.Append(_hitTargets[i] != null ? _hitTargets[i].CharacterName : "?");
            }
            if (!show) return GameText.Format(UiAudHitHidden, names.ToString());
            return GameText.Format(_hitTargets.Count == 1 ? UiAudHitOne : UiAudHitEach, names.ToString(), _shownAmount);
        }

        private static Sprite IconOf(Band.MusicianBase m) =>
            m != null && m.MusicianCharacterData != null ? m.MusicianCharacterData.CharacterIcon : null;

        // Root = ring (identityColor circle), child disc (dark circle, inset by the ring
        // width), child face (musician icon). Children of the intention image, as before.
        private TargetIcon CreateTargetIcon(int index)
        {
            var root = NewImage($"IntentTarget{index}", intentImage.transform, targetIconSize);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = targetIconOffset + new Vector2(index * (targetIconSize + targetIconGap), 0f);
            root.sprite = Circle();
            root.raycastTarget = true; // [S4b] hovering a face opens the same column as the intention icon

            var disc = NewImage("Disc", rt, Mathf.Max(0f, targetIconSize - 2f * targetRingWidth));
            disc.sprite = Circle();
            disc.color = targetDiscColor;

            var face = NewImage("Face", rt, targetIconSize * targetFaceScale);
            face.preserveAspect = true;

            if (index == 0)
                Debug.Log($"[S4b] Iconos de objetivo con disco y aro: {name}");
            return new TargetIcon { Root = root.gameObject, Ring = root, Disc = disc, Face = face };
        }

        private Image NewImage(string goName, Transform parent, float size)
        {
            var go = new GameObject(goName, typeof(RectTransform), typeof(Image));
            go.layer = intentImage.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(size, size);
            var img = go.GetComponent<Image>();
            // Discs, faces and labels stay non-raycast; the ring / segments catch the pointer
            // and the event bubbles to this CharacterCanvas (same handler as the intention icon).
            img.raycastTarget = false;
            return img;
        }

        // Runtime anti-aliased circle, shared by every canvas. Built-in UI/Skin/Knob is an
        // editor resource and is not guaranteed in a player build.
        private static Sprite Circle()
        {
            if (s_circle != null) return s_circle;
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false)
            {
                name = "S4b_TargetCircle", wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave
            };
            var px = new Color32[n * n];
            float r = n * 0.5f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = x + 0.5f - r, dy = y + 0.5f - r;
                    float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy));
                    px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            s_circle = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
            s_circle.hideFlags = HideFlags.DontSave;
            return s_circle;
        }


        // [CONV-TINT-1b / D-H2=A] State-driven concealment of the whole canvas. A
        // CanvasGroup on the canvas root multiplies with every child group (the bar's
        // own S5e-ext group included), so child visibility rules keep running and are
        // simply not seen; lifting the concealment shows whatever they decided meanwhile.
        // Surfaces routed to the GLOBAL TooltipManager live outside this group, so those
        // are gated explicitly on _convincedHidden.
        private CanvasGroup _stateGroup;
        private bool _convincedHidden;

        public bool IsConvincedHidden => _convincedHidden;

        public void SetConvincedHidden(bool hidden)
        {
            _convincedHidden = hidden;

            // TryGetComponent, not "GetComponent() ?? AddComponent()": in the Editor a
            // missing component can come back as a fake-null object that ?? treats as real.
            if (_stateGroup == null && !TryGetComponent(out _stateGroup))
                _stateGroup = gameObject.AddComponent<CanvasGroup>();

            _stateGroup.alpha = hidden ? 0f : 1f;
            _stateGroup.blocksRaycasts = !hidden;
            _stateGroup.interactable = !hidden;
        }

        public AudienceAbilityData NextAbility;
        private AudienceIntentionData CurrentIntention => NextAbility.Intention;

        // [S5a] Vibe Telegraph (C2 effectiveness + C3 projected number). All fields
        // optional/null-guarded so an unwired prefab degrades to "no telegraph"
        // rather than NRE (UI-fix-A recurrence vector). vibeTelegraphRoot is the
        // parent toggled for show/hide; the two texts live under it.
        [Header("Vibe Telegraph (S5a)")]
        [SerializeField] private GameObject vibeTelegraphRoot;
        [SerializeField] private TextMeshProUGUI effectivenessLabel; // C2
        [SerializeField] private TextMeshProUGUI projectedVibeText;  // C3

        // [S5a-SMOKE] True when all three telegraph slots are wired on the prefab.
        // GigManager logs this once per audience at song start (ST-S5a-9 wiring check).
        public bool IsVibeTelegraphWired =>
            vibeTelegraphRoot != null && effectivenessLabel != null && projectedVibeText != null;

        // [R4 / D-R4-4=A -> PRES-1 / D-PRES1-3=A] Taste reveal.
        //
        // R4 shipped this as a persistent panel on the audience canvas. PRES-1
        // keeps WHERE it lives (D-R4-4=A is not reopened) and changes only HOW it
        // presents: the taste TEXT now composes into the single hover tooltip, and
        // an icon is the discreet persistent "revealed" marker. Rationale: one
        // hover surface per character is a canvas invariant, and the persistent
        // panel also overlapped the "Songs left" readout.
        [Header("Taste Reveal (R4 -> PRES-1)")]
        [Tooltip("[PRES-1] Persistent 'tastes revealed' marker. Null-guarded: an " +
                 "unwired icon degrades to no marker; the tooltip still works.")]
        [SerializeField] private GameObject revealedTasteIcon;

        // [R5-pre, 2026-08-11] The retired persistent-panel slots (tastePanelRoot /
        // tasteText) were REMOVED here, together with the TastePanel object in the
        // audience prefab. They were kept inert at PRES-1 only so live wiring would
        // not dangle before that prefab pass. Reveal surface: the hover tooltip
        // (SSoT_Audience_and_Reactions §6.4).

        // [PRES-1] Cached reveal state for tooltip composition. The AUTHORITATIVE
        // "has been revealed" flag still lives on AudienceCharacterBase and the
        // data still lives on AudienceCharacterData (SSoT_Audience_and_Reactions
        // §6.4); this is a presentation cache, not a second source of truth.
        private TastePreferences _revealedTaste;
        private bool _tasteRevealed;

        /// <summary>
        /// [R4-SMOKE -> PRES-1] SEMANTIC SHIFT with D-PRES1-3=A: this used to mean
        /// "both panel slots wired". It now means "the persistent icon is wired".
        /// The tooltip composition needs no prefab wiring at all and works
        /// regardless of this flag.
        /// </summary>
        public bool IsTastePanelWired => revealedTasteIcon != null;

        // [PRES-1] ESP copy, hardcoded per D-S5f-7=A / D-S5f-8=A like the Blocked
        // tooltip and telegraph labels; all migrate together in the S5f-ext pass.
        //
        // [PRES-1c / D-PRES1c-1=A] TMP rich text: the block header is bolded so it
        // reads as a SECTION heading rather than as one more taste line. Without it
        // the taste block ran on visually from the intention text, which is what
        // ST-PRES1-7 surfaced. If the tooltip body's TMP has richText disabled the
        // tags render literally — that is a prefab fix, not a code one (see
        // ST-PRES1-7b).
        // [TXT-4 / F-TXT-2-1] Words from the glossary (ui_aud_*); the bold wrapper is formatting and stays here.
        private static string TasteBlockHeader => "<b>" + GameText.Format(UiAudTasteBlockHeader) + "</b>";
        private static string TasteOnlyHeader => GameText.Format(UiAudTasteHeader);


        // [BIGNUM-1 / D-BN-9=A] The step-by-step Vibe breakdown composes into the SAME
        // hover tooltip as intention + tastes (PRES-1 invariant: one hover surface per
        // character). This is a presentation cache of what GigManager projected — not
        // a second source of truth; GigManager.BuildVibeProjection owns the numbers.
        // ESP copy hardcoded like the rest of this canvas (D-S5f-7=A).
        // [BIGNUM-1r] VibeBlockHeader retired with the D-BN-9 revert; the bar tooltip
        // uses VibeOnlyHeader as its plain header.
        // [TEXT-VIBE-1 / F-TV-3] Was a literal left behind in population 5. Same word in EN/ES today,
        // still a player surface: it reads the `vibe` keyword concept, like the Flow/Captivated lines.
        private static string VibeOnlyHeader => GameText.ConceptName("vibe");
        private VibeProjection _projection;
        private bool _hasProjection;

        private bool _barHovered;

        /// <summary>[BIGNUM-1r / D-BN-15] Set by VibeBarTooltipTarget (child, runs first).</summary>
        public void NotifyBarHover(bool hovered) { _barHovered = hovered; }

        /// <summary>[BIGNUM-1r / D-BN-14=A] The step-by-step breakdown, on the bar's hover only.</summary>
        public void ShowVibeTooltip()
        {
            if (!_hasProjection) return;
            if (_convincedHidden) return; // [CONV-TINT-1b] TooltipManager is outside the group
            HideTooltipInfo(TooltipManager.Instance); // [GIG-UI-1] replace a body-hover column, don't stack on it
            ShowPanel(BuildVibeText(in _projection), VibeOnlyHeader, TooltipKind.Default);
        }

        public void HideVibeTooltip()
        {
            HideTooltipInfo(TooltipManager.Instance);
            RefreshHoverTooltip(); // [GIG-UI-1] back to the body/icon column if still hovered
        }

        /// <summary>
        /// [BIGNUM-1r / D-BN-14=A] Drop the predicted segment. GigManager calls this
        /// right before the song-end ApplyIncomingVibe, so the real bar lerps down
        /// through where the ghost was — prediction becomes payment on screen.
        /// </summary>
        public void ClearPredictedVibe()
        {
            healthBar?.SetPredictedLoss(0);
        }

        // [BIGNUM-1 / D-BN-4] "KO" tint for the C3 number when this song would convince
        // the member (projected >= remaining resistance). The default tint is captured
        // from the prefab the first time the number shows, so an unstyled prefab keeps
        // its own colour and nothing else changes.
        [Header("Vibe Telegraph — KO (BIGNUM-1)")]
        [SerializeField] private Color projectedKoColor = new Color(1f, 0.85f, 0.25f);
        private Color _projectedDefaultColor;
        private bool _projectedDefaultCached;

        // Lines follow the PAYOUT order (SSoT_Scoring 6.1 -> 7.1 -> 6.2 -> gate).
        // [TEXT-VIBE-1 / D1, F-TIP-6] A step line prints only when its OUTPUT differs from its INPUT,
        // compared on RAW values in the int domain each step emits; the tastes step rounds its float
        // input with the same Mathf.RoundToInt that GigManager.ComputeLPart uses. "x 1,00" is NOT the
        // test: a 1,004 that moves the number would pass it, and "+ 0" would need a second rule.
        // Guard rails: the Hype line and the song-end line ALWAYS print; the Blocked and Indifferent
        // branches are untouched because they explain a zero, which is where the reason matters most.
        // This REVERSES BIGNUM-1's "every line is shown even when neutral", by observation
        // (ST-TIP-10): four inert lines buried the two that moved the number.
        // Authority: planning/Design_Vibe_Telegraph_v0_1.md 10.2.
        private static string BuildVibeText(in VibeProjection p)
        {
            // [TXT-4 / F-TXT-2-1] Every line is a ui_aud_vibe_* template; status names resolve by StatusKey
            // (GameText.ConceptName) so "Captivado" stops being a second home for the captivated status.
            var sb = new System.Text.StringBuilder();
            sb.Append(GameText.Format(UiAudVibeHype, GameText.Number(p.HypeBase, "0.#")));

            if (p.Blocked)
            {
                sb.Append('\n').Append(GameText.Format(UiAudVibeBlocked));
                sb.Append('\n').Append(GameText.Format(UiAudVibeSongEnd, "0"));
                return sb.ToString();
            }

            // 6.1 tastes: input HypeBase (float, printed "0.#"), output LAfterImpression (int).
            if (Mathf.RoundToInt(p.HypeBase) != p.LAfterImpression)
                sb.Append('\n').Append(GameText.Format(UiAudVibeTastes, GameText.Number(p.ImpressionFactor, "0.00"), p.LAfterImpression));
            // 7.1 Flow: input LAfterImpression, output LAfterFlow.
            if (p.LAfterFlow != p.LAfterImpression)
                sb.Append('\n').Append(GameText.Format(UiAudVibeStatusMult, GameText.Number(p.FlowMult, "0.00"), GameText.ConceptName("flow"), p.FlowStacks, p.LAfterFlow));
            // 6.2 SFX: input LAfterFlow, output Subtotal (equivalent to Sfx != 0).
            if (p.Subtotal != p.LAfterFlow)
                sb.Append('\n').Append(GameText.Format(UiAudVibeSfx, p.Sfx, p.Subtotal));

            // Gate. Indifferent prints ALWAYS (it explains the zero). Captivated prints when it moved Final.
            if (p.Indifferent)
                sb.Append('\n').Append(GameText.Format(UiAudVibeIndifferent, GameText.ConceptName("indifference")));
            else if (p.Final != p.Subtotal)
                sb.Append('\n').Append(GameText.Format(UiAudVibeStatusMult, GameText.Number(p.CaptivatedMult, "0.00"), GameText.ConceptName("captivated"), p.CaptivatedStacks, p.Final));

            sb.Append('\n').Append(GameText.Format(UiAudVibeSongEnd, p.Final > 0 ? "-" + p.Final : "0"));
            if (p.Ko) sb.Append('\n').Append(GameText.Format(UiAudVibeKo));
            return sb.ToString();
        }

        // [GIG-UI-1] Two hover sources feed ONE rebuild (Claudio, 2026-11-02):
        // the intent icon (UI hover on this canvas) and the body (CharacterBase physics
        // hover on the collider, the same one that lights the outline and that cards
        // target). BOTH show the same column -- Blocked (when tinted), ability, revealed
        // tastes -- because a column that changed with where you pointed read as two
        // different tooltips. Every change hides the whole column and rebuilds it, so the
        // two sources can overlap (icon inside the collider) without duplicating panels,
        // and leaving one does not wipe what the other still owns.
        private bool _iconHovered;
        private bool _bodyHovered;

        /// <summary>[GIG-UI-1] Set by AudienceCharacterBase: is the Blocked tint showing now?</summary>
        public System.Func<bool> BlockedQuery { get; set; }

        // [GIG-UI-1] Column placement: a point at the top-right of the body collider
        // (AudienceCharacterBase creates it). The column grows DOWN from it, a few pixels
        // to the right, so every member gets the same placement whatever its canvas
        // offsets are (the Kid canvas sat lower, so its descriptionRoot did too).
        private Transform _tooltipAnchor;
        // Colours and the column offset live in TooltipStyleSO (on the TooltipManager
        // prefab), so every tooltip kind is tuned in one place.
        private static readonly Vector2 FallbackColumnOffset = new Vector2(8f, 40f);

        public void SetTooltipAnchor(Transform anchor) { _tooltipAnchor = anchor; }

        private void ShowPanel(string content, string header, TooltipKind kind)
        {
            var tm = TooltipManager.Instance;
            if (tm == null) return;
            if (_tooltipAnchor != null)
            {
                var offset = tm.Style != null ? tm.Style.AudienceColumnOffset : FallbackColumnOffset;
                tm.ShowTooltip(content, header, _tooltipAnchor, null, 0f,
                    offset, growDown: true, kind: kind);
            }
            else
                tm.ShowTooltip(content, header, descriptionRoot, null, 0f,
                    default(Vector2), growDown: false, kind: kind);
        }

        /// <summary>[GIG-UI-1] Called by AudienceCharacterBase on sprite hover enter/exit.</summary>
        public void SetBodyHover(bool hovered)
        {
            _bodyHovered = hovered;
            RefreshHoverTooltip();
        }

        protected override void ShowTooltipInfo()
        {
            base.ShowTooltipInfo();
            _iconHovered = true;
            RefreshHoverTooltip();
        }

        protected override void OnCanvasPointerExit()
        {
            _iconHovered = false;
            RefreshHoverTooltip();
        }

        private void RefreshHoverTooltip()
        {
            // [BIGNUM-1r / D-BN-15] The bar's own tooltip is showing; PointerEnter reaches
            // the child (VibeBarTooltipTarget) before this canvas, so the flag is already
            // set when we get here. Stand down instead of overwriting its tooltip.
            if (_barHovered) return;

            HideTooltipInfo(TooltipManager.Instance);
            if (_convincedHidden) return; // [CONV-TINT-1b] TooltipManager is outside the group
            if (!_iconHovered && !_bodyHovered) return;

            // [S5f / E-lite] The Blocked legend explains the tint, so it leads the column.
            if (BlockedQuery != null && BlockedQuery())
                ShowPanel(BlockedTooltipBody, BlockedTooltipHeader, TooltipKind.Blocked);

            string header = null;
            string body = null;

            if (NextAbility != null && CurrentIntention != null)
            {
                // [TEXT-5 / D-T5-1=A] Name + body from the glossary by abilityId (one concept per ability).
                // abilityName / intention.contentText are developer labels; a missing entry shows the raw
                // id and warns once (SSoT_Game_Text §3 miss policy). The null guard above is unchanged:
                // the intention SO still gates the hover (it also carries the sprite).
                header = GameText.ConceptName(NextAbility.AbilityId);
                body = GameText.ConceptDescription(NextAbility.AbilityId);
                string hit = BuildHitPhrase();
                if (!string.IsNullOrEmpty(hit))
                {
                    body = "<b>" + hit + "</b>\n" + body;
                    if (hit != _lastHitLog)
                        Debug.Log($"[S4b] Frase de golpe en el tooltip de {name}: {hit}");
                }
                _lastHitLog = hit;
            }

            // [GIG-UI-1] REVERSES the single-surface half of PRES-1 / D-PRES1-3=A, at Claudio's
            // request (2026-11-02, Monster Train reference): the ability and the revealed tastes
            // are now separate stacked panels, each with its own header and colour, instead of
            // one box with an inline "-- Gustos --" separator. What D-PRES1-3 protected still
            // holds: no persistent taste panel, and tastes still show when there is no
            // intention (the taste panel then stands alone).
            if (body != null)
                ShowPanel(body, header, TooltipKind.Ability);
            if (_tasteRevealed)
                ShowPanel(BuildTasteText(_revealedTaste), TasteOnlyHeader, TooltipKind.Tastes);
        }

        /// <summary>
        /// [S5a/T10] Show the per-enemy telegraph. <paramref name="tier"/> drives the
        /// effectiveness label (C2); <paramref name="projectedNumber"/> is the live
        /// "+N Vibe" this member will receive at song end (C3), hidden when Immune or
        /// when the caller opts out. Pure presentation - no state.
        /// </summary>
        public void SetVibeTelegraph(
            VibeEffectiveness tier, int projectedNumber, bool showNumber,
            bool showLabel = true)
        {
            bool anyVisible = showLabel ||
                (showNumber && tier != VibeEffectiveness.Immune);
            if (vibeTelegraphRoot != null)
                vibeTelegraphRoot.SetActive(anyVisible);

            if (effectivenessLabel != null)
            {
                effectivenessLabel.gameObject.SetActive(showLabel);
                if (showLabel)
                {
                    effectivenessLabel.text = LabelFor(tier);
                    effectivenessLabel.color = ColorFor(tier);
                }
            }

            if (projectedVibeText != null)
            {
                bool show = showNumber && tier != VibeEffectiveness.Immune;
                projectedVibeText.gameObject.SetActive(show);
                if (show)
                {
                    // [BIGNUM-1] Capture the prefab tint once; reset to it on every
                    // show so a previous KO tint never leaks into the next refresh.
                    if (!_projectedDefaultCached)
                    {
                        _projectedDefaultColor = projectedVibeText.color;
                        _projectedDefaultCached = true;
                    }
                    projectedVibeText.color = _projectedDefaultColor;
                    projectedVibeText.text = $"-{projectedNumber}";
                }
            }
        }

        /// <summary>
        /// [BIGNUM-1 / D-BN-2=A] Telegraph from a full projection: caches it for the
        /// tooltip breakdown (D-BN-9=A) and tints the C3 number when this song would
        /// convince the member (D-BN-4 KO). The S5a surface is delegated to the S5a
        /// method; this adds presentation on top, no numbers of its own.
        /// </summary>
        public void SetVibeTelegraph(in VibeProjection p, bool showNumber, bool showLabel = true)
        {
            _projection = p;
            _hasProjection = true;

            SetVibeTelegraph(p.Tier, p.Final, showNumber, showLabel);

            // [BIGNUM-1r2 / D-BN-16=A] The ghost is its OWN surface: it survives the
            // numeric-label toggle. showVibeProjectedNumbers governs the "-N" text, not
            // the bar segment — turning the number off to reduce clutter should leave
            // the bar readable, which is the whole point of the segment.
            // Immune/blocked project 0, so the ghost simply does not show for them.
            healthBar?.SetPredictedLoss(p.Final);

            SetPredictionVisible(p.Final > 0); // [BIGNUM-1r3 / D-BN-17=A]

            if (projectedVibeText != null && projectedVibeText.gameObject.activeSelf && p.Ko)
                projectedVibeText.color = projectedKoColor;
        }

        /// <summary>[S5a] Hide the telegraph (between songs / audience turn).</summary>
        public void HideVibeTelegraph()
        {
            _hasProjection = false; // [BIGNUM-1] no breakdown in the tooltip between songs

            healthBar?.SetPredictedLoss(0); // [BIGNUM-1r] no ghost between songs either

            SetPredictionVisible(false);    // [BIGNUM-1r3] full bars go back to hidden

            // Deactivate the labels explicitly too, so hide works whether they are
            // children of vibeTelegraphRoot or siblings of it (prefab-layout robust).
            if (vibeTelegraphRoot != null)
                vibeTelegraphRoot.SetActive(false);
            if (effectivenessLabel != null)
                effectivenessLabel.gameObject.SetActive(false);
            if (projectedVibeText != null)
                projectedVibeText.gameObject.SetActive(false);
        }

        // [S5f / D-S5f-8=A] ESP copy for the tester build (D-REPLAN-1:
        // unassisted Spanish comprehension). ENG originals: "Super!" /
        // "Resists" / "Immune" / "Normal". Hardcoded like the Blocked
        // tooltip (D-S5f-7=A); migrates in the S5f-ext localization pass.
        private static string LabelFor(VibeEffectiveness tier)
        {
            switch (tier)
            {
                case VibeEffectiveness.SuperEffective: return GameText.Format(UiAudEffSuper);       // [TXT-4]
                case VibeEffectiveness.NotVeryEffective: return GameText.Format(UiAudEffResists);
                case VibeEffectiveness.Immune: return GameText.Format(UiAudEffImmune);
                default: return GameText.Format(UiAudEffNormal);
            }
        }

        // Palette aligned with the impression/floater language (green = good,
        // grey = neutral, red = resisting, muted = gated).
        private static Color ColorFor(VibeEffectiveness tier)
        {
            switch (tier)
            {
                case VibeEffectiveness.SuperEffective: return new Color(0.40f, 1.0f, 0.40f);
                case VibeEffectiveness.NotVeryEffective: return new Color(1.0f, 0.45f, 0.40f);
                case VibeEffectiveness.Immune: return new Color(0.50f, 0.50f, 0.50f);
                default: return new Color(0.80f, 0.80f, 0.80f);
            }
        }

        // [S5f / E-lite] Blocked ("oscurito") legend. Blocked is sprite-tint
        // only per M1.2 Decision E3 (no status icon), so this hover tooltip is
        // the only textual surface explaining the tint. ESP copy (tester build
        // default; D-S5f-7=A — hardcoded until the S5f-ext localization pass).
        // ENG: "Blocked — someone tall is in the way. Immune to persuasion
        // from this position."
        // [TXT-4 / F-TXT-2-1] The Blocked legend IS the `blocked` glossary concept (SSoT_Game_Text §5,
        // glossary-native): header + body come from there, so the tint tooltip and the tutorial tag can
        // never disagree. The former ES literal was a second home for the same concept.
        private static string BlockedTooltipHeader => GameText.ConceptName("blocked");
        private static string BlockedTooltipBody => GameText.ConceptDescription("blocked");

        /// <summary>[S5f / E-lite] Show the Blocked-tint explanation tooltip.</summary>
        public void ShowBlockedTooltip()
        {
            ShowTooltipInfo(TooltipManager.Instance,
                BlockedTooltipBody, BlockedTooltipHeader, descriptionRoot);
        }

        /// <summary>[S5f / E-lite] Hide the Blocked tooltip (pointer exit).</summary>
        public void HideBlockedTooltip()
        {
            HideTooltipInfo(TooltipManager.Instance);
        }

        // ---- [R4 / D-R0-1=A + D-R4-4=A -> PRES-1 / D-PRES1-3=A] Taste reveal ----
        //
        // Presentation only. AudienceCharacterBase owns the "has been revealed"
        // flag and its per-gig idempotence; AudienceCharacterData owns the
        // preference data. ESP copy is hardcoded, matching the Blocked tooltip and
        // the telegraph labels (D-S5f-7=A / D-S5f-8=A); all three migrate together
        // in the S5f-ext localization pass.

        private static string TasteNeutralText => GameText.Format(UiAudTasteNeutral);   // [TXT-4]

        /// <summary>
        /// [R4 -> PRES-1 / D-PRES1-3=A] Register this member's tastes as revealed.
        ///
        /// Method name deliberately unchanged: the caller
        /// (AudienceCharacterBase.RevealPreferences) is NOT touched by PRES-1, so
        /// its idempotence and one-call-per-gig guarantee carry over untouched.
        /// What changed is the surface — the persistent panel is retired; this now
        /// caches the taste for tooltip composition and lights the discreet icon.
        /// </summary>
        public void ShowTastePanel(TastePreferences taste)
        {
            _revealedTaste = taste;
            _tasteRevealed = true;

            if (revealedTasteIcon != null)
                revealedTasteIcon.SetActive(true);
        }

        /// <summary>
        /// [R4 -> PRES-1 -> R5-pre] Clear the reveal (gig teardown / dev reset).
        /// The defensive hide of the retired legacy panel is gone with the panel
        /// itself (R5-pre prefab pass): there is no longer an object to strand.
        /// </summary>
        public void HideTastePanel()
        {
            _revealedTaste = null;
            _tasteRevealed = false;

            if (revealedTasteIcon != null)
                revealedTasteIcon.SetActive(false);
        }

        // Mirrors the four frozen taste axes of SSoT_Audience_and_Reactions section 6.1
        // in the same order the impression algorithm walks them. An axis with no
        // authored data prints nothing, so a neutral archetype (all axes disabled,
        // always 0 impression) reads as "doesn't care" instead of an empty box.
        private static string BuildTasteText(TastePreferences t)
        {
            if (t == null) return TasteNeutralText;

            var sb = new System.Text.StringBuilder();

            if (t.tempoMatchOnFast)
                AppendTasteLine(sb, GameText.Format(UiAudTasteFast));      // [TXT-4]
            if (t.tempoMismatchOnSlow)
                AppendTasteLine(sb, GameText.Format(UiAudTasteSlow));

            if (t.roleCountMatchOnRich)
                AppendTasteLine(sb, GameText.Format(UiAudTasteTracks, t.preferAtLeastRoles));

            // [LEGIB-1b / D3=C] Player text for the enums (the same ids the composition strip
            // uses: ui_ctx_meter_* -> "4/4", ui_tonality_* -> "Alegre"), not "FourFour"/"Ionian".
            if (t.preferredTimeSignatures != null && t.preferredTimeSignatures.Count > 0)
                AppendTasteLine(sb, "+ " + JoinTexts(t.preferredTimeSignatures, m => GameText.Format(UiCtxMeter(m))));
            if (t.dislikedTimeSignatures != null && t.dislikedTimeSignatures.Count > 0)
                AppendTasteLine(sb, "- " + JoinTexts(t.dislikedTimeSignatures, m => GameText.Format(UiCtxMeter(m))));

            if (t.preferredTonalities != null && t.preferredTonalities.Count > 0)
                AppendTasteLine(sb, "+ " + JoinTexts(t.preferredTonalities, x => GameText.Format(UiTonality(x))));
            if (t.dislikedTonalities != null && t.dislikedTonalities.Count > 0)
                AppendTasteLine(sb, "- " + JoinTexts(t.dislikedTonalities, x => GameText.Format(UiTonality(x))));

            return sb.Length == 0 ? TasteNeutralText : sb.ToString();
        }

        private static string JoinTexts<T>(System.Collections.Generic.IEnumerable<T> values, System.Func<T, string> text)
        {
            var parts = new System.Collections.Generic.List<string>();
            foreach (var v in values) parts.Add(text(v));
            return string.Join(", ", parts);
        }

        private static void AppendTasteLine(System.Text.StringBuilder sb, string line)
        {
            if (sb.Length > 0) sb.Append('\n');
            sb.Append(line);
        }
    }
}