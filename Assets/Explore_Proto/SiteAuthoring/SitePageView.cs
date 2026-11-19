using TMPro;
using UnityEngine;

public sealed class SitePageView : MonoBehaviour
{
    public TextAsset source;
    public TMP_Text status;
    public SitePageData Data { get; private set; }
    public ExploreLinkCase Browser { get; private set; }

    void OnEnable() => AlignHorizontally();
    void OnRectTransformDimensionsChange() => AlignHorizontally();

    void AlignHorizontally()
    {
        if (!isActiveAndEnabled || !(transform is RectTransform page) ||
            !(page.parent is RectTransform viewport)) return;

        // With a top-center anchor and pivot, offset half the overflow to pin the left edge.
        var position = page.anchoredPosition;
        float x = Mathf.Max(0f, (page.rect.width - viewport.rect.width) * .5f);
        if (Mathf.Approximately(position.x, x)) return;
        page.anchoredPosition = new Vector2(x, position.y);
    }

    public void Bind(ExploreLinkCase browser)
    {
        Browser = browser;
        Data = JsonUtility.FromJson<SitePageData>(source.text);
        foreach (var slot in GetComponentsInChildren<SiteSlot>(true))
        {
            var content = Data.slots.Find(s => s.slotId == slot.id);
            if (content == null) slot.gameObject.SetActive(false);
            else if (slot.kind == SiteSlotKind.Text && content.blocks.Count == 1 &&
                slot.TryGetComponent<TMP_Text>(out var label)) label.text = content.blocks[0].text;
        }
    }
}
