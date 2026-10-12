using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Sequences the saved failure overlays and retries the existing battle checkpoint.</summary>
public sealed class BloodyMaryGameOverController : MonoBehaviour
{
    public enum GameOverState { Idle, PhaseFailure, Hands, SignalLost }
    public BloodyMaryPhaseController phaseOne;
    public BloodyMaryPhaseTwoController phaseTwo;
    public BattleProtoRetrySequence retrySequence;
    public GameObject overlayRoot, dimScreen, centeredAttack, blackout, signalLostScreen;
    public GameObject phaseTwoWarning;
    public GameObject[] hands;
    public UnityEngine.UI.Button retryButton, exitButton;
    [Min(0f)] public float failureHoldSeconds = 2f;
    [Min(.01f)] public float handInterval = .07f;
    [Min(0f)] public float handsHoldSeconds = .4f;
    public GameOverState State { get; private set; }
    public bool IsBlocking => State != GameOverState.Idle;
    bool configured;

    void Awake()
    {
        configured = phaseOne && phaseTwo && retrySequence && overlayRoot && dimScreen &&
            centeredAttack && blackout && signalLostScreen && phaseTwoWarning && retryButton && exitButton &&
            hands != null && hands.Length == 12;
        if (configured)
        {
            var unique = new System.Collections.Generic.HashSet<GameObject>();
            foreach (var hand in hands)
                if (!hand || !unique.Add(hand) || !hand.transform.IsChildOf(blackout.transform)) configured = false;
        }
        if (!configured)
        {
            Debug.LogError("게임 오버 씬 참조 또는 손 이미지 12개를 확인하세요.", this);
            return;
        }
        HidePresentation();
    }

    public void FailPhaseOne()
    {
        if (!configured || IsBlocking || !phaseOne.CanInteract) return;
        BeginFailure(false);
    }

    public void FailPhaseTwo()
    {
        if (!configured || IsBlocking || phaseTwo.State != BloodyMaryPhaseTwoController.PhaseState.Running) return;
        BeginFailure(true);
    }

    void BeginFailure(bool secondPhase)
    {
        State = GameOverState.PhaseFailure; // Lock gameplay before activating any overlay.
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
        overlayRoot.SetActive(true);
        retryButton.interactable = false;
        exitButton.interactable = false;
        phaseOne.imageScroll.StopMovement();
        if (secondPhase)
        {
            phaseTwo.StopForGameOver();
            phaseTwo.window.gameObject.SetActive(true);
            phaseTwo.window.SetAsLastSibling();
            phaseTwoWarning.SetActive(true);
            phaseTwoWarning.transform.SetAsLastSibling();
        }
        else
        {
            dimScreen.SetActive(true);
            centeredAttack.SetActive(true);
        }
        StartCoroutine(PresentGameOver());
    }

    IEnumerator PresentGameOver()
    {
        yield return new WaitForSecondsRealtime(failureHoldSeconds);
        blackout.SetActive(true); // Higher canvas order also covers every desktop window and button.
        dimScreen.SetActive(false);
        centeredAttack.SetActive(false);
        phaseTwoWarning.SetActive(false);
        State = GameOverState.Hands;
        foreach (var hand in hands)
        {
            yield return new WaitForSecondsRealtime(handInterval);
            hand.SetActive(true); // Position, size, rotation and opacity remain as authored in the scene.
        }
        yield return new WaitForSecondsRealtime(handsHoldSeconds);
        signalLostScreen.SetActive(true);
        State = GameOverState.SignalLost;
        retryButton.interactable = true;
        exitButton.interactable = true;
    }

    public void Retry()
    {
        if (!configured || State != GameOverState.SignalLost) return;
        StopAllCoroutines();
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
        retrySequence.RestoreBattleCheckpoint();
        phaseOne.RestartAfterGameOver();
        HidePresentation();
        State = GameOverState.Idle;
    }

    public void Exit()
    {
        // Reserved for the later exit flow.
    }

    void HidePresentation()
    {
        foreach (var hand in hands) hand.SetActive(false);
        dimScreen.SetActive(false);
        centeredAttack.SetActive(false);
        blackout.SetActive(false);
        signalLostScreen.SetActive(false);
        phaseTwoWarning.SetActive(false);
        overlayRoot.SetActive(false);
        retryButton.interactable = exitButton.interactable = false;
    }
}
