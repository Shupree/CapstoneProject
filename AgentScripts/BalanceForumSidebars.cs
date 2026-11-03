var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity") throw new System.InvalidOperationException("Explore_Proto edit mode required.");
var m = System.Linq.Enumerable.Single(System.Linq.Enumerable.SelectMany(scene.GetRootGameObjects(), r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)));
var p = m.forumArticlePage;
var background = (UnityEngine.RectTransform)p.page.Find("PageBackground");
var body = p.body.rectTransform;
if(p.page.Find("RightSidebarSpacer") || !background || body.pivot.x != 0 || p.sidebar.pivot.x != 0 || p.page.pivot.x != 0)
 throw new System.InvalidOperationException("Unexpected forum layout or spacer already exists.");
float bodyLeft = body.anchoredPosition.x;
float width = bodyLeft*2+body.rect.width;
float sidebarLeft = p.sidebar.anchoredPosition.x;
float spacerLeft = width-sidebarLeft-p.sidebar.rect.width;
UnityEditor.Undo.IncrementCurrentGroup(); int group = UnityEditor.Undo.GetCurrentGroup();
UnityEditor.Undo.SetCurrentGroupName("Balance forum article with transparent right sidebar");
UnityEditor.Undo.RecordObject(p.page,"Center article between equal sidebar spaces");
p.page.SetSizeWithCurrentAnchors(UnityEngine.RectTransform.Axis.Horizontal,width);
UnityEditor.Undo.RecordObject(background,"Extend white page background");
background.SetSizeWithCurrentAnchors(UnityEngine.RectTransform.Axis.Horizontal,width);
var go = new UnityEngine.GameObject("RightSidebarSpacer",typeof(UnityEngine.RectTransform),typeof(UnityEngine.UI.Image));
UnityEditor.Undo.RegisterCreatedObjectUndo(go,"Add transparent right sidebar section");
go.layer = p.sidebar.gameObject.layer;
var spacer = (UnityEngine.RectTransform)go.transform;
spacer.SetParent(p.page,false); spacer.anchorMin = spacer.anchorMax = new UnityEngine.Vector2(0,1); spacer.pivot = new UnityEngine.Vector2(0,1);
spacer.sizeDelta = p.sidebar.rect.size;
spacer.anchoredPosition = new UnityEngine.Vector2(spacerLeft,p.sidebar.anchoredPosition.y);
var image = go.GetComponent<UnityEngine.UI.Image>(); image.color = UnityEngine.Color.clear; image.raycastTarget = false;
float viewportWidth = ((UnityEngine.RectTransform)m.siteDetailPanel.transform).rect.width;
float margin = UnityEngine.Mathf.Max(0,(viewportWidth-width)*.5f);
var position = p.page.anchoredPosition; position.x = margin; p.page.anchoredPosition = position;
foreach(var bar in p.fullWidthBackgrounds) {
 UnityEditor.Undo.RecordObject(bar,"Preserve full viewport background");
 bar.SetSizeWithCurrentAnchors(UnityEngine.RectTransform.Axis.Horizontal,viewportWidth);
 var barPosition = bar.anchoredPosition; barPosition.x = -margin; bar.anchoredPosition = barPosition;
}
UnityEditor.Undo.CollapseUndoOperations(group);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "Saved transparent right section "+spacer.rect.size+"; page width="+width+", body width="+body.rect.width+", left/right article space="+bodyLeft+". Existing scene edits retained.";
