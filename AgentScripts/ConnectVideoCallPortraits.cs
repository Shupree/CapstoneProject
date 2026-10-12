using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Only scene references are assigned. Does not invoke conversation or preview methods.
public static class ConnectVideoCallPortraits
{
    [Serializable] class Lines
    {
        public ExploreProtoMessenger.CallLine[] explore;
        public BattleProtoConversation.CallLine[] battle;
    }

    public static string Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || Application.dataPath != "D:/CWS/Zol_Project/Assets")
            throw new InvalidOperationException("Target edit mode required.");
        var originalActive = SceneManager.GetActiveScene();
        var opened = new List<Scene>();
        var results = new List<string>();
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
                var messenger = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)).Single();
                var battle = messenger.GetComponent<BattleProtoConversation>();
                if (messenger.callPortraits || messenger.GetComponent<VideoCallPortraits>())
                    throw new InvalidOperationException("Portraits already connected: " + path);
                var images = messenger.callWindow.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                var heroine = images.Single(x => x.name == "PlayerSprite");
                var client = images.Single(x => x.name == "SiyulSprite");
                var friend = images.SingleOrDefault(x => x.name == "FriendPlaceholder");
                string before = JsonUtility.ToJson(new Lines { explore = messenger.callLines, battle = battle ? battle.callLines : null });
                Undo.IncrementCurrentGroup();
                int group = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("Connect dialogue portraits");
                try
                {
                    var portraits = Undo.AddComponent<VideoCallPortraits>(messenger.gameObject);
                    portraits.heroineImage = heroine;
                    portraits.clientImage = client;
                    portraits.friendImage = friend;
                    Undo.RecordObject(messenger, "Connect exploration call portraits");
                    messenger.callPortraits = portraits;
                    if (battle)
                    {
                        Undo.RecordObject(battle, "Connect battle call portraits");
                        battle.callPortraits = portraits;
                        EditorUtility.SetDirty(battle);
                    }
                    string after = JsonUtility.ToJson(new Lines { explore = messenger.callLines, battle = battle ? battle.callLines : null });
                    if (before != after) throw new InvalidOperationException("Existing dialogue was modified.");
                    EditorUtility.SetDirty(portraits);
                    EditorUtility.SetDirty(messenger);
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
                    Undo.CollapseUndoOperations(group);
                    results.Add(scene.name + ": portraits connected; exploration lines=" + messenger.callLines.Length +
                        ", battle lines=" + (battle ? battle.callLines.Length : 0) + "; dialogue preserved.");
                }
                catch { Undo.RevertAllDownToGroup(group); throw; }
            }
        }
        finally
        {
            if (originalActive.IsValid() && originalActive.isLoaded) SceneManager.SetActiveScene(originalActive);
            foreach (var scene in opened) EditorSceneManager.CloseScene(scene, true);
        }
        return string.Join("; ", results);
    }
}
