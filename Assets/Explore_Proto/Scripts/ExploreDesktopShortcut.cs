using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ExploreDesktopShortcut : MonoBehaviour, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    private enum App { Mail, Internet, Chat, Memo }
    [SerializeField] private App app;
    [SerializeField] private Image highlight;
    [SerializeField] private ExploreProtoMessenger desktop;
    [SerializeField] private ExploreProtoEmailController mail;
    private float lastClick = float.NegativeInfinity;

    public void OnSelect(BaseEventData data) => highlight.color = new Color(0.15f, 0.32f, 0.65f, 0.55f);

    public void OnDeselect(BaseEventData data)
    {
        highlight.color = Color.clear;
        lastClick = float.NegativeInfinity;
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (data.button != PointerEventData.InputButton.Left) return;
        EventSystem.current.SetSelectedGameObject(gameObject, data);
        float now = Time.unscaledTime;
        if (now - lastClick > 0.5f)
        {
            lastClick = now;
            return;
        }
        lastClick = float.NegativeInfinity;
        switch (app)
        {
            case App.Mail: mail.ShowEmail(); break;
            case App.Chat: desktop.ShowMessenger(); break;
            case App.Internet:
                if (desktop.internetWindow.activeSelf) desktop.internetWindow.transform.SetAsLastSibling();
                else desktop.ToggleInternet();
                break;
            case App.Memo:
                if (desktop.memoWindow.activeSelf) desktop.memoWindow.transform.SetAsLastSibling();
                else desktop.ToggleMemo();
                break;
        }
    }
}
