using UnityEngine;
using UnityEngine.EventSystems;

namespace CyberExorcist
{
    public sealed class PrototypeWindowDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] RectTransform window;
        Vector2 offset;
        public void Initialize(RectTransform target) { window = target; }
        public void OnBeginDrag(PointerEventData e)
        {
            if (!window) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)window.parent, e.position, e.pressEventCamera, out var p);
            offset = p - window.anchoredPosition;
        }
        public void OnDrag(PointerEventData e)
        {
            if (!window) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)window.parent, e.position, e.pressEventCamera, out var p);
            var position = p - offset;
            window.anchoredPosition = new Vector2(Mathf.Clamp(position.x, 111, 470), Mathf.Clamp(position.y, -132, -68));
        }
    }
}
