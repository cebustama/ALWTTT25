using ALWTTT.Data;
using System.Collections;
using UnityEngine;

namespace ALWTTT.Characters.Audience
{
    /// <summary>
    /// [S9 / REINA-1, D8=A] The Reina's larva as a visual clock for the egg · egg · hatch pattern.
    /// One sprite beside her that changes stage with her abilities:
    ///   egg 1 fired → stage 1 · egg 2 fired → stage 2 · hatch announced → stage 3 ·
    ///   hatch fired → stage 4 (held <see cref="burstHoldSeconds"/>, then hidden: the clutch is spent).
    /// Hidden before the first egg and once the owner is convinced.
    ///
    /// Presentation only: reads the owner's ability ids (same prefixes as the JSON: ab_egg*, ab_hatch)
    /// through AudienceCharacterBase.OnAbilityShown / OnAbilityFired. No numbers, targets or order
    /// change. The stages keep their RELATIVE pixel size (the art grows from egg to larva).
    /// Size: the tallest stage is drawn at the scale authored on this Transform in the prefab, so the
    /// clock follows its parent. Hang it under the Reina's 'Sprite' child: the venue slot scale and
    /// the Lineup height change (CharacterSizing.SetHeight scales that child) then move and resize
    /// it with her. [S9 fix: it used to force a fixed world height and ignored both.]
    /// </summary>
    [DisallowMultipleComponent]
    public class BroodClock : MonoBehaviour
    {
        [Tooltip("Stages 1–4: larva_1 (egg) … larva_4 (bursting larva).")]
        [SerializeField] private Sprite[] stages = new Sprite[4];

        [Tooltip("SpriteRenderer that shows the stage. Empty = the one on this GameObject.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private string eggPrefix = "ab_egg";
        [SerializeField] private string hatchPrefix = "ab_hatch";

        [Tooltip("Seconds the burst (stage 4) stays on screen before the clutch is cleared.")]
        [SerializeField, Min(0f)] private float burstHoldSeconds = 1.5f;

        [Tooltip("Pop when the stage changes: peak scale over 1.")]
        [SerializeField, Range(0f, 1f)] private float popAmount = 0.25f;
        [SerializeField, Min(0.01f)] private float popSeconds = 0.25f;

        private AudienceCharacterBase _owner;
        private int _stage;      // 0 = hidden
        private int _eggs;       // eggs laid in the current clutch
        private Coroutine _anim;
        private Vector3 _baseScale = Vector3.one;
        private Vector3 _authoredScale = Vector3.one;
        private bool _hasAuthored;

        public int Stage => _stage;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            _owner = GetComponentInParent<AudienceCharacterBase>();
            CaptureAuthored();   // scale of the tallest stage, from what the prefab shows
            SetStage(0, "inicio", animate: false);
        }

        private void OnEnable()
        {
            if (_owner == null) _owner = GetComponentInParent<AudienceCharacterBase>();
            if (_owner == null)
            {
                Debug.LogWarning("[S9] Cría | sin AudienceCharacterBase en el padre: el reloj no se mueve.");
                return;
            }
            _owner.OnAbilityShown += HandleShown;
            _owner.OnAbilityFired += HandleFired;
        }

        private void OnDisable()
        {
            if (_owner == null) return;
            _owner.OnAbilityShown -= HandleShown;
            _owner.OnAbilityFired -= HandleFired;
        }

        private void Update()
        {
            // Convinced = the clutch no longer matters (and the member stops acting).
            if (_stage != 0 && _owner != null && _owner.AudienceStats != null && _owner.AudienceStats.IsConvinced)
                SetStage(0, "convencida", animate: false);
        }

        private bool Is(AudienceAbilityData a, string prefix)
            => a != null && !string.IsNullOrEmpty(a.AbilityId) && !string.IsNullOrEmpty(prefix)
               && a.AbilityId.StartsWith(prefix);

        private void HandleShown(AudienceAbilityData a)
        {
            if (Is(a, hatchPrefix)) SetStage(3, "eclosión anunciada");
        }

        private void HandleFired(AudienceAbilityData a)
        {
            if (Is(a, eggPrefix))
            {
                _eggs++;
                SetStage(Mathf.Clamp(_eggs, 1, 2), $"huevo {_eggs}");
            }
            else if (Is(a, hatchPrefix))
            {
                _eggs = 0;
                SetStage(4, "eclosión");
                if (burstHoldSeconds > 0f) StartCoroutine(ClearAfter(burstHoldSeconds));
                else SetStage(0, "nidada vacía", animate: false);
            }
        }

        private IEnumerator ClearAfter(float s)
        {
            yield return new WaitForSeconds(s);
            if (_stage == 4) SetStage(0, "nidada vacía", animate: false);
        }

        private void SetStage(int stage, string why, bool animate = true)
        {
            if (spriteRenderer == null) return;
            if (stage == _stage && stage != 0) return;
            _stage = stage;

            var sprite = stage >= 1 && stage <= stages.Length ? stages[stage - 1] : null;
            spriteRenderer.enabled = sprite != null;
            spriteRenderer.sprite = sprite;
            if (sprite != null) FitHeight(sprite);

            if (Application.isPlaying)
                Debug.Log($"[S9] Cría | {(_owner != null ? _owner.name : name)} | etapa {stage} | {why}");

            if (animate && sprite != null && isActiveAndEnabled)
            {
                if (_anim != null) StopCoroutine(_anim);
                _anim = StartCoroutine(Pop());
            }
        }

        /// <summary>Size factor of a stage against the tallest one (1 for the tallest), keeping the
        /// pixel ratio of the art whatever each sprite's pixels-per-unit.</summary>
        private float StageFactor(Sprite sprite)
        {
            Sprite tallest = null;
            foreach (var st in stages)
                if (st != null && (tallest == null || st.rect.height > tallest.rect.height)) tallest = st;
            if (sprite == null || tallest == null || sprite.bounds.size.y <= 0f) return 1f;
            return (sprite.rect.height / tallest.rect.height) * (tallest.bounds.size.y / sprite.bounds.size.y);
        }

        /// <summary>The prefab may be saved showing any stage: undo that stage's factor to get the
        /// scale of the tallest stage. Relative to the parent, so the clock follows the Reina.</summary>
        private void CaptureAuthored()
        {
            float k = spriteRenderer != null ? StageFactor(spriteRenderer.sprite) : 1f;
            if (k <= 0f) k = 1f;
            var ls = transform.localScale;
            _authoredScale = new Vector3(ls.x / k, ls.y / k, ls.z);
            _hasAuthored = true;
        }

        private void FitHeight(Sprite sprite)
        {
            if (!_hasAuthored) CaptureAuthored();
            float k = StageFactor(sprite);
            _baseScale = new Vector3(_authoredScale.x * k, _authoredScale.y * k, _authoredScale.z);
            transform.localScale = _baseScale;
        }

        private IEnumerator Pop()
        {
            float t = 0f;
            while (t < popSeconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / popSeconds);
                float bump = Mathf.Sin(u * Mathf.PI) * popAmount; // up and back down
                transform.localScale = _baseScale * (1f + bump);
                yield return null;
            }
            transform.localScale = _baseScale;
            _anim = null;
        }

#if UNITY_EDITOR
        /// <summary>Inspector preview: right-click the component ▸ a stage.</summary>
        [ContextMenu("Vista previa ▸ etapa 1")] private void P1() => Preview(1);
        [ContextMenu("Vista previa ▸ etapa 2")] private void P2() => Preview(2);
        [ContextMenu("Vista previa ▸ etapa 3")] private void P3() => Preview(3);
        [ContextMenu("Vista previa ▸ etapa 4")] private void P4() => Preview(4);
        private void Preview(int s)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            // Takes the size you gave the stage on screen (Scale tool) and shows the next one in
            // proportion; the others follow at runtime.
            CaptureAuthored();
            _stage = -1;
            SetStage(s, "vista previa", animate: false);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(transform);
        }
#endif
    }
}
