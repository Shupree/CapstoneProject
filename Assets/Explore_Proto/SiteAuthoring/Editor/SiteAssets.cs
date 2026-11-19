using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

internal static class SiteAssets
{
    public const string Root = "Assets/Explore_Proto/SiteAuthoring";
    public const string Templates = Root + "/Templates";
    public const string Pages = Root + "/Pages";
    public const string Shelf = Root + "/BrowserPages.prefab";

    public static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(Templates)) AssetDatabase.CreateFolder(Root, "Templates");
        if (!AssetDatabase.IsValidFolder(Pages)) AssetDatabase.CreateFolder(Root, "Pages");
    }

    public static string Key(Object obj)
    {
        return obj && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long local)
            ? guid + ":" + local : "";
    }

    public static T Resolve<T>(string key) where T : Object
    {
        if (string.IsNullOrEmpty(key)) return null;
        var parts = key.Split(':');
        if (parts.Length != 2) return null;
        var path = AssetDatabase.GUIDToAssetPath(parts[0]);
        return string.IsNullOrEmpty(path) ? null :
            AssetDatabase.LoadAllAssetsAtPath(path).OfType<T>().FirstOrDefault(obj => Key(obj) == key);
    }

    public static List<(TextAsset file, SitePageData data)> ReadPages()
    {
        EnsureFolders();
        var list = new List<(TextAsset, SitePageData)>();
        foreach (var path in Directory.GetFiles(Pages, "*.json").OrderBy(p => p))
        {
            var file = AssetDatabase.LoadAssetAtPath<TextAsset>(path.Replace('\\','/'));
            if (!file) continue;
            try
            {
                var page = JsonUtility.FromJson<SitePageData>(file.text);
                if (page == null || string.IsNullOrEmpty(page.id)) throw new FormatException("페이지 ID 누락");
                list.Add((file,page));
            }
            catch (Exception ex) { throw new FormatException(path + " · " + ex.Message, ex); }
        }
        return list;
    }

    [Serializable] class LegacyPage { public string url; }
    [Serializable] class LegacyCatalog { public LegacyPage[] pages; }
    public static string Validate(SitePageData page, GameObject template, TextAsset current)
    {
        if (!template || !AssetDatabase.GetAssetPath(template).StartsWith(Templates + "/"))
            return "사이트 제작 폴더의 템플릿 프리팹을 선택하세요.";
        var address = SitePageData.Normalize(page.address);
        if (!Uri.TryCreate("https://" + address, UriKind.Absolute, out var uri) ||
            !uri.Host.Contains(".") || address.Any(char.IsWhiteSpace) || address.StartsWith("inspect/") ||
            address.Contains("://")) return "주소에 도메인과 선택적 경로를 입력하세요. 예: lounge.local/post/1";
        var legacy = Resources.Load<TextAsset>("ExploreBrowserPages");
        var pages = ReadPages();
        if (JsonUtility.FromJson<LegacyCatalog>(legacy.text).pages.Any(p => p.url == address) ||
            pages.Any(p => p.file != current && SitePageData.Normalize(p.data.address) == address))
            return "페이지 주소 중복 · " + address;
        if (pages.Any(p => p.file != current && p.data.id == page.id)) return "페이지 ID 중복 · " + page.id;
        var slots = template.GetComponentsInChildren<SiteSlot>(true);
        if (slots.Any(s => string.IsNullOrEmpty(s.id)) || slots.GroupBy(s => s.id).Any(g => g.Count() > 1))
            return "템플릿의 슬롯 ID가 비었거나 중복되었습니다. 템플릿 창에서 복제한 슬롯의 ID를 갱신하세요.";
        foreach (var content in page.slots)
        {
            var slot = slots.FirstOrDefault(s => s.id == content.slotId);
            if (!slot) return "삭제된 슬롯 · " + content.slotId + " (내용을 확인하고 제거하세요)";
            if (slot.kind != SiteSlotKind.Body && content.blocks.Count != 1) return slot.name + " · 슬롯 종류와 내용이 다릅니다.";
            for (int i = 0; i < content.blocks.Count; i++)
            {
                var block = content.blocks[i];
                string where = slot.name + " / " + (i + 1) + "번 · ";
                if (slot.kind == SiteSlotKind.Text && block.kind != SiteBlockKind.Paragraph ||
                    slot.kind == SiteSlotKind.Image && block.kind != SiteBlockKind.Image)
                    return where + "슬롯 종류를 확인하세요.";
                if (block.kind == SiteBlockKind.Link && (!pages.Any(p => p.data.id == block.destinationId) || string.IsNullOrWhiteSpace(block.text)))
                    return where + "링크 문구와 목적지 페이지를 확인하세요.";
                if (block.kind == SiteBlockKind.Image && !Resolve<Sprite>(block.assetId))
                    return where + "Sprite가 누락되었습니다.";
                if (block.kind == SiteBlockKind.Prefab)
                {
                    var asset = Resolve<GameObject>(block.assetId);
                    var path = AssetDatabase.GetAssetPath(asset);
                    if (!asset || !(asset.transform is RectTransform) || !path.EndsWith(".prefab") ||
                        AssetDatabase.LoadMainAssetAtPath(path) != asset)
                        return where + "RectTransform이 있는 UI 프리팹을 선택하세요.";
                    if (path == Shelf || AssetDatabase.GetDependencies(path).Contains(Shelf))
                        return where + "브라우저 페이지 모음을 포함하는 프리팹은 순환 참조됩니다.";
                }
                for (int a = 0; a < block.areas.Count; a++)
                {
                    var area = block.areas[a];
                    var r = area.rect;
                    if (string.IsNullOrWhiteSpace(area.eventId) || r.width <= 0 || r.height <= 0 ||
                        r.x < 0 || r.y < 0 || r.xMax > 1.00001f || r.yMax > 1.00001f)
                        return where + "조사 영역 " + (a + 1) + "의 이벤트 ID와 0~1 범위를 확인하세요.";
                }
            }
        }
        return null;
    }

    public static RectTransform Node(string name, Transform parent)
    {
        var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        if (parent && rect.gameObject.scene != parent.gameObject.scene)
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(rect.gameObject, parent.gameObject.scene);
        rect.SetParent(parent, false);
        return rect;
    }

    public static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    public static TextMeshProUGUI Label(GameObject go)
    {
        var label = go.AddComponent<TextMeshProUGUI>();
        label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            AssetDatabase.GUIDToAssetPath("eeb43719f3f8b074c883376ceb2a11e2"));
        label.fontSize = 36;
        label.color = new Color(.10f, .13f, .16f);
        label.richText = false;
        label.raycastTarget = false;
        return label;
    }
}
