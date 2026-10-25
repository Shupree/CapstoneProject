using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>씬에 저장된 배경과 대사를 클릭 한 번에 한 줄씩 진행한다.</summary>
public sealed class BackgroundConversationController : MonoBehaviour
{
    [Serializable] public class Line
    {
        public string speaker;
        [TextArea(2, 5)] public string text;
        public bool showPortrait;
    }
    [Serializable] public class Segment
    {
        public string label;
        public Texture2D background;
        public Line[] lines;
        public bool useDialogueBox;
        public bool fadeThroughBlackAfter;
    }
    [Header("배경별 대사 (순서대로 재생)")]
    public Segment[] segments;
    [Header("씬에 저장된 UI")]
    public UnityEngine.UI.RawImage backgroundImage;
    public UnityEngine.UI.AspectRatioFitter backgroundAspect;
    public UnityEngine.UI.RawImage transitionImage;
    public UnityEngine.UI.AspectRatioFitter transitionAspect;
    public TMP_Text speakerText, dialogueText;
    public UnityEngine.UI.Button advanceButton;
    [Header("아침 장면 UI (씬에서 배치)")]
    public GameObject legacyDialogueShade;
    public CanvasGroup boxedDialogue;
    public TMP_Text boxedSpeakerText, boxedDialogueText;
    public CanvasGroup portrait;
    [Header("마지막 대사 이후")]
    public string nextSceneName = "Explore_Proto";
    [Header("페이지 전환 페이드")]
    [Min(0f)] public float fadeOutSeconds = .45f;
    [UnityEngine.Serialization.FormerlySerializedAs("fadeInSeconds")]
    [Min(0f)] public float crossFadeSeconds = .45f;
    int segmentIndex, lineIndex;
    bool ready, leaving, transitioning;
    Color backgroundColor, speakerColor, dialogueColor;

    void Start()
    {
        if (!backgroundImage || !backgroundAspect || !transitionImage || !transitionAspect || !speakerText || !dialogueText || !advanceButton ||
            segments == null || segments.Length == 0)
        {
            Debug.LogError("Conversation의 화면 참조와 배경별 대사를 연결해 주세요.", this);
            return;
        }
        foreach (var segment in segments)
        {
            if (segment == null || !segment.background || segment.lines == null || segment.lines.Length == 0 ||
                Array.Exists(segment.lines, line => line == null))
            {
                Debug.LogError("Conversation의 각 배경에는 이미지와 한 줄 이상의 대사가 필요합니다.", this);
                advanceButton.interactable = false;
                return;
            }
        }
        ready = true;
        transitionImage.gameObject.SetActive(false);
        backgroundColor = backgroundImage.color;
        speakerColor = speakerText.color;
        dialogueColor = dialogueText.color;
        advanceButton.onClick.AddListener(Advance);
        ShowLine();
    }
    public void Advance()
    {
        if (!ready || leaving || transitioning) return;
        if (lineIndex + 1 < segments[segmentIndex].lines.Length) lineIndex++;
        else if (segmentIndex + 1 < segments.Length)
        {
            StartCoroutine(ChangePage(false));
            return;
        }
        else
        {
            if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                Debug.LogError("다음 씬을 Build Settings에 등록해 주세요: " + nextSceneName, this);
                return;
            }
            leaving = true;
            StartCoroutine(ChangePage(true));
            return;
        }
        ShowLine();
    }
    IEnumerator ChangePage(bool exitScene)
    {
        transitioning = true;
        advanceButton.interactable = false;
        if (exitScene)
        {
            yield return Fade(1f, 0f, fadeOutSeconds);
            yield return null;
            yield return SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
            yield break;
        }
        if (segments[segmentIndex].fadeThroughBlackAfter)
        {
            yield return Fade(1f, 0f, fadeOutSeconds);
            segmentIndex++;
            lineIndex = 0;
            ShowLine();
            SetBrightness(0f);
            yield return Fade(0f, 1f, crossFadeSeconds);
            transitioning = false;
            advanceButton.interactable = true;
            yield break;
        }
        var nextBackground = segments[segmentIndex + 1].background;
        transitionImage.texture = nextBackground;
        transitionAspect.aspectRatio = (float)nextBackground.width / nextBackground.height;
        transitionImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0f);
        transitionImage.gameObject.SetActive(true);
        // 현재 배경은 그대로 두고 다음 배경의 불투명도만 올려 암전을 방지한다.
        for (float elapsed = 0f; elapsed < crossFadeSeconds; elapsed += Time.unscaledDeltaTime)
        {
            transitionImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b,
                backgroundColor.a * elapsed / crossFadeSeconds);
            yield return null;
        }
        segmentIndex++;
        lineIndex = 0;
        ShowLine();
        transitionImage.gameObject.SetActive(false);
        transitioning = false;
        advanceButton.interactable = true;
    }
    IEnumerator Fade(float from, float to, float seconds)
    {
        for (float elapsed = 0f; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
        {
            SetBrightness(Mathf.Lerp(from, to, elapsed / seconds));
            yield return null;
        }
        SetBrightness(to);
    }
    void SetBrightness(float value)
    {
        backgroundImage.color = new Color(backgroundColor.r * value, backgroundColor.g * value,
            backgroundColor.b * value, backgroundColor.a);
        speakerText.color = new Color(speakerColor.r, speakerColor.g, speakerColor.b, speakerColor.a * value);
        dialogueText.color = new Color(dialogueColor.r, dialogueColor.g, dialogueColor.b, dialogueColor.a * value);
        if (boxedDialogue) boxedDialogue.alpha = value;
        if (portrait) portrait.alpha = value;
    }
    void ShowLine()
    {
        var segment = segments[segmentIndex];
        var line = segment.lines[lineIndex];
        backgroundImage.texture = segment.background;
        backgroundAspect.aspectRatio = (float)segment.background.width / segment.background.height;
        bool useBox = segment.useDialogueBox && boxedDialogue && boxedSpeakerText && boxedDialogueText;
        bool hasSpeaker = !string.IsNullOrWhiteSpace(line.speaker) && line.speaker != "나레이션";
        if (legacyDialogueShade) legacyDialogueShade.SetActive(!useBox);
        if (boxedDialogue) boxedDialogue.gameObject.SetActive(useBox);
        if (portrait) portrait.gameObject.SetActive(line.showPortrait);
        speakerText.gameObject.SetActive(!useBox && hasSpeaker);
        dialogueText.gameObject.SetActive(!useBox);
        var nameText = useBox ? boxedSpeakerText : speakerText;
        var bodyText = useBox ? boxedDialogueText : dialogueText;
        nameText.text = hasSpeaker ? line.speaker : "";
        bodyText.text = line.text + "  ▾";
    }
    void OnDestroy()
    {
        if (advanceButton) advanceButton.onClick.RemoveListener(Advance);
    }
}
