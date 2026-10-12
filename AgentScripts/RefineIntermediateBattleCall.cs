using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Targeted scene authoring; never runs gameplay, previews or simulated input.
public static class RefineIntermediateBattleCall
{
    public static string Apply()
    {
        if (Application.dataPath != "D:/CWS/Zol_Project/Assets" || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Target project edit mode required.");
        var scene = SceneManager.GetSceneByPath("Assets/Scenes/Battle_Proto.unity");
        if (!scene.IsValid() || !scene.isLoaded) throw new InvalidOperationException("Battle scene must be open.");
        var sequence = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BattleProtoRetrySequence>(true)).Single();
        var window = sequence.intermediateCallWindow.transform;
        var root = sequence.intermediateCallRoot.transform;
        if (root.Find("IntermediateDialogueBox") || sequence.backgroundChange.image)
            throw new InvalidOperationException("Refinement already applied.");
        var background = sequence.spriteChanges.Single(c => c.image.name == "Background");
        var heroine = (RectTransform)root.Find("HeroineVideoPane");
        var mary = (RectTransform)root.Find("MerryVideoPane");
        var playerOriginal = (RectTransform)window.Find("PlayerVideoPane");
        var friendOriginal = (RectTransform)window.Find("FriendVideoPane");
        var maryName = mary.Find("MerryName").GetComponent<TMPro.TMP_Text>();
        var conversation = sequence.GetComponent<BattleProtoConversation>();
        string beforeConversation = JsonUtility.ToJson(conversation);
        string beforeWindow = EditorJsonUtility.ToJson(window);
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Refine intermediate call and background timing");
        try
        {
            Undo.RecordObjects(new UnityEngine.Object[] { heroine, mary, maryName, sequence }, "Refine intermediate call");
            MatchPane(heroine, playerOriginal, .18f);
            MatchPane(mary, friendOriginal, .51f);
            MatchImage((RectTransform)heroine.Find("HeroineSprite"), (RectTransform)playerOriginal.Find("PlayerSprite"));
            MatchImage((RectTransform)mary.Find("MerrySprite"), (RectTransform)friendOriginal.Find("FriendPlaceholder"));
            maryName.text = "M#!?A$Ry";

            var dialogue = UnityEngine.Object.Instantiate(window.Find("DialogueBox").gameObject, root, false);
            Undo.RegisterCreatedObjectUndo(dialogue, "Add blank intermediate dialogue UI");
            dialogue.name = "IntermediateDialogueBox";
            foreach (var text in dialogue.GetComponentsInChildren<TMPro.TMP_Text>(true)) text.text = string.Empty;
            dialogue.SetActive(true);
            sequence.backgroundChange = background;
            sequence.spriteChanges = sequence.spriteChanges.Where(c => c.image != background.image).ToArray();
            EditorUtility.SetDirty(sequence);
            EditorUtility.SetDirty(maryName);
            if (beforeConversation != JsonUtility.ToJson(conversation) || beforeWindow != EditorJsonUtility.ToJson(window))
                throw new InvalidOperationException("Existing conversation/window settings changed.");
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved: original pane/image dimensions, centered pair, blank dialogue panel, name M#!?A$Ry; window/conversation preserved; background separated from 4 delayed icon changes.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static void MatchPane(RectTransform target, RectTransform source, float left)
    {
        Vector2 width = source.anchorMax - source.anchorMin;
        target.anchorMin = new Vector2(left, source.anchorMin.y);
        target.anchorMax = new Vector2(left + width.x, source.anchorMax.y);
        target.pivot = source.pivot;
        target.sizeDelta = source.sizeDelta;
        target.anchoredPosition3D = source.anchoredPosition3D;
        target.localScale = source.localScale;
        target.localRotation = source.localRotation;
    }

    static void MatchImage(RectTransform target, RectTransform source)
    {
        Undo.RecordObject(target, "Match original image dimensions");
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.sizeDelta = source.sizeDelta;
        target.anchoredPosition3D = source.anchoredPosition3D;
        target.localScale = source.localScale;
        target.localRotation = source.localRotation;
    }
}
