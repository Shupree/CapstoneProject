using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>화상 통화에서 한글 음절을 모아 문장을 완성하는 2페이즈.</summary>
public sealed class BloodyMaryPhaseTwoController : MonoBehaviour
{
    public enum PhaseState { NotStarted, Running, Completed, Failed }
    public BloodyMaryPhaseController phaseOne;
    public BloodyMaryGameOverController gameOver;
    public RectTransform window, phaseRoot, phraseArea;
    public TextMeshProUGUI timerText, instructionText, completedText;
    public BloodyMarySyllableSource[] sources;
    public BloodyMarySelectedSyllable[] selectedSlots;
    [Min(.01f)] public float partOneSeconds = 20f, partTwoBonusSeconds = 15f;
    // 2파트의 한 주기는 맨 윗줄부터 맨 아랫줄까지 교체하는 데 걸리는 시간이다.
    [Min(.2f)] public float partOneWaveSeconds = 2.4f, partTwoWaveSeconds = 4f;
    public UnityEvent onPhaseTwoCompleted = new UnityEvent();

    // 띄어쓰기는 화면의 음절 슬롯 위치로 자동 표현한다.
    const string Answer = "네아기는죽지않았어";
    static readonly string[] FirstPhrases =
    {
        "네가 죽였어.", "왜 아무도 구해주지 않았어?", "울지마렴 내 아가..",
        "가증스러운 것들", "용서 못해", "더 많은 생기가 필요해", "너는 죽어야 해"
    };
    const int FigureRows = 16;
    static readonly Color FigureRed = new(1f,.05f,.07f,1f);
    static readonly Color PhraseRed = new(1f,.07f,.1f,1f);

    readonly List<char> selected = new(Answer.Length);
    readonly List<Rect> placed = new();
    readonly List<int> order = new();
    int figureRow;
    bool figureRowReplaced;
    float remaining, waveClock;
    public PhaseState State { get; private set; }
    public int Part { get; private set; }
    public float RemainingSeconds => Mathf.Max(0f, remaining);
    public bool CanInteract => State == PhaseState.Running && gameOver && !gameOver.IsBlocking &&
        window && window.gameObject.activeInHierarchy && phaseRoot && phaseRoot.gameObject.activeInHierarchy;

    void Awake() { if (phaseRoot) phaseRoot.gameObject.SetActive(false); }

    public void BeginPhase()
    {
        if (State != PhaseState.NotStarted || !phaseOne || !phaseOne.IsCompleted) return;
        if (!ValidateConfiguration())
        {
            Debug.LogError("화상통화 2페이즈 UI 또는 음절 슬롯 참조를 확인하세요.", this);
            return;
        }
        selected.Clear();
        foreach (var source in sources) source.Clear();
        State = PhaseState.Running;
        Part = 1;
        remaining = partOneSeconds;
        waveClock = 0f;
        window.gameObject.SetActive(true);
        window.SetAsLastSibling();
        phaseRoot.gameObject.SetActive(true);
        instructionText.gameObject.SetActive(true);
        completedText.gameObject.SetActive(false);
        instructionText.text = "붉은 글자에서 「네 아기는」을 완성하세요";
        RefreshSelection();
        SpawnFirstWave();
        RefreshTimer();
    }

    void Update()
    {
        if (State != PhaseState.Running || !gameOver || gameOver.IsBlocking) return;
        remaining -= Time.unscaledDeltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            RefreshTimer();
            gameOver.FailPhaseTwo();
            return;
        }
        RefreshTimer();
        if (!CanInteract) return;
        if (Part == 2)
        {
            UpdateFigure(Time.unscaledDeltaTime);
            return;
        }
        waveClock += Time.unscaledDeltaTime;
        if (waveClock >= partOneWaveSeconds)
        {
            waveClock = 0f;
            SpawnFirstWave();
        }
        float alpha = Mathf.Min(1f, waveClock / .18f, (partOneWaveSeconds - waveClock) / .25f);
        for (int i = 0; i < sources.Length; i++)
        {
            if (!sources[i].gameObject.activeSelf) continue;
            sources[i].SetAlpha(Mathf.Clamp01(alpha));
        }
    }

    void UpdateFigure(float deltaTime)
    {
        float rowSeconds = partTwoWaveSeconds / FigureRows;
        waveClock += deltaTime;
        while (waveClock >= rowSeconds)
        {
            if (!figureRowReplaced) RestoreFigureRow(figureRow);
            SetFigureRowAlpha(figureRow, 1f);
            waveClock -= rowSeconds;
            figureRow = (figureRow + 1) % FigureRows;
            figureRowReplaced = false;
        }
        if (!figureRowReplaced && waveClock >= rowSeconds * .5f)
        {
            RestoreFigureRow(figureRow);
            figureRowReplaced = true;
        }
        // 현재 줄만 희미해졌다가 새 문구로 나타난다. 다른 줄의 위치는 고정이다.
        SetFigureRowAlpha(figureRow, Mathf.Abs(1f - 2f * waveClock / rowSeconds));
    }

    void SpawnFirstWave()
    {
        foreach (var source in sources) source.Clear();
        placed.Clear();
        order.Clear();
        for (int i = 0; i < FirstPhrases.Length; i++) order.Add(i);
        Shuffle(order);
        for (int i = 0; i < FirstPhrases.Length; i++)
        {
            var source = sources[i];
            source.label.color = PhraseRed;
            string phrase = FirstPhrases[order[i]];
            Vector2 size = source.Measure(phrase);
            Rect area = phraseArea.rect;
            float left = area.xMin + size.x * .5f + 35f;
            float right = area.xMax - size.x * .5f - 35f;
            float bottom = area.yMin + size.y * .5f + 20f;
            float top = area.yMax - size.y * .5f - 20f;
            if (right < left || top < bottom) continue;
            Vector2 point = Vector2.zero;
            bool found = false;
            for (int attempt = 0; attempt < 80; attempt++)
            {
                point = new Vector2(UnityEngine.Random.Range(left, right), UnityEngine.Random.Range(bottom, top));
                var bounds = new Rect(point - size * .5f - Vector2.one * 12f, size + Vector2.one * 24f);
                bool overlaps = false;
                foreach (var used in placed) if (used.Overlaps(bounds)) { overlaps = true; break; }
                if (overlaps) continue;
                placed.Add(bounds);
                found = true;
                break;
            }
            if (!found) point = new Vector2(UnityEngine.Random.Range(left, right), UnityEngine.Random.Range(bottom, top));
            source.Show(phrase, point, 0f);
        }
    }

    void SpawnFigure()
    {
        foreach (var source in sources) source.Clear();
        figureRow = 0;
        figureRowReplaced = false;
        waveClock = 0f;
        foreach (var source in sources)
        {
            source.ShowFixedFigure(phraseArea.rect);
            source.label.color = FigureRed;
            source.SetAlpha(1f);
        }
    }

    void RestoreFigureRow(int row)
    {
        foreach (var source in sources)
            if (source.figureRow == row) source.ShowFixedFigure(phraseArea.rect);
    }

    void SetFigureRowAlpha(int row, float alpha)
    {
        foreach (var source in sources)
            if (source.figureRow == row) source.SetAlpha(alpha);
    }

    static void Shuffle(List<int> values)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
    }

    public void CollectSyllable(BloodyMarySyllableSource source, int characterIndex, int generation)
    {
        if (!CanInteract || !source || Array.IndexOf(sources, source) < 0 ||
            selected.Count >= (Part == 1 ? 4 : Answer.Length) ||
            !source.TryConsume(characterIndex, generation, out char syllable)) return;
        selected.Add(syllable);
        RefreshSelection();
        if (Part == 1 && MatchesPrefix(4)) BeginPartTwo();
        else if (Part == 2 && MatchesPrefix(Answer.Length)) CompletePhase();
    }

    public void RemoveSyllable(int index)
    {
        if (!CanInteract || index < 0 || index >= selected.Count) return;
        selected.RemoveAt(index);
        RefreshSelection();
    }

    bool MatchesPrefix(int length)
    {
        if (selected.Count != length) return false;
        for (int i = 0; i < length; i++) if (selected[i] != Answer[i]) return false;
        return true;
    }

    void BeginPartTwo()
    {
        Part = 2;
        remaining += partTwoBonusSeconds;
        waveClock = 0f;
        instructionText.text = "";
        instructionText.gameObject.SetActive(false);
        SpawnFigure();
        RefreshTimer();
    }

    void CompletePhase()
    {
        State = PhaseState.Completed;
        foreach (var source in sources) source.Clear();
        timerText.text = "";
        instructionText.text = "문장을 완성했다";
        completedText.text = "네 아기는 죽지 않았어.";
        completedText.gameObject.SetActive(true);
        onPhaseTwoCompleted.Invoke();
    }

    void RefreshSelection()
    {
        for (int i = 0; i < selectedSlots.Length; i++)
            selectedSlots[i].Show(i < selected.Count ? selected[i].ToString() : "",
                i < selected.Count && selected[i] == Answer[i]);
    }

    void RefreshTimer() { if (timerText) timerText.text = Mathf.CeilToInt(RemainingSeconds).ToString(); }

    public void StopForGameOver()
    {
        State = PhaseState.Failed;
        foreach (var source in sources) if (source) source.Clear();
        if (phaseRoot) phaseRoot.gameObject.SetActive(false);
    }

    public void ResetForRetry()
    {
        State = PhaseState.NotStarted;
        Part = 0;
        remaining = waveClock = 0f;
        selected.Clear();
        if (sources != null) foreach (var source in sources) if (source) source.Clear();
        if (phaseRoot) phaseRoot.gameObject.SetActive(false);
        if (selectedSlots != null && selectedSlots.Length == Answer.Length) RefreshSelection();
    }

    bool ValidateConfiguration()
    {
        if (!phaseOne || !gameOver || !window || !phaseRoot || !phraseArea || !timerText ||
            !instructionText || !completedText || !phaseRoot.IsChildOf(window) ||
            !phraseArea.IsChildOf(phaseRoot) || sources == null || sources.Length < FirstPhrases.Length ||
            selectedSlots == null || selectedSlots.Length != Answer.Length ||
            !float.IsFinite(partOneSeconds) || partOneSeconds <= 0f ||
            !float.IsFinite(partTwoBonusSeconds) || partTwoBonusSeconds <= 0f ||
            !float.IsFinite(partOneWaveSeconds) || partOneWaveSeconds <= 0f ||
            !float.IsFinite(partTwoWaveSeconds) || partTwoWaveSeconds <= 0f) return false;
        var unique = new HashSet<BloodyMarySyllableSource>();
        var populatedRows = new bool[FigureRows];
        foreach (var source in sources)
        {
            if (!source || !unique.Add(source) || source.phase != this || !source.label || !source.group ||
                !source.Rect.IsChildOf(phraseArea) || string.IsNullOrWhiteSpace(source.figurePhrase) ||
                source.figureRow < 0 || source.figureRow >= FigureRows ||
                !float.IsFinite(source.figurePosition.x) || !float.IsFinite(source.figurePosition.y) ||
                !float.IsFinite(source.figureAngle) || !float.IsFinite(source.figureFontSize) ||
                source.figureFontSize < 8f) return false;
            populatedRows[source.figureRow] = true;
        }
        foreach (bool populated in populatedRows) if (!populated) return false;
        foreach (var slot in selectedSlots)
            if (!slot || slot.phase != this || !slot.label || !slot.button || !slot.transform.IsChildOf(phaseRoot)) return false;
        return true;
    }

    // 비활성화된 예전 인터넷 UI의 직렬화 버튼과의 호환용.
    public void CollectWord(BloodyMaryPhraseSlot slot, int index, int generation) { }
    public void RemoveWord(int index, int revision) { }
    public void Submit() { }
}
