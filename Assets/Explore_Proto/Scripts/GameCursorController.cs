using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>Uses the foremost UI hit to choose a native cursor without creating UI.</summary>
[DisallowMultipleComponent]
public sealed class GameCursorController : MonoBehaviour
{
    public Texture2D defaultCursor;
    public Texture2D pointerCursor;
    [Tooltip("원본 이미지의 왼쪽 위 기준 클릭 지점 (픽셀)")]
    public Vector2 defaultHotspot = new(5f, 1f);
    public Vector2 pointerHotspot = new(7f, 1f);

    readonly List<RaycastResult> hits = new();
    EventSystem cachedEventSystem;
    PointerEventData pointer;
    Texture2D appliedTexture;
    Vector2 appliedHotspot;
    bool applied;

    void OnEnable() => Apply(false);

    void LateUpdate()
    {
        var mouse = Mouse.current;
        if (!Application.isFocused || mouse == null)
        {
            RestoreCursor();
            return;
        }
        Vector2 position = mouse.position.ReadValue();
        if (position.x < 0f || position.y < 0f || position.x >= Screen.width || position.y >= Screen.height)
        {
            RestoreCursor();
            return;
        }
        Apply(IsPointerTarget(position));
    }

    bool IsPointerTarget(Vector2 position)
    {
        var system = EventSystem.current;
        if (!system) return false;
        if (cachedEventSystem != system || pointer == null)
        {
            cachedEventSystem = system;
            pointer = new PointerEventData(system);
        }
        pointer.Reset();
        pointer.position = position;
        hits.Clear();
        system.RaycastAll(pointer, hits);
        if (hits.Count == 0) return false;

        // Never look through an overlay or a disabled button to something behind it.
        var target = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
        if (!target) return false;
        var selectable = target.GetComponent<UnityEngine.UI.Selectable>();
        if (selectable && (!selectable.IsActive() || !selectable.IsInteractable())) return false;

        var setting = target.GetComponent<GameCursorTarget>();
        if (setting) return setting.showPointer;

        // Text-only buttons and clickable article/syllable text keep the arrow.
        // A normal button's child caption does not turn that button into a text target.
        if (target.GetComponent<TMP_Text>() || target.GetComponent<UnityEngine.UI.Text>() ||
            target.GetComponent<TMP_InputField>() || target.GetComponent<UnityEngine.UI.InputField>() ||
            target.GetComponent<Battle02WordSource>() || target.GetComponent<BloodyMaryWordSlot>() ||
            target.GetComponent<BloodyMarySyllableSource>() || target.GetComponent<ExploreProtoArticleLinks>() ||
            target.GetComponent<BloodyMaryFigureImage>()) return false;
        if (selectable && (selectable.targetGraphic is TMP_Text || selectable.targetGraphic is UnityEngine.UI.Text))
            return false;
        return true;
    }

    void Apply(bool showPointer)
    {
        var texture = showPointer && pointerCursor ? pointerCursor : defaultCursor;
        var hotspot = showPointer && pointerCursor ? pointerHotspot : defaultHotspot;
        if (applied && appliedTexture == texture && appliedHotspot == hotspot) return;
        Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
        appliedTexture = texture;
        appliedHotspot = hotspot;
        applied = true;
    }

    void RestoreCursor()
    {
        if (!applied) return;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        applied = false;
    }

    void OnApplicationFocus(bool focused)
    {
        if (!focused) RestoreCursor();
    }
    void OnDisable() => RestoreCursor();
}
