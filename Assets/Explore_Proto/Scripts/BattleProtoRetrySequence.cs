using System;
using UnityEngine;

/// <summary>Switches the existing desktop theme when the battle conversation ends.</summary>
public sealed class BattleProtoRetrySequence : MonoBehaviour
{
    [Serializable]
    public struct SpriteChange
    {
        public UnityEngine.UI.Image image;
        public Sprite battleSprite;
    }
    public AudioSource effectsAudio, backgroundMusic;
    public AudioClip lightSwitchClip, battleMusic;
    public BattleMaryIntroController maryIntro;
    public GameObject[] windowsToClose;
    public SpriteChange backgroundChange;
    public SpriteChange[] spriteChanges;
    public bool HasStarted { get; private set; }
    public bool IsThemeApplied { get; private set; }
    public event Action ThemeApplied;
    Vector3[] checkpointWindowPositions;
    Sprite initialBackground;
    Sprite[] initialSprites;

    void Awake()
    {
        initialBackground = backgroundChange.image ? backgroundChange.image.sprite : null;
        initialSprites = new Sprite[spriteChanges == null ? 0 : spriteChanges.Length];
        for (int i = 0; i < initialSprites.Length; i++)
            initialSprites[i] = spriteChanges[i].image ? spriteChanges[i].image.sprite : null;
    }

    // 완료 상태를 보존한 채 통화 복귀용 화면 테마만 되돌린다.
    public void RestoreInitialVisuals()
    {
        if (backgroundChange.image) backgroundChange.image.sprite = initialBackground;
        if (initialSprites == null || spriteChanges == null) return;
        for (int i = 0; i < Mathf.Min(initialSprites.Length, spriteChanges.Length); i++)
            if (spriteChanges[i].image) spriteChanges[i].image.sprite = initialSprites[i];
    }

    public bool BeginThemeTransition()
    {
        if (HasStarted) return false;
        if (!effectsAudio || !backgroundMusic || !battleMusic || !maryIntro || !maryIntro.IsReady ||
            !backgroundChange.image || !backgroundChange.battleSprite || spriteChanges == null || windowsToClose == null)
        {
            Debug.LogError("배틀 테마 전환의 음원, 배경 또는 창 참조가 없습니다.", this);
            return false;
        }
        foreach (var change in spriteChanges)
            if (!change.image || !change.battleSprite) return false;
        HasStarted = true;
        var conversation = GetComponent<BattleProtoConversation>();
        if (conversation && conversation.programmingAudio) conversation.programmingAudio.Stop();
        if (maryIntro.Begin()) return true;
        HasStarted = false;
        return false;
    }

    // 암전이 화면을 가린 동안 테마를 교체한다.
    public void ApplyThemeAfterBlackout()
    {
        if (!HasStarted || IsThemeApplied) return;
        ApplyBattleTheme();
        IsThemeApplied = true;
        ThemeApplied?.Invoke();
    }

    void ApplyBattleTheme()
    {
        backgroundChange.image.sprite = backgroundChange.battleSprite;
        if (checkpointWindowPositions == null)
        {
            checkpointWindowPositions = new Vector3[windowsToClose.Length];
            for (int i = 0; i < windowsToClose.Length; i++)
                if (windowsToClose[i]) checkpointWindowPositions[i] = ((RectTransform)windowsToClose[i].transform).anchoredPosition3D;
        }
        foreach (var window in windowsToClose)
            if (window) window.SetActive(false);
        foreach (var change in spriteChanges) change.image.sprite = change.battleSprite;
    }

    public void StartBattleMusic()
    {
        if (!backgroundMusic || !battleMusic) return;
        if (backgroundMusic.isPlaying && backgroundMusic.clip == battleMusic) return;
        backgroundMusic.clip = battleMusic;
        backgroundMusic.loop = true;
        backgroundMusic.Play();
    }

    public void StopBattleMusic()
    {
        if (backgroundMusic) backgroundMusic.Stop();
    }

    public void RestoreBattleCheckpoint()
    {
        if (!IsThemeApplied) return;
        effectsAudio.Stop();
        for (int i = 0; i < windowsToClose.Length; i++)
            if (windowsToClose[i]) ((RectTransform)windowsToClose[i].transform).anchoredPosition3D = checkpointWindowPositions[i];
        ApplyBattleTheme();
    }
}
