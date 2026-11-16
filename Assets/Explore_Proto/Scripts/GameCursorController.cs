using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>Draws the cursor in game coordinates and uses the foremost UI hit to select its texture.</summary>
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
    [Tooltip("최대화한 Game 뷰에서 게임 영역의 기준 높이(화면 픽셀)")]
    [Min(1f)] public float referenceHeight = 835f;
    bool applied;
    bool previousVisibility;



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
        if (!texture)
        {
            RestoreCursor();
            return;
        }
        if (!applied) previousVisibility = Cursor.visible;
        Cursor.visible = false;
        appliedTexture = texture;
        appliedHotspot = hotspot;
        applied = true;
    }

    void OnGUI()
    {
        if (!applied || Event.current.type != EventType.Repaint || Mouse.current == null) return;
        Vector2 point = Mouse.current.position.ReadValue();
        float scale = 2f * Screen.height / Mathf.Max(1f, referenceHeight);
        var rect = new Rect(point.x - appliedHotspot.x * scale,
            Screen.height - point.y - appliedHotspot.y * scale,
            appliedTexture.width * scale, appliedTexture.height * scale);
        GUI.depth = -10000;
        GUI.DrawTexture(rect, appliedTexture);
    }

    void RestoreCursor()
    {
        if (!applied) return;
        Cursor.visible = previousVisibility;
        applied = false;
    }

    void OnApplicationFocus(bool focused)
    {
        if (!focused) RestoreCursor();
    }
    void OnDisable() => RestoreCursor();
}
