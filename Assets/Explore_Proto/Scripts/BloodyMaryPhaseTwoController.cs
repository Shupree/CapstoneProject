using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Collects words using only UI slots authored in the scene.</summary>
public sealed class BloodyMaryPhaseTwoController : MonoBehaviour
{
    [Serializable]
    public sealed class Phrase
    {
        public string[] words;
        public Phrase(params string[] values) { words = values; }
    }
    public enum PhaseState { NotStarted, Running, Completed, Failed }
    public BloodyMaryPhaseController phaseOne;
    public BloodyMaryGameOverController gameOver;
    [Min(.01f)] public float timeLimit = 30f;
    public RectTransform window, phraseArea;
    public TMPro.TMP_InputField inputFrame;
    public UnityEngine.UI.Button submitButton;
    public BloodyMaryPhraseSlot[] slots;
    public BloodyMaryWordSlot[] selectedSlots;
    public Phrase[] phrases = {
        new Phrase("내", "아기는", "네가", "죽인거야"),
        new Phrase("아무도", "구해", "주지", "않았어"),
        new Phrase("네", "탓이야"),
        new Phrase("이대로는", "죽지", "못", "해")
    };
    public string[] answer = { "네", "아기는", "죽지", "않았어" };
    [Min(.01f)] public float spawnInterval = 1f, fadeSeconds = 1f, lifetime = 4f;
    [Min(1)] public int placementAttempts = 64;
    [Min(0f)] public float edgePadding = 16f, phrasePadding = 12f;
    public UnityEvent onPhaseTwoCompleted = new UnityEvent();
    public PhaseState State { get; private set; }
    public bool IsCompleted => State == PhaseState.Completed;
    public bool CanInteract => State == PhaseState.Running && gameOver && !gameOver.IsBlocking &&
        window && window.gameObject.activeInHierarchy;
    public float ElapsedSeconds { get; private set; }
    readonly List<string> selected = new List<string>(4);
    int selectionRevision;
    float spawnClock;

    public void StopForGameOver()
    {
        State = PhaseState.Failed;
        submitButton.interactable = false;
        foreach (var slot in slots) slot.Clear();
        foreach (var slot in selectedSlots) slot.Hide();
    }

    public void ResetForRetry()
    {
        State = PhaseState.NotStarted;
        selected.Clear();
        spawnClock = ElapsedSeconds = 0f;
        foreach (var slot in slots) slot.Clear();
        RefreshSelection();
        foreach (var slot in selectedSlots) slot.Hide();
        window.gameObject.SetActive(false);
    }

    void Awake()
    {
        // The scene also serializes these flags, so the frame never accepts keyboard input.
        if (inputFrame) { inputFrame.readOnly = true; inputFrame.enabled = false; }
        if (submitButton) submitButton.interactable = false;
    }

    public void BeginPhase()
    {
        if (State != PhaseState.NotStarted || !phaseOne || !phaseOne.IsCompleted) return;
        if (!ValidateConfiguration(out string error))
        {
            Debug.LogError("블러디 메리 2페이즈 설정 오류: " + error, this);
            return;
        }
        selected.Clear();
        foreach (var slot in slots) slot.Clear();
        RefreshSelection();
        State = PhaseState.Running;
        ElapsedSeconds = 0f;
        spawnClock = 0f;
        SpawnPhrase();
    }

    void Update()
    {
        // The overall deadline continues while the window is closed; phrase lifetimes still pause.
        if (State != PhaseState.Running || !gameOver || gameOver.IsBlocking) return;
        ElapsedSeconds += Time.unscaledDeltaTime;
        if (ElapsedSeconds >= timeLimit)
        {
            gameOver.FailPhaseTwo();
            return;
        }
        if (!CanInteract) return;
        float delta = Time.unscaledDeltaTime;
        // Expire before assigning a new phrase at the four-second boundary.
        foreach (var slot in slots) slot.Tick(delta, fadeSeconds, lifetime);
        spawnClock += delta;
        if (spawnClock < spawnInterval) return;
        spawnClock %= spawnInterval;
        SpawnPhrase(); // Do not burst-spawn after a long frame.
    }

    void SpawnPhrase()
    {
        BloodyMaryPhraseSlot free = null;
        foreach (var slot in slots) if (!slot.Active) { free = slot; break; }
        if (!free) return;
        var text = phrases[UnityEngine.Random.Range(0, phrases.Length)].words;
        var size = free.Measure(text);
        var area = phraseArea.rect;
        float minX = area.xMin + edgePadding, minY = area.yMin + edgePadding;
        float maxX = area.xMax - edgePadding - size.x, maxY = area.yMax - edgePadding - size.y;
        if (maxX < minX || maxY < minY) return;
        for (int attempt = 0; attempt < placementAttempts; attempt++)
        {
            var point = new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
            var candidate = new Rect(point - Vector2.one * phrasePadding, size + Vector2.one * phrasePadding * 2f);
            bool overlaps = false;
            foreach (var slot in slots) if (slot.Active && candidate.Overlaps(slot.Bounds)) { overlaps = true; break; }
            if (overlaps) continue;
            free.Display(text, point, size);
            return;
        }
    }

    public void CollectWord(BloodyMaryPhraseSlot slot, int index, int generation)
    {
        if (!CanInteract || selected.Count >= selectedSlots.Length || !slot || !slot.IsVisible ||
            Array.IndexOf(slots, slot) < 0 || slot.Generation != generation ||
            index < 0 || index >= slot.words.Length || !slot.words[index].Visible) return;
        selected.Add(slot.words[index].label.text);
        slot.Consume(index);
        RefreshSelection();
    }

    public void RemoveWord(int index, int revision)
    {
        if (!CanInteract || revision != selectionRevision || index < 0 || index >= selected.Count) return;
        selected.RemoveAt(index);
        RefreshSelection();
    }

    void RefreshSelection()
    {
        selectionRevision++;
        for (int i = 0; i < selectedSlots.Length; i++)
            selectedSlots[i].Show(i < selected.Count ? selected[i] : string.Empty, selectionRevision);
        submitButton.interactable = selected.Count == answer.Length;
    }

    public void Submit()
    {
        if (!CanInteract || selected.Count != answer.Length) return;
        for (int i = 0; i < answer.Length; i++) if (selected[i] != answer[i]) return;
        State = PhaseState.Completed;
        submitButton.interactable = false;
        foreach (var slot in slots) slot.Clear();
        foreach (var slot in selectedSlots) slot.Hide();
        window.gameObject.SetActive(false);
        onPhaseTwoCompleted.Invoke();
    }

    bool ValidateConfiguration(out string error)
    {
        error = "창·입력 프레임·버튼·표시 영역 참조를 확인하세요.";
        if (!gameOver || !(timeLimit > 0f) || float.IsInfinity(timeLimit) ||
            !window || !phraseArea || !inputFrame || !submitButton || phaseOne.clearedInternetWindow != window ||
            !phraseArea.IsChildOf(window) || !inputFrame.transform.IsChildOf(window) ||
            !submitButton.transform.IsChildOf(window)) return false;
        error = "생성 간격·페이드·수명·풀 용량을 확인하세요.";
        if (!(spawnInterval > 0f) || !(fadeSeconds > 0f) || !(lifetime >= fadeSeconds) ||
            float.IsInfinity(spawnInterval) || float.IsInfinity(lifetime) || placementAttempts < 1 ||
            slots == null || slots.Length < Mathf.CeilToInt(lifetime / spawnInterval) ||
            selectedSlots == null || selectedSlots.Length != 4 || answer == null || answer.Length != 4) return false;
        error = "문장·정답 어절과 표시 슬롯을 확인하세요.";
        if (phrases == null || phrases.Length == 0) return false;
        var unique = new HashSet<BloodyMaryWordSlot>();
        var uniquePhrases = new HashSet<BloodyMaryPhraseSlot>();
        foreach (var slot in slots)
        {
            if (!slot || !uniquePhrases.Add(slot) || !slot.fade || slot.words == null || slot.words.Length < 4 ||
                slot.transform.parent != phraseArea) return false;
            for (int i = 0; i < slot.words.Length; i++)
                if (!ValidWord(slot.words[i], i, false, unique) || slot.words[i].phrase != slot ||
                    !slot.words[i].transform.IsChildOf(slot.transform)) return false;
            foreach (var phrase in phrases)
            {
                if (phrase == null || phrase.words == null || phrase.words.Length == 0 || phrase.words.Length > slot.words.Length) return false;
                foreach (var word in phrase.words) if (string.IsNullOrWhiteSpace(word) || word.Contains(" ")) return false;
                var size = slot.Measure(phrase.words);
                if (size.x + edgePadding * 2f > phraseArea.rect.width || size.y + edgePadding * 2f > phraseArea.rect.height) return false;
            }
        }
        for (int i = 0; i < selectedSlots.Length; i++)
            if (!ValidWord(selectedSlots[i], i, true, unique) || !selectedSlots[i].transform.IsChildOf(inputFrame.transform)) return false;
        foreach (var word in answer)
        {
            bool found = false;
            foreach (var phrase in phrases) if (Array.IndexOf(phrase.words, word) >= 0) found = true;
            if (!found) return false;
        }
        error = null;
        return true;
    }

    bool ValidWord(BloodyMaryWordSlot slot, int index, bool isSelected, HashSet<BloodyMaryWordSlot> unique)
        => slot && unique.Add(slot) && slot.phase == this && slot.label && slot.button &&
            slot.wordIndex == index && slot.selectedWord == isSelected;
}
