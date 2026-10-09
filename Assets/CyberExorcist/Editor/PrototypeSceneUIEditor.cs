using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CyberExorcist.Editor
{
    [CustomEditor(typeof(PrototypeSceneUI))]
    public sealed class PrototypeSceneUIEditor : UnityEditor.Editor
    {
        public enum Screen
        {
            Mail, NoContact, Chat0, Chat1, Chat2, Chat3, ChatCompleted,
            SearchEmpty, SearchReady, Results, ArticleLocked, ArticleFound,
            Information, Isolation, Cameras, Ghost0, Ghost1, Ghost2, Ghost3, Ending, Restart
        }

        static readonly List<string> ScreenNames = new List<string>
        {
            "메일", "대화 / 연락처 없음", "대화 / 첫 연락", "대화 / 접속 경위", "대화 / 키워드 획득",
            "대화 / 검색 안내", "대화 / 감사 메시지", "검색 / 키워드 없음", "검색 / 키워드 선택",
            "검색 / 결과", "괴담 / 단서 미확인", "괴담 / 단서 확인", "일반 정보 게시물",
            "사이트 / 격리 안내", "사이트 / 카메라 조사", "귀신 / 발견", "귀신 / 대화 1",
            "귀신 / 대화 2", "귀신 / 대화 3", "의뢰 완료", "재시작 확인창"
        };

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new HelpBox("Play를 끈 상태에서 화면을 골라 편집하세요. 위치·크기·색과 일반 문구는 자식 UI 오브젝트에서, 상태별 문구는 아래 Values에서 수정합니다. Play는 첫 메일부터 시작합니다.", HelpBoxMessageType.Info));
            var preview = new VisualElement();
            var dropdown = new PopupField<string>("편집할 화면", ScreenNames, 0);
            preview.Add(dropdown);
            preview.Add(new UnityEngine.UIElements.Button(() => ApplyPreview((PrototypeSceneUI)target, (Screen)dropdown.index)) { text = "화면 미리보기 적용" });
            preview.Add(new UnityEngine.UIElements.Button(() =>
            {
                var ui = (PrototypeSceneUI)target;
                var selected = VisiblePanel(ui);
                Selection.activeGameObject = selected;
                EditorGUIUtility.PingObject(selected);
            }) { text = "Hierarchy에서 현재 화면 선택" });
            preview.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
            root.Add(preview);
            var properties = new Foldout { text = "씬 참조 / 상태별 문구", value = true };
            InspectorElement.FillDefaultInspector(properties, serializedObject, this);
            root.Add(properties);
            return root;
        }

        public static void ApplyPreview(PrototypeSceneUI ui, Screen screen)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Preview is available only outside Play mode.");
            var controller = ui.GetComponent<CyberExorcistPrototype>();
            if (!controller || !ui.Window) throw new InvalidOperationException("Scene UI references are missing.");

            var changed = new List<UnityEngine.Object>();
            changed.AddRange(ui.GetComponentsInChildren<Transform>(true).Select(t => (UnityEngine.Object)t.gameObject));
            changed.AddRange(ui.GetComponentsInChildren<TextMeshProUGUI>(true));
            changed.AddRange(ui.GetComponentsInChildren<UnityEngine.UI.Button>(true));
            changed.Add(ui.GhostGroup);
            Undo.RecordObjects(changed.ToArray(), "Preview prototype screen");

            ui.Refresh(controller);
            ui.Window.gameObject.SetActive(true);
            ui.Notification.SetActive(false);
            ui.RestartDialog.SetActive(screen == Screen.Restart);
            int app = screen >= Screen.NoContact && screen <= Screen.ChatCompleted ? 1 :
                screen >= Screen.SearchEmpty && screen <= Screen.Ending ? 2 : 0;
            Select(ui.AppPanels, app);
            ui.WindowTitle.Show(app);
            if (app == 1)
            {
                ui.ChatEmpty.SetActive(screen == Screen.NoContact);
                ui.ChatConversation.SetActive(screen != Screen.NoContact);
                Select(ui.ChatStages, screen == Screen.ChatCompleted ? 4 : Math.Max(0, (int)screen - (int)Screen.Chat0));
            }
            if (app == 2)
            {
                int page = screen >= Screen.Isolation ? 3 : screen == Screen.Information ? 2 :
                    screen == Screen.ArticleLocked || screen == Screen.ArticleFound ? 1 : 0;
                Select(ui.BrowserPages, page);
                ui.Address.Show(page);
                ui.BackButton.interactable = screen != Screen.SearchEmpty && screen != Screen.SearchReady;
                bool keywords = screen != Screen.SearchEmpty;
                ui.SearchKeywords.SetActive(keywords);
                ui.SearchNoKeywords.SetActive(!keywords);
                ui.SearchIntro.SetActive(screen != Screen.Results);
                ui.SearchResults.SetActive(screen == Screen.Results);
                ui.SearchText.Show(keywords ? 3 : 0);
                ui.TermA.Show(keywords ? 1 : 0);
                ui.TermB.Show(keywords ? 1 : 0);
                bool clue = screen == Screen.ArticleFound;
                ui.ArticleClue.Show(clue ? 1 : 0);
                ui.ArticleHint.Show(clue ? 1 : 0);
                ui.EnterSite.interactable = clue;
                int phase = screen == Screen.Ending ? 3 : screen >= Screen.Ghost0 ? 2 : screen == Screen.Cameras ? 1 : 0;
                Select(ui.HauntedPhases, phase);
                int step = Mathf.Clamp((int)screen - (int)Screen.Ghost0, 0, 3);
                ui.GhostSpeaker.Show(step == 0 ? 0 : 1);
                ui.GhostLines.Show(step);
                ui.GhostChoice.Show(step);
                ui.GhostGroup.alpha = 1;
            }
            foreach (var item in changed) if (item) EditorUtility.SetDirty(item);
            EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
            Canvas.ForceUpdateCanvases();
            SceneView.RepaintAll();
        }

        static void Select(GameObject[] items, int selected)
        {
            for (int i = 0; i < items.Length; i++) items[i].SetActive(i == selected);
        }

        static GameObject VisiblePanel(PrototypeSceneUI ui)
        {
            if (ui.RestartDialog.activeSelf) return ui.RestartDialog;
            if (ui.AppPanels[0].activeSelf) return ui.AppPanels[0];
            if (ui.AppPanels[1].activeSelf)
                return ui.ChatEmpty.activeSelf ? ui.ChatEmpty : ui.ChatStages.First(x => x.activeSelf);
            if (ui.BrowserPages[3].activeSelf) return ui.HauntedPhases.First(x => x.activeSelf);
            return ui.BrowserPages.First(x => x.activeSelf);
        }
    }
}
