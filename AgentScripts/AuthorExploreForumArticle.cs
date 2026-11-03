using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// One-time targeted authoring of the new forum document; does not enter gameplay.
public static class AuthorExploreForumArticle
{
    static TMP_FontAsset font;
    static readonly Color Purple = new Color32(169, 93, 207, 255);
    static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Add forum document element");
        go.layer = parent.gameObject.layer;
        var r = (RectTransform)go.transform; r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, -y); r.sizeDelta = new Vector2(width, height);
        return r;
    }
    static UnityEngine.UI.Image Image(string name, Transform parent, float x, float y, float w, float h, Color color, Sprite sprite = null)
    {
        var r = Rect(name, parent, x, y, w, h);
        var image = Undo.AddComponent<UnityEngine.UI.Image>(r.gameObject);
        image.color = color; image.sprite = sprite; image.raycastTarget = false;
        return image;
    }
    static TMP_Text Text(string name, Transform parent, float x, float y, float w, float h, string value, float size = 30,
        TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft, bool bold = false, Color? color = null)
    {
        var r = Rect(name, parent, x, y, w, h);
        var t = Undo.AddComponent<TextMeshProUGUI>(r.gameObject);
        t.font = font; t.fontSize = size; t.enableAutoSizing = false;
        t.color = color ?? new Color32(24, 24, 24, 255); t.text = value; t.alignment = alignment;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.textWrappingMode = TextWrappingModes.Normal; t.overflowMode = TextOverflowModes.Masking;
        t.raycastTarget = false; return t;
    }
    static void Line(Transform parent, float x, float y, float w, bool strong = false)
        => Image("Divider", parent, x, y, w, strong ? 4 : 2, strong ? new Color32(65,65,65,255) : new Color32(185,185,185,255));
    static Sprite ImportSprite(string name)
    {
        string path = "Assets/Sprite/UI/Internet/" + name + ".png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static string Author(string source) => source.Split('\n')[0].Replace("작성자 : ", "");
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity")
            throw new InvalidOperationException("Explore_Proto edit mode required.");
        var m = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)).Single();
        var scroll = m.siteArticleScroll;
        if(m.forumArticlePage || scroll.viewport.Find("ForumContent")) throw new InvalidOperationException("Forum already authored.");
        if(m.siteEntries[0].title != "블러디 메리는 어떤 칵테일일까?" || m.siteEntries[0].article.Contains("<link")) throw new InvalidOperationException("Unexpected article.");
        font = m.siteArticleText.font;
        var logo = ImportSprite("ForumLogo");
        var dummy = ImportSprite("ForumDummy");
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Add fixed-width forum article");
        Undo.RecordObject(m, "Connect forum document");
        var presenter = Undo.AddComponent<ExploreForumArticlePage>(m.gameObject);
        m.forumArticlePage = presenter;
        presenter.siteIndices = new[] { 0, 1, 2 }; presenter.scroll = scroll; presenter.defaultContent = scroll.content;
        presenter.defaultCategory = m.siteCategoryText.gameObject; presenter.defaultTitle = m.siteTitleText.gameObject;
        var scrollRect = (RectTransform)scroll.transform;
        presenter.defaultScrollOffsetMin = scrollRect.offsetMin; presenter.defaultScrollOffsetMax = scrollRect.offsetMax;
        var content = Rect("ForumContent", scroll.viewport, 0, 0, 0, 2670);
        content.anchorMax = Vector2.one;
        // Only the horizontal axis stretches; height remains a long scrollable document.
        content.anchorMin = new Vector2(0,1); content.anchorMax = new Vector2(1,1);
        presenter.forumContent = content;
        var page = Rect("ForumPage", content, 0, 0, 1600, 2670); presenter.page = page;
        Image("PageBackground", page, 0, 0, 1600, 2670, Color.white);
        var logoImage = Image("Logo", page, 58, 48, 230, 100, Color.white, logo); logoImage.preserveAspect = true;
        Text("BoardName", page, 500, 67, 800, 65, "칵테일 이야기 게시판", 44, TextAlignmentOptions.TopLeft, true);
        Image("HeaderStripe", page, 0, 195, 1600, 56, Purple);
        presenter.heading = Text("PostTitle", page, 330, 320, 1196, 62, m.siteEntries[0].title, 42, TextAlignmentOptions.TopLeft, true);
        Text("PostStats", page, 1050, 294, 470, 30, "조회 38 | 댓글 2 | 추천 5", 21, TextAlignmentOptions.TopRight);
        Line(page, 330, 390, 1196);
        presenter.author = Text("Author", page, 330, 411, 850, 40, m.siteEntries[0].article.Split('\n')[0], 27);
        Text("PostDate", page, 1215, 411, 300, 40, "20xx.xx.xx", 27, TextAlignmentOptions.TopRight);
        Line(page, 330, 464, 1196, true);
        var source = m.siteEntries[0].article.Replace("\r", "");
        var article = source.Substring(source.IndexOf('\n') + 1).Trim();
        int split = article.IndexOf("\n\n댓글\n", StringComparison.Ordinal);
        var body = article.Substring(0, split).TrimEnd();
        var replies = article.Substring(split + "\n\n댓글\n".Length).Split(new[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
        presenter.body = Text("PostBody", page, 330, 520, 1196, 350, body, 32);
        Image("PostImage", page, 330, 890, 288, 280, Color.white, dummy);
        Text("PostImageLabel", page, 346, 985, 256, 90, "이미지\n(게시글 내용)", 27);
        Image("RecommendImage", page, 800, 1196, 288, 118, Color.white, dummy);
        Text("RecommendLabel", page, 800, 1220, 144, 80, "추천\n(5)", 27, TextAlignmentOptions.Top);
        Text("DislikeLabel", page, 944, 1220, 144, 80, "비추천\n(2)", 27, TextAlignmentOptions.Top);
        Image("VoteDivider", page, 943, 1196, 2, 118, new Color32(130,130,130,255));
        Line(page, 330, 1358, 1196, true);
        presenter.commentCount = Text("CommentCount", page, 330, 1382, 800, 42, "댓글 (" + replies.Length + ")", 29);
        Line(page, 330, 1436, 1196);
        presenter.commentAuthors = new TMP_Text[replies.Length]; presenter.commentBodies = new TMP_Text[replies.Length];
        for(int i=0; i<replies.Length; i++) {
            var row = Rect("Comment" + (i+1), page, 330, 1450 + i*106, 1196, 106);
            int separator = replies[i].IndexOf(" : ", StringComparison.Ordinal);
            presenter.commentAuthors[i] = Text("Author", row, 0, 16, 220, 65, replies[i].Substring(0, separator), 25);
            presenter.commentBodies[i] = Text("Body", row, 228, 16, 730, 85, replies[i].Substring(separator+3), 25);
            Text("Date", row, 985, 16, 200, 60, "20xx.xx.xx", 24, TextAlignmentOptions.TopRight);
            Line(row, 0, 102, 1196);
        }
        Line(page, 330, 1690, 1196, true);
        Text("CommentPage", page, 330, 1720, 1196, 60, "<u>1</u>", 30, TextAlignmentOptions.Top);
        Image("BottomAd", page, 348, 1836, 1156, 205, Color.white, dummy);
        Text("BottomAdLabel", page, 348, 1905, 1156, 70, "광고 2", 43, TextAlignmentOptions.Top, true);
        Text("RelatedTitle", page, 330, 2075, 1196, 50, "관련 게시물", 35, TextAlignmentOptions.TopLeft, true);
        Line(page, 330, 2131, 1196, true);
        Image("RelatedHeader", page, 330, 2136, 1196, 86, Purple);
        Text("RelatedHeaderNumber", page, 355, 2160, 120, 45, "번호", 27, color: Color.white);
        Text("RelatedHeaderTitle", page, 505, 2160, 600, 45, "제목", 27, color: Color.white);
        Text("RelatedHeaderAuthor", page, 1190, 2160, 205, 45, "작성자", 27, color: Color.white);
        Text("RelatedHeaderViews", page, 1400, 2160, 120, 45, "조회수", 27, color: Color.white);
        for(int i=0;i<2;i++) {
            float y = 2245 + i*86;
            Text("RelatedNumber"+i, page, 355, y, 120, 60, (12+i).ToString("000"), 27);
            Text("RelatedPost"+i, page, 505, y, 660, 70, m.siteEntries[i+1].title, 27);
            Text("RelatedAuthor"+i, page, 1190, y, 205, 70, Author(m.siteEntries[i+1].article), 25);
            Text("RelatedViews"+i, page, 1400, y, 120, 60, "38", 27, TextAlignmentOptions.Top);
            Line(page, 330, y+65, 1196);
        }
        Text("RelatedPage", page, 330, 2430, 1196, 55, "<u>1</u>", 30, TextAlignmentOptions.Top);
        Image("Footer", page, 0, 2500, 1600, 170, Purple);
        Text("FooterLinks", page, 330, 2536, 1196, 50, "회사 소개 | 이용약관 | 개인정보처리방침", 23, TextAlignmentOptions.Top, color: Color.white);
        Text("Copyright", page, 330, 2590, 1196, 42, "Copyright 2003 - 2023 Boardlive. All rights reserved.", 20, TextAlignmentOptions.Top, color: Color.white);
        // The sidebar is authored once; runtime only changes its vertical position for sticky scrolling.
        var sidebar = Rect("StickySidebar", page, 42, 288, 239, 837); presenter.sidebar = sidebar;
        Image("AdImage", sidebar, 0, 0, 239, 837, Color.white, dummy);
        Text("AdLabel", sidebar, 15, 235, 215, 430, "광고\n(슬라이드)\n\n클릭 시\n미니게임 등\n사이드 요소", 38, TextAlignmentOptions.TopLeft, true);
        presenter.fullWidthBackgrounds = new[] { (RectTransform)page.Find("HeaderStripe"), (RectTransform)page.Find("Footer") };
        content.gameObject.SetActive(false);
        EditorUtility.SetDirty(presenter); EditorUtility.SetDirty(m);
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        return "Forum document saved for site 0; existing scene edits retained.";
    }
}
