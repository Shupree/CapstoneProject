using UnityEngine.EventSystems;

/// <summary>Pointer-only submission: keyboard navigation must not submit this puzzle.</summary>
public sealed class BloodyMarySubmitButton : UnityEngine.UI.Button
{
    public override void OnSubmit(BaseEventData eventData) { }
}
