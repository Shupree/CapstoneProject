using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>씬에 미리 배치한 문구. 가져간 음절의 글리프만 숨겨 원문 배치를 유지한다.</summary>
public sealed class BloodyMarySyllableSource : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public BloodyMaryPhaseTwoController phase;
    public TextMeshProUGUI label;
    public CanvasGroup group;
    [Header("2파트 고정 문구")]
    public string figurePhrase;
    public int figureRow;
    public Vector2 figurePosition;
    public float figureAngle;
    [Min(8f)] public float figureFontSize = 30f;
    public RectTransform Rect => (RectTransform)transform;
    public int Generation { get; private set; }
    string original = "";
    bool[] used;
    bool selectable;
    int pressedIndex = -1, pressedGeneration, pressedPointer;

    void OnEnable() { if (label) label.OnPreRenderText += HideConsumedGlyphs; }
    void OnDisable() { if (label) label.OnPreRenderText -= HideConsumedGlyphs; }

    void HideConsumedGlyphs(TMP_TextInfo info)
    {
        if (used == null) return;
        for (int i = 0; i < used.Length && i < info.characterCount; i++)
        {
            if (!used[i] || !info.characterInfo[i].isVisible) continue;
            var character = info.characterInfo[i];
            var colors = info.meshInfo[character.materialReferenceIndex].colors32;
            for (int vertex = character.vertexIndex; vertex < character.vertexIndex + 4; vertex++)
            {
                var color = colors[vertex];
                color.a = 0;
                colors[vertex] = color;
            }
        }
    }

    public Vector2 Measure(string value)
    {
        label.fontSize = phase && phase.Part == 2 ? figureFontSize : 58f;
        // TMP의 비활성 오브젝트에서는 GetPreferredValues가 0에 가까운 값을 돌려줄 수 있다.
        // 저장된 폰트의 글리프 폭으로 계산하면 첫 등장 위치도 정확하다.
        var font = label.font;
        float scale = label.fontSize / Mathf.Max(1f, font.faceInfo.pointSize);
        float width = 0f;
        foreach (char letter in value)
            width += font.characterLookupTable.TryGetValue(letter, out var glyph)
                ? glyph.glyph.metrics.horizontalAdvance * scale : label.fontSize * .5f;
        return new Vector2(width + 20f, Mathf.Max(72f, font.faceInfo.lineHeight * scale + 16f));
    }

    public void Show(string value, Vector2 position, float angle)
    {
        Generation++;
        original = value;
        used = new bool[value.Length];
        selectable = false;
        foreach (char character in value)
            if (char.IsLetter(character)) { selectable = true; break; }
        pressedIndex = -1;
        Rect.sizeDelta = Measure(value);
        Rect.anchoredPosition = position;
        Rect.localRotation = Quaternion.Euler(0f, 0f, angle);
        label.text = value;
        label.raycastTarget = selectable;
        group.alpha = 0f;
        group.blocksRaycasts = false;
        gameObject.SetActive(true);
    }

    public void ShowFixedFigure(Rect area)
    {
        Show(figurePhrase, new Vector2(figurePosition.x * area.width,
            figurePosition.y * area.height), figureAngle);
    }

    public void SetAlpha(float alpha)
    {
        group.alpha = alpha;
        group.blocksRaycasts = selectable && alpha > .2f;
    }

    public void Clear()
    {
        Generation++;
        pressedIndex = -1;
        original = "";
        used = null;
        selectable = false;
        if (label) label.text = "";
        if (group) { group.alpha = 0f; group.blocksRaycasts = false; }
        gameObject.SetActive(false);
    }

    public bool TryConsume(int index, int generation, out char value)
    {
        value = default;
        if (Generation != generation || used == null || index < 0 || index >= original.Length ||
            used[index] || !char.IsLetter(original[index])) return false;
        value = original[index];
        used[index] = true;
        // 텍스트에 태그를 끼워 넣으면 글자 인덱스/간격이 달라질 수 있다.
        // 원문은 그대로 두고 렌더링 직전에 선택한 글리프의 알파만 0으로 만든다.
        label.ForceMeshUpdate();
        return true;
    }

    public void OnPointerDown(PointerEventData data)
    {
        pressedIndex = -1;
        if (data.button != PointerEventData.InputButton.Left || !phase || !phase.CanInteract ||
            !group.blocksRaycasts || used == null) return;
        label.ForceMeshUpdate();
        int index = TMP_TextUtilities.FindIntersectingCharacter(label, data.position, data.pressEventCamera, true);
        if (index < 0 || index >= original.Length || used[index] || !char.IsLetter(original[index])) return;
        pressedIndex = index;
        pressedGeneration = Generation;
        pressedPointer = data.pointerId;
    }

    public void OnPointerClick(PointerEventData data)
    {
        int index = pressedIndex;
        pressedIndex = -1;
        if (index < 0 || pressedGeneration != Generation || pressedPointer != data.pointerId ||
            data.button != PointerEventData.InputButton.Left || data.dragging || !data.eligibleForClick ||
            !phase || !phase.CanInteract) return;
        phase.CollectSyllable(this, index, Generation);
    }
}
