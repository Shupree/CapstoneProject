using System;
using UnityEngine;

/// <summary>Changes the existing video-call images without altering their scene-authored layout.</summary>
public sealed class VideoCallPortraits : MonoBehaviour
{
    [InspectorName("주인공 이미지")] public UnityEngine.UI.Image heroineImage;
    [InspectorName("이서연 이미지")] public UnityEngine.UI.Image clientImage;
    [InspectorName("친구 이미지")] public UnityEngine.UI.Image friendImage;
    [Header("현재 화자 테두리")]
    public GameObject heroineHighlight;
    public GameObject clientHighlight;
    public string heroineSpeakerId = "유미나";
    public string clientSpeakerId = "이서연";
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
        HighlightSpeaker(null);
    }

    public void Apply(Sprite heroine, Sprite client, Sprite friend)
    {
        CaptureDefaults();
        // Empty per-line fields retain the previous image, including on narration lines.
        if (heroineImage && heroine) heroineImage.sprite = heroine;
        if (clientImage && client) clientImage.sprite = client;
        if (friendImage && friend) friendImage.sprite = friend;
    }

    /// <summary>현재 화자의 카드 테두리를 켠다. 쉼표로 묶인 공동 대사는 두 카드를 모두 강조한다.</summary>
    public void HighlightSpeaker(string speakerId)
    {
        if (heroineHighlight)
            heroineHighlight.SetActive(HasSpeaker(speakerId, heroineSpeakerId));
        if (clientHighlight)
            clientHighlight.SetActive(HasSpeaker(speakerId, clientSpeakerId));
    }

    static bool HasSpeaker(string speakerIds, string target)
    {
        if (string.IsNullOrWhiteSpace(speakerIds) || string.IsNullOrWhiteSpace(target)) return false;
        foreach (var speaker in speakerIds.Split(','))
            if (string.Equals(speaker.Trim(), target, StringComparison.Ordinal)) return true;
        return false;
    }
}
