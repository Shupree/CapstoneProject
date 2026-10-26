using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>2페이즈 완료 이후의 봉인 연출. 모든 UI와 효과 슬롯은 씬에 저장한다.</summary>
public sealed class BattleSealingController : MonoBehaviour
{
    [Serializable]
    public class RopePiece
    {
        public RectTransform rect;
        [Min(0f)] public float startSeconds;
        public Vector2 assembleOffset = new(0, -120);
    }
    [Serializable]
    public class RopeTalisman
    {
        public SealingVisual visual;
        [Tooltip("이 로프 조각이 조립된 뒤부터 자란다.")]
        public int attachmentPiece;
    }

    [Header("완료 연결 / 저장된 씬 UI")]
    public BloodyMaryPhaseTwoController phaseTwo;
    public GameObject root;
    public GameObject[] desktopButtons, windowsToHide;
    public UnityEngine.UI.Button activateButton;
    public SealingVisual mary, calmMary;
    public Texture2D[] maryFrames, minaFrames, effectFrames;
    public UnityEngine.UI.RawImage mina;
    public RectTransform heroineGroup;
    public CanvasGroup talismanGroup;
    public RectTransform talisman;
    public SealingVisual emblem;
    public Texture2D finishedEmblem;
    public SealingVisual[] effectSlots;
    public RopePiece[] ropePieces;
    public RopeTalisman[] smallTalismans;

    [Header("입력 / 프레임")]
    [Min(1)] public int requiredPresses = 20;
    [Min(.01f)] public float frameSeconds = .2f;
    [Min(0f)] public float minaActiveSeconds = .5f;
    [Header("연출 시간 (초)")]
    [Min(0f)] public float completedHoldSeconds = 1.5f;
    [Min(0f)] public float unfoldSeconds = .35f;
    [Min(0f)] public float heroineRiseSeconds = .2f;
    [Min(0f)] public float heroineExitSeconds = .2f;
    [Min(0f)] public float effectDissolveSeconds = .5f;
    [Min(0f)] public float brightenSeconds = 1.5f;
    [Min(0f)] public float brightnessIncrease = .5f;
    [Min(0f)] public float talismanFadeSeconds = .5f;
    [Min(0f)] public float ropeAssembleSeconds = .16f;
    [Min(0f)] public float smallTalismanGrowSeconds = .35f;
    [Min(0f)] public float calmDelaySeconds = .75f;
    [Min(0f)] public float calmDissolveSeconds = 1f;

    enum Stage { Idle, Waiting, Button, Opening, Input, Finishing, Complete }
    Stage stage;
    int presses, minaFrame;
    float maryClock, minaClock, lastInput = float.NegativeInfinity;
    bool animateMary;
    Vector2 heroinePosition;
    Vector3 talismanScale;
    Vector2[] ropePositions;
    float[] effectStarted;
    public int PressCount => presses;

    void Awake()
    {
        if (root) root.SetActive(false);
        if (!Configured()) { Debug.LogError("봉인 연출의 씬 참조와 이미지 배열을 확인하세요.", this); enabled = false; return; }
        heroinePosition = heroineGroup.anchoredPosition;
        talismanScale = talisman.localScale;
        ropePositions = new Vector2[ropePieces.Length];
        for (int i = 0; i < ropePieces.Length; i++) ropePositions[i] = ropePieces[i].rect.anchoredPosition;
        effectStarted = new float[effectSlots.Length];
        Array.Fill(effectStarted, float.NegativeInfinity);
        phaseTwo.onPhaseTwoCompleted.AddListener(Begin);
        activateButton.onClick.AddListener(Activate);
    }

    bool Configured()
    {
        if (!phaseTwo || !root || !activateButton || !mary || !calmMary || !mina || !heroineGroup ||
            !talismanGroup || !talisman || !emblem || !finishedEmblem ||
            !Frames(maryFrames, 5) || !Frames(minaFrames, 4) || !Frames(effectFrames, 2) ||
            effectSlots == null || effectSlots.Length < 2 || ropePieces == null || ropePieces.Length == 0 ||
            smallTalismans == null || smallTalismans.Length == 0 || desktopButtons == null || windowsToHide == null) return false;
        if (!Visual(mary) || !Visual(calmMary) || !Visual(emblem)) return false;
        foreach (var slot in effectSlots) if (!Visual(slot)) return false;
        foreach (var piece in ropePieces) if (piece == null || !piece.rect) return false;
        foreach (var small in smallTalismans)
            if (small == null || !Visual(small.visual) || small.attachmentPiece < 0 || small.attachmentPiece >= ropePieces.Length) return false;
        return true;
    }
    static bool Visual(SealingVisual visual) => visual && visual.image && visual.template;
    static bool Frames(Texture2D[] frames, int count) => frames != null && frames.Length >= count && Array.TrueForAll(frames, f => f);

    public void Begin()
    {
        if (!enabled || stage != Stage.Idle || phaseTwo.State != BloodyMaryPhaseTwoController.PhaseState.Completed) return;
        stage = Stage.Waiting;
        StartCoroutine(ShowMary());
    }
    IEnumerator ShowMary()
    {
        yield return new WaitForSecondsRealtime(completedHoldSeconds);
        phaseTwo.window.gameObject.SetActive(false);
        foreach (var window in windowsToHide) if (window) window.SetActive(false);
        foreach (var button in desktopButtons) if (button) button.SetActive(false);
        talismanGroup.gameObject.SetActive(false);
        heroineGroup.gameObject.SetActive(false);
        calmMary.gameObject.SetActive(false);
        foreach (var effect in effectSlots) effect.gameObject.SetActive(false);
        foreach (var piece in ropePieces) piece.rect.gameObject.SetActive(false);
        foreach (var small in smallTalismans) small.visual.gameObject.SetActive(false);
        root.SetActive(true);
        root.transform.SetAsLastSibling();
        mary.Set(); mary.image.texture = maryFrames[0];
        animateMary = true;
        activateButton.gameObject.SetActive(true);
        activateButton.interactable = true;
        stage = Stage.Button;
    }
    void Activate()
    {
        if (stage != Stage.Button) return;
        stage = Stage.Opening;
        activateButton.interactable = false;
        activateButton.gameObject.SetActive(false);
        StartCoroutine(OpenTalisman());
    }
    IEnumerator OpenTalisman()
    {
        talismanGroup.gameObject.SetActive(true);
        talismanGroup.alpha = 1;
        emblem.Set(0f);
        yield return Tween(unfoldSeconds, t => talisman.localScale = Vector3.Scale(talismanScale, new Vector3(t, 1, 1)));
        mina.texture = minaFrames[0];
        heroineGroup.gameObject.SetActive(true);
        Vector2 below = BelowScreen();
        yield return Tween(heroineRiseSeconds, t => heroineGroup.anchoredPosition = Vector2.Lerp(below, heroinePosition, t));
        stage = Stage.Input;
    }
    Vector2 BelowScreen()
    {
        var parent = (RectTransform)heroineGroup.parent;
        return heroinePosition + Vector2.down * (parent.rect.height + heroineGroup.rect.height);
    }

    void Update()
    {
        if (animateMary)
        {
            maryClock += Time.unscaledDeltaTime;
            mary.image.texture = maryFrames[Mathf.FloorToInt(maryClock / Mathf.Max(.01f, frameSeconds)) % maryFrames.Length];
        }
        UpdateEffects();
        if (stage != Stage.Input) return;
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.wasPressedThisFrame) Press();
            if (keyboard.dKey.wasPressedThisFrame && stage == Stage.Input) Press();
        }
        // 마지막 입력부터 0.5초 동안 재생하며 연타로 프레임을 되감지 않는다.
        if (stage == Stage.Input && Time.unscaledTime - lastInput < minaActiveSeconds)
        {
            minaClock += Time.unscaledDeltaTime;
            while (minaClock >= Mathf.Max(.01f, frameSeconds))
            {
                minaClock -= Mathf.Max(.01f, frameSeconds);
                minaFrame = (minaFrame + 1) % minaFrames.Length;
            }
            mina.texture = minaFrames[minaFrame];
        }
    }
    void Press()
    {
        lastInput = Time.unscaledTime;
        presses++;
        float progress = Mathf.Clamp01((float)presses / Mathf.Max(1, requiredPresses));
        emblem.Set(progress);
        EmitEffects(progress);
        if (presses < requiredPresses) return;
        stage = Stage.Finishing;
        StartCoroutine(Finish());
    }
    void EmitEffects(float progress)
    {
        // 두 슬롯을 한 쌍으로 사용하며 풀은 씬에 미리 저장되어 있다.
        int pairCount = effectSlots.Length / 2;
        int first = ((presses - 1) % pairCount) * 2;
        int textureIndex = (presses - 1) % effectFrames.Length;
        var r = emblem.image.rectTransform.rect;
        for (int i = 0; i < 2; i++)
        {
            var slot = effectSlots[first + i];
            slot.image.texture = effectFrames[(textureIndex + i) % effectFrames.Length];
            slot.image.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(r.xMin, r.xMax, progress), (i == 0 ? .4f : -.4f) * r.height);
            slot.Set();
            slot.gameObject.SetActive(true);
            effectStarted[first + i] = Time.unscaledTime;
        }
    }
    void UpdateEffects()
    {
        if (effectStarted == null) return;
        for (int i = 0; i < effectSlots.Length; i++)
        {
            if (float.IsNegativeInfinity(effectStarted[i])) continue;
            float t = (Time.unscaledTime - effectStarted[i]) / Mathf.Max(.001f, effectDissolveSeconds);
            effectSlots[i].Set(1, t);
            if (t >= 1) { effectSlots[i].gameObject.SetActive(false); effectStarted[i] = float.NegativeInfinity; }
        }
    }
    IEnumerator Finish()
    {
        emblem.image.texture = finishedEmblem;
        emblem.Set();
        Vector2 below = BelowScreen();
        float duration = Mathf.Max(brightenSeconds, heroineExitSeconds);
        yield return Tween(duration, t =>
        {
            float elapsed = t * duration;
            float exit = Mathf.Clamp01(elapsed / Mathf.Max(.001f, heroineExitSeconds));
            heroineGroup.anchoredPosition = Vector2.Lerp(heroinePosition, below, exit);
            emblem.Set(1, 0, 1 + brightnessIncrease * Mathf.Clamp01(elapsed / Mathf.Max(.001f, brightenSeconds)));
        });
        heroineGroup.gameObject.SetActive(false);
        yield return Tween(talismanFadeSeconds, t => talismanGroup.alpha = 1 - t);
        talismanGroup.gameObject.SetActive(false);
        yield return AssembleRopes();
        yield return new WaitForSecondsRealtime(calmDelaySeconds);
        animateMary = false;
        mary.image.texture = maryFrames[0];
        calmMary.gameObject.SetActive(true);
        // 진정된 메리를 뒤에 두고 픽셀 단위로 기존 메리를 걷어낸다.
        yield return Tween(calmDissolveSeconds, t => { mary.Set(1, t); calmMary.Set(1, 1 - t); });
        mary.gameObject.SetActive(false);
        stage = Stage.Complete;
    }
    IEnumerator AssembleRopes()
    {
        float total = 0;
        foreach (var piece in ropePieces) total = Mathf.Max(total, piece.startSeconds + ropeAssembleSeconds);
        foreach (var small in smallTalismans)
            total = Mathf.Max(total, ropePieces[small.attachmentPiece].startSeconds + ropeAssembleSeconds + smallTalismanGrowSeconds);
        yield return Tween(total, t =>
        {
            float elapsed = t * total;
            for (int i = 0; i < ropePieces.Length; i++)
            {
                var piece = ropePieces[i];
                if (elapsed < piece.startSeconds) continue;
                piece.rect.gameObject.SetActive(true);
                float p = Mathf.Clamp01((elapsed - piece.startSeconds) / Mathf.Max(.001f, ropeAssembleSeconds));
                piece.rect.anchoredPosition = ropePositions[i] + piece.assembleOffset * (1 - p);
                piece.rect.localScale = Vector3.one * Mathf.Lerp(.1f, 1, p);
            }
            foreach (var small in smallTalismans)
            {
                float start = ropePieces[small.attachmentPiece].startSeconds + ropeAssembleSeconds;
                if (elapsed < start) continue;
                small.visual.gameObject.SetActive(true);
                small.visual.Set(Mathf.Clamp01((elapsed - start) / Mathf.Max(.001f, smallTalismanGrowSeconds)));
            }
        });
    }
    static IEnumerator Tween(float duration, Action<float> apply)
    {
        apply(0);
        for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            apply(Mathf.SmoothStep(0, 1, elapsed / duration));
            yield return null;
        }
        apply(1);
    }
    void OnDestroy()
    {
        if (phaseTwo) phaseTwo.onPhaseTwoCompleted.RemoveListener(Begin);
        if (activateButton) activateButton.onClick.RemoveListener(Activate);
    }
}
