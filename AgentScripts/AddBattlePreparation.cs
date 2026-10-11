using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time authoring only. Subsequent edits belong to the saved Battle_Proto scene.
public static class AddBattlePreparation
{
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Battle_Proto.unity")
            throw new InvalidOperationException("Open Battle_Proto in edit mode.");
        var m = UnityEngine.Object.FindFirstObjectByType<ExploreProtoMessenger>();
        if (!m || m.GetComponent<BattleProtoConversation>())
            throw new InvalidOperationException("Desktop missing or battle preparation already authored.");
        Undo.RegisterFullObjectHierarchyUndo(m.callWindow.transform.parent.gameObject, "Add battle conversation and group call");
        var battle = Undo.AddComponent<BattleProtoConversation>(m.gameObject);
        battle.messenger = m;
        battle.groupCallWindow = m.callWindow;
        string[] lines = {
            "시율씨 준비되셨나요?",
            "네 준비됐어요. 친구도 지금은 괜찮은 것 같네요.",
            "다행이네요. 통화에 들어와주시겠어요?"
        };
        battle.preparationMessages = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            bool outgoing = i != 1;
            var template = m.investigation.followUpMessages[outgoing ? 1 : 2];
            var row = UnityEngine.Object.Instantiate(template, m.conversationScroll.content, false);
            Undo.RegisterCreatedObjectUndo(row, "Add battle message");
            row.name = "BattlePreparationMessage_" + i;
            row.transform.Find("Bubble/Speaker").GetComponent<TMP_Text>().text = outgoing ? "주인공" : "시율";
            row.transform.Find("Bubble/Message").GetComponent<TMP_Text>().text = lines[i];
            var layout = row.GetComponent<UnityEngine.UI.LayoutElement>();
            layout.minHeight = layout.preferredHeight = 240;
            row.SetActive(false);
            battle.preparationMessages[i] = row;
        }
        var reply = UnityEngine.Object.Instantiate(m.investigation.replyButton.gameObject,
            m.investigation.replyButton.transform.parent, false);
        Undo.RegisterCreatedObjectUndo(reply, "Add battle call choice");
        reply.name = "StartGroupVideoCall";
        var button = reply.GetComponent<UnityEngine.UI.Button>();
        while (button.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(button.onClick, 0);
        reply.GetComponentInChildren<TMP_Text>(true).text = "화상통화를 건다.";
        battle.startCallButton = button;
        reply.SetActive(false);

        var call = m.callWindow.transform;
        var player = (RectTransform)call.Find("PlayerVideoPane");
        var siyul = (RectTransform)call.Find("SiyulVideoPane");
        var friend = UnityEngine.Object.Instantiate(siyul.gameObject, call, false);
        Undo.RegisterCreatedObjectUndo(friend, "Add friend video pane");
        friend.name = "FriendVideoPane";
        Place(player, .02f, .12f, .33f, .89f);
        Place(siyul, .345f, .12f, .655f, .89f);
        Place((RectTransform)friend.transform, .67f, .12f, .98f, .89f);
        Place((RectTransform)player.Find("PlayerSprite"), .10f, .16f, .90f, .98f);
        Place((RectTransform)siyul.Find("SiyulSprite"), .10f, .16f, .90f, .98f);
        player.Find("PlayerSprite").GetComponent<UnityEngine.UI.Image>().preserveAspect = true;
        siyul.Find("SiyulSprite").GetComponent<UnityEngine.UI.Image>().preserveAspect = true;
        Place((RectTransform)player.Find("PlayerName"), .04f, .035f, .96f, .115f);
        Place((RectTransform)siyul.Find("SiyulName"), .04f, .035f, .96f, .115f);
        player.Find("PlayerName").GetComponent<TMP_Text>().text = "주인공";
        var placeholder = friend.transform.Find("SiyulSprite");
        placeholder.name = "FriendPlaceholder";
        Place((RectTransform)placeholder, .10f, .16f, .90f, .98f);
        var gray = placeholder.GetComponent<UnityEngine.UI.Image>();
        gray.sprite = null;
        gray.overrideSprite = null;
        gray.type = UnityEngine.UI.Image.Type.Simple;
        gray.color = new Color(.45f, .45f, .45f, 1);
        gray.preserveAspect = false;
        gray.raycastTarget = false;
        var friendName = friend.transform.Find("SiyulName");
        friendName.name = "FriendName";
        Place((RectTransform)friendName, .04f, .035f, .96f, .115f);
        friendName.GetComponent<TMP_Text>().text = "친구";
        call.Find("CallHeader").GetComponent<TMP_Text>().text = "●  화상 통화  ·  3명 접속";
        call.Find("DialogueBox").gameObject.SetActive(false);
        call.Find("EndCallHint").gameObject.SetActive(false);
        // Keep the close/drag controls, but remove the old click-to-advance surface.
        m.callAdvanceButton.enabled = false;
        friend.transform.SetSiblingIndex(siyul.GetSiblingIndex() + 1);
        m.callWindow.SetActive(false);
        EditorUtility.SetDirty(battle);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        return "Saved 3 preparation messages, call choice, and 3 video panes in Battle_Proto.";
    }

    static void Place(RectTransform r, float left, float bottom, float right, float top)
    {
        r.anchorMin = new Vector2(left, bottom);
        r.anchorMax = new Vector2(right, top);
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
}
