using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>2페이즈 2파트의 PNG 한 장에 음절 클릭 영역을 연결한다.</summary>
public sealed class BloodyMaryFigureImage : MonoBehaviour, IPointerClickHandler
{
    [Serializable]
    public struct SyllableHotspot
    {
        public string syllable;
        // PNG 원본의 왼쪽 위를 (0, 0)으로 하는 픽셀 좌표다.
        public RectInt pixels;
    }

    public BloodyMaryPhaseTwoController phase;
    public RawImage image;
    public Texture2D sourceTexture;
    public SyllableHotspot[] hotspots;

    // 이 좌표와 대사는 Phase2_Part2.png 원본(1546×653)에 맞춘다. 한 장의 PNG에서
    // 음절을 찾기 위해 별도의 텍스트 오브젝트를 만들지 않는다.
    readonly struct PhraseRegion
    {
        public readonly string text;
        public readonly RectInt pixels;
        public PhraseRegion(string text, int x, int y, int width, int height)
        {
            this.text = text;
            pixels = new RectInt(x, y, width, height);
        }
    }

    static readonly PhraseRegion[] Phrases =
    {
        new("그 아이의 몸은 내꺼야 더 많은 생기가 필요해 울지마렴 내 아가..", 184, 4, 1180, 48),
        new("어째서 왜 아무도 구해주지 않았어? 죽어", 120, 61, 612, 43),
        new("돌아가게 두지 않겠어 용서 못해", 774, 61, 610, 43),
        new("네 몸을 내놔 가증스러운 것들", 0, 103, 480, 48),
        new("그 아이의 몸은 내꺼야", 505, 104, 470, 47),
        new("죽어 그 아이의 몸은 내꺼야", 1080, 105, 410, 45),
        new("울지마렴 내 아가..", 116, 158, 330, 43),
        new("돌아가게 두지 않겠어", 565, 159, 400, 43),
        new("왜 날 방해하지?", 1160, 158, 305, 43),
        new("죽어 네 몸을 내놔", 30, 207, 300, 38),
        new("용서 못해 죽어 어째서", 545, 207, 390, 40),
        new("가증스러운 것들", 1215, 211, 260, 39),
        new("다 죽어버려", 86, 256, 175, 39),
        new("돌아가게 두지 않겠어 어째서", 545, 257, 415, 42),
        new("왜 아무도 구해주지 않았어?", 1163, 255, 383, 43),
        new("돌아가게 두지 않겠어", 0, 308, 310, 42),
        new("네 몸을 내놔 아아..", 585, 308, 370, 42),
        new("용서 못해", 1260, 308, 286, 43),
        new("왜? 어째서", 88, 351, 176, 43),
        new("다 죽어버려 죽어", 594, 353, 357, 42),
        new("울지마렴 내 아가..", 1246, 352, 300, 43),
        new("용서 못해", 76, 398, 186, 43),
        new("아아.. 어째서", 579, 398, 186, 43),
        new("어째서 왜?", 1259, 398, 287, 43),
        new("왜 날 방해하지?", 52, 444, 230, 42),
        new("내 아기..", 590, 443, 171, 43),
        new("네 몸을 내놔", 1259, 444, 185, 43),
        new("다 죽어버려", 76, 491, 176, 41),
        new("용서 못해", 617, 489, 148, 42),
        new("가증스러운 것들", 1246, 490, 300, 42),
        new("어째서", 72, 534, 97, 42),
        new("왜?", 1411, 532, 65, 43),
        new("죽어", 99, 575, 68, 42),
        new("어째서", 1410, 573, 103, 43),
        new("용서 못해", 26, 615, 145, 38)
    };

    const int RowCount = 16;
    Texture2D workingTexture;
    Color32[] originalPixels;
    Color32[] displayPixels;
    bool[] used;
    readonly HashSet<int> usedPhraseCharacters = new();
    readonly List<RectInt> usedExtraPixels = new();
    int hiddenRow = -1;

    // 1파트에서 '네 아기는'을 모으므로 이 이미지에는 2파트의 다섯 음절이 필요하다.
    public bool IsConfigured => phase && image && sourceTexture && sourceTexture.isReadable &&
        image.transform == transform && hotspots != null && hotspots.Length >= 5;

    public void Show()
    {
        Clear();
        if (!IsConfigured) return;
        originalPixels = sourceTexture.GetPixels32();
        displayPixels = new Color32[originalPixels.Length];
        used = new bool[hotspots.Length];
        usedPhraseCharacters.Clear();
        usedExtraPixels.Clear();
        workingTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = "Phase2_Part2_Runtime"
        };
        image.texture = workingTexture;
        image.raycastTarget = true;
        gameObject.SetActive(true);
        RebuildTexture();
    }

    public void Clear()
    {
        used = null;
        usedPhraseCharacters.Clear();
        usedExtraPixels.Clear();
        originalPixels = null;
        displayPixels = null;
        hiddenRow = -1;
        if (image) image.texture = sourceTexture;
        if (workingTexture)
        {
            Destroy(workingTexture);
            workingTexture = null;
        }
        gameObject.SetActive(false);
    }

    // 4초 동안 위에서 아래로 각 행을 잠깐 숨겼다가 같은 PNG의 원본으로 복원한다.
    public void SetHiddenRow(int row)
    {
        if (used == null || row == hiddenRow) return;
        int previousRow = hiddenRow;
        hiddenRow = row;
        if (previousRow >= 0 && previousRow != row)
        {
            for (int i = 0; i < hotspots.Length; i++)
                if (RowOf(hotspots[i].pixels) == previousRow) used[i] = false;
            usedPhraseCharacters.RemoveWhere(key => RowOf(Phrases[key / 100].pixels) == previousRow);
            usedExtraPixels.RemoveAll(rect => RowOf(rect) == previousRow);
        }
        RebuildTexture();
    }

    int RowOf(RectInt rect) => Mathf.Clamp(rect.y / Mathf.CeilToInt(sourceTexture.height / (float)RowCount), 0, RowCount - 1);

    void RebuildTexture()
    {
        if (!workingTexture || originalPixels == null) return;
        Array.Copy(originalPixels, displayPixels, originalPixels.Length);
        for (int i = 0; i < hotspots.Length; i++)
            if (used[i]) Erase(hotspots[i].pixels);
        foreach (var rect in usedExtraPixels) Erase(rect);
        if (hiddenRow >= 0)
        {
            int height = sourceTexture.height;
            int rowHeight = Mathf.CeilToInt(height / (float)RowCount);
            int top = hiddenRow * rowHeight;
            for (int y = top; y < Mathf.Min(height, top + rowHeight); y++)
                Erase(new RectInt(0, y, sourceTexture.width, 1));
        }
        workingTexture.SetPixels32(displayPixels);
        workingTexture.Apply(false, false);
    }

    void Erase(RectInt rect)
    {
        int width = sourceTexture.width;
        int height = sourceTexture.height;
        for (int topY = Mathf.Max(0, rect.yMin); topY < Mathf.Min(height, rect.yMax); topY++)
        {
            int row = (height - 1 - topY) * width;
            for (int x = Mathf.Max(0, rect.xMin); x < Mathf.Min(width, rect.xMax); x++)
                displayPixels[row + x].a = 0;
        }
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left || data.dragging || !data.eligibleForClick ||
            !phase || !phase.CanInteract || phase.Part != 2 || used == null) return;
        var rect = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, data.position, data.pressEventCamera, out Vector2 local)) return;
        Rect bounds = rect.rect;
        if (!bounds.Contains(local)) return;
        int x = Mathf.FloorToInt((local.x - bounds.xMin) / bounds.width * sourceTexture.width);
        int y = Mathf.FloorToInt((bounds.yMax - local.y) / bounds.height * sourceTexture.height);
        var point = new Vector2Int(x, y);
        if (RowOf(new RectInt(x, y, 1, 1)) == hiddenRow) return;
        for (int i = 0; i < hotspots.Length; i++)
            if (used[i] && hotspots[i].pixels.Contains(point)) return;
        foreach (var erased in usedExtraPixels)
            if (erased.Contains(point)) return;
        if (!HasVisiblePixelNearby(point, 13)) return;
        // 직접 표시한 음절 영역을 우선한다. 클릭 영역이 좁아도 가까운 글자는 선택되도록 한다.
        int nearest = -1;
        int nearestDistance = int.MaxValue;
        for (int i = 0; i < hotspots.Length; i++)
        {
            if (used[i] || RowOf(hotspots[i].pixels) == hiddenRow || string.IsNullOrEmpty(hotspots[i].syllable)) continue;
            RectInt hit = hotspots[i].pixels;
            int dx = Mathf.Max(hit.xMin - x, 0, x - hit.xMax);
            int dy = Mathf.Max(hit.yMin - y, 0, y - hit.yMax);
            int distance = dx * dx + dy * dy;
            if (distance > 13 * 13 || distance >= nearestDistance) continue;
            nearest = i;
            nearestDistance = distance;
        }
        if (nearest >= 0)
        {
            used[nearest] = true;
            if (!phase.CollectFigureSyllable(hotspots[nearest].syllable[0])) used[nearest] = false;
            else RebuildTexture();
            return;
        }

        for (int i = 0; i < Phrases.Length; i++)
        {
            PhraseRegion phrase = Phrases[i];
            if (!phrase.pixels.Contains(point) || RowOf(phrase.pixels) == hiddenRow) continue;
            if (!TryCharacterAt(phrase, x, out int characterIndex, out RectInt characterPixels)) return;
            int key = i * 100 + characterIndex;
            if (usedPhraseCharacters.Contains(key)) return;
            char syllable = phrase.text[characterIndex];
            if (!phase.CollectFigureSyllable(syllable)) return;
            usedPhraseCharacters.Add(key);
            usedExtraPixels.Add(characterPixels);
            RebuildTexture();
            return;
        }

        // 눈을 이루는 작은 특수기호도 누르면 오답으로 반응한다.
        if (!phase.CollectFigureSyllable('?')) return;
        usedExtraPixels.Add(new RectInt(x - 9, y - 9, 19, 19));
        RebuildTexture();
    }

    bool HasVisiblePixelNearby(Vector2Int point, int radius)
    {
        if (displayPixels == null) return false;
        int width = sourceTexture.width;
        int height = sourceTexture.height;
        for (int y = Mathf.Max(0, point.y - radius); y <= Mathf.Min(height - 1, point.y + radius); y++)
        {
            int row = (height - 1 - y) * width;
            for (int x = Mathf.Max(0, point.x - radius); x <= Mathf.Min(width - 1, point.x + radius); x++)
                if (displayPixels[row + x].a > 32) return true;
        }
        return false;
    }

    static float CharacterWeight(char value) => char.IsWhiteSpace(value) ? .55f :
        char.IsPunctuation(value) ? .55f : 1f;

    bool TryCharacterAt(PhraseRegion phrase, int x, out int index, out RectInt pixels)
    {
        index = -1;
        pixels = default;
        // 대략 지정한 문구 범위에서 실제 붉은 픽셀이 차지하는 폭을 사용한다.
        // PNG를 교체하며 여백이 달라져도 작은 글자의 음절 판정이 밀리지 않게 한다.
        int left = sourceTexture.width;
        int right = -1;
        int width = sourceTexture.width;
        int height = sourceTexture.height;
        for (int row = Mathf.Max(0, phrase.pixels.yMin); row < Mathf.Min(height, phrase.pixels.yMax); row++)
        {
            int offset = (height - 1 - row) * width;
            for (int column = Mathf.Max(0, phrase.pixels.xMin); column < Mathf.Min(width, phrase.pixels.xMax); column++)
            {
                if (originalPixels[offset + column].a <= 32) continue;
                left = Mathf.Min(left, column);
                right = Mathf.Max(right, column);
            }
        }
        if (right < left) return false;
        float total = 0f;
        foreach (char value in phrase.text) total += CharacterWeight(value);
        float advance = (right - left + 1f) / total;
        float position = left;
        x = Mathf.Clamp(x, left, right);
        for (int i = 0; i < phrase.text.Length; i++)
        {
            float next = position + advance * CharacterWeight(phrase.text[i]);
            if (x >= position && x < next)
            {
                if (char.IsWhiteSpace(phrase.text[i])) return false;
                index = i;
                pixels = new RectInt(Mathf.FloorToInt(position), phrase.pixels.yMin,
                    Mathf.CeilToInt(next - position), phrase.pixels.height);
                return true;
            }
            position = next;
        }
        return false;
    }
}
