using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Targeted edit-mode authoring. Does not invoke search, clue collection or gameplay.
public static class AuthorExploreSearchPosts
{
    const string SourcePath = "Assets/Scripts/Story_Parts/Chapter1_BloodyMary/SearchPosts.md";
    const string ScenePath = "Assets/Scenes/Explore_Proto.unity";

    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath)
            throw new InvalidOperationException("Explore_Proto edit mode required.");
        var m = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)).Single();
        var posts = Regex.Matches(File.ReadAllText(SourcePath).Replace("\r\n", "\n"),
            @"^## (?<id>\w+)\n검색어: (?<query>[^\n]+)\n분류: (?<category>[^\n]+)\n제목: (?<title>[^\n]+)\n요약: (?<summary>[^\n]+)\n```text\n(?<body>[\s\S]*?)\n```", RegexOptions.Multiline)
            .Cast<Match>().ToArray();
        if (posts.Length != 18 || m.siteEntries.Length != 7)
            throw new InvalidOperationException("Expected 18 source posts and the original 7 rows; do not reapply to an authored scene.");
        var content = m.searchResults.GetComponent<UnityEngine.UI.ScrollRect>().content;
        var original = m.siteEntries.Select(s => s.button).ToArray();
        if (original.Any(b => !b || b.transform.parent != content) ||
            !content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>() ||
            !content.GetComponent<UnityEngine.UI.ContentSizeFitter>())
            throw new InvalidOperationException("Search row hierarchy differs from the inspected scene.");
        foreach (var b in original)
            foreach (var name in new[] { "Category", "Title", "Summary" })
                if (!b.transform.Find(name)?.GetComponent<TMP_Text>())
                    throw new InvalidOperationException("Missing result label: " + name);

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Author exploration search posts");
        try
        {
            Undo.RecordObject(m, "Set site contents and search terms");
            var entries = new ExploreProtoMessenger.SiteEntry[posts.Length];
            for (int i = 0; i < posts.Length; i++)
            {
                var p = posts[i];
                var button = i < original.Length ? original[i] : UnityEngine.Object.Instantiate(original[0], content, false);
                if (i >= original.Length) Undo.RegisterCreatedObjectUndo(button.gameObject, "Add saved search result row");
                Undo.RecordObject(button.gameObject, "Name result row");
                button.gameObject.name = "SiteResult_" + p.Groups["id"].Value;
                button.transform.SetSiblingIndex(i);
                entries[i] = new ExploreProtoMessenger.SiteEntry
                {
                    category = p.Groups["category"].Value,
                    title = p.Groups["title"].Value,
                    summary = p.Groups["summary"].Value,
                    article = p.Groups["body"].Value,
                    searchTerms = new[] { p.Groups["query"].Value },
                    openDialogueSceneId = p.Groups["id"].Value == "ritual_09" ? "Explore_ClientPostOpened" : string.Empty,
                    button = button
                };
                SetLabel(button.transform, "Category", entries[i].category);
                SetLabel(button.transform, "Title", entries[i].title);
                SetLabel(button.transform, "Summary", entries[i].summary);
                // The first group is authored visible; runtime only filters these saved rows.
                button.gameObject.SetActive(i < 8);
            }
            m.siteEntries = entries;
            Undo.RecordObject(m.investigation, "Align revealed clue memo");
            var clues = m.investigation.clues;
            int curse = Array.FindIndex(clues, c => c.id == "mary_curse");
            if (curse < 0) throw new InvalidOperationException("Missing existing mary_curse clue.");
            clues[curse].memoText = "메리가 당신을 용서하지 않을 것이다.";
            m.investigation.clues = clues;
            m.investigation.evidenceDialogueSceneId = "Explore_ClientPostEvidence";
            EditorUtility.SetDirty(m);
            EditorUtility.SetDirty(m.investigation);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Scene save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved 18 posts (8 general, 9 ritual, 1 Bloody.org) and existing clue links in " + scene.path;
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            throw;
        }
    }

    // Update only the Bloody.org article and add the client's ninth ritual result.
    public static string UpdateEvidence()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != ScenePath)
            throw new InvalidOperationException("Explore_Proto edit mode required.");
        var m = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ExploreProtoMessenger>(true)).Single();
        if (m.siteEntries.Length != 17 || m.siteEntries.Any(s => s.button.name == "SiteResult_ritual_09"))
            throw new InvalidOperationException("Expected the 17-post scene before the evidence addition.");
        var posts = Regex.Matches(File.ReadAllText(SourcePath).Replace("\r\n", "\n"),
            @"^## (?<id>\w+)\n검색어: (?<query>[^\n]+)\n분류: (?<category>[^\n]+)\n제목: (?<title>[^\n]+)\n요약: (?<summary>[^\n]+)\n```text\n(?<body>[\s\S]*?)\n```", RegexOptions.Multiline)
            .Cast<Match>().ToArray();
        var bloody = posts.Single(p => p.Groups["id"].Value == "bloody_org");
        var evidence = posts.Single(p => p.Groups["id"].Value == "ritual_09");
        int index = Array.FindIndex(m.siteEntries, s => s.button.name == "SiteResult_bloody_org");
        var template = m.siteEntries.Single(s => s.button.name == "SiteResult_ritual_08").button;
        var content = m.searchResults.GetComponent<UnityEngine.UI.ScrollRect>().content;
        if (index < 0 || template.transform.parent != content)
            throw new InvalidOperationException("Search layout differs from inspected scene.");
        var reader = m.investigation.bottomDialogue.GetComponent<DialogueCsvReader>();
        if (!reader || AssetDatabase.GetAssetPath(reader.CsvAsset) != "Assets/Explore_Proto/Data/Explore_InvestigationMonologues.csv")
            throw new InvalidOperationException("Unexpected monologue CSV reference.");

        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Connect client evidence post");
        try
        {
            Undo.RecordObject(m, "Revise articles");
            var entries = m.siteEntries.ToList();
            var revised = entries[index];
            revised.article = bloody.Groups["body"].Value;
            entries[index] = revised;
            var button = UnityEngine.Object.Instantiate(template, content, false);
            Undo.RegisterCreatedObjectUndo(button.gameObject, "Add saved client post row");
            button.name = "SiteResult_ritual_09";
            button.transform.SetAsLastSibling();
            button.gameObject.SetActive(false);
            var added = new ExploreProtoMessenger.SiteEntry
            {
                category = evidence.Groups["category"].Value, title = evidence.Groups["title"].Value,
                summary = evidence.Groups["summary"].Value, article = evidence.Groups["body"].Value,
                searchTerms = new[] { evidence.Groups["query"].Value },
                openDialogueSceneId = "Explore_ClientPostOpened", button = button
            };
            SetLabel(button.transform, "Category", added.category);
            SetLabel(button.transform, "Title", added.title);
            SetLabel(button.transform, "Summary", added.summary);
            entries.Add(added);
            m.siteEntries = entries.ToArray();
            Undo.RecordObject(m.investigation, "Connect evidence monologue");
            m.investigation.evidenceDialogueSceneId = "Explore_ClientPostEvidence";
            var clues = m.investigation.clues;
            int clue = Array.FindIndex(clues, c => c.id == "mary_comment");
            if (clue < 0) throw new InvalidOperationException("Missing mary_comment progression clue.");
            clues[clue].memoText = "seoyeon041은 금지된 말의 부작용을 묻고, 오늘 친구에게 해 보겠다는 댓글을 남겼다.";
            m.investigation.clues = clues;
            // Keep the forbidden phrase consistent with the new source and evidence post.
            foreach (var topic in m.investigation.researchQuestions.topics.Where(t => t.id == "side_effect"))
                foreach (var label in topic.messages.SelectMany(g => g.GetComponentsInChildren<TMP_Text>(true)))
                    if (label.text.Contains("네 아기는 내가 죽였어"))
                    {
                        Undo.RecordObject(label, "Align quoted forbidden phrase");
                        label.text = label.text.Replace("네 아기는 내가 죽였어", "네 아이는 내가 죽였어");
                        EditorUtility.SetDirty(label);
                    }
            EditorUtility.SetDirty(m); EditorUtility.SetDirty(m.investigation);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Scene save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved revised Bloody.org and ninth ritual post with visit/click monologue references.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    static void SetLabel(Transform row, string name, string value)
    {
        var label = row.Find(name).GetComponent<TMP_Text>();
        Undo.RecordObject(label, "Set search result label");
        label.text = value;
        EditorUtility.SetDirty(label);
    }
}
