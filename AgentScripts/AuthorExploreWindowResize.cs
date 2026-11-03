var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity") throw new System.InvalidOperationException("Explore_Proto edit mode required.");
var window = System.Linq.Enumerable.Single(System.Linq.Enumerable.SelectMany(scene.GetRootGameObjects(), r => r.GetComponentsInChildren<UnityEngine.RectTransform>(true)), r => r.name == "InternetWindow");
if (window.Find("ResizeHandles")) throw new System.InvalidOperationException("Resize UI already exists.");
var image = window.GetComponent<UnityEngine.UI.Image>();
var path = UnityEditor.AssetDatabase.GetAssetPath(image.sprite);
if (path != "Assets/Sprite/ProtoType/Internet_Window.png") throw new System.InvalidOperationException("Unexpected window sprite.");
var importer = UnityEditor.AssetImporter.GetAtPath(path);
var factory = new UnityEditor.U2D.Sprites.SpriteDataProviderFactories();
factory.Init();
var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
if (provider == null) throw new System.InvalidOperationException("Sprite data provider missing.");
provider.InitSpriteEditorDataProvider();
var capability = provider.GetDataProvider<UnityEditor.U2D.Sprites.ISpriteFrameEditCapability>();
if (capability == null || !capability.GetEditCapability().HasCapability(UnityEditor.U2D.Sprites.EEditCapability.EditBorder)) throw new System.InvalidOperationException("Importer cannot edit borders.");
var rects = provider.GetSpriteRects();
if (rects.Length != 1 || rects[0].name != image.sprite.name) throw new System.InvalidOperationException("Unexpected sprite slices.");
rects[0].border = new UnityEngine.Vector4(660, 130, 460, 210);
provider.SetSpriteRects(rects);
provider.Apply();
importer.SaveAndReimport();
UnityEditor.Undo.IncrementCurrentGroup();
int group = UnityEditor.Undo.GetCurrentGroup();
UnityEditor.Undo.SetCurrentGroupName("Make exploration browser resizable");
UnityEditor.Undo.RecordObject(image, "Slice browser frame");
image.type = UnityEngine.UI.Image.Type.Sliced;
image.preserveAspect = false;
image.pixelsPerUnitMultiplier = image.sprite.rect.width / window.rect.width * window.GetComponentInParent<UnityEngine.Canvas>().referencePixelsPerUnit / image.sprite.pixelsPerUnit;
// Preserve current placement while changing anchors to fixed margins and heights.
void Reanchor(UnityEngine.RectTransform r, UnityEngine.Vector2 min, UnityEngine.Vector2 max) {
 UnityEditor.Undo.RecordObject(r, "Anchor browser content");
 var parent = (UnityEngine.RectTransform)r.parent;
 var corners = new UnityEngine.Vector3[4]; r.GetWorldCorners(corners);
 var lower = (UnityEngine.Vector2)parent.InverseTransformPoint(corners[0]);
 var upper = (UnityEngine.Vector2)parent.InverseTransformPoint(corners[2]);
 r.anchorMin = min; r.anchorMax = max;
 r.offsetMin = lower - (parent.rect.min + UnityEngine.Vector2.Scale(parent.rect.size, min));
 r.offsetMax = upper - (parent.rect.min + UnityEngine.Vector2.Scale(parent.rect.size, max));
}
Reanchor((UnityEngine.RectTransform)window.Find("InternetTitleDragArea"), new UnityEngine.Vector2(0,1), UnityEngine.Vector2.one);
Reanchor((UnityEngine.RectTransform)window.Find("InternetCloseX"), UnityEngine.Vector2.one, UnityEngine.Vector2.one);
Reanchor((UnityEngine.RectTransform)window.Find("SearchResults"), UnityEngine.Vector2.zero, UnityEngine.Vector2.one);
Reanchor((UnityEngine.RectTransform)window.Find("SiteDetailPanel"), UnityEngine.Vector2.zero, UnityEngine.Vector2.one);
var chrome = window.Find("SearchChrome");
foreach (var name in new[]{"SearchFieldOutline", "KeywordFieldHitArea", "SearchHint", "SearchInputField"})
 Reanchor((UnityEngine.RectTransform)chrome.Find(name), new UnityEngine.Vector2(0,1), UnityEngine.Vector2.one);
foreach (var name in new[]{"SearchKeywordChip", "Icon_Find"})
 Reanchor((UnityEngine.RectTransform)chrome.Find(name), new UnityEngine.Vector2(0,1), new UnityEngine.Vector2(0,1));
Reanchor((UnityEngine.RectTransform)chrome.Find("ClearKeyword"), UnityEngine.Vector2.one, UnityEngine.Vector2.one);
var handles = new UnityEngine.GameObject("ResizeHandles", typeof(UnityEngine.RectTransform));
UnityEditor.Undo.RegisterCreatedObjectUndo(handles, "Add resize handles");
handles.layer = window.gameObject.layer;
var root = (UnityEngine.RectTransform)handles.transform;
root.SetParent(window, false); root.anchorMin = UnityEngine.Vector2.zero; root.anchorMax = UnityEngine.Vector2.one; root.sizeDelta = UnityEngine.Vector2.zero;
for(int y=-1;y<=1;y++) for(int x=-1;x<=1;x++) {
 if(x==0 && y==0) continue;
 var name = (y < 0 ? "Bottom" : y > 0 ? "Top" : "") + (x < 0 ? "Left" : x > 0 ? "Right" : "");
 var go = new UnityEngine.GameObject(name, typeof(UnityEngine.RectTransform), typeof(UnityEngine.UI.Image), typeof(ExploreWindowResizeHandle));
 UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Add resize hit area"); go.layer = window.gameObject.layer;
 var r = (UnityEngine.RectTransform)go.transform; r.SetParent(root,false);
 r.anchorMin = new UnityEngine.Vector2(x < 0 ? 0 : x > 0 ? 1 : 0, y < 0 ? 0 : y > 0 ? 1 : 0);
 r.anchorMax = new UnityEngine.Vector2(x < 0 ? 0 : 1, y < 0 ? 0 : 1);
 r.pivot = new UnityEngine.Vector2(x < 0 ? 0 : x > 0 ? 1 : .5f, y < 0 ? 0 : y > 0 ? 1 : .5f);
 if(x != 0 && y != 0) { r.sizeDelta = new UnityEngine.Vector2(36,36); r.anchoredPosition = UnityEngine.Vector2.zero; }
 else if(x == 0) { r.sizeDelta = new UnityEngine.Vector2(-72,14); r.anchoredPosition = UnityEngine.Vector2.zero; }
 else { r.sizeDelta = new UnityEngine.Vector2(14,-72); r.anchoredPosition = UnityEngine.Vector2.zero; }
 var hit = go.GetComponent<UnityEngine.UI.Image>(); hit.color = UnityEngine.Color.clear; hit.raycastTarget = true;
 var resize = go.GetComponent<ExploreWindowResizeHandle>(); resize.window = window; resize.direction = new UnityEngine.Vector2(x,y); resize.minimumSize = new UnityEngine.Vector2(1700,1000);
}
UnityEditor.EditorUtility.SetDirty(image);
UnityEditor.Undo.CollapseUndoOperations(group);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "Saved 9-sliced browser frame and 8 resize handles; no game testing performed.";
