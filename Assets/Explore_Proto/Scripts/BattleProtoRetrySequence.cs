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
    public GameObject[] windowsToClose;
    public SpriteChange[] spriteChanges;
    public bool HasStarted { get; private set; }

    public bool BeginRetry()
    {
        if (HasStarted) return false;
        if (!effectsAudio || !backgroundMusic || !programmingClip || !whiteNoiseClip || !lightSwitchClip || !battleMusic)
        {
            Debug.LogError("재시도 연출의 음원 참조가 없습니다.", this);
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
        backgroundMusic.clip = battleMusic;
        backgroundMusic.loop = true;
        backgroundMusic.Play();
        foreach (var window in windowsToClose)
            if (window) window.SetActive(false);
        foreach (var change in spriteChanges)
            change.image.sprite = change.battleSprite;
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
