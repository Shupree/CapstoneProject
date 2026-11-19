using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public sealed class SitePreviewWindow : EditorWindow
{
    // Size of the authored page shelf inside Explore_Proto's LinkBrowserSurface.
    static readonly Vector2 PageSize = new Vector2(2581.298f * .9f, 1167.814f * .78f);
    PreviewRenderUtility preview;
    ScrollRect[] scrollers;
    string caption;

    internal static void ShowPage(GameObject template, SitePageData draft)
    {
        var window = GetWindow<SitePreviewWindow>("사이트 미리보기");
        window.Release();
        try
        {
            window.preview = new PreviewRenderUtility();
            var camera = window.preview.camera;
            camera.orthographic = true;
            camera.orthographicSize = PageSize.y / 2;
            camera.transform.SetPositionAndRotation(new Vector3(0, 0, -100), Quaternion.identity);
            camera.nearClipPlane = 1;
            camera.farClipPlane = 300;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.gray;

            var root = new GameObject("페이지 미리보기", typeof(RectTransform), typeof(Canvas));
            window.preview.AddSingleGO(root);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = PageSize;
            SitePageBake.Populate(rect, draft, template, true);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            window.scrollers = root.GetComponentsInChildren<ScrollRect>();
            foreach (var scroll in window.scrollers) scroll.verticalNormalizedPosition = 1;
            window.caption = string.IsNullOrWhiteSpace(draft.address) ? template.name : draft.address;
        }
        catch (Exception ex)
        {
            window.Release();
            window.caption = "미리보기를 표시할 수 없습니다. " + ex.Message;
            Debug.LogException(ex);
        }
        window.Repaint();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField(caption ?? "페이지 제작 창에서 사이트 미리보기를 누르세요.");
        if (preview == null) return;
        foreach (var scroll in scrollers)
        {
            if (!scroll.vertical || !scroll.content || !scroll.viewport ||
                scroll.content.rect.height <= scroll.viewport.rect.height + 1) continue;
            float current = 1 - scroll.verticalNormalizedPosition;
            float offset = EditorGUILayout.Slider(scroll.name, current, 0, 1);
            if (!Mathf.Approximately(current, offset)) scroll.verticalNormalizedPosition = 1 - offset;
        }
        var bounds = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (Event.current.type != EventType.Repaint || bounds.width < 2 || bounds.height < 2) return;
        float scale = Mathf.Min(bounds.width / PageSize.x, bounds.height / PageSize.y);
        var area = new Rect(bounds.center - PageSize * (scale / 2), PageSize * scale);
        preview.BeginPreview(area, GUIStyle.none);
        try
        {
            preview.camera.aspect = PageSize.x / PageSize.y;
            Canvas.ForceUpdateCanvases();
            preview.Render(true);
        }
        finally { preview.EndAndDrawPreview(area); }
    }

    void OnDisable() => Release();

    void Release()
    {
        preview?.Cleanup();
        preview = null;
        scrollers = null;
    }
}
