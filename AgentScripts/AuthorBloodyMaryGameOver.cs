using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time scene authoring only. Never calls a gameplay, retry, preview or test method.
public static class AuthorBloodyMaryGameOver
{
    static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Author battle game over");
        if (parent) { go.layer = parent.gameObject.layer; go.transform.SetParent(parent, false); }
        return (RectTransform)go.transform;
    }

    static void Fill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    static UnityEngine.UI.Image Panel(string name, Transform parent, Color color, bool blocks = false)
    {
        var rect = Rect(name, parent);
        Fill(rect);
        var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = color;
        image.raycastTarget = blocks;
        return image;
    }

    static TextMeshProUGUI Text(string name, Transform parent, TMP_FontAsset font, string value, float size)
    {
        var rect = Rect(name, parent);
        Fill(rect);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.text = value;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.enableAutoSizing = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    static UnityEngine.UI.Button Button(string name, Transform parent, TMP_FontAsset font, string value, float y)
    {
        var rect = Rect(name, parent);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, y);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(600f, 138f);
        var hit = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        hit.color = Color.clear;
        var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = Text("Label", rect, font, value, 92f);
        button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, .8f, .8f);
        colors.pressedColor = new Color(.8f, .8f, .8f);
        button.colors = colors;
        button.interactable = false;
        return button;
    }

    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || Application.dataPath != "D:/CWS/Zol_Project/Assets" ||
            scene.path != "Assets/Scenes/Battle_Proto.unity") throw new InvalidOperationException("Target Battle_Proto edit mode required.");
        var one = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BloodyMaryPhaseController>(true)).Single();
        var two = one.phaseTwo;
        var main = one.GetComponent<ExploreProtoMessenger>().internetWindow.GetComponentInParent<Canvas>();
        var handTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprite/ProtoType/GameOver_hand.png");
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Explore_Proto/Fonts/ExploreProtoKorean.asset");
        if (!two || !main || !handTexture || !font || one.GetComponent<BloodyMaryGameOverController>() ||
            scene.GetRootGameObjects().Any(r => r.name == "BattleGameOverCanvas"))
            throw new InvalidOperationException("Missing asset or game over already authored.");
        string dialogue = JsonUtility.ToJson(one.GetComponent<BattleProtoConversation>());
        float[] volumes = one.GetComponents<AudioSource>().Select(x => x.volume).ToArray();
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Author battle game over");
        try
        {
            var over = Undo.AddComponent<BloodyMaryGameOverController>(one.gameObject);
            Undo.RecordObject(one, "Connect phase one game over");
            Undo.RecordObject(two, "Connect phase two game over");
            one.gameOver = two.gameOver = over;
            one.inactivityLimit = 10f;
            two.timeLimit = 30f;
            over.phaseOne = one;
            over.phaseTwo = two;
            over.retrySequence = one.retrySequence;

            var root = Rect("BattleGameOverCanvas", null);
            var canvas = root.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var scaler = root.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            EditorUtility.CopySerialized(main.GetComponent<UnityEngine.UI.CanvasScaler>(), scaler);
            root.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            over.overlayRoot = root.gameObject;
            Panel("InputBlocker", root, Color.clear, true);
            over.dimScreen = Panel("DimScreen", root, new Color(0f, 0f, 0f, .5f)).gameObject;

            var attack = Rect("CenteredAttackMary", root);
            attack.anchorMin = attack.anchorMax = Vector2.one * .5f;
            attack.anchoredPosition = Vector2.zero;
            attack.sizeDelta = one.slots[0].GetComponent<RectTransform>().sizeDelta;
            var artwork = attack.gameObject.AddComponent<UnityEngine.UI.Image>();
            artwork.sprite = one.attackSprite;
            artwork.preserveAspect = true;
            artwork.raycastTarget = false;
            over.centeredAttack = attack.gameObject;
            over.blackout = Panel("BlackoutAndHands", root, Color.black).gameObject;
            over.hands = new GameObject[12];
            // Fixed anchors, sizes, rotations and opacities, loosely matching the supplied collage.
            Vector2[] positions = {
                new Vector2(.16f, .73f), new Vector2(.09f, .27f), new Vector2(.33f, .20f),
                new Vector2(.39f, .48f), new Vector2(.50f, .70f), new Vector2(.56f, .47f),
                new Vector2(.65f, .43f), new Vector2(.63f, .70f), new Vector2(.81f, .73f),
                new Vector2(.72f, .20f), new Vector2(.90f, .19f), new Vector2(.87f, .43f)
            };
            float[] sizes = { 1050f, 1280f, 1000f, 750f, 930f, 1120f, 590f, 1190f, 970f, 990f, 1230f, 730f };
            float[] rotations = { -28f, -13f, 56f, -10f, -7f, 7f, 12f, -8f, -15f, -34f, 44f, 12f };
            float[] opacity = { 1f, .45f, 1f, .72f, 1f, .53f, .70f, .37f, 1f, 1f, .69f, .62f };
            for (int i = 0; i < 12; i++)
            {
                var rect = Rect("Hand_" + (i + 1).ToString("00"), over.blackout.transform);
                rect.anchorMin = rect.anchorMax = positions[i];
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.one * sizes[i];
                rect.localEulerAngles = new Vector3(0f, 0f, rotations[i]);
                var image = rect.gameObject.AddComponent<UnityEngine.UI.RawImage>();
                image.texture = handTexture; // Entire transparent texture, including its separated fingers.
                image.color = new Color(1f, 1f, 1f, opacity[i]);
                image.raycastTarget = false;
                rect.gameObject.SetActive(false);
                over.hands[i] = rect.gameObject;
            }

            var lost = Panel("SignalLostScreen", root, new Color(184f / 255f, 0f, 0f)).rectTransform;
            over.signalLostScreen = lost.gameObject;
            var title = Text("SignalLostTitle", lost, font, "Signal Lost", 190f).rectTransform;
            title.anchorMin = new Vector2(.12f, .49f);
            title.anchorMax = new Vector2(.88f, .70f);
            title.offsetMin = title.offsetMax = Vector2.zero;
            over.retryButton = Button("RetryButton", lost, font, "Retry", .46f);
            over.exitButton = Button("ExitButton", lost, font, "Exit", .38f);
            UnityEventTools.AddPersistentListener(over.retryButton.onClick, over.Retry);
            UnityEventTools.AddPersistentListener(over.exitButton.onClick, over.Exit);

            var warning = Panel("PhaseTwoDeathWarning", two.window, Color.black, true).rectTransform;
            var background = two.window.Find("BattleBlackBackground") as RectTransform;
            if (!background) throw new InvalidOperationException("Existing black background is required.");
            warning.anchorMin = background.anchorMin;
            warning.anchorMax = background.anchorMax;
            warning.offsetMin = warning.offsetMax = Vector2.zero;
            warning.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            // Every label is scene-editable; no runtime UI generation.
            for (int i = 0; i < 12; i++)
            {
                var text = Text("AccusationRow_" + (i + 1).ToString("00"), warning, font,
                    "네가 죽였어 네가 죽였어 네가 죽였어 네가 죽였어 네가 죽였어 네가 죽였어 네가 죽였어 네가 죽였어", 78f);
                text.color = new Color(.95f, .015f, .025f);
                text.overflowMode = TextOverflowModes.Overflow;
                var rect = text.rectTransform;
                rect.anchorMin = new Vector2(0f, i / 12f);
                rect.anchorMax = new Vector2(1f, (i + 1) / 12f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
            over.phaseTwoWarning = warning.gameObject;
            warning.gameObject.SetActive(false);
            over.dimScreen.SetActive(false);
            over.centeredAttack.SetActive(false);
            over.blackout.SetActive(false);
            lost.gameObject.SetActive(false);
            root.gameObject.SetActive(false);

            if (dialogue != JsonUtility.ToJson(one.GetComponent<BattleProtoConversation>()) ||
                !volumes.SequenceEqual(one.GetComponents<AudioSource>().Select(x => x.volume)))
                throw new InvalidOperationException("Existing conversation or audio settings changed.");
            EditorUtility.SetDirty(one);
            EditorUtility.SetDirty(two);
            EditorUtility.SetDirty(over);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved two deadline connections, failure overlays, 12 fixed full-texture hands, warning rows, and Retry/Exit buttons. Existing dialogue and audio volumes preserved. No gameplay invoked.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }
}
