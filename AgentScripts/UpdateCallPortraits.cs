using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Edit-mode sprite assignment only; no gameplay or screen-preview functions are called.
public static class UpdateCallPortraits
{
    public static string Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || Application.dataPath != "D:/CWS/Zol_Project/Assets")
            throw new InvalidOperationException("Target project edit mode required.");
        var originalActive = SceneManager.GetActiveScene();
        var opened = new List<Scene>();
        var assignments = new List<KeyValuePair<UnityEngine.UI.Image, Sprite>>();
        var scenes = new List<Scene>();
        var names = new[] { "Heroine_1", "Client_1", "Friend_1" };
        var portraits = names.Select(n => AssetDatabase.LoadAllAssetsAtPath(
            "Assets/Sprite/ProtoType/SpriteNew/" + n + ".png").OfType<Sprite>().Single()).ToArray();
        try
        {
            foreach (string path in new[] { "Assets/Scenes/Battle_Proto.unity", "Assets/Scenes/Explore_Proto.unity" })
            {
                var scene = SceneManager.GetSceneByPath(path);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                    opened.Add(scene);
                }
                scenes.Add(scene);
                var images = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<UnityEngine.UI.Image>(true)).ToArray();
                assignments.Add(new KeyValuePair<UnityEngine.UI.Image, Sprite>(images.Single(x => x.name == "PlayerSprite"), portraits[0]));
                assignments.Add(new KeyValuePair<UnityEngine.UI.Image, Sprite>(images.Single(x => x.name == "SiyulSprite"), portraits[1]));
                if (scene.name == "Battle_Proto")
                    assignments.Add(new KeyValuePair<UnityEngine.UI.Image, Sprite>(images.Single(x => x.name == "FriendPlaceholder"), portraits[2]));
            }
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Update video call portraits");
            try
            {
                foreach (var pair in assignments)
                {
                    Undo.RecordObject(pair.Key, "Update video call portrait");
                    pair.Key.sprite = pair.Value;
                    if (pair.Key.name == "FriendPlaceholder")
                    {
                        pair.Key.color = Color.white; // Remove the old gray placeholder tint.
                        pair.Key.preserveAspect = true;
                    }
                    EditorUtility.SetDirty(pair.Key);
                }
                foreach (var scene in scenes)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed: " + scene.path);
                }
                Undo.CollapseUndoOperations(group);
                return string.Join("; ", assignments.Select(p => p.Key.gameObject.scene.name + "/" + p.Key.name + " -> " + p.Value.name));
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }
        finally
        {
            if (originalActive.IsValid() && originalActive.isLoaded) SceneManager.SetActiveScene(originalActive);
            foreach (var scene in opened) EditorSceneManager.CloseScene(scene, true);
        }
    }
}
