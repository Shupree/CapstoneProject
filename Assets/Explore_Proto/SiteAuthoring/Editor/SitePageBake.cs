using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

internal static class SitePageBake
{
    // The scene contains this shelf as a prefab instance. Only editor saves change its hierarchy.
    public static void Write(TextAsset source, SitePageData page, GameObject template)
    {
        var shelf = PrefabUtility.LoadPrefabContents(SiteAssets.Shelf);
        try
        {
            var old = shelf.GetComponentsInChildren<SitePageView>(true)
                .FirstOrDefault(existing => existing.source == source);
            if (old) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var root = Populate((RectTransform)shelf.transform, page, template);
            var view = root.gameObject.AddComponent<SitePageView>();
            view.source = source;
            var status = SiteAssets.Node("조사 결과", root);
            status.anchorMin = Vector2.zero;
            status.anchorMax = Vector2.right;
            status.pivot = new Vector2(.5f,0);
            status.sizeDelta = new Vector2(0,64);
            status.anchoredPosition = new Vector2(0, -64);
            view.status = SiteAssets.Label(status.gameObject);
            root.gameObject.SetActive(false);
            PrefabUtility.RecordPrefabInstancePropertyModifications(root.gameObject);
            if (!PrefabUtility.SaveAsPrefabAsset(shelf, SiteAssets.Shelf)) throw new InvalidOperationException("페이지 배치 저장 실패");
        }
        finally { PrefabUtility.UnloadPrefabContents(shelf); }
    }

    internal static RectTransform Populate(RectTransform root, SitePageData page, GameObject template, bool preview = false)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(template, root);
        var frame = (RectTransform)instance.transform;
        frame.anchorMin = frame.anchorMax = frame.pivot = new Vector2(.5f, 1f);
        frame.sizeDelta = ((RectTransform)template.transform).rect.size;
        frame.anchoredPosition = Vector2.zero;
        PrefabUtility.RecordPrefabInstancePropertyModifications(frame);
        foreach (var slot in instance.GetComponentsInChildren<SiteSlot>(true))
        {
            var content = page.slots.Find(s => s.slotId == slot.id);
            if (content == null) continue;
            var parent = slot.kind == SiteSlotKind.Body ? Body(slot) : slot.transform;
            for (int i = 0; i < content.blocks.Count; i++)
                if (!preview || content.blocks[i].kind != SiteBlockKind.Prefab ||
                    SiteAssets.Resolve<GameObject>(content.blocks[i].assetId)?.transform is RectTransform)
                    Fill(slot, parent, content.blocks[i], i);
        }
        return frame;
    }

    static RectTransform Body(SiteSlot slot)
    {
        var viewport = SiteAssets.Node("본문 스크롤", slot.transform);
        SiteAssets.Stretch(viewport);
        viewport.gameObject.AddComponent<Image>().color = Color.clear;
        viewport.gameObject.AddComponent<RectMask2D>();
        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        var rows = SiteAssets.Node("본문 내용", viewport);
        rows.anchorMin = new Vector2(0,1);
        rows.anchorMax = Vector2.one;
        rows.pivot = new Vector2(.5f,1);
        rows.sizeDelta = Vector2.zero;
        var layout = rows.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 12;
        rows.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.viewport = viewport;
        scroll.content = rows;
        scroll.scrollSensitivity = 75;
        return rows;
    }

    static void Fill(SiteSlot slot, Transform parent, SiteBlock block, int index)
    {
        if (block.kind == SiteBlockKind.Prefab)
        {
            var prefab = SiteAssets.Resolve<GameObject>(block.assetId);
            var child = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            var rect = (RectTransform)child.transform;
            if (LayoutUtility.GetPreferredHeight(rect) <= 0)
            {
                var layout = child.GetComponent<LayoutElement>() ?? child.AddComponent<LayoutElement>();
                layout.preferredHeight = Mathf.Max(1, rect.rect.height);
                PrefabUtility.RecordPrefabInstancePropertyModifications(layout);
            }
            return;
        }
        var go = slot.kind == SiteSlotKind.Body ? SiteAssets.Node(block.kind + " " + (index + 1), parent).gameObject : slot.gameObject;
        if (block.kind == SiteBlockKind.Image)
        {
            var image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
            image.sprite = SiteAssets.Resolve<Sprite>(block.assetId);
            image.type = Image.Type.Simple;
            image.useSpriteMesh = false;
            image.preserveAspect = true;
            image.raycastTarget = block.areas.Count > 0;
            if (slot.kind == SiteSlotKind.Body) go.AddComponent<SiteImageHeight>();
        }
        else
        {
            var label = go.GetComponent<TMP_Text>() ?? SiteAssets.Label(go);
            label.text = block.text;
            if (block.kind == SiteBlockKind.Link)
            {
                label.color = new Color(.12f,.33f,.57f);
                label.fontStyle |= FontStyles.Underline;
                label.raycastTarget = true;
            }
        }
        if (block.kind == SiteBlockKind.Link || block.kind == SiteBlockKind.Image)
        {
            var action = go.AddComponent<SitePageAction>();
            action.slotId = slot.id;
            action.blockIndex = index;
        }
        if (PrefabUtility.IsPartOfPrefabInstance(go))
            foreach (var c in go.GetComponents<Component>()) PrefabUtility.RecordPrefabInstancePropertyModifications(c);
    }
}
