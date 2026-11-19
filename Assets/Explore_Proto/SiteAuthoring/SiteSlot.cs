using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class SiteSlot : MonoBehaviour
{
    [HideInInspector] public string id;
    [HideInInspector] public SiteSlotKind kind;
    void Reset() => id = System.Guid.NewGuid().ToString("N");
}
