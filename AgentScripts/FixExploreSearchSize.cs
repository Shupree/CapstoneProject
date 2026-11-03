var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity") throw new System.InvalidOperationException("Explore_Proto edit mode required.");
var m = System.Linq.Enumerable.Single(System.Linq.Enumerable.SelectMany(scene.GetRootGameObjects(), r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)));
var chrome = (UnityEngine.RectTransform)m.internetWindow.transform.Find("SearchChrome");
if (!chrome || chrome.GetComponent<UnityEngine.UI.LayoutGroup>()) throw new System.InvalidOperationException("Unexpected search layout.");
var names = new[]{"SearchFieldOutline","KeywordFieldHitArea","SearchKeywordChip","ClearKeyword","Icon_Find","SearchHint","SearchInputField"};
foreach(var name in names) if(!chrome.Find(name)) throw new System.InvalidOperationException("Missing search element: "+name);
UnityEditor.Undo.IncrementCurrentGroup(); int group = UnityEditor.Undo.GetCurrentGroup();
UnityEditor.Undo.SetCurrentGroupName("Fix browser search controls size");
var lines = new System.Collections.Generic.List<string>();
foreach(UnityEngine.RectTransform r in chrome) {
 UnityEditor.Undo.RecordObject(r,"Keep search element dimensions fixed");
 var corners = new UnityEngine.Vector3[4]; r.GetWorldCorners(corners);
 var lower = (UnityEngine.Vector2)chrome.InverseTransformPoint(corners[0]);
 var upper = (UnityEngine.Vector2)chrome.InverseTransformPoint(corners[2]);
 r.anchorMin = r.anchorMax = new UnityEngine.Vector2(0,1);
 r.offsetMin = lower - new UnityEngine.Vector2(chrome.rect.xMin,chrome.rect.yMax);
 r.offsetMax = upper - new UnityEngine.Vector2(chrome.rect.xMin,chrome.rect.yMax);
 lines.Add(r.name+" fixed size="+r.rect.size);
}
var mask = chrome.GetComponent<UnityEngine.UI.RectMask2D>();
if(!mask) mask = UnityEditor.Undo.AddComponent<UnityEngine.UI.RectMask2D>(chrome.gameObject);
else UnityEditor.Undo.RecordObject(mask,"Clip fixed search controls inside window");
mask.padding = new UnityEngine.Vector4(80,0,80,0);
UnityEditor.EditorUtility.SetDirty(mask);
UnityEditor.Undo.CollapseUndoOperations(group);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return string.Join("\n",lines);
