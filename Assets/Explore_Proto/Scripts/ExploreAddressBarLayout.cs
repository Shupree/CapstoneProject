using UnityEngine;

/// <summary>Keeps browser controls at their opening pixel size as the canvas and window resize.</summary>
[RequireComponent(typeof(RectTransform))]
public sealed class ExploreAddressBarLayout : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    private RectTransform bar, window;
    private Canvas canvasRoot;
    private Vector2 originalSize, originalPosition;
    private float openingScale, originalContentTop;

    private void Awake()
    {
        bar = (RectTransform)transform;
        window = (RectTransform)bar.parent;
        canvasRoot = GetComponentInParent<Canvas>().rootCanvas;
        originalSize = bar.sizeDelta;
        originalPosition = bar.anchoredPosition;
        originalContentTop = content.offsetMax.y;
    }

    private void LateUpdate()
    {
        float scale = canvasRoot.scaleFactor;
        if (scale <= 0f) return;
        if (openingScale == 0f) openingScale = scale;
        float ratio = openingScale / scale;

        // The title bar follows the desktop scale. Keep our top edge against its bottom.
        float height = originalSize.y * ratio;
        bar.localScale = new Vector3(ratio, ratio, 1f);
        float availableWidth = window.rect.width + originalSize.x;
        bar.sizeDelta = new Vector2(availableWidth / ratio - window.rect.width, originalSize.y);
        float heightChange = height - originalSize.y;
        bar.anchoredPosition = originalPosition - Vector2.up * (heightChange * (1f - bar.pivot.y));
        Vector2 top = content.offsetMax;
        top.y = originalContentTop - heightChange;
        content.offsetMax = top;
    }
}
