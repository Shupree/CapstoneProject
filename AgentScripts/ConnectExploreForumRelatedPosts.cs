using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class ConnectExploreForumRelatedPosts
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if(EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity")
            throw new InvalidOperationException("Explore_Proto edit mode required.");
        var m = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)).Single();
        var p = m.forumArticlePage;
        if(!p || !m.browserNavigation || !p.page || p.page.Find("RelatedRow0")) throw new InvalidOperationException("Unexpected forum hierarchy or links already connected.");
        if(m.siteEntries.Length < 3 || Enumerable.Range(0,3).Any(i => m.siteEntries[i].article.Contains("<link")))
            throw new InvalidOperationException("Unexpected forum source articles.");
        var names = new[]{"RelatedPost", "RelatedAuthor", "RelatedNumber", "RelatedViews"};
        for(int i=0;i<2;i++) foreach(var name in names)
            if(!p.page.Find(name+i)) throw new InvalidOperationException("Missing related post cell: " + name + i);
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Connect forum related posts");
        Undo.RecordObject(p,"Share forum layout across three articles");
        p.siteIndices = new[]{0,1,2}; p.messenger = m; p.navigation = m.browserNavigation;
        p.relatedPosts = new ExploreForumArticlePage.RelatedPost[2];
        for(int i=0;i<2;i++) {
            var go = new GameObject("RelatedRow"+i,typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button));
            Undo.RegisterCreatedObjectUndo(go,"Add related article row button"); go.layer = p.page.gameObject.layer;
            var row = (RectTransform)go.transform; row.SetParent(p.page,false);
            row.anchorMin = row.anchorMax = new Vector2(0,1); row.pivot = new Vector2(0,1);
            row.anchoredPosition = new Vector2(330, -2222-i*86); row.sizeDelta = new Vector2(1196,86);
            var hit = go.GetComponent<UnityEngine.UI.Image>(); hit.color = Color.clear; hit.raycastTarget = true;
            var button = go.GetComponent<UnityEngine.UI.Button>(); button.targetGraphic = hit;
            button.transition = UnityEngine.UI.Selectable.Transition.None;
            button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            var cells = names.Select(name => (RectTransform)p.page.Find(name+i)).ToArray();
            var title = cells[0].GetComponent<TMP_Text>();
            var author = cells[1].GetComponent<TMP_Text>();
            var number = cells[2].GetComponent<TMP_Text>();
            foreach(var cell in cells) {
                var position = cell.anchoredPosition;
                Undo.SetTransformParent(cell,row,"Group related post row");
                Undo.RecordObject(cell,"Preserve related cell position");
                cell.anchorMin = cell.anchorMax = new Vector2(0,1); cell.pivot = new Vector2(0,1);
                cell.anchoredPosition = position - row.anchoredPosition;
            }
            Undo.RecordObject(title,"Style related link");
            title.color = new Color32(112,45,155,255); title.fontStyle |= FontStyles.Underline;
            p.relatedPosts[i] = new ExploreForumArticlePage.RelatedPost {
                button = button, title = title, author = author, number = number, siteIndex = i+1
            };
            UnityEventTools.AddIntPersistentListener(button.onClick,p.OpenRelatedPost,i);
            EditorUtility.SetDirty(button);
        }
        EditorUtility.SetDirty(p);
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        return "Saved related article row buttons and forum indices 0,1,2.";
    }
}
