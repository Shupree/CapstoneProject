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
    public SealingVisual sdMary;
    public RectTransform leftRope, rightRope;
    public BattleMaryStorageSequence storageSequence;
    public Texture2D[] maryFrames, minaFrames, effectFrames;
    public UnityEngine.UI.RawImage mina;
    public RectTransform heroineGroup;
    public CanvasGroup sealingScreen;
    public UnityEngine.UI.RectMask2D screenRevealMask;
    public GameObject keyNote;
    public CanvasGroup talismanGroup;
    public RectTransform talisman;
    public SealingVisual emblem;
    public Texture2D finishedEmblem;
    public SealingVisual[] effectSlots;
    public RopePiece[] ropePieces;
    public RopeTalisman[] smallTalismans;

    [Header("입력 / 프레임")]
    [Min(1)] public int requiredPresses = 30;
    [Tooltip("입력 가능 상태에서 진행도가 1 감소하는 간격. 입력해도 주기는 초기화되지 않는다.")]
    [Min(.01f)] public float progressDecaySeconds = .3f;
    [Min(.01f)] public float frameSeconds = .2f;
    [Min(0f)] public float minaActiveSeconds = .5f;
    [Header("연출 시간 (초)")]
    [Min(0f)] public float completedHoldSeconds = 1.5f;
    [Min(0f)] public float unfoldSeconds = .35f;
    [Min(0f)] public float heroineDelaySeconds = 1f;
    [Min(0f)] public float horizontalRevealSeconds = .35f;
    [Min(0f)] public float eyeOpenHoldSeconds = .75f;
    [Min(0f)] public float inputDelaySeconds = .5f;
    [Range(0f, 1f)] public float stripCenter = .45f;
    [Range(.01f, 1f)] public float stripHeight = .1f;
    [Min(0f)] public float heroineExitSeconds = .2f;
    [Min(0f)] public float effectDissolveSeconds = .5f;
    [Min(0f)] public float brightenSeconds = 1.5f;
    [Min(0f)] public float brightnessIncrease = .5f;
    [Min(0f)] public float talismanFadeSeconds = .5f;
    [Min(0f)] public float ropeAssembleSeconds = .16f;
    [Min(0f)] public float smallTalismanGrowSeconds = .35f;
    [Min(0f)] public float calmDelaySeconds = .75f;
    [Min(0f)] public float calmDissolveSeconds = 1f;
    [Header("SD 메리 전환 (초)")]
    [Min(0f)] public float maryWhiteSeconds = 1f;
    [Min(0f)] public float sdWhiteHoldSeconds = 1f;
    [Min(0f)] public float maryShrinkSeconds = 1.5f;
    [Min(0f)] public float sdColorDelaySeconds = 1f;
    [Min(0f)] public float sdColorRestoreSeconds = 1f;
    [Min(0f)] public float ropeExitSeconds = 1f;

    enum Stage { Idle, Waiting, Button, Opening, Input, Finishing, Complete }
    Stage stage;
    int presses, minaFrame, effectSequence;
    double nextDecayTime;
    float maryClock, minaClock, lastInput = float.NegativeInfinity;
    bool animateMary;
    Vector2 heroinePosition;
    Vector2[] ropePositions;
    float[] effectStarted;
    public int PressCount => presses;

    void Awake()
    {
        if (root) root.SetActive(false);
        if (!Configured()) { Debug.LogError("봉인 연출의 씬 참조와 이미지 배열을 확인하세요.", this); enabled = false; return; }
        heroinePosition = heroineGroup.anchoredPosition;
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
            !sealingScreen || !screenRevealMask || !keyNote ||
            !talismanGroup || !talisman || !emblem || !finishedEmblem ||
            !Frames(maryFrames, 5) || !Frames(minaFrames, 4) || !Frames(effectFrames, 2) ||
            effectSlots == null || effectSlots.Length < 2 || ropePieces == null || ropePieces.Length == 0 ||
            smallTalismans == null || smallTalismans.Length == 0 || desktopButtons == null || windowsToHide == null) return false;
        if (!Visual(mary) || !Visual(calmMary) || !Visual(emblem) || !Visual(sdMary) || !leftRope || !rightRope || !storageSequence || !storageSequence.IsConfigured) return false;
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
        sealingScreen.gameObject.SetActive(false);
        heroineGroup.gameObject.SetActive(false);
        calmMary.gameObject.SetActive(false);
        sdMary.gameObject.SetActive(false);
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
        sealingScreen.alpha = 1;
        sealingScreen.gameObject.SetActive(true);
        talismanGroup.gameObject.SetActive(true);
        talismanGroup.alpha = 1;
        emblem.Set(0f);
        mina.texture = minaFrames[0];
        heroineGroup.anchoredPosition = heroinePosition;
        heroineGroup.gameObject.SetActive(true);
        keyNote.SetActive(false);
        screenRevealMask.enabled = true;
        yield return Tween(horizontalRevealSeconds, t => SetScreenReveal(t, 0));
        yield return new WaitForSecondsRealtime(heroineDelaySeconds);
        minaFrame = 1;
        mina.texture = minaFrames[minaFrame];
        yield return new WaitForSecondsRealtime(eyeOpenHoldSeconds);
        yield return Tween(unfoldSeconds, t => SetScreenReveal(1, t));
        yield return new WaitForSecondsRealtime(inputDelaySeconds);
        keyNote.SetActive(true);
        nextDecayTime = Time.unscaledTimeAsDouble + Mathf.Max(.01f, progressDecaySeconds);
        stage = Stage.Input;
    }
    void SetScreenReveal(float horizontal, float vertical)
    {
        // 전체 UI의 배치를 고정하고 마스크의 공개 범위만 넓힌다.
        var size = ((RectTransform)screenRevealMask.transform).rect.size;
        float bottom = Mathf.Clamp01(stripCenter - stripHeight * .5f);
        float top = 1 - Mathf.Clamp01(stripCenter + stripHeight * .5f);
        screenRevealMask.padding = new Vector4(0, size.y * bottom * (1 - vertical),
            size.x * (1 - horizontal), size.y * top * (1 - vertical));
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
        DecayProgress();
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
                minaFrame = 1 + minaFrame % (minaFrames.Length - 1);
            }
            mina.texture = minaFrames[minaFrame];
        }
    }
    void DecayProgress()
    {
        double now = Time.unscaledTimeAsDouble;
        if (now < nextDecayTime) return;
        double interval = Mathf.Max(.01f, progressDecaySeconds);
        double ticks = Math.Floor((now - nextDecayTime) / interval) + 1;
        nextDecayTime += ticks * interval;
        presses = (int)Math.Max(0, presses - ticks);
        emblem.Set((float)presses / Mathf.Max(1, requiredPresses));
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
        int first = (effectSequence % pairCount) * 2;
        int textureIndex = effectSequence % effectFrames.Length;
        effectSequence = (effectSequence + 1) % (pairCount * effectFrames.Length);
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
        yield return Tween(talismanFadeSeconds, t => sealingScreen.alpha = 1 - t);
        talismanGroup.gameObject.SetActive(false);
        sealingScreen.gameObject.SetActive(false);
        storageSequence.RestoreDesktopButtons();
        yield return AssembleRopes();
        yield return new WaitForSecondsRealtime(calmDelaySeconds);
        animateMary = false;
        mary.image.texture = maryFrames[0];
        calmMary.gameObject.SetActive(true);
        // 진정된 메리를 뒤에 두고 픽셀 단위로 기존 메리를 걷어낸다.
        yield return Tween(calmDissolveSeconds, t => { mary.Set(1, t); calmMary.Set(1, 1 - t); });
        mary.gameObject.SetActive(false);
        yield return TransformToSDMary();
        yield return storageSequence.StoreMary();
        stage = Stage.Complete;
    }
    IEnumerator TransformToSDMary()
    {
        yield return Tween(maryWhiteSeconds, t => calmMary.Set(white: t));
        sdMary.Set(white: 1);
        sdMary.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(sdWhiteHoldSeconds);
        var large = calmMary.image.rectTransform;
        var small = sdMary.image.rectTransform;
        Vector3 fromScale = large.localScale;
        Vector3 fromPosition = large.position;
        float ratio = small.rect.height * small.lossyScale.y / Mathf.Max(.001f, large.rect.height * large.lossyScale.y);
        yield return Tween(maryShrinkSeconds, t =>
        {
            large.localScale = fromScale * Mathf.Lerp(1, ratio, t);
            large.position = Vector3.Lerp(fromPosition, small.position, t);
            calmMary.Set(white: 1, opacity: 1 - Mathf.InverseLerp(.75f, 1, t));
        });
        calmMary.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(sdColorDelaySeconds);
        yield return Tween(sdColorRestoreSeconds, t => sdMary.Set(white: 1 - t));
        Vector2 leftStart = leftRope.anchoredPosition, rightStart = rightRope.anchoredPosition;
        var area = ((RectTransform)leftRope.parent).rect.size;
        var leftBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(leftRope.parent, leftRope);
        var rightBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(rightRope.parent, rightRope);
        Vector2 leftOffset = new(-area.x - leftBounds.size.x, area.y + leftBounds.size.y);
        Vector2 rightOffset = new(area.x + rightBounds.size.x, area.y + rightBounds.size.y);
        yield return Tween(ropeExitSeconds, t =>
        {
            leftRope.anchoredPosition = leftStart + leftOffset * t;
            rightRope.anchoredPosition = rightStart + rightOffset * t;
        });
        leftRope.gameObject.SetActive(false);
        rightRope.gameObject.SetActive(false);
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
