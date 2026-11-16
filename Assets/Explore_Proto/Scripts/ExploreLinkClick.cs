using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public sealed class ExploreLinkClick : MonoBehaviour, IPointerClickHandler, IScrollHandler
{
    public ExploreLinkCase investigation;

    public void OnScroll(PointerEventData eventData)
    {
        if (eventData.scrollDelta.y == 0) return;
        var label = GetComponent<TMP_Text>();
        int step = eventData.scrollDelta.y < 0 ? 1 : -1;
        if (label == investigation.article) investigation.TurnSheet(step);
        else if (label == investigation.postComments || label == investigation.postBody)
            label.pageToDisplay = Mathf.Clamp(label.pageToDisplay + step, 1, Mathf.Max(1, label.textInfo.pageCount));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        var label = GetComponent<TMP_Text>();
        int index = TMP_TextUtilities.FindIntersectingLink(label, eventData.position, eventData.pressEventCamera);
        if (index >= 0) investigation.Follow(label.textInfo.linkInfo[index].GetLinkID());
    }
}
