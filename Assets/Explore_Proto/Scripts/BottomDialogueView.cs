using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Hierarchy에 미리 배치한 하단 대화 UI의 내용과 밝기만 바꾼다.</summary>
public sealed class BottomDialogueView : MonoBehaviour
{
    [Header("미리 배치한 UI")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private Button advanceButton;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private GameObject nameBar;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [Header("이 씬에서 사용할 표시 방식")]
    [SerializeField] private GameObject screenDimmer;
    [SerializeField] private bool barOnly;

    [Header("현재 화자 표시")]
    [SerializeField] private Color speakingTint = Color.white;
    [SerializeField] private Color listeningTint = new Color(0.5f, 0.5f, 0.5f, 1f);

    /// <summary>화면 전체를 덮는 다음 대사 버튼. Controller가 클릭을 연결한다.</summary>
    public Button AdvanceButton => advanceButton;

    /// <summary>Inspector에서 필요한 UI 오브젝트를 모두 연결했는지 확인한다.</summary>
    public bool IsConfigured => visualRoot && advanceButton && leftPortrait && rightPortrait &&
                                nameBar && speakerText && dialogueText;

    /// <summary>저장된 UI를 표시하거나 숨긴다. 표시할 때 다른 창 위로 가져온다.</summary>
    public void SetVisible(bool visible)
    {
        if (!visualRoot) return;
        if (visible) transform.SetAsLastSibling();
        if (visible && screenDimmer) screenDimmer.SetActive(!barOnly);
        visualRoot.SetActive(visible);
    }

    /// <summary>현재 줄의 이름·대사·좌우 스프라이트와 화자 밝기를 갱신한다.</summary>
    public void ShowLine(string speaker, string dialogue, Sprite leftSprite, Sprite rightSprite,
        BottomDialogueSide speakingSide)
    {
        if (!IsConfigured) return;

        speakerText.text = speaker ?? string.Empty;
        dialogueText.text = dialogue ?? string.Empty;
        nameBar.SetActive(!string.IsNullOrWhiteSpace(speaker));

        SetPortrait(leftPortrait, barOnly ? null : leftSprite,
            speakingSide == BottomDialogueSide.Left ? speakingTint : listeningTint);
        SetPortrait(rightPortrait, barOnly ? null : rightSprite,
            speakingSide == BottomDialogueSide.Right ? speakingTint : listeningTint);
    }

    /// <summary>투명도는 유지하고 RGB만 낮춰 비화자를 어둡게 만든다.</summary>
    private static void SetPortrait(Image image, Sprite sprite, Color tint)
    {
        image.sprite = sprite;
        image.enabled = sprite != null;
        image.color = tint;
    }
}
