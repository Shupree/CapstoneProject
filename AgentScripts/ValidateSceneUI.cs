using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CyberExorcist;
using TMPro;
using UnityEngine;

public static class ValidateSceneUI
{
    static CyberExorcistPrototype Game => UnityEngine.Object.FindFirstObjectByType<CyberExorcistPrototype>();
    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    static void Click(string id)
    {
        var game = Game;
        var button = game.GetComponentsInChildren<UnityEngine.UI.Button>(true)
            .FirstOrDefault(b => b.name == id && b.gameObject.activeInHierarchy && b.interactable);
        Require(button, "No active button: " + id);
        Require(button.onClick.GetPersistentEventCount() == 1, "Missing saved event: " + id);
        Require(button.onClick.GetPersistentTarget(0) == game, "Wrong event target: " + id);
        button.onClick.Invoke();
    }
    // TMP can create its own fallback-material submeshes while rendering a new glyph.
    // They are renderer internals, not authored game UI.
    static Transform[] AuthoredObjects() => Game.GetComponentsInChildren<Transform>(true).Where(x => !x.GetComponent<TMP_SubMeshUI>()).ToArray();
    static int[] Ids() => AuthoredObjects().Select(x => x.GetInstanceID()).OrderBy(x => x).ToArray();

    public static object Investigate()
    {
        Require(Application.isPlaying, "Enter Play mode first.");
        var g = Game;
        var ids = Ids();
        var checks = new List<string>();
        Require(g.ContactAdded && g.CurrentApp == "chat", "Real pointer contact click did not arrive.");
        checks.Add("Input System pointer: contact added");
        Click("restart"); Click("cancel-reset");
        Require(g.ContactAdded, "Cancel restart lost state.");
        Click("restart"); Click("confirm-reset");
        Require(!g.ContactAdded && g.CurrentApp == "mail", "Restart failed.");
        var title = g.UI.AppPanels[0].GetComponentsInChildren<TextMeshProUGUI>().First(x => x.text == "누군가 제 화면을 보고 있어요");
        var oldText = title.text;
        var oldPosition = title.rectTransform.anchoredPosition;
        title.text = "씬에서 편집한 제목";
        title.rectTransform.anchoredPosition += new Vector2(3, 2);
        Click("app:chat");
        Require(g.UI.ChatEmpty.activeInHierarchy, "Empty chat missing.");
        Click("app:browser"); Click("search");
        Require(g.BrowserPage == "home", "Search without keywords unlocked.");
        g.Handle("enter-site");
        Require(g.BrowserPage == "home", "Site unlocked early.");
        Click("app:mail"); Click("contact");
        Click("chat-next"); Click("chat-next");
        Require(g.KeywordAcquired, "Keywords not acquired.");
        Click("chat-next"); Click("chat-next");
        Click("term-a"); Click("search");
        Require(g.BrowserPage == "home", "Single keyword unlocked search.");
        Click("term-b"); Click("search");
        Require(g.BrowserPage == "results" && g.UI.SearchResults.activeInHierarchy, "Results missing.");
        Click("nav:side"); Click("back");
        Require(g.BrowserPage == "results", "Information back failed.");
        Click("nav:article");
        Require(!g.UI.EnterSite.interactable, "Site button enabled before clue.");
        Click("clue"); Click("enter-site");
        Require(g.ClueFound && g.UI.HauntedPhases[0].activeInHierarchy, "Isolation screen missing.");
        checks.Add("Keywords, result/back navigation and clue lock passed");
        Click("isolate"); Click("camera:0"); Click("camera:1");
        Require(g.CamerasRemoved == 2 && !g.UI.Cameras[0].activeSelf && g.UI.Glimpse.activeSelf, "Camera visibility wrong.");
        Click("app:mail");
        Require(title.text == "씬에서 편집한 제목" && title.rectTransform.anchoredPosition == oldPosition + new Vector2(3, 2), "Refresh overwrote authored UI.");
        title.text = oldText; title.rectTransform.anchoredPosition = oldPosition;
        Click("app:browser");
        Require(g.CamerasRemoved == 2, "App switch lost camera state.");
        Click("minimize");
        Require(!g.UI.Window.gameObject.activeSelf, "Minimize failed.");
        Click("app:browser");
        Require(g.UI.Window.gameObject.activeSelf, "Restore failed.");
        Click("mute"); Require(g.Muted, "Mute failed.");
        Click("mute"); Require(!g.Muted, "Unmute failed.");
        Click("camera:2"); Click("camera:3"); Click("camera:4");
        Require(g.CamerasRemoved == 5 && g.UI.HauntedPhases[2].activeInHierarchy, "Ghost did not appear.");
        Click("ghost-next"); Click("ghost-next"); Click("ghost-next");
        Require(g.GhostDialogue == 3, "Ghost conversation failed.");
        Require(ids.SequenceEqual(Ids()), "Runtime created, replaced or destroyed scene objects.");
        checks.Add("Five cameras, dialogue, app state, minimize/restore and mute passed");
        checks.Add("All scene object identities and edited title/layout preserved");
        return new { checks, objectCount = ids.Length, snapshot = Snapshot() };
    }

    public static async Task<object> Complete()
    {
        var g = Game;
        var ids = Ids();
        Click("ghost-next");
        Require(g.Departing && !g.UI.GhostNext.interactable, "Departure did not lock input.");
        g.Handle("restart");
        Require(!g.UI.RestartDialog.activeSelf, "Departure allowed restart.");
        await Task.Delay(2400);
        Require(g.Completed && !g.Departing && g.UI.HauntedPhases[3].activeInHierarchy, "Departure did not complete.");
        var ending = Snapshot();
        Click("app:chat");
        Require(g.UI.ChatStages[4].activeInHierarchy, "Thank-you message missing.");
        Click("report");
        Require(g.UI.HauntedPhases[3].activeInHierarchy, "Report failed.");
        Click("restart"); Click("cancel-reset");
        Require(g.Completed, "Cancel reset lost completion.");
        Click("restart"); Click("confirm-reset");
        Require(!g.Completed && !g.ContactAdded && !g.KeywordAcquired && !g.ClueFound &&
            g.CamerasRemoved == 0 && g.GhostDialogue == 0 && !g.Isolated && !g.CanGoBack &&
            !g.TermASelected && !g.TermBSelected && g.UI.GhostGroup.alpha == 1,
            "Reset left stale state.");
        Require(g.UI.Cameras.All(x => x.activeSelf), "Reset did not restore cameras.");
        Require(ids.SequenceEqual(Ids()), "Completion/reset replaced scene objects.");
        return new { checks = "Departure, input lock, ending, thanks, cancel/reset and stable object identities passed", ending, afterReset = Snapshot() };
    }

    public static object Snapshot()
    {
        var g = Game;
        Canvas.ForceUpdateCanvases();
        var texts = g.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var t in texts) t.ForceMeshUpdate();
        return new
        {
            g.CurrentApp, g.BrowserPage, g.Completed, g.GhostDialogue,
            objectCount = g.GetComponentsInChildren<Transform>(true).Length,
            eventSystems = UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length,
            overflow = texts.Where(t => t.isTextOverflowing).Select(t => new { t.name, t.text }).ToArray()
        };
    }

    public static object StartupAndDrag()
    {
        var g = Game;
        Require(Application.isPlaying && g.CurrentApp == "mail" && !g.ContactAdded && !g.UI.RestartDialog.activeSelf,
            "Editor preview leaked into initial gameplay.");
        var window = g.UI.Window;
        var initial = window.anchoredPosition;
        var drag = g.GetComponentInChildren<PrototypeWindowDrag>();
        var rect = (RectTransform)drag.transform;
        var point = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
        var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        { position = point, button = UnityEngine.EventSystems.PointerEventData.InputButton.Left };
        var hits = new List<UnityEngine.EventSystems.RaycastResult>();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
        Require(hits.Count > 0, "Title bar does not receive UI raycasts.");
        var receiver = UnityEngine.EventSystems.ExecuteEvents.GetEventHandler<UnityEngine.EventSystems.IDragHandler>(hits[0].gameObject);
        Require(receiver == drag.gameObject, "Title bar drag is blocked by another UI object.");
        UnityEngine.EventSystems.ExecuteEvents.Execute(receiver, pointer, UnityEngine.EventSystems.ExecuteEvents.beginDragHandler);
        pointer.position += new Vector2(30, -10);
        UnityEngine.EventSystems.ExecuteEvents.Execute(receiver, pointer, UnityEngine.EventSystems.ExecuteEvents.dragHandler);
        Require(window.anchoredPosition != initial, "Serialized window drag target does not work.");
        window.anchoredPosition = initial;
        return new { startup = "mail from saved ghost preview", drag = "Raycast and EventSystem drag passed", before = initial.ToString(), restored = window.anchoredPosition.ToString() };
    }

    public static object SerializedScene()
    {
        var g = Game;
        Require(g && g.UI, "Missing controller/UI reference.");
        var ui = g.UI;
        foreach (var field in typeof(PrototypeSceneUI).GetFields())
        {
            var value = field.GetValue(ui);
            Require(value != null, "Missing UI binding: " + field.Name);
            if (value is UnityEngine.Object obj) Require(obj, "Missing object: " + field.Name);
            if (value is GameObject[] array) Require(array.Length > 0 && array.All(x => x), "Missing array element: " + field.Name);
            if (value is PrototypeSceneUI.TextStates states)
                Require(states.Label && states.Values.Length > 0, "Missing text state: " + field.Name);
        }
        var buttons = g.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        Require(buttons.All(b => b.onClick.GetPersistentEventCount() == 1 && b.onClick.GetPersistentTarget(0) == g &&
            b.onClick.GetPersistentMethodName(0) == "Click"), "Unwired scene button.");
        var drag = g.GetComponentInChildren<PrototypeWindowDrag>(true);
        var target = typeof(PrototypeWindowDrag).GetField("window", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(drag);
        Require((RectTransform)target == ui.Window, "Drag target did not serialize.");
        return new { objects = g.GetComponentsInChildren<Transform>(true).Length, savedButtons = buttons.Length, allBindingsValid = true, dragTargetValid = true };
    }

    public static object PreviewScreens()
    {
        Require(!Application.isPlaying, "Stop Play mode first.");
        var ui = Game.UI;
        var ids = Ids();
        var originalObjects = AuthoredObjects().ToDictionary(t => t.GetInstanceID(), t => t.name);
        var results = new List<object>();
        foreach (CyberExorcist.Editor.PrototypeSceneUIEditor.Screen screen in Enum.GetValues(typeof(CyberExorcist.Editor.PrototypeSceneUIEditor.Screen)))
        {
            CyberExorcist.Editor.PrototypeSceneUIEditor.ApplyPreview(ui, screen);
            Require(ui.AppPanels.Count(x => x.activeSelf) == 1, "Overlapping apps in preview: " + screen);
            Canvas.ForceUpdateCanvases();
            var texts = Game.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var t in texts) t.ForceMeshUpdate();
            var overflow = texts.Where(t => t.isTextOverflowing).Select(t => t.text).ToArray();
            Require(overflow.Length == 0, "Text overflow in " + screen + ": " + string.Join("; ", overflow));
            results.Add(new { screen = screen.ToString(), visibleTexts = texts.Length });
        }
        var finalObjects = AuthoredObjects().ToDictionary(t => t.GetInstanceID(), t => t.name);
        Require(ids.SequenceEqual(Ids()), "Preview replaced objects. Added: " + string.Join(", ", finalObjects.Where(x => !originalObjects.ContainsKey(x.Key)).Select(x => x.Value)) + "; removed: " + string.Join(", ", originalObjects.Where(x => !finalObjects.ContainsKey(x.Key)).Select(x => x.Value)));
        // Leave a non-initial screen to check that Play correctly starts at the first mail.
        CyberExorcist.Editor.PrototypeSceneUIEditor.ApplyPreview(ui, CyberExorcist.Editor.PrototypeSceneUIEditor.Screen.Ghost3);
        return new { previews = results, noTextOverflow = true, noObjectReplacement = true };
    }
}
