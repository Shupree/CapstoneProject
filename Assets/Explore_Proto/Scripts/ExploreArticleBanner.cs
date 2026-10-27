using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Scene-authored article banner; keeps the original article data intact.</summary>
public sealed class ExploreArticleBanner : MonoBehaviour
{
    public TMP_Text author;
    public UnityEngine.UI.Image banner;
    public UnityEngine.UI.LayoutElement bannerLayout;
    public ExploreProtoExorcismVision vision;
    public Sprite basic, left, right, top, bottom;
    public string authorPrefix = "작성자 : 괴담레스토랑";
    [Tooltip("Eye midpoint in the image, normalized from bottom-left.")]
    public Vector2 eyePosition = new Vector2(.787f, .606f);

    public string PrepareArticle(string source)
    {
        bool visible = !string.IsNullOrEmpty(source) && source.StartsWith(authorPrefix, System.StringComparison.Ordinal);
        author.gameObject.SetActive(visible);
        bannerLayout.gameObject.SetActive(visible);
        if (!visible) return source;
        int end = source.IndexOf('\n');
        author.text = end < 0 ? source : source.Substring(0, end).TrimEnd('\r');
        UpdateBanner();
        return end < 0 ? string.Empty : source.Substring(end).TrimStart('\r', '\n');
    }

    void LateUpdate()
    {
        if (banner && banner.gameObject.activeInHierarchy) UpdateBanner();
    }

    void UpdateBanner()
    {
        if (!basic || !banner || !bannerLayout) return;
        float height = banner.rectTransform.rect.width * basic.rect.height / basic.rect.width + 40f;
        if (!Mathf.Approximately(bannerLayout.preferredHeight, height)) bannerLayout.preferredHeight = height;
        Sprite next = basic;
        if (vision && vision.IsActive)
        {
            next = left;
            var mouse = Mouse.current;
            var canvas = banner.canvas;
            Camera camera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (mouse != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                banner.rectTransform, mouse.position.ReadValue(), camera, out var point))
            {
                Rect rect = banner.rectTransform.rect;
                Vector2 delta = new Vector2((point.x - rect.xMin) / Mathf.Max(1, rect.width),
                    (point.y - rect.yMin) / Mathf.Max(1, rect.height)) - eyePosition;
                // Compare normalized axes for the wide banner. Ties (including zero) choose vertical.
                next = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                    ? (delta.x > 0 ? right : left) : (delta.y >= 0 ? top : bottom);
            }
        }
        if (banner.sprite != next) banner.sprite = next;
    }
}
