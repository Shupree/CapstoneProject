using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>퇴마프로그램의 화면 필터와 숨겨진 단서 표시 상태를 관리한다.</summary>
public sealed class ExploreProtoExorcismVision : MonoBehaviour
{
    [SerializeField] private Button toggleButton;
    [SerializeField] private TMP_Text buttonLabel;
    [SerializeField] private GameObject grayOverlay;

    public bool IsActive { get; private set; }
    public event Action<bool> StateChanged;

    private void Awake()
    {
        IsActive = false;
        grayOverlay.SetActive(false);
        buttonLabel.text = "퇴마";
        toggleButton.onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        IsActive = !IsActive;
        grayOverlay.SetActive(IsActive);
        buttonLabel.text = IsActive ? "정지" : "퇴마";
        StateChanged?.Invoke(IsActive);
    }
}
