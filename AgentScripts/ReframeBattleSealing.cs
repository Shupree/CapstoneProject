using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ReframeBattleSealing
{
    static void Layout(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min; rect.anchorMax = max;
        rect.pivot = new Vector2(.5f,.5f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity")
            throw new InvalidOperationException("Battle_Proto 편집 씬에서만 실행하세요.");
        var controller = GameObject.Find("ExploreProtoMessenger").GetComponent<BattleSealingController>();
        var parent = controller.root.transform;
        if (parent.Find("SealingScreen")) throw new InvalidOperationException("전체화면 UI가 이미 존재합니다.");
        var key = controller.heroineGroup.Find("KeyNote");
        if (!key) throw new InvalidOperationException("KeyNote 참조가 없습니다.");
        var screen = new GameObject("SealingScreen", typeof(RectTransform), typeof(CanvasGroup), typeof(UnityEngine.UI.RectMask2D));
        screen.layer = parent.gameObject.layer;
        screen.transform.SetParent(parent, false);
        Layout((RectTransform)screen.transform, Vector2.zero, Vector2.one);
        Undo.RegisterCreatedObjectUndo(screen, "봉인 전체화면 구성");
        var background = new GameObject("SolidBackground", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        background.layer = screen.layer;
        background.transform.SetParent(screen.transform, false);
        Layout((RectTransform)background.transform, Vector2.zero, Vector2.one);
        var image = background.GetComponent<UnityEngine.UI.Image>();
        image.color = new Color32(31,34,33,255);
        image.raycastTarget = true;
        Undo.SetTransformParent(controller.talisman, screen.transform, "부적 화면에 연결");
        Layout(controller.talisman, new Vector2(.06f,.50f), new Vector2(.94f,.85f));
        Undo.SetTransformParent(controller.heroineGroup, screen.transform, "미나 화면에 연결");
        Layout(controller.heroineGroup, new Vector2(.17f,0), new Vector2(.83f,.77f));
        Layout((RectTransform)key, new Vector2(.38f,.20f), new Vector2(.62f,.33f));
        // 내부 이미지는 기존 참조를 유지하고 단색 배경 위에 모두 저장한다.
        controller.talisman.gameObject.SetActive(true);
        controller.heroineGroup.gameObject.SetActive(true);
        key.gameObject.SetActive(false);
        Undo.RecordObject(controller, "봉인 화면 연출 연결");
        controller.sealingScreen = screen.GetComponent<CanvasGroup>();
        controller.screenRevealMask = screen.GetComponent<UnityEngine.UI.RectMask2D>();
        controller.keyNote = key.gameObject;
        controller.heroineDelaySeconds = 1f;
        controller.eyeOpenHoldSeconds = .75f;
        controller.inputDelaySeconds = .5f;
        controller.horizontalRevealSeconds = .35f;
        controller.unfoldSeconds = .35f;
        controller.stripCenter = .45f;
        controller.stripHeight = .1f;
        screen.SetActive(false);
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("씬 저장 실패");
        return "전체화면 마스크·단색 배경 추가, 기존 부적/미나/키 안내/문양/효과 재연결 완료";
    }
}
