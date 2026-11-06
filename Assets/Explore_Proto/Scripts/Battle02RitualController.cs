using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Battle02_Proto 전용 통화 위협과 다섯 차례의 단어 수집을 진행한다. UI는 씬에 저장한다.</summary>
public sealed class Battle02RitualController : MonoBehaviour
{
    [Serializable]
    public sealed class Round
    {
        public GameObject root;
        public Battle02WordSource[] words;
        public RectTransform[] glitchTargets;
    }

    static readonly string[] Answer = { "네", "아기는", "죽지", "않았어" };
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
    public RawImage maryGlitch;
    public AudioSource noiseSource;
    public AudioClip noiseClip;

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

    [Header("시간")]
    [Min(.1f)] public float callGlitchInterval = 5f;
    [Min(.1f)] public float callGlitchSeconds = 1f;
    [Min(.1f)] public float firstKnifeMinSeconds = 8f;
    [Min(.1f)] public float firstKnifeMaxSeconds = 14f;
    [Min(.1f)] public float knifeFailureSeconds = 4f;
    [Min(.1f)] public float wordGlitchMinSeconds = 3f;
    [Min(.1f)] public float wordGlitchMaxSeconds = 4f;
    [Min(.1f)] public float wordGlitchSeconds = .5f;
    [Min(0f)] public float hangupCooldownSeconds = .5f;
    public Vector2 hangupMessengerPosition = new(0f, -700f);
    [Min(.1f)] public float painSeconds = .85f;

    enum RitualState { Idle, Running, Transition, Failed, Completed }
    RitualState state;
    readonly List<Battle02WordSource> selected = new(Answer.Length);
    int roundIndex;
    bool knifeVisible, wordGlitchRunning, inputResetting;
    double knifeDeadline, nextKnifeAt, nextCallGlitchAt, nextWordGlitchAt, noiseStopAt, nextHangupAt;
    Coroutine inputResetRoutine, wordGlitchRoutine;
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
        if (!Configured())
        {
            Debug.LogError("Battle02 기믹의 씬 UI, 단어 또는 이미지 참조를 확인하세요.", this);
            enabled = false;
            return;
        }
        maryPosition = maryArtwork.rectTransform.anchoredPosition;
        hangupNormalPosition = ((RectTransform)hangupButton.transform).anchoredPosition;
        maryColor = maryArtwork.color;
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
            int index = i;
            selectedWordButtons[i].onClick.AddListener(() => RemoveSelectedWord(index));
        }
    }

    bool Configured()
    {
        if (!retrySequence || !gameOver || !completionBridge || !callWindow || !introRoot ||
            !maryArtwork || !normalMary || !knifeMary || !maryGlitch || !noiseSource || !noiseClip ||
            !hangupButton || !dockMessengerButton || !darkMessengerWindow || !darkMessengerCloseButton ||
            !roundWordsGroup || !textGlitch || !inputGlitch ||
            extraWordGlitches == null || extraWordGlitches.Length != 4 ||
            selectedWordLabels == null || selectedWordLabels.Length != Answer.Length ||
            selectedWordButtons == null || selectedWordButtons.Length != Answer.Length ||
            rounds == null || rounds.Length != 5) return false;
        for (int i = 0; i < Answer.Length; i++)
            if (!selectedWordLabels[i] || !selectedWordButtons[i]) return false;
        foreach (var glitch in extraWordGlitches) if (!glitch) return false;
        foreach (var round in rounds)
        {
            if (round == null || !round.root || round.words == null || round.words.Length < Answer.Length ||
                round.glitchTargets == null || round.glitchTargets.Length < 4) return false;
            foreach (var word in round.words)
                if (!word || word.controller != this || !word.label || !word.button || string.IsNullOrEmpty(word.word)) return false;
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
        inputResetRoutine = null;
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
        wordGlitchRunning = inputResetting = knifeVisible = false;
        darkMessengerWindow.SetActive(false);
        callWindow.gameObject.SetActive(true);
        introRoot.SetActive(true);
        hangupButton.gameObject.SetActive(true);
        hangupButton.interactable = true;
        ((RectTransform)hangupButton.transform).anchoredPosition = hangupNormalPosition;
        nextHangupAt = 0;
        RefreshSelection();
        ScheduleKnife();
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
            maryArtwork.sprite = knifeMary;
            knifeDeadline = now + knifeFailureSeconds;
        }
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
        PlayNoise(callGlitchSeconds);
        maryGlitch.gameObject.SetActive(true);
        for (float elapsed = 0f; elapsed < callGlitchSeconds && state == RitualState.Running;
             elapsed += Time.unscaledDeltaTime)
        {
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
        ((RectTransform)hangupButton.transform).anchoredPosition = hangupMessengerPosition;
        ScheduleWordGlitch();
    }

    public void CloseDarkMessenger()
    {
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
            inputResetting || selected.Count >= Answer.Length ||
            !source || source.IsConsumed ||
            Array.IndexOf(rounds[roundIndex].words, source) < 0) return;
        if (glitchedPhrases.Contains(source.transform.parent as RectTransform)) return;
        if (knifeVisible && Time.unscaledTimeAsDouble >= knifeDeadline)
        {
            gameOver.FailBattle02();
            return;
        }
        source.SetConsumed(true);
        selected.Add(source);
        RefreshSelection();
        if (selected.Count == Answer.Length && MatchesAnswer()) CompleteRound();
    }

    void RemoveSelectedWord(int index)
    {
        if (state != RitualState.Running || inputResetting || index < 0 || index >= selected.Count) return;
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
            selectedWordLabels[i].color = filled && selected[i].word != Answer[i] ? WrongColor : CorrectColor;
            selectedWordButtons[i].interactable = filled;
        }
    }

    void ClearSelection()
    {
        foreach (var word in selected) word.SetConsumed(false);
        selected.Clear();
        RefreshSelection();
    }

    void HangUp()
    {
        if (state != RitualState.Running || gameOver.IsBlocking) return;
        double now = Time.unscaledTimeAsDouble;
        if (knifeVisible && now >= knifeDeadline)
        {
            gameOver.FailBattle02();
            return;
        }
        if (now < nextHangupAt) return;
        nextHangupAt = now + hangupCooldownSeconds;
        hangupButton.interactable = false;
        StartCoroutine(ReenableHangup());
        if (knifeVisible)
        {
            knifeVisible = false;
            maryArtwork.sprite = normalMary;
            ScheduleKnife();
            return;
        }
        if (inputResetRoutine != null) StopCoroutine(inputResetRoutine);
        inputResetRoutine = StartCoroutine(ResetInputWithGlitch());
    }

    IEnumerator ReenableHangup()
    {
        yield return new WaitForSecondsRealtime(hangupCooldownSeconds);
        if (state == RitualState.Running) hangupButton.interactable = true;
    }

    IEnumerator ResetInputWithGlitch()
    {
        inputResetting = true;
        PlayNoise(wordGlitchSeconds);
        inputGlitch.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(wordGlitchSeconds);
        ClearSelection();
        inputGlitch.gameObject.SetActive(false);
        inputResetting = false;
        inputResetRoutine = null;
    }

    IEnumerator GlitchWords()
    {
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
            graphic.rectTransform.sizeDelta = target.sizeDelta + new Vector2(24f, 12f);
            graphic.gameObject.SetActive(true);
        }
        yield return new WaitForSecondsRealtime(wordGlitchSeconds + .1f * roundIndex);
        HideWordGlitches();
        wordGlitchRoutine = null;
    }

    void CompleteRound()
    {
        state = RitualState.Transition;
        StopAllCoroutines();
        inputResetRoutine = null;
        wordGlitchRoutine = null;
        inputResetting = false;
        HideWordGlitches();
        maryGlitch.gameObject.SetActive(false);
        noiseSource.Stop();
        maryArtwork.rectTransform.anchoredPosition = maryPosition;
        maryArtwork.color = maryColor;
        knifeVisible = false;
        maryArtwork.sprite = normalMary;
        CloseDarkMessenger();
        StartCoroutine(PainThenContinue());
    }

    IEnumerator PainThenContinue()
    {
        // 전용 고통 스프라이트가 추가되기 전까지 기존 메리 이미지에 짧은 흔들림을 준다.
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
        if (roundIndex == rounds.Length - 1)
        {
            state = RitualState.Completed;
            hangupButton.gameObject.SetActive(false);
            retrySequence.StopBattleMusic();
            if (!completionBridge.CompleteFromExternal())
                Debug.LogError("Battle02 기믹 완료 후 기존 봉인 연출로 연결하지 못했습니다.", this);
            yield break;
        }
        ClearSelection();
        roundIndex++;
        state = RitualState.Running;
        hangupButton.interactable = true;
        ScheduleKnife();
    }

    public void StopForGameOver()
    {
        if (state != RitualState.Running) return;
        state = RitualState.Failed;
        StopAllCoroutines();
        inputResetRoutine = null;
        wordGlitchRoutine = null;
        inputResetting = false;
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
