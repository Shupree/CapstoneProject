using System;
using UnityEngine;

/// <summary>하단 대화창에서 캐릭터가 표시될 위치.</summary>
public enum BottomDialogueSide
{
    None,
    Left,
    Right
}

/// <summary>CSV의 SpeakerID·Image 문자열을 미리 등록한 Sprite와 화면 위치에 연결한다.</summary>
[CreateAssetMenu(fileName = "BottomDialoguePortraitLibrary", menuName = "Explore Proto/Bottom Dialogue Portrait Library")]
public sealed class BottomDialoguePortraitLibrary : ScriptableObject
{
    [Serializable]
    public struct PortraitEntry
    {
        [Tooltip("CSV Image 열에 적는 이름. 대소문자를 구분합니다.")]
        public string imageId;
        public Sprite sprite;
    }

    [Serializable]
    public struct SpeakerEntry
    {
        [Tooltip("CSV SpeakerID 열에 적는 이름. 대소문자를 구분합니다.")]
        public string speakerId;
        public BottomDialogueSide side;
        [Tooltip("해당 화자의 Image 셀이 비어 있을 때 사용할 기본 이미지 ID.")]
        public string defaultImageId;
    }

    [SerializeField] private PortraitEntry[] portraits = Array.Empty<PortraitEntry>();
    [SerializeField] private SpeakerEntry[] speakers = Array.Empty<SpeakerEntry>();

    /// <summary>CSV의 이미지 ID에 대응하는 Sprite를 찾는다.</summary>
    public bool TryGetSprite(string imageId, out Sprite sprite)
    {
        sprite = null;
        if (string.IsNullOrEmpty(imageId)) return false;

        foreach (var entry in portraits)
        {
            if (!string.Equals(entry.imageId, imageId, StringComparison.Ordinal) || !entry.sprite)
                continue;

            sprite = entry.sprite;
            return true;
        }

        return false;
    }

    /// <summary>CSV 화자 ID에 대응하는 초상화 위치와 기본 이미지 ID를 찾는다.</summary>
    public bool TryGetSpeaker(string speakerId, out SpeakerEntry speaker)
    {
        speaker = default;
        if (string.IsNullOrEmpty(speakerId)) return false;

        foreach (var entry in speakers)
        {
            if (!string.Equals(entry.speakerId, speakerId, StringComparison.Ordinal))
                continue;

            speaker = entry;
            return true;
        }

        return false;
    }
}
