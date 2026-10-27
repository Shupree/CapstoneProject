using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>In-scene browser history; renders existing article/search UI without changing investigation state.</summary>
public sealed class ExploreBrowserNavigation : MonoBehaviour
{
    [Serializable]
    public sealed class NavigationButton
    {
        public UnityEngine.UI.Button button;
        public UnityEngine.UI.Image[] icons;
        public Sprite[] enabledSprites, disabledSprites;
        public void SetAvailable(bool available)
        {
            button.interactable = available;
            var sprites = available ? enabledSprites : disabledSprites;
            for (int i = 0; i < icons.Length; i++) icons[i].sprite = sprites[i];
        }
    }
    public sealed class Page
    {
        public int siteIndex = -1;
        public bool keywordSelected, resultsVisible;
        public string hint;
        public float scroll = 1f;
        public Page Copy() => (Page)MemberwiseClone();
    }

    public ExploreProtoMessenger messenger;
    public GameObject searchChrome;
    public NavigationButton home, back, forward;
    readonly List<Page> history = new List<Page>();
    int cursor = -1;

    void Awake()
    {
        home.button.onClick.AddListener(GoHome);
        back.button.onClick.AddListener(GoBack);
        forward.button.onClick.AddListener(GoForward);
    }
    void Start() { EnsureHistory(); RefreshControls(); }
    void OnDestroy()
    {
        home.button.onClick.RemoveListener(GoHome);
        back.button.onClick.RemoveListener(GoBack);
        forward.button.onClick.RemoveListener(GoForward);
    }
    void EnsureHistory()
    {
        if (cursor >= 0) return;
        var page = messenger.CaptureBrowserPage();
        if (page.siteIndex >= 0)
        {
            var search = page.Copy(); search.siteIndex = -1; search.resultsVisible = search.keywordSelected; search.scroll = 1f;
            history.Add(search);
        }
        history.Add(page);
        cursor = history.Count - 1;
    }
    public void RememberCurrentPage()
    {
        EnsureHistory();
        history[cursor] = messenger.CaptureBrowserPage();
    }
    public void SearchChanged()
    {
        EnsureHistory();
        // Keyword/search edits replace the current search home and start a fresh forward branch.
        history[cursor] = messenger.CaptureBrowserPage();
        DiscardForward();
        RefreshControls();
    }
    void DiscardForward()
    {
        int count = history.Count - cursor - 1;
        if (count > 0) history.RemoveRange(cursor + 1, count);
    }
    public void OpenSite(int index)
    {
        if (index < 0 || index >= messenger.siteEntries.Length) return;
        RememberCurrentPage();
        var page = history[cursor].Copy();
        page.siteIndex = index; page.scroll = 1f;
        DiscardForward();
        history.Add(page); cursor++;
        ShowCurrent();
    }
    int HomeIndex()
    {
        if (cursor < 0 || history[cursor].siteIndex < 0) return -1;
        for (int i = cursor - 1; i >= 0; i--) if (history[i].siteIndex < 0) return i;
        return -1;
    }
    public void GoHome()
    {
        EnsureHistory();
        int target = HomeIndex();
        if (target < 0) return;
        RememberCurrentPage(); cursor = target; ShowCurrent();
    }
    public void GoBack()
    {
        EnsureHistory();
        if (cursor <= 0) return;
        RememberCurrentPage(); cursor--; ShowCurrent();
    }
    public void GoForward()
    {
        EnsureHistory();
        if (cursor >= history.Count - 1) return;
        RememberCurrentPage(); cursor++; ShowCurrent();
    }
    void ShowCurrent()
    {
        messenger.ApplyBrowserPage(history[cursor]);
        RefreshControls();
        RestoreScroll();
    }
    public void OnWindowOpened()
    {
        EnsureHistory();
        RefreshControls();
        RestoreScroll();
    }
    void RestoreScroll()
    {
        var page = history[cursor];
        var scroll = page.siteIndex >= 0 ? messenger.siteArticleScroll : messenger.searchResults.GetComponent<UnityEngine.UI.ScrollRect>();
        if (!scroll || !scroll.isActiveAndEnabled) return;
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        Canvas.ForceUpdateCanvases();
        scroll.StopMovement();
        scroll.verticalNormalizedPosition = scroll.content.rect.height <= scroll.viewport.rect.height ? 1f : page.scroll;
    }
    void RefreshControls()
    {
        searchChrome.SetActive(history[cursor].siteIndex < 0);
        home.SetAvailable(HomeIndex() >= 0);
        back.SetAvailable(cursor > 0);
        forward.SetAvailable(cursor < history.Count - 1);
    }
}
