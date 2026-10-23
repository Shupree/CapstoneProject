using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Highlights and collects only the marked sentences inside the article's TMP links.</summary>
public sealed class ExploreProtoArticleLinks : MonoBehaviour, IPointerEnterHandler,
    IPointerMoveHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text article;
    public UnityEngine.UI.ScrollRect articleScroll;
    public ExploreProtoInvestigation investigation;
    public ExploreProtoExorcismVision exorcismVision;
    string source = string.Empty, hoveredId, rendered;
    bool pointerInside;
    Vector2 pointerPosition;
    Camera eventCamera;
    static readonly Regex Links = new Regex("<link=\"([^\"]+)\">(.*?)</link>", RegexOptions.Singleline);
    static readonly Regex HiddenByExorcism = new Regex("<vision>(.*?)</vision>", RegexOptions.Singleline);

    void OnEnable()
    {
        pointerInside = false; hoveredId = null;
        articleScroll.onValueChanged.AddListener(OnScrolled);
        if (exorcismVision) exorcismVision.StateChanged += OnVisionChanged;
        Render();
    }

    void OnDisable()
    {
        articleScroll.onValueChanged.RemoveListener(OnScrolled);
        if (exorcismVision) exorcismVision.StateChanged -= OnVisionChanged;
        pointerInside = false; hoveredId = null;
    }

    public void SetArticle(string value)
    {
        source = value;
        hoveredId = null; pointerInside = false;
        Render();
        investigation.SetArticleHint(null);
    }

    private void OnVisionChanged(bool enabled)
    {
        hoveredId = null;
        Render();
        if (pointerInside) SetHovered(LinkAt(pointerPosition, eventCamera));
        else investigation.SetArticleHint(null);
    }

    public void OnPointerEnter(PointerEventData data) => Track(data);
    public void OnPointerMove(PointerEventData data) => Track(data);
    public void OnPointerExit(PointerEventData data)
    {
        pointerInside = false;
        SetHovered(null);
    }
    void Track(PointerEventData data)
    {
        pointerInside = true; pointerPosition = data.position; eventCamera = data.enterEventCamera;
        SetHovered(LinkAt(pointerPosition, eventCamera));
    }
    void OnScrolled(Vector2 position)
    {
        if (pointerInside) SetHovered(LinkAt(pointerPosition, eventCamera));
    }
    string LinkAt(Vector2 position, Camera camera)
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(articleScroll.viewport, position, camera)) return null;
        int index = TMP_TextUtilities.FindIntersectingLink(article, position, camera);
        if (index < 0) return null;
        string id = article.textInfo.linkInfo[index].GetLinkID();
        return investigation.IsClue(id) ? id : null;
    }
    void SetHovered(string id)
    {
        if (hoveredId == id) return;
        hoveredId = id;
        Render();
        investigation.SetArticleHint(id);
    }
    public void OnPointerClick(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left || data.dragging) return;
        string id = LinkAt(data.position, data.pressEventCamera);
        if (id == null) return;
        investigation.Collect(id);
        Render();
        investigation.SetArticleHint(id);
    }
    void Render()
    {
        // 퇴마프로그램이 꺼져 있으면 숨겨진 문장의 링크 자체가 TMP에 전달되지 않는다.
        string visibleSource = HiddenByExorcism.Replace(source,
            match => exorcismVision && exorcismVision.IsActive ? match.Groups[1].Value : string.Empty);
        string next = Links.Replace(visibleSource, match =>
        {
            string id = match.Groups[1].Value, body = match.Groups[2].Value;
            if (investigation.IsCollected(id)) body = "<color=#327466><u>" + body + "</u></color>";
            else if (id == hoveredId) body = "<mark=#F2C56B66><u>" + body + "</u></mark>";
            return "<link=\"" + id + "\">" + body + "</link>";
        });
        if (rendered == next) return;
        rendered = next; article.text = next;
        // Highlighting changes decoration only, so hovering never changes line wrapping.
        if (article.gameObject.activeInHierarchy) article.ForceMeshUpdate();
    }
}
