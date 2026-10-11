using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>One-use, in-memory handoff between the two scene-authored desktops.</summary>
public static class ExploreProtoSceneTransfer
{
    const string ExploreScene = "Explore_Proto";
    const string BattleScene = "Battle_Proto";
    const string BattlePath = "Assets/Scenes/Battle_Proto.unity";
    static DesktopState pending;
    static bool loading;

    public sealed class DesktopState
    {
        public string[] clueIds;
        public bool keywordSelected, resultsVisible, detailVisible;
        public int siteIndex;
        public string searchHint;
        public WindowState[] windows;
        public ScrollState[] scrolls;
    }

    public struct WindowState
    {
        public bool active;
        public Vector3 position;
        public int sibling;
    }

    public struct ScrollState
    {
        public Vector2 normalized, contentPosition;
    }

    // Also reset when Enter Play Mode has domain reload disabled.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession()
    {
        pending = null;
        loading = false;
    }

    public static void InitializeScene(ExploreProtoMessenger desktop)
    {
        string scene = desktop.gameObject.scene.name;
        if (scene == ExploreScene)
        {
            ResetSession();
            return;
        }
        if (scene != BattleScene) return;
        // Start runs after both controllers' Awake initializers.
        var state = pending;
        ResetSession();
        desktop.RestoreCompletedDesktop(state);
        if (state != null) RestoreScrolls(desktop.DesktopScrolls, state.scrolls);
        var battle = desktop.GetComponent<BattleProtoConversation>();
        if (battle) battle.BeginPreparation();
    }

    public static void EnterBattle(ExploreProtoMessenger desktop)
    {
        if (loading || desktop.gameObject.scene.name != ExploreScene) return;
        if (!Application.CanStreamedLevelBeLoaded(BattlePath))
        {
            ShowLoadFailure(desktop, "Battle_Proto 씬이 빌드 씬 목록에 등록되어 있지 않습니다.");
            return;
        }
        pending = desktop.CaptureDesktop();
        loading = true;
        try
        {
            // Keep the existing desktop visible while the next scene loads.
            var operation = SceneManager.LoadSceneAsync(BattlePath, LoadSceneMode.Single);
            if (operation == null) ShowLoadFailure(desktop, "Battle_Proto 씬을 불러오지 못했습니다.");
        }
        catch (Exception error)
        {
            ShowLoadFailure(desktop, error.Message);
        }
    }

    static void ShowLoadFailure(ExploreProtoMessenger desktop, string message)
    {
        ResetSession();
        Debug.LogError(message, desktop);
        desktop.investigation.replyLabel.text = "배틀 씬으로 이동 다시 시도";
        desktop.investigation.replyButton.gameObject.SetActive(true);
    }

    public static WindowState[] CaptureWindows(GameObject[] windows)
    {
        var states = new WindowState[windows.Length];
        for (int i = 0; i < windows.Length; i++)
            states[i] = new WindowState
            {
                active = windows[i].activeSelf,
                position = ((RectTransform)windows[i].transform).anchoredPosition3D,
                sibling = windows[i].transform.GetSiblingIndex()
            };
        return states;
    }

    public static void RestoreWindows(GameObject[] windows, WindowState[] states)
    {
        int count = Mathf.Min(windows.Length, states.Length);
        var order = new int[count];
        for (int i = 0; i < count; i++)
        {
            order[i] = i;
            ((RectTransform)windows[i].transform).anchoredPosition3D = states[i].position;
            windows[i].SetActive(states[i].active);
        }
        Array.Sort(order, (a, b) => states[a].sibling.CompareTo(states[b].sibling));
        foreach (int index in order) windows[index].transform.SetSiblingIndex(states[index].sibling);
    }

    public static ScrollState[] CaptureScrolls(UnityEngine.UI.ScrollRect[] scrolls)
    {
        var states = new ScrollState[scrolls.Length];
        for (int i = 0; i < scrolls.Length; i++)
            if (scrolls[i]) states[i] = new ScrollState
            {
                normalized = scrolls[i].normalizedPosition,
                contentPosition = scrolls[i].content.anchoredPosition
            };
        return states;
    }

    static void RestoreScrolls(UnityEngine.UI.ScrollRect[] scrolls, ScrollState[] states)
    {
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < Mathf.Min(scrolls.Length, states.Length); i++)
        {
            var scroll = scrolls[i];
            if (!scroll) continue;
            scroll.StopMovement();
            if (scroll.isActiveAndEnabled)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
                scroll.normalizedPosition = states[i].normalized;
            }
            else scroll.content.anchoredPosition = states[i].contentPosition;
        }
    }
}
