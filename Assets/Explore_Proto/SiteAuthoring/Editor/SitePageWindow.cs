using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class SitePageWindow : EditorWindow
{
    [SerializeField] SitePageData page;
    [SerializeField] TextAsset source;
    [SerializeField] GameObject template;
    [SerializeField] string original;
    List<(TextAsset file, SitePageData data)> pages;
    Vector2 scroll, dragStart, dragEnd;
    SiteBlock drawing;
    string notice;
    readonly Dictionary<string, UnityEngine.Object> assetCache = new Dictionary<string, UnityEngine.Object>();

    [MenuItem("Tools/사이트 제작/페이지 제작 에디터")]
    static void Open() => GetWindow<SitePageWindow>("페이지 제작");

    void OnEnable()
    {
        if (page == null || string.IsNullOrEmpty(page.id)) NewPage();
        Refresh();
    }
    void OnProjectChange() { Refresh(); Repaint(); }
    void Refresh()
    {
        assetCache.Clear();
        try { pages = SiteAssets.ReadPages(); }
        catch (Exception e) { pages = null; notice = e.Message; }
    }
    void NewPage()
    {
        source = null;
        page = new SitePageData { id = Guid.NewGuid().ToString("N"), templateId = SiteAssets.Key(template) };
        original = "";
        notice = "";
        hasUnsavedChanges = false;
        drawing = null;
        scroll = Vector2.zero;
    }
    bool MayLeave() => !hasUnsavedChanges || EditorUtility.DisplayDialog("페이지 전환", "저장하지 않은 편집을 버릴까요?", "버리고 전환", "계속 편집");
    void LoadPage(int index)
    {
        (source, page) = pages[index];
        original = source.text;
        page = JsonUtility.FromJson<SitePageData>(original);
        template = SiteAssets.Resolve<GameObject>(page.templateId);
        hasUnsavedChanges = false;
        notice = "";
        drawing = null;
        scroll = Vector2.zero;
    }

    void OnGUI()
    {
        if (GUILayout.Button("새 페이지") && MayLeave()) NewPage();
        if (pages == null) { EditorGUILayout.HelpBox(notice, MessageType.Error); return; }
        var labels = new[] { "저장한 페이지 열기…" }.Concat(pages.Select(p => p.data.address)).ToArray();
        int opened = EditorGUILayout.Popup(0, labels);
        if (opened > 0 && MayLeave()) LoadPage(opened - 1);
        EditorGUI.BeginChangeCheck();
        var chosen = (GameObject)EditorGUILayout.ObjectField("템플릿 프리팹", template, typeof(GameObject), false);
        if (chosen != template)
        {
            template = chosen;
            page.templateId = SiteAssets.Key(template);
        }
        page.address = EditorGUILayout.TextField("게임 내 주소", page.address ?? "");
        EditorGUILayout.LabelField("페이지 ID", page.id);
        using (new EditorGUI.DisabledScope(!template || !(template.transform is RectTransform)))
            if (GUILayout.Button("사이트 미리보기")) SitePreviewWindow.ShowPage(template, page);
        if (GUILayout.Button("페이지 저장")) SaveChanges();
        if (!string.IsNullOrEmpty(notice)) EditorGUILayout.HelpBox(notice, MessageType.Info);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (template) DrawSlots();
        EditorGUILayout.EndScrollView();
        if (EditorGUI.EndChangeCheck()) hasUnsavedChanges = true;
        saveChangesMessage = "페이지 변경 내용을 저장할까요?";
    }

    void DrawSlots()
    {
        var slots = template.GetComponentsInChildren<SiteSlot>(true);
        foreach (var slot in slots)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(slot.name + " · " + slot.kind, EditorStyles.boldLabel);
            var content = page.slots.Find(s => s.slotId == slot.id);
            if (content == null)
            {
                content = new SiteSlotContent { slotId = slot.id };
                page.slots.Add(content);
                if (slot.kind != SiteSlotKind.Body) content.blocks.Add(new SiteBlock { kind = slot.kind == SiteSlotKind.Text ? SiteBlockKind.Paragraph : SiteBlockKind.Image });
            }
            if (slot.kind == SiteSlotKind.Body)
            {
                for (int i = 0; i < content.blocks.Count; i++)
                {
                    var block = content.blocks[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField((i + 1) + ". " + block.kind, GUILayout.MinWidth(70));
                    bool up, down;
                    using (new EditorGUI.DisabledScope(i == 0)) up = GUILayout.Button("↑", GUILayout.Width(32));
                    using (new EditorGUI.DisabledScope(i + 1 == content.blocks.Count)) down = GUILayout.Button("↓", GUILayout.Width(32));
                    bool remove = GUILayout.Button("삭제", GUILayout.Width(50));
                    EditorGUILayout.EndHorizontal();
                    if (up || down || remove)
                    {
                        content.blocks.RemoveAt(i);
                        if (!remove) content.blocks.Insert(i + (up ? -1 : 1), block);
                        hasUnsavedChanges = true;
                        break;
                    }
                    DrawBlock(block);
                }
                EditorGUILayout.BeginHorizontal();
                Add(content,"문단",SiteBlockKind.Paragraph);
                Add(content,"이미지",SiteBlockKind.Image);
                Add(content,"링크",SiteBlockKind.Link);
                Add(content,"UI 프리팹",SiteBlockKind.Prefab);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                var expected = slot.kind == SiteSlotKind.Text ? SiteBlockKind.Paragraph : SiteBlockKind.Image;
                if (content.blocks.Count == 1 && content.blocks[0].kind == expected) DrawBlock(content.blocks[0]);
                else if (GUILayout.Button("변경된 슬롯 종류로 내용 초기화") && EditorUtility.DisplayDialog("슬롯 초기화", slot.name + "의 내용을 초기화할까요?", "초기화", "취소"))
                    content.blocks = new List<SiteBlock> { new SiteBlock { kind = expected } };
            }
            EditorGUILayout.EndVertical();
        }
        foreach (var orphan in page.slots.Where(s => !slots.Any(slot => slot.id == s.slotId)).ToArray())
        {
            EditorGUILayout.HelpBox("템플릿에서 사라진 슬롯: " + orphan.slotId + " · 블록 " + orphan.blocks.Count + "개", MessageType.Warning);
            if (GUILayout.Button("이 슬롯 내용 제거")) { page.slots.Remove(orphan); hasUnsavedChanges = true; }
        }
    }

    static void Add(SiteSlotContent content, string name, SiteBlockKind kind)
    {
        if (GUILayout.Button("+ " + name)) { content.blocks.Add(new SiteBlock { kind = kind }); GUI.changed = true; }
    }

    void DrawBlock(SiteBlock block)
    {
        if (block.kind == SiteBlockKind.Paragraph)
            block.text = EditorGUILayout.TextArea(block.text ?? "", GUILayout.MinHeight(44));
        else if (block.kind == SiteBlockKind.Link)
        {
            block.text = EditorGUILayout.TextField("링크 문구", block.text ?? "");
            var labels = new[] { string.IsNullOrEmpty(block.destinationId) ? "목적지 선택…" : "목적지 누락 / 선택…" }
                .Concat(pages.Select(p => p.data.address)).ToArray();
            int index = pages.FindIndex(p => p.data.id == block.destinationId) + 1;
            int selected = EditorGUILayout.Popup("목적지", index, labels);
            if (index != selected) block.destinationId = selected == 0 ? "" : pages[selected - 1].data.id;
        }
        else
        {
            bool image = block.kind == SiteBlockKind.Image;
            UnityEngine.Object asset = null;
            if (!string.IsNullOrEmpty(block.assetId) && !assetCache.TryGetValue(block.assetId, out asset))
            {
                asset = image ? (UnityEngine.Object)SiteAssets.Resolve<Sprite>(block.assetId) : SiteAssets.Resolve<GameObject>(block.assetId);
                assetCache[block.assetId] = asset;
            }
            var next = EditorGUILayout.ObjectField(image ? "Sprite" : "UI 프리팹", asset, image ? typeof(Sprite) : typeof(GameObject), false);
            if (next != asset) block.assetId = SiteAssets.Key(next);
            if (next is Sprite sprite) DrawAreas(block, sprite);
        }
    }

    void DrawAreas(SiteBlock block, Sprite sprite)
    {
        EditorGUILayout.LabelField("이미지 위를 드래그하여 조사 영역 추가");
        var box = GUILayoutUtility.GetRect(50, 200, GUILayout.ExpandWidth(true));
        var size = sprite.rect.size;
        size *= Mathf.Min(box.width / size.x, box.height / size.y);
        var image = new Rect(box.center - size * .5f, size);
        var texture = UnityEditor.Sprites.SpriteUtility.GetSpriteTexture(sprite, false);
        if (!texture) texture = sprite.texture;
        var src = sprite.rect;
        GUI.DrawTextureWithTexCoords(image, texture, new Rect(src.x / texture.width, src.y / texture.height,
            src.width / texture.width, src.height / texture.height));
        for (int i = 0; i < block.areas.Count; i++) Overlay(image, block.areas[i].rect, (i + 1).ToString());
        var e = Event.current;
        int control = GUIUtility.GetControlID(FocusType.Passive);
        var uv = new Vector2(Mathf.Clamp01((e.mousePosition.x - image.x) / image.width),
            Mathf.Clamp01((e.mousePosition.y - image.y) / image.height));
        if (e.type == EventType.MouseDown && e.button == 0 && image.Contains(e.mousePosition))
        {
            GUIUtility.hotControl = control;
            drawing = block;
            dragStart = dragEnd = uv;
            e.Use();
        }
        if (drawing == block && GUIUtility.hotControl == control)
        {
            if (e.type == EventType.MouseDrag || e.type == EventType.MouseUp) dragEnd = uv;
            var selection = Rect.MinMaxRect(Mathf.Min(dragStart.x,dragEnd.x), Mathf.Min(dragStart.y,dragEnd.y),
                Mathf.Max(dragStart.x,dragEnd.x), Mathf.Max(dragStart.y,dragEnd.y));
            Overlay(image, selection, "+");
            if (e.type == EventType.MouseUp)
            {
                if (selection.width * size.x >= 2 && selection.height * size.y >= 2)
                {
                    block.areas.Add(new SiteArea { rect = selection });
                    hasUnsavedChanges = true;
                }
                GUIUtility.hotControl = 0;
                drawing = null;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag) { Repaint(); e.Use(); }
        }
        for (int i = 0; i < block.areas.Count; i++)
        {
            var area = block.areas[i];
            area.eventId = EditorGUILayout.TextField("영역 " + (i + 1) + " · 이벤트 ID", area.eventId ?? "");
            area.rect = EditorGUILayout.RectField("상대 좌표 (좌상단 기준)", area.rect);
            if (GUILayout.Button("영역 " + (i + 1) + " 삭제")) { block.areas.RemoveAt(i); hasUnsavedChanges = true; break; }
        }
    }
    static void Overlay(Rect bounds, Rect uv, string label)
    {
        var rect = new Rect(bounds.x + bounds.width * uv.x, bounds.y + bounds.height * uv.y,
            bounds.width * uv.width, bounds.height * uv.height);
        EditorGUI.DrawRect(rect, new Color(.13f,.7f,1,.42f));
        GUI.Label(rect, label, EditorStyles.whiteBoldLabel);
    }

    public override void SaveChanges()
    {
        try
        {
            if (source && source.text != original) throw new InvalidOperationException("원본 JSON이 변경되었습니다. 저장한 페이지를 다시 여세요.");
            var error = SiteAssets.Validate(page, template, source);
            if (error != null) throw new InvalidOperationException(error);
            page.address = SitePageData.Normalize(page.address);
            var path = source ? AssetDatabase.GetAssetPath(source) : SiteAssets.Pages + "/" + page.id + ".json";
            if (!source && File.Exists(path)) throw new IOException("같은 이름의 JSON 파일이 이미 있습니다: " + path);
            var json = JsonUtility.ToJson(page, true);
            bool existed = source;
            try
            {
                File.WriteAllText(path, json);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var file = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                SitePageBake.Write(file, page, template);
                source = file;
            }
            catch
            {
                if (existed) { File.WriteAllText(path,original); AssetDatabase.ImportAsset(path); }
                else AssetDatabase.DeleteAsset(path);
                throw;
            }
            original = json;
            Refresh();
            notice = "저장됨 · " + path;
            base.SaveChanges();
        }
        catch (Exception e)
        {
            notice = e.Message;
            EditorUtility.DisplayDialog("페이지 저장 확인", notice, "확인");
        }
    }
}
