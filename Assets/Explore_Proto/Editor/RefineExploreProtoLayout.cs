using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Edits the existing scene objects; never rebuilds the desktop at runtime.</summary>
public static class RefineExploreProtoLayout
{
    const float FrameWidth = 2048f, FrameHeight = 1147f;
    static readonly Color Ink = new Color(.20f, .18f, .17f);
    static readonly Color Muted = new Color(.48f, .40f, .39f);

    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before editing the scene.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/Explore_Proto.unity")
            throw new InvalidOperationException("Open Explore_Proto before editing its layout.");
        var canvas = GameObject.Find("ExploreProtoCanvas");
        var ctrl = UnityEngine.Object.FindFirstObjectByType<ExploreProtoMessenger>();
        if (!canvas || !ctrl) throw new InvalidOperationException("Explore_Proto UI was not found.");
        Undo.RegisterFullObjectHierarchyUndo(canvas, "Refine Explore Proto window layout");
        Undo.RecordObject(ctrl, "Update Explore Proto UI references");

        LayoutMessenger(ctrl);
        LayoutMemo(ctrl);
        LayoutInternet(ctrl);
        LayoutCall(ctrl);
        LayoutTranscript(ctrl);
        LayoutPopup(ctrl);

        // Keep the initial scene state. Runtime code only changes visibility and content.
        EditorUtility.SetDirty(ctrl);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static RectTransform At(Transform parent, string path)
    {
        var child = parent.Find(path);
        if (!child) throw new InvalidOperationException("Missing UI: " + parent.name + "/" + path);
        return (RectTransform)child;
    }

    static RectTransform EnsureRect(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (child) return (RectTransform)child;
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Add UI content container");
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static T Ensure<T>(GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        return component ? component : Undo.AddComponent<T>(go);
    }

    // Bounds are specified from the top left of the supplied reference rectangle.
    // Fractional anchors keep the content aligned with the artwork when the frame changes size.
    static void Area(RectTransform r, float x, float y, float w, float h, float parentW, float parentH)
    {
        r.anchorMin = new Vector2(x / parentW, 1f - (y + h) / parentH);
        r.anchorMax = new Vector2((x + w) / parentW, 1f - y / parentH);
        r.pivot = new Vector2(.5f, .5f);
        r.offsetMin = r.offsetMax = Vector2.zero;
        r.localScale = Vector3.one;
        r.localRotation = Quaternion.identity;
    }

    static void FrameArea(Transform parent, string path, float x, float y, float w, float h)
        => Area(At(parent, path), x, y, w, h, FrameWidth, FrameHeight);

    static void Inset(RectTransform r, float left, float top, float right, float bottom)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.pivot = new Vector2(.5f, .5f);
        r.offsetMin = new Vector2(left, bottom); r.offsetMax = new Vector2(-right, -top);
        r.localScale = Vector3.one;
    }

    static void TopRow(RectTransform r, float left, float top, float right, float height)
    {
        r.anchorMin = new Vector2(0, 1); r.anchorMax = Vector2.one; r.pivot = new Vector2(.5f, 1);
        r.anchoredPosition = new Vector2((left - right) * .5f, -top);
        r.sizeDelta = new Vector2(-left - right, height); r.localScale = Vector3.one;
    }

    static void Window(RectTransform r, Vector2 size, Vector2 position)
    {
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f, .5f);
        r.sizeDelta = size; r.anchoredPosition = position; r.localScale = Vector3.one;
        var image = r.GetComponent<UnityEngine.UI.Image>();
        if (image) { image.raycastTarget = true; image.preserveAspect = true; }
    }

    static TMP_Text Text(RectTransform r, float size, TextAlignmentOptions align = TextAlignmentOptions.Left)
    {
        var text = r.GetComponent<TMP_Text>();
        text.fontSize = size; text.enableAutoSizing = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Truncate;
        text.alignment = align; text.margin = Vector4.zero;
        text.lineSpacing = 4; text.paragraphSpacing = 10; text.raycastTarget = false;
        return text;
    }

    static void ButtonLabel(Transform button, float font = 36)
    {
        var label = At(button, "Label");
        Inset(label, 24, 12, 24, 12);
        Text(label, font, TextAlignmentOptions.Center);
    }

    static void Clip(Transform t) => Ensure<UnityEngine.UI.RectMask2D>(t.gameObject);

    static void LayoutMessenger(ExploreProtoMessenger ctrl)
    {
        var window = (RectTransform)ctrl.messengerWindow.transform;
        Window(window, new Vector2(2680, 2680 * FrameHeight / FrameWidth), new Vector2(180, 35));
        FrameArea(window, "TitleBar", 0, 0, 2048, 100);
        Area(At(window, "TitleBar/CloseMessenger"), 1893, 17, 61, 61, 2048, 100);
        At(window, "TitleBar/AppTitle").gameObject.SetActive(false);

        FrameArea(window, "ContactPane", 46, 210, 330, 845);
        var contactPane = At(window, "ContactPane"); Clip(contactPane);
        Area(At(contactPane, "SiyulContact"), 0, 12, 330, 168, 330, 845);
        var contact = At(contactPane, "SiyulContact");
        Area(At(contact, "SiyulAvatar"), 16, 24, 72, 106, 330, 168);
        Area(At(contact, "ContactName"), 105, 22, 209, 40, 330, 168);
        Area(At(contact, "ContactPreview"), 105, 74, 209, 76, 330, 168);
        Text(At(contact, "ContactName"), 40);
        Text(At(contact, "ContactPreview"), 28, TextAlignmentOptions.TopLeft);

        FrameArea(window, "ConversationPane", 414, 113, 1550, 949);
        var chat = At(window, "ConversationPane"); Clip(chat);
        Area(At(chat, "ConversationHeader"), 24, 12, 1480, 54, 1550, 949);
        Text(At(chat, "ConversationHeader"), 38);
        Area(At(chat, "SiyulChatAvatar"), 24, 140, 80, 100, 1550, 949);
        Area(At(chat, "FirstIncomingBubble"), 140, 130, 1110, 180, 1550, 949);
        Inset(At(chat, "FirstIncomingBubble/FirstMessage"), 30, 24, 30, 24);
        Text(At(chat, "FirstIncomingBubble/FirstMessage"), 38);
        Area(At(chat, "CallRequestBubble"), 140, 342, 1110, 125, 1550, 949);
        Inset(At(chat, "CallRequestBubble/CallRequest"), 30, 22, 30, 22);
        Text(At(chat, "CallRequestBubble/CallRequest"), 36);

        // The call action is replaced by the outgoing recording link at the same location.
        Area(At(chat, "StartVideoCall"), 1080, 505, 440, 90, 1550, 949);
        Area(At(chat, "VideoCallRecording"), 1080, 505, 440, 90, 1550, 949);
        ButtonLabel(At(chat, "StartVideoCall"));
        ButtonLabel(At(chat, "VideoCallRecording"));
        Area(At(chat, "LinkMessage"), 320, 622, 1200, 95, 1550, 949);
        Text(At(chat, "LinkMessage"), 36, TextAlignmentOptions.Right);
        Area(At(chat, "AddBloodyMaryKeyword"), 900, 751, 620, 90, 1550, 949);
        ButtonLabel(At(chat, "AddBloodyMaryKeyword"), 34);
    }

    static void LayoutMemo(ExploreProtoMessenger ctrl)
    {
        var window = (RectTransform)ctrl.memoWindow.transform;
        var sprite = window.GetComponent<UnityEngine.UI.Image>().sprite;
        Window(window, sprite.rect.size * 1.5f, new Vector2(540, -20));
        Area(At(window, "MemoTitleDragArea"), 22, 4, 724, 58, 981, 772);
        Area(At(window, "MemoCloseX"), 875, 10, 44, 44, 981, 772);
        Area((RectTransform)ctrl.memoKeyword.transform, 92, 116, 146, 44, 981, 772);
        Text((RectTransform)ctrl.memoKeyword.transform, 32, TextAlignmentOptions.Center);
    }

    static void LayoutInternet(ExploreProtoMessenger ctrl)
    {
        var window = (RectTransform)ctrl.internetWindow.transform;
        Window(window, new Vector2(2680, 2680 * FrameHeight / FrameWidth), new Vector2(180, 35));
        FrameArea(window, "InternetTitleDragArea", 32, 10, 1660, 78);
        FrameArea(window, "InternetCloseX", 1893, 17, 61, 61);
        FrameArea(window, "SearchFieldOutline", 340, 170, 1368, 102);
        FrameArea(window, "KeywordFieldHitArea", 434, 178, 1250, 86);
        ctrl.keywordFieldButton.transition = UnityEngine.UI.Selectable.Transition.None;
        FrameArea(window, "Icon_Find", 365, 195, 50, 50);
        FrameArea(window, "SearchKeywordChip", 442, 182, 300, 78);
        var chip = At(window, "SearchKeywordChip");
        chip.GetComponent<UnityEngine.UI.Image>().preserveAspect = false;
        Area((RectTransform)ctrl.searchKeywordText.transform, 16, 8, 222, 62, 300, 78);
        Text((RectTransform)ctrl.searchKeywordText.transform, 34, TextAlignmentOptions.Center);
        FrameArea(window, "ClearKeyword", 689, 204, 34, 34);
        ctrl.clearKeywordButton.GetComponent<UnityEngine.UI.Image>().color = Color.white;
        FrameArea(window, "SearchFieldPlaceholder", 455, 185, 1120, 72);
        Text(At(window, "SearchFieldPlaceholder"), 34);
        FrameArea(window, "SearchHint", 350, 292, 1348, 62);
        Text(At(window, "SearchHint"), 34);

        FrameArea(window, "SearchResults", 112, 385, 1824, 655);
        var scroll = ctrl.searchResults.GetComponent<UnityEngine.UI.ScrollRect>();
        scroll.scrollSensitivity = 70;
        Inset(scroll.viewport, 0, 0, 0, 0);
        var viewportImage = scroll.viewport.GetComponent<UnityEngine.UI.Image>();
        viewportImage.color = Color.white; viewportImage.raycastTarget = true;
        scroll.viewport.GetComponent<UnityEngine.UI.Mask>().showMaskGraphic = false;
        var content = scroll.content;
        TopRow(content, 0, 0, 0, 0);
        var layout = content.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 12, 30); layout.spacing = 18;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
        foreach (var site in ctrl.siteEntries)
        {
            var row = (RectTransform)site.button.transform;
            var element = row.GetComponent<UnityEngine.UI.LayoutElement>();
            element.minHeight = element.preferredHeight = 230; element.flexibleHeight = 0;
            TopRow(At(row, "Category"), 220, 18, 70, 42);
            TopRow(At(row, "Title"), 220, 75, 70, 60);
            TopRow(At(row, "Summary"), 220, 150, 70, 60);
            Text(At(row, "Category"), 30).color = Muted;
            Text(At(row, "Title"), 42).color = Ink;
            Text(At(row, "Summary"), 32).color = Ink;
            Area(At(row, "Divider"), .02f, .99f, .96f, .005f, 1, 1);
        }

        FrameArea(window, "SiteDetailPanel", 112, 385, 1824, 655);
        var detail = (RectTransform)ctrl.siteDetailPanel.transform; Clip(detail);
        TopRow((RectTransform)ctrl.siteCategoryText.transform, 100, 20, 100, 55);
        TopRow((RectTransform)ctrl.siteTitleText.transform, 100, 85, 100, 100);
        Text((RectTransform)ctrl.siteCategoryText.transform, 30).color = Muted;
        Text((RectTransform)ctrl.siteTitleText.transform, 48).color = Ink;
        var articleScroll = TextScroll(detail, "ArticleScroll", ctrl.siteArticleText, 40, Ink);
        Inset((RectTransform)articleScroll.transform, 100, 215, 100, 140);
        ctrl.siteArticleScroll = articleScroll;
        var back = (RectTransform)ctrl.siteBackButton.transform;
        back.anchorMin = back.anchorMax = back.pivot = new Vector2(0, 0);
        back.anchoredPosition = new Vector2(100, 26); back.sizeDelta = new Vector2(400, 86);
        ButtonLabel(back, 34);
    }

    static void LayoutCall(ExploreProtoMessenger ctrl)
    {
        var window = (RectTransform)ctrl.callWindow.transform;
        Window(window, new Vector2(2680, 1500), new Vector2(180, 35));
        Area(At(window, "CallHeader"), 48, 22, 2330, 80, 2680, 1500);
        Text(At(window, "CallHeader"), 40);
        Area(At(window, "CallTitleDragArea"), 20, 10, 2470, 110, 2680, 1500);
        Area(At(window, "CallCloseX"), 2535, 22, 100, 100, 2680, 1500);
        ButtonLabel(At(window, "CallCloseX"), 60);
        At(window, "CallCloseX/Label").GetComponent<TMP_Text>().color = Color.white;
        ctrl.callAdvanceButton.transition = UnityEngine.UI.Selectable.Transition.None;

        Area(At(window, "PlayerVideoPane"), 50, 170, 1265, 680, 2680, 1500);
        Area(At(window, "SiyulVideoPane"), 1365, 170, 1265, 680, 2680, 1500);
        foreach (var person in new[] { "Player", "Siyul" })
        {
            var pane = At(window, person + "VideoPane"); Clip(pane);
            Area(At(pane, person + "Sprite"), 390, 15, 485, 650, 1265, 680);
            Area(At(pane, person + "Name"), 38, 588, 300, 60, 1265, 680);
            Text(At(pane, person + "Name"), 38).color = person == "Player" ? Ink : Color.white;
        }
        Area(At(window, "DialogueBox"), 100, 920, 2480, 400, 2680, 1500);
        var dialog = At(window, "DialogueBox"); Clip(dialog);
        Area(At(dialog, "Speaker"), 38, 26, 2350, 68, 2480, 400);
        Text(At(dialog, "Speaker"), 36);
        Area(At(dialog, "DialogueText"), 38, 116, 2404, 236, 2480, 400);
        Text(At(dialog, "DialogueText"), 42, TextAlignmentOptions.TopLeft);
        Area(At(window, "EndCallHint"), 865, 1370, 950, 80, 2680, 1500);
        ButtonLabel(At(window, "EndCallHint"), 30);
    }

    static void LayoutTranscript(ExploreProtoMessenger ctrl)
    {
        var window = (RectTransform)ctrl.transcriptWindow.transform;
        Window(window, new Vector2(2040, 1320), new Vector2(270, 20));
        Area(At(window, "TranscriptTitle"), 60, 26, 1740, 90, 2040, 1320);
        Text(At(window, "TranscriptTitle"), 46);
        Area(At(window, "TranscriptTitleDragArea"), 20, 12, 1820, 110, 2040, 1320);
        Area(At(window, "TranscriptCloseX"), 1870, 25, 100, 100, 2040, 1320);
        ButtonLabel(At(window, "TranscriptCloseX"), 60);
        At(window, "TranscriptCloseX/Label").GetComponent<TMP_Text>().color = Color.white;
        ctrl.transcriptTitleCloseButton = At(window, "TranscriptCloseX").GetComponent<UnityEngine.UI.Button>();
        ctrl.transcriptScroll = TextScroll(window, "TranscriptScroll", ctrl.transcriptText, 36, new Color(.92f, .94f, .97f));
        Area((RectTransform)ctrl.transcriptScroll.transform, 60, 160, 1920, 960, 2040, 1320);
        Area(At(window, "CloseTranscript"), 850, 1170, 340, 95, 2040, 1320);
        ButtonLabel(At(window, "CloseTranscript"), 34);
    }

    static UnityEngine.UI.ScrollRect TextScroll(Transform parent, string name, TMP_Text text, float font, Color color)
    {
        var root = EnsureRect(parent, name);
        var graphic = Ensure<UnityEngine.UI.Image>(root.gameObject);
        graphic.color = Color.clear; graphic.raycastTarget = true;
        var scroll = Ensure<UnityEngine.UI.ScrollRect>(root.gameObject);
        scroll.horizontal = false; scroll.vertical = true;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 70;
        var viewport = EnsureRect(root, "Viewport"); Inset(viewport, 0, 0, 0, 0);
        var maskImage = Ensure<UnityEngine.UI.Image>(viewport.gameObject);
        maskImage.color = Color.white; maskImage.raycastTarget = true;
        Ensure<UnityEngine.UI.Mask>(viewport.gameObject).showMaskGraphic = false;
        var content = EnsureRect(viewport, "Content"); TopRow(content, 0, 0, 0, 0);
        var layout = Ensure<UnityEngine.UI.VerticalLayoutGroup>(content.gameObject);
        layout.padding = new RectOffset(8, 24, 8, 28);
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
        var fitter = Ensure<UnityEngine.UI.ContentSizeFitter>(content.gameObject);
        fitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        text.transform.SetParent(content, false);
        Text((RectTransform)text.transform, font, TextAlignmentOptions.TopLeft).color = color;
        text.overflowMode = TextOverflowModes.Overflow;
        scroll.viewport = viewport; scroll.content = content;
        return scroll;
    }

    static void LayoutPopup(ExploreProtoMessenger ctrl)
    {
        var window = (RectTransform)ctrl.keywordPopup.transform;
        Window(window, new Vector2(740, 410), new Vector2(-250, 80));
        Inset(At(window, "PopupBackground"), 0, 0, 0, 0);
        At(window, "PopupBackground").GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
        Area(At(window, "PopupTitle"), 32, 24, 560, 76, 740, 410);
        Text(At(window, "PopupTitle"), 38);
        Area(At(window, "PopupClose"), 638, 24, 76, 76, 740, 410);
        ButtonLabel(At(window, "PopupClose"), 40);
        Area(At(window, "PopupKeyword"), 150, 164, 440, 142, 740, 410);
        ButtonLabel(At(window, "PopupKeyword"), 36);
        Area(At(window, "PopupDragArea"), 12, 12, 602, 100, 740, 410);
    }
}
