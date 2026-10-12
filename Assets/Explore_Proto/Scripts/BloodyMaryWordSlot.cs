using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>A scene-saved word button with a pointer press bound to its current identity.</summary>
[DisallowMultipleComponent]
public sealed class BloodyMaryWordSlot : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
{
    public TMPro.TextMeshProUGUI label;
    public UnityEngine.UI.Button button;
    public BloodyMaryPhaseTwoController phase;
    public BloodyMaryPhraseSlot phrase;
    public int wordIndex;
    public bool selectedWord;
    public bool Visible { get; private set; }
    int revision, pressedRevision, pointer;
    bool armed;

    public void Show(string value, int identity)
    {
        revision = identity;
        armed = false;
        Visible = !string.IsNullOrEmpty(value);
        label.text = value;
        label.enabled = Visible;
        label.raycastTarget = Visible;
        button.interactable = Visible;
    }

    public void Hide() { Show(string.Empty, revision + 1); }

    bool CanClick => Visible && phase && phase.CanInteract && button.IsActive() && button.IsInteractable()
        && (selectedWord || (phrase && phrase.IsVisible));

    public void OnPointerDown(PointerEventData data)
    {
        armed = false;
        if (data.button != PointerEventData.InputButton.Left || !CanClick) return;
        armed = true;
        pressedRevision = revision;
        pointer = data.pointerId;
    }

    public void OnPointerClick(PointerEventData data)
    {
        bool valid = armed && pointer == data.pointerId && pressedRevision == revision;
        armed = false;
        float threshold = EventSystem.current ? EventSystem.current.pixelDragThreshold : 5f;
        if (!valid || !CanClick || data.button != PointerEventData.InputButton.Left || data.dragging ||
            !data.eligibleForClick || (data.position - data.pressPosition).sqrMagnitude > threshold * threshold) return;
        if (selectedWord) phase.RemoveWord(wordIndex, revision);
        else phase.CollectWord(phrase, wordIndex, revision);
    }

    void OnDisable() { armed = false; }
}
