using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class AuthorExploreArticleBanner
{
    static Sprite Sprite(string suffix) => AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/UI/Internet/Site_Banner_" + suffix + ".png").OfType<Sprite>().First();
    static GameObject Child(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Add article banner");
        go.transform.SetParent(parent, false);
        return go;
    }
    public static string Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/Explore_Proto.unity" || scene.isDirty)
            throw new InvalidOperationException("Saved Explore_Proto in edit mode required.");
        var links = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<ExploreProtoArticleLinks>(true)).Single();
        if (links.siteBanner) throw new InvalidOperationException("Banner already authored.");
        var sprites = new[] { Sprite("Basic"), Sprite("Left"), Sprite("Right"), Sprite("Top"), Sprite("Bottom") };
        var content = links.articleScroll.content;
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        try
        {
            var authorGo = Child("SiteAuthor", content);
            var author = Undo.AddComponent<TMPro.TextMeshProUGUI>(authorGo);
            author.font = links.article.font; author.fontSharedMaterial = links.article.fontSharedMaterial;
            author.fontSize = links.article.fontSize; author.color = links.article.color;
            author.alignment = links.article.alignment; author.raycastTarget = false;
            author.margin = new Vector4(0, 0, 0, 15);
            authorGo.transform.SetSiblingIndex(0);
            var slot = Child("SiteBannerSlot", content); slot.transform.SetSiblingIndex(1);
            var layout = Undo.AddComponent<UnityEngine.UI.LayoutElement>(slot);
            layout.preferredHeight = links.article.rectTransform.rect.width * sprites[0].rect.height / sprites[0].rect.width + 40;
            var imageGo = Child("SiteBanner", slot.transform);
            var rect = (RectTransform)imageGo.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(0, 25); rect.offsetMax = new Vector2(0, -15);
            var image = Undo.AddComponent<UnityEngine.UI.Image>(imageGo);
            image.sprite = sprites[0]; image.raycastTarget = false;
            var banner = Undo.AddComponent<ExploreArticleBanner>(content.gameObject);
            banner.author = author; banner.banner = image; banner.bannerLayout = layout; banner.vision = links.exorcismVision;
            banner.basic = sprites[0]; banner.left = sprites[1]; banner.right = sprites[2]; banner.top = sprites[3]; banner.bottom = sprites[4];
            author.text = banner.authorPrefix;
            var slotRect = (RectTransform)slot.transform;
            slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, links.article.rectTransform.rect.width);
            slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layout.preferredHeight);
            Undo.RecordObject(links, "Connect article banner"); links.siteBanner = banner;
            // Author the currently saved article without invoking gameplay or page navigation.
            Undo.RecordObject(links.article, "Separate article author");
            string text = links.article.text;
            bool visible = text.StartsWith(banner.authorPrefix, StringComparison.Ordinal);
            authorGo.SetActive(visible); slot.SetActive(visible);
            if (visible)
            {
                int end = text.IndexOf('\n');
                author.text = text.Substring(0, end).TrimEnd('\r');
                links.article.text = text.Substring(end).TrimStart('\r', '\n');
            }
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Undo.CollapseUndoOperations(group);
            return "Saved author + banner + original article layout; sprite references connected.";
        }
        catch { Undo.RevertAllDownToGroup(group); throw; }
    }
}
