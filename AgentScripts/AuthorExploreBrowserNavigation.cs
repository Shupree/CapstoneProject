using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Targeted scene authoring only; never runs gameplay or rebuilds existing UI.
public static class AuthorExploreBrowserNavigation
{
    static Sprite[] Sprites(string file) => AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/Icon/" + file + ".png")
        .OfType<Sprite>().OrderBy(s => s.name, StringComparer.Ordinal).ToArray();
    static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Add browser navigation");
        var r = (RectTransform)go.transform; r.SetParent(parent, false);
        return r;
    }
    static void Bounds(RectTransform r, Vector2 min, Vector2 max)
    {
        Undo.RecordObject(r, "Resize browser article area");
        r.anchorMin = min; r.anchorMax = max; r.offsetMin = r.offsetMax = Vector2.zero;
    }
    static ExploreBrowserNavigation.NavigationButton Button(Transform parent, string name, float x, Sprite[] black, Sprite[] gray)
    {
        if (black.Length == 0 || black.Length != gray.Length) throw new InvalidOperationException("Missing icons: " + name);
        var r = Rect(name, parent);
        r.anchorMin = r.anchorMax = new Vector2(0, 1); r.pivot = new Vector2(.5f, .5f);
        r.anchoredPosition = new Vector2(x, -225); r.sizeDelta = new Vector2(70, 90);
        var hit = r.gameObject.AddComponent<UnityEngine.UI.Image>(); hit.color = Color.clear;
        var button = r.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = hit;
        button.transition = UnityEngine.UI.Selectable.Transition.None;
        button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
        button.interactable = false;
        var icons = new UnityEngine.UI.Image[black.Length];
        // Home is authored as two overlapping sprite slices. Place each at its original texture offset.
        float left = black.Min(s => s.rect.xMin), right = black.Max(s => s.rect.xMax);
        for (int i = 0; i < icons.Length; i++)
        {
            var icon = Rect("Icon" + i, r);
            icon.anchorMin = icon.anchorMax = icon.pivot = new Vector2(.5f, .5f);
            icon.sizeDelta = black[i].rect.size * 2f;
            icon.anchoredPosition = new Vector2((black[i].rect.center.x - (left + right) * .5f) * 2f, 0);
            icons[i] = icon.gameObject.AddComponent<UnityEngine.UI.Image>();
            icons[i].sprite = gray[i]; icons[i].preserveAspect = true; icons[i].raycastTarget = false;
        }
        return new ExploreBrowserNavigation.NavigationButton { button = button, icons = icons, enabledSprites = black, disabledSprites = gray };
    }
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity")
            throw new InvalidOperationException("Explore_Proto edit mode required.");
        var m = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)).Single();
        if (m.browserNavigation || m.internetWindow.transform.Find("BrowserNavigation")) throw new InvalidOperationException("Navigation already exists; inspect instead of rebuilding.");
        var names = new[] { "SearchFieldOutline", "KeywordFieldHitArea", "SearchKeywordChip", "ClearKeyword", "Icon_Find", "SearchFieldPlaceholder", "SearchHint" };
        var items = names.Select(name => m.internetWindow.transform.Find(name)).ToArray();
        if (items.Any(t => !t)) throw new InvalidOperationException("Search UI differs from inspected hierarchy.");
        var homeBlack = Sprites("GoHome_B"); var homeGray = Sprites("GoHome_G");
        var backBlack = Sprites("GoBack_B"); var backGray = Sprites("GoBack_G");
        var forwardBlack = Sprites("GoFront_B"); var forwardGray = Sprites("GoFront_G");
        if (homeBlack.Length != 2 || homeGray.Length != 2 || backBlack.Length != 1 || backGray.Length != 1 || forwardBlack.Length != 1 || forwardGray.Length != 1)
            throw new InvalidOperationException("Unexpected icon slice count.");
        string articlesBefore = string.Join("\n", m.siteEntries.Select(s => s.article));
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Internet history and larger article view");
        try
        {
            Undo.RecordObject(m, "Connect browser history");
            var navigation = Undo.AddComponent<ExploreBrowserNavigation>(m.gameObject);
            navigation.messenger = m; m.browserNavigation = navigation;
            var chrome = Rect("SearchChrome", m.internetWindow.transform); Bounds(chrome, Vector2.zero, Vector2.one);
            foreach (var item in items)
            {
                var r = (RectTransform)item;
                Vector2 min = r.anchorMin, max = r.anchorMax, pivot = r.pivot, size = r.sizeDelta, pos = r.anchoredPosition;
                Undo.SetTransformParent(r, chrome, "Group search-only UI");
                Undo.RecordObject(r, "Preserve search layout");
                r.anchorMin = min; r.anchorMax = max; r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = pos;
            }
            navigation.searchChrome = chrome.gameObject;
            var bar = Rect("BrowserNavigation", m.internetWindow.transform); Bounds(bar, Vector2.zero, Vector2.one);
            navigation.home = Button(bar, "Home", 140, homeBlack, homeGray);
            navigation.back = Button(bar, "Back", 220, backBlack, backGray);
            navigation.forward = Button(bar, "Forward", 300, forwardBlack, forwardGray);
            Bounds((RectTransform)m.siteDetailPanel.transform, new Vector2(.055f, .055f), new Vector2(.945f, .81f));
            var category = m.siteCategoryText.rectTransform; Undo.RecordObject(category, "Move article category");
            category.anchoredPosition = new Vector2(0, -15); category.sizeDelta = new Vector2(-200, 55);
            var title = m.siteTitleText.rectTransform; Undo.RecordObject(title, "Move article title");
            title.anchoredPosition = new Vector2(0, -70); title.sizeDelta = new Vector2(-200, 75);
            var article = (RectTransform)m.siteArticleScroll.transform; Undo.RecordObject(article, "Expand article viewport");
            article.anchorMin = Vector2.zero; article.anchorMax = Vector2.one;
            article.offsetMin = new Vector2(100, 20); article.offsetMax = new Vector2(-100, -155);
            var results = (RectTransform)m.searchResults.transform; Undo.RecordObject(results, "Extend search results bottom");
            results.anchorMin = new Vector2(results.anchorMin.x, .055f);
            // Preserve the hint reference used by investigation code, but remove it from presentation.
            Undo.RecordObject(m.investigation.articleHint.gameObject, "Hide browser objective");
            m.investigation.articleHint.gameObject.SetActive(false);
            if (m.siteBackButton) Undo.DestroyObjectImmediate(m.siteBackButton.gameObject);
            m.siteBackButton = null;
            if (articlesBefore != string.Join("\n", m.siteEntries.Select(s => s.article))) throw new InvalidOperationException("Article content changed.");
            EditorUtility.SetDirty(m); EditorUtility.SetDirty(navigation);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved Explore_Proto: Home/Back/Forward icons, grouped search UI, larger article viewport, old return button removed, footer hint hidden. Article data unchanged.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }
}
