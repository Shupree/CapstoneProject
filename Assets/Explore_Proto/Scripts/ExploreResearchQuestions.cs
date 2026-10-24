using System;
using System.Collections;
using UnityEngine;

/// <summary>단서 조건으로 열리는 질문과 씬에 저장된 메신저 대화.</summary>
public sealed class ExploreResearchQuestions : MonoBehaviour
{
    [Serializable] public class Topic
    {
        public string id, clueId;
        public UnityEngine.UI.Button button;
        public GameObject[] messages;
        [NonSerialized] public bool completed;
    }
    public ExploreProtoInvestigation investigation;
    public ExploreProtoMessenger messenger;
    public Topic[] topics;
    public GameObject choicesRoot;
    public UnityEngine.UI.Button truthCallButton;
    public ExploreProtoNightSequence nightSequence;
    public DialogueCsvReader truthCallReader;
    public string truthCallSceneId = "Explore_TruthCall";
    [Min(0f)] public float messageGapSeconds = .7f;
    public bool TruthCallCompleted { get; private set; }
    public bool RequiredResearchCompleted => TruthCallCompleted && investigation.RequiredCollectedCount == investigation.RequiredClueCount;
    bool busy, callPending;

    void Awake()
    {
        foreach (var topic in topics)
        {
            var selected = topic;
            topic.completed = false;
            foreach (var message in topic.messages) message.SetActive(false);
            topic.button.onClick.AddListener(() => Ask(selected));
        }
        truthCallButton.onClick.AddListener(StartTruthCall);
        messenger.EmbeddedCallCompleted += OnCallCompleted;
        messenger.EmbeddedCallFailed += OnCallFailed;
        RefreshChoices();
    }
    bool Done(string id) => Array.Exists(topics, t => t.id == id && t.completed);
    bool Available(Topic topic) => !topic.completed && investigation.IsCollected(topic.clueId) &&
        (topic.id != "truth" || (Done("friend") && Done("side_effect")));

    public void RefreshChoices()
    {
        bool ready = messenger.HasCompletedFirstCall && !busy && !messenger.IsEmbeddedCallActive;
        bool timeAdvanced = nightSequence && nightSequence.IsTimeAdvanced;
        bool any = false;
        foreach (var topic in topics)
        {
            bool show = ready && !timeAdvanced && !callPending && Available(topic);
            topic.button.gameObject.SetActive(show);
            any |= show;
        }
        bool showTruthCall = ready && !timeAdvanced && callPending;
        truthCallButton.gameObject.SetActive(showTruthCall);
        bool showNightChoice = nightSequence && nightSequence.RefreshChoice(ready);
        choicesRoot.SetActive(any || showTruthCall || showNightChoice);
    }
    void Ask(Topic topic)
    {
        if (busy || callPending || messenger.IsEmbeddedCallActive || !Available(topic) || !messenger.HasCompletedFirstCall) return;
        StartCoroutine(RevealMessages(topic));
    }
    IEnumerator RevealMessages(Topic topic)
    {
        busy = true;
        RefreshChoices();
        // 선택한 대화 묶음을 현재 대화 맨 아래로 옮긴다. 오브젝트는 생성하지 않는다.
        if (topic.messages.Length > 0) topic.messages[0].transform.parent.SetAsLastSibling();
        choicesRoot.transform.SetAsLastSibling();
        foreach (var message in topic.messages)
        {
            message.SetActive(true);
            messenger.ScrollConversationToLatest();
            yield return new WaitForSecondsRealtime(messageGapSeconds);
        }
        topic.completed = true;
        if (topic.id == "truth") callPending = true;
        busy = false;
        RefreshChoices();
        messenger.ScrollConversationToLatest();
    }
    void StartTruthCall()
    {
        if (!callPending || busy) return;
        if (messenger.StartAdditionalCall(truthCallSceneId, truthCallReader)) RefreshChoices();
    }
    void OnCallCompleted(string sceneId)
    {
        if (sceneId == truthCallSceneId) { callPending = false; TruthCallCompleted = true; }
        RefreshChoices();
    }
    void OnCallFailed(string sceneId) => RefreshChoices();
    void OnDestroy()
    {
        if (messenger) messenger.EmbeddedCallCompleted -= OnCallCompleted;
        if (messenger) messenger.EmbeddedCallFailed -= OnCallFailed;
    }
}
