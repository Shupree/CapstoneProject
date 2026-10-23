using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>탐색 씬의 메일 열람과 이서연 연락처 등록을 관리한다.</summary>
public sealed class ExploreProtoEmailController : MonoBehaviour
{
    [SerializeField] private GameObject emailWindow;
    [SerializeField] private GameObject unreadBadge;
    [SerializeField] private TMP_Text unreadCount;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button addContactButton;
    [SerializeField] private TMP_Text addContactLabel;
    [SerializeField] private GameObject contactEntry;
    [SerializeField] private GameObject conversationPane;
    [SerializeField] private GameObject emptyConversation;
    [SerializeField] private ExploreProtoMessenger messenger;

    private bool contactAdded;
    public bool ContactAdded => contactAdded;

    private void Awake()
    {
        contactAdded = false;
        emailWindow.SetActive(false);
        unreadCount.text = "1";
        unreadBadge.SetActive(true);
        contactEntry.SetActive(false);
        conversationPane.SetActive(false);
        if (emptyConversation) emptyConversation.SetActive(true);
        addContactButton.interactable = true;
        addContactLabel.text = "메신저에 추가";
        messenger.SetContactRegistered(false);
        closeButton.onClick.AddListener(CloseEmail);
        addContactButton.onClick.AddListener(AddContact);
    }

    public void ToggleEmail()
    {
        if (emailWindow.activeSelf) CloseEmail();
        else ShowEmail();
    }

    public void ShowEmail()
    {
        emailWindow.SetActive(true);
        emailWindow.transform.SetAsLastSibling();
        unreadBadge.SetActive(false);
        unreadCount.text = "0";
    }

    public void CloseEmail() => emailWindow.SetActive(false);

    public void AddContact()
    {
        if (contactAdded) return;
        contactAdded = true;
        contactEntry.SetActive(true);
        conversationPane.SetActive(true);
        if (emptyConversation) emptyConversation.SetActive(false);
        addContactButton.interactable = false;
        addContactLabel.text = "메신저에 추가됨";
        messenger.SetContactRegistered(true);
    }
}
