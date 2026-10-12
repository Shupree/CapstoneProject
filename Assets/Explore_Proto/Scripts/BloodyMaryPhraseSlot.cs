using UnityEngine;

/// <summary>Fixed pool slot; collected words leave their original space empty.</summary>
public sealed class BloodyMaryPhraseSlot : MonoBehaviour
{
    public CanvasGroup fade;
    public BloodyMaryWordSlot[] words;
    public float wordSpacing = 14f;
    public RectTransform Rect => (RectTransform)transform;
    public int Generation { get; private set; }
    public bool Active { get; private set; }
    public bool IsVisible => Active && fade.alpha > 0f;
    public Rect Bounds => new Rect(Rect.anchoredPosition, Rect.sizeDelta);
    float age;

    public Vector2 Measure(string[] text)
    {
        float width = 0f, height = 0f;
        for (int i = 0; i < text.Length; i++)
        {
            var size = words[i].label.GetPreferredValues(text[i]);
            width += size.x + 12f + (i == 0 ? 0f : wordSpacing);
            height = Mathf.Max(height, size.y + 12f);
        }
        return new Vector2(width, height);
    }

    public void Display(string[] text, Vector2 position, Vector2 size)
    {
        Generation++;
        Active = true;
        age = 0f;
        Rect.anchoredPosition = position;
        Rect.sizeDelta = size;
        float x = 0f;
        for (int i = 0; i < words.Length; i++)
        {
            if (i >= text.Length) { words[i].Hide(); continue; }
            var rect = (RectTransform)words[i].transform;
            float width = words[i].label.GetPreferredValues(text[i]).x + 12f;
            rect.anchoredPosition = new Vector2(x, 0f);
            rect.sizeDelta = new Vector2(width, size.y);
            x += width + wordSpacing;
            words[i].Show(text[i], Generation);
        }
        fade.alpha = 0f;
        fade.blocksRaycasts = false;
        gameObject.SetActive(true);
    }

    public void Tick(float delta, float fadeSeconds, float lifetime)
    {
        if (!Active) return;
        age += delta;
        if (age >= lifetime) { Clear(); return; }
        fade.alpha = Mathf.Clamp01(age / fadeSeconds);
        fade.blocksRaycasts = fade.alpha > 0f;
    }

    public void Consume(int index)
    {
        words[index].Hide();
        foreach (var word in words) if (word.Visible) return;
        Clear();
    }

    public void Clear()
    {
        Generation++;
        Active = false;
        foreach (var word in words) word.Hide();
        fade.alpha = 0f;
        fade.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
