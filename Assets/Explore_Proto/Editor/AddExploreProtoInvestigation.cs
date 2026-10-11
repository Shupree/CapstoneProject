using System;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Adds the investigation UI to the existing saved scene once.</summary>
public static class AddExploreProtoInvestigation
{
    static readonly Color Ink = new Color(.20f, .18f, .17f);
    static TMP_FontAsset font;

    public static void Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity")
            throw new InvalidOperationException("Open Explore_Proto in edit mode first.");
        var messenger = UnityEngine.Object.FindFirstObjectByType<ExploreProtoMessenger>();
        if (!messenger || messenger.GetComponent<ExploreProtoInvestigation>())
            throw new InvalidOperationException("Investigation UI already exists, or the desktop is missing. Edit the saved objects.");
        var canvas = messenger.messengerWindow.transform.parent;
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Explore_Proto/Fonts/ExploreProtoKorean.asset");
        Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Add Explore Proto investigation UI");
        Undo.RecordObject(messenger, "Connect investigation UI");
        var investigation = Undo.AddComponent<ExploreProtoInvestigation>(messenger.gameObject);
        investigation.messenger = messenger;
        messenger.investigation = investigation;
        investigation.clues = new[]
        {
            new ExploreProtoInvestigation.Clue { id = "mary_weapon", memoText = "메리가 칼이나 도끼같은 흉기를 들고 있을 때에는 강령술을 하는 주체에게 위해가 가해졌다." },
            new ExploreProtoInvestigation.Clue { id = "mary_child", memoText = "과거 죽은 아기를 그리워하고 있다." },
            new ExploreProtoInvestigation.Clue { id = "mary_words", memoText = "\"네 아기는 죽지 않았어.\"라는 말에 기이한 반응을 보인다." },
            new ExploreProtoInvestigation.Clue { id = "mary_reflection", memoText = "메리가 마주본 자는 저주에 걸린다.\n(그렇다면 메리가 메리를 본다면 어떻게 될까?)" }
        };
        AddArticles(messenger);
        AddMemo(investigation);
        AddMonologue(canvas, investigation);
        AddFollowUp(messenger, investigation);
        AddArticleInteraction(messenger, investigation);
        EditorUtility.SetDirty(messenger);
        EditorUtility.SetDirty(investigation);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static RectTransform Rect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Add investigation UI element");
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }
    static void Area(RectTransform r, float x, float y, float w, float h, float parentW, float parentH)
    {
        r.anchorMin = new Vector2(x / parentW, 1 - (y + h) / parentH);
        r.anchorMax = new Vector2((x + w) / parentW, 1 - y / parentH);
        r.pivot = new Vector2(.5f, .5f); r.offsetMin = r.offsetMax = Vector2.zero;
    }
    static void Inset(RectTransform r, float left, float top, float right, float bottom)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.pivot = new Vector2(.5f, .5f);
        r.offsetMin = new Vector2(left, bottom); r.offsetMax = new Vector2(-right, -top);
    }
    static void Top(RectTransform r, float left, float top, float right, float height)
    {
        r.anchorMin = new Vector2(0, 1); r.anchorMax = Vector2.one; r.pivot = new Vector2(.5f, 1);
        r.anchoredPosition = new Vector2((left - right) * .5f, -top); r.sizeDelta = new Vector2(-left - right, height);
    }
    static UnityEngine.UI.Image Image(RectTransform r, Color color, bool raycast = false)
    {
        var image = r.gameObject.AddComponent<UnityEngine.UI.Image>();
        image.color = color; image.raycastTarget = raycast;
        return image;
    }
    static TMP_Text Label(Transform parent, string name, string value, float size, Color color)
    {
        var r = Rect(parent, name);
        var text = r.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font; text.fontSize = size; text.color = color; text.text = value;
        text.alignment = TextAlignmentOptions.TopLeft; text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Truncate; text.raycastTarget = false;
        text.lineSpacing = 5; text.paragraphSpacing = 12;
        return text;
    }
    static UnityEngine.UI.Button Button(RectTransform r, Color color)
    {
        var image = Image(r, color, true);
        var button = r.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = image;
        var navigation = button.navigation; navigation.mode = UnityEngine.UI.Navigation.Mode.None; button.navigation = navigation;
        return button;
    }
    static UnityEngine.UI.ScrollRect Scroll(Transform parent, string name)
    {
        var root = Rect(parent, name); Image(root, Color.clear, true);
        var scroll = root.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 70;
        var viewport = Rect(root, "Viewport"); Inset(viewport, 0, 0, 0, 0); Image(viewport, Color.white, true);
        viewport.gameObject.AddComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
        var content = Rect(viewport, "Content"); Top(content, 0, 0, 0, 0);
        var layout = content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
        layout.spacing = 20; layout.padding = new RectOffset(12, 12, 12, 28);
        var fitter = content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = content; scroll.viewport = viewport;
        return scroll;
    }

    static void AddArticles(ExploreProtoMessenger messenger)
    {
        string[] articles =
        {
            "이 게시판에도 블러디 메리 이야기가 자주 올라온다. 거울 앞에서 이름을 부른다는 줄거리는 같지만, 목격한 모습이나 그 뒤에 일어난 일은 글마다 다르다.\n\n어떤 글은 의식을 한 사람에게, 어떤 글은 거울을 들여다본 사람에게 일이 생겼다고 한다. 그냥 ‘무서운 이야기’로 넘기기에는 서로 겹치는 기록이 많다.\n\n관련 글을 읽을 때는 메리의 모습, 행동, 반응을 따로 정리해 보자. 흩어진 이야기를 비교하면 공통점을 찾을 수 있을 것이다.",
            "블러디 메리 강령술에 관한 오래된 글들을 모았다. 이름을 부르는 횟수나 준비물은 기록마다 달라, 이 글에서는 의식 뒤에 나타난 현상에 집중한다.\n\n목격담에서 특히 반복되는 것은 거울 속 메리의 손에 들린 물건이었다. 아무것도 들지 않았다는 글도 있었지만, 무언가 들고 나타났다는 글들은 결말이 비슷했다.\n\n<link=\"mary_weapon\">메리가 칼이나 도끼같은 흉기를 들고 있을 때에는 강령술을 하는 주체에게 위해가 가해졌다.</link>\n\n의식의 방식만 확인하고 이 대목을 놓친 사람들이 많았다. 메리가 나타났다면 무엇을 들고 있는지부터 살펴볼 필요가 있다.",
            "블러디 메리라는 이름으로 전해지는 이야기에는 여러 모습의 여인이 등장한다. 그중 한 전승에서는 여인이 텅 빈 요람 앞을 떠나지 못했다고 한다.\n\n아이를 안듯이 허공을 끌어안거나, 누군가를 재우는 듯한 노랫소리를 냈다는 기록도 남아 있다.\n\n<link=\"mary_child\">과거 죽은 아기를 그리워하고 있다.</link>\n\n이 전승의 메리는 단순히 거울을 떠도는 형체가 아니었다. 잃어버린 누군가를 계속 찾고 있다는 대목이 다른 기록과 구별된다.",
            "낡은 의식 기록의 여백에는 이름을 부르는 절차보다, 메리가 나타난 뒤 오간 말이 더 자세히 적혀 있었다. 처음에는 어떤 물음에도 대답하지 않았다고 한다.\n\n그러나 한 문장만은 반응을 바꾸었다. 글쓴이는 그 말을 듣자 메리의 움직임이 멎고, 목소리가 들린 쪽으로 고개를 돌렸다고 기록했다.\n\n<link=\"mary_words\">\"네 아기는 죽지 않았어.\"라는 말에 기이한 반응을 보인다.</link>\n\n뒷부분은 찢겨 있어 그 다음에 어떤 일이 있었는지는 알 수 없다. 다만 이 문장에는 여러 번 밑줄이 그어져 있었다.",
            "처음엔 친구가 장난을 치는 줄 알았다. 강령회가 끝났는데도 혼자 남아 거울을 보고 있었고, 말을 걸어도 한참 뒤에야 돌아봤다.\n\n다음 날부터 평소와 다른 말투를 쓰기 시작했다. 사진 속 자기 얼굴을 오래 들여다보는 것도 이상했다.\n\n이 글을 쓰는 지금도 그날 들었던 노랫소리가 기억난다. 다른 사람도 비슷한 일을 겪었다면 당시 상황을 자세히 남겨 주었으면 한다.",
            "지역별 거울 괴담을 비교하다 보면, 이름을 부른 사람과 메리를 마주본 사람이 서로 다른 경우가 나온다. 이 둘을 같은 사람으로 생각하면 기록의 앞뒤가 맞지 않는다.\n\n한 전승은 저주가 누구에게 옮겨 가는지 다음과 같이 설명한다.\n\n<link=\"mary_reflection\">메리가 마주본 자는 저주에 걸린다.</link>\n\n남겨진 삽화에는 사람과 거울의 위치가 표시되어 있었다. 저주를 이해하려면 누가 누구를 보고 있었는지를 살펴야 할 것 같다.",
            "친구들과 거울 괴담을 따라 한 뒤부터 한 친구가 이상해졌어요. 처음에는 별일 아닌 줄 알았는데, 이제는 저희를 알아보지 못하는 것 같아요.\n\n가끔은 처음 듣는 목소리로 말을 해요. 무슨 말을 하는지는 제대로 못 들었어요. 강령회 때 봤던 글을 다시 찾으려고 해도 비슷한 제목이 너무 많네요.\n\n당시 친구가 무엇을 봤는지, 무슨 말을 들었는지부터 기억을 맞춰 보려 합니다. 같은 의식을 했던 사람들의 기록을 찾고 있어요."
        };
        for (int i = 0; i < articles.Length; i++)
        {
            var site = messenger.siteEntries[i]; site.article = articles[i]; messenger.siteEntries[i] = site;
        }
    }

    static void AddMemo(ExploreProtoInvestigation investigation)
    {
        var body = Rect(investigation.messenger.memoWindow.transform, "CollectedClues");
        Area(body, 76, 198, 830, 472, 981, 772);
        body.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
        var notes = Label(body, "MemoNotes", "", 40, Ink);
        Inset((RectTransform)notes.transform, 0, 0, 0, 0);
        notes.lineSpacing = 8; notes.paragraphSpacing = 14;
        investigation.memoNotes = notes;
    }

    static void AddMonologue(Transform canvas, ExploreProtoInvestigation investigation)
    {
        var root = Rect(canvas, "ResearchCompleteMonologue");
        root.gameObject.SetActive(false); Inset(root, 0, 0, 0, 0);
        var dismiss = Button(root, Color.clear); dismiss.transition = UnityEngine.UI.Selectable.Transition.None;
        var panel = Rect(root, "DialoguePanel");
        panel.anchorMin = Vector2.zero; panel.anchorMax = new Vector2(1, 0); panel.pivot = new Vector2(.5f, 0);
        panel.sizeDelta = new Vector2(0, 390); panel.anchoredPosition = Vector2.zero;
        Image(panel, new Color(.06f, .065f, .08f, .95f));
        var stripe = Rect(panel, "TopAccent"); Top(stripe, 0, 0, 0, 4); Image(stripe, new Color(.19f, .71f, .68f));
        var speaker = Label(panel, "Speaker", "주인공 · 속마음", 34, new Color(.49f, .89f, .85f));
        Top((RectTransform)speaker.transform, 140, 36, 140, 60);
        var text = Label(panel, "Monologue", "좋았어. 이정도면 블러디 메리 강령술에 대한 정보는 충분할 것 같아. 시율이에게 다시 연락하자.", 44, Color.white);
        Top((RectTransform)text.transform, 140, 128, 140, 172);
        var hint = Label(panel, "DismissHint", "클릭하여 계속", 28, new Color(.74f, .77f, .80f));
        Top((RectTransform)hint.transform, 140, 318, 140, 42); hint.alignment = TextAlignmentOptions.Right;
        investigation.monologueWindow = root.gameObject; investigation.monologueDismissButton = dismiss;
    }

    static void AddFollowUp(ExploreProtoMessenger messenger, ExploreProtoInvestigation investigation)
    {
        var chat = messenger.messengerWindow.transform.Find("ConversationPane");
        var scroll = Scroll(chat, "ConversationScroll");
        Area((RectTransform)scroll.transform, 0, 110, 1550, 718, 1550, 949);
        messenger.conversationScroll = scroll;
        var history = Rect(scroll.content, "InitialConversation");
        var historyHeight = history.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
        historyHeight.minHeight = historyHeight.preferredHeight = 1040;
        string[] names = { "SiyulChatAvatar", "FirstIncomingBubble", "CallRequestBubble", "StartVideoCall", "VideoCallRecording", "LinkMessage", "AddBloodyMaryKeyword" };
        var boxes = new[]
        {
            new Rect(24, 30, 80, 100), new Rect(140, 20, 1110, 180), new Rect(140, 232, 1110, 125),
            new Rect(1080, 395, 440, 90), new Rect(1080, 395, 440, 90), new Rect(320, 512, 1200, 95), new Rect(900, 641, 620, 90)
        };
        for (int i = 0; i < names.Length; i++)
        {
            var item = (RectTransform)chat.Find(names[i]);
            Undo.SetTransformParent(item, history, "Place existing messages in scroll history");
            var box = boxes[i]; Area(item, box.x, box.y, box.width, box.height, 1550, 800);
        }

        string[] lines =
        {
            "호..혹시 강령술에 대한 정보는 다 찾으셨나요?",
            "네! 필요한 정보는 다 찾았어요. 메리가 무엇에 반응하는지도 알아냈고요. 이제 친구분을 되찾을 방법을 같이 시도해 봐요.",
            "정말요? 친구가 자꾸 혼잣말을 해요... 그래도 방법이 있다니 조금 안심돼요. 어떻게 하면 될까요?",
            "시율 씨와 귀신이 씌인 친구분, 그리고 제가 함께 다시 블러디 메리 강령술을 시도해 보려고 해요. 저는 화상 통화로 상황을 보면서 도울게요.",
            "또 강령술을 하는 건 무서워요... 그래도 친구를 이대로 둘 수는 없어요. 저도 함께할게요.",
            "고마워요. 이번에는 모은 정보를 확인하면서 차근차근 진행해요. 친구분과 함께 기다려 주세요. 준비가 되면 제가 연락해서 시작 신호를 보낼게요!",
            "알겠어요. 친구와 함께 기다릴게요. 꼭 다시 연락해 주세요."
        };
        investigation.followUpMessages = new GameObject[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            var row = Rect(scroll.content, "ResearchMessage_" + i);
            var layout = row.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            layout.minHeight = layout.preferredHeight = i == 0 ? 190 : 280;
            bool outgoing = i % 2 == 1;
            var bubble = Rect(row, "Bubble");
            Area(bubble, outgoing ? .22f : .025f, .02f, .755f, .96f, 1, 1);
            Image(bubble, outgoing ? new Color(.24f, .45f, .59f) : new Color(.88f, .91f, .93f));
            var name = Label(bubble, "Speaker", outgoing ? "나" : "시율", 28,
                outgoing ? new Color(.78f, .90f, .94f) : new Color(.36f, .42f, .46f));
            Top((RectTransform)name.transform, 30, 20, 30, 42);
            var line = Label(bubble, "Message", lines[i], 36, outgoing ? Color.white : Ink);
            Inset((RectTransform)line.transform, 30, 74, 30, 24);
            row.gameObject.SetActive(false); investigation.followUpMessages[i] = row.gameObject;
        }
        var reply = Rect(chat, "ResearchReply");
        Area(reply, 1010, 852, 510, 78, 1550, 949);
        investigation.replyButton = Button(reply, new Color(.24f, .45f, .59f));
        investigation.replyLabel = Label(reply, "Label", "조사 결과 전하기", 34, Color.white);
        Inset((RectTransform)investigation.replyLabel.transform, 24, 12, 24, 12);
        investigation.replyLabel.alignment = TextAlignmentOptions.Center;
        reply.gameObject.SetActive(false);
        investigation.replyLabels = new[] { "조사 결과 전하기", "다시 강령술을 제안하기", "시율과 약속하기" };
        var hint = Label(chat, "ConversationScrollHint", "마우스 휠로 이전 대화를 볼 수 있어요.", 26, new Color(.49f, .44f, .40f));
        Area((RectTransform)hint.transform, 24, 866, 900, 46, 1550, 949);
        var contact = messenger.messengerWindow.transform.Find("ContactPane/SiyulContact");
        investigation.contactPreview = contact.Find("ContactPreview").GetComponent<TMP_Text>();
        var contactButton = contact.gameObject.AddComponent<UnityEngine.UI.Button>();
        contactButton.targetGraphic = contact.GetComponent<UnityEngine.UI.Image>();
        contactButton.targetGraphic.raycastTarget = true;
        UnityEventTools.AddPersistentListener(contactButton.onClick, messenger.ShowMessenger);
    }

    static void AddArticleInteraction(ExploreProtoMessenger messenger, ExploreProtoInvestigation investigation)
    {
        var hint = Label(messenger.siteDetailPanel.transform, "ClueHint", "문장 위에 마우스를 올려 단서를 살펴보세요.  0/4", 28, new Color(.40f, .44f, .41f));
        var r = (RectTransform)hint.transform;
        r.anchorMin = Vector2.zero; r.anchorMax = new Vector2(1, 0); r.pivot = new Vector2(.5f, 0);
        r.anchoredPosition = new Vector2(230, 26); r.sizeDelta = new Vector2(-660, 86);
        hint.alignment = TextAlignmentOptions.Right;
        investigation.articleHint = hint;
        var links = messenger.siteArticleText.gameObject.AddComponent<ExploreProtoArticleLinks>();
        links.article = messenger.siteArticleText; links.articleScroll = messenger.siteArticleScroll; links.investigation = investigation;
        messenger.siteArticleText.raycastTarget = true;
        messenger.articleLinks = links;
    }
}
