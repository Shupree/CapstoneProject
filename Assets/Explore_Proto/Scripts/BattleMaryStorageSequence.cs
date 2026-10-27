using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>씬에 저장된 SD 메리·파일명·폴더·팝업으로 저장 완료 연출을 진행한다.</summary>
public sealed class BattleMaryStorageSequence : MonoBehaviour
{
    public BattleSealingController sealing;
    public BattleProtoConversation conversation;
    public RectTransform maryFileGroup, folder;
    public CanvasGroup maryFileFade;
    public TMP_Text fileName;
    public GameObject savedPopup;
    public UnityEngine.UI.Button confirmButton;
    public SealingVisual transitionCover;
    [Header("저장 연출")]
    [Min(0f)] public float characterSeconds = .12f;
    [Min(0f)] public float folderRiseSeconds = .45f;
    [Min(.01f)] public float jumpSeconds = 1.1f;
    [Min(0f)] public float jumpHeight = 600f;
    [Min(0f)] public float impactSeconds = .45f;
    [Min(0f)] public float impactDepth = 90f;
    [Min(0f)] public float folderHoldSeconds = 1f;
    [Min(0f)] public float folderExitSeconds = .45f;
    [Min(0f)] public float returnDissolveSeconds = .65f;
    bool awaitingConfirm, returning;
    bool[] buttonStates;
    UnityEngine.UI.Button[] buttons;
    public bool IsConfigured => sealing && conversation && maryFileGroup && folder && maryFileFade && fileName &&
        savedPopup && confirmButton && transitionCover && transitionCover.image && transitionCover.template;

    void Awake()
    {
        if (savedPopup) savedPopup.SetActive(false);
        if (folder) folder.gameObject.SetActive(false);
        if (fileName) fileName.gameObject.SetActive(false);
        if (transitionCover) transitionCover.gameObject.SetActive(false);
        if (confirmButton) confirmButton.onClick.AddListener(Confirm);
    }
    public void RestoreDesktopButtons()
    {
        buttons = new UnityEngine.UI.Button[sealing.desktopButtons.Length];
        buttonStates = new bool[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            var go = sealing.desktopButtons[i];
            if (!go) continue;
            go.SetActive(true);
            go.transform.SetAsLastSibling();
            buttons[i] = go.GetComponent<UnityEngine.UI.Button>();
            if (!buttons[i]) continue;
            buttonStates[i] = buttons[i].interactable;
            // 연출 도중 다른 창이 위로 올라오는 것을 막고 복귀 후 원래 상태로 돌린다.
            buttons[i].interactable = false;
        }
    }
    public IEnumerator StoreMary()
    {
        fileName.gameObject.SetActive(true);
        fileName.maxVisibleCharacters = 0;
        for (int i = 1; i <= fileName.text.Length; i++)
        {
            fileName.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(characterSeconds);
        }
        Vector2 folderRest = folder.anchoredPosition;
        Vector2 hidden = new(folderRest.x, -folder.rect.height * (1 - folder.pivot.y) - 30);
        folder.gameObject.SetActive(true);
        yield return Animate(folderRiseSeconds, t => folder.anchoredPosition = Vector2.Lerp(hidden, folderRest, t));
        Vector3 start = maryFileGroup.localPosition;
        Vector3 end = maryFileGroup.parent.InverseTransformPoint(folder.TransformPoint(new Vector3(0, folder.rect.height * .15f, 0)));
        Vector3 scale = maryFileGroup.localScale;
        // 등가속도 포물선: 상승한 뒤 속도가 붙으며 폴더 안으로 낙하한다.
        for (float elapsed = 0; elapsed < jumpSeconds; elapsed += Time.unscaledDeltaTime)
        {
            float t = elapsed / jumpSeconds;
            maryFileGroup.localPosition = Vector3.Lerp(start, end, t) + Vector3.up * (4 * jumpHeight * t * (1 - t));
            float enter = Mathf.InverseLerp(.72f, 1, t);
            maryFileGroup.localScale = scale * Mathf.Lerp(1, .1f, enter);
            maryFileFade.alpha = 1 - enter;
            yield return null;
        }
        maryFileGroup.gameObject.SetActive(false);
        yield return Animate(impactSeconds, t => folder.anchoredPosition = folderRest +
            Vector2.down * (Mathf.Sin(t * Mathf.PI * 2) * (1 - t) * impactDepth));
        folder.anchoredPosition = folderRest;
        yield return new WaitForSecondsRealtime(folderHoldSeconds);
        yield return Animate(folderExitSeconds, t => folder.anchoredPosition = Vector2.Lerp(folderRest, hidden, t));
        folder.gameObject.SetActive(false);
        savedPopup.SetActive(true);
        savedPopup.transform.SetAsLastSibling();
        confirmButton.interactable = true;
        awaitingConfirm = true;
    }
    void Confirm()
    {
        if (!awaitingConfirm || returning) return;
        awaitingConfirm = false; returning = true;
        confirmButton.interactable = false;
        StartCoroutine(ReturnToCall());
    }
    IEnumerator ReturnToCall()
    {
        // Dissolve the outgoing screen itself, revealing the restored theme underneath.
        // Reuse the scene-authored cover; only a temporary screen texture is created.
        transitionCover.gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        var snapshot = ScreenCapture.CaptureScreenshotAsTexture();
        var image = transitionCover.image;
        var previousTexture = image.texture;
        var previousColor = image.color;
        bool previousRaycast = image.raycastTarget;
        try
        {
            image.texture = snapshot;
            image.color = Color.white;
            image.raycastTarget = true;
            transitionCover.Set(dissolve: 0);
            savedPopup.SetActive(false);
            sealing.root.SetActive(false);
            conversation.ShowAfterSealing();
            transitionCover.gameObject.SetActive(true);
            transitionCover.transform.SetAsLastSibling();
            yield return Animate(returnDissolveSeconds, t => transitionCover.Set(dissolve: t));
        }
        finally
        {
            transitionCover.gameObject.SetActive(false);
            image.texture = previousTexture;
            image.color = previousColor;
            image.raycastTarget = previousRaycast;
            if (snapshot) Destroy(snapshot);
        }
        if (buttons != null)
            for (int i = 0; i < buttons.Length; i++) if (buttons[i]) buttons[i].interactable = buttonStates[i];
    }
    static IEnumerator Animate(float seconds, Action<float> apply)
    {
        apply(0);
        for (float elapsed = 0; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
        { apply(Mathf.SmoothStep(0,1,elapsed / seconds)); yield return null; }
        apply(1);
    }
    void OnDestroy() { if (confirmButton) confirmButton.onClick.RemoveListener(Confirm); }
}
