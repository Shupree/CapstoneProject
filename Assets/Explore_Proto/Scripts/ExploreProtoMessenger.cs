using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>State and click behavior for the prebuilt Explore_Proto desktop apps.</summary>
public sealed class ExploreProtoMessenger : MonoBehaviour
{
    [Serializable] public struct CallLine { public string speaker; [TextArea] public string text; }
    [Serializable] public struct SiteEntry { public string category, title, summary; [TextArea] public string article; public Button button; }

    public GameObject messengerWindow, callWindow, transcriptWindow, memoWindow, internetWindow;
    public GameObject unreadBadge, linkButtonObject, keywordPopup, searchResults, siteDetailPanel, searchKeywordChip;
    public GameObject searchKeywordPlaceholder;
    public TMP_Text unreadCount;
    public TMP_Text callSpeaker, callLineText, transcriptText, memoKeyword, searchKeywordText, searchHint;
    public TMP_Text siteCategoryText, siteTitleText, siteArticleText;
    public Button messengerButton, emojiButton, internetButton, callAdvanceButton;
    public Button callButton, recordingButton, transcriptCloseButton, messengerCloseButton;
    public Button memoCloseButton, internetCloseButton, callCloseButton, transcriptTitleCloseButton;
    public ScrollRect transcriptScroll, siteArticleScroll;
    public ScrollRect conversationScroll;
    public ExploreProtoInvestigation investigation;
    public ExploreProtoArticleLinks articleLinks;
    public Button addKeywordButton, keywordFieldButton, popupCloseButton, popupKeywordButton, findButton, clearKeywordButton, siteBackButton;
    public TMP_Text linkMessage;
    public CallLine[] callLines;
    public SiteEntry[] siteEntries;
    private int lineIndex;
    private bool callCompleted, bloodyMaryStored, bloodyMarySelected;
    public bool HasBloodyMaryKeyword => bloodyMaryStored;

    private void Awake()
    {
        messengerButton.onClick.AddListener(ToggleMessenger);
        emojiButton.onClick.AddListener(ToggleMemo);
        internetButton.onClick.AddListener(ToggleInternet);
        messengerCloseButton.onClick.AddListener(CloseMessenger);
        memoCloseButton.onClick.AddListener(CloseMemo);
        internetCloseButton.onClick.AddListener(CloseInternet);
        callButton.onClick.AddListener(StartCall);
        callCloseButton.onClick.AddListener(CloseCall);
        callAdvanceButton.onClick.AddListener(AdvanceCall);
        recordingButton.onClick.AddListener(ShowTranscript);
        transcriptCloseButton.onClick.AddListener(CloseTranscript);
        if (transcriptTitleCloseButton) transcriptTitleCloseButton.onClick.AddListener(CloseTranscript);
        addKeywordButton.onClick.AddListener(AddBloodyMaryKeyword);
        keywordFieldButton.onClick.AddListener(ToggleKeywordPopup);
        popupCloseButton.onClick.AddListener(() => keywordPopup.SetActive(false));
        popupKeywordButton.onClick.AddListener(SelectBloodyMaryKeyword);
        findButton.onClick.AddListener(Search);
        clearKeywordButton.onClick.AddListener(ClearSearchKeyword);
        siteBackButton.onClick.AddListener(BackToResults);
        for (int i = 0; i < siteEntries.Length; i++)
        {
            int index = i;
            siteEntries[i].button.onClick.AddListener(() => OpenSite(index));
        }
        bloodyMaryStored = false;
        bloodyMarySelected = false;
        memoKeyword.gameObject.SetActive(false);
        searchKeywordChip.SetActive(false);
        searchKeywordText.gameObject.SetActive(false);
        searchKeywordPlaceholder.SetActive(true);
        clearKeywordButton.gameObject.SetActive(false);
        keywordPopup.SetActive(false);
        searchResults.SetActive(false);
        siteDetailPanel.SetActive(false);
        linkButtonObject.SetActive(false);
        findButton.interactable = false;
        popupKeywordButton.gameObject.SetActive(false);
        searchKeywordText.text = "키워드를 선택하세요";
        searchHint.text = "획득한 키워드를 선택해 검색하세요.";
    }

    public void ToggleMessenger()
    {
        // A new message brings an already open window to the front instead of closing it.
        if (!messengerWindow.activeSelf || unreadBadge.activeSelf) ShowMessenger();
        else CloseMessenger();
    }
    public void ShowMessenger()
    {
        messengerWindow.SetActive(true);
        messengerWindow.transform.SetAsLastSibling();
        MarkMessengerRead();
        if (callCompleted) ScrollConversationToLatest();
        if (investigation) investigation.OnMessengerRead();
    }
    public void ReceiveUnreadMessage()
    {
        unreadCount.text = "1";
        unreadBadge.SetActive(true);
    }
    public void ScrollConversationToLatest()
    {
        if (!conversationScroll || !messengerWindow.activeInHierarchy) return;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(conversationScroll.content);
        conversationScroll.StopMovement();
        conversationScroll.verticalNormalizedPosition = 0f;
    }
    private void MarkMessengerRead()
    {
        unreadBadge.SetActive(false);
        unreadCount.text = "0";
    }
    public void ToggleMemo() => ToggleWindow(memoWindow);
    public void ToggleInternet() => ToggleWindow(internetWindow);
    private static void ToggleWindow(GameObject window)
    {
        bool open = !window.activeSelf;
        window.SetActive(open);
        if (open) window.transform.SetAsLastSibling();
    }
    public void CloseMessenger() => messengerWindow.SetActive(false);
    public void CloseMemo() => memoWindow.SetActive(false);
    public void CloseInternet() { internetWindow.SetActive(false); keywordPopup.SetActive(false); }

    public void StartCall()
    {
        MarkMessengerRead();
        lineIndex = 0; callCompleted = false; callButton.gameObject.SetActive(false);
        callWindow.SetActive(true); callWindow.transform.SetAsLastSibling(); ShowCallLine();
    }
    public void CloseCall() => callWindow.SetActive(false);
    public void AdvanceCall()
    {
        if (!callWindow.activeSelf) return;
        lineIndex++;
        if (lineIndex >= callLines.Length)
        {
            callWindow.SetActive(false); callCompleted = true;
            MarkMessengerRead();
            recordingButton.gameObject.SetActive(true);
            if (linkMessage) linkMessage.gameObject.SetActive(true);
            linkButtonObject.SetActive(true);
            ScrollConversationToLatest();
            return;
        }
        ShowCallLine();
    }
    private void ShowCallLine()
    {
        callSpeaker.text = callLines[lineIndex].speaker;
        callLineText.text = callLines[lineIndex].text;
    }
    public void ShowTranscript()
    {
        if (!callCompleted) return;
        var b = new StringBuilder();
        foreach (var line in callLines) b.Append("[<b>").Append(line.speaker).Append("</b>] ").Append(line.text).Append("\n\n");
        transcriptText.text = b.ToString(); transcriptWindow.SetActive(true); transcriptWindow.transform.SetAsLastSibling();
        ResetScroll(transcriptScroll);
    }
    public void CloseTranscript() => transcriptWindow.SetActive(false);

    public void AddBloodyMaryKeyword()
    {
        bloodyMaryStored = true;
        memoKeyword.text = "블러디 메리";
        memoKeyword.gameObject.SetActive(true);
        var label = addKeywordButton.GetComponentInChildren<TMP_Text>();
        if (label) label.text = "'블러디 메리' 키워드 추가";
        addKeywordButton.interactable = false;
        popupKeywordButton.gameObject.SetActive(true);
    }
    public void ToggleKeywordPopup()
    {
        keywordPopup.SetActive(!keywordPopup.activeSelf);
        if (keywordPopup.activeSelf)
        {
            keywordPopup.transform.SetAsLastSibling();
            PositionKeywordPopup();
        }
        popupKeywordButton.gameObject.SetActive(bloodyMaryStored);
    }
    private void PositionKeywordPopup()
    {
        var popup = (RectTransform)keywordPopup.transform;
        var parent = (RectTransform)popup.parent;
        var field = (RectTransform)keywordFieldButton.transform;
        var corners = new Vector3[4];
        field.GetWorldCorners(corners);
        var position = parent.InverseTransformPoint(corners[0]);
        position.x += popup.rect.width * .5f;
        position.y -= popup.rect.height * .5f + 16f;
        position.x = Mathf.Clamp(position.x, parent.rect.xMin + popup.rect.width * .5f + 24f, parent.rect.xMax - popup.rect.width * .5f - 24f);
        position.y = Mathf.Clamp(position.y, parent.rect.yMin + popup.rect.height * .5f + 24f, parent.rect.yMax - popup.rect.height * .5f - 24f);
        popup.localPosition = position;
    }
    public void SelectBloodyMaryKeyword()
    {
        if (!bloodyMaryStored) return;
        bloodyMarySelected = true;
        searchKeywordText.text = "블러디 메리";
        searchKeywordChip.SetActive(true);
        searchKeywordText.gameObject.SetActive(true);
        searchKeywordPlaceholder.SetActive(false);
        clearKeywordButton.gameObject.SetActive(true);
        findButton.interactable = true;
        searchHint.text = "선택한 키워드로 검색할 준비가 되었어요.";
        keywordPopup.SetActive(false);
    }
    public void ClearSearchKeyword()
    {
        bloodyMarySelected = false;
        searchKeywordText.text = "키워드를 선택하세요";
        searchKeywordChip.SetActive(false);
        searchKeywordText.gameObject.SetActive(false);
        searchKeywordPlaceholder.SetActive(true);
        clearKeywordButton.gameObject.SetActive(false);
        findButton.interactable = false;
        searchResults.SetActive(false);
        siteDetailPanel.SetActive(false);
        searchHint.text = "획득한 키워드를 선택해 검색하세요.";
    }
    public void Search()
    {
        if (!bloodyMarySelected) return;
        keywordPopup.SetActive(false); siteDetailPanel.SetActive(false);
        searchResults.SetActive(true); searchHint.text = "‘블러디 메리’ 검색 결과";
        ResetScroll(searchResults.GetComponent<ScrollRect>());
    }
    private void OpenSite(int index)
    {
        if (index < 0 || index >= siteEntries.Length) return;
        var site = siteEntries[index];
        siteCategoryText.text = site.category;
        siteTitleText.text = site.title;
        if (articleLinks) articleLinks.SetArticle(site.article);
        else siteArticleText.text = site.article;
        searchResults.SetActive(false); siteDetailPanel.SetActive(true);
        ResetScroll(siteArticleScroll);
    }
    private static void ResetScroll(ScrollRect scroll)
    {
        if (!scroll) return;
        // Text height is updated before returning the document to its first line.
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        scroll.StopMovement();
        scroll.verticalNormalizedPosition = 1f;
    }
    public void BackToResults()
    {
        siteDetailPanel.SetActive(false);
        searchResults.SetActive(true);
    }
}
