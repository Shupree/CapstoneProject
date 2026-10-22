using UnityEngine;

/// <summary>Per-camera parameters consumed by the full-screen URP render pass.</summary>
[DisallowMultipleComponent]
public sealed class BattleScreenInterference : MonoBehaviour
{
    public int GlitchLines { get; private set; }
    public float CRTAmount { get; private set; }
    public bool IsActive => GlitchLines > 0 || CRTAmount > 0f;
    public void SetLines(int count) => GlitchLines = Mathf.Clamp(count, 0, 2);
    public void SetCRT(float amount) => CRTAmount = Mathf.Clamp01(amount);
    public void Clear() { GlitchLines = 0; CRTAmount = 0f; }
    void OnDisable() => Clear();
}
