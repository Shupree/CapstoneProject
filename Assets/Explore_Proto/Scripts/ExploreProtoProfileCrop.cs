using UnityEngine;
using UnityEngine.UI;

/// <summary>프로필 영역의 실제 크기에 맞춰 이미지 상단을 왜곡 없이 잘라 보여준다.</summary>
[RequireComponent(typeof(RawImage))]
public sealed class ExploreProtoProfileCrop : MonoBehaviour
{
    [SerializeField, Range(0.1f, 1f)] private float visibleTopFraction = 0.75f;

    private RawImage profileImage;
    private RectTransform profileRect;
    private Vector2 previousSize;
    private Texture previousTexture;

    private void OnEnable() => RefreshCrop();

    private void OnRectTransformDimensionsChange() => RefreshCrop();

    private void LateUpdate()
    {
        // 레이아웃이 첫 프레임 뒤에 확정되거나 원본 이미지가 교체된 경우에도 갱신한다.
        if (!profileRect) profileRect = (RectTransform)transform;
        if (!profileImage) profileImage = GetComponent<RawImage>();
        if (profileRect.rect.size != previousSize || profileImage.texture != previousTexture)
            RefreshCrop();
    }

    private void RefreshCrop()
    {
        if (!profileImage) profileImage = GetComponent<RawImage>();
        if (!profileRect) profileRect = (RectTransform)transform;

        Texture source = profileImage.texture;
        Vector2 size = profileRect.rect.size;
        if (!source || size.x <= 0f || size.y <= 0f) return;

        float height = visibleTopFraction;
        float width = height * (size.x / size.y) * (source.height / (float)source.width);
        if (width > 1f)
        {
            width = 1f;
            height = (source.width / (float)source.height) * (size.y / size.x);
        }

        Rect uv = new Rect((1f - width) * 0.5f, 1f - height, width, height);
        if (profileImage.uvRect != uv) profileImage.uvRect = uv;
        previousSize = size;
        previousTexture = source;
    }
}
