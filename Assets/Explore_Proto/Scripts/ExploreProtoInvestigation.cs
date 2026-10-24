using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>Clue collection and the second messenger exchange, using scene-authored UI.</summary>
public sealed class ExploreProtoInvestigation : MonoBehaviour
{
    [Serializable]
    public struct Clue
    {
        public string id;
        public bool optional;
        public bool excludeFromMemo;
        [TextArea(2, 5)] public string memoText;
    }

    public ExploreProtoMessenger messenger;
    public Clue[] clues;
    public TMP_Text memoNotes, articleHint, contactPreview, replyLabel;
    public GameObject monologueWindow;
    public UnityEngine.UI.Button monologueDismissButton, replyButton;
    public GameObject[] followUpMessages;
    public string[] replyLabels;
    [Header("탐색 씬 독백")]
    public BottomDialogueController bottomDialogue;
    public bool stopAfterResearch;
    public ExploreResearchQuestions researchQuestions;
    public string nicknameDialogueSceneId = "Explore_FamiliarNickname";
    public string memoryDialogueSceneId = "Explore_ClueMemory";
    public string completionDialogueSceneId = "Explore_ResearchComplete";

    readonly HashSet<string> collected = new HashSet<string>();
    readonly List<string> collectionOrder = new List<string>();
    readonly Queue<string> pendingMonologues = new Queue<string>();
    bool completionShown, followUpDelivered, followUpRead;
    int visibleMessages, replyStep;

    void Awake()
    {
        collected.Clear(); collectionOrder.Clear();
        memoNotes.text = string.Empty;
        monologueWindow.SetActive(false);
        replyButton.gameObject.SetActive(false);
        foreach (var message in followUpMessages) message.SetActive(false);
        monologueDismissButton.onClick.AddListener(DismissMonologue);
        replyButton.onClick.AddListener(ReplyToSeoyeon);
        if (bottomDialogue) bottomDialogue.DialogueCompleted += OnDialogueCompleted;
        SetArticleHint(null);
    }

    private void OnDestroy()
    {
        if (bottomDialogue) bottomDialogue.DialogueCompleted -= OnDialogueCompleted;
    }

    public bool IsClue(string id) => Array.Exists(clues, clue => clue.id == id);
    public bool IsCollected(string id) => collected.Contains(id);
    public int RequiredClueCount => Array.FindAll(clues, c => !c.optional).Length;
    public int RequiredCollectedCount => Array.FindAll(clues, c => !c.optional && collected.Contains(c.id)).Length;

    public string[] CollectedClueIds => collectionOrder.ToArray();

    // Restore data directly; do not replay collection, monologues or incoming notifications.
    public void RestoreCompletedInvestigation(string[] orderedIds)
    {
        collected.Clear(); collectionOrder.Clear();
        if (orderedIds != null)
            foreach (var id in orderedIds)
                if (IsClue(id) && collected.Add(id)) collectionOrder.Add(id);
        foreach (var clue in clues)
            if (collected.Add(clue.id)) collectionOrder.Add(clue.id);
        RefreshNotes();
        completionShown = followUpDelivered = followUpRead = true;
        visibleMessages = followUpMessages.Length;
        replyStep = replyLabels.Length;
        foreach (var message in followUpMessages) message.SetActive(true);
        monologueWindow.SetActive(false);
        replyButton.gameObject.SetActive(false);
        contactPreview.text = "함께 강령술을 준비하자.";
        SetArticleHint(null);
    }

    public bool Collect(string id)
    {
        if (!messenger.HasBloodyMaryKeyword || !IsClue(id) || !collected.Add(id)) return false;
        collectionOrder.Add(id);
        RefreshNotes();
        SetArticleHint(id);
        var collectedClue = Array.Find(clues, clue => clue.id == id);
        if (stopAfterResearch && !collectedClue.optional && !collectedClue.excludeFromMemo)
            QueueMonologue("Explore_RequiredMemoUpdated");
        if (researchQuestions && id == "mary_comment") QueueMonologue(nicknameDialogueSceneId);
        if (researchQuestions) researchQuestions.RefreshChoices();
        if (!researchQuestions && RequiredCollectedCount == RequiredClueCount && !completionShown)
        {
            completionShown = true;
            if (stopAfterResearch) QueueMonologue(completionDialogueSceneId);
            else
            {
                monologueWindow.SetActive(true);
                monologueWindow.transform.SetAsLastSibling();
            }
        }
        return true;
    }

    private void QueueMonologue(string sceneId)
    {
        if (!bottomDialogue || string.IsNullOrEmpty(sceneId)) return;
        pendingMonologues.Enqueue(sceneId);
        PlayNextMonologue();
    }

    private void PlayNextMonologue()
    {
        if (bottomDialogue && !bottomDialogue.IsPlaying && pendingMonologues.Count > 0)
            bottomDialogue.StartDialogue(pendingMonologues.Dequeue());
    }

    private void OnDialogueCompleted(string sceneId) => PlayNextMonologue();

    void RefreshNotes()
    {
        var notes = new StringBuilder();
        foreach (var savedId in collectionOrder)
        {
            var clue = Array.Find(clues, clue => clue.id == savedId);
            if (clue.excludeFromMemo) continue;
            if (notes.Length > 0) notes.Append("\n\n");
            notes.Append("• ").Append(clue.memoText);
        }
        memoNotes.text = notes.ToString();
    }

    public void SetArticleHint(string id)
    {
        if (!string.IsNullOrEmpty(id) && IsClue(id))
            articleHint.text = Array.Find(clues, c => c.id == id).excludeFromMemo
                ? (IsCollected(id) ? "확인한 단서입니다." : "클릭하면 단서를 확인합니다.")
                : (IsCollected(id) ? "메모장에 기록한 정보입니다." : "클릭하면 메모장에 기록됩니다.");
        else
            articleHint.text = "문장 위에 마우스를 올려 단서를 살펴보세요.  필수 " + RequiredCollectedCount + "/" + RequiredClueCount;
    }

    public void DismissMonologue()
    {
        if (stopAfterResearch) return;
        if (!completionShown || followUpDelivered || !monologueWindow.activeSelf) return;
        monologueWindow.SetActive(false);
        followUpDelivered = true;
        visibleMessages = 1;
        followUpMessages[0].SetActive(true);
        contactPreview.text = "새 메시지\n정보는 찾으셨나요?";
        messenger.ReceiveUnreadMessage();
    }

    public void OnMessengerRead()
    {
        if (stopAfterResearch) return;
        if (!followUpDelivered) return;
        followUpRead = true;
        bool canReply = visibleMessages < followUpMessages.Length;
        replyButton.gameObject.SetActive(canReply);
        if (canReply) replyLabel.text = replyLabels[replyStep];
        contactPreview.text = canReply ? "강령술에 대해 대화 중" : "함께 강령술을 준비하자.";
        messenger.ScrollConversationToLatest();
    }

    public void ReplyToSeoyeon()
    {
        if (stopAfterResearch) return;
        if (!followUpDelivered || !followUpRead) return;
        if (visibleMessages >= followUpMessages.Length)
        {
            ExploreProtoSceneTransfer.EnterBattle(messenger);
            return;
        }
        // Each click adds one player reply and the NPC's answer to the saved conversation.
        int next = Mathf.Min(visibleMessages + 2, followUpMessages.Length);
        while (visibleMessages < next) followUpMessages[visibleMessages++].SetActive(true);
        replyStep++;
        if (visibleMessages < followUpMessages.Length) replyLabel.text = replyLabels[replyStep];
        else
        {
            replyButton.gameObject.SetActive(false);
            contactPreview.text = "함께 강령술을 준비하자.";
        }
        messenger.ScrollConversationToLatest();
        if (visibleMessages == followUpMessages.Length)
            ExploreProtoSceneTransfer.EnterBattle(messenger);
    }
}
