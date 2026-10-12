using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time authoring of a scene-saved input field; no gameplay invocation.
public static class AddPhaseTwoInput
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity")
            throw new InvalidOperationException("Open Battle_Proto in edit mode.");
        var phase = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BloodyMaryPhaseController>(true)).Single();
        var parent = phase.clearedInternetWindow;
        if (!parent || parent.Find("PhaseTwoInput")) throw new InvalidOperationException("Missing window or input already authored.");
        var source = phase.internetWindow.transform.Find("SearchFieldOutline");
        var textSource = phase.internetWindow.transform.Find("SearchFieldPlaceholder");
        if (!source || !textSource) throw new InvalidOperationException("Existing search design required.");
        string dialogue = JsonUtility.ToJson(phase.GetComponent<BattleProtoConversation>());
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Add phase two input");
        try
        {
            var go = UnityEngine.Object.Instantiate(source.gameObject, parent, false);
            Undo.RegisterCreatedObjectUndo(go, "Add phase two input");
            go.name = "PhaseTwoInput";
            go.SetActive(true);
            var rect = (RectTransform)go.transform;
            // Keep the original search frame's width and height; move to lower center.
            var height = rect.anchorMax.y - rect.anchorMin.y;
            rect.anchorMin = new Vector2(rect.anchorMin.x, .20f - height * .5f);
            rect.anchorMax = new Vector2(rect.anchorMax.x, .20f + height * .5f);
            rect.anchoredPosition = Vector2.zero;
            var background = go.GetComponent<UnityEngine.UI.Image>();
            background.raycastTarget = true;

            var viewport = new GameObject("TextArea", typeof(RectTransform), typeof(UnityEngine.UI.RectMask2D));
            viewport.layer = go.layer;
            var viewRect = (RectTransform)viewport.transform;
            viewRect.SetParent(rect, false);
            viewRect.anchorMin = new Vector2(.075f, .15f);
            viewRect.anchorMax = new Vector2(.925f, .85f);
            viewRect.offsetMin = viewRect.offsetMax = Vector2.zero;
            var textObject = UnityEngine.Object.Instantiate(textSource.gameObject, viewRect, false);
            textObject.name = "Text";
            textObject.SetActive(true);
            var text = textObject.GetComponent<TMPro.TextMeshProUGUI>();
            text.text = string.Empty;
            text.raycastTarget = false;
            text.richText = false;
            text.enableAutoSizing = false;
            text.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            text.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            var input = go.AddComponent<TMPro.TMP_InputField>();
            input.targetGraphic = background;
            input.textViewport = viewRect;
            input.textComponent = text;
            input.contentType = TMPro.TMP_InputField.ContentType.Standard;
            input.lineType = TMPro.TMP_InputField.LineType.SingleLine;
            input.richText = false;
            input.readOnly = false;
            input.transition = UnityEngine.UI.Selectable.Transition.None;
            input.customCaretColor = true;
            input.caretColor = text.color;
            input.text = string.Empty;
            if (dialogue != JsonUtility.ToJson(phase.GetComponent<BattleProtoConversation>()))
                throw new InvalidOperationException("Dialogue changed.");
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved PhaseTwoInput in lower center with existing search frame and font. No submit action, no gameplay executed.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }
}
