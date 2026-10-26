using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>기존 씬을 재생성하지 않고 봉인 연출 UI만 추가하는 편집용 도구.</summary>
public static class AuthorBattleSealing
{
    const string Art = "Assets/Sprite/Cutscene/";
    static Material material;
    static Texture2D Texture(string name)
    {
        var value = AssetDatabase.LoadAssetAtPath<Texture2D>(Art + name + ".png");
        if (!value) throw new InvalidOperationException("이미지 없음: " + name);
        return value;
    }
    static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        var r = (RectTransform)go.transform;
        r.SetParent(parent, false);
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;
        return r;
    }
    static SealingVisual Visual(string name, Transform parent, Texture2D texture, Vector2 min, Vector2 max)
    {
        var r = Rect(name, parent, min, max);
        var raw = r.gameObject.AddComponent<UnityEngine.UI.RawImage>();
        raw.texture = texture; raw.raycastTarget = false;
        var v = r.gameObject.AddComponent<SealingVisual>();
        v.image = raw; v.template = material;
        v.pixelGrid = new Vector2(Mathf.Max(1, texture.width / 6), Mathf.Max(1, texture.height / 6));
        return v;
    }
    public static string Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("편집 모드에서만 실행하세요.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Battle_Proto.unity") throw new InvalidOperationException("Battle_Proto 씬이 아닙니다.");
        var canvas = GameObject.Find("ExploreProtoCanvas");
        var host = GameObject.Find("ExploreProtoMessenger");
        if (!canvas || !host || canvas.transform.Find("BattleSealingRoot") || host.GetComponent<BattleSealingController>())
            throw new InvalidOperationException("기존 Canvas/호스트를 찾지 못했거나 봉인 UI가 이미 있습니다.");
        var phase = host.GetComponent<BloodyMaryPhaseTwoController>();
        var retry = host.GetComponent<BattleProtoRetrySequence>();
        if (!phase || !phase.window || !retry || !retry.backgroundChange.battleSprite)
            throw new InvalidOperationException("완료 연결과 배경 참조가 없습니다.");
        var textures = new Dictionary<string, Texture2D>();
        foreach (var name in new[] { "Talisman", "Emblem", "Emblem_Fin", "Key_note", "CalmMary", "rope_1", "rope_2", "miniTalisman" }) textures[name] = Texture(name);
        var maryFrames = Enumerable.Range(1,5).Select(i => Texture("mary_" + i.ToString("00"))).ToArray();
        var minaFrames = Enumerable.Range(1,4).Select(i => Texture("Mina_" + i.ToString("00"))).ToArray();
        var effects = Enumerable.Range(1,3).Select(i => Texture("effect/TalEffect_" + i.ToString("00"))).ToArray();
        var oldButtons = new[] { "Button_Internet", "Button_Email", "Button_Messenger", "Button_Memo" }.Select(n => canvas.transform.Find(n)).ToArray();
        if (oldButtons.Any(t => !t)) throw new InvalidOperationException("바탕화면 버튼이 없습니다.");
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GalmuriMono11 SDF 1.asset");
        var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Explore_Proto/SealingPixel.shader");
        if (!font || !shader) throw new InvalidOperationException("폰트/셰이더가 없습니다.");
        material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Explore_Proto/SealingPixel.mat");
        if (!material)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, "Assets/Explore_Proto/SealingPixel.mat");
        }
        var root = Rect("BattleSealingRoot", canvas.transform, Vector2.zero, Vector2.one);
        Undo.RegisterCreatedObjectUndo(root.gameObject, "봉인 연출 UI 추가");
        var backdrop = root.gameObject.AddComponent<UnityEngine.UI.Image>();
        backdrop.sprite = retry.backgroundChange.battleSprite;
        backdrop.raycastTarget = true;
        // 기존 작업 표시줄 위의 바탕화면 영역. 아래 바는 원래 배경으로 유지한다.
        var desktop = Rect("DesktopVisuals", root, new Vector2(0,.12f), Vector2.one);
        desktop.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var calm = Visual("CalmMary", desktop, textures["CalmMary"], new Vector2(.3f,0), new Vector2(.7f,.97f));
        calm.image.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>().aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
        calm.image.GetComponent<UnityEngine.UI.AspectRatioFitter>().aspectRatio = (float)textures["CalmMary"].width / textures["CalmMary"].height;
        var mary = Visual("Mary", desktop, maryFrames[0], Vector2.zero, Vector2.one);
        // 그림 비율을 유지하면서 화면 높이 전체를 채운다.
        var maryFit = mary.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
        maryFit.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
        maryFit.aspectRatio = (float)maryFrames[0].width / maryFrames[0].height;
        var ropeRoot = Rect("Ropes", desktop, Vector2.zero, Vector2.one);
        var ropePieces = new List<BattleSealingController.RopePiece>();
        var smalls = new List<BattleSealingController.RopeTalisman>();
        for (int side = 0; side < 2; side++)
        {
            float sign = side == 0 ? -1 : 1;
            var sideRoot = Rect(side == 0 ? "LeftRope" : "RightRope", ropeRoot, Vector2.zero, Vector2.one);
            const int pieces = 24;
            for (int i = 0; i < pieces; i++)
            {
                float p = (float)i / (pieces - 1);
                float x = .5f + sign * (.28f + .21f * p * p);
                float y = 1.025f - p * .96f;
                var visual = Visual("Rope_" + i.ToString("00") + (i % 2 == 0 ? "_Part1" : "_Part2"), sideRoot,
                    textures[i % 2 == 0 ? "rope_1" : "rope_2"], new Vector2(x,y), new Vector2(x,y));
                var rect = visual.image.rectTransform;
                rect.sizeDelta = i % 2 == 0 ? new Vector2(152,122) : new Vector2(60,72);
                // 원본 파츠의 대각선 축을 경로의 접선에 맞춘다.
                float angle = -sign * Mathf.Lerp(0, 45, p);
                rect.localEulerAngles = new Vector3(0,0, i % 2 == 0 ? angle - 55 : angle);
                ropePieces.Add(new BattleSealingController.RopePiece { rect = rect, startSeconds = i * .065f,
                    assembleOffset = new Vector2(sign * 50, -110) });
                if (i == 3 || i == 9 || i == 15 || i == 21)
                {
                    var small = Visual("MiniTalisman_" + i, sideRoot, textures["miniTalisman"], new Vector2(x,y), new Vector2(x,y));
                    small.image.rectTransform.sizeDelta = new Vector2(440,196);
                    // 로프 쪽 끝을 고정하고 화면 안쪽으로 이미지를 펼친다.
                    small.image.rectTransform.pivot = new Vector2(side == 0 ? 0 : 1, .5f);
                    small.image.rectTransform.localEulerAngles = new Vector3(0,0, angle + (side == 0 ? 12 : -12));
                    small.revealFromRight = side == 1;
                    smalls.Add(new BattleSealingController.RopeTalisman { visual = small, attachmentPiece = ropePieces.Count - 1 });
                }
            }
            // 부적은 모든 로프 조각 앞에 그린다.
            foreach (var small in smalls.Where(s => s.visual.transform.parent == sideRoot)) small.visual.transform.SetAsLastSibling();
        }
        var talismanRect = Rect("TalismanGroup", root, new Vector2(.14f,.55f), new Vector2(.86f,.85f));
        var group = talismanRect.gameObject.AddComponent<CanvasGroup>();
        var paper = Visual("Talisman", talismanRect, textures["Talisman"], Vector2.zero, Vector2.one);
        // 종이와 문양의 크기는 씬에서 편집하며 가운데부터 좌우로 펼친다.
        var emblem = Visual("Emblem", talismanRect, textures["Emblem"], new Vector2(.045f,.10f), new Vector2(.955f,.90f));
        var slots = new List<SealingVisual>();
        for (int i = 0; i < 40; i++)
        {
            var effect = Visual("TalEffect_" + i.ToString("00"), emblem.transform, effects[i % effects.Length], Vector2.one * .5f, Vector2.one * .5f);
            effect.image.rectTransform.sizeDelta = new Vector2(290,240);
            effect.gameObject.SetActive(false);
            slots.Add(effect);
        }
        var heroine = Rect("HeroineGroup", root, new Vector2(.19f,0), new Vector2(.81f,.74f));
        var mina = Visual("Mina", heroine, minaFrames[0], Vector2.zero, Vector2.one);
        var key = Visual("KeyNote", heroine, textures["Key_note"], new Vector2(.40f,.30f), new Vector2(.60f,.42f));
        var buttonRect = Rect("ActivateSealingButton", root, new Vector2(0,0), new Vector2(0,0));
        buttonRect.pivot = new Vector2(0,0);
        buttonRect.anchoredPosition = new Vector2(55,25);
        buttonRect.sizeDelta = new Vector2(1160,150);
        var buttonImage = buttonRect.gameObject.AddComponent<UnityEngine.UI.Image>();
        buttonImage.color = new Color(.84f,.84f,.82f);
        var button = buttonRect.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = buttonImage;
        var navigation = button.navigation; navigation.mode = UnityEngine.UI.Navigation.Mode.None; button.navigation = navigation;
        var labelRect = Rect("Label", buttonRect, Vector2.zero, Vector2.one);
        var label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font; label.fontSize = 50; label.color = new Color(.08f,.08f,.08f);
        label.alignment = TextAlignmentOptions.Center;
        label.text = "봉인 프로그램 가동"; label.raycastTarget = false;
        var c = Undo.AddComponent<BattleSealingController>(host);
        c.phaseTwo = phase; c.root = root.gameObject; c.activateButton = button;
        c.desktopButtons = oldButtons.Select(t => t.gameObject).ToArray();
        c.windowsToHide = new[] { "MessengerWindow", "VideoCallWindow", "TranscriptWindow", "MemoWindow", "InternetWindow", "KeywordPopup", "ClearedInternetWindow", "BattleBlackout", "MerryKnife" }
            .Select(n => canvas.transform.Find(n)).Where(t => t).Select(t => t.gameObject).ToArray();
        c.mary = mary; c.calmMary = calm;
        c.maryFrames = maryFrames; c.minaFrames = minaFrames; c.effectFrames = effects;
        c.mina = mina.image; c.heroineGroup = heroine;
        c.talismanGroup = group; c.talisman = talismanRect;
        c.emblem = emblem; c.finishedEmblem = textures["Emblem_Fin"];
        c.effectSlots = slots.ToArray(); c.ropePieces = ropePieces.ToArray(); c.smallTalismans = smalls.ToArray();
        calm.gameObject.SetActive(false); talismanRect.gameObject.SetActive(false); heroine.gameObject.SetActive(false);
        ropeRoot.gameObject.SetActive(true);
        foreach (var p in ropePieces) p.rect.gameObject.SetActive(false);
        foreach (var s in smalls) s.visual.gameObject.SetActive(false);
        root.gameObject.SetActive(false);
        EditorUtility.SetDirty(c);
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("씬 저장에 실패했습니다.");
        return "Battle_Proto에 봉인 UI와 완료 연결 저장: 로프 48조각, 작은 부적 8장, 효과 슬롯 40개.";
    }
}
