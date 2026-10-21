using TMPro;
using UnityEngine;

/// <summary>입력창의 음절 버튼. 오답은 붉게 표시하고 클릭하면 해당 음절을 지운다.</summary>
public sealed class BloodyMarySelectedSyllable : MonoBehaviour
{
    public BloodyMaryPhaseTwoController phase;
    public TextMeshProUGUI label;
    public UnityEngine.UI.Button button;
    public int index;

    void Awake() { if (button) button.onClick.AddListener(Remove); }
    void OnDestroy() { if (button) button.onClick.RemoveListener(Remove); }

    public void Show(string value, bool correct)
    {
        label.text = value;
        label.color = correct ? Color.white : new Color(1f, .16f, .18f, 1f);
        button.interactable = !string.IsNullOrEmpty(value);
    }

    void Remove() { if (phase) phase.RemoveSyllable(index); }
}
