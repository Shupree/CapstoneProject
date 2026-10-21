using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>A scene-authored call card. Artwork and button sprites remain prefab-editable.</summary>
[DisallowMultipleComponent]
public sealed class BloodyMaryCallCard : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BloodyMaryPhaseController phase;
    public UnityEngine.UI.Image artwork;
    public UnityEngine.UI.Button dismissButton;
    public GameObject threatOverlay;
    public bool isKnife;
    RectTransform rect;
    Vector2 initialPosition, dragOffset;
    int dragPointer;
    bool captured, dragging;

    void Awake()
    {
        CapturePlacement();
        if (dismissButton) dismissButton.onClick.AddListener(Dismiss);
    }

    void CapturePlacement()
    {
        if (captured) return;
        rect = (RectTransform)transform;
        initialPosition = rect.anchoredPosition;
        captured = true;
    }

    public void RestorePlacement()
    {
        CapturePlacement();
        dragging = false;
        if (threatOverlay) threatOverlay.SetActive(false);
        if (artwork) artwork.enabled = true;
        rect.anchoredPosition = initialPosition;
        ClampToWindow();
    }

    public void ShowThreat()
    {
        dragging = false;
        if (dismissButton) dismissButton.interactable = false;
        if (artwork) artwork.enabled = false;
        if (threatOverlay) threatOverlay.SetActive(true);
    }

    public void PlaceRandomly()
    {
        CapturePlacement();
        var bounds = ((RectTransform)rect.parent).rect;
        var size = Vector2.Scale(rect.rect.size, rect.localScale);
        rect.localPosition = new Vector3(
            Random.Range(bounds.xMin + size.x * rect.pivot.x, bounds.xMax - size.x * (1f - rect.pivot.x)),
            Random.Range(bounds.yMin + size.y * rect.pivot.y, bounds.yMax - size.y * (1f - rect.pivot.y)), 0f);
        ClampToWindow();
    }

    public void ClampToWindow()
    {
        CapturePlacement();
        var bounds = ((RectTransform)rect.parent).rect;
        var size = Vector2.Scale(rect.rect.size, rect.localScale);
        var pos = rect.localPosition;
        pos.x = size.x <= bounds.width ? Mathf.Clamp(pos.x, bounds.xMin + size.x * rect.pivot.x, bounds.xMax - size.x * (1f - rect.pivot.x)) : bounds.center.x;
        pos.y = size.y <= bounds.height ? Mathf.Clamp(pos.y, bounds.yMin + size.y * rect.pivot.y, bounds.yMax - size.y * (1f - rect.pivot.y)) : bounds.center.y;
        rect.localPosition = pos;
    }

    bool CanDrag(PointerEventData data)
    {
        if (!phase || !phase.CanInteract || !gameObject.activeInHierarchy || data.button != PointerEventData.InputButton.Left) return false;
        var hit = data.pointerPressRaycast.gameObject;
        return !hit || !dismissButton || !hit.transform.IsChildOf(dismissButton.transform);
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (CanDrag(data)) transform.SetAsLastSibling();
    }

    public void OnBeginDrag(PointerEventData data)
    {
        dragging = false;
        if (!CanDrag(data)) return;
        CapturePlacement();
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rect.parent, data.position, data.pressEventCamera, out var local)) return;
        dragOffset = (Vector2)rect.localPosition - local;
        dragPointer = data.pointerId;
        dragging = true;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData data)
    {
        if (!dragging || data.pointerId != dragPointer || !phase || !phase.CanInteract) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rect.parent, data.position, data.pressEventCamera, out var local)) return;
        rect.localPosition = local + dragOffset;
        ClampToWindow();
    }

    public void OnEndDrag(PointerEventData data) { if (data.pointerId == dragPointer) dragging = false; }
    void OnDisable() { dragging = false; }
    void Dismiss() { if (phase) phase.DismissCard(this); }
    void OnDestroy() { if (dismissButton) dismissButton.onClick.RemoveListener(Dismiss); }
}
