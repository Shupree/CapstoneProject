using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time authoring only. Does not enter Play mode or invoke gameplay methods.
public static class AuthorCallCardPhase
{
    const string Folder = "Assets/Prefab/BloodyMaryCall";
    static Sprite Sprite(string file) => AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/ProtoType/" + file + ".png").OfType<Sprite>().Single();
    static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        return rect;
    }
    static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = min; rect.offsetMax = max;
    }
    static UnityEngine.UI.Image Image(RectTransform rect, Sprite sprite, bool raycast)
    {
        var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.sprite = sprite; image.color = Color.white; image.raycastTarget = raycast;
        return image;
    }
    static GameObject SaveVariant(GameObject source, string name, Sprite portrait, bool button, bool knife)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        try
        {
            instance.name = name;
            var card = instance.GetComponent<BloodyMaryCallCard>();
            card.artwork.sprite = portrait;
            card.artwork.enabled = true;
            card.isKnife = knife;
            if (button)
            {
                ((RectTransform)instance.transform).sizeDelta = new Vector2(400.2f, 552f);
                ((RectTransform)card.dismissButton.transform).sizeDelta = new Vector2(82.8f, 82.8f);
                ((RectTransform)card.dismissButton.transform.Find("Icon")).sizeDelta = new Vector2(69f, 69f);
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(card.dismissButton.transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(card.dismissButton.transform.Find("Icon"));
            }
            card.dismissButton.gameObject.SetActive(button);
            PrefabUtility.RecordPrefabInstancePropertyModifications(card);
            PrefabUtility.RecordPrefabInstancePropertyModifications(card.artwork);
            PrefabUtility.RecordPrefabInstancePropertyModifications(card.dismissButton.gameObject);
            return PrefabUtility.SaveAsPrefabAsset(instance, Folder + "/" + name + ".prefab");
        }
        finally { UnityEngine.Object.DestroyImmediate(instance); }
    }
    public static string CreatePrefabs()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit mode required.");
        if (AssetDatabase.IsValidFolder(Folder)) throw new InvalidOperationException("Prefab folder already exists; do not overwrite.");
        // Resolve all existing sprites before writing assets.
        var background = Sprite("card_background"); var heroine = Sprite("card_main");
        var mary = Sprite("card_merry"); var knife = Sprite("card_merry_knife"); var dismiss = Sprite("dismiss_btn");
        if (!AssetDatabase.IsValidFolder("Assets/Prefab")) AssetDatabase.CreateFolder("Assets", "Prefab");
        AssetDatabase.CreateFolder("Assets/Prefab", "BloodyMaryCall");
        var root = Rect("CallCard", null);
        try
        {
            root.sizeDelta = new Vector2(348f, 480f);
            Image(root, background, true);
            var art = Rect("Artwork", root);
            art.anchorMin = new Vector2(3f / 58f, 7f / 80f);
            art.anchorMax = new Vector2(55f / 58f, 76f / 80f);
            art.offsetMin = art.offsetMax = Vector2.zero;
            var artwork = Image(art, null, false);
            artwork.preserveAspect = true; artwork.enabled = false;
            var buttonRect = Rect("DismissButton", root);
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(.5f, 7f / 80f);
            buttonRect.sizeDelta = new Vector2(72f, 72f);
            // Keep a generous rectangular hit target and a separate replaceable icon.
            var hitImage = Image(buttonRect, null, true); hitImage.color = Color.clear;
            var button = buttonRect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = hitImage; button.transition = UnityEngine.UI.Selectable.Transition.None;
            button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            var icon = Rect("Icon", buttonRect); icon.sizeDelta = new Vector2(60f, 60f);
            Image(icon, dismiss, false).preserveAspect = true;
            var card = root.gameObject.AddComponent<BloodyMaryCallCard>();
            card.artwork = artwork; card.dismissButton = button;
            buttonRect.gameObject.SetActive(false);
            var basePrefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Folder + "/CallCard.prefab");
            SaveVariant(basePrefab, "HeroineCallCard", heroine, false, false);
            SaveVariant(basePrefab, "MerryConfrontationCard", mary, false, false);
            SaveVariant(basePrefab, "MerryCallCard", mary, true, false);
            SaveVariant(basePrefab, "MerryKnifeCallCard", knife, true, true);
        }
        finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
        AssetDatabase.SaveAssets();
        return "Created common CallCard and four prefab variants in " + Folder;
    }
    static BloodyMaryCallCard Card(string prefab, string name, Transform parent, BloodyMaryPhaseController phase, Vector2 pos)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/" + prefab + ".prefab");
        if (!asset) throw new InvalidOperationException("Missing prefab " + prefab);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        instance.name = name;
        var card = instance.GetComponent<BloodyMaryCallCard>();
        card.phase = phase;
        ((RectTransform)instance.transform).anchoredPosition = pos;
        PrefabUtility.RecordPrefabInstancePropertyModifications(card);
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(instance);
        return card;
    }
    public static string ApplyScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Battle_Proto.unity" || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Battle_Proto edit mode required.");
        var phase = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BloodyMaryPhaseController>(true)).Single();
        if (phase.callWindow) throw new InvalidOperationException("Call board already assigned; stop to preserve edits.");
        var canvas = phase.retrySequence.intermediateCallWindow.transform.parent;
        if (canvas.Find("BattleCallWindow")) throw new InvalidOperationException("Existing board must be inspected first.");
        var font = phase.retrySequence.intermediateCallWindow.GetComponentsInChildren<TMPro.TMP_Text>(true).First(t => t.font).font;
        var windowSprite = Sprite("SpriteNew/Window_Dark");
        string conversationBefore = JsonUtility.ToJson(phase.GetComponent<BattleProtoConversation>());
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Author call-card phase one");
        try
        {
            Undo.RecordObject(phase, "Connect call-card phase");
            var window = Rect("BattleCallWindow", canvas);
            Undo.RegisterCreatedObjectUndo(window.gameObject, "Create call-card window");
            // Existing canvas is 3200 x 1800. Keep the left 360 and bottom 210 units available.
            Stretch(window, new Vector2(360f, 210f), new Vector2(-40f, -60f));
            Image(window, windowSprite, true);
            var interior = Rect("CallArea", window);
            Stretch(interior, new Vector2(40f, 40f), new Vector2(-40f, -100f));
            Image(interior, null, true).color = new Color(.045f, .045f, .055f, 1f);
            interior.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var intro = Rect("Confrontation", interior); Stretch(intro, Vector2.zero, Vector2.zero);
            var hero = Card("HeroineCallCard", "Heroine", intro, phase, new Vector2(-380f, 20f));
            var mary = Card("MerryConfrontationCard", "Merry", intro, phase, new Vector2(380f, 20f));
            ((RectTransform)hero.transform).sizeDelta = ((RectTransform)mary.transform).sizeDelta = new Vector2(464f, 640f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(hero.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(mary.transform);
            var dialogue = Rect("Dialogue", interior); dialogue.sizeDelta = new Vector2(1040f, 180f);
            Image(dialogue, null, true).color = new Color(.015f, .015f, .02f, 1f);
            var textRect = Rect("Text", dialogue); Stretch(textRect, new Vector2(32f, 22f), new Vector2(-32f, -22f));
            var text = textRect.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
            text.font = font; text.fontSize = 42f; text.color = Color.white;
            text.alignment = TMPro.TextAlignmentOptions.Center; text.raycastTarget = false;
            text.text = "갑자기 화면이 이상해졌어.";
            var firstPositions = new Vector2[] { new(380,20), new(-690,180), new(-250,-300), new(720,260), new(-110,170), new(-860,-210), new(510,-280), new(70,-100), new(900,-80) };
            var counts = new[] {9, 12, 18}; var knives = new[] {1, 3, 5};
            var stages = new BloodyMaryPhaseController.Stage[3];
            for (int part = 0; part < 3; part++)
            {
                var root = Rect("Part" + (part + 1), interior); Stretch(root, Vector2.zero, Vector2.zero);
                stages[part] = new BloodyMaryPhaseController.Stage(counts[part], knives[part]);
                stages[part].root = root.gameObject;
                stages[part].cards = new BloodyMaryCallCard[counts[part]];
                for (int i = 0; i < counts[part]; i++)
                {
                    bool knife = part == 0 ? i == 4 : i < knives[part];
                    // First part is authored, repeatable and mildly overlapped. Later grids are editor placeholders only.
                    int columns = part == 0 ? 3 : 6;
                    var position = part == 0 ? firstPositions[i]
                        : new Vector2((i % columns - 2.5f) * 390f, (1 - i / columns) * 350f);
                    stages[part].cards[i] = Card(knife ? "MerryKnifeCallCard" : "MerryCallCard", "Card" + (i + 1).ToString("00"), root, phase, position);
                }
                root.gameObject.SetActive(false);
            }
            dialogue.SetAsLastSibling(); dialogue.gameObject.SetActive(false);
            window.gameObject.SetActive(false);
            phase.callWindow = window; phase.cardArea = interior;
            phase.cardSpawnGapSeconds = .1f;
            phase.confrontationRoot = intro.gameObject; phase.dialogueRoot = dialogue.gameObject;
            phase.confrontationSeconds = 1f; phase.dialogueSeconds = 2f; phase.inactivityLimit = 10f;
            phase.stages = stages;
            // Remove only the old phase-two transition; preserve any unrelated authored listeners.
            for (int i = phase.onPhaseCompleted.GetPersistentEventCount() - 1; i >= 0; i--)
                if (phase.onPhaseCompleted.GetPersistentTarget(i) == phase.phaseTwo && phase.onPhaseCompleted.GetPersistentMethodName(i) == "BeginPhase")
                    UnityEventTools.RemovePersistentListener(phase.onPhaseCompleted, i);
            EditorUtility.SetDirty(phase);
            if (conversationBefore != JsonUtility.ToJson(phase.GetComponent<BattleProtoConversation>())) throw new InvalidOperationException("Existing conversation changed.");
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved BattleCallWindow, confrontation pair, dialogue, and 9/12/18 prefab cards (1/3/5 knives). Old phase-two event disconnected. Existing conversation preserved.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }
}
