using System;
using TMPro;
using UnityEngine;

namespace CyberExorcist
{
    /// <summary>References to authored scene objects. Refresh never creates or replaces UI.</summary>
    public sealed class PrototypeSceneUI : MonoBehaviour
    {
        [Serializable]
        public sealed class TextStates
        {
            public TextMeshProUGUI Label;
            [TextArea(2, 6)] public string[] Values;
            public void Show(int index)
            {
                if (Label && Values != null && index >= 0 && index < Values.Length)
                    Label.text = Values[index];
            }
        }

        [Header("Desktop / shared window")]
        public RectTransform Desktop;
        public RectTransform Window;
        public GameObject[] AppPanels;
        public TextStates WindowTitle;
        public TextMeshProUGUI Clock;
        public GameObject Notification;
        public TextMeshProUGUI NotificationText;
        public GameObject RestartDialog;
        public TextStates MuteLabel;
        public AudioSource Sound;

        [Header("Mail / chat")]
        public TextStates ContactLabel;
        public GameObject ChatEmpty;
        public GameObject ChatConversation;
        [Tooltip("Conversation steps 0–3, followed by the completed case.")]
        public GameObject[] ChatStages;

        [Header("Browser")]
        [Tooltip("Search, article, information, haunted site.")]
        public GameObject[] BrowserPages;
        public TextStates Address;
        public UnityEngine.UI.Button BackButton;
        public GameObject SearchKeywords;
        public GameObject SearchNoKeywords;
        public GameObject SearchIntro;
        public GameObject SearchResults;
        [Tooltip("Empty, first keyword, second keyword, both keywords.")]
        public TextStates SearchText;
        public TextStates TermA;
        public TextStates TermB;
        public TextStates ArticleClue;
        public TextStates ArticleHint;
        public UnityEngine.UI.Button EnterSite;

        [Header("Haunted site")]
        [Tooltip("Isolation, cameras, ghost conversation, ending.")]
        public GameObject[] HauntedPhases;
        public GameObject[] Cameras;
        public GameObject Glimpse;
        [Tooltip("Indexed by the number of removed cameras (0–5).")]
        public TextStates CameraCount;
        public RectTransform GhostArt;
        public CanvasGroup GhostGroup;
        public TextStates GhostSpeaker;
        public TextStates GhostLines;
        public TextStates GhostChoice;
        public UnityEngine.UI.Button GhostNext;

        [Header("Case notebook")]
        public GameObject[] PendingSteps;
        public GameObject[] DoneSteps;
        public TextStates CaseStatus;
        public TextStates CollectedKeywords;
        public TextStates ClueSummary;
        public TextStates Goal;

        public void Refresh(CyberExorcistPrototype state)
        {
            int appIndex = state.CurrentApp == "mail" ? 0 : state.CurrentApp == "chat" ? 1 : 2;
            Select(AppPanels, appIndex);
            WindowTitle.Show(appIndex);
            ContactLabel.Show(state.ContactAdded ? 1 : 0);
            Set(ChatEmpty, !state.ContactAdded);
            Set(ChatConversation, state.ContactAdded);
            Select(ChatStages, state.Completed ? 4 : state.ChatStep);

            int pageIndex = state.BrowserPage == "article" ? 1 :
                state.BrowserPage == "side" ? 2 : state.BrowserPage == "haunted" ? 3 : 0;
            Select(BrowserPages, pageIndex);
            Address.Show(pageIndex);
            BackButton.interactable = state.CanGoBack;
            Set(SearchKeywords, state.KeywordAcquired);
            Set(SearchNoKeywords, !state.KeywordAcquired);
            Set(SearchIntro, state.BrowserPage != "results");
            Set(SearchResults, state.BrowserPage == "results");
            SearchText.Show((state.TermASelected ? 1 : 0) + (state.TermBSelected ? 2 : 0));
            TermA.Show(state.TermASelected ? 1 : 0);
            TermB.Show(state.TermBSelected ? 1 : 0);
            ArticleClue.Show(state.ClueFound ? 1 : 0);
            ArticleHint.Show(state.ClueFound ? 1 : 0);
            EnterSite.interactable = state.ClueFound;

            Select(HauntedPhases, state.Completed ? 3 : !state.Isolated ? 0 : state.CamerasRemoved < 5 ? 1 : 2);
            for (int i = 0; i < Cameras.Length; i++) Set(Cameras[i], !state.IsCameraRemoved(i));
            Set(Glimpse, state.CamerasRemoved >= 2);
            CameraCount.Show(state.CamerasRemoved);
            GhostSpeaker.Show(state.GhostDialogue == 0 ? 0 : 1);
            GhostLines.Show(state.GhostDialogue);
            GhostChoice.Show(state.Departing ? 4 : state.GhostDialogue);
            GhostNext.interactable = !state.Departing;

            bool[] checks = { state.ContactAdded, state.KeywordAcquired, state.ClueFound, state.CamerasRemoved == 5, state.Completed };
            for (int i = 0; i < checks.Length; i++)
            {
                Set(PendingSteps[i], !checks[i]);
                Set(DoneSteps[i], checks[i]);
            }
            CaseStatus.Show(state.Completed ? 1 : 0);
            CollectedKeywords.Show(state.KeywordAcquired ? 1 : 0);
            ClueSummary.Show(!state.ClueFound ? 0 : state.CamerasRemoved == 5 ? 2 : 1);
            Goal.Show(state.Completed ? 5 : !state.ContactAdded ? 0 : !state.KeywordAcquired ? 1 :
                !state.ClueFound ? 2 : state.CamerasRemoved < 5 ? 3 : 4);
            MuteLabel.Show(state.Muted ? 1 : 0);
        }

        static void Select(GameObject[] objects, int selected)
        {
            for (int i = 0; i < objects.Length; i++) Set(objects[i], i == selected);
        }

        static void Set(GameObject target, bool active)
        {
            if (target && target.activeSelf != active) target.SetActive(active);
        }
    }
}

