using ALWTTT.Managers;
using UnityEngine;

namespace ALWTTT.UI
{
    public class CanvasBase : MonoBehaviour
    {
        protected GigManager GigManager => GigManager.Instance;
        protected DeckManager DeckManager => DeckManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        protected UIManager UIManager => UIManager.Instance;

        public virtual void OpenCanvas()
        {
            gameObject.SetActive(true);
        }

        public virtual void CloseCanvas()
        {
            gameObject.SetActive(false);
        }

        public virtual void ResetCanvas()
        {

        }
    }

}