using System;
using System.Collections;
using UnityEngine;

/// <summary>Plays the second attempt and changes the existing desktop to its battle theme.</summary>
public sealed class BattleProtoRetrySequence : MonoBehaviour
{
    [Serializable]
    public struct SpriteChange
    {
        public UnityEngine.UI.Image image;
        public Sprite battleSprite;
    }
    public AudioSource effectsAudio, backgroundMusic;
    public AudioClip programmingClip, whiteNoiseClip, lightSwitchClip, battleMusic;
    [Min(0)] public float delayAfterProgramming = 2f;
    [Header("테마 전환 전 화상 통화")]
    [InspectorName("중간 통화 유지 시간 (초)"), Min(0)]
    public float intermediateCallSeconds = 1.5f;
    public GameObject intermediateCallWindow;
    public GameObject intermediateCallRoot;
    public GameObject[] normalCallObjects;
    public GameObject[] windowsToClose;
    public SpriteChange backgroundChange;
    public SpriteChange[] spriteChanges;
    public bool HasStarted { get; private set; }
    public bool IsThemeApplied { get; private set; }
    public event Action ThemeApplied;
    Vector3[] checkpointWindowPositions;

    public bool BeginRetry()
    {
        if (HasStarted) return false;
        if (!effectsAudio || !backgroundMusic || !programmingClip || !whiteNoiseClip || !lightSwitchClip || !battleMusic)
        {
            Debug.LogError("재시도 연출의 음원 참조가 없습니다.", this);
            return false;
        }
        if (!intermediateCallWindow || !intermediateCallRoot || normalCallObjects == null)
        {
            Debug.LogError("테마 전환 전 화상 통화 참조가 없습니다.", this);
            return false;
        }
        if (!backgroundChange.image || !backgroundChange.battleSprite)
        {
            Debug.LogError("배틀 배경 참조가 없습니다.", this);
            return false;
        }
        foreach (var change in spriteChanges)
            if (!change.image || !change.battleSprite)
            {
                Debug.LogError("배틀 배경 또는 아이콘 참조가 없습니다.", this);
                return false;
            }
        HasStarted = true;
        StartCoroutine(Run());
        return true;
    }

    IEnumerator Run()
    {
        yield return PlayEffect(programmingClip);
        yield return new WaitForSecondsRealtime(delayAfterProgramming);
        yield return PlayEffect(whiteNoiseClip);

        effectsAudio.clip = lightSwitchClip;
        effectsAudio.Play();
        ApplyBattleBackground();
        CaptureCheckpointWindowPositions();
        foreach (var window in windowsToClose)
            if (window && window != intermediateCallWindow) window.SetActive(false);
        foreach (var item in normalCallObjects)
            if (item) item.SetActive(false);
        // The new call phase owns its 1-second confrontation and 2-second dialogue.
        // Preserve the old intermediate call for scenes that have no call-card board.
        var phase = GetComponent<BloodyMaryPhaseController>();
        if (!phase || !phase.callWindow)
        {
            intermediateCallWindow.SetActive(true);
            intermediateCallWindow.transform.SetAsLastSibling();
            intermediateCallRoot.SetActive(true);
            if (intermediateCallSeconds > 0f)
                yield return new WaitForSecondsRealtime(intermediateCallSeconds);
        }
        ApplyBattleTheme();
        IsThemeApplied = true;
        ThemeApplied?.Invoke();
    }

    void ApplyBattleTheme()
    {
        intermediateCallRoot.SetActive(false);
        ApplyBattleBackground();
        CaptureCheckpointWindowPositions();
        backgroundMusic.clip = battleMusic;
        backgroundMusic.loop = true;
        backgroundMusic.Play();
        foreach (var window in windowsToClose)
            if (window) window.SetActive(false);
        foreach (var change in spriteChanges)
            change.image.sprite = change.battleSprite;
    }

    void CaptureCheckpointWindowPositions()
    {
        if (checkpointWindowPositions != null) return;
        checkpointWindowPositions = new Vector3[windowsToClose.Length];
        for (int i = 0; i < windowsToClose.Length; i++)
            checkpointWindowPositions[i] = ((RectTransform)windowsToClose[i].transform).anchoredPosition3D;
    }

    void ApplyBattleBackground()
    {
        backgroundChange.image.sprite = backgroundChange.battleSprite;
    }

    public void RestoreBattleCheckpoint()
    {
        if (!IsThemeApplied) return;
        StopAllCoroutines();
        effectsAudio.Stop();
        var conversation = GetComponent<BattleProtoConversation>();
        if (conversation && conversation.programmingAudio) conversation.programmingAudio.Stop();
        for (int i = 0; i < windowsToClose.Length; i++)
            ((RectTransform)windowsToClose[i].transform).anchoredPosition3D = checkpointWindowPositions[i];
        ApplyBattleTheme();
    }

    IEnumerator PlayEffect(AudioClip clip)
    {
        effectsAudio.loop = false;
        effectsAudio.clip = clip;
        effectsAudio.Play();
        yield return null;
        while (effectsAudio.isPlaying) yield return null;
    }
}
