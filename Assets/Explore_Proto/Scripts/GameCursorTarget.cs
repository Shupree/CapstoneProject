using UnityEngine;

/// <summary>Overrides cursor appearance for a clickable UI object; does not change input.</summary>
[DisallowMultipleComponent]
public sealed class GameCursorTarget : MonoBehaviour
{
    [Tooltip("켜면 손가락 커서, 끄면 기본 커서. 클릭 처리 컴포넌트와 같은 오브젝트에 추가합니다.")]
    public bool showPointer;
}
