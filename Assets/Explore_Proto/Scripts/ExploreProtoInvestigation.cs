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
        [TextArea(2, 5)] public string memoText;
    }

    public ExploreProtoMessenger messenger;
    public Clue[] clues;
    public TMP_Text memoNotes, articleHint, contactPreview, replyLabel;
    public GameObject monologueWindow;
    public UnityEngine.UI.Button monologueDismissButton, replyButton;
    public GameObject[] followUpMessages;
    public string[] replyLabels;

    readonly HashSet<string> collected = new HashSet<string>();
    readonly List<string> collectionOrder = new List<string>();
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
        replyButton.onClick.AddListener(ReplyToSiyul);
        SetArticleHint(null);
    }

    public bool IsClue(string id) => Array.Exists(clues, clue => clue.id == id);
    public bool IsCollected(string id) => collected.Contains(id);

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
        if (collected.Count == clues.Length && !completionShown)
        {
            completionShown = true;
            monologueWindow.SetActive(true);
            monologueWindow.transform.SetAsLastSibling();
        }
        return true;
    }

    void RefreshNotes()
    {
        var notes = new StringBuilder();
        foreach (var savedId in collectionOrder)
        {
            if (notes.Length > 0) notes.Append("\n\n");
            notes.Append("• ").Append(Array.Find(clues, clue => clue.id == savedId).memoText);
        }
        memoNotes.text = notes.ToString();
    }

    public void SetArticleHint(string id)
    {
        if (!string.IsNullOrEmpty(id) && IsClue(id))
            articleHint.text = IsCollected(id) ? "메모장에 기록한 정보입니다." : "클릭하면 메모장에 기록됩니다.";
        else
            articleHint.text = "문장 위에 마우스를 올려 단서를 살펴보세요.  " + collected.Count + "/" + clues.Length;
    }

    public void DismissMonologue()
    {
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
        if (!followUpDelivered) return;
        followUpRead = true;
        bool canReply = visibleMessages < followUpMessages.Length;
        replyButton.gameObject.SetActive(canReply);
        if (canReply) replyLabel.text = replyLabels[replyStep];
        contactPreview.text = canReply ? "강령술에 대해 대화 중" : "함께 강령술을 준비하자.";
        messenger.ScrollConversationToLatest();
    }

    public void ReplyToSiyul()
    {
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
