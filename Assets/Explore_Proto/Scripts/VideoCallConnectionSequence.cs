using System;
using System.Collections;
using UnityEngine;

/// <summary>씬에 배치된 화상통화 UI에서 입장음과 연결음을 순서대로 재생한다.</summary>
public sealed class VideoCallConnectionSequence : MonoBehaviour
{
    public enum ConnectionMode
    {
        Incoming,
        Outgoing,
        UrgentOutgoing
    }

    [Header("효과음")]
    public AudioSource audioSource;
    public AudioClip enterCall;
    public AudioClip waitCall;
    [Min(0f)] public float waitCallSeconds = 2.5f;

    [Header("씬에 저장된 통화 UI")]
    public GameObject remotePortrait;
    public GameObject connectingIndicator;

    Coroutine currentSequence;

    public bool IsConnecting => currentSequence != null;

    /// <summary>상대가 입장하는 EnterCall과 동시에 이미지를 표시하고, 연출이 끝나면 onConnected를 호출한다.</summary>
    public bool TryBegin(ConnectionMode mode, Action onConnected)
    {
        if (IsConnecting || !audioSource || !enterCall ||
            (mode == ConnectionMode.Outgoing && !waitCall))
        {
            Debug.LogError("화상통화 연결 효과음 또는 AudioSource 참조를 확인해 주세요.", this);
            return false;
        }

        if (remotePortrait) remotePortrait.SetActive(false);
        if (connectingIndicator) connectingIndicator.SetActive(true);
        currentSequence = StartCoroutine(PlaySequence(mode, onConnected));
        return true;
    }

    IEnumerator PlaySequence(ConnectionMode mode, Action onConnected)
    {
        if (mode == ConnectionMode.Incoming) RevealRemote();
        yield return PlayClip(enterCall);

        if (mode == ConnectionMode.Outgoing)
        {
            audioSource.loop = true;
            audioSource.clip = waitCall;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(waitCallSeconds);
            audioSource.Stop();
            audioSource.loop = false;
            RevealRemote();
            yield return PlayClip(enterCall);
        }
        else if (mode == ConnectionMode.UrgentOutgoing)
        {
            RevealRemote();
            yield return PlayClip(enterCall);
        }

        currentSequence = null;
        onConnected?.Invoke();
    }

    void RevealRemote()
    {
        if (remotePortrait) remotePortrait.SetActive(true);
        if (connectingIndicator) connectingIndicator.SetActive(false);
    }

    IEnumerator PlayClip(AudioClip clip)
    {
        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(clip.length / Mathf.Max(0.01f, Mathf.Abs(audioSource.pitch)));
        audioSource.Stop();
    }

    /// <summary>통화가 닫히거나 다른 씬으로 이동할 때 진행 중인 연결음을 정리한다.</summary>
    public void Cancel()
    {
        if (currentSequence != null) StopCoroutine(currentSequence);
        currentSequence = null;
        if (audioSource)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
        if (connectingIndicator) connectingIndicator.SetActive(false);
        if (remotePortrait) remotePortrait.SetActive(true);
    }

    void OnDisable() => Cancel();
}
