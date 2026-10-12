using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Authoring only: saves the new call stage without invoking gameplay or previews.
public static class AddIntermediateBattleCall
{
    public static string Apply()
    {
        if (Application.dataPath != "D:/CWS/Zol_Project/Assets" || EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Target project edit mode required.");
        var scene = SceneManager.GetSceneByPath("Assets/Scenes/Battle_Proto.unity");
        if (!scene.IsValid() || !scene.isLoaded) throw new InvalidOperationException("Open Battle_Proto first.");
        var sequence = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BattleProtoRetrySequence>(true)).Single();
        var conversation = sequence.GetComponent<BattleProtoConversation>();
        var window = conversation.groupCallWindow.transform;
        if (window.Find("IntermediateCall") || sequence.intermediateCallRoot)
            throw new InvalidOperationException("Intermediate call already exists.");
        var heroine = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/ProtoType/SpriteNew/Heroine_6.png").OfType<Sprite>().Single();
        var merry = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/ProtoType/SpriteNew/Merry.png").OfType<Sprite>().Single();
        string dialogueBefore = JsonUtility.ToJson(conversation);
        string musicBefore = EditorJsonUtility.ToJson(sequence.backgroundMusic);
        string effectsBefore = EditorJsonUtility.ToJson(sequence.effectsAudio);
        var normalObjects = Enumerable.Range(0, window.childCount).Select(i => window.GetChild(i).gameObject).ToArray();
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Add two-person intermediate battle call");
        try
        {
            var root = new GameObject("IntermediateCall", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(root, "Create intermediate call");
            root.transform.SetParent(window, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var header = Clone(window.Find("CallHeader"), root.transform, "IntermediateHeader");
            header.GetComponent<TMPro.TMP_Text>().text = "●  화상 통화  ·  2명 접속";
            var player = Clone(window.Find("PlayerVideoPane"), root.transform, "HeroineVideoPane");
            SetBounds(player, new Vector2(.02f, .1f), new Vector2(.49f, .89f));
            var playerImage = player.transform.Find("PlayerSprite").GetComponent<UnityEngine.UI.Image>();
            playerImage.sprite = heroine;
            playerImage.color = Color.white;
            playerImage.preserveAspect = true;
            playerImage.gameObject.name = "HeroineSprite";
            var mary = Clone(window.Find("FriendVideoPane"), root.transform, "MerryVideoPane");
            SetBounds(mary, new Vector2(.51f, .1f), new Vector2(.98f, .89f));
            var maryImage = mary.transform.Find("FriendPlaceholder").GetComponent<UnityEngine.UI.Image>();
            maryImage.sprite = merry;
            maryImage.color = Color.white;
            maryImage.preserveAspect = true;
            maryImage.gameObject.name = "MerrySprite";
            var maryName = mary.transform.Find("FriendName");
            maryName.GetComponent<TMPro.TMP_Text>().text = "메리";
            maryName.name = "MerryName";
            root.SetActive(false);
            Undo.RecordObject(sequence, "Connect intermediate call");
            sequence.intermediateCallSeconds = 1.5f;
            sequence.intermediateCallWindow = window.gameObject;
            sequence.intermediateCallRoot = root;
            sequence.normalCallObjects = normalObjects;
            EditorUtility.SetDirty(sequence);
            if (dialogueBefore != JsonUtility.ToJson(conversation) ||
                musicBefore != EditorJsonUtility.ToJson(sequence.backgroundMusic) ||
                effectsBefore != EditorJsonUtility.ToJson(sequence.effectsAudio))
                throw new InvalidOperationException("Existing dialogue/audio settings changed.");
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved Battle_Proto: hidden two-pane intermediate call; Heroine_6/Merry; 1.5s; " +
                normalObjects.Length + " normal call objects linked; existing dialogue/audio settings preserved.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static GameObject Clone(Transform source, Transform parent, string name)
    {
        var clone = UnityEngine.Object.Instantiate(source.gameObject, parent, false);
        Undo.RegisterCreatedObjectUndo(clone, "Copy call appearance");
        clone.name = name;
        return clone;
    }

    static void SetBounds(GameObject pane, Vector2 min, Vector2 max)
    {
        var rect = (RectTransform)pane.transform;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
