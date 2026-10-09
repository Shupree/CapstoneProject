using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace CyberExorcist.Editor
{
    /// <summary>Initial migration only. The saved scene is the source of truth after creation.</summary>
    public sealed class PrototypeSceneBuilder
    {
        CyberExorcistPrototype controller;
        PrototypeSceneUI ui;
        TMP_FontAsset KoreanFont;
        Texture2D CameraArtwork, GhostReference;
        RectTransform desktop, window, body, record, modal;
        TextMeshProUGUI goalLabel, clockLabel, toastLabel;
        RectTransform ghostArt;
        CanvasGroup ghostGroup;
        Vector2 ghostBase;
        int chatStep, ghostStep;
        // Initial states used solely to preserve the original prototype artwork and layout.
        const bool ContactAdded = false, ClueFound = false, termA = false, termB = false;
        readonly Color ink = C("252C46"), mutedInk = C("65718A"), blue = C("3C5BE8"),
            paper = C("FAFBFF"), pale = C("EDF1FB"), green = C("287663");

        [MenuItem("Tools/Cyber Exorcist/Create Scene UI (Once)")]
        public static void Create()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Stop Play mode before creating scene UI.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != PrototypeSetup.ScenePath)
                throw new InvalidOperationException("Open " + PrototypeSetup.ScenePath + " first.");
            var target = UnityEngine.Object.FindFirstObjectByType<CyberExorcistPrototype>();
            if (!target) throw new InvalidOperationException("Prototype controller is missing.");
            if (target.UI || target.transform.Find("ExorcistDesktop"))
                throw new InvalidOperationException("Scene UI already exists. Edit its objects directly; rebuilding would discard authored edits.");
            if (!target.KoreanFont || !target.CameraArtwork || !target.GhostReference)
                throw new InvalidOperationException("Prototype artwork or font references are missing.");

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create authored prototype UI");
            Undo.RecordObject(target, "Connect scene UI");
            var builder = new PrototypeSceneBuilder
            {
                controller = target, KoreanFont = target.KoreanFont,
                CameraArtwork = target.CameraArtwork, GhostReference = target.GhostReference
            };
            builder.ui = Undo.AddComponent<PrototypeSceneUI>(target.gameObject);
            target.UI = builder.ui;
            builder.BuildAll();
            Undo.RegisterCreatedObjectUndo(target.transform.Find("ExorcistDesktop").gameObject, "Create scene UI");
            var eventSystem = target.transform.Find("EventSystem");
            if (eventSystem) Undo.RegisterCreatedObjectUndo(eventSystem.gameObject, "Create UI input");
            EditorUtility.SetDirty(target);
            EditorUtility.SetDirty(builder.ui);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = builder.ui.Window.gameObject;
            Debug.Log("Scene UI created. Save the scene, then edit ExorcistDesktop directly.");
        }

        static PrototypeSceneUI.TextStates States(TextMeshProUGUI label, params string[] values)
        {
            return new PrototypeSceneUI.TextStates { Label = label, Values = values };
        }

        RectTransform Panel(Transform parent, string name)
        {
            var panel = Rect(parent, name, 0, 0, 1120, 641);
            return panel;
        }

        void BindButton(string id, UnityEngine.UI.Button button, TextMeshProUGUI label)
        {
            switch (id)
            {
                case "contact": ui.ContactLabel = States(label, "+  minseo_404 연락처 추가", "민서에게 연락하기  →"); break;
                case "mute": ui.MuteLabel = States(label, "소리 켜짐", "소리 꺼짐"); break;
                case "term-a": ui.TermA = States(label, "+ 관음증", "− 관음증"); break;
                case "term-b": ui.TermB = States(label, "+ 귀신", "− 귀신"); break;
                case "enter-site": ui.EnterSite = button; break;
            }
        }

        void BuildAll()
        {
            BuildDesktop();
            BuildRecord();
            var header = Box(window, "TitleBar", 0, 0, 1120, 51, C("DFE7F8"), true);
            header.gameObject.AddComponent<PrototypeWindowDrag>().Initialize(window);
            ui.WindowTitle = States(Text(header, "01  /  받은 메일", 23, 12, 890, 33, 20, ink, true),
                "01  /  받은 메일", "02  /  메신저", "03  /  인터넷");
            Button(header, "minimize", "—", 1010, 7, 45, 36);
            Button(header, "minimize", "×", 1064, 7, 45, 36);
            var content = Rect(window, "Content", 0, 51, 1120, 641);
            var mail = Panel(content, "Mail");
            var chat = Panel(content, "Chat");
            var browser = Panel(content, "Browser");
            ui.AppPanels = new[] { mail.gameObject, chat.gameObject, browser.gameObject };
            body = mail; Mail();
            body = chat; BuildChat();
            body = browser; BuildBrowser();
            ConfirmRestart();
            ui.RestartDialog = modal.gameObject;
            ui.RestartDialog.SetActive(false);
            ui.Refresh(controller);
            ui.Notification.SetActive(false);
        }

        static Color C(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var color); return color; }

        RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        RectTransform Box(Transform parent, string name, float x, float y, float w, float h, Color color, bool hit = false)
        {
            var rt = Rect(parent, name, x, y, w, h);
            var image = rt.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color; image.raycastTarget = hit;
            return rt;
        }

        TextMeshProUGUI Text(Transform parent, string value, float x, float y, float w, float h, int size = 20, Color? color = null, bool bold = false)
        {
            var rt = Rect(parent, "Text_" + (value.Length == 0 ? "Label" : value.Substring(0, Mathf.Min(value.Length, 24)).Replace("\n", " ").Replace("/", "_")), x, y, w, h);
            var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = KoreanFont; text.text = value; text.fontSize = size; text.color = color ?? ink;
            text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            text.raycastTarget = false; text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Ellipsis; text.margin = Vector4.zero;
            return text;
        }

        UnityEngine.UI.Button Button(Transform parent, string id, string label, float x, float y, float w, float h, bool primary = false, bool enabled = true)
        {
            var rt = Box(parent, id, x, y, w, h, primary ? blue : pale, true);
            var button = rt.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = rt.GetComponent<UnityEngine.UI.Image>(); button.interactable = enabled;
            var colors = button.colors; colors.highlightedColor = C("DCE3FF"); colors.pressedColor = C("BCC9F8");
            colors.disabledColor = new Color(.78f, .79f, .83f, .55f); button.colors = colors;
            var labelText = Text(rt, label, 8, 0, w - 16, h, 19, primary ? Color.white : ink, primary);
            labelText.alignment = TextAlignmentOptions.Midline;
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(button.onClick, controller.Click, id);
            BindButton(id, button, labelText);
            return button;
        }


        void BuildDesktop()
        {
            ui.Sound = controller.GetComponent<AudioSource>();
            if (!ui.Sound) ui.Sound = Undo.AddComponent<AudioSource>(controller.gameObject);
            ui.Sound.playOnAwake = false; ui.Sound.volume = .13f;
            var canvasObject = new GameObject("ExorcistDesktop", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasObject.transform.SetParent(controller.transform, false);
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900); scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            var bg = Box(canvas.transform, "FullScreenBackground", 0, 0, 0, 0, C("B8CAF1"));
            bg.anchorMin = Vector2.zero; bg.anchorMax = Vector2.one; bg.offsetMin = bg.offsetMax = Vector2.zero;
            desktop = Rect(canvas.transform, "Desktop", 0, 0, 1600, 900);
            desktop.anchorMin = desktop.anchorMax = desktop.pivot = new Vector2(.5f, .5f);
            desktop.anchoredPosition = Vector2.zero;
            for (int i = 0; i < 20; i++) Box(desktop, "GridLine", i * 86, 0, 1, 900, new Color(1, 1, 1, .13f));
            for (int i = 0; i < 12; i++) Box(desktop, "GridLine", 0, i * 86, 1600, 1, new Color(1, 1, 1, .13f));
            Box(desktop, "TopBar", 0, 0, 1600, 64, C("E9EFFB"));
            Text(desktop, "AFTER / NET", 30, 17, 260, 33, 24, ink, true);
            Text(desktop, "사이버 퇴마 사무소", 280, 21, 350, 30, 17, mutedInk);
            Text(desktop, "CASE 01   /   지켜보는 눈", 1080, 21, 430, 30, 17, mutedInk);
            Box(desktop, "Dock", 16, 108, 90, 350, C("E9EFFB"));
            Button(desktop, "app:mail", "메일", 25, 123, 72, 82);
            Button(desktop, "app:chat", "대화", 25, 221, 72, 82);
            Button(desktop, "app:browser", "검색", 25, 319, 72, 82);
            Text(desktop, "INTERNET\nEXORCIST", 145, 260, 980, 170, 60, new Color(.23f, .34f, .61f, .2f), true);
            Text(desktop, "남겨진 기록을 따라, 화면 너머의 존재에게.", 150, 445, 900, 50, 25, C("6F87B5"));
            window = Box(desktop, "ApplicationWindow", 130, 103, 1120, 692, paper, true);
            var shadow = window.gameObject.AddComponent<UnityEngine.UI.Shadow>(); shadow.effectDistance = new Vector2(5, -7); shadow.effectColor = new Color(.18f, .25f, .43f, .18f);
            record = Box(desktop, "CaseNotebook", 1280, 108, 294, 687, C("EAF0FB"));
            Box(desktop, "TaskBar", 0, 827, 1600, 73, C("E9EFFB"));
            Button(desktop, "desktop", "바탕화면", 24, 843, 138, 42);
            goalLabel = Text(desktop, "", 190, 851, 820, 35, 19, ink);
            Button(desktop, "mute", "소리 켜짐", 1075, 843, 145, 42);
            Button(desktop, "restart", "처음부터", 1235, 843, 135, 42);
            clockLabel = Text(desktop, "", 1390, 855, 190, 30, 16, mutedInk);
            var toast = Box(desktop, "Notification", 394, 69, 770, 30, C("263451"));
            toastLabel = Text(toast, "", 22, 4, 726, 24, 16, Color.white);
            modal = Rect(desktop, "ModalLayer", 0, 0, 1600, 900);
            if (!UnityEngine.Object.FindFirstObjectByType<EventSystem>())
            {
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(controller.transform, false);
            }
            ui.Desktop = desktop; ui.Window = window; ui.Clock = clockLabel;
            ui.Notification = toast.gameObject; ui.NotificationText = toastLabel;
            ui.Goal = States(goalLabel, "현재 목표  /  메일의 연락처를 추가하세요.", "현재 목표  /  민서와 대화해 단서를 얻으세요.", "현재 목표  /  키워드를 조합해 괴담 기록을 조사하세요.", "현재 목표  /  문제의 사이트에서 촬영도구를 치우세요.", "현재 목표  /  귀신의 이야기를 듣고 성불을 도와주세요.", "현재 목표  /  의뢰 해결. 민서의 답장을 확인하세요.");
        }


        void BuildRecord()
        {
            Text(record, "CASE FILE / 001", 22, 25, 250, 27, 16, blue, true);
            Text(record, "지켜보는 눈", 22, 69, 255, 49, 29, ink, true);
            ui.CaseStatus = States(Text(record, "조사 중 · 의뢰인 민서", 22, 121, 250, 34, 17, mutedInk),
                "조사 중 · 의뢰인 민서", "종결 · 잔류 신호 없음");
            Box(record, "Divider", 22, 171, 250, 1, C("C9D4E9"));
            string[] steps = { "의뢰 접수", "키워드 확보", "괴담 기록 대조", "숨은 괴이 발견", "대화와 성불" };
            ui.PendingSteps = new GameObject[5]; ui.DoneSteps = new GameObject[5];
            for (int i = 0; i < steps.Length; i++)
            {
                for (int done = 0; done < 2; done++)
                {
                    var row = Rect(record, "Step" + (i + 1) + (done == 1 ? "_Done" : "_Pending"), 0, 195 + i * 49, 294, 49);
                    Text(row, done == 1 ? "●" : "○", 23, 0, 28, 29, 20, done == 1 ? green : mutedInk);
                    Text(row, steps[i], 59, 2, 217, 29, 18, done == 1 ? green : mutedInk, done == 1);
                    if (done == 1) ui.DoneSteps[i] = row.gameObject; else ui.PendingSteps[i] = row.gameObject;
                }
            }
            Box(record, "Divider", 22, 463, 250, 1, C("C9D4E9"));
            Text(record, "수집한 단서", 23, 485, 240, 29, 17, ink, true);
            ui.CollectedKeywords = States(Text(record, "", 23, 527, 245, 62, 18, blue),
                "아직 수집된 키워드가 없습니다.", "관음증 / 귀신");
            ui.ClueSummary = States(Text(record, "", 23, 591, 245, 70, 16, mutedInk),
                "메일과 게시글의 증상을\n대조해 연결점을 찾으세요.",
                "자동 녹화 + 눈동자 아이콘\n의뢰인과 같은 증상 확인",
                "자동 녹화 + 눈동자 아이콘\n촬영도구 뒤에 숨은 존재");
        }

        void Mail()
        {
            Box(body, "MailSidebar", 0, 0, 217, 641, pale);
            Text(body, "MAILBOX", 25, 31, 177, 32, 16, mutedInk, true);
            Button(body, "noop", "받은 편지함  1", 16, 92, 185, 47, true);
            Text(body, "의뢰함\n\n보낸 메일\n\n보관함", 32, 164, 165, 150, 18, mutedInk);
            Text(body, "NEW REQUEST", 258, 31, 790, 30, 15, blue, true);
            Text(body, "누군가 제 화면을 보고 있어요", 258, 77, 820, 57, 34, ink, true);
            Text(body, "민서  <minseo_404>                         오늘 10:35", 258, 143, 825, 34, 17, mutedInk);
            Box(body, "Divider", 258, 192, 820, 1, C("DAE0ED"));
            Text(body, "안녕하세요. 인터넷 퇴마 의뢰를 드립니다.\n\n며칠 전 이상한 사이트에 접속한 뒤로\n누군가 계속 저를 지켜보는 느낌이 들어요.\n\n화면 녹화가 저절로 켜지고,\n바탕화면에는 눈동자 아이콘이 자꾸 생깁니다.\n\n보안 업체에서도 원인을 찾지 못했어요. 제발 도와주세요.", 258, 218, 820, 300, 21);
            Button(body, "contact", ContactAdded ? "민서에게 연락하기  →" : "+  minseo_404 연락처 추가", 258, 548, 418, 52, true);
            Text(body, "첨부파일 없음 · 메시지 1개", 726, 565, 360, 32, 15, mutedInk);
        }


        void BuildChat()
        {
            var root = body;
            Box(root, "Contacts", 0, 0, 217, 641, pale);
            Text(root, "CONTACTS", 24, 31, 180, 32, 16, mutedInk, true);
            body = Panel(root, "NoContact");
            ui.ChatEmpty = body.gameObject;
            Text(body, "등록된 연락처가 없습니다", 297, 195, 750, 56, 30, ink, true);
            Text(body, "메일의 연락처를 눌러 의뢰인을 추가하세요.", 297, 269, 730, 70, 22, mutedInk);
            Button(body, "app:mail", "메일 열기", 297, 375, 260, 52, true);

            var conversation = Panel(root, "Conversation");
            ui.ChatConversation = conversation.gameObject;
            body = conversation;
            Button(body, "noop", "●  민서", 16, 92, 185, 51, true);
            Text(body, "minseo_404\n온라인", 30, 163, 175, 80, 17, mutedInk);
            Text(body, "민서", 254, 24, 500, 37, 27, ink, true);
            Text(body, "암호화된 대화 · 의뢰 001", 254, 69, 700, 29, 16, mutedInk);
            ui.ChatStages = new GameObject[5];
            for (chatStep = 0; chatStep < 4; chatStep++)
            {
                body = Panel(conversation, "Step" + chatStep);
                ui.ChatStages[chatStep] = body.gameObject;
                BuildChatStep();
            }
            body = Panel(conversation, "Completed");
            ui.ChatStages[4] = body.gameObject;
            Bubble("민서", "눈동자 아이콘이 사라졌어요.\n화면 녹화도 멈췄어요. 정말 감사합니다!", 133, false);
            Bubble("나", "이제 괜찮습니다. 의뢰를 마무리할게요.", 289, true);
            Button(body, "report", "의뢰 완료 기록 보기", 255, 546, 817, 54, true);
        }

        void BuildChatStep()
        {
            string[] incoming = { "연락 주셔서 감사합니다.\n정말 귀신 때문일까요?", "괴담 영상을 보다가\n설명에 있던 링크를 눌렀어요.", "제목이 ‘관음증 귀신’이었어요.\n다시 찾아보니 영상은 삭제됐더라고요.", "네, 기다리고 있을게요.\n조심하세요." };
            string[] outgoing = { "", "그 사이트에는 어떻게 들어가셨나요?", "영상 제목이나 내용이 기억나세요?", "같은 일을 겪은 사람이 있는지 찾아볼게요." };
            if (chatStep > 0) Bubble("나", outgoing[chatStep], 123, true);
            Bubble("민서", incoming[chatStep], chatStep > 0 ? 271 : 142, false);
            if (chatStep >= 2) Text(body, "키워드 기록됨  /  관음증 + 귀신", 267, 463, 789, 35, 18, blue, true);
            string[] choices = { "그 사이트에는 어떻게 들어가셨나요?", "영상 제목이나 내용이 기억나세요?", "같은 일을 겪은 사람이 있는지 찾아볼게요.", "인터넷에서 단서 검색하기  →" };
            Button(body, "chat-next", choices[chatStep], 255, 546, 817, 54, true);
        }


        void Bubble(string speaker, string line, float y, bool mine)
        {
            var box = Box(body, "Message", mine ? 401 : 255, y, 667, 124, mine ? C("E1E8FF") : pale);
            Text(box, speaker, 19, 13, 624, 24, 15, mutedInk, true);
            Text(box, line, 19, 43, 624, 75, 21);
        }


        void BuildBrowser()
        {
            var root = body;
            Box(body, "BrowserToolbar", 0, 0, 1120, 57, pale);
            ui.BackButton = Button(body, "back", "‹", 13, 9, 45, 39, false, false);
            Button(body, "home", "홈", 67, 9, 60, 39);
            Box(body, "Address", 146, 10, 950, 37, Color.white);
            ui.Address = States(Text(body, "", 161, 16, 925, 29, 16, mutedInk),
                "search.after.net", "archive.after.net / urban / 00404",
                "search.after.net / information", "unknown / watching-you");
            var search = Panel(root, "Search");
            var article = Panel(root, "Article");
            var side = Panel(root, "Information");
            var haunted = Panel(root, "HauntedSite");
            ui.BrowserPages = new[] { search.gameObject, article.gameObject, side.gameObject, haunted.gameObject };
            body = search; Search();
            body = article; Article();
            body = side; Side();
            body = haunted; BuildHaunted();
        }

        void Search()
        {
            Text(body, "S E E K", 45, 83, 630, 51, 35, blue, true);
            Text(body, "흔적은 어딘가에 남아 있습니다.", 45, 139, 810, 31, 18, mutedInk);
            Box(body, "SearchBar", 43, 192, 811, 59, pale);
            var searchLabel = Text(body, (termA ? "관음증  " : "") + (termB ? "귀신" : "") + (!termA && !termB ? "아래에서 획득한 키워드를 선택하세요" : ""), 62, 208, 762, 36, 23, termA || termB ? ink : mutedInk);
            Button(body, "search", "검색", 868, 192, 206, 59, true);
            ui.SearchText = States(searchLabel, "아래에서 획득한 키워드를 선택하세요", "관음증  ", "귀신", "관음증  귀신");
            var searchRoot = body;
            body = Panel(searchRoot, "AcquiredKeywords"); ui.SearchKeywords = body.gameObject;
            {
                Button(body, "term-a", (termA ? "− " : "+ ") + "관음증", 44, 266, 153, 40);
                Button(body, "term-b", (termB ? "− " : "+ ") + "귀신", 208, 266, 132, 40);
                Text(body, "키워드를 클릭하면 검색창에 추가 / 제거됩니다.", 367, 274, 707, 31, 16, mutedInk);
            }
            body = Panel(searchRoot, "NoKeywords"); ui.SearchNoKeywords = body.gameObject;
            Button(body, "app:chat", "의뢰인과 대화해 키워드 얻기", 44, 266, 405, 45);
            body = Panel(searchRoot, "SearchIntro"); ui.SearchIntro = body.gameObject;
            {
                Text(body, "검색의 시작은 의뢰인의 한마디.", 49, 409, 1000, 47, 27, ink, true);
                Text(body, "두 키워드를 조합해 같은 일을 겪은 사람의 기록을 찾아보세요.", 49, 475, 1000, 65, 21, mutedInk);
            }
            body = Panel(searchRoot, "SearchResults"); ui.SearchResults = body.gameObject;
            SearchResult("side", "01 / 용어 사전", "관음증이란? 용어와 의미", "일반적인 용어를 설명하는 정보 게시물", 332, false);
            SearchResult("side", "02 / 심리 이야기", "관음증에 관한 흔한 오해", "일상 표현과 전문 용어의 차이", 426, false);
            SearchResult("article", "03 / 괴담 게시판", "관음증 귀신을 아시나요?", "화면 녹화가 혼자 켜지고, 바탕화면에 눈동자가 생긴다면…", 520, true);
        }


        void SearchResult(string target, string category, string title, string preview, float y, bool clue)
        {
            var b = Button(body, "nav:" + target, "", 43, y, 1034, 86);
            b.targetGraphic.color = clue ? C("E2E9FE") : C("F1F3F9");
            Text(b.transform, category, 17, 8, 310, 23, 13, mutedInk);
            Text(b.transform, title, 17, 30, 951, 31, 23, clue ? blue : ink, true);
            Text(b.transform, preview, 17, 61, 980, 23, 14, mutedInk);
        }


        void Article()
        {
            Text(body, "기록 보관소  /  익명 · 괴담 기록", 43, 81, 1000, 29, 16, mutedInk);
            Text(body, "관음증 귀신을 아시나요?", 43, 123, 1015, 54, 34, ink, true);
            Text(body, "그 사이트에 접속한 뒤부터 이상한 일이 시작됐습니다.", 43, 196, 1020, 45, 21);
            var clueButton = Button(body, "clue", "", 43, 253, 1034, 97);
            clueButton.targetGraphic.color = ClueFound ? C("E2F0E9") : C("E7EDFF");
            Text(clueButton.transform, "화면 녹화가 저절로 켜지고, 바탕화면에 눈동자 아이콘이 생깁니다.", 19, 16, 992, 38, 22, ClueFound ? green : blue, true);
            var clueStatus = Text(clueButton.transform, ClueFound ? "기록 완료  /  의뢰인의 증상과 일치" : "단서 조사  /  이 문장을 클릭해 의뢰인의 증상과 대조", 19, 59, 993, 29, 16, mutedInk);
            Text(body, "기기를 꺼도 누가 지켜보는 기분이 사라지지 않아요.\n같은 일을 겪은 분이 있다면 이 기록을 남겨 주세요.", 43, 382, 1034, 80, 22);
            Text(body, "주의  /  컴퓨터 버려도 되는 분만 여세요.", 43, 496, 1034, 33, 18, C("956039"));
            Button(body, "enter-site", "문제의 사이트로 이동  →", 43, 546, 480, 53, true, ClueFound);
            var siteHint = Text(body, ClueFound ? "주소 확보 · 격리 접속 가능" : "먼저 강조된 문장에서 단서를 확인하세요.", 548, 560, 540, 37, 16, mutedInk);
            ui.ArticleClue = States(clueStatus, "단서 조사  /  이 문장을 클릭해 의뢰인의 증상과 대조", "기록 완료  /  의뢰인의 증상과 일치");
            ui.ArticleHint = States(siteHint, "먼저 강조된 문장에서 단서를 확인하세요.", "주소 확보 · 격리 접속 가능");
        }


        void Side()
        {
            Text(body, "일반 정보 게시물", 45, 101, 1000, 30, 17, mutedInk);
            Text(body, "누군가를 지켜본다는 것", 45, 165, 1000, 57, 34, ink, true);
            Text(body, "이 글은 일반적인 용어와 표현을 다룹니다.\n\n화면 녹화나 눈동자 아이콘에 관한 이야기는 없습니다.\n의뢰인이 겪는 현상과는 연결되지 않는 것 같습니다.", 45, 256, 1010, 180, 23);
            Button(body, "back", "다른 검색 결과 살펴보기  ←", 45, 518, 452, 54, true);
        }


        void BuildHaunted()
        {
            var root = body;
            var isolate = Panel(root, "Isolation");
            var cameras = Panel(root, "Cameras");
            var ghost = Panel(root, "GhostConversation");
            var ending = Panel(root, "Ending");
            ui.HauntedPhases = new[] { isolate.gameObject, cameras.gameObject, ghost.gameObject, ending.gameObject };
            body = isolate;
            Text(body, "UNKNOWN SIGNAL", 47, 121, 1000, 40, 21, blue, true);
            Text(body, "누군가, 이쪽을 보고 있다.", 47, 204, 1020, 67, 39, ink, true);
            Text(body, "사이트 곳곳에 촬영도구가 놓여 있다.\n바깥과의 연결을 차단한 뒤 내부를 조사하자.", 47, 318, 1000, 127, 24, mutedInk);
            Button(body, "isolate", "외부 연결을 차단하고 조사하기", 47, 518, 562, 59, true);
            body = cameras;
            Text(body, "격리 접속 중  /  촬영도구 뒤에서 기척이 느껴집니다.", 28, 78, 878, 31, 17, mutedInk);
            ui.CameraCount = States(Text(body, "5 / 5", 970, 75, 120, 38, 23, blue, true),
                "5 / 5", "4 / 5", "3 / 5", "2 / 5", "1 / 5", "0 / 5");
            Box(body, "CameraField", 22, 121, 1076, 421, Color.white);
            var glimpse = Text(body, "···", 513, 292, 120, 85, 55, C("C1C9DA"));
            glimpse.alignment = TextAlignmentOptions.Center; ui.Glimpse = glimpse.gameObject;
            Vector2[] positions = { new Vector2(68, 140), new Vector2(392, 130), new Vector2(742, 140), new Vector2(238, 345), new Vector2(617, 345) };
            ui.Cameras = new GameObject[5];
            for (int i = 0; i < 5; i++)
            {
                var b = Button(body, "camera:" + i, "", positions[i].x, positions[i].y, 249, 184);
                ui.Cameras[i] = b.gameObject;
                b.targetGraphic.color = Color.white;
                Raw(b.transform, CameraArtwork, 7, 3, 235, 158);
                Text(b.transform, "촬영도구 " + (i + 1) + "  /  치우기", 13, 157, 228, 25, 14, mutedInk);
            }
            Box(body, "Hint", 22, 560, 1076, 57, pale);
            Text(body, "촬영도구를 클릭해 시야를 확보하세요. 뒤에 무엇이 숨어 있을까요?", 39, 574, 1041, 36, 19);
            body = ghost; BuildGhost();
            body = ending; Ending();
        }

        void BuildGhost()
        {
            Text(body, "SIGNAL FOUND  /  관음증 귀신", 31, 78, 1000, 32, 17, blue, true);
            var artArea = Box(body, "GhostArea", 24, 118, 1072, 291, Color.white);
            ghostArt = Rect(artArea, "Ghost", 397, 16, 277, 250); ghostBase = ghostArt.anchoredPosition;
            ghostGroup = ghostArt.gameObject.AddComponent<CanvasGroup>();
            var raw = Raw(ghostArt, GhostReference, 0, 0, 277, 250);
            raw.uvRect = new Rect(733f / 1657f, 1f - 590f / 949f, 289f / 1657f, 260f / 949f);
            Raw(artArea, CameraArtwork, 44, 170, 140, 110);
            string[] lines = { "카메라를 치우자 숨어 있던 귀신이 모습을 드러냈다.\n대화를 시도해 보자.", "…나 보이는 거야?\n카메라 뒤에 숨어 있었는데.", "예전 일은 기억 안 나. 그냥 심심했어.\n남들 일상을 구경하는 게 좋았거든…", "성불하는 법도 몰랐어.\n알려주면 이제 갈게." };
            Box(body, "Dialogue", 25, 423, 1070, 112, pale);
            var speaker = Text(body, ghostStep == 0 ? "귀신 발견" : "관음증 귀신", 45, 442, 188, 43, 22, ink, true);
            var line = Text(body, lines[ghostStep], 250, 442, 823, 87, 22);
            string[] choices = { "대화 시도하기", "왜 사람들을 지켜보고 있었어?", "이제 그만하고, 떠나는 건 어때?", "성불을 도와준다" };
            var next = Button(body, "ghost-next", choices[0], 290, 561, 540, 53, true);
            ui.GhostArt = ghostArt; ui.GhostGroup = ghostGroup;
            ui.GhostSpeaker = States(speaker, "귀신 발견", "관음증 귀신");
            ui.GhostLines = States(line, lines);
            ui.GhostChoice = States(next.GetComponentInChildren<TextMeshProUGUI>(), choices[0], choices[1], choices[2], choices[3], "귀신을 배웅하는 중…");
            ui.GhostNext = next;
        }


        UnityEngine.UI.RawImage Raw(Transform parent, Texture texture, float x, float y, float w, float h)
        {
            var rt = Rect(parent, "Artwork", x, y, w, h);
            var raw = rt.gameObject.AddComponent<UnityEngine.UI.RawImage>(); raw.texture = texture; raw.raycastTarget = false; return raw;
        }


        void Ending()
        {
            Text(body, "CASE CLOSED", 308, 123, 580, 43, 23, green, true);
            Text(body, "첫 번째 의뢰 해결", 308, 193, 730, 71, 43, ink, true);
            Text(body, "작은 귀신은 조용히 빛으로 흩어졌다.\n더는 누군가의 화면을 지켜보는 시선이 느껴지지 않는다.", 115, 300, 933, 114, 24, mutedInk);
            Box(body, "Thanks", 115, 427, 889, 90, C("E4F1EB"));
            Text(body, "민서  /  눈동자 아이콘이 사라졌어요.\n화면 녹화도 멈췄어요. 정말 감사합니다!", 137, 441, 845,  h: 70, size: 21, color: green);
            Button(body, "app:chat", "민서의 답장 확인", 115, 551, 424, 55, true);
            Button(body, "restart", "처음부터 다시 하기", 567, 551, 437, 55);
        }


        void ConfirmRestart()
        {
            Box(modal, "Shade", 0, 0, 1600, 900, new Color(.1f, .15f, .25f, .65f), true);
            var dialog = Box(modal, "Confirm", 490, 305, 620, 272, paper, true);
            Text(dialog, "처음부터 다시 시작할까요?", 30, 29, 560, 49, 28, ink, true);
            Text(dialog, "이번 의뢰에서 수집한 단서와 대화 진행이 초기화됩니다.", 30, 97, 560, 64, 21, mutedInk);
            Button(dialog, "cancel-reset", "계속 조사하기", 30, 193, 264, 51);
            Button(dialog, "confirm-reset", "처음부터", 311, 193, 279, 51, true);
        }


    }
}
