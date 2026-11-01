using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Targeted content edit only; no gameplay or validation functions are invoked.
public static class ConnectMaryHopeClue
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity")
            throw new InvalidOperationException("Explore_Proto edit mode required.");
        var m = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)).Single();
        int siteIndex = Array.FindIndex(m.siteEntries, s => s.button && s.button.name == "SiteResult_bloody_org");
        int clueIndex = Array.FindIndex(m.investigation.clues, c => c.id == "mary_weakness");
        if (siteIndex < 0 || clueIndex < 0) throw new InvalidOperationException("Missing Bloody.org or existing weakness clue.");
        string source = File.ReadAllText("Assets/Scripts/Story_Parts/Chapter1_BloodyMary/SearchPosts.md").Replace("\r\n", "\n");
        var match = Regex.Match(source, @"^## bloody_org\n[\s\S]*?```text\n(?<body>[\s\S]*?)\n```", RegexOptions.Multiline);
        if (!match.Success) throw new InvalidOperationException("Missing article source.");
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Connect hidden Mary hope clue");
        Undo.RecordObject(m, "Update Bloody.org article");
        Undo.RecordObject(m.investigation, "Update weakness memo");
        m.siteEntries[siteIndex].article = match.Groups["body"].Value;
        m.investigation.clues[clueIndex].memoText = "메리는 '네 아기는 죽지 않았어' 라는 말에 희망을 갖는다";
        EditorUtility.SetDirty(m); EditorUtility.SetDirty(m.investigation);
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Scene save failed.");
        Undo.CollapseUndoOperations(group);
        return "Saved vision-only item 3 with mary_weakness link and matching memo.";
    }
}
