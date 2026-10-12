using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Runs the image phase using only the slots already saved in Battle_Proto.</summary>
public sealed class BloodyMaryPhaseController : MonoBehaviour
{
    [Serializable]
    public struct Stage
    {
        [Min(1)] public int imageCount;
        [Min(1)] public int attackCount;
        public Stage(int images, int attacks) { imageCount = images; attackCount = attacks; }
    }

    public BattleProtoRetrySequence retrySequence;
    public GameObject internetWindow, exorcismContent, keywordPopup;
    public RectTransform clearedInternetWindow;
    public BloodyMaryPhaseTwoController phaseTwo;
    public BloodyMaryGameOverController gameOver;
    [Min(.01f)] public float inactivityLimit = 10f;
    [Tooltip("Existing investigation panels to hide after the battle theme has been applied.")]
    public GameObject[] investigationPanels;
    public Sprite normalSprite, attackSprite;
    public Stage[] stages = { new Stage(12, 1), new Stage(18, 2), new Stage(24, 3) };
    public BloodyMaryImageSlot[] slots;
    public UnityEngine.UI.ScrollRect imageScroll;
    [Tooltip("Invoked once after the last attack image is cleared. No follow-up is assigned by default.")]
    public UnityEvent onPhaseCompleted = new UnityEvent();

    public bool HasEntered { get; private set; }
    public bool IsCompleted { get; private set; }
    public int StageNumber => stageIndex + 1;
    public int ActiveImageCount { get; private set; }
    public int RemainingAttacks { get; private set; }
    public int BoardRevision { get; private set; }
    public bool CanInteract => HasEntered && !IsCompleted && gameOver && !gameOver.IsBlocking &&
        internetWindow && internetWindow.activeInHierarchy && exorcismContent && exorcismContent.activeInHierarchy;
    public float InactivitySeconds { get; private set; }

    int stageIndex = -1, lastInputFrame = -1;
    bool changingBoard, resetScrollWhenOpened;
    bool[] attacks;
    int[] candidates;
    Vector2 closedScrollPosition;

    void Update()
    {
        if (!CanInteract) return;
        InactivitySeconds += Time.unscaledDeltaTime;
        if (InactivitySeconds >= inactivityLimit) gameOver.FailPhaseOne();
    }

    public void RestartAfterGameOver()
    {
        if (!retrySequence || !retrySequence.IsThemeApplied) return;
        phaseTwo.ResetForRetry();
        HasEntered = IsCompleted = false;
        changingBoard = false;
        lastInputFrame = -1;
        InactivitySeconds = 0f;
        clearedInternetWindow.gameObject.SetActive(false);
        BeginPhase();
    }

    void Awake()
    {
        // The controller lives on the always-active desktop, not on the closable window.
        if (exorcismContent) exorcismContent.SetActive(false);
        if (clearedInternetWindow) clearedInternetWindow.gameObject.SetActive(false);
        if (retrySequence) retrySequence.ThemeApplied += BeginPhase;
    }

    void Start()
    {
        // Handles a late-enabled controller without using the earlier HasStarted state.
        if (retrySequence && retrySequence.IsThemeApplied) BeginPhase();
    }

    void OnDestroy()
    {
        if (retrySequence) retrySequence.ThemeApplied -= BeginPhase;
    }

    void BeginPhase()
    {
        if (HasEntered || !retrySequence || !retrySequence.IsThemeApplied) return;
        if (!HasValidConfiguration())
        {
            Debug.LogError("블러디 메리 페이즈의 슬롯, 단계 또는 씬 참조를 확인하세요.", this);
            return;
        }
        attacks = new bool[slots.Length];
        candidates = new int[slots.Length];
        HasEntered = true;
        InactivitySeconds = 0f;
        foreach (var panel in investigationPanels) panel.SetActive(false);
        if (keywordPopup) keywordPopup.SetActive(false);
        internetWindow.SetActive(false);
        exorcismContent.SetActive(true);
        StartStage(0);
        // The player opens the existing internet window with the desktop icon.
    }

    bool HasValidConfiguration()
    {
        if (!gameOver || !phaseTwo || !(inactivityLimit > 0f) || float.IsInfinity(inactivityLimit) ||
            !internetWindow || !clearedInternetWindow || !exorcismContent || !normalSprite || !attackSprite ||
            !imageScroll || !imageScroll.content || !imageScroll.viewport ||
            slots == null || slots.Length == 0 || stages == null || stages.Length == 0 ||
            investigationPanels == null) return false;
        if (!exorcismContent.transform.IsChildOf(internetWindow.transform)) return false;
        var unique = new HashSet<BloodyMaryImageSlot>();
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (!slot || !unique.Add(slot) || slot.phase != this || slot.slotIndex != i ||
                !slot.artwork || !slot.button || !slot.transform.IsChildOf(imageScroll.content)) return false;
        }
        int previousCount = 0;
        foreach (var stage in stages)
        {
            if (stage.imageCount <= previousCount || stage.imageCount > slots.Length ||
                stage.attackCount < 1 || stage.attackCount > stage.imageCount) return false;
            previousCount = stage.imageCount;
        }
        foreach (var panel in investigationPanels)
            if (!panel || panel == internetWindow || exorcismContent.transform.IsChildOf(panel.transform)) return false;
        return true;
    }

    void StartStage(int index)
    {
        changingBoard = true;
        try
        {
            stageIndex = index;
            BoardRevision++;
            ActiveImageCount = stages[index].imageCount;
            RemainingAttacks = stages[index].attackCount;
            Array.Clear(attacks, 0, attacks.Length);
            for (int i = 0; i < ActiveImageCount; i++) candidates[i] = i;
            // Partial Fisher-Yates: every active slot is eligible, with no duplicate picks.
            for (int i = 0; i < RemainingAttacks; i++)
            {
                int pick = UnityEngine.Random.Range(i, ActiveImageCount);
                int chosen = candidates[pick];
                candidates[pick] = candidates[i];
                candidates[i] = chosen;
                attacks[chosen] = true;
            }
            for (int i = 0; i < slots.Length; i++)
            {
                bool visible = i < ActiveImageCount;
                slots[i].SetAppearance(attacks[i] ? attackSprite : normalSprite, visible);
                slots[i].gameObject.SetActive(visible);
            }
            resetScrollWhenOpened = true;
            if (imageScroll.gameObject.activeInHierarchy) ResetScrollToTop();
        }
        finally { changingBoard = false; }
    }

    public void SelectImage(int index, int pressedRevision)
    {
        if (!CanInteract || changingBoard || !internetWindow.activeInHierarchy ||
            !exorcismContent.activeInHierarchy || pressedRevision != BoardRevision ||
            index < 0 || index >= ActiveImageCount || lastInputFrame == Time.frameCount) return;
        lastInputFrame = Time.frameCount;
        InactivitySeconds = 0f;
        if (!attacks[index])
        {
            // Reset this phase only. Investigation, memo, battle theme and music are untouched.
            StartStage(0);
            return;
        }
        changingBoard = true;
        attacks[index] = false;
        BoardRevision++;
        slots[index].SetAppearance(normalSprite, true);
        RemainingAttacks--;
        if (RemainingAttacks != 0) { changingBoard = false; return; }
        if (stageIndex + 1 < stages.Length) { StartStage(stageIndex + 1); return; }

        IsCompleted = true;
        for (int i = 0; i < ActiveImageCount; i++) slots[i].SetAppearance(normalSprite, false);
        changingBoard = false;
        OpenClearedInternetWindow();
        // Set completion before notifying listeners so reentrant calls cannot complete twice.
        onPhaseCompleted.Invoke();
    }

    public void ToggleInternet()
    {
        if (!HasEntered || (gameOver && gameOver.IsBlocking)) return;
        if (IsCompleted)
        {
            if (phaseTwo && phaseTwo.IsCompleted) return;
            bool open = !clearedInternetWindow.gameObject.activeSelf;
            clearedInternetWindow.gameObject.SetActive(open);
            if (open) clearedInternetWindow.SetAsLastSibling();
            return;
        }
        if (internetWindow.activeSelf) { CloseInternet(); return; }
        internetWindow.SetActive(true);
        internetWindow.transform.SetAsLastSibling();
        if (resetScrollWhenOpened) ResetScrollToTop();
        else
        {
            RebuildScrollLayout();
            imageScroll.StopMovement();
            imageScroll.content.anchoredPosition = closedScrollPosition;
        }
    }

    public void CloseInternet()
    {
        if (!HasEntered || (gameOver && gameOver.IsBlocking)) return;
        if (IsCompleted)
        {
            clearedInternetWindow.gameObject.SetActive(false);
            return;
        }
        if (internetWindow.activeSelf)
        {
            InactivitySeconds = 0f;
            imageScroll.StopMovement();
            closedScrollPosition = imageScroll.content.anchoredPosition;
        }
        internetWindow.SetActive(false);
        if (keywordPopup) keywordPopup.SetActive(false);
    }

    void OpenClearedInternetWindow()
    {
        var previous = (RectTransform)internetWindow.transform;
        clearedInternetWindow.anchorMin = previous.anchorMin;
        clearedInternetWindow.anchorMax = previous.anchorMax;
        clearedInternetWindow.pivot = previous.pivot;
        clearedInternetWindow.sizeDelta = previous.sizeDelta;
        clearedInternetWindow.anchoredPosition3D = previous.anchoredPosition3D;
        clearedInternetWindow.localRotation = previous.localRotation;
        clearedInternetWindow.localScale = previous.localScale;
        imageScroll.StopMovement();
        internetWindow.SetActive(false);
        if (keywordPopup) keywordPopup.SetActive(false);
        clearedInternetWindow.gameObject.SetActive(true);
        clearedInternetWindow.SetAsLastSibling();
    }

    void ResetScrollToTop()
    {
        RebuildScrollLayout();
        imageScroll.StopMovement();
        imageScroll.verticalNormalizedPosition = 1f;
        closedScrollPosition = imageScroll.content.anchoredPosition;
        resetScrollWhenOpened = false;
    }

    void RebuildScrollLayout()
    {
        // Layout only: the scene-authored hierarchy is never instantiated or reconstructed.
        Canvas.ForceUpdateCanvases();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(imageScroll.content);
    }
}
