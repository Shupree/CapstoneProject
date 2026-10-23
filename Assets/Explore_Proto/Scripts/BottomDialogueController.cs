using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>CSV의 SceneID로 하단 대화를 시작하고, 한 줄씩 진행한 뒤 종료를 알린다.</summary>
public sealed class BottomDialogueController : MonoBehaviour
{
    [Header("CSV와 화면 연결")]
    [SerializeField] private DialogueCsvReader dialogueReader;
    [SerializeField] private BottomDialogueView view;
    [SerializeField] private BottomDialoguePortraitLibrary portraitLibrary;

    [Header("외부 진행 코드 연결")]
    [SerializeField] private UnityEvent<string> onEventRequested = new UnityEvent<string>();
    [SerializeField] private UnityEvent<string> onDialogueCompleted = new UnityEvent<string>();

    // 현재 SceneID의 대사와 표시 위치. null이면 대화가 진행 중이지 않다.
    private IReadOnlyList<DialogueLine> lines;
    private int lineIndex;
    private string currentSceneId;
    private Sprite leftSprite;
    private Sprite rightSprite;

    public bool IsPlaying => lines != null;
    public string CurrentSceneId => currentSceneId;
    /// <summary>마지막 줄을 닫은 뒤 외부 진행 코드에 완료된 대화 묶음을 알린다.</summary>
    public event Action<string> DialogueCompleted;

    /// <summary>미리 배치한 클릭 영역만 연결한다. 게임 오브젝트는 생성하지 않는다.</summary>
    private void Awake()
    {
        if (!dialogueReader) dialogueReader = GetComponent<DialogueCsvReader>();
        if (!view) view = GetComponent<BottomDialogueView>();

        if (!view) return;
        view.SetVisible(false);
        if (view.AdvanceButton) view.AdvanceButton.onClick.AddListener(NextDialogue);
    }

    /// <summary>컴포넌트가 없어질 때 버튼에 등록한 콜백을 해제한다.</summary>
    private void OnDestroy()
    {
        if (view && view.AdvanceButton)
            view.AdvanceButton.onClick.RemoveListener(NextDialogue);
    }

    /// <summary>지정한 SceneID의 CSV 대화를 시작한다. 기존 화상통화와 같은 리더를 사용한다.</summary>
    /// <returns>CSV와 필수 참조를 읽고 첫 줄을 표시했으면 true.</returns>
    public bool StartDialogue(string sceneId)
    {
        if (!dialogueReader || !view || !view.IsConfigured || !portraitLibrary)
        {
            Debug.LogError("하단 대화창의 CSV 리더, UI 또는 초상화 라이브러리 연결을 확인해 주세요.", this);
            return false;
        }

        if (!dialogueReader.TryGetScene(sceneId, out var loadedLines) || loadedLines.Count == 0)
        {
            Debug.LogError($"하단 대화 CSV에서 SceneID '{sceneId}'를 찾지 못했습니다.", this);
            return false;
        }

        lines = loadedLines;
        currentSceneId = sceneId;
        lineIndex = 0;
        PreparePortraits();
        view.SetVisible(true);
        ShowCurrentLine();
        return true;
    }

    /// <summary>Button 또는 UnityEvent의 문자열 인자로 대화를 시작할 때 쓰는 void 진입점.</summary>
    public void Play(string sceneId) => StartDialogue(sceneId);

    /// <summary>화면 클릭 또는 다른 게임 코드에서 호출해 다음 대사로 진행한다.</summary>
    public void NextDialogue()
    {
        if (!IsPlaying) return;

        lineIndex++;
        if (lineIndex < lines.Count)
        {
            ShowCurrentLine();
            return;
        }

        var completedSceneId = currentSceneId;
        StopDialogue();
        onDialogueCompleted.Invoke(completedSceneId);
        DialogueCompleted?.Invoke(completedSceneId);
    }

    /// <summary>대화를 중단하고 UI를 숨긴다. 정상 완료 이벤트는 보내지 않는다.</summary>
    public void StopDialogue()
    {
        lines = null;
        currentSceneId = null;
        lineIndex = 0;
        leftSprite = null;
        rightSprite = null;
        if (view) view.SetVisible(false);
    }

    /// <summary>모든 행을 살펴 두 캐릭터의 첫 이미지가 첫 대사부터 보이도록 준비한다.</summary>
    private void PreparePortraits()
    {
        leftSprite = null;
        rightSprite = null;
        var foundLeftImage = false;
        var foundRightImage = false;

        foreach (var line in lines)
        {
            if (!portraitLibrary.TryGetSpeaker(line.SpeakerID, out var speaker)) continue;

            if (speaker.side == BottomDialogueSide.Left)
            {
                if (!leftSprite && portraitLibrary.TryGetSprite(speaker.defaultImageId, out var fallback))
                    leftSprite = fallback;
                if (!foundLeftImage && portraitLibrary.TryGetSprite(line.Image, out var firstImage))
                {
                    leftSprite = firstImage;
                    foundLeftImage = true;
                }
            }
            else if (speaker.side == BottomDialogueSide.Right)
            {
                if (!rightSprite && portraitLibrary.TryGetSprite(speaker.defaultImageId, out var fallback))
                    rightSprite = fallback;
                if (!foundRightImage && portraitLibrary.TryGetSprite(line.Image, out var firstImage))
                {
                    rightSprite = firstImage;
                    foundRightImage = true;
                }
            }
        }
    }

    /// <summary>CSV 한 줄을 UI에 표시하고 EventID가 있으면 외부 리스너에 전달한다.</summary>
    private void ShowCurrentLine()
    {
        var line = lines[lineIndex];
        var side = BottomDialogueSide.None;

        if (portraitLibrary.TryGetSpeaker(line.SpeakerID, out var speaker))
            side = speaker.side;
        else if (!string.IsNullOrEmpty(line.SpeakerID))
            Debug.LogWarning($"CSV {line.SourceLine}행: 화자 '{line.SpeakerID}'의 초상화 위치가 등록되지 않았습니다.", this);

        if (!string.IsNullOrEmpty(line.Image))
        {
            if (!portraitLibrary.TryGetSprite(line.Image, out var sprite))
                Debug.LogWarning($"CSV {line.SourceLine}행: 이미지 ID '{line.Image}'에 연결된 Sprite가 없습니다.", this);
            else if (side == BottomDialogueSide.Left)
                leftSprite = sprite;
            else if (side == BottomDialogueSide.Right)
                rightSprite = sprite;
        }

        view.ShowLine(line.SpeakerID, line.Dialogue, leftSprite, rightSprite, side);
        if (!string.IsNullOrEmpty(line.EventID))
            onEventRequested.Invoke(line.EventID);
    }
}
