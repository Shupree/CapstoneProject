using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CyberExorcist
{
    // Gameplay state only. All visual objects and button events are saved in the scene.
    public sealed class CyberExorcistPrototype : MonoBehaviour
    {
        [Header("Scene UI")]
        public PrototypeSceneUI UI;

        // Retained for the initial editor setup and existing serialized resource references.
        [HideInInspector] public TMP_FontAsset KoreanFont;
        [HideInInspector] public Texture2D CameraArtwork;
        [HideInInspector] public Texture2D GhostReference;

        [Header("Ghost animation")]
        [Min(0.1f)] public float DepartureDuration = 1.8f;
        public float DepartureRise = 75.6f;
        public float FloatAmplitude = 6f;
        public float FloatSpeed = 1.8f;

        public bool ContactAdded { get; private set; }
        public bool KeywordAcquired => chatStep >= 2;
        public bool ClueFound { get; private set; }
        public bool Completed { get; private set; }
        public int CamerasRemoved => removed.Count;
        public string CurrentApp => app;
        public string BrowserPage => page;
        public int GhostDialogue => ghostStep;
        public bool Departing => departing;
        public int ChatStep => chatStep;
        public bool TermASelected => termA;
        public bool TermBSelected => termB;
        public bool Isolated => isolated;
        public bool Muted => muted;
        public bool CanGoBack => history.Count > 0;
        public bool IsCameraRemoved(int index) => removed.Contains(index);

        int chatStep, ghostStep;
        bool termA, termB, isolated, departing, muted;
        string app = "mail", page = "home";
        readonly HashSet<int> removed = new HashSet<int>();
        readonly Stack<string> history = new Stack<string>();
        Vector2 ghostBase, initialWindowPosition;
        float toastUntil;
        AudioClip clickTone, clueTone;

        void Start()
        {
            if (!UI)
            {
                Debug.LogError("Scene UI is missing. Open the authored CyberExorcistPrototype scene.", this);
                enabled = false;
                return;
            }
            ghostBase = UI.GhostArt.anchoredPosition;
            initialWindowPosition = UI.Window.anchoredPosition;
            clickTone = Tone(650, .045f);
            clueTone = Tone(1040, .18f);
            UI.GhostGroup.alpha = 1;
            UI.RestartDialog.SetActive(false);
            ShowApp("mail");
            Toast("새로운 의뢰가 도착했습니다. 메일을 확인하세요.");
        }

        void Update()
        {
            if (!UI) return;
            UI.Clock.text = "LOCAL  /  " + DateTime.Now.ToString("HH:mm");
            UI.Notification.SetActive(Time.unscaledTime < toastUntil);
            if (!departing)
                UI.GhostArt.anchoredPosition = ghostBase + Vector2.up * Mathf.Sin(Time.unscaledTime * FloatSpeed) * FloatAmplitude;
        }

        // Persistently wired to Buttons in the scene, so the events remain inspectable.
        public void Click(string id)
        {
            if (!Application.isPlaying || !enabled) return;
            PlayTone(false);
            Handle(id);
        }

        void Render() { UI.Refresh(this); }
        void ShowApp(string name) { app = name; UI.Window.gameObject.SetActive(true); Render(); }

        void Navigate(string next, bool remember = true)
        {
            if (remember && page != next) history.Push(page);
            page = next;
            ShowApp("browser");
        }

        public void Handle(string id)
        {
            if (departing) return;
            if (id.StartsWith("app:")) { var next = id.Substring(4); if (next == "mail" || next == "chat" || next == "browser") ShowApp(next); return; }
            if (id.StartsWith("nav:")) { var next = id.Substring(4); if (KeywordAcquired && termA && termB && (next == "article" || next == "side")) Navigate(next); return; }
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
                case "report": if (Completed) Navigate("haunted"); break;
                case "minimize": case "desktop": UI.Window.gameObject.SetActive(false); break;
                case "restart": UI.RestartDialog.SetActive(true); break;
                case "cancel-reset": UI.RestartDialog.SetActive(false); break;
                case "confirm-reset": ResetCase(); break;
                case "mute":
                    muted = !muted;
                    Render();
                    Toast(muted ? "효과음을 껐습니다." : "효과음을 켰습니다."); break;
            }
        }


        IEnumerator Departure()
        {
            departing = true;
            Render();
            PlayTone(true);
            float duration = Mathf.Max(.1f, DepartureDuration);
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                UI.GhostGroup.alpha = 1 - t / duration;
                UI.GhostArt.anchoredPosition = ghostBase + Vector2.up * (t / duration * DepartureRise);
                yield return null;
            }
            Completed = true;
            departing = false;
            Render();
            Toast("의뢰 001 해결  /  민서에게 새 메시지가 도착했습니다.");
        }

        void ResetCase()
        {
            StopAllCoroutines();
            chatStep = ghostStep = 0;
            ContactAdded = ClueFound = Completed = isolated = departing = termA = termB = false;
            removed.Clear();
            history.Clear();
            page = "home";
            UI.RestartDialog.SetActive(false);
            UI.GhostGroup.alpha = 1;
            UI.GhostArt.anchoredPosition = ghostBase;
            UI.Window.anchoredPosition = initialWindowPosition;
            ShowApp("mail");
            Toast("새로운 의뢰가 도착했습니다.");
        }

        void Toast(string message)
        {
            UI.NotificationText.text = message;
            toastUntil = Time.unscaledTime + 4.1f;
            UI.Notification.SetActive(true);
        }

        void PlayTone(bool clue)
        {
            if (!muted && UI.Sound) UI.Sound.PlayOneShot(clue ? clueTone : clickTone);
        }

        AudioClip Tone(float frequency, float length)
        {
            const int rate = 22050;
            var samples = new float[(int)(rate * length)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / samples.Length;
                samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / rate) * Mathf.Sin(Mathf.PI * t) * (1 - t) * .35f;
            }
            var clip = AudioClip.Create("DesktopTone", samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        void OnDestroy()
        {
            if (clickTone) Destroy(clickTone);
            if (clueTone) Destroy(clueTone);
        }
    }
}
