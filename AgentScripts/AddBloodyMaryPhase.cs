using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time edit-mode authoring. Never called at runtime; refuses existing phase UI.
public static class AddBloodyMaryPhase
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity")
            throw new InvalidOperationException("Open Battle_Proto in edit mode before authoring.");
        var messenger = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)).Single();
        var retry = messenger.GetComponent<BattleProtoRetrySequence>();
        var conversation = messenger.GetComponent<BattleProtoConversation>();
        if (!retry || !conversation || !messenger.internetWindow)
            throw new InvalidOperationException("Existing battle desktop references are required.");
        if (messenger.bloodyMaryPhase || messenger.GetComponent<BloodyMaryPhaseController>() ||
            messenger.internetWindow.transform.Find("ExorcismContent"))
            throw new InvalidOperationException("Phase already authored. Edit the saved scene instead of rebuilding it.");

        string[] investigationNames = {
            "SearchFieldOutline", "KeywordFieldHitArea", "SearchKeywordChip", "ClearKeyword",
            "Icon_Find", "SearchFieldPlaceholder", "SearchHint", "SearchResults", "SiteDetailPanel"
        };
        var panels = investigationNames.Select(name => messenger.internetWindow.transform.Find(name)).ToArray();
        if (panels.Any(t => !t)) throw new InvalidOperationException("Existing investigation panel not found; do not guess its replacement.");
        var normal = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/ProtoType/bloody_mary.png");
        var attack = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/ProtoType/bloody_mary_attack.png");
        if (!normal || !attack) throw new InvalidOperationException("Both imported Mary sprites are required.");

        // Read and retain the existing user-authored dialogue without rewriting it.
        string dialogueBefore = JsonUtility.ToJson(conversation);
        var existingText = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TMPro.TMP_Text>(true))
            .ToDictionary(t => t, t => t.text);
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Add Bloody Mary image phase");
        try
        {
            Undo.RecordObject(messenger, "Connect Bloody Mary phase");
            var phase = Undo.AddComponent<BloodyMaryPhaseController>(messenger.gameObject);
            phase.retrySequence = retry;
            phase.internetWindow = messenger.internetWindow;
            phase.keywordPopup = messenger.keywordPopup;
            phase.normalSprite = normal;
            phase.attackSprite = attack;
            phase.investigationPanels = panels.Select(t => t.gameObject).ToArray();
            phase.stages = new[] {
                new BloodyMaryPhaseController.Stage(12, 1),
                new BloodyMaryPhaseController.Stage(18, 2),
                new BloodyMaryPhaseController.Stage(24, 3)
            };

            var area = Child(messenger.internetWindow.transform, "ExorcismContent");
            area.gameObject.SetActive(false);
            area.offsetMin = new Vector2(100, 84);
            area.offsetMax = new Vector2(-100, -160);
            phase.exorcismContent = area.gameObject;
            var scrollRoot = Child(area, "ImageScroll");
            var scroll = scrollRoot.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            scroll.inertia = true;
            scroll.scrollSensitivity = 100;
            var viewport = Child(scrollRoot, "Viewport");
            var viewportGraphic = viewport.gameObject.AddComponent<UnityEngine.UI.Image>();
            viewportGraphic.color = Color.white;
            viewportGraphic.raycastTarget = true;
            viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
            var content = Child(viewport, "Content");
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1);
            content.anchoredPosition = Vector2.zero;

            // Native UI size under this canvas's reference PPU; unchanged across all stages.
            var canvas = messenger.internetWindow.GetComponentInParent<Canvas>();
            Vector2 cell = normal.rect.size * (canvas.referencePixelsPerUnit / normal.pixelsPerUnit);
            const float columnGap = 80, rowGap = 64;
            const int topPadding = 72, bottomPadding = 72;
            var grid = content.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.cellSize = cell;
            grid.spacing = new Vector2(columnGap, rowGap);
            grid.padding = new RectOffset(0, 0, topPadding, bottomPadding);
            var fitter = content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            fitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            float initialHeight = topPadding + bottomPadding + cell.y * 4 + rowGap * 3;
            content.sizeDelta = new Vector2(0, initialHeight);
            float width = ((RectTransform)messenger.internetWindow.transform).rect.width - 200;
            float left = (width - cell.x * 3 - columnGap * 2) * .5f;
            if (left < 0) throw new InvalidOperationException("Three native-size cards do not fit the current window.");
            scroll.content = content;
            scroll.viewport = viewport;
            phase.imageScroll = scroll;
            phase.slots = new BloodyMaryImageSlot[24];
            for (int i = 0; i < phase.slots.Length; i++)
            {
                var rt = Child(content, "MaryImage_" + (i + 1).ToString("00"));
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
                rt.sizeDelta = cell;
                rt.anchoredPosition = new Vector2(left + (i % 3) * (cell.x + columnGap),
                    -(topPadding + (i / 3) * (cell.y + rowGap)));
                var image = rt.gameObject.AddComponent<UnityEngine.UI.Image>();
                image.sprite = normal;
                image.type = UnityEngine.UI.Image.Type.Simple;
                image.preserveAspect = true;
                image.raycastTarget = true;
                var button = rt.gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = image;
                var colors = button.colors;
                colors.highlightedColor = new Color(.94f, .94f, .94f, 1);
                colors.pressedColor = new Color(.76f, .76f, .76f, 1);
                colors.disabledColor = Color.white;
                button.colors = colors;
                button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
                var slot = rt.gameObject.AddComponent<BloodyMaryImageSlot>();
                slot.phase = phase;
                slot.slotIndex = i;
                slot.artwork = image;
                slot.button = button;
                phase.slots[i] = slot;
                rt.gameObject.SetActive(i < 12);
            }
            messenger.bloodyMaryPhase = phase;
            // Preserve the existing close button and title drag above the added content.
            // The new area is inset beneath them; no existing transform or listener is edited.
            if (dialogueBefore != JsonUtility.ToJson(conversation) || existingText.Any(pair => pair.Key.text != pair.Value))
                throw new InvalidOperationException("Existing dialogue/text must remain unchanged.");
            EditorUtility.SetDirty(phase);
            EditorUtility.SetDirty(messenger);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save Battle_Proto.");
            Undo.CollapseUndoOperations(undoGroup);
            return "Saved 24 scene-authored slots; native cell " + cell + "; existing dialogue and TMP text preserved. No gameplay executed.";
        }
        catch
        {
            Undo.RevertAllDownToGroup(undoGroup);
            throw;
        }
    }

    static RectTransform Child(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Author image phase UI");
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }
}
