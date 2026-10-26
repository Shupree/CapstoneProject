using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AddSDMaryEnding
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity")
            throw new InvalidOperationException("Battle_Proto 편집 씬에서 실행하세요.");
        var c = GameObject.Find("ExploreProtoMessenger").GetComponent<BattleSealingController>();
        var desktop = c.calmMary.transform.parent;
        if (desktop.Find("SDMary")) throw new InvalidOperationException("SDMary가 이미 존재합니다.");
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprite/Cutscene/SDMary.png");
        var left = desktop.Find("Ropes/LeftRope") as RectTransform;
        var right = desktop.Find("Ropes/RightRope") as RectTransform;
        if (!texture || !left || !right) throw new InvalidOperationException("이미지 또는 로프 참조 없음");
        var go = new GameObject("SDMary", typeof(RectTransform), typeof(UnityEngine.UI.RawImage), typeof(SealingVisual));
        go.layer = desktop.gameObject.layer;
        var rect = (RectTransform)go.transform;
        rect.SetParent(desktop, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(460f * texture.width / texture.height, 460f);
        rect.SetSiblingIndex(c.calmMary.transform.GetSiblingIndex());
        var raw = go.GetComponent<UnityEngine.UI.RawImage>();
        raw.texture = texture; raw.raycastTarget = false;
        var visual = go.GetComponent<SealingVisual>();
        visual.image = raw; visual.template = c.calmMary.template;
        Undo.RegisterCreatedObjectUndo(go, "SDMary 추가");
        Undo.RecordObject(c, "SDMary 연출 연결");
        c.sdMary = visual; c.leftRope = left; c.rightRope = right;
        go.SetActive(false);
        EditorUtility.SetDirty(c);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("씬 저장 실패");
        return "SDMary 및 양쪽 로프 퇴장 참조 저장 완료";
    }
}
