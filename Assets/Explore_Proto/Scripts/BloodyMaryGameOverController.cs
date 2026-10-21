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
    public AudioSource handsAudio, signalLostAudio;
    [Range(0f, 5f)] public float handsVolumeMultiplier = 1f;
    [Min(.01f)] public float signalFadeSeconds = 1f;
    [Min(0f)] public float failureHoldSeconds = 2f;
    [Min(.01f)] public float handInterval = .07f;
    [Min(0f)] public float handsHoldSeconds = .4f;
    public GameOverState State { get; private set; }
    public bool IsBlocking => State != GameOverState.Idle;
    bool configured;
    bool signalVolumeCaptured;
    float signalStartVolume;

    void Awake()
    {
        configured = phaseOne && phaseTwo && retrySequence && overlayRoot && dimScreen &&
            centeredAttack && blackout && signalLostScreen && phaseTwoWarning && retryButton && exitButton &&
            handsAudio && handsAudio.clip && signalLostAudio && signalLostAudio.clip && handsAudio != signalLostAudio &&
            signalFadeSeconds > 0f && !float.IsInfinity(signalFadeSeconds) &&
            hands != null && hands.Length == 12;
        if (configured)
        {
            var unique = new System.Collections.Generic.HashSet<GameObject>();
            foreach (var hand in hands)
                if (!hand || !unique.Add(hand) || !hand.transform.IsChildOf(blackout.transform)) configured = false;
        }
        if (!configured)
        {
            Debug.LogError("게임 오버 씬 참조, 효과음 또는 손 이미지 12개를 확인하세요.", this);
            return;
        }
        HidePresentation();
        StopGameOverAudio();
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
        StopGameOverAudio();
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
        overlayRoot.SetActive(true);
        retryButton.interactable = false;
        exitButton.interactable = false;
        if (phaseOne.imageScroll) phaseOne.imageScroll.StopMovement();
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
        yield return new WaitForSecondsRealtime(handInterval);
        double handsStarted = Time.unscaledTimeAsDouble;
        for (int i = 0; i < hands.Length; i++)
        {
            // Use one timeline so frame rounding does not accumulate between the twelve images.
            while (Time.unscaledTimeAsDouble < handsStarted + i * (double)handInterval) yield return null;
            hands[i].SetActive(true); // Position, size, rotation and opacity remain as authored in the scene.
            if (i == 0) handsAudio.PlayOneShot(handsAudio.clip, Mathf.Clamp(handsVolumeMultiplier, 0f, 5f));
        }
        double redAt = handsStarted + (hands.Length - 1) * (double)handInterval + handsHoldSeconds;
        while (Time.unscaledTimeAsDouble < redAt) yield return null;
        handsAudio.Stop();
        signalLostScreen.SetActive(true);
        signalStartVolume = signalLostAudio.volume;
        signalVolumeCaptured = true;
        signalLostAudio.Play();
        StartCoroutine(FadeSignalEnding());
        State = GameOverState.SignalLost;
        retryButton.interactable = true;
        exitButton.interactable = true;
    }

    public void Retry()
    {
        if (!configured || State != GameOverState.SignalLost) return;
        StopAllCoroutines();
        StopGameOverAudio();
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

    IEnumerator FadeSignalEnding()
    {
        // Wait for playback to start, then use the actual sample position, including MP3 playback.
        yield return null;
        float fadeDuration = Mathf.Min(signalFadeSeconds, signalLostAudio.clip.length);
        while (signalLostAudio.isPlaying)
        {
            float secondsLeft = (signalLostAudio.clip.samples - signalLostAudio.timeSamples) /
                (float)signalLostAudio.clip.frequency;
            signalLostAudio.volume = signalStartVolume * Mathf.Clamp01(secondsLeft / fadeDuration);
            yield return null;
        }
        RestoreSignalVolume();
    }

    void RestoreSignalVolume()
    {
        if (signalVolumeCaptured && signalLostAudio) signalLostAudio.volume = signalStartVolume;
        signalVolumeCaptured = false;
    }

    void StopGameOverAudio()
    {
        if (handsAudio) handsAudio.Stop();
        if (signalLostAudio) signalLostAudio.Stop();
        RestoreSignalVolume();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        StopGameOverAudio();
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
