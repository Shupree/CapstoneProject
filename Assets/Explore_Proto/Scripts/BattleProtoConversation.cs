using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>Continues the saved messenger conversation and opens the scene-authored three-person call.</summary>
public sealed class BattleProtoConversation : MonoBehaviour
{
    public ExploreProtoMessenger messenger;
    public GameObject[] preparationMessages;
    public UnityEngine.UI.Button startCallButton;
    public GameObject groupCallWindow;
    [Serializable]
    public struct CallLine
    {
        public string speaker;
        [TextArea(2, 5)] public string text;
        public bool playProgrammingAfter;
        [InspectorName("주인공 스프라이트"), Tooltip("비워두면 이전 대사의 이미지를 유지합니다.")]
        public Sprite heroineSprite;
        [InspectorName("시율 스프라이트"), Tooltip("비워두면 이전 대사의 이미지를 유지합니다.")]
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
    bool initialized;
    bool callStarted, waitingForSound;
    int callIndex;

    void Awake()
    {
        foreach (var message in preparationMessages) message.SetActive(false);
        startCallButton.gameObject.SetActive(false);
        startCallButton.onClick.AddListener(OpenGroupCall);
        nextButton.onClick.AddListener(AdvanceDialogue);
    }

    // Called after the exploration state has been restored, for both entry paths.
    public void BeginPreparation()
    {
        if (initialized) return;
        initialized = true;
        foreach (var message in preparationMessages) message.SetActive(true);
        startCallButton.gameObject.SetActive(true);
        messenger.ShowMessenger();
    }

    public void OpenGroupCall()
    {
        if (!initialized || (retrySequence && retrySequence.HasStarted)) return;
        groupCallWindow.SetActive(true);
        groupCallWindow.transform.SetAsLastSibling();
        if (!callStarted)
        {
            callStarted = true;
            callIndex = 0;
            if (callPortraits) callPortraits.ResetToDefaults();
            ShowLine();
        }
    }

    public void AdvanceDialogue()
    {
        if (!callStarted || !groupCallWindow.activeInHierarchy || waitingForSound || (retrySequence && retrySequence.HasStarted)) return;
        if (callIndex >= callLines.Length - 1)
        {
            if (retrySequence && retrySequence.BeginRetry())
            {
                nextButton.interactable = false;
                nextLabel.text = "프로그램 실행 중...";
            }
            return;
        }
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
        bool named = !string.IsNullOrEmpty(line.speaker) && line.speaker != "나레이션";
        callSpeaker.text = named ? line.speaker : string.Empty;
        callSpeaker.gameObject.SetActive(named);
        callText.text = line.text;
        bool hasNext = callIndex < callLines.Length - 1;
        nextButton.interactable = hasNext || (retrySequence && !retrySequence.HasStarted);
        nextLabel.text = hasNext ? "다음" : "다시 한번 시도하기";
    }
}
