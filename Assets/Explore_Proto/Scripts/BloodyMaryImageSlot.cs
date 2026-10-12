using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Routes a single click to its saved slot index; the parent ScrollRect owns dragging.</summary>
[DisallowMultipleComponent]
public sealed class BloodyMaryImageSlot : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, ISubmitHandler
{
    public BloodyMaryPhaseController phase;
    [Min(0)] public int slotIndex;
    public UnityEngine.UI.Image artwork;
    public UnityEngine.UI.Button button;

    bool armed;
    int pressedRevision, pointerId;
    Vector2 pressedScrollPosition;

    public void SetAppearance(Sprite sprite, bool clickable)
    {
        artwork.sprite = sprite;
        button.interactable = clickable;
        armed = false;
        // Never call SetNativeSize: both sprites use this slot's fixed scene RectTransform.
    }

    bool CanClick => phase && phase.CanInteract && button && button.IsActive() && button.IsInteractable();

    public void OnPointerDown(PointerEventData data)
    {
        armed = false;
        if (data.button != PointerEventData.InputButton.Left || !CanClick) return;
        armed = true;
        pointerId = data.pointerId;
        pressedRevision = phase.BoardRevision;
        pressedScrollPosition = phase.imageScroll.content.anchoredPosition;
    }

    public void OnPointerClick(PointerEventData data)
    {
        bool samePress = armed && pointerId == data.pointerId;
        armed = false;
        if (!samePress || !CanClick || data.button != PointerEventData.InputButton.Left ||
            data.dragging || !data.eligibleForClick) return;
        float threshold = EventSystem.current ? EventSystem.current.pixelDragThreshold : 5f;
        if ((data.position - data.pressPosition).sqrMagnitude > threshold * threshold) return;
        if ((phase.imageScroll.content.anchoredPosition - pressedScrollPosition).sqrMagnitude > .25f) return;
        phase.SelectImage(slotIndex, pressedRevision);
    }

    public void OnSubmit(BaseEventData data)
    {
        if (CanClick) phase.SelectImage(slotIndex, phase.BoardRevision);
    }

    void OnDisable() { armed = false; }
    // No IDragHandler here: EventSystem forwards drag/wheel input to the parent ScrollRect.
}
