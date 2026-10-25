using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>Two-person call and the battle desktop transition.</summary>
public sealed class BattleProtoConversation : MonoBehaviour
{
    public ExploreProtoMessenger messenger;
    public GameObject groupCallWindow;
    [Serializable]
    public struct CallLine
    {
        public string speaker;
        [TextArea(2, 5)] public string text;
        public bool playProgrammingAfter;
        [InspectorName("주인공 스프라이트"), Tooltip("비워두면 이전 대사의 이미지를 유지합니다.")]
        public Sprite heroineSprite;
        [InspectorName("이서연 스프라이트"), Tooltip("비워두면 이전 대사의 이미지를 유지합니다.")]
        public Sprite clientSprite;
        [InspectorName("친구 스프라이트"), Tooltip("비워두면 이전 대사의 이미지를 유지합니다.")]
        public Sprite friendSprite;
    }
    public CallLine[] callLines;
    public TMP_Text callSpeaker, callText, nextLabel;
    public UnityEngine.UI.Button nextButton;
    public AudioSource programmingAudio;
    public BattleProtoRetrySequence retrySequence;
    public VideoCallPortraits callPortraits;
    public VideoCallConnectionSequence callConnection;
    [Header("공통 하단 대화 UI")]
    public BottomDialogueView callDialogueView;
    [Header("테마 전환")]
    [Min(0)] public int themeChangeAfterElement = 10;
    bool initialized;
    bool callStarted, waitingForSound;
    int callIndex;

    void Awake()
    {
        groupCallWindow.SetActive(false);
        nextButton.onClick.AddListener(AdvanceDialogue);
    }

    // Called after the exploration state has been restored, for both entry paths.
    public void BeginCall()
    {
        if (initialized) return;
        initialized = true;
        messenger.CloseMessenger();
        OpenGroupCall();
    }

    public void OpenGroupCall()
    {
        if (!initialized || (retrySequence && retrySequence.HasStarted)) return;
        groupCallWindow.SetActive(true);
        groupCallWindow.transform.SetAsLastSibling();
        if (callDialogueView) callDialogueView.SetVisible(false);
        if (!callStarted)
        {
            callStarted = true;
            callIndex = 0;
            if (callPortraits) callPortraits.ResetToDefaults();
            callSpeaker.text = string.Empty;
            callSpeaker.gameObject.SetActive(false);
            callText.text = string.Empty;
            nextLabel.text = "연결 중...";
            nextButton.interactable = false;
            if (!callConnection || !callConnection.TryBegin(
                    VideoCallConnectionSequence.ConnectionMode.UrgentOutgoing, ShowLine))
                ShowLine();
        }
    }

    public void AdvanceDialogue()
    {
        if (!callStarted || !groupCallWindow.activeInHierarchy || waitingForSound ||
            (callConnection && callConnection.IsConnecting) || (retrySequence && retrySequence.HasStarted)) return;
        if (callIndex >= themeChangeAfterElement)
        {
            if (retrySequence && retrySequence.BeginThemeTransition())
            {
                nextButton.interactable = false;
                if (callDialogueView) callDialogueView.SetVisible(false);
            }
            return;
        }
        if (callIndex >= callLines.Length - 1) return;
        if (callLines[callIndex].playProgrammingAfter)
            StartCoroutine(PlayProgrammingThenContinue());
        else { callIndex++; ShowLine(); }
    }

    IEnumerator PlayProgrammingThenContinue()
    {
        waitingForSound = true;
        nextButton.interactable = false;
        nextLabel.text = "프로그램 실행 중...";
        programmingAudio.Play();
        yield return null;
        while (programmingAudio.isPlaying) yield return null;
        waitingForSound = false;
        callIndex++;
        ShowLine();
    }

    void ShowLine()
    {
        if (callLines == null || callLines.Length == 0) return;
        var line = callLines[callIndex];
        if (callPortraits) callPortraits.Apply(line.heroineSprite, line.clientSprite, line.friendSprite);
        if (callPortraits) callPortraits.HighlightSpeaker(line.speaker);
        if (callDialogueView)
        {
            callDialogueView.SetVisible(true);
            callDialogueView.ShowLine(line.speaker, line.text, null, null, BottomDialogueSide.None);
        }
        bool named = !string.IsNullOrEmpty(line.speaker) && line.speaker != "나레이션";
        callSpeaker.text = named ? line.speaker : string.Empty;
        callSpeaker.gameObject.SetActive(named);
        callText.text = line.text;
        nextButton.interactable = callIndex < callLines.Length - 1 ||
            (callIndex >= themeChangeAfterElement && retrySequence && !retrySequence.HasStarted);
        nextLabel.text = "다음";
    }

}
