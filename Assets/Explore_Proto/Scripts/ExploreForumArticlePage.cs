using System;
using TMPro;
using UnityEngine;

/// <summary>Shows a scene-authored, fixed-width forum document in the existing browser scroll view.</summary>
public sealed class ExploreForumArticlePage : MonoBehaviour
{
    [Serializable]
    public sealed class RelatedPost
    {
        public UnityEngine.UI.Button button;
        public TMP_Text title, author, number;
        public int siteIndex = -1;
    }

    public int[] siteIndices = { 0, 1, 2 };
    public ExploreProtoMessenger messenger;
    public ExploreBrowserNavigation navigation;
    public RelatedPost[] relatedPosts = Array.Empty<RelatedPost>();
    public UnityEngine.UI.ScrollRect scroll;
    public RectTransform defaultContent, forumContent, page, sidebar;
    [Tooltip("본문 크기와 별도로 게시물 표시 영역의 가로 폭을 채우는 배경입니다.")]
    public RectTransform[] fullWidthBackgrounds = Array.Empty<RectTransform>();
    public GameObject defaultCategory, defaultTitle;
    public Vector2 defaultScrollOffsetMin, defaultScrollOffsetMax;
    public TMP_Text heading, author, body, commentCount;
    public TMP_Text[] commentAuthors, commentBodies;
    [Min(0)] public float stickyTopPadding = 24;
    [Min(0)] public float sidebarStart = 288;
    [Min(0)] public float footerClearance = 170;
    private bool showingForum;

    public void ShowSite(int index, ExploreProtoMessenger.SiteEntry site)
    {
        showingForum = Array.IndexOf(siteIndices, index) >= 0;
        scroll.StopMovement();
        defaultCategory.SetActive(!showingForum);
        defaultTitle.SetActive(!showingForum);
        defaultContent.gameObject.SetActive(!showingForum);
        forumContent.gameObject.SetActive(showingForum);
        scroll.content = showingForum ? forumContent : defaultContent;
        scroll.horizontal = false;
        scroll.GetComponent<RectTransform>().offsetMin = showingForum ? Vector2.zero : defaultScrollOffsetMin;
        scroll.GetComponent<RectTransform>().offsetMax = showingForum ? Vector2.zero : defaultScrollOffsetMax;
        if (!showingForum) return;

        heading.text = site.title;
        string source = (site.article ?? string.Empty).Replace("\r", string.Empty);
        int firstLine = source.IndexOf('\n');
        author.text = firstLine < 0 ? string.Empty : source.Substring(0, firstLine);
        string article = firstLine < 0 ? source : source.Substring(firstLine + 1).Trim();
        int comments = article.IndexOf("\n\n댓글\n", StringComparison.Ordinal);
        body.text = comments < 0 ? article : article.Substring(0, comments).TrimEnd();
        var replies = comments < 0 ? Array.Empty<string>() : article.Substring(comments + "\n\n댓글\n".Length)
            .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        commentCount.text = "댓글 (" + replies.Length + ")";
        for (int i = 0; i < commentBodies.Length; i++)
        {
            bool visible = i < replies.Length;
            commentBodies[i].transform.parent.gameObject.SetActive(visible);
            if (!visible) continue;
            int separator = replies[i].IndexOf(" : ", StringComparison.Ordinal);
            commentAuthors[i].text = separator < 0 ? string.Empty : replies[i].Substring(0, separator);
            commentBodies[i].text = separator < 0 ? replies[i] : replies[i].Substring(separator + 3);
        }
        UpdateRelatedPosts(index);
        UpdateDocumentPosition();
    }

    private void UpdateRelatedPosts(int currentIndex)
    {
        int row = 0;
        foreach (int target in siteIndices)
        {
            if (target == currentIndex || !messenger || target < 0 || target >= messenger.siteEntries.Length) continue;
            if (row >= relatedPosts.Length) break;
            var link = relatedPosts[row++];
            var site = messenger.siteEntries[target];
            link.siteIndex = target;
            link.title.text = site.title;
            string source = site.article ?? string.Empty;
            int lineEnd = source.IndexOf('\n');
            link.author.text = (lineEnd < 0 ? source : source.Substring(0, lineEnd))
                .Replace("작성자 : ", string.Empty).Trim();
            link.number.text = (11 + target).ToString("000");
            link.button.gameObject.SetActive(true);
            link.button.interactable = navigation;
        }
        for (; row < relatedPosts.Length; row++)
        {
            relatedPosts[row].siteIndex = -1;
            relatedPosts[row].button.gameObject.SetActive(false);
        }
    }

    // Called by the scene's persistent Button events, preserving the browser's history and scroll restoration.
    public void OpenRelatedPost(int row)
    {
        if (!showingForum || !forumContent.gameObject.activeInHierarchy || !navigation ||
            row < 0 || row >= relatedPosts.Length) return;
        int target = relatedPosts[row].siteIndex;
        if (Array.IndexOf(siteIndices, target) >= 0) navigation.OpenSite(target);
    }

    private void LateUpdate()
    {
        if (showingForum && forumContent.gameObject.activeInHierarchy) UpdateDocumentPosition();
    }

    private void UpdateDocumentPosition()
    {
        // Wide windows add equal outer margins; narrow ones crop the fixed document from its right edge.
        var position = page.anchoredPosition;
        position.x = Mathf.Max(0, (scroll.viewport.rect.width - page.rect.width) * .5f);
        page.anchoredPosition = position;

        foreach (var background in fullWidthBackgrounds)
        {
            if (!background) continue;
            if (!Mathf.Approximately(background.rect.width, scroll.viewport.rect.width))
                background.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scroll.viewport.rect.width);
            var backgroundPosition = background.anchoredPosition;
            backgroundPosition.x = -position.x;
            background.anchoredPosition = backgroundPosition;
        }

        // Equivalent to a sticky sidebar: scroll normally until it meets the viewport, then follow it.
        var top = scroll.viewport.TransformPoint(new Vector3(0, scroll.viewport.rect.yMax, 0));
        float viewportTop = page.InverseTransformPoint(top).y;
        float lastTop = -page.rect.height + footerClearance + sidebar.rect.height;
        position = sidebar.anchoredPosition;
        position.y = Mathf.Max(lastTop, Mathf.Min(-sidebarStart, viewportTop - stickyTopPadding));
        sidebar.anchoredPosition = position;
    }
}
