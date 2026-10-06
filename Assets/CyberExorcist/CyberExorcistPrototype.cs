using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace CyberExorcist
{
    // One scene, one short case. All websites and messages are fictional, local UI.
    public sealed class CyberExorcistPrototype : MonoBehaviour
    {
        public TMP_FontAsset KoreanFont;
        public Texture2D CameraArtwork;
        public Texture2D GhostReference;

        public bool ContactAdded { get; private set; }
        public bool KeywordAcquired => chatStep >= 2;
        public bool ClueFound { get; private set; }
        public bool Completed { get; private set; }
        public int CamerasRemoved => removed.Count;
        public string CurrentApp => app;
        public string BrowserPage => page;
        public int GhostDialogue => ghostStep;
        public bool Departing => departing;

        int chatStep, ghostStep;
        bool termA, termB, isolated, departing, muted;
        string app = "mail", page = "home", toastMessage;
        readonly HashSet<int> removed = new HashSet<int>();
        readonly Stack<string> history = new Stack<string>();
        RectTransform desktop, window, body, record, modal;
        TextMeshProUGUI goalLabel, clockLabel, toastLabel;
        CanvasGroup ghostGroup;
        RectTransform ghostArt;
        Vector2 ghostBase;
        float toastUntil;
        AudioSource sound;
        AudioClip clickTone, clueTone;
        readonly Color ink = C("252C46"), mutedInk = C("65718A"), blue = C("3C5BE8"), paper = C("FAFBFF"), pale = C("EDF1FB"), green = C("287663");

        void Start()
        {
            BuildDesktop();
            ShowApp("mail");
            Toast("새로운 의뢰가 도착했습니다. 메일을 확인하세요.");
        }

        void Update()
        {
            if (clockLabel) clockLabel.text = "LOCAL  /  " + DateTime.Now.ToString("HH:mm");
            if (toastLabel) toastLabel.transform.parent.gameObject.SetActive(Time.unscaledTime < toastUntil);
            if (ghostArt && !departing) ghostArt.anchoredPosition = ghostBase + Vector2.up * Mathf.Sin(Time.unscaledTime * 1.8f) * 6f;
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
            var rt = Rect(parent, "Text", x, y, w, h);
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
            button.onClick.AddListener(() => { PlayTone(false); Handle(id); });
            return button;
        }

        void Clear(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--) { var child = parent.GetChild(i).gameObject; child.SetActive(false); Destroy(child); }
        }

        void BuildDesktop()
        {
            sound = gameObject.AddComponent<AudioSource>(); sound.playOnAwake = false; sound.volume = .13f;
            clickTone = Tone(650, .045f); clueTone = Tone(1040, .18f);
            var canvasObject = new GameObject("ExorcistDesktop", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
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
            if (!FindFirstObjectByType<EventSystem>())
            {
                var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false);
            }
            RefreshRecord();
        }

        void Render()
        {
            Clear(window); ghostArt = null; ghostGroup = null;
            var header = Box(window, "TitleBar", 0, 0, 1120, 51, C("DFE7F8"), true);
            header.gameObject.AddComponent<PrototypeWindowDrag>().Initialize(window);
            Text(header, app == "mail" ? "01  /  받은 메일" : app == "chat" ? "02  /  메신저" : "03  /  인터넷", 23, 12, 890, 33, 20, ink, true);
            Button(header, "minimize", "—", 1010, 7, 45, 36);
            Button(header, "minimize", "×", 1064, 7, 45, 36);
            body = Rect(window, "Content", 0, 51, 1120, 641);
            if (app == "mail") Mail(); else if (app == "chat") Chat(); else Browser();
            RefreshRecord();
        }

        void ShowApp(string name) { app = name; window.gameObject.SetActive(true); Render(); }

        void RefreshRecord()
        {
            if (!record) return;
            Clear(record);
            Text(record, "CASE FILE / 001", 22, 25, 250, 27, 16, blue, true);
            Text(record, "지켜보는 눈", 22, 69, 255, 49, 29, ink, true);
            Text(record, Completed ? "종결 · 잔류 신호 없음" : "조사 중 · 의뢰인 민서", 22, 121, 250, 34, 17, Completed ? green : mutedInk);
            Box(record, "Divider", 22, 171, 250, 1, C("C9D4E9"));
            string[] steps = { "의뢰 접수", "키워드 확보", "괴담 기록 대조", "숨은 괴이 발견", "대화와 성불" };
            bool[] checks = { ContactAdded, KeywordAcquired, ClueFound, CamerasRemoved == 5, Completed };
            for (int i = 0; i < steps.Length; i++)
            {
                Text(record, checks[i] ? "●" : "○", 23, 195 + i * 49, 28, 29, 20, checks[i] ? green : mutedInk);
                Text(record, steps[i], 59, 197 + i * 49, 217, 29, 18, checks[i] ? green : mutedInk, checks[i]);
            }
            Box(record, "Divider", 22, 463, 250, 1, C("C9D4E9"));
            Text(record, "수집한 단서", 23, 485, 240, 29, 17, ink, true);
            Text(record, KeywordAcquired ? "관음증 / 귀신" : "아직 수집된 키워드가 없습니다.", 23, 527, 245, 62, 18, KeywordAcquired ? blue : mutedInk);
            Text(record, ClueFound ? "자동 녹화 + 눈동자 아이콘\n" + (CamerasRemoved == 5 ? "촬영도구 뒤에 숨은 존재" : "의뢰인과 같은 증상 확인") : "메일과 게시글의 증상을\n대조해 연결점을 찾으세요.", 23, 591, 245, 70, 16, mutedInk);
            goalLabel.text = "현재 목표  /  " + (Completed ? "의뢰 해결. 민서의 답장을 확인하세요." : !ContactAdded ? "메일의 연락처를 추가하세요." : !KeywordAcquired ? "민서와 대화해 단서를 얻으세요." : !ClueFound ? "키워드를 조합해 괴담 기록을 조사하세요." : CamerasRemoved < 5 ? "문제의 사이트에서 촬영도구를 치우세요." : "귀신의 이야기를 듣고 성불을 도와주세요.");
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

        void Chat()
        {
            Box(body, "Contacts", 0, 0, 217, 641, pale);
            Text(body, "CONTACTS", 24, 31, 180, 32, 16, mutedInk, true);
            if (!ContactAdded)
            {
                Text(body, "등록된 연락처가 없습니다", 297, 195, 750, 56, 30, ink, true);
                Text(body, "메일의 연락처를 눌러 의뢰인을 추가하세요.", 297, 269, 730, 70, 22, mutedInk);
                Button(body, "app:mail", "메일 열기", 297, 375, 260, 52, true); return;
            }
            Button(body, "noop", "●  민서", 16, 92, 185, 51, true);
            Text(body, "minseo_404\n온라인", 30, 163, 175, 80, 17, mutedInk);
            Text(body, "민서", 254, 24, 500, 37, 27, ink, true);
            Text(body, "암호화된 대화 · 의뢰 001", 254, 69, 700, 29, 16, mutedInk);
            if (Completed)
            {
                Bubble("민서", "눈동자 아이콘이 사라졌어요.\n화면 녹화도 멈췄어요. 정말 감사합니다!", 133, false);
                Bubble("나", "이제 괜찮습니다. 의뢰를 마무리할게요.", 289, true);
                Button(body, "report", "의뢰 완료 기록 보기", 255, 546, 817, 54, true); return;
            }
            string[] incoming = { "연락 주셔서 감사합니다.\n정말 귀신 때문일까요?", "괴담 영상을 보다가\n설명에 있던 링크를 눌렀어요.", "제목이 ‘관음증 귀신’이었어요.\n다시 찾아보니 영상은 삭제됐더라고요.", "네, 기다리고 있을게요.\n조심하세요." };
            string[] outgoing = { "", "그 사이트에는 어떻게 들어가셨나요?", "영상 제목이나 내용이 기억나세요?", "같은 일을 겪은 사람이 있는지 찾아볼게요." };
            if (chatStep > 0) Bubble("나", outgoing[chatStep], 123, true);
            Bubble("민서", incoming[chatStep], chatStep > 0 ? 271 : 142, false);
            if (KeywordAcquired) Text(body, "키워드 기록됨  /  관음증 + 귀신", 267, 463, 789, 35, 18, blue, true);
            string[] choices = { "그 사이트에는 어떻게 들어가셨나요?", "영상 제목이나 내용이 기억나세요?", "같은 일을 겪은 사람이 있는지 찾아볼게요.", "인터넷에서 단서 검색하기  →" };
            Button(body, "chat-next", choices[chatStep], 255, 546, 817, 54, true);
        }

        void Bubble(string speaker, string line, float y, bool mine)
        {
            var box = Box(body, "Message", mine ? 401 : 255, y, 667, 124, mine ? C("E1E8FF") : pale);
            Text(box, speaker, 19, 13, 624, 24, 15, mutedInk, true);
            Text(box, line, 19, 43, 624, 75, 21);
        }

        void Browser()
        {
            Box(body, "BrowserToolbar", 0, 0, 1120, 57, pale);
            Button(body, "back", "‹", 13, 9, 45, 39, false, history.Count > 0);
            Button(body, "home", "홈", 67, 9, 60, 39);
            var address = page == "home" || page == "results" ? "search.after.net" : page == "article" ? "archive.after.net / urban / 00404" : page == "haunted" ? "unknown / watching-you" : "search.after.net / information";
            Box(body, "Address", 146, 10, 950, 37, Color.white);
            Text(body, address, 161, 16, 925, 29, 16, mutedInk);
            if (page == "article") Article(); else if (page == "side") Side(); else if (page == "haunted") Haunted(); else Search();
        }

        void Search()
        {
            Text(body, "S E E K", 45, 83, 630, 51, 35, blue, true);
            Text(body, "흔적은 어딘가에 남아 있습니다.", 45, 139, 810, 31, 18, mutedInk);
            Box(body, "SearchBar", 43, 192, 811, 59, pale);
            Text(body, (termA ? "관음증  " : "") + (termB ? "귀신" : "") + (!termA && !termB ? "아래에서 획득한 키워드를 선택하세요" : ""), 62, 208, 762, 36, 23, termA || termB ? ink : mutedInk);
            Button(body, "search", "검색", 868, 192, 206, 59, true);
            if (KeywordAcquired)
            {
                Button(body, "term-a", (termA ? "− " : "+ ") + "관음증", 44, 266, 153, 40);
                Button(body, "term-b", (termB ? "− " : "+ ") + "귀신", 208, 266, 132, 40);
                Text(body, "키워드를 클릭하면 검색창에 추가 / 제거됩니다.", 367, 274, 707, 31, 16, mutedInk);
            }
            else Button(body, "app:chat", "의뢰인과 대화해 키워드 얻기", 44, 266, 405, 45);
            if (page != "results")
            {
                Text(body, "검색의 시작은 의뢰인의 한마디.", 49, 409, 1000, 47, 27, ink, true);
                Text(body, "두 키워드를 조합해 같은 일을 겪은 사람의 기록을 찾아보세요.", 49, 475, 1000, 65, 21, mutedInk); return;
            }
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
            Text(clueButton.transform, ClueFound ? "기록 완료  /  의뢰인의 증상과 일치" : "단서 조사  /  이 문장을 클릭해 의뢰인의 증상과 대조", 19, 59, 993, 29, 16, mutedInk);
            Text(body, "기기를 꺼도 누가 지켜보는 기분이 사라지지 않아요.\n같은 일을 겪은 분이 있다면 이 기록을 남겨 주세요.", 43, 382, 1034, 80, 22);
            Text(body, "주의  /  컴퓨터 버려도 되는 분만 여세요.", 43, 496, 1034, 33, 18, C("956039"));
            Button(body, "enter-site", "문제의 사이트로 이동  →", 43, 546, 480, 53, true, ClueFound);
            Text(body, ClueFound ? "주소 확보 · 격리 접속 가능" : "먼저 강조된 문장에서 단서를 확인하세요.", 548, 560, 540, 37, 16, mutedInk);
        }

        void Side()
        {
            Text(body, "일반 정보 게시물", 45, 101, 1000, 30, 17, mutedInk);
            Text(body, "누군가를 지켜본다는 것", 45, 165, 1000, 57, 34, ink, true);
            Text(body, "이 글은 일반적인 용어와 표현을 다룹니다.\n\n화면 녹화나 눈동자 아이콘에 관한 이야기는 없습니다.\n의뢰인이 겪는 현상과는 연결되지 않는 것 같습니다.", 45, 256, 1010, 180, 23);
            Button(body, "back", "다른 검색 결과 살펴보기  ←", 45, 518, 452, 54, true);
        }

        void Haunted()
        {
            if (Completed) { Ending(); return; }
            if (!isolated)
            {
                Text(body, "UNKNOWN SIGNAL", 47, 121, 1000, 40, 21, blue, true);
                Text(body, "누군가, 이쪽을 보고 있다.", 47, 204, 1020, 67, 39, ink, true);
                Text(body, "사이트 곳곳에 촬영도구가 놓여 있다.\n바깥과의 연결을 차단한 뒤 내부를 조사하자.", 47, 318, 1000, 127, 24, mutedInk);
                Button(body, "isolate", "외부 연결을 차단하고 조사하기", 47, 518, 562, 59, true); return;
            }
            if (CamerasRemoved < 5)
            {
                Text(body, "격리 접속 중  /  촬영도구 뒤에서 기척이 느껴집니다.", 28, 78, 878, 31, 17, mutedInk);
                Text(body, (5 - CamerasRemoved) + " / 5", 970, 75, 120, 38, 23, blue, true);
                Box(body, "CameraField", 22, 121, 1076, 421, Color.white);
                if (CamerasRemoved >= 2)
                {
                    var glimpse = Text(body, "···", 513, 292, 120, 85, 55, C("C1C9DA")); glimpse.alignment = TextAlignmentOptions.Center;
                }
                Vector2[] positions = { new Vector2(68, 140), new Vector2(392, 130), new Vector2(742, 140), new Vector2(238, 345), new Vector2(617, 345) };
                for (int i = 0; i < 5; i++)
                {
                    if (removed.Contains(i)) continue;
                    var b = Button(body, "camera:" + i, "", positions[i].x, positions[i].y, 249, 184);
                    b.targetGraphic.color = Color.white;
                    Raw(b.transform, CameraArtwork, 7, 3, 235, 158);
                    Text(b.transform, "촬영도구 " + (i + 1) + "  /  치우기", 13, 157, 228, 25, 14, mutedInk);
                }
                Box(body, "Hint", 22, 560, 1076, 57, pale);
                Text(body, "촬영도구를 클릭해 시야를 확보하세요. 뒤에 무엇이 숨어 있을까요?", 39, 574, 1041, 36, 19);
                return;
            }
            Text(body, "SIGNAL FOUND  /  관음증 귀신", 31, 78, 1000, 32, 17, blue, true);
            var artArea = Box(body, "GhostArea", 24, 118, 1072, 291, Color.white);
            ghostArt = Rect(artArea, "Ghost", 397, 16, 277, 250); ghostBase = ghostArt.anchoredPosition;
            ghostGroup = ghostArt.gameObject.AddComponent<CanvasGroup>();
            var raw = Raw(ghostArt, GhostReference, 0, 0, 277, 250);
            raw.uvRect = new Rect(733f / 1657f, 1f - 590f / 949f, 289f / 1657f, 260f / 949f);
            Raw(artArea, CameraArtwork, 44, 170, 140, 110);
            string[] lines = { "카메라를 치우자 숨어 있던 귀신이 모습을 드러냈다.\n대화를 시도해 보자.", "…나 보이는 거야?\n카메라 뒤에 숨어 있었는데.", "예전 일은 기억 안 나. 그냥 심심했어.\n남들 일상을 구경하는 게 좋았거든…", "성불하는 법도 몰랐어.\n알려주면 이제 갈게." };
            Box(body, "Dialogue", 25, 423, 1070, 112, pale);
            Text(body, ghostStep == 0 ? "귀신 발견" : "관음증 귀신", 45, 442, 188, 43, 22, ink, true);
            Text(body, lines[ghostStep], 250, 442, 823, 87, 22);
            string[] choices = { "대화 시도하기", "왜 사람들을 지켜보고 있었어?", "이제 그만하고, 떠나는 건 어때?", "성불을 도와준다" };
            Button(body, "ghost-next", departing ? "귀신을 배웅하는 중…" : choices[ghostStep], 290, 561, 540, 53, true, !departing);
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

        void Navigate(string next, bool remember = true)
        {
            if (remember && page != next) history.Push(page);
            page = next; ShowApp("browser");
        }

        public void Handle(string id)
        {
            if (departing) return;
            if (id.StartsWith("app:")) { ShowApp(id.Substring(4)); return; }
            if (id.StartsWith("nav:")) { if (KeywordAcquired && termA && termB) Navigate(id.Substring(4)); return; }
            if (id.StartsWith("camera:"))
            {
                if (!ClueFound || !isolated || page != "haunted" || !int.TryParse(id.Substring(7), out int n) || n < 0 || n >= 5) return;
                if (removed.Add(n)) { Render(); Toast(CamerasRemoved == 5 ? "카메라 뒤에서 작은 귀신을 발견했습니다." : "촬영도구를 치웠습니다. 남은 도구 " + (5 - CamerasRemoved) + "개"); }
                return;
            }
            switch (id)
            {
                case "contact": ContactAdded = true; ShowApp("chat"); break;
                case "chat-next":
                    if (!ContactAdded) break;
                    if (chatStep < 3) { chatStep++; Render(); if (chatStep == 2) { Toast("새로운 키워드 획득  /  관음증 + 귀신"); PlayTone(true); } }
                    else Navigate("home"); break;
                case "term-a": if (KeywordAcquired) { termA = !termA; page = "home"; Render(); } break;
                case "term-b": if (KeywordAcquired) { termB = !termB; page = "home"; Render(); } break;
                case "search": if (KeywordAcquired && termA && termB) Navigate("results"); else Toast("획득한 ‘관음증’과 ‘귀신’ 키워드를 모두 선택하세요."); break;
                case "clue": if (page == "article") { ClueFound = true; Render(); Toast("단서 일치  /  의뢰인의 증상과 같은 기록입니다."); PlayTone(true); } break;
                case "enter-site": if (ClueFound) Navigate("haunted"); break;
                case "isolate": if (ClueFound && page == "haunted") { isolated = true; Render(); Toast("바깥과 연결은 차단했으니 안전할 거야."); } break;
                case "ghost-next":
                    if (CamerasRemoved != 5 || page != "haunted" || Completed) break;
                    if (ghostStep < 3) { ghostStep++; Render(); } else StartCoroutine(Departure()); break;
                case "back": if (history.Count > 0) Navigate(history.Pop(), false); break;
                case "home": Navigate("home"); break;
                case "report": Navigate("haunted"); break;
                case "minimize": case "desktop": window.gameObject.SetActive(false); break;
                case "restart": ConfirmRestart(); break;
                case "cancel-reset": Clear(modal); break;
                case "confirm-reset": ResetCase(); break;
                case "mute":
                    muted = !muted;
                    var label = desktop.Find("mute").GetComponentInChildren<TextMeshProUGUI>(); label.text = muted ? "소리 꺼짐" : "소리 켜짐";
                    Toast(muted ? "효과음을 껐습니다." : "효과음을 켰습니다."); break;
            }
        }

        IEnumerator Departure()
        {
            departing = true; Render(); PlayTone(true);
            for (float t = 0; t < 1.8f; t += Time.unscaledDeltaTime)
            {
                if (ghostGroup) ghostGroup.alpha = 1 - t / 1.8f;
                if (ghostArt) ghostArt.anchoredPosition = ghostBase + Vector2.up * (t * 42);
                yield return null;
            }
            Completed = true; departing = false; Render(); Toast("의뢰 001 해결  /  민서에게 새 메시지가 도착했습니다.");
        }

        void ConfirmRestart()
        {
            Clear(modal);
            Box(modal, "Shade", 0, 0, 1600, 900, new Color(.1f, .15f, .25f, .65f), true);
            var dialog = Box(modal, "Confirm", 490, 305, 620, 272, paper, true);
            Text(dialog, "처음부터 다시 시작할까요?", 30, 29, 560, 49, 28, ink, true);
            Text(dialog, "이번 의뢰에서 수집한 단서와 대화 진행이 초기화됩니다.", 30, 97, 560, 64, 21, mutedInk);
            Button(dialog, "cancel-reset", "계속 조사하기", 30, 193, 264, 51);
            Button(dialog, "confirm-reset", "처음부터", 311, 193, 279, 51, true);
        }

        void ResetCase()
        {
            StopAllCoroutines(); chatStep = ghostStep = 0;
            ContactAdded = ClueFound = Completed = isolated = departing = termA = termB = false;
            removed.Clear(); history.Clear(); page = "home"; Clear(modal);
            window.anchoredPosition = new Vector2(130, -103); ShowApp("mail"); Toast("새로운 의뢰가 도착했습니다.");
        }

        void Toast(string message) { toastMessage = message; if (toastLabel) toastLabel.text = toastMessage; toastUntil = Time.unscaledTime + 4.1f; }
        void PlayTone(bool clue) { if (!muted && sound) sound.PlayOneShot(clue ? clueTone : clickTone); }
        AudioClip Tone(float frequency, float length)
        {
            const int rate = 22050; var samples = new float[(int)(rate * length)];
            for (int i = 0; i < samples.Length; i++) { float t = (float)i / samples.Length; samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / rate) * Mathf.Sin(Mathf.PI * t) * (1 - t) * .35f; }
            var clip = AudioClip.Create("DesktopTone", samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
        void OnDestroy() { if (clickTone) Destroy(clickTone); if (clueTone) Destroy(clueTone); }
    }
}
