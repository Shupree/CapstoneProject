using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Moves the assigned scene window while dragging its title-bar hit area.</summary>
public sealed class ExploreProtoWindowDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public RectTransform window;
    private RectTransform canvasRect;
    private Vector2 pointerOffset;

    private void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas ? canvas.transform as RectTransform : null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        window.SetAsLastSibling();
        if (canvasRect && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out var local))
            pointerOffset = (Vector2)window.localPosition - local;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!canvasRect || !RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out var local)) return;
        var position = local + pointerOffset;
        window.anchoredPosition = position;
    }
}
