using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AddMaryStorage
{
    static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.layer = parent.gameObject.layer;
        var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
        rect.anchorMin = min; rect.anchorMax = max; rect.sizeDelta = size; rect.anchoredPosition = Vector2.zero;
        return rect;
    }
    static TextMeshProUGUI Text(string name, Transform parent, TMP_FontAsset font, string text, float size)
    {
        var rect = Rect(name,parent,Vector2.zero,Vector2.one,Vector2.zero);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font; label.text = text; label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
        return label;
    }
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity")
            throw new InvalidOperationException("Battle_Proto 편집 씬에서 실행하세요.");
        var host = GameObject.Find("ExploreProtoMessenger");
        var sealing = host.GetComponent<BattleSealingController>();
        if (host.GetComponent<BattleMaryStorageSequence>()) throw new InvalidOperationException("이미 연결됨");
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprite/Cutscene/folder.png");
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GalmuriMono11 SDF 1.asset");
        if (!texture || !font) throw new InvalidOperationException("폴더 또는 폰트가 없습니다.");
        var canvas = sealing.root.transform.parent;
        var sd = sealing.sdMary.image.rectTransform;
        var group = Rect("MaryFileGroup",sd.parent,sd.anchorMin,sd.anchorMax,sd.sizeDelta);
        group.anchoredPosition = sd.anchoredPosition;
        group.SetSiblingIndex(sd.GetSiblingIndex());
        Undo.RegisterCreatedObjectUndo(group.gameObject,"메리 파일 그룹 추가");
        Undo.SetTransformParent(sd,group,"메리 파일 그룹 연결");
        sd.anchorMin = sd.anchorMax = new Vector2(.5f,.5f); sd.anchoredPosition = Vector2.zero;
        var fade = group.gameObject.AddComponent<CanvasGroup>();
        var name = Text("FileName",group,font,"mary.exe",62);
        var nameRect = name.rectTransform; nameRect.anchorMin = nameRect.anchorMax = new Vector2(.5f,0);
        nameRect.sizeDelta = new Vector2(600,90); nameRect.anchoredPosition = new Vector2(0,-60);
        name.gameObject.SetActive(false);
        var folder = Rect("MaryFolder",sealing.root.transform,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(600,600f*texture.height/texture.width));
        folder.anchoredPosition = new Vector2(0,130);
        var folderImage = folder.gameObject.AddComponent<UnityEngine.UI.RawImage>();
        folderImage.texture = texture; folderImage.raycastTarget = false;
        folder.gameObject.SetActive(false);
        var popup = Rect("MarySavedPopup",canvas,Vector2.zero,Vector2.one,Vector2.zero);
        var dimmer = popup.gameObject.AddComponent<UnityEngine.UI.Image>(); dimmer.color = new Color(0,0,0,.45f);
        var panel = Rect("Panel",popup,Vector2.one*.5f,Vector2.one*.5f,new Vector2(1500,460));
        var panelImage = panel.gameObject.AddComponent<UnityEngine.UI.Image>(); panelImage.color = new Color(.12f,.14f,.14f,1);
        var message = Text("Message",panel,font,"mary.exe(이)가 저장되었다.",62);
        message.rectTransform.anchorMin = new Vector2(.05f,.42f); message.rectTransform.anchorMax = new Vector2(.95f,.90f);
        var confirm = Rect("Confirm",panel,new Vector2(.5f,.22f),new Vector2(.5f,.22f),new Vector2(320,110));
        var buttonImage = confirm.gameObject.AddComponent<UnityEngine.UI.Image>(); buttonImage.color = new Color(.82f,.88f,.85f);
        var button = confirm.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = buttonImage;
        var label = Text("Label",confirm,font,"확인",55); label.color = new Color(.06f,.08f,.08f);
        popup.gameObject.SetActive(false);
        var coverRect = Rect("MaryReturnPixelTransition",canvas,Vector2.zero,Vector2.one,Vector2.zero);
        var coverImage = coverRect.gameObject.AddComponent<UnityEngine.UI.RawImage>(); coverImage.texture = Texture2D.whiteTexture;
        coverImage.color = Color.black; coverImage.raycastTarget = true;
        var cover = coverRect.gameObject.AddComponent<SealingVisual>(); cover.image = coverImage; cover.template = sealing.sdMary.template;
        cover.pixelGrid = new Vector2(160,90); cover.gameObject.SetActive(false);
        var sequence = Undo.AddComponent<BattleMaryStorageSequence>(host);
        sequence.sealing = sealing; sequence.conversation = host.GetComponent<BattleProtoConversation>();
        sequence.maryFileGroup = group; sequence.maryFileFade = fade; sequence.folder = folder; sequence.fileName = name;
        sequence.savedPopup = popup.gameObject; sequence.confirmButton = button; sequence.transitionCover = cover;
        Undo.RecordObject(sealing,"메리 저장 연출 연결"); sealing.storageSequence = sequence;
        EditorUtility.SetDirty(sealing); EditorUtility.SetDirty(sequence);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("씬 저장 실패");
        return "메리 파일 그룹·폴더·저장 팝업·픽셀 전환 UI 연결 완료";
    }
}
