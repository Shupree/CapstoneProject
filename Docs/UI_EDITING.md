# 씬 UI 편집 안내

## 시작하기

1. `Assets/Scenes/CyberExorcistPrototype.unity`를 연다.
2. Play를 끈 상태에서 Hierarchy의 `CyberExorcistPrototype`을 선택한다.
3. Inspector의 `Prototype Scene UI` 컴포넌트에서 **편집할 화면**을 고르고 **화면 미리보기 적용**을 누른다.
4. **Hierarchy에서 현재 화면 선택**을 누르면 해당 화면 오브젝트가 선택된다. 자식의 RectTransform, Image, RawImage, TextMeshProUGUI를 수정한다.
5. 씬을 저장한다(Ctrl+S). Game 탭에서 배치를 확인하고 Play로 조작을 확인한다.

미리보기는 편집 중 표시할 화면만 바꾼다. 어떤 화면을 저장했든 Play는 첫 메일부터 시작한다. 미리보기 변경은 Undo할 수 있다. Play 중 수정은 Unity의 일반 규칙에 따라 Play 종료 시 복원되므로, 영구 수정은 편집 모드에서 한다.

## 주요 Hierarchy

```text
CyberExorcistPrototype
├── ExorcistDesktop (Canvas / CanvasScaler / GraphicRaycaster)
│   ├── FullScreenBackground
│   └── Desktop
│       ├── ApplicationWindow
│       │   ├── TitleBar
│       │   └── Content
│       │       ├── Mail
│       │       ├── Chat
│       │       │   ├── NoContact
│       │       │   └── Conversation
│       │       │       ├── Step0 ~ Step3
│       │       │       └── Completed
│       │       └── Browser
│       │           ├── BrowserToolbar
│       │           ├── Search
│       │           │   ├── AcquiredKeywords / NoKeywords
│       │           │   └── SearchIntro / SearchResults
│       │           ├── Article
│       │           ├── Information
│       │           └── HauntedSite
│       │               ├── Isolation
│       │               ├── Cameras
│       │               ├── GhostConversation
│       │               └── Ending
│       ├── CaseNotebook
│       ├── Notification
│       └── ModalLayer (재시작 확인창)
└── EventSystem
```

## 무엇을 어디서 수정하나

| 수정할 것 | 위치 |
| --- | --- |
| 위치·크기·정렬 | 해당 오브젝트의 RectTransform |
| 배경·버튼 색, 그림 | Image / RawImage 및 Button의 Colors |
| 메일 본문·게시글·단계별 메신저 대사 | 해당 화면의 TextMeshProUGUI Text |
| 단계별 귀신 대사 | 루트의 Prototype Scene UI → Ghost Lines → Values (0~3) |
| 귀신 선택지 | Ghost Choice → Values (0~3: 대화, 4: 성불 연출 중) |
| 앱 제목·검색어·목표 등 상태별 문구 | Prototype Scene UI의 해당 TextStates → Values |
| 글꼴·크기·텍스트 색 | 각 TextMeshProUGUI (상태 문구도 동일) |
| 버튼 동작 | Button → On Click → CyberExorcistPrototype.Click(string) |
| 귀신 움직임과 성불 시간 | CyberExorcistPrototype의 Ghost animation |
| 효과음 음량 | 루트의 AudioSource |
| 기준 해상도 | ExorcistDesktop의 CanvasScaler (현재 1600×900 / Expand) |

상태별 문구의 TMP Text만 수정하면 다음 상태 갱신에서 Values 값으로 돌아간다. 해당 문구는 Values에서 수정한다. 시계와 일시 알림은 진행 코드가 갱신한다.

화면별 오브젝트는 처음부터 씬에 있고 진행에 따라 활성화된다. 메일·대화·브라우저는 공통 창을 공유하므로 공통 창의 크기를 변경하면 각 화면 배치도 함께 확인한다. 다른 화면이 겹치면 미리보기를 적용해 활성 상태를 맞춘다.

오브젝트를 옮기거나 이름을 바꿔도 직렬화된 참조와 버튼 연결은 유지된다. 참조된 오브젝트를 삭제하거나 새 오브젝트로 교체했다면 Prototype Scene UI의 해당 참조와 버튼 이벤트를 다시 연결한다. 창 드래그는 TitleBar의 PrototypeWindowDrag가 Window를 참조한다.

## 사용자 테스트 목록

2026-10-10부터 게임 테스트는 사용자가 직접 수행한다. 아래는 확인할 항목과 기대 결과이며, 완료 표시가 없는 항목은 이번 사용자 확인 결과가 아직 기록되지 않은 상태다. 에이전트는 이 목록을 실행하지 않는다.

- [ ] **실행 전 오브젝트 존재:** Play를 끈 상태에서 루트 아래 ExorcistDesktop과 EventSystem, Content 아래 Mail·Chat·Browser가 이미 있는지 확인한다.
- [ ] **편집 화면 선택:** Prototype Scene UI의 '편집할 화면'에서 메일·대화·검색·카메라·귀신·완료 화면을 각각 선택하고 적용한다. 선택한 화면이 보이고 다른 앱 화면이 겹치지 않아야 한다.
- [ ] **씬 수정 유지:** 편집 모드에서 메일 제목의 위치·크기·색·문구를 원하는 값으로 바꾸고 씬을 저장한다. Play에서 앱을 전환하고 재시작한 뒤에도 설정한 모습이 유지되어야 한다. Play 종료 후에도 편집 모드에서 저장한 값이 남아야 한다.
- [ ] **상태별 문구 수정:** Prototype Scene UI의 Ghost Lines → Values 중 한 대사를 수정하고 저장한다. 해당 단계 미리보기와 실제 귀신 대화에서 수정한 대사가 보여야 한다. 단순 TMP Text 수정과 Values 수정을 구분한다.
- [ ] **시작 상태:** 귀신이나 완료 화면을 미리보기한 채 Play를 시작한다. 실제 게임은 첫 메일부터 시작하고 재시작 확인창은 닫혀 있어야 한다.
- [ ] **메일과 대화:** 연락처를 추가하고 민서와 대화한다. 키워드 '관음증'과 '귀신'을 얻고 검색 화면으로 이동할 수 있어야 한다.
- [ ] **진행 잠금과 뒤로 이동:** 키워드 획득 전이나 하나만 선택한 상태에서는 검색 결과로 진행되지 않아야 한다. 두 키워드로 검색한 뒤 일반 정보 글에서 뒤로 가기가 가능하고, 괴담 단서를 확인하기 전에는 사이트 이동 버튼이 비활성화되어야 한다.
- [ ] **카메라 조사:** 단서 확인 → 사이트 이동 → 격리를 진행하고 촬영도구를 치운다. 치운 도구만 사라지고, 다른 앱으로 갔다 돌아와도 제거 상태가 유지되며, 다섯 개를 모두 치우면 귀신이 나타나야 한다.
- [ ] **성불과 완료:** 귀신 대화를 끝낸다. 성불 연출 중 진행 조작이 잠기고, 이후 완료 화면과 민서의 감사 메시지를 볼 수 있어야 한다.
- [ ] **창과 소리:** 제목 표시줄 드래그, 최소화 후 앱 버튼으로 복원, 음소거·해제를 확인한다. 화면과 버튼이 정상 반응하고 진행 상태가 유지되어야 한다.
- [ ] **재시작:** 재시작 취소 시 현재 진행이 유지되고, 확인 시 키워드·단서·카메라·완료 상태가 초기화되어야 한다. 다시 진행하면 카메라 다섯 개와 귀신이 정상 표시되어야 한다.
- [ ] **표시 상태와 오류:** 각 화면에서 한글 누락, 텍스트 잘림, 버튼 겹침을 확인한다. Game 뷰 크기를 바꿔도 조작할 수 있어야 하며, Console에 새 오류가 발생하지 않아야 한다.

문제가 있으면 해당 항목, 발생 직전 조작, 기대한 결과와 실제 결과, Console 오류가 있다면 그 내용을 알려준다.

## 최초 배치 도구와 이전 검증 자료

`Tools > Cyber Exorcist > Create Scene UI (Once)`는 UI가 없는 프로토타입 씬을 처음 구성할 때만 사용한다. 이미 구성된 씬에서는 중단하여 직접 편집한 내용을 보호한다. `Setup Single Scene Prototype`도 기존 UI를 재구성하지 않는다. 평소 UI 수정에는 두 메뉴 모두 필요 없다.

`AgentScripts/ValidateSceneUI.cs`는 이전 작업에서 사용한 Unity Pipeline 검증 스크립트이며 게임 빌드에 포함되지 않는다. 2026-10-10 규칙에 따라 에이전트는 이를 실행하지 않는다. 아래는 기존 스크립트의 역할을 설명하는 참고 기록이다.
- 편집 모드: `SerializedScene`은 참조와 저장된 버튼 이벤트를 검사한다.
- 편집 모드: `PreviewScreens`는 21개 미리보기를 순회하고 텍스트 넘침을 검사한다. 검사 후 귀신 대화 미리보기를 남기므로 작업 후 메일 미리보기로 돌리고 저장한다.
- Play: `StartupAndDrag`는 초기 상태, 제목 표시줄 Raycast와 EventSystem 드래그를 검사한다.
- Play: 실제 입력으로 첫 연락처를 추가한 뒤 `Investigate`, 이어서 `Complete`를 실행하면 진행 잠금·의뢰 완료·재시작·씬 오브젝트 유지 여부를 검사한다. 테스트용 새 Play 세션에서 사용한다.

2026-10-09: 컴파일, 저장 후 씬 재열기, 전체 진행, 실제 포인터 연락처 추가, EventSystem 드래그, 21개 미리보기의 텍스트 넘침 검사를 수행했다. 독립 실행 파일 빌드와 모든 버튼의 실제 포인터 입력 검증은 수행하지 않았다.
