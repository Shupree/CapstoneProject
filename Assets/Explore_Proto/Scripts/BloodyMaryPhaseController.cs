using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Three call-card parts, using prefab instances already saved in Battle_Proto.</summary>
public sealed class BloodyMaryPhaseController : MonoBehaviour
{
    [Serializable]
    public struct Stage
    {
        [Min(1)] public int imageCount;
        [Min(1)] public int attackCount;
        public GameObject root;
        public BloodyMaryCallCard[] cards;
        public Stage(int images, int attacks) { imageCount = images; attackCount = attacks; root = null; cards = null; }
    }
    public enum PhaseState { NotStarted, Confrontation, Dialogue, Running, Completed, Spawning }
    public BattleProtoRetrySequence retrySequence;
    [Header("기존 인터넷 UI 참조 (배틀에서 숨김)")]
    public GameObject internetWindow, exorcismContent, keywordPopup;
    public RectTransform clearedInternetWindow;
    public BloodyMaryPhaseTwoController phaseTwo;
    public BloodyMaryGameOverController gameOver;
    public GameObject[] investigationPanels;
    // Retained for serialized legacy slots and the existing game-over integration.
    [HideInInspector] public Sprite normalSprite, attackSprite;
    [HideInInspector] public BloodyMaryImageSlot[] slots;
    [HideInInspector] public UnityEngine.UI.ScrollRect imageScroll;

    [Header("씬에 저장된 화상통화 UI")]
    public RectTransform callWindow, cardArea;
    public GameObject confrontationRoot, dialogueRoot;
    [Min(0f)] public float cardSpawnGapSeconds = .1f;
    [Min(0f)] public float confrontationSeconds = 1f, dialogueSeconds = 2f;
    [Tooltip("클릭/드래그로 연장되지 않는 파트별 제한 시간"), Min(.01f)] public float inactivityLimit = 10f;
    public Stage[] stages = { new Stage(9, 1), new Stage(12, 3), new Stage(18, 5) };
    [Tooltip("완료 후 확장용 이벤트. 현재 2페이즈에는 연결하지 않습니다.")]
    public UnityEvent onPhaseCompleted = new UnityEvent();
    public PhaseState State { get; private set; }
    public bool HasEntered => State != PhaseState.NotStarted;
    public bool IsCompleted => State == PhaseState.Completed;
    public int StageNumber => stageIndex + 1;
    public int ActiveImageCount { get; private set; }
    public int RemainingAttacks { get; private set; }
    public int BoardRevision { get; private set; }
    public bool CanInteract => State == PhaseState.Running && gameOver && !gameOver.IsBlocking && callWindow && callWindow.gameObject.activeInHierarchy;
    public float InactivitySeconds => State == PhaseState.Running ? (float)(Time.unscaledTimeAsDouble - stageStartedAt) : 0f;
    int stageIndex = -1;
    double stateStartedAt, stageStartedAt;

    void Awake()
    {
        if (callWindow) callWindow.gameObject.SetActive(false);
        if (exorcismContent) exorcismContent.SetActive(false);
        if (clearedInternetWindow) clearedInternetWindow.gameObject.SetActive(false);
        if (retrySequence) retrySequence.ThemeApplied += BeginPhase;
    }
    void Start() { if (retrySequence && retrySequence.IsThemeApplied) BeginPhase(); }
    void OnDestroy() { if (retrySequence) retrySequence.ThemeApplied -= BeginPhase; }

    void Update()
    {
        if (!HasEntered || IsCompleted || !gameOver || gameOver.IsBlocking) return;
        if (State == PhaseState.Confrontation && Time.unscaledTimeAsDouble - stateStartedAt >= confrontationSeconds)
        {
            dialogueRoot.SetActive(true);
            State = PhaseState.Dialogue;
            stateStartedAt = Time.unscaledTimeAsDouble;
        }
        else if (State == PhaseState.Dialogue && Time.unscaledTimeAsDouble - stateStartedAt >= dialogueSeconds)
        {
            confrontationRoot.SetActive(false);
            dialogueRoot.SetActive(false);
            StartStage(0);
        }
        else if (State == PhaseState.Running && InactivitySeconds >= inactivityLimit)
            gameOver.FailPhaseOne();
    }

    void BeginPhase()
    {
        if (HasEntered || !retrySequence || !retrySequence.IsThemeApplied) return;
        if (!HasValidConfiguration())
        {
            Debug.LogError("화상통화 1페이즈의 카드, 단계 또는 씬 참조를 확인하세요.", this);
            return;
        }
        foreach (var panel in investigationPanels) if (panel) panel.SetActive(false);
        if (keywordPopup) keywordPopup.SetActive(false);
        if (internetWindow) internetWindow.SetActive(false);
        if (exorcismContent) exorcismContent.SetActive(false);
        if (clearedInternetWindow) clearedInternetWindow.gameObject.SetActive(false);
        foreach (var stage in stages) stage.root.SetActive(false);
        stageIndex = -1;
        ActiveImageCount = RemainingAttacks = 0;
        callWindow.gameObject.SetActive(true);
        callWindow.SetAsLastSibling();
        confrontationRoot.SetActive(true);
        dialogueRoot.SetActive(false);
        State = PhaseState.Confrontation;
        stateStartedAt = Time.unscaledTimeAsDouble;
    }

    bool HasValidConfiguration()
    {
        if (!gameOver || !callWindow || !cardArea || !confrontationRoot || !dialogueRoot ||
            !float.IsFinite(cardSpawnGapSeconds) || cardSpawnGapSeconds < 0f ||
            !float.IsFinite(inactivityLimit) || inactivityLimit <= 0f || investigationPanels == null ||
            stages == null || stages.Length != 3 || !cardArea.IsChildOf(callWindow)) return false;
        var unique = new HashSet<BloodyMaryCallCard>();
        foreach (var stage in stages)
        {
            if (!stage.root || !stage.root.transform.IsChildOf(cardArea) || stage.cards == null || stage.cards.Length != stage.imageCount) return false;
            int knives = 0;
            foreach (var card in stage.cards)
            {
                if (!card || !unique.Add(card) || card.phase != this || !card.artwork || !card.dismissButton || !card.transform.IsChildOf(stage.root.transform)) return false;
                if (card.isKnife) knives++;
            }
            if (knives != stage.attackCount || knives < 1) return false;
        }
        return true;
    }

    void StartStage(int index)
    {
        foreach (var stage in stages) stage.root.SetActive(false);
        stageIndex = index;
        BoardRevision++;
        var current = stages[index];
        ActiveImageCount = current.imageCount;
        RemainingAttacks = current.attackCount;
        State = PhaseState.Spawning;
        // Set final positions while hidden. Revealing never moves the cards.
        foreach (var card in current.cards)
        {
            card.RestorePlacement();
            card.dismissButton.interactable = false;
            card.gameObject.SetActive(false);
            if (index > 0) card.PlaceRandomly();
        }
        var order = (BloodyMaryCallCard[])current.cards.Clone();
        if (index > 0)
        {
            // Random reveal/depth order prevents knives from always appearing first.
            for (int i = order.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var swap = order[i]; order[i] = order[j]; order[j] = swap;
            }
        }
        current.root.SetActive(true);
        StartCoroutine(RevealStage(order, index == 2 ? 2 : 1));
    }

    IEnumerator RevealStage(BloodyMaryCallCard[] order, int batchSize)
    {
        for (int i = 0; i < order.Length; i += batchSize)
        {
            int end = Mathf.Min(i + batchSize, order.Length);
            for (int j = i; j < end; j++)
            {
                order[j].transform.SetAsLastSibling();
                order[j].gameObject.SetActive(true);
            }
            if (end < order.Length && cardSpawnGapSeconds > 0f)
                yield return new WaitForSecondsRealtime(cardSpawnGapSeconds);
            else
                yield return null; // Render the final card before unlocking input and starting the clock.
        }
        foreach (var card in order) card.dismissButton.interactable = true;
        stageStartedAt = Time.unscaledTimeAsDouble;
        State = PhaseState.Running;
    }

    public void DismissCard(BloodyMaryCallCard card)
    {
        if (!CanInteract || !card || !card.gameObject.activeInHierarchy || !card.isKnife ||
            Array.IndexOf(stages[stageIndex].cards, card) < 0) return;
        // Enforce the deadline even if the input module runs before this frame's Update.
        if (InactivitySeconds >= inactivityLimit) { gameOver.FailPhaseOne(); return; }
        card.gameObject.SetActive(false);
        RemainingAttacks--;
        if (RemainingAttacks > 0) return;
        stages[stageIndex].root.SetActive(false);
        if (stageIndex + 1 < stages.Length) { StartStage(stageIndex + 1); return; }
        State = PhaseState.Completed;
        ActiveImageCount = 0;
        BoardRevision++;
        // Keep the empty call window. A future phase can subscribe without changing this controller.
        onPhaseCompleted.Invoke();
    }

    public void RestartAfterGameOver()
    {
        if (!retrySequence || !retrySequence.IsThemeApplied) return;
        StopAllCoroutines();
        if (phaseTwo) phaseTwo.ResetForRetry();
        State = PhaseState.NotStarted;
        BeginPhase();
    }

    // Existing desktop and old saved image buttons retain valid targets, but cannot reopen the old puzzle.
    public void ToggleInternet() { }
    public void CloseInternet() { }
    public void SelectImage(int index, int pressedRevision) { }
}
