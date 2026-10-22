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
    public UnityEngine.UI.Image maryArtwork;
    public CanvasGroup knifeFadeGroup;
    public BattleScreenInterference screenInterference;
    [Tooltip("칼끝이 닿는 왼쪽 카드의 배열 인덱스")]
    public int knifeCardIndex = 2;

    [Header("연출 사운드")]
    public AudioSource ambienceSource;
    public AudioClip ambienceClip;
    public AudioClip bellClip;
    public AudioClip appearanceClip;
    public AudioClip maryVoiceClip;
    public AudioClip babyCryClip;
    [Range(0f, 1f)] public float ambienceVolume = .32f;

    [Header("자동 연출 시간")]
    [Min(.01f)] public float glitchPulseSeconds = .18f;
    [Min(0f)] public float glitchStartGapSeconds = 1f;
    [Min(0f)] public float crtFadeSeconds = 2f;
    [Min(0f)] public float blackoutSeconds = 1f;
    [Min(0f)] public float heroineSeconds = 1.9f;
    [Min(0f)] public float marySeconds = 3.4f;
    [Min(0f)] public float cardFrameGapSeconds = .15f;
    [Min(0f)] public float blankCardsSeconds = .35f;
    [Min(0f)] public float knifeFadeSeconds = 2f;
    [Min(0f)] public float secondFaceDelay = 1.5f;
    [Min(0f)] public float thirdFaceDelay = 1f;
    [Min(0f)] public float remainingFacesDelay = 1f;
    [Min(0f)] public float remainingFaceGap = .2f;
    [Min(0f)] public float knifeMoveSeconds = 4.7f;
    [Min(0f)] public float knifeHoldSeconds = .75f;

    bool started;

    public bool IsReady => retrySequence && phaseOne && blackoutRoot && blackoutImage && introRoot &&
        heroineCard && maryCard && knifeRoot && ambienceSource && ambienceClip && bellClip && appearanceClip &&
        maryVoiceClip && babyCryClip && maryArtwork && knifeFadeGroup && screenInterference &&
        knifeCardIndex >= 0 && knifeCardIndex < phaseOne.IntroCardCount &&
        phaseOne.stages[0].cards[knifeCardIndex].isKnife;

    void Awake()
    {
        if (blackoutRoot) blackoutRoot.SetActive(false);
        if (introRoot) introRoot.SetActive(false);
        if (knifeRoot) knifeRoot.SetActive(false);
        if (screenInterference) screenInterference.Clear();
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
        screenInterference.SetLines(1);
        yield return Wait(glitchPulseSeconds);
        screenInterference.SetLines(0);
        yield return Wait(Mathf.Max(0f, glitchStartGapSeconds - glitchPulseSeconds));
        screenInterference.SetLines(2);
        yield return Wait(glitchPulseSeconds);
        screenInterference.SetLines(0);
        for (float elapsed = 0f; elapsed < crtFadeSeconds; elapsed += Time.unscaledDeltaTime)
        {
            screenInterference.SetCRT(elapsed / crtFadeSeconds);
            yield return null;
        }
        screenInterference.SetCRT(1f);
        yield return Wait(blackoutSeconds);

        retrySequence.ApplyThemeAfterBlackout();
        if (!phaseOne.PrepareIntroWindow())
        {
            Debug.LogError("메리 등장 연출에 사용할 화상통화 창을 준비하지 못했습니다.", this);
            blackoutRoot.SetActive(false);
            screenInterference.Clear();
            yield break;
        }
        introRoot.SetActive(true);
        heroineCard.SetActive(true);
        maryCard.SetActive(true); // Both frames appear with the heroine; Mary's artwork comes later.
        maryArtwork.enabled = false;
        knifeRoot.SetActive(false);
        knifeFadeGroup.alpha = 1f;
        blackoutRoot.transform.SetAsLastSibling();
        StartAmbience();
        screenInterference.Clear();
        PlayEffect(bellClip, .65f);
        yield return Wait(heroineSeconds);

        maryArtwork.enabled = true;
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
        // Keep both people behind the incoming opaque frames until they are covered.
        introRoot.transform.SetAsFirstSibling();
        phaseOne.stages[0].root.transform.SetAsLastSibling();
        for (int i = 0; i < phaseOne.IntroCardCount; i++)
        {
            phaseOne.RevealIntroCardFrame(i);
            if (i + 1 < phaseOne.IntroCardCount)
                yield return Wait(cardFrameGapSeconds);
        }
        yield return null; // The complete set of frames is drawn before removing the covered people.
        heroineCard.SetActive(false);
        maryCard.SetActive(false);
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
        knifeRoot.transform.SetAsLastSibling();
        PlayEffect(babyCryClip, .35f);
        yield return SlideKnife(knifeRect, knifeStart, knifeDestination);

        knifeRect.anchoredPosition = knifeDestination;
        yield return Wait(knifeHoldSeconds);
        phaseOne.RevealIntroCard(knifeCardIndex);
        for (float elapsed = 0f; elapsed < knifeFadeSeconds; elapsed += Time.unscaledDeltaTime)
        {
            knifeFadeGroup.alpha = 1f - elapsed / knifeFadeSeconds;
            yield return null;
        }
        knifeFadeGroup.alpha = 0f;
        knifeRoot.SetActive(false);
        introRoot.SetActive(false);
        int revealed = 0;
        for (int i = 0; i < phaseOne.IntroCardCount; i++)
        {
            if (i == knifeCardIndex) continue;
            if (revealed == 1) yield return Wait(secondFaceDelay);
            else if (revealed == 2) yield return Wait(thirdFaceDelay);
            else if (revealed == 3) yield return Wait(remainingFacesDelay);
            else if (revealed > 3) yield return Wait(remainingFaceGap);
            phaseOne.RevealIntroCard(i);
            revealed++;
        }
        yield return null; // Keep exactly this board; unlock only after the final face is rendered.
        blackoutRoot.SetActive(false);
        ambienceSource.Stop();
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

    void OnDisable()
    {
        if (screenInterference) screenInterference.Clear();
    }
}
