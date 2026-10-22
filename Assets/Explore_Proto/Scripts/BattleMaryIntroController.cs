using System.Collections;
using UnityEngine;

/// <summary>통화 종료부터 1페이즈 시작까지 저장된 UI를 자동으로 재생한다.</summary>
public sealed class BattleMaryIntroController : MonoBehaviour
{
    [Header("씬에 저장된 연출 UI")]
    public BattleProtoRetrySequence retrySequence;
    public BloodyMaryPhaseController phaseOne;
    public GameObject blackoutRoot;
    public UnityEngine.UI.Image blackoutImage;
    public GameObject introRoot;
    public GameObject heroineCard;
    public GameObject maryCard;
    public GameObject knifeRoot;

    [Header("연출 사운드")]
    public AudioSource ambienceSource;
    public AudioClip ambienceClip;
    public AudioClip bellClip;
    public AudioClip appearanceClip;
    public AudioClip maryVoiceClip;
    public AudioClip babyCryClip;
    [Range(0f, 1f)] public float ambienceVolume = .32f;

    [Header("자동 연출 시간")]
    [Min(0f)] public float fadeSeconds = .45f;
    [Min(0f)] public float blackoutSeconds = 1.65f;
    [Min(0f)] public float heroineSeconds = 1.9f;
    [Min(0f)] public float marySeconds = 3.4f;
    [Min(0f)] public float cardFrameGapSeconds = .15f;
    [Min(0f)] public float blankCardsSeconds = .35f;
    [Min(0f)] public float echoGapSeconds = .58f;
    [Range(.1f, 1f)] public float revealAcceleration = .8f;
    [Min(0f)] public float knifeMoveSeconds = 4.7f;
    [Min(0f)] public float knifeHoldSeconds = .75f;

    bool started;

    public bool IsReady => retrySequence && phaseOne && blackoutRoot && blackoutImage && introRoot &&
        heroineCard && maryCard && knifeRoot && ambienceSource && ambienceClip && bellClip && appearanceClip &&
        maryVoiceClip && babyCryClip && phaseOne.IntroCardCount > 0;

    void Awake()
    {
        if (blackoutRoot) blackoutRoot.SetActive(false);
        if (introRoot) introRoot.SetActive(false);
    }

    public bool Begin()
    {
        if (started || !IsReady) return false;
        started = true;
        StartCoroutine(PlaySequence());
        return true;
    }

    IEnumerator PlaySequence()
    {
        // 전체 화면 Image는 투명해도 클릭을 가로막아 연출 중 창 조작을 막는다.
        retrySequence.backgroundMusic.Stop();
        blackoutRoot.SetActive(true);
        blackoutRoot.transform.SetAsLastSibling();
        SetBlackoutAlpha(0f);
        yield return FadeBlackout(0f, 1f);
        PlayEffect(bellClip, .65f);
        yield return Wait(blackoutSeconds);

        retrySequence.ApplyThemeAfterBlackout();
        if (!phaseOne.PrepareIntroWindow())
        {
            Debug.LogError("메리 등장 연출에 사용할 화상통화 창을 준비하지 못했습니다.", this);
            blackoutRoot.SetActive(false);
            yield break;
        }
        introRoot.SetActive(true);
        heroineCard.SetActive(true);
        maryCard.SetActive(false);
        knifeRoot.SetActive(false);
        blackoutRoot.transform.SetAsLastSibling();
        StartAmbience();
        yield return FadeBlackout(1f, 0f);
        yield return Wait(heroineSeconds);

        maryCard.SetActive(true);
        ambienceSource.Stop();
        retrySequence.StartBattleMusic();
        PlayEffect(appearanceClip, .7f);
        PlayEffect(maryVoiceClip, .5f);
        yield return Wait(marySeconds);

        // 카드 틀을 하나씩 배치한 뒤, 칼 연출 후 같은 카드에 메리 이미지를 공개한다.
        if (!phaseOne.PrepareIntroCards())
        {
            Debug.LogError("메리 등장 연출에 사용할 1페이즈 카드를 준비하지 못했습니다.", this);
            introRoot.SetActive(false);
            blackoutRoot.SetActive(false);
            yield break;
        }
        heroineCard.SetActive(false);
        maryCard.SetActive(false);
        for (int i = 0; i < phaseOne.IntroCardCount; i++)
        {
            phaseOne.RevealIntroCardFrame(i);
            if (i + 1 < phaseOne.IntroCardCount)
                yield return Wait(cardFrameGapSeconds);
        }
        yield return Wait(blankCardsSeconds);

        var knifeRect = knifeRoot.transform as RectTransform;
        Vector2 knifeDestination = knifeRect.anchoredPosition;
        // 활성화 전에 화면 밖 출발점으로 옮겨 도착 위치가 한 프레임 보이지 않게 한다.
        var knifeParent = (RectTransform)knifeRect.parent;
        float knifeHalfWidth = knifeRect.rect.width * knifeRect.localScale.x * .5f;
        // 해상도가 바뀌어도 손 이미지 오른쪽이 통화창 밖에서 잘리지 않게 한다.
        knifeDestination.x = Mathf.Min(knifeDestination.x, knifeParent.rect.xMax - knifeHalfWidth - 50f);
        Vector2 knifeStart = new(knifeParent.rect.xMax + knifeHalfWidth + 50f, knifeDestination.y);
        knifeRect.anchoredPosition = knifeStart;
        knifeRoot.SetActive(true);
        PlayEffect(babyCryClip, .35f);
        yield return SlideKnife(knifeRect, knifeStart, knifeDestination);

        knifeRect.anchoredPosition = knifeDestination;
        yield return Wait(knifeHoldSeconds);
        introRoot.SetActive(false);
        for (int i = 0; i < phaseOne.IntroCardCount; i++)
        {
            phaseOne.RevealIntroCard(i);
            if (i + 1 < phaseOne.IntroCardCount)
                yield return Wait(echoGapSeconds * Mathf.Pow(revealAcceleration, i));
        }
        blackoutRoot.SetActive(false);
        retrySequence.effectsAudio.Stop();
        if (!phaseOne.BeginAfterIntro())
            Debug.LogError("메리 등장 연출 후 1페이즈를 시작하지 못했습니다.", this);
    }

    void StartAmbience()
    {
        retrySequence.backgroundMusic.Stop();
        ambienceSource.clip = ambienceClip;
        ambienceSource.loop = true;
        ambienceSource.volume = ambienceVolume;
        ambienceSource.Play();
    }

    void PlayEffect(AudioClip clip, float volume)
    {
        retrySequence.effectsAudio.PlayOneShot(clip, volume);
    }

    IEnumerator FadeBlackout(float from, float to)
    {
        if (fadeSeconds <= 0f) { SetBlackoutAlpha(to); yield break; }
        for (float elapsed = 0f; elapsed < fadeSeconds; elapsed += Time.unscaledDeltaTime)
        {
            SetBlackoutAlpha(Mathf.Lerp(from, to, elapsed / fadeSeconds));
            yield return null;
        }
        SetBlackoutAlpha(to);
    }

    void SetBlackoutAlpha(float alpha)
    {
        blackoutImage.color = new Color(0f, 0f, 0f, alpha);
    }

    IEnumerator SlideKnife(RectTransform knife, Vector2 start, Vector2 destination)
    {
        if (knifeMoveSeconds <= 0f) { knife.anchoredPosition = destination; yield break; }
        for (float elapsed = 0f; elapsed < knifeMoveSeconds; elapsed += Time.unscaledDeltaTime)
        {
            float t = KnifeTravelProgress(elapsed / knifeMoveSeconds);
            knife.anchoredPosition = Vector2.Lerp(start, destination, t);
            yield return null;
        }
        knife.anchoredPosition = destination;
    }

    // 이동 거리의 22%는 기본 속도, 다음 11%는 감속한다. 이 패턴을 세 번 반복한다.
    static float KnifeTravelProgress(float normalizedTime)
    {
        const float fastDistance = .22f;
        const float slowDistance = .11f;
        const float slowSpeed = .45f;
        const float finalDistance = .01f;
        float work = Mathf.Clamp01(normalizedTime) *
            (3f * (fastDistance + slowDistance / slowSpeed) + finalDistance);
        float distance = 0f;
        for (int repeat = 0; repeat < 3; repeat++)
        {
            if (work <= fastDistance) return distance + work;
            work -= fastDistance;
            distance += fastDistance;

            float slowWork = slowDistance / slowSpeed;
            if (work <= slowWork) return distance + work * slowSpeed;
            work -= slowWork;
            distance += slowDistance;
        }
        return Mathf.Min(1f, distance + work);
    }

    static IEnumerator Wait(float seconds)
    {
        if (seconds > 0f) yield return new WaitForSecondsRealtime(seconds);
    }
}
