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
    public enum PhaseState { NotStarted = 0, Running = 3, Completed = 4, Spawning = 5, Transition = 6 }
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
    [Header("이전 인터넷 진입 UI (사용하지 않음)")]
    public UnityEngine.UI.Button maryEntryButton;
    [Min(0f)] public float cardSpawnGapSeconds = .18f;
    [Range(.1f, 1f)] public float cardSpawnAcceleration = .9f;
    [Min(0f)] public float threatSeconds = 1f;
    [Tooltip("클릭/드래그로 연장되지 않는 파트별 제한 시간"), Min(.01f)] public float inactivityLimit = 10f;
    public Stage[] stages = { new Stage(9, 1), new Stage(12, 3), new Stage(18, 5) };
    [Tooltip("완료 후 확장용 이벤트. 현재 2페이즈에는 연결하지 않습니다.")]
    public UnityEvent onPhaseCompleted = new UnityEvent();
    public PhaseState State { get; private set; }
    public bool HasEntered => State != PhaseState.NotStarted;
    public bool IsBattleDesktop => retrySequence && retrySequence.IsThemeApplied;
    public bool IsCompleted => State == PhaseState.Completed;
    public int StageNumber => stageIndex + 1;
    public int ActiveImageCount { get; private set; }
    public int RemainingAttacks { get; private set; }
    public int BoardRevision { get; private set; }
    public int IntroCardCount => stages != null && stages.Length > 0 && stages[0].cards != null ? stages[0].cards.Length : 0;
    public bool CanInteract => State == PhaseState.Running && gameOver && !gameOver.IsBlocking && callWindow && callWindow.gameObject.activeInHierarchy;
    public float InactivitySeconds => State == PhaseState.Running ? (float)(Time.unscaledTimeAsDouble - stageStartedAt) : 0f;
    int stageIndex = -1;
    double stageStartedAt;
    Vector2 phaseOneAnchorMin, phaseOneAnchorMax, phaseOnePivot, phaseOneSize, phaseOnePosition;

    void Awake()
    {
        if (callWindow)
        {
            phaseOneAnchorMin = callWindow.anchorMin;
            phaseOneAnchorMax = callWindow.anchorMax;
            phaseOnePivot = callWindow.pivot;
            phaseOneSize = callWindow.sizeDelta;
            phaseOnePosition = callWindow.anchoredPosition;
            callWindow.gameObject.SetActive(false);
        }
        if (exorcismContent) exorcismContent.SetActive(false);
        if (clearedInternetWindow) clearedInternetWindow.gameObject.SetActive(false);
        if (maryEntryButton)
            maryEntryButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!HasEntered || IsCompleted || !gameOver || gameOver.IsBlocking) return;
        if (State == PhaseState.Running && InactivitySeconds >= inactivityLimit)
            gameOver.FailPhaseOne();
    }

    // 연출 동안 기존 1페이즈 창을 보여주되, 카드 입력은 아직 시작하지 않는다.
    public bool PrepareIntroWindow()
    {
        if (HasEntered || !IsBattleDesktop || !callWindow || !cardArea) return false;
        foreach (var panel in investigationPanels) if (panel) panel.SetActive(false);
        if (keywordPopup) keywordPopup.SetActive(false);
        if (internetWindow) internetWindow.SetActive(false);
        if (exorcismContent) exorcismContent.SetActive(false);
        if (clearedInternetWindow) clearedInternetWindow.gameObject.SetActive(false);
        foreach (var stage in stages) if (stage.root) stage.root.SetActive(false);
        if (maryEntryButton) maryEntryButton.gameObject.SetActive(false);
        RestorePhaseOneWindowLayout();
        callWindow.gameObject.SetActive(true);
        callWindow.SetAsLastSibling();
        return true;
    }

    // 이전 씬의 직렬화된 버튼 호출이 남아 있어도 인터넷에서 전투를 시작하지 않는다.
    public void BeginFromInternet()
    {
    }

    // 등장 연출에 쓸 기존 1페이즈 카드들을 숨긴 상태로 준비한다.
    public bool PrepareIntroCards()
    {
        if (!IsBattleDesktop || HasEntered || !HasValidConfiguration() || !callWindow.gameObject.activeInHierarchy) return false;
        foreach (var stage in stages) stage.root.SetActive(false);
        stageIndex = 0;
        BoardRevision++;
        ActiveImageCount = stages[0].imageCount;
        RemainingAttacks = stages[0].attackCount;
        State = PhaseState.Spawning;
        foreach (var card in stages[0].cards)
        {
            card.RestorePlacement();
            card.dismissButton.interactable = false;
            card.artwork.enabled = false;
            card.gameObject.SetActive(false);
        }
        stages[0].root.SetActive(true);
        return true;
    }

    public void RevealIntroCardFrame(int index)
    {
        if (State != PhaseState.Spawning || stageIndex != 0 || index < 0 || index >= IntroCardCount) return;
        stages[0].cards[index].gameObject.SetActive(true);
    }

    public void RevealIntroCard(int index)
    {
        if (State != PhaseState.Spawning || stageIndex != 0 || index < 0 || index >= IntroCardCount) return;
        var card = stages[0].cards[index];
        if (!card.gameObject.activeInHierarchy) return;
        card.transform.SetAsLastSibling();
        card.artwork.enabled = true;
    }

    public bool BeginAfterIntro()
    {
        if (!IsBattleDesktop || State != PhaseState.Spawning || stageIndex != 0 || !gameOver || gameOver.IsBlocking) return false;
        foreach (var card in stages[0].cards)
            if (!card.gameObject.activeInHierarchy || !card.artwork.enabled) return false;
        foreach (var card in stages[0].cards) card.dismissButton.interactable = true;
        retrySequence.StartBattleMusic();
        stageStartedAt = Time.unscaledTimeAsDouble;
        State = PhaseState.Running;
        return true;
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
        if (maryEntryButton) maryEntryButton.gameObject.SetActive(false);
        RestorePhaseOneWindowLayout();
        callWindow.gameObject.SetActive(true);
        callWindow.SetAsLastSibling();
        retrySequence.StartBattleMusic();
        StartStage(0);
    }

    void RestorePhaseOneWindowLayout()
    {
        // 1페이즈의 카드판은 씬에 저장된 원래 창 크기와 위치로 되돌린다.
        callWindow.anchorMin = phaseOneAnchorMin;
        callWindow.anchorMax = phaseOneAnchorMax;
        callWindow.pivot = phaseOnePivot;
        callWindow.sizeDelta = phaseOneSize;
        callWindow.anchoredPosition = phaseOnePosition;
    }

    bool HasValidConfiguration()
    {
        if (!gameOver || !callWindow || !cardArea ||
            !float.IsFinite(cardSpawnGapSeconds) || cardSpawnGapSeconds < 0f ||
            !float.IsFinite(cardSpawnAcceleration) || cardSpawnAcceleration <= 0f || cardSpawnAcceleration > 1f ||
            !float.IsFinite(inactivityLimit) || inactivityLimit <= 0f || investigationPanels == null ||
            stages == null || stages.Length != 3 || !cardArea.IsChildOf(callWindow)) return false;
        var unique = new HashSet<BloodyMaryCallCard>();
        foreach (var stage in stages)
        {
            if (!stage.root || !stage.root.transform.IsChildOf(cardArea) || stage.cards == null || stage.cards.Length != stage.imageCount) return false;
            int knives = 0;
            foreach (var card in stage.cards)
            {
                if (!card || !unique.Add(card) || card.phase != this || !card.artwork || !card.dismissButton || !card.threatOverlay || !card.transform.IsChildOf(stage.root.transform)) return false;
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
        StartCoroutine(RevealStage(order));
    }

    IEnumerator RevealStage(BloodyMaryCallCard[] order)
    {
        for (int i = 0; i < order.Length; i++)
        {
            order[i].transform.SetAsLastSibling();
            order[i].gameObject.SetActive(true);
            if (i + 1 < order.Length && cardSpawnGapSeconds > 0f)
                yield return new WaitForSecondsRealtime(cardSpawnGapSeconds * Mathf.Pow(cardSpawnAcceleration, i));
            else
                yield return null; // 마지막 카드가 렌더링된 뒤 입력과 제한 시간을 시작한다.
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
        if (stageIndex + 1 < stages.Length) { StartStage(stageIndex + 1); return; }
        StartCoroutine(EndPhaseOne());
    }

    IEnumerator EndPhaseOne()
    {
        State = PhaseState.Transition;
        ActiveImageCount = 0;
        BoardRevision++;
        // 18개의 창을 다시 드러낸 뒤 인물 대신 검은 경고 화면을 1초간 표시한다.
        foreach (var card in stages[stageIndex].cards)
        {
            card.gameObject.SetActive(true);
            card.ShowThreat();
        }
        yield return new WaitForSecondsRealtime(threatSeconds);
        stages[stageIndex].root.SetActive(false);
        State = PhaseState.Completed;
        onPhaseCompleted.Invoke();
        if (phaseTwo) phaseTwo.BeginPhase();
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
    public void ToggleInternet()
    {
        // 테마 전환 뒤에는 인터넷 진입 없이 연출과 1페이즈가 이어진다.
    }
    public void CloseInternet()
    {
        if (!HasEntered && internetWindow) internetWindow.SetActive(false);
    }
    public void SelectImage(int index, int pressedRevision) { }
}
