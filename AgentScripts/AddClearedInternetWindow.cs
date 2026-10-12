using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time scene authoring only. Never runs during gameplay.
public static class AddClearedInternetWindow
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity")
            throw new InvalidOperationException("Open Battle_Proto in edit mode.");
        var phase = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BloodyMaryPhaseController>(true)).Single();
        var source = (RectTransform)phase.internetWindow.transform;
        if (phase.clearedInternetWindow || source.parent.Find("ClearedInternetWindow"))
            throw new InvalidOperationException("Window already authored; edit the saved scene.");
        var conversation = phase.GetComponent<BattleProtoConversation>();
        string dialogue = JsonUtility.ToJson(conversation);
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Add cleared internet window");
        try
        {
            var go = new GameObject("ClearedInternetWindow", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            Undo.RegisterCreatedObjectUndo(go, "Add cleared internet window");
            go.layer = source.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(source.parent, false);
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            rect.sizeDelta = source.sizeDelta;
            rect.anchoredPosition3D = source.anchoredPosition3D;
            rect.localScale = source.localScale;
            rect.localRotation = source.localRotation;
            EditorUtility.CopySerialized(source.GetComponent<UnityEngine.UI.Image>(), go.GetComponent<UnityEngine.UI.Image>());
            foreach (string name in new[] { "InternetTitleDragArea", "InternetCloseX" })
            {
                var original = source.Find(name);
                if (!original) throw new InvalidOperationException("Missing frame control: " + name);
                var child = UnityEngine.Object.Instantiate(original.gameObject, rect, false);
                child.name = name;
                var drag = child.GetComponent<ExploreProtoWindowDrag>();
                if (drag) drag.window = rect;
                var button = child.GetComponent<UnityEngine.UI.Button>();
                if (button)
                {
                    button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                    UnityEventTools.AddPersistentListener(button.onClick, phase.CloseInternet);
                }
            }
            go.SetActive(false);
            Undo.RecordObject(phase, "Connect cleared internet window");
            phase.clearedInternetWindow = rect;
            EditorUtility.SetDirty(phase);
            if (dialogue != JsonUtility.ToJson(conversation)) throw new InvalidOperationException("Dialogue changed.");
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved hidden empty frame with title drag and close button. Size " + rect.sizeDelta + "; dialogue preserved. No gameplay executed.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }
}
