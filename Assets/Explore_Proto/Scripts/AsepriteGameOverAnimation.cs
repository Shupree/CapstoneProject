using System.Collections;
using UnityEngine;

/// <summary>Plays the imported, merged Aseprite clip once through a scene-authored UI image.</summary>
[RequireComponent(typeof(Animator))]
public sealed class AsepriteGameOverAnimation : MonoBehaviour
{
    public AnimationClip clip;
    public SpriteRenderer spriteSource;
    public UnityEngine.UI.Image artwork;
    [Tooltip("Aseprite 원본 캔버스의 가로·세로 픽셀 크기. 원본 캔버스를 리사이즈하면 함께 변경하세요.")]
    public Vector2 sourceCanvasSize = new(512f, 300f);

    public bool IsConfigured => clip && clip.length > 0f && spriteSource && artwork &&
        spriteSource.gameObject == gameObject && GetComponent<Animator>() &&
        sourceCanvasSize.x > 0f && sourceCanvasSize.y > 0f;

    public IEnumerator PlayOnce()
    {
        if (!IsConfigured) yield break;
        gameObject.SetActive(true);
        GetComponent<Animator>().cullingMode = AnimatorCullingMode.AlwaysAnimate;
        // The renderer receives the native Aseprite sprite track; only the uGUI image is visible.
        spriteSource.forceRenderingOff = true;
        double started = Time.unscaledTimeAsDouble;
        float duration = clip.length;
        while (Time.unscaledTimeAsDouble - started < duration)
        {
            clip.SampleAnimation(gameObject, (float)(Time.unscaledTimeAsDouble - started));
            CopySpriteToUI();
            yield return null;
        }
        gameObject.SetActive(false);
    }

    void CopySpriteToUI()
    {
        var sprite = spriteSource.sprite;
        artwork.enabled = sprite && spriteSource.enabled;
        if (!sprite) return;
        artwork.sprite = sprite;
        // Map the complete source canvas to the authored viewport, retaining each cropped
        // sprite's position within it. Independent axes fill the screen without cropping.
        var viewport = ((RectTransform)transform).rect.size;
        var scale = new Vector2(viewport.x / sourceCanvasSize.x, viewport.y / sourceCanvasSize.y);
        artwork.rectTransform.sizeDelta = Vector2.Scale(sprite.rect.size, scale);
        artwork.rectTransform.anchoredPosition = Vector2.Scale(
            (Vector2)sprite.bounds.center * sprite.pixelsPerUnit, scale);
    }
}
