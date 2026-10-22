using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Targeted edit-mode authoring. Does not run gameplay, buttons, or previews.
public static class ReworkBattleEntry
{
    [Serializable] class Lines { public BattleProtoConversation.CallLine[] items; }
    static BattleProtoConversation Conversation()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity")
            throw new InvalidOperationException("Open Battle_Proto in edit mode.");
        return scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<BattleProtoConversation>(true)).Single();
    }
    static RectTransform Required(Transform root, string name)
    {
        var child = root.Find(name) as RectTransform;
        if (!child) throw new InvalidOperationException("Missing " + name);
        return child;
    }
    static void Place(RectTransform r, Vector2 min, Vector2 max)
    {
        Undo.RecordObject(r, "Arrange battle entry");
        r.anchorMin = min; r.anchorMax = max;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
    public static string Apply()
    {
        var c = Conversation();
        var p = c.GetComponent<BloodyMaryPhaseController>();
        var call = (RectTransform)c.groupCallWindow.transform;
        var internet = (RectTransform)p.internetWindow.transform;
        var black = Required(internet, "BattleBlackBackground");
        var player = Required(call, "PlayerVideoPane");
        var client = Required(call, "SiyulVideoPane");
        var mary = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/ProtoType/card_merry_02.png").OfType<Sprite>().Single();
        if (c.callLines.Length <= 12) throw new InvalidOperationException("Element 12 is missing.");
        string linesBefore = JsonUtility.ToJson(new Lines { items = c.callLines });
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Rework battle call and internet entry");
        Undo.RegisterFullObjectHierarchyUndo(call.parent.gameObject, "Rework battle entry");
        Undo.RecordObject(c, "Connect battle conversation");
        Undo.RecordObject(p, "Connect internet entry");
        try
        {
            // Reuse the exact internet frame and black interior, preserving existing controls.
            EditorUtility.CopySerialized(internet.GetComponent<UnityEngine.UI.Image>(), call.GetComponent<UnityEngine.UI.Image>());
            call.sizeDelta = internet.sizeDelta;
            var callBlack = call.Find("BattleBlackBackground");
            if (!callBlack)
            {
                var go = UnityEngine.Object.Instantiate(black.gameObject, call, false);
                Undo.RegisterCreatedObjectUndo(go, "Add saved call interior");
                go.name = "BattleBlackBackground";
                callBlack = go.transform;
            }
            callBlack.SetAsFirstSibling();
            callBlack.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            Place(player, new Vector2(.16f, .43f), new Vector2(.49f, .89f));
            Place(client, new Vector2(.51f, .43f), new Vector2(.84f, .89f));
            player.GetComponent<UnityEngine.UI.Image>().color = Color.black;
            client.GetComponent<UnityEngine.UI.Image>().color = Color.black;
            Required(player, "PlayerName").GetComponent<TMPro.TMP_Text>().text = "유미나";
            Required(client, "SiyulName").GetComponent<TMPro.TMP_Text>().text = "이서연";
            var header = Required(call, "CallHeader");
            header.GetComponent<TMPro.TMP_Text>().text = "화상 통화 · 2명 접속";
            Place(header, new Vector2(.10f, .925f), new Vector2(.80f, .985f));
            var drag = Required(call, "CallTitleDragArea");
            var sourceDrag = Required(internet, "InternetTitleDragArea");
            Place(drag, sourceDrag.anchorMin, sourceDrag.anchorMax);
            var close = Required(call, "CallCloseX");
            var sourceClose = Required(internet, "InternetCloseX");
            Place(close, sourceClose.anchorMin, sourceClose.anchorMax);
            EditorUtility.CopySerialized(sourceClose.GetComponent<UnityEngine.UI.Image>(), close.GetComponent<UnityEngine.UI.Image>());
            if (close.Find("Label")) close.Find("Label").gameObject.SetActive(false);
            Required(call, "DialogueBox").GetComponent<UnityEngine.UI.Image>().color = Color.black;
            foreach (var portraits in call.GetComponentsInChildren<VideoCallPortraits>(true)) portraits.friendImage = null;
            if (c.callPortraits) { Undo.RecordObject(c.callPortraits, "Remove third participant"); c.callPortraits.friendImage = null; }
            if (c.messenger.callPortraits) { Undo.RecordObject(c.messenger.callPortraits, "Remove third participant"); c.messenger.callPortraits.friendImage = null; }
            foreach (string name in new[] { "FriendVideoPane", "IntermediateCall" })
                if (call.Find(name)) Undo.DestroyObjectImmediate(call.Find(name).gameObject);
            foreach (string name in new[] { "Confrontation", "Dialogue" })
            {
                var old = p.callWindow.GetComponentsInChildren<RectTransform>(true).SingleOrDefault(t => t.name == name);
                if (old) Undo.DestroyObjectImmediate(old.gameObject);
            }
            c.themeChangeAfterElement = 12;
            if (!p.maryEntryButton)
            {
                var go = new GameObject("MaryBattleEntry", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
                Undo.RegisterCreatedObjectUndo(go, "Add saved internet Mary entry");
                go.layer = internet.gameObject.layer;
                go.transform.SetParent(black, false);
                p.maryEntryButton = go.GetComponent<UnityEngine.UI.Button>();
            }
            var entry = (RectTransform)p.maryEntryButton.transform;
            entry.anchorMin = entry.anchorMax = entry.pivot = new Vector2(.5f, .5f);
            entry.anchoredPosition = Vector2.zero;
            float height = black.rect.height * .86f;
            float width = height * mary.rect.width / mary.rect.height;
            if (width > black.rect.width * .8f) { height *= black.rect.width * .8f / width; width = black.rect.width * .8f; }
            entry.sizeDelta = new Vector2(width, height);
            var art = entry.GetComponent<UnityEngine.UI.Image>();
            art.sprite = mary; art.color = Color.white; art.preserveAspect = true; art.raycastTarget = true;
            p.maryEntryButton.targetGraphic = art;
            p.maryEntryButton.transition = UnityEngine.UI.Selectable.Transition.None;
            p.maryEntryButton.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.None };
            p.maryEntryButton.onClick = new UnityEngine.UI.Button.ButtonClickedEvent(); // Wired once in Awake.
            entry.gameObject.SetActive(false);
            c.nextLabel.text = "다음";
            c.callSpeaker.text = c.callLines[0].speaker;
            c.callText.text = c.callLines[0].text;
            if (linesBefore != JsonUtility.ToJson(new Lines { items = c.callLines })) throw new InvalidOperationException("Dialogue was changed.");
            EditorUtility.SetDirty(c); EditorUtility.SetDirty(p);
            EditorSceneManager.MarkSceneDirty(call.gameObject.scene);
            if (!EditorSceneManager.SaveScene(call.gameObject.scene)) throw new InvalidOperationException("Scene save failed.");
            Undo.CollapseUndoOperations(group);
            return "Saved two-person call with internet frame/black interior; removed old intro; Mary entry size=" + entry.sizeDelta + "; dialogue preserved (" + c.callLines.Length + " lines); transition after Element 12. No gameplay executed.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }

    public static string DescribeSavedSetup()
    {
        var c = Conversation();
        var p = c.GetComponent<BloodyMaryPhaseController>();
        var r = c.retrySequence;
        return "scene=" + c.gameObject.scene.path + "; playing=" + EditorApplication.isPlaying +
            "; lines=" + c.callLines.Length + "; end=" + c.callLines[12].text +
            "; friendRef=" + (bool)c.callPortraits.friendImage +
            "; entry=" + p.maryEntryButton.name + "; entrySize=" + ((RectTransform)p.maryEntryButton.transform).rect.size +
            "; entryRaycast=" + p.maryEntryButton.targetGraphic.raycastTarget +
            "; closesCall=" + r.windowsToClose.Contains(c.groupCallWindow) +
            "; switch=" + AssetDatabase.GetAssetPath(r.lightSwitchClip) +
            "; icons=" + r.spriteChanges.Length + "; stages=" + string.Join(",", p.stages.Select(s => s.imageCount + "/" + s.attackCount)) +
            "; oldIntroObjects=" + p.callWindow.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Confrontation" || t.name == "Dialogue");
    }
}

