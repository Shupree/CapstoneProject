using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>씬에 배치한 붉은 음절 한 개의 클릭과 소모 상태를 관리한다.</summary>
public sealed class Battle02WordSource : MonoBehaviour
{
    public Battle02RitualController controller;
    public TMP_Text label;
    public Button button;
    [Tooltip("클릭으로 선택할 한 음절")]
    public string word;

    public bool IsConsumed { get; private set; }

    void Awake()
    {
        if (!label) label = GetComponent<TMP_Text>();
        if (!button) button = GetComponent<Button>();
        if (button) button.onClick.AddListener(Select);
    }

    void OnDestroy()
    {
        if (button) button.onClick.RemoveListener(Select);
    }

    void Select()
    {
        if (controller) controller.SelectWord(this);
    }

    /// <summary>단어의 배치는 그대로 두고 글자만 숨겨 가져간 자리를 표시한다.</summary>
    public void SetConsumed(bool consumed)
    {
        IsConsumed = consumed;
        if (label) label.text = consumed ? string.Empty : word;
        if (button) button.interactable = !consumed;
    }
}
