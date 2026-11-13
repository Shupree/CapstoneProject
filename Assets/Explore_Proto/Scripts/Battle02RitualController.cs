using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Battle02_Proto 전용 통화 위협과 세 차례의 음절 수집을 진행한다. UI는 씬에 저장한다.</summary>
public sealed class Battle02RitualController : MonoBehaviour
{
    [Serializable]
    public sealed class Round
    {
        public GameObject root;
        public Battle02WordSource[] words;
        public RectTransform[] glitchTargets;
    }

    static readonly string[] Answer = { "네", "아", "이", "가", "기", "다", "리", "고", "있", "어" };
    static readonly Color CorrectColor = Color.white;
    static readonly Color WrongColor = new(1f, .22f, .24f, 1f);

    [Header("기존 연출과의 연결")]
    public BattleProtoRetrySequence retrySequence;
    public BloodyMaryGameOverController gameOver;
    public BloodyMaryPhaseTwoController completionBridge;
    public RectTransform callWindow;
    public GameObject introRoot;
    public Image maryArtwork;
    public Sprite normalMary, knifeMary;
    public Sprite knifeMaryB, knifeMaryC;
    public Sprite attackSuccessMary, hurtMary;
    [Header("메리 이미지 전환")]
    [InspectorName("이미지 전환 암전 시간 (초)"), Min(0f)] public float maryBlackoutSeconds = .5f;
    Coroutine maryTransition;
    Sprite requestedMary;
    [Header("칼 든 메리 단계별 표시 시간 (암전 시간은 별도 추가)")]
    [InspectorName("모습 A 표시 시간 (초)"), Min(.01f)] public float knifeASeconds = 1.6f;
    [InspectorName("모습 B 표시 시간 (초)"), Min(.01f)] public float knifeBSeconds = 1.2f;
    [InspectorName("모습 C 표시 시간 (초)"), Min(.01f)] public float knifeCSeconds = 1.2f;
    public RawImage maryGlitch;
    public AudioSource noiseSource;
    public AudioClip noiseClip;
    [Header("기믹 효과음")]
    public AudioSource interactionAudio;
    public AudioClip glassBreakClip, syllableClickClip;

    [Header("씬에 저장된 조작 UI")]
    public Button hangupButton;
    public Button dockMessengerButton;
    public GameObject darkMessengerWindow;
    public Button darkMessengerCloseButton;
    public CanvasGroup roundWordsGroup;
    public RawImage textGlitch, inputGlitch;
    public RawImage[] extraWordGlitches;
    public TMP_Text[] selectedWordLabels;
    public Button[] selectedWordButtons;
    public Round[] rounds;
    [Header("음절 이동")]
    [Min(.01f)] public float syllableMoveSeconds = .2f;
    [Min(0f)] public float syllableMoveArc = 40f;
    readonly bool[] flying = new bool[Answer.Length];
    readonly Vector3[] slotPositions = new Vector3[Answer.Length];
    readonly Vector3[] flightStarts = new Vector3[Answer.Length];
    readonly float[] slotFontSizes = new float[Answer.Length];
    readonly float[] flightFontSizes = new float[Answer.Length];
    readonly Color[] flightColors = new Color[Answer.Length];
    readonly float[] flightElapsed = new float[Answer.Length];
    bool selectionLayoutCaptured;
    bool HasFlyingSyllables => Array.Exists(flying, value => value);

    [Header("잘못된 통화 종료: 창 파손 3단계")]
    public UnityEngine.UI.Image brokenWindow;
    public Sprite[] brokenWindowStages;
    int wrongHangupCount;
    public int WrongHangupCount => wrongHangupCount;
    public bool HangupLocked => wrongHangupCount >= 3;

    [Header("글리치 스프라이트 (시트에서 분할한 5종)")]
    public Sprite[] maryGlitchFrames;
    public Sprite[] wordGlitchFrames;
    [Min(.01f)] public float glitchFrameSeconds = .06f;
    [Tooltip("투명 여백을 제외한 문구 글리치의 가로·세로 배율")]
    public Vector2 wordGlitchSizeMultiplier = new(1.15f, 1.2f);
    public Vector2 wordGlitchPadding = new(24f, 12f);

    [Header("시간")]
    [Min(.1f)] public float callGlitchInterval = 5f;
    [Min(.1f)] public float callGlitchSeconds = 1f;
    [Min(.1f)] public float firstKnifeMinSeconds = 8f;
    [Min(.1f)] public float firstKnifeMaxSeconds = 14f;
    public float knifeFailureSeconds => knifeASeconds + knifeBSeconds + knifeCSeconds + 3f * Mathf.Max(0f, maryBlackoutSeconds);
    [Min(.1f)] public float wordGlitchMinSeconds = 3f;
    [Min(.1f)] public float wordGlitchMaxSeconds = 4f;
    [Min(.1f)] public float wordGlitchSeconds = .5f;
    [Min(0f)] public float hangupCooldownSeconds = .5f;
    [Min(.1f)] public float painSeconds = .85f;

    enum RitualState { Idle, Running, Transition, Failed, Completed }
    RitualState state;
    readonly List<Battle02WordSource> selected = new(Answer.Length);
    int roundIndex;
    bool knifeVisible, wordGlitchRunning;
    double knifeDeadline, nextKnifeAt, nextCallGlitchAt, nextWordGlitchAt, noiseStopAt, nextHangupAt;
    double knifeBAt, knifeCAt;
    Coroutine wordGlitchRoutine;
    Vector2 maryPosition;
    Vector2 hangupNormalPosition;
    Color maryColor;
    readonly HashSet<RectTransform> glitchedPhrases = new();

    public bool HasStarted => state != RitualState.Idle;
    public bool IsRunning => state == RitualState.Running;
    public int RoundNumber => roundIndex + 1;
    public bool KnifeVisible => knifeVisible;

    void Awake()
    {
        // Preserve authored placement even when another required reference is missing.
        if (hangupButton) hangupNormalPosition = ((RectTransform)hangupButton.transform).anchoredPosition;
        if (!Configured())
        {
            Debug.LogError("Battle02 기믹의 씬 UI, 단어 또는 이미지 참조를 확인하세요.", this);
            enabled = false;
            return;
        }
        maryPosition = maryArtwork.rectTransform.anchoredPosition;
        hangupNormalPosition = ((RectTransform)hangupButton.transform).anchoredPosition;
        maryColor = maryArtwork.color;
        brokenWindow.gameObject.SetActive(false);
        darkMessengerWindow.SetActive(false);
        hangupButton.gameObject.SetActive(false);
        maryGlitch.gameObject.SetActive(false);
        HideWordGlitches();
        inputGlitch.gameObject.SetActive(false);
        foreach (var round in rounds) round.root.SetActive(false);
        RefreshSelection();
        hangupButton.onClick.AddListener(HangUp);
        darkMessengerCloseButton.onClick.AddListener(CloseDarkMessenger);
        for (int i = 0; i < selectedWordButtons.Length; i++)
        {
            slotPositions[i] = selectedWordLabels[i].rectTransform.localPosition;
            slotFontSizes[i] = selectedWordLabels[i].fontSize;
            int index = i;
            selectedWordButtons[i].onClick.AddListener(() => RemoveSelectedWord(index));
        }
        selectionLayoutCaptured = true;
    }

    bool Configured()
    {
        if (!brokenWindow || brokenWindowStages == null || brokenWindowStages.Length != 3 ||
            Array.Exists(brokenWindowStages, sprite => !sprite)) return false;
        if (!normalMary || !knifeMary || !knifeMaryB || !knifeMaryC || !attackSuccessMary || !hurtMary)
        {
            Debug.LogError("Battle02 메리 Sprite 참조 누락: Normal Mary / Knife Mary(A) / Knife Mary B / Knife Mary C / Attack Success Mary / Hurt Mary를 확인하세요.", this);
            return false;
        }
        if (!knifeMaryB || !knifeMaryC ||
            !float.IsFinite(knifeASeconds) || knifeASeconds <= 0f ||
            !float.IsFinite(knifeBSeconds) || knifeBSeconds <= 0f ||
            !float.IsFinite(knifeCSeconds) || knifeCSeconds <= 0f) return false;
        if (!retrySequence || !gameOver || !completionBridge || !callWindow || !introRoot ||
            !maryArtwork || !normalMary || !knifeMary || !maryGlitch || !noiseSource || !noiseClip ||
            !hangupButton || !dockMessengerButton || !darkMessengerWindow || !darkMessengerCloseButton ||
            !roundWordsGroup || !textGlitch || !inputGlitch ||
            extraWordGlitches == null || extraWordGlitches.Length != 4 ||
            selectedWordLabels == null || selectedWordLabels.Length != Answer.Length ||
            selectedWordButtons == null || selectedWordButtons.Length != Answer.Length ||
            rounds == null || rounds.Length != 3 || !interactionAudio || !glassBreakClip || !syllableClickClip) return false;
        for (int i = 0; i < Answer.Length; i++)
            if (!selectedWordLabels[i] || !selectedWordButtons[i]) return false;
        foreach (var glitch in extraWordGlitches) if (!glitch) return false;
        foreach (var round in rounds)
        {
            if (round == null || !round.root || round.words == null || round.words.Length < Answer.Length ||
                round.glitchTargets == null || round.glitchTargets.Length < 4) return false;
            foreach (var word in round.words)
                if (!word || word.controller != this || !word.label || !word.button ||
                    string.IsNullOrEmpty(word.word) || word.word.Length != 1) return false;
            foreach (var target in round.glitchTargets) if (!target) return false;
            foreach (string answer in Answer)
                if (!Array.Exists(round.words, word => word.word == answer)) return false;
        }
        return true;
    }

    /// <summary>기존 두 인물과 메리의 첫 조우가 끝난 자리에서 새 기믹을 시작한다.</summary>
    public bool BeginAfterIntro()
    {
        if (!enabled || state != RitualState.Idle || !retrySequence.IsThemeApplied ||
            !callWindow.gameObject.activeInHierarchy) return false;
        ResetGameplay();
        retrySequence.StartBattleMusic();
        return true;
    }

    /// <summary>첫 조우 연출에도 통화 종료 아이콘을 보여주되 기믹 시작 전에는 입력을 막는다.</summary>
    public void ShowIntroHangup()
    {
        ((RectTransform)hangupButton.transform).anchoredPosition = hangupNormalPosition;
        hangupButton.gameObject.SetActive(true);
        hangupButton.interactable = false;
    }

    void ResetGameplay()
    {
        StopAllCoroutines();
        maryTransition = null;
        requestedMary = normalMary;
        CancelFlyingSyllables();
        wrongHangupCount = 0;
        brokenWindow.gameObject.SetActive(false);
        wordGlitchRoutine = null;
        state = RitualState.Running;
        roundIndex = 0;
        selected.Clear();
        foreach (var round in rounds)
        {
            round.root.SetActive(false);
            foreach (var word in round.words) word.SetConsumed(false);
        }
        maryArtwork.sprite = normalMary;
        maryArtwork.color = maryColor;
        maryArtwork.rectTransform.anchoredPosition = maryPosition;
        maryArtwork.enabled = true;
        maryGlitch.gameObject.SetActive(false);
        noiseSource.Stop();
        noiseStopAt = 0;
        HideWordGlitches();
        inputGlitch.gameObject.SetActive(false);
        roundWordsGroup.interactable = true;
        wordGlitchRunning = knifeVisible = false;
        darkMessengerWindow.SetActive(false);
        callWindow.gameObject.SetActive(true);
        introRoot.SetActive(true);
        hangupButton.gameObject.SetActive(true);
        hangupButton.interactable = true;
        ((RectTransform)hangupButton.transform).anchoredPosition = hangupNormalPosition;
        nextHangupAt = 0;
        RefreshSelection();
        // Only the first attack of a fresh attempt uses the fixed tutorial-like delay.
        nextKnifeAt = Time.unscaledTimeAsDouble + 1d;
        nextCallGlitchAt = Time.unscaledTimeAsDouble + callGlitchInterval;
    }

    void Update()
    {
        if (noiseSource && noiseSource.isPlaying && Time.unscaledTimeAsDouble >= noiseStopAt)
            noiseSource.Stop();
        if (state != RitualState.Running || gameOver.IsBlocking) return;
        double now = Time.unscaledTimeAsDouble;
        if (knifeVisible && now >= knifeDeadline)
        {
            gameOver.FailBattle02();
            return;
        }
        if (!knifeVisible && now >= nextKnifeAt)
        {
            knifeVisible = true;
            RequestMary(knifeMary);
            // Capture all boundaries together; Inspector edits apply to the next attack.
            float gap = Mathf.Max(0f, maryBlackoutSeconds);
            knifeBAt = now + gap + knifeASeconds;
            knifeCAt = knifeBAt + gap + knifeBSeconds;
            knifeDeadline = knifeCAt + gap + knifeCSeconds;
        }
        if (knifeVisible)
            RequestMary(now >= knifeCAt ? knifeMaryC : now >= knifeBAt ? knifeMaryB : knifeMary);
        UpdateSyllableFlights();
        if (state != RitualState.Running) return;
        if (now >= nextCallGlitchAt)
        {
            nextCallGlitchAt = now + callGlitchInterval;
            StartCoroutine(GlitchMary());
        }
        if (darkMessengerWindow.activeSelf && !wordGlitchRunning && now >= nextWordGlitchAt)
        {
            ScheduleWordGlitch();
            wordGlitchRoutine = StartCoroutine(GlitchWords());
        }
    }

    void ScheduleKnife()
    {
        float min = Mathf.Max(.1f, firstKnifeMinSeconds - roundIndex);
        float max = Mathf.Max(min, firstKnifeMaxSeconds - roundIndex);
        nextKnifeAt = Time.unscaledTimeAsDouble + UnityEngine.Random.Range(min, max);
    }

    void RequestMary(Sprite sprite)
    {
        if (requestedMary == sprite) return;
        if (maryTransition != null) StopCoroutine(maryTransition);
        maryTransition = StartCoroutine(ChangeMary(sprite));
    }

    IEnumerator ChangeMary(Sprite sprite)
    {
        requestedMary = sprite;
        maryArtwork.enabled = false;
        if (maryBlackoutSeconds > 0f)
            yield return new WaitForSecondsRealtime(maryBlackoutSeconds);
        maryArtwork.sprite = sprite;
        maryArtwork.enabled = true;
        maryTransition = null;
    }

    public IEnumerator ShowAttackSuccess()
    {
        yield return ChangeMary(attackSuccessMary);
    }

    void ScheduleWordGlitch()
    {
        float reduction = .35f * roundIndex;
        float min = Mathf.Max(1.25f, wordGlitchMinSeconds - reduction);
        float max = Mathf.Max(min, wordGlitchMaxSeconds - reduction);
        nextWordGlitchAt = Time.unscaledTimeAsDouble + UnityEngine.Random.Range(min, max);
    }

    RawImage WordGlitchAt(int index) => index == 0 ? textGlitch : extraWordGlitches[index - 1];

    void HideWordGlitches()
    {
        glitchedPhrases.Clear();
        textGlitch.gameObject.SetActive(false);
        if (extraWordGlitches != null)
            foreach (var glitch in extraWordGlitches) if (glitch) glitch.gameObject.SetActive(false);
        wordGlitchRunning = false;
    }

    IEnumerator GlitchMary()
    {
        var frames = ShuffleGlitchFrames(maryGlitchFrames);
        int previousFrame = -1;
        PlayNoise(callGlitchSeconds);
        maryGlitch.gameObject.SetActive(true);
        for (float elapsed = 0f; elapsed < callGlitchSeconds && state == RitualState.Running;
             elapsed += Time.unscaledDeltaTime)
        {
            int frame = Mathf.FloorToInt(elapsed / Mathf.Max(.01f, glitchFrameSeconds));
            if (frame != previousFrame)
            {
                ApplyGlitchFrame(maryGlitch, frames, frame);
                previousFrame = frame;
            }
            maryArtwork.rectTransform.anchoredPosition = maryPosition +
                new Vector2(UnityEngine.Random.Range(-13f, 13f), UnityEngine.Random.Range(-5f, 5f));
            float brightness = UnityEngine.Random.Range(.35f, 1f);
            maryArtwork.color = new Color(maryColor.r * brightness, maryColor.g * brightness,
                maryColor.b * brightness, maryColor.a);
            maryGlitch.rectTransform.anchoredPosition =
                new Vector2(0f, UnityEngine.Random.Range(-240f, 240f));
            maryGlitch.enabled = Mathf.FloorToInt(elapsed * 14f) % 3 != 0;
            yield return null;
        }
        maryArtwork.rectTransform.anchoredPosition = maryPosition;
        maryArtwork.color = maryColor;
        maryGlitch.enabled = true;
        maryGlitch.gameObject.SetActive(false);
    }

    void PlayNoise(float seconds)
    {
        if (!noiseSource || !noiseClip) return;
        if (!noiseSource.isPlaying)
        {
            noiseSource.clip = noiseClip;
            noiseSource.Play();
        }
        noiseStopAt = Math.Max(noiseStopAt, Time.unscaledTimeAsDouble + seconds);
    }

    /// <summary>메신저 창은 통화 화면 앞에 열고, 닫아도 현재 선택 단어를 유지한다.</summary>
    public void ToggleDarkMessenger()
    {
        if (state != RitualState.Running || gameOver.IsBlocking) return;
        if (darkMessengerWindow.activeSelf) { CloseDarkMessenger(); return; }
        foreach (var round in rounds) round.root.SetActive(false);
        rounds[roundIndex].root.SetActive(true);
        darkMessengerWindow.SetActive(true);
        darkMessengerWindow.transform.SetAsLastSibling();
        ((RectTransform)hangupButton.transform).anchoredPosition = hangupNormalPosition;
        ScheduleWordGlitch();
    }

    public void CloseDarkMessenger()
    {
        CancelFlyingSyllables();
        darkMessengerWindow.SetActive(false);
        if (wordGlitchRoutine != null) StopCoroutine(wordGlitchRoutine);
        wordGlitchRoutine = null;
        HideWordGlitches();
        ((RectTransform)hangupButton.transform).anchoredPosition = hangupNormalPosition;
        roundWordsGroup.interactable = true;
    }

    public void SelectWord(Battle02WordSource source)
    {
        if (state != RitualState.Running || !darkMessengerWindow.activeSelf ||
            selected.Count >= Answer.Length ||
            !source || source.IsConsumed ||
            Array.IndexOf(rounds[roundIndex].words, source) < 0) return;
        if (glitchedPhrases.Contains(source.transform.parent as RectTransform)) return;
        if (knifeVisible && Time.unscaledTimeAsDouble >= knifeDeadline)
        {
            gameOver.FailBattle02();
            return;
        }
        int slot = selected.Count;
        var label = selectedWordLabels[slot];
        flightStarts[slot] = label.transform.parent.InverseTransformPoint(source.label.transform.position);
        flightFontSizes[slot] = source.label.fontSize;
        flightColors[slot] = source.label.color;
        flightElapsed[slot] = 0f;
        flying[slot] = true;
        source.SetConsumed(true);
        interactionAudio.PlayOneShot(syllableClickClip);
        selected.Add(source);
        RefreshSelection();
        label.rectTransform.localPosition = flightStarts[slot];
        label.fontSize = flightFontSizes[slot];
        label.color = flightColors[slot];
    }

    void RemoveSelectedWord(int index)
    {
        if (state != RitualState.Running || HasFlyingSyllables ||
            index < 0 || index >= selected.Count) return;
        selected[index].SetConsumed(false);
        selected.RemoveAt(index);
        RefreshSelection();
    }

    bool MatchesAnswer()
    {
        for (int i = 0; i < Answer.Length; i++)
            if (selected[i].word != Answer[i]) return false;
        return true;
    }

    void RefreshSelection()
    {
        if (selectedWordLabels == null || selectedWordButtons == null) return;
        for (int i = 0; i < Mathf.Min(selectedWordLabels.Length, selectedWordButtons.Length); i++)
        {
            bool filled = i < selected.Count;
            selectedWordLabels[i].text = filled ? selected[i].word : string.Empty;
            if (!flying[i])
                selectedWordLabels[i].color = filled && selected[i].word != Answer[i] ? WrongColor : CorrectColor;
            selectedWordButtons[i].interactable = filled && !HasFlyingSyllables;
            selectedWordLabels[i].raycastTarget = selectedWordButtons[i].interactable;
        }
    }

    void UpdateSyllableFlights()
    {
        if (!HasFlyingSyllables) return;
        for (int i = 0; i < selected.Count; i++)
        {
            if (!flying[i]) continue;
            flightElapsed[i] += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(flightElapsed[i] / Mathf.Max(.01f, syllableMoveSeconds));
            float eased = Mathf.SmoothStep(0f, 1f, t);
            var label = selectedWordLabels[i];
            label.rectTransform.localPosition = Vector3.Lerp(flightStarts[i], slotPositions[i], eased)
                + Vector3.up * (Mathf.Sin(t * Mathf.PI) * syllableMoveArc);
            label.fontSize = Mathf.Lerp(flightFontSizes[i], slotFontSizes[i], eased);
            label.color = Color.Lerp(flightColors[i], selected[i].word == Answer[i] ? CorrectColor : WrongColor, eased);
            if (t < 1f) continue;
            flying[i] = false;
            label.rectTransform.localPosition = slotPositions[i];
        }
        RefreshSelection();
        if (!HasFlyingSyllables && selected.Count == Answer.Length && MatchesAnswer())
            CompleteRound();
    }

    void CancelFlyingSyllables()
    {
        if (!selectionLayoutCaptured) return;
        // Remove in reverse order so simultaneous arrivals retain their selection order.
        for (int i = selected.Count - 1; i >= 0; i--)
        {
            if (!flying[i]) continue;
            selected[i].SetConsumed(false);
            selected.RemoveAt(i);
        }
        for (int i = 0; i < flying.Length; i++)
        {
            flying[i] = false;
            selectedWordLabels[i].rectTransform.localPosition = slotPositions[i];
            selectedWordLabels[i].fontSize = slotFontSizes[i];
        }
        RefreshSelection();
    }

    void ClearSelection()
    {
        foreach (var word in selected) word.SetConsumed(false);
        selected.Clear();
        RefreshSelection();
    }

    void HangUp()
    {
        if (state != RitualState.Running || gameOver.IsBlocking || HangupLocked) return;
        double now = Time.unscaledTimeAsDouble;
        if (knifeVisible && now >= knifeDeadline)
        {
            gameOver.FailBattle02();
            return;
        }
        if (now < nextHangupAt) return;
        nextHangupAt = now + hangupCooldownSeconds;
        hangupButton.interactable = false;
        if (knifeVisible)
        {
            knifeVisible = false;
            RequestMary(normalMary);
            ScheduleKnife();
        }
        else
        {
            wrongHangupCount++;
            interactionAudio.PlayOneShot(glassBreakClip);
            brokenWindow.sprite = brokenWindowStages[wrongHangupCount - 1];
            brokenWindow.gameObject.SetActive(true);
            if (HangupLocked)
            {
                // Only hangup stays locked; messenger interaction and sentence completion continue.
                return;
            }
        }
        StartCoroutine(ReenableHangup());
    }

    IEnumerator ReenableHangup()
    {
        yield return new WaitForSecondsRealtime(hangupCooldownSeconds);
        if (state == RitualState.Running && !HangupLocked) hangupButton.interactable = true;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        CancelFlyingSyllables();
    }

    IEnumerator GlitchWords()
    {
        var frames = ShuffleGlitchFrames(wordGlitchFrames);
        wordGlitchRunning = true;
        var targets = rounds[roundIndex].glitchTargets;
        var genuine = rounds[roundIndex].words[
            UnityEngine.Random.Range(0, rounds[roundIndex].words.Length)].transform.parent as RectTransform;
        if (genuine) glitchedPhrases.Add(genuine);
        int count = Mathf.Min(roundIndex + 1, targets.Length, extraWordGlitches.Length + 1);
        while (glitchedPhrases.Count < count)
            glitchedPhrases.Add(targets[UnityEngine.Random.Range(0, targets.Length)]);
        int overlay = 0;
        foreach (var target in glitchedPhrases)
        {
            var graphic = WordGlitchAt(overlay++);
            graphic.rectTransform.anchoredPosition = target.anchoredPosition;
            graphic.rectTransform.sizeDelta = Vector2.Scale(target.rect.size,
                new Vector2(Mathf.Max(.01f, wordGlitchSizeMultiplier.x),
                    Mathf.Max(.01f, wordGlitchSizeMultiplier.y))) + wordGlitchPadding;
            graphic.gameObject.SetActive(true);
        }
        int previousFrame = -1;
        float duration = wordGlitchSeconds + .1f * roundIndex;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            int frame = Mathf.FloorToInt(elapsed / Mathf.Max(.01f, glitchFrameSeconds));
            if (frame != previousFrame)
            {
                for (int i = 0; i < overlay; i++)
                    ApplyGlitchFrame(WordGlitchAt(i), frames, frame + i, true);
                previousFrame = frame;
            }
            yield return null;
        }
        HideWordGlitches();
        wordGlitchRoutine = null;
    }

    static Sprite[] ShuffleGlitchFrames(Sprite[] source)
    {
        if (source == null || source.Length == 0) return source;
        var frames = (Sprite[])source.Clone();
        for (int i = frames.Length - 1; i > 0; i--)
        {
            int other = UnityEngine.Random.Range(0, i + 1);
            (frames[i], frames[other]) = (frames[other], frames[i]);
        }
        return frames;
    }

    static void ApplyGlitchFrame(UnityEngine.UI.RawImage graphic, Sprite[] frames, int index,
        bool trimTransparentPadding = false)
    {
        if (frames == null || frames.Length == 0) return;
        var sprite = frames[index % frames.Length];
        if (!sprite) return;
        // Keep the scene-authored RawImage and sample only the sliced sprite, not the entire sheet.
        var rect = trimTransparentPadding ? sprite.textureRect : sprite.rect;
        var texture = sprite.texture;
        graphic.texture = texture;
        graphic.uvRect = new Rect(rect.x / texture.width, rect.y / texture.height,
            rect.width / texture.width, rect.height / texture.height);
    }

    void CompleteRound()
    {
        state = RitualState.Transition;
        StopAllCoroutines();
        maryTransition = null;
        wordGlitchRoutine = null;
        HideWordGlitches();
        maryGlitch.gameObject.SetActive(false);
        noiseSource.Stop();
        maryArtwork.rectTransform.anchoredPosition = maryPosition;
        maryArtwork.color = maryColor;
        knifeVisible = false;
        CloseDarkMessenger();
        StartCoroutine(PainThenContinue());
    }

    IEnumerator PainThenContinue()
    {
        yield return ChangeMary(hurtMary);
        for (float elapsed = 0f; elapsed < painSeconds; elapsed += Time.unscaledDeltaTime)
        {
            maryArtwork.rectTransform.anchoredPosition =
                maryPosition + new Vector2(UnityEngine.Random.Range(-8f, 8f), 0f);
            maryArtwork.color = Mathf.FloorToInt(elapsed * 12f) % 2 == 0
                ? new Color(1f, .5f, .5f, maryColor.a) : maryColor;
            yield return null;
        }
        maryArtwork.rectTransform.anchoredPosition = maryPosition;
        maryArtwork.color = maryColor;
        yield return ChangeMary(normalMary);
        if (roundIndex == rounds.Length - 1)
        {
            state = RitualState.Completed;
            brokenWindow.gameObject.SetActive(false);
            hangupButton.gameObject.SetActive(false);
            retrySequence.StopBattleMusic();
            if (!completionBridge.CompleteFromExternal())
                Debug.LogError("Battle02 기믹 완료 후 기존 봉인 연출로 연결하지 못했습니다.", this);
            yield break;
        }
        ClearSelection();
        roundIndex++;
        state = RitualState.Running;
        hangupButton.interactable = !HangupLocked;
        ScheduleKnife();
    }

    public void StopForGameOver()
    {
        if (state != RitualState.Running) return;
        state = RitualState.Failed;
        StopAllCoroutines();
        maryTransition = null;
        wordGlitchRoutine = null;
        HideWordGlitches();
        maryGlitch.gameObject.SetActive(false);
        noiseSource.Stop();
        maryArtwork.rectTransform.anchoredPosition = maryPosition;
        maryArtwork.color = maryColor;
        CloseDarkMessenger();
    }

    public void RestartAfterGameOver()
    {
        if (!HasStarted || !retrySequence.IsThemeApplied) return;
        ResetGameplay();
        retrySequence.StartBattleMusic();
    }
}
