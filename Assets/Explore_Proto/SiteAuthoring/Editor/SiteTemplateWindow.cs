using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public sealed class SiteTemplateWindow : EditorWindow
{
    [SerializeField] GameObject template;
    [SerializeField] string templateName = "Lounge X 게시글";
    string slotName = "제목";
    SiteSlotKind kind;
    Sprite decoration;
    Vector2 scroll;

    [MenuItem("Tools/사이트 제작/템플릿 제작 에디터")]
    static void Open() => GetWindow<SiteTemplateWindow>("템플릿 제작");

    void OnEnable() => SiteAssets.EnsureFolders();

    void OnGUI()
    {
        templateName = EditorGUILayout.TextField("새 템플릿 이름", templateName);
        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(templateName) ||
            templateName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            if (GUILayout.Button("새 템플릿 만들기"))
            {
                var path = AssetDatabase.GenerateUniqueAssetPath(SiteAssets.Templates + "/" + templateName.Trim() + ".prefab");
                var root = SiteAssets.Node(templateName, null);
                try
                {
                    root.sizeDelta = new Vector2(2200, 850);
                    root.gameObject.AddComponent<Image>().color = Color.white;
                    template = PrefabUtility.SaveAsPrefabAsset(root.gameObject, path);
                }
                finally { DestroyImmediate(root.gameObject); }
                AssetDatabase.OpenAsset(template);
            }
        template = (GameObject)EditorGUILayout.ObjectField("기존 템플릿", template, typeof(GameObject), false);
        if (template && AssetDatabase.GetAssetPath(template).StartsWith(SiteAssets.Templates + "/") &&
            GUILayout.Button("프리팹 편집 열기")) AssetDatabase.OpenAsset(template);

        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null || !stage.assetPath.StartsWith(SiteAssets.Templates + "/")) return;
        var rootObject = stage.prefabContentsRoot;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(rootObject.name, EditorStyles.boldLabel);
        slotName = EditorGUILayout.TextField("요소 이름", slotName);
        kind = (SiteSlotKind)EditorGUILayout.EnumPopup("슬롯 종류", kind);
        if (GUILayout.Button("슬롯 추가"))
        {
            var rect = AddElement(rootObject, slotName, kind == SiteSlotKind.Body ? 460 : 90);
            var slot = Undo.AddComponent<SiteSlot>(rect.gameObject);
            slot.id = Guid.NewGuid().ToString("N");
            slot.kind = kind;
            if (kind == SiteSlotKind.Text) SiteAssets.Label(rect.gameObject).text = rect.name;
            if (kind == SiteSlotKind.Image) rect.gameObject.AddComponent<Image>().preserveAspect = true;
        }
        decoration = (Sprite)EditorGUILayout.ObjectField("공용 Sprite", decoration, typeof(Sprite), false);
        if (GUILayout.Button("공용 이미지 추가"))
        {
            var rect = AddElement(rootObject, decoration ? decoration.name : "배경", 120);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = decoration;
            image.preserveAspect = true;
            image.raycastTarget = false;
        }
        EditorGUILayout.HelpBox("목록에서 슬롯을 선택하고 Scene의 Rect Tool(T)로 배치하세요. 공용 요소는 Hierarchy에서도 추가할 수 있습니다. Ctrl+S로 프리팹을 저장하세요.", MessageType.Info);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var slot in rootObject.GetComponentsInChildren<SiteSlot>(true))
        {
            if (GUILayout.Button(slot.name + " · " + slot.kind)) Selection.activeGameObject = slot.gameObject;
            if (string.IsNullOrEmpty(slot.id) || Array.Exists(rootObject.GetComponentsInChildren<SiteSlot>(true),
                other => other != slot && other.id == slot.id))
                if (GUILayout.Button(slot.name + "에 새 ID 부여 (복제본 선택)"))
                {
                    Undo.RecordObject(slot, "슬롯 ID 갱신");
                    slot.id = Guid.NewGuid().ToString("N");
                    EditorUtility.SetDirty(slot);
                }
        }
        EditorGUILayout.EndScrollView();
    }

    static RectTransform AddElement(GameObject root, string name, float height)
    {
        var rect = SiteAssets.Node(string.IsNullOrWhiteSpace(name) ? "요소" : name, root.transform);
        rect.sizeDelta = new Vector2(1400, height);
        Undo.RegisterCreatedObjectUndo(rect.gameObject, "사이트 요소 추가");
        Selection.activeGameObject = rect.gameObject;
        EditorSceneManager.MarkSceneDirty(root.scene);
        return rect;
    }
}
