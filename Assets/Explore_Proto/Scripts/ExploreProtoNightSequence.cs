using System.Collections;
using UnityEngine;

/// <summary>탐색 완료 후 밤으로 시간을 바꾸고, 메신저 대화가 끝나면 배틀 통화를 연다.</summary>
public sealed class ExploreProtoNightSequence : MonoBehaviour
{
    [Header("진행 참조")]
    public ExploreResearchQuestions researchQuestions;
    public ExploreProtoMessenger messenger;
    public UnityEngine.UI.Button waitButton;
    public UnityEngine.UI.Button startRitualButton;

    [Header("씬에 저장된 연출 UI")]
    public UnityEngine.UI.Image blackOverlay;
    public GameObject nightConversation;
    public GameObject[] nightMessages;

    [Header("연출 시간")]
    [Min(0f)] public float fadeSeconds = 0.7f;
    [Min(0f)] public float blackHoldSeconds = 1f;
    [Min(0f)] public float messageGapSeconds = 0.7f;

    public bool IsTimeAdvanced { get; private set; }
    bool ritualReady;

    void Awake()
    {
        blackOverlay.gameObject.SetActive(false);
        waitButton.gameObject.SetActive(false);
        startRitualButton.gameObject.SetActive(false);
        foreach (var message in nightMessages) message.SetActive(false);
        waitButton.onClick.AddListener(WaitUntilNight);
        startRitualButton.onClick.AddListener(StartRitual);
    }

    void OnDestroy()
    {
        if (waitButton) waitButton.onClick.RemoveListener(WaitUntilNight);
        if (startRitualButton) startRitualButton.onClick.RemoveListener(StartRitual);
    }

    /// <summary>필수 단서 5개와 진실 확인 통화를 마쳐야 기다리기 선택지를 보인다.</summary>
    public bool RefreshChoice(bool messengerReady)
    {
        bool showWait = messengerReady && !IsTimeAdvanced && researchQuestions.RequiredResearchCompleted;
        bool showRitual = messengerReady && ritualReady;
        waitButton.gameObject.SetActive(showWait);
        startRitualButton.gameObject.SetActive(showRitual);
        return showWait || showRitual;
    }

    void WaitUntilNight()
    {
        if (IsTimeAdvanced || !researchQuestions.RequiredResearchCompleted || messenger.IsEmbeddedCallActive) return;
        IsTimeAdvanced = true;
        researchQuestions.RefreshChoices();
        StartCoroutine(AdvanceToNight());
    }

    IEnumerator AdvanceToNight()
    {
        blackOverlay.color = new Color(0f, 0f, 0f, 0f);
        blackOverlay.gameObject.SetActive(true);
        blackOverlay.transform.SetAsLastSibling();
        yield return FadeTo(1f);
        yield return new WaitForSecondsRealtime(blackHoldSeconds);
        yield return FadeTo(0f);
        blackOverlay.gameObject.SetActive(false);

        nightConversation.transform.SetAsLastSibling();
        researchQuestions.choicesRoot.transform.SetAsLastSibling();
        foreach (var message in nightMessages)
        {
            message.SetActive(true);
            messenger.ScrollConversationToLatest();
            yield return new WaitForSecondsRealtime(messageGapSeconds);
        }
        ritualReady = true;
        researchQuestions.RefreshChoices();
        messenger.ScrollConversationToLatest();
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = blackOverlay.color.a;
        if (fadeSeconds <= 0f)
        {
            blackOverlay.color = new Color(0f, 0f, 0f, targetAlpha);
            yield break;
        }
        for (float elapsed = 0f; elapsed < fadeSeconds; elapsed += Time.unscaledDeltaTime)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeSeconds);
            blackOverlay.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        blackOverlay.color = new Color(0f, 0f, 0f, targetAlpha);
    }

    void StartRitual()
    {
        if (!ritualReady || !researchQuestions.RequiredResearchCompleted) return;
        ExploreProtoSceneTransfer.EnterBattle(messenger);
    }
}
