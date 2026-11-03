using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Resizes a saved scene window from an edge or corner without scaling its contents.</summary>
public sealed class ExploreWindowResizeHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform window;
    [Tooltip("-1: left/bottom, 0: unchanged, 1: right/top")]
    public Vector2 direction;
    public Vector2 minimumSize = new Vector2(1700, 1000);

    private RectTransform parentRect;
    private Vector2 startPointer;
    private Vector2 startSize;
    private Vector2 startPosition;
    private Vector2 maximumSize;
    private bool dragging;

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = false;
        if (!window || eventData.button != PointerEventData.InputButton.Left) return;
        parentRect = window.parent as RectTransform;
        if (!parentRect || window.localScale.x <= 0 || window.localScale.y <= 0 ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position,
                eventData.pressEventCamera, out startPointer)) return;

        window.SetAsLastSibling();
        startSize = window.rect.size;
        startPosition = window.anchoredPosition;
        var position = (Vector2)window.localPosition;
        var scale = (Vector2)window.localScale;
        var lower = position + Vector2.Scale(window.rect.min, scale);
        var upper = position + Vector2.Scale(window.rect.max, scale);
        maximumSize = new Vector2(
            (direction.x < 0 ? upper.x - parentRect.rect.xMin : parentRect.rect.xMax - lower.x) / scale.x,
            (direction.y < 0 ? upper.y - parentRect.rect.yMin : parentRect.rect.yMax - lower.y) / scale.y);
        // A previously moved window must still be able to shrink if it extends beyond the desktop.
        maximumSize = Vector2.Max(maximumSize, Vector2.Max(startSize, minimumSize));
        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || !window || !parentRect ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position,
                eventData.pressEventCamera, out var pointer)) return;

        var delta = pointer - startPointer;
        var size = startSize;
        var position = startPosition;
        if (direction.x != 0)
        {
            size.x = Mathf.Clamp(startSize.x + delta.x / window.localScale.x * direction.x,
                minimumSize.x, maximumSize.x);
            position.x += (size.x - startSize.x) * window.localScale.x *
                (direction.x < 0 ? window.pivot.x - 1 : window.pivot.x);
        }
        if (direction.y != 0)
        {
            size.y = Mathf.Clamp(startSize.y + delta.y / window.localScale.y * direction.y,
                minimumSize.y, maximumSize.y);
            position.y += (size.y - startSize.y) * window.localScale.y *
                (direction.y < 0 ? window.pivot.y - 1 : window.pivot.y);
        }
        window.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        window.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        window.anchoredPosition = position;
    }

    public void OnEndDrag(PointerEventData eventData) => dragging = false;
    private void OnDisable() => dragging = false;
}
