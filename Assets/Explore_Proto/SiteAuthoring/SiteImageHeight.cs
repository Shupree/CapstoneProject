using UnityEngine;
using UnityEngine.UI;

public sealed class SiteImageHeight : LayoutElement
{
    public override float preferredHeight
    {
        get
        {
            var image = GetComponent<Image>();
            if (!image || !image.sprite) return 0;
            var size = image.sprite.rect.size;
            return image.rectTransform.rect.width * size.y / size.x;
        }
        set => base.preferredHeight = value;
    }
}
