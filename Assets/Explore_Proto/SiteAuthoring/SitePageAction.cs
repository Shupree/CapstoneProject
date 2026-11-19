using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SitePageAction : MonoBehaviour, IPointerClickHandler
{
    public string slotId;
    public int blockIndex;

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left || e.dragging) return;
        var view = GetComponentInParent<SitePageView>();
        var slot = view?.Data?.slots.Find(s => s.slotId == slotId);
        if (slot == null || blockIndex < 0 || blockIndex >= slot.blocks.Count) return;
        var block = slot.blocks[blockIndex];
        if (block.kind == SiteBlockKind.Link)
        {
            view.Browser.OpenAuthoredPage(block.destinationId);
            return;
        }
        var image = GetComponent<Image>();
        if (!image || !image.sprite) return;
        var rect = image.GetPixelAdjustedRect();
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(image.rectTransform,
            e.position, e.pressEventCamera, out var point)) return;
        // The editor uses the top-left origin of the full sprite rectangle.
        var size = image.sprite.rect.size;
        float scale = Mathf.Min(rect.width / size.x, rect.height / size.y);
        if (scale <= 0) return;
        size *= scale;
        var min = rect.min + Vector2.Scale(rect.size - size, image.rectTransform.pivot);
        var uv = new Vector2((point.x - min.x) / size.x, 1 - (point.y - min.y) / size.y);
        foreach (var area in block.areas)
            if (area.rect.Contains(uv))
            {
                view.Browser.Follow("inspect/" + area.eventId);
                return;
            }
    }
}
