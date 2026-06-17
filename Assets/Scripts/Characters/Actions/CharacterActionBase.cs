using ALWTTT.Enums;
using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.Actions
{
    public abstract class CharacterActionBase
    {
        protected CharacterActionBase() { }
        public abstract CharacterActionType ActionType { get; }
        public abstract string ActionName { get; }
        public abstract void DoAction(CharacterActionParameters p);

        // TODO: Maybe add the PlaySFX method here

        #region Cache
        protected GameManager GameManager => GameManager.Instance;
        protected GigManager GigManager => GigManager.Instance;
        protected DeckManager DeckManager => DeckManager.Instance;
        protected FxManager FxManager => FxManager.Instance;
        protected AudioManager AudioManager => AudioManager.Instance;
        #endregion
    }
}