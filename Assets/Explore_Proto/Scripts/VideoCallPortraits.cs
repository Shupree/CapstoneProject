using UnityEngine;

/// <summary>Changes the existing video-call images without altering their scene-authored layout.</summary>
public sealed class VideoCallPortraits : MonoBehaviour
{
    [InspectorName("주인공 이미지")] public UnityEngine.UI.Image heroineImage;
    [InspectorName("이서연 이미지")] public UnityEngine.UI.Image clientImage;
    [InspectorName("친구 이미지")] public UnityEngine.UI.Image friendImage;
    Sprite defaultHeroine, defaultClient, defaultFriend;
    bool captured;

    void Awake() => CaptureDefaults();

    void CaptureDefaults()
    {
        if (captured) return;
        defaultHeroine = heroineImage ? heroineImage.sprite : null;
        defaultClient = clientImage ? clientImage.sprite : null;
        defaultFriend = friendImage ? friendImage.sprite : null;
        captured = true;
    }

    public void ResetToDefaults()
    {
        CaptureDefaults();
        if (heroineImage) heroineImage.sprite = defaultHeroine;
        if (clientImage) clientImage.sprite = defaultClient;
        if (friendImage) friendImage.sprite = defaultFriend;
    }

    public void Apply(Sprite heroine, Sprite client, Sprite friend)
    {
        CaptureDefaults();
        // Empty per-line fields retain the previous image, including on narration lines.
        if (heroineImage && heroine) heroineImage.sprite = heroine;
        if (clientImage && client) clientImage.sprite = client;
        if (friendImage && friend) friendImage.sprite = friend;
    }
}
