using ALWTTT.Enums;
using ALWTTT.Interfaces;
using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.Characters
{
    public abstract class CharacterBase : MonoBehaviour, ICharacter
    {
        public virtual IMusicianStats MusicianStats => null;
        public virtual IAudienceStats AudienceStats => null;

        [Header("Base settings")]
        [SerializeField] private CharacterType characterType;
        [SerializeField] private Transform textSpawnRoot;
        [SerializeField] private Transform headRoot;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] protected Color obscuredColor;
        // TODO: Connect with Abilities instead
        [SerializeField] protected Transform speechBubblePrefab;
        [SerializeField] protected LayerMask characterLayerMask;
        [SerializeField] protected CharacterAnimator characterAnimator;
        [SerializeField] protected Animator animator;

        #region Encapsulation
        public CharacterType CharacterType => characterType;
        public Transform TextSpawnRoot => textSpawnRoot;
        public Transform HeadRoot => headRoot;
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public CharacterAnimator CharacterAnimator => characterAnimator;
        #endregion

        #region Cache
        protected GigManager GigManager => GigManager.Instance;
        protected GameManager GameManager => GameManager.Instance;

        public bool IsStunned { get; set; }
        #endregion

        private bool isPointerOver = false;

        protected virtual void Update()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 100f, characterLayerMask))
            {
                var charHit = hit.collider.GetComponent<ICharacter>();
                if (charHit != null && charHit.GetCharacterBase() == this)
                {
                    if (!isPointerOver)
                    {
                        isPointerOver = true;
                        OnPointerEnter();
                    }
                    return;
                }
            }

            if (isPointerOver)
            {
                isPointerOver = false;
                OnPointerExit();
            }
        }

        public virtual void BuildCharacter()
        {

        }

        public CharacterBase GetCharacterBase()
        {
            return this;
        }

        public CharacterType GetCharacterType()
        {
            return CharacterType;
        }

        protected virtual void OnPointerEnter()
        {
            
        }

        protected virtual void OnPointerExit()
        {
            
        }
    }
}