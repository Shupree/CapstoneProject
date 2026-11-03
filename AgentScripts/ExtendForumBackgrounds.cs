var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity") throw new System.InvalidOperationException("Explore_Proto edit mode required.");
var m = System.Linq.Enumerable.Single(System.Linq.Enumerable.SelectMany(scene.GetRootGameObjects(), r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)));
var p = m.forumArticlePage;
var header = (UnityEngine.RectTransform)p.page.Find("HeaderStripe");
var footer = (UnityEngine.RectTransform)p.page.Find("Footer");
if(!header || !footer || header.pivot.x != 0 || footer.pivot.x != 0) throw new System.InvalidOperationException("Unexpected forum background layout.");
UnityEditor.Undo.IncrementCurrentGroup(); int group = UnityEditor.Undo.GetCurrentGroup();
UnityEditor.Undo.SetCurrentGroupName("Extend forum header and footer across viewport");
UnityEditor.Undo.RecordObject(p,"Connect full width backgrounds");
p.fullWidthBackgrounds = new[]{header, footer};
float width = ((UnityEngine.RectTransform)m.siteDetailPanel.transform).rect.width;
float margin = UnityEngine.Mathf.Max(0,(width-p.page.rect.width)*.5f);
UnityEditor.Undo.RecordObject(p.page,"Center fixed forum document");
var pagePosition = p.page.anchoredPosition; pagePosition.x = margin; p.page.anchoredPosition = pagePosition;
foreach(var r in p.fullWidthBackgrounds) {
 UnityEditor.Undo.RecordObject(r,"Extend purple background");
 r.SetSizeWithCurrentAnchors(UnityEngine.RectTransform.Axis.Horizontal,width);
 var position = r.anchoredPosition; position.x = -margin; r.anchoredPosition = position;
}
UnityEditor.EditorUtility.SetDirty(p);
UnityEditor.Undo.CollapseUndoOperations(group);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "Saved full-width HeaderStripe and Footer backgrounds; preserved heights and vertical positions.";
