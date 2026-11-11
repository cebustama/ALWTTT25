using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ALWTTT.ShipHub
{
    /// <summary>
    /// [S5 NAVE-1] One clickable room over the ship cutaway. Created at runtime
    /// by <see cref="ShipHubController"/> from its room list; the Image is a
    /// transparent rectangle that tints while hovered (alpha 0 still receives
    /// UI raycasts). Reports hover and click back to the controller.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ShipRoomZone : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public int Index { get; private set; }

        public event Action<ShipRoomZone> HoverEnter;
        public event Action<ShipRoomZone> HoverExit;
        public event Action<ShipRoomZone> Clicked;

        private Image _image;
        private Color _idle;
        private Color _hover;

        public RectTransform Rect => (RectTransform)transform;

        public void Init(int index, Color idle, Color hover)
        {
            Index = index;
            _image = GetComponent<Image>();
            _idle = idle;
            _hover = hover;
            _image.color = _idle;
            _image.raycastTarget = true;
        }

        public void SetHighlighted(bool on)
        {
            if (_image != null) _image.color = on ? _hover : _idle;
        }

        public void OnPointerEnter(PointerEventData eventData) => HoverEnter?.Invoke(this);
        public void OnPointerExit(PointerEventData eventData) => HoverExit?.Invoke(this);

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            Clicked?.Invoke(this);
        }
    }
}
