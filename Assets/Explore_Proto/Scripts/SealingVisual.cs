using UnityEngine;

/// <summary>씬에 저장된 이미지의 픽셀 소멸·가로 공개·밝기. 원본 머티리얼은 변경하지 않는다.</summary>
[RequireComponent(typeof(UnityEngine.UI.RawImage))]
public sealed class SealingVisual : MonoBehaviour
{
    public UnityEngine.UI.RawImage image;
    public Material template;
    public Vector2 pixelGrid = new(150, 100);
    public bool revealFromRight;
    Material instance;

    public void Set(float reveal = 1f, float dissolve = 0f, float brightness = 1f)
    {
        if (!instance)
        {
            instance = new Material(template);
            image.material = instance;
        }
        instance.SetFloat("_Reveal", Mathf.Clamp01(reveal));
        instance.SetFloat("_Dissolve", Mathf.Clamp01(dissolve));
        instance.SetFloat("_Brightness", brightness);
        instance.SetFloat("_RevealFromRight", revealFromRight ? 1 : 0);
        instance.SetVector("_PixelGrid", new Vector4(pixelGrid.x, pixelGrid.y, 0, 0));
    }

    void OnDestroy()
    {
        if (instance) Destroy(instance);
    }
}
